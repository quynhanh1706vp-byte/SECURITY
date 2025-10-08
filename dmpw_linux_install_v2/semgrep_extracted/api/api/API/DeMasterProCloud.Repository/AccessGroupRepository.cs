using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// Interface for AccessGroup repository
    /// </summary>
    public interface IAccessGroupRepository : IGenericRepository<AccessGroup>
    {
        AccessGroup GetByIdAndCompanyId(int companyId, int id);
        AccessGroup GetDefaultAccessGroup(int companyId);
        List<AccessGroup> GetByIdsAndCompanyId(int companyId, List<int> accessGroupIds);
        List<AccessGroup> GetListAccessGroups(int companyId);
        List<AccessGroup> GetAccessGroupUnSetDefault(int companyId, int id);
        AccessGroup GetNoAccessGroup(int companyId);
        AccessGroup GetFullAccessGroup(int companyId);
        AccessGroup GetVisitAccessGroup(int companyId);
        AccessGroup GetByNameAndCompanyId(int companyId, string name);
        bool HasExistName(int accessGroupId, int companyId, string name);
        void AddDefault(Company company, IConfiguration _configuration);
        List<int> GetAccessGroupIdByAccountDepartmentRole(int companyId, int accountId);
        List<IcuDevice> GetDevicesByAccessGroupId(int accessGroupId);
        IQueryable<AccessGroup> GetParentsByIds(List<int> ids);
    }
    public class AccessGroupRepository : GenericRepository<AccessGroup>, IAccessGroupRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;

        public AccessGroupRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<AccessGroupRepository>();
        }

        /// <summary>
        /// Get default access group
        /// </summary>
        /// <returns></returns>
        public AccessGroup GetDefaultAccessGroup(int companyId)
        {
            try
            {
                return _dbContext.AccessGroup.FirstOrDefault(x => x.IsDefault && x.CompanyId == companyId && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDefaultAccessGroup");
                return null;
            }
        }

        /// <summary>
        /// Get by ids and company
        /// </summary>
        /// <param name="accessGroupIds"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<AccessGroup> GetByIdsAndCompanyId(int companyId, List<int> accessGroupIds)
        {
            try
            {
                return _dbContext.AccessGroup.Include(m => m.User)
                    .Where(m => accessGroupIds.Contains(m.Id) && m.CompanyId == companyId && !m.IsDeleted).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdsAndCompanyId");
                return new List<AccessGroup>();
            }
        }

        /// <summary>
        /// Get list access group is valid
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<AccessGroup> GetListAccessGroups(int companyId)
        {
            try
            {
                return _dbContext.AccessGroup.Where(x => x.CompanyId == companyId && !x.IsDeleted).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetListAccessGroups");
                return new List<AccessGroup>();
            }
        }

        /// <summary>
        /// Get access group by id and companyid
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public AccessGroup GetByIdAndCompanyId(int companyId, int id)
        {
            try
            {
                return _dbContext.AccessGroup
                                .Include(m => m.AccessGroupDevice)
                                .ThenInclude(n => n.Icu)
                                .FirstOrDefault(x => x.CompanyId == companyId && x.Id == id && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdAndCompanyId");
                return null;
            }
        }

        /// <summary>
        /// Get list access group unset default
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<AccessGroup> GetAccessGroupUnSetDefault(int companyId, int id)
        {
            try
            {
                return _dbContext.AccessGroup.Where(x => x.CompanyId == companyId && x.Id != id && !x.IsDeleted).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccessGroupUnSetDefault");
                return new List<AccessGroup>();
            }
        }

        /// <summary>
        /// Get no access group
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public AccessGroup GetNoAccessGroup(int companyId)
        {
            try
            {
                return Get(x => x.CompanyId == companyId && x.Type == (short)AccessGroupType.NoAccess && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNoAccessGroup");
                return null;
            }
        }

        /// <summary>
        /// Get full access group
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public AccessGroup GetFullAccessGroup(int companyId)
        {
            try
            {
                return Get(x => x.CompanyId == companyId && x.Type == (short)AccessGroupType.FullAccess && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFullAccessGroup");
                return null;
            }
        }

        /// <summary>
        /// Get visit access group
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public AccessGroup GetVisitAccessGroup(int companyId)
        {
            try
            {
                return Get(x => x.CompanyId == companyId && x.Type == (short)AccessGroupType.VisitAccess && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetVisitAccessGroup");
                return null;
            }
        }

        /// <summary>
        /// Check name of access group name is already
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="companyId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasExistName(int accessGroupId, int companyId, string name)
        {
            try
            {
                return _dbContext.AccessGroup.Any(x => x.Id != accessGroupId && x.CompanyId == companyId && x.Name == name && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HasExistName");
                return false;
            }
        }

        /// <summary>
        /// Get the AccessGroup by name and companyId
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public AccessGroup GetByNameAndCompanyId(int companyId, string name)
        {
            try
            {
                return _dbContext.AccessGroup.FirstOrDefault(x => x.CompanyId == companyId && x.Name == name && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByNameAndCompanyId");
                return null;
            }
        }


        /// <summary>
        /// Get the AccessGroup by companyId
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<AccessGroup> GetByCompanyId(int companyId)
        {
            try
            {
                return _dbContext.AccessGroup.Where(x => x.CompanyId == companyId && !x.IsDeleted).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCompanyId");
                return new List<AccessGroup>();
            }
        }

        public void AddDefault(Company company, IConfiguration _configuration)
        {
            var accessGroupFullAccess = new AccessGroup
            {
                Name = _configuration[Constants.Settings.DefaultAccessGroupFullAccess],
                IsDefault = true,
                Type = (short)AccessGroupType.FullAccess,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                IsDeleted = false,
                CompanyId = company.Id,
            };
            var accessGroupNoAccess = new AccessGroup
            {
                Name = _configuration[Constants.Settings.DefaultAccessGroupNoAccess],
                IsDefault = false,
                Type = (short)AccessGroupType.NoAccess,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                IsDeleted = false,
                CompanyId = company.Id,
            };
            var accessGroupVisitAccess = new AccessGroup
            {
                Name = _configuration[Constants.Settings.DefaultAccessGroupVisitAccess],
                IsDefault = false,
                Type = (short)AccessGroupType.VisitAccess,
                CreatedOn = DateTime.UtcNow,
                UpdatedOn = DateTime.UtcNow,
                IsDeleted = false,
                CompanyId = company.Id,
            };

            Add(accessGroupFullAccess);
            Add(accessGroupNoAccess);
            Add(accessGroupVisitAccess);
        }

        public List<int> GetAccessGroupIdByAccountDepartmentRole(int companyId, int accountId)
        {
            try
            {
                // get users enable level department permission
                // Get a list of access groups where the department of the logged-in user matches the department of the user who created the access group.
                var departmentIdOfUserLogin = _dbContext.User
                    .FirstOrDefault(x => x.AccountId == accountId && !x.IsDeleted && x.CompanyId == companyId)
                    ?.DepartmentId ?? 0;

                var listAccessGroups = (from a in _dbContext.AccessGroup
                    join u in _dbContext.User on a.CreatedBy equals u.AccountId
                    join d in _dbContext.Department on u.DepartmentId equals d.Id
                    join ca in _dbContext.CompanyAccount.Include(x => x.DynamicRole) on a.CreatedBy equals ca.AccountId
                    where !a.IsDeleted
                          && !u.IsDeleted
                          && !d.IsDeleted
                          && a.CompanyId == companyId
                          && d.CompanyId == companyId
                          && ca.CompanyId == companyId
                          // && ca.AccountId == accountId
                          && ca.DynamicRole.EnableDepartmentLevel
                          && (d.Id == departmentIdOfUserLogin || d.ParentId == departmentIdOfUserLogin)
                    select a.Id).Distinct().ToList();

                // add noAG and FullAG
                var noAgId = GetNoAccessGroup(companyId).Id;
                var fullAgId = GetFullAccessGroup(companyId).Id;
                listAccessGroups.Add(noAgId);
                listAccessGroups.Add(fullAgId);
                return listAccessGroups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccessGroupIdByAccountDepartmentRole");
                return new List<int>();
            }
        }

        public List<IcuDevice> GetDevicesByAccessGroupId(int accessGroupId)
        {
            try
            {
                var deviceIds = _dbContext.AccessGroupDevice.Where(m => m.AccessGroupId == accessGroupId).Select(m => m.IcuId).ToList();
                return _dbContext.IcuDevice.Where(m => !m.IsDeleted && deviceIds.Contains(m.Id)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDevicesByAccessGroupId");
                return new List<IcuDevice>();
            }
        }

        public IQueryable<AccessGroup> GetParentsByIds(List<int> ids)
        {
            try
            {
                return _dbContext.AccessGroup.Include(m => m.Parent)
                    .Where(m => ids.Contains(m.Id) && m.Parent != null).Select(m => m.Parent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetParentsByIds");
                return Enumerable.Empty<AccessGroup>().AsQueryable();
            }
        }
    }
}
