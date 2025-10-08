using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Attendance;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Repository
{
    public interface IAttendanceRepository : IGenericRepository<Attendance>
    {
        void AddNewAttendance(int userId, int companyId, DateTime today, string workingTime, DateTime start, DateTime end, int type);

        Attendance GetAttendanceAlreadyCreated(int userId, int companyId, DateTime today);
        bool CheckAttendanceAlreadyCreated(int userId, int companyId, DateTime today);
        
        Attendance GetAttendanceByIdAndCompanyId(int companyId, int attendanceId);
        Attendance GetAttendanceByUserIdAndDate(int userId, DateTime date);
        List<Attendance> GetTodayAttendances(int companyId);
        List<Attendance> GetAttendancesByDate(int companyId, DateTime attendateDate);
        AttendanceSetting GetAttendanceSetting(int companyId);
        void UpdateAttendanceSetting(AttendanceSetting attendanceSetting);
        IQueryable<AttendanceLeaveReportModel> GetAttendanceReportYear(LeaveReportFilterModel filter, out int totalRecords, out int recordsFiltered);
        IQueryable<AttendanceLeaveModel> GetAttendanceLeavesUser(LeaveReportFilterModel filter, out int totalRecords, out int recordsFiltered);
    }

    public class AttendanceRepository : GenericRepository<Attendance>, IAttendanceRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        public AttendanceRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<AttendanceRepository>();
        }
        
        public void AddNewAttendance(int userId, int companyId, DateTime today, string workingTime, DateTime start, DateTime end, int type)
        {
            var attendance = new Attendance
            {
                UserId = userId,
                CompanyId = companyId,
                Date = today,
                WorkingTime = workingTime,
                StartD = start,
                EndD = end,
                Type = type
            };
            Add(attendance);
        }

        public bool CheckAttendanceAlreadyCreated(int userId, int companyId, DateTime today)
        {
            try
            {
                return Get(m =>
                           m.UserId == userId && m.Date == today && m.CompanyId ==  companyId) != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAttendanceAlreadyCreated");
                return false;
            }
        }
        
        public Attendance GetAttendanceAlreadyCreated(int userId, int companyId, DateTime today)
        {
            try
            {
                return _dbContext.Attendance.Where(a => a.UserId == userId && a.CompanyId == companyId && a.Date == today).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAttendanceAlreadyCreated");
                return null;
            }
        }

        public Attendance GetAttendanceByUserIdAndDate(int userId, DateTime today)
        {
            try
            {
                return _dbContext.Attendance.Where(a => a.UserId == userId && a.Date == today).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAttendanceByUserIdAndDate");
                return null;
            }
        }

        public Attendance GetAttendanceByIdAndCompanyId(int companyId, int attendanceId)
        {
            try
            {
                return Get(m =>
                           m.Id == attendanceId && m.CompanyId ==  companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAttendanceByIdAndCompanyId");
                return null;
            }
        }

        public List<Attendance> GetTodayAttendances(int companyId)
        {
            try
            {
                return _dbContext.Attendance.Where(m => m.CompanyId == companyId && m.Date == DateTime.Now.Date).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTodayAttendances");
                return new List<Attendance>();
            }
        }

        public List<Attendance> GetAttendancesByDate(int companyId, DateTime attendateDate)
        {
            try
            {
                return _dbContext.Attendance.Where(m => m.CompanyId == companyId && m.Date == attendateDate.Date).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAttendancesByDate");
                return new List<Attendance>();
            }
        }

        public AttendanceSetting GetAttendanceSetting(int companyId)
        {
            try
            {
                return _dbContext.AttendanceSetting.FirstOrDefault(a => a.CompanyId == companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAttendanceSetting");
                return null;
            }
        }

        public void UpdateAttendanceSetting(AttendanceSetting attendanceSetting)
        {
            try
            {
                _dbContext.AttendanceSetting.Update(attendanceSetting);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateAttendanceSetting");
            }
        }

        public IQueryable<AttendanceLeaveReportModel> GetAttendanceReportYear(LeaveReportFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                DateTime startDate, endDate;
                if (int.TryParse(filter.Year, out int year))
                {
                    startDate = new DateTime(year, 1, 1);
                    endDate = startDate.AddYears(1);
                }
                else
                {
                    startDate = filter.StartDate;
                    endDate = filter.EndDate;
                }

                var data = from user in _dbContext.User
                    join department in _dbContext.Department on user.DepartmentId equals department.Id
                    where user.CompanyId == filter.CompanyId && user.Status == (short) Status.Valid && !user.IsDeleted
                    select new AttendanceLeaveReportModel()
                    {
                        Id = user.Id,
                        Name = user.FirstName,
                        DepartmentId = department.Id,
                        DepartmentName = department.DepartName,
                        TotalOverTime = _dbContext.Attendance.Count(m => m.UserId == user.Id && m.Type == (short)AttendanceType.OverTime && startDate <= m.Date && m.Date < endDate),
                        TotalBusinessTrip = _dbContext.Attendance.Count(m => m.UserId == user.Id && m.Type == (short)AttendanceType.BusinessTrip && startDate <= m.Date && m.Date < endDate),
                        TotalVacation = _dbContext.Attendance.Count(m => m.UserId == user.Id && m.Type == (short)AttendanceType.Vacation && startDate <= m.Date && m.Date < endDate),
                        TotalSickness = _dbContext.Attendance.Count(m => m.UserId == user.Id && m.Type == (short)AttendanceType.Sickness && startDate <= m.Date && m.Date < endDate),
                        TotalRemote = _dbContext.Attendance.Count(m => m.UserId == user.Id && m.Type == (short)AttendanceType.Remote && startDate <= m.Date && m.Date < endDate),
                        TotalLateIn = _dbContext.AttendanceLeave.Count(m => m.UserId == user.Id && m.Type == (short)AttendanceType.LateIn && startDate <= m.Date && m.Date < endDate),
                        TotalEarlyOut = _dbContext.AttendanceLeave.Count(m => m.UserId == user.Id && m.Type == (short)AttendanceType.EarlyOut && startDate <= m.Date && m.Date < endDate),
                        TotalOffDutyBreak = _dbContext.AttendanceLeave.Count(m => m.UserId == user.Id && m.Type == (short)AttendanceType.OffDutyBreak && startDate <= m.Date && m.Date < endDate),
                    };

                totalRecords = data.Count();

                if (filter.DepartmentIds != null && filter.DepartmentIds.Any())
                {
                    data = data.Where(m => filter.DepartmentIds.Contains(m.DepartmentId));
                }

                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var searchTerm = filter.Search.ToLower();
                    data = data.Where(m => m.Name != null && m.Name.ToLower().Contains(searchTerm));
                }

                recordsFiltered = data.Count();

                filter.SortColumn = char.ToUpper(filter.SortColumn[0]) + filter.SortColumn.Substring(1);
                if(typeof(AttendanceLeaveReportModel).GetProperty(filter.SortColumn) != null)
                    data = data.OrderBy($"{filter.SortColumn} {filter.SortDirection}");
                else
                    data = data.OrderBy(m => m.Name);

                data = data.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAttendanceReportYear");
                totalRecords = 0;
                recordsFiltered = 0;
                return Enumerable.Empty<AttendanceLeaveReportModel>().AsQueryable();
            }
        }

        public IQueryable<AttendanceLeaveModel> GetAttendanceLeavesUser(LeaveReportFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var data = from m in _dbContext.AttendanceLeave
                    join user in _dbContext.User on m.UserId equals user.Id
                    where m.CompanyId == filter.CompanyId && user.Status != (short)Status.Invalid && filter.UserId == user.Id
                    select new AttendanceLeaveModel()
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
                        Creator = m.CreatedBy == 0 || m.CreatedBy == m.User.AccountId
                            ? m.User.FirstName
                            : _dbContext.Account.FirstOrDefault(a => a.Id == m.CreatedBy) != null ? _dbContext.Account.FirstOrDefault(a => a.Id == m.CreatedBy).Username : null,
                    };

                totalRecords = data.Count();

                if (!string.IsNullOrEmpty(filter.Search))
                {
                    var searchTerm = filter.Search.ToLower();
                    data = data.Where(x => x.UserName != null && x.UserName.ToLower().Contains(searchTerm));
                }

                if (!string.IsNullOrEmpty(filter.AttendanceType))
                {
                    String[] strList = filter.AttendanceType.Split(",");
                    if (strList.Any())
                    {
                        data = data.Where(x => strList.Contains(x.Type.ToString()));
                    }
                }

                data = filter.StartDate <= filter.EndDate
                    ? data.Where(o => o.Date >= filter.StartDate && o.Date <= filter.EndDate)
                    : data.Where(o => o.Date <= filter.StartDate && o.Date.AddDays(1) <= filter.EndDate);

                recordsFiltered = data.Count();
                if (string.IsNullOrEmpty(filter.SortColumn)) filter.SortColumn = "Status";
                data = data.OrderBy($"{filter.SortColumn} {filter.SortDirection}");
                data = data.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAttendanceLeavesUser");
                totalRecords = 0;
                recordsFiltered = 0;
                return Enumerable.Empty<AttendanceLeaveModel>().AsQueryable();
            }
        }
    }
}
