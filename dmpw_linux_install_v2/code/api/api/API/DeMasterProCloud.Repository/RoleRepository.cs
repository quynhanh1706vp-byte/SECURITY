using DeMasterProCloud.Common.Infrastructure;
using Microsoft.Extensions.Logging;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Role;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeMasterProCloud.DataAccess.Migrations;

namespace DeMasterProCloud.Repository
{
    public interface IRoleRepository : IGenericRepository<DynamicRole>
    {
        IQueryable<DynamicRole> GetByIdAndCompanyId(int roleId, int companyId);
        RoleModel GetByAccountId(int accountId, int companyId);

        IQueryable<DynamicRole> GetByyIdsAndCompanyId(List<int> ids, int companyId);
        IQueryable<DynamicRole> GetByCompanyId(int companyId);
        IQueryable<DynamicRole> GetByTypeAndCompanyId(int typeId, int companyId);
        IQueryable<DynamicRole> GetByNameAndCompanyId(string roleName, int companyId);
        DynamicRole GetDefaultRoleSettingByCompany(int companyId);
    }

    public class RoleRepository : GenericRepository<DynamicRole>, IRoleRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;

        public RoleRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<RoleRepository>();
        }

        public IQueryable<DynamicRole> GetByIdAndCompanyId(int roleId, int companyId)
        {
            try
            {
                var role = _dbContext.DynamicRole.Where(m => m.Id == roleId && m.CompanyId == companyId && !m.IsDeleted);

                return role;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdAndCompanyId");
                return new List<DynamicRole>().AsQueryable();
            }
        }

        /// <summary>
        /// Get role by accountId
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public RoleModel GetByAccountId(int accountId, int companyId)
        {
            try
            {
                var role = (from a in _dbContext.DynamicRole
                            join b in _dbContext.CompanyAccount on a.Id equals b.DynamicRoleId
                            where b.AccountId == accountId && a.CompanyId == companyId
                            select new RoleModel
                            {
                                Id = a.Id,
                                RoleName = a.Name,
                                IsDefault = a.TypeId != (int)AccountType.DynamicRole,
                                PermissionGroups = JsonConvert.DeserializeObject<List<PermissionGroupModel>>(a.PermissionList),
                                Description = a.Description,
                            }).FirstOrDefault();

                return role;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetByAccountId");
                return null;
            }
        }

        /// <summary>
        /// Get roles by list of role identifier
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IQueryable<DynamicRole> GetByyIdsAndCompanyId(List<int> ids, int companyId)
        {
            try
            {
                var data = _dbContext.DynamicRole.Where(m => ids.Contains(m.Id) && m.CompanyId == companyId && !m.IsDeleted);

                return data;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetByyIdsAndCompanyId");
                return new List<DynamicRole>().AsQueryable();
            }
        }

        public IQueryable<DynamicRole> GetByCompanyId(int companyId)
        {
            try
            {
                var data = _dbContext.DynamicRole.Where(m => m.CompanyId == companyId && !m.IsDeleted);

                return data;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCompanyId");
                return new List<DynamicRole>().AsQueryable();
            }
        }

        /// <summary>
        /// Get dynamic role by type id and company id
        /// </summary>
        /// <param name="typeId"> identifier of account type </param>
        /// <param name="companyId"> identifier of company </param>
        /// <returns></returns>
        public IQueryable<DynamicRole> GetByTypeAndCompanyId(int typeId, int companyId)
        {
            try
            {
                var data = _dbContext.DynamicRole.Where(m => m.CompanyId == companyId && m.TypeId == typeId && !m.IsDeleted);

                return data;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetByTypeAndCompanyId");
                return new List<DynamicRole>().AsQueryable();
            }
        }


        /// <summary>
        /// Get dynamic role by name and company id
        /// </summary>
        /// <param name="typeName"> name of role </param>
        /// <param name="companyId"> identifier of company </param>
        /// <returns></returns>
        public IQueryable<DynamicRole> GetByNameAndCompanyId(string roleName, int companyId)
        {
            try
            {
                var data = _dbContext.DynamicRole.Where(m => m.CompanyId == companyId && m.Name == roleName && !m.IsDeleted);

                return data;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetByNameAndCompanyId");
                return new List<DynamicRole>().AsQueryable();
            }
        }

        public DynamicRole GetDefaultRoleSettingByCompany(int companyId)
        {
            try
            {
                var role = _dbContext.DynamicRole.FirstOrDefault(m => !m.IsDeleted && m.CompanyId == companyId && m.RoleSettingDefault);
                if (role == null)
                {
                    return _dbContext.DynamicRole.FirstOrDefault(m => !m.IsDeleted && m.CompanyId == companyId && m.TypeId == (short)AccountType.Employee);
                }

                return role;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetDefaultRoleSettingByCompany");
                return null;
            }
        }
    }
}