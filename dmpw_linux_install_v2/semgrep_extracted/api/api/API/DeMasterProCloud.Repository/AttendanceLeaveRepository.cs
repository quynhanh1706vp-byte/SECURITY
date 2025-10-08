using System;
using DeMasterProCloud.Common.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.DataAccess;
using DeMasterProCloud.DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Repository
{
    public interface IAttendanceLeaveRepository: IGenericRepository<AttendanceLeave>
    {
        AttendanceLeave GetByIdAndCompanyId(int companyId, int attendanceId);
        IQueryable<AttendanceLeave> GetByIdsAndCompanyId(int companyId, List<int> attendanceIds);
        IQueryable<AttendanceLeave> GetByCompanyId(int companyId);
        LeaveRequestSetting GetSettingByCompanyId(int companyId);
        void UpdateLeaveRequestSetting(LeaveRequestSetting setting);
    }
    
    public class AttendanceLeaveRepository : GenericRepository<AttendanceLeave>, IAttendanceLeaveRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;

        public AttendanceLeaveRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<AttendanceLeaveRepository>();
        }
        
        public AttendanceLeave GetByIdAndCompanyId(int companyId, int attendanceId)
        {
            return Get(a => a.CompanyId == companyId && a.Id == attendanceId);
        }

        public IQueryable<AttendanceLeave> GetByIdsAndCompanyId(int companyId, List<int> attendanceIds)
        {
            return _dbContext.AttendanceLeave.Where(m => m.CompanyId == companyId && attendanceIds.Contains(m.Id));
        }


        public IQueryable<AttendanceLeave> GetByCompanyId(int companyId)
        {
            return _dbContext.AttendanceLeave.Where(al => al.CompanyId == companyId);
        }

        public LeaveRequestSetting GetSettingByCompanyId(int companyId)
        {
            return _dbContext.LeaveRequestSetting.FirstOrDefault(m => m.CompanyId == companyId);
        }

        public void UpdateLeaveRequestSetting(LeaveRequestSetting setting)
        {
            try
            {
                _dbContext.LeaveRequestSetting.Update(setting);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateLeaveRequestSetting");
            }
        }
    }
}