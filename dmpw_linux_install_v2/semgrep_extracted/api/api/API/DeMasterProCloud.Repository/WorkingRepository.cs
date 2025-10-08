using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace DeMasterProCloud.Repository
{
    
    public interface IWorkingRepository : IGenericRepository<WorkingType>
    {
        WorkingType CheckWorkingTypeExisted(string workingName, int companyId);
        void AddWorkingTypeDefault(int companyId);
        
        int GetWorkingTypeDefault(int companyId);
        bool CheckNameWorkingTime(string workingName, int companyId, int id);
        WorkingType GetByUserId(int userId);
    }
    
    public class WorkingRepository : GenericRepository<WorkingType>, IWorkingRepository
    {
        private readonly AppDbContext _dbContext;
        
        public WorkingRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
        }
        
        public WorkingType CheckWorkingTypeExisted(string workingName, int companyId)
        {

        return Get(m =>
                   m.Name == workingName && m.CompanyId == companyId);

        }


        public bool CheckNameWorkingTime(string workingName, int companyId, int id)
        {
            var checkWorkingTime = _dbContext.WorkingType.Where(x => x.Name == workingName && x.CompanyId == companyId && x.Id != id).FirstOrDefault();
            if (checkWorkingTime != null)
                return false;
            return true;
        }
        public int GetWorkingTypeDefault(int companyId)
        {

            return Get(m => m.CompanyId == companyId && m.IsDefault).Id;

        }

        public void AddWorkingTypeDefault(int companyId)
        {
            var workingType = new WorkingType
            {
                Name = Constants.Attendance.DefaultName,
                IsDefault = true,
                CompanyId = companyId,
                WorkingDay = Constants.Attendance.DefaultWorkingTime,
                WorkingHourType = Constants.Attendance.DefaultWorkingHourType
            };

            Add(workingType);
        }

        public WorkingType GetByUserId(int userId)
        {
            var user = _dbContext.User.FirstOrDefault(m => m.Id == userId && !m.IsDeleted);
            if (user?.WorkingTypeId != null)
                return _dbContext.WorkingType.FirstOrDefault(m => m.Id == user.WorkingTypeId);
            else
                return null;
        }
    }
}