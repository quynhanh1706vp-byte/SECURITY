using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using AutoMapper;
using Bogus;
using Bogus.Extensions;
using ClosedXML.Excel;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Category;
using DeMasterProCloud.DataModel.Dashboard;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.EventLog;
using DeMasterProCloud.DataModel.Header;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.DataModel.Vehicle;
using DeMasterProCloud.DataModel.Visit;
using DeMasterProCloud.DataModel.WorkingModel;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Service.Infrastructure.Header;
using DeMasterProCloud.Service.Protocol;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using CardType = DeMasterProCloud.Common.Infrastructure.CardType;
using Company = DeMasterProCloud.DataAccess.Models.Company;

namespace DeMasterProCloud.Service
{
    /// <summary>
    /// EventLog service interface
    /// </summary>
    public interface IEventLogService
    {
        List<EventLogListModel> GetPaginated(EventLogFilterModel filter, out int totalRecords, out int recordsFiltered);
        IQueryable<EventLog> GetVehicleNormalAccessByDay(int companyId, DateTime dateTime);
        
        List<EventLogReportListModel> GetPaginatedEventLogReport(EventLogFilterModel filter, out int totalRecords, out int recordsFiltered);

        IQueryable<EventLog> GetPaginatedOpenEventLogReport(int userId, out int totalRecords, out int recordsFiltered);

        bool HasData(int? companyId);

        byte[] Export(string type, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, DateTime from, DateTime to, List<int> eventType, string userName, List<int> workType, List<int> inOutType, string cardId, string search, List<int> doorIds,
            List<int> buildingIds, List<int> cardTypes, List<int> departmentIds, List<int> verifyModeIds,List<int> objectType, int? company, List<int> isValid);

        byte[] ExportReport(string type, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, DateTime from, DateTime to,
            List<int> eventType, string userName, List<int> workType, List<int> inOutType, string cardId, string search, List<int> doorIds, List<int> objectType,
            List<int> buildingIds, List<int> departmentIds, List<int> cardtype, int? company, List<int> isValid, string culture);

        /// <summary>
        /// Export to excel
        /// </summary>
        /// <returns></returns>
        byte[] ExportExcel(string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, DateTime from, DateTime to, List<int> eventType, string userName, List<int> workType, List<int> inOutType, string cardId, string search, List<int> doorIds,
            List<int> buildingIds, List<int> cardTypes, List<int> departmentIds, List<int> verifyModeIds, List<int> objectType, int? company, List<int> isValid);

        BaseReportModel<EventReportPdfModel> ExportPdf(string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, DateTime from, DateTime to, List<int> eventType, string userName, List<int> inOutType, string cardId, string search, List<int> doorIds,
            List<int> buildingIds, List<int> departmentIds, List<int> verifyModeIds, List<int> objectType, int? company, List<int> isValid);

        /// <summary>
        /// Export to Text file
        /// </summary>
        /// <returns></returns>
        byte[] ExportTxt(string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, DateTime from, DateTime to, List<int> eventType, string userName, List<int> workType, List<int> inOutType, string cardId, string search, List<int> doorIds,
            List<int> buildingIds, List<int> cardTypes, List<int> departmentIds, List<int> verifyModeIds, List<int> objectType, int? company, List<int> isValid);

        EventLogViewModel InitData();
        EventLogReportViewModel InitReportData();

        void SaveSystemLogExport(string fileName, EventLogExportFilterModel filter = null);

        IEnumerable<EventLogListModel> FilterDataWithOrder(DateTime from, DateTime to, List<int> personType, List<int> eventType, string userName, 
            List<int> inOutType, string cardId, string search, List<int> doorIds, List<int> buildingIds, List<int> cardTypes, List<int> departmentIds, 
            List<int> verifyModeIds, List<int> objectType, List<int> vehicleClassificationIds, int? company, List<int> isValid, string sortColumn, string sortDirection, 
            int pageNumber, int pageSize, out int totalRecords, out int recordsFiltered);

        IQueryable<IcuDevice> GetPaginatedRecoveryDevices(string filter, int pageNumber, int pageSize,
            string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        List<RecoveryDeviceModel> GetPaginatedEventRecoveryInquireDevices(List<int> deviceIds, string accessDateFrom,
            string accessDateTo, string accessTimeFrom, string accessTimeTo, string filter, int pageNumber, int pageSize,
            string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);


        //List<EventCountByDeviceModel> EventCountInquiry(List<int> ids, string accessDateFrom,
        //    string accessDateTo, string accessTimeFrom, string accessTimeTo);

        void EventRecovery(List<EventRecoveryProgressModel> models, string accessDateFrom,
            string accessDateTo, string accessTimeFrom, string accessTimeTo);

        int GetCardStatus(string cardId);

        IEnumerable<EventLogHistory> GetAccessHistoryAttendance(int userId, DateTime start, DateTime end, int eventType, 
            string inOut, int cardType, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        IEnumerable<EventLogHistory> GetAccessHistoryByAttendanceId(int attendanceId, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        void CreateEventLogForDesktopApp(string rid, User user, string message, string action);

        void GenerateTestData(int numberOfEvent);

        int GetTotalNormalAccessByDay(int companyId, DateTime day, int accountId);
        int GetTotalNormalPersonAccessByDay(int companyId, DateTime day, int accountId, bool isUser, bool isIn);
        int GetTotalEventLogs(DateTime start, DateTime end);
        int GetEventNormalAccess(DateTime start, DateTime end);
        int GetEventAbNormalAccess(DateTime start, DateTime end);
        int GetEventByTypeAccess(List<int> eventTypes, DateTime start, DateTime end);

        //List<int> GetAccessChartDataByDoor(int deviceId, string inOut, DateTime endTime, bool isVisit = false);
        List<EventChartDataModel> GetAccessChartDataByDoor(IQueryable<IcuDevice> devices, DateTime startTime, bool isVisit = false);
        int GetUniqueUserAccessByDay(int companyId, DateTime day);
        Dictionary<string, int[]> GetWorkingUserCountEveryDepartment(int companyId, DateTime attendanceDate);

        // For Duali Korea
        IQueryable<EventLog> GetAttendenceForDuali(string accessDateFrom, string accessDateTo,
            List<int> userIds, List<int> departmentIds);
        int GetTotalAbnormalAccessByDay(int companyId, DateTime day, int accountId);
        int GetTotalVisitorAccessByDay(int companyId, DateTime day, int accountId);
        EventLog GetById(int id);
        EventLog GetDetailById(int id);
        void Update(EventLog eventLog);
        List<string> GetListFileBackupByCompany(int companyId, string userName, bool self = false);
        string SaveImageToEventLog(IFormFile file);
        string SaveVideoToEventLog(IFormFile file);
        string SaveRecordVideoToEventLog(IFormFile file);
        object GetAccessStatisticsPerson(DateTime from, DateTime to, List<int> inOutType, List<int> buildingIds, List<int> eventTypes, int companyId);
        object GetAccessStatisticsVehicle(DateTime from, DateTime to, List<int> inOutType, List<int> buildingIds, List<int> eventTypes, int companyId);

        IQueryable<EventLog> GetEventLogForADD(int size);

        void EditEventLogForADD(List<int> ids);

        List<VehicleEventLogListModel> GetPaginatedVehicle(DateTime from, DateTime to, List<int> eventType,
            string userName, string plateNumber, string search, List<int> inOutType, List<int> doorIds,
            List<int> buildingIds, List<int> departmentIds, List<int> verifyModeIds, List<int> objectType, List<int> vehicleClassificationIds, int? company, List<int> isValid,
            int pageNumber, int pageSize, string sortColumn, string sortDirection, out int totalRecords, out int recordsFiltered);

        List<HeaderData> GetVehicleEventLogHeaderData();

        VehicleEventLogViewModel InitVehicleReportData();
        byte[] ExportVehicleReport(string type, List<VehicleEventLogListModel> eventLogs, string culture);
        Dictionary<string, object> GetInitByDataTokenMonitoring(EventLogFilterModel filter);
        Dictionary<string, object> GetInitByDataTokenMonitoringToSchool(EventLogFilterModel filter);
        List<EventLogDetailModel> GetEventLogsRelated(EventLogRelatedFilterModel filter, out int recordsTotal, out int recordsFiltered);

        List<EventLogByWorkTypeCount> GetCountByWorkType();

        List<EventLogByWorkTypeCount> GetCountByVisitType();

        int GetCountByFilter(EventLogExportFilterModel filter);

        List<EventLogByWorkType> GetAttendanceStatus(List<int> deviceIds, List<int> eventTypes, List<int> workTypes, List<int> inOut,
            List<int> departmentIds = null, DateTime? lastEventTime = null, string userName = null, string cardId = null, string militaryNumber = null,
            string model = null, string color = null, string plateNumber = null);
        
        bool UploadImageToEventLog(IcuDevice device, UploadImageEventModel model);
        bool UploadImageVisitCheckinToEventLog(IcuDevice device, UploadImageVisitCheckinModel model);

        List<HeaderData> GetEventHeaderSettings(int companyId, int accountId, string pageName);
    }

    public class EventLogService : IEventLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly IWebSocketService _webSocketService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private readonly List<string> _header = new List<string>
        {
            EventLogResource.lblIDX,
            EventLogResource.lblAccessTime,
            EventLogResource.lblUserName,

            EventLogResource.lblWorkType,

            EventLogResource.lblBirthDay,
            EventLogResource.lblUserCode,
            EventLogResource.lblDepartment,
            EventLogResource.lblCardId,
            EventLogResource.lblRID,
            EventLogResource.lblDoorName,
            EventLogResource.lblBuilding,
            EventLogResource.lblInOut,
            EventLogResource.lblEventDetail,
            EventLogResource.lblIssueCount,
            EventLogResource.lblCardStatus,
            EventLogResource.lblCardType,
            EventLogResource.lblBodyTemperature
        };

        public EventLogService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor,
            IWebSocketService webSocketService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration;
            _webSocketService = webSocketService;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<EventLogService>();
            _mapper = MapperInstance.Mapper;
        }

        /// <summary>
        /// Initial data
        /// </summary>
        /// <returns></returns>
        public EventLogViewModel InitData()
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();

                var model =
                    new EventLogViewModel
                    {
                        EventTypeList = EnumHelper.ToEnumList<EventType>(),
                        InOutList = EnumHelper.ToEnumList<Antipass>(),
                        VerifyModeList = EnumHelper.ToEnumList<VerifyMode>()
                    };

                var doors = _unitOfWork.IcuDeviceRepository.GetDoors();
                var buildings = _unitOfWork.BuildingRepository.GetByCompanyId(companyId);
                var departments = _unitOfWork.DepartmentRepository.GetByCompanyId(companyId);
                if (_httpContext.User.GetAccountType() == (short)AccountType.SuperAdmin)
                {
                    model.CompanyItems = _unitOfWork.CompanyRepository.GetCompanies()
                        .Select(c => new SelectListItemModel
                        {
                            Id = c.Id,
                            Name = c.Name
                        }).ToList();
                }
                else
                {
                    doors = doors.Where(m => m.CompanyId == companyId);
                }

                model.DoorList = doors.Select(m => new SelectListItemModel
                {
                    Id = m.Id,
                    Name = m.Name
                }).ToList();

                model.BuildingList = buildings.Select(m => new SelectListItemModel
                {
                    Id = m.Id,
                    Name = m.Name
                }).ToList();

                model.DepartmentList = departments.Select(m => new SelectListItemModel
                {
                    Id = m.Id,
                    Name = m.DepartName
                }).ToList();

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitData");
                return null;
            }
        }


