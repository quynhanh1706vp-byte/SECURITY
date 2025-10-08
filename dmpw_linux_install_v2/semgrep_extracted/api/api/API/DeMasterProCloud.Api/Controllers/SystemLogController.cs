using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.Service;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Web;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.IO;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.DataModel.SystemLog;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.DataModel.Header;
using System.Collections.Generic;
using DeMasterProCloud.Service.Infrastructure.Header;


namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// SystemLog controller
    /// </summary>
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SystemLogController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly ISystemLogService _systemLogService;
        private readonly HttpContext _httpContext;
        private IConverter _converter;

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
        /// <param name="systemLogService"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="converter"></param>
        public SystemLogController(ISystemLogService systemLogService,
            IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IConverter converter)
        {
            _systemLogService = systemLogService;
            _configuration = configuration;
            _httpContext = httpContextAccessor.HttpContext;
            _converter = converter;
        }

        /// <summary>
        /// Get list of system logs with pagination. Filter by time model(OpeDateFrom - OpeDateTo, OpeTimeFrom - OpeTimeTo)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="objectType">type of object</param>
        /// <param name="company">company id</param>
        /// <param name="search">Query string that filter by user account</param>
        /// <param name="actionType">type of action</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiSystemLogs)]
        [CheckPermission(ActionName.View + Page.SystemLog)]
        public IActionResult SystemLog(SystemLogOperationTime model, int? objectType, int? company, string search, int? actionType, int pageNumber = 1,
            int pageSize = 10, string sortColumn = "OperationTime", string sortDirection = "desc")
        {
            sortColumn = SortColumnMapping.SystemLogColumn(sortColumn);
            var systemLogs = _systemLogService.GetPaginated(
                    model.OpeDateFrom, model.OpeDateTo, model.OpeTimeFrom,
                    model.OpeTimeTo, objectType, actionType, company, search, out var totalRecords,
                    out var recordsFiltered, pageNumber, pageSize, sortColumn, sortDirection);
            
            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();

            IPageHeader pageHeader = new PageHeader(_configuration, Page.SystemLog, companyId);
            List<HeaderData> header = pageHeader.GetHeaderList(companyId, accountId);

            var pagingData = new PagingData<SystemLogListModel, HeaderData>
            {
                Data = systemLogs,
                Header = header,
                Meta = { RecordsTotal = totalRecords, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }

        /// GET /system-logs/ActionListItems
        /// <summary>
        /// Get Action List items of System Log
        /// </summary>
        /// <param name="systemLogType">type of log in system</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiSystemLogsActionListItems)]
        [CheckPermission(ActionName.View + Page.SystemLog)]
        public IActionResult GetActionListItems(int systemLogType)
        {
            var actionListItems = _systemLogService.GetActionListItems(systemLogType);
            return Ok(actionListItems);
        }

        /// GET /system-logs/SystemLogListItems
        /// <summary>
        /// Get SystemLogTypelistItems
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiSystemLogsTypeListItems)]
        [CheckPermission(ActionName.View + Page.SystemLog)]
        public IActionResult GetSystemLogTypes()
        {
            var result = _systemLogService.GetSystemLogTypeListItems();
            return Ok(result);
        }


        /// <summary>
        /// View the system log data through pdf file. Filter by time model(OpeDateFrom - OpeDateTo, OpeTimeFrom - OpeTimeTo)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="objectType">type of object</param>
        /// <param name="action">type of action</param>
        /// <param name="company">company id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiSystemLogsExportPdf)]
        [CheckPermission(ActionName.Export + Page.SystemLog)]
        public IActionResult PdfView(SystemLogOperationTime model, string sortColumn, string sortDirection,
            int? objectType, int? action, int? company)
        {
            sortColumn = Helpers.CheckPropertyInObject<SystemLogListModel>(sortColumn, "OperationTime", ColumnDefines.SystemLogForReport);
            var accountType = _httpContext.User.GetAccountType();
            var systemLogs = _systemLogService.FilterDataWithOrder(sortColumn, sortDirection, 0, 0, out _, out _
                    , model.OpeDateFrom, model.OpeDateTo, model.OpeTimeFrom, model.OpeTimeTo, objectType, action,
                    company).AsEnumerable()
                .Select(m =>
                {
                    var result = new SystemLogListModel
                    {
                        //UserAccount = (accountType == (short)AccountType.SuperAdmin)
                        //    ? m.Company.Name
                        //    : m.CreatedByNavigation.Username,
                        UserAccount = m.UserAccount,
                        OperationTime = m.OperationTime,
                        OperationType = m.OperationType,
                        Action = m.Action,
                        OperationAction = m.OperationAction,
                        Message = HttpUtility.HtmlDecode(m.Message),
                    };
                    return result;
                }).ToList();

            ViewBag.Message = systemLogs;

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "PDF Report",
            };

            var view = this.View("PdfView");
            var html = ViewResultExtensions.ToHtml(View(), _httpContext);
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/css", "styles.css") },
                HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
                FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            var file = _converter.Convert(pdf);
            var filename = string.Format(Constants.ExportFileFormat, SystemLogResource.lblSystemLog, DateTime.Now) + ".pdf";

            _systemLogService.SaveSystemLogExport(filename, model);

            return File(file, "application/pdf", filename);
        }

        /// GET /system-logs/export
        /// <summary>
        /// Export file for system Logs. Filter by time model(OpeDateFrom - OpeDateTo, OpeTimeFrom - OpeTimeTo)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="objectType">type of object</param>
        /// <param name="company">company id</param>
        /// <param name="type">type of file export</param>
        /// <param name="filter"> text data to filter </param>
        /// <param name="search"> text data to filter </param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="action">type of action</param>
        /// <param name="actionType">type of action</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiSystemLogsExport)]
        [CheckPermission(ActionName.Export + Page.SystemLog)]
        //public IActionResult Export(SystemLogOperationTime model, int? objectType, int? action, int? company,
        //    string type = "excel", string filter = "", string sortColumn = null, string sortDirection = "desc")
        public IActionResult Export(SystemLogOperationTime model,  int? objectType, int? action, int? actionType, int? company, string filter, string search,  string type = "excel",
            string sortColumn = "OperationTime", string sortDirection = "desc")
        {
            sortColumn = SortColumnMapping.SystemLogColumn(sortColumn);
            //var stopWatch = Stopwatch.StartNew();
            var fileData = _systemLogService.Export(type, sortColumn, sortDirection, out _, out _
                , model.OpeDateFrom, model.OpeDateTo, model.OpeTimeFrom, model.OpeTimeTo, objectType, action ?? actionType, company, string.IsNullOrWhiteSpace(filter) ? search : filter);
            //stopWatch.Stop();
            var filename = string.Format(Constants.ExportFileFormat, SystemLogResource.lblSystemLog, DateTime.Now);
            var fullName = type == "excel" ? $"{filename}.xlsx" : $"{filename}.csv";

            _systemLogService.SaveSystemLogExport(fullName, model, string.IsNullOrWhiteSpace(filter) ? search : filter, objectType, action ?? actionType);

            if (fileData == null)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.msgExportFailed);
            }
            if (type != null && type.Equals("excel"))
                return File(fileData, "application/ms-excel", fullName);
            return File(fileData, "application/text", fullName);

        }
    }
}