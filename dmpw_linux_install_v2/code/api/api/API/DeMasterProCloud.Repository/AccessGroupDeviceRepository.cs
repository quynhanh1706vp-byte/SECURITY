using System;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Migrations;
using DeMasterProCloud.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// Interface for AccessGroupDevice repository
    /// </summary>
    public interface IAccessGroupDeviceRepository : IGenericRepository<AccessGroupDevice>
    {
        IQueryable<AccessGroupDevice> GetByAccessGroupId(int companyId, int accessGroupId, List<int> deviceTypes = null, bool includeParent = true);
        int GetUserCount(int icuId);
        List<User> GetUserList(int icuId);
        IQueryable<AccessGroupDevice> GetByUnAssignAccessGroupId(int companyId, int accessGroupId);
        bool HasData(int companyId, int accessGroupId);
        List<AccessGroupDevice> GetByAccessGroupIdAndDeviceIds(int companyId, int accessGroupId, List<int> deviceIds);
        AccessGroupDevice GetByAccessGroupIdAndDeviceId(int companyId, int accessGroupId, int deviceId);
        List<AccessGroupDevice> GetByTimezoneId(int companyId, int tzId);
        IQueryable<AccessGroupDevice> GetByIcuId(int companyId, int icuId);
        IQueryable<AccessGroupDevice> GetByIcuIdInOtherCompany(int companyId, int icuId);
        bool HasTimezone(int timezoneId);
        List<EnumModel> GetListAccessTimeIdByListFloorId(int companyId, List<int> floorIds);
        AccessGroupDevice GetDetail(int accessGroupId, int deviceId);
        List<AccessGroupDevice> GetByAccessGroupAndDevice(int accessGroupId, int deviceId);
    }
    public class AccessGroupDeviceRepository : GenericRepository<AccessGroupDevice>, IAccessGroupDeviceRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        public AccessGroupDeviceRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<AccessGroupDeviceRepository>();
        }

        /// <summary>
        /// Get by access group id
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="companyId"></param>
        /// <param name="deviceTypes"></param>
        /// <param name="includeParent"></param>
        /// <returns></returns>
        public IQueryable<AccessGroupDevice> GetByAccessGroupId(int companyId, int accessGroupId, List<int> deviceTypes = null, bool includeParent = true)
        {
            try
            {
                var accessGroup = _dbContext.AccessGroup.FirstOrDefault(m => m.Id == accessGroupId);

                IQueryable<AccessGroupDevice> data = null;

                if(accessGroup != null)
                {
                    if(includeParent && accessGroup.ParentId != null)
                    {
                        // There is parent AG.
                        data = _dbContext.AccessGroupDevice
                                .Include(c => c.Icu)
                                .Include(c => c.Icu.Building)
                                .Include(c => c.Tz)
                                .Where(c => (c.AccessGroupId == accessGroupId || c.AccessGroupId == accessGroup.ParentId) &&
                                            c.Icu.Status == (short)Status.Valid && !c.Icu.IsDeleted);
                    }
                    else
                    {
                        data = _dbContext.AccessGroupDevice
                                .Include(c => c.Icu)
                                .Include(c => c.Icu.Building)
                                .Include(c => c.Tz)
                                .Where(c => c.AccessGroupId == accessGroupId &&
                                            c.Icu.Status == (short)Status.Valid && !c.Icu.IsDeleted);
                    }

                    if (companyId != 0)
                    {
                        data = data.Where(c => c.AccessGroup.CompanyId == companyId);
                    }

                    if (deviceTypes != null && deviceTypes.Any())
                    {
                        data = data.Where(c => deviceTypes.Contains(c.Icu.DeviceType));
                    }
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByAccessGroupId");
                return Enumerable.Empty<AccessGroupDevice>().AsQueryable();
            }
        }

        /// <summary>
        /// Count number of user in a device
        /// </summary>
        /// <param name="icuId"></param>
        /// <returns></returns>
        public int GetUserCount(int icuId)
        {
            try
            {
                return _dbContext.AccessGroupDevice.Where(c => c.IcuId == icuId).Include(c => c.AccessGroup)
                    .ThenInclude(c => c.User).Where(c => !c.AccessGroup.IsDeleted && !c.Icu.IsDeleted)
                    .SelectMany(c => c.AccessGroup.User).Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserCount");
                return 0;
            }
        }

        /// <summary>
        /// List of user in a device
        /// </summary>
        /// <param name="icuId"></param>
        /// <returns></returns>
        public List<User> GetUserList(int icuId)
        {
            try
            {
                return _dbContext.AccessGroupDevice.Where(c => c.IcuId == icuId).Include(c => c.AccessGroup)
                    .ThenInclude(accessGroup => accessGroup.User).ThenInclude(user => user.Card).Where(c => !c.AccessGroup.IsDeleted && !c.Icu.IsDeleted)
                    .SelectMany(c => c.AccessGroup.User).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserList");
                return new List<User>();
            }
        }

        /// <summary>
        /// Get not by access group id
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IQueryable<AccessGroupDevice> GetByUnAssignAccessGroupId(int companyId, int accessGroupId)
        {
            try
            {
                return _dbContext.AccessGroupDevice.Include(c => c.Icu).Include(c => c.Icu).Include(c => c.Tz)
                    .Where(c => c.AccessGroupId != accessGroupId && c.AccessGroup.CompanyId == companyId && !c.Icu.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByUnAssignAccessGroupId");
                return Enumerable.Empty<AccessGroupDevice>().AsQueryable();
            }
        }

        /// <summary>
        /// Check if having any device was assigned to an access group
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="accessGroupId"></param>
        /// <returns></returns>
        public bool HasData(int companyId, int accessGroupId)
        {
            try
            {
                return _dbContext.AccessGroupDevice.Include(c => c.AccessGroup).Include(c => c.Icu).Any(c =>
                    c.AccessGroupId == accessGroupId && c.AccessGroup.CompanyId == companyId && !c.AccessGroup.IsDeleted &&
                    !c.Icu.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HasData");
                return false;
            }
        }

        /// <summary>
        /// Get not by access group id
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="deviceIds"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<AccessGroupDevice> GetByAccessGroupIdAndDeviceIds(int companyId, int accessGroupId, List<int> deviceIds)
        {
            try
            {
                return _dbContext.AccessGroupDevice
                    .Include(c => c.Icu)
                    .Include(c => c.AccessGroup).ThenInclude(c => c.User).ThenInclude(c => c.Card)
                    .Include(c => c.AccessGroup).ThenInclude(c => c.User).ThenInclude(m => m.Department)
                    .Where(c => c.AccessGroupId == accessGroupId && deviceIds.Contains(c.IcuId) &&
                                c.AccessGroup.CompanyId == companyId && !c.AccessGroup.IsDeleted && !c.Icu.IsDeleted)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByAccessGroupIdAndDeviceIds");
                return new List<AccessGroupDevice>();
            }
        }

        /// <summary>
        /// Get by accessgrop and device id
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="accessGroupId"></param>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public AccessGroupDevice GetByAccessGroupIdAndDeviceId(int companyId, int accessGroupId, int deviceId)
        {
            try
            {
                return _dbContext.AccessGroupDevice.Include(c => c.Tz)
                    .FirstOrDefault(c => (c.AccessGroupId == accessGroupId)&& deviceId == c.IcuId &&
                                         c.AccessGroup.CompanyId == companyId && !c.AccessGroup.IsDeleted &&
                                         !c.Icu.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByAccessGroupIdAndDeviceId");
                return null;
            }
        }

        /// <summary>
        /// Get by timezone id
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="tzId"></param>
        /// <returns></returns>
        public List<AccessGroupDevice> GetByTimezoneId(int companyId, int tzId)
        {
            try
            {
                return _dbContext.AccessGroupDevice.Include(x => x.AccessGroup).Include(x => x.Icu).Where(x =>
                        x.TzId == tzId && x.AccessGroup.CompanyId == companyId && !x.AccessGroup.IsDeleted && !x.Icu.IsDeleted).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByTimezoneId");
                return new List<AccessGroupDevice>();
            }
        }

        /// <summary>
        /// Get by icuid
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="icuId"></param>
        /// <returns></returns>
        public IQueryable<AccessGroupDevice> GetByIcuId(int companyId, int icuId)
        {
            try
            {
                var data = _dbContext.AccessGroupDevice
                    .Include(x => x.AccessGroup)
                    .Include(x => x.Icu)
                    .Include(x => x.Tz)
                    .Where(x => x.IcuId == icuId && !x.AccessGroup.IsDeleted && !x.Icu.IsDeleted);

                if(companyId != 0)
                {
                    data = data.Where(x => x.AccessGroup.CompanyId == companyId);
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIcuId");
                return Enumerable.Empty<AccessGroupDevice>().AsQueryable();
            }
        }

        /// <summary>
        /// Get by icuid
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="icuId"></param>
        /// <returns></returns>
        public IQueryable<AccessGroupDevice> GetByIcuIdInOtherCompany(int companyId, int icuId)
        {
            try
            {
                return _dbContext.AccessGroupDevice.Include(x => x.AccessGroup).Include(x => x.Icu).Include(x => x.Tz).Where(x =>
                    x.IcuId == icuId && x.AccessGroup.CompanyId != companyId && !x.AccessGroup.IsDeleted && !x.Icu.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIcuIdInOtherCompany");
                return Enumerable.Empty<AccessGroupDevice>().AsQueryable();
            }
        }

        /// <summary>
        /// Check if door is assigned to a timezone
        /// </summary>
        /// <param name="timezoneId"></param>
        /// <returns></returns>
        public bool HasTimezone(int timezoneId)
        {
            try
            {
                return _dbContext.AccessGroupDevice.Any(c => c.TzId == timezoneId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HasTimezone");
                return false;
            }
        }

        public List<EnumModel> GetListAccessTimeIdByListFloorId(int companyId, List<int> floorIds)
        {
            try
            {
                var agdsDb = _dbContext.AccessGroupDevice
                    .Include(m => m.Icu)
                    .Include(m => m.Tz)
                    .Where(m => m.Icu.CompanyId == companyId && !string.IsNullOrEmpty(m.FloorIds) && !m.Icu.IsDeleted)
                    .Select(m => new {m.TzId, m.FloorIds, Name = m.Tz.Name}).ToList();
                var agds = agdsDb.Select(m => new {m.TzId, Name = m.Name, FloorIds = JsonConvert.DeserializeObject<List<int>>(m.FloorIds)});
                EnumModel[] tzIds = new EnumModel[floorIds.Count];
                for (int i = 0; i < floorIds.Count(); i++)
                {
                    var agd = agds.FirstOrDefault(m => m.FloorIds.Contains(floorIds[i]));
                    tzIds[i] = agd == null ? new EnumModel {Id = 0, Name = "Unknown"} : new EnumModel {Id = agd.TzId, Name = agd.Name};
                }

                return tzIds.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetListAccessTimeIdByListFloorId");
                return new List<EnumModel>();
            }
        }

        public AccessGroupDevice GetDetail(int accessGroupId, int deviceId)
        {
            try
            {
                return _dbContext.AccessGroupDevice
                    .Include(m => m.Tz)
                    .FirstOrDefault(m => m.AccessGroupId == accessGroupId && m.IcuId == deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDetail");
                return null;
            }
        }

        public List<AccessGroupDevice> GetByAccessGroupAndDevice(int accessGroupId, int deviceId)
        {
            try
            {
                return _dbContext.AccessGroupDevice.Where(m => m.AccessGroupId == accessGroupId && m.IcuId == deviceId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByAccessGroupAndDevice");
                return new List<AccessGroupDevice>();
            }
        }
    }
}
