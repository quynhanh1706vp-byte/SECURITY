using System;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Dashboard;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Dashboard controller
    /// </summary>
    [Produces("application/json")]
    [CheckPermission(ActionName.View + Page.DashBoard)]
    public class DashboardController : Controller
    {
        private readonly IDeviceService _deviceService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly INotificationService _notificationService;
        private readonly HttpContext _httpContext;
        private readonly ICompanyService _companyService;
        private readonly IAccountService _accountService;
        private readonly IAttendanceService _attendanceService;
        private readonly IEventLogService _eventLogService;
        private readonly IPluginService _pluginService;
        private readonly IVisitService _visitService;
        private readonly IVehicleService _vehicleService;

        /// <summary>
        /// Device controller
        /// </summary>
        /// <param name="deviceService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="userService"></param>
        /// <param name="roleService"></param>
        /// <param name="notificationService"></param>
        /// <param name="companyService"></param>
        /// <param name="accountService"></param>
        /// <param name="eventLogService"></param>
        /// <param name="attendanceService"></param>
        /// <param name="pluginService"></param>
        /// <param name="visitService"></param>
        public DashboardController(IDeviceService deviceService,
            IHttpContextAccessor httpContextAccessor, IUserService userService,
            IRoleService roleService, INotificationService notificationService,
            ICompanyService companyService, IAccountService accountService,
            IEventLogService eventLogService, IAttendanceService attendanceService,
            IPluginService pluginService, IVisitService visitService, IVehicleService vehicleService)
        {
            _deviceService = deviceService;
            _userService = userService;
            _roleService = roleService;
            _httpContext = httpContextAccessor.HttpContext;
            _notificationService = notificationService;
            _companyService = companyService;
            _accountService = accountService;
            _attendanceService = attendanceService;
            _eventLogService = eventLogService;
            _pluginService = pluginService;
            _visitService = visitService;
            _vehicleService = vehicleService;
        }

        /// <summary>
        /// Return total number for admin dashboards
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDashboardAdmin)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[CheckPermission(PermissionGroupName.Device, PermissionActionName.View)]
        public IActionResult AdminDashboard()
        {
            var totalCompanies = _companyService.GetCompanies().Count();

            var devices = _deviceService.GetDevicesForDashBoard((short)PreferredSystem.AccessSystem, new List<int>());
            var onCount = devices.Where(m => m.ConnectionStatus == (int)ConnectionStatus.Online).Count();
            var offCount = devices.Where(m => m.ConnectionStatus == (int)ConnectionStatus.Offline).Count();

            DateTime end = DateTime.UtcNow;
            DateTime startToday = DateTime.UtcNow.Date;
            DateTime startMin = DateTime.MinValue;
            AdminDashboardModel dashboardModel = new AdminDashboardModel
            {
                TotalCompanies = totalCompanies,
                TotalOnlineDevices = onCount,
                TotalOfflineDevices = offCount,
                TotalDevices = devices.Count(),
                TotalAccounts = _accountService.GetSystemTotalAccount(),
                TotalEventLogs = _eventLogService.GetTotalEventLogs(startMin, end),
                TotalEventNormalAccess = _eventLogService.GetEventNormalAccess(startMin, end),
                TotalEventAbNormalAccess = _eventLogService.GetEventAbNormalAccess(startMin, end),
                TotalEventLogsToday = _eventLogService.GetTotalEventLogs(startToday, end),
                TotalEventNormalAccessToday = _eventLogService.GetEventNormalAccess(startToday, end),
                TotalEventAbNormalAccessToday = _eventLogService.GetEventAbNormalAccess(startToday, end)
            };
            return Ok(dashboardModel);
        }


        /// <summary>
        /// Return number for company dashboard
        /// </summary>
        /// <param name="accessDate"></param>
        /// <param name="type">0 - both, 1 - only info number</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDashboardAccess)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[CheckPermission(PermissionGroupName.Device, PermissionActionName.View)]
        public IActionResult AccessDashboard(string accessDate, int type)
        {
            var dataAccess = accessDate.ConvertDefaultStringToDateTime() ?? DateTime.Now.Date;
            dataAccess = dataAccess == DateTime.MinValue ? DateTime.Now.Date : dataAccess.Date;
            int companyId = _httpContext.User.GetCompanyId();
            int accountId = _httpContext.User.GetAccountId();

            DateTime c4 = DateTime.Now;
            // [DashBoard] Get device data4
            var devices = _deviceService.GetDevicesForDashBoard((short)PreferredSystem.AccessSystem, new List<int>(companyId));
            var onCount = devices.Where(m => m.ConnectionStatus == (int)ConnectionStatus.Online).Count();
            var offCount = devices.Where(m => m.ConnectionStatus == (int)ConnectionStatus.Offline).Count();
            var doorStatus = EnumHelper.ToEnumList<DoorStatus>();

            DateTime c5 = DateTime.Now;
            // [DashBoard] Make Chart data
            List<EventChartDataModel> eventChartData = new List<EventChartDataModel>();

            if(type != 1)
            {
                eventChartData = _eventLogService.GetAccessChartDataByDoor(devices, dataAccess);
            }
            

            DateTime c6 = DateTime.Now;
            // [DashBoard] Make Dash Board Model
            AccessDashboardModel dashboardModel = new AccessDashboardModel
            {
                TotalOnlineDevices = onCount,
                TotalOfflineDevices = offCount,
                TotalDevices = devices.Count(),
                TotalUsers = _userService.GetTotalUserByCompany(companyId),
                TotalUsersIn = _eventLogService.GetTotalNormalPersonAccessByDay(companyId, dataAccess, accountId, true, true),
                TotalUsersOut = _eventLogService.GetTotalNormalPersonAccessByDay(companyId, dataAccess, accountId, true, false),
                TotalUsersAccess = _eventLogService.GetUniqueUserAccessByDay(companyId, dataAccess),
                TotalAccessEvents = _eventLogService.GetTotalNormalAccessByDay(companyId, dataAccess, accountId),
                TotalAbnormalEvents = _eventLogService.GetTotalAbnormalAccessByDay(companyId, dataAccess, accountId),
                TotalUnknownPerson = _eventLogService.GetEventByTypeAccess(new List<int>(){(short) EventType.UnknownPerson}, dataAccess.Date, dataAccess.Date.AddDays(1)),
                EventChartData = eventChartData,
                DoorStatus = doorStatus,
                TotalVisits = _visitService.GetTotalVisitByCompany(companyId),
                TotalVisitsIn = _eventLogService.GetTotalNormalPersonAccessByDay(companyId, dataAccess, accountId, false, true),
                TotalVisitsOut = _eventLogService.GetTotalNormalPersonAccessByDay(companyId, dataAccess, accountId, false, false),
            };

            DateTime c7 = DateTime.Now;

            Console.WriteLine("[DashBoard] Get device data             {0}", c5.Subtract(c4).TotalMilliseconds);
            Console.WriteLine("[DashBoard] Make Chart data             {0}", c6.Subtract(c5).TotalMilliseconds);
            Console.WriteLine("[DashBoard] Make Dash Board Model       {0}", c7.Subtract(c6).TotalMilliseconds);

            return Ok(dashboardModel);
        }


        /// <summary>
        /// Return number for company dashboard
        /// </summary>
        /// <param name="attendanceDate">String for DateTime of Attendance</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Condition of Plugin Service null</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDashboardAttendance)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[CheckPermission(PermissionGroupName.Device, PermissionActionName.View)]
        public IActionResult AttendanceDashboard(string attendanceDate)
        {
            int companyId = _httpContext.User.GetCompanyId();
            if (!_pluginService.CheckPluginCondition(Constants.PlugIn.TimeAttendance, companyId))
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }
            //attendanceDate = DateTime.SpecifyKind(attendanceDate, DateTimeKind.Unspecified);
            // DateTime startTime = attendanceDate.Date;
            // List<int> WorkingUsersChart = _eventLogService.GetWorkingUserCount(companyId, startTime);
            AttendanceTypeChart attendanceTypeChart = new AttendanceTypeChart()
            {
                Labels = new List<string>(),
                Data = new List<int>()
            };
            var date = attendanceDate.ConvertDefaultStringToDateTime() ?? DateTime.Now;
            Dictionary<string, int> attendanceCount = _attendanceService.GetAttendanceTypeCount(companyId, date);

            int totalAbsentUsers = attendanceCount[EnumHelper.GetDescription(AttendanceType.AbsentNoReason)];
            int totalLateUsers = attendanceCount[EnumHelper.GetDescription(AttendanceType.LateIn)]
                                + attendanceCount[EnumHelper.GetDescription(AttendanceType.LateInEarlyOut)];
            
            //Get data for attendance chart
            foreach (KeyValuePair<string, int> entry in attendanceCount)
            {
                // do something with entry.Value or entry.Key
                if (entry.Value > 0)
                {
                    attendanceTypeChart.Labels.Add(entry.Key);
                    attendanceTypeChart.Data.Add(entry.Value);
                }

            }
            
            var dataUserWorkingEveryDepartment = _eventLogService.GetWorkingUserCountEveryDepartment(companyId, date);
            var datasets = new List<DataLineChart>();
            int totalUserInOffice = 0;
            foreach (var item in dataUserWorkingEveryDepartment)
            {
                datasets.Add(new DataLineChart()
                {
                    Label = item.Key,
                    Data = new List<int>(item.Value)
                });
                totalUserInOffice += item.Value.Last();
            }
            AttendanceDashboardModel dashboardModel = new AttendanceDashboardModel
            {
                // Datasets = datasets,
                TotalUserInOffice = totalUserInOffice,
                TotalAbsentUsers = totalAbsentUsers,
                TotalLateUsers = totalLateUsers,
                AttendanceTypeChart = attendanceTypeChart,
                WorkingUsersChart = datasets,
            };
            return Ok(dashboardModel);
        }


        /// <summary>
        /// Return number for company dashboard
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDashboardVisit)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[CheckPermission(PermissionGroupName.Device, PermissionActionName.View)]
        public IActionResult VisitDashboard(string visitDate)
        {
            var dataAccess = visitDate.ConvertDefaultStringToDateTime() ?? DateTime.Now.Date;
            dataAccess = dataAccess == DateTime.MinValue ? DateTime.Now.Date : dataAccess.Date;
            int companyId = _httpContext.User.GetCompanyId();
            int accountId = _httpContext.User.GetAccountId();
            if (!_pluginService.CheckPluginCondition(Constants.PlugIn.VisitManagement, companyId))
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            var devices = _deviceService.GetDevicesForDashBoard((short) PreferredSystem.AccessSystem, new List<int>(companyId));

            // Check if company enable visit management plugin
            int totalVisitorAccess = _eventLogService.GetTotalVisitorAccessByDay(companyId, dataAccess, accountId);
            int totalAwaitingVisitors = _visitService.GetRequestApprovalCount(companyId, accountId);
            int totalRegisteredVisitor = _visitService.GetTodayRegisteredVisitorsCount(companyId, dataAccess);
            var visitChartData = _eventLogService.GetAccessChartDataByDoor(devices, dataAccess, true);

            VisitDashboardModel dashboardModel = new VisitDashboardModel
            {
                TotalAwaitingVisitors = totalAwaitingVisitors,
                TotalVisitorAccess = totalVisitorAccess,
                TotalRegisteredVisitor = totalRegisteredVisitor,
                VisitChartData = visitChartData,
                TotalVisits = _visitService.GetTotalVisitByCompany(companyId),
                TotalVisitsIn = _eventLogService.GetTotalNormalPersonAccessByDay(companyId, dataAccess, accountId, false, true),
                TotalVisitsOut = _eventLogService.GetTotalNormalPersonAccessByDay(companyId, dataAccess, accountId, false, false),
            };

            return Ok(dashboardModel);
        }

        /// <summary>
        /// Return list of disconnected devices in the last 24 hours
        /// </summary>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by index of the column. Example </param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDashboardRecentlyDisconnectedDevices)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[CheckPermission(PermissionGroupName.Device, PermissionActionName.View)]
        public IActionResult RecentlyDisconnectedDevices(int pageNumber = 1, int pageSize = 10, int sortColumn = 0,
            string sortDirection = "desc")
        {
            var devices = _deviceService.getRecentlyDisconnectedDevices(pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered).ToList();

            var pagingData = new PagingData<RecentlyDisconnectedDeviceModel>
            {
                Data = devices,

                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }
        
        /// <summary>
        /// get visit application request, access application request and 5 notices
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDashboardNotice)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult NoticeDashboard()
        {
            //return Ok(_notificationService.GetDashboardNotice(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId()));
            return Ok(_notificationService.GetDashboardNotice(_httpContext.User.GetCompanyId()));
        }
        
        /// <summary>
        /// get dash board of vehicle
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDashboardVehicle)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult VehicleDashBoard(string accessDate)
        {
            var dataAccess = accessDate.ConvertDefaultStringToDateTime() ?? DateTime.Now.Date;
            dataAccess = dataAccess == DateTime.MinValue ? DateTime.Now.Date : dataAccess.Date;
            return Ok(_vehicleService.GetDashBoardVehicles(dataAccess));
        }
    }
}