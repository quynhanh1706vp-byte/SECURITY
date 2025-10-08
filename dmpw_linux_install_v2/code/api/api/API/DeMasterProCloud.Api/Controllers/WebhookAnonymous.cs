using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.DeviceSDK;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service;
using DeMasterProCloud.Service.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text.Encodings.Web;

namespace DeMasterProCloud.Api.Controllers
{
    [Produces("application/ms-excel", "application/json", "application/text")]
    public class WebhookAnonymousController : Controller
    {
        private readonly IShortenLinkService _shortenLinkService;
        private readonly IConfiguration _configuration;
        private readonly IAttendanceService _attendanceService;
        private readonly ICameraService _cameraService;

        private static Regex DECODING_REGEX = new Regex(@"\\u(?<Value>[a-fA-F0-9]{4})", RegexOptions.Compiled);
        private const string PLACEHOLDER = @"#!쀍쀍쀍!#";

        public WebhookAnonymousController(IShortenLinkService shortenLinkService, IConfiguration configuration, IAttendanceService attendanceService,
            ICameraService cameraService)
        {
            _shortenLinkService = shortenLinkService;
            _configuration = configuration;
            _attendanceService = attendanceService;
            _cameraService = cameraService;
        }
        
        /// <summary>
        /// Get image static from link file local
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="companyCode"></param>
        /// <param name="date"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiImageStatic)]
        [AllowAnonymous]
        public IActionResult GetFileFromPath(string rootFolder, string companyCode, string date, string fileName)
        {
            try
            {
                if (IsUnicode(fileName))
                {
                    fileName = UnicodeToString(fileName);
                }

                var basePath = _configuration["FileStoragePath"] ?? Constants.Settings.DefineFolderImages;
                var fullPath = FileHelpers.GetSecurePath(basePath, rootFolder, companyCode, date, fileName);

                if (fullPath == null)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, "Invalid path parameters");
                }