        public EventLogReportViewModel InitReportData()
        {
            try
            {
                int companyId = _httpContext.User.GetCompanyId();
                int accountId = _httpContext.User.GetAccountId();
                short accountType = _httpContext.User.GetAccountType();
                EventLogReportViewModel model = new EventLogReportViewModel();

                // event type
                model.EventTypeList = EnumHelper.ToEnumList<EventType>().OrderBy(m => m.Name);

                // in out type
                model.InOutList = EnumHelper.ToEnumList<Antipass>();

                // card type
                model.CardTypeList = EnumHelper.ToEnumList<CardType>().OrderBy(m => m.Name);

                var account = _unitOfWork.AccountRepository.GetById(accountId);
                if (string.IsNullOrEmpty(account.TimeZone))
                {
                    Building defaultBuilding = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(companyId);
                    account.TimeZone = defaultBuilding.TimeZone;
                    _unitOfWork.Save();
                }

                // date time filter
                DateTime today = DateTime.UtcNow.AddMinutes(Convert.ToDouble(DateTimeHelper.GetUTCtime(account.TimeZone))).Date;
                model.FromDate = today.ConvertDefaultDateTimeToString();
                model.ToDate = today.AddDays(1).AddSeconds(-1).ConvertDefaultDateTimeToString();

                // device list
                var doors = _unitOfWork.IcuDeviceRepository.GetDoors();
                if (accountType == (short)AccountType.SuperAdmin)
                {
                    model.CompanyItems = _unitOfWork.CompanyRepository.GetCompanies()
                        .Select(c => new SelectListItemModel
                        {
                            Id = c.Id,
                            Name = c.Name
                        }).ToList();
                }
                else
                {
                    doors = doors.Where(m => m.CompanyId == companyId);
                }

                model.DoorList = doors.Select(m => new SelectListItemModel
                {
                    Id = m.Id,
                    Name = m.Name
                }).ToList();

                // building
                model.BuildingList = _unitOfWork.BuildingRepository.GetByCompanyId(companyId)
                    .Select(m => new SelectListItemModel
                    {
                        Id = m.Id,
                        Name = m.Name
                    }).OrderBy(m => m.Name).ToList();

                // department
                model.DepartmentList = _unitOfWork.DepartmentRepository.GetByCompanyId(companyId)
                    .Select(m => new SelectListItemModel
                    {
                        Id = m.Id,
                        Name = m.DepartName
                    }).OrderBy(m => m.Name).ToList();

                List<EnumModel> workTypeList;
                string keyValue = "";
                workTypeList = EnumHelper.ToEnumList<WorkType>();
                keyValue = "work_type_default";

                var setting = _unitOfWork.SettingRepository.GetByKey(keyValue, companyId);
                if (setting != null)
                {
                    var values = JsonConvert.DeserializeObject<List<string>>(setting.Value).Select(int.Parse).ToList();
                    model.WorkTypeList = workTypeList.Where(d => values.Contains(d.Id));
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitReportData");
                return null;
            }
        }

        public List<EventLogListModel> GetPaginated(EventLogFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            var data = _unitOfWork.EventLogRepository.Gets(m => m.CompanyId == filter.CompanyId)
                .Include(m => m.User)
                .Include(m => m.User.Department)
                .Include(m => m.Icu)
                .Include(m => m.Icu.Building)
                .Include(m => m.Company)
                .Include(m => m.Visit).AsQueryable();
            
            // check permission account type dynamic role department level
            if (filter.AccountType == (short)AccountType.DynamicRole)
            {
                var roleDepartmentIds = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(filter.CompanyId, filter.AccountId);
                if (roleDepartmentIds.Count != 0)
                {
                    data = data.Where(d => roleDepartmentIds.Contains(d.User.DepartmentId));
                }
            }
            totalRecords = data.Count();
            
            // filter
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var normalizedSearch = filter.Search.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(m => (m.UserName?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) == true)
                                       || (m.CardId?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) == true)).AsQueryable();
            }
            if (filter.From != null && filter.From != DateTime.MinValue)
            {
                data = data.Where(m => filter.From <= m.EventTime);
            }
            if (filter.To != null && filter.To != DateTime.MinValue)
            {
                data = data.Where(m => m.EventTime <= filter.To);
            }
            if (filter.PersonTypes != null && filter.PersonTypes.Count != 0)
            {
                List<string> personTypeText = new List<string>();
                filter.PersonTypes.ForEach(m => personTypeText.Add(m.ToString()));
                data = data.Where(m => (m.UserId.HasValue && m.User.WorkType.HasValue && filter.PersonTypes.Contains(m.User.WorkType.Value))
                                       || (m.VisitId.HasValue && personTypeText.Contains(m.Visit.VisitType)));
            }
            if (filter.EventTypes != null && filter.EventTypes.Count != 0)
            {
                data = data.Where(m => filter.EventTypes.Contains(m.EventType));
            }
            if (filter.InOutTypes != null && filter.InOutTypes.Count != 0)
            {
                List<string> inOutTypeDescription = new List<string>();
                filter.InOutTypes.ForEach(m => inOutTypeDescription.Add(((Antipass)Convert.ToInt32(m)).ToString().ToLower()));
                data = data.Where(m => inOutTypeDescription.Contains(m.Antipass.ToLower()));
            }
            if (filter.IsValid != null && filter.IsValid.Count != 0)
            {
                data = data.Where(m => filter.IsValid.Contains(m.User.Status));
            }
            if (!string.IsNullOrEmpty(filter.CardId))
            {
                data = data.Where(m => !string.IsNullOrEmpty(m.CardId) && m.CardId.ToLower().Contains(filter.CardId.ToLower()));
            }
            if (filter.DoorIds != null && filter.DoorIds.Count != 0)
            {
                data = data.Where(m => filter.DoorIds.Contains(m.IcuId));
            }
            if (filter.BuildingIds != null && filter.BuildingIds.Count != 0)
            {
                data = data.Where(m => m.Icu.BuildingId.HasValue && filter.BuildingIds.Contains(m.Icu.BuildingId.Value));
            }
            if (filter.VerifyModeIds != null && filter.VerifyModeIds.Count != 0)
            {
                data = data.Where(m => filter.VerifyModeIds.Contains(m.Icu.VerifyMode));
            }
            if (filter.ObjectType != null && filter.ObjectType.Count != 0)
            {
                data = data.Where(m =>
                    (filter.ObjectType.Contains((int)ObjectTypeEvent.User) && m.UserId != null) ||
                    (filter.ObjectType.Contains((int)ObjectTypeEvent.Visit) && m.VisitId != null) ||
                    (filter.ObjectType.Contains((int)ObjectTypeEvent.Warning) && m.UserId == null && m.VisitId == null)
                );
            }
            if (filter.CardTypes != null && filter.CardTypes.Count != 0)
            {
                data = data.Where(m => filter.CardTypes.Contains(m.CardType));
            }
            if (filter.DepartmentIds != null && filter.DepartmentIds.Count != 0)
            {
                data = data.Where(m => filter.DepartmentIds.Contains(m.User.Department.Id) || filter.DepartmentIds.Contains(m.Visit.VisiteeDepartmentId.Value));
            }
            if (filter.VehicleClassificationIds != null && filter.VehicleClassificationIds.Count != 0)
            {
                data = data.Where(m => (m.CardType == (short)CardType.VehicleId || m.CardType == (short)CardType.VehicleMotoBikeId) &&
                                       _unitOfWork.AppDbContext.Vehicle.Any(n => !n.IsDeleted &&
                                           m.CardId.ToLower() == n.PlateNumber.ToLower() && n.CompanyId == m.CompanyId));
            }

            recordsFiltered = data.Count();
            data = data.OrderBy($"{filter.SortColumn} {filter.SortDirection}");
            if (filter.PageSize > 0)
            {
                data = data.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);
            }
            
            List<EventLogListModel> result = new List<EventLogListModel>();
            int idx = (filter.PageNumber - 1) * filter.PageSize + 1;
            var listData = data.Select(_mapper.Map<EventLogListModel>);
            foreach (var eventLog in listData)
            {
                eventLog.Id = idx;
                eventLog.IsRegisteredCard = (eventLog.EventType == (short)EventType.UnRegisteredID
                                             && !string.IsNullOrEmpty(eventLog.CardId)
                                             && !_unitOfWork.CardRepository.IsCardIdExist(filter.CompanyId, eventLog.CardId))
                                            && filter.AccountType != (short)AccountType.SuperAdmin;

                if (string.IsNullOrEmpty(eventLog.Avatar) && !string.IsNullOrEmpty(eventLog.UserName))
                {
                    eventLog.Avatar = Constants.Image.DefaultMale;
                }
                
                if ((eventLog.CardTypeId == (int)CardType.VehicleId || eventLog.CardTypeId == (int)CardType.VehicleMotoBikeId) 
                    && !string.IsNullOrEmpty(eventLog.ImageCamera))
                {
                    var vehicleImage = JsonConvert.DeserializeObject<List<DataImageCamera>>(eventLog.ImageCamera);
                    eventLog.Avatar = vehicleImage.FirstOrDefault()?.Link ?? eventLog.Avatar;
                }
                
                result.Add(eventLog);
                idx++;
            }
            
            return result;
        }

        public IQueryable<EventLog> GetVehicleNormalAccessByDay(int companyId, DateTime dateTime)
        {
            var accountId = _httpContext.User.GetAccountId();
            DateTime start = dateTime.Date;
            DateTime end = dateTime.Date.AddDays(1);
            if (accountId != 0)
            {
                string timezone = _unitOfWork.AccountRepository.GetById(accountId)?.TimeZone;
                var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;
                start = start.ConvertToSystemTime(offSet);
                end = end.ConvertToSystemTime(offSet);
            }

            var data = _unitOfWork.EventLogRepository.Gets(m => m.CompanyId == companyId
                                                                && m.EventType == (int)EventType.NormalAccess
                                                                && m.EventTime > start && m.EventTime <= end
                                                                && (m.CardType == (int)CardType.VehicleId || m.CardType == (int)CardType.VehicleMotoBikeId));
            return data;
        }

        public List<EventLogReportListModel> GetPaginatedEventLogReport(EventLogFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            var data = _unitOfWork.EventLogRepository
                .Gets(m => m.CompanyId == filter.CompanyId && !m.IsVisit)
                .Include(m => m.User)
                .Include(m => m.User.Department)
                .Include(m => m.Icu)
                .Include(m => m.Icu.Building)
                .Include(m => m.Company)
                .Include(m => m.Visit).AsQueryable();
            
            // check permission account type dynamic role department level
            if (filter.AccountType == (short)AccountType.DynamicRole)
            {
                var roleDepartmentIds = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(filter.CompanyId, filter.AccountId);
                if (roleDepartmentIds.Count != 0)
                {
                    data = data.Where(d => roleDepartmentIds.Contains(d.User.DepartmentId));
                }
            }
            // check whether user can see all data or not
            RoleService roleService = new RoleService(_unitOfWork, new HttpContextAccessor(), null, _configuration, null);
            bool isViewOnlyMe = roleService.CheckPermissionEnabled(ActionName.View + Page.Report, filter.AccountId, filter.CompanyId);
            bool isViewAll = roleService.CheckPermissionEnabled(ActionName.ViewAll + Page.Report, filter.AccountId, filter.CompanyId);
            if (isViewOnlyMe && !isViewAll)
            {
                data = data.Where(m => m.User.AccountId == filter.AccountId);
            }
            totalRecords = data.Count();
            
            // filter
            if (filter.From != null && filter.From != DateTime.MinValue)
            {
                data = data.Where(m => filter.From <= m.EventTime);
            }
            if (filter.To != null && filter.To != DateTime.MinValue)
            {
                data = data.Where(m => m.EventTime <= filter.To);
            }
            if (filter.DoorIds != null && filter.DoorIds.Count != 0)
            {
                data = data.Where(m => filter.DoorIds.Contains(m.IcuId));
            }
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var normalizedSearch = filter.Search.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(m => (m.UserName?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) == true) ||
                                       (m.CardId?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) == true)).AsQueryable();
            }
            if (!string.IsNullOrEmpty(filter.CardId))
            {
                data = data.Where(m => m.CardId.Contains(filter.CardId));
            }
            if (filter.DepartmentIds != null && filter.DepartmentIds.Count != 0)
            {
                data = data.Where(m => filter.DepartmentIds.Contains(m.User.DepartmentId));
            }
            if (filter.InOutIds != null && filter.InOutIds.Count != 0)
            {
                List<string> inOutTypeDescription = new List<string>();
                foreach (var type in filter.InOutIds)
                {
                    inOutTypeDescription.Add(Enum.GetName(typeof(Antipass), type)?.ToLower());
                }
                data = data.Where(m => inOutTypeDescription.Count == 0 || inOutTypeDescription.Contains(m.Antipass.ToLower()));
            }
            if (filter.WorkTypeIds != null && filter.WorkTypeIds.Count != 0)
            {
                data = data.Where(m => m.User != null && filter.WorkTypeIds.Contains((int)m.User.WorkType));
            }
            if (filter.EventTypes != null && filter.EventTypes.Count != 0)
            {
                data = data.Where(m => filter.EventTypes.Contains(m.EventType));
            }
            if (filter.BuildingIds != null && filter.BuildingIds.Count != 0)
            {
                data = data.Where(m => m.Icu.BuildingId.HasValue && filter.BuildingIds.Contains(m.Icu.BuildingId.Value));
            }
            if (filter.CardTypes != null && filter.CardTypes.Count != 0)
            {
                data = data.Where(m => filter.CardTypes.Contains(m.CardType));
            }
            if (filter.IsValid != null && filter.IsValid.Count != 0)
            {
                data = data.Where(m => filter.IsValid.Contains(m.User.Status));
            }
            
            recordsFiltered = data.Count();
            data = data.OrderBy($"{filter.SortColumn} {filter.SortDirection}");
            if (filter.PageNumber > 0 && filter.PageSize > 0)
                data = data.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);

            List<EventLogReportListModel> result = new List<EventLogReportListModel>();
            int idx = (filter.PageNumber - 1) * filter.PageSize + 1;
            
            var listData = data.Select(_mapper.Map<EventLogReportListModel>);
            foreach (var eventLog in listData)
            {
                eventLog.Id = idx;
                if (!string.IsNullOrEmpty(eventLog.ImageCamera) && eventLog.ImageCamera.Contains("\"Link\""))
                {
                    List<DataImageCamera> imageCameras = JsonConvert.DeserializeObject<List<DataImageCamera>>(eventLog.ImageCamera);
                    var cameraEventCheckInModels = imageCameras.Select(_mapper.Map<CameraEventCheckInModel>);
                    eventLog.ImageCamera = JsonConvert.SerializeObject(cameraEventCheckInModels);
                }
                
                result.Add(eventLog);
                idx++;
            }
            
            return result;
        }


        public IQueryable<EventLog> GetPaginatedOpenEventLogReport(int userId, out int totalRecords,
            out int recordsFiltered)
        {
            var data = _unitOfWork.AppDbContext.EventLog
                .Include(m => m.User)
                .Include(m => m.User.Department)
                .Include(m => m.Icu)
                .Include(m => m.Icu.Building)
                .Include(m => m.Company)
                .Include(m => m.Visit)
                .AsQueryable();


            data = data.Where(m => m.CompanyId == _httpContext.User.GetCompanyId());

            var cardList = _unitOfWork.CardRepository.GetByUserId(_httpContext.User.GetCompanyId(), userId);

            totalRecords = data.Count();

            data = data.Where(m =>
                cardList.Contains(_unitOfWork.CardRepository.GetByCardId(_httpContext.User.GetCompanyId(), m.CardId)));

            recordsFiltered = data.Count();
            return data;
        }


        /// <summary>
        /// Filter data
        /// </summary>
        /// <param name="isValid"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="personType"></param>
        /// <param name="eventType"></param>
        /// <param name="userName"></param>
        /// <param name="inOutType"></param>
        /// <param name="cardId"></param>
        /// <param name="search"></param>
        /// <param name="verifyModeIds"></param>
        /// <param name="vehicleClassificationIds"></param>
        /// <param name="company"></param>
        /// <param name="doorIds"></param>
        /// <param name="buildingIds"></param>
        /// <param name="cardTypes"></param>
        /// <param name="departmentIds"></param>
        /// <returns></returns>
        public IEnumerable<EventLogListModel> FilterDataWithOrder(DateTime from, DateTime to,
            List<int> personType, List<int> eventType, string userName, List<int> inOutType, string cardId, string search, List<int> doorIds,
            List<int> buildingIds, List<int> cardTypes, List<int> departmentIds, List<int> verifyModeIds,List<int> objectType, List<int> vehicleClassificationIds, int? company, List<int> isValid,
            string sortColumn, string sortDirection,
            int pageNumber, int pageSize,
            out int totalRecords,
            out int recordsFiltered)
        {
            int companyId = company ?? _httpContext.User.GetCompanyId();

            var account = _unitOfWork.AppDbContext.Account.FirstOrDefault(m => m.Id == _httpContext.User.GetAccountId());

            var data = _unitOfWork.AppDbContext.EventLog
                .Include(m => m.User)
                .Include(m => m.User.Department)
                .Include(m => m.Icu)
                .Include(m => m.Icu.Building)
                .Include(m => m.Company)
                .Include(m => m.Visit)
                .Where(m => m.CompanyId == companyId)
                .Where(m => m.EventTime >= from)
                .Where(m => m.EventTime <= to);

            // check permission account type dynamic role department level
            if (_httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
            {
                var roleDepartmentIds = _unitOfWork.DepartmentRepository
                    .GetDepartmentIdsByAccountDepartmentRole(_httpContext.User.GetCompanyId(),
                        _httpContext.User.GetAccountId());
                if (roleDepartmentIds.Count != 0)
                {
                    data = data.Where(d => roleDepartmentIds.Contains(d.User.DepartmentId));
                }
            }

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = search.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(m => (m.CardId?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) == true) || (m.UserName?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) == true)).AsQueryable();
            }
            if (personType != null && personType.Count != 0)
            {
                List<string> personTypeText = new List<string>();
                personType.ForEach(m => personTypeText.Add(m.ToString()));
                data = data.Where(m => (m.UserId.HasValue && m.User.WorkType.HasValue && personType.Contains(m.User.WorkType.Value))
                                    || (m.VisitId.HasValue && personTypeText.Contains(m.Visit.VisitType)));
            }

            if (eventType != null && eventType.Count != 0)
            {
                data = data.Where(m => eventType.Contains(m.EventType));
            }

            if (!string.IsNullOrEmpty(userName))
            {
                data = data.Where(m => m.UserName.ToLower().Contains(userName.ToLower()));
            }

            if (inOutType != null && inOutType.Count != 0)
            {
                List<string> inOutTypeDescription = new List<string>();

                foreach (var type in inOutType)
                    inOutTypeDescription.Add(((Antipass)Convert.ToInt32(type)).ToString().ToLower());

                data = data.Where(m => inOutTypeDescription.Contains(m.Antipass.ToLower()));
            }

            if (isValid != null && isValid.Count != 0)
            {
                data = data.Where(m => isValid.Contains(m.User.Status));
            }

            if (!string.IsNullOrEmpty(cardId))
                data = data.Where(m => m.CardId.Contains(cardId));

            if (doorIds != null && doorIds.Count != 0)
                data = data.Where(m => doorIds.Contains(m.IcuId));

            if (buildingIds != null && buildingIds.Count != 0)
                data = data.Where(m => buildingIds.Contains(m.Icu.Building.Id));

            if (verifyModeIds != null && verifyModeIds.Count != 0)
            {
                data = data.Where(m => verifyModeIds.Contains(m.Icu.VerifyMode));
            }
            if (objectType != null && objectType.Count != 0)
            {
                data = data.Where(m =>
                    (objectType.Contains((int)ObjectTypeEvent.User) && m.UserId != null) ||
                    (objectType.Contains((int)ObjectTypeEvent.Visit) && m.VisitId != null) ||
                    (objectType.Contains((int)ObjectTypeEvent.Warning) && m.UserId == null && m.VisitId == null)
                );
            }

            if (cardTypes != null && cardTypes.Count != 0)
            {
                data = data.Where(m => cardTypes.Contains(m.CardType));
            }

            if (departmentIds != null && departmentIds.Count != 0)
            {
                data = data.Where(m => departmentIds.Contains(m.User.Department.Id));
            }

            if (vehicleClassificationIds != null && vehicleClassificationIds.Count != 0)
            {
                data = data.Where(m => (m.CardType == (short)CardType.VehicleId || m.CardType == (short)CardType.VehicleMotoBikeId) &&
                                       _unitOfWork.AppDbContext.Vehicle.Any(n => !n.IsDeleted &&
                                           m.CardId.ToLower() == n.PlateNumber.ToLower() && 
                                           n.CompanyId == m.CompanyId));
            }

            //data = FilterData(accessDateFrom, accessDateTo, accessTimeFrom, accessTimeTo, eventType, userName,
            //    inOutType, cardId, doorIds, building, department, verifymode, data);

            recordsFiltered = data.Count();
            sortColumn = char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);
            var nameColumn = typeof(EventLog).GetProperty(sortColumn);
            if (nameColumn != null)
            {
                data = data.OrderBy($"{sortColumn} {sortDirection}");
            }
            else
            {
                data = data.OrderBy($"EventTime {sortDirection}");
            }

            if (pageSize > 0)
            {
                data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            var accountLanguage = "en-US";
            if (account != null)
                accountLanguage = account.Language;

            var listData = _unitOfWork.EventLogRepository
                .GetPaginatedEventLog(data.ToList(), accountLanguage).ToList();

            foreach (var eachData in listData)
            {
                if (!string.IsNullOrEmpty(eachData.InOut) && !string.IsNullOrEmpty(eachData.CardId) && string.IsNullOrEmpty(eachData.UserName))
                {
                    var card = _unitOfWork.AppDbContext.Card.FirstOrDefault(m => m.CardId.ToLower().Equals(eachData.CardId.ToLower()) && !m.IsDeleted);

                    if (card != null && card.UserId != null)
                    {
                        var user = _unitOfWork.AppDbContext.User
                            .Include(m => m.Department).FirstOrDefault(m => m.Id == card.UserId && m.CompanyId == companyId);

                        if (user != null)
                        {
                            eachData.UserName = (user.FirstName + " " + user.LastName).Trim();
                            //eachData.UserType = user.WorkType;
                            eachData.UserType = (int)UserType.Normal;
                            eachData.Type = user.WorkType.Value;
                            eachData.UserId = user.Id;

                            eachData.Department = user.Department.DepartName;
                            eachData.DepartmentName = user.Department.DepartName;
                        }
                    }
                    else if (card != null && card.VisitId != null)
                    {
                        var visit = _unitOfWork.AppDbContext.Visit.FirstOrDefault(m => m.Id == card.VisitId);

                        if (visit != null)
                        {
                            eachData.UserName = visit.VisitorName;
                            eachData.VisitId = visit.Id;
                            eachData.UserType = (int)UserType.Visit;
                            eachData.Type = (short)Int32.Parse(visit.VisitType);

                            eachData.Department = visit.VisiteeDepartment;
                            eachData.DepartmentName = visit.VisiteeDepartment;
                        }
                    }
                }

                if (string.IsNullOrEmpty(eachData.Avatar))
                {
                    if (!string.IsNullOrEmpty(eachData.UserName))
                    {
                        var user = _unitOfWork.UserRepository.GetUsersByFirstName(eachData.UserName)
                            .FirstOrDefault(m => m.CompanyId == companyId);

                        eachData.Avatar = user?.Avatar ?? Constants.Image.DefaultMale;
                    }
                }
            }

            return listData;
        }
        
        public IEnumerable<EventLogReportListModel> FilterDataWithOrderEventLogReport(DateTime from, DateTime to,
            List<int> doorIds, List<int> objectType, string userName,
            string cardId, string search, List<int> departmentIds, List<int> workType, List<int> inOutType, List<int> eventType, List<int> buildingList,
            List<int> cardType, string culture, List<int> isValid, string sortColumn, string sortDirection,
            int pageNumber, int pageSize,
            out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var account = _unitOfWork.AppDbContext.Account.FirstOrDefault(m => m.Id == _httpContext.User.GetAccountId());
            TimeZoneInfo cstZone = account.TimeZone.ToTimeZoneInfo();
            TimeSpan offSet = cstZone.BaseUtcOffset;

            var data = _unitOfWork.AppDbContext.EventLog
                .Include(m => m.User)
                .Include(m => m.User.Department)
                .Include(m => m.Icu)
                .Include(m => m.Icu.Building)
                .Include(m => m.Company)
                .Include(m => m.Visit)
                .Where(m => m.CompanyId == companyId)
                .Where(m => !m.IsVisit)
                .Where(m => m.EventTime >= from)
                .Where(m => m.EventTime <= to);

            // check permission account type dynamic role department level
            if (_httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
            {
                var roleDepartmentIds = _unitOfWork.DepartmentRepository
                    .GetDepartmentIdsByAccountDepartmentRole(_httpContext.User.GetCompanyId(),
                        _httpContext.User.GetAccountId());
                if (roleDepartmentIds.Count != 0)
                {
                    data = data.Where(d => roleDepartmentIds.Contains(d.User.DepartmentId));
                }
            }

            // check whether user can see all data or not
            RoleService roleService = new RoleService(_unitOfWork, new HttpContextAccessor(), null, _configuration, null);
            bool isViewOnlyMe = roleService.CheckPermissionEnabled(ActionName.View + Page.Report, account.Id, companyId);
            bool isViewAll = roleService.CheckPermissionEnabled(ActionName.ViewAll + Page.Report, account.Id, companyId);

            if (isViewOnlyMe == true && isViewAll == false)
            {
                data = data.Where(m => m.User.AccountId == account.Id);
            }

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = search.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(m => (m.UserName?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) == true) || (m.CardId?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) == true)).AsQueryable();
            }
            else
            {

                if (doorIds != null && doorIds.Count != 0)
                {
                    data = data.Where(m => doorIds.Contains(m.IcuId));
                }
                else
                {
                    doorIds = _unitOfWork.IcuDeviceRepository.GetByCompany(companyId).Select(m => m.Id).ToList();
                }
                if (objectType != null && objectType.Count != 0)
                {
                    data = data.Where(m =>
                        (objectType.Contains((int)ObjectTypeEvent.User) && m.UserId != null) ||
                        (objectType.Contains((int)ObjectTypeEvent.Visit) && m.VisitId != null) ||
                        (objectType.Contains((int)ObjectTypeEvent.Warning) && m.UserId == null && m.VisitId == null)
                    );
                }
                if (!string.IsNullOrEmpty(userName))
                {
                    data = data.Where(m => m.UserName.ToLower().Contains(userName.ToLower()));
                }

                if (!string.IsNullOrEmpty(cardId))
                {
                    data = data.Where(m => m.CardId.Contains(cardId));
                }

                if (departmentIds != null && departmentIds.Count != 0)
                {
                    data = data.Where(m => departmentIds.Contains(m.User.DepartmentId));
                }

                // [TEMP]: search by string In, Out
                if (inOutType != null && inOutType.Count != 0)
                {
                    List<string> inOutTypeDescription = new List<string>();
                    
                    foreach (var type in inOutType)
                        inOutTypeDescription.Add(((Antipass)Convert.ToInt32(type)).ToString().ToLower());

                    data = data.Where(m => inOutTypeDescription.Contains(m.Antipass.ToLower()));
                }

                if (workType != null && workType.Count != 0)
                {
                    data = data.Where(m => m.User != null && workType.Contains((int) m.User.WorkType));
                }

                if (eventType != null && eventType.Count != 0)
                {
                    data = data.Where(m => eventType.Contains(m.EventType));
                }

                if (buildingList != null && buildingList.Count != 0)
                {
                    data = data.Where(m => buildingList.Contains(m.Icu.Building.Id));
                }

                if (cardType != null && cardType.Count != 0)
                {
                    data = data.Where(m => cardType.Contains(m.CardType));
                }

                if (isValid != null && isValid.Count != 0)
                {
                    data = data.Where(m => isValid.Contains(m.User.Status));
                }
            }


            recordsFiltered = data.Count();
            if (data != null && data.Any())
            {
                data = data.OrderBy($"{sortColumn} {sortDirection}");

                if (pageNumber > 0 && pageSize > 0)
                    data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            var listData = _unitOfWork.EventLogRepository
                .GetPaginatedEventLogReport(data.ToList(), culture, offSet)
                .ToList();

            return listData;
        }

        /// <summary>
        /// Check if there is any data in eventlog table
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
                        return _unitOfWork.AppDbContext.EventLog.Any(m => m.CompanyId == companyId);
                    }
                    else
                    {
                        return _unitOfWork.AppDbContext.EventLog.Any();
                    }
                }

                return _unitOfWork.AppDbContext.EventLog.Any(
                    m => m.CompanyId == _httpContext.User.GetCompanyId());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HasData");
                return false;
            }
        }

        /// <summary>
        /// Export data in excel or txt format
        /// </summary>
        /// <param name="type"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="eventType"></param>
        /// <param name="userName"></param>
        /// <param name="inOutType"></param>
        /// <param name="cardId"></param>
        /// <param name="search"></param>
        /// <param name="verifyModeIds"></param>
        /// <param name="objectType"></param>
        /// <param name="company"></param>
        /// <param name="doorIds"></param>
        /// <param name="buildingIds"></param>
        /// <param name="cardTypes"></param>
        /// <param name="departmentIds"></param>
        /// <param name="isValid"></param>
        /// <returns></returns>
        public byte[] Export(string type, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, DateTime from, DateTime to,
            List<int> eventType, string userName, List<int> workType, List<int> inOutType, string cardId, string search, List<int> doorIds,
            List<int> buildingIds, List<int> cardTypes, List<int> departmentIds, List<int> verifyModeIds,List<int> objectType, int? company, List<int> isValid)
        {
            try
            {
                return type == Constants.Excel
                    ? ExportExcel(sortColumn, sortDirection, out totalRecords,
                        out recordsFiltered, from, to, eventType, userName, workType, inOutType, cardId, search,
                        doorIds, buildingIds, cardTypes, departmentIds, verifyModeIds, objectType, company, isValid)
                    : ExportTxt(sortColumn, sortDirection, out totalRecords,
                        out recordsFiltered, from, to, eventType, userName, workType, inOutType, cardId, search,
                        doorIds, buildingIds, cardTypes, departmentIds, verifyModeIds, objectType, company, isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Export");
                totalRecords = 0;
                recordsFiltered = 0;
                return Array.Empty<byte>();
            }
        }

        public byte[] ExportReport(string type, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, DateTime from, DateTime to,
            List<int> eventType, string userName, List<int> workType, List<int> inOutType, string cardId, string search, List<int> doorIds, List<int> objectType,
            List<int> building, List<int> departmentIds, List<int> cardtype, int? company, List<int> isValid, string culture)
        {
            try
            {
                byte[] result = Array.Empty<byte>();

                switch (type.ToLower())
                {
                    case Constants.Excel:
                        result = ExportExcelReport(sortColumn, sortDirection, out totalRecords,
                        out recordsFiltered, from, to, eventType, userName, workType, inOutType, cardId, search,
                        doorIds, objectType, building, departmentIds, cardtype, company, isValid, culture);
                        break;
                    case "hancell":
                        result = ExportHancellReport(sortColumn, sortDirection, out totalRecords,
                        out recordsFiltered, from, to, eventType, userName, workType, inOutType, cardId, search,
                        doorIds, objectType, building, departmentIds, cardtype, company, isValid, culture);
                        break;
                    default:
                        result = ExportTxtReport(sortColumn, sortDirection, out totalRecords,
                        out recordsFiltered, from, to, eventType, userName, workType, inOutType, cardId, search,
                        doorIds,  objectType,building, departmentIds, cardtype, company, isValid, culture);
                        break;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExportReport");
                totalRecords = 0;
                recordsFiltered = 0;
                return Array.Empty<byte>();
            }
        }

        public byte[] ExportVehicleReport(string type, List<VehicleEventLogListModel> eventLogs, string culture)
        {
            try
            {
                return type == Constants.Excel
                    ? ExportExcelVehicleReport(eventLogs, culture)
                    : ExportTxtVehicleReport(eventLogs, culture);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExportVehicleReport");
                return Array.Empty<byte>();
            }
        }

        /// <summary>
        /// Export data in excel
        /// </summary>
        /// 
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="eventType"></param>
        /// <param name="userName"></param>
        /// <param name="inOutType"></param>
        /// <param name="cardId"></param>
        /// <param name="cardTypes"></param>
        /// <param name="departmentIds"></param>
        /// <param name="verifyModeIds"></param>
        /// <param name="objectType"></param>
        /// <param name="company"></param>
        /// <param name="doorIds"></param>
        /// <param name="buildingIds"></param>
        /// <param name="isValid"></param>
        /// <returns></returns>
        public byte[] ExportExcel(string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, DateTime from, DateTime to, List<int> eventType, string userName, List<int> workType, List<int> inOutType, string cardId, string search, List<int> doorIds,
            List<int> buildingIds, List<int> cardTypes, List<int> departmentIds, List<int> verifyModeIds,List<int> objectType, int? company, List<int> isValid)
        {
            try
            {
                byte[] result;

                //var package = new ExcelPackage();
                using (var package = new ExcelPackage())
                {
                    // add a new worksheet to the empty workbook
                    var worksheet =
                        package.Workbook.Worksheets.Add(EventLogResource.lblEventLog); //Worksheet name

                    //First add the headers
                    for (var i = 0; i < _header.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = _header[i];
                    }

                    var data = FilterDataWithOrder(from, to, workType, eventType,
                        userName, inOutType, cardId, search, doorIds, buildingIds, cardTypes, departmentIds, verifyModeIds,objectType, null, company, isValid,
                        sortColumn, sortDirection, pageNumber: 1, pageSize: 0, out totalRecords, out recordsFiltered).ToList();

                    var recordIndex = 2;
                    foreach (var report in data)
                    {
                        var colIndex = 1;

                        worksheet.Cells[recordIndex, colIndex++].Value = report.EventTime;
                        worksheet.Cells[recordIndex, colIndex++].Value = report.UserName;

                        worksheet.Cells[recordIndex, colIndex++].Value = report.WorkTypeName;

                        worksheet.Cells[recordIndex, colIndex++].Value = report.CardId;
                        worksheet.Cells[recordIndex, colIndex++].Value = report.DeviceAddress;
                        worksheet.Cells[recordIndex, colIndex++].Value = report.DoorName;
                        worksheet.Cells[recordIndex, colIndex++].Value = report.Building;
                        worksheet.Cells[recordIndex, colIndex++].Value = report.VerifyMode;
                        worksheet.Cells[recordIndex, colIndex++].Value = report.InOut;
                        worksheet.Cells[recordIndex, colIndex].Value = report.EventDetail;
                        recordIndex++;
                    }

                    result = package.GetAsByteArray();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExportExcel");
                totalRecords = 0;
                recordsFiltered = 0;
                return Array.Empty<byte>();
            }
        }

        private byte[] ExportExcelReport(string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, DateTime from, DateTime to,
            List<int> eventType, string userName, List<int> workType, List<int> inOutType, string cardId, string search, List<int> doorIds,
            List<int> objectType, List<int> buildingList, List<int> departmentIds, List<int> cardType, int? company, List<int> isValid,
            string culture)
        {
            byte[] result;

            //var package = new ExcelPackage();
            using (var package = new ExcelPackage())
            {
                var data = FilterDataWithOrderEventLogReport(from, to, doorIds,objectType, userName,
                        cardId, search, departmentIds, workType, inOutType, eventType, buildingList,
                        cardType, culture, isValid, sortColumn, sortDirection, 0, 0, out totalRecords,
                        out recordsFiltered)
                    .ToList();

                var splitList = data.SplitList(50000);
                int sheetIndex = 0;

                foreach (var dataList in splitList)
                {
                    sheetIndex++;

                    // add a new worksheet to the empty workbook
                    var worksheet =
                        package.Workbook.Worksheets.Add($"{EventLogResource.lblEventLog}_{sheetIndex}"); //Worksheet name

                    //First add the headers
                    for (var i = 0; i < _header.Count; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = _header[i];
                    }

                    worksheet.View.FreezePanes(2, 1);

                    var recordIndex = 2;
                    foreach (var report in dataList)
                    {
                        //var card = _unitOfWork.CardRepository.GetByCardId(_httpContext.User.GetCompanyId(), report.CardId);
                        var colIndex = 1;

                        worksheet.Cells[recordIndex, colIndex++].Value = recordIndex - 1; //idx
                        worksheet.Cells[recordIndex, colIndex++].Value = report.EventTime; //출입시간
                        worksheet.Cells[recordIndex, colIndex++].Value = report.UserName; //이름

                        worksheet.Cells[recordIndex, colIndex++].Value = report.WorkTypeName; // 인원 구분

                        worksheet.Cells[recordIndex, colIndex++].Value = report.BirthDay; //생년월일
                        worksheet.Cells[recordIndex, colIndex++].Value = report.EmployeeNumber; //사원번호
                                                                                                //worksheet.Cells[recordIndex, colIndex++].Value = $"=\"{report.UserCode}\""; //사원번호
                        worksheet.Cells[recordIndex, colIndex++].Value = report.DepartmentName; //소속
                        worksheet.Cells[recordIndex, colIndex++].Value = report.CardId; //카드ID
                                                                                        //worksheet.Cells[recordIndex, colIndex++].Value = $"=\"{report.CardId}\""; //카드ID
                        worksheet.Cells[recordIndex, colIndex++].Value = report.DeviceAddress; //RID
                                                                                               //worksheet.Cells[recordIndex, colIndex++].Value = $"=\"{report.DeviceAddress}\""; //RID
                        worksheet.Cells[recordIndex, colIndex++].Value = report.DoorName; //출입문 이름 
                        worksheet.Cells[recordIndex, colIndex++].Value = report.Building; //장소
                        worksheet.Cells[recordIndex, colIndex++].Value = report.InOut; //입/출
                        worksheet.Cells[recordIndex, colIndex++].Value = report.EventDetail; //이벤트 종류
                        worksheet.Cells[recordIndex, colIndex++].Value = report.IssueCount; //발급차수
                        worksheet.Cells[recordIndex, colIndex++].Value = report.CardStatus; //카드상태
                        worksheet.Cells[recordIndex, colIndex++].Value = report.CardType; //카드구분
                        worksheet.Cells[recordIndex, colIndex++].Value = report.BodyTemperature;

                        recordIndex++;
                    }

                    worksheet.Cells.AutoFitColumns(20);
                }

                result = package.GetAsByteArray();
            }

            return result;
        }

        public byte[] ExportExcelVehicleReport(List<VehicleEventLogListModel> eventLogs, string culture)
        {
            byte[] result;

            //var package = new ExcelPackage();
            using (var package = new ExcelPackage())
            {
                // add a new worksheet to the empty workbook
                var worksheet =
                    package.Workbook.Worksheets.Add(EventLogResource.lblVehicleEventLog); //Worksheet name

                //First add the headers
                //for (var i = 0; i < _header.Count; i++)
                //{
                //    worksheet.Cells[1, i + 1].Value = _header[i];
                //}

                var vehicleEventHeader = GetVehicleEventLogHeaderData().Select(m => m.HeaderName).ToList();

                for (var i = 0; i < vehicleEventHeader.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = _header[i];
                }

                var recordIndex = 2;
                foreach (var report in eventLogs)
                {
                    //var card = _unitOfWork.CardRepository.GetByCardId(_httpContext.User.GetCompanyId(), report.CardId);
                    var colIndex = 1;

                    worksheet.Cells[recordIndex, colIndex++].Value = recordIndex - 1; // idx
                    worksheet.Cells[recordIndex, colIndex++].Value = report.EventTime; // Access time
                    worksheet.Cells[recordIndex, colIndex++].Value = report.DoorName; // Door name
                    worksheet.Cells[recordIndex, colIndex++].Value = report.Model; // Vehicle model
                    worksheet.Cells[recordIndex, colIndex++].Value = report.PlateNumber; // Plate number
                    worksheet.Cells[recordIndex, colIndex++].Value = report.DepartmentName; // Department name
                    worksheet.Cells[recordIndex, colIndex++].Value = report.UserName; // User name
                    //worksheet.Cells[recordIndex, colIndex++].Value = report.CategoryOptions.Where(m => m.Category.Name.Equals("계급")); // User name
                    worksheet.Cells[recordIndex, colIndex++].Value = report.EventDetail; // Event type
                    worksheet.Cells[recordIndex, colIndex++].Value = report.InOut; // In/OUT

                    recordIndex++;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }


        /// <summary>   Export text 2. </summary>
        /// <remarks>   Edward, 2020-03-02. </remarks>
        /// <param name="sortColumn">       column using in sorting. </param>
        /// <param name="sortDirection">    Ascending or Descending. </param>
        /// <param name="totalRecords">     [out] total records count. </param>
        /// <param name="recordsFiltered">  [out] filtered records count. </param>
        /// <param name="accessDateFrom">   Start date. </param>
        /// <param name="accessDateTo">     End date. </param>
        /// <param name="accessTimeFrom">   Start time. </param>
        /// <param name="accessTimeTo">     End time. </param>
        /// <param name="eventType">        Event type. </param>
        /// <param name="userName">         User name. </param>
        /// <param name="inOutType">        In or Out. </param>
        /// <param name="cardId">           Card Id. </param>
        /// <param name="doorIds">          Door Id list. </param>
        /// <param name="buildingList">     List of buildings. </param>
        /// <param name="department">       Name of department. </param>
        /// <param name="CardType">         Type of the card. </param>
        /// <param name="company">          Company Id. </param>
        /// <returns>   A byte[]. </returns>
        public byte[] ExportTxtVehicleReport(List<VehicleEventLogListModel> eventLogs, string culture)
        {
            var reportTxt = new StringBuilder();
            int idx = 1;
            // Build the file content
            foreach (var report in eventLogs)
            {
                var obj = new List<object>
                {
                    idx++,

                    report.EventTime,
                    report.DoorName,
                    report.Model,
                    report.PlateNumber,
                    report.DepartmentName,
                    report.UserName,
                    report.EventDetail,
                    report.InOut
                };

                reportTxt.AppendLine(string.Join(",", obj));
            }

            byte[] buffer = Encoding.UTF8.GetBytes($"{string.Join(",", _header)}\r\n{reportTxt}");
            buffer = Encoding.UTF8.GetPreamble().Concat(buffer).ToArray();

            return buffer;
        }
        
        private byte[] ExportHancellReport(string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, DateTime from, DateTime to,
            List<int> eventType, string userName, List<int> workType, List<int> inOutType, string cardId, string search, List<int> doorIds,List<int> objectType,
            List<int> buildingList, List<int> departmentIds, List<int> cardType, int? company, List<int> isValid,
            string culture)
        {
            using (XLWorkbook xl = new XLWorkbook())
            {
                var data = FilterDataWithOrderEventLogReport(from, to, doorIds,objectType, userName,
                        cardId, search, departmentIds, workType, inOutType, eventType, buildingList,
                        cardType, culture, isValid, sortColumn, sortDirection, 0, 0, out totalRecords,
                        out recordsFiltered)
                    .ToList();

                var splitList = data.SplitList(50000);
                int sheetIndex = 0;

                foreach (var dataList in splitList)
                {
                    sheetIndex++;

                    // add a new worksheet to the empty workbook
                    var worksheet = xl.Worksheets.Add($"{EventLogResource.lblEventLog}_{sheetIndex}");

                    //First add the headers
                    for (var i = 0; i < _header.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = _header[i];
                    }

                    var recordIndex = 2;
                    foreach (var report in dataList)
                    {
                        //var card = _unitOfWork.CardRepository.GetByCardId(_httpContext.User.GetCompanyId(), report.CardId);
                        var colIndex = 1;

                        worksheet.Cell(recordIndex, colIndex++).Value = recordIndex - 1; //idx
                        worksheet.Cell(recordIndex, colIndex++).Value = report.EventTime; //출입시간
                        worksheet.Cell(recordIndex, colIndex++).Value = report.UserName; //이름

                        worksheet.Cell(recordIndex, colIndex++).Value = report.WorkTypeName; // 인원 구분

                        worksheet.Cell(recordIndex, colIndex++).Value = report.BirthDay; //생년월일
                        worksheet.Cell(recordIndex, colIndex++).Value = report.EmployeeNumber; //사원번호
                        worksheet.Cell(recordIndex, colIndex++).Value = report.DepartmentName; //소속
                        worksheet.Cell(recordIndex, colIndex++).Value = report.CardId; //카드ID
                        worksheet.Cell(recordIndex, colIndex++).Value = report.DeviceAddress; //RID
                        worksheet.Cell(recordIndex, colIndex++).Value = report.DoorName; //출입문 이름 
                        worksheet.Cell(recordIndex, colIndex++).Value = report.Building; //장소
                        worksheet.Cell(recordIndex, colIndex++).Value = report.InOut; //입/출
                        worksheet.Cell(recordIndex, colIndex++).Value = report.EventDetail; //이벤트 종류
                        worksheet.Cell(recordIndex, colIndex++).Value = report.IssueCount; //발급차수
                        worksheet.Cell(recordIndex, colIndex++).Value = report.CardStatus; //카드상태
                        worksheet.Cell(recordIndex, colIndex++).Value = report.CardType; //카드구분
                        worksheet.Cell(recordIndex, colIndex++).Value = report.BodyTemperature;

                        recordIndex++;
                    }
                }

                //xl.SaveAs("DirectSave1.xlsx");
                ////xl.SaveAs("DirectSave2.cell");

                using (MemoryStream fs = new MemoryStream())
                {
                    xl.SaveAs(fs);
                    fs.Position = 0;
                    return fs.ToArray();
                }
            }
        }

        /// <summary>
        /// Export data in pdf
        /// </summary>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="eventType"></param>
        /// <param name="userName"></param>
        /// <param name="inOutType"></param>
        /// <param name="cardId"></param>
        /// <param name="search"></param>
        /// <param name="doorIds"></param>
        /// <param name="verifyModeIds"></param>
        /// <param name="company"></param>
        /// <param name="buildingIds"></param>
        /// <param name="departmentIds"></param>
        /// <param name="isValid"></param>
        /// <returns></returns>
        public BaseReportModel<EventReportPdfModel> ExportPdf(string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered,
            DateTime from, DateTime to, List<int> eventType,
            string userName, List<int> inOutType, string cardId, string search, List<int> doorIds, List<int> buildingIds,
            List<int> departmentIds, List<int> verifyModeIds, List<int> objectType,
            int? company, List<int> isValid)
        {
            var data = FilterDataWithOrder(from, to, null, eventType,
                userName, inOutType, cardId, search, doorIds, buildingIds, null, departmentIds, verifyModeIds, objectType, null, company, isValid,
                sortColumn, sortDirection, pageNumber: 1, pageSize: 0, out totalRecords, out recordsFiltered).ToList();

            var companyData = _unitOfWork.CompanyRepository.GetById(_httpContext.User.GetCompanyId());
            var result = new BaseReportModel<EventReportPdfModel>(companyData);
            foreach (var report in data)
            {
                result.Rows.Add(new EventReportPdfModel
                {
                    EventTime = report.EventTime,
                    FirstName = report.UserName,
                    CardId = report.CardId,
                    DeviceAddress = report.DeviceAddress,
                    DoorName = report.DoorName,
                    CardType = report.CardType,
                    InOutStatus = report.InOut,
                    EventType = report.EventDetail,
                });
            }

            return result;
        }

        /// <summary>
        /// Export data in txt format
        /// </summary>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="eventType"></param>
        /// <param name="userName"></param
        /// <param name="inOutType"></param>
        /// <param name="cardId"></param>
        /// <param name="doorIds"></param>
        /// <param name="verifyModeIds"></param>
        /// <param name="objectType"></param>
        /// <param name="company"></param>
        /// <param name="buildingIds"></param>
        /// <param name="cardTypes"></param>
        /// <param name="departmentIds"></param>
        /// <param name="isValid"></param>
        /// <returns></returns>
        public byte[] ExportTxt(string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, DateTime from, DateTime to,
            List<int> eventType, string userName, List<int> workType, List<int> inOutType, string cardId, string search, List<int> doorIds,
            List<int> buildingIds, List<int> cardTypes, List<int> departmentIds, List<int> verifyModeIds,List<int> objectType, int? company, List<int> isValid)
        {
            var data = FilterDataWithOrder(from, to, workType, eventType,
                userName, inOutType, cardId, search, doorIds, buildingIds, cardTypes, departmentIds, verifyModeIds,objectType, null, company, isValid,
                sortColumn, sortDirection, pageNumber: 1, pageSize: 0, out totalRecords, out recordsFiltered).ToList();

            var accountType = _httpContext.User.GetAccountType();
            var reportTxt = new StringBuilder();
            // Build the file content
            foreach (var report in data)
            {
                var obj = new List<object>
                {
                    report.EventTime,
                    report.UserName,
                    report.WorkTypeName,
                    report.CardId,
                    report.DeviceAddress,
                    report.DoorName,
                    report.CardType,
                    report.InOut,
                    report.EventDetail
                };

                // [Edward] System admin cannot export EventLog.
                //if (accountType == (short)AccountType.SuperAdmin)
                //{
                //    obj.Insert(0, report.Company.Name);
                //}

                reportTxt.AppendLine(string.Join(",", obj));
            }

            if (accountType == (short)AccountType.SuperAdmin)
            {
                _header.Insert(0, EventLogResource.lblCompany);
            }

            byte[] buffer = Encoding.UTF8.GetBytes($"{string.Join(",", _header)}\r\n{reportTxt}");
            return buffer;
        }

        /// <summary>   Export text 2. </summary>
        /// <remarks>   Edward, 2020-03-02. </remarks>
        /// <param name="sortColumn">       column using in sorting. </param>
        /// <param name="sortDirection">    Ascending or Descending. </param>
        /// <param name="totalRecords">     [out] total records count. </param>
        /// <param name="recordsFiltered">  [out] filtered records count. </param>
        /// <param name="from">   Start time. </param>
        /// <param name="to">     End time. </param>
        /// <param name="eventType">        Event type. </param>
        /// <param name="userName">         User name. </param>
        /// <param name="inOutType">        In or Out. </param>
        /// <param name="cardId">           Card Id. </param>
        /// <param name="doorIds">          Door Id list. </param>
        /// <param name="buildingList">     List of buildings. </param>
        /// <param name="department">       Name of department. </param>
        /// <param name="CardType">         Type of the card. </param>
        /// <param name="company">          Company Id. </param>
        /// <returns>   A byte[]. </returns>
        public byte[] ExportTxtReport(string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered, DateTime from, DateTime to,
            List<int> eventType, string userName, List<int> workType, List<int> inOutType, string cardId, string search, List<int> doorIds,List<int> objectType,
            List<int> buildingList, List<int> departmentIds, List<int> CardType, int? company, List<int> isValid,
            string culture)
        {
            var data = FilterDataWithOrderEventLogReport(from, to, doorIds,objectType, userName,
                    cardId, search, departmentIds, workType, inOutType, eventType, buildingList,
                    CardType, culture, isValid, sortColumn, sortDirection, 0, 0, out totalRecords, out recordsFiltered)
                .ToList();

            var reportTxt = new StringBuilder();
            int idx = 1;
            // Build the file content
            foreach (var report in data)
            {
                var obj = new List<object>
                {
                    idx++,

                    report.EventTime,
                    report.UserName,
                    report.WorkTypeName,
                    report.BirthDay,
                    //report.EmployeeNumber,
                    $"=\"{report.UserCode}\"",
                    report.DepartmentName,
                    $"=\"{report.CardId}\"",
                    $"=\"{report.DeviceAddress}\"",
                    report.DoorName,
                    report.Building,
                    report.InOut,
                    report.EventDetail,
                    report.IssueCount,
                    report.CardStatus,
                    report.CardType,
                    report.BodyTemperature
                };

                reportTxt.AppendLine(string.Join(",", obj));
            }

            byte[] buffer = Encoding.UTF8.GetBytes($"{string.Join(",", _header)}\r\n{reportTxt}");
            buffer = Encoding.UTF8.GetPreamble().Concat(buffer).ToArray();

            return buffer;
        }

        /// <summary>   Saves a system log export. </summary>
        /// <remarks>   Edward, 2020-03-02. </remarks>
        /// <param name="fileName"> Filename of the file. </param>
        public void SaveSystemLogExport(string fileName, EventLogExportFilterModel filter = null)
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
                        if (filter != null)
                        {
                            var companyId = _httpContext.User.GetCompanyId();

                            // Date Time range
                            var accountTimezone = _unitOfWork.AccountRepository.Get(m => m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted).TimeZone;
                            var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

                            DateTime opeDateTimeFrom = filter.StartDate.ConvertToUserTime(offSet);
                            DateTime opeDateTimeTo = filter.EndDate.ConvertToUserTime(offSet);

                            string dateTimeRange = $"{CommonResource.lblDateTimeRange} : {opeDateTimeFrom} ~ {opeDateTimeTo}";
                            contentDetails.Add(dateTimeRange);

                            // Name string
                            if (!string.IsNullOrWhiteSpace(filter.UserName))
                                contentDetails.Add($"{CommonResource.lblName} : {filter.UserName}");

                            // CardId string
                            if (!string.IsNullOrWhiteSpace(filter.CardId))
                                contentDetails.Add($"{UserResource.lblCardId} : {filter.CardId}");

                            // Department list
                            if(filter.DepartmentIds != null && filter.DepartmentIds.Count > 0)
                            {
                                var departmentNames = _unitOfWork.DepartmentRepository.GetIQueryableByCompanyId(companyId).Where(d => filter.DepartmentIds.Contains(d.Id)).Select(d => " - " + d.DepartName).ToList();
                                contentDetails.Add($"{UserResource.lblDepartmentName} : <br />{string.Join("<br />", departmentNames)}");
                            }

                            // Event types
                            if(filter.EventTypes != null && filter.EventTypes.Count > 0)
                            {
                                var eventTypes = EnumHelper.ToEnumList<EventType>().Where(e => filter.EventTypes.Contains(e.Id)).Select(e => " - " + e.Name);
                                contentDetails.Add($"{EventLogResource.lblEventType} : <br />{string.Join("<br />", eventTypes)}");
                            }

                            // Building list
                            if(filter.BuildingIds != null && filter.BuildingIds.Count > 0)
                            {
                                var buildingNames = _unitOfWork.BuildingRepository.GetByCompanyId(companyId).Where(b => filter.BuildingIds.Contains(b.Id)).Select(b => " - " + b.Name).ToList();
                                contentDetails.Add($"{EventLogResource.lblBuilding} : <br />{string.Join("<br />", buildingNames)}");
                            }
                            
                            // Device list
                            if(filter.DoorIds != null && filter.DoorIds.Count > 0)
                            {
                                var deviceNames = _unitOfWork.IcuDeviceRepository.GetDevicesByCompany(companyId).Where(d => filter.DoorIds.Contains(d.Id)).Select(d => " - " + d.Name).ToList();
                                contentDetails.Add($"{EventLogResource.lblDoorName} : <br />{string.Join("<br />", deviceNames)}");
                            }

                            // In/Out types
                            if (filter.InOutType != null && filter.InOutType.Count > 0)
                            {
                                var inOutTypes = EnumHelper.ToEnumList<Antipass>().Where(e => filter.InOutType.Contains(e.Id)).Select(e => " - " + e.Name);
                                contentDetails.Add($"{EventLogResource.lblInOut} : <br />{string.Join("<br />", inOutTypes)}");
                            }

                            // Card types
                            if (filter.CardType != null && filter.CardType.Count > 0)
                            {
                                var cardTypes = EnumHelper.ToEnumList<CardType>().Where(e => filter.CardType.Contains(e.Id)).Select(e => " - " + e.Name);
                                contentDetails.Add($"{EventLogResource.lblCardType} : <br />{string.Join("<br />", cardTypes)}");
                            }

                            // Work types
                            if (filter.WorkType != null && filter.WorkType.Count > 0)
                            {
                                var workTypes = EnumHelper.ToEnumList<WorkType>().Where(e => filter.WorkType.Contains(e.Id)).Select(e => " - " + e.Name);
                                contentDetails.Add($"{EventLogResource.lblWorkType} : <br />{string.Join("<br />", workTypes)}");
                            }
                        }

                        string contentsDetail = String.Empty;
                        if (contentDetails.Count > 0)
                            contentsDetail = string.Join("\n <br />", contentDetails);

                        _unitOfWork.SystemLogRepository.Add(1, SystemLogType.Report, ActionLogType.EventExport,
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

        /// <summary>   Saves a system log recovery. </summary>
        /// <remarks>   Edward, 2020-03-02. </remarks>
        /// <param name="deviceAddress">  Device address. </param>
        public void SaveSystemLogRecovery(string deviceAddress)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //Save system log
                        var content = $"{EventLogResource.lblEventRecovery}\n" +
                                      $"{DeviceResource.lblDeviceAddress} : {deviceAddress}";
                        _unitOfWork.SystemLogRepository.Add(1, SystemLogType.EventRecovery, ActionLogType.Recovery,
                            content, null, null, _httpContext.User.GetCompanyId());

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
        /// Get paginated event recovery devices
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public IQueryable<IcuDevice> GetPaginatedRecoveryDevices(string filter, int pageNumber, int pageSize,
            string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();

            var data = _unitOfWork.AppDbContext.IcuDevice
                .Include(m => m.Building)
                .Where(m => m.CompanyId == companyId && !m.IsDeleted);

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                var normalizedFilter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x =>
                    x.Name.RemoveDiacritics().ToLower().Contains(normalizedFilter) ||
                    x.DeviceAddress.RemoveDiacritics().ToLower().Contains(normalizedFilter) ||
                    x.Building.Name.RemoveDiacritics().ToLower().Contains(normalizedFilter)).AsQueryable();
            }

            recordsFiltered = data.Count();
            data = data.OrderBy($"{sortColumn} {sortDirection}");
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return data;
        }

        /// <summary>   Gets paginated event recovery inquire devices. </summary>
        /// <remarks>   Edward, 2020-03-02. </remarks>
        /// <param name="deviceIds">        List of identifiers for the devices. </param>
        /// <param name="accessDateFrom">   Start date. </param>
        /// <param name="accessDateTo">     End date. </param>
        /// <param name="accessTimeFrom">   Start time. </param>
        /// <param name="accessTimeTo">     End time. </param>
        /// <param name="filter">           Word for searching. </param>
        /// <param name="pageNumber">       Page number. </param>
        /// <param name="pageSize">         Page size. </param>
        /// <param name="sortColumn">       column using in sorting. </param>
        /// <param name="sortDirection">    Ascending or Descending. </param>
        /// <param name="totalRecords">     [out] total records count. </param>
        /// <param name="recordsFiltered">  [out] filtered records count. </param>
        /// <returns>   The paginated event recovery inquire devices. </returns>
        public List<RecoveryDeviceModel> GetPaginatedEventRecoveryInquireDevices(List<int> deviceIds,
            string accessDateFrom, string accessDateTo, string accessTimeFrom, string accessTimeTo,
            string filter, int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();

            var data = _unitOfWork.AppDbContext.IcuDevice
                .Include(m => m.Building)
                .Where(m => m.CompanyId == companyId && !m.IsDeleted);

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                var normalizedFilter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x =>
                    x.Name.RemoveDiacritics().ToLower().Contains(normalizedFilter) ||
                    x.DeviceAddress.RemoveDiacritics().ToLower().Contains(normalizedFilter) ||
                    x.Building.Name.RemoveDiacritics().ToLower().Contains(normalizedFilter)).AsQueryable();
            }

            if (deviceIds.Count > 0)
            {
                data = data.Where(x =>
                    deviceIds.Contains(x.Id));
            }

            recordsFiltered = data.Count();

            //Convert to RecoveryDeviceModel
            var resultList = Helpers.SortData<RecoveryDeviceModel>(data.AsEnumerable<IcuDevice>().Select(_mapper.Map<RecoveryDeviceModel>), sortDirection, sortColumn);
            var recoveryDeviceModels = resultList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            if (recoveryDeviceModels.Count != 0)
            {
                var eventCountByDeviceModel = EventCountInquiry(recoveryDeviceModels, accessDateFrom, accessDateTo, accessTimeFrom, accessTimeTo);

                foreach (var recoveryDeviceModel in recoveryDeviceModels)
                {
                    recoveryDeviceModel.DB = eventCountByDeviceModel.Where(m => m.DeviceId == recoveryDeviceModel.Id)
                        .Select(m => m.Count).FirstOrDefault();
                }

                //recoveryDeviceModels = recoveryDeviceModels.AsQueryable().OrderBy($"{sortColumn} {sortDirection}").ToList();
                //recoveryDeviceModels = recoveryDeviceModels.AsQueryable().Skip((pageNumber - 1) * pageSize).Take(pageSize)
                //    .ToList();
            }

            return recoveryDeviceModels;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recoveryDeviceModels"></param>
        /// <param name="accessDateFrom"></param>
        /// <param name="accessDateTo"></param>
        /// <param name="accessTimeFrom"></param>
        /// <param name="accessTimeTo"></param>
        /// <returns></returns>
        public List<EventCountByDeviceModel> EventCountInquiry(List<RecoveryDeviceModel> recoveryDeviceModels,
            string accessDateFrom,
            string accessDateTo, string accessTimeFrom, string accessTimeTo)
        {
            List<int> exceptEventType = new List<int>
            {
                (int) EventType.CommunicationFailed,
                (int) EventType.CommunicationSucceed,
                (int) EventType.InvalidDoor,
                (int) EventType.ValidDoor,
                (int) EventType.DeviceInstructionOpen,
                (int) EventType.DeviceInstructionSettime,
                (int) EventType.DeviceInstructionReset,
                (int) EventType.DeviceInstructionForceOpen,
                (int) EventType.DeviceInstructionForceClose,
                (int) EventType.DeviceInstructionRelease,
                (int) EventType.DeviceInstructionDeleteAllEvent,
                (int) EventType.DeviceInstructionDeleteAllUser,
                (int) EventType.DeviceInstructionSendAllUser,
                (int) EventType.FirmwareApplicationUpdate,
                (int) EventType.FirmwareDownloadFailed,
            };

            var companyId = _httpContext.User.GetCompanyId();
            var deviceIds = recoveryDeviceModels.Select(m => m.Id);

            var accountTimezone = _unitOfWork.AppDbContext.Account.Where(m => m.Id == _httpContext.User.GetAccountId())
                .FirstOrDefault().TimeZone;
            var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;
            var fromDateTime = "00000000000000";
            var toDateTime = "00000000000000";
            DateTime accessDateTimeFrom = new DateTime();
            DateTime accessDateTimeTo = new DateTime();
            if (!string.IsNullOrEmpty(accessDateFrom))
            {
                accessDateTimeFrom = Helpers.GetFromToDateTime(accessDateFrom, accessTimeFrom, false);
                fromDateTime = accessDateTimeFrom.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);

                //accessDateTimeFrom = accessDateTimeFrom.Subtract(offSet);
                accessDateTimeFrom = accessDateTimeFrom.ConvertToSystemTime(offSet);

                //data = data.Where(m =>
                //    m.EventTime >= accessDateTimeFrom);
            }

            if (!string.IsNullOrEmpty(accessDateTo))
            {
                accessDateTimeTo = Helpers.GetFromToDateTime(accessDateTo, accessTimeTo, true);
                toDateTime = accessDateTimeTo.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
                //accessDateTimeTo = accessDateTimeTo.Subtract(offSet);
                accessDateTimeTo = accessDateTimeTo.ConvertToSystemTime(offSet);

                //data = data.Where(m =>
                //    m.EventTime <= accessDateTimeTo);
            }

            var data = _unitOfWork.AppDbContext.EventLog.AsNoTracking()
                //.Include(m => m.User)
                //.Include(m => m.User.Department)
                //.Include(m => m.Icu)
                //.Include(m => m.Icu.Building)
                //.Include(m => m.Company)
                .Where(m => !exceptEventType.Contains(m.EventType) && m.CompanyId == companyId)
                .Where(m => deviceIds.Contains(m.IcuId))
                .Where(m => !string.IsNullOrEmpty(accessDateFrom) && m.EventTime >= accessDateTimeFrom)
                .Where(m => !string.IsNullOrEmpty(accessDateTo) && m.EventTime <= accessDateTimeTo);

            //var eventCount = data.Count();

            var dbCountList = data
                //.Where(d =>
                //    (recoveryDeviceModels.Count <= 0 || recoveryDeviceModels.Select(m => m.Id).Contains(d.IcuId)))
                .GroupBy(g => g.IcuId).Select(r => new EventCountByDeviceModel
                {
                    DeviceId = r.Key,
                    Count = r.Count()
                }).ToList();

            return dbCountList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="models"></param>
        /// <param name="accessDateFrom"></param>
        /// <param name="accessDateTo"></param>
        /// <param name="accessTimeFrom"></param>
        /// <param name="accessTimeTo"></param>
        public void EventRecovery(List<EventRecoveryProgressModel> models, string accessDateFrom,
            string accessDateTo, string accessTimeFrom, string accessTimeTo)
        {
            try
            {
                var fromDateTime = "00000000000000";
                var toDateTime = "00000000000000";
                if (!string.IsNullOrEmpty(accessDateFrom))
                {
                    var accessDateTimeFrom = Helpers.GetFromToDateTime(accessDateFrom, accessTimeFrom, false);
                    fromDateTime = accessDateTimeFrom.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
                }

                if (!string.IsNullOrEmpty(accessDateTo))
                {
                    var accessDateTimeTo = Helpers.GetFromToDateTime(accessDateTo, accessTimeTo, true);
                    toDateTime = accessDateTimeTo.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
                }

                foreach (var model in models)
                {
                    var id = model.DeviceId;
                    var device = _unitOfWork.IcuDeviceRepository.GetByIcuId(id);

                    var processId = model.ProcessId;
                    SaveSystemLogRecovery(device.DeviceAddress);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EventRecovery");
            }
        }

        /// <summary>   Gets card status. </summary>
        /// <remarks>   Edward, 2020-03-02. </remarks>
        /// <param name="cardId">   Card ID. </param>
        /// <returns>   The card status. </returns>
        public int GetCardStatus(string cardId)
        {
            try
            {
                var user = _unitOfWork.UserRepository.GetByCardId(_httpContext.User.GetCompanyId(), cardId);
                if (user != null)
                    return _unitOfWork.CardRepository.GetCardStatusByUserIdAndCardId(_httpContext.User.GetCompanyId(),
                        user.Id, cardId);
                else
                    return (int)(CardStatus.Normal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCardStatus");
                return 0;
            }
        }

        /// <summary>   Gets access history attendance. </summary>
        /// <remarks>   Edward, 2020-03-02. </remarks>
        /// <param name="userId">           Identifier for the user. </param>
        /// <param name="start">            The start Date/Time. </param>
        /// <param name="end">              The end Date/Time. </param>
        /// <param name="eventType">        event type. </param>
        /// <param name="inOut">            type of in out.</param>
        /// <param name="cardType">         card type. </param>
        /// <param name="pageNumber">       Page Number. </param>
        /// <param name="pageSize">         Page size. </param>
        /// <param name="sortColumn">       column using in sorting. </param>
        /// <param name="sortDirection">    ascending or descending. </param>
        /// <param name="totalRecords">     [out] total records count. </param>
        /// <param name="recordsFiltered">  [out] filtered record count. </param>
        /// <returns>   The access history attendance. </returns>
        public IEnumerable<EventLogHistory> GetAccessHistoryAttendance(int userId,
            DateTime start, DateTime end, int eventType, string inOut, int cardType, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var buildingDefault =
                _unitOfWork.BuildingRepository.GetDefaultByCompanyId(_httpContext.User.GetCompanyId());

            TimeZoneInfo zone = buildingDefault.TimeZone.ToTimeZoneInfo();

            var offset = zone.BaseUtcOffset;

            var endDate = end.Date.AddDays(1).Subtract(offset);
            var startDate = start.Date.Subtract(offset);
            var filterData = _unitOfWork.AppDbContext.EventLog.Where(c =>
                    c.CompanyId == _httpContext.User.GetCompanyId() /*&& c.CardId != null*/ /*&& c.IcuId != null*/
                    && c.UserId == userId && c.EventTime >= startDate && c.EventTime < endDate)
                .Include(m => m.User)
                .Include(m => m.User.Department)
                .Include(m => m.Icu)
                .Include(m => m.Icu.Building)
                .Include(m => m.Company)
                .Include(m => m.Visit).ToList();

            var result = eventType != 0 ? filterData.Where(x => x.EventType == eventType) 
                : filterData.Where(x => x.EventType == (short)EventType.NormalAccess);
            if (cardType != 0)
            {
                result = filterData.Where(x => x.CardType == cardType);
            }
            if (!string.IsNullOrEmpty(inOut))
            {
                result = filterData.Where(x => x.Antipass.ToLower().Equals(inOut.ToLower()));
            }
            var data = result.Select(x => new EventLogHistory
            {
                InOut = x.Antipass,
                AccessTime = DateTimeHelper.ConvertDateTimeToIso(x.EventTime),
                CardId = x.CardId,
                EventDetail = x.EventType,
                DoorName = x.DoorName,
                UnixTime = x.EventTime.ToSettingDateTimeUnique(),
                Avatar = x.IsVisit ? x.Visit.Avatar : x.User.Avatar,
                ImageCamera = x.ImageCamera,
                ResultCheckIn = x.ResultCheckIn,
                Building = x.Icu.Building.Name,
                CardTypeId = x.CardType,
                CardType = Enum.GetName(typeof(CardType), x.CardType)
            });

            totalRecords = data.Count();

            recordsFiltered = data.Count();
            // data = data.OrderByDescending(m => DateTimeHelper.ConvertIsoToDateTime(m.AccessTime));
            data = data.AsQueryable().OrderBy($"{sortColumn} {sortDirection}");
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return data;
        }

        public IEnumerable<EventLogHistory> GetAccessHistoryByAttendanceId(int attendanceId, int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered)
        {
            var attendance = _unitOfWork.AppDbContext.Attendance
                .Include(m => m.User).ThenInclude(m => m.WorkingType)
                .FirstOrDefault(m => m.Id == attendanceId);
            if (attendance == null)
            {
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<EventLogHistory>();
            }

            var workingTimes = JsonConvert.DeserializeObject<List<WorkingTime>>(attendance.User.WorkingType.WorkingDay);
            var workingTime = workingTimes.First(m => m.Name == attendance.Date.DayOfWeek.ToString());
            if (string.IsNullOrEmpty(workingTime.StartTimeWorking))
                workingTime.StartTimeWorking = "00:00";

            DateTime startDate = attendance.StartD.Add(TimeSpan.Parse(workingTime.StartTimeWorking) - TimeSpan.Parse(workingTime.Start));
            var endDate = startDate.AddDays(1);

            var result = _unitOfWork.AppDbContext.EventLog.Where(c =>
                    c.CompanyId == attendance.CompanyId && c.UserId == attendance.UserId
                    && c.EventTime >= startDate && c.EventTime <= endDate
                    && c.EventType == (short)EventType.NormalAccess)
                .Include(m => m.User)
                .Include(m => m.User.Department)
                .Include(m => m.Icu)
                .Include(m => m.Icu.Building)
                .Include(m => m.Company)
                .Include(m => m.Visit).ToList();

            var data = result.Select(x => new EventLogHistory
            {
                InOut = x.Antipass,
                AccessTime = DateTimeHelper.ConvertDateTimeToIso(x.EventTime),
                CardId = x.CardId,
                EventDetail = x.EventType,
                DoorName = x.DoorName,
                UnixTime = x.EventTime.ToSettingDateTimeUnique(),
                Avatar = x.IsVisit ? x.Visit.Avatar : x.User.Avatar,
                ImageCamera = x.ImageCamera,
                ResultCheckIn = x.ResultCheckIn,
                Building = x.Icu.Building.Name,
                CardTypeId = x.CardType,
                CardType = Enum.GetName(typeof(CardType), x.CardType)
            });

            totalRecords = data.Count();
            recordsFiltered = data.Count();
            data = data.AsQueryable().OrderBy($"{sortColumn} {sortDirection}");
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return data;
        }

        public void CreateEventLogForDesktopApp(string rid, User user, string message, string actionType)
        {
            try
            {
                var eventType = (short)EventType.NormalAccess;
                if (message == MessageResource.msgDynamicQrExpired)
                {
                    eventType = (short)EventType.ExpiredQrCode;
                }
                else if (message == MessageResource.msgUserIsInValid)
                {
                    eventType = (short)EventType.UnRegisteredID;
                }

                var identification = _unitOfWork.CardRepository.GetQrCode(user.Id);
                var icuDevice = _unitOfWork.IcuDeviceRepository.GetDeviceByRid(_httpContext.User.GetCompanyId(), rid);
                var building = icuDevice.Building;

                TimeZoneInfo tzInfo = building.TimeZone.ToTimeZoneInfo();

                var eventLog = new EventLog
                {
                    IcuId = icuDevice.Id,
                    DoorName = icuDevice.Name,
                    CompanyId = icuDevice.CompanyId,
                    CardId = identification.CardId,
                    IssueCount = 0,
                    UserId = user?.Id,
                    UserName = user?.FirstName + user?.LastName,
                    DeptId = user?.DepartmentId,
                    Antipass = actionType,
                    EventType = eventType,
                    EventTime = DateTime.Now,
                    CardType = (short)CardType.QrCode,
                };

                if (_unitOfWork.EventLogRepository.IsDuplicated(eventLog))
                {
                    //eventLogs.Add(eventLog);
                    return;
                }

                _unitOfWork.EventLogRepository.Add(eventLog);

                if (eventType != (short)EventType.ExpiredQrCode && eventType != (short)EventType.UnRegisteredID)
                {
                    if (actionType.ToLower() == Constants.Attendance.In.ToLower() || actionType.ToLower() == Constants.Attendance.Out.ToLower())
                    {
                        new AttendanceService(new HttpContextAccessor(), _configuration).AddClockInOut(user, actionType,
                            DateTime.Now, tzInfo);
                    }
                }

                var eventLogJsonString = JsonConvert.SerializeObject(eventLog);
                var eventLogs = new List<EventLog> { eventLog };
                SendDataToFe(eventLogs, user, icuDevice);
                AppLog.EventSave(eventLogJsonString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateEventLogForDesktopApp");
            }
        }

        private void SendDataToFe(List<EventLog> eventLogs, User user, IcuDevice icuDevice)
        {
            for (int i = 0; i < eventLogs.Count(); i++)
            {
                var department =
                    _unitOfWork.DepartmentRepository.GetByIdAndCompanyId(user.DepartmentId,
                        _httpContext.User.GetCompanyId());
                int minus = eventLogs.Count() == 2 ? 2 : 1;

                var eventLog = eventLogs.ElementAt(eventLogs.Count() - minus + i);

                var eventLogDetail = _mapper.Map<SendEventLogDetailData>(eventLog);
                eventLogDetail.Id = Guid.NewGuid();
                eventLogDetail.Device = icuDevice.DeviceAddress;
                eventLogDetail.DeviceAddress = icuDevice.DeviceAddress;
                eventLogDetail.UnixTime =
                    (eventLogDetail.AccessTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                eventLogDetail.DoorName = icuDevice.Name;

                eventLogDetail.Department = department.DepartName;
                eventLogDetail.DepartmentName = department.DepartName;
                eventLogDetail.UserName = user?.FirstName + " " + user?.LastName;
                eventLogDetail.ExpireDate = user?.ExpiredDate.ToSettingDateString();
                eventLogDetail.UserId = user?.Id;
                if (icuDevice.BuildingId != null)
                {
                    eventLogDetail.BuildingId = icuDevice.BuildingId.Value;
                }
                eventLogDetail.Building = icuDevice.Building?.Name;
                eventLogDetail.InOut = eventLog.Antipass;

                _webSocketService.SendWebSocketToFE(Constants.Protocol.EventLogType, icuDevice.CompanyId ?? icuDevice.Company.Id, eventLogDetail);

                if (eventLogs.Count() != 2) break;
            }

        }


        /// <summary>
        /// Generate test data
        /// </summary>
        /// <param name="numberOfEvent"></param>
        public void GenerateTestData(int numberOfEvent)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();

                var defaultDepartment =
                    _unitOfWork.DepartmentRepository.GetDefautDepartmentByCompanyId(companyId);

                var device = _unitOfWork.IcuDeviceRepository.GetByCompany(companyId).First();

                var user = _unitOfWork.UserRepository.GetByCompanyId(companyId).FirstOrDefault();

                var card = _unitOfWork.CardRepository.GetByUserId(companyId, user.Id).FirstOrDefault();

                for (var i = 0; i < numberOfEvent; i++)
                {
                    var fakeEvent = new Faker<EventLog>()
                        .RuleFor(u => u.Antipass, f => Antipass.In.GetDescription())
                        .RuleFor(u => u.CardId, f => card.CardId)
                        .RuleFor(u => u.IssueCount, f => 0)
                        .RuleFor(u => u.CardType, f => (short)0)
                        .RuleFor(u => u.CompanyId, f => companyId)
                        .RuleFor(u => u.DeptId, f => 1)
                        .RuleFor(u => u.DoorName, f => device.Name)
                        .RuleFor(u => u.EventTime, f => DateTime.UtcNow)
                        .RuleFor(u => u.EventType, f => (int)EventType.NormalAccess)
                        .RuleFor(u => u.IcuId, f => device.Id)
                        .RuleFor(u => u.Index, f => (long)0)

                        .RuleFor(u => u.UserName, f => user.FirstName)
                        .RuleFor(u => u.UserId, f => user.Id)
                        .RuleFor(u => u.IsVisit, f => false)

                        .RuleFor(u => u.CardStatus, f => (short)0);

                    var testEvent = fakeEvent.Generate();
                    _unitOfWork.EventLogRepository.Add(testEvent);
                }

                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateTestData");
            }
        }

        public int GetTotalNormalPersonAccessByDay(int companyId, DateTime day, int accountId, bool isUser, bool isIn)
        {
            List<int> eventTypes = new List<int>(new int[]
            {
                (int) EventType.NormalAccess,
            });
            DateTime start = day.Date;
            DateTime end = day.Date.AddDays(1);
            if (accountId != 0)
            {
                string timezone = _unitOfWork.AccountRepository.GetById(accountId)?.TimeZone;
                var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;
                start = start.ConvertToSystemTime(offSet);
                end = end.ConvertToSystemTime(offSet);
            }

            return _unitOfWork.EventLogRepository.GetEventLogsPersonCount(companyId, eventTypes, start, end, isUser, isIn);
        }

        public int GetTotalEventLogs(DateTime start, DateTime end)
        {
            return _unitOfWork.EventLogRepository.GetEventLogsCount(start, end);
        }

        public int GetEventNormalAccess(DateTime start, DateTime end)
        {
            return _unitOfWork.EventLogRepository.GetEventByTypeAccess(new List<int>() { (short)EventType.NormalAccess },
                start, end);
        }

        public int GetEventAbNormalAccess(DateTime start, DateTime end)
        {
            return _unitOfWork.EventLogRepository.GetEventByTypeAccess(new List<int>()
            {
                (short) EventType.UnRegisteredID,
                (short) EventType.NoUserAccessTime,
                (short) EventType.InvalidDoor,
                (short) EventType.ExpirationDate,
                (short) EventType.ExpiredQrCode,
                (short) EventType.MprError,
                (short) EventType.MprIntervalExpired,
                (short) EventType.NoDoorActiveTime,
                (short) EventType.EffectiveDateNotStarted,
                (short) EventType.WrongQRcode,
                (short) EventType.WrongIssueCount,
                (short) EventType.WrongAccessType
            }, start, end);
        }

        public int GetEventByTypeAccess(List<int> eventTypes, DateTime start, DateTime end)
        {
            return _unitOfWork.EventLogRepository.GetEventByTypeAccess(eventTypes, start, end);
        }

        public Dictionary<string, object> GetInitByDataTokenMonitoring(EventLogFilterModel filter)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            List<int> typeNormals = new List<int>()
            {
                (int) EventType.NormalAccess,
                (int) EventType.NormalVehicle,
            };
            List<int> typeCanteens = new List<int>()
            {
                (int) EventType.Meal1,
                (int) EventType.Meal2,
                (int) EventType.Meal3,
                (int) EventType.Meal4,
                (int) EventType.Meal5,
                (int) EventType.Meal6,
                (int) EventType.Meal7,
                (int) EventType.Meal8,
                (int) EventType.Meal9,
                (int) EventType.Meal10,
            };

            DateTime start = DateTime.UtcNow.ConvertToUserTime(filter.DataTokenMonitoring.Timezone).Date;
            start = start.ConvertToSystemTime(filter.DataTokenMonitoring.Timezone);
            DateTime end = start.AddDays(1);

            var eventLogs = _unitOfWork.EventLogRepository.GetEventLogByTypesAndDeviceIds(filter.EventTypes, filter.DataTokenMonitoring.DeviceIds, start, end)
                .Include(m => m.User).ThenInclude(m => m.Department)
                .Include(m => m.Visit).ToList();

            // event canteen
            result.Add("eventCanteen", eventLogs.Count(m => typeCanteens.Contains(m.EventType)));

            // event users
            result.Add("eventUsers", eventLogs.Count(m => !m.IsVisit && typeNormals.Contains(m.EventType)));

            // event visitors
            result.Add("eventVisitors", eventLogs.Count(m => m.IsVisit && typeNormals.Contains(m.EventType)));

            // event abnormal
            result.Add("eventAbnormal", eventLogs.Count(m => !typeNormals.Contains(m.EventType)));

            // history events
            var events = eventLogs.AsQueryable().OrderBy($"{filter.SortColumn} {filter.SortDirection}")
                .Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);
            List<EventLogListModel> eventLogListModels = new List<EventLogListModel>();
            int idx = 0;
            foreach (var item in events)
            {
                var temp = _mapper.Map<EventLogListModel>(item);
                temp.Id = idx++;
                temp.Department = item.User != null ? item.User.Department?.DepartName : item.Visit?.VisitorDepartment;
                temp.DepartmentName = item.User != null ? item.User.Department?.DepartName : item.Visit?.VisitorDepartment;
                temp.Avatar = item.User != null ? item.User.Avatar : item.Visit?.Avatar;

                if ((item.CardType == (int)CardType.VehicleId || item.CardType == (int)CardType.VehicleMotoBikeId) && !string.IsNullOrEmpty(item.ImageCamera))
                {
                    var vehicleImage = JsonConvert.DeserializeObject<List<DataImageCamera>>(item.ImageCamera);
                    temp.Avatar = vehicleImage.First().Link;
                }

                if (string.IsNullOrEmpty(temp.Avatar))
                {
                    if (!string.IsNullOrEmpty(item.UserName))
                    {
                        var user = _unitOfWork.UserRepository.GetUsersByFirstName(item.UserName)
                            .FirstOrDefault(m => m.CompanyId == filter.DataTokenMonitoring.CompanyId);

                        temp.Avatar = user?.Avatar ?? Constants.Image.DefaultMale;
                    }
                }

                eventLogListModels.Add(temp);
            }
            var pagingData = new PagingData<EventLogListModel>
            {
                Data = eventLogListModels,
                Meta = { RecordsTotal = eventLogs.Count, RecordsFiltered = eventLogs.Count }
            };
            result.Add("eventLogs", pagingData);

            // logo for company
            string logo = Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetLogo(filter.DataTokenMonitoring.CompanyId).Value);
            result.Add("logo", logo);

            // get description event-type
            var eventTypes = EnumHelper.ToEnumList<EventType>().Where(m => filter.EventTypes.Contains(m.Id));
            result.Add("eventTypes", eventTypes);

            // get visitor current day
            var pagingDataVisitor = new PagingData<VisitListModel>() { Data = new List<VisitListModel>(), Meta = new Meta() };
            if (filter.DataTokenMonitoring.EnableDisplayListVisitor)
            {
                var visitors = _unitOfWork.VisitRepository.GetByCompanyId(filter.DataTokenMonitoring.CompanyId)
                    .Where(m => !((m.EndDate < start) || (end < m.StartDate)))
                    .Select(visit => _mapper.Map<VisitListModel>(visit)).ToList();

                if (visitors.Count != 0)
                {
                    visitors.ForEach(visit =>
                    {
                        if (visit.VisiteeId != 0)
                        {
                            var visitee = _unitOfWork.UserRepository.GetByUserId(filter.DataTokenMonitoring.CompanyId, visit.VisiteeId);
                            if (visitee != null)
                            {
                                visit.VisiteeName = visitee.FirstName;
                                visit.VisiteeSite = visitee.Department?.DepartName ?? visit.VisiteeSite;
                            }
                        }
                    });
                }

                var visitorApproval = visitors.Where(m => m.StatusCode == (int)VisitChangeStatusType.Approved || m.StatusCode == (int)VisitChangeStatusType.AutoApproved);
                var visitorReject = visitors.Where(m => m.StatusCode == (int)VisitChangeStatusType.Rejected);
                var visitorWait = visitors.Where(m => m.StatusCode == (int)VisitChangeStatusType.Waiting || m.StatusCode == (int)VisitChangeStatusType.Approved1);
                visitors = visitors.Where(m => visitorApproval.All(n => n.Id != m.Id) && visitorReject.All(n => n.Id != m.Id) && visitorWait.All(n => n.Id != m.Id)).ToList();

                var pagingVisitor = new List<VisitListModel>();
                pagingVisitor.AddRange(visitorApproval);
                pagingVisitor.AddRange(visitorReject);
                pagingVisitor.AddRange(visitorWait);
                pagingVisitor.AddRange(visitors);

                pagingDataVisitor = new PagingData<VisitListModel>
                {
                    Data = pagingVisitor,
                    Meta = { RecordsTotal = pagingVisitor.Count, RecordsFiltered = pagingVisitor.Count }
                };
            }
            result.Add("visitors", pagingDataVisitor);

            // get visitor status type
            var visitorStatusTypes = EnumHelper.ToEnumList<VisitChangeStatusType>();
            result.Add("visitorStatusTypes", visitorStatusTypes);

            return result;
        }
        public Dictionary<string, object> GetInitByDataTokenMonitoringToSchool(EventLogFilterModel filter)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            List<int> typeNormals = new List<int>()
            {
                (int) EventType.NormalAccess,
                (int) EventType.NormalVehicle,
            };

            DateTime start = DateTime.UtcNow.ConvertToUserTime(filter.DataTokenMonitoring.Timezone).Date;
            start = start.ConvertToSystemTime(filter.DataTokenMonitoring.Timezone);
            DateTime end = start.AddDays(1);
            
            var timeCheck = DateTime.UtcNow.ConvertToUserTime(filter.DataTokenMonitoring.Timezone);
            DateTime timeReset = DateTime.ParseExact(filter.DataTokenMonitoring.TimeReset ?? "12:00", "HH:mm", CultureInfo.InvariantCulture);
            
            DateTime timeStartCheckIn = DateTime.ParseExact(filter.DataTokenMonitoring.TimeStartCheckIn ?? "07:00", "HH:mm", CultureInfo.InvariantCulture).ConvertToSystemTime(filter.DataTokenMonitoring.Timezone);
            DateTime timeEndCheckIn = DateTime.ParseExact(filter.DataTokenMonitoring.TimeEndCheckIn ?? "10:00", "HH:mm", CultureInfo.InvariantCulture).ConvertToSystemTime(filter.DataTokenMonitoring.Timezone);
            DateTime timeStartCheckOut = DateTime.ParseExact(filter.DataTokenMonitoring.TimeStartCheckOut ?? "15:30", "HH:mm", CultureInfo.InvariantCulture).ConvertToSystemTime(filter.DataTokenMonitoring.Timezone);
            DateTime timeEndCheckOut = DateTime.ParseExact(filter.DataTokenMonitoring.TimeEndCheckOut ?? "18:30", "HH:mm", CultureInfo.InvariantCulture).ConvertToSystemTime(filter.DataTokenMonitoring.Timezone);
            
            var eventLogs = _unitOfWork.EventLogRepository.GetEventLogByTypesAndDeviceIds(filter.EventTypes, filter.DataTokenMonitoring.DeviceIds, start, end)
                .Include(m => m.User).ThenInclude(m => m.Department).ThenInclude(m => m.Parent)
                .Include(m => m.Visit).ToList();

            var filteredEvents = eventLogs.Where(m =>
                    !m.IsVisit && typeNormals.Contains(m.EventType) && m.UserId != null
                    && (m.User.WorkType == (int)WorkType.PermanentWorker ||
                        m.User.WorkType == (int)WorkType.ResidentWorker) && start <= m.EventTime && m.EventTime <= end).ToList();
            
            // users Id checkin
            result.Add("usersIdIn", filteredEvents.Where(m => m.Antipass.ToLower() == "in" 
                         && timeStartCheckIn <= m.EventTime && m.EventTime <= timeEndCheckIn)
                        .Select(x => x.UserId).Distinct());
            
            // users Id checkout
            result.Add("usersIdOut", filteredEvents.Where(m => m.Antipass.ToLower() == "out" 
                         && timeStartCheckOut <= m.EventTime && m.EventTime <= timeEndCheckOut)
                        .Select(x => x.UserId).Distinct());

            // event users
            result.Add("eventUsers", eventLogs.Count(m => !m.IsVisit && typeNormals.Contains(m.EventType)));

            // event visitors
            result.Add("eventVisitors", eventLogs.Count(m => m.IsVisit && typeNormals.Contains(m.EventType)));

            // event abnormal
            result.Add("eventAbnormal", eventLogs.Count(m => !typeNormals.Contains(m.EventType)));

            // // only display eventlog of student Normal Access in morning
            // if (timeStartCheckIn <= timeUtcNow && timeUtcNow <= timeEndCheckIn)
            // {
            //     eventLogs = eventLogs.Where(x => x.EventType == (int)EventType.NormalAccess && x.Antipass.ToLower() == "in"
            //                                      && (x.User?.WorkType ?? 0) == (int)WorkType.PermanentWorker ).ToList();
            // }
            //
            // //  Only display student and parent normal access in afternoon
            // if (timeStartCheckOut <= timeUtcNow && timeUtcNow <= timeEndCheckOut)
            // {
            //     eventLogs = eventLogs.Where(x => x.EventType == (int)EventType.NormalAccess && x.Antipass.ToLower() == "out"
            //                 && ((x.User?.WorkType ?? 0) == (int)WorkType.PermanentWorker 
            //                     || (x.User?.WorkType ?? 0) == (int)WorkType.ContractWorker)).ToList();
            // }
            
            // history events
            var events = eventLogs.AsQueryable().OrderBy($"{filter.SortColumn} {filter.SortDirection}")
                .Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);
            List<EventLogListModel> eventLogListModels = new List<EventLogListModel>();
            int idx = 0;
            foreach (var item in events)
            {
                var temp = _mapper.Map<EventLogListModel>(item);
                temp.Id = idx++;
                if (Enum.TryParse(item.Antipass, true, out Antipass inOutType))
                {
                    temp.InOutType = (int)inOutType;
                }
                temp.Department = item.User != null ? item.User.Department?.DepartName : "";
                temp.DepartmentName = item.User != null ? item.User.Department?.DepartName : "";
                temp.ParentDepartmentId = item.User?.Department?.Parent?.Id ?? 0;
                temp.Parent = item.User != null ? item.User.Department?.Parent?.DepartName : "";
                temp.Avatar = item.User != null ? item.User.Avatar : item.Visit?.Avatar;

                if ((item.CardType == (int)CardType.VehicleId || item.CardType == (int)CardType.VehicleMotoBikeId) && !string.IsNullOrEmpty(item.ImageCamera))
                {
                    var vehicleImage = JsonConvert.DeserializeObject<List<DataImageCamera>>(item.ImageCamera);
                    temp.Avatar = vehicleImage.First().Link;
                }

                if (string.IsNullOrEmpty(temp.Avatar))
                {
                    if (!string.IsNullOrEmpty(item.UserName))
                    {
                        var user = _unitOfWork.UserRepository.GetUsersByFirstName(item.UserName)
                            .FirstOrDefault(m => m.CompanyId == filter.DataTokenMonitoring.CompanyId);

                        temp.Avatar = user?.Avatar ?? Constants.Image.DefaultMale;
                    }
                }

                eventLogListModels.Add(temp);
            }
            var pagingData = new PagingData<EventLogListModel>
            {
                Data = eventLogListModels,
                Meta = { RecordsTotal = eventLogs.Count, RecordsFiltered = eventLogs.Count }
            };
            result.Add("eventLogs", pagingData);

            // logo for company
            string logo = Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetLogo(filter.DataTokenMonitoring.CompanyId).Value);
            result.Add("logo", logo);

            // get description event-type
            var eventTypes = EnumHelper.ToEnumList<EventType>().Where(m => filter.EventTypes.Contains(m.Id));
            result.Add("eventTypes", eventTypes);

            // list user leave request today.
            var listUserLeaveRequest = _unitOfWork.AttendanceLeaveRepository.GetMany(atend =>
                atend.Status == (short)AttendanceStatus.Approved
                && start <= atend.End.ConvertToUserTime(filter.DataTokenMonitoring.Timezone)
                && atend.Start.ConvertToUserTime(filter.DataTokenMonitoring.Timezone) <= end).Select(x => x.UserId);
            
            var allUser = _unitOfWork.UserRepository.GetByCompanyId(filter.DataTokenMonitoring.CompanyId)
                .Include(x => x.Department).ThenInclude(x => x.Parent);
            
            //get count student in, out in class
            var students = allUser.Where(m => m.WorkType == (int)WorkType.PermanentWorker && m.Status != (short)Status.Invalid).ToList();
            result.Add("Total", students.Count);

            // get count student not have leave request.
            var studentLeaveRequest = students.Count(x => listUserLeaveRequest.Contains(x.Id));
            result.Add("StudentLeaveRequest", studentLeaveRequest);
            students = students.Where(x => !listUserLeaveRequest.Contains(x.Id)).ToList();
            int studentNotLeave = students.Count;
            result.Add("StudentNotLeaveRequest", studentNotLeave);
            
            // list user before or after timeCheck
            if (timeCheck.TimeOfDay >= timeReset.TimeOfDay)
            {
                // students = students.Where(m => m.EventLog.OrderBy(x => x.EventTime)
                //     .LastOrDefault(n => start <= n.EventTime && n.EventTime <= end)?.Antipass.ToLower() == "in").ToList();
                students = students.Where(m => m.EventLog.Any(n => start <= n.EventTime && n.EventTime <= end 
                                                   && timeStartCheckIn <= n.EventTime && n.EventTime <= timeEndCheckIn && n.Antipass.ToLower() == "in")
                                               && !m.EventLog.Any(n => start <= n.EventTime && n.EventTime <= end 
                                                   && timeStartCheckOut <= n.EventTime && n.EventTime <= timeEndCheckOut && n.Antipass.ToLower() == "out")).ToList();
                result.Add("TotalStudents", students.Count);
            
                var pagingDataUser = new PagingData<UserListModel>() { Data = new List<UserListModel>(), Meta = new Meta() };
                var data = students.Select(user => _mapper.Map<UserListModel>(user)).ToList();
                data.ForEach(user =>
                {
                    user.FirstName = students.FirstOrDefault(x => x.Id == user.Id)?.Department?.DepartName;
                    user.DepartmentName = students.FirstOrDefault(x => x.Id == user.Id)?.Department?.Parent?.DepartName;
                });
                pagingDataUser = new PagingData<UserListModel>
                {
                    Data = data,
                    Meta = { RecordsTotal = data.Count, RecordsFiltered = data.Count }
                };
                result.Add("users", pagingDataUser);
                // get count student available
                result.Add("StudentAvailable", data.Count);
            }
            else
            {
            
                var pagingDataUser = new PagingData<UserListModel>() { Data = new List<UserListModel>(), Meta = new Meta() };
                students = students.Where(m => !m.EventLog.Any(n => start <= n.EventTime && n.EventTime <= end 
                    && timeStartCheckIn <= n.EventTime && n.EventTime <= timeEndCheckIn && n.Antipass.ToLower() == "in")).ToList();
                result.Add("TotalStudents", students.Count);
            
                var data = students.Select(user => _mapper.Map<UserListModel>(user)).ToList();
                data.ForEach(user =>
                {
                    user.FirstName = students.FirstOrDefault(x => x.Id == user.Id)?.Department?.DepartName;
                    user.DepartmentName = students.FirstOrDefault(x => x.Id == user.Id)?.Department?.Parent?.DepartName;
                });
                pagingDataUser = new PagingData<UserListModel>
                {
                    Data = data,
                    Meta = { RecordsTotal = data.Count, RecordsFiltered = data.Count }
                };
                result.Add("users", pagingDataUser);
                // get count student available
                result.Add("StudentAvailable", (studentNotLeave - students.Count) > 0 ? studentNotLeave - students.Count : 0);
            }

            var isCheckTeacherOut = filter.DataTokenMonitoring.IsCheckTeacherOut;
            // parent departmen
            var listDeparmentMonitoring = new List<ParentDepartmentMonitoring>();
            if (filter.DataTokenMonitoring.ParentDepartment.Count > 0)
            {
                var departments =
                    _unitOfWork.DepartmentRepository.GetMany(
                        x => filter.DataTokenMonitoring.ParentDepartment.Contains(x.Id));
                foreach (var department in departments)
                {
                    var deparmentMonitoring = new ParentDepartmentMonitoring();
                    var userByDepartment = allUser.Where(x => x.Status != (short)Status.Invalid 
                                                              && x.Department.Parent.Id == department.Id && x.WorkType != (short)WorkType.ContractWorker).ToList();

                    //var isTeacher = userByDepartment.All(x => x.WorkType == (short)WorkType.ResidentWorker);

                    deparmentMonitoring.Id = department.Id;
                    deparmentMonitoring.Name = department.DepartName;
                    deparmentMonitoring.TotalUser = userByDepartment.Count();
                    deparmentMonitoring.UserLeaveRequest = userByDepartment.Count(x => listUserLeaveRequest.Contains(x.Id));
                    deparmentMonitoring.UserNotLeaveRequest = userByDepartment.Count(x => !listUserLeaveRequest.Contains(x.Id));
                    
                    // list user before or after timeCheck
                    if (timeCheck.TimeOfDay >= timeReset.TimeOfDay)
                    {
                        var userAvailable = userByDepartment.Where(m => m.EventLog.Any(n => start <= n.EventTime && n.EventTime <= end 
                                    && timeStartCheckIn <= n.EventTime && n.EventTime <= timeEndCheckIn && n.Antipass.ToLower() == "in")
                                && !m.EventLog.Any(n => start <= n.EventTime && n.EventTime <= end 
                                    && timeStartCheckOut <= n.EventTime && n.EventTime <= timeEndCheckOut && n.Antipass.ToLower() == "out")).ToList();

                        // get count student available
                        deparmentMonitoring.UserAvailable = userAvailable.Count;
                        // if (!isCheckTeacherOut && isTeacher)
                        // {
                        //     // get count student available
                        //     deparmentMonitoring.UserAvailable = userByDepartment.Count(m => m.WorkType == (short)WorkType.ResidentWorker 
                        //         && m.EventLog.Any(n => start <= n.EventTime && n.EventTime <= end && timeStartCheckIn.TimeOfDay <= n.EventTime.TimeOfDay 
                        //             && n.EventTime.TimeOfDay <= timeEndCheckIn.TimeOfDay && n.Antipass.ToLower() == "in")
                        //         && !m.EventLog.Any(n => start <= n.EventTime && n.EventTime <= end && timeStartCheckOut.TimeOfDay <= n.EventTime.TimeOfDay 
                        //             && n.EventTime.TimeOfDay <= timeEndCheckOut.TimeOfDay && n.Antipass.ToLower() == "out"));
                        //     deparmentMonitoring.IsTeacher = true;
                        // }
                    }
                    else
                    {
                        var userAvailable = userByDepartment.Where(m => !m.EventLog.Any(n => start <= n.EventTime && n.EventTime <= end 
                            && timeStartCheckIn <= n.EventTime && n.EventTime <= timeEndCheckIn && n.Antipass.ToLower() == "in")).ToList();
                        
                        // get count student available
                        deparmentMonitoring.UserAvailable = (deparmentMonitoring.UserNotLeaveRequest - userAvailable.Count) > 0 
                            ? deparmentMonitoring.UserNotLeaveRequest - userAvailable.Count : 0;
                        
                        // if (!isCheckTeacherOut && isTeacher)
                        // {
                        //     // get count student available
                        //     var teacherAvailable = userByDepartment.Count(m =>
                        //         m.WorkType == (short)WorkType.ResidentWorker && !m.EventLog.Any(n => start <= n.EventTime &&
                        //             n.EventTime <= end
                        //             && timeStartCheckIn.TimeOfDay <= n.EventTime.TimeOfDay &&
                        //             n.EventTime.TimeOfDay <= timeEndCheckIn.TimeOfDay && n.Antipass.ToLower() == "in"));
                        //     
                        //     deparmentMonitoring.UserAvailable = (deparmentMonitoring.UserNotLeaveRequest - teacherAvailable) > 0 
                        //         ? deparmentMonitoring.UserNotLeaveRequest - teacherAvailable : 0;
                        //     deparmentMonitoring.IsTeacher = true;
                        // }
                    }
                    
                    
                    listDeparmentMonitoring.Add(deparmentMonitoring);
                }
                result.Add("ParentDepartment", listDeparmentMonitoring);
            }
            else
            {
                result.Add("ParentDepartment", listDeparmentMonitoring);
            }
            return result;
        }

        //=====================================================================================================================================//
        // For Duali-Korea


        public IQueryable<EventLog> GetAttendenceForDuali(string accessDateFrom, string accessDateTo,
            List<int> userIds, List<int> departmentIds)
        {
            var data = _unitOfWork.AppDbContext.EventLog
                .Include(m => m.User)
                .Include(m => m.User.Department)
                .AsQueryable();

            var timezone = _unitOfWork.AccountRepository.Get(m =>
                m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted).TimeZone;
            var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;

            foreach (var eachData in data)
            {
                //eachData.EventTime = Helpers.ConvertToUserTime(eachData.EventTime, timezone);
                eachData.EventTime = Helpers.ConvertToUserTime(eachData.EventTime, offSet);
            }

            if (!string.IsNullOrEmpty(accessDateFrom))
            {
                var accessDateTimeFrom = Helpers.GetFromToDateTimeConvert(accessDateFrom, "", false);

                data = data.Where(m =>
                    m.EventTime >= accessDateTimeFrom);
            }

            if (!string.IsNullOrEmpty(accessDateTo))
            {
                var accessDateTimeTo = Helpers.GetFromToDateTimeConvert(accessDateTo, "", true);

                data = data.Where(m =>
                    m.EventTime <= accessDateTimeTo);
            }

            if (userIds != null && userIds.Count() > 0)
            {
                data = data.Where(m => userIds.Contains(m.UserId ?? 0));
            }

            if (departmentIds != null && departmentIds.Count() > 0)
            {
                data = data.Where(m => departmentIds.Contains(m.DeptId ?? 0));
            }

            return data;
        }

        public int GetTotalNormalAccessByDay(int companyId, DateTime day,int accountId)
        {
            List<int> eventTypes = new List<int>(new int[]
            {
                (int) EventType.NormalAccess,
                (int) EventType.NormalAccessMpr,
                (int) EventType.DeviceInstructionOpen
            });
            DateTime start = day.Date;
            DateTime end = day.Date.AddDays(1);
            if (accountId != 0)
            {
                string timezone = _unitOfWork.AccountRepository.GetById(accountId)?.TimeZone;
                var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;
                start = start.ConvertToSystemTime(offSet);
                end = end.ConvertToSystemTime(offSet);
            }

            return _unitOfWork.EventLogRepository.GetEventLogsCount(companyId, eventTypes, start, end);
        }

        public int GetUniqueUserAccessByDay(int companyId, DateTime day)
        {
            DateTime start = day.Date;
            DateTime end = day.Date.AddDays(1);
            var accountId = _httpContext.User.GetAccountId();
            if (accountId != 0)
            {
                string timezone = _unitOfWork.AccountRepository.GetById(accountId)?.TimeZone;
                var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;
                start = start.ConvertToSystemTime(offSet);
                end = end.ConvertToSystemTime(offSet);
            }

            return _unitOfWork.EventLogRepository.GetUniqueUserCount(companyId, start, end);
        }
        
        public List<EventChartDataModel> GetAccessChartDataByDoor(IQueryable<IcuDevice> devices, DateTime startTime,
            bool isVisit = false)
        {
            Account account = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId());
            var offSet = account.TimeZone.ToTimeZoneInfo().BaseUtcOffset;

            //startTime = startTime.ConvertToSystemTime(account.TimeZone);
            startTime = startTime.ConvertToSystemTime(offSet);

            List<int> eventTypes =
                new List<int>(new int[] { (int)EventType.NormalAccess, (int)EventType.PressedButton });

            var endTime = startTime.AddHours(24);
            var listEventLog = _unitOfWork.AppDbContext.EventLog
                .Where(m => m.EventTime > startTime && eventTypes.Contains(m.EventType) && m.IsVisit == isVisit)
                .GroupBy(m => new { m.EventTime.Date, m.EventTime.Hour, m.IcuId, m.Antipass })
                .Select(m => new { Metric = m.Key, Count = m.Count() }).ToList();
            var inEvents = listEventLog.Where(m => !string.IsNullOrEmpty(m.Metric.Antipass) && m.Metric.Antipass.ToLower() == "in")
                .OrderBy(x => x.Metric.IcuId).ThenBy(x => x.Metric.Date).ThenBy(x => x.Metric.Hour).ToList();
            var outEvents = listEventLog.Where(m => !string.IsNullOrEmpty(m.Metric.Antipass) && m.Metric.Antipass.ToLower() == "out")
                .OrderBy(x => x.Metric.IcuId).ThenBy(x => x.Metric.Date).ThenBy(x => x.Metric.Hour).ToList();

            List<EventChartDataModel> eventChartData = new List<EventChartDataModel>();
            foreach (var device in devices)
            {
                List<int> inData = new List<int>();
                List<int> outData = new List<int>();

                DateTime curentTime = startTime;
                while (curentTime <= endTime)
                {
                    var inEventsByDevice = inEvents.FirstOrDefault(m =>
                        m.Metric.Date == curentTime.Date && m.Metric.Hour == curentTime.Hour && m.Metric.IcuId == device.Id);
                    var outEventsByDevice = outEvents.FirstOrDefault(m =>
                        m.Metric.Date == curentTime.Date && m.Metric.Hour == curentTime.Hour && m.Metric.IcuId == device.Id);

                    inData.Add(inEventsByDevice?.Count ?? 0);
                    outData.Add(outEventsByDevice?.Count ?? 0);

                    curentTime = curentTime.AddHours(1);
                }
                EventChartDataModel doorData = new EventChartDataModel
                {
                    DoorName = device.Name,
                    DeviceId = device.Id,
                    BuildingId = device.BuildingId ?? 0,
                    BuildingName = device.Building?.Name,
                    //InData = inEvents.Where(m => m.Metric.IcuId == device.Id).Select(m  => m.Count).ToList(),
                    //OutData = outEvents.Where(m => m.Metric.IcuId == device.Id).Select(m => m.Count).ToList(),
                    InData = inData,
                    OutData = outData,
                };
                eventChartData.Add(doorData);
            }
            return eventChartData;
        }

        EventLog GetLastEvent(List<EventLog> eventLogs, int userId, DateTime startTime, DateTime endTime,
            EventType eventType, string inOut)
        {
            return eventLogs.Where(e => e.UserId == userId
                                        // && e.CardId != null
                                        && e.EventType == (int)eventType
                                        && !string.IsNullOrEmpty(e.Antipass) 
                                        && e.Antipass.ToLower() == inOut.ToLower()
                                        && e.EventTime > startTime
                                        && e.EventTime <= endTime).OrderBy(e => e.EventTime).LastOrDefault();
        }

        public Dictionary<string, int[]> GetWorkingUserCountEveryDepartment(int companyId, DateTime attendanceDate)
        {
            Account account = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId());
            string timezone = account.TimeZone;
            var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;

            DateTime startTime = attendanceDate.ConvertToSystemTime(offSet);
            var departmentUser = _unitOfWork.AppDbContext.User
                .Include(m => m.Department)
                .Where(m => m.CompanyId == companyId && !m.IsDeleted)
                .Select(m => new { m.Id, m.Department.DepartName });
            var departmentNames = departmentUser.Select(m => m.DepartName).Distinct();
            Dictionary<string, int[]> result = new Dictionary<string, int[]>();
            foreach (var item in departmentNames)
            {
                result.Add(item, new int[24]);
            }

            // Get all eventlog during given time
            List<EventLog> eventLogs = _unitOfWork.EventLogRepository.GetAllEventLogNormalAccessByTime(companyId, startTime, startTime.AddDays(1));

            var eventlog = _unitOfWork.EventLogRepository.GetUniqueAccessUserIds(companyId, startTime);
            for (int i = 0; i < 24; i++)
            {
                DateTime checkPoint = startTime.AddHours(i);
                // Get list of User that access office during given time
                List<int?> userIds = eventlog.Where(x => x.EventTime <= checkPoint).Select(e=>e.UserId).Distinct().ToList();

                //int totalIn = 0;
                foreach (int userId in userIds)
                {
                    //Check if given user ID is in office or out
                    //Get last "In" event of given user
                    Boolean inOffice = false;
                    EventLog inEvent = GetLastEvent(eventLogs, userId, startTime, checkPoint, EventType.NormalAccess, "In");
                    if (inEvent != null)
                    {
                        // Get last Out event
                        EventLog outEvent = GetLastEvent(eventLogs, userId, startTime, checkPoint, EventType.NormalAccess, "Out");
                        if (outEvent == null)
                        {
                            inOffice = true;
                        }
                        else if (outEvent.EventTime < inEvent.EventTime)
                        {
                            inOffice = true;
                        }

                    }

                    if (inOffice)
                    {
                        var departmentName = departmentUser.First(m => m.Id == userId).DepartName;
                        result[departmentName][i] += 1;
                    }
                }
            }

            return result;
        }

        public int GetTotalAbnormalAccessByDay(int companyId, DateTime day, int accountId)
        {
            List<int> eventTypes = new List<int>(new int[]
            {
                (int) EventType.UnRegisteredID,
                (int) EventType.NoUserAccessTime,
                (int) EventType.InvalidDoor,
                (int) EventType.ExpirationDate,
                (int) EventType.ExpiredQrCode,
                (int) EventType.MprError,
                (int) EventType.MprIntervalExpired,
                (int) EventType.NoDoorActiveTime,
                (int) EventType.EffectiveDateNotStarted,
                (int) EventType.WrongQRcode,
                (int) EventType.WrongIssueCount,
                (int) EventType.WrongAccessType,
                (int) EventType.UnknownPerson,
                (int) EventType.UnregisteredVehicle
            });
            DateTime start = day.Date;
            DateTime end = day.Date.AddDays(1);
            if (accountId != 0)
            {
                string timezone = _unitOfWork.AccountRepository.GetById(accountId)?.TimeZone;
                var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;
                start = start.ConvertToSystemTime(offSet);
                end = end.ConvertToSystemTime(offSet);
            }

            return _unitOfWork.EventLogRepository.GetEventLogsCount(companyId, eventTypes, start, end);
        }

        public int GetTotalVisitorAccessByDay(int companyId, DateTime day, int accountId)
        {
            DateTime start = day.Date;
            DateTime end = day.Date.AddDays(1);
            if (accountId != 0)
            {
                string timezone = _unitOfWork.AccountRepository.GetById(accountId)?.TimeZone;
                var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;

                //start = start.ConvertToSystemTime(timezone);
                //end = end.ConvertToSystemTime(timezone);
                start = start.ConvertToSystemTime(offSet);
                end = end.ConvertToSystemTime(offSet);
            }

            return _unitOfWork.EventLogRepository.GetTotalVisitorAccessByTime(companyId, start, end);
        }

        public EventLog GetById(int id)
        {
            try
            {
                return _unitOfWork.EventLogRepository.GetById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return null;
            }
        }

        public EventLog GetDetailById(int id)
        {
            try
            {
                return _unitOfWork.EventLogRepository.GetDetailById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDetailById");
                return null;
            }
        }

        public void Update(EventLog eventLog)
        {
            try
            {
                _unitOfWork.EventLogRepository.Update(eventLog);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Update");
            }
        }

        public List<string> GetListFileBackupByCompany(int companyId, string userName, bool self = false)
        {
            List<string> files = new List<string>();
            var companyCode = _unitOfWork.CompanyRepository.GetById(companyId).Code;
            string folderBackup = Constants.Settings.DefineFolderNameStoreEventLog;

            if (!Directory.Exists($"{folderBackup}/{companyCode}"))
            {
                return files;
            }
            else
            {
                string[] result = Directory.GetFiles($"{folderBackup}/{companyCode}");
                if (result.Any())
                {
                    Dictionary<DateTime, string> dic = new Dictionary<DateTime, string>();
                    foreach (string item in result)
                    {
                        var fileNames = item.Split("/");
                        string fileName = fileNames.LastOrDefault()?.Replace(".zip", "");
                        if (self)
                        {
                            if(!fileName.Contains(userName, StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }
                        }
                        
                        if (fileName.Contains(companyCode))
                        {
                            fileNames = fileName.Split("\\");
                            fileName = fileNames.LastOrDefault();
                        }

                        string[] dateString = fileName?.Split('_');
                        if (dateString != null)
                        {
                            try
                            {
                                dic.Add(new DateTime(int.Parse(dateString[0]), int.Parse(dateString[1]), 1), fileName);
                            }
                            catch (Exception e)
                            {
                                dic.Add(DateTime.Now, fileName);
                            }
                        }
                    }

                    files = dic.OrderByDescending(m => m.Key).Select(m => m.Value).ToList();
                }

                return files;
            }
        }

        //[Format FileName]: CameraId-CardId-DDMMYYYYhhmmss-1.jpg
        public string SaveImageToEventLog(IFormFile file)
        {
            try
            {
                Console.WriteLine("[UPLOAD IMAGE EVENT-LOG]: " + file.FileName);
            string[] fieldsInFileName = file.FileName.Split('-');

            // check file format
            if (fieldsInFileName.Length < 4 || fieldsInFileName[2].Length < 8)
            {
                _logger.LogWarning("File Name wrong format");
                return "File Name wrong format";
            }

            // check camera
            var camera = _unitOfWork.CameraRepository.GetByCameraId(fieldsInFileName[0]);
            if (camera == null)
            {
                _logger.LogWarning("Camera not found");
                return "Camera not found";
            }

            if (camera.IcuDevice == null)
            {
                _logger.LogWarning("Camera not link to device");
                return "Camera not link to device";
            }

            // // check card id
            // var card = _unitOfWork.CardRepository.GetByCardId(_httpContext.User.GetCompanyId(), fieldsInFileName[1]);
            // if (card == null)
            // {
            //     return "Card not found";
            // }

            var connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
            if (string.IsNullOrEmpty(connectionApi))
            {
                return "Server error config";
            }

            string folderImageDay = fieldsInFileName[2].Substring(0, 8);
            Company company = _unitOfWork.CompanyRepository.GetById(_httpContext.User.GetCompanyId());
            // Save file
            if (!Directory.Exists($"{Constants.Settings.DefineFolderImages}/{company.Code}/{folderImageDay}"))
            {
                Directory.CreateDirectory($"{Constants.Settings.DefineFolderImages}/{company.Code}/{folderImageDay}");
            }

            // Use secure file saving to prevent path traversal attacks
            bool isSuccess = FileHelpers.SaveFileByIFormFileSecure(file, $"{Constants.Settings.DefineFolderImages}/{company.Code}/{folderImageDay}", file.FileName);
            if (!isSuccess)
                return "Invalid file name or save failed";

            string linkFile = $"{Constants.Settings.DefineFolderImages}/{company.Code}/{folderImageDay}/{file.FileName}";

            // save image to event-log
            var building = _unitOfWork.BuildingRepository.GetById(camera.IcuDevice.BuildingId.Value);
            string timezone = building?.TimeZone;
            var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;

            DateTime eventTime = DateTime
                .ParseExact(fieldsInFileName[2], Constants.DateTimeFormat.DdMMyyyyHHmmss, null)
                .ConvertToSystemTime(offSet);

            DateTime minEventTime = eventTime.AddSeconds(-Constants.Settings.TimeoutImageVehicle);
            DateTime maxEventTime = eventTime.AddSeconds(Constants.Settings.TimeoutImageVehicle);

            var eventLogs = _unitOfWork.AppDbContext.EventLog
                .Include(m => m.User)
                .Include(m => m.User.Department)
                .Include(m => m.Visit)
                .Include(m => m.Icu)
                .ThenInclude(m => m.Building).Where(m =>
                m.CompanyId == company.Id &&
                m.IcuId == camera.IcuDeviceId &&
                !Constants.Settings.ListEventTypeNotUseCameraVideo.Contains(m.EventType) &&
                (string.IsNullOrEmpty(fieldsInFileName[1]) || m.CardId == fieldsInFileName[1]) &&
                minEventTime <= m.EventTime && m.EventTime <= maxEventTime);
            if (!eventLogs.Any())
                return null;

            foreach (var eventLog in eventLogs)
            {
                bool isSendToFe = false;
                if (eventLog.CardType == (short)CardType.VehicleId || eventLog.CardType == (int)CardType.VehicleMotoBikeId)
                {
                    List<DataImageCamera> imageCameras = new List<DataImageCamera>();
                    if (!string.IsNullOrEmpty(eventLog.ImageCamera))
                    {
                        imageCameras = JsonConvert.DeserializeObject<List<DataImageCamera>>(eventLog.ImageCamera);
                    }
                    imageCameras.Add(new DataImageCamera()
                    {
                        FileName = file.FileName,
                        Link = connectionApi + "/static/" + linkFile,
                        UserName = eventLog.UserName,
                        UserId = eventLog.UserId,
                    });
                    eventLog.ImageCamera = JsonConvert.SerializeObject(imageCameras);

                    if (fieldsInFileName[3][0] == '2')
                    {
                        eventLog.ResultCheckIn = connectionApi + "/static/" + linkFile;
                        isSendToFe = true;
                    }

                    _unitOfWork.EventLogRepository.Update(eventLog);
                    _unitOfWork.Save();
                }
                else
                {
                    if ((eventTime - eventLog.EventTime).Seconds == 0)
                    {
                        List<DataImageCamera> imageCameras = new List<DataImageCamera>();
                        if (!string.IsNullOrEmpty(eventLog.ImageCamera))
                        {
                            imageCameras = JsonConvert.DeserializeObject<List<DataImageCamera>>(eventLog.ImageCamera);
                        }
                        imageCameras.Add(new DataImageCamera()
                        {
                            FileName = file.FileName,
                            Link = connectionApi + "/static/" + linkFile,
                            UserName = eventLog.UserName,
                            UserId = eventLog.UserId,
                        });
                        eventLog.ImageCamera = JsonConvert.SerializeObject(imageCameras);

                        eventLog.ResultCheckIn = connectionApi + "/static/" + linkFile;
                        isSendToFe = true;
                        _unitOfWork.EventLogRepository.Update(eventLog);
                        _unitOfWork.Save();
                    }
                }

                if (isSendToFe)
                    PushEventLogToFontEnd(eventLog, company.Id);
            }

            return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveImageToEventLog");
                return null;
            }
        }

        //[Format FileName]: CameraId-DDMMYYYYhhmmss.mp4
        public string SaveVideoToEventLog(IFormFile file)
        {
            try
            {
                Console.WriteLine("[UPLOAD VIDEO EVENT-LOG]: " + file.FileName);
                string[] fieldsInFileName = file.FileName.Split('-');

                // check file format
                if (fieldsInFileName.Length < 2 || fieldsInFileName[1].Length < 8)
                {
                    _logger.LogWarning("File Name wrong format");
                    return "File Name wrong format";
                }

                // check camera
                var camera = _unitOfWork.CameraRepository.GetByCameraId(fieldsInFileName[0]);
                if (camera == null)
                {
                    _logger.LogWarning("Camera not found");
                    return "Camera not found";
                }

                if (!camera.IcuDeviceId.HasValue || camera.IcuDevice == null)
                {
                    _logger.LogWarning("Camera not link to device");
                    return "Camera not link to device";
                }

                var connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                if (string.IsNullOrEmpty(connectionApi))
                {
                    return "Server error config";
                }

                string folderVideoDay = fieldsInFileName[1].Substring(0, 8);
                Company company = _unitOfWork.CompanyRepository.GetById(_httpContext.User.GetCompanyId());

                // Use secure file saving to prevent path traversal attacks
                bool isSuccess = FileHelpers.SaveFileByIFormFileSecure(file, $"{Constants.Settings.DefineFolderVideos}/{company.Code}/{folderVideoDay}", file.FileName);
                if (!isSuccess)
                    return "Invalid file name or save failed";

                string linkFile = $"{Constants.Settings.DefineFolderVideos}/{company.Code}/{folderVideoDay}/{file.FileName}";
                linkFile = linkFile.Replace(".mp4", "-not-converted.mp4");

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveVideoToEventLog");
                return null;
            }
        }
        //[Format FileName]: RID-DDMMYYYYhhmmss-DDMMYYYYhhmmss.mp4 (RID-startTime-endTime.mp4)
        public string SaveRecordVideoToEventLog(IFormFile file)
        {
            try
            {
                Console.WriteLine("[UPLOAD VIDEO EVENT-LOG]: " + file.FileName);
                string[] fieldsInFileName = file.FileName.Split('-');

                // check file format
                if (fieldsInFileName.Length < 3 || fieldsInFileName[1].Length < 8 | fieldsInFileName[2].Length < 8)
                {
                    _logger.LogWarning("File Name wrong format");
                    return "File Name wrong format";
                }

                // check camera
                var device = _unitOfWork.IcuDeviceRepository.GetDeviceByAddress(fieldsInFileName[0]);
                if (device == null)
                {
                    _logger.LogWarning("Device not found");
                    return "Device not found";
                }

                var connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                if (string.IsNullOrEmpty(connectionApi))
                {
                    return "Server error config";
                }

                string folderVideoDay = fieldsInFileName[1].Substring(0, 8);
                Company company = _unitOfWork.CompanyRepository.GetById(device.CompanyId ?? 0);
                if (company == null)
                {
                    _logger.LogWarning("Device unregisted to company");
                    return "Device unregisted to company";
                }

                // Use secure file saving to prevent path traversal attacks
                bool isSuccess = FileHelpers.SaveFileByIFormFileSecure(file, $"{Constants.Settings.DefineFolderRecordVideos}/{company.Code}/{folderVideoDay}", file.FileName);
                if (!isSuccess)
                    return "Invalid file name or save failed";

                string linkFile =
                    $"{Constants.Settings.DefineFolderRecordVideos}/{company.Code}/{folderVideoDay}/{file.FileName}";

                // update video for event-logs
                string linkVideo = connectionApi + "/static/" + linkFile;
                var building = _unitOfWork.BuildingRepository.GetById(device.BuildingId ?? 0);
                string timezone = building?.TimeZone;
                var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;
                DateTime minTime = DateTime
                    .ParseExact(fieldsInFileName[1], Constants.DateTimeFormat.DdMMyyyyHHmmss, null)
                    .ConvertToSystemTime(offSet);
                DateTime maxTime = DateTime.ParseExact(fieldsInFileName[2].Replace(".mp4", ""),
                        Constants.DateTimeFormat.DdMMyyyyHHmmss, null)
                    .ConvertToSystemTime(offSet);
                var eventLogs = _unitOfWork.AppDbContext.EventLog.Where(m =>
                    !Constants.Settings.ListEventTypeNotUseCameraVideo.Contains(m.EventType) &&
                    m.IcuId == device.Id && minTime <= m.EventTime && m.EventTime <= maxTime);
                foreach (var eventLog in eventLogs)
                { List<string> videos = new List<string>();
                    if (!string.IsNullOrEmpty(eventLog.Videos))
                    {
                        videos = JsonConvert.DeserializeObject<List<string>>(eventLog.Videos);
                    }

                    if (!videos.Contains(linkVideo))
                    {
                        videos.Add(linkVideo);
                    }

                    if (videos.Any())
                    {
                        eventLog.Videos = JsonConvert.SerializeObject(videos);
                        _unitOfWork.EventLogRepository.Update(eventLog);
                    }
                }

                _unitOfWork.Save();
                Console.WriteLine("[CONVERT VIDEO]: End convert video " + linkFile);
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return e.Message;
            }
        }
        public async void PushEventLogToFontEnd(EventLog eventLog, int companyId)
        {
            // Convert data send to FE
            SendEventLogDetailData sendEventLog = _mapper.Map<SendEventLogDetailData>(eventLog);
            sendEventLog.Id = Guid.NewGuid();
            sendEventLog.UnixTime = (sendEventLog.AccessTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            sendEventLog.DoorName = eventLog.DoorName;
            sendEventLog.EventDetailCode = eventLog.EventType;
            sendEventLog.IssueCount = eventLog.IssueCount.ToString();
            sendEventLog.InOut = eventLog.Antipass;
            if (eventLog.IsVisit && eventLog.Visit != null)
            {
                sendEventLog.Department = eventLog.Visit.VisitorDepartment;
                sendEventLog.DepartmentName = eventLog.Visit.VisitorDepartment;
                sendEventLog.UserName = eventLog.Visit.VisitorName;
                sendEventLog.ExpireDate = eventLog.Visit.StartDate.ToSettingDateString();
                sendEventLog.VisitId = eventLog.Visit.Id;
                sendEventLog.Avatar = eventLog.Visit.Avatar;
            }
            else if (!eventLog.IsVisit && eventLog.User != null)
            {
                sendEventLog.Department = eventLog.User.Department == null ? "" : eventLog.User.Department.DepartName;
                sendEventLog.DepartmentName = eventLog.User.Department == null ? "" : eventLog.User.Department.DepartName;
                sendEventLog.UserName = eventLog.User.FirstName + " " + eventLog.User.LastName;
                sendEventLog.ExpireDate = eventLog.User.ExpiredDate.ToSettingDateString();
                sendEventLog.UserId = eventLog.User.Id;
                if (eventLog.Icu.BuildingId != null) sendEventLog.BuildingId = eventLog.Icu.BuildingId.Value;
                sendEventLog.UserType = (short)UserType.Normal;
                sendEventLog.Avatar = eventLog.User.Avatar;
            }
            
            _webSocketService.SendWebSocketToFE(Constants.Protocol.EventLogType, companyId, sendEventLog);
            Console.WriteLine($"message {JsonConvert.SerializeObject(sendEventLog)}");
        }

        public IQueryable<EventLog> GetEventLogForADD(int size)
        {
            List<int> eventTypes = new List<int>()
            {
                (int)EventType.NormalAccess,
                (int)EventType.UnRegisteredID,
                (int)EventType.ExpirationDate,
                (int)EventType.EffectiveDateNotStarted,
                (int)EventType.WrongIssueCount,
                (int)EventType.NoUserAccessTime,
                (int)EventType.NoDoorActiveTime,
                (int)EventType.AntiPassbackError,
                (int)EventType.AntiPassbackDenied,
                (int)EventType.TryAccessInEmergencyClose,
                (int)EventType.TryAccessInInvalidDoor,
                (int)EventType.MasterCardAccess
            };

            var data = _unitOfWork.AppDbContext.EventLog
                .Include(m => m.User.Card)
                .Include(m => m.Icu)
                .Include(m => m.User.Department)
                .Where(m => m.CompanyId == _httpContext.User.GetCompanyId() && m.Index == 0)
                .Where(m => eventTypes.Contains(m.EventType))
                .AsQueryable();

            data = data.Take(size);

            return data;
        }

        public void EditEventLogForADD(List<int> ids)
        {
            try
            {
                var data = _unitOfWork.AppDbContext.EventLog.Where(m => ids.Contains(m.Id)).ToList();
                data.ForEach(m => m.Index = 1);

                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditEventLogForADD");
            }
        }

        public object GetAccessStatisticsPerson(DateTime from, DateTime to, List<int> inOutType, List<int> buildingIds, List<int> eventTypes, int companyId)
        {
            try
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                List<Building> buildings;
                if (buildingIds != null && buildingIds.Count != 0)
                    buildings = _unitOfWork.BuildingRepository.GetByIdsAndCompanyId(companyId, buildingIds);
                else
                    buildings = _unitOfWork.BuildingRepository.GetByCompanyId(companyId).ToList();
                var buildingOrderBy = buildings.OrderBy(m => m.Name);

                List<string> inOutTypeDescription = new List<string>();
                foreach (var type in inOutType)
                {
                    // inOutTypeDescription.Add(((Antipass)type).GetDescription());
                    inOutTypeDescription.Add(Enum.GetName(typeof(Antipass), type));
                }

                var typeUser = typeof(WorkType);
                var typeVisit = typeof(VisitType);

                foreach (var building in buildingOrderBy)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>();
                    data.Add("Building", building.Name);

                    int total = 0;
                    // user
                    foreach (var enumName in typeUser.GetEnumNames())
                    {
                        int enumValue = (int)Enum.Parse(typeUser, enumName);
                        int countEvent = _unitOfWork.AppDbContext.EventLog.Include(m => m.Icu).ThenInclude(m => m.Building)
                            .Include(m => m.User)
                            .Count(m => !m.IsVisit && m.User.WorkType == enumValue && m.CompanyId == companyId
                                        && m.CardType != (short)CardType.VehicleId && m.CardType != (short)CardType.VehicleMotoBikeId
                                        && from <= m.EventTime && m.EventTime <= to
                                        && (inOutTypeDescription.Count == 0 || inOutTypeDescription.Contains(m.Antipass))
                                        && m.Icu.Building.Name == building.Name
                                        && eventTypes.Contains(m.EventType));

                        total += countEvent;
                        data.Add(enumName, countEvent);
                    }

                    // visit
                    foreach (var enumName in typeVisit.GetEnumNames())
                    {
                        string enumValue = ((int)Enum.Parse(typeVisit, enumName)).ToString();
                        int countEvent = _unitOfWork.AppDbContext.EventLog.Include(m => m.Icu).ThenInclude(m => m.Building)
                            .Include(m => m.Visit)
                            .Count(m => m.IsVisit && m.Visit.VisitType == enumValue && m.CompanyId == companyId
                                        && m.CardType != (short)CardType.VehicleId && m.CardType != (short)CardType.VehicleMotoBikeId
                                        && from <= m.EventTime && m.EventTime <= to
                                        && (inOutTypeDescription.Count == 0 || inOutTypeDescription.Contains(m.Antipass))
                                        && m.Icu.Building.Name == building.Name
                                        && eventTypes.Contains(m.EventType));

                        total += countEvent;
                        data.Add(enumName, countEvent);
                    }

                    data.Add("Total", total);

                    result.Add(data);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccessStatisticsPerson");
                return null;
            }
        }

        public object GetAccessStatisticsVehicle(DateTime from, DateTime to, List<int> inOutType, List<int> buildingIds, List<int> eventTypes, int companyId)
        {
            try
            {
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                List<Building> buildings;
                if (buildingIds != null && buildingIds.Count != 0)
                    buildings = _unitOfWork.BuildingRepository.GetByIdsAndCompanyId(companyId, buildingIds);
                else
                    buildings = _unitOfWork.BuildingRepository.GetByCompanyId(companyId).ToList();
                var buildingOrderBy = buildings.OrderBy(m => m.Name);

                List<string> inOutTypeDescription = new List<string>();
                foreach (var type in inOutType)
                {
                    // inOutTypeDescription.Add(((Antipass)type).GetDescription());
                    inOutTypeDescription.Add(Enum.GetName(typeof(Antipass), type));
                }

                foreach (var building in buildingOrderBy)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>();
                    data.Add("Building", building.Name);

                    int total = 0;
                    // vehicle: VehicleRule
                    foreach (var enumName in typeof(VehicleRule).GetEnumNames())
                    {
                        int countEvent = _unitOfWork.AppDbContext.EventLog.Include(m => m.Icu).ThenInclude(m => m.Building)
                            .Include(m => m.User).ThenInclude(m => m.Vehicle)
                            .Include(m => m.Visit).ThenInclude(m => m.Vehicle)

                            .Count(m => m.CompanyId == companyId
                                        && (m.CardType == (short)CardType.VehicleId || m.CardType == (short)CardType.VehicleMotoBikeId)
                                        && from <= m.EventTime && m.EventTime <= to
                                        && (inOutTypeDescription.Count == 0 || inOutTypeDescription.Contains(m.Antipass))
                                        && m.Icu.Building.Name == building.Name
                                        && eventTypes.Contains(m.EventType)
                                        && (
                                            (!m.IsVisit && m.User.Vehicle.Any(v => !v.IsDeleted && v.PlateNumber.ToLower() == m.CardId.ToLower()))
                                            ||
                                            (m.IsVisit && m.Visit.Vehicle.Any(v => !v.IsDeleted && v.PlateNumber.ToLower() == m.CardId.ToLower()))
                                            )
                                  );

                        total += countEvent;
                        data.Add(enumName, countEvent);
                    }

                    data.Add("Total", total);

                    result.Add(data);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccessStatisticsVehicle");
                return null;
            }
        }


        /// <summary>
        /// Get vehicle data with pagination
        /// </summary>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="isValid"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="eventType"></param>
        /// <param name="userName"></param>
        /// <param name="plateNumber"></param>
        /// <param name="inOutType"></param>
        /// <param name="doorIds"></param>
        /// <param name="vehicleClassificationIds"></param>
        /// <param name="company"></param>
        /// <param name="buildingIds"></param>
        /// <param name="departmentIds"></param>
        /// <param name="verifyModeIds"></param>
        /// <returns></returns>
        public List<VehicleEventLogListModel> GetPaginatedVehicle(DateTime from, DateTime to, List<int> eventType,
            string userName, string plateNumber, string search, List<int> inOutType, List<int> doorIds,
            List<int> buildingIds, List<int> departmentIds, List<int> verifyModeIds, List<int> objectType, List<int> vehicleClassificationIds, int? company, List<int> isValid, int pageNumber,
            int pageSize, string sortColumn, string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            // Get default filter data
            // var eIds = defaultData.ItemLists.EventTypeList.Select(m => m.Id).ToList();
            // var dIds = defaultData.ItemLists.DoorList.Select(m => m.Id).ToList();

            // // Set default doorIds
            // if (doorIds.Count != 0)
            //     doorIds = doorIds.Intersect(dIds).ToList();
            // else
            //     doorIds = dIds;

            var data = FilterDataWithOrder(
                from: from,
                to: to,
                personType: null,
                eventType: eventType,
                userName: userName,
                inOutType: inOutType,
                cardId: plateNumber,
                search: search,
                doorIds: doorIds,
                buildingIds: buildingIds,
                cardTypes: new List<int>() { (int)CardType.VehicleId, (int)CardType.VehicleMotoBikeId },
                departmentIds: departmentIds,
                verifyModeIds: verifyModeIds,
                objectType: objectType,
                vehicleClassificationIds: vehicleClassificationIds,
                company: company,
                isValid: isValid,
                sortColumn: sortColumn,
                sortDirection: sortDirection,
                pageNumber: pageNumber,
                pageSize: pageSize,
                totalRecords: out totalRecords,
                recordsFiltered: out recordsFiltered);

            var companyId = _httpContext.User.GetCompanyId();
            List<VehicleEventLogListModel> vehicleEvents = new List<VehicleEventLogListModel>();

            var idx = (pageNumber - 1) * pageSize + 1;

            foreach (var eachData in data)
            {
                eachData.Id = idx++;

                var vehicleData = _mapper.Map<VehicleEventLogListModel>(eachData);
                vehicleData.UnixTime = (DateTime.Parse(eachData.EventTime) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

                if (eachData.UserId != null)
                {
                    var user = _unitOfWork.UserRepository.GetByUserId(companyId, eachData.UserId.Value);
                    if (user != null && user.Vehicle != null && user.Vehicle.Any(v => !v.IsDeleted) && !string.IsNullOrEmpty(vehicleData.PlateNumber))
                    {
                        var vehicle = user.Vehicle.FirstOrDefault(m => !m.IsDeleted && m.PlateNumber.ToLower().Equals(vehicleData.PlateNumber.ToLower()));
                        if (vehicle != null)
                        {
                            vehicleData.Model = vehicle.Model;
                            vehicleData.VehicleColor = vehicle.Color;
                        }
                    }
                }
                else
                {
                    var vehicle = _unitOfWork.VehicleRepository.GetByPlateNumber(eachData.CardId, companyId);
                    if (vehicle != null)
                    {
                        vehicleData.Model = vehicle.Model;
                        vehicleData.VehicleColor = vehicle.Color;
                    }
                }

                vehicleEvents.Add(vehicleData);
            }

            return vehicleEvents;
        }

        public List<HeaderData> GetVehicleEventLogHeaderData()
        {
            List<HeaderData> headers = new List<HeaderData>();

            foreach (var column in ColumnDefines.VehicleEventLogHeader)
            {
                foreach (var element in Enum.GetValues(typeof(VehicleEventLogHeaderColumn)))
                {
                    if (column.Equals(element.GetName()))
                    {
                        HeaderData header = new HeaderData()
                        {
                            HeaderId = (int)element,
                            HeaderName = element.GetDescription(),
                            HeaderVariable = element.GetName(),
                            IsCategory = false
                        };

                        headers.Add(header);
                    }
                }
            }

            return headers;
        }

        /// <summary>
        /// Initial data
        /// </summary>
        /// <returns></returns>
        public VehicleEventLogViewModel InitVehicleReportData()
        {
            var model = new VehicleEventLogViewModel();
            var companyId = _httpContext.User.GetCompanyId();

            var buildings = _unitOfWork.BuildingRepository.GetByCompanyId(companyId);
            var departments = _unitOfWork.DepartmentRepository.GetByCompanyId(companyId);

            List<short> vehicleDeviceTypeList = new List<short>()
            {
                (short)DeviceType.NexpaLPR,
            };
            var lprDevices = _unitOfWork.AppDbContext.IcuDevice.Where(m => !m.IsDeleted && m.CompanyId == companyId && vehicleDeviceTypeList.Contains(m.DeviceType)).ToList();

            // Get EventLog list
            var eventTypes = EnumHelper.ToEnumList<VehicleEventType>();

            // Get LPR camera from Camera table
            var lprCamera = _unitOfWork.AppDbContext.Camera.Include(c => c.IcuDevice).Where(m => m.CompanyId == companyId && m.Type == (int)CameraType.CameraLPR);
            var lprCameraDevices = lprCamera.Select(m => m.IcuDevice).ToList();
            // Add Camera device to LprDevice list
            if (lprCameraDevices.Count != 0)
            {
                lprDevices.AddRange(lprCameraDevices);

                // Add some normal eventTypes
                // Normal Access
                eventTypes.Add(new EnumModel()
                {
                    Id = (int)EventType.NormalAccess,
                    Name = EventType.NormalAccess.GetDescription()
                });
                // UnRegistered ID
                eventTypes.Add(new EnumModel()
                {
                    Id = (int)EventType.UnRegisteredID,
                    Name = EventType.UnRegisteredID.GetDescription()
                });
            }

            var optionList = new VehicleEventLogViewOptionModel()
            {
                EventTypeList = eventTypes.OrderBy(m => m.Name),
                InOutList = EnumHelper.ToEnumList<Antipass>(),
                ApproveList = EnumHelper.ToEnumList<VehicleStatus>(),
                BuildingList = buildings.Select(m => new SelectListItemModel
                {
                    Id = m.Id,
                    Name = m.Name
                }).OrderBy(m => m.Name).ToList(),
                DepartmentList = departments.Select(m => new SelectListItemModel
                {
                    Id = m.Id,
                    Name = m.DepartName
                }).OrderBy(m => m.Name).ToList(),
                VehicleClassificationList = EnumHelper.ToEnumList<VehicleClass>(),
                DoorList = lprDevices.Select(m => new SelectListItemModel
                {
                    Id = m.Id,
                    Name = m.Name
                }).OrderBy(m => m.Name).ToList(),
            };

            var account = _unitOfWork.AppDbContext.Account.Where(m => m.Id == _httpContext.User.GetAccountId())
               .FirstOrDefault();
            if (String.IsNullOrEmpty(account.TimeZone))
            {
                Building defaultBuilding = buildings.OrderBy(m => m.Id).First();
                account.TimeZone = defaultBuilding.TimeZone;
                _unitOfWork.Save();
            }

            // Set "FROM" date
            model.AccessDateFrom = DateTime.UtcNow
                .AddMinutes(Convert.ToDouble(DateTimeHelper.GetUTCtime(account.TimeZone))).ToString("yyyy-MM-dd");
            model.AccessTimeFrom = "00:00:00";

            // Set "TO" date
            model.AccessDateTo = DateTime.UtcNow
                .AddMinutes(Convert.ToDouble(DateTimeHelper.GetUTCtime(account.TimeZone))).ToString("yyyy-MM-dd");
            model.AccessTimeTo = "23:59:59";

            model.ItemLists = optionList;

            return model;
        }

        public List<EventLogDetailModel> GetEventLogsRelated(EventLogRelatedFilterModel filter, out int recordsTotal, out int recordsFiltered)
        {
            var eventLog = filter.EventLog ?? _unitOfWork.AppDbContext.EventLog
                .Include(m => m.Icu).ThenInclude(m => m.Building)
                .FirstOrDefault(m => m.Id == filter.EventLogId);

            if (eventLog == null || eventLog.IcuId == 0)
            {
                recordsFiltered = 0;
                recordsTotal = 0;
                return new List<EventLogDetailModel>();
            }

            var data = _unitOfWork.AppDbContext.EventLog
                .Include(m => m.User).ThenInclude(m => m.Department)
                .Include(m => m.Visit)
                .Include(m => m.Icu).ThenInclude(m => m.Building)
                .Where(m => m.IcuId == eventLog.IcuId && m.Id != filter.EventLogId);
            recordsTotal = data.Count();

            // get event-logs EventTime: 00:00 -> 24:00
            var startDate = eventLog.EventTime.ConvertToUserTime(eventLog.Icu?.Building?.TimeZone).Date;
            startDate = startDate.ConvertToSystemTime(eventLog.Icu?.Building?.TimeZone);
            var endDate = startDate.AddHours(24);
            data = data.Where(m => startDate <= m.EventTime && m.EventTime < endDate);

            if (filter.UserId != 0)
            {
                data = data.Where(m => m.UserId == filter.UserId);
            }
            else if (filter.VisitId != 0)
            {
                data = data.Where(m => m.VisitId == filter.VisitId);
            }
            else if (filter.EventTypes != null && filter.EventTypes.Count != 0)
            {
                data = data.Where(m => filter.EventTypes.Contains(m.EventType));
            }
            else
            {
                data = data.Where(m => !m.UserId.HasValue && !m.VisitId.HasValue && string.IsNullOrEmpty(m.CardId));
            }

            recordsFiltered = data.Count();

            data = data.OrderBy($"{filter.SortColumn} {filter.SortDirection}");
            data = data.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);
            return data.AsEnumerable<EventLog>().Select(_mapper.Map<EventLogDetailModel>).ToList();
        }

        /// <summary>
        /// Get In/Out count by work type
        /// </summary>
        /// <returns></returns>
        public List<EventLogByWorkTypeCount> GetCountByWorkType()
        {
            // Get companyId
            var companyId = _httpContext.User.GetCompanyId();
            // Get today start date.
            var accountTimezone = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId()).TimeZone;
            var current = Helpers.ConvertToUserTime(DateTime.UtcNow, accountTimezone);
            // 1 day
            //var startTime = DateTime.ParseExact(current.Date.ToString("yyyyMMdd") + "000000", "yyyyMMddHHmmss", null).ConvertToSystemTime(accountTimezone);
            // 1 month
            //var startTime = DateTime.ParseExact(current.Date.ToString("yyyyMM") + "01000000", "yyyyMMddHHmmss", null).ConvertToSystemTime(accountTimezone);
            var startTime = DateTime.ParseExact(current.Date.AddDays(-30).ToString("yyyyMMdd") + "000000", "yyyyMMddHHmmss", null).ConvertToSystemTime(accountTimezone);

            // Get attendanceSetting value from DB.
            var attendanceSetting = _unitOfWork.AttendanceRepository.GetAttendanceSetting(companyId);
            List<int> icuIds = new List<int>();
            if (attendanceSetting != null && !string.IsNullOrEmpty(attendanceSetting.InReaders) && !string.IsNullOrEmpty(attendanceSetting.OutReaders))
            {
                var inReaders = JsonConvert.DeserializeObject<List<int>>(attendanceSetting.InReaders);
                var outReaders = JsonConvert.DeserializeObject<List<int>>(attendanceSetting.OutReaders);

                icuIds = inReaders.Union(outReaders).ToList();
            }

            if (icuIds.Count == 0)
            {
                icuIds = _unitOfWork.AppDbContext.IcuDevice.Where(m => !m.IsDeleted && m.CompanyId == companyId).Select(m => m.Id).ToList();
            }

            // Get eventLogs in 1 month
            var eventLogs = _unitOfWork.AppDbContext.EventLog
                            .Include(e => e.User)
                            .Where(m => m.EventTime >= startTime
                                    && m.CompanyId == companyId
                                    && icuIds.Contains(m.IcuId)
                                    && m.UserId != null
                                    && m.CardType != (short)CardType.VehicleId && m.CardType != (short)CardType.VehicleMotoBikeId)
                            .ToList()
                            .GroupBy(e => e.UserId)
                            .Select(d => new KeyValuePair<int, EventLog>(d.Key.Value, d.OrderByDescending(e => e.EventTime).First()))
                            .ToList();

            List<EventLogByWorkTypeCount> result = new List<EventLogByWorkTypeCount>();
            foreach (var eventByUser in eventLogs)
            {
                var lastEvent = eventByUser.Value;
                if (lastEvent != null)
                {
                    var workType = lastEvent.User.WorkType.Value;
                    if (!result.Where(d => d.WorkType == workType).Any())
                    {
                        result.Add(new EventLogByWorkTypeCount()
                        {
                            WorkType = workType,
                            InCnt = 0,
                            OutCnt = 0,
                            Total = 0
                        });
                    }

                    if (lastEvent.Antipass.ToLower().Equals("in"))
                        result.First(m => m.WorkType == workType).InCnt++;
                    else if (lastEvent.Antipass.ToLower().Equals("out"))
                        result.First(m => m.WorkType == workType).OutCnt++;
                }
            }

            // Get user count by workType
            var users = _unitOfWork.AppDbContext.User.Where(u => u.CompanyId == companyId && !u.IsDeleted && u.Status != (short)UserStatus.NotUse).ToList();
            if (users.Count != 0)
            {
                var userCountByWorkType = users.GroupBy(u => u.WorkType).Select(d => new KeyValuePair<int, int>(d.Key.Value, d.Count())).ToList();
                foreach (var keyValue in userCountByWorkType)
                {
                    var data = result.FirstOrDefault(d => d.WorkType == keyValue.Key);
                    if (data == null)
                    {
                        result.Add(new EventLogByWorkTypeCount()
                        {
                            WorkType = (short)keyValue.Key,
                            Total = keyValue.Value,
                            InCnt = 0,
                            OutCnt = 0
                        });
                    }
                    else
                        data.Total = keyValue.Value;
                }
            }

            //foreach (var data in result)
            //    data.OutCnt = data.Total - data.InCnt;

            return result;
        }

        /// <summary>
        /// Get In/Out count by visitor type
        /// </summary>
        /// <returns></returns>
        public List<EventLogByWorkTypeCount> GetCountByVisitType()
        {
            // Get companyId
            var companyId = _httpContext.User.GetCompanyId();
            // Get today start date.
            var accountTimezone = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId()).TimeZone;
            var current = Helpers.ConvertToUserTime(DateTime.UtcNow, accountTimezone);
            // 1 day
            //var startTime = DateTime.ParseExact(current.Date.ToString("yyyyMMdd") + "000000", "yyyyMMddHHmmss", null).ConvertToSystemTime(accountTimezone);
            // 1 month
            //var startTime = DateTime.ParseExact(current.Date.ToString("yyyyMM") + "01000000", "yyyyMMddHHmmss", null).ConvertToSystemTime(accountTimezone);
            var startTime = DateTime.ParseExact(current.Date.AddDays(-30).ToString("yyyyMMdd") + "000000", "yyyyMMddHHmmss", null).ConvertToSystemTime(accountTimezone);

            // Get attendanceSetting value from DB.
            var attendanceSetting = _unitOfWork.AttendanceRepository.GetAttendanceSetting(companyId);
            List<int> icuIds = new List<int>();
            if (attendanceSetting != null && !string.IsNullOrEmpty(attendanceSetting.InReaders) && !string.IsNullOrEmpty(attendanceSetting.OutReaders))
            {
                var inReaders = JsonConvert.DeserializeObject<List<int>>(attendanceSetting.InReaders);
                var outReaders = JsonConvert.DeserializeObject<List<int>>(attendanceSetting.OutReaders);

                icuIds = inReaders.Union(outReaders).ToList();
            }

            if (icuIds.Count == 0)
            {
                icuIds = _unitOfWork.AppDbContext.IcuDevice.Where(m => !m.IsDeleted && m.CompanyId == companyId).Select(m => m.Id).ToList();
            }

            // Get eventLogs in 1 month
            var eventLogs = _unitOfWork.AppDbContext.EventLog
                            .Include(e => e.Visit)
                            .Where(m => m.EventTime >= startTime
                                    && m.CompanyId == companyId
                                    && icuIds.Contains(m.IcuId)
                                    && m.VisitId != null
                                    && m.CardType != (short)CardType.VehicleId && m.CardType != (short)CardType.VehicleMotoBikeId)
                            .ToList()
                            .GroupBy(e => e.VisitId)
                            .Select(d => new KeyValuePair<int, EventLog>(d.Key.Value, d.OrderByDescending(e => e.EventTime).First()))
                            .ToList();

            List<EventLogByWorkTypeCount> result = new List<EventLogByWorkTypeCount>();
            foreach (var eventByUser in eventLogs)
            {
                var lastEvent = eventByUser.Value;
                if (lastEvent != null)
                {
                    var visitType = Int32.Parse(lastEvent.Visit.VisitType);
                    if (!result.Where(d => d.WorkType == visitType).Any())
                    {
                        result.Add(new EventLogByWorkTypeCount()
                        {
                            WorkType = (short)visitType,
                            InCnt = 0,
                            OutCnt = 0,
                            Total = 0
                        });
                    }

                    if (lastEvent.Antipass.ToLower().Equals("in"))
                        result.First(m => m.WorkType == visitType).InCnt++;
                    else if (lastEvent.Antipass.ToLower().Equals("out"))
                        result.First(m => m.WorkType == visitType).OutCnt++;
                }
            }

            // Get visitor count by workType
            var visitors = _unitOfWork.AppDbContext.Visit.Where(u => u.CompanyId == companyId && !u.IsDeleted).ToList();
            if (visitors.Count != 0)
            {
                var visitorCountByWorkType = visitors.GroupBy(u => u.VisitType).Select(d => new KeyValuePair<int, int>(Int32.Parse(d.Key), d.Count())).ToList();
                foreach (var keyValue in visitorCountByWorkType)
                {
                    var data = result.FirstOrDefault(d => d.WorkType == keyValue.Key);
                    if (data == null)
                    {
                        result.Add(new EventLogByWorkTypeCount()
                        {
                            WorkType = (short)keyValue.Key,
                            Total = keyValue.Value,
                            InCnt = 0,
                            OutCnt = 0
                        });
                    }
                    else
                        data.Total = keyValue.Value;
                }
            }

            foreach (var data in result)
            {
                if(data.Total > (data.OutCnt + data.InCnt))
                    data.OutCnt = data.Total - data.InCnt;
            }

            return result;
        }

        /// <summary>
        /// Get attendance status (in or out) by user
        /// This is considered by eventLog. (not attendance table)
        /// </summary>
        /// <param name="deviceIds"></param>
        /// <param name="eventTypes"></param>
        /// <param name="workTypes"></param>
        /// <returns></returns>
        public List<EventLogByWorkType> GetAttendanceStatus(List<int> deviceIds, List<int> eventTypes, List<int> workTypes, List<int> inOut,
             List<int> departmentIds = null, DateTime? lastEventTime = null, string userName = null, string cardId = null, string militaryNumber = null,
             string model = null, string color = null, string plateNumber = null)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                //var accountId = _httpContext.User.GetAccountId();
                //var userTimezone = _unitOfWork.AccountRepository.GetById(accountId).TimeZone;

                if (deviceIds == null || deviceIds.Count == 0)
                {
                    var attendanceSetting = _unitOfWork.AttendanceRepository.GetAttendanceSetting(companyId);

                    if (attendanceSetting != null)
                    {
                        if (string.IsNullOrEmpty(attendanceSetting.InReaders))
                        {
                            throw new Exception("There is not yet 'In reader' settings.");
                        }

                        if (string.IsNullOrEmpty(attendanceSetting.OutReaders))
                        {
                            throw new Exception("There is not yet 'Out reader' settings.");
                        }

                        var inReaders = string.IsNullOrEmpty(attendanceSetting.InReaders)
                            ? new List<int>()
                            : JsonConvert.DeserializeObject<List<int>>(attendanceSetting.InReaders);
                        var outReaders = string.IsNullOrEmpty(attendanceSetting.OutReaders)
                            ? new List<int>()
                            : JsonConvert.DeserializeObject<List<int>>(attendanceSetting.OutReaders);

                        deviceIds = inReaders.Union(outReaders).ToList();
                    }
                }

                //if (deviceIds == null || !deviceIds.Any())
                //    deviceIds = _unitOfWork.AppDbContext.IcuDevice
                //                    .Where(d => d.CompanyId == companyId
                //                            && !d.IsDeleted
                //                            && d.Status == (short)Status.Valid)
                //                    .Select(d => d.Id)
                //                    .ToList();

                if (deviceIds == null || deviceIds.Count == 0)
                {
                    throw new Exception("There is not yet 'In and Out readers' settings.");
                }

                if (eventTypes == null || eventTypes.Count == 0)
                    eventTypes = EnumHelper.ToEnumList<EventType>().Select(e => e.Id).ToList();
                
                var data = _unitOfWork.AppDbContext.User
                    .Include(u => u.Card)
                    .Include(u => u.Vehicle)
                    .Include(u => u.EventLog).ThenInclude(e => e.Icu)
                    .Include(u => u.Department)
                    .Include(u => u.AttendanceLeave)
                    .Where(u => u.CompanyId == companyId
                            && !u.IsDeleted);

                if (workTypes != null && workTypes.Count != 0)
                    data = data.Where(u => workTypes.Contains((int)u.WorkType));

                if (departmentIds != null && departmentIds.Count != 0)
                    data = data.Where(u => departmentIds.Contains(u.DepartmentId));

                if (!string.IsNullOrEmpty(userName))
                    data = data.Where(u => u.FirstName.Contains(userName));

                if (!string.IsNullOrEmpty(cardId))
                    data = data.Where(u => u.Card.Any(c => c.CardId.Contains(cardId)));

                if (!string.IsNullOrEmpty(model))
                    data = data.Where(u => u.Vehicle != null && u.Vehicle.Any(v => v.Model.Contains(model)));

                if (!string.IsNullOrEmpty(color))
                    data = data.Where(u => u.Vehicle != null && u.Vehicle.Any(v => v.Color.Contains(color)));

                if (!string.IsNullOrEmpty(plateNumber))
                    data = data.Where(u => u.Vehicle != null && u.Vehicle.Any(v => v.PlateNumber.Contains(plateNumber)));

                List<string> inOutTypeDescription = new List<string>();
                if (inOut != null && inOut.Count != 0)
                {
                    foreach (var type in inOut)
                        inOutTypeDescription.Add(((Antipass)Convert.ToInt32(type)).ToString().ToLower());
                }

                // Get start date.
                var accountTimezone = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId()).TimeZone;
                var current = Helpers.ConvertToUserTime(DateTime.UtcNow, accountTimezone);
                var startTime = DateTime.ParseExact(current.Date.AddDays(-30).ToString("yyyyMMdd") + "000000", "yyyyMMddHHmmss", null).ConvertToSystemTime(accountTimezone);

                List<EventLogByWorkType> result = new List<EventLogByWorkType>();

                foreach (var user in data)
                {
                    var userData = result.FirstOrDefault(r => r.WorkType == user.WorkType.Value);
                    if (userData == null)
                    {
                        userData = new EventLogByWorkType()
                        {
                            WorkType = user.WorkType.Value,
                            EventLogsByUser = new List<EventLogByUser>()
                        };
                        result.Add(userData);

                        userData = result.FirstOrDefault(r => r.WorkType == user.WorkType.Value);
                    }

                    var latestEvent = (user.EventLog != null && user.EventLog.Count != 0)
                        ? user.EventLog.Where(e => deviceIds.Contains(e.IcuId)
                                                && eventTypes.Contains(e.EventType)
                                                && e.EventTime >= startTime)
                                    .OrderByDescending(e => e.EventTime)
                                    .FirstOrDefault()
                        : null;

                    if (latestEvent == null) continue;
                    else if (!inOutTypeDescription.Contains(latestEvent.Antipass.ToLower())) continue;

                    if(lastEventTime != null && lastEventTime != new DateTime())
                    {
                        if(latestEvent.EventTime < lastEventTime || latestEvent.EventTime > lastEventTime.Value.AddDays(1))
                        {
                            continue;
                        }
                    }
                    
                    userData.EventLogsByUser.Add(new EventLogByUser()
                    {
                        UserId = user.Id,
                        UserName = user.FirstName,
                        DepartmentName = user.Department.DepartName,
                        EventLogs = new List<EventLogSimpleModel>(){
                            new EventLogSimpleModel()
                            {
                                Antipass = latestEvent != null ? latestEvent.Antipass : "",
                                //EventTime = latestEvent != null ? latestEvent.EventTime.ToString("yyyyMMddHHmmss") : "",
                                EventTime = latestEvent != null ? latestEvent.EventTime : new DateTime(),
                                IcuId = latestEvent != null ? (int) latestEvent.IcuId : 0,
                                EventLogId = latestEvent != null ? (int) latestEvent.Id : 0,
                                EventType = latestEvent != null ? (int) latestEvent.EventType : 0,
                                CardId = latestEvent != null ? latestEvent.CardId : "",
                                DeviceAddress = latestEvent != null && latestEvent.Icu != null ? latestEvent.Icu.DeviceAddress : ""
                            }
                        },
                        CardList = user.Card.Where(c => !c.IsDeleted && c.CardType != (int)CardType.VehicleId && c.CardType != (int)CardType.VehicleMotoBikeId).Select(c => new CardModel()
                        {
                            CardId = c.CardId,
                            CardType = c.CardType,
                            CardStatus = c.Status,
                            Description = c.Note,
                            IssueCount = c.IssueCount,
                            Id = c.Id
                        }).ToList(),
                        VehicleList = user.Vehicle.Where(v => !v.IsDeleted).Select(v => new VehicleModel()
                        {
                            Id = v.Id,
                            Color = v.Color,
                            Model = v.Model,
                            VehicleType = v.VehicleType,
                            PlateNumber = v.PlateNumber
                        }).ToList(),
                        AttendanceLeave = user.AttendanceLeave.Select(al => new DataModel.Attendance.AttendanceLeaveModel()
                        {
                            Date = al.Date,
                            DepartmentName = user.Department.DepartName,
                            Start = al.Start,
                            End = al.End,
                            Reason = al.Reason,
                            Type = al.Type,
                            Status = al.Status
                        }).ToList()
                    });
                }

                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public int GetCountByFilter(EventLogExportFilterModel filter)
        {
            var data = _unitOfWork.AppDbContext.EventLog
                .Include(m => m.User)
                .Include(m => m.User.Department)
                .Include(m => m.Icu)
                .Include(m => m.Icu.Building)
                .Include(m => m.Company)
                .Include(m => m.Visit)
                .Where(m => m.CompanyId == filter.CompanyId)
                .Where(m => !m.IsVisit)
                .Where(m => m.EventTime >= filter.StartDate)
                .Where(m => m.EventTime <= filter.EndDate);

            // check permission account type dynamic role department level
            if (_httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
            {
                var roleDepartmentIds = _unitOfWork.DepartmentRepository
                    .GetDepartmentIdsByAccountDepartmentRole(_httpContext.User.GetCompanyId(),
                        _httpContext.User.GetAccountId());
                if (roleDepartmentIds.Count != 0)
                {
                    data = data.Where(d => roleDepartmentIds.Contains(d.User.DepartmentId));
                }
            }

            if (filter.DoorIds != null && filter.DoorIds.Count != 0)
            {
                data = data.Where(m => filter.DoorIds.Contains(m.IcuId));
            }
            if (!string.IsNullOrEmpty(filter.UserName))
            {
                data = data.Where(m => m.UserName.ToLower().Contains(filter.UserName.ToLower()));
            }
            if (!string.IsNullOrEmpty(filter.CardId))
            {
                data = data.Where(m => m.CardId.Contains(filter.CardId));
            }
            if (filter.DepartmentIds != null && filter.DepartmentIds.Count != 0)
            {
                data = data.Where(m => filter.DepartmentIds.Contains(m.User.DepartmentId));
            }
            if (filter.ObjectType != null && filter.ObjectType.Count != 0)
            {
                data = data.Where(m =>
                    (filter.ObjectType.Contains((int)ObjectTypeEvent.User) && m.UserId != null) ||
                    (filter.ObjectType.Contains((int)ObjectTypeEvent.Visit) && m.VisitId != null) ||
                    (filter.ObjectType.Contains((int)ObjectTypeEvent.Warning) && m.UserId == null && m.VisitId == null)
                );
            }
            if (filter.InOutType != null && filter.InOutType.Count != 0)
            {
                var inOutTypeDescription = filter.InOutType.Select(type => Enum.GetName(typeof(Antipass), type)?.ToLower());

                data = data.Where(m => inOutTypeDescription.Contains(m.Antipass.ToLower()));
            }
            if (filter.EventTypes != null && filter.EventTypes.Count != 0)
            {
                data = data.Where(m => filter.EventTypes.Contains(m.EventType));
            }
            if (filter.BuildingIds != null && filter.BuildingIds.Count != 0)
            {
                data = data.Where(m => filter.BuildingIds.Contains(m.Icu.Building.Id));
            }
            if (filter.CardType != null && filter.CardType.Count != 0)
            {
                data = data.Where(m => filter.CardType.Contains(m.CardType));
            }
            if (filter.IsValid != null && filter.IsValid.Count != 0)
            {
                data = data.Where(m => filter.IsValid.Contains(m.User.Status));
            }

            return data.Count();
        }

        public bool UploadImageToEventLog(IcuDevice device, UploadImageEventModel model)
        {
            try
            {
                var building = _unitOfWork.BuildingRepository.GetByDeviceId(device.Id);
                var company = _unitOfWork.CompanyRepository.GetById(device.CompanyId ?? 0);
                if (company == null)
                {
                    _logger.LogWarning($"Company not found by device {device.DeviceAddress}");
                    return false;
                }
                
                var eventTime = DateTime.ParseExact(model.EventTime, Constants.DateTimeFormat.DdMMyyyyHHmmss, null).ConvertToSystemTime(building?.TimeZone);
                var eventLog = _unitOfWork.EventLogRepository.GetUniqueEventLog(device.CompanyId ?? 0, device.Id, eventTime, model.CardId);
                if (eventLog == null)
                {
                    _logger.LogWarning($"Event log of device {device.DeviceAddress} not existed in system: ({JsonConvert.SerializeObject(model.EventTime)})");

                    // Use secure file saving to prevent path traversal attacks
                    string tempFileName = $"{device.DeviceAddress}.{model.EventTime}.{model.CardId}.jpg";
                    string tempBasePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/{model.EventTime.Substring(0, 8)}";
                    bool isSavedTemp = FileHelpers.SaveFileImageSecure(model.Image, tempBasePath, tempFileName);
                    if (!isSavedTemp)
                    {
                        _logger.LogWarning("Image invalid or unsafe filename" + "\n" + model.Image);
                        return false;
                    }
                    return true;
                }
                
                var connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                if (string.IsNullOrEmpty(connectionApi))
                {
                    _logger.LogWarning("Server error config");
                    return false;
                }
                
                // Use secure file saving to prevent path traversal attacks
                string fileName = Guid.NewGuid() + ".jpg";
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/{model.EventTime.Substring(0, 8)}";
                bool isSaved = FileHelpers.SaveFileImageSecure(model.Image, basePath, fileName);
                if (!isSaved)
                {
                    _logger.LogWarning("Image invalid or unsafe filename" + "\n" + model.Image);
                    return false;
                }

                string pathFile = $"{basePath}/{fileName}";
                
                var imageCameras = JsonConvert.DeserializeObject<List<DataImageCamera>>(eventLog.ImageCamera ?? "[]");
                imageCameras.Add(new DataImageCamera()
                {
                    FileName = fileName,
                    Link = connectionApi + "/static/" + pathFile,
                    UserName = eventLog.UserName,
                    UserId = eventLog.UserId,
                });
                
                var cardList = new List<OtherCardListModel>();
                if (model.CardRequestList is { Count: > 0 })
                {
                    foreach (var cardRequest in model.CardRequestList)
                    {
                        // Use secure file saving to prevent path traversal attacks
                        string fileImageName = Guid.NewGuid() + ".jpg";
                        string basePathImage = $"{Constants.Settings.DefineFolderImages}/{company.Code}/{model.EventTime.Substring(0, 8)}";
                        bool isSavedImage = FileHelpers.SaveFileImageSecure(cardRequest.Image, basePathImage, fileImageName);
                        if (!isSavedImage)
                        {
                            _logger.LogWarning("Image invalid or unsafe filename" + "\n" + cardRequest.Image);
                            return false;
                        }

                        string pathImageFile = $"{basePathImage}/{fileImageName}";
                        imageCameras.Add(new DataImageCamera()
                        {
                            FileName = fileImageName,
                            Link = connectionApi + "/static/" + pathImageFile,
                            UserName = eventLog.UserName,
                            UserId = eventLog.UserId,
                        });
                        
                        cardList.Add(new OtherCardListModel()
                        {
                            CardId = cardRequest.CardId,
                            CardType = cardRequest.CardType
                        });
                    }
                }
                
                eventLog.ImageCamera = JsonConvert.SerializeObject(imageCameras);
                eventLog.OtherCardId = JsonConvert.SerializeObject(cardList);
                eventLog.ResultCheckIn = connectionApi + "/static/" + pathFile;
                _unitOfWork.EventLogRepository.Update(eventLog);
                _unitOfWork.Save();
                
                PushEventLogToFontEnd(eventLog, company.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
                return false;
            }
        }
        
        public bool UploadImageVisitCheckinToEventLog(IcuDevice device, UploadImageVisitCheckinModel model)
        {
            try
            {
                var company = _unitOfWork.CompanyRepository.GetById(device.CompanyId ?? 0);
                if (company == null)
                {
                    _logger.LogWarning($"Company not found by device {device.DeviceAddress}");
                    return false;
                }

                var connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                if (string.IsNullOrEmpty(connectionApi))
                {
                    _logger.LogWarning("Server error config");
                    return false;
                }
                
                var visit = _unitOfWork.VisitRepository.GetById(model.VisitId);
                if (visit == null)
                {
                    _logger.LogWarning($"Visit not found by id ={model.VisitId}");
                    return false;
                }

                var building = _unitOfWork.BuildingRepository.GetById(device.BuildingId ?? 0);
                string folderEvent = visit.StartDate.ConvertToUserTime(building?.TimeZone).ToString(Constants.DateTimeFormat.DdMMyyyy);
                
                // Use secure file saving to prevent path traversal attacks
                string fileName = $"{Constants.ImageConfig.PrefixVisitCheckinFile}_{model.VisitId}_{device.DeviceAddress}.jpg";
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/{folderEvent}";
                bool isSaved = FileHelpers.SaveFileImageSecure(model.Image, basePath, fileName);
                if (!isSaved)
                {
                    _logger.LogWarning("Image invalid or unsafe filename" + "\n" + model.Image);
                    return false;
                }

                string pathFile = $"{basePath}/{fileName}";

                var cardLFace = _unitOfWork.CardRepository.GetByVisitId(visit.CompanyId, visit.Id).FirstOrDefault(m => m.CardType == (short)CardType.LFaceId);
                if (cardLFace != null)
                {
                    string linkFile = connectionApi + "/static/" + pathFile;
                    string language = Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetLanguage(visit.CompanyId)?.Value) ?? Constants.DefaultLanguage;
                    var eventLog = new EventLog()
                    {
                        Icu = device,
                        IcuId = device.Id,
                        DoorName = device.Name,
                        CompanyId = device.CompanyId,
                        IssueCount = cardLFace.IssueCount,
                        UserName = visit.VisitorName,
                        DeptId = 0,
                        Antipass = EventLogResource.ResourceManager.GetString("lblInDevice", new CultureInfo(language)),
                        IsVisit = true,
                        CardType = (short)cardLFace.CardType,
                        CardStatus = cardLFace.CardStatus,
                        EventTime = visit.StartDate,
                        EventType = (int)EventType.NormalAccess,
                        CardId = cardLFace.CardId,
                        VisitId = visit.Id,
                        ResultCheckIn = linkFile,
                        ImageCamera = JsonConvert.SerializeObject(new List<DataImageCamera>()
                        {
                            new DataImageCamera()
                            {
                                Link = linkFile,
                            }
                        })
                    };
                    _unitOfWork.EventLogRepository.Add(eventLog);
                    _unitOfWork.Save();
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
                return false;
            }
        }
        
        /// <summary>
        /// Get page header about events
        /// </summary>
        /// <param name="companyId"> company identifier </param>
        /// <param name="accountId"> account identifier </param>
        /// <param name="pageName"> a page name </param>
        /// <returns></returns>
        public List<HeaderData> GetEventHeaderSettings(int companyId, int accountId, string pageName)
        {
            IPageHeader pageHeader = new PageHeader(_configuration, pageName, companyId);
            var headers = pageHeader.GetHeaderList(companyId, accountId);

            return headers;
        }
    }
}
