using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;

namespace DeMasterProCloud.Repository
{
    public interface IAccessTimeRepository : IGenericRepository<AccessTime>
    {
        AccessTime GetByIdAndCompany(int? timezoneId, int companyId);
        void AddDefaultTimezone(int companyId);
        AccessTime GetTimezoneByNameAndCompany(int companyId, string name);
        List<AccessTime> GetByCompany(int? companyId);
        List<int> GetPositionsByCompany(int companyId);
        int GetTimezoneCount(int companyId);
        List<AccessTime> GetByIdsAndCompany(List<int> ids, int companyId);
        AccessTime GetDefaultTimezone(int companyId);
        AccessTime GetDefaultTzByCompanyId(int tzPosition, int companyId);
        AccessTime GetByPositionAndCompany(int position, int? companyId);
    }
    public class AccessTimeRepository : GenericRepository<AccessTime>, IAccessTimeRepository
    {
        private readonly AppDbContext _dbContext;
        public AccessTimeRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get timezone by id and company
        /// </summary>
        /// <param name="accessTimeId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public AccessTime GetByIdAndCompany(int? accessTimeId, int companyId)
        {

            if(companyId != 0)
            {
                return Get(c => c.CompanyId == companyId
                            && c.Id == accessTimeId
                            && !c.IsDeleted);
            }
            else
            {
                return Get(c => c.Id == accessTimeId
                            && !c.IsDeleted);
            }

            //return Get(c => c.CompanyId == companyId
            //                && c.Id == timezoneId
            //                && !c.IsDeleted);
        }

        /// <summary>
        /// Get default timezone in company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public AccessTime GetDefaultTimezone(int companyId)
        {
            return Get(c => c.CompanyId == companyId
                            && c.Position == Constants.Tz24hPos
                            && !c.IsDeleted);
        }

        /// <summary>
        /// Get default timezone in company by timezone position
        /// </summary>
        /// <param name="atPosition"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public AccessTime GetDefaultTzByCompanyId(int atPosition, int companyId)
        {
            return Get(c => c.CompanyId == companyId
                            && c.Position == atPosition
                            && !c.IsDeleted);
        }


        /// <summary>
        /// Get timezones by company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<AccessTime> GetByCompany(int? companyId)
        {
            return GetMany(m => !m.IsDeleted && m.CompanyId == companyId)
                .ToList();
        }

        /// <summary>
        /// Get position of timezone by company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<int> GetPositionsByCompany(int companyId)
        {
            return GetMany(m => !m.IsDeleted && m.CompanyId == companyId)
                .Select(c => c.Position)
                .ToList();
        }

        /// <summary>
        /// Add default timezone for company
        /// </summary>
        /// <param name="companyId"></param>
        public void AddDefaultTimezone(int companyId)
        {
            var activeAccessTime = new AccessTime
            {
                //Id = Constants.DefaultActiveTimezoneId,
                CompanyId = companyId,
                Name = AccessTimeResource.lblDefaultTimezoneName,
                Remarks = AccessTimeResource.lblDefaultTimezoneName,
                Position = Constants.Settings.DefaultPositionActiveTimezone,
                IsDeleted = false,
                MonTime1 = Constants.Settings.DefaultTimezoneTime,
                TueTime1 = Constants.Settings.DefaultTimezoneTime,
                WedTime1 = Constants.Settings.DefaultTimezoneTime,
                ThurTime1 = Constants.Settings.DefaultTimezoneTime,
                FriTime1 = Constants.Settings.DefaultTimezoneTime,
                SatTime1 = Constants.Settings.DefaultTimezoneTime,
                SunTime1 = Constants.Settings.DefaultTimezoneTime,
                HolType1Time1 = Constants.Settings.DefaultTimezoneTime,
                HolType2Time1 = Constants.Settings.DefaultTimezoneTime,
                HolType3Time1 = Constants.Settings.DefaultTimezoneTime
            };
            var passageAccessTime = new AccessTime
            {
                //Id = Constants.DefaultPassageTimezoneId,
                CompanyId = companyId,
                Name = AccessTimeResource.lblDefaultNotUseTimezoneName,
                Remarks = AccessTimeResource.lblDefaultNotUseTimezoneName,
                Position = Constants.Settings.DefaultPositionPassageTimezone,
                IsDeleted = false,
                MonTime1 = Constants.Settings.DefaultTimezoneNotUse,
                TueTime1 = Constants.Settings.DefaultTimezoneNotUse,
                WedTime1 = Constants.Settings.DefaultTimezoneNotUse,
                ThurTime1 = Constants.Settings.DefaultTimezoneNotUse,
                FriTime1 = Constants.Settings.DefaultTimezoneNotUse,
                SatTime1 = Constants.Settings.DefaultTimezoneNotUse,
                SunTime1 = Constants.Settings.DefaultTimezoneNotUse,
                HolType1Time1 = Constants.Settings.DefaultTimezoneNotUse,
                HolType2Time1 = Constants.Settings.DefaultTimezoneNotUse,
                HolType3Time1 = Constants.Settings.DefaultTimezoneNotUse
            };
            Add(passageAccessTime);
            Add(activeAccessTime);
        }

        /// <summary>
        /// Get timezone by name and company
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public AccessTime GetTimezoneByNameAndCompany(int companyId, string name)
        {
            return Get(c => c.CompanyId == companyId && c.Name == name
                    && !c.IsDeleted);
        }

        /// <summary>
        /// Count timezone in company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public int GetTimezoneCount(int companyId)
        {
            return _dbContext.AccessTime
                .Where(m => !m.IsDeleted && m.CompanyId == companyId)
                .Select(c => c.Id)
                .Count();
        }

        /// <summary>
        /// Get timezones by list of id and company
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<AccessTime> GetByIdsAndCompany(List<int> ids, int companyId)
        {
            return GetMany(c => c.CompanyId == companyId && ids.Contains(c.Id) && !c.IsDeleted)
                .ToList();
        }

        /// <summary>
        /// Get by position
        /// </summary>
        /// <param name="position"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public AccessTime GetByPositionAndCompany(int position, int? companyId)
        {
            if (companyId!= null) return _dbContext.AccessTime.FirstOrDefault(x =>
                x.Position == position && x.CompanyId == companyId && !x.IsDeleted);
            return null;
        }
    }
}
