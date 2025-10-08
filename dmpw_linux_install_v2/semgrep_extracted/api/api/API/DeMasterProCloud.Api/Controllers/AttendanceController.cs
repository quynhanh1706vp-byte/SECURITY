using System;
using System.Collections.Generic;
using System.Globalization;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.WorkingModel;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Attendance;
using DeMasterProCloud.DataModel.EventLog;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using System.IO.Compression;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Attendance controller
    /// </summary>
    [Produces("application/ms-excel", "application/json", "application/text")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [CheckAddOnAttribute(Constants.PlugIn.TimeAttendance)]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        private readonly HttpContext _httpContext;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly ICompanyService _companyService;
        private readonly IEventLogService _eventLogService;
        private readonly IConfiguration _configuration;

        public AttendanceController(IAttendanceService attendanceService, IHttpContextAccessor httpContextAccessor,
            IUserService userService, ICompanyService companyService,
            IAccountService accountService, IEventLogService eventLogService, IConfiguration configuration)
        {
            _attendanceService = attendanceService;
            _httpContext = httpContextAccessor.HttpContext;
            _userService = userService;
            _companyService = companyService;
            _accountService = accountService;
            _eventLogService = eventLogService;
            _configuration = configuration;
        }

        /// <summary>
        /// Get init page attendances management
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiAttendancesInit)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetInit()
        {
            return Ok(_attendanceService.GetInit(_httpContext.User.GetCompanyId()));
        }

        /// <summary>
        /// Get Attendance of Users with company
        /// </summary>
        /// <param name="search">Query string that filter Attendance by Name</param>
        /// <param name="departmentId">Id of department</param>
        /// <param name="attendanceType">Type of Attendance</param>
        /// <param name="timezone">String of Timezone</param>
        /// <param name="start">String time start</param>
        /// <param name="end">String time end</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending</param>
        /// <param name="userId">UserId: Id of user</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiGetAttendanceByCompany)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.TimeAttendanceReport)]
        public IActionResult Get(string search, string departmentId, string attendanceType, string timezone, DateTime start, DateTime end, int pageNumber = 1, int pageSize = 10, string sortColumn = "UserName",
            string sortDirection = "asc", int userId = 0)
        {
            var attendance = _attendanceService
                .GetPaginatedAttendanceReport(search, departmentId, attendanceType, timezone, start, end, pageNumber, pageSize, sortColumn, sortDirection, userId, out var recordsTotal,
                    out var recordsFiltered).ToList();

            var pagingData = new PagingData<AttendanceListReportModel>
            {
                Data = attendance,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get attendance report detail by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEditAttendance)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.TimeAttendanceReport)]
        public IActionResult GetById(int id)
        {
            var attendance = _attendanceService.GetAttendanceById(id);
            if (attendance == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);

            return Ok(attendance);
        }

        /// <summary>
        /// Re-Calculate Attendances about search data.
        /// </summary>
        /// <param name="search">Query string that filter Attendance by Name</param>
        /// <param name="departmentId">Id of department</param>
        /// <param name="attendanceType">Type of Attendance</param>
        /// <param name="timezone">String of Timezone</param>
        /// <param name="start">String time start</param>
        /// <param name="end">String time end</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending</param>
        /// <param name="userId">UserId: Id of user</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpPatch]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiReCalculateAttendance)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.TimeAttendanceReport)]
        public IActionResult Recalculate(string search, string departmentId, string attendanceType, string timezone, DateTime start, DateTime end, int pageNumber = 1, int pageSize = 10, string sortColumn = "UserName",
            string sortDirection = "asc", int userId = 0)
        {
            _attendanceService
                .ReCalculateAttendance(search, departmentId, attendanceType, start, end, pageNumber, pageSize, sortColumn, sortDirection, userId);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                                        string.Format(MessageResource.UpdateSuccessAttendance));
        }

        /// <summary>
        /// Get list of Access history of User with attendance by id
        /// </summary>
        /// <param name="id">identified of attendance</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="404">Not Found: User does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAttendanceAccessHistory)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ViewHistory + Page.TimeAttendanceReport)]
        public IActionResult GetAccessHistory(int id, int pageNumber = 1, int pageSize = 10, string sortColumn = "AccessTime", string sortDirection = "desc")
        {
            var attendance = _attendanceService.GetByIdAndCompanyId(_httpContext.User.GetCompanyId(), id);
            if (attendance == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            var workDay = JsonConvert.DeserializeObject<WorkingTime>(attendance.WorkingTime);
            List<EventLogHistory> accessHistory;
            int recordsFiltered, recordsTotal;
            if (TimeSpan.Parse(workDay.Start) < TimeSpan.Parse(workDay.End))
            {
                accessHistory = _eventLogService.GetAccessHistoryAttendance(attendance.UserId, attendance.Date, attendance.Date,
                    0, null, 0,
                    pageNumber, pageSize, sortColumn, sortDirection, out recordsTotal, out recordsFiltered).ToList();
            }
            else
            {
                accessHistory = _eventLogService.GetAccessHistoryByAttendanceId(id, pageNumber, pageSize, sortColumn, sortDirection, out recordsTotal, out recordsFiltered).ToList();
            }

            var pagingData = new PagingData<EventLogHistory>
            {
                Data = accessHistory,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }

        /// <summary>
        /// Load old event log to attendance list
        /// </summary>
        /// <param name="companyId">identified of company</param>
        /// <param name="fromAccessTime">String of time start</param>
        /// <param name="toAccessTime">String of time end</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpPatch]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiReCheckAttendanceFromEventLog)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Recheck(int companyId = 0, string fromAccessTime = null, string toAccessTime = null)
        {
            _attendanceService.Recheck(Constants.Attendance.RangTimeAllDay, companyId, fromAccessTime, toAccessTime);
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.UpdateSuccessAttendance);
        }

        /// <summary>
        /// Get setting param recheck attendance
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiReCheckAttendanceSetting)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult SettingRecheck(int id)
        {
            var company = _companyService.GetById(id);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            return Ok(new SettingRecheckAttendance()
            {
                UpdateAttendanceRealTime = company.UpdateAttendanceRealTime,
                TimeRecheckAttendance = company.TimeRecheckAttendance
            });
        }

        /// <summary>
        /// Setting param recheck attendance
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiReCheckAttendanceSetting)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult SettingRecheck(int id, [FromBody] SettingRecheckAttendance model)
        {
            var company = _companyService.GetById(id);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }
            _companyService.UpdateSettingRecheckAttendance(id, model);

            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.UpdateSuccessAttendance);
        }

        /// <summary>
        /// Edit Attendance 
        /// </summary>
        /// <param name="id">identified of attendance</param>
        /// <param name="model">JSON model for object(type attendance, string of time in, string of time out)</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Attendance Id does not exist in DB</response>
        /// <response code="422">Unprocessable Entity: Wrong data of clock in and out</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPatch]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEditAttendance)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.TimeAttendanceReport)]
        public IActionResult Edit(int id, [FromBody] AttendanceModel model)
        {
            try
            {
                if (id != 0)
                {
                    var attendance = _attendanceService.GetByIdAndCompanyId(_httpContext.User.GetCompanyId(), id);
                    if (attendance != null)
                    {
                        var user = _userService.GetById(attendance.UserId);
                        var attendanceSetting = _attendanceService.GetAttendanceSetting(_httpContext.User.GetCompanyId());
                        if (attendanceSetting != null)
                        {
                            if (attendanceSetting.ApprovarAccounts[0] != '[') attendanceSetting.ApprovarAccounts = $"[{attendanceSetting.ApprovarAccounts}]";
                            List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(attendanceSetting.ApprovarAccounts);

                            if (!accountIds.Contains(_httpContext.User.GetAccountId()))
                            {
                                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                                    string.Format(MessageResource.NotIsDepartmentManager));
                            }

                            if (Enum.IsDefined(typeof(AttendanceType), model.Type))
                            {
                                if (model.ClockInD <= model.ClockOutD)
                                {
                                    double totalWorkingHours = _attendanceService.Update(id, model);
                                    return new ApiSuccessResult(StatusCodes.Status200OK,
                                        string.Format(MessageResource.UpdateSuccessAttendance), totalWorkingHours.ToString());
                                }
                                else
                                {
                                    var basicTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                                    if (!user.WorkingType.CheckClockOut || model.ClockOutD == basicTime)
                                    {
                                        double totalWorkingHours = _attendanceService.Update(id, model);
                                        return new ApiSuccessResult(StatusCodes.Status200OK,
                                            string.Format(MessageResource.UpdateSuccessAttendance), totalWorkingHours.ToString());
                                    }
                                    else
                                    {
                                        return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                                        string.Format(MessageResource.TimeNotIncorrect));
                                    }
                                }
                            }
                            else
                            {
                                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                                    string.Format(MessageResource.TypeisNotDefine));
                            }
                        }
                        else
                        {
                            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                                string.Format(MessageResource.NotIsDepartmentManager));
                        }
                    }
                    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
                }
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }
            catch (Exception)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                                        string.Format(MessageResource.MsgFail));
            }
        }
        /// <summary>
        /// Get list type report attendance
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiTypesExportAttendance)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // [CheckPermission(ActionName.Export + Page.TimeAttendanceReport)]
        public IActionResult GetListTypesReport()
        {
            List<EnumModel> models = EnumHelper.ToEnumList<AttendanceReportType>();
            string language = _accountService.GetById(_httpContext.User.GetAccountId())?.Language;
            if (!string.IsNullOrEmpty(language))
            {
                foreach (var model in models)
                {
                    model.Name = AttendanceResource.ResourceManager.GetString("lblAttendanceReportType" + model.Id, new CultureInfo(language));
                }
            }

            return Ok(models);
        }


        /// <summary>
        /// Get users with Attendance
        /// </summary>
        /// <param name="start">String time start</param>
        /// <param name="end">String time end</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by index of the column. Example </param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Attendance Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiGetAttendanceRecordEachUser)]
        [Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdminAndEmployee, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetAttendanceRecordEachUser(DateTime start, DateTime end, int pageNumber = 1, int pageSize = 10, int sortColumn = 0,
            string sortDirection = "desc")
        {
            var user = _userService.GetUserByAccountId(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId());
            if (user != null)
            {
                var attendance = _attendanceService
                    .GetPaginatedAttendanceRecordEachUser(user.Id, start, end, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                        out var recordsFiltered).AsEnumerable().ToList();

                var pagingData = new PagingData<AttendanceListReportModel>
                {
                    Data = attendance,
                    Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
                };
                return Ok(pagingData);
            }

            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
        }

        /// <summary>
        /// Get attendance setting for company
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Attendance Setting Id does not exist in DB</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiSettingAttendance)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.UpdateAttendanceSetting + Page.TimeAttendanceReport)]
        public IActionResult GetAttendanceSetting()
        {
            var companyId = _httpContext.User.GetCompanyId();
            var model = _attendanceService.GetAttendanceSetting(companyId);
            if (model == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            return Ok(model);
        }

        /// <summary>
        /// Update approval accounts for attendance setting
        /// </summary>
        /// <param name="model">JSON model for AttendanceSettingModel</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiSettingAttendance)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.UpdateAttendanceSetting + Page.TimeAttendanceReport)]
        public IActionResult UpdateAttendanceSetting([FromBody] AttendanceSettingModel model)
        {
            var companyId = _httpContext.User.GetCompanyId();

            var attendanceSetting = new AttendanceSetting()
            {
                Id = model.Id,
                CompanyId = companyId,
                ApproverAccounts = model.ApprovarAccounts,
                TimeFormatId = model.TimeFormatId,
                EnableNotifyCheckinLate = model.EnableNotifyCheckinLate
            };
            _attendanceService.UpdateAttendanceSetting(attendanceSetting);

            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }

        /// <summary>
        /// Manager can create new leave request for user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAddLeaveAttendance)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.LeaveRequest)]
        public IActionResult AddLeaveRequest([FromBody] LeaveRequestModel model)
        {
            var textRegister = _attendanceService.AddAttendanceLeave(model);
            if (!string.IsNullOrEmpty(textRegister))
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, textRegister);
            }

            return new ApiSuccessResult(StatusCodes.Status200OK, AttendanceResource.messageRegisterSuccess);
        }

        /// <summary>
        /// Employee can create attendance leave request
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiRegisterLeaveAttendance)]
        [CheckPermission(ActionName.ManageOwnRecord + Page.LeaveRequest)]
        public IActionResult RegisterMyLeaveAttendance([FromBody] AttendanceRegister model)
        {
            var user = _userService.GetUserByAccountId(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId());
            if (user == null)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.msgUserIsInValid);
            }

            var textRegister = _attendanceService.RegisterAttendanceLeave(model);
            if (!string.IsNullOrEmpty(textRegister))
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, textRegister);
            }

            return new ApiSuccessResult(StatusCodes.Status200OK, AttendanceResource.messageRegisterSuccess);
        }

        /// <summary>
        /// Delete leave requests by ids
        /// </summary>
        /// <param name="ids">list of leave request ids</param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiRegisterLeaveAttendance)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.LeaveRequest)]
        public IActionResult DeleteMultiLeaveAttendance(List<int> ids)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var attendanceLeaves = _attendanceService.GetRequestByIdsAndCompanyId(companyId, ids);
            var account = _accountService.GetById(_httpContext.User.GetAccountId());

            foreach (var attendance in attendanceLeaves)
            {
                string attendanceDate = attendance.Date.ConvertDefaultDateTimeToString();
                if (account != null)
                {
                    attendanceDate = attendance.Date.ConvertToUserTime(account.TimeZone).ToSettingDateTimeString(account.Language);
                }
                if (attendance.Start < DateTime.Now || attendance.Status != (short)AttendanceStatus.Waiting)
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                        AttendanceResource.cannotDeleteRequest + $" ({attendanceDate})");

                // check condition user
                var user = _userService.GetById(attendance.UserId);
                if (user == null)
                    return new ApiErrorResult(StatusCodes.Status404NotFound,
                        AttendanceResource.cannotDeleteRequest + $" ({attendanceDate})");

                if (user.AccountId != _httpContext.User.GetAccountId() && attendance.CreatedBy != _httpContext.User.GetAccountId())
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                        AttendanceResource.cannotDeleteRequest + $" ({attendanceDate})");
            }

            _attendanceService.DeleteAttendanceLeaves(attendanceLeaves);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, "").ReplaceSpacesString());
        }

        /// <summary>
        /// Delete request attendance leave
        /// </summary>
        /// <param name="id">Id attendance</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Unprocessable Entity: Record attendance leave can't delete</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEditLeaveAttendance)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ManageOwnRecord + Page.LeaveRequest)]
        public IActionResult DeleteMyAttendance(int id)
        {
            // check condition attendance leave
            var attendance = _attendanceService.GetRequestByIdAndCompanyId(_httpContext.User.GetCompanyId(), id);
            if (attendance == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);

            if (attendance.Start < DateTime.Now || attendance.Status != (short)AttendanceStatus.Waiting)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AttendanceResource.cannotDeleteRequest);

            // check condition user
            var user = _userService.GetById(attendance.UserId);
            if (user == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgUserIsInValid);

            if (user.AccountId != _httpContext.User.GetAccountId() && attendance.CreatedBy != _httpContext.User.GetAccountId())
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AttendanceResource.cannotDeleteRequest);

            _attendanceService.DeleteAttendance(attendance);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, "").ReplaceSpacesString());
        }

        /// <summary>
        /// Edit request attendance leave
        /// </summary>
        /// <param name="id">Id attendance</param>
        /// <param name="model">JSON Model Attendance Leave</param>
        /// <returns></returns>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEditLeaveAttendance)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ManageOwnRecord + Page.LeaveRequest)]
        public IActionResult EditAttendanceLeave(int id, [FromBody] LeaveRequestModel model)
        {
            // check condition attendance leave
            var attendance = _attendanceService.GetRequestByIdAndCompanyId(_httpContext.User.GetCompanyId(), id);
            if (attendance == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);

            if (model.StartD >= model.EndD)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.InvalidDate, AttendanceResource.lblEndDate, AttendanceResource.lblStartDate));

            if (attendance.Start < DateTime.Now || attendance.Status != (short)AttendanceStatus.Waiting)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AttendanceResource.cannotEditRequest);

            // check condition user
            var user = _userService.GetById(attendance.UserId);
            if (user == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgUserIsInValid);

            if (user.AccountId != _httpContext.User.GetAccountId() && attendance.CreatedBy != _httpContext.User.GetAccountId())
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AttendanceResource.cannotEditRequest);

            attendance.Start = model.StartD;
            attendance.End = model.EndD;
            attendance.Type = model.Type;
            attendance.Reason = model.Reason;
            attendance.EditedBy = _httpContext.User.GetAccountId();
            if (model.UserId != 0 && _userService.GetById(model.UserId) != null)
                attendance.UserId = model.UserId;

            var msgError = _attendanceService.EditAttendanceLeave(attendance);
            if (!string.IsNullOrEmpty(msgError))
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, msgError);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }

        /// <summary>
        /// Get leave request page attendance
        /// </summary>
        /// <param name="search">string of username</param>
        /// <param name="attendanceType">string list of type attendance</param>
        /// <param name="start">date time start</param>
        /// <param name="end">date time end</param>
        /// <param name="status">status attendance leave (default 0: get all attendances (AttendanceStatus = 1,2,3))</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column</param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiRegisterLeaveAttendance)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.LeaveRequest)]
        public IActionResult GetLeaveAttendance(string search, string attendanceType, DateTime start, DateTime end, int status = 0, int pageNumber = 1, int pageSize = 10, string sortColumn = "Status", string sortDirection = "asc")
        {
            var attendance = _attendanceService.GetAttendanceLeaves(search, attendanceType, start, end, status, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal, out var recordsFiltered);

            var pagingData = new PagingData<AttendanceLeaveModel>
            {
                Data = attendance,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get leave request page attendance for user
        /// </summary>
        /// <param name="search">string of username</param>
        /// <param name="attendanceType">string list of type attendance</param>
        /// <param name="start">date time start</param>
        /// <param name="end">date time end</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column</param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiRegisterLeaveAttendanceUser)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ManageOwnRecord + Page.LeaveRequest)]
        public IActionResult GetLeaveAttendanceUser(string search, string attendanceType, DateTime start, DateTime end, int pageNumber = 1, int pageSize = 10, string sortColumn = "Status", string sortDirection = "desc")
        {
            var filter = new LeaveReportFilterModel()
            {
                Search = search,
                AttendanceType = attendanceType,
                StartDate = start,
                EndDate = end,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                CompanyId = _httpContext.User.GetCompanyId(),
            };
            var attendance = _attendanceService.GetAttendanceLeavesUser(filter, out var recordsTotal, out var recordsFiltered);

            var pagingData = new PagingData<AttendanceLeaveModel>
            {
                Data = attendance,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Accept or Reject request leave attendance
        /// </summary>
        /// <param name="id">Attendance id</param>
        /// <param name="model">Boolean accept request</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUpdateLeaveAttendance)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.LeaveRequest)]
        public IActionResult UpdateRequestLeave(int id, [FromBody] ActionApproval model)
        {
            if (!model.IsAccept && string.IsNullOrWhiteSpace(model.RejectReason))
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.ApprovedSuccessfully);

            var attendanceSetting = _attendanceService.GetAttendanceSetting(_httpContext.User.GetCompanyId());
            if (string.IsNullOrWhiteSpace(attendanceSetting.ApprovarAccounts))
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.UnprocessableEntity);

            if (attendanceSetting.ApprovarAccounts[0] != '[')
            {
                attendanceSetting.ApprovarAccounts = $"[{attendanceSetting.ApprovarAccounts}]";
            }
            List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(attendanceSetting.ApprovarAccounts);
            if (!accountIds.Contains(_httpContext.User.GetAccountId()))
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.msgNotPermission);

            var result = _attendanceService.ApprovedAttendanceLeave(id, model);
            if (!result)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }

        /// <summary>
        /// Get parameter init in page attendance leave request
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response> 
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiLeaveAttendanceInit)]
        [Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdminAndEmployee, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult InitPageAttendanceLeave()
        {
            return Ok(_attendanceService.GetLeaveInit(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId()));
        }


        /// <summary>
        /// Get all of list attendance types
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiGetAttendanceTypeList)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetAttendanceTypeList()
        {
            var attendanceTypes = _attendanceService.GetAttendanceTypeList();

            return Ok(attendanceTypes);
        }

        /// <summary>
        /// Get Attendance of Users with company every month
        /// </summary>
        /// <param name="search">Query string that filter by UserName</param>
        /// <param name="month">Query attendance of month</param>
        /// <param name="departmentIds">Id of department</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column</param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAttendancesByCompanyEveryMonth)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.TimeAttendanceReport)]
        public IActionResult GetAttendancesEveryMonth(string search, DateTime month, List<int> departmentIds,
            int pageNumber = 1, int pageSize = 10, string sortColumn = "UserName", string sortDirection = "asc")
        {
            var data = _attendanceService.GetAttendanceReportMonth(search, month, departmentIds,
                pageNumber, pageSize, sortColumn, sortDirection, out int totalRecords, out int recordsFiltered);
            var pagingData = new PagingData<AttendanceReportMonthModel>
            {
                Data = data,
                Meta = { RecordsTotal = totalRecords, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// get report total leave request of users
        /// </summary>
        /// <param name="search"></param>
        /// <param name="year"></param>
        /// <param name="endDate"></param>
        /// <param name="departmentIds"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAttendanceLeaveReportYear)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ViewLeaveManagement + Page.LeaveRequest)]
        public IActionResult GetLeaveReportYear(string search, string year, DateTime startDate, DateTime endDate, List<int> departmentIds,
            int pageNumber = 1, int pageSize = 10, string sortColumn = "name", string sortDirection = "asc")
        {
            var filter = new LeaveReportFilterModel
            {
                Search = search,
                Year = year,
                StartDate = startDate,
                EndDate = endDate,
                DepartmentIds = departmentIds,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                CompanyId = _httpContext.User.GetCompanyId(),
            };

            var data = _attendanceService.GetAttendanceReportYear(filter, out int totalRecords, out int recordsFiltered);
            var pagingData = new PagingData<AttendanceLeaveReportModel>
            {
                Data = data,
                Meta = { RecordsTotal = totalRecords, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// get history leave request of user
        /// </summary>
        /// <param name="id">user id</param>
        /// <param name="search"></param>
        /// <param name="attendanceType"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiLeaveRequestHistoryOfUser)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.LeaveRequest)]
        public IActionResult GetLeaveAttendanceByUserId(int id, string search, string attendanceType, DateTime start, DateTime end, int pageNumber = 1, int pageSize = 10, string sortColumn = "start", string sortDirection = "desc")
        {
            var user = _userService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
            if (user == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgUserIsInValid);
            }

            var filter = new LeaveReportFilterModel()
            {
                Search = search,
                AttendanceType = attendanceType,
                StartDate = start,
                EndDate = end,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                UserId = user.Id,
                CompanyId = _httpContext.User.GetCompanyId(),
            };
            var attendance = _attendanceService.GetAttendanceLeavesUser(filter, out var recordsTotal, out var recordsFiltered);

            var pagingData = new PagingData<AttendanceLeaveModel>
            {
                Data = attendance,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get leave request setting
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiSettingLeaveRequest)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetLeaveRequestSetting()
        {
            return Ok(_attendanceService.GetLeaveRequestSettingByCompanyId(_httpContext.User.GetCompanyId()));
        }

        /// <summary>
        /// update leave request setting
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiSettingLeaveRequest)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdateLeaveRequestSetting([FromBody] LeaveSettingModel model)
        {
            _attendanceService.UpdateLeaveRequestSetting(model);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }


        /// <summary>
        /// Set StartTime to check attendance
        /// </summary>
        /// <param name="startTime"></param>
        /// <returns></returns>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAttendanceStartTime)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.UpdateAttendanceSetting + Page.TimeAttendanceReport)]
        public IActionResult SetStartTime(string startTime)
        {
            if (string.IsNullOrWhiteSpace(startTime))
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.Required, "Time"));

            _attendanceService.SetStartTime(startTime);

            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }


        /// <summary>
        /// Get StartTime to check attendance
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAttendanceStartTime)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.UpdateAttendanceSetting + Page.TimeAttendanceReport)]
        public IActionResult GetStartTime()
        {
            var startTime = _attendanceService.GetStartTime();

            return Ok(startTime);
        }
    }


}