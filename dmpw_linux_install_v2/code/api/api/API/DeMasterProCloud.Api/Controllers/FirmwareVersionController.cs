using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.FirmwareVersion;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace DeMasterProCloud.Api.Controllers
{
    public class FirmwareVersionController : Controller
    {
        private readonly IFirmwareVersionService _firmwareVersionService;
        private readonly IConfiguration _configuration;

        public FirmwareVersionController(IConfiguration configuration, IFirmwareVersionService firmwareVersionService)
        {
            _configuration = configuration;
            _firmwareVersionService = firmwareVersionService;
        }

        /// <summary>
        /// Get list firmware version
        /// </summary>
        /// <param name="version"></param>
        /// <param name="deviceTypes"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiFirmwareVersion)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Gets(string version, List<int> deviceTypes, int pageNumber = 1, int pageSize = 25, string sortColumn="version", string sortDirection="desc")
        {
            var filter = new FirmwareVersionFilterModel()
            {
                Version = version,
                DeviceTypes = deviceTypes,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
            };

            var data = _firmwareVersionService.Gets(filter, out int recordsFiltered, out int recordsTotal);
            var pagingData = new PagingData<FirmwareVersionListModel>()
            {
                Data = data,
                Meta = new Meta() { RecordsFiltered = recordsFiltered, RecordsTotal = recordsTotal },
            };

            return Ok(pagingData);
        }

        /// <summary>
        /// Add new firmware version
        /// </summary>
        /// <param name="file"></param>
        /// <param name="version"></param>
        /// <param name="note"></param>
        /// <param name="deviceType"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiFirmwareVersion)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Add([FromForm] IFormFile file, [FromForm] string version, [FromForm] string note, [FromForm] short deviceType)
        {
            if (file == null || file.Length == 0)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.BadRequest);
            }

            // Use secure file saving to prevent path traversal attacks
            bool isSuccess = FileHelpers.SaveFileByIFormFileSecure(file, Constants.Settings.DefineFolderFirmwareVersion, file.FileName);
            if (!isSuccess)
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.MessageAddNewFailed, "firmware"));

            string hostApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Value;
            string urlDownload = $"{hostApi}{Constants.Route.ApiFirmwareVersionDownload}?fileName={file.FileName}";

            var model = new FirmwareVersionModel()
            {
                Id = 0,
                FileName = file.FileName,
                Version = version,
                DeviceType = deviceType,
                LinkFile = urlDownload,
                Note = note,
            };
            isSuccess = _firmwareVersionService.Add(model);
            if (!isSuccess)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.MessageAddNewFailed, "firmware"));

            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageAddSuccess, "firmware"));
        }

        /// <summary>
        /// Delete multi firmware version
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route(Constants.Route.ApiFirmwareVersion)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Delete(List<int> ids)
        {
            if (ids == null || !ids.Any())
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.BadRequest);
            
            _firmwareVersionService.Delete(ids);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, "firmware"));
        }

        /// <summary>
        /// Download firmware version
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiFirmwareVersionDownload)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DownloadFirmwareVersion(string fileName)
        {
            var securePath = FileHelpers.GetSecurePath(Constants.Settings.DefineFolderFirmwareVersion, fileName);
            if (securePath == null)
                return new ApiErrorResult(StatusCodes.Status400BadRequest, "Invalid file name");

            if (!System.IO.File.Exists(securePath))
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);

            return File(System.IO.File.ReadAllBytes(securePath), FileHelpers.GetContentTypeByFileName(fileName), fileName);

        }

        /// <summary>
        /// Update firmware 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="processIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiFirmwareVersionIdUpdateDevice)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdateFirmwareToDevices(int id, [FromBody] List<string> processIds)
        {
            var firmwareVersion = _firmwareVersionService.GetById(id);
            if (firmwareVersion == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound); 

            var securePath = FileHelpers.GetSecurePath(Constants.Settings.DefineFolderFirmwareVersion, firmwareVersion.FileName);
            if (securePath == null || !System.IO.File.Exists(securePath))
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);

            if (processIds == null || !processIds.Any())
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.BadRequest);

            _firmwareVersionService.UpdateFirmwareToDevices(firmwareVersion, processIds);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }
        
        /// <summary>
        /// Device will using this to download file firmware
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="hash">MD5(fileName + companyCode + ddMMyyyyHH (UTC time))</param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiFirmwareVersionDeviceDownload)]
        [AllowAnonymous]
        public IActionResult DeviceDownloadFirmware(string fileName, string hash)
        {
            var securePath = FileHelpers.GetSecurePath(Constants.Settings.DefineFolderFirmwareVersion, fileName);
            if (securePath == null)
                return new ApiErrorResult(StatusCodes.Status400BadRequest, "Invalid file name");

            if (!System.IO.File.Exists(securePath))
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);

            bool isVerified = _firmwareVersionService.CheckHashFirmware(fileName, hash);
            if (!isVerified)
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.BadRequest);

            return File(System.IO.File.ReadAllBytes(securePath), FileHelpers.GetContentTypeByFileName(fileName), fileName);
        }
    }
}
