using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.EventLog;
using DeMasterProCloud.Service;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using AutoMapper;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Device;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Diagnostics;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using Newtonsoft.Json;
using DeMasterProCloud.DataModel.Vehicle;
using DeMasterProCloud.Service.Protocol;
using DeMasterProCloud.DataModel.Header;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// EventLog controller
    /// </summary>
    [Produces("application/json")]
    [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EventLogController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IEventLogService _eventLogService;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly IConverter _converter;
        private readonly IDeviceService _deviceService;
        private readonly IVehicleService _vehicleService;
        private readonly ICompanyService _companyService;
        private readonly HttpContext _httpContext;
        private readonly IMapper _mapper;

        private readonly string[] _headers = {
            "Request",
            "Number of event",
            "Time receiving request (t1)",
            "Time finishing request (t2)",
            "Processing time in seconds (t2-t1)"
        };

        /// <summary>
        /// Report controller
        /// </summary>
        /// <param name="eventLogService"></param>
        /// <param name="userService"></param>
        /// <param name="accountService"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="converter"></param>
        /// <param name="deviceService"> service of device </param>
        /// <param name="vehicleService"> service of vehicle </param>
        /// <param name="companyService"> service of company </param>
        /// <param name="mapper"> service of vehicle </param>
        public EventLogController(IEventLogService eventLogService, IUserService userService, IAccountService accountService,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IConverter converter,
            IDeviceService deviceService, IVehicleService vehicleService, ICompanyService companyService, IMapper mapper)
        {
            _eventLogService = eventLogService;
            _userService = userService;
            _configuration = configuration;
            _httpContext = httpContextAccessor.HttpContext;
            _converter = converter;
            _deviceService = deviceService;
            _accountService = accountService;
            _vehicleService = vehicleService;
            _companyService = companyService;
            _mapper = mapper;
        }

        /// <summary>
        /// Init data for function event log
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsInit)]
        public IActionResult EventLogInit()
        {
            var model = _eventLogService.InitData();
            return Ok(model);
        }

        /// <summary>
        /// Init data for function event log report
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsReportInit)]
        public IActionResult EventLogReportInit()
        {
            var model = _eventLogService.InitReportData();
            return Ok(model);
        }

        /// <summary>
        /// This is an API that can inquire access events.
        /// Data is output in order of progress number, access time, name, date of birth, user code, department, card number, RID, door name, place (building name), entry/exit, and event type.
        /// This function is used in Monitoring page.
        /// Filter by time model(AccessDateFrom - AccessDateTo, AccessTimeFrom - AccessTimeTo)
        /// </summary>
        /// <param name="from">This parameter set string of access date time start</param>
        /// <param name="to">This parameter set string of access date time end</param>
        /// <param name="personType">Person type</param>
        /// <param name="eventType">[Integer (1~44)]This parameter can be filtered when searching for specific events. ex) General access: 1 (Refer to the manual for event type definition.)</param>
        /// <param name="userName">[String]This parameter allows you to filter the username.</param>
        /// <param name="inOutType">[Integer (1, 2)This parameter can filter the In/Out type of the door. ex) In : 1, Out : 2</param>
        /// <param name="cardId">[String]This parameter allows you to filter the card ID.</param>
        /// <param name="doorIds">[Integer]This parameter can filter the ID of the door.</param>
        /// <param name="buildingIds">[String]This parameter allows you to filter the building ids.</param>
        /// <param name="departmentIds">[String]This parameter can filter the department ids.</param>
        /// <param name="verifyModeIds">[Integer (0~3)]This parameter allows you to filter the authentication mode.</param>
        /// <param name="objectType">[Integer (0~2)]This parameter allows you to filter the object type of event.</param>
        /// <param name="company">Company Id</param>
        /// <param name="isValid"> Only valid user of only invalid user of all user </param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string of the column</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogs)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Monitoring)]
        public IActionResult EventLogs(string from, string to, List<int> personType, List<int> eventType, string search,
            List<string> inOutType, string cardId, List<int> doorIds, List<int> buildingIds, List<int> departmentIds, List<int> verifyModeIds, List<int> objectType, 
            int? company, List<int> isValid = null, int pageNumber = 1, int pageSize = 10, string sortColumn = "EventTime", string sortDirection = "desc")
        {
            var startTime = DateTime.Now;
            EventLogFilterModel filter = new EventLogFilterModel()
            {
                From = from.ConvertDefaultStringToDateTime(),
                To = to.ConvertDefaultStringToDateTime(),
                EventTypes = eventType,
                PersonTypes = personType,
                Search = search,
                InOutTypes = inOutType,
                CardId = cardId,
                DoorIds = doorIds,
                BuildingIds = buildingIds,
                DepartmentIds = departmentIds,
                VerifyModeIds = verifyModeIds,
                ObjectType = objectType,
                CompanyId = company ?? _httpContext.User.GetCompanyId(),
                IsValid = isValid,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = SortColumnMapping.EventLogReportColumn(sortColumn),
                SortDirection = sortDirection,
                AccountId = _httpContext.User.GetAccountId(),
                AccountType = _httpContext.User.GetAccountType(),
            };
            var eventLogs = _eventLogService.GetPaginated(filter, out var recordsTotal,  out var recordsFiltered);
            var endTime = DateTime.Now;
            WritePerformanceCsvData(pageSize, startTime, endTime);

            List<HeaderData> monitoringHeaderData = _eventLogService.GetEventHeaderSettings(companyId: _httpContext.User.GetCompanyId(),
                                                                                            accountId: _httpContext.User.GetAccountId(),
                                                                                            pageName: Page.Monitoring);

            var pagingData = new PagingDataWithHeader<EventLogListModel>
            {
                Data = eventLogs,
                Header = monitoringHeaderData,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Write performance csv data
        /// </summary>
        /// <param name="numberOfEvent"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        [NonAction]
        private void WritePerformanceCsvData(int numberOfEvent, DateTime startTime, DateTime endTime)
        {
            var enableLog = Convert.ToBoolean(_configuration[Constants.Settings.EnablePerformanceLog]);
            if (enableLog)
            {
                var csvFile = $"GetEventLog_{DateTime.Now.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.csv";
                var contents = new[]
                {
                    _httpContext.Request.GetEncodedUrl(),
                    numberOfEvent.ToString(),
                    startTime.ToString(Constants.DateTimeFormat.YyyyMmDdHhMmSs),
                    endTime.ToString(Constants.DateTimeFormat.YyyyMmDdHhMmSs),
                    endTime.Subtract(startTime).TotalSeconds.ToString(CultureInfo.InvariantCulture)
                };
                DeMasterProCloud.Service.Csv.CsvHelper.Write(_headers, new List<string[]> { contents }, csvFile);

            }
        }

        /// <summary>
        /// Export event log to file. Filter by time model(AccessDateFrom - AccessDateTo, AccessTimeFrom - AccessTimeTo)
        /// </summary>
        /// <param name="from">This parameter set string of access date start</param>
        /// <param name="to">This parameter set string of access date end</param>
        /// <param name="eventType">[Integer (1~44)]This parameter can be filtered when searching for specific events. ex) General access: 1 (Refer to the manual for event type definition.)</param>
        /// <param name="userName">[String]This parameter allows you to filter the username.</param>
        /// <param name="workType"></param>
        /// <param name="inOutType">[Integer (1, 2)This parameter can filter the In/Out type of the door. ex) In : 1, Out : 2</param>
        /// <param name="cardId">[String]This parameter allows you to filter the card ID.</param>
        /// <param name="search">[String]This parameter allows you to filter search all.</param>
        /// <param name="doorIds">[Integer]This parameter can filter the ID of the door.</param>
        /// <param name="buildingIds">[String]This parameter allows you to filter the building ids.</param>
        /// <param name="departmentIds">[String]This parameter can filter the department ids.</param>
        /// <param name="cardType">List of card types</param>
        /// <param name="verifyModeIds">[Integer (0~3)]This parameter allows you to filter the authentication mode.</param>
        /// <param name="objectType">[Integer (0~2)]This parameter allows you to filter the object type of event.</param>
        /// <param name="company">Company Id</param>
        /// <param name="isValid"> only valid user or only invalid user or all user </param>
        /// <param name="type">Type of file to export</param>
        /// <param name="sortColumn">Sort Column by string of the column</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Unprocessable Entity: Time model filter wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsExport)]
        public IActionResult Export(string from, string to, List<int> eventType, string userName,
            List<int> workType, List<int> inOutType, string cardId, string search, List<int> doorIds, List<int> buildingIds, List<int> departmentIds, List<int> cardType,
            List<int> verifyModeIds, List<int> objectType, int? company, List<int> isValid = null, string type = "excel", string sortColumn = null, string sortDirection = "desc")
        {
            var dateTimeFrom = from.ConvertDefaultStringToDateTime() ?? DateTime.MinValue;
            var dateTimeTo = to.ConvertDefaultStringToDateTime() ?? DateTime.MaxValue;
            
            var fileData = _eventLogService.Export(type, sortColumn, sortDirection, out _, out _
                , dateTimeFrom, dateTimeTo, eventType, userName, workType, inOutType, cardId, search, doorIds, buildingIds, cardType, departmentIds, verifyModeIds, objectType,
                company, isValid);
            var filename = string.Format(Constants.ExportFileFormat, EventLogResource.lblEventLog, DateTime.Now);
            var fullName = type == "excel" ? $"{filename}.xlsx" : $"{filename}.txt";


            _eventLogService.SaveSystemLogExport(fullName);

            if (type != null && type.Equals("excel"))
                return File(fileData, "application/ms-excel", fullName);
            return File(fileData, "application/text", fullName);

        }

        /// <summary>
        /// Export 2 - Export file report event log. Filter by time model(AccessDateFrom - AccessDateTo, AccessTimeFrom - AccessTimeTo)
        /// </summary>
        /// <param name="eventType">[Integer (1~44)]This parameter can be filtered when searching for specific events. ex) General access: 1 (Refer to the manual for event type definition.)</param>
        /// <param name="userName">[String]This parameter allows you to filter the username.</param>
        /// <param name="inOutType">[Integer (1, 2)This parameter can filter the In/Out type of the door. ex) In : 1, Out : 2</param>
        /// <param name="cardId">[String]This parameter allows you to filter the card ID.</param>
        /// <param name="search">[String]This parameter allows you to filter the search all</param>
        /// <param name="doorIds">[Integer]This parameter can filter the ID of the door.</param>
        /// <param name="buildingList">[String]This parameter allows you to filter the building name.</param>
        /// <param name="departmentIds">This parameter can filter the department ids.</param>
        /// <param name="objectType">[Integer (0~2)]This parameter allows you to filter the object type of event.</param>
        /// <param name="workType"></param>
        /// <param name="cardType">Type of card</param>
        /// <param name="company">Company Id</param>
        /// <param name="culture"> localization code </param>
        /// <param name="type">Type of file to export</param>
        /// <param name="isValid"> Only valid user or only invalid user or all user </param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Unprocessable Entity: Time model filter wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsReportExport)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Export + Page.Report)]
        public IActionResult ExportReport(string from, string to, List<int> eventType, string userName,
           List<int> inOutType, string cardId, string search, List<int> doorIds, List<int> buildingIds, List<int> departmentIds, List<int> objectType, List<int> workType,
            List<int> cardType, int? company, string culture, string type = "excel", List<int> isValid = null, string sortColumn = null, string sortDirection = "desc")
        {
            string extension;
            string fileMIME;
            int maximumRecordExport;
            int allowMaximumRecordExport;
            var exportSetting = _configuration.GetSection(Constants.Settings.DefineSettingThreadExport).Get<ThreadExportEventSetting>();
            
            switch (type.ToLower())
            {
                case "excel":
                    extension = "xlsx";
                    fileMIME = "application/ms-excel";
                    maximumRecordExport = Constants.ExportFileSetting.MaximumRecordExport;
                    maximumRecordExport = exportSetting?.MaximumRecordExport ?? maximumRecordExport;
                    allowMaximumRecordExport = Constants.ExportFileSetting.AllowMaximumRecordExport;
                    allowMaximumRecordExport = exportSetting?.AllowMaximumRecordExport ?? allowMaximumRecordExport;
                    break;
                case "hancell":
                    extension = "cell";
                    fileMIME = "application/octet-stream";
                    maximumRecordExport = Constants.ExportFileSetting.MaximumRecordHancellExport;
                    maximumRecordExport = (exportSetting != null && exportSetting.MaximumRecordExportHancell != 0) ? exportSetting.MaximumRecordExportHancell : maximumRecordExport;
                    allowMaximumRecordExport = Constants.ExportFileSetting.AllowMaximumRecordHancellExport;
                    allowMaximumRecordExport = (exportSetting != null && exportSetting.AllowMaximumRecordExportHancell != 0) ? exportSetting.AllowMaximumRecordExportHancell : allowMaximumRecordExport;
                    break;
                default:
                    extension = "csv";
                    fileMIME = "text/csv";
                    maximumRecordExport = Constants.ExportFileSetting.MaximumRecordExport;
                    maximumRecordExport = exportSetting?.MaximumRecordExport ?? maximumRecordExport;
                    allowMaximumRecordExport = Constants.ExportFileSetting.AllowMaximumRecordExport;
                    allowMaximumRecordExport = exportSetting?.AllowMaximumRecordExport ?? allowMaximumRecordExport;
                    break;
            }

            var account = _accountService.GetById(_httpContext.User.GetAccountId());
            var accessDateTimeFrom = from.ConvertDefaultStringToDateTime() ?? DateTime.MinValue;
            var accessDateTimeTo = to.ConvertDefaultStringToDateTime() ?? DateTime.MaxValue;

            if (string.IsNullOrWhiteSpace(culture))
            {
                culture = Thread.CurrentThread.CurrentCulture.Name;
            }

            sortColumn = Helpers.CheckPropertyInObject<DataAccess.Models.EventLog>(sortColumn, "EventTime", ColumnDefines.EventLogForReport);
            EventLogExportFilterModel filter = new EventLogExportFilterModel()
            {
                StartDate = accessDateTimeFrom,
                EndDate = accessDateTimeTo,
                UserName = userName,
                CardId = cardId,
                CompanyId = _httpContext.User.GetCompanyId(),
                Culture = culture,
                Type = type,
                EventTypes = eventType,
                WorkType = workType,
                InOutType = inOutType,
                DoorIds = doorIds,
                BuildingIds = buildingIds,
                DepartmentIds = departmentIds,
                ObjectType = objectType,
                CardType = cardType,
                IsValid = isValid,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                ExportAccountId = _httpContext.User.GetAccountId(),
            };

            int count = _eventLogService.GetCountByFilter(filter);
            
            if(count == 0)
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.NoData);

            if (count >= maximumRecordExport)
            {
                if (count > allowMaximumRecordExport)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(EventLogResource.msgLimitAllowExport, allowMaximumRecordExport));
                }
                
                // new thread export
                filter.TotalRecord = count;
                TimeZoneInfo cstZone = account.TimeZone.ToTimeZoneInfo();
                TimeSpan offSet = cstZone.BaseUtcOffset;
                filter.OffSet = offSet;
                
                return new ApiSuccessResult(StatusCodes.Status200OK, EventLogResource.msgYourRequestExecuting);
            }

            var fileData = _eventLogService.ExportReport(type, sortColumn, sortDirection, out _, out _
                , accessDateTimeFrom, accessDateTimeTo,
                eventType, userName, workType, inOutType, cardId, search, doorIds, objectType, buildingIds, departmentIds, cardType,
                company, isValid, culture);

            var filename = string.Format(Constants.ExportFileFormat, EventLogResource.lblEventLog, DateTime.Now);
            string fullName = $"{filename}.{extension}";
            _eventLogService.SaveSystemLogExport(fullName, filter);

            return File(fileData, fileMIME, fullName);
        }


        /// <summary>
        /// View the event log data through pdf file. Filter by time model(AccessDateFrom - AccessDateTo, AccessTimeFrom - AccessTimeTo)
        /// </summary>
        /// <param name="model.AccessDateFrom">This parameter set string of access date start</param>
        /// <param name="model.AccessDateTo">This parameter set string of access date end</param>
        /// <param name="model.AccessTimeFrom">This parameter set string of access time start</param>
        /// <param name="model.AccessTimeTo">This parameter set string of access time end</param>
        /// <param name="model">This parameter sets the time range when searching events.</param>
        /// <param name="eventType">[Integer (1~44)]This parameter can be filtered when searching for specific events. ex) General access: 1 (Refer to the manual for event type definition.)</param>
        /// <param name="userName">[String]This parameter allows you to filter the username.</param>
        /// <param name="inOutType">[Integer (1, 2)This parameter can filter the In/Out type of the door. ex) In : 1, Out : 2</param>
        /// <param name="cardId">[String]This parameter allows you to filter the card ID.</param>
        /// <param name="doorIds">[Integer]This parameter can filter the ID of the door.</param>
        /// <param name="buildingIds">[String]This parameter allows you to filter the building ids.</param>
        /// <param name="departmentIds">[String]This parameter can filter the department ids.</param>
        /// <param name="verifyModeIds">[Integer (0~3)]This parameter allows you to filter the authentication mode.</param>
        /// <param name="objectType">[Integer (0~2)]This parameter allows you to filter the object type of event.</param>
        /// <param name="company">Company Id</param>
        /// <param name="search">search all</param>
        /// <param name="isValid"> Only valid user or only invalid user or all user </param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Unprocessable Entity: Time model filter wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsExportPdf)]
        public IActionResult EventLogPdfView(string from, string to, List<int> eventType, string userName,
            List<int> inOutType, string cardId, string search, List<int> doorIds, List<int> buildingIds, List<int> departmentIds,
            List<int> verifyModeIds,List<int> objectType, int? company, List<int> isValid = null, string sortColumn = null, string sortDirection = "desc")
        {
            var dateTimeFrom = from.ConvertDefaultStringToDateTime() ?? DateTime.MinValue;
            var dateTimeTo = to.ConvertDefaultStringToDateTime() ?? DateTime.MaxValue;
            
            var eventLogs = _eventLogService.FilterDataWithOrder(dateTimeFrom, dateTimeTo, null,
                eventType, userName, inOutType, cardId, search, doorIds, buildingIds, null, departmentIds, verifyModeIds,objectType, null,
                company, isValid, sortColumn, sortDirection, 1, 0, out _, out _).AsEnumerable().ToList();
            //.Select(m =>
            //{
            //    var result = new EventLogListModel
            //    {
            //        //Access Time,User Name,Card Id,Device,Door Name,Card Type,IN/OUT,Event Detail
            //        AccessTime = m.CreatedOn.ToString(Constants.DateTimeFormat.YyyyMmDdHhMmSs),//Access time
            //        UserName = m.EventType != (int)EventType.UnRegisteredID ? m.User?.FirstName : string.Empty,
            //        CardId = m.CardType == (short)CardType.PassCode ? "******" : m.CardId,
            //        Device = m.Icu.DeviceAddress,
            //        DoorName = m.DoorName,
            //        CardType = ((CardType)m.CardType).GetDescription(),
            //        InOut = ((InOut)m.Icu.PassbackRule).GetDescription(),
            //        EventDetail = ((EventType)m.EventType).GetDescription(),
            //    };
            //    return result;
            //}).ToList();

            ViewBag.Message = eventLogs;
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "PDF Report",
            };

            var html = View().ToHtml(_httpContext);
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/css", "styles.css") },
                HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" },
            };

            var pdf = new HtmlToPdfDocument
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf);
            var filename = string.Format(Constants.ExportFileFormat, EventLogResource.lblEventLog, DateTime.Now) + ".pdf";

            _eventLogService.SaveSystemLogExport(filename);

            return File(file, "application/pdf", filename);
        }

        /// <summary>
        /// Get Device List for Event Recovery. Filter by time model(AccessDateFrom - AccessDateTo, AccessTimeFrom - AccessTimeTo)
        /// </summary>
        /// <param name="search">Query string that filter Device by Name, Device Address or Building Name</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns>Json result object</returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsRecoveryDevices)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetDeviceList(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id",
            string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<RecoveryDeviceModel>(sortColumn, "Id", ColumnDefines.Device);
            var doors = _eventLogService.GetPaginatedRecoveryDevices(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                out var recordsFiltered).AsEnumerable().Select(_mapper.Map<RecoveryDeviceModel>).AsQueryable().ToList();

            var pagingData = new PagingData<RecoveryDeviceModel>
            {
                Data = doors,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Update Body-Temperature
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiEventLogIdBodyTemperature)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdateBodyTemperature(int id, [FromBody] BodyTemperatureModel model)
        {
            var eventLog = _eventLogService.GetById(id);
            if (eventLog == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundEventLog);

            eventLog.BodyTemperature = model.BodyTemperature;
            _eventLogService.Update(eventLog);

            string msg = string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString();
            return new ApiSuccessResult(StatusCodes.Status200OK, msg);
        }

        /// <summary>
        /// Get Device List for Event Recovery
        /// </summary>
        /// <param name="deviceIds">List of device ids</param>
        /// <param name="model.AccessDateFrom">This parameter set string of access date start</param>
        /// <param name="model.AccessDateTo">This parameter set string of access date end</param>
        /// <param name="model.AccessTimeFrom">This parameter set string of access time start</param>
        /// <param name="model.AccessTimeTo">This parameter set string of access time end</param>
        /// <param name="model">This parameter sets the time range when searching events.</param>
        /// <param name="search">Query string that filter Device by Name, Device Address or Building Name</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns>Json result object</returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Unprocessable Entity: Time model filter wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsEventInquiry)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult EventCountInquiry(List<int> deviceIds, EventLogAccessTimeModel model, string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id",
            string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<RecoveryDeviceModel>(sortColumn, "Id", ColumnDefines.Device);
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }

            var doors = _eventLogService.GetPaginatedEventRecoveryInquireDevices(deviceIds, model.AccessDateFrom, model.AccessDateTo, model.AccessTimeFrom, model.AccessTimeTo, search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                out var recordsFiltered);

            var pagingData = new PagingData<RecoveryDeviceModel>
            {
                Data = doors,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Event Recovery. Filter by time model(AccessDateFrom - AccessDateTo, AccessTimeFrom - AccessTimeTo)
        /// </summary>
        /// <param name="eventRecoveryProgressModels">JSON model for array object(device id and process id)</param>
        /// <param name="model.AccessDateFrom">This parameter set string of access date start</param>
        /// <param name="model.AccessDateTo">This parameter set string of access date end</param>
        /// <param name="model.AccessTimeFrom">This parameter set string of access time start</param>
        /// <param name="model.AccessTimeTo">This parameter set string of access time end</param>
        /// <param name="model">This parameter sets the time range when searching events.</param>
        /// <returns>Json result object</returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Unprocessable Entity: Event recovery model not null</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiEventLogsRecovery)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult EventRecovery([FromBody] List<EventRecoveryProgressModel> eventRecoveryProgressModels, EventLogAccessTimeModel model)
        {
            if (eventRecoveryProgressModels == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound);
            }
            _eventLogService.EventRecovery(eventRecoveryProgressModels, model.AccessDateFrom, model.AccessDateTo,
                model.AccessTimeFrom, model.AccessTimeTo);
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.msgEventRecoverySuccess);
        }

        /// <summary>
        /// This function is used in Report page. Filter by time model(AccessDateFrom - AccessDateTo, AccessTimeFrom - AccessTimeTo)    
        /// </summary>
        /// <param name="from">This parameter set string of access date time start</param>
        /// <param name="to">This parameter set string of access date time end</param>
        /// <param name="eventType">[Integer (1~44)]This parameter can be filtered when searching for specific events. ex) General access: 1 (Refer to the manual for event type definition.)</param>
        /// <param name="search">[String]This parameter allows you to filter the username.</param>
        /// <param name="departmentIds">This parameter can filter the department ids</param>
        /// <param name="workType"></param>
        /// <param name="inOutType">[Integer (1, 2)This parameter can filter the In/Out type of the door. ex) In : 1, Out : 2</param>
        /// <param name="cardId">[String]This parameter allows you to filter the card ID.</param>
        /// <param name="doorIds">[Integer]This parameter can filter the ID of the door.</param>
        /// <param name="objectType">[Integer (0~2)]This parameter allows you to filter the object type of event.</param>
        /// <param name="buildingList">[String]This parameter allows you to filter the building name.</param>
        /// <param name="cardType">Type of Card</param>
        /// <param name="isValid"> Only valid user of only invalid user or all user </param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="culture">This parameter set culture</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Unprocessable Entity: Time model filter wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsReport)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckMultiPermission(new string[] { ActionName.View + Page.Report, ActionName.ViewAll + Page.Report }, false) ]
        public IActionResult EventReport(string from, string to, List<int> doorIds,List<int> objectType, string search, string cardId,
            List<int> departmentIds, List<int> workType, List<int> inOutType, List<int> eventType, List<int> buildingIds, List<int> cardType, 
            string culture, List<int> isValid = null, int pageNumber = 1, int pageSize = 10, string sortColumn = "EventTime", string sortDirection = "desc")
        {
            var stopWatch = Stopwatch.StartNew();
            var filter = new EventLogFilterModel()
            {
                From = from.ConvertDefaultStringToDateTime(),
                To = to.ConvertDefaultStringToDateTime(),
                EventTypes = eventType,
                Search = search,
                InOutIds = inOutType,
                CardId = cardId,
                DoorIds = doorIds,
                ObjectType = objectType,
                BuildingIds = buildingIds,
                DepartmentIds = departmentIds,
                IsValid = isValid,
                WorkTypeIds = workType,
                CardTypes = cardType,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = SortColumnMapping.EventLogReportColumn(sortColumn),
                SortDirection = sortDirection,
                CompanyId = _httpContext.User.GetCompanyId(),
                AccountId = _httpContext.User.GetAccountId(),
                AccountType = _httpContext.User.GetAccountType(),
            };
            
            var eventLogs = _eventLogService.GetPaginatedEventLogReport(filter, out var recordsTotal, out var recordsFiltered);
            List<HeaderData> reportHeaderData = _eventLogService.GetEventHeaderSettings(companyId: _httpContext.User.GetCompanyId(),
                                                                                            accountId: _httpContext.User.GetAccountId(),
                                                                                            pageName: Page.Report);

            var pagingData = new PagingDataWithHeader<EventLogReportListModel>
            {
                Data = eventLogs,
                Header = reportHeaderData,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            
            stopWatch.Stop();
            Trace.WriteLine($"Elapsed time {stopWatch.ElapsedMilliseconds} ms");
            Trace.WriteLine($"Search {recordsTotal} event(s) successfully in {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds).TotalSeconds} seconds!");
            Trace.WriteLine($"PageNumber : {pageNumber}, PageSize : {pageSize}");
            
            return Ok(pagingData);
        }

        /// <summary>
        /// Get all files backup with company
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogListFileBackup)]
        [Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Export + Page.Report)]
        public IActionResult GetAllFileBackup(bool self = false)
        {
            var listFiles = _eventLogService.GetListFileBackupByCompany(_httpContext.User.GetCompanyId(), _httpContext.User.GetUsername(), self);
            return Ok(listFiles);
        }

        /// <summary>
        /// Upload image plate number
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiEventLogImages)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UploadImagePlateNumber(IFormFile file)
        {
            if (file.Length == 0)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, "File Error (0 byte)");
            }
            
            string messageError = _eventLogService.SaveImageToEventLog(file);
            if (string.IsNullOrEmpty(messageError))
                return Ok();

            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, messageError);
        }

        /// <summary>
        /// Upload video plate number
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiEventLogVideos)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UploadVideoPlateNumber(IFormFile file)
        {
            if (file.Length == 0)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, "File Error (0 byte)");
            }
        
            string messageError = _eventLogService.SaveVideoToEventLog(file);
            if (string.IsNullOrEmpty(messageError))
                return Ok();

            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, messageError);
        }
        
        /// <summary>
        /// Upload record video
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="startTime"></param>
        /// <param name="hash"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(Constants.Route.ApiEventLogRecordVideos)]
        public IActionResult UploadVideoEventLog(string rid, string startTime, string hash, IFormFile file)
        {
            if (file.Length == 0)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, "File Error (0 byte)");
            }
            var device = _deviceService.GetByDeviceAddress(rid);
            if (device == null || !device.CompanyId.HasValue)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);

            var company = _companyService.GetById(device.CompanyId.Value);
            if (company == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);

            if (string.IsNullOrEmpty(hash) || !CryptographyHelper.VerifyMD5Hash(startTime + rid + company.Code, hash))
                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.Unauthorized);
            
            string messageError = _eventLogService.SaveRecordVideoToEventLog(file);
            if (string.IsNullOrEmpty(messageError))
                return Ok();

            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, messageError);
        }
        
        /// <summary>
        /// Get access statistics
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="inOutType"></param>
        /// <param name="buildingIds"></param>
        /// <param name="type">"person" or "vehicle"</param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiEventLogAccessStatistics)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetAccessStatistics(DateTime from, DateTime to, List<int> inOutType, List<int> buildingIds, string type="person")
        {
            List<int> eventTypes = new List<int>
            {
                (short)EventType.NormalAccess,
                (short)EventType.NormalVehicle,
                (short)EventType.NormalAccessMpr
            };

            var account = _accountService.GetById(_httpContext.User.GetAccountId());
            if (account != null)
            {
                from = from.ConvertToSystemTime(account.TimeZone);
                to = to.ConvertToSystemTime(account.TimeZone);
            }
            
            if(type == "vehicle")
            {
                var data = _eventLogService.GetAccessStatisticsVehicle(from, to, inOutType, buildingIds, eventTypes, _httpContext.User.GetCompanyId());
                return Ok(data);
            }
            else
            {
                var data = _eventLogService.GetAccessStatisticsPerson(from, to, inOutType, buildingIds, eventTypes, _httpContext.User.GetCompanyId());
                return Ok(data);
            }
        }

        /// <summary>
        /// Open API-User event inquiry
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>return Event log for 1 user</returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsOpenReport)]
        public IActionResult OpenAPIEventReport(int id)
        {
            var eventLogs = _eventLogService.GetPaginatedOpenEventLogReport(id,
                    out var recordsTotal, out var recordsFiltered).ToList()
                .Select(m =>
                {
                    var result = new EventLogReportListModel
                    {
                        //Id = Guid.NewGuid(),
                        AccessTime = m.EventTime.ConvertDefaultDateTimeToString(),
                        EventTime = m.EventTime.ConvertDefaultDateTimeToString(),
                        UserName = m.User != null ? m.User.FirstName + " " + m.User.LastName : m.UserName ?? String.Empty,
                        BirthDay = m.IsVisit ? m.Visit?.BirthDay.ToSettingDateString() : m.User?.BirthDay.ToSettingDateString(),
                        EmployeeNumber = m.IsVisit ? m.Visit?.VisitorEmpNumber : m.User?.EmpNumber,
                        Department = m.IsVisit ? m.Visit?.VisitorDepartment : m.User?.Department?.DepartName,
                        DepartmentName = m.IsVisit ? m.Visit?.VisitorDepartment : m.User?.Department?.DepartName,
                        CardId = m.CardId,

                        DeviceAddress = m.Icu?.DeviceAddress,
                        DoorName = m.DoorName,
                        Building = m.Icu?.Building.Name,
                        //InOut = Constants.AntiPass.Contains(m.Antipass)
                        //    ? ((Antipass)Enum.Parse(typeof(Antipass), m.Antipass)).GetDescription()
                        //    : string.Empty,
                        InOut = Constants.AntiPass.ToList().FindIndex(x => x.Equals(m.Antipass, StringComparison.OrdinalIgnoreCase)) != -1
                              ? ((Antipass)Enum.Parse(typeof(Antipass), m.Antipass, true)).GetDescription()
                              : "",
                        IcuId = m.Icu?.Id,

                        UserId = m.User?.Id,
                        VisitId = m.Visit?.Id,
                        EventLogId = m.Id,
                        EventDetail = ((EventType)m.EventType).GetDescription(),
                        IssueCount = m.IssueCount,
                        CardStatus = ((CardStatus)_eventLogService.GetCardStatus(m.CardId)).GetDescription(),
                        CardType = ((PassType)m.CardType).GetDescription(),
                    };

                    return result;
                }).ToList();



            var idx = 1;
            foreach (var eventLog in eventLogs)
            {
                eventLog.Id = idx++;
            }

            var pagingData = new PagingData<EventLogReportListModel>
            {
                Data = eventLogs,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }


        /// <summary>
        /// Generate test data
        /// </summary>
        /// <param name="numberOfevent"> number of events</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route("/event-logs/create-test-data")]
        [Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CreateTestData(int numberOfevent)
        {
            if (numberOfevent == 0)
            {
                return Ok(new { message = "No user data is created!" });
            }

            var stopWatch = Stopwatch.StartNew();
            _eventLogService.GenerateTestData(numberOfevent);
            stopWatch.Stop();
            Trace.WriteLine($"Elapsed time {stopWatch.ElapsedMilliseconds} ms");
            return Ok(new
            {
                message =
                    $"{numberOfevent} user(s) data were created successfully in {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds).TotalSeconds} seconds!"
            });
        }


        //=========================================================================================================================================//
        // For Duali Korea

        /// <summary>
        /// Get event log for attendence
        /// </summary>
        /// <param name="AccessDateFrom"> start date </param>
        /// <param name="AccessDateTo"> end date </param>
        /// <param name="userIds"> list of identifier of user </param>
        /// <param name="departmentIds"> list of identifier of department </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAttendance)]
        public IActionResult GetEventLogForAttendance(string AccessDateFrom, string AccessDateTo, List<int> userIds, List<int> departmentIds)
        {
            var eventLogs = _eventLogService.GetAttendenceForDuali(AccessDateFrom, AccessDateTo,
                   userIds, departmentIds).ToList()
                .Select(m =>
                {
                    var result = new EventLogListModel
                    {
                        AccessTime = m.EventTime.ConvertDefaultDateTimeToString(),
                        EventTime = m.EventTime.ConvertDefaultDateTimeToString(),
                        UserName = m.UserName,
                        Department = m.User?.Department.DepartName,
                        DepartmentName = m.User?.Department.DepartName,
                        UserId = m.User?.Id,
                    };

                    return result;
                }).ToList();

            var pagingData = new PagingData<EventLogListModel>
            {
                Data = eventLogs,
            };

            return Ok(pagingData);
        }




        /// <summary>
        /// Get event log for ADD data sync
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiADDEventLog)]
        public IActionResult GetEventLogNotUploaded(int size = 100)
        {
            string timezone = _accountService.GetById(_httpContext.User.GetAccountId()).TimeZone;
            var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;

            var eventLogs = _eventLogService.GetEventLogForADD(size).ToList()
                .Select(m =>
                {
                    var result = new EventLogListModel
                    {
                        Id = m.Id,
                        AccessTime = m.EventTime.ConvertToUserTime(offSet).ToString("yyyyMMddHHmmss"),
                        EventTime = m.EventTime.ConvertToUserTime(offSet).ToString("yyyyMMddHHmmss"),
                        Device = m.Icu.DeviceAddress,
                        DeviceAddress = m.Icu.DeviceAddress,
                        EventType = (short)m.EventType,
                        CardId = m.CardType != (short)CardType.NFC ? m.User?.Card.FirstOrDefault(x => x.CardType == (short)CardType.NFC) != null ? m.User?.Card.FirstOrDefault(x => x.CardType == (short)CardType.NFC).CardId : m.CardId : m.CardId,
                        IssueCount = m.IssueCount,
                        CardType = m.CardType.ToString(),
                        UserName = m.UserName,
                        Department = m.User?.Department.DepartNo,
                        DepartmentName = m.User?.Department.DepartNo,
                        UserId = m.User?.Id,
                    };

                    return result;
                }).ToList();

            var pagingData = new PagingData<EventLogListModel>
            {
                Data = eventLogs,
            };

            return Ok(pagingData);
        }


        /// <summary>
        /// Edit event log for ADD data sync (index : 0 to 1)
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiADDEventLog)]
        public IActionResult EditEventLogNotUploaded([FromBody] List<int> ids)
        {
            _eventLogService.EditEventLogForADD(ids);

            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, EventLogResource.lblEventLog, ""));
        }




        /////////////// vehicle ///////////////
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVehicleEventLogReport)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Monitoring)]
        public IActionResult GetVehicleEventLogsReport(string from, string to, List<int> eventType, string userName, string plateNumber, string search, List<int> vehicleClassificationIds,
            List<int> inOutType, List<int> doorIds, List<int> buildingIds, List<int> departmentIds, List<int> verifyModeIds,List<int> objectType, int? company, List<int> isValid = null, int pageNumber = 1,
            int pageSize = 10, string sortColumn = "EventTime", string sortDirection = "desc")
        {
            var eventLogs = _eventLogService.GetPaginatedVehicle(
                    from: from.ConvertDefaultStringToDateTime() ?? DateTime.MinValue,
                    to: to.ConvertDefaultStringToDateTime() ?? DateTime.MaxValue,
                    eventType: eventType,
                    userName: userName,
                    plateNumber: plateNumber,
                    search: search,
                    inOutType: inOutType,
                    doorIds: doorIds,
                    buildingIds: buildingIds,
                    departmentIds: departmentIds,
                    verifyModeIds: verifyModeIds,
                    objectType: objectType,
                    vehicleClassificationIds: vehicleClassificationIds,
                    company: company,
                    isValid: isValid,
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    sortColumn: sortColumn,
                    sortDirection: sortDirection,
                    totalRecords: out var recordsTotal,
                    recordsFiltered: out var recordsFiltered).ToList();

            List<HeaderData> headerData = _eventLogService.GetVehicleEventLogHeaderData();

            var pagingData = new PagingData<VehicleEventLogListModel, HeaderData>
            {
                Data = eventLogs,
                Header = headerData,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }


        /// <summary>
        /// Get initial data about vehicle report data.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVehicleEventLogReportInit)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Monitoring)]
        public IActionResult InitVehicleEventLogReportFilter()
        {
            var model = _eventLogService.InitVehicleReportData();

            return Ok(model);
        }


        /// <summary>
        /// Export - Export file report vehicle event log. Filter by time model(AccessDateFrom - AccessDateTo, AccessTimeFrom - AccessTimeTo)
        /// </summary>
        /// <param name="from">This parameter set string of access date start</param>
        /// <param name="to">This parameter set string of access date end</param>
        /// <param name="eventType">[Integer (1~44)]This parameter can be filtered when searching for specific events. ex) General access: 1 (Refer to the manual for event type definition.)</param>
        /// <param name="userName">[String]This parameter allows you to filter the username.</param>
        /// <param name="plateNumber"></param>
        /// <param name="search"></param>
        /// <param name="inOutType">[Integer (1, 2)This parameter can filter the In/Out type of the door. ex) In : 1, Out : 2</param>
        /// <param name="doorIds">[Integer]This parameter can filter the ID of the door.</param>
        /// <param name="objectType">[Integer (0~2)]This parameter allows you to filter the object type of event.</param>
        /// <param name="buildingIds">[String]This parameter allows you to filter the building ids.</param>
        /// <param name="departmentIds">[String]This parameter can filter the department ids.</param>
        /// <param name="vehicleClassificationIds"></param>
        /// <param name="cardType">Type of card</param>
        /// <param name="company">Company Id</param>
        /// <param name="culture"> localization code </param>
        /// <param name="type">Type of file to export</param>
        /// <param name="isValid"> Only valid user or only invalid user or all user </param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Unprocessable Entity: Time model filter wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiVehicleEventLogExport)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Export + Page.Report)]
        public IActionResult ExportVehicleReport(string from, string to, List<int> eventType, string userName, string plateNumber, string search,
           List<int> inOutType, List<int> doorIds,List<int> objectType, List<int> buildingIds, List<int> departmentIds, List<int> vehicleClassificationIds,
            List<int> cardType, int? company, string culture, string type = "excel", List<int> isValid = null, string sortColumn = null, string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<DataAccess.Models.EventLog>(sortColumn, "EventTime", ColumnDefines.EventLogForReport);

            int dayExport = _configuration.GetSection(Constants.Settings.DefineLimitRangeDayExport).Get<int>();
            dayExport = dayExport == 0 ? 90 : dayExport;

            if (!string.IsNullOrWhiteSpace(from) && !string.IsNullOrWhiteSpace(to))
            {
                DateTime start = DateTime.Parse(from);
                DateTime end = DateTime.Parse(to);
                if ((end - start).Days > dayExport)
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(EventLogResource.msgErrorLimitRangeDayExport, dayExport));
                }
            }

            var eventLogs = _eventLogService.GetPaginatedVehicle(
                    from: from.ConvertDefaultStringToDateTime() ?? DateTime.MinValue,
                    to: to.ConvertDefaultStringToDateTime() ?? DateTime.MaxValue,
                    eventType: eventType,
                    userName: userName,
                    plateNumber: plateNumber,
                    search: search,
                    inOutType: inOutType.Select(m => m).ToList(),
                    doorIds: doorIds,
                    buildingIds: buildingIds,
                    departmentIds: departmentIds,
                    verifyModeIds: null,
                    objectType: objectType,
                    vehicleClassificationIds: vehicleClassificationIds,
                    company: company,
                    isValid: isValid,
                    pageNumber: 1,
                    pageSize: 0,
                    sortColumn: sortColumn,
                    sortDirection: sortDirection,
                    totalRecords: out var recordsTotal,
                    recordsFiltered: out var recordsFiltered).ToList();

            type = type.ToLower();
            type = "csv";

            var fileData = _eventLogService.ExportVehicleReport(type, eventLogs, culture);

            var filename = string.Format(Constants.ExportFileFormat, EventLogResource.lblVehicleEventLog, DateTime.Now);
            var fullName = type == "excel" ? $"{filename}.xlsx" : $"{filename}.csv";

            _eventLogService.SaveSystemLogExport(fullName);

            if (type != null && type.Equals("excel"))
                return File(fileData, "application/ms-excel", fullName);

            return File(fileData, "text/csv", fullName);

        }
        
        /// <summary>
        /// Get event log and init page monitoring by token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="eventType"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiEventLogMonitoring)]
        [AllowAnonymous]
        public IActionResult GetEventByTokenScreenMonitoring(string token, List<int> eventType, 
            int pageNumber = 1, int pageSize = 10, string sortColumn = "EventTime", string sortDirection = "desc")
        {
            var data = _deviceService.GetDataByTokenMonitoring(token);
            if (data == null)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.InvalidToken);
            }

            var filter = new EventLogFilterModel()
            {
                DataTokenMonitoring = data,
                EventTypes = eventType,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
            };

            return Ok(_eventLogService.GetInitByDataTokenMonitoring(filter));
        }

        /// <summary>
        /// Get event log and init page monitoring by token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="eventType"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiEventLogMonitoringToSchool)]
        [AllowAnonymous]
        public IActionResult GetEventByTokenScreenMonitoringToSchool(string token, List<int> eventType, 
            int pageNumber = 1, int pageSize = 10, string sortColumn = "EventTime", string sortDirection = "desc")
        {
            var data = _deviceService.GetDataByTokenMonitoring(token);
            if (data == null)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.InvalidToken);
            }

            var filter = new EventLogFilterModel()
            {
                DataTokenMonitoring = data,
                EventTypes = eventType,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
            };

            return Ok(_eventLogService.GetInitByDataTokenMonitoringToSchool(filter));
        }
        
        /// <summary>
        /// Get detail event-logs by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetEventLogDetailById(int id)
        {
            var eventLog = _eventLogService.GetDetailById(id);
            var data = _mapper.Map<EventLogDetailModel>(eventLog);
            
            // init related url
            EventLogRelatedFilterModel filter = new EventLogRelatedFilterModel()
            {
                EventLogId = eventLog.Id,
                EventLog = eventLog,
                PageNumber = 1,
                PageSize = 10,
                SortColumn = "EventTime",
                SortDirection = "desc",
            };
            
            var eventTypeAbNormal = new List<int>()
            {
                (short)EventType.UnknownPerson,
                (short)EventType.UnRegisteredID,
                (short)EventType.UnregisteredVehicle,
            };
            if (data.UserId.HasValue && data.UserId != 0)
            {
                filter.UserId = data.UserId.Value;
            }
            else if (data.VisitId.HasValue && data.VisitId != 0)
            {
                filter.VisitId = data.VisitId.Value;
            }
            else if (eventTypeAbNormal.Contains(data.EventType))
            {
                filter.EventTypes = eventTypeAbNormal;
            }
            
            data.RelatedEventLogs = _eventLogService.GetEventLogsRelated(filter, out _, out _);
            return Ok(data);
        }

        /// <summary>
        /// Get list event-logs related
        /// </summary>
        /// <param name="eventLogId"></param>
        /// <param name="userId"></param>
        /// <param name="visitId"></param>
        /// <param name="eventTypes"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsRelated)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetEventLogRelated(int eventLogId, int userId, int visitId, List<int> eventTypes,
            int pageNumber = 1, int pageSize = 10, string sortColumn = "EventTime", string sortDirection = "desc")
        {
            var filter = new EventLogRelatedFilterModel()
            {
                EventLogId = eventLogId,
                UserId = userId,
                VisitId = visitId,
                EventTypes = eventTypes,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
            };
            
            var data = _eventLogService.GetEventLogsRelated(filter, out int recordsTotal, out int recordsFiltered);
            
            var pagingData = new PagingData<EventLogDetailModel>
            {
                Data = data,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }


        /// <summary>
        /// Get the latest event-logs and figure out the count by workType
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsWorkTypeCount)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetCountByWorkType()
        {
            var data = _eventLogService.GetCountByWorkType();

            return Ok(data);
        }


        /// <summary>
        /// Get the latest event-logs and figure out the count by visitor Type
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsVisitTypeCount)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetCountByVisitType()
        {
            var data = _eventLogService.GetCountByVisitType();

            return Ok(data);
        }


        /// <summary>
        /// Get the latest event-logs and figure out the count by workType
        /// </summary>
        /// <param name="deviceIds"> list of device identifier (DB index)  </param>
        /// <param name="eventTypes"> list of eventType </param>
        /// <param name="workTypes"> List of workType to get EventLog </param>
        /// <param name="inOut"> List of in/Out type </param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiEventLogsWorkType)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetEventLogByWorkType(List<int> deviceIds, List<int> eventTypes, List<int> workTypes, List<int> inOut)
        {
            try
            {
                var data = _eventLogService.GetAttendanceStatus(deviceIds, eventTypes, workTypes, inOut);

                return Ok(data);
            }
            catch (Exception e)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, e.Message);
            }
        }


        /// <summary>
        /// Get in-out status of users by using eventLogs
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="deviceIds"></param>
        /// <param name="workTypes"></param>
        /// <param name="inOut"></param>
        /// <param name="departmentIds"></param>
        /// <param name="lastEventTime"></param>
        /// <param name="userName"></param>
        /// <param name="cardId"></param>
        /// <param name="militaryNumber"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiInOutStatusUser)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUserInOutStatus(string search, int pageNumber, int pageSize, string sortColumn, string sortDirection, 
            List<int> deviceIds, List<int> workTypes, List<int> inOut, List<int> departmentIds, DateTime lastEventTime, string userName, string cardId, string militaryNumber)
        {
            try
            {
                if (inOut == null || !inOut.Any())
                    inOut = new List<int>() { 0 };

                List<int> eventTypes = EnumHelper.ToEnumList<EventType>().Select(m => m.Id).Except(new List<int>()
                {
                    (int) EventType.NormalVehicle,
                    (int) EventType.ExceptionVehicle,
                    (int) EventType.Rule2Vehicle,
                    (int) EventType.Rule5Vehicle,
                    (int) EventType.ViolatedVehicle,
                    (int) EventType.IdCertifiedVehicle,
                    (int) EventType.UnregisteredVehicle,
                }).ToList();

                var data = _eventLogService.GetAttendanceStatus(deviceIds, eventTypes, workTypes, inOut, departmentIds, lastEventTime, userName, cardId, militaryNumber);

                var result = _userService.GetInOutStatus(data, search, pageNumber, pageSize, sortColumn, sortDirection, out int recordsTotal, out int recordsFiltered);

                var pagingData = new PagingData<UserInOutStatusListModel>
                {
                    Data = result,
                    Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
                };

                return Ok(pagingData);
            }
            catch (Exception e)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, e.Message);
            }
        }


        /// <summary>
        /// Get in-out status of vehicle by using eventLogs
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="deviceIds"></param>
        /// <param name="workTypes"></param>
        /// <param name="inOut"></param>
        /// <param name="departmentIds"></param>
        /// <param name="lastEventTIme"></param>
        /// <param name="userName"></param>
        /// <param name="model"></param>
        /// <param name="color"></param>
        /// <param name="plateNumber"></param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiInOutStatusVehicle)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetVehicleInOutStatus(string search, int pageNumber, int pageSize, string sortColumn, string sortDirection, List<int> deviceIds, List<int> workTypes, List<int> inOut, 
            List<int> departmentIds, DateTime lastEventTIme, string userName, string model, string color, string plateNumber)
        {
            try
            {
                if (inOut == null || !inOut.Any())
                    inOut = new List<int>() { 0 };

                List<int> eventTypes = new List<int>()
                {
                    (int) EventType.NormalVehicle,
                    (int) EventType.ExceptionVehicle,
                    (int) EventType.Rule2Vehicle,
                    (int) EventType.Rule5Vehicle,
                    (int) EventType.ViolatedVehicle,
                    (int) EventType.IdCertifiedVehicle,
                    (int) EventType.UnregisteredVehicle,
                };

                var data = _eventLogService.GetAttendanceStatus(deviceIds, eventTypes, workTypes, inOut, departmentIds, lastEventTIme, userName, model: model, color: color, plateNumber: plateNumber);

                var result = _deviceService.GetInOutStatus(data, search, pageNumber, pageSize, sortColumn, sortDirection, out int recordsTotal, out int recordsFiltered);

                var pagingData = new PagingData<VehicleInOutStatusListModel>
                {
                    Data = result,
                    Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
                };

                return Ok(pagingData);
            }
            catch (Exception e)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, e.Message);
            }
        }

        /// <summary>
        /// Upload image to update image of event-log
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(Constants.Route.ApiEventLogRidImages)]
        public IActionResult UploadImageToEventLog(string rid, [FromBody] UploadImageEventModel model)
        {
            var device = _deviceService.GetByDeviceAddress(rid);
            if (device == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);

            bool verifyHash = CryptographyHelper.VerifyMD5Hash($"{rid}{model.Id}", model.Hash);
            if (!verifyHash)
            {
                Console.WriteLine($"[UPLOAD IMAGE TO EVENT LOG FAILED][VERIFY HASH FAILED][{rid}]");
                Console.WriteLine(JsonConvert.SerializeObject(model, Formatting.Indented));
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.msgVerifyHashFailed);
            }

            if (!model.Image.IsTextBase64())
            {
                Console.WriteLine($"[UPLOAD IMAGE TO EVENT LOG FAILED][{rid}]");
                Console.WriteLine(JsonConvert.SerializeObject(model, Formatting.Indented));
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.msgImageInvalid);
            }
            
            bool isSuccess = _eventLogService.UploadImageToEventLog(device, model);
            if (!isSuccess)
            {
                Console.WriteLine($"[UPLOAD IMAGE TO EVENT LOG FAILED][{rid}]");
                Console.WriteLine(JsonConvert.SerializeObject(model, Formatting.Indented));
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.MsgFail);
            }

            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSuccess);
        }

        /// <summary>
        /// When register visit by Android Terminal Device, that device will upload file checkin
        /// </summary>
        /// <param name="rid"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(Constants.Route.ApiEventLogRidImagesCheckinVisit)]
        public IActionResult UploadImageCheckinVisitToEventLog(string rid, [FromBody] UploadImageVisitCheckinModel model)
        {
            var device = _deviceService.GetByDeviceAddress(rid);
            if (device == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundDevice);

            bool verifyHash = CryptographyHelper.VerifyMD5Hash($"{rid}{model.Id}", model.Hash);
            if (!verifyHash)
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.msgVerifyHashFailed);

            bool isSuccess = _eventLogService.UploadImageVisitCheckinToEventLog(device, model);
            if(!isSuccess)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.MsgFail);

            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSuccess);
        }
    }
}