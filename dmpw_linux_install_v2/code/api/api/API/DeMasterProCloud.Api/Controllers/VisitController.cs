using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.DataModel.Visit;
using Newtonsoft.Json;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
using DeMasterProCloud.DataModel.EventLog;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.DataModel.AccessGroup;
using DeMasterProCloud.DataModel.Account;
using DeMasterProCloud.DataModel.Device;
using System.Text.RegularExpressions;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using DeMasterProCloud.DataModel.Login;
using DeMasterProCloud.DataModel.Header;
using Ganss.Xss;
using DeMasterProCloud.DataModel.AccessGroupDevice;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Visit controller
    /// </summary>
    [Produces("application/ms-excel", "application/json", "application/text")]
    [CheckAddOn(Constants.PlugIn.VisitManagement)]
    public class VisitController : Controller
    {
        private readonly IVisitService _visitService;
        private readonly IAccountService _accountService;
        private readonly IAccessTimeService _accessTimeService;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly ICameraService _cameraService;
        private readonly INotificationService _notificationService;
        private readonly ICompanyService _companyService;
        private readonly IDepartmentService _departmentService;
        private readonly IUserService _userService;
        private readonly IAccessGroupService _accessGroupService;
        private readonly IAccessGroupDeviceService _accessGroupDeviceService;
        private readonly IMapper _mapper;


        /// <summary>
        /// VisitorController constructor
        /// </summary>
        /// <param name="visitService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        /// <param name="accountService"></param>
        /// <param name="cameraService"></param>
        /// <param name="notificationService"></param>
        /// <param name="accessTimeService"></param>
        /// <param name="companyService"></param>
        /// <param name="departmentService"></param>
        /// <param name="userService"></param>
        /// <param name="accessGroupService"></param>
        public VisitController(IVisitService visitService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IAccountService accountService,
            ICameraService cameraService, INotificationService notificationService, IAccessTimeService accessTimeService, ICompanyService companyService,
            IDepartmentService departmentService, IUserService userService, IAccessGroupService accessGroupService,
            IAccessGroupDeviceService accessGroupDeviceService, IMapper mapper)
        {
            _visitService = visitService;
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration;
            _accountService = accountService;
            _cameraService = cameraService;
            _notificationService = notificationService;
            _accessTimeService = accessTimeService;
            _companyService = companyService;
            _departmentService = departmentService;
            _userService = userService;
            _accessGroupService = accessGroupService;
            _accessGroupDeviceService = accessGroupDeviceService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get visit by id. 
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVisitsId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult Get(int id)
        {
            var model = new VisitDataModel();
            if (id != 0)
            {
                var visit = _visitService.GetByIdWithTimezone(id);
                if (visit == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound);
                }
                else
                {
                    visit.IsDecision = false;
                    if (visit.Status != (short)VisitChangeStatusType.Approved && visit.Status != (short)VisitChangeStatusType.AutoApproved)
                    {
                        var visitSetting = _visitService.GetVisitSettingCompany();
                        var accountId = _httpContext.User.GetAccountId();
                        if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.NoStep 
                            && visit.Status == (short)VisitChangeStatusType.Waiting && visit.ApproverId1 == accountId)
                        {
                            visit.IsDecision = true;
                        }
                        if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.FirstStep 
                            && visit.Status == (short)VisitChangeStatusType.Waiting && visit.ApproverId1 == accountId)
                        {
                            visit.IsDecision = true;
                        }
                        if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.SecondStep)
                        {
                            if (visit.Status == (short)VisitChangeStatusType.Waiting && visit.ApproverId1 == accountId)
                            {
                                visit.IsDecision = true;
                            }
                            else if (visit.Status == (short)VisitChangeStatusType.Approved1)
                            {
                                var secondApprovalAccounts = JsonConvert.DeserializeObject<List<int>>(visitSetting.SecondsApproverAccounts);
                                if (secondApprovalAccounts.Contains(_httpContext.User.GetAccountId()))
                                {
                                    visit.IsDecision = true;
                                }
                            }
                        }
                    }
                   
                }
                model = _mapper.Map<VisitDataModel>(visit);

                // add info visitee
                if (visit != null)
                {

                    if(visit.VisiteeId != null)
                    {
                        var visitee = _userService.GetById(visit.VisiteeId.Value);
                        if(visitee != null)
                        {
                            model.VisiteePhone = visitee.HomePhone;
                            model.VisiteeEmail = visitee.Email;
                            
                        }
                    }
                }

                model.DynamicQrCode = _visitService.GetStaticQrCode(visit);
            }
            _visitService.InitData(model);

            model.ListCardStatus = EnumHelper.ToEnumList<VisitingCardStatusNormalType>();
            model.CardStatus = (short)VisitingCardStatusType.Delivered;

            return Ok(model);
        }

        /// <summary>
        /// Get visit list. Filter by time model(OpeDateFrom - OpeDateTo, OpeTimeFrom - OpeTimeTo)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="startDateFrom">start filter open visit from</param>
        /// <param name="endDateFrom">end filter open visit from</param>
        /// <param name="visitorName">name of visit</param>
        /// <param name="birthDay">date of birthday</param>
        /// <param name="visitorDepartment">department of visit</param>
        /// <param name="position">position</param>
        /// <param name="visiteeSite">site of visit</param>
        /// <param name="visitReason">reason of visit</param>
        /// <param name="visiteeName">name of visit</param>
        /// <param name="phone">phone number</param>
        /// <param name="processStatus">status of process</param>
        /// <param name="approverName1">approve name 1</param>
        /// <param name="approverName2">approve name 2</param>
        /// <param name="rejectReason">reason for reject</param>
        /// <param name="search">search general</param>
        /// <param name="cardId">card id</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVisits)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult Gets(VisitOperationTime model, string startDateFrom, string endDateFrom, string visitorName,
            string birthDay, string visitorDepartment, string position, string visiteeSite, string visitReason, string visiteeName, string phone, List<int> processStatus,
            string approverName1, string approverName2, string rejectReason, string cardId, string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "Status",
            string sortDirection = "desc")
        {
            sortColumn = SortColumnMapping.VisitColumn(sortColumn);
            var filter = new VisitFilterModel()
            {
                StartDateFrom = startDateFrom,
                EndDateFrom = endDateFrom,
                AccessDateFrom = model.OpeDateFrom,
                AccessDateTo = model.OpeDateTo,
                VisitorName = visitorName,
                BirthDay = birthDay,
                VisitorDepartment = visitorDepartment,
                Position = position,
                VisiteeSite = visiteeSite,
                VisitReason = visitReason,
                VisiteeName = visiteeName,
                Phone = phone,
                ProcessStatus = processStatus,
                ApproverName1 = approverName1,
                ApproverName2 = approverName2,
                RejectReason = rejectReason,
                CardId = cardId,
                Search = search,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                IsOnyVisitTarget = false,
            };
            var visits = _visitService.GetPaginated(filter, out var recordsTotal, out var recordsFiltered);

            var pagingData = new PagingData<VisitListModel>
            {
                Data = visits,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get visit list. Filter by time model(OpeDateFrom - OpeDateTo, OpeTimeFrom - OpeTimeTo)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="visitorName">name of visit</param>
        /// <param name="birthDay">date of birthday</param>
        /// <param name="visitorDepartment">department of visit</param>
        /// <param name="position">position</param>
        /// <param name="visiteeSite">site of visit</param>
        /// <param name="visitReason">reason of visit</param>
        /// <param name="visiteeName">name of visit</param>
        /// <param name="phone">phone number</param>
        /// <param name="processStatus">status of process</param>
        /// <param name="approverName1">approve name 1</param>
        /// <param name="approverName2">approve name 2</param>
        /// <param name="rejectReason">reason for reject</param>
        /// <param name="cardId">card id</param>
        /// <param name="search">search general</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiMyVisits)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult GetMyVisits(VisitOperationTime model, string visitorName,
            string birthDay, string visitorDepartment, string position, string visiteeSite, string visitReason, string visiteeName, string phone, List<int> processStatus,
            string approverName1, string approverName2, string rejectReason, string cardId, string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "Status",
            string sortDirection = "desc")
        {
            sortColumn = SortColumnMapping.VisitColumn(sortColumn);
            var filter = new VisitFilterModel()
            {
                AccessDateFrom = model.OpeDateFrom,
                AccessDateTo = model.OpeDateTo,
                VisitorName = visitorName,
                BirthDay = birthDay,
                VisitorDepartment = visitorDepartment,
                Position = position,
                VisiteeSite = visiteeSite,
                VisitReason = visitReason,
                VisiteeName = visiteeName,
                Phone = phone,
                ProcessStatus = processStatus,
                ApproverName1 = approverName1,
                ApproverName2 = approverName2,
                RejectReason = rejectReason,
                CardId = cardId,
                Search = search,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                IsOnyVisitTarget = true,
            };
            var visits = _visitService.GetPaginated(filter, out var recordsTotal, out var recordsFiltered);

            var pagingData = new PagingData<VisitListModel>
            {
                Data = visits,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Visit Report Init
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVisitsReportInit)]
        [CheckPermission(ActionName.View + Page.VisitReport)]
        public IActionResult VisitReportInit()
        {
            var model = _visitService.InitReportData();
            return Ok(model);
        }

        /// <summary>
        /// Get visit list with report. Filter by time model(OpeDateFrom - OpeDateTo, OpeTimeFrom - OpeTimeTo)
        /// </summary>
        /// <param name="from">Date Access Start</param>
        /// <param name="to">Date Access End</param>
        /// <param name="doorIds">list of door ids</param>
        /// <param name="DeviceReaderIds">list of device reader ids</param>
        /// <param name="visitorName">name of visitor</param>
        /// <param name="cardId">card id</param>
        /// <param name="search">search general</param>
        /// <param name="inOutType">type of visit(in/out)</param>
        /// <param name="eventType">list of event types</param>
        /// <param name="visiteeSite">visit of website</param>
        /// <param name="cardStatus">status of card</param>
        /// <param name="cardTypes">list of card types</param>
        /// <param name="visitorTypes"> list of visit types </param>
        /// <param name="birthDay">date of birthday</param>
        /// <param name="visitorDepartment">visitor department</param>
        /// <param name="building">building of icu</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVisitsReport)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitReport)]
        public IActionResult VisitReport(string from,
            string to, List<int> doorIds, List<int> DeviceReaderIds, string visitorName, string cardId, string search, List<int> inOutType, List<int> eventType, string visiteeSite,
            List<int> cardStatus, List<int> cardTypes, List<int> visitorTypes, string birthDay, string visitorDepartment, List<int> building,
            int pageNumber = 1, int pageSize = 10, string sortColumn = "Id",
            string sortDirection = "desc")
        {
            sortColumn = SortColumnMapping.VisitReportColumn(sortColumn);
            var visitReports = _visitService
                .VisitReport(from, to, doorIds, DeviceReaderIds,
                    visitorName, cardId, search, inOutType, eventType, visiteeSite, cardStatus, cardTypes, visitorTypes, birthDay, visitorDepartment, building, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered);

            var pagingData = new PagingData<VisitReportModel>
            {
                Data = visitReports,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }


        /// <summary>
        /// Get init visit management page
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVisitsInit)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult VisitManageInit()
        {
            var data = _visitService.GetInit();
            return Ok(data);
        }

        /// <summary>
        /// Add new a visit
        /// </summary>
        /// <param name="model">JSON model for Visit</param>
        /// <returns></returns>
        /// <response code="201">Create new a visit</response>
        /// <response code="400">Bad Request: Data approverId1 of Model JSON wrong</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiVisits)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.VisitManagement)]
        public IActionResult Add([FromBody] VisitModel model)
        {
            if (model != null)
            {
                Console.WriteLine("[ADD NEW VISIT]");
                Console.WriteLine(JsonConvert.SerializeObject(model));
            }
            
            // check validation of visit model
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            var companyId = _httpContext.User.GetCompanyId();

            string avatarBase64 = "";
            string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
            var company = _companyService.GetById(companyId);

            // stored image avatar
            if (model.Avatar.IsTextBase64())
            {
                avatarBase64 = model.Avatar;
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/visitor";
                string fileName = $"{Guid.NewGuid().ToString()}.jpg";
                FileHelpers.SaveFileImageSecure(model.Avatar, basePath, fileName, Constants.Image.MaximumImageStored);
                string path = Path.Combine(basePath, fileName);
                model.Avatar = $"{connectionApi}/static/{path}";
            }

            // stored image NationalIdNumber front
            if (model.ImageCardIdFont.IsTextBase64())
            {
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/visitor";
                string fileName = $"card_id_front.{Guid.NewGuid().ToString()}.jpg";
                FileHelpers.SaveFileImageSecure(model.ImageCardIdFont, basePath, fileName, Constants.Image.MaximumImageStored);
                string path = Path.Combine(basePath, fileName);
                model.ImageCardIdFont = $"{connectionApi}/static/{path}";
            }

            // stored image NationalIdNumber back
            if (model.ImageCardIdBack.IsTextBase64())
            {
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/visitor";
                string fileName = $"card_id_back.{Guid.NewGuid().ToString()}.jpg";
                FileHelpers.SaveFileImageSecure(model.ImageCardIdBack, basePath, fileName, Constants.Image.MaximumImageStored);
                string path = Path.Combine(basePath, fileName);
                model.ImageCardIdBack = $"{connectionApi}/static/{path}";
            }
            
            // stored avatar of National ID Card
            if (model.NationalIdCard != null && model.NationalIdCard.Avatar.IsTextBase64())
            {
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/visitor";
                string fileName = $"card_avatar.{Guid.NewGuid().ToString()}.jpg";
                FileHelpers.SaveFileImageSecure(model.NationalIdCard.Avatar, basePath, fileName, Constants.Image.MaximumImageStored);
                string path = Path.Combine(basePath, fileName);
                model.NationalIdCard.Avatar = $"{connectionApi}/static/{path}";
            }

            var visitSetting = _visitService.GetVisitSettingCompany();

            if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.NoStep)
            {
                model.ApproverId1 = _httpContext.User.GetAccountId();
            }

            var visit = _visitService.Add(model);
            int visitorId = visit.Id;
            var visitorName = visit.VisitorName;

            if (visitorId > 0 && model.ApproverId1 > 0)
            {
                if (visitSetting.ApprovalStepNumber != (short)VisitSettingType.NoStep)
                {
                    var notify = _notificationService.MappingNoti(companyId, model.ApproverId1,
                        (short)NotificationType.NotificationVisit,
                        "contentVisitorNotification",
                        JsonConvert.SerializeObject(new
                        { visitor_name = visitorName, visit_date = model.StartDate }),
                        "/visit?id=" + visitorId + "");
                    _notificationService.AddNotification(notify, (short)NotificationType.NotificationVisit);

                    // pushing notification
                    _notificationService.PushingNotificationToUser(
                        "info", visit.Id,
                        NotificationResource.TitleNotificationVisit,
                        notify.Content, model.ApproverId1, company.Id);
                }
            }

            return new ApiSuccessResult(StatusCodes.Status201Created, string.Format(MessageResource.MessageAddSuccess, VisitResource.lblVisit.ToLower()), $"{visitorId}");
        }

        /// <summary>
        /// Fill information form register
        /// </summary>
        /// <param name="companyCode">Company code</param>
        /// <param name="meetingId">meeting id</param>
        /// <param name="visitTargetId">Account company Id</param>
        /// <returns></returns>
        /// <response code="404">Company does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiRegisterVisit)]
        [AllowAnonymous]
        public IActionResult GetInformationRegisterVisit(string companyCode, int visitTargetId, int meetingId)
        {
            var company = _companyService.GetByCode(companyCode);
            if (company == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundCompany);

            string targetDepartment = "";
            string targetName = "";

            
            var user = _userService.GetUserByAccountId(visitTargetId, company.Id);
            if (user != null)
            {
                targetName = user.FirstName;

                var department = _departmentService.GetById(user.DepartmentId);
                if (department != null)
                    targetDepartment = department.DepartName;
            }

            var visitSetting = _visitService.GetVisitSettingByCompanyId(company.Id);
            var model = _visitService.GetInitVisitForm(company.Id);
            var accountTarget = _accountService.GetAccountsById(visitTargetId);
            var timeZone = accountTarget?.TimeZone ?? Constants.DefaultTimeZone;
            List<EnumModel> departments = new List<EnumModel>();
            if (visitSetting.AllowGetUserTarget)
            {
                departments = _departmentService.GetByCompanyId(company.Id).Select(m => new EnumModel()
                {
                    Id = m.Id,
                    Name = m.DepartName,
                }).OrderBy(m => m.Name).ToList();
            }
            return Ok(new VisitTargetRegister()
            {
                TargetId = user?.Id ?? 0,
                TargetDepartment = targetDepartment,
                TargetName = targetName,
                SiteKey = visitSetting.EnableCaptCha ? _configuration.GetSection(Constants.Settings.ReCaptChaSiteKey).Value : null,
                Logo = model.Logo,
                Language = model.Language,
                CompanyName = company.Name,
                ListFieldsEnable = visitSetting.ListFieldsEnable,
                AllowGetUserTarget = visitSetting.AllowGetUserTarget,
                FieldRegisterLeft = string.IsNullOrWhiteSpace(visitSetting.FieldRegisterLeft) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(visitSetting.FieldRegisterLeft),
                FieldRegisterRight = string.IsNullOrWhiteSpace(visitSetting.FieldRegisterRight) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(visitSetting.FieldRegisterRight),
                FieldRequired = string.IsNullOrWhiteSpace(visitSetting.FieldRequired) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(visitSetting.FieldRequired),
                RidDefault = model.RidDefault,
                TimeZone = timeZone,
                ListVisitPurpose = string.IsNullOrEmpty(visitSetting.ListVisitPurpose) ? new List<string>() : visitSetting.ListVisitPurpose.Split("\n").ToList(),
                Departments = departments,
            });
            
        }

        /// <summary>
        /// Visitor can register visit by themself
        /// </summary>
        /// <param name="companyCode">Company code</param>
        /// <param name="meetingId">meeting Id</param>
        /// <param name="visitTargetId">Account Id CreatedBy</param>
        /// <param name="generateImageQrCode">Enable/Disable generate link image qr-code</param>
        /// <param name="model">Model JSON for Visit</param>
        /// <returns></returns>
        /// <response code="201">Create new a visit</response>
        /// <response code="400">ReCaptcha false</response>
        /// <response code="404">Company does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiRegisterVisit)]
        [AllowAnonymous]
        public IActionResult RegisterVisit(string companyCode, int visitTargetId, int meetingId, bool generateImageQrCode, [FromBody] VisitModel model)
        {
            var company = _companyService.GetByCode(companyCode);
            if (company == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundCompany);
            
            var user = _userService.GetUserByAccountId(visitTargetId, company.Id);
            if (user == null && meetingId == 0)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);

            if (string.IsNullOrWhiteSpace(model.StartDate) || string.IsNullOrWhiteSpace(model.EndDate))
            {
                DateTime now = DateTime.UtcNow;
                var accountTarget = _accountService.GetAccountsById(visitTargetId);
                DateTime startDate = now.ConvertToUserTime(accountTarget?.TimeZone);
                string[] textStartDate = startDate.ToString(Constants.DateTimeFormatDefault).Split(' ');

                model.StartDate = textStartDate[0];
                model.EndDate = textStartDate[0];
            }

            var visitSetting = _visitService.GetVisitSettingByCompanyId(company.Id);

            // check validation of visit model
            if (visitSetting.EnableCaptCha && !ModelState.IsValid && string.IsNullOrWhiteSpace(model.GReCaptchaResponse))
            {
                return new ValidationFailedResult(ModelState);
            }

            string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
            string avatarBase64 = "";
            // stored image avatar
            if (model.Avatar.IsTextBase64())
            {
                avatarBase64 = model.Avatar;
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/visitor";
                string fileName = $"{Guid.NewGuid().ToString()}.jpg";
                FileHelpers.SaveFileImageSecure(model.Avatar, basePath, fileName, Constants.Image.MaximumImageStored);
                string path = Path.Combine(basePath, fileName);
                model.Avatar = $"{connectionApi}/static/{path}";
            }

            // stored image NationalIdNumber front
            if (model.ImageCardIdFont.IsTextBase64())
            {
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/visitor";
                string fileName = $"card_id_front.{Guid.NewGuid().ToString()}.jpg";
                FileHelpers.SaveFileImageSecure(model.ImageCardIdFont, basePath, fileName, Constants.Image.MaximumImageStored);
                string path = Path.Combine(basePath, fileName);
                model.ImageCardIdFont = $"{connectionApi}/static/{path}";
            }

            // stored image NationalIdNumber back
            if (model.ImageCardIdBack.IsTextBase64())
            {
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/visitor";
                string fileName = $"card_id_back.{Guid.NewGuid().ToString()}.jpg";
                FileHelpers.SaveFileImageSecure(model.ImageCardIdBack, basePath, fileName, Constants.Image.MaximumImageStored);
                string path = Path.Combine(basePath, fileName);
                model.ImageCardIdBack = $"{connectionApi}/static/{path}";
            }

            // stored avatar of National ID Card
            if (model.NationalIdCard != null && model.NationalIdCard.Avatar.IsTextBase64())
            {
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/visitor";
                string fileName = $"card_avatar.{Guid.NewGuid().ToString()}.jpg";
                FileHelpers.SaveFileImageSecure(model.NationalIdCard.Avatar, basePath, fileName, Constants.Image.MaximumImageStored);
                string path = Path.Combine(basePath, fileName);
                model.NationalIdCard.Avatar = $"{connectionApi}/static/{path}";
            }

            if (model.VisiteeId == 0) model.VisiteeId = meetingId != 0 ? 0 : user?.Id;
            
            if (visitSetting.ApprovalStepNumber != (short)VisitSettingType.NoStep || !visitSetting.EnableAutoApproval)
            {
                List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.FirstApproverAccounts);
                var accounts = _accountService.GetPaginatedAccountListByIds("", accountIds, company.Id, Page.VisitSetting + Page.User, 0, 0, "Id", "desc",
                out _, out _, out _);
                
                if(visitSetting.ApprovalStepNumber == 1 && accounts.Count > 0)
                {
                    model.ApproverId1 = accounts.Count > 0 ? accounts.First().AccountId : visitTargetId;
                }
                else if(visitSetting.ApprovalStepNumber == 2 && accounts.Count > 1)
                {
                    model.ApproverId1 = accounts.Count > 0 ? accounts.First().AccountId : visitTargetId;
                    model.ApproverId2 = accounts.Count > 1 
                    ? accounts.First(x => x.AccountId != model.ApproverId1).AccountId
                    : visitTargetId;
                }
                else 
                {
                    model.ApproverId1 = visitTargetId;
                }
            }

            model.VisiteeDepartmentId = meetingId != 0 ? 0 : user.DepartmentId;
            var visit = meetingId != 0 ? _visitService.RegisterVisit(model, company.Id, visitTargetId, false) : _visitService.RegisterVisit(model, company.Id, visitTargetId);
            
            if (visit.Id > 0)
            {
                if (visitSetting.ApprovalStepNumber != (short)VisitSettingType.NoStep || !visitSetting.EnableAutoApproval)
                {
                    if (model.ApproverId1 > 0)
                    {
                        Notification noti = _notificationService.MappingNoti(company.Id, model.ApproverId1,
                            (short)NotificationType.NotificationVisit,
                            "contentVisitorNotification",
                            JsonConvert.SerializeObject(new
                            { visitor_name = visit.VisitorName, visit_date = model.StartDate }),
                            "/visit?id=" + visit.Id + "");
                        _notificationService.AddNotification(noti, (short)NotificationType.NotificationVisit);

                        // pushing notification
                        _notificationService.PushingNotificationToUser(
                            "info", visit.Id,
                            NotificationResource.TitleNotificationVisit,
                            noti.Content, model.ApproverId1, company.Id);
                    }
                    if (model.ApproverId2 > 0)
                    {
                        Notification noti = _notificationService.MappingNoti(company.Id, model.ApproverId2,
                            (short)NotificationType.NotificationVisit,
                            "contentVisitorNotification",
                            JsonConvert.SerializeObject(new
                            { visitor_name = visit.VisitorName, visit_date = model.StartDate }),
                            "/visit?id=" + visit.Id + "");
                        _notificationService.AddNotification(noti, (short)NotificationType.NotificationVisit);

                        // pushing notification
                        _notificationService.PushingNotificationToUser(
                            "info", visit.Id,
                            NotificationResource.TitleNotificationVisit,
                            noti.Content, model.ApproverId2, company.Id);
                    }
                }
            }
            
            string qrCode = _visitService.PrepareQrCodeForVisitor(visit, (short)CardStatus.InValid);

            if (meetingId != 0)
            {
                _visitService.SendCardToDeviceRegisterMeeting(visit ,company.Id);
            }
            string linkImageQrCode = null;
            if (generateImageQrCode) linkImageQrCode = _visitService.GenerateImageQRCode(qrCode);
            
            Dictionary<string, object> data = new Dictionary<string, object>()
            {
                { "qrCode", qrCode },
                { "imageQrCode", linkImageQrCode },
                { "visiteeName", meetingId != 0 ? " ":user.FirstName },
                { "visiteeDepartmentName", meetingId != 0 ? " " : _departmentService.GetById(user.DepartmentId)?.DepartName },
                { "visitId", visit.Id },
            };
            
            return Ok(data);
        }

        /// <summary>
        /// Edit visitor by id
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <param name="model">JSON model for Visit</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: account not role edit. Or status is not waiting</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit Id does not exist in DB</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiVisitsId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitManagement)]
        public IActionResult Edit(int id, [FromBody] VisitModel model)
        {
            model.Id = id;
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }
            var visit = _visitService.GetById(id);
            if (visit == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }

            var companyId = _httpContext.User.GetCompanyId();
            string avatarBase64 = "";
            string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
            var company = _companyService.GetById(companyId);

            // change avatar
            if (model.Avatar.IsTextBase64())
            {
                if (!string.IsNullOrWhiteSpace(visit.Avatar))
                {
                    FileHelpers.DeleteFileFromLink(visit.Avatar.Replace($"{connectionApi}/static/", ""));
                }

                avatarBase64 = model.Avatar;
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/visitor";
                string fileName = $"{Guid.NewGuid().ToString()}.jpg";
                FileHelpers.SaveFileImageSecure(model.Avatar, basePath, fileName, Constants.Image.MaximumImageStored);
                string path = Path.Combine(basePath, fileName);
                model.Avatar = $"{connectionApi}/static/{path}";
            }
            else if (!string.IsNullOrWhiteSpace(model.Avatar))
            {
                avatarBase64 = model.Avatar;
            }

            // change NationalIdNumber front
            if (model.ImageCardIdFont.IsTextBase64())
            {
                if (!string.IsNullOrWhiteSpace(visit.ImageCardIdFont))
                {
                    FileHelpers.DeleteFileFromLink(visit.ImageCardIdFont.Replace($"{connectionApi}/static/", ""));
                }

                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/visitor";
                string fileName = $"{Guid.NewGuid().ToString()}.jpg";
                FileHelpers.SaveFileImageSecure(model.ImageCardIdFont, basePath, fileName, Constants.Image.MaximumImageStored);
                string path = Path.Combine(basePath, fileName);
                model.ImageCardIdFont = $"{connectionApi}/static/{path}";
            }

            // change NationalIdNumber back
            if (model.ImageCardIdBack.IsTextBase64())
            {
                if (!string.IsNullOrWhiteSpace(visit.ImageCardIdBack))
                {
                    FileHelpers.DeleteFileFromLink(visit.ImageCardIdBack.Replace($"{connectionApi}/static/", ""));
                }

                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/visitor";
                string fileName = $"{Guid.NewGuid().ToString()}.jpg";
                FileHelpers.SaveFileImageSecure(model.ImageCardIdBack, basePath, fileName, Constants.Image.MaximumImageStored);
                string path = Path.Combine(basePath, fileName);
                model.ImageCardIdBack = $"{connectionApi}/static/{path}";
            }

            _visitService.Update(model);

            try
            {
                var visitSetting = _visitService.GetVisitSettingByCompanyId(companyId);
                string warningLocation = _visitService.CheckLocationWarning(model.Address, visitSetting.AllLocationWarning, visitSetting.CompanyId);
                if (!string.IsNullOrWhiteSpace(warningLocation))
                {
                    _notificationService.SendMessage(Constants.MessageType.Error,
                        Constants.NotificationType.SendUserError,
                        _httpContext.User.GetUsername(),
                        warningLocation,
                        company.Id);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(("**************" + ex.Message));
            }

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, VisitResource.lblVisit.ToLower(), ""));
        }

        /// <summary>
        /// Edit visitor by id (No option(condition))
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <param name="model">JSON model for Visit</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: account not role edit. Or status is not waiting</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit Id does not exist in DB</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiVisitsIdForced)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitManagement)]
        public IActionResult EditForced(int id, [FromBody] VisitModel model)
        {
            model.Id = id;
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }
            var visit = _visitService.GetById(id);
            if (visit == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }

            _visitService.Update(model);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, VisitResource.lblVisit.ToLower(), ""));
        }

        ///// <summary>
        ///// Change status
        ///// </summary>
        ///// <param name="ids">List of Visit Ids</param>
        ///// <param name="statusId">Status Id</param>
        ///// <param name="cardId">Card Id</param>
        ///// <param name="reason">Detail for Reason</param>
        ///// <returns></returns>
        ///// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        ///// <response code="404">Not Found: List of Visit Ids does not exist in DB</response>
        ///// <response code="422">Unprocessable Entity: Data CardId of Model JSON wrong</response>
        ///// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        //[HttpPut]
        //[Route(Constants.Route.ApiVisitsChangeStatus)]
        ////[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        ////[CheckPermission(PermissionGroupName.Visit, PermissionActionName.Edit)]
        //public IActionResult ChangeStatus(List<int> ids, int statusId, string cardId, string reason)
        //{

        //    var visits = _visitService.GetByIds(ids);
        //    if (visits == null)
        //    {
        //        return new ApiErrorResult(StatusCodes.Status404NotFound);
        //    }

        //    //회수 처리
        //    if (statusId == (short)VisitStatus.Reclamation)
        //    {
        //        //if (visits.Count > 1)
        //        //{
        //        //    //교부되지 않은 카드입니다.
        //        //    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(VisitResource.msgNotDeliveredCard));
        //        //}
        //        var visit = _visitService.GetByCardId(cardId);
        //        if (visit == null)
        //        {
        //            //교부되지 않은 카드입니다.
        //            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(VisitResource.msgNotDeliveredCard));
        //        }

        //        if (visit.Status == (short)VisitStatus.Reclamation)
        //        {
        //            //이미 회수된 카드입니다.
        //            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(VisitResource.msgAlreadyReturnedCard));
        //        }

        //        var card = _visitService.GetCardByCardId(cardId);
        //        if (card.CardType != (short)PassType.VisitCard)
        //        {
        //            //방문증이 아닙니다.
        //            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(VisitResource.msgNotVisitCard));
        //        }

        //        _visitService.ReturnCard(cardId);
        //    }

        //    //교부 처리
        //    else if (statusId == (short)VisitStatus.DeliveryOk)
        //    {

        //        var visitCard = _visitService.GetCardByCardId(cardId);
        //        var visit = _visitService.GetById(ids.FirstOrDefault());

        //        if (visit != null && visit.Status != (short)VisitStatus.Issueing)
        //        {
        //            //승인되지 않았습니다.
        //            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(VisitResource.msgNotApprovedVisit));
        //        }

        //        if (visitCard != null && visitCard.VisitId != null)
        //        {
        //            //이미 교부된 카드입니다.
        //            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
        //                string.Format(VisitResource.msgAlreadyDeliveredCard));
        //        }
        //        _visitService.PassOnCard(ids.FirstOrDefault(), cardId);
        //    }

        //    //이 외에
        //    else
        //    {
        //        _visitService.ChangeStatus(statusId, ids, reason);
        //    }


        //    return new ApiSuccessResult(StatusCodes.Status200OK,
        //        string.Format(MessageResource.MessageUpdateSuccess, VisitResource.lblVisit, ""));
        //}

        /// <summary>
        ///  Export visits to file
        /// </summary>
        /// <param name="model"></param>
        /// <param name="startDateFrom">start filter open visit from</param>
        /// <param name="endDateFrom">end filter open visit from</param>
        /// <param name="visitorName">name of visit</param>
        /// <param name="birthDay">date of birthday</param>
        /// <param name="visitorDepartment">department of visit</param>
        /// <param name="position">position</param>
        /// <param name="visiteeSite">site of visit</param>
        /// <param name="visitReason">reason of visit</param>
        /// <param name="visiteeName">name of visit</param>
        /// <param name="phone">phone number</param>
        /// <param name="cardStatus">status of card</param>
        /// <param name="approverName1">approve name 1</param>
        /// <param name="approverName2">approve name 2</param>
        /// <param name="rejectReason">reason for reject</param>
        /// <param name="cardId">card id</param>
        /// <param name="search">search general</param>
        /// <param name="type">type of file export</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [Produces("application/csv", "application/ms-excel", "application/json")]
        [HttpGet]
        [Route(Constants.Route.ApiVisitsExport)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Export + Page.VisitManagement)]
        public IActionResult ExportVisitManage(VisitOperationTime model, string startDateFrom, string endDateFrom, string visitorName,
            string birthDay, string visitorDepartment, string position, string visiteeSite, string visitReason,
            string visiteeName, string phone, List<int> cardStatus, string approverName1, string approverName2,
            string rejectReason, string cardId, string search, string type = "excel", string sortColumn = "VisitorName", string sortDirection = "desc")
        {
            var sanitizer = new HtmlSanitizer();

            if (model == null)
            {
                return new ValidationFailedResult(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            var opeDateFrom = model.OpeDateFrom.ToLower().Trim();
            opeDateFrom = Regex.Replace(opeDateFrom, @"\s+", " ");
            opeDateFrom = sanitizer.Sanitize(opeDateFrom);

            var opeDateTo = model.OpeDateTo.ToLower().Trim();
            opeDateTo = Regex.Replace(opeDateTo, @"\s+", " ");
            opeDateTo = sanitizer.Sanitize(opeDateTo);
            var filter = new VisitFilterModel()
            {
                AccessDateFrom = model.OpeDateFrom,
                AccessDateTo = model.OpeDateTo,
                StartDateFrom = startDateFrom,
                EndDateFrom = endDateFrom,
                VisitorName = visitorName,
                BirthDay = birthDay,
                VisitorDepartment = visitorDepartment,
                Position = position,
                VisiteeSite = visiteeSite,
                VisitReason = visitReason,
                VisiteeName = visiteeName,
                Phone = phone,
                ProcessStatus = cardStatus,
                ApproverName1 = approverName1,
                ApproverName2 = approverName2,
                RejectReason = rejectReason,
                CardId = cardId,
                Search = search,
                PageNumber = 0,
                PageSize = 0,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                IsOnyVisitTarget = false,
                ExportType = type,
            };
            var fileData = _visitService.Export(filter, out var totalRecords, out var recordsFiltered);
            var filename = string.Format(Constants.ExportFileFormat, VisitResource.lblExport + "_Visitor", DateTime.Now);
            var fullName = type == "excel" ? $"{filename}.xlsx" : $"{filename}.csv";
            if (totalRecords == 0 || recordsFiltered == 0)
            {
                return new ApiSuccessResult(StatusCodes.Status422UnprocessableEntity,
                    string.Format(MessageResource.MessageExportDataIsZero, VisitResource.lblVisit.ToLower()));
            }

            return File(fileData, type.Equals("excel") ? "application/ms-excel" : "application/csv", fullName);
        }

        /// <summary>
        /// Export visit with report to file
        /// </summary>
        /// <param name="accessDateFrom">Date Access Start</param>
        /// <param name="accessDateTo">Date Access End</param>
       
        /// <param name="doorIds">list of door ids</param>
        /// <param name="DeviceReaderIds">list of device reader ids</param>
        /// <param name="visitorName">name of visitor</param>
        /// <param name="cardId">card id</param>
        /// <param name="search">search general</param>
        /// <param name="inOutType">type of visit(in/out)</param>
        /// <param name="eventType">list of event types</param>
        /// <param name="visiteeSite">visit of website</param>
        /// <param name="cardStatus">status of card</param>
        /// <param name="cardTypes">list of card types</param>
        /// <param name="birthDay">date of birthday</param>
        /// <param name="visitorDepartment">visitor department</param>
        /// <param name="building">building of icu</param>
        /// <param name="type"></param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [Produces("application/csv", "application/ms-excel", "application/json")]
        [HttpGet]
        [Route(Constants.Route.ApiVisitsReportExport)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Export + Page.VisitReport)]
        public IActionResult ExportVisitReportManage(string accessDateFrom,
            string accessDateTo, List<int> doorIds, List<int> DeviceReaderIds, string visitorName, string cardId, string search, List<int> inOutType, List<int> eventType,
            string visiteeSite, List<int> cardStatus, List<int> cardTypes, string birthDay, string visitorDepartment, List<int> building, string type = "excel", string sortColumn = "EventTime", string sortDirection = "desc")
        {
            sortColumn = SortColumnMapping.VisitReportColumn(sortColumn);
            if (string.IsNullOrWhiteSpace(accessDateFrom) || string.IsNullOrWhiteSpace(accessDateTo))
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                    string.Format(MessageResource.InvalidDateFormat, EventLogResource.lblAccessDateFrom, Helpers.GetDateServerFormat()));
            }
            int dayExport = _configuration.GetSection(Constants.Settings.DefineLimitRangeDayExport).Get<int>();
            dayExport = dayExport == 0 ? 90 : dayExport;
            DateTime start = DateTime.ParseExact(accessDateFrom, Constants.DateTimeFormatDefault, CultureInfo.InvariantCulture);
            DateTime end = DateTime.ParseExact(accessDateTo, Constants.DateTimeFormatDefault, CultureInfo.InvariantCulture);
            if ((end - start).Days > dayExport)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(EventLogResource.msgErrorLimitRangeDayExport, dayExport));
            }

            var fileData = _visitService.ExportVisitReport(type, accessDateFrom,
                accessDateTo, doorIds, DeviceReaderIds,
                visitorName, cardId, search, inOutType, eventType, visiteeSite,
                cardStatus, cardTypes, birthDay, visitorDepartment, building, sortColumn, sortDirection, out var totalRecords, out var recordsFiltered);
            var filename = string.Format(Constants.ExportFileFormat, VisitResource.lblExport + "_VisitorReport", DateTime.Now);
            var fullName = type == "excel" ? $"{filename}.xlsx" : $"{filename}.csv";
            if (totalRecords == 0 || recordsFiltered == 0)
            {
                return new ApiSuccessResult(StatusCodes.Status422UnprocessableEntity,
                    string.Format(MessageResource.MessageExportDataIsZero, VisitResource.lblVisit.ToLower()));
            }

            return File(fileData, type.Equals("excel") ? "application/ms-excel" : "application/csv", fullName);
        }



        /// <summary>
        /// init pre-register visit 
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVisitsPreRegisterInit)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult VisitPreRegisterInit()
        {
            var model = new VisitDataModel();
            _visitService.InitData(model);
            model.VisitType = (short)VisitType.OutSider;
            model.ListCardStatus = EnumHelper.ToEnumList<VisitingCardStatusPreRegisterType>();
            model.CardStatus = (short)VisitingCardStatusPreRegisterType.Request;
            return Ok(model);
        }

        /// <summary>
        /// Add new a visit
        /// </summary>
        /// <param name="model">JSON model for Visit</param>
        /// <returns></returns>
        /// <response code="201">Create new a visit</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiVisitsPreRegister)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult VisitPreRegister([FromBody] VisitModel model)
        {
            //if (!ModelState.IsValid)
            //{
            //    return new ValidationFailedResult(ModelState);
            //}

            _visitService.PreRegister(model);
            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, VisitResource.lblVisit.ToLower()));
        }

        /// <summary>
        /// Update visit setting
        /// </summary>
        /// <param name="model">JSON model for Setting visit</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Data of Approvals wrong</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Data of Approvals wrong</response>
        /// <response code="404">Not Found: Access Time Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiVisitsSetting)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitSetting)]
        public IActionResult EditVisitSettingType([FromBody] VisitSettingModel model)
        {
            try
            {
                // Validation for ListVisitPurpose field
                if (!string.IsNullOrEmpty(model.ListVisitPurpose))
                {
                    if (model.ListVisitPurpose.Length > 1000)
                    {
                        return new ApiErrorResult(StatusCodes.Status400BadRequest,
                            string.Format(MessageResource.LengthNotGreaterThan, VisitResource.lblListVisitPurpose, "1000"));
                    }
                    if (Helpers.ContainsCode(model.ListVisitPurpose))
                    {
                        return new ApiErrorResult(StatusCodes.Status400BadRequest, 
                            string.Format(MessageResource.msgInvalidInput, VisitResource.lblListVisitPurpose));
                    }
                }

                _visitService.UpdateVisitSettingCompany(model);

                return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, VisitResource.lblVisitSetting.ToLower(), ""));
            }
            catch (Exception)
            {
                return new ApiSuccessResult(StatusCodes.Status422UnprocessableEntity,
                string.Format(MessageResource.MessageUpdateFailed, VisitResource.lblVisitSetting.ToLower()));
            }


        }

        /// <summary>
        /// Get visit setting
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiVisitsSetting)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetVisitSetting()
        {
            var visitSetting = _visitService.GetVisitSettingCompany();
            var visitSettingInitModel = _mapper.Map<VisitSettingInitModel>(visitSetting);

            return Ok(visitSettingInitModel);
        }

        /// <summary>
        /// Get identification of visitor
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiVisitIdentification)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult GetIdentificationVisitor(int id)
        {
            var cards = _visitService.GetCardByVisitor(id);
            return Ok(cards);
        }

        /// <summary>
        /// Add identification to visitor
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <param name="model">JSON model for identification of visitor</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Data of Approval wrong</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit id does not exist in DB</response>
        /// <response code="422">Not Found: Card id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiVisitIdentification)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitManagement)]
        public IActionResult AddIdentificationVisitor(int id, [FromBody] CardModel model)
        {
            if (model.CardId != null)
            {
                var visitor = _visitService.GetById(id);
                if (visitor == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
                }

                if (visitor.Status == (short)VisitChangeStatusType.Approved
                        || visitor.Status == (short)VisitChangeStatusType.CardReturned
                        || visitor.Status == (short)VisitChangeStatusType.AutoApproved)
                {
                    var isCardExist = _visitService.IsCardIdExist(model.CardId, visitor.CompanyId);

                    if (isCardExist)
                    {
                        return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.Exist, UserResource.lblCardId));
                    }
                    else
                    {
                        var card = _visitService.GetCardByVisitor(id);

                        if (card != null)
                        {
                            if (card.CardId != model.CardId)
                            {
                                _visitService.DeletedAssignedCardVisitor(id);

                                var cardId = _visitService.AssignedCardVisitor(id, model);
                            }

                            return new ApiSuccessResult(StatusCodes.Status200OK,
                                string.Format(MessageResource.MessageSuccessIssueCard));
                        }
                        else
                        {
                            var cardId = _visitService.AssignedCardVisitor(id, model);

                            return new ApiSuccessResult(StatusCodes.Status200OK,
                                string.Format(MessageResource.MessageSuccessIssueCard));
                        }
                    }
                }
                else
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest,
                        MessageResource.CannotAssignedCardNotApproved);
                }

            }
            return new ApiErrorResult(StatusCodes.Status400BadRequest,
                MessageResource.CardIdBlank);
        }


        /// <summary>
        /// Add identification to multiple visitor
        /// </summary>
        /// <param name="models">JSON model for identification of multiple visitor</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Data of Approval wrong</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit id does not exist in DB</response>
        /// <response code="422">Not Found: Card id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiVisitIdentificationMulti)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitManagement)]
        public IActionResult AddIdentificationMultiVisitor([FromBody] List<MultipleVisitorCardModel> models)
        {
            if (models == null || !models.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.NoData);
            }

            List<string> duplicatedCardId = new List<string>();
            int totalCount = models.Count;
            int successCount = 0;

            // Set culture.
            var currentAcocunt = _accountService.GetById(_httpContext.User.GetAccountId());
            if (currentAcocunt != null)
            {
                var currentCulture = new CultureInfo(currentAcocunt.Language);
                Thread.CurrentThread.CurrentUICulture = currentCulture;
            }

            foreach (var model in models)
            {
                if (!string.IsNullOrWhiteSpace(model.CardModel.CardId))
                {
                    model.CardModel.CardId = model.CardModel.CardId.RemoveAllEmptySpace();

                    var visitor = _visitService.GetById(model.Id);
                    if (visitor == null)
                    {
                        continue;
                    }

                    if (visitor.Status == (short)VisitChangeStatusType.Approved
                        || visitor.Status == (short)VisitChangeStatusType.CardReturned
                        || visitor.Status == (short)VisitChangeStatusType.AutoApproved)
                    {
                        var isCardExist = _visitService.IsCardIdExist(model.CardModel.CardId, visitor.CompanyId);
                        if (isCardExist)
                        {
                            _visitService.WriteSystemLog(model.Id,
                                ActionLogType.Update,
                                string.Format(MessageResource.Exist, UserResource.lblCard),
                                $"{VisitResource.lblVisitName} : {visitor.VisitorName}<br />{UserResource.lblCardId} : {model.CardModel.CardId}");

                            duplicatedCardId.Add(model.CardModel.CardId);
                        }
                        else
                        {
                            try
                            {
                                var card = _visitService.GetCardByVisitor(model.Id);
                                if (card != null)
                                {
                                    if (card.CardId != model.CardModel.CardId)
                                    {
                                        _visitService.DeletedAssignedCardVisitor(model.Id);
                                        var cardId = _visitService.AssignedCardVisitor(model.Id, model.CardModel);

                                        _visitService.WriteSystemLog(model.Id,
                                            ActionLogType.Update,
                                            string.Format(MessageResource.MessageAssignSuccess, UserResource.lblCard),
                                            $"{VisitResource.lblVisitName} : {visitor.VisitorName}<br />{VisitResource.lblCardID} : {model.CardModel.CardId}");

                                        successCount++;
                                    }
                                }
                                else
                                {
                                    var cardId = _visitService.AssignedCardVisitor(model.Id, model.CardModel);

                                    _visitService.WriteSystemLog(model.Id,
                                        ActionLogType.Update,
                                        string.Format(MessageResource.MessageAssignSuccess, UserResource.lblCard),
                                        $"{VisitResource.lblVisitName} : {visitor.VisitorName}<br />{VisitResource.lblCardID} : {model.CardModel.CardId}");

                                    successCount++;
                                }
                            }
                            catch (Exception e)
                            {
                                _visitService.WriteSystemLog(model.Id,
                                ActionLogType.Update,
                                string.Format(MessageResource.AnUnexpectedErrorOccurred),
                                $"{VisitResource.lblVisitName} : {visitor.VisitorName}<br />{UserResource.lblCardId} : {model.CardModel.CardId}<br />{e.Message}");

                                continue;
                            }
                        }
                    }
                    else
                    {
                        _visitService.WriteSystemLog(model.Id,
                                ActionLogType.Update,
                                $"{MessageResource.CannotAssignCardStatus}",
                                $"{VisitResource.lblVisitName} : {visitor.VisitorName}<br />{VisitResource.lblVisitStatus} : {((VisitChangeStatusType)visitor.Status).GetDescription()}");

                        continue;
                    }
                }
            }

            if (totalCount == successCount)
            {
                return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageAssignSuccess,
                                UserResource.lblCard));
            }

            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                string.Format(MessageResource.msgAssignSuccessFail,
                                UserResource.lblCard,
                                successCount,
                                (totalCount - successCount),
                                duplicatedCardId.Count));
        }



        /// <summary>
        /// Add and send QR to visitor
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <param name="email">Email for visitor</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Data of Approval wrong. Or email empty</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiVisitQR)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitManagement)]
        public IActionResult AddQRVisitor(int id, string email)
        {
            if (!string.IsNullOrWhiteSpace(email))
            {
                var visitor = _visitService.GetById(id);

                if (visitor == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
                }

                visitor.Email = email;

                if (visitor.Status == (short)VisitChangeStatusType.Approved
                        || visitor.Status == (short)VisitChangeStatusType.CardReturned
                        || visitor.Status == (short)VisitChangeStatusType.AutoApproved)
                {
                    _visitService.AssignedQRVisitor(id, email);

                    return new ApiSuccessResult(StatusCodes.Status200OK,
                        string.Format(MessageResource.MessageSuccessIssueQR));
                }
                else
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest,
                        MessageResource.CannotAssignedQRNotApproved);
                }

            }
            return new ApiErrorResult(StatusCodes.Status400BadRequest,
                MessageResource.EmailEmpty);
        }

        /// <summary>
        /// Get string base 64 qr-code
        /// </summary>
        /// <param name="tokenVisit">token include: company code and visitorId</param>
        /// <returns></returns>
        /// <response code="404">Company does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        // [ResponseCache(Duration = 0, Location  = ResponseCacheLocation.None, NoStore = true)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route(Constants.Route.ApiQRCodeSMS)]
        [AllowAnonymous]
        public IActionResult LinkQRCodeVisitor(string tokenVisit)
        {
            var jwt = new JwtSecurityTokenHandler().ReadToken(tokenVisit) as JwtSecurityToken;
            var str_visitorId = jwt.Claims.First(claim => claim.Type == Constants.ClaimName.VisitorId).Value;
            int visitorId = 0;
            if (!int.TryParse(str_visitorId, out visitorId))
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }
            var qrCode = _visitService.CreateQRCodeForVisitor(visitorId);
            if (qrCode == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundQrCode);

            return Ok(qrCode);
        }

        /// <summary>
        /// Accept visit approval
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <param name="model">JSON model include approved boolean</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Data of Approval wrong</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPatch]
        [Route(Constants.Route.ApiApprovedVisitor)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Approve + Page.VisitManagement)]
        public IActionResult ApprovedVisitor(int id, [FromBody] ApprovedModel model)
        {
            var visitSetting = _visitService.GetVisitSettingCompany();

            var visitor = _visitService.GetById(id);
            int createdBy = visitor.CreatedBy;
            if (visitor.Id == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }

            if (visitor.Status == (short)VisitChangeStatusType.Rejected ||
                visitor.Status == (short)VisitChangeStatusType.Finished ||
                visitor.Status == (short)VisitChangeStatusType.FinishedWithoutReturnCard ||
                visitor.Status == (short)VisitChangeStatusType.CardIssued)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    MessageResource.CannotContinueApproved);
            }

            if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.NoStep)
            {
                if (_httpContext.User.GetAccountId() != visitor.ApproverId1)
                {
                    return new ApiErrorResult(StatusCodes.Status403Forbidden,
                        MessageResource.PermissionforApproved);
                }
                if (model.Approved)
                {
                    _visitService.UpdateApprovalVisitor(id, (short)VisitChangeStatusType.Approved, true);

                    //send message to created by 
                    if (_httpContext.User.GetAccountId() != visitor.CreatedBy)
                    {
                        var noti = _notificationService.MappingNoti(_httpContext.User.GetCompanyId(), createdBy, (short)NotificationType.NotificationVisit, "ApproveNotification"
                            , JsonConvert.SerializeObject(new { visitor_name = visitor.VisitorName, status = "Approved" }).ToString(), "/visit");

                        _notificationService.AddNotification(noti, (short)NotificationType.NotificationVisit);
                    }

                    // HanetUpdateAvatar(visitor, Helpers.GetImageBase64FromUrl(visitor.Avatar));
                }
                else
                {
                    _visitService.UpdateApprovalVisitor(id, (short)VisitChangeStatusType.Rejected, true);

                    //send message to created by 
                    if (_httpContext.User.GetAccountId() != visitor.CreatedBy)
                    {
                        var noti = _notificationService.MappingNoti(_httpContext.User.GetCompanyId(), createdBy, (short)NotificationType.NotificationVisit, "RejectNotification",
                            JsonConvert.SerializeObject(new { visitor_name = visitor.VisitorName, status = "Rejected" }).ToString(), "/visit");

                        _notificationService.AddNotification(noti, (short)NotificationType.NotificationVisit);
                    }
                }
                return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.ApprovedSuccessfully);
            }
            else
            {
                if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.FirstStep)
                {
                    if (visitor.Status == (short)VisitChangeStatusType.Waiting)
                    {
                        if (_httpContext.User.GetAccountId() != visitor.ApproverId1)
                        {
                            return new ApiErrorResult(StatusCodes.Status403Forbidden,
                                MessageResource.PermissionforApproved);
                        }

                        if (model.Approved)
                        {
                            _visitService.UpdateApprovalVisitor(id, (short)VisitChangeStatusType.Approved, true);

                            //send message to created by
                            if (_httpContext.User.GetAccountId() != visitor.CreatedBy)
                            {
                                var noti = _notificationService.MappingNoti(_httpContext.User.GetCompanyId(), createdBy, (short)NotificationType.NotificationVisit, "ApproveNotification"
                                    , JsonConvert.SerializeObject(new { visitor_name = visitor.VisitorName, status = "Approved" }).ToString(), "/visit");

                                _notificationService.AddNotification(noti, (short)NotificationType.NotificationVisit);
                            }
                            // HanetUpdateAvatar(visitor, Helpers.GetImageBase64FromUrl(visitor.Avatar));
                        }
                        else
                        {
                            _visitService.UpdateApprovalVisitor(id, (short)VisitChangeStatusType.Rejected, true);

                            //send message to created by 
                            if (_httpContext.User.GetAccountId() != visitor.CreatedBy)
                            {
                                var noti = _notificationService.MappingNoti(_httpContext.User.GetCompanyId(), createdBy, (short)NotificationType.NotificationVisit, "RejectNotification",
                                    JsonConvert.SerializeObject(new { visitor_name = visitor.VisitorName, status = "Rejected" }).ToString(), "/visit");

                                _notificationService.AddNotification(noti, (short)NotificationType.NotificationVisit);
                            }
                        }
                        return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.ApprovedSuccessfully);
                    }
                    else
                    {
                        return new ApiErrorResult(StatusCodes.Status400BadRequest,
                            MessageResource.CannotContinueApproved);
                    }
                }
                else
                {
                    var lstSecondApproved = new string[] { _visitService.GetVisitSettingCompany().SecondsApproverAccounts };
                    List<string> numbers = lstSecondApproved[0].Replace("[", "").Replace("]", "").Split(',').ToList<string>();

                    if (visitor.Status == (short)VisitChangeStatusType.Waiting)
                    {
                        if (_httpContext.User.GetAccountId() != visitor.ApproverId1)
                        {
                            return new ApiErrorResult(StatusCodes.Status403Forbidden,
                                MessageResource.PermissionforApproved);
                        }
                        if (model.Approved)
                        {

                            _visitService.UpdateApprovalVisitor(id, (short)VisitChangeStatusType.Approved1, true);

                            foreach (var item in numbers)
                            {
                                //check item
                                var _noti = _notificationService.MappingNoti(_httpContext.User.GetCompanyId(), Convert.ToInt32(item), (short)NotificationType.NotificationVisit,
                                    "contentVisitorNotification", JsonConvert.SerializeObject(new { visitor_name = visitor.VisitorName, status = "Approved" }).ToString(), "/visit?id=" + id + "");
                                _notificationService.AddNotification(_noti, (short)NotificationType.NotificationVisit);
                            }
                            // HanetUpdateAvatar(visitor, Helpers.GetImageBase64FromUrl(visitor.Avatar));
                        }
                        else
                        {
                            _visitService.UpdateApprovalVisitor(id, (short)VisitChangeStatusType.Rejected, true);

                            var noti = _notificationService.MappingNoti(_httpContext.User.GetCompanyId(), createdBy, (short)NotificationType.NotificationVisit,
                                 "RejectNotification", JsonConvert.SerializeObject(new { visitor_name = visitor.VisitorName, status = "Rejected" }).ToString(), "/visit");
                            _notificationService.AddNotification(noti, (short)NotificationType.NotificationVisit);



                        }
                        return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.ApprovedSuccessfully);
                    }
                    else if (visitor.Status == (short)VisitChangeStatusType.Approved1)
                    {
                        var secondApprovalAccounts = JsonConvert.DeserializeObject<List<int>>(visitSetting.SecondsApproverAccounts);
                        if (!secondApprovalAccounts.Contains(_httpContext.User.GetAccountId()))
                        {
                            return new ApiErrorResult(StatusCodes.Status403Forbidden,
                                MessageResource.PermissionforApproved);
                        }
                        if (model.Approved)
                        {
                            _visitService.UpdateApprovalVisitor(id, (short)VisitChangeStatusType.Approved, true);

                            var noti = _notificationService.MappingNoti(_httpContext.User.GetCompanyId(), createdBy, (short)NotificationType.NotificationVisit,
                                "ApproveNotification", JsonConvert.SerializeObject(new { visitor_name = visitor.VisitorName, status = "Approved" }).ToString(), "/visit");
                            _notificationService.AddNotification(noti, (short)NotificationType.NotificationVisit);

                            // HanetUpdateAvatar(visitor, Helpers.GetImageBase64FromUrl(visitor.Avatar));
                        }
                        else
                        {
                            _visitService.UpdateApprovalVisitor(id, (short)VisitChangeStatusType.Rejected, true);
                            var noti = _notificationService.MappingNoti(_httpContext.User.GetCompanyId(), createdBy, (short)NotificationType.NotificationVisit,
                                "RejectNotification", JsonConvert.SerializeObject(new { visitor_name = visitor.VisitorName, status = "Rejected" }).ToString(), "/visit");

                            _notificationService.AddNotification(noti, (short)NotificationType.NotificationVisit);
                        }
                        return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.ApprovedSuccessfully);
                    }
                    else
                    {
                        return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.CannotContinueApproved);
                    }
                }
            }
        }



        /// <summary>
        /// Accept multiple visit approval
        /// </summary>
        /// <param name="ids"> a list of visitor identifier </param>
        /// <param name="model">JSON model include approved boolean</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Data of Approval wrong</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPatch]
        [Route(Constants.Route.ApiApprovedVisitorMulti)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Approve + Page.VisitManagement)]
        public IActionResult ApprovedMultipleVisitor(List<int> ids, [FromBody] ApprovedModel model)
        {
            if (ids is null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }

            var visitors = _visitService.GetByIds(ids);

            if (!visitors.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.NoVisitorInformation);
            }

            var visitSetting = _visitService.GetVisitSettingCompany();
            var currentAccountId = _httpContext.User.GetAccountId();
            var currentCompanyId = _httpContext.User.GetCompanyId();
            
            List<short> cannotApprovedStatus = new List<short>()
            {
                (short)VisitChangeStatusType.Rejected,
                (short)VisitChangeStatusType.Finished,
                (short)VisitChangeStatusType.FinishedWithoutReturnCard,
                (short)VisitChangeStatusType.CardIssued
            };

            // Check visitor's status
            var cannotApprovedVisitors = visitors.Where(v => cannotApprovedStatus.Contains(v.Status)).ToList();
            if (cannotApprovedVisitors.Any())
            {
                var visitorNames = cannotApprovedVisitors.Select(v => v.VisitorName).ToList();

                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    $"{MessageResource.CannotContinueApproved} ({string.Join(", ", visitorNames)})");
            }

            // Check permission
            List<string> notPermittedVisitor = new List<string>();

            foreach (var visitor in visitors)
            {
                if ((visitor.Status == (short)VisitChangeStatusType.Waiting && currentAccountId != visitor.ApproverId1)
                    || (visitor.Status == (short)VisitChangeStatusType.Approved1 && currentAccountId != visitor.ApproverId2))
                {
                    notPermittedVisitor.Add(visitor.VisitorName);
                }
            }
            if (notPermittedVisitor.Any())
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden,
                    $"{MessageResource.PermissionforApproved} ({string.Join(", ", notPermittedVisitor)})");
            }

            List<int> secondApproverIds = new List<int>();

            foreach (var visitor in visitors)
            {
                try
                {
                    int createdBy = visitor.CreatedBy;

                    var status = "Rejected";
                    string resourceName = "RejectNotification";
                    short updatedStatus = (short)VisitChangeStatusType.Rejected;

                    if (model.Approved)
                    {
                        status = "Approved";
                        resourceName = "ApproveNotification";
                        updatedStatus = (short)VisitChangeStatusType.Approved;

                        if (visitor.Status == (short)VisitChangeStatusType.Waiting)
                        {
                            if (visitor.ApproverId2 != 0)
                            {
                                // This visitor needs to get the first and second approval.
                                updatedStatus = (short)VisitChangeStatusType.Approved1;

                                //// if visitor needs to get the approval by today, save approver ids to send Push Notification 
                                //if (visitor.StartDate == DateTime.Today.ConvertToSystemTime())
                                //    secondApproverIds.Add(visitor.ApproverId2);


                                //var test = DateTime.Today.ToUniversalTime();
                                //Console.WriteLine($"##### DATETIME UNIV : {test}");
                                //Console.WriteLine($"##### visitor.StartDate : {visitor.StartDate}");
                            } 
                        }

                        // if (!string.IsNullOrEmpty(visitor.Avatar))
                        // {
                        //     HanetUpdateAvatar(visitor, Helpers.GetImageBase64FromUrl(visitor.Avatar));
                        // }
                    }

                    // Make notification
                    if (currentAccountId != createdBy)
                    {
                        var noti = _notificationService.MappingNoti(currentCompanyId, createdBy, (short)NotificationType.NotificationVisit, resourceName
                                    , JsonConvert.SerializeObject(new { visitor_name = visitor.VisitorName, status = status }).ToString(), "/visit");

                        _notificationService.AddNotification(noti, (short)NotificationType.NotificationVisit);
                    }

                    _visitService.UpdateApprovalVisitor(visitor.Id, updatedStatus, false);
                }
                catch (Exception e)
                {
                    _visitService.WriteSystemLog(visitor.Id,
                                ActionLogType.Fail,
                                string.Format(MessageResource.AnUnexpectedErrorOccurred),
                                $"{VisitResource.lblVisitName} : {visitor.VisitorName}<br />{e.Message}");

                    return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.CannotContinueApproved);
                }
            }

            return new ApiSuccessResult(StatusCodes.Status200OK, model.Approved ? MessageResource.ApprovedSuccessfully : MessageResource.RejectSuccess);
        }

        /// <summary>
        /// Assign doors to visitor ( AccessGroup )
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <param name="door">List of door ids</param>
        /// <returns></returns>
        /// <response code="201">Assign door to visit success</response>
        /// <response code="400">Bad Request: Data of Model JSON wrong</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiAssignedDoorVisitor)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitManagement)]
        public IActionResult AssingedDoorVisitor(int id, List<int> door)
        {
            var success = _visitService.AssignedDoorVisitor(id, door);
            if (success)
            {
                return new ApiSuccessResult(StatusCodes.Status201Created,
                    string.Format(MessageResource.MessageAddSuccess, AccessGroupResource.lblAccessGroup));
            }
            return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.CannotAssignedDoorVisitor);
        }

        /// <summary>
        /// Reject visit approval
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <param name="model">JSON model include reason reject</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Data of Model JSON wrong</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPatch]
        [Route(Constants.Route.ApiRejectVisitor)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Approve + Page.VisitManagement)]
        public IActionResult RejectVisitor(int id, [FromBody] RejectedModel model)
        {
            var success = _visitService.RejectVisitor(id, model);
            var visit = _visitService.GetById(id);
            if (success)
            {

                var noti = _notificationService.MappingNoti(_httpContext.User.GetCompanyId(), visit.CreatedBy, (short)NotificationType.NotificationVisit,
                                    "RejectNotification", JsonConvert.SerializeObject(new { visitor_name = visit.VisitorName, status = "Rejected" }).ToString(), "/visit");
                _notificationService.AddNotification(noti, (short)NotificationType.NotificationVisit);



                return new ApiSuccessResult(StatusCodes.Status200OK,
                    MessageResource.RejectSuccess);
            }

            return new ApiErrorResult(StatusCodes.Status400BadRequest,
                MessageResource.RejectFailed);
        }

        /// <summary>
        /// Get visit history
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiHistoryVisitor)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ViewHistory + Page.VisitManagement)]
        public IActionResult VisitHistoryLog(int id, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id", string sortDirection = "desc")
        {
            var visitReports = _visitService
                .GetPaginatedVisitHistoryLog(id, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered).AsEnumerable().ToList();

            var visitIds = visitReports.Select(m => m.VisitorId).ToList();
            var visitors = _visitService.GetByIds(visitIds);


            List<int> personalStatus = new List<int>()
                {
                    (int)VisitChangeStatusType.CardIssued,
                    (int)VisitChangeStatusType.CardReturned,
                    (int)VisitChangeStatusType.Finished,
                };


            // Set visitor name for multiple visitors.
            foreach (var visitReport in visitReports)
            {
                var visitor = visitors.FirstOrDefault(m => m.Id == visitReport.VisitorId);

                if (visitor != null)
                {
                    if (visitIds.Count > 1 && visitor.Id == id && !personalStatus.Contains(visitReport.NewStatus))
                    {
                        visitReport.VisitorName = string.Format(VisitResource.lblMultiVisitorName, visitor.VisitorName, visitIds.Count);
                    }
                    else
                    {
                        visitReport.VisitorName = visitor.VisitorName;
                    }
                }

                var account = _accountService.GetById(visitReport.UpdatedBy);
                var updatedByName = "";
                if(account != null)
                {
                    updatedByName = account.Username;

                    var user = _userService.GetUserByAccountId(account.Id, visitor.CompanyId);
                    if(user != null)
                    {
                        updatedByName = $"{user.FirstName}({updatedByName})";
                    }
                }

                visitReport.UpdatedByName = updatedByName;
            }

            var pagingData = new PagingData<VisitListHistoryModel>
            {
                Data = visitReports,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get count of visitor that need to review
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiGetLengthVisitsReview)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[CheckPermission(PermissionGroupName.Visit, PermissionActionName.View)]
        public IActionResult GetLengthVisitNeedToReview()
        {
            var count = _visitService.LengthVisitNeedToReview();
            return Ok(count);

        }

        /// <summary>
        /// Return visitor's card.
        /// </summary>
        /// <param name="model">JSON model include cardId, reason of visit and list of visit ids</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Card ID can not be blank</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiReturnCard)]
        //[Authorize(Policy = Constants.Policy.PrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ReturnCard + Page.VisitManagement)]
        public IActionResult ReturnCard([FromBody] GetBackVisit model)
        {
            if (model.VisitIds == null || !model.VisitIds.Any())
            {
                if (model.CardId == null || string.IsNullOrWhiteSpace(model.CardId))
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest,
                        MessageResource.CardIdBlank);
                }
                _visitService.ReturnCardVisitor(model.CardId, model.Reason);
                return new ApiSuccessResult(StatusCodes.Status200OK,
                    string.Format(MessageResource.MessageUpdateSuccess, VisitResource.lblVisit.ToLower(), ""));
            }
            _visitService.ReturnVisitor(model.VisitIds, model.Reason);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.msgReturnCardSuccess));
        }



        /// <summary>
        /// Return multiple visitor's card.
        /// </summary>
        /// <param name="models">JSON model include cardId, reason of visit and list of visit ids</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Card ID can not be blank</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiReturnCardMulti)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ReturnCard + Page.VisitManagement)]
        public IActionResult ReturnMultipleCard([FromBody] List<VisitCardReturnModel> models)
        {
            if (models == null || !models.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.NoData);
            }

            int totalCount = models.Count;
            int successCount = 0;
            List<int> notMatched = new List<int>();

            // Set culture.
            var currentAcocunt = _accountService.GetById(_httpContext.User.GetAccountId());
            if (currentAcocunt != null)
            {
                var currentCulture = new CultureInfo(currentAcocunt.Language);
                Thread.CurrentThread.CurrentUICulture = currentCulture;
            }

            foreach (var model in models)
            {
                var visit = _visitService.GetById(model.VisitId);

                if (visit == null)
                {
                    _visitService.WriteSystemLog(model.VisitId,
                        ActionLogType.Update,
                        string.Format(MessageResource.NoData), null);

                    continue;
                }

                if (model.CardId == null || string.IsNullOrWhiteSpace(model.CardId))
                {
                    _visitService.WriteSystemLog(model.VisitId,
                        ActionLogType.Update,
                        string.Format(MessageResource.Required, VisitResource.lblCardID),
                        $"{VisitResource.lblVisitName} : {visit.VisitorName}");

                    continue;
                }
                else
                {
                    model.CardId = model.CardId.RemoveAllEmptySpace();
                }

                var card = _visitService.GetCardByCardId(model.CardId);
                if (card == null)
                {
                    _visitService.WriteSystemLog(model.VisitId,
                        ActionLogType.Update,
                        string.Format(MessageResource.NonExist, VisitResource.lblCardID),
                        $"{VisitResource.lblCardID} : {model.CardId}");

                    continue;
                }

                if (card.VisitId == model.VisitId)
                {
                    try
                    {
                        // correct case
                        _visitService.ReturnCardVisitor(model.VisitId, model.CardId, model.Reason);

                        _visitService.WriteSystemLog(model.VisitId,
                            ActionLogType.Update,
                            MessageResource.msgReturnCardSuccess,
                            $"{VisitResource.lblVisitName} : {visit.VisitorName}<br />{VisitResource.lblCardID} : {model.CardId}");

                        successCount++;
                    }
                    catch (Exception e)
                    {
                        _visitService.WriteSystemLog(model.VisitId,
                                ActionLogType.Update,
                                string.Format(MessageResource.AnUnexpectedErrorOccurred),
                                $"{VisitResource.lblVisitName} : {visit.VisitorName}<br />{UserResource.lblCardId} : {model.CardId}<br />{e.Message}");

                        continue;
                    }
                }
                else
                {
                    _visitService.WriteSystemLog(model.VisitId,
                        ActionLogType.Update,
                        MessageResource.msgNotMatched,
                        $"{VisitResource.lblVisitName} : {visit.VisitorName}<br />{UserResource.lblCardId} : {model.CardId}");

                    // wrong case
                    notMatched.Add(model.VisitId);

                    continue;
                }
            }

            if (totalCount == successCount)
            {
                return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.msgReturnCardSuccess);
            }


            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                string.Format(MessageResource.msgReturnCardSuccessFail,
                successCount,
                totalCount - successCount,
                notMatched.Count));
        }



        /// <summary>
        /// Delete Visit by id
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Account has not role delete</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiVisitsId)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.VisitManagement)]
        public IActionResult Delete(int id)
        {
            var visit = _visitService.GetById(id);
            if (visit == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.DeletedVisitFailed);
            }

            var visitSetting = _visitService.GetVisitSettingByCompanyId(_httpContext.User.GetCompanyId());
            if (!visitSetting.AllowDeleteRecord && visit.Status != (short)VisitChangeStatusType.Waiting)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.DeletedVisitFailed);
            }

            if (!visitSetting.AllowDeleteRecord && _httpContext.User.GetAccountId() != visit.CreatedBy)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.DeletedVisitFailedRegisterPermission);
            }

            _visitService.Delete(id);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, VisitResource.lblVisit.ToLower(), ""));
        }

        /// <summary>
        /// Delete Visit by list of id
        /// </summary>
        /// <param name="ids">Visit Id</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Account has not role delete</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiVisits)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.VisitManagement)]
        public IActionResult DeleteRange(List<int> ids)
        {
            List<int> unDeletedIds = new List<int>();

            var visits = _visitService.GetByIds(ids);
            if (visits == null || !visits.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }

            var visitSetting = _visitService.GetVisitSettingByCompanyId(_httpContext.User.GetCompanyId());
            foreach (var visit in visits)
            {
                if (!visitSetting.AllowDeleteRecord && visit.Status != (short)VisitChangeStatusType.Waiting)
                {
                    ids.Remove(visit.Id);
                    unDeletedIds.Add(visit.Id);
                    //return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.DeletedVisitFailed);
                }

                if (!visitSetting.AllowDeleteRecord && _httpContext.User.GetAccountId() != visit.CreatedBy)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.DeletedVisitFailedRegisterPermission);
                }
            }

            _visitService.DeleteRange(ids);

            if (unDeletedIds.Any())
            {
                var unDeletedVisitors = visits.Where(m => unDeletedIds.Contains(m.Id)).Select(m => m.VisitorName).ToList();

                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.DeletedFailed, CommonResource.Visitor, string.Join(",", unDeletedVisitors)));
            }
            else
            {
                return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, VisitResource.lblVisit.ToLower(), ""));
            }
        }

        /// <summary>
        /// Delete Visit by id (No option(condition))
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Account has not role delete</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiVisitsIdForced)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.VisitManagement)]
        public IActionResult DeleteForced(int id)
        {
            var visit = _visitService.GetById(id);
            if (visit == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }

            _visitService.Delete(id);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, VisitResource.lblVisit.ToLower(), ""));
        }

        /// <summary>
        /// Delete Visit by list of id (No option(condition))
        /// </summary>
        /// <param name="ids"> List of visitor identifier </param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Account has not role delete</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiVisitsForced)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.VisitManagement)]
        public IActionResult DeleteRangeForced(List<int> ids)
        {
            var visits = _visitService.GetByIds(ids);
            if (visits == null || !visits.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }

            _visitService.DeleteRange(ids);

            return new ApiSuccessResult(StatusCodes.Status200OK,
            string.Format(MessageResource.MessageDeleteSuccess, VisitResource.lblVisit.ToLower(), ""));
        }

        /// <summary>
        /// Return History of Visitor
        /// </summary>
        /// <param name="id">VisitId</param>
        /// <param name="pageNumber">current page number</param>
        /// <param name="pageSize">Number of items to show on a page</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVisitsAccessHistory)]
        //[Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.ViewHistory + Page.VisitManagement)]
        public IActionResult GetHistoryVisitor(int id, int pageNumber = 1, int pageSize = 10)
        {
            var eventLogs = _visitService.GetHistoryVisitor(id, pageNumber, pageSize, out var recordsTotal, out int recordsFiltered)
                .AsEnumerable().Select(_mapper.Map<EventLogListModel>).ToList();

            var pagingData = new PagingData<EventLogListModel>
            {
                Data = eventLogs,
                Meta = { RecordsTotal = eventLogs.Count == 0 ? 0 : recordsTotal , RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get user list with pagination
        /// </summary>
        /// <param name="search">Query string that filter by user name, email, name of department or expired date</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="isValid"> valid user or invalid user or all user </param>
        /// <param name="departmentId"> identifier of department </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVisitsUsersTarget)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult GetUsersTarget(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "FirstName",
            string sortDirection = "desc", List<int> isValid = null, int departmentId = 0)
        {
            var filter = new UserFilterModel()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                FirstName = search,
                Status = isValid,
            };
            var users = _userService.GetPaginated(filter, out var recordsTotal, out var recordsFiltered, out var userHeader, "", departmentId)
                .AsEnumerable().ToList();

            var pagingData = new PagingData<UserListModel>
            {
                Data = users,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get user target by company code
        /// </summary>
        /// <param name="name"></param>
        /// <param name="departmentIds"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiRegisterVisitUsersTarget)]
        [AllowAnonymous]
        public IActionResult GetRegisterVisitUsersTarget(string name, List<int> departmentIds, int pageNumber = 1, int pageSize = 10,
            string sortColumn = "FirstName", string sortDirection = "desc", string companyCode = null)
        {
            var company = _companyService.GetByCode(companyCode);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundCompany);
            }
            var setting = _visitService.GetVisitSettingByCompanyId(company.Id);
            if (!setting.AllowGetUserTarget)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgIsAllowUserTarget);
            }

            var users = _visitService.GetUserTarget(new UserTargetFilterModel()
            {
                CompanyId = company.Id,
                Name = name,
                DepartmentIds = departmentIds,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                BothUserNoHasAccount = false,
            }, out int recordsFiltered, out int recordsTotal);

            var pagingData = new PagingData<UserListModel>
            {
                Data = users,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }
        /// <summary>
        /// Get all info visit card army
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiGetInfoVisitArmyByCardId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult GetAllInfoVisitorByCardId(string cardId)
        {
            var visitCardInfo = _visitService.GetAllInfoByCardVisit(cardId);
            if (visitCardInfo == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);

            return Ok(visitCardInfo);
        }

        /// <summary>
        /// get list first approver accounts
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiVisitsSettingFirstApprover)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitSetting)]
        public IActionResult GetFirstApproverSetting(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "UserName", string sortDirection = "asc")
        {
            int companyId = _httpContext.User.GetCompanyId();
            var visitSetting = _visitService.GetVisitSettingCompany();
            List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.FirstApproverAccounts);
            var accounts = _accountService.GetPaginatedAccountListByIds(search, accountIds, companyId, Page.VisitSetting + Page.User, pageNumber, pageSize, sortColumn, sortDirection,
                out var recordsFiltered, out var recordsTotal, out List<HeaderData> userHeader);

            var pagingData = new PagingData<AccountListModel, HeaderData>
            {
                Data = accounts,
                Header = userHeader,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// add first approver accounts
        /// </summary>
        /// <param name="ids">account ids</param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiVisitsSettingFirstApprover)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitSetting)]
        public IActionResult AddFirstApproverSetting([FromBody] List<int> ids)
        {
            int companyId = _httpContext.User.GetCompanyId();
            _visitService.AddFirstApproverSetting(ids, companyId);
            return new ApiSuccessResult(StatusCodes.Status201Created, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }

        /// <summary>
        /// delete first approver accounts
        /// </summary>
        /// <param name="ids">account ids</param>
        /// <returns></returns>
        [HttpDelete]
        [Route(Constants.Route.ApiVisitsSettingFirstApprover)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitSetting)]
        public IActionResult DeleteFirstApproverSetting(List<int> ids)
        {
            int companyId = _httpContext.User.GetCompanyId();
            _visitService.DeleteFirstApproverSetting(ids, companyId);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }

        /// <summary>
        /// get list second approver accounts
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiVisitsSettingSecondApprover)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitSetting)]
        public IActionResult GetSecondApproverSetting(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "UserName", string sortDirection = "asc")
        {
            int companyId = _httpContext.User.GetCompanyId();
            var visitSetting = _visitService.GetVisitSettingCompany();
            List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.SecondsApproverAccounts);
            var accounts = _accountService.GetPaginatedAccountListByIds(search, accountIds, companyId, Page.VisitSetting + Page.User, pageNumber, pageSize, sortColumn, sortDirection,
                out var recordsFiltered, out var recordsTotal, out List<HeaderData> userHeader);

            var pagingData = new PagingData<AccountListModel, HeaderData>
            {
                Data = accounts,
                Header = userHeader,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// add second approver accounts
        /// </summary>
        /// <param name="ids">account ids</param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiVisitsSettingSecondApprover)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitSetting)]
        public IActionResult AddSecondApproverSetting([FromBody] List<int> ids)
        {
            int companyId = _httpContext.User.GetCompanyId();
            _visitService.AddSecondApproverSetting(ids, companyId);
            return new ApiSuccessResult(StatusCodes.Status201Created, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }

        /// <summary>
        /// delete second approver accounts
        /// </summary>
        /// <param name="ids">account ids</param>
        /// <returns></returns>
        [HttpDelete]
        [Route(Constants.Route.ApiVisitsSettingSecondApprover)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitSetting)]
        public IActionResult DeleteSecondApproverSetting(List<int> ids)
        {
            int companyId = _httpContext.User.GetCompanyId();
            _visitService.DeleteSecondApproverSetting(ids, companyId);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }

        /// <summary>
        /// get list check manager accounts
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiVisitsSettingCheckManager)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitSetting)]
        public IActionResult GetCheckManagerSetting(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "UserName", string sortDirection = "asc")
        {
            int companyId = _httpContext.User.GetCompanyId();
            var visitSetting = _visitService.GetVisitSettingCompany();
            List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.VisitCheckManagerAccounts);
            var accounts = _accountService.GetPaginatedAccountListByIds(search, accountIds, companyId, Page.VisitSetting + Page.User, pageNumber, pageSize, sortColumn, sortDirection,
                out var recordsFiltered, out var recordsTotal, out List<HeaderData> userHeader);

            var pagingData = new PagingData<AccountListModel, HeaderData>
            {
                Data = accounts,
                Header = userHeader,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// add visit check manager accounts
        /// </summary>
        /// <param name="ids">account ids</param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiVisitsSettingCheckManager)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitSetting)]
        public IActionResult AddCheckManagerSetting([FromBody] List<int> ids)
        {
            int companyId = _httpContext.User.GetCompanyId();
            _visitService.AddCheckManagerSetting(ids, companyId);
            return new ApiSuccessResult(StatusCodes.Status201Created, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }

        /// <summary>
        /// delete visit check manager accounts
        /// </summary>
        /// <param name="ids">account ids</param>
        /// <returns></returns>
        [HttpDelete]
        [Route(Constants.Route.ApiVisitsSettingCheckManager)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitSetting)]
        public IActionResult DeleteCheckManagerSetting(List<int> ids)
        {
            int companyId = _httpContext.User.GetCompanyId();
            _visitService.DeleteCheckManagerSetting(ids, companyId);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }

        /// <summary>
        /// Assign doors to an access group with visitor
        /// </summary>
        /// <param name="id">Visit Id</param>
        /// <param name="model">JSON model for (doorId, timezoneId and companyId)</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Access group Id does not exist, model of the doors empty</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPost]
        [Route(Constants.Route.ApiVisitAssignDoors)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessGroup)]
        public IActionResult AssignDoors(int id, [FromBody] AccessGroupAssignDoor model)
        {
            if (id == 0 || model == null || !model.Doors.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.NotSelected, MessageResource.lblDoor));
            }

            var visit = _visitService.GetById(id);
            if (visit == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }

            var newModel = new AccessGroupAssignDoor();
            int companyId = _httpContext.User.GetCompanyId();
            int accountId = _httpContext.User.GetAccountId();
            var visitSetting = _visitService.GetVisitSettingCompany();

            // check role aprroval
            List<int> accountEdited = new List<int>()
            {
                visit.CreatedBy,
                visit.ApproverId1,
                visit.ApproverId2
            };
            if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.FirstStep)
            {
                switch (visit.Status)
                {
                    case (short)VisitChangeStatusType.Waiting:
                        {
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                    case (short)VisitChangeStatusType.Approved1:
                        {
                            accountEdited.Remove(visit.CreatedBy);
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                }
            }
            else if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.SecondStep)
            {
                switch (visit.Status)
                {
                    case (short)VisitChangeStatusType.Waiting:
                        {
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            // I make the below source code as comment because of the request of customer(army).
                            // As I know, the normal company (not army) doesn't use the SecondStep of approval step.
                            // So, I think this update doesn't effect to old normal companies, but if there are any problem caused of this update, please tell me. (Edward)
                            //else if (accountId != visit.ApproverId1 && accountId == visit.ApproverId2)
                            //{
                            //    return new ApiErrorResult(StatusCodes.Status403Forbidden, VisitResource.msgRequiredFirstApproval);
                            //}
                            break;
                        }
                    case (short)VisitChangeStatusType.Approved1:
                        {
                            accountEdited.Remove(visit.CreatedBy);
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                    case (short)VisitChangeStatusType.Approved:
                        {
                            accountEdited.Remove(visit.CreatedBy);
                            accountEdited.Remove(visit.ApproverId1);
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                    case (short)VisitChangeStatusType.AutoApproved:
                        {
                            accountEdited.Remove(visit.ApproverId1);
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                }
            }

            var accessGroupDevice = _accessGroupDeviceService.GetByAccessGroupId(visitSetting.AccessGroupId);
            foreach (var door in model.Doors)
            {
                var detailModel = new AccessGroupAssignDoorDetail()
                {
                    DoorId = door.DoorId,
                    TzId = accessGroupDevice.FirstOrDefault(x => x.IcuId == door.DoorId)?.IcuId ?? door.TzId,
                    CompanyId = companyId
                };

                newModel.Doors.Add(detailModel);
            }

            if (string.IsNullOrWhiteSpace(visit.GroupId))
            {
                var result = _accessGroupService.AssignDoors(visit.AccessGroupId, newModel);
                if (!string.IsNullOrEmpty(result))
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, result);
                }
            }
            else
            {
                // for Group visitors
                if (!string.IsNullOrWhiteSpace(visit.GroupId))
                {
                    // This visitor is group visitor.
                    var groupVisitors = _visitService.GetGroupsByIdWithTimezone(visit.Id);

                    foreach (var eachVisitor in groupVisitors)
                    {
                        var result = _accessGroupService.AssignDoors(eachVisitor.AccessGroupId, newModel);

                        if (!string.IsNullOrEmpty(result))
                        {
                            return new ApiErrorResult(StatusCodes.Status400BadRequest, result);
                        }
                    }
                }
            }

            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.msgAssignDoorSuccess);
        }

        /// <summary>
        /// Remove doors from an access group with visitors
        /// </summary>
        /// <param name="id">Visitor Id</param>
        /// <param name="doorIds">List of ids with doors</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Access group Id does not exist, model of the doors empty</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpDelete]
        [Route(Constants.Route.ApiVisitUnAssignDoors)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessGroup)]
        public IActionResult UnAssignDoors(int id, [FromBody] List<int> doorIds)
        {
            if (id == 0 || doorIds == null || !doorIds.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.NotSelected, MessageResource.lblDoor));
            }

            var visit = _visitService.GetById(id);
            if (visit == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }

            int accountId = _httpContext.User.GetAccountId();
            var visitSetting = _visitService.GetVisitSettingCompany();
            // check role aprroval
            List<int> accountEdited = new List<int>()
            {
                visit.CreatedBy,
                visit.ApproverId1,
                visit.ApproverId2
            };
            if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.FirstStep)
            {
                switch (visit.Status)
                {
                    case (short)VisitChangeStatusType.Waiting:
                        {
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                    case (short)VisitChangeStatusType.Approved1:
                        {
                            accountEdited.Remove(visit.CreatedBy);
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                }
            }
            else if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.SecondStep)
            {
                switch (visit.Status)
                {
                    case (short)VisitChangeStatusType.Waiting:
                        {
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            else if (accountId != visit.ApproverId1 && accountId == visit.ApproverId2)
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, VisitResource.msgRequiredFirstApproval);
                            }
                            break;
                        }
                    case (short)VisitChangeStatusType.Approved1:
                        {
                            accountEdited.Remove(visit.CreatedBy);
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                    case (short)VisitChangeStatusType.Approved:
                        {
                            accountEdited.Remove(visit.CreatedBy);
                            accountEdited.Remove(visit.ApproverId1);
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                    case (short)VisitChangeStatusType.AutoApproved:
                        {
                            accountEdited.Remove(visit.ApproverId1);
                            if (!accountEdited.Contains(accountId))
                            {
                                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgNotPermission);
                            }
                            break;
                        }
                }
            }

            if (string.IsNullOrWhiteSpace(visit.GroupId))
            {
                var accessGroup = _accessGroupService.GetById(visit.AccessGroupId);
                if (accessGroup == null || accessGroup.Type == (short)AccessGroupType.FullAccess)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.msgCanNotDeleteDevice);
                }

                _accessGroupService.UnAssignDoors(visit.AccessGroupId, doorIds);
            }
            else
            {
                // This visitor is group visitor.
                var groupVisitors = _visitService.GetGroupsByIdWithTimezone(visit.Id);
                foreach (var eachVisitor in groupVisitors)
                {
                    _accessGroupService.UnAssignDoors(eachVisitor.AccessGroupId, doorIds);
                }
            }

            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.msgUnassignDoorSuccess);
        }

        [HttpGet]
        [Route(Constants.Route.ApiVisitUnAssignDoors)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessGroup)]
        public IActionResult GetUnAssignDoors(int id, string search, List<int> operationType, List<int> buildingIds, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id", string sortDirection = "desc")
        {
            var visit = _visitService.GetById(id);
            if (visit == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }

            var unAssignDoors = _accessGroupService.GetPaginatedForUnAssignDoors(visit.AccessGroupId, search, operationType, buildingIds, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal, out var recordsFiltered);
            var pagingData = new PagingData<AccessGroupDeviceUnAssignDoor>
            {
                Data = unAssignDoors,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            
            return Ok(pagingData);
        }
        [HttpGet]
        [Route(Constants.Route.ApiVisitAssignDoors)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessGroup)]
        public IActionResult GetAssignDoors(int id, string search, List<int> operationType, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id", string sortDirection = "desc")
        {
            var visit = _visitService.GetById(id);
            if (visit == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }

            var assignDoors = _accessGroupService.GetPaginatedForAssignDoors(visit.AccessGroupId, search, operationType, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal, out var recordsFiltered);
            var pagingData = new PagingData<AccessGroupDeviceAssignDoor>
            {
                Data = assignDoors,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            
            return Ok(pagingData);
        }

        /// <summary>
        /// Get visitor by qrcode
        /// </summary>
        /// <param name="qrCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiVisitsQrCode)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult GetVisitorByQrCode(string qrCode)
        {
            if (string.IsNullOrEmpty(qrCode))
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, VisitResource.msgNotVisitCard);
            }

            var visitData = _visitService.GetVisitByQrCodeEncrypt(qrCode, _httpContext.User.GetCompanyId());
            if (visitData == null)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, VisitResource.msgNotVisitCard);
            }

            return Ok(visitData);
        }

        /// <summary>
        /// Get visitors by cardIds
        /// </summary>
        /// <param name="cardIds"> A list of cardId. </param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiVisitsByCardIds)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult GetVisitorsByCardIds(List<string> cardIds)
        {
            if (cardIds == null || !cardIds.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.CardIdBlank);
            }

            var visitorData = _visitService.GetVisitorsByCardIds(cardIds);
            if (visitorData == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            return Ok(visitorData);
        }



        /// <summary>
        /// Get card information by visitId
        /// </summary>
        /// <param name="ids"> a list of visitor identifier </param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiVisitCardById)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.VisitManagement)]
        public IActionResult GetVisitorCardByIds(List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.VisitIdBlank);
            }

            var visitorData = _visitService.GetVisitorCardByVisitIds(ids);
            if (visitorData == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            return Ok(visitorData);
        }

        /// <summary>
        /// Update layout form visit register
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiRegisterVisitSetting)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.VisitSetting)]
        public IActionResult UpdateSettingLayoutRegister([FromBody] FieldLayoutRegister model)
        {
            _visitService.UpdateFieldLayoutRegister(model, _httpContext.User.GetCompanyId());
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSuccess);
        }

        /// <summary>
        /// Anonymous get visitor by phone (page register visit)
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiAnonymousGetVisitByPhone)]
        [AllowAnonymous]
        public IActionResult AnonymousGetVisitorByPhone(string companyCode, string phone)
        {
            var company = _companyService.GetByCode(companyCode);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundCompany);
            }
            var setting = _visitService.GetVisitSettingByCompanyId(company.Id);
            if (!setting.AllowGetUserTarget)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgIsAllowUserTarget);
            }

            var visit = _visitService.GetByPhoneAndCompanyId(phone, company.Id);
            if (visit == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundVisit);
            }

            var model = _mapper.Map<VisitDataModel>(visit);
            model.BirthDay = visit.BirthDay.ToString(Constants.DateTimeFormatDefault);

            return Ok(model);
        }

        /// <summary>
        /// Anonymous get departments target
        /// </summary>
        /// <param name="companyCode">code of company</param>
        /// <param name="name"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiVisitsDepartmentsTarget)]
        [AllowAnonymous]
        public IActionResult AnonymousGetDepartmentTarget(string companyCode, string name, int pageNumber = 1, int pageSize = 10)
        {
            var company = _companyService.GetByCode(companyCode);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundCompany);
            }
            var setting = _visitService.GetVisitSettingByCompanyId(company.Id);
            if (!setting.AllowGetUserTarget)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgIsAllowUserTarget);
            }

            var departments = _departmentService.GetByCompanyId(company.Id);
            int totalRecords = departments.Count();
            var data = departments
                .Where(m => string.IsNullOrEmpty(name) || m.DepartName.ToLower().Contains(name.Trim().ToLower()))
                .Select(m => new EnumModel()
                {
                    Id = m.Id,
                    Name = m.DepartName,
                })
                .OrderBy(m => m.Name).ToList();
            int recordsFiltered = data.Count();
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return Ok(new PagingData<EnumModel>()
            {
                Data = data,
                Meta = new Meta()
                {
                    RecordsTotal = totalRecords,
                    RecordsFiltered = recordsFiltered,
                }
            });
        }

        /// <summary>
        /// authentication and get setting app register visit
        /// </summary>
        /// <param name="loginModel">Enter account/password</param>
        /// <param name="companyCode">Company Code (using for account has multi company account)</param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiRegisterVisitAuthentication)]
        [AllowAnonymous]
        public IActionResult RegisterVisitAuthentication([FromBody] LoginModel loginModel, string companyCode)
        {
            if (loginModel == null || string.IsNullOrWhiteSpace(loginModel.Username) || string.IsNullOrWhiteSpace(loginModel.Password))
            {
                return new ApiErrorResult((int)LoginUnauthorized.InvalidCredentials, MessageResource.InvalidCredentials);
            }
            
            //Get the account in the system
            var account = _accountService.GetAuthenticatedAccount(loginModel);
            if (account == null)
            {
                return new ApiErrorResult((int)LoginUnauthorized.InvalidCredentials, MessageResource.InvalidCredentials);
            }
            
            //Check where if the account is valid status
            if (account.IsDeleted)
            {
                return new ApiErrorResult((int)LoginUnauthorized.InvalidCredentials, MessageResource.InvalidCredentials);
            }
            
            List<Company> companies = _accountService.GetCompanyList(account);
            Company company = null;

            if (!string.IsNullOrWhiteSpace(companyCode) && companies.Count > 1)
            {
                company = companies.FirstOrDefault(m => m.Code == companyCode);
            }
            else
            {
                company = companies.FirstOrDefault();
            }

            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundCompany);
            }
            
            string targetDepartment = "";
            string targetName = "";

            var user = _userService.GetUserByAccountId(account.Id, company.Id);
            if (user != null)
            {
                targetName = user.FirstName;

                var department = _departmentService.GetById(user.DepartmentId);
                if (department != null)
                    targetDepartment = department.DepartName;
            }

            var visitSetting = _visitService.GetVisitSettingByCompanyId(company.Id);
            var model = _visitService.GetInitVisitForm(company.Id);

            return Ok(new VisitTargetRegister()
            {
                TargetId = account.Id,
                TargetDepartment = targetDepartment,
                TargetName = targetName,
                SiteKey = visitSetting.EnableCaptCha ? _configuration.GetSection(Constants.Settings.ReCaptChaSiteKey).Value : null,
                Logo = model.Logo,
                Language = model.Language,
                CompanyName = company.Name,
                CompanyCode = company.Code,
                AllowGetUserTarget = visitSetting.AllowGetUserTarget,
                FieldRequired = string.IsNullOrWhiteSpace(visitSetting.FieldRequired) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(visitSetting.FieldRequired),
            });
        }
        
        /// <summary>
        /// Get file template import visit
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiGetVisitorTemplate)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.VisitManagement)]
        public IActionResult GetFileImportVisitTemplate(string type = "excel")
        {
            int companyId = _httpContext.User.GetCompanyId();
            byte[] data;
            switch (type)
            {
                default:
                    data = _visitService.GetFileExcelImportVisitTemplate(companyId);
                    break;
            }
            
            return File(data, type.Equals("excel") ? "application/ms-excel" : "application/csv", 
                type.Equals("excel") ? "Visit_Template.xlsx" : "VisitTemplate.csv");
        }
        
        /// <summary>
        /// Import visitors data
        /// </summary>
        /// <param name="file">file include list of visitors to import</param>
        /// <param name="type">type of file</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Data of file imported wrong. Or extension file wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiVisitorImport)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.VisitManagement)]
        public async Task<IActionResult> ImportVisitor(IFormFile file, string type = "excel")
        {
            if (file.Length == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgFailLengthFile);
            }
            var fileType = FileHelpers.GetFileExtension(file);
            List<string> extensions = new List<string>();

            switch (type.ToLower())
            {
                case "excel":
                    extensions.Add(".xls");
                    extensions.Add(".xlsx");
                    break;
                default:
                    extensions.Add(".txt");
                    extensions.Add(".csv");
                    break;
            }

            if (!extensions.Contains(fileType.ToLower()))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    string.Format(MessageResource.msgErrorFileExtension, string.Join(" or ", extensions)));
            }
            try
            {
                // Validate file headers before processing
                var headerValidation = _visitService.ValidateImportFileHeaders(file);
                if (!headerValidation.Result)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, headerValidation.Message);
                }

                var stream = FileHelpers.ConvertToStream(file);
                var companyId = _httpContext.User.GetCompanyId();
                var accountId = _httpContext.User.GetAccountId();
                var accountName = _httpContext.User.GetUsername();

                var result = await _visitService.ImportFile(type, stream, companyId, accountId, accountName);
                if (!result.Result)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, result.Message);
                }
                return new ApiErrorResult(StatusCodes.Status200OK, result.Message);
            }
            catch (Exception e)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, "ERROR to visitor import.");
            }
        }

        /// <summary>
        /// Import multi visitors
        /// </summary>
        /// <param name="code"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiVisitsImportMulti)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.VisitManagement)]
        public IActionResult ImportMultiVisitors(string code, [FromBody]List<ImportMultiVisitModel> models)
        {
            var company = _companyService.GetByCode(code);
            if (company == null || models == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);
            
            if (models.Count > 100)
                return new ApiErrorResult(StatusCodes.Status400BadRequest);
            
            Dictionary<bool, List<string>> result = _visitService.ImportMultiVisitors(models, company.Id);
            if (result[false].Count > 0)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Join(", ", result[false]));
            
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSuccess);
        }
    }
}
