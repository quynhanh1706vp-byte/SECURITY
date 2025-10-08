using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.Header;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.DataModel.Visit;
using DeMasterProCloud.Service;
using DeMasterProCloud.Service.Infrastructure.Header;
using DeMasterProCloud.Service.Protocol;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Page = DeMasterProCloud.Common.Infrastructure.Page;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Device controller
    /// </summary>
    [Produces("application/json")]
    public class DeviceController : Controller
    {
        private readonly IDeviceService _deviceService;
        private readonly IConfiguration _configuration;
        private readonly IVisitService _visitService;
        private readonly IDepartmentService _departmentService;
        private readonly ICompanyService _companyService;
        private readonly IUserService _userService;
        private readonly IBuildingService _buildingService;
        private readonly HttpContext _httpContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// Device controller
        /// </summary>
        /// <param name="deviceService"> Service of device </param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="visitService"> Service of visit </param>
        /// <param name="departmentService"></param>
        /// <param name="companyService"></param>
        /// <param name="userService"></param>
        /// <param name="buildingService"></param>
        /// <param name="configuration"></param>
        /// <param name="mapper"></param>
        public DeviceController(IDeviceService deviceService, IHttpContextAccessor httpContextAccessor, IVisitService visitService,
            IDepartmentService departmentService, ICompanyService companyService, IUserService userService, IBuildingService buildingService,
            IConfiguration configuration, IMapper mapper)
        {
            _deviceService = deviceService;
            _visitService = visitService;
            _departmentService = departmentService;
            _companyService = companyService;
            _userService = userService;
            _buildingService = buildingService;
            _configuration = configuration;
            _httpContext = httpContextAccessor.HttpContext;
            _mapper = mapper;
        }

        /// <summary>
        /// init device page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDeviceInit)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.DeviceSetting)]
        public IActionResult GetDeviceInit()
        {
            var templateMonitoring = EnumHelper.ToEnumList<TypeSubMonitoring>().OrderBy(m => m.Name);
            var eventType = EnumHelper.ToEnumList<EventType>().OrderBy(m => m.Name);
            var doorStatus = _deviceService.InitDoorStatus(EnumHelper.ToEnumList<DoorStatus>().OrderBy(m => m.Id));
            return Ok(new { templateMonitoring, eventType, doorStatus });
        }

        /// <summary>
        /// Return Json object for all device list
        /// </summary>
        /// <param name="search">Query string that filter Devices by Name, Device Address or Name of Building</param>
        /// <param name="deviceType">Device type</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending </param>
        /// <param name="operationType">operation type (restaurant - 1, entrance - 0)</param>
        /// <param name="companyId">Company Id</param>
        /// <param name="connectionStatus">status connection</param>
        /// <param name="status">list of status device</param>
        /// <param name="pageName"></param>
        /// <param name="buildingIds">list of Building Ids</param>
        /// <param name="mealServiceTimeIds">list of meal service time ids</param>
        /// <param name="isAllBuildingParent">Option get all list building parent</param>
        /// <param name="ignoreIds"></param>
        /// <param name="checkCount"> a flag to distinguish calculating registered user count </param>
        /// <example> 
        /// 0: Do not calculate registered user count 
        /// 1: Calculate registered user count in background (Thread)
        /// 2: Calculate registered user count in API process (same as original)
        /// </example>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDevices)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.DeviceSetting)]
        public IActionResult Gets(string search, List<int> deviceType = null, int pageNumber = 1, int pageSize = 10, string sortColumn = "doorName",
            string sortDirection = "asc", List<int> operationType = null, List<int> companyId = null, List<int> connectionStatus = null, List<int> status = null,
            string pageName = null, List<int> buildingIds = null, List<int> mealServiceTimeIds = null, bool isAllBuildingParent = false, int checkCount = 2, List<int> ignoreIds = null)
        {
            // fwVersion is not included in 'DeviceListModel' columns.
            // There is 'Version' Column.
            if (!string.IsNullOrWhiteSpace(sortColumn) && sortColumn.ToLower().Equals("fwversion"))
            {
                sortColumn = "FirmwareVersion";
            }

            var filter = new DeviceFilterModel()
            {
                Filter = search,
                OperationTypes = operationType,
                CompanyIds = companyId,
                ConnectionStatus = connectionStatus,
                Status = status,
                DeviceTypes = deviceType,
                BuildingIds = buildingIds,
                IgnoreIds = ignoreIds,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
            };
            var devices = _deviceService.GetPaginatedDevices(filter, out var recordsTotal, out var recordsFiltered).ToList();

            
            if (isAllBuildingParent)
            {
                List<List<string>> buildingPath = _buildingService.GenerateAllBuildingPath(_httpContext.User.GetCompanyId());
                foreach (var item in devices)
                {
                    item.BuildingParents = buildingPath.FirstOrDefault(m => m.Last() == item.Building);
                }
            }

            if (checkCount == 2)
            {
                foreach (var item in devices)
                {
                    // If checkCount value is 2, Counting registered ID is same as before
                    item.FromDbIdNumber = _deviceService.CountRegisteredIdByDeviceId(item.Id);
                }
            }

            if (string.IsNullOrWhiteSpace(pageName))
            {
                var pagingData = new PagingData<DeviceListModel>
                {
                    Data = devices,
                    Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
                };
                return Ok(pagingData);
            }
            else
            {
                IPageHeader pageHeader = new PageHeader(_configuration, pageName, _httpContext.User.GetCompanyId());
                var header = pageHeader.GetHeaderList(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId());

                var pagingData = new PagingData<DeviceListModel, HeaderData>
                {
                    Data = devices,
                    Header = header,
                    Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
                };
                return Ok(pagingData);
            }
        }

        /// <summary>
        /// Return Json object for all device list
        /// </summary>
        /// <param name="search">Query string that filter Devices by Name or Device Address</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending </param>
        /// <param name="companyId">Company Id</param>
        /// <param name="connectionStatus">status connection</param>
        /// <param name="deviceType">Device type</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiValidDevices)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.DeviceMonitoring)]
        public IActionResult GetValidDevice(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "0",
            string sortDirection = "desc", List<int> companyId = null, List<int> connectionStatus = null, List<int> deviceType = null)
        {
            sortColumn = Helpers.CheckPropertyInObject<DeviceListModel>(sortColumn, "Company.Name", ColumnDefines.DeviceValid);
            var devices = _deviceService.GetPaginatedDeviceValid(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                out var recordsFiltered, companyId, connectionStatus, deviceType).AsEnumerable().Select(_mapper.Map<DeviceListModel>).ToList();
            foreach (var item in devices)
            {
                var totalTime = Math.Abs(Math.Ceiling(DateTime.Now.Subtract(item.CreatedOn).TotalMinutes));
                item.TotalTime = Convert.ToInt32(totalTime);
                item.UpTime = Convert.ToInt32(Math.Ceiling((decimal)(item.UpTimeOnlineDevice * 100 / totalTime)));
            }
            var pagingData = new PagingData<DeviceListModel>
            {
                Data = devices,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get devices filtered by company and deviceType
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDeviceGetFilter)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult FilterCompanyAndDeviceType()
        {
            var data = _deviceService.ListFilterDeviceMonitoring();

            return Ok(data);
        }
        /// <summary>
        /// Get init
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDeviceGetInit)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetInit()
        {
            var data = _deviceService.getInit();

            return Ok(data);
        }

        /// <summary>
        /// Get device information by id
        /// </summary>
        /// <param name="id">device id to get</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDevicesId)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Get(int id)
        {
            var model = new DeviceDataModel();
            var device = new IcuDevice();

            if (id != 0)
            {
                device = _deviceService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
                if (device == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
                }
                model = _mapper.Map<DeviceDataModel>(device);
            }
            _deviceService.InitData(model, device.CompanyId ?? 0);
            DeviceDetailModel data = _mapper.Map<DeviceDetailModel>(model);
            data.DoorStatus = device.DoorStatus;
            data.ConnectionStatus = device.ConnectionStatus;
            return Ok(data);
        }

        /// <summary>
        /// Add device to system
        /// </summary>
        /// <param name="model">This model has information about the device to be added.</param>
        /// <returns></returns>
        /// <response code="201">Create new a device</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiDevices)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Add([FromBody] DeviceModel model)
        {
            // check limit add device to system
            var checkLimit = _deviceService.CheckLimitDevicesAdded(1);
            if (!checkLimit.IsAdded)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(DeviceResource.msgMaximumAddDevice, checkLimit.NumberOfMaximum));
            }

            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }
            if (model.Image.IsTextBase64())
            {
                string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();

                // Use secure file saving to prevent path traversal attacks
                string fileName = $"{model.DeviceAddress}.{Guid.NewGuid().ToString()}.jpg";
                string basePath = $"{Constants.Settings.DefineFolderImages}/device/{Enum.GetName(typeof(DeviceType), model.DeviceType)}";
                bool success = FileHelpers.SaveFileImageSecure(model.Image, basePath, fileName, Constants.Image.MaximumImageStored);

                if (!success)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, "Invalid device address or image save failed");
                }

                string path = Path.Combine(basePath, fileName);
                model.Image = $"{connectionApi}/static/{path}";
            }

            model.Id = 0;
            int deviceId = _deviceService.Add(model);
            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, DeviceResource.lblDevice.ToLower()), deviceId.ToString());
        }

        /// <summary>
        /// Update device information
        /// </summary>
        /// <param name="id">device id to edit</param>
        /// <param name="model">This model has modified information about the device.</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiDevicesId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.DeviceSetting)]
        public IActionResult Edit(int id, [FromBody] DeviceModel model)
        {
            model.Id = id;

            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }

            var device = _deviceService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());

            if (device == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }
            // check door is already in department with department level
            bool doorsInvalid = _departmentService.CheckDoorInDepartment(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId(), new List<int>() { device.Id });
            if (!doorsInvalid)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, AccessGroupResource.msgNotPermissionDevices);
            }
            if (model.Image.IsTextBase64())
            {
                string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                if (!string.IsNullOrEmpty(device.Image))
                {
                    FileHelpers.DeleteFileFromLink(device.Image.Replace($"{connectionApi}/static/", ""));
                }

                // Use secure file saving to prevent path traversal attacks
                string fileName = $"{model.DeviceAddress}.{Guid.NewGuid().ToString()}.jpg";
                string basePath = $"{Constants.Settings.DefineFolderImages}/device/{Enum.GetName(typeof(DeviceType), model.DeviceType)}";
                bool success = FileHelpers.SaveFileImageSecure(model.Image, basePath, fileName, Constants.Image.MaximumImageStored);

                if (!success)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, "Invalid device address or image save failed");
                }

                string path = Path.Combine(basePath, fileName);
                model.Image = $"{connectionApi}/static/{path}";
            }
            _deviceService.Update(model);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, DeviceResource.lblDevice.ToLower() + " " + device.DeviceAddress, ""));
        }

        /// <summary>
        /// Delete device by id ( only 1 device )
        /// </summary>
        /// <param name="id">device id to delete</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiDevicesId)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Delete(int id)
        {
            var device = _deviceService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
            if (device == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }

            _deviceService.Delete(device);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, DeviceResource.lblDevice.ToLower()));
        }

        /// <summary>
        /// Delete devices by multi id ( 1~multi device(s) )
        /// </summary>
        /// <param name="ids">list of devices to delete</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: List of Device Ids does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiDevices)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteMultiple(List<int> ids)
        {
            var devices = _deviceService.GetByIdsAndCompany(ids, _httpContext.User.GetCompanyId())
                .ToList();
            if (devices.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }

            _deviceService.DeleteRange(devices);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteMultipleSuccess, DeviceResource.lblDevice.ToLower()));
        }

        /// <summary>
        /// Change device status to Valid or Invalid
        /// </summary>
        /// <param name="id">Device ID to change status</param>
        /// <param name="model">This model has information about device status.</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiDeviceUpdateStatus)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ToggleStatus(int id, [FromBody] DeviceStatus model)
        {
            var device = _deviceService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
            if (device == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }

            _deviceService.UpdateDeviceStatus(device, model.Status);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, DeviceResource.lblDevice.ToLower(), ""));
        }

        /// <summary>
        /// get device infomation in response. ( ex. Send current time )
        /// </summary>
        /// <param name="id">device id to get information about</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPatch]
        [Route(Constants.Route.ApiDeviceInfo)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetDeviceInfoResponse(int id)
        {
            var device = _deviceService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
            if (device == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }
            _deviceService.SendDeviceInfo(device.DeviceAddress);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.PublishMessageToGetInfoDevice, DeviceResource.lblDevice.ToLower()));
        }

        /// <summary>
        /// send device instruction command.
        /// </summary>
        /// <param name="model">This model has information about instruction.</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: command in Model JSON not null</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiDeviceInstruction)]
        //[Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.SendInstruction + Page.DeviceMonitoring)]
        public IActionResult SendDeviceInstruction([FromBody] DeviceInstruction model)
        {
            int companyId = _httpContext.User.GetCompanyId();
            var devices = _deviceService.GetByIdsAndCompany(model.Ids, companyId);
            if (devices == null || devices.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }

            if (string.IsNullOrWhiteSpace(model.Command))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.CommandCanBeNotEmpty);
            }
            if (model.Command == Constants.CommandType.Open)
            {
                var statusInvalids = new List<int>()
                {
                    (short)DoorStatus.EmergencyClosed,
                    (short)DoorStatus.EmergencyOpened,
                };
                var deviceInvalids = devices.Where(m => statusInvalids.Contains(m.DoorStatusId)).ToList();
                if (deviceInvalids.Count > 0)
                {
                    string msgDeviceInvalid = "";
                    deviceInvalids.ForEach(m => msgDeviceInvalid += $"{m.Name} - {((DoorStatus)m.DoorStatusId).GetDescription()},");
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, DeviceResource.msgDeviceNotReady + $" ({msgDeviceInvalid.Substring(0, msgDeviceInvalid.Length - 1)})");
                }
            }

            var offlineDeviceAddr = "";
            _deviceService.GetOnlineDevices(devices, ref offlineDeviceAddr);
            if (!string.IsNullOrEmpty(offlineDeviceAddr))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgDeviceDisconnected, offlineDeviceAddr));
            }

            var company = _companyService.GetById(companyId);
            List<IcuDevice> onlineDevices;
            if (company != null && !company.AutoSyncUserData)
                onlineDevices = _deviceService.GetOnlineDevices(devices, ref offlineDeviceAddr);
            else
                onlineDevices = devices;

            // check door is already in department with department level
            bool doorsInvalid = _departmentService.CheckDoorInDepartment(companyId, _httpContext.User.GetAccountId(),
                onlineDevices.Select(x => x.Id).ToList());
            if (!doorsInvalid)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, AccessGroupResource.msgCannotUnAssignDevices);
            }

            _deviceService.SendInstruction(onlineDevices, model);

            if (onlineDevices.Count == 0 || !string.IsNullOrEmpty(offlineDeviceAddr))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgOfflineDevice, offlineDeviceAddr));
            }
            else
            {
                string message = "";
                switch (model.Command)
                {
                    case Constants.CommandType.ForceOpen:
                        message = DeviceResource.msgForcedOpenSuccess;
                        break;
                    case Constants.CommandType.Open when model.OpenPeriod > 0:
                        message = string.Format(DeviceResource.msgOpenDoorTimePeriod, model.OpenPeriod);
                        break;
                    case Constants.CommandType.Open when !string.IsNullOrWhiteSpace(model.OpenUntilTime):
                        var account = _userService.GetAccountById(_httpContext.User.GetAccountId());
                        message = string.Format(DeviceResource.msgOpenDoorUtilTime, DateTime.ParseExact(model.OpenUntilTime, Constants.DateTimeFormat.ddMMyyyyHHmmsszzz, null)
                            .ConvertToUserTime(account?.TimeZone).ToSettingDateTimeString());
                        break;
                    case Constants.CommandType.ForceClose:
                        message = DeviceResource.msgForcedCloseSuccess;
                        break;
                    case Constants.CommandType.Release:
                        message = DeviceResource.msgForcedReleaseSuccess;
                        break;
                    default:
                        message = string.Format(MessageResource.SendMessageDeviceInstruction, DeviceResource.lblDevice.ToLower());
                        break;
                }

                return new ApiSuccessResult(StatusCodes.Status200OK, message);
            }
        }

        /// <summary>
        /// send device instruction command by using RID
        /// </summary>
        /// <param name="model">This model has information about instruction.</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: command in Model JSON not null</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [AllowAnonymous]
        [Route(Constants.Route.ApiDeviceInstructionRID)]
        public IActionResult SendDeviceInstructionRID([FromBody] DeviceInstructionRID model)
        {
            var devices = _deviceService.GetByRidsAndCompany(model.Rids, _httpContext.User.GetCompanyId());
            if (devices == null || devices.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }

            if (string.IsNullOrWhiteSpace(model.Command))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.CommandCanBeNotEmpty);
            }

            var offlineDeviceAddr = "";

            var company = _companyService.GetById(_httpContext.User.GetCompanyId());
            List<IcuDevice> onlineDevices;
            if (company != null && !company.AutoSyncUserData)
                onlineDevices = _deviceService.GetOnlineDevices(devices, ref offlineDeviceAddr);
            else
                onlineDevices = devices;

            onlineDevices = _deviceService.GetOnlineDevices(devices, ref offlineDeviceAddr);

            DeviceInstruction newModel = new DeviceInstruction()
            {
                Ids = devices.Select(m => m.Id).ToList(),
                Command = model.Command,
                OpenPeriod = model.OpenPeriod,
                OpenUntilTime = model.OpenUntilTime,
                //LocalMqtt = model.LocalMqtt
            };

            _deviceService.SendInstruction(onlineDevices, newModel);

            if (onlineDevices.Count == 0 || !string.IsNullOrEmpty(offlineDeviceAddr))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgOfflineDevice, offlineDeviceAddr));
            }
            else
            {
                return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.SendMessageDeviceInstruction, DeviceResource.lblDevice.ToLower()));
            }
        }

        /// <summary>
        /// Reinstall the devices
        /// </summary>
        /// <param name="deviceDetails">List of ReinstallDeviceDetail class.
        ///                             This class includes DeviceId and ProcessId.
        ///                             DeviceId is device id to reinstall.
        ///                             ProcessId is process id made by web app to show percentage through progress bar.</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: address of Device Offline not null or list of device online empty</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Device Id in List of Model Service does not exist in DB</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiReinstallDevices)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Reinstall + Page.DeviceSetting)]
        public IActionResult ReinstallDevices([FromBody] List<ReinstallDeviceDetail> deviceDetails)
        {
            var ids = new List<int>();

            foreach (var deviceDetail in deviceDetails)
            {
                ids.Add(deviceDetail.DeviceId);
            }

            var devices = _deviceService.GetByIdsAndCompany(ids, _httpContext.User.GetCompanyId(), true).ToList();
            if (devices.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }

            var offlineDeviceAddr = "";

            var company = _companyService.GetById(_httpContext.User.GetCompanyId());
            List<IcuDevice> onlineDevices;
            if (company != null && !company.AutoSyncUserData)
                onlineDevices = _deviceService.GetOnlineDevices(devices, ref offlineDeviceAddr);
            else
                onlineDevices = devices;

            _deviceService.Reinstall(onlineDevices, false, deviceDetails);

            if (onlineDevices.Count == 0 || !string.IsNullOrEmpty(offlineDeviceAddr))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgOfflineDevice, offlineDeviceAddr));
            }
            else
            {
                return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.IsReinstalling));
            }
        }

        /// <summary>
        /// Copy device
        /// </summary>
        /// <param name="id">Device Id template</param>
        /// <param name="ids">JSON model for list of Device Ids to copy</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: address of Device Offline not null or list of device online empty</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiCopyDevices)]
        //[Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Copy + Page.DeviceSetting)]
        public IActionResult CopyDevices(int id, [FromBody] List<int> ids)
        {
            var deviceCopy = _deviceService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
            var devices = _deviceService.GetByIdsAndCompany(ids, _httpContext.User.GetCompanyId()).ToList();

            if (deviceCopy == null || devices.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }

            // Check device status ( device to be pasted )
            var offlineDeviceAddr = "";

            var company = _companyService.GetById(_httpContext.User.GetCompanyId());
            List<IcuDevice> onlineDevices;
            if (company != null && !company.AutoSyncUserData)
                onlineDevices = _deviceService.GetOnlineDevices(devices, ref offlineDeviceAddr);
            else
                onlineDevices = devices;

            _deviceService.CopyDevices(deviceCopy, onlineDevices);

            if (onlineDevices.Count == 0 || !string.IsNullOrEmpty(offlineDeviceAddr))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgOfflineDevice, offlineDeviceAddr));
            }
            else
            {
                return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.CopySuccess, DeviceResource.lblDevice.ToLower()));
            }

        }

        /// <summary>
        /// Get device type list
        /// </summary>
        /// <param name="ids">List of Device Ids</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Device Id in List of Model Service does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDeviceTypes)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetListDeviceTypes(List<int> ids)
        {
            var devices = _deviceService.GetByIds(ids);
            if (devices == null || devices.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }

            var model = _deviceService.GetListDeviceType(devices);
            return Ok(model);
        }

        /// <summary>
        /// file upload to update device
        /// </summary>
        /// <param name="models">file upload include list of device to update</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: File not found</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiDeviceUploadFile)]
        [Consumes("multipart/form-data")]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UploadFileProgress(IFormCollection models)
        {
            var files = models.Files;
            if (files == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound);
            }
            _deviceService.UploadFile(files);
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.UploadFile);
        }

        
        /// <summary>
        /// get Transmit information
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiTransmitInfo)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetTransmitInfo()
        {
            var transmitData = _deviceService.GetTransmitAllData();
            return Ok(transmitData);
        }
        
        /// <summary>
        /// Stop update firmware for list of device
        /// </summary>
        /// <param name="ids">list device ids</param>
        /// <param name="processIds">list of process ids</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Device Id in List of Model Service does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiStopProcess)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult StopProcess(List<int> ids, [FromBody] List<string> processIds)
        {
            if (processIds == null || processIds.Count == 0 || ids == null || ids.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgStopProcess);
            }

            var devices = _deviceService.GetByIds(ids);

            var stopInstruction = new DeviceInstruction()
            {
                Ids = ids,
                Command = Constants.CommandType.StopUpdateFW,
                OpenPeriod = 0,
                OpenUntilTime = ""
            };

            _deviceService.StopProcess(devices, processIds);
            //_deviceService.SendInstruction(devices, stopInstruction);
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.msgStopSuccess);
        }

        /// <summary>
        /// transmit data to device(s).
        /// </summary>
        ///
        /// <remarks>   Edward, 2020-02-03. </remarks>
        ///
        /// <param name="model">    This model has information about the device to receive data.  </param>
        ///
        /// <returns>   An IActionResult. </returns>
        /// <response code="400">Bad Request: address of Device Offline not null or list of device online empty</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiTransmitData)]
        //[Authorize(Policy = Constants.Policy.SystemAndSuperAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Reinstall + Page.DeviceSetting)]
        public IActionResult TransmitData([FromBody] TransmitDataModel model)
        {
            if (model == null || model.TransmitIds.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgEmptyTransmit);
            }

            if (model.Devices == null || model.Devices.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, string.Format(MessageResource.Required, DeviceResource.lblDevice.ToLower()));
            }

            var ids = new List<int>();

            foreach (var deviceDetail in model.Devices)
            {
                ids.Add(deviceDetail.DeviceId);
            }

            var devices = _deviceService.GetByIdsAndCompany(ids, _httpContext.User.GetCompanyId(), model.TransmitIds.Contains((int)TransmitType.UserInfo)).ToList();
            if (devices.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }

            var offlineDeviceAddr = "";
            //_deviceService.GetOnlineDevices(devices, ref offlineDeviceAddr);
            //if (!string.IsNullOrEmpty(offlineDeviceAddr))
            //{
            //    return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgDeviceDisconnected, offlineDeviceAddr));
            //}
            var company = _companyService.GetById(_httpContext.User.GetCompanyId());
            List<IcuDevice> onlineDevices;
            if (company != null && !company.AutoSyncUserData)
                onlineDevices = _deviceService.GetOnlineDevices(devices, ref offlineDeviceAddr);
            else
                onlineDevices = devices;

            // check door is already in department with department level
            bool doorsInvalid = _departmentService.CheckDoorInDepartment(_httpContext.User.GetCompanyId(),
                _httpContext.User.GetAccountId(), onlineDevices.Select(x => x.Id).ToList());
            if (!doorsInvalid)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, AccessGroupResource.msgNotPermissionDevices);
            }
            _deviceService.TransmitData(model, onlineDevices.ToList());

            if (onlineDevices.Count == 0 || !string.IsNullOrEmpty(offlineDeviceAddr))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgDeviceDisconnected, offlineDeviceAddr));
            }
            else
            {
                if (model.UserIds != null && model.UserIds.Count != 0)
                {
                    return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.msgSuccessToSend);
                }
                else
                {
                    return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.msgTransmitSuccess);
                }
            }
        }

        /// <summary>
        /// Get information user of Device
        /// </summary>
        /// <param name="id">Device Id</param>
        /// <param name="cardId">Card Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDevicesUsersInfo)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUserInfo(int id, string cardId)
        {
            var device = _deviceService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
            if (device == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }

            var userInfo = _deviceService.GetUserInfo(device, cardId);
            return Ok(userInfo);
        }

        /// <summary>
        /// Get user infomation by user's card ID
        /// </summary>
        /// <param name="id">device ID</param>
        /// <param name="cardId">Card ID</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDevicesUsersInfoByCardId)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUserInfoByCardId(int id, string cardId)
        {
            var device = _deviceService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
            if (device == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }
            // check door is already in department with department level
            bool doorsInvalid = _departmentService.CheckDoorInDepartment(_httpContext.User.GetCompanyId(),
                _httpContext.User.GetAccountId(), new List<int>() { id });
            if (!doorsInvalid)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, AccessGroupResource.msgNotPermissionDevices);
            }
            var userInfo = _deviceService.GetUserInfoByCardId(device, cardId);
            return Ok(userInfo);
        }


        /// <summary>
        /// Get user infomation by user's card ID
        /// Search device by deviceAddress
        /// </summary>
        /// <param name="address">device address number</param>
        /// <param name="cardId">Card ID</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUserInfoByCardId)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUserInfoByCardIdwithRid(string address, string cardId)
        {
            var device = _deviceService.GetByDeviceAddress(address);
            if (device == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }

            var userInfo = _deviceService.CheckUserInfoInDevice(device, cardId, false);

            return Ok(userInfo);
        }

        /// <summary>
        /// get initialize data of device
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDeviceInitialize)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetList()
        {
            var model = _deviceService.InitializeData();
            return Ok(model);
        }


        /// <summary>
        /// Backup Request
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiBackupRequest)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult BackupRequest()
        {
            //유효성 체크(검증)
            _deviceService.RequestBackup();
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.msgTransmitSuccess);
        }


        /// <summary>
        /// Set Max Icu Send User Count(default : 5) If input is 0, set default value.
        /// Set Delay between sending user protocol (unit : ms)
        /// </summary>
        /// <param name="count">Max Icu Send User</param>
        /// <param name="delay">Delay between sending user protocol</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiTestSetSendIcuUserCount)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult SetMaxIcuSendUserCount(int count = 5, int delay = 0)
        {
            if (count == 0)
                Constants.MaxSendIcuUserCount = 5;
            else
                Constants.MaxSendIcuUserCount = count;

            Constants.MaxSendUserDelay = delay;


            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, DeviceResource.lblDevice.ToLower(), ""));
        }

        /// <summary>
        ///             gets device history.
        /// </summary>
        ///
        /// <remarks>   Edward, 2020-03-06. </remarks>
        ///
        /// <param name="id">           device id to get. </param>
        /// <param name="pageNumber">   (Optional) current page number. </param>
        /// <param name="pageSize">     (Optional) Number of items to show on a page. </param>
        ///
        /// <returns>   The device history. </returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDeviceHistory)]
        //[Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ViewHistory + Page.DeviceSetting)]
        public IActionResult GetDeviceHistory(int id, int pageNumber = 1, int pageSize = 10)
        {
            var device = _deviceService.GetById(id);

            if (device != null)
            {
                var deviceHistory = _deviceService.GetHistory(device, pageNumber, pageSize, out var totalRecords).ToList();

                var pagingData = new PagingData<DeviceHistoryModel>
                {
                    Data = deviceHistory,
                    Meta = { RecordsTotal = totalRecords, RecordsFiltered = totalRecords}
                };

                return Ok(pagingData);
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
        }

        /// <summary>
        /// Re-update uptime for all of list devices
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: List  of Device in DB empty</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiReUpdateUpTimeDevice)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ReUpdateUpTimeOnlineDevice()
        {
            try
            {
                int count = _deviceService.ReUpdateUpTimOnlineDevice();
                var reponse = new ResponseStatus();
                reponse.message = "Success";
                reponse.statusCode = true;
                reponse.data = count;
                return Ok(reponse);
            }
            catch (Exception ex)
            {
                ex.ToString();
                return new ApiErrorResult(StatusCodes.Status400BadRequest, ex.Message);
            }

        }

        /// <summary>
        /// Re-update uptime by id
        /// </summary>
        /// <param name="id">Device Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: List  of Device in DB empty</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiReUpdateUpTimeDeviceById)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ReUpdateUpTimeOnlineDeviceById(int id)
        {
            try
            {
                _deviceService.ReUpdateUpTimOnlineDeviceById(id);
                return Ok();
            }
            catch (Exception ex)
            {
                ex.ToString();
                return new ApiErrorResult(StatusCodes.Status400BadRequest, ex.Message);
            }

        }

        /// <summary>
        /// Config local mqtt for device
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <response code="400">Bad Request: command in Model JSON is null</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiDeviceConfigLocalMqtt)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ConfigLocalMqtt([FromBody] ConfigLocalMqttModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }
            var devices = _deviceService.GetByIds(model.DeviceIds);
            if (devices.Count == 0)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);

            var offlineDeviceAddr = "";

            var company = _companyService.GetById(_httpContext.User.GetCompanyId());
            List<IcuDevice> onlineDevices;
            if (company != null && !company.AutoSyncUserData)
                onlineDevices = _deviceService.GetOnlineDevices(devices, ref offlineDeviceAddr);
            else
                onlineDevices = devices;

            _deviceService.SendInstruction(onlineDevices, new DeviceInstruction()
            {
                Command = Constants.CommandType.SendConfigLocalMqtt,
                Ids = model.DeviceIds,
                LocalMqtt = model.LocalMqtt
            });

            if (onlineDevices.Count == 0 || !string.IsNullOrEmpty(offlineDeviceAddr))
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.msgDeviceDisconnected, offlineDeviceAddr));
            }

            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSuccess);
        }


        /// <summary>
        /// Turn on/off alarm (ICU 2nd relay)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiDevicesAlarm)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult TurnOnOrOffAlarm([FromBody] DeviceConfigAlarmModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }
            var devices = _deviceService.GetByIds(model.DeviceIds);
            if (devices.Count == 0)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);

            var offlineDeviceAddr = "";

            var company = _companyService.GetById(_httpContext.User.GetCompanyId());
            List<IcuDevice> onlineDevices;
            if (company != null && !company.AutoSyncUserData)
                onlineDevices = _deviceService.GetOnlineDevices(devices, ref offlineDeviceAddr);
            else
                onlineDevices = devices;

            _deviceService.SendInstruction(onlineDevices, new DeviceInstruction()
            {
                Command = model.Status ? Constants.CommandType.TurnOnAlarm : Constants.CommandType.TurnOffAlarm,
                Ids = model.DeviceIds
            });

            if (onlineDevices.Count == 0 || !string.IsNullOrEmpty(offlineDeviceAddr))
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.msgDeviceDisconnected, offlineDeviceAddr));
            }

            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSuccess);
        }


        /// <summary>
        /// Check the RID is existing in server.
        /// </summary>
        /// <param name="model"> This model has the address of device. </param>
        /// <returns></returns>
        /// <response code="400">Bad Request: command in Model JSON not null</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Device Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [AllowAnonymous]
        [Route(Constants.Route.ApiDeviceExistRID)]
        public IActionResult CheckRIDExist([FromBody] DeviceRidModel model)
        {
            var devices = _deviceService.GetByRidsAndCompany(new List<string>() { model.Rid }, 0);

            if (devices.Count != 0)
            {
                return new ApiSuccessResult(StatusCodes.Status200OK, $"{model.Rid} : {DeviceResource.msgDeviceAddressInUse}");
            }
            else
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }
        }

        /// <summary>
        /// Get sub-display device info (last of event-log) by token device
        /// </summary>
        /// <param name="token">token device</param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiDeviceSubDisplayInfo)]
        [AllowAnonymous]
        public IActionResult GetSubDeviceInfo(string token)
        {
            var data = _deviceService.GetSubDisplayDeviceInfoByToken(token);
            if (data == null)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.InvalidToken);

            return Ok(data);
        }


        /// <summary>
        /// Assign users to device.
        /// </summary>
        /// <param name="id">       Device identifier. </param>
        /// <param name="userIds">  List of identifiers for the users. </param>
        /// <returns>   An IActionResult. </returns>
        /// <response code="400">Bad Request: Access group Id does not exist, list of User Ids empty or user assigned for access group</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPost]
        [Route(Constants.Route.ApiDeviceAssignUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessGroup)]
        public IActionResult AssignUsersToDevice(int id, [FromBody] List<int> userIds)
        {
            if (id == 0 || userIds == null || userIds.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.msgEmptyUser);
            }
            // check door is already in department with department level
            bool doorsInvalid = _departmentService.CheckDoorInDepartment(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId(), new List<int>() { id });
            if (!doorsInvalid)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, AccessGroupResource.msgNotPermissionDevices);
            }
            var result = _deviceService.AssignUsersToDevice(id, userIds);
            if (!string.IsNullOrEmpty(result))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, result);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, AccessGroupResource.msgAssignUsersToDoor);
        }


        /// <summary>
        /// Un-assign users to device.
        /// </summary>
        /// <param name="id">       Device identifier. </param>
        /// <param name="userIds">  List of identifiers for the users. </param>
        /// <param name="ok">       boolean value that means "ok" or "no" in dialog. </param>
        /// <returns>   An IActionResult. </returns>
        /// <response code="400">Bad Request: Access group Id does not exist, list of User Ids empty or user assigned for access group</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpDelete]
        [Route(Constants.Route.ApiDeviceUnassignUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessGroup)]
        public IActionResult UnassignUsersFromDevice(int id, [FromBody] List<int> userIds, bool ok = false)
        {
            if (id == 0 || userIds == null || userIds.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.msgEmptyUser);
            }

            // check door is already in department with department level
            bool doorsInvalid = _departmentService.CheckDoorInDepartment(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId(), new List<int>() { id });
            if (!doorsInvalid)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, AccessGroupResource.msgNotPermissionDevices);
            }
            var result = _deviceService.UnassignUsersFromDevice(id, userIds, ok);
            if (!string.IsNullOrEmpty(result))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, result);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, AccessGroupResource.msgUnAssignUsersFromDoor);
        }

        /// <summary>
        /// Device will use this api to upload file logs
        /// </summary>
        /// <param name="rid">device address</param>
        /// <param name="msgId">message id of mqtt</param>
        /// <param name="hash">hash md5 (msgId + rid + companyCode)</param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(Constants.Route.ApiDevicesFileLogs)]
        public IActionResult UploadLogDevice(string rid, string msgId, string hash, IFormFile file)
        {
            var device = _deviceService.GetByDeviceAddress(rid);
            if (device == null || !device.CompanyId.HasValue)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);

            var company = _companyService.GetById(device.CompanyId.Value);
            if (company == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);

            if (string.IsNullOrEmpty(hash) || !CryptographyHelper.VerifyMD5Hash(msgId + rid + company.Code, hash))
                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.Unauthorized);

            _deviceService.UploadFileLogDevice(device, msgId, file);
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSuccess);
        }

        /// <summary>
        /// Get all file logs of device by deviceId
        /// </summary>
        /// <param name="deviceId">identity of device</param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiDevicesFileLogs)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ViewHistory + Page.DeviceSetting)]
        public IActionResult GetAllLogsOfDevice(int deviceId)
        {
            var device = _deviceService.GetById(deviceId);
            if (device == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            }

            return Ok(_deviceService.GetAllLogsOfDevice(device));
        }

        /// <summary>
        /// Download file log of device
        /// </summary>
        /// <param name="id">device id</param>
        /// <param name="fileName">if fileName is empty, the server will send to all logs (zip file)</param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiDevicesIdFileLogs)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ViewHistory + Page.DeviceSetting)]
        public IActionResult DownloadFileLogOfDevice(int id, string fileName)
        {
            var device = _deviceService.GetById(id);
            if (device == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);
            if (string.IsNullOrEmpty(fileName))
            {
                var data = _deviceService.GetFileLogOfDevice(device);
                if (data == null)
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.msgFileNotFound);

                return File(data, "application/zip", $"log_{device.DeviceAddress}_{DateTime.UtcNow.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss)}_UTC.zip");
            }
            else
            {
                var data = _deviceService.GetFileLogOfDevice(device, fileName);
                if (data == null)
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.msgFileNotFound);

                return File(data, FileHelpers.GetContentTypeByFileName(fileName), fileName);
            }
        }

        /// <summary>
        /// Request file log of device
        /// </summary>
        /// <param name="id">device id</param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiDevicesIdRequestFileLogs)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ViewHistory + Page.DeviceSetting)]
        public IActionResult RequestLogFile(int id)
        {
            var device = _deviceService.GetById(id);
            if (device == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);

            if (device.ConnectionStatus != (short)ConnectionStatus.Online)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.msgDeviceDisconnected, device.Name));

            string msgId = Guid.NewGuid().ToString();
            var result = _deviceService.SendDeviceRequest(msgId, Constants.CommandType.RequestLogFile, device);
            if (!result)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.SystemError);

            string msgIdMqtt = Helpers.CreateMsgIdProcess(msgId, 0, 0, Constants.CommandType.RequestLogFile);
            return new ApiSuccessResult(StatusCodes.Status200OK, DeviceResource.msgYourRequestSuccess, "{\"id\":\"" + msgIdMqtt + "\"}");
        }

        
        /// <summary>
        /// Get registered cards 
        /// </summary>
        /// <param name="id"> identifier of device </param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDevicesRegisteredCardsByTypes)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.RegisteredUser)]
        public IActionResult GetRegisteredCardsByType(int id)
        {
            // Return result model that is consisted by cardTypes.
            // model has 3 properties.
            // 1. index value of cardType
            // 2. name of cardType
            // 3. number of cards

            // Get device data by id value.
            var device = _deviceService.GetById(id);
            // If there is not the device data, return 400.
            if (device == null) return new ApiErrorResult(StatusCodes.Status400BadRequest);

            var result = _deviceService.GetRegisteredCardsByType(id);

            return Ok(result);
        }
    }
}