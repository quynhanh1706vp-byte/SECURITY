using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.SystemLog;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Bogus.Extensions;
using SelectListItem = Microsoft.AspNetCore.Mvc.Rendering.SelectListItem;

namespace DeMasterProCloud.Service
{
    /// <summary>
    /// SystemLog service interface
    /// </summary>
    public interface ISystemLogService
    {
        void Add(int logObjId, SystemLogType sysType, ActionLogType type, string content = null,
            string contentDetails = null, List<int> logObjIds = null, int? companyId = null, int? createdBy = null);

        Task AddAsync(int logObjId, SystemLogType sysType, ActionLogType type, string content = null,
            string contentDetails = null, List<int> logObjIds = null, int? companyId = null, int? createdBy = null);

        void InitData(SystemLogModel model);

        List<SystemLogListModel> GetPaginated(string opeDateFrom, string opeDateTo,
            string opeTimeFrom, string opeTimeTo, int? objectType, int? action, int? company, string search, out int totalRecords,
            out int recordsFiltered, int pageNumber, int pageSize, string sortColumn,
            string sortDirection);

        bool HasData(int? companyId);

        SystemLogAction GetActionListItems(int systemLogType);

        SystemLogOperationType GetSystemLogTypeListItems();

        void Save();

        string ExportPDF(string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, string opeDateFrom, string opeDateTo, string opeTimeFrom, string opeTimeTo,
            int? objectType, int? action, int? company);

        List<SystemLogListModel> FilterDataWithOrder(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            out int totalRecords, out int recordsFiltered, string opeDateFrom, string opeDateTo, string opeTimeFrom, string opeTimeTo,
            int? objectType, int? action, int? company, string search = "", string dateFormat = Constants.DateTimeFormatDefault);

        byte[] Export(string type, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, string opeDateFrom, string opeDateTo, string opeTimeFrom, string opeTimeTo,
            int? objectType, int? action, int? company, string search);

        string GetUserAccount(int userId);

        void SaveSystemLogExport(string fileName, SystemLogOperationTime model, string search = null, int? objectType = null, int? action = null);
    }

    public class SystemLogService : ISystemLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        private readonly List<string> _header = new List<string>
        {
            SystemLogResource.lblOperationTime,
            SystemLogResource.lblUserAccount,
            SystemLogResource.lblOperationType,
            SystemLogResource.lblAction,
            SystemLogResource.lblMessage,
            SystemLogResource.lblMessageDetail
        };

        public SystemLogService(IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<SystemLogService>();
        }