                if (System.IO.File.Exists(fullPath))
                {
                    var bytes = System.IO.File.ReadAllBytes(fullPath);
                    string extension = "";
                    if (fileName.Contains("."))
                    {
                        extension = fileName.Split(".").Last().ToLower();
                    }

                    switch (extension)
                    {
                        case "jpg":
                        case "jpeg":
                        case "png":
                        {
                            return File(bytes, $"image/{extension}");
                        }
                        case "mp4":
                        case "avi":
                        case "webm":
                        {
                            return File(bytes, $"video/{extension}");
                        }
                        default:
                            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
                    }
                }
            }
            catch
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
            }
            
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
        }

        /// <summary>
        /// Get image static from link file local for categories like avatar, visitor
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="companyCode"></param>
        /// <param name="category"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiImageStaticCategory)]
        [AllowAnonymous]
        public IActionResult GetFileFromPathByCategory(string rootFolder, string companyCode, string category, string fileName)
        {
            try
            {
                if (IsUnicode(fileName))
                {
                    fileName = UnicodeToString(fileName);
                }

                var basePath = _configuration["FileStoragePath"] ?? rootFolder;
                var fullPath = FileHelpers.GetSecurePath(basePath, companyCode, category, fileName);

                if (fullPath == null)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, "Invalid path parameters");
                }

                if (System.IO.File.Exists(fullPath))
                {
                    var bytes = System.IO.File.ReadAllBytes(fullPath);
                    string extension = "";
                    if (fileName.Contains("."))
                    {
                        extension = fileName.Split(".").Last().ToLower();
                    }

                    switch (extension)
                    {
                        case "jpg":
                        case "jpeg":
                        case "png":
                        {
                            return File(bytes, $"image/{extension}");
                        }
                        case "mp4":
                        case "avi":
                        case "webm":
                        {
                            return File(bytes, $"video/{extension}");
                        }
                        default:
                            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
                    }
                }
            }
            catch
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
            }

            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
        }

        /// <summary>
        /// Check string is contains unicode
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool IsUnicode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }
            var asciiBytesCount = Encoding.ASCII.GetByteCount(input);
            var unicodeBytesCount = Encoding.UTF8.GetByteCount(input);
            return asciiBytesCount != unicodeBytesCount;
        }

        /// <summary>
        /// Unicode Decoding with XSS protection
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public string UnicodeToString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;

            string decoded = DECODING_REGEX
                .Replace(str.Replace(@"\\", PLACEHOLDER), new MatchEvaluator(CapText))
                .Replace(PLACEHOLDER, @"\\");
    
            return HtmlEncoder.Default.Encode(decoded);
        }
        
        static string CapText(Match m)
        { 
            // Get the matched string.
            return ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString();
        }
        
        /// <summary>
        /// Get full path by short path
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiShortenLink)]
        [AllowAnonymous]
        public IActionResult GetFullPathByShortenLink(string pathname)
        {
            var fullPath = _shortenLinkService.GetFullPathByShortPath(pathname);
            if (string.IsNullOrWhiteSpace(fullPath))
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundPath);
            
            return Ok(fullPath);
        }
        
        /// <summary>
        /// Get image static from path hanet
        /// </summary>
        /// <param name="cameraId"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="date"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiFaceUploadStatic)]
        [AllowAnonymous]
        public IActionResult GetFileHanetFromPath(string cameraId, string year, string month, string date, string fileName)
        {
            try
            {
                if (IsUnicode(fileName))
                {
                    fileName = UnicodeToString(fileName);
                }

                var basePath = "hanet/images";
                var fullPath = FileHelpers.GetSecurePath(basePath, cameraId, year, month, date, fileName);

                if (fullPath == null)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, "Invalid path parameters");
                }

                if (System.IO.File.Exists(fullPath))
                {
                    var bytes = System.IO.File.ReadAllBytes(fullPath);
                    string extension = "";
                    if (fileName.Contains("."))
                    {
                        extension = fileName.Split(".").Last().ToLower();
                    }

                    switch (extension)
                    {
                        case "jpg":
                        case "jpeg":
                        case "png":
                        {
                            return File(bytes, $"image/{extension}");
                        }
                        case "mp4":
                        case "avi":
                        case "webm":
                        {
                            return File(bytes, $"video/{extension}");
                        }
                        default:
                            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
                    }
                }
            }
            catch
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
            }

            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
        }
        /// <summary>
        /// Receive event data checkin from DC camera.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiCameraWebhookTsCamera)]
        [AllowAnonymous]
        public IActionResult ReceiverDataCheckInFromTsCamera(IFormFile faceImage, IFormFile vehicleImage)
        {
            if (faceImage.Length == 0)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, "File Error (0 byte)");
            }
            
            string messageError = _cameraService.ReceiveWebhookFromTsCamera(faceImage, vehicleImage);
            if (string.IsNullOrEmpty(messageError))
                return Ok();

            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, messageError);
        }
        
        /// <summary>
        /// Receive event data che
        /// <summary>
        /// Get fingerprint template by card id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiCardsIdFingerprintTemplate)]
        [AllowAnonymous]
        public IActionResult GetFingerprintTemplate(int id)
        {
            IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
            List<string> templates = new List<string>();
            try
            {
                templates = unitOfWork.CardRepository.GetFingerPrintByCard(id).Select(m => m.Templates).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine("[Exception Get Fingerprint Template]");
                Console.WriteLine(e);
            }
            finally
            {
                unitOfWork.Dispose();
            }

            return Ok(new { data = templates });
        }
        
        [HttpPost]
        [Route(Constants.Route.ApiWebhookReceiveEventLog)]
        [AllowAnonymous]
        public IActionResult SDKReceiveEvent()
        {
            try
            {
                string body = "";
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    body = reader.ReadToEnd();
                }
                
                Console.WriteLine($"[SDKReceiveEvent]: {body}");
                var data = JsonConvert.DeserializeObject<SDKDataWebhookModel>(body);
                new Thread(() =>
                {
                    IDeviceSDKService deviceService = new DeviceSDKService(ApplicationVariables.Configuration);
                    deviceService.ReceiveDataWebhook(data);
                }).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SDKReceiveEvent]: {ex}");
            }

            return Ok();
        }
        
        [HttpPost]
        [Route(Constants.Route.ApiWebhookReceiveDoorStatus)]
        [AllowAnonymous]
        public IActionResult SDKReceiveDoorStatus()
        {
            try
            {
                string body = "";
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    body = reader.ReadToEnd();
                }
                
                Console.WriteLine($"[SDKReceiveDoorStatus]: {body}");
                var data = JsonConvert.DeserializeObject<SDKDataWebhookModel>(body);
                new Thread(() =>
                {
                    IDeviceSDKService deviceService = new DeviceSDKService(ApplicationVariables.Configuration);
                    deviceService.ReceiveDataWebhook(data);
                }).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SDKReceiveDoorStatus]: {ex}");
            }

            return Ok();
        }

        [HttpPost]
        [Route("/test/test-websocket")]
        [AllowAnonymous]
        public IActionResult TestWebsocket([FromBody] string body)
        {
            ApplicationVariables.SendMessageToAllClients(body);
            return Ok();
        }
    }
}