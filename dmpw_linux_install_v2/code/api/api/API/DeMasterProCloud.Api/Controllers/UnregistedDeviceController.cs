using Microsoft.AspNetCore.Antiforgery;
﻿using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.UnregistedDevice;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// UnregistedDevice controller
    /// </summary>
[Route("api/[controller]")]
[ApiController]
[IgnoreAntiforgeryToken]
[Authorize]
    public class UnregistedDeviceController : Controller
    {
        private readonly IUnregistedDeviceService _unregistedDeviceService;
        private readonly IDeviceService _deviceService;

        /// <summary>
        /// Unregisted controller
        /// </summary>
        /// <param name="unregistedDeviceService"></param>
        /// <param name="deviceService"></param>
        public UnregistedDeviceController(IUnregistedDeviceService unregistedDeviceService, IDeviceService deviceService)
        {
            _unregistedDeviceService = unregistedDeviceService;
            _deviceService = deviceService;
        }

        /// <summary>
        /// Get the missingDevice(s)
        /// </summary>
        /// <param name="search">Query string that filter by device address or ip address</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUnregitedDevices)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUnregisterDevices(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id",
            string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<UnregistedDeviceModel>(sortColumn, "Id", ColumnDefines.UnregistedDevices);
            var devices = _unregistedDeviceService.GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                out var recordsFiltered).AsEnumerable().ToList();

            var pagingData = new PagingData<UnregistedDeviceModel>
            {
                Data = devices,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get the missingDevice(s)
        /// </summary>
        /// <param name="search">Query string that filter by device address or ip address</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUnregisteredDevices)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUnregisteredDevices(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id",
            string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<UnregistedDeviceModel>(sortColumn, "Id", ColumnDefines.UnregistedDevices);
            var devices = _unregistedDeviceService.GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                out var recordsFiltered).AsEnumerable().ToList();

            var pagingData = new PagingData<UnregistedDeviceModel>
            {
                Data = devices,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Add a missingDevice to icuDevice
        /// </summary>
        /// <param name="ids"> a list of unregistered device's identifier </param>
        /// <response code="400">Bad Request: missing devices with unregister not null</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiAddMissingDevices)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult AddMissingDevice(List<int> ids = null)
        {
            //var missingDevices = _unregistedDeviceService.GetByCompanyId();
            var missingDevices = _unregistedDeviceService.GetAll();
            if (missingDevices == null)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.msgNotFoundUnregistedDevice);
            }

            // [Edward] Temporary code
            //missingDevices = missingDevices.Where(m => m.RegisterType == (int)Registertype.NewDevice).ToList();

            if(ids != null && ids.Any())
                missingDevices = missingDevices.Where(m => ids.Contains(m.Id)).ToList();

            int countAddNewDevice = missingDevices.Count(m => m.RegisterType == (short)Registertype.NewDevice);
            
            // check limit add device to system
            var checkLimit = _deviceService.CheckLimitDevicesAdded(countAddNewDevice);
            if (!checkLimit.IsAdded)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(DeviceResource.msgMaximumAddDevice, checkLimit.NumberOfMaximum));
            }
            
            _unregistedDeviceService.AddMissingDevice(missingDevices);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateMissingDeviceSuccess));
        }

        /// <summary>
        /// Delete unregistered device from database
        /// </summary>
        /// <param name="ids"> list of unregistered device's identifier </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: List of Devices do not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiUnregisteredDevices)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Delete(List<int> ids = null)
        {
            var devices = _unregistedDeviceService.GetByIds(ids);

            if(devices == null || devices.Count() == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUnregistedDevice);
            }

            _unregistedDeviceService.DeleteMultiple(ids);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.msgDeleteUnregisteredDevice));
        }
    }
}