        /// <summary>
        /// Initial data
        /// </summary>
        /// <param name="model"></param>
        public void InitData(SystemLogModel model)
        {
            try
            {
                model.ObjectTypeItems = EnumHelper.ToSelectList<SystemLogType>().ToList();
                model.ObjectTypeItems.Insert(0, new SelectListItem
                {
                    Text = CommonResource.lblDefaultSelectListItem,
                    Value = string.Empty
                });
                model.ActionItems.Add(new SelectListItem
                {
                    Text = CommonResource.lblDefaultSelectListItem,
                    Value = string.Empty
                });
                if (_httpContext.User.GetAccountType() == (short)AccountType.SuperAdmin)
                {
                    model.CompanyItems = _unitOfWork.CompanyRepository.GetCompanies()
                        .Select(c => new SelectListItem
                        {
                            Text = c.Name,
                            Value = c.Id.ToString()
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitData");
            }
        }

        /// <summary>
        /// Add system log
        /// </summary>
        /// <param name="logObjId"></param>
        /// <param name="sysType"></param>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <param name="contentDetails"></param>
        /// <param name="logObjIds"></param>
        /// <param name="companyId"></param>
        /// <param name="createdBy"></param>
        public void Add(int logObjId, SystemLogType sysType, ActionLogType type, string content = null,
            string contentDetails = null, List<int> logObjIds = null, int? companyId = null, int? createdBy = null)
        {
            try
            {
                _unitOfWork.SystemLogRepository.Add(logObjId, sysType, type, content, contentDetails, logObjIds, companyId, createdBy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Add");
            }
        }

        /// <summary>
        /// Add async system log
        /// </summary>
        /// <param name="logObjId"></param>
        /// <param name="sysType"></param>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <param name="contentDetails"></param>
        /// <param name="logObjIds"></param>
        /// <param name="companyId"></param>
        /// <param name="createdBy"></param>
        /// <returns></returns>
        public Task AddAsync(int logObjId, SystemLogType sysType, ActionLogType type, string content = null,
            string contentDetails = null, List<int> logObjIds = null, int? companyId = null, int? createdBy = null)
        {
            try
            {
                return Task.Run(() =>
                {
                    Add(logObjId, sysType, type, content, contentDetails, logObjIds, companyId, createdBy);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAsync");
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Check if there is any system log data
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public bool HasData(int? companyId)
        {
            try
            {
                if (_httpContext.User.GetAccountType() == (short)AccountType.SuperAdmin)
                {
                    if (companyId.HasValue)
                    {
                        return _unitOfWork.AppDbContext.SystemLog.Any(m => m.CompanyId == companyId);
                    }

                    return _unitOfWork.AppDbContext.SystemLog.Any();
                }
                return _unitOfWork.AppDbContext.SystemLog.Any(m => m.CompanyId == _httpContext.User.GetCompanyId());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HasData");
                return false;
            }
        }

        /// <summary>
        /// Get system log data with pagination
        /// </summary>
        /// <param name="search"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="opeDateFrom"></param>
        /// <param name="opeDateTo"></param>
        /// <param name="opeTimeFrom"></param>
        /// <param name="opeTimeTo"></param>
        /// <param name="objectType"></param>
        /// <param name="action"></param>
        /// <param name="company"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public List<SystemLogListModel> GetPaginated(string opeDateFrom, string opeDateTo,
            string opeTimeFrom, string opeTimeTo, int? objectType, int? action, int? company, string search, out int totalRecords,
            out int recordsFiltered, int pageNumber, int pageSize, string sortColumn, string sortDirection)
        {
            try
            {
                var data = FilterDataWithOrder(sortColumn, sortDirection, pageNumber, pageSize, out totalRecords,
                        out recordsFiltered, opeDateFrom, opeDateTo, opeTimeFrom, opeTimeTo, objectType, action, company, search);

                foreach (var record in data)
                {
                    record.Htmlparse = true;

                    //record.OperationTime = Helpers.ConvertToUserTime(Convert.ToDateTime(record.OperationTime), accountTimezone).ToSettingDateTimeString();
                    //record.OperationTime = Helpers.ConvertToUserTime(Convert.ToDateTime(record.OperationTime), offSet).ToSettingDateTimeString();

                    record.OperationTime = DateTime.TryParseExact(record.OperationTime, Constants.Settings.DateTimeFormatDefault, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtValue)
                        ? dtValue.ConvertDefaultDateTimeToString() : record.OperationTime;
                }

                return data.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginated");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<SystemLogListModel>();
            }
        }

        /// <summary>
        /// Filter data to export to a file
        /// </summary>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="opeDateFrom"></param>
        /// <param name="opeDateTo"></param>
        /// <param name="opeTimeFrom"></param>
        /// <param name="opeTimeTo"></param>
        /// <param name="objectType"></param>
        /// <param name="action"></param>
        /// <param name="company"></param>
        /// <param name="search"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public List<SystemLogListModel> FilterDataWithOrder(string sortColumn, string sortDirection, int pageNumber, int pageSize,
            out int totalRecords, out int recordsFiltered,
            string opeDateFrom, string opeDateTo, string opeTimeFrom, string opeTimeTo,
            int? objectType, int? action, int? company, string search = "", string dateFormat = Constants.DateTimeFormatDefault)
        {
            DateTime opeDateTimeFrom = DateTime.UtcNow;
            DateTime opeDateTimeTo = DateTime.UtcNow;

            MakeConditionToSearch(opeDateFrom, opeDateTo, opeTimeFrom, opeTimeTo, ref opeDateTimeFrom, ref opeDateTimeTo, ref search);

            var accountTimezone = _unitOfWork.AccountRepository.Get(x => x.Id == _httpContext.User.GetAccountId() && !x.IsDeleted).TimeZone;
            var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

            //opeDateTimeFrom = opeDateTimeFrom.ConvertToSystemTime(accountTimezone);
            //opeDateTimeTo = opeDateTimeTo.ConvertToSystemTime(accountTimezone);
            opeDateTimeFrom = opeDateTimeFrom.ConvertToSystemTime(offSet);
            opeDateTimeTo = opeDateTimeTo.ConvertToSystemTime(offSet);

            var companyId = _httpContext.User.GetCompanyId();

            // Include Pagination
            var systemLogs = _unitOfWork.AppDbContext.SystemLog
                .Include(m => m.CreatedByNavigation)
                .Include(m => m.Company)
                .Where(m => m.CompanyId == companyId)
                .Where(m => m.OpeTime >= opeDateTimeFrom && m.OpeTime <= opeDateTimeTo)
                .Where(m => objectType == null || m.Type == objectType)
                .Where(m => action == null || m.Action == action);

            totalRecords = systemLogs.Count();
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().RemoveDiacritics().ToLower();
                systemLogs = systemLogs.AsEnumerable().Where(
                    m =>
                        (m.CreatedByNavigation != null && m.CreatedByNavigation.Username?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                        || (m.Content?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                        || (m.ContentDetails?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)).AsQueryable();
            }

            recordsFiltered = systemLogs.Count();
            
            var resultOrdered = systemLogs.OrderBy($"{sortColumn} {sortDirection}");
            List<SystemLog> result = new List<SystemLog>(resultOrdered);
            if (pageSize > 0)
            {
                result = resultOrdered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }
            
            var data = result.Select(m => new SystemLogListModel()
                {
                    Id = m.Id,
                    //OperationTime = Helpers.ConvertToUserTimeZoneReturnDate(m.OpeTime, accountTimezone).ToSettingDateTimeString(),
                    OperationTime = m.OpeTime.ConvertDefaultDateTimeToString(dateFormat),
                    UserAccount = m.CreatedByNavigation.Username,
                    OperationType = ((SystemLogType)m.Type).GetDescription(),
                    Action = ((ActionLogType)m.Action).GetDescription(),
                    OperationAction = ((ActionLogType)m.Action).GetDescription(),
                    Message = HttpUtility.HtmlDecode(m.Content),
                    Details = string.IsNullOrEmpty(HttpUtility.HtmlDecode(m.ContentDetails)) ? "" : HttpUtility.HtmlDecode(m.ContentDetails)
            }
            );

            return data.ToList();
        }

        ///// <summary>
        ///// Filter data
        ///// </summary>
        ///// <param name="opeDateFrom"></param>
        ///// <param name="opeDateTo"></param>
        ///// <param name="opeTimeFrom"></param>
        ///// <param name="opeTimeTo"></param>
        ///// <param name="objectType"></param>
        ///// <param name="action"></param>
        ///// <param name="data"></param>
        ///// <param name="search"></param>
        ///// <returns></returns>
        //private IQueryable<SystemLog> FilterData(string opeDateFrom, string opeDateTo, string opeTimeFrom, string opeTimeTo,
        //    int? objectType, int? action, IQueryable<SystemLog> data, string search = "")
        //{
        //    if (!string.IsNullOrEmpty(opeDateFrom))
        //    {
        //        var opeDateTimeFrom = Helpers.GetFromToDateTime(opeDateFrom, opeTimeFrom, false);
        //        data = data.Where(m =>
        //            m.OpeTime >= opeDateTimeFrom);
        //    }

        //    if (!string.IsNullOrEmpty(opeDateTo))
        //    {
        //        var opeDateTimeTo = Helpers.GetFromToDateTime(opeDateTo, opeTimeTo, true);
        //        data = data.Where(m =>
        //            m.OpeTime <= opeDateTimeTo);
        //    }
        //    if (!string.IsNullOrEmpty(search))
        //    {
        //        search = search.ToLower();
        //        data = data.Where(x =>
        //            x.OpeTime.ToSettingDateTimeString().ToLower().Contains(search) ||
        //            GetUserAccount(x.CreatedBy).ToLower().Contains(search) ||
        //            ((SystemLogType)x.Type).GetDescription().ToLower().Contains(search) ||
        //            ((ActionLogType)x.Action).GetDescription().ToLower().Contains(search) ||
        //            HttpUtility.HtmlDecode(x.Content).ToLower().Contains(search));
        //    }

        //    if (objectType.HasValue)
        //    {
        //        data = data.Where(m => m.Type == objectType.Value);
        //    }

        //    if (action.HasValue)
        //    {
        //        data = data.Where(m => m.Action == action.Value);
        //    }

        //    return data;
        //}

        /// <summary>
        /// Make "Where" clause
        /// </summary>
        /// <param name="opeDateFrom"></param>
        /// <param name="opeDateTo"></param>
        /// <param name="opeTimeFrom"></param>
        /// <param name="opeTimeTo"></param>
        /// <param name="objectType"></param>
        /// <param name="action"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        private void MakeConditionToSearch(string opeDateFrom, string opeDateTo, string opeTimeFrom, string opeTimeTo,
            ref DateTime opeDateTimeFrom, ref DateTime opeDateTimeTo, ref string search)
        {
            DateTimeFilterModel dateTimeFilterModel = new DateTimeFilterModel()
            {
                DateFrom = opeDateFrom,
                DateTo = opeDateTo,
                TimeFrom = opeTimeFrom,
                TimeTo = opeTimeTo,
            };

            if (!string.IsNullOrEmpty(opeDateFrom))
            {
                //opeDateTimeFrom = Helpers.GetFromToDateTime(opeDateFrom, opeTimeFrom, false);
                opeDateTimeFrom = dateTimeFilterModel.GetDateTime_From();
                if(opeDateTimeFrom == new DateTime())
                    opeDateTimeFrom = DateTime.UtcNow.AddDays(-1);
            }
            else
            {
                opeDateTimeFrom = DateTime.UtcNow.AddDays(-1);
            }

            if (!string.IsNullOrEmpty(opeDateTo))
            {
                //opeDateTimeTo = Helpers.GetFromToDateTime(opeDateTo, opeTimeTo, true);
                opeDateTimeTo = dateTimeFilterModel.GetDateTime_To();
                if (opeDateTimeTo == new DateTime())
                    opeDateTimeTo = DateTime.UtcNow.Date.AddDays(2);
            }
            else
            {
                opeDateTimeTo = DateTime.UtcNow.Date.AddDays(2);
            }

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
            }
            else
            {
                search = "";
            }
        }


        public void SaveSystemLogExport(string fileName, SystemLogOperationTime model, string search = null, int? objectType = null, int? action = null)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //Save system log
                        var content = $"{CommonResource.lblFileName} : {fileName}";

                        List<string> contentDetails = [];

                        // Date Time range
                        DateTime opeDateTimeFrom = DateTime.UtcNow;
                        DateTime opeDateTimeTo = DateTime.UtcNow;

                        MakeConditionToSearch(model.OpeDateFrom, model.OpeDateTo, model.OpeTimeFrom, model.OpeTimeTo, ref opeDateTimeFrom, ref opeDateTimeTo, ref search);

                        string dateTimeRange = $"{CommonResource.lblDateTimeRange} : {opeDateTimeFrom} ~ {opeDateTimeTo}";
                        contentDetails.Add(dateTimeRange);

                        // Search string
                        if (!string.IsNullOrWhiteSpace(search))
                        {
                            string searchFilter = $"{CommonResource.lblSearch} : {search}";
                            contentDetails.Add(searchFilter);
                        }

                        // Operation type
                        if (objectType != null)
                        {
                            string operationType = $"{SystemLogResource.lblOperationType} : {((SystemLogType)objectType.Value).GetDescription()}";
                            contentDetails.Add(operationType);
                        }

                        // Action
                        if (action != null)
                        {
                            string typeFilter = $"{SystemLogResource.lblAction} : {((ActionLogType) action.Value).GetDescription()}";
                            contentDetails.Add(typeFilter);
                        }

                        string contentsDetail = String.Empty;
                        if (contentDetails.Any())
                        {
                            contentsDetail = string.Join("\n", contentDetails);
                        }

                        _unitOfWork.SystemLogRepository.Add(1, SystemLogType.Report, ActionLogType.SystemLogExport,
                            content, contentsDetail, null, _httpContext.User.GetCompanyId());

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Export data in excel or txt format
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="opeDateFrom"></param>
        /// <param name="opeDateTo"></param>
        /// <param name="opeTimeFrom"></param>
        /// <param name="opeTimeTo"></param>
        /// <param name="objectType"></param>
        /// <param name="action"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        public byte[] Export(string type, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, string opeDateFrom, string opeDateTo, string opeTimeFrom, string opeTimeTo,
            int? objectType, int? action, int? company, string search)
        {
            try
            {
                var culture = Thread.CurrentThread.CurrentCulture.Name;
                var dateTimeFormat = ApplicationVariables.Configuration[Constants.DateTimeServerFormat + ":" + culture];

                var data = FilterDataWithOrder(sortColumn, sortDirection, 0, 0, out totalRecords,
                        out recordsFiltered, opeDateFrom, opeDateTo, opeTimeFrom, opeTimeTo, objectType, action, company, search, dateFormat: dateTimeFormat)
                    .ToList();

                byte[] fileByte;

                ExportHelper exportHelper = new(_configuration, _httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId());
                switch (type.ToLower())
                {
                    case "csv":
                        fileByte = exportHelper.ExportDataToCsv(data, Page.SystemLog);
                        break;
                    case "hancell":
                        fileByte = exportHelper.ExportDataToHancell(data, Page.SystemLog);
                        break;
                    case "excel":
                    default:
                        fileByte = exportHelper.ExportDataToExcel(data, Page.SystemLog);
                        break;
                }

                return fileByte;

                //return type == Constants.Excel
                //    ? ExportExcel(sortColumn, sortDirection, out totalRecords,
                //    out recordsFiltered, opeDateFrom, opeDateTo, opeTimeFrom, opeTimeTo, objectType, action, company, search)
                //    : ExportTxt(sortColumn, sortDirection, out totalRecords,
                //        out recordsFiltered, opeDateFrom, opeDateTo, opeTimeFrom, opeTimeTo, objectType, action, company, search);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Export");
                totalRecords = 0;
                recordsFiltered = 0;
                return new byte[0];
            }
        }

        /// <summary>
        /// Export data in excel format
        /// </summary>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="opeDateFrom"></param>
        /// <param name="opeDateTo"></param>
        /// <param name="opeTimeFrom"></param>
        /// <param name="opeTimeTo"></param>
        /// <param name="objectType"></param>
        /// <param name="action"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        private byte[] ExportExcel(string sortColumn, string sortDirection, out int totalRecords,
        out int recordsFiltered, string opeDateFrom, string opeDateTo, string opeTimeFrom, string opeTimeTo,
        int? objectType, int? action, int? company, string search)
        {
            byte[] result;
            try
            {
                var accountTimezone = _unitOfWork.AccountRepository.Get(m =>
                      m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted).TimeZone;
                var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

                //var package = new ExcelPackage();
                using (var package = new ExcelPackage())
                {
                    // add a new worksheet to the empty workbook
                    var worksheet =
                        package.Workbook.Worksheets.Add(SystemLogResource.lblSystemLog); //Worksheet name

                    worksheet.Cells.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;

                    //Check if it's not root company the remove company field
                    var accountType = _httpContext.User.GetAccountType();

                    //First add the _headers
                    for (var i = 0; i < _header.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = _header[i];
                    }

                    worksheet.View.FreezePanes(2, 1);

                    var culture = Thread.CurrentThread.CurrentCulture.Name;
                    var dateTimeFormat = ApplicationVariables.Configuration[Constants.DateTimeServerFormat + ":" + culture];

                    var data = FilterDataWithOrder(sortColumn, sortDirection, 0, 0, out totalRecords,
                            out recordsFiltered, opeDateFrom, opeDateTo, opeTimeFrom, opeTimeTo, objectType, action, company, search, dateFormat: dateTimeFormat)
                        .ToList();

                    var recordIndex = 2;
                    foreach (var report in data)
                    {
                        var colIndex = 1;

                        worksheet.Cells[recordIndex, colIndex++].Value = string.IsNullOrWhiteSpace(report.OperationTime)
                        ? ""
                        : DateTime.SpecifyKind(DateTime.ParseExact(report.OperationTime, dateTimeFormat, null), DateTimeKind.Utc).ConvertToUserTime(offSet).ToSettingDateTimeString();

                        worksheet.Cells[recordIndex, colIndex++].Value =
                            report.UserAccount;
                        worksheet.Cells[recordIndex, colIndex++].Value =
                            report.OperationType;
                        worksheet.Cells[recordIndex, colIndex++].Value =
                            report.Action;

                        if (!string.IsNullOrWhiteSpace(report.Message))
                        {
                            report.Message = report.Message.Replace("<br />", "\n");
                            // worksheet.Cells[recordIndex, colIndex].Style.WrapText = true;
                            worksheet.Cells[recordIndex, colIndex].Value = report.Message.Replace("\n", Environment.NewLine);
                            worksheet.Row(recordIndex).Height = Math.Max(20, report.Message.Split('\n').Length * 15);
                        }

                        if (!string.IsNullOrWhiteSpace(report.Details))
                        {
                            colIndex++;
                            
                            report.Details = report.Details.Replace("<br />", "\n");
                            // worksheet.Cells[recordIndex, colIndex].Style.WrapText = true;
                            worksheet.Cells[recordIndex, colIndex].Value = report.Details.Replace("\n", Environment.NewLine);
                            worksheet.Row(recordIndex).Height = Math.Max(20, report.Details.Split('\n').Length * 15);
                        }
                        recordIndex++;
                    }

                    worksheet.Cells.AutoFitColumns();
                    result = package.GetAsByteArray();
                }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            totalRecords = 0;
            recordsFiltered = 0;
            return null;
        }

        /// <summary>
        /// Export data in txt format
        /// </summary>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="opeDateFrom"></param>
        /// <param name="opeDateTo"></param>
        /// <param name="opeTimeFrom"></param>
        /// <param name="opeTimeTo"></param>
        /// <param name="objectType"></param>
        /// <param name="action"></param>
        /// <param name="company"></param>
        /// <returns></returns>
        public byte[] ExportTxt(string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, string opeDateFrom, string opeDateTo, string opeTimeFrom, string opeTimeTo,
            int? objectType, int? action, int? company, string search)
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            var dateTimeFormat = ApplicationVariables.Configuration[Constants.DateServerFormat + ":" + culture];

            var data = FilterDataWithOrder(sortColumn, sortDirection, 0, 0, out totalRecords,
                    out recordsFiltered, opeDateFrom, opeDateTo, opeTimeFrom, opeTimeTo, objectType, action, company, search, dateFormat: dateTimeFormat)
                .ToList();
            var accountType = _httpContext.User.GetAccountType();
            var reportTxt = new StringBuilder();
            // Build the file content
            foreach (var report in data)
            {
                if (!string.IsNullOrWhiteSpace(report.Message))
                {
                    report.Message = report.Message.Replace("<br />", "\n");
                    report.Message = report.Message.Replace("\n", " | ");
                }

                if (!string.IsNullOrWhiteSpace(report.Details))
                {
                    report.Details = report.Details.Replace("<br />", "\n");
                    report.Details = report.Details.Replace("\n", " | ");
                }

                var obj = new List<object>
                    {
                        report.UserAccount,
                        report.OperationTime,
                        report.OperationType,
                        report.Action,
                        report.Message,
                        report.Details
                    };

                reportTxt.AppendLine(string.Join(",", obj));
            }
            if (accountType == (short)AccountType.SuperAdmin)
            {
                _header.Insert(0, SystemLogResource.lblCompany);
            }
            byte[] buffer = Encoding.UTF8.GetBytes($"{string.Join(",", _header)}\r\n{reportTxt}");
            buffer = Encoding.UTF8.GetPreamble().Concat(buffer).ToArray();

            return buffer;
        }

        public string ExportPDF(string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, string opeDateFrom, string opeDateTo, string opeTimeFrom, string opeTimeTo,
            int? objectType, int? action, int? company)
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            var dateTimeFormat = ApplicationVariables.Configuration[Constants.DateServerFormat + ":" + culture];

            //Check if it's not root company the remove company field
            var accountType = _httpContext.User.GetAccountType();
            var data = FilterDataWithOrder(sortColumn, sortDirection, 0, 0, out totalRecords,
                    out recordsFiltered, opeDateFrom, opeDateTo, opeTimeFrom, opeTimeTo, objectType, action, company, dateFormat: dateTimeFormat)
                .ToList();

            return GetSystemLogHTMLString(_header, data, accountType);
        }

        private string GetSystemLogHTMLString(List<string> header, List<SystemLogListModel> data, int accountType)
        {
            var sb = new StringBuilder();
            sb.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>

                                <div class='header'><h1>DMPW SystemLog</h1></div>
                                <table align='center'>
                                    <tr>");
            foreach (var head in header)
            {
                sb.AppendFormat(@"
                                        <th>{0}</th>
                                        ", head);
            }
            sb.Append("</tr>");

            foreach (var report in data)
            {
                string userAccount = "";
                //if (accountType == (short)AccountType.SuperAdmin)
                //{
                //    userAccount = report.Company.Name;
                //}
                // 
                //else

                userAccount = report.UserAccount;

                string opeTime = report.OperationTime;
                string type = report.OperationType;
                string action = report.Action;
                string content = report.Message;
                string details = report.Details.Replace("<br />", "\n");

                //+(!string.IsNullOrEmpty(report.ContentDetails)
                //       ? $" ({HttpUtility.HtmlDecode(report.ContentDetails).Replace("<br>", Environment.NewLine)})"
                //       : string.Empty);

                sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                    <td>{4}</td>
                                    <td>{5}</td>
                                  </tr>", userAccount, opeTime, type, action, content, details);
            }

            sb.Append(@"
                                </table>
                            </body>
                        </html>");

            return sb.ToString();
        }

        /// <summary>
        /// Get System Log list item
        /// </summary>
        /// <returns></returns>
        public SystemLogOperationType GetSystemLogTypeListItems()
        {
            try
            {
                var systemLogTypeListItem = EnumHelper.ToSelectList<SystemLogType>().OrderBy(m => m.Text).ToList();
                return new SystemLogOperationType { Data = systemLogTypeListItem };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSystemLogTypeListItems");
                return new SystemLogOperationType { Data = new List<SelectListItem>() };
            }
        }

        /// <summary>
        /// Get action list item
        /// </summary>
        /// <param name="systemLogType"></param>
        /// <returns></returns>
        public SystemLogAction GetActionListItems(int systemLogType)
        {
            try
            {
                if (systemLogType == 0)
                {
                    var listEmpty = new List<SelectListItem> {
                        new SelectListItem
                    {
                        Text = CommonResource.lblDefaultSelectListItem,
                        Value = string.Empty
                    }};
                    return new SystemLogAction { Data = listEmpty };
                }
                var actionListItem = EnumHelper.ToSelectList<ActionLogType>().ToList();
                var baseListItem = actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.Add) ||
                        c.Value == Convert.ToString((short)ActionLogType.Update) ||
                        c.Value == Convert.ToString((short)ActionLogType.Delete) ||
                        c.Value == Convert.ToString((short)ActionLogType.DeleteMultiple))
                    .ToList();
            switch ((SystemLogType)systemLogType)
            {
                case SystemLogType.Company:
                    break;

                case SystemLogType.AccountManagement:
                    baseListItem.AddRange(actionListItem.Where(c =>
                            c.Value == Convert.ToString((short)ActionLogType.ChangePassword) ||
                            c.Value == Convert.ToString((short)ActionLogType.Login) ||
                            c.Value == Convert.ToString((short)ActionLogType.Logout))
                        .ToList());
                    break;

                case SystemLogType.Building:
                    baseListItem.AddRange(actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.AssignDoor) ||
                        c.Value == Convert.ToString((short)ActionLogType.UpdateDoor) ||
                        c.Value == Convert.ToString((short)ActionLogType.UnassignDoor) ||
                        c.Value == Convert.ToString((short)ActionLogType.AddMaster) ||
                        c.Value == Convert.ToString((short)ActionLogType.DeleteMaster))
                    .ToList());
                    break;

                case SystemLogType.Department:
                    baseListItem.AddRange(actionListItem.Where(c =>
                       c.Value == Convert.ToString((short)ActionLogType.Export) ||
                       c.Value == Convert.ToString((short)ActionLogType.Import))
                   .ToList());
                    break;

                case SystemLogType.User:
                    baseListItem.AddRange(actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.Export) ||
                        c.Value == Convert.ToString((short)ActionLogType.Import) ||
                        c.Value == Convert.ToString((short)ActionLogType.AccessibleDoorExport))
                    .ToList());
                    break;

                case SystemLogType.Holiday:
                    break;

                case SystemLogType.AccessTime:
                    break;

                case SystemLogType.DeviceMonitoring:
                    baseListItem.Clear();
                    baseListItem.AddRange(actionListItem.Where(c =>
                            c.Value == Convert.ToString((short)ActionLogType.DoorOpen) ||
                            c.Value == Convert.ToString((short)ActionLogType.Reset) ||
                            c.Value == Convert.ToString((short)ActionLogType.Sync))
                        .ToList());
                    break;

                case SystemLogType.DeviceUpdate:
                    baseListItem.Clear();
                    baseListItem.AddRange(actionListItem.Where(c =>
                            c.Value == Convert.ToString((short)ActionLogType.Update) ||
                            c.Value == Convert.ToString((short)ActionLogType.Fail) ||
                            c.Value == Convert.ToString((short)ActionLogType.Success))
                        .ToList());
                    break;

                case SystemLogType.DeviceSetting:
                    baseListItem.AddRange(actionListItem.Where(c =>
                            c.Value == Convert.ToString((short)ActionLogType.InvalidDoor) ||
                            c.Value == Convert.ToString((short)ActionLogType.ValidDoor) ||
                            c.Value == Convert.ToString((short)ActionLogType.CopyDoorSetting) ||
                            c.Value == Convert.ToString((short)ActionLogType.Reinstall) ||
                            c.Value == Convert.ToString((short)ActionLogType.AutoRegister))
                        .ToList());
                    break;

                case SystemLogType.AccessGroup:
                    baseListItem.AddRange(actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.AssignDoor) ||
                        c.Value == Convert.ToString((short)ActionLogType.UnassignDoor) ||
                        c.Value == Convert.ToString((short)ActionLogType.AssignUser) ||
                        c.Value == Convert.ToString((short)ActionLogType.UnassignUser))
                    .ToList());
                    break;

                case SystemLogType.MessageSetting:
                    baseListItem.AddRange(actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.CopyDoorSetting))
                    .ToList());
                    break;

                case SystemLogType.SystemSetting:
                    baseListItem.Clear();
                    baseListItem.AddRange(actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.Update) ||
                        c.Value == Convert.ToString((short)ActionLogType.UpdateMultipleUser))
                    .ToList());
                    break;

                case SystemLogType.CheckDeviceSetting:
                    baseListItem.Clear();
                    baseListItem.AddRange(actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.BasicInfoTransmit) ||
                        c.Value == Convert.ToString((short)ActionLogType.HolidayTransmit) ||
                        c.Value == Convert.ToString((short)ActionLogType.TimezoneTransmit))
                    .ToList());
                    break;

                case SystemLogType.Emergency:
                    baseListItem.Clear();
                    baseListItem.AddRange(actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.ForcedOpen) ||
                        c.Value == Convert.ToString((short)ActionLogType.Release) ||
                        c.Value == Convert.ToString((short)ActionLogType.ForcedClose))
                    .ToList());
                    break;

                case SystemLogType.EventRecovery:
                    baseListItem.Clear();
                    baseListItem.AddRange(actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.Transmit))
                    .ToList());
                    break;

                case SystemLogType.CheckUserInformation:
                    baseListItem.Clear();
                    baseListItem.AddRange(actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.Transmit))
                    .ToList());
                    break;

                case SystemLogType.TransmitAllData:
                    baseListItem.Clear();
                    baseListItem.AddRange(actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.Transmit))
                    .ToList());
                    break;

                case SystemLogType.Report:
                    baseListItem.Clear();
                    baseListItem.AddRange(actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.EventExport) ||
                        c.Value == Convert.ToString((short)ActionLogType.SystemLogExport) ||
                        //c.Value == Convert.ToString((short)ActionLogType.AccessibleDoorExport) ||
                        //c.Value == Convert.ToString((short)ActionLogType.RegisteredUserExport) ||
                        c.Value == Convert.ToString((short)ActionLogType.AnalysisExport))
                    .ToList());
                    break;

                case SystemLogType.RegisteredUser:
                    baseListItem.Clear();
                    baseListItem.AddRange(actionListItem.Where(c =>
                        c.Value == Convert.ToString((short)ActionLogType.AssignUser) ||
                        c.Value == Convert.ToString((short)ActionLogType.UnassignUser) ||
                        c.Value == Convert.ToString((short)ActionLogType.RegisteredUserExport))
                    .ToList());
                    break;
                
                case SystemLogType.VisitManagement:
                    break;
            }
                baseListItem.Insert(0, new SelectListItem
                {
                    Text = CommonResource.lblDefaultSelectListItem,
                    Value = string.Empty
                });
                return new SystemLogAction { Data = baseListItem };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetActionListItems");
                return new SystemLogAction { Data = new List<SelectListItem>() };
            }
        }

        /// <summary>
        /// Save data into Db
        /// </summary>
        public void Save()
        {
            try
            {
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Save");
            }
        }

        public List<SystemLog> GetSystemLogs_All()
        {
            return _unitOfWork.SystemLogRepository.GetAll().ToList();
        }

        /// <summary>
        /// Get user account
        /// </summary>
        /// <param name="accId"></param>
        /// <returns></returns>
        public string GetUserAccount(int accId)
        {
            try
            {
                var userAccount = "";
                using (var unitOfWork = DbHelper.CreateUnitOfWork(_configuration))
                {
                    //var account = unitOfWork.AccountRepository.GetByIdAndCompanyId(_httpContext.User.GetCompanyId(), accId);
                    var account = unitOfWork.AccountRepository.GetById(accId);

                    if (account != null)
                    {
                        if (account.Type != (short)AccountType.SystemAdmin)
                        {
                            if (account.CompanyId == _httpContext.User.GetCompanyId())
                            {
                                userAccount = account.Username;
                            }
                        }
                        else
                        {
                            userAccount = account.Username;
                        }
                    }

                    unitOfWork.Dispose();
                }

                return userAccount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserAccount");
                return string.Empty;
            }
        }

        /// <summary>   Query if 'str2' is contain in 'str1'. </summary>
        ///
        /// <remarks>   Edward, 2020-01-31. </remarks>
        ///
        /// <param name="str1"> The 1. </param>
        /// <param name="str2"> The 2. </param>
        ///
        /// <returns>   True if contain, false if not. </returns>

        public Boolean IsContain(string str1, string str2)
        {
            if (!string.IsNullOrEmpty(str1) && !string.IsNullOrEmpty(str2))
            {
                str1 = str1.ToLower();
                str2 = str2.ToLower();

                if (str1.Contains(str2))
                    return true;
                else
                    return false;
            }
            else
            {
                return false;
            }
        }
    }
}