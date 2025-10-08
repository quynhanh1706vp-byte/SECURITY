using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DeMasterProCloud.DataModel.Role;
using System.Collections.Generic;
using DeMasterProCloud.DataModel.PlugIn;
using System.Globalization;
using DeMasterProCloud.Common.Resources;
using System.Reflection;
using Bogus.Extensions;

namespace DeMasterProCloud.Service
{
    /// <summary>
    /// interface of role service.
    /// </summary>
    public interface IRoleService : IPaginationService<RoleModel>
    {
        void Add(RoleModel model);
        void Update(RoleModel model);
        void Delete(DynamicRole role);
        void DeleteMultiple(List<DynamicRole> roles);
        DynamicRole GetByIdAndCompanyId(int roleId, int companyId);
        List<DynamicRole> GetByIdsAndCompanyId(List<int> ids, int companyId);

        List<PermissionGroupModel> ChangeJSONtoModel(string permissionList);
        List<PermissionGroupModel> GetDefaultRoleValueByCompany(int companyId, int accountType = 6);

        List<EnumModel> GetRoleList();

        bool CheckPermissionEnabled(string permissionName, int accountId, int companyId);
        bool CheckPermissionEnabled(string permissionGroup, string permission, int accountId, int companyId);
        void AddDefaultRole(int companyId);
        void UpdatePermissionsInDB(PlugIn plugIn, int accountType);


        Dictionary<string, Dictionary<string, bool>> GetPermissionsByAccountId(int accountId);
        Dictionary<string, Dictionary<string, bool>> GetPermissionsByCompanyAccountId(int companyAccountId);
        Dictionary<string, Dictionary<string, bool>> GetPermissionsByCompanyAccountId(CompanyAccount companyAccount);

        void AddDefault();
        void UpdateDefaultPermission();
        bool IsExist(int roleId, string roleName);

        List<DynamicRole> GetByTypeAndCompanyId(int type, int companyId);

        void ChangeDefaultSettingRoleCompany(int roleId, int companyId);
        bool CheckPermissions(List<PermissionGroupModel> permissions);

        int GetUserCountByRoleId(int roleId, int companyId);
    }

    /// <summary>
    /// Class of role service.
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Service of dynamic role function.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="companyService"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public RoleService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor,
            /*ICompanyService companyService, */ISettingService settingService, IConfiguration configuration, ILogger<AccountService> logger)
        {
            _unitOfWork = unitOfWork;
            //_companyService = companyService;
            _settingService = settingService;
            _httpContext = httpContextAccessor.HttpContext;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Get role data in paginated form.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public IQueryable<RoleModel> GetPaginated(string filter, int pageNumber, int pageSize,
            string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();

            var data = _unitOfWork.AppDbContext.DynamicRole.Where(m => m.CompanyId == companyId && !m.IsDeleted).ToList();

            var result = data.Select(m => new RoleModel()
            {
                Id = m.Id,
                RoleName = m.Name,
                IsDefault = m.RoleSettingDefault,
                EnableDepartmentLevel = m.EnableDepartmentLevel,
                RoleSettingDefault = m.RoleSettingDefault,
                Description = m.Description,
                UserCount = GetUserCountByRoleId(m.Id, companyId),
                //PermissionGroups = ChangeJSONtoModel(m.PermissionList)
            });

            totalRecords = result.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().RemoveDiacritics().ToLower();

                result = result.Where(m => m.RoleName?.RemoveDiacritics()?.ToLower()?.Contains(filter) == true);
            }

            recordsFiltered = result.Count();

            result = result.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return result.AsQueryable();
        }

        public List<DynamicRole> GetByTypeAndCompanyId(int type, int companyId)
        {
            var roles = _unitOfWork.RoleRepository.GetByTypeAndCompanyId(type, companyId);

            return roles.ToList();
        }

        /// <summary>
        /// This function changes JSON data to list of model data.
        /// This function is used to get data from DB with name and description about permissions.
        /// </summary>
        /// <param name="permissionList"> Permission data in JSON form </param>
        /// <returns> list of model data </returns>
        public List<PermissionGroupModel> ChangeJSONtoModel(string permissionList)
        {
            var accountLanguage = _unitOfWork.AppDbContext.Account.Where(m => m.Id == _httpContext.User.GetAccountId()).FirstOrDefault().Language;
            //var companyId = _httpContext.User.GetCompanyId();
            //var companyLanguage = _settingService.GetLanguage(companyId);
            var culture = new CultureInfo(accountLanguage);

            List<PermissionGroupModel> permissionGroupModel = new List<PermissionGroupModel>();

            if (string.IsNullOrEmpty(permissionList))
            {
                return permissionGroupModel;
            }

            var jsonObject = JsonConvert.DeserializeObject<List<PermissionGroupDataModel>>(permissionList);

            permissionGroupModel = jsonObject.Select(
                m =>
                {
                    var permissionGroup = new PermissionGroupModel
                    {
                        Title = m.Title,
                        GroupName = PermissionResource.ResourceManager.GetString(m.Title, culture),
                        Permissions = m.Permissions.Select(
                            c =>
                            {
                                var permission = new PermissionModel
                                {
                                    Title = c.Title,
                                    PermissionName = PermissionResource.ResourceManager.GetString(c.Title, culture),
                                    Description = PermissionResource.ResourceManager.GetString(Constants.Description + c.Title, culture),
                                    IsEnabled = c.IsEnabled
                                };

                                return permission;
                            }).ToList()
                    };

                    return permissionGroup;
                }).ToList();

            return permissionGroupModel;
        }

        /// <summary>
        /// This function changes model data to JSON string form data.
        /// This function is used to store data in DB without name and description.
        /// </summary>
        /// <param name="permissionGroups"> Permission model data </param>
        /// <returns> permission data in JSON string form </returns>
        public string ChangeModelToJSON(List<PermissionGroupModel> permissionGroups)
        {
            if (permissionGroups == null || !permissionGroups.Any())
            {
                return null;
            }

            List<PermissionGroupDataModel> permissionGroupDatas = new List<PermissionGroupDataModel>();

            permissionGroupDatas = permissionGroups.Select(
                m =>
                {
                    var permissionGroupData = new PermissionGroupDataModel
                    {
                        Title = m.Title,
                        Permissions = m.Permissions.Select(
                            c =>
                            {
                                var permissionData = new PermissionDataModel
                                {
                                    Title = c.Title,
                                    IsEnabled = c.IsEnabled
                                };

                                return permissionData;
                            }).ToList()
                    };

                    return permissionGroupData;
                }).ToList();

            var permissionList = JsonConvert.SerializeObject(permissionGroupDatas, Formatting.None);

            return permissionList;
        }

        /// <summary>
        /// This function changes model data to JSON string form data.
        /// This function is used to store data in DB without name and description.
        /// </summary>
        /// <param name="permissionDataGroups"></param>
        /// <returns></returns>
        public string ChangeModelToJSON(List<PermissionGroupDataModel> permissionDataGroups)
        {
            var permissionList = JsonConvert.SerializeObject(permissionDataGroups, Formatting.None);

            return permissionList;
        }


        /// <summary>
        /// Add a new role
        /// </summary>
        /// <param name="model"></param>
        public void Add(RoleModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var companyId = _httpContext.User.GetCompanyId();

                        DynamicRole role = new DynamicRole
                        {
                            Name = model.RoleName,
                            PermissionList = ChangeModelToJSON(model.PermissionGroups),
                            CompanyId = companyId,
                            TypeId = (int)AccountType.DynamicRole,
                            RoleSettingDefault = model.IsDefault ? model.IsDefault : model.RoleSettingDefault,
                            Description = model.Description,
                            EnableDepartmentLevel = model.EnableDepartmentLevel
                        };

                        _unitOfWork.RoleRepository.Add(role);
                        _unitOfWork.Save();

                        if (model.IsDefault)
                        {
                            // Update all other roles to not default
                            var roleList = _unitOfWork.RoleRepository.GetAll().Where(m => m.CompanyId == companyId && m.TypeId != (int)AccountType.DynamicRole && m.Id != role.Id).ToList();
                            foreach (var roleTmp in roleList)
                            {
                                roleTmp.RoleSettingDefault = false;
                                _unitOfWork.RoleRepository.Update(roleTmp);
                                _unitOfWork.Save();
                            }
                        }
                        
                        //Save system log
                        var content = RoleResource.lblAddNewRole;
                        List<string> details = new List<string>
                        {
                            $"{RoleResource.lblRoleName} : {role.Name}",
                            $"{RoleResource.lblRole} : {role.PermissionList}"
                        };
                        var contentsDetails = string.Join("\n", details);

                        _unitOfWork.SystemLogRepository.Add(role.Id, SystemLogType.Role, ActionLogType.Add,
                            content, contentsDetails, null, _httpContext.User.GetCompanyId());

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in Add");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Update a role
        /// </summary>
        /// <param name="model"></param>
        public void Update(RoleModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var companyId = _httpContext.User.GetCompanyId();
                        var role = _unitOfWork.RoleRepository.GetByIdAndCompanyId(model.Id, companyId).FirstOrDefault();

                        role.Name = model.RoleName;
                        role.EnableDepartmentLevel = model.EnableDepartmentLevel;
                        role.Description = model.Description;
                        role.RoleSettingDefault = model.IsDefault ? model.IsDefault : model.RoleSettingDefault;
                        if (model.PermissionGroups != null && model.PermissionGroups.Any())
                        {
                            role.PermissionList = ChangeModelToJSON(model.PermissionGroups);
                        }

                        _unitOfWork.RoleRepository.Update(role);
                        _unitOfWork.Save();
                        
                        if (model.IsDefault)
                        {
                            // Update all other roles to not default
                            var roleList = _unitOfWork.RoleRepository.GetAll().Where(m => m.CompanyId == companyId && m.TypeId != (int)AccountType.DynamicRole && m.Id != role.Id).ToList();
                            foreach (var roleTmp in roleList)
                            {
                                roleTmp.RoleSettingDefault = false;
                                _unitOfWork.RoleRepository.Update(roleTmp);
                                _unitOfWork.Save();
                            }
                        }

                        //Save system log
                        var content = RoleResource.lblUpdateRole;
                        List<string> details = new List<string>
                        {
                            $"{RoleResource.lblRoleName} : {role.Name}",
                            $"{RoleResource.lblEnableDepartmentRole} : {role.EnableDepartmentLevel}",
                            $"{RoleResource.lblRole} : {role.PermissionList}"
                        };
                        var contentsDetails = string.Join("\n", details);

                        _unitOfWork.SystemLogRepository.Add(role.Id, SystemLogType.Role, ActionLogType.Update,
                            content, contentsDetails, null, _httpContext.User.GetCompanyId());

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in Update");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }


        /// <summary>
        /// Delete a role
        /// </summary>
        /// <param name="role"></param>
        public void Delete(DynamicRole role)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //Change
                        // Delete -> Update
                        // Bacause of account's foreign key.
                        //_unitOfWork.RoleRepository.Delete(role);

                        role.IsDeleted = true;
                        role.UpdatedOn = DateTime.UtcNow;
                        _unitOfWork.RoleRepository.Update(role);

                        //Save system log
                        var content = RoleResource.lblDeleteRole;
                        List<string> details = new List<string>
                        {
                            $"{RoleResource.lblRoleName} : {role.Name}",
                        };
                        var contentsDetails = string.Join("\n", details);

                        _unitOfWork.SystemLogRepository.Add(role.Id, SystemLogType.Role, ActionLogType.Delete,
                            content, contentsDetails, null, _httpContext.User.GetCompanyId());

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in Delete");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Delete roles
        /// </summary>
        /// <param name="roles"></param>
        public void DeleteMultiple(List<DynamicRole> roles)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //_unitOfWork.RoleRepository.DeleteRange(roles);

                        List<string> details = new List<string>();
                        List<int> assignedIds = new List<int>();

                        foreach (var role in roles)
                        {
                            role.IsDeleted = true;
                            role.UpdatedOn = DateTime.UtcNow;
                            _unitOfWork.RoleRepository.Update(role);

                            details.Add($"{RoleResource.lblRoleName} : {role.Name}");
                            assignedIds.Add(role.Id);
                        }

                        //Save system log
                        var content = RoleResource.lblDeleteRole;
                        var contentsDetails = string.Join("\n", details);

                        _unitOfWork.SystemLogRepository.Add(roles.First().Id, SystemLogType.Role, ActionLogType.DeleteMultiple,
                            content, contentsDetails, assignedIds, _httpContext.User.GetCompanyId());

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in DeleteMultiple");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Get default permission value(true/false) by company (plugIn) 
        /// This function is used to show on web.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="accountType"> integer flag to distinguish that account is primary manager or employee or not </param>
        /// <returns></returns>
        public List<PermissionGroupModel> GetDefaultRoleValueByCompany(int companyId, int accountType = 6)
        {
            var accountLanguage = _unitOfWork.AppDbContext.Account.Where(m => m.Id == _httpContext.User.GetAccountId()).FirstOrDefault().Language;
            //var companyLanguage = _settingService.GetLanguage(companyId);
            var culture = new CultureInfo(accountLanguage);

            var permissionGroups = new List<PermissionGroupModel>();

            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
            var pluginPolicy = JsonConvert.DeserializeObject<Dictionary<string, bool>>(plugin.PlugIns);

            if (!pluginPolicy.ContainsKey(Constants.PlugIn.Common))
            {
                var permission = PagePermission.GetCommonPermssions().FirstOrDefault();
                permissionGroups.Add(GetListModel(permission, culture, accountType));
            }

            foreach (var keyPlug in pluginPolicy.ToList())
            {
                if (!keyPlug.Value && keyPlug.Key != Constants.PlugIn.Common)
                    pluginPolicy.Remove(keyPlug.Key);
                else
                {
                    foreach (var eachData in PagePermission.GetPermssionsByPlugIn(keyPlug.Key))
                    {
                        permissionGroups.Add(GetListModel(eachData, culture, accountType));
                    }
                }
            }

            return permissionGroups;
        }

        /// <summary>
        /// Set default permission value(true/false) by company (plugIn) 
        /// This function is used to store in DB.
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="accountType"> integer flag to distinguish that account is primary manager or employee or not </param>
        /// <returns></returns>
        public List<PermissionGroupDataModel> SetDefaultRoleValueByCompany(int companyId, int accountType = 6, string currentPermissions = null)
        {
            var permissionGroups = new List<PermissionGroupDataModel>();

            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
            var pluginPolicy = JsonConvert.DeserializeObject<Dictionary<string, bool>>(plugin.PlugIns);

            if (!pluginPolicy.ContainsKey(Constants.PlugIn.Common))
            {
                var permission = PagePermission.GetCommonPermssions().FirstOrDefault();
                permissionGroups.Add(GetListModelforDB(permission, accountType, currentPermissions));
            }

            foreach (var keyPlug in pluginPolicy.ToList())
            {
                if (!keyPlug.Value && keyPlug.Key != Constants.PlugIn.Common)
                    pluginPolicy.Remove(keyPlug.Key);
                else
                {
                    foreach (var eachData in PagePermission.GetPermssionsByPlugIn(keyPlug.Key))
                    {
                        permissionGroups.Add(GetListModelforDB(eachData, accountType, currentPermissions));
                    }
                }
            }

            return permissionGroups;
        }

        /// <summary>
        /// Get data to display on the web, including translated sentences.
        /// </summary>
        /// <param name="data"> role and permission data in Key-Value pair form </param>
        /// <param name="culture"> culture information for localizaion </param>
        /// <param name="accountType"> integer flag to distinguish that account is primary manager or employee or not(dynamic) </param>
        /// <param name="forDB"> boolean flag to distinguish that data is for on web or to DB. </param>
        /// <returns></returns>
        public PermissionGroupModel GetListModel(KeyValuePair<string, Dictionary<string, bool>> data, CultureInfo culture, int accountType = 6)
        {
            var permissionGroup = new PermissionGroupModel();

            var permissions = new List<PermissionModel>();

            foreach (var permission in data.Value)
            {
                var permissionModel = new PermissionModel
                {
                    Title = permission.Key,
                    PermissionName = PermissionResource.ResourceManager.GetString(permission.Key, culture),
                    Description = PermissionResource.ResourceManager.GetString(Constants.Description + permission.Key, culture),
                    IsEnabled = accountType == (int)AccountType.DynamicRole
                                ? permission.Value
                                : IsEnabledByAccount(accountType, permission)
                };
                permissions.Add(permissionModel);
            }
            permissionGroup.Title = data.Key;
            permissionGroup.GroupName = PermissionResource.ResourceManager.GetString(data.Key, culture);
            permissionGroup.Permissions = permissions;

            return permissionGroup;
        }

        /// <summary>
        /// Get data to store in DB. ( Compare version )
        /// </summary>
        /// <param name="data"> role and permission data in Key-Value pair form </param>
        /// <param name="accountType"> integer flag to distinguish that account is primary manager or employee or not(dynamic) </param>
        /// <returns></returns>
        public PermissionGroupDataModel GetListModelforDB(KeyValuePair<string, Dictionary<string, bool>> data, int accountType = 6, string currentPermissions = null)
        {
            var permissionGroup = new PermissionGroupDataModel();

            var permissions = new List<PermissionDataModel>();

            foreach (var permission in data.Value)
            {
                var permissionModel = new PermissionDataModel
                {
                    Title = permission.Key,
                    IsEnabled = accountType == (int)AccountType.DynamicRole && string.IsNullOrEmpty(currentPermissions)
                                ? permission.Value
                                : IsEnabledByAccount(accountType, permission, currentPermissions)
                };
                permissions.Add(permissionModel);
            }
            permissionGroup.Title = data.Key;
            permissionGroup.Permissions = permissions;

            return permissionGroup;
        }


        /// <summary>
        /// This function can distinguish an secondary can have specific permission or not through the permission name check.
        /// </summary>
        /// <param name="permissionName"> name of permisson </param>
        /// <returns></returns>
        private bool IsSecondaryEnabled(string permissionName)
        {
            bool isView = permissionName.Contains(ActionName.View);

            return isView;
        }


        /// <summary>
        /// This function has the list of permission name that employee can do.
        /// This function can distinguish an employee can have specific permission or not through the list.
        /// </summary>
        /// <param name="permissionName"> name of permisson </param>
        /// <returns></returns>
        private bool IsEmployeeEnabled(string permissionName)
        {
            List<string> enableList = new List<string>
            {
                //new MonitoringPage().GetPermission(nameof(MonitoringPage.View)),
                //new TimeAttendanceReportPage().GetPermission(nameof(TimeAttendanceReportPage.View))
                // ActionName.View + Page.Monitoring,
                ActionName.View + Page.TimeAttendanceReport,
                ActionName.View + Page.BookManagement,

                ActionName.View + Page.VisitManagement,
                ActionName.Add + Page.VisitManagement,
                ActionName.Edit + Page.VisitManagement,
                ActionName.ViewHistory + Page.VisitManagement,
                ActionName.Approve + Page.VisitManagement,
                ActionName.ReturnCard + Page.VisitManagement,

                ActionName.View + Page.Building,
                ActionName.View + Page.AccessibleDoor,

                ActionName.View + Page.Report,

                ActionName.ManageOwnRecord + Page.LeaveRequest,
            };

            if (enableList.Contains(permissionName))
                return true;
            else
                return false;
        }

        private bool IsDynamicEnabled(string permissionName, bool permissionValue, string currentPermission)
        {
            var permission = JsonConvert.DeserializeObject<List<PermissionGroupDataModel>>(currentPermission)
                            .SelectMany(m => m.Permissions)
                            .Where(m => m.Title == permissionName)
                            .FirstOrDefault();

            if (permission != null)
            {
                return permission.IsEnabled;
            }
            else
            {
                return permissionValue;
            }
        }

        /// <summary>
        /// This function is for existing account type.
        /// </summary>
        /// <param name="accountType"></param>
        /// <param name="permissionName"></param>
        /// <returns></returns>
        private bool IsEnabledByAccount(int accountType, KeyValuePair<string, bool> permission, string currentPermission = null)
        {
            switch (accountType)
            {
                case (int)AccountType.SystemAdmin:
                case (int)AccountType.SuperAdmin:
                case (int)AccountType.PrimaryManager:
                    return true;
                case (int)AccountType.SecondaryManager:
                    return IsSecondaryEnabled(permission.Key);
                case (int)AccountType.Employee:
                    return IsEmployeeEnabled(permission.Key);
                case (int)AccountType.DynamicRole:
                    return IsDynamicEnabled(permission.Key, permission.Value, currentPermission);
                default:
                    return false;
            }
        }


        /// <summary>
        /// Get dynamic role by id and company id
        /// </summary>
        /// <param name="roleId"> identifier of role </param>
        /// <param name="companyId"> identifier of company </param>
        /// <returns></returns>
        public DynamicRole GetByIdAndCompanyId(int roleId, int companyId)
        {
            var role = _unitOfWork.RoleRepository.GetByIdAndCompanyId(roleId, companyId).FirstOrDefault();

            return role;
        }

        /// <summary>
        /// Get dynamic role by companyId and list of id
        /// </summary>
        /// <param name="ids"> list of dynamic role identifier </param>
        /// <param name="companyId"> identifier of company </param>
        /// <returns></returns>
        public List<DynamicRole> GetByIdsAndCompanyId(List<int> ids, int companyId)
        {
            var roles = _unitOfWork.RoleRepository.GetByyIdsAndCompanyId(ids, companyId).ToList();

            return roles;
        }

        /// <summary>
        /// Get role list with identifier of role
        /// </summary>
        /// <returns></returns>
        public List<EnumModel> GetRoleList()
        {
            var companyId = _httpContext.User.GetCompanyId();
            //var defaultRoleMaxId = EnumHelper.ToEnumList<AccountType>().Max(m => m.Id);

            var data = _unitOfWork.RoleRepository.GetByCompanyId(companyId).ToList();

            List<EnumModel> result = data.Select(
                m =>
                {
                    var list = new EnumModel
                    {
                        //Id = m.Id + defaultRoleMaxId,
                        Id = m.Id,
                        Name = m.Name
                    };

                    return list;
                }).ToList();

            return result;
        }


        /// <summary>
        /// This function is used when login.
        /// This function returns permission list that specific account has.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, bool>> GetPermissionsByAccountId(int accountId)
        {
            Dictionary<string, Dictionary<string, bool>> result = new Dictionary<string, Dictionary<string, bool>>();
            var account = _unitOfWork.AccountRepository.GetByIdWithRole(accountId);

            if (account != null && account.DynamicRole != null)
            {
                var permissionList = account.DynamicRole.PermissionList;

                var modelData = JsonConvert.DeserializeObject<List<PermissionGroupDataModel>>(permissionList);

                foreach (var model in modelData)
                {
                    Dictionary<string, bool> permission = new Dictionary<string, bool>();

                    foreach (var permissionModel in model.Permissions)
                    {
                        permission.Add(permissionModel.Title, permissionModel.IsEnabled);
                    }

                    result.Add(model.Title, permission);
                }

                return result;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// This function is used when login.
        /// This function returns permission list that specific company account has.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, bool>> GetPermissionsByCompanyAccountId(int companyAccountId)
        {
            Dictionary<string, Dictionary<string, bool>> result = new Dictionary<string, Dictionary<string, bool>>();
            var companyAccount = _unitOfWork.CompanyAccountRepository.GetCompanyAccountById(companyAccountId);

            if (companyAccount != null && companyAccount.DynamicRole != null)
            {
                var permissionList = companyAccount.DynamicRole.PermissionList;

                var modelData = JsonConvert.DeserializeObject<List<PermissionGroupDataModel>>(permissionList);

                foreach (var model in modelData)
                {
                    Dictionary<string, bool> permission = new Dictionary<string, bool>();

                    foreach (var permissionModel in model.Permissions)
                    {
                        permission.Add(permissionModel.Title, permissionModel.IsEnabled);
                    }

                    result.Add(model.Title, permission);
                }

                return result;
            }
            else
            {
                return result;
            }
        }

        /// <summary>
        /// This function is used when login.
        /// This function returns permission list that specific company account has.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string, bool>> GetPermissionsByCompanyAccountId(CompanyAccount companyAccount)
        {
            Dictionary<string, Dictionary<string, bool>> result = new Dictionary<string, Dictionary<string, bool>>();
            //var companyAccount = _unitOfWork.CompanyAccountRepository.GetCompanyAccountById(companyAccountId);

            if (companyAccount != null && companyAccount.DynamicRole != null)
            {
                var permissionList = companyAccount.DynamicRole.PermissionList;

                var modelData = JsonConvert.DeserializeObject<List<PermissionGroupDataModel>>(permissionList);

                foreach (var model in modelData)
                {
                    Dictionary<string, bool> permission = new Dictionary<string, bool>();

                    foreach (var permissionModel in model.Permissions)
                    {
                        permission.Add(permissionModel.Title, permissionModel.IsEnabled);
                    }

                    result.Add(model.Title, permission);
                }

                return result;
            }
            else
            {
                return result;
            }
        }


        /// <summary>
        /// Check whether the permission is true or not
        /// </summary>
        /// <param name="permissionName"> permission name </param>
        /// <param name="accountId" > identifier of account </param>
        /// <param name="companyId"> idenetifier of company </param>
        /// <returns></returns>
        public bool CheckPermissionEnabled(string permissionName, int accountId, int companyId)
        {
            var role = _unitOfWork.RoleRepository.GetByAccountId(accountId, companyId);

            if (role == null)
            {
                return false;
            }
            else
            {
                var permission = role.PermissionGroups.SelectMany(m => m.Permissions).Where(m => m.Title == permissionName).FirstOrDefault();

                if (permission == null)
                    return false;

                return permission.IsEnabled;
            }
        }

        /// <summary>
        /// Check whether the permission is true or not
        /// </summary>
        /// <param name="permissionGroupName"> permission group name </param>
        /// <param name="permissionName"> permission name </param>
        /// <param name="accountId" > identifier of account </param>
        /// <param name="companyId"> idenetifier of company </param>
        /// <returns></returns>
        public bool CheckPermissionEnabled(string permissionGroupName, string permissionName, int accountId, int companyId)
        {
            var role = _unitOfWork.RoleRepository.GetByAccountId(accountId, companyId);

            if (role == null)
            {
                return false;
            }
            else
            {
                PermissionGroupModel permissionGroup = role.PermissionGroups.Where(m => m.Title == permissionGroupName).FirstOrDefault();

                PermissionModel permission = permissionGroup.Permissions.Where(m => m.Title == permissionName).FirstOrDefault();

                if (permission == null)
                    return false;

                return permission.IsEnabled;
            }
        }

        /// <summary>
        /// Add default role when a new company is created.
        /// </summary>
        /// <param name="companyId"></param>
        public void AddDefaultRole(int companyId)
        {
            DynamicRole primary = new DynamicRole()
            {
                CompanyId = companyId,
                TypeId = (int)AccountType.PrimaryManager,
                Name = AccountType.PrimaryManager.GetDescription(),
                PermissionList = ChangeModelToJSON(SetDefaultRoleValueByCompany(companyId, (int)AccountType.PrimaryManager)),
                Description = AccountType.PrimaryManager.GetDescription(),
            };

            _unitOfWork.RoleRepository.Add(primary);

            DynamicRole secondary = new DynamicRole()
            {
                CompanyId = companyId,
                TypeId = (int)AccountType.SecondaryManager,
                Name = AccountType.SecondaryManager.GetDescription(),
                PermissionList = ChangeModelToJSON(SetDefaultRoleValueByCompany(companyId, (int)AccountType.SecondaryManager)),
                Description = AccountType.SecondaryManager.GetDescription(),
            };

            _unitOfWork.RoleRepository.Add(secondary);

            DynamicRole employee = new DynamicRole()
            {
                CompanyId = companyId,
                TypeId = (int)AccountType.Employee,
                Name = AccountType.Employee.GetDescription(),
                PermissionList = ChangeModelToJSON(SetDefaultRoleValueByCompany(companyId, (int)AccountType.Employee)),
                RoleSettingDefault = true,
                Description = AccountType.Employee.GetDescription(),
            };

            try
            {
                _unitOfWork.RoleRepository.Add(employee);

                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddDefaultRole");
            }
        }

        /// <summary>
        /// Update existing permissions in database.
        /// </summary>
        /// <param name="companyId"> identifier of company </param>
        /// <param name="plugIn"> updated plugin </param>
        public void UpdatePermissionsInDB(PlugIn plugIn, int accountType)
        {
            var roles = _unitOfWork.RoleRepository.GetByTypeAndCompanyId(accountType, plugIn.CompanyId);

            if (roles != null && roles.Any())
            {
                var pluginPolicy = JsonConvert.DeserializeObject<Dictionary<string, bool>>(plugIn.PlugIns);

                foreach (var role in roles)
                {
                    var permissionGroups = new List<PermissionGroupDataModel>();

                    if (!pluginPolicy.ContainsKey(Constants.PlugIn.Common))
                    {
                        var permission = PagePermission.GetCommonPermssions().FirstOrDefault();
                        permissionGroups.Add(GetListModelforDB(permission, accountType, role.PermissionList));
                    }

                    foreach (var keyPlug in pluginPolicy.ToList())
                    {
                        if (!keyPlug.Value && keyPlug.Key != Constants.PlugIn.Common)
                            pluginPolicy.Remove(keyPlug.Key);
                        else
                        {
                            foreach (var eachData in PagePermission.GetPermssionsByPlugIn(keyPlug.Key))
                            {
                                permissionGroups.Add(GetListModelforDB(eachData, accountType, role.PermissionList));
                            }
                        }
                    }

                    role.PermissionList = JsonConvert.SerializeObject(permissionGroups, Formatting.None);
                    try
                    {
                        _unitOfWork.RoleRepository.Update(role);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in UpdatePermissionsInDB");
                    }
                }

                try
                {
                    _unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in UpdatePermissionsInDB");
                }
            }
        }


        /// <summary>
        /// Add default roles to existing companies.
        /// </summary>
        public void AddDefault()
        {
            var data = _unitOfWork.CompanyRepository.GetAll().Where(m => !m.IsDeleted);

            foreach (var company in data)
            {
                //var accounts = _unitOfWork.AccountRepository.GetAccountsByCompanyId(company.Id);
                var companyAccounts = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompany(company.Id);

                var primaryRoles = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)AccountType.PrimaryManager, company.Id);

                if (primaryRoles == null || !primaryRoles.Any())
                {
                    DynamicRole primary = new DynamicRole()
                    {
                        CompanyId = company.Id,
                        TypeId = (int)AccountType.PrimaryManager,
                        Name = AccountType.PrimaryManager.GetDescription(),
                        PermissionList = ChangeModelToJSON(SetDefaultRoleValueByCompany(company.Id, (int)AccountType.PrimaryManager)),
                        Description = AccountType.PrimaryManager.GetDescription(),
                    };

                    _unitOfWork.RoleRepository.Add(primary);
                    _unitOfWork.Save();

                    // set value DynamicRoleId attribute
                    //foreach (var account in accounts)
                    //{
                    //    if (account.Type == (short)AccountType.PrimaryManager)
                    //    {
                    //        account.DynamicRoleId = primary.Id;
                    //        _unitOfWork.AccountRepository.Update(account);
                    //    }
                    //}
                }

                var secondaryRoles = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)AccountType.SecondaryManager, company.Id);

                if (secondaryRoles == null || !secondaryRoles.Any())
                {
                    DynamicRole secondary = new DynamicRole()
                    {
                        CompanyId = company.Id,
                        TypeId = (int)AccountType.SecondaryManager,
                        Name = AccountType.SecondaryManager.GetDescription(),
                        PermissionList = ChangeModelToJSON(SetDefaultRoleValueByCompany(company.Id, (int)AccountType.SecondaryManager)),
                        Description = AccountType.SecondaryManager.GetDescription(),
                    };

                    _unitOfWork.RoleRepository.Add(secondary);
                    _unitOfWork.Save();

                    // set value DynamicRoleId attribute
                    //foreach (var account in accounts)
                    //{
                    //    if (account.Type == (short)AccountType.SecondaryManager)
                    //    {
                    //        account.DynamicRoleId = secondary.Id;
                    //        _unitOfWork.AccountRepository.Update(account);
                    //    }
                    //}
                }


                var employeeRoles = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)AccountType.Employee, company.Id);

                if (employeeRoles == null || !employeeRoles.Any())
                {
                    DynamicRole employee = new DynamicRole()
                    {
                        CompanyId = company.Id,
                        TypeId = (int)AccountType.Employee,
                        Name = AccountType.Employee.GetDescription(),
                        PermissionList = ChangeModelToJSON(SetDefaultRoleValueByCompany(company.Id, (int)AccountType.Employee)),
                        Description = AccountType.Employee.GetDescription(),
                    };

                    _unitOfWork.RoleRepository.Add(employee);
                    _unitOfWork.Save();

                    // set value DynamicRoleId attribute
                    // If there isn't DynamicRoleId of companyAccount, set the value as Employee.
                    foreach (var companyAccount in companyAccounts)
                    {
                        if (companyAccount.DynamicRoleId == null)
                        {
                            companyAccount.DynamicRoleId = employee.Id;
                            _unitOfWork.CompanyAccountRepository.Update(companyAccount);
                        }
                    }
                }
            }

            _unitOfWork.Save();
        }

        /// <summary>
        /// This function is used only one time when a new functions are added in system.
        /// </summary>
        public void UpdateDefaultPermission()
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var companies = _unitOfWork.CompanyRepository.GetAll().Where(m => !m.IsDeleted);

                        foreach (var company in companies)
                        {
                            var primaryRole = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)AccountType.PrimaryManager, company.Id).FirstOrDefault();
                            primaryRole.PermissionList = ChangeModelToJSON(SetDefaultRoleValueByCompany(company.Id, (int)AccountType.PrimaryManager));
                            primaryRole.Description = AccountType.PrimaryManager.GetDescription();
                            _unitOfWork.RoleRepository.Update(primaryRole);

                            var secondaryRole = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)AccountType.SecondaryManager, company.Id).FirstOrDefault();
                            secondaryRole.PermissionList = ChangeModelToJSON(SetDefaultRoleValueByCompany(company.Id, (int)AccountType.SecondaryManager));
                            secondaryRole.Description = AccountType.SecondaryManager.GetDescription();
                            _unitOfWork.RoleRepository.Update(secondaryRole);

                            var employeeRole = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)AccountType.Employee, company.Id).FirstOrDefault();
                            employeeRole.PermissionList = ChangeModelToJSON(SetDefaultRoleValueByCompany(company.Id, (int)AccountType.Employee));
                            employeeRole.Description = AccountType.Employee.GetDescription();
                            _unitOfWork.RoleRepository.Update(employeeRole);

                            var dynamicRoles = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)AccountType.DynamicRole, company.Id).ToList();
                            foreach (var dynamicRole in dynamicRoles)
                            {
                                dynamicRole.PermissionList = ChangeModelToJSON(SetDefaultRoleValueByCompany(company.Id, (int)AccountType.DynamicRole, dynamicRole.PermissionList));
                                dynamicRole.Description = dynamicRole.Description;
                                _unitOfWork.RoleRepository.Update(dynamicRole);
                            }

                            // Get all accounts in the company.
                            //var accounts = _unitOfWork.AccountRepository.GetAccountsByCompanyId(company.Id);

                            //foreach (var account in accounts)
                            //{
                            //    if (account.Type == (short)AccountType.PrimaryManager)
                            //    {
                            //        account.DynamicRoleId = primaryRole.Id;
                            //        _unitOfWork.AccountRepository.Update(account);
                            //    }
                            //    else if (account.Type == (short)AccountType.SecondaryManager)
                            //    {
                            //        account.DynamicRoleId = secondaryRole.Id;
                            //        _unitOfWork.AccountRepository.Update(account);
                            //    }
                            //    else if (account.Type == (short)AccountType.Employee)
                            //    {
                            //        account.DynamicRoleId = employeeRole.Id;
                            //        _unitOfWork.AccountRepository.Update(account);
                            //    }
                            //    else if(account.Type == (short)AccountType.DynamicRole && account.DynamicRoleId == null)
                            //    {
                            //        account.Type = (short)AccountType.Employee;
                            //        account.DynamicRoleId = employeeRole.Id;
                            //        _unitOfWork.AccountRepository.Update(account);
                            //    }
                            //}
                        }

                        //Save and commit
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in UpdateDefaultPermission");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }


        public bool IsExist(int roleId, string roleName)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var role = _unitOfWork.RoleRepository.GetByNameAndCompanyId(roleName, _httpContext.User.GetCompanyId());
            role = role.Where(m => m.Id != roleId);

            if (role.Count() > 0)
                return true;
            else
                return false;

        }

        public void ChangeDefaultSettingRoleCompany(int roleId, int companyId)
        {
            var roles = _unitOfWork.RoleRepository.GetByCompanyId(companyId);
            foreach (var role in roles)
            {
                role.RoleSettingDefault = role.Id == roleId;
                _unitOfWork.RoleRepository.Update(role);
            }
            _unitOfWork.Save();
        }


        public bool CheckPermissions(List<PermissionGroupModel> permissions)
        {
            var permissionGroups = permissions.GroupBy(m => m.Title);

            return !permissionGroups.Any(m => m.Count() > 1);
        }

        public int GetUserCountByRoleId(int roleId, int companyId)
        {
            var accountList = _unitOfWork.CompanyAccountRepository.GetAll().Where(m => m.DynamicRoleId == roleId && m.CompanyId == companyId).ToList();
            return accountList.Count;
        }
    }
}