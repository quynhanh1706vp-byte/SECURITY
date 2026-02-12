using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Service.Protocol;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Define api camera
    /// </summary>
    [Produces("application/json")]
    public class CameraController : Controller
    {
        private readonly HttpContext _httpContext;
        private readonly ICameraService _cameraService;
        private readonly IDeviceService _deviceService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="cameraService"></param>
        /// <param name="deviceService"></param>
        public CameraController(IHttpContextAccessor httpContextAccessor, ICameraService cameraService, IDeviceService deviceService)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _cameraService = cameraService;
            _deviceService = deviceService;
        }
        
        /// <summary>
        /// Get init page cameras
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCamerasInit)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // [CheckAddOn(Constants.PlugIn.CameraPlugIn)]
        public IActionResult GetInit()
        {
            return Ok(_cameraService.GetInit(_httpContext.User.GetCompanyId()));
        }

        /// <summary>
        /// Get list of camera
        /// </summary>
        /// <param name="search">search string by name</param>
        /// <param name="types">types of camera</param>
        /// <param name="deviceIds"></param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCameras)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // [CheckAddOn(Constants.PlugIn.CameraPlugIn)]
        public IActionResult Gets(string search, List<int> types, List<int> deviceIds, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id", string sortDirection = "desc")
        {
            CameraFilterModel filterModel = new CameraFilterModel
            {
                Search = search,
                Types = types,
                CompanyId = _httpContext.User.GetCompanyId(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                DeviceIds = deviceIds,
            };
            
            var models = _cameraService.GetPaginated(filterModel, out var recordsTotal, out var recordsFiltered);
            
            var pagingData = new PagingData<CameraListModel>
            {
                Data = models,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// add new camera
        /// </summary>
        /// <param name="model">Json model camera</param>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCameras)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // [CheckAddOn(Constants.PlugIn.CameraPlugIn)]
        public IActionResult Add([FromBody] CameraModel model)
        {
            // check auto open door
            // if (model.AutoOpenDoor && model.TimeOpenDoor <= 0)
            // {
            //     return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.BadRequest);
            // }
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }
            
            // check IcuDeviceId
            if (model.IcuId > 0)
            {
                var device = _deviceService.GetById(model.IcuId);
                if(device == null) return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
            }
            // check cameraId
            if (_cameraService.GetByCameraId(model.CameraId) != null)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, CameraResource.DuplicateCameraId);
            }

            if (model.VideoLength == 0) model.VideoLength = Constants.HanetApiCamera.DefaultDelayCallApi;
            var add = _cameraService.Add(model);
            if(!add) return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.UnprocessableEntity);
            
            return new ApiSuccessResult(StatusCodes.Status201Created, string.Format(MessageResource.MessageAddSuccess, "camera"));
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCameraId)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Get(int id)
        {
            var c = _cameraService.GetDetailById(id);
            if(c == null) return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            CameraListModel camera = new CameraListModel();
            camera.Id = c.Id;
            camera.Name = c.Name;
            camera.PlaceID = c.PlaceID;
            camera.IcuId = c.IcuDeviceId ?? 0;
            camera.CompanyId = c.CompanyId;
            camera.DeviceName = c.IcuDevice?.Name;
            camera.CameraId = c.CameraId;
            camera.VideoLength = c.VideoLength;
            camera.Type = c.Type;
            camera.ConnectionStatus = c.ConnectionStatus;
            camera.RoleReader = c.RoleReader;
            camera.SaveEventUnknownFace = c.SaveEventUnknownFace;
            camera.CheckEventFromWebHook = c.CheckEventFromWebHook;
            camera.BuildingId = c.IcuDevice?.BuildingId;
            camera.UrlStream = c.UrlStream;
            camera.VmsUrlStream = c.VmsUrlStream;
            camera.Similarity = c.Similarity;
            camera.VoiceAlarm = c.VoiceAlarm;
            camera.LightAlarm = c.LightAlarm;
            return Ok(camera);
        }
        
        /// <summary>
        /// edit camera by id
        /// </summary>
        /// <param name="id">camera id</param>
        /// <param name="model">Json model camera</param>
        /// <returns></returns>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCameraId)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // [CheckAddOn(Constants.PlugIn.CameraPlugIn)]
        public IActionResult Update(int id, [FromBody] CameraModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }
            
            var camera = _cameraService.GetById(id);
            if(camera == null) return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            
            // check auto open door
            // if (model.AutoOpenDoor && model.TimeOpenDoor <= 0)
            // {
            //     return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.BadRequest);
            // }
            // check IcuDeviceId
            if (model.IcuId > 0)
            {
                var device = _deviceService.GetById(model.IcuId);
                if(device == null) return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
            }
            // check camera id
            if (!string.IsNullOrEmpty(model.CameraId) && camera.CameraId != model.CameraId)
            {
                if (_cameraService.GetByCameraId(model.CameraId) != null)
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, CameraResource.DuplicateCameraId);
                }
            }
            
            // send device config
            int deviceIdReset = 0;
            if (camera.IcuDeviceId.HasValue && camera.IcuDeviceId != 0 && camera.IcuDeviceId != model.IcuId)
            {
                deviceIdReset = camera.IcuDeviceId.Value;
            }
            
            camera.Name = string.IsNullOrEmpty(model.Name) ? camera.Name : model.Name;
            camera.IcuDeviceId = model.IcuId == 0 ? camera.IcuDeviceId : model.IcuId;
            camera.CompanyId = model.CompanyId == 0 ? camera.CompanyId : model.CompanyId;
            camera.CameraId = string.IsNullOrEmpty(model.CameraId) ? camera.Name : model.CameraId;
            camera.VideoLength = model.VideoLength == 0 ? camera.VideoLength : model.VideoLength;
            // camera.AutoOpenDoor = model.AutoOpenDoor;
            // camera.TimeOpenDoor = model.TimeOpenDoor == 0 ? camera.TimeOpenDoor : model.TimeOpenDoor;
            camera.RoleReader = model.RoleReader;
            camera.SaveEventUnknownFace = model.SaveEventUnknownFace;
            camera.CheckEventFromWebHook = model.CheckEventFromWebHook;
            camera.UrlStream = model.UrlStream;
            camera.Type = model.Type;
            camera.Similarity = model.Similarity;
            camera.VoiceAlarm = model.VoiceAlarm;
            camera.LightAlarm = model.LightAlarm;
            
            var update = _cameraService.Update(camera, deviceIdReset);
            if(!update) return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.UnprocessableEntity);
            
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }

        /// <summary>
        /// delete camera by id
        /// </summary>
        /// <param name="id">camera id</param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCameraId)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // [CheckAddOn(Constants.PlugIn.CameraPlugIn)]
        public IActionResult Delete(int id)
        {
            var camera = _cameraService.GetById(id);
            if(camera == null) return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            
            _cameraService.Delete(camera);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, "camera"));
        }

        /// <summary>
        /// ReCheck cameras event-log
        /// </summary>
        /// <param name="fromDate">Date start (yyyy-mm-DD)</param>
        /// <param name="fromTime">Time start (hh:mm:ss)</param>
        /// <param name="toDate">Date end (yyyy-mm-DD)</param>
        /// <param name="toTime">Time end (hh:mm:ss)</param>
        /// <returns></returns>
        [HttpPatch]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCameraRecheckEventLog)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult RecheckCameraEventLog(string fromDate, string fromTime, string toDate, string toTime)
        {
            DateTime start = Helpers.GetFromToDateTimeConvert(fromDate, fromTime, false);
            DateTime end = Helpers.GetFromToDateTimeConvert(toDate, toTime, true);
            _cameraService.RecheckEventLog(_httpContext.User.GetCompanyId(), start, end);
            return Ok();
        }

        /// <summary>
        /// get camera histories
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCameraHistory)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetCameraHistory(int id, int pageNumber = 1, int pageSize = 10)
        {
            var camera = _cameraService.GetById(id);
            if (camera != null)
            {
                var cameraHistory = _cameraService.GetHistory(id, pageNumber, pageSize, out var totalRecords).ToList();

                var pagingData = new PagingData<DeviceHistoryModel>
                {
                    Data = cameraHistory,
                    Meta = { RecordsTotal = totalRecords, RecordsFiltered = totalRecords}
                };

                return Ok(pagingData);
            }
            
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
        }
    }
}