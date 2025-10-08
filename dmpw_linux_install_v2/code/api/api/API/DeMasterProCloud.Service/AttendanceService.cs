using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using AutoMapper;
using Bogus.Extensions;
using Microsoft.Extensions.Configuration;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Attendance;
using DeMasterProCloud.DataModel.WorkingModel;
using DeMasterProCloud.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Tasks;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Common.Resources;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Threading;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Notification = DeMasterProCloud.DataAccess.Models.Notification;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Api.Infrastructure.Mapper;

namespace DeMasterProCloud.Service
{
    public interface IAttendanceService : IPaginationService<AttendanceListModel>
    {
        Dictionary<string, object> GetInit(int companyId);
        Dictionary<string, object> GetLeaveInit(int accountId, int companyId);
        List<AttendanceListReportModel> GetPaginatedAttendanceReport(string name, string departmentIds, string attendanceType, string timezone,
            DateTime start, DateTime end, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, int userId, out int totalRecords, out int recordsFiltered);
        AttendanceListReportModel GetAttendanceById(int id);
        void Recheck(string rangeTime, int companyId, string fromAccessTime, string toAccessTime);
        double Update(int attendanceId, AttendanceModel model);

        Attendance GetByIdAndCompanyId(int companyId, int attendanceId);

        IQueryable<AttendanceListModel> ExportAttendanceReport(string name, string departmentIds,
            string attendanceType,
            DateTime start, DateTime end, out int totalRecords, out int recordsFiltered);

        IQueryable<AttendanceListReportModel> GetPaginatedAttendanceRecordEachUser(int userId,
            DateTime start, DateTime end, int pageNumber, int pageSize, int sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        Dictionary<string, int> GetAttendanceTypeCount(int companyId, DateTime attendanceDate);
        AttendanceLeave GetRequestByIdAndCompanyId(int companyId, int attendanceId);
        List<AttendanceLeave> GetRequestByIdsAndCompanyId(int companyId, List<int> attendanceIds);
        List<AttendanceLeaveModel> GetAttendanceLeaves(string search, string attendanceType, DateTime start, DateTime end, int status, int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered);
        List<AttendanceLeaveModel> GetAttendanceLeavesUser(LeaveReportFilterModel filter, out int totalRecords, out int recordsFiltered);
        string AddAttendanceLeave(LeaveRequestModel model);
        string RegisterAttendanceLeave(AttendanceRegister model);
        AttendanceSettingModel GetAttendanceSetting(int companyId);
        void UpdateAttendanceSetting(AttendanceSetting attendanceSetting);
        bool ApprovedAttendanceLeave(int id, ActionApproval action);
        void DeleteAttendance(AttendanceLeave attendance);
        void DeleteAttendanceLeaves(List<AttendanceLeave> attendanceLeaves);
        string EditAttendanceLeave(AttendanceLeave attendance);

        List<EnumModel> GetAttendanceTypeList();

        void ReCalculateAttendance(string name, string departmentIds, string attendanceType, DateTime start, DateTime end, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, int userId);
        List<AttendanceReportMonthModel> GetAttendanceReportMonth(string search, DateTime month, List<int> departmentIds,
            int pageNumber, int pageSize, string sortColumn, string sortDirection, out int totalRecords, out int recordsFiltered);
        List<AttendanceLeaveReportModel> GetAttendanceReportYear(LeaveReportFilterModel filter, out int totalRecords, out int recordsFiltered);
        LeaveSettingModel GetLeaveRequestSettingByCompanyId(int companyId);
        void UpdateLeaveRequestSetting(LeaveSettingModel setting);

        void SetStartTime(string startTime);
        string GetStartTime();
    }


    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        private readonly string[] _excelHeader =
        {
            "headerName",
            "headerDate",
            "headerDayOfWeek",
            "headerClockIn",
            "headerClockOut",
            "headerTotalTime",
            "headerDiffTime",
            "headerAttendanceType"
        };

        private readonly string[] _excelHeaderWithOutClockOut =
        {
            "headerName",
            "headerDate",
            "headerDayOfWeek",
            "headerClockIn",
            "headerAttendanceType"
        };
        
        public AttendanceService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration;
            _unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<AttendanceService>();
            _mapper = MapperInstance.Mapper;
        }

        public Dictionary<string, object> GetInit(int companyId)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            
            // type export excel
            data.Add("typeExport", EnumHelper.ToEnumList<AttendanceReportType>().OrderBy(m => m.Id));
            
            // attendance types
            data.Add("types", EnumHelper.ToEnumList<AttendanceType>().OrderBy(m => m.Id));
            
            // departments
            var departments = _unitOfWork.DepartmentRepository.GetByCompanyId(companyId)
                .OrderBy(m => m.DepartName)
                .Select(m => new EnumModel() { Id = m.Id, Name = m.DepartName });
            
            // check account type dynamic role enable department role
            // var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
            // PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
            var accountType = _httpContext.User.GetAccountType();
            if (/*plugIns.DepartmentAccessLevel && */accountType == (short)AccountType.DynamicRole)
            {
                var departmentIds = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, _httpContext.User.GetAccountId());
                if (departmentIds.Any())
                {
                    departments = departments.Where(x => departmentIds.Contains(x.Id));
                }
            }
            data.Add("departments", departments);
            
            // attendance setting
            data.Add("setting", GetAttendanceSetting(companyId));
            
            return data;
        }
        
        public Dictionary<string, object> GetLeaveInit(int accountId, int companyId)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            
            string language = "en-US";
            var account = _unitOfWork.AccountRepository.GetById(accountId);
            if (account?.Language != null)
            {
                language = account.Language;
            }

            var types = new Dictionary<int, string>()
            {
                {(short)AttendanceType.OverTime, UserResource.ResourceManager.GetString("lblOverTime", new CultureInfo(language))},
                {(short)AttendanceType.BusinessTrip, UserResource.ResourceManager.GetString("lblBusinessTrip", new CultureInfo(language))},
                {(short)AttendanceType.Vacation, UserResource.ResourceManager.GetString("lblVacation", new CultureInfo(language))},
                {(short)AttendanceType.Sickness, UserResource.ResourceManager.GetString("lblSickness", new CultureInfo(language))},
                {(short)AttendanceType.Remote, UserResource.ResourceManager.GetString("lblRemote", new CultureInfo(language))},
                {(short)AttendanceType.LateIn, UserResource.ResourceManager.GetString("lblLateIn", new CultureInfo(language))},
                {(short)AttendanceType.EarlyOut, UserResource.ResourceManager.GetString("lblEarlyOut", new CultureInfo(language))},
                {(short)AttendanceType.OffDutyBreak, UserResource.ResourceManager.GetString("lblOffDutyBreak", new CultureInfo(language))},
            };
            data.Add("types", types);
            
            var status = new Dictionary<int, string>()
            {
                {(short)AttendanceStatus.Waiting, AttendanceResource.ResourceManager.GetString("lblStatusWaiting", new CultureInfo(language))},
                {(short)AttendanceStatus.Approved, AttendanceResource.ResourceManager.GetString("lblStatusAprroval", new CultureInfo(language))},
                {(short)AttendanceStatus.Reject, AttendanceResource.ResourceManager.GetString("lblStatusReject", new CultureInfo(language))},
            };
            data.Add("status", status);
            
            // departments
            var departments = _unitOfWork.DepartmentRepository.GetByCompanyId(companyId)
                .OrderBy(m => m.DepartName)
                .Select(m => new EnumModel() { Id = m.Id, Name = m.DepartName });
            
            // check account type dynamic role enable department role
            // var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
            // PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
            var accountType = _httpContext.User.GetAccountType();
            if (/*plugIns.DepartmentAccessLevel && */accountType == (short)AccountType.DynamicRole)
            {
                var departmentIds = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
                if (departmentIds.Any())
                {
                    departments = departments.Where(x => departmentIds.Contains(x.Id));
                }
            }
            data.Add("departments", departments);
            
            // attendance setting
            data.Add("setting", GetAttendanceSetting(companyId));
            
            return data;
        }

        public IQueryable<AttendanceListModel> GetPaginated(string name, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var data = _unitOfWork.AppDbContext.Attendance
                .Where(c => c.CompanyId == _httpContext.User.GetCompanyId())
                .Select(m => new AttendanceListModel()
                {
                    Id = m.Id,
                    Date = m.Date,
                    Start = (m.StartD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds.Equals(0.0)
                        ? 0 : (m.StartD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    End = (m.EndD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds.Equals(0.0)
                        ? 0 : (m.EndD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    WorkingTime = m.WorkingTime,
                    Type = m.Type,
                    UserId = m.UserId,
                    ClockIn = (m.ClockInD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    ClockOut = (m.ClockOutD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    UserName = m.User.FirstName
                });

            totalRecords = data.Count();


            recordsFiltered = data.Count();
            data = data.OrderBy($"{sortColumn} {sortDirection}");
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return data;
        }

        public List<AttendanceListReportModel> GetPaginatedAttendanceReport(string name, string departmentIds, string attendanceType, string timezone, DateTime start, DateTime end, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, int userId, out int totalRecords, out int recordsFiltered)
        {
            var data = _unitOfWork.AppDbContext.Attendance.Include(m => m.User)
                .Where(c => c.CompanyId == _httpContext.User.GetCompanyId() && c.User.Status != (short)Status.Invalid 
                       && (userId == 0 || c.UserId == userId) && !c.User.IsDeleted && c.User.ExpiredDate > DateTime.UtcNow)
                .Select(m => new AttendanceListReportModel()
                {
                    Id = m.Id,
                    Date = m.Date,
                    Start = (m.StartD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds.Equals(0.0)
                        ? 0 : (m.StartD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    End = (m.EndD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds.Equals(0.0)
                        ? 0 : (m.EndD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    WorkingTime = m.WorkingTime,
                    Type = m.Type,
                    UserId = m.UserId,
                    ClockIn = (m.ClockInD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    ClockOut = (m.ClockOutD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    UserName = m.User.FirstName,
                    DepartmentId = m.User.DepartmentId,
                    TotalWorkingTime = m.TotalWorkingTime,
                    EditedBy = m.EditedBy,
                });
            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(name))
            {
                var normalizedName = name.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x => x.UserName.RemoveDiacritics().ToLower().Contains(normalizedName)).AsQueryable();
            }

            if (!string.IsNullOrEmpty(attendanceType))
            {
                String[] strlist = attendanceType.Split(",");
                if (strlist.Any())
                {
                    data = data.Where(x => strlist.Contains(x.Type.ToString()));
                }
            }

            // check permission account type dynamic role department level
            if (_httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
            {
                var companyId = _httpContext.User.GetCompanyId();
                var accountId = _httpContext.User.GetAccountId();
                var departmentRoleLevelIds = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
                if (departmentRoleLevelIds.Any())
                {
                    data = data.Where(x => departmentRoleLevelIds.Contains(x.DepartmentId));
                }
            }
            
            if (!string.IsNullOrEmpty(departmentIds))
            {
                String[] strlist = departmentIds.Split(",");
                if (strlist.Any())
                {
                    var listDepartmentSearch = new List<int>();
                    List<int> listDepartmentIds = strlist.Select(int.Parse).ToList();
                    var departments = _unitOfWork.DepartmentRepository.GetMany(x => listDepartmentIds.Contains(x.ParentId ?? 0) 
                        && !x.IsDeleted && x.CompanyId == _httpContext.User.GetCompanyId()).ToList();
                    foreach (var departmentId in listDepartmentIds)
                    {
                        var childDepartmentId = departments.Where(x => x.ParentId == departmentId).Select(x => x.Id).ToList();

                        if (childDepartmentId.Any())
                        {
                            listDepartmentSearch.AddRange(childDepartmentId);
                        }
                        listDepartmentSearch.Add(departmentId);
                    }
                    
                    data = data.Where(x => listDepartmentSearch.Contains(x.DepartmentId));
                }
            }

            // start = start > DateTime.Now ? DateTime.Now.Date : start;
            // end = end > DateTime.Now ? DateTime.Now.Date : end;
            data = start <= end
                ? data.Where(o => o.Date >= start && o.Date <= end)
                : data.Where(o => o.Date <= start && o.Date.AddDays(1) <= end);

            recordsFiltered = data.Count();
            // data = data.OrderBy(m => m.Date)/*.ThenBy($"{sortColumn} {sortDirection}")*/;
            data = data.OrderBy($"{sortColumn} {sortDirection}").ThenBy(m => m.Date);
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            var result = data.ToList();

            // init editor name
            List<Account> accountsEditedBy = new List<Account>();
            for (int i = 0; i < result.Count; i++)
            {
                var item = result[i];

                if (item.EditedBy.HasValue)
                {
                    var accountEditedBy1 = accountsEditedBy.FirstOrDefault(m => m.Id == item.EditedBy.Value);
                    if (accountEditedBy1 != null)
                    {
                        item.EditorName = accountEditedBy1.Username;
                        result[i] = item;
                        continue;
                    }

                    var accountEditedBy2 = _unitOfWork.AccountRepository.GetById(item.EditedBy.Value);
                    if (accountEditedBy2 != null)
                    {
                        item.EditorName = accountEditedBy2.Username;
                        accountsEditedBy.Add(accountEditedBy2);
                        result[i] = item;
                    }
                }
            }

            return result;
        }

        public AttendanceListReportModel GetAttendanceById(int id)
        {
            var data = _unitOfWork.AppDbContext.Attendance.Include(m => m.User)
                .Where(c => c.CompanyId == _httpContext.User.GetCompanyId() && c.User.Status != (short)Status.Invalid && c.Id == id)
                .Select(m => new AttendanceListReportModel()
                {
                    Id = m.Id,
                    Date = m.Date,
                    Start = (m.StartD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds.Equals(0.0)
                        ? 0 : (m.StartD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    End = (m.EndD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds.Equals(0.0)
                        ? 0 : (m.EndD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    WorkingTime = m.WorkingTime,
                    Type = m.Type,
                    UserId = m.UserId,
                    ClockIn = (m.ClockInD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    ClockOut = (m.ClockOutD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    UserName = m.User.FirstName,
                    DepartmentId = m.User.DepartmentId,
                    TotalWorkingTime = m.TotalWorkingTime,
                }).FirstOrDefault();
            return data;
        }

        /// <summary>
        /// This function is for re-calculate
        /// </summary>
        /// <param name="name"></param>
        /// <param name="departmentIds"></param>
        /// <param name="attendanceType"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="userId"></param>
        public void ReCalculateAttendance(string name, string departmentIds, string attendanceType, DateTime start, DateTime end, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, int userId)
        {
            try
            {
                var attendances = _unitOfWork.AppDbContext.Attendance.Include(m => m.User).ThenInclude(m => m.WorkingType)
                    .Where(c => c.CompanyId == _httpContext.User.GetCompanyId() && c.User.Status != (short)Status.Invalid && (userId == 0 || c.UserId == userId));

                if (!string.IsNullOrEmpty(name))
                {
                    var normalizedName = name.Trim().RemoveDiacritics().ToLower();
                    attendances = attendances.AsEnumerable().Where(x => x.User?.FirstName?.RemoveDiacritics()?.ToLower()?.Contains(normalizedName) == true).AsQueryable();
                }

                if (!string.IsNullOrEmpty(attendanceType))
                {
                    String[] strlist = attendanceType.Split(",");
                    if (strlist.Any())
                    {
                        attendances = attendances.Where(x => strlist.Contains(x.Type.ToString()));
                    }
                }

                if (!string.IsNullOrEmpty(departmentIds))
                {
                    String[] strlist = departmentIds.Split(",");
                    if (strlist.Any())
                    {
                        attendances = attendances.Where(x => strlist.Contains(x.User.DepartmentId.ToString()));
                    }
                }

                attendances = start <= end
                    ? attendances.Where(o => o.Date >= start && o.Date <= end)
                    : attendances.Where(o => o.Date <= start && o.Date.AddDays(1) <= end);

                var timeFormatId = _unitOfWork.AppDbContext.AttendanceSetting.FirstOrDefault(m => m.CompanyId == _httpContext.User.GetCompanyId()).TimeFormatId;

                // Re-calculate attendances
                foreach (var attendance in attendances)
                {
                    // Update total working hours
                    attendance.TotalWorkingTime = CalculateTotalWorkingTime(attendance, attendance.User.WorkingType, timeFormatId);

                    // Update attendance type
                    var zero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                    attendance.Type = attendance.Type == (short)AttendanceType.Holiday
                                    ? attendance.Type
                                    : Helpers.CheckStatusAttendance(
                                        (attendance.StartD - zero).TotalSeconds,
                                        (attendance.EndD - zero).TotalSeconds,
                                        (attendance.ClockInD - zero).TotalSeconds,
                                        (attendance.ClockOutD - zero).TotalSeconds,
                                        attendance.User.WorkingType.CheckClockOut,
                                        true);

                    // Update attendances
                    _unitOfWork.AttendanceRepository.Update(attendance);
                }

                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReCalculateAttendance");
            }
        }

        public void Recheck(string rangeTime, int companyId, string fromAccessTime, string toAccessTime)
        {
            try
            {
                DateTime firstTime;
                DateTime lastTime;
                if (fromAccessTime == null || toAccessTime == null)
                {
                    firstTime = DateTime.Now.AddMonths(-1).Date;
                    lastTime = DateTime.Now.Date;
                }
                else
                {
                    firstTime = DateTimeHelper.ConverStringToDateTime(fromAccessTime);
                    lastTime = DateTimeHelper.ConverStringToDateTime(toAccessTime);
                }

                List<DateTime> allDates = new List<DateTime>();
                for (DateTime i = firstTime; i <= lastTime; i = i.AddDays(1))
                {
                    allDates.Add(i);
                }
                List<Company> companies = new List<Company>();
                if (companyId == 0)
                {
                    //Get List of company that enable Time Attendance
                    companies = _unitOfWork.CompanyRepository.GetCompaniesByPlugin(Constants.PlugIn.TimeAttendance);
                }
                else
                {
                    var company = _unitOfWork.CompanyRepository.GetById(companyId);
                    if (company != null)
                    {
                        var valid = _unitOfWork.CompanyRepository.CheckCompanyByPlugin(Constants.PlugIn.TimeAttendance, companyId);
                        if (valid)
                            companies.Add(company);
                    }
                }
                _unitOfWork.AppDbContext.ChangeTracker.AutoDetectChangesEnabled = false;

                foreach (var company in companies)
                {
                    var buildingDefault = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(company.Id);
                    TimeZoneInfo zone = buildingDefault.TimeZone.ToTimeZoneInfo();
                    IQueryable<User> users = _unitOfWork.UserRepository
                        .GetByCompanyId(company.Id, new List<int> { (short)Status.Valid })
                        .Select(m => new User()
                        {
                            Id = m.Id,
                            CompanyId = m.CompanyId,
                            WorkingType = m.WorkingType,
                            WorkType = m.WorkType,
                            WorkingTypeId = m.WorkingTypeId,
                            FirstName = m.FirstName
                        });

                    var attendanceSetting = _unitOfWork.AttendanceRepository.GetAttendanceSetting(company.Id);
                    foreach (var item in allDates)
                    {
                        foreach (User user in users)
                        {
                            // if working time type null, set default to user
                            if (!user.WorkingTypeId.HasValue)
                            {
                                var workingTypeId = _unitOfWork.WorkingRepository.GetWorkingTypeDefault(companyId);
                                var userUpdate = _unitOfWork.UserRepository.GetById(user.Id);
                                userUpdate.WorkingTypeId = workingTypeId;
                                _unitOfWork.UserRepository.Update(userUpdate);
                                _unitOfWork.Save();
                                user.WorkingTypeId = workingTypeId;
                            }

                            RecheckAttendance_test(user, item.Date, zone, attendanceSetting.TimeFormatId);
                        }
                    }
                }
                _unitOfWork.AppDbContext.ChangeTracker.AutoDetectChangesEnabled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Recheck");
            }


        }


        /// <summary>
        /// Calcualte total working time 
        /// </summary>
        /// <param name="attendance"> Given working type </param>
        /// 
        private double CalculateTotalWorkingTime(Attendance attendance, WorkingType workingType, int timeFormatId)
        {
            if (!workingType.CheckClockOut) {
                return 0;
            }

            if (workingType.WorkingHourType == (int)WorkingHourType.TotalInCompany)
            {
                var zero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var clockIn = attendance.ClockInD;
                var clockOut = attendance.ClockOutD;

                // If clock-In or clock-out is same as zero (1970/1/1 00:00:00) totalWorkingHours is 0
                if (clockIn == zero || clockOut == zero)
                {
                    return 0;
                }

                // Calculate totalWorkingTime = Clock-Out - Clock-In
                if (timeFormatId == (int)TimeFormatType.HHmm)
                {
                    var seconds = clockIn.Second;
                    clockIn = clockIn.AddSeconds(-seconds);

                    seconds = clockOut.Second;
                    clockOut = clockOut.AddSeconds(-seconds);
                }

                TimeSpan totalWorkingTime;

                if (workingType.CheckLunchTime)
                {
                    TimeZoneInfo cstZone = null;
                    string timezoneId;

                    // Get tinezone about this User.
                    // If attendance doesn't have timezone information, get from account linked this user.
                    if (string.IsNullOrEmpty(attendance.TimeZone))
                    {
                        var userAccountId = _unitOfWork.AppDbContext.User.FirstOrDefault(m => m.Id == attendance.UserId).AccountId;
                        timezoneId = _unitOfWork.AppDbContext.Account.FirstOrDefault(m => m.Id == userAccountId).TimeZone;
                    }
                    // If attendance has tinezone information, use attendance's.
                    else
                    {
                        timezoneId = attendance.TimeZone;
                    }

                    // Get TimeZoneInfo for getting offset about this attendance.
                    cstZone = timezoneId.ToTimeZoneInfo();

                    var offset = cstZone.BaseUtcOffset;

                    // Calculate clock-in & out including offset.
                    clockIn += offset;
                    clockOut += offset;

                    var diffDate = DateTime.UtcNow.Date - attendance.Date;

                    // Get lunchTime information from setting(WorkingTimeType).
                    var lunchTime = JsonConvert.DeserializeObject<LunchTime>(workingType.LunchTime);
                    var lunchStart = DateTime.ParseExact(lunchTime.Start, "HH:mm", null).Add(-diffDate);
                    var lunchEnd = DateTime.ParseExact(lunchTime.End, "HH:mm", null).Add(-diffDate);

                    // Compare condition to calculate lunch time.
                    // if the attendance satisfy below condition, attendance should be calcuated including lunch time setting.
                    // ( * This means that 'totalWorkingHours' is calculated except during lunch time. )
                    if (clockIn >= lunchStart || clockOut <= lunchEnd)
                    {
                        totalWorkingTime = clockOut - clockIn;
                    }
                    // if the attendance doesn't satisfy the condition, attendance should be calculated without lunch time setting.
                    // (* This means that 'totalWorkingHours' is just calculated from clock-in to clock-out. )
                    else
                    {
                        // Calculate lunch time
                        totalWorkingTime = (lunchStart - clockIn) + (clockOut - lunchEnd);
                    }
                }
                else
                {
                    totalWorkingTime = clockOut - clockIn;
                }

                if (totalWorkingTime.TotalSeconds <= 0)
                {
                    // 1) There is not clock-out. ( < 0 )
                    // 2) There are not clock-In and clock-out. ( = 0 )
                    return 0;
                }
                else
                {
                    return totalWorkingTime.TotalSeconds;
                }
            }

            // Get all normal access events in Today.
            var workingTimes = JsonConvert.DeserializeObject<List<WorkingTime>>(workingType.WorkingDay);
            var workingTime = workingTimes.First(m => m.Name == attendance.Date.DayOfWeek.ToString());
            List<EventLog> eventLogs;

            if (TimeSpan.Parse(workingTime.Start) > TimeSpan.Parse(workingTime.End))
            {
                if (string.IsNullOrEmpty(workingTime.StartTimeWorking))
                    workingTime.StartTimeWorking = "00:00";

                DateTime start = attendance.StartD.Add(TimeSpan.Parse(workingTime.StartTimeWorking) - TimeSpan.Parse(workingTime.Start));
                eventLogs = _unitOfWork.EventLogRepository.GetAccessNormalByUserAndDateRange(attendance.UserId, start, start.AddDays(1)).ToList();
            }
            else
            {
                eventLogs = _unitOfWork.EventLogRepository.GetAllNormalAccessEventByUser(attendance.UserId, attendance.Date);
            }


            double totalTime = 0;
            EventLog lastIn = null;
            foreach (EventLog eventLog in eventLogs)
            {
                if (eventLog.Antipass == "In" && lastIn == null)
                {
                    lastIn = eventLog;
                }
                else if (eventLog.Antipass == "In" && lastIn != null)
                {
                    lastIn = eventLog;
                }
                else if (eventLog.Antipass == "Out" && lastIn != null)
                {
                    if (timeFormatId == (int)TimeFormatType.HHmm)
                    {
                        var sec1 = eventLog.EventTime.Second;
                        var sec2 = lastIn.EventTime.Second;

                        totalTime += eventLog.EventTime.AddSeconds(-sec1).Subtract(lastIn.EventTime.AddSeconds(-sec2)).TotalSeconds;
                    }
                    else
                    {
                        totalTime += eventLog.EventTime.Subtract(lastIn.EventTime).TotalSeconds;
                    }

                    lastIn = null;
                }
            }
            // Console.WriteLine("Update time attendance {0} event logs", eventLogs.Count());

            return totalTime;
        }

        public void RecheckAttendance_test(User user, DateTime startDate, TimeZoneInfo tzInfo, int timeFormatId)
        {
            try
            {
                DateTime zero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                TimeSpan companyOffset = tzInfo.BaseUtcOffset;
                bool isExistAttendance = true;

                // Get attendance
                Attendance attendance = _unitOfWork.AppDbContext.Attendance.FirstOrDefault(m => m.UserId == user.Id && m.Date == startDate.Date);

                // Get workingType
                if (user.WorkingType == null)
                {
                    user.WorkingType = _unitOfWork.AppDbContext.WorkingType.FirstOrDefault(m => m.Id == user.WorkingTypeId);
                }

                // Check if the attendance is null or not.
                // If it is null, make a new attendance.
                if (attendance == null)
                {
                    isExistAttendance = false;
                    attendance = new Attendance();
                }
                // If this attendance had edited manually, API shouldn't recheck this attendance.
                else if (attendance.EditedBy != null)
                {
                    return;
                }

                var listWorking = JsonConvert.DeserializeObject<List<WorkingTime>>(user.WorkingType.WorkingDay);
                var workTime = listWorking.FirstOrDefault(m => m.Name == startDate.DayOfWeek.ToString());

                TimeSpan diffDate = DateTime.Now.Date - startDate.Date;
                // Reset this attendance.
                attendance.UserId = user.Id;
                attendance.Date = startDate.Date;
                attendance.CompanyId = user.CompanyId;
                attendance.ClockInD = zero;
                attendance.ClockOutD = zero;
                attendance.StartD = DateTime.ParseExact(workTime.Start, "HH:mm", null).Subtract(diffDate).Subtract(companyOffset);
                attendance.EndD = DateTime.ParseExact(workTime.End, "HH:mm", null).Subtract(diffDate).Subtract(companyOffset);
                attendance.EndD = Helpers.ReFixEndDAttendance(attendance.StartD, attendance.EndD);
                attendance.WorkingTime = JsonConvert.SerializeObject(workTime);

                attendance.TimeZone = string.IsNullOrEmpty(attendance.TimeZone) ? tzInfo.Id : attendance.TimeZone;

                // If today is holiday, write a attendanceType as holiday.
                if (workTime.Type == Enum.GetName(typeof(AttendanceType), (short)AttendanceType.Holiday))
                {
                    attendance.Type = (short)AttendanceType.Holiday;
                }
                else
                {
                    attendance.Type = (short)AttendanceType.AbsentNoReason;
                }

                // Check record holiday in database
                var holidayAttendance = _unitOfWork.AppDbContext.Holiday.FirstOrDefault(h =>
                    h.StartDate <= attendance.Date && attendance.Date <= h.EndDate
                    && !h.IsDeleted && h.CompanyId == attendance.CompanyId);
                if (holidayAttendance != null)
                {
                    attendance.Type = (short)AttendanceType.Holiday;
                }

                TimeZoneInfo zone = attendance.TimeZone.ToTimeZoneInfo();

                // Get all normal access events in Today.
                IQueryable<EventLog> eventLogs;
                if (TimeSpan.Parse(workTime.Start) > TimeSpan.Parse(workTime.End))
                {
                    if (string.IsNullOrEmpty(workTime.StartTimeWorking))
                        workTime.StartTimeWorking = "00:00";

                    DateTime start = attendance.StartD.Add(TimeSpan.Parse(workTime.StartTimeWorking) - TimeSpan.Parse(workTime.Start));
                    eventLogs = _unitOfWork.EventLogRepository.GetAccessNormalByUserAndDateRange(user.Id, start, start.AddDays(1));
                }
                else
                {
                    var offSet = zone.BaseUtcOffset;
                    var localTime = startDate.Subtract(offSet);
                    eventLogs = _unitOfWork.AppDbContext.EventLog.Include(m => m.Icu)
                        .Where(m => m.UserId == user.Id
                               && m.EventType == (short)EventType.NormalAccess
                               && m.EventTime >= localTime
                               && m.EventTime < localTime.AddDays(1));
                }

                if (eventLogs.Any())
                {
                    // Write clock-in/out
                    if (!user.WorkingType.CheckClockOut)
                    {
                        attendance.ClockInD = eventLogs.Where(m => m.Antipass.ToLower() == "in").OrderBy(m => m.EventTime).Select(m => m.EventTime).FirstOrDefault();
                    }
                    else if (user.WorkingType.UseClockOutDevice)
                    {
                        attendance.ClockInD = eventLogs.Where(m => m.Antipass.ToLower() == "in").OrderBy(m => m.EventTime).Select(m => m.EventTime).FirstOrDefault();
                        attendance.ClockOutD = eventLogs.Where(m => m.Antipass.ToLower() == "out").OrderByDescending(m => m.EventTime).Select(m => m.EventTime).FirstOrDefault();
                    }
                    else if (!user.WorkingType.UseClockOutDevice)
                    {
                        attendance.ClockInD = eventLogs.OrderBy(m => m.EventTime).Select(m => m.EventTime).FirstOrDefault();
                        attendance.ClockOutD = eventLogs.OrderByDescending(m => m.EventTime).Select(m => m.EventTime).FirstOrDefault();
                    }

                    // Reset to "zero" if the attendance's clock-in/out time is same as DateTime's default value.
                    if (attendance.ClockInD.Equals(default)) { attendance.ClockInD = zero; }
                    if (attendance.ClockOutD.Equals(default)) { attendance.ClockOutD = zero; }
                }

                // Judge attendanceType
                attendance.Type = JudgeAttendanceType(attendance, user.WorkingType);
                // Calculate total working time
                attendance.TotalWorkingTime = CalculateTotalWorkingTime(attendance, user.WorkingType, timeFormatId);

                // Add/Update attendance
                if (isExistAttendance)
                {
                    _unitOfWork.AttendanceRepository.Update(attendance);
                }
                else
                {
                    _unitOfWork.AttendanceRepository.Add(attendance);
                }

                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RecheckAttendance_test");
            }

        }

        public async Task<Attendance> AddClockInOutAsync(User user, string inOut, DateTime epoch, TimeZoneInfo tzInfo, int timeFormatId)
        {
            try
            {
                Console.WriteLine($"##-## [Attendance] 2");

                await Task.Delay(1);
                DateTime start = DateTime.Now;

                // Write clock-in / clock-out time
                Attendance attendance = WriteAttendance(user, inOut, epoch, tzInfo);

                DateTime start1 = DateTime.Now;

                if (user.WorkingType.CheckClockOut)
                {
                    List<EventLog> eventLogs = _unitOfWork.EventLogRepository.GetAllNormalAccessEventByUser(attendance.UserId, attendance.Date);

                    //We dont process when number of log too big, should be issue here
                    if (eventLogs.Count < Constants.TimeAttendanceEventLogLimit)
                    {
                        WorkingType workingTypes = _unitOfWork.WorkingRepository.GetById(user.WorkingTypeId.Value);

                        // Calculate total working time
                        attendance.TotalWorkingTime = CalculateTotalWorkingTime(attendance, workingTypes, timeFormatId);

                        _unitOfWork.AttendanceRepository.Update(attendance);
                        _unitOfWork.Save();
                    }
                }

                DateTime start2 = DateTime.Now;

                //// get attendance request
                //// check approved
                //var checkLeaveAttendances = _unitOfWork.AppDbContext.AttendanceLeave
                //    .Where(x => x.Status == (short)AttendanceStatus.Approved &&
                //                x.Start <= attendance.Date && x.End >= attendance.Date &&
                //                attendance.UserId == x.UserId);

                //if (checkLeaveAttendances.Any())
                //{
                //    attendance.Type = checkLeaveAttendances.ToList()[0].Type;
                //}

                //_unitOfWork.AttendanceRepository.Update(attendance);
                //_unitOfWork.Save();
                DateTime end = DateTime.Now;
                //Console.WriteLine("Update time attendance for user {0} take {1} - {2} - {3}", user.FirstName, end.Subtract(start).TotalMilliseconds, start1.Subtract(start).TotalMilliseconds, start2.Subtract(start1).TotalMilliseconds);

                // notify user checkin first of day
                var eventLogsNormal = _unitOfWork.EventLogRepository.GetAllNormalAccessEventByUser(attendance.UserId, attendance.Date.ConvertToSystemTime(attendance.TimeZone));
                if (eventLogsNormal.Count == 1)
                {
                    SendMessageToUserCheckinFirst(user, attendance);
                }

                // notify user checkin late
                // var attendanceSetting = _unitOfWork.AttendanceRepository.GetAttendanceSetting(attendance.CompanyId);
                // if (attendance.Type == (short)AttendanceType.LateIn && attendanceSetting.EnableNotifyCheckinLate)
                // {
                //     SendMessageToUserCheckinLate(attendance.UserId, attendance.CompanyId);
                // }

                return attendance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddClockInOutAsync");
                return null;
            }
        }

        private async void SendMessageToUserCheckinFirst(User user, Attendance attendance)
        {
            IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
            Company company = unitOfWork.CompanyRepository.GetById(attendance.CompanyId);
            
            try
            {
                if (!user.AccountId.HasValue)
                {
                    // user not map account
                    return;
                }

                string language = unitOfWork.AccountRepository.GetById(user.AccountId.Value).Language;
                if (string.IsNullOrEmpty(language))
                {
                    language = Helpers.GetStringFromValueSetting(unitOfWork.SettingRepository.GetLanguage(attendance.CompanyId).Value);
                }
                var accountCultureInfo = new CultureInfo(language);
                string contentNotification = AttendanceResource.ResourceManager.GetString("msgNotifyCheckinFirst", accountCultureInfo);
                contentNotification += $" ({attendance.Date:yyyy-MM-dd})";
                
                var messaging = FirebaseMessaging.DefaultInstance;
                if (messaging != null)
                {
                    var topic = $"{company.Code}_{user.AccountId}";
                    var fbMessage = new Message()
                    {
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = MessageResource.ResourceManager.GetString("FB_Title_NormalAccess", accountCultureInfo),
                            Body = contentNotification,
                        },
                        Topic = topic,
                        Data = new Dictionary<string, string>()
                        {
                            {"type", "info"},
                        }
                    };
                    var res = await messaging.SendAsync(fbMessage);
                }

                // add new notification
                unitOfWork.NotificationRepository.AddNotification(new Notification()
                {
                    CompanyId = user.CompanyId,
                    Type = (short)NotificationType.NotificationAttendance,
                    CreatedOn = DateTime.Now,
                    Status = false,
                    ReceiveId = user.AccountId.Value,
                    Content = contentNotification,
                    RelatedUrl = "/notifications"
                });
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Attendance Checkin Late Exception]: {e.Message} {e.StackTrace}");
                Console.WriteLine($"UserId = {user.Id} - CompanyId = {attendance.CompanyId} - Name: {company?.Name}");
            }

            unitOfWork.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="inOut"></param>
        /// <param name="epoch"></param>
        /// <param name="buildingId"></param>
        public Attendance AddClockInOut(User user, string inOut, DateTime epoch, TimeZoneInfo tzInfo)
        {
            try
            {
                Attendance attendance = null;
                if (user != null)
                {
                    var offSet = tzInfo.BaseUtcOffset;
                    var date = epoch + offSet;
                    attendance = _unitOfWork.AttendanceRepository.GetAttendanceAlreadyCreated(user.Id, user.CompanyId, date.Date);
                    if (attendance != null)
                    {
                        var zero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        var startD = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        var EndD = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        WorkingType workingTypes = _unitOfWork.WorkingRepository.GetById(user.WorkingTypeId.Value);
                        var listWorkings = JsonConvert.DeserializeObject<List<WorkingTime>>(workingTypes.WorkingDay);
                        foreach (var timeWork in listWorkings)
                        {
                            if (timeWork.Name == date.DayOfWeek.ToString())
                            {
                                String[] strStart = timeWork.Start.Split(':', ' ');
                                String[] strEnd = timeWork.End.Split(':', ' ');
                                var hourStart = Convert.ToInt32(strStart.FirstOrDefault());
                                var hourEnd = Convert.ToInt32(strEnd.FirstOrDefault());
                                var minutesStart = Convert.ToInt32(strStart[strStart.Length - 1]);
                                var minutesEnd = Convert.ToInt32(strEnd[strEnd.Length - 1]);

                                startD = date.Date.AddHours(hourStart).AddMinutes(minutesStart) - offSet;
                                EndD = date.Date.AddHours(hourEnd).AddMinutes(minutesEnd) - offSet;

                                if (timeWork.Type == Enum.GetName(typeof(AttendanceType), (short)AttendanceType.Holiday))
                                    attendance.Type = (short)AttendanceType.Holiday;
                                //else
                                //    attendance.Type = (short)AttendanceType.Normal;

                                // [Edward] This 'break' is just for the current system.
                                // If the system can have more working time in a day, this 'break' should be removed.
                                break;
                            }
                        }

                        attendance.StartD = startD;
                        attendance.EndD = EndD;

                        if (inOut.ToLower() == Constants.Attendance.In.ToLower())
                        {
                            if (attendance.ClockInD > attendance.ClockOutD)
                            {
                                attendance.ClockOutD = zero;
                            }

                            if (attendance.ClockInD == zero)
                            {
                                // write clock-in
                                attendance.ClockInD = date - offSet;

                                // write timezone only when the clock-in is written.
                                attendance.TimeZone = tzInfo.Id;

                                // write attendance type about clock-in
                                attendance.Type = JudgeAttendanceType(attendance, workingTypes);
                            }
                        }
                        else
                        {
                            if (workingTypes.CheckClockOut)
                            {
                                // write clock-out
                                attendance.ClockOutD = date - offSet;

                                // write attendance type about clock-out
                                attendance.Type = JudgeAttendanceType(attendance, workingTypes);
                            }
                        }

                        _unitOfWork.AttendanceRepository.Update(attendance);
                        _unitOfWork.Save();
                    }
                    else
                    {
                        WorkingType workingType = _unitOfWork.WorkingRepository.GetById(user.WorkingTypeId.Value);
                        var listWorking = JsonConvert.DeserializeObject<List<WorkingTime>>(workingType.WorkingDay);
                        foreach (var timeWork in listWorking)
                        {
                            if (timeWork.Name == date.DayOfWeek.ToString())
                            {
                                String[] strStart = timeWork.Start.Split(':', ' ');
                                String[] strEnd = timeWork.End.Split(':', ' ');
                                var hourStart = Convert.ToInt32(strStart.FirstOrDefault());
                                var hourEnd = Convert.ToInt32(strEnd.FirstOrDefault());
                                var minutesStart = Convert.ToInt32(strStart[strStart.Length - 1]);
                                var minutesEnd = Convert.ToInt32(strEnd[strEnd.Length - 1]);

                                var start = date.Date.AddHours(hourStart).AddMinutes(minutesStart) - offSet;
                                var end = date.Date.AddHours(hourEnd).AddMinutes(minutesEnd) - offSet;

                                var epochStart = (start - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                                var epochEnd = (end - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                                var zero = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

                                var jsonString = JsonConvert.SerializeObject(timeWork);
                                var newAttendance = new Attendance
                                {
                                    UserId = user.Id,
                                    Date = date.Date,
                                    CompanyId = user.CompanyId,
                                    StartD = start,
                                    ClockInD = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                    ClockOutD = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                                    EndD = end,
                                    WorkingTime = jsonString
                                };

                                if (timeWork.Type == Enum.GetName(typeof(AttendanceType), (short)AttendanceType.Holiday))
                                {
                                    newAttendance.Type = (short)AttendanceType.Holiday;
                                }

                                // Check In/Out
                                if (inOut.ToLower() == Constants.Attendance.In.ToLower())
                                {
                                    var epochClockIn = (date - offSet - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                                    if (newAttendance.ClockInD == new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                                        newAttendance.ClockInD = date - offSet;
                                    newAttendance.Type = newAttendance.Type == (short)AttendanceType.Holiday
                                        ? newAttendance.Type
                                        : Helpers.CheckStatusAttendance(epochStart, epochEnd, epochClockIn, zero, workingType.CheckClockOut, false);
                                }
                                else
                                {
                                    if (workingType.CheckClockOut)
                                    {
                                        var epochClockOut = (date - offSet - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
                                        newAttendance.ClockOutD = date - offSet;
                                        newAttendance.Type = newAttendance.Type == (short)AttendanceType.Holiday
                                            ? newAttendance.Type
                                            : Helpers.CheckStatusAttendance(epochStart, epochEnd, zero, epochClockOut, workingType.CheckClockOut, false);
                                    }
                                }
                                _unitOfWork.AttendanceRepository.Add(newAttendance);
                                attendance = newAttendance;
                                _unitOfWork.Save();
                            }

                        }
                    }
                }
                return attendance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddClockInOut");
                return null;
            }
        }

        /// <summary>
        /// Write an attendance.
        /// </summary>
        /// <param name="user"> an user information </param>
        /// <param name="inOut"> in or out </param>
        /// <param name="time"> event time </param>
        /// <param name="tzInfo"> timezone information of this event </param>
        /// <returns></returns>
        public Attendance WriteAttendance(User user, string inOut, DateTime time, TimeZoneInfo tzInfo)
        {
            try
            {
                // check if the user is null or not.
                // If null, return null attendance. ( = null )
                if (user == null)
                {
                    return null;
                }

                Attendance attendance = null;
                TimeSpan offSet = tzInfo.BaseUtcOffset;
                time += offSet;
                DateTime zero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                bool isExistAttendance = true;

                // Check if the user's workingType is null or not.
                // If null, get the workingType from DB.
                if (user.WorkingType == null)
                {
                    user.WorkingType = _unitOfWork.AppDbContext.WorkingType.FirstOrDefault(m => m.Id == user.WorkingTypeId);
                }

                var listWorking = JsonConvert.DeserializeObject<List<WorkingTime>>(user.WorkingType.WorkingDay);
                var workTime = listWorking.FirstOrDefault(m => m.Name == time.DayOfWeek.ToString());
                // Get date for attendance
                DateTime dateRecordAttendance = Helpers.GetDateWriteAttendance(time, workTime.StartTimeWorking, workTime.Start, workTime.End);
                attendance = _unitOfWork.AttendanceRepository.GetAttendanceAlreadyCreated(user.Id, user.CompanyId, dateRecordAttendance.Date);

                // Check if the attendance is null or not.
                // If it is null, make a new attendance.
                if (attendance == null)
                {
                    isExistAttendance = false;

                    TimeSpan diffDate = DateTime.Now.Date - time.Date;

                    attendance = new Attendance()
                    {
                        UserId = user.Id,
                        Date = dateRecordAttendance,
                        CompanyId = user.CompanyId,
                        ClockInD = zero,
                        ClockOutD = zero,
                        StartD = DateTime.ParseExact(workTime.Start, "HH:mm", null).Subtract(diffDate).Subtract(offSet),
                        EndD = DateTime.ParseExact(workTime.End, "HH:mm", null).Subtract(diffDate).Subtract(offSet),
                        WorkingTime = JsonConvert.SerializeObject(workTime)
                    };
                    attendance.EndD = Helpers.ReFixEndDAttendance(attendance.StartD, attendance.EndD);

                    // If today is holiday, write a attendanceType as holiday.
                    if (workTime.Type == Enum.GetName(typeof(AttendanceType), (short)AttendanceType.Holiday))
                    {
                        attendance.Type = (short)AttendanceType.Holiday;
                    }
                    // Check record holiday in database
                    var holidayAttendance = _unitOfWork.AppDbContext.Holiday.FirstOrDefault(h =>
                        h.StartDate <= attendance.Date && attendance.Date <= h.EndDate
                                                       && !h.IsDeleted && h.CompanyId == attendance.CompanyId);
                    if (holidayAttendance != null)
                    {
                        attendance.Type = (short)AttendanceType.Holiday;
                    }
                }

                // Check if this user is using an workingType that have checkClockOut option.
                if (user.WorkingType.CheckClockOut)
                {
                    // If this user is using CheckClockOut option, check if UseClockOutDevice option.
                    if (user.WorkingType.UseClockOutDevice)
                    {
                        // Existing logic. ( distinguish In/Out )
                        if (inOut.ToLower() == Constants.Attendance.In.ToLower())
                        {
                            if (attendance.ClockInD == zero)
                            {
                                // write clock-in time
                                attendance.ClockInD = time - offSet;
                                // write timezone only when the clock-in is written.
                                attendance.TimeZone = tzInfo.Id;
                            }
                        }
                        else if (inOut.ToLower() == Constants.Attendance.Out.ToLower())
                        {
                            // write clock-out time
                            attendance.ClockOutD = time - offSet;
                        }
                    }
                    else
                    {
                        // If there isn't clock-out device, check whether there is a clock-in time.
                        // If there is a clock-in time in attendance, write this event as clock-out.
                        // If there isn't clock-in time in attendance(or there isn't attendance), write this event as clock-in(or make a new attendance and write clock-in).
                        if (attendance.ClockInD == zero)
                        {
                            // write clock-in time
                            attendance.ClockInD = time - offSet;
                            // write timezone only when the clock-in is written.
                            attendance.TimeZone = tzInfo.Id;
                        }
                        else
                        {
                            // write clock-out time
                            attendance.ClockOutD = time - offSet;
                        }
                    }
                }
                else
                {
                    // Check in/out.
                    // If "In", check if there is a clock-in time in attendance.
                    // If "Out", ignore this event.
                    if (inOut.ToLower() == Constants.Attendance.In.ToLower() && attendance.ClockInD == zero)
                    {
                        // write clock-in time
                        attendance.ClockInD = time - offSet;
                        // write timezone only when the clock-in is written.
                        attendance.TimeZone = tzInfo.Id;
                    }
                }

                // Judge attendanceType
                if (inOut.ToLower() == "in" && attendance.ClockOutD != zero && attendance.EndD > time.ConvertToSystemTime(attendance.TimeZone))
                {
                    // ignored calculate attendance type
                }
                else
                {
                    attendance.Type = JudgeAttendanceType(attendance, user.WorkingType);
                }

                // Add/Update attendance
                if (isExistAttendance)
                {
                    _unitOfWork.AttendanceRepository.Update(attendance);
                }
                else
                {
                    _unitOfWork.AttendanceRepository.Add(attendance);
                }
                _unitOfWork.Save();

                return attendance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WriteAttendance");
                return null;
            }
        }

        public Attendance GetByIdAndCompanyId(int companyId, int attendanceId)
        {
            return _unitOfWork.AttendanceRepository.GetAttendanceByIdAndCompanyId(companyId, attendanceId);
        }

        public AttendanceLeave GetRequestByIdAndCompanyId(int companyId, int attendanceId)
        {
            return _unitOfWork.AttendanceLeaveRepository.GetByIdAndCompanyId(companyId, attendanceId);
        }

        public List<AttendanceLeave> GetRequestByIdsAndCompanyId(int companyId, List<int> attendanceIds)
        {
            return _unitOfWork.AttendanceLeaveRepository.GetByIdsAndCompanyId(companyId, attendanceIds).ToList();
        }

        public double Update(int attendanceId, AttendanceModel model)
        {
            double totalWorkingHours = 0;

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var attendance = GetByIdAndCompanyId(_httpContext.User.GetCompanyId(), attendanceId);

                        totalWorkingHours = attendance.TotalWorkingTime;

                        attendance.ClockInD = model.ClockInD;
                        attendance.ClockOutD = model.ClockOutD;
                        attendance.EditedBy = _httpContext.User.GetAccountId();

                        // Update totalWorkingHours
                        var workingType = _unitOfWork.AppDbContext.User.Include(m => m.WorkingType).Where(m => m.Id == attendance.UserId).FirstOrDefault().WorkingType;
                        if (workingType.CheckClockOut)
                        {
                            if (workingType.WorkingHourType == (int)WorkingHourType.TotalInCompany)
                            {
                                int timeFormatId = 0;

                                var attendanceSetting = _unitOfWork.AttendanceRepository.GetAttendanceSetting(attendance.CompanyId);
                                if (attendanceSetting != null)
                                {
                                    timeFormatId = attendanceSetting.TimeFormatId;
                                }

                                attendance.TotalWorkingTime = CalculateTotalWorkingTime(attendance, workingType, timeFormatId);
                                totalWorkingHours = attendance.TotalWorkingTime;
                            }
                        }


                        // Update attendanceType
                        // This means that this attendance has not changed in this edit
                        // API re-judge automatically attendanceType in this condition.
                        if (attendance.Type == model.Type)
                        {
                            model.Type = JudgeAttendanceType(attendance, workingType);
                        }

                        // If the attendanceType is changed in this edit, the changed type is applied in below source code.
                        _mapper.Map(model, attendance);
                        _unitOfWork.AttendanceRepository.Update(attendance);
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

            return totalWorkingHours;
        }

        public int JudgeAttendanceType(Attendance attendance, WorkingType workingType)
        {
            // Get TimeZone information and OffSet
            var timeZone = attendance.TimeZone;

            if (timeZone == null)
            {
                var accountId = _unitOfWork.AppDbContext.User.FirstOrDefault(m => m.Id == attendance.UserId).AccountId;
                timeZone = _unitOfWork.AppDbContext.Account.FirstOrDefault(m => m.Id == accountId).TimeZone;
            }

            TimeZoneInfo tzInfo = timeZone.ToTimeZoneInfo();

            TimeSpan offset = tzInfo.BaseUtcOffset;

            // Check whether there are any Leave Request in this day.
            var leaveRequest = _unitOfWork.AppDbContext.AttendanceLeave.FirstOrDefault(m =>
                m.Start.AddSeconds(offset.TotalSeconds).Date <= attendance.Date && m.End.AddSeconds(offset.TotalSeconds).Date >= attendance.Date
                                                  && m.Status == (int)AttendanceStatus.Approved
                                                  && m.Type != (short)AttendanceType.LateIn && m.Type != (short)AttendanceType.EarlyOut
                                                  && m.UserId == attendance.UserId);

            if (leaveRequest != null)
            {
                return leaveRequest.Type;
            }

            int attendanceType = attendance.Type;

            if (attendance.Type != (int)AttendanceType.Holiday
                && attendance.Type != (int)AttendanceType.BusinessTrip
                && attendance.Type != (int)AttendanceType.Vacation)
            {
                var zero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                // Check StartD, EndD.
                // If StartD or EndD are same as zero, it means there is not any events from this user in Today.
                if (attendance.StartD == zero || attendance.EndD == zero)
                {
                    TimeSpan diffDate = DateTime.UtcNow.Date - attendance.Date;

                    var workingTime = JsonConvert.DeserializeObject<WorkingTime>(attendance.WorkingTime);

                    attendance.StartD = DateTime.ParseExact(workingTime.Start, "HH:mm", null).Subtract(offset).Subtract(diffDate);
                    attendance.EndD = DateTime.ParseExact(workingTime.End, "HH:mm", null).Subtract(offset).Subtract(diffDate);
                }

                // Update attendanceType
                attendanceType = Helpers.CheckStatusAttendance((attendance.StartD - zero).TotalSeconds,
                                                                (attendance.EndD - zero).TotalSeconds,
                                                                (attendance.ClockInD - zero).TotalSeconds,
                                                                (attendance.ClockOutD - zero).TotalSeconds,
                                                                workingType.CheckClockOut,
                                                                false);
            }

            return attendanceType;
        }

        public IQueryable<AttendanceListModel> ExportAttendanceReport(string name, string departmentIds, string attendanceType, DateTime start, DateTime end, out int totalRecords, out int recordsFiltered)
        {
            var data = _unitOfWork.AppDbContext.Attendance.Include(x => x.User).Include(m => m.User.WorkingType)/*.ThenInclude(user => user.WorkingType)*/
                .Where(c => c.CompanyId == _httpContext.User.GetCompanyId() && c.User.Status != (short)Status.Invalid
                 && !c.User.IsDeleted && c.User.ExpiredDate > DateTime.UtcNow)
            .Select(m => new AttendanceListModel()
            {
                    Id = m.Id,
                    Date = m.Date,
                    Start = (m.StartD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    End = (m.EndD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    WorkingTime = m.WorkingTime,
                    TotalWorkingTime = m.TotalWorkingTime,
                    Type = m.Type,
                    UserId = m.UserId,
                    ClockIn = (m.ClockInD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    ClockOut = (m.ClockOutD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    UserName = m.User.FirstName,
                    User = m.User
                });

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(name))
            {
                var normalizedName = name.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x => x.UserName.RemoveDiacritics().ToLower().Contains(normalizedName)).AsQueryable();
            }

            if (!string.IsNullOrEmpty(attendanceType))
            {
                String[] strlist = attendanceType.Split(",");
                if (strlist.Any())
                {
                    data = data.Where(x => strlist.Contains(x.Type.ToString()));
                }
            }

            


            if (!string.IsNullOrEmpty(departmentIds))
            {
                String[] strlist = departmentIds.Split(",");
                if (strlist.Any())
                {
                    var listDepartmentSearch = new List<int>();
                    List<int> listDepartmentIds = strlist.Select(int.Parse).ToList();
                    var departments = _unitOfWork.DepartmentRepository.GetMany(x => listDepartmentIds.Contains(x.ParentId ?? 0)
                        && !x.IsDeleted && x.CompanyId == _httpContext.User.GetCompanyId()).ToList();
                    foreach (var departmentId in listDepartmentIds)
                    {
                        var childDepartmentId = departments.Where(x => x.ParentId == departmentId).Select(x => x.Id).ToList();

                        if (childDepartmentId.Any())
                        {
                            listDepartmentSearch.AddRange(childDepartmentId);
                        }
                        listDepartmentSearch.Add(departmentId);
                    }
                    
                    data = data.Where(x => listDepartmentSearch.Contains(x.User.DepartmentId));
                }
            }
            else
            {
                // check permission account type dynamic role department level
                if (_httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
                {
                    var companyId = _httpContext.User.GetCompanyId();
                    var accountId = _httpContext.User.GetAccountId();
                    var departmentRoleLevelIds = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
                    if (departmentRoleLevelIds.Any())
                    {
                        data = data.Where(x => departmentRoleLevelIds.Contains(x.User.DepartmentId));
                    }
                }
            }

            data = start <= end
                ? data.Where(o => o.Date >= start && o.Date <= end)
                : data.Where(o => o.Date <= start && o.Date.AddDays(1) <= end);

            data = data.OrderBy(m => m.Date).ThenBy(m => m.UserId);

            recordsFiltered = data.Count();

            return data;
        }
       
        public void AddWeeklyData(ref int rowIndex, ref double weeklyWorkingHours, ref double weeklyDiffTime, ref ExcelWorksheet worksheet, CultureInfo culture, int timeformat, bool IsLast = false)
        {
            var colIndex = Array.IndexOf(_excelHeader, "headerTotalTime") + 1;

            worksheet.Cells[rowIndex, colIndex].Style.Font.Color.SetColor(System.Drawing.Color.SteelBlue);
            worksheet.Cells[rowIndex, colIndex++].Value = AttendanceResource.ResourceManager.GetString("lblWeekWorkingHour", culture);

            worksheet.Cells[rowIndex, 2].Value = AttendanceResource.ResourceManager.GetString("lblWeekly", culture);
            worksheet.Cells[rowIndex, 2, rowIndex + 1, 5].Merge = true;

            rowIndex++;

            colIndex = Array.IndexOf(_excelHeader, "headerTotalTime") + 1;

            worksheet.Cells[rowIndex, colIndex++].Value = ConvertSecToTimeStr(weeklyWorkingHours, timeformat);

            weeklyWorkingHours = 0;
            weeklyDiffTime = 0;

            rowIndex++;

            if (!IsLast)
            {
                worksheet.Cells[rowIndex, 2, rowIndex, _excelHeader.Length].Merge = true;

                rowIndex++;

                AddHeaderData(true, false, ref rowIndex, ref worksheet, culture);
            }
        }

        public void AddTotalData(ref int rowIndex, ref double totalWorkingHours, ref double totalDiffTime, ref ExcelWorksheet worksheet, CultureInfo culture, int timeformat, bool IsLast = false)
        {
            var colIndex = Array.IndexOf(_excelHeader, "headerTotalTime") + 1;

            worksheet.Cells[rowIndex, colIndex].Style.Font.Color.SetColor(System.Drawing.Color.SteelBlue);
            worksheet.Cells[rowIndex, colIndex].Style.Font.Bold = true;
            worksheet.Cells[rowIndex, colIndex++].Value = AttendanceResource.ResourceManager.GetString("lblTotalWorkingHour", culture);

            worksheet.Cells[rowIndex, 1].Value = AttendanceResource.ResourceManager.GetString("lblTotal", culture);
            worksheet.Cells[rowIndex, 1, rowIndex + 1, 5].Merge = true;

            // Move to next row
            rowIndex++;

            colIndex = Array.IndexOf(_excelHeader, "headerTotalTime") + 1;

            worksheet.Cells[rowIndex, colIndex].Style.Font.Bold = true;
            worksheet.Cells[rowIndex, colIndex++].Value = ConvertSecToTimeStr(totalWorkingHours, timeformat);

            worksheet.Cells[rowIndex, colIndex].Style.Font.Bold = true;

            totalWorkingHours = 0;
            totalDiffTime = 0;

            // Move to next row
            rowIndex++;
            worksheet.Cells[rowIndex, 1, rowIndex, _excelHeader.Length].Merge = true;
            rowIndex++;
        }

        internal void AddHeaderData(bool checkClockOut, bool isStart, ref int rowIndex, ref ExcelWorksheet worksheet, CultureInfo culture)
        {
            if (checkClockOut)
            {
                for (var i = 0; i < _excelHeader.Length; i++)
                {
                    if (isStart)
                    {
                        worksheet.Cells[rowIndex, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                        worksheet.Cells[rowIndex, i + 1].Style.Font.Bold = true;
                    }
                    worksheet.Cells[rowIndex, i + 1].Value = AttendanceResource.ResourceManager.GetString(_excelHeader[i], culture);
                }
            }
            else
            {
                for (var i = 0; i < _excelHeaderWithOutClockOut.Length; i++)
                {
                    if (isStart)
                    {
                        worksheet.Cells[rowIndex, i + 1].Style.Font.Color.SetColor(System.Drawing.Color.Blue);
                        worksheet.Cells[rowIndex, i + 1].Style.Font.Bold = true;
                    }
                    worksheet.Cells[rowIndex, i + 1].Value = AttendanceResource.ResourceManager.GetString(_excelHeaderWithOutClockOut[i], culture);
                }
            }

            rowIndex++;
        }

        internal void MergeNameColumns(bool checkClockOut, int startRowIndex, int rowIndex, ref ExcelWorksheet worksheet)
        {
            if (checkClockOut)
            {
                worksheet.Cells[startRowIndex, 1, rowIndex + 1, 1].Merge = true;
            }
            else
            {
                worksheet.Cells[startRowIndex, 1, rowIndex - 1, 1].Merge = true;
            }
        }

        public IQueryable<AttendanceListReportModel> GetPaginatedAttendanceRecordEachUser(int userId, DateTime start, DateTime end, int pageNumber, int pageSize, int sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var data = _unitOfWork.AppDbContext.Attendance
                .Where(c => c.CompanyId == _httpContext.User.GetCompanyId() && c.UserId == userId)
                .Select(m => new AttendanceListReportModel()
                {
                    Id = m.Id,
                    Date = m.Date,
                    Start = (m.StartD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    End = (m.EndD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    WorkingTime = m.WorkingTime,
                    Type = m.Type,
                    UserId = m.UserId,
                    ClockIn = (m.ClockInD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    ClockOut = (m.ClockOutD - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds,
                    UserName = m.User.FirstName,
                    DepartmentId = m.User.DepartmentId
                });

            totalRecords = data.Count();

            data = start <= end
                ? data.Where(o => o.Date >= start && o.Date <= end)
                : data.Where(o => o.Date <= start && o.Date.AddDays(1) <= end);

            recordsFiltered = data.Count();
            data = data.OrderBy(m => m.Date);
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return data;
        }

        internal string ConvertSecToTimeStr(double seconds, int timeformat)
        {
            if (seconds <= 0) return "";

            TimeSpan time = TimeSpan.FromSeconds(seconds);

            string result = "";

            switch (timeformat)
            {
                case (int)TimeFormatType.HHmmss:
                    result = string.Format("{0:D2}h {1:D2}m {2:D2}s",
                                            (int)time.TotalHours,
                                            time.Minutes,
                                            time.Seconds);
                    break;
                case (int)TimeFormatType.HHmm:
                    result = string.Format("{0:D2}h {1:D2}m",
                                (int)time.TotalHours,
                                time.Minutes);
                    break;
                case (int)TimeFormatType.HHmmDot:
                    result = string.Format("{0:D2}.{1:D2}",
                                (int)time.TotalHours,
                                time.Minutes);
                    break;
                case (int)TimeFormatType.OnlyHours:
                    result = string.Format("{0:0.0}", time.TotalHours);
                    break;
                default:
                    result = string.Format("{0:D2}h {1:D2}m {2:D2}s",
                                            (int)time.TotalHours,
                                            time.Minutes,
                                            time.Seconds);
                    break;
            }

            return result;
        }

        internal string GetDayOfWeek(DateTime dateTime)
        {
            var day = dateTime.DayOfWeek;
            string week = string.Empty;

            switch (day)
            {
                case DayOfWeek.Monday:
                    week = AttendanceResource.lblMonday;
                    break;
                case DayOfWeek.Tuesday:
                    week = AttendanceResource.lblTuesday;
                    break;
                case DayOfWeek.Wednesday:
                    week = AttendanceResource.lblWednesday;
                    break;
                case DayOfWeek.Thursday:
                    week = AttendanceResource.lblThursday;
                    break;
                case DayOfWeek.Friday:
                    week = AttendanceResource.lblFriday;
                    break;
                case DayOfWeek.Saturday:
                    week = AttendanceResource.lblSaturday;
                    break;
                case DayOfWeek.Sunday:
                    week = AttendanceResource.lblSunday;
                    break;
            }

            return week;
        }

        public Dictionary<string, int> GetAttendanceTypeCount(int companyId, DateTime attendanceDate)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            Account account = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId());
            string timezone = account.TimeZone;
            var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;

            // Calculate time to user timezone
            //DateTime startTime = attendanceDate.ConvertToSystemTime(timezone);
            attendanceDate = attendanceDate.ConvertToSystemTime(offSet);
            List<Attendance> attendances = _unitOfWork.AttendanceRepository.GetAttendancesByDate(companyId, attendanceDate);
            foreach (EnumModel type in EnumHelper.ToEnumList<AttendanceType>())
            {
                int value = attendances.Where(m => m.Type == type.Id).Count();
                result.Add(type.Name, value);
            }
            return result;
        }

        public List<AttendanceLeaveModel> GetAttendanceLeaves(string search, string attendanceType, DateTime start, DateTime end, int status, int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered)
        {
            // check account type dynamic role enable department role
            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();
            var accountType = _httpContext.User.GetAccountType();
            var departmentIds = new List<int>();
            if (accountType == (short)AccountType.DynamicRole)
            {
                departmentIds = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
            }

            var data = _unitOfWork.AppDbContext.AttendanceLeave
                .Include(m => m.User).ThenInclude(m => m.Department)
                .Where(c =>
                    c.CompanyId == companyId
                    && c.User.Status != (short)Status.Invalid && (status == 0 || c.Status == status)
                    && (!departmentIds.Any() || departmentIds.Contains(c.User.DepartmentId)))
                .Select(m => new AttendanceLeaveModel()
                {
                    Id = m.Id,
                    Date = m.Date,
                    Start = m.Start,
                    End = m.End,
                    Type = m.Type,
                    Reason = m.Reason,
                    RejectReason = m.RejectReason,
                    Status = m.Status,
                    UserName = m.User.FirstName,
                    Avatar = m.User.Avatar,
                    UserId = m.UserId,
                    DepartmentName = m.User.Department.DepartName,
                    Creator = m.CreatedBy == 0 || m.CreatedBy == m.User.AccountId
                        ? m.User.FirstName
                        : _unitOfWork.AccountRepository.GetById(m.CreatedBy) != null ? _unitOfWork.AccountRepository.GetById(m.CreatedBy).Username : null,
                });

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = search.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x => x.UserName.RemoveDiacritics().ToLower().Contains(normalizedSearch)).AsQueryable();
            }

            if (!string.IsNullOrEmpty(attendanceType))
            {
                String[] strlist = attendanceType.Split(",");
                if (strlist.Any())
                {
                    data = data.Where(x => strlist.Contains(x.Type.ToString()));
                }
            }

            data = start <= end
                ? data.Where(o => o.Date >= start && o.Date <= end)
                : data.Where(o => o.Date <= start && o.Date.AddDays(1) <= end);

            recordsFiltered = data.Count();
            if (string.IsNullOrEmpty(sortColumn)) sortColumn = "Status";
            data = data.OrderBy($"{sortColumn} {sortDirection}");
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return data.ToList();
        }

        public List<AttendanceLeaveModel> GetAttendanceLeavesUser(LeaveReportFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            if (filter.UserId == 0)
            {
                var user = _unitOfWork.UserRepository.GetUserByAccountId(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId());
                if (user == null)
                {
                    totalRecords = 0;
                    recordsFiltered = 0;
                    return new List<AttendanceLeaveModel>();
                }

                filter.UserId = user.Id;
            }

            var data = _unitOfWork.AttendanceRepository.GetAttendanceLeavesUser(filter, out totalRecords, out recordsFiltered);
            return data.ToList();
        }

        public string AddAttendanceLeave(LeaveRequestModel model)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var user = _unitOfWork.UserRepository.GetById(model.UserId);

                if (user == null || !(!user.IsDeleted && user.CompanyId == companyId))
                {
                    return MessageResource.RecordNotFound;
                }
                if (model.StartD >= model.EndD)
                {
                    return string.Format(MessageResource.InvalidDate, AttendanceResource.lblEndDate, AttendanceResource.lblStartDate);
                }

                var accountManager = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId());
                // convert time start/end to timezone UTC
                if (!string.IsNullOrEmpty(accountManager?.TimeZone))
                {
                    var offSet = accountManager.TimeZone.ToTimeZoneInfo().BaseUtcOffset;
                    model.StartD = model.StartD.ConvertToSystemTime(offSet);
                    model.EndD = model.EndD.ConvertToSystemTime(offSet);
                }
                else
                {
                    var buildingDefault = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(companyId);
                    var offSet = buildingDefault.TimeZone.ToTimeZoneInfo().BaseUtcOffset;
                    model.StartD = model.StartD.ConvertToSystemTime(offSet);
                    model.EndD = model.EndD.ConvertToSystemTime(offSet);
                }

                // check request attendance in db
                // check status: Waiting or Approved
                // check date: start / end
                var listAttendanceLeaves = _unitOfWork.AppDbContext.AttendanceLeave.Where(
                    a => (a.Status == (short)AttendanceStatus.Waiting || a.Status == (short)AttendanceStatus.Approved)
                    && a.UserId == user.Id
                    && ((model.StartD <= a.Start && a.Start <= model.EndD) || (model.StartD <= a.End && a.End <= model.EndD)));

                if (listAttendanceLeaves.Any())
                {
                    return AttendanceResource.requestAttendanceExist;
                }

                var attendanceLeave = new AttendanceLeave()
                {
                    Type = model.Type,
                    Status = (short)AttendanceStatus.Waiting,
                    Start = model.StartD,
                    End = model.EndD,
                    Reason = model.Reason,
                    Date = DateTime.UtcNow,
                    UserId = user.Id,
                    CompanyId = companyId,
                    CreatedBy = _httpContext.User.GetAccountId(),
                };
                _unitOfWork.AttendanceLeaveRepository.Add(attendanceLeave);

                ISettingService settingService = new SettingService(_unitOfWork, _configuration);
                var companyLanguage = settingService.GetLanguage(_httpContext.User.GetAccountId(), companyId);
                var culture = new CultureInfo(companyLanguage);

                // send notification to approval accounts
                INotificationService notificationService = new NotificationService(_unitOfWork, _configuration);
                var attendanceSetting = GetAttendanceSetting(_httpContext.User.GetCompanyId());
                if (attendanceSetting != null && !string.IsNullOrEmpty(attendanceSetting.ApprovarAccounts))
                {
                    if (attendanceSetting.ApprovarAccounts[0] != '[')
                    {
                        attendanceSetting.ApprovarAccounts = $"[{attendanceSetting.ApprovarAccounts}]";
                    }
                    List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(attendanceSetting.ApprovarAccounts);
                    string content = string.Format(AttendanceResource.ResourceManager.GetString("contentRegisterAttenance", culture), user.FirstName);
                    foreach (var accountId in accountIds)
                    {
                        _unitOfWork.NotificationRepository.AddNotification(new Notification()
                        {
                            CompanyId = companyId,
                            Type = (short)NotificationType.NotificationAttendance,
                            CreatedOn = DateTime.Now,
                            Status = false,
                            ReceiveId = accountId,
                            RelatedUrl = "/attendance-leave?id=" + attendanceLeave.Id,
                            Content = content
                        });

                        // pushing notification
                        string title = AttendanceResource.ResourceManager.GetString("titleNoticationLeaveRequest", culture);
                        notificationService.PushingNotificationToUser(Constants.Notification.PushingNotyLeaveRequestReview, attendanceLeave.Id, title, content, accountId, companyId);
                    }
                }

                _unitOfWork.Save();
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAttendanceLeave");
                return null;
            }
        }

        /// <summary>
        /// register attendance leave. If return null => successfully
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public string RegisterAttendanceLeave(AttendanceRegister model)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var user = _unitOfWork.AppDbContext.User.Include(m => m.Account).FirstOrDefault(u =>
                    u.AccountId == _httpContext.User.GetAccountId() && !u.IsDeleted &&
                    u.CompanyId == companyId);

                if (user == null || !(!user.IsDeleted && user.CompanyId == companyId))
                {
                    return MessageResource.RecordNotFound;
                }
                if (model.StartD >= model.EndD)
                {
                    return string.Format(MessageResource.InvalidDate, AttendanceResource.lblEndDate, AttendanceResource.lblStartDate);
                }

                // convert time start/end to timezone UTC
                if (!string.IsNullOrEmpty(user?.Account?.TimeZone))
                {
                    string accountTimezone = user?.Account?.TimeZone;
                    var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;
                    model.StartD = model.StartD.ConvertToSystemTime(offSet);
                    model.EndD = model.EndD.ConvertToSystemTime(offSet);
                }
                else
                {
                    var buildingDefault = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(companyId);
                    var offSet = buildingDefault.TimeZone.ToTimeZoneInfo().BaseUtcOffset;
                    model.StartD = model.StartD.ConvertToSystemTime(offSet);
                    model.EndD = model.EndD.ConvertToSystemTime(offSet);
                }

                // check request attendance in db
                // check status: Waiting or Approved
                // check date: start / end
                var listAttendanceLeaves = _unitOfWork.AppDbContext.AttendanceLeave.Where(
                    a => (a.Status == (short)AttendanceStatus.Waiting || a.Status == (short)AttendanceStatus.Approved)
                    && a.UserId == user.Id
                    && ((model.StartD <= a.Start && a.Start <= model.EndD) || (model.StartD <= a.End && a.End <= model.EndD)));

                if (listAttendanceLeaves.Any())
                {
                    return AttendanceResource.requestAttendanceExist;
                }

                var attendanceLeave = new AttendanceLeave()
                {
                    Type = model.Type,
                    Status = (short)AttendanceStatus.Waiting,
                    Start = model.StartD,
                    End = model.EndD,
                    Reason = model.Reason,
                    Date = DateTime.Now,
                    UserId = user.Id,
                    CompanyId = companyId,
                    CreatedBy = _httpContext.User.GetAccountId(),
                };
                _unitOfWork.AttendanceLeaveRepository.Add(attendanceLeave);

                ISettingService settingService = new SettingService(_unitOfWork, _configuration);
                var companyLanguage = settingService.GetLanguage(_httpContext.User.GetAccountId(), companyId);
                var culture = new CultureInfo(companyLanguage);

                // send notification to approval accounts
                INotificationService notificationService = new NotificationService(_unitOfWork, _configuration);
                var attendanceSetting = GetAttendanceSetting(_httpContext.User.GetCompanyId());
                if (attendanceSetting != null && !string.IsNullOrEmpty(attendanceSetting.ApprovarAccounts))
                {
                    if (attendanceSetting.ApprovarAccounts[0] != '[')
                    {
                        attendanceSetting.ApprovarAccounts = $"[{attendanceSetting.ApprovarAccounts}]";
                    }
                    List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(attendanceSetting.ApprovarAccounts);
                    string content = string.Format(AttendanceResource.ResourceManager.GetString("contentRegisterAttenance", culture), user == null ? _httpContext.User.GetUsername() : user.FirstName);
                    foreach (var accountId in accountIds)
                    {
                        _unitOfWork.NotificationRepository.AddNotification(new Notification()
                        {
                            CompanyId = companyId,
                            Type = (short)NotificationType.NotificationAttendance,
                            CreatedOn = DateTime.Now,
                            Status = false,
                            ReceiveId = accountId,
                            RelatedUrl = "/attendance-leave?id=" + attendanceLeave.Id,
                            Content = content
                        });

                        // pushing notification
                        string title = AttendanceResource.ResourceManager.GetString("titleNoticationLeaveRequest", culture);
                        notificationService.PushingNotificationToUser(Constants.Notification.PushingNotyLeaveRequestReview, attendanceLeave.Id, title, content, accountId, companyId);
                    }
                }

                _unitOfWork.Save();
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RegisterAttendanceLeave");
                return null;
            }
        }

        public bool ApprovedAttendanceLeave(int id, ActionApproval action)
        {
            try
            {
                bool isAccept = action.IsAccept;
                var leaveRequest = _unitOfWork.AppDbContext.AttendanceLeave.FirstOrDefault(a => a.Id == id);
                if (leaveRequest != null)
                {
                    leaveRequest.RejectReason = !isAccept ? action.RejectReason : "";

                    leaveRequest.Status = isAccept ? (short)AttendanceStatus.Approved : (short)AttendanceStatus.Reject;
                    leaveRequest.EditedBy = _httpContext.User.GetAccountId();
                    _unitOfWork.AppDbContext.AttendanceLeave.Update(leaveRequest);

                    // update attendance absence
                    if (isAccept)
                    {
                        TimeSpan offSet = new TimeSpan();
                        var buildings = _unitOfWork.AppDbContext.Building.Where(b => b.CompanyId == _httpContext.User.GetCompanyId() && !b.IsDeleted);
                        if (buildings.Any())
                        {
                            var timeZone = buildings.ToList()[0].TimeZone;
                            TimeZoneInfo tzInfo = timeZone.ToTimeZoneInfo();
                            offSet = tzInfo.BaseUtcOffset;
                        }

                        // convert start/end (stored in db timezone UTC) leave-request to timezone user (timezone building)
                        var leaveRequestStart = leaveRequest.Start.Add(offSet);
                        var leaveRequestEnd = leaveRequest.End.Add(offSet);

                        for (DateTime date = leaveRequestStart.Date; date <= leaveRequestEnd.Date; date = date.AddDays(1))
                        {
                            var attendance = _unitOfWork.AppDbContext.Attendance.FirstOrDefault(m => m.Date == date && m.UserId == leaveRequest.UserId);
                            var zero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                            var user = _unitOfWork.AppDbContext.User.Include(m => m.WorkingType).FirstOrDefault(m => m.Id == leaveRequest.UserId);
                            if (user == null)
                            {
                                _logger.LogError("[ApprovedAttendanceLeave]: user null");
                                return false;
                            }
                            var listWorking = JsonConvert.DeserializeObject<List<WorkingTime>>(user.WorkingType.WorkingDay);
                            var workTime = listWorking.FirstOrDefault(m => m.Name == date.DayOfWeek.ToString());
                            if (workTime == null)
                            {
                                _logger.LogError("[ApprovedAttendanceLeave]: workTime null");
                                return false;
                            }
                            TimeSpan diffDate = DateTime.Now.Date - date.Date;

                            if (attendance != null)
                            {
                                attendance.Type = leaveRequest.Type;
                                _unitOfWork.AppDbContext.Attendance.Update(attendance);
                                _unitOfWork.Save();
                            }
                            else
                            {
                                attendance = new Attendance()
                                {
                                    UserId = user.Id,
                                    Date = date.Date,
                                    CompanyId = user.CompanyId,
                                    ClockInD = zero,
                                    ClockOutD = zero,
                                    StartD = DateTime.ParseExact(workTime.Start, "HH:mm", null).Subtract(diffDate).Subtract(offSet),
                                    EndD = DateTime.ParseExact(workTime.End, "HH:mm", null).Subtract(diffDate).Subtract(offSet),
                                    WorkingTime = JsonConvert.SerializeObject(workTime),
                                    Type = leaveRequest.Type
                                };

                                _unitOfWork.AppDbContext.Attendance.Add(attendance);
                                _unitOfWork.Save();
                            }

                            // add FK Table AttendanceLeaveRequest
                            _unitOfWork.AppDbContext.AttendanceLeaveRequest.Add(new AttendanceLeaveRequest()
                            {
                                AttendanceId = attendance.Id,
                                AttendanceLeaveId = leaveRequest.Id,
                            });
                            _unitOfWork.Save();

                            // update StartDate - request LateIn, EndDate - request EarlyOut
                            if (leaveRequest.Type == (short)AttendanceType.LateIn)
                            {
                                workTime.Start = leaveRequestStart.ToString("HH:mm");
                                attendance.StartD = DateTime.ParseExact(workTime.Start, "HH:mm", null).Subtract(diffDate).Subtract(offSet);
                                attendance.WorkingTime = JsonConvert.SerializeObject(workTime);
                                attendance.Type = Helpers.CheckStatusAttendance((attendance.StartD - zero).TotalSeconds,
                                    (attendance.EndD - zero).TotalSeconds,
                                    (attendance.ClockInD - zero).TotalSeconds,
                                    (attendance.ClockOutD - zero).TotalSeconds,
                                    user.WorkingType.CheckClockOut,
                                    false);
                                _unitOfWork.AttendanceRepository.Update(attendance);
                                _unitOfWork.Save();
                            }
                            else if (leaveRequest.Type == (short)AttendanceType.EarlyOut)
                            {
                                workTime.End = leaveRequestEnd.ToString("HH:mm");
                                attendance.EndD = DateTime.ParseExact(workTime.End, "HH:mm", null).Subtract(diffDate).Subtract(offSet);
                                attendance.WorkingTime = JsonConvert.SerializeObject(workTime);
                                attendance.Type = Helpers.CheckStatusAttendance((attendance.StartD - zero).TotalSeconds,
                                    (attendance.EndD - zero).TotalSeconds,
                                    (attendance.ClockInD - zero).TotalSeconds,
                                    (attendance.ClockOutD - zero).TotalSeconds,
                                    user.WorkingType.CheckClockOut,
                                    false);
                                _unitOfWork.AttendanceRepository.Update(attendance);
                                _unitOfWork.Save();
                            }
                        }
                    }

                    // send notification
                    var companyId = _httpContext.User.GetCompanyId();
                    ISettingService settingService = new SettingService(_unitOfWork, _configuration);
                    var companyLanguage = settingService.GetLanguage(_httpContext.User.GetAccountId(), companyId);
                    var culture = new CultureInfo(companyLanguage);
                    var accountUser = _unitOfWork.AppDbContext.User.FirstOrDefault(u =>
                        u.AccountId == _httpContext.User.GetAccountId() && !u.IsDeleted &&
                        u.CompanyId == companyId);

                    var userReceive = _unitOfWork.UserRepository.GetByUserId(companyId, leaveRequest.UserId);
                    string content = string.Format(
                        AttendanceResource.ResourceManager.GetString(isAccept ? "contentAcceptAttendanceLeave" : "contentRejectAttendanceLeave", culture),
                        accountUser == null ? _httpContext.User.GetUsername() : accountUser.FirstName);
                    if (userReceive.AccountId.HasValue)
                    {
                        _unitOfWork.NotificationRepository.AddNotification(new Notification()
                        {
                            CompanyId = companyId,
                            Type = (short)NotificationType.NotificationAttendance,
                            CreatedOn = DateTime.Now,
                            Status = false,
                            ReceiveId = userReceive.AccountId.Value,
                            RelatedUrl = "/attendance-leave?id=" + leaveRequest.Id,
                            Content = content
                        });

                        // pushing notification
                        string title = AttendanceResource.ResourceManager.GetString("titleNoticationLeaveRequest", culture);
                        INotificationService notificationService = new NotificationService(_unitOfWork, _configuration);
                        notificationService.PushingNotificationToUser(Constants.Notification.PushingNotyLeaveRequest, id, title, content, userReceive.AccountId.Value, companyId);
                    }
                    _unitOfWork.Save();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApprovedAttendanceLeave");
                return false;
            }
        }

        public AttendanceSettingModel GetAttendanceSetting(int companyId)
        {
            var attendanceSetting = _unitOfWork.AttendanceRepository.GetAttendanceSetting(companyId);
            if (attendanceSetting != null)
            {
                AttendanceSettingModel model = new AttendanceSettingModel()
                {
                    Id = attendanceSetting.Id,
                    ApprovarAccounts = attendanceSetting.ApproverAccounts,
                    TimeFormatId = attendanceSetting.TimeFormatId,
                    TimeFormatList = GetTimeFormatList(),
                    EnableNotifyCheckinLate = attendanceSetting.EnableNotifyCheckinLate
                };
                return model;
            }

            return null;
        }

        public List<TimeformatTypeModel> GetTimeFormatList()
        {
            var accountLanguage = _unitOfWork.AppDbContext.Account.Where(m => m.Id == _httpContext.User.GetAccountId()).FirstOrDefault().Language;
            var culture = new CultureInfo(accountLanguage);

            // FE doesn't give "Culture" info.
            Thread.CurrentThread.CurrentUICulture = culture;

            List<TimeformatTypeModel> timeformatList = new List<TimeformatTypeModel>();

            var timeformatTypes = EnumHelper.ToEnumList<TimeFormatType>();

            foreach (var type in timeformatTypes)
            {
                TimeformatTypeModel timeformatType = new TimeformatTypeModel()
                {
                    Id = type.Id,
                    Name = type.Name,
                    Value = type.Id == 0 ? "HH:mm:ss" : "HH:mm"
                };

                timeformatList.Add(timeformatType);
            }

            return timeformatList;
        }

        public void UpdateAttendanceSetting(AttendanceSetting attendanceSetting)
        {
            _unitOfWork.AttendanceRepository.UpdateAttendanceSetting(attendanceSetting);
        }

        public void DeleteAttendance(AttendanceLeave attendance)
        {
            try
            {
                _unitOfWork.AttendanceLeaveRepository.Delete(attendance);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteAttendance");
            }
        }

        public void DeleteAttendanceLeaves(List<AttendanceLeave> attendanceLeaves)
        {
            try
            {
                _unitOfWork.AttendanceLeaveRepository.DeleteRange(attendanceLeaves);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteAttendanceLeaves");
            }
        }

        public string EditAttendanceLeave(AttendanceLeave attendanceLeave)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var user = _unitOfWork.AppDbContext.User.Include(m => m.Account).FirstOrDefault(u =>
                    u.AccountId == _httpContext.User.GetAccountId() && !u.IsDeleted &&
                    u.CompanyId == companyId);
                if (user != null && !string.IsNullOrEmpty(user.Account?.TimeZone))
                {
                    var offSet = user.Account.TimeZone.ToTimeZoneInfo().BaseUtcOffset;
                    attendanceLeave.Start = attendanceLeave.Start.ConvertToSystemTime(offSet);
                    attendanceLeave.End = attendanceLeave.End.ConvertToSystemTime(offSet);
                }
                else
                {
                    var buildingDefault = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(companyId);
                    var offSet = buildingDefault.TimeZone.ToTimeZoneInfo().BaseUtcOffset;
                    attendanceLeave.Start = attendanceLeave.Start.ConvertToSystemTime(offSet);
                    attendanceLeave.End = attendanceLeave.End.ConvertToSystemTime(offSet);
                }

                var listAttendanceLeaves = _unitOfWork.AppDbContext.AttendanceLeave.Where(
                    a => (a.Status == (short)AttendanceStatus.Waiting || a.Status == (short)AttendanceStatus.Approved)
                         && a.Id != attendanceLeave.Id
                         && a.UserId == attendanceLeave.UserId
                         && ((a.Start <= attendanceLeave.Start && attendanceLeave.Start <= a.End) || (a.Start <= attendanceLeave.End && attendanceLeave.End <= a.End)));

                if (listAttendanceLeaves.Any())
                {
                    return AttendanceResource.requestAttendanceExist;
                }

                _unitOfWork.AttendanceLeaveRepository.Update(attendanceLeave);
                _unitOfWork.Save();
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in EditAttendanceLeave");
                return null;
            }
        }

        public List<EnumModel> GetAttendanceTypeList()
        {
            var accountLanguage = _unitOfWork.AppDbContext.Account.Where(m => m.Id == _httpContext.User.GetAccountId()).FirstOrDefault().Language;
            var culture = new CultureInfo(accountLanguage);

            Thread.CurrentThread.CurrentUICulture = culture;

            var test = EnumHelper.ToEnumList<AttendanceType>();

            return test;
        }

        public List<AttendanceReportMonthModel> GetAttendanceReportMonth(string search, DateTime month, List<int> departmentIds,
            int pageNumber, int pageSize, string sortColumn, string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();
            DateTime start = new DateTime(month.Year, month.Month, 1);
            DateTime end = start.AddMonths(1);
            if (end > DateTime.Now) end = DateTime.Now.Date.AddDays(1);
            List<int> typeNormal = new List<int> { (short)AttendanceType.Normal, (short)AttendanceType.Holiday };
            List<int> ignoredTypeNormal = new List<int>();

            var data = from user in _unitOfWork.AppDbContext.User
                       join department in _unitOfWork.AppDbContext.Department on user.DepartmentId equals department.Id
                       where user.CompanyId == companyId && user.Status == (short)Status.Valid
                       select new AttendanceReportMonthModel()
                       {
                           UserId = user.Id,
                           UserName = user.FirstName,
                           Avatar = user.Avatar,
                           DepartmentId = user.DepartmentId,
                           DepartmentName = department.DepartName,
                           TotalDaysNormal = _unitOfWork.AppDbContext.Attendance.Count(attendance =>
                               attendance.UserId == user.Id && typeNormal.Contains(attendance.Type)
                               && start <= attendance.Date && attendance.Date < end),
                           TotalDays = _unitOfWork.AppDbContext.Attendance.Count(attendance =>
                               attendance.UserId == user.Id && !ignoredTypeNormal.Contains(attendance.Type)
                               && start <= attendance.Date && attendance.Date < end)
                       };

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = search.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x => x.UserName.RemoveDiacritics().ToLower().Contains(normalizedSearch)).AsQueryable();
            }

            if (departmentIds != null && departmentIds.Any())
            {
                data = data.Where(m => departmentIds.Contains(m.DepartmentId));
            }
            else
            {
                // check permission account type dynamic role department level
                if (_httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
                {
                    var departmentRoleLevelIds = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
                    if (departmentRoleLevelIds.Any())
                    {
                        data = data.Where(x => departmentRoleLevelIds.Contains(x.DepartmentId));
                    }
                }
            }

            recordsFiltered = data.Count();
            if (typeof(AttendanceReportMonthModel).GetProperty(sortColumn) != null)
                data = data.OrderBy(m => m.DepartmentName).ThenBy($"{sortColumn} {sortDirection}");
            else
                data = data.OrderBy(m => m.DepartmentName).ThenBy(m => m.UserName);

            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return data.ToList();
        }

        public List<AttendanceLeaveReportModel> GetAttendanceReportYear(LeaveReportFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            var data = _unitOfWork.AttendanceRepository.GetAttendanceReportYear(filter, out totalRecords, out recordsFiltered);
            return data.ToList();
        }

        public LeaveSettingModel GetLeaveRequestSettingByCompanyId(int companyId)
        {
            return _mapper.Map<LeaveSettingModel>(_unitOfWork.AttendanceLeaveRepository.GetSettingByCompanyId(companyId));
        }

        public void UpdateLeaveRequestSetting(LeaveSettingModel model)
        {
            var setting = _unitOfWork.AttendanceLeaveRepository.GetSettingByCompanyId(_httpContext.User.GetCompanyId());
            setting.NumberDayOffYear = model.NumberDayOffYear;
            setting.NumberDayOffPreviousYear = model.NumberDayOffPreviousYear;
            _unitOfWork.AttendanceLeaveRepository.UpdateLeaveRequestSetting(setting);
        }
        /// <summary>
        /// Set startTime
        /// </summary>
        /// <param name="startTime"> time value (HHmm) </param>
        public void SetStartTime(string startTime)
        {
            try
            {
                // Get attendanceSetting value from DB.
                var companyId = _httpContext.User.GetCompanyId();
                var attendanceSetting = _unitOfWork.AttendanceRepository.GetAttendanceSetting(companyId);

                attendanceSetting.DayStartTime = startTime;

                _unitOfWork.AttendanceRepository.UpdateAttendanceSetting(attendanceSetting);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetStartTime");
            }
        }
        /// <summary>
        /// Get startTime
        /// </summary>
        /// <returns></returns>
        public string GetStartTime()
        {
            // Get attendanceSetting value from DB.
            var companyId = _httpContext.User.GetCompanyId();
            var attendanceSetting = _unitOfWork.AttendanceRepository.GetAttendanceSetting(companyId);

            if (string.IsNullOrEmpty(attendanceSetting.DayStartTime)) attendanceSetting.DayStartTime = "";

            return attendanceSetting.DayStartTime;
        }
    }
}