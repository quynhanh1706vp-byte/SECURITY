using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.DeviceReader;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// DeviceReader controller
    /// </summary>
    [Produces("application/json")]
    [CheckMultipleAddOnAttribute(new string [] { Constants.PlugIn.TimeAttendance, Constants.PlugIn.AccessControl })]
    public class DeviceReaderController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IDeviceReaderService _deviceReaderService;
        private readonly HttpContext _httpContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// DeviceReader controller
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="deviceReaderService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="mapper"></param>
        public DeviceReaderController(IConfiguration configuration, IDeviceReaderService deviceReaderService,
            IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _configuration = configuration;
            _deviceReaderService = deviceReaderService;
            _httpContext = httpContextAccessor.HttpContext;
            _mapper = mapper;
        }

        /// GET /deviceReaders : return all existing deviceReaders with paging and sorting
        /// <summary>
        /// Return Json object for all deviceReader list
        /// </summary>
        /// <param name="search">Query string that filter DeviceReader by Name or Type</param>
        /// <param name="buildingIds">List of building ids</param>
        /// <param name="deviceTypeIds">List of device type ids</param>
        /// <param name="deviceIds">List device ids</param>
        /// <param name="statusIds">List of status ids</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDeviceReaders)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.DeviceReader)]
        public IActionResult Gets(string search, List<int> buildingIds, List<int> deviceTypeIds, List<int> deviceIds, List<int> statusIds, int pageNumber = 1, int pageSize = 10, string sortColumn = "Name",
            string sortDirection = "desc")
        {
            int companyId = _httpContext.User.GetCompanyId();
            var deviceReaders = _deviceReaderService.GetPaginated(search, buildingIds, deviceTypeIds, deviceIds, statusIds, companyId, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered);

            var pagingData = new PagingData<DeviceReaderListModel>
            {
                Data = deviceReaders,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// GET /deviceReaders/{ID} : Get details of one deviceReader
        /// <summary>
        /// Get deviceReader detail information by id
        /// </summary>
        /// <param name="id">DeviceReader Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: DeviceReader Id not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDeviceReadersId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.DeviceReader)]
        public IActionResult Get(int id)
        {
            var model = new DeviceReaderModel();
            if (id != 0)
            {
                var deviceReader = _deviceReaderService.GetById(id);

                if (deviceReader == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, DeviceReaderResource.msgNotFoundDeviceReader);
                }
                model = _mapper.Map<DeviceReaderModel>(deviceReader);
            }
           
            return Ok(model);
        }

        /// POST /deviceReaders : Create new deviceReader
        /// <summary>
        /// Add new a deviceReader
        /// </summary>
        /// <param name="model">JSON model for DeviceReader</param>
        /// <returns></returns>
        /// <response code="201">Create new a deviceReader</response>
        /// <response code="400">Bad Request: count of deviceReaders must littler than max of deviceReaders</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON not valid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiDeviceReaders)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.DeviceReader)]
        public IActionResult Add([FromBody]DeviceReaderModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

           
            

          


           

            var isSuccess = _deviceReaderService.Add(model);

            if (!isSuccess)
            {
                return new ApiSuccessResult(StatusCodes.Status400BadRequest,
                    string.Format(MessageResource.MessageAddNewFailed, DeviceReaderResource.lblDeviceReader));
            }
            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, DeviceReaderResource.lblDeviceReader));
        }

        /// PUT /deviceReaders/{ID} : Update a deviceReader
        /// <summary>
        /// Update deviceReader
        /// </summary>
        /// <param name="id">DeviceReader Id</param>
        /// <param name="model">JSON model for deviceReader</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: DeviceReader Id not exist in DB</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON not valid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiDeviceReadersId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.DeviceReader)]
        public IActionResult Edit(int id, [FromBody]DeviceReaderModel model)
        {
            model.Id = id;
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }
            
            var deviceReader = _deviceReaderService.GetById(id);
            if (deviceReader == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, DeviceReaderResource.msgNotFoundDeviceReader);
            }

            _deviceReaderService.Update(model, deviceReader);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, DeviceReaderResource.lblDeviceReader, deviceReader.Name));
        }

        /// DELETE /deviceReaders : Delete multiple deviceReaders by list of ids
        /// <summary>
        /// Delete deviceReaders by multi id
        /// </summary>
        /// <param name="ids">List of deviceReader ids</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: List of DeviceReader Ids not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiDeviceReaders)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.DeviceReader)]
        public IActionResult DeleteMultiple(List<int> ids)
        {
            var deviceReaders = _deviceReaderService.GetByIds(ids)
                .ToList();
            if (!deviceReaders.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, DeviceReaderResource.msgNotFoundDeviceReader);
            }

            _deviceReaderService.DeleteRange(deviceReaders);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteMultipleSuccess, DeviceReaderResource.lblDeviceReader));
        }

        /// DELETE /deviceReaders/{ID} : Delete 1 deviceReader by ID
        /// <summary>
        /// Delete deviceReader by Id
        /// </summary>
        /// <param name="id">DeviceReader Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: DeviceReader Id not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiDeviceReadersId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.DeviceReader)]
        public IActionResult Delete(int id)
        {
            var deviceReader = _deviceReaderService.GetById(id);
            if (deviceReader == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, DeviceReaderResource.msgNotFoundDeviceReader);
            }

            _deviceReaderService.Delete(deviceReader);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, DeviceReaderResource.lblDeviceReader));
        }

        /// <summary>
        /// Export to file excel
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiDeviceReadersExport)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.DeviceReader)]
        public IActionResult Export()
        {
            int companyId = _httpContext.User.GetCompanyId();
            byte[] data = _deviceReaderService.ExportToFileExcel(companyId);
            return File(data, "application/ms-excel", $"device_{DateTime.UtcNow}.xlsx");
        }
    }
}