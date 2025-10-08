using System.Collections.Generic;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Role;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DocumentFormat.OpenXml.Office.CustomUI;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Controller of Role setting
    /// </summary>
    [Produces("application/ms-excel", "application/json", "application/text")]
    [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RoleController : Controller
    {
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly ICompanyService _companyService;
        private readonly IAccountService _accountService;

        private readonly IRoleService _roleService;

        /// <summary>
        /// Role controller
        /// </summary>
        /// <param name="roleService"> Service of Dynamic role </param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"> configuration </param>
        /// <param name="companyService"> service of company </param>
        /// <param name="accountService"> service of account </param>
        public RoleController(IRoleService roleService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, ICompanyService companyService, IAccountService accountService)
        {
            _roleService = roleService;
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration;
            _companyService = companyService;
            _accountService = accountService;
        }

        /// <summary>
        /// Get list of Role from DB
        /// </summary>
        /// <param name="search">Query string that filter by name</param>
        /// <param name="pageNumber">Page number start from 1</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiRoles)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Gets(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "0",
            string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<RoleModel>(sortColumn, "RoleName");
            var permissions = _roleService
                .GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered).AsEnumerable().ToList();

            var pagingData = new PagingData<RoleModel>
            {
                Data = permissions,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }


        /// <summary>
        /// Get role information by id
        /// </summary>
        /// <param name="id"> identifier of role </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Role does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiRolesId)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Get(int id)
        {
            var model = new RoleModel();
            var role = new DynamicRole();
            var companyId = _httpContext.User.GetCompanyId();

            if (id != 0)
            {
                role = _roleService.GetByIdAndCompanyId(id, companyId);
                if (role == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundRole);
                }

                // Should compare below two list.
                // To print out the list of permissions created after the role creation.
                // If it is a permission in the DB, print it out as the value stored in the DB.
                // But the permission that is created after the role creation, print it out the value as "false".
                var defaultPermissionGroupList = _roleService.GetDefaultRoleValueByCompany(companyId);
                var userPermissionGroups = _roleService.ChangeJSONtoModel(role.PermissionList);

                foreach (var defaultGroupModel in defaultPermissionGroupList)
                {
                    var userPermissionGroup = userPermissionGroups.Where(m => m.Title == defaultGroupModel.Title).FirstOrDefault();

                    if (userPermissionGroup != null)
                    {
                        foreach (var defaultModel in defaultGroupModel.Permissions)
                        {
                            var userPermission = userPermissionGroup.Permissions.Where(m => m.Title == defaultModel.Title).FirstOrDefault();

                            if (userPermission != null)
                            {
                                defaultModel.IsEnabled = userPermission.IsEnabled;
                            }
                            else
                            {
                                defaultModel.IsEnabled = false;
                            }
                        }
                    }
                    else
                    {
                        foreach (var defaultModel in defaultGroupModel.Permissions)
                        {
                            defaultModel.IsEnabled = false;
                        }
                    }
                }

                model.Id = role.Id;
                model.RoleName = role.Name;
                model.IsDefault = role.RoleSettingDefault;
                model.RoleSettingDefault = role.RoleSettingDefault;
                model.PermissionGroups = defaultPermissionGroupList;
                model.EnableDepartmentLevel = role.EnableDepartmentLevel;
                model.Description = role.Description;
                model.UserCount = _roleService.GetUserCountByRoleId(role.Id, companyId);
            }
            else
            {
                model.PermissionGroups = _roleService.GetDefaultRoleValueByCompany(companyId);
            }

            model.PermissionGroups = model.PermissionGroups.OrderBy(m => m.Title).ToList();
            return Ok(model);
        }

        /// <summary>
        /// Add a new role to system
        /// </summary>
        /// <param name="model"> This model has information about the device to be added. </param>
        /// <param name="similarId"> identifier of role that is similar-made </param>
        /// <returns></returns>
        /// <response code="201">Create new a role</response>
        /// <response code="400">Bad Request: similar role not null</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiRoles)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Add([FromBody]RoleModel model, int similarId = 0)
        {
            // Below code is for unexcepted exception.
            // There is an error about edit -> add in very quick.
            // In some cases, API can't check the duplication because the Id value in model is sent as same with edited data's Id from FE.
            // ex) Id = 108 data is edited, and new role is added, Add model's Id value is 108.
            model.Id = 0;

            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }

            var companyId = _httpContext.User.GetCompanyId();

            if (similarId != 0)
            {
                var similarRole = _roleService.GetByIdAndCompanyId(similarId, companyId);

                if(similarRole != null)
                    model.PermissionGroups = _roleService.ChangeJSONtoModel(similarRole.PermissionList);
                else
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.AnUnexpectedErrorOccurred));
            }

            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }

            if (!_roleService.CheckPermissions(model.PermissionGroups))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgDuplicatedData, RoleResource.lblRole.ToLower()));
            }

            _roleService.Add(model);

            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, RoleResource.lblRole.ToLower()));
        }


        /// <summary>
        /// Update permission list of spectific role
        /// </summary>
        /// <param name="id"> identifier of this role </param>
        /// <param name="model"> data model that include information </param>
        /// <param name="similarId"> identifier of role that is similar-made </param>
        /// <returns></returns>
        /// <response code="400">Bad Request: similar role not null</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiRolesId)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Edit(int id, [FromBody]RoleModel model, int similarId = 0)
        {
            model.Id = id;
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }

            var companyId = _httpContext.User.GetCompanyId();

            if(similarId != 0)
            {
                var similarRole = _roleService.GetByIdAndCompanyId(similarId, companyId);

                if (similarRole != null)
                    model.PermissionGroups = _roleService.ChangeJSONtoModel(similarRole.PermissionList);
                else
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.AnUnexpectedErrorOccurred));
            }

            if (model.PermissionGroups != null && model.PermissionGroups.Any() && !_roleService.CheckPermissions(model.PermissionGroups))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgDuplicatedData, RoleResource.lblRole.ToLower()));
            }

            var role = _roleService.GetByIdAndCompanyId(id, companyId);

            if (role != null)
            {
                model.Id = id;

                // if(role.TypeId != (int)AccountType.DynamicRole && model.PermissionGroups != null)
                // {
                //     return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.InvalidRoleUpdate));
                // }

                _roleService.Update(model);

                return new ApiSuccessResult(StatusCodes.Status200OK,
                    string.Format(MessageResource.MessageUpdateSuccess, role.Name, $"({RoleResource.lblRole.ToLower()})"));
            }

            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundRole);
        }


        /// <summary>
        /// Delete a role
        /// </summary>
        /// <param name="id"> identifier of dynamic role </param>
        /// <returns></returns>
        /// <response code="400">Bad Request: type role in role not null</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Role does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiRolesId)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Delete(int id)
        {
            var companyId = _httpContext.User.GetCompanyId();

            var role = _roleService.GetByIdAndCompanyId(id, companyId);
            if (role == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundRole);
            }

            if (role.TypeId != (int)AccountType.DynamicRole)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgFailedDeleteBasicRole));
            }

            var companyAccounts = _accountService.GetCompanyAccountByRoleId(role.Id, companyId);
            if(companyAccounts != null && companyAccounts.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.roleIsAssignedAndCannotDelete));
            }

            _roleService.Delete(role);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, RoleResource.lblRole.ToLower()));
        }


        /// <summary>
        /// Delete multiple roles
        /// </summary>
        /// <param name="ids"> list of identifier of dynamic role </param>
        /// <returns></returns>
        /// <response code="400">Bad Request: type role in role not null</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: List of Roles does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiRoles)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteMultiple(List<int> ids)
        {
            var companyId = _httpContext.User.GetCompanyId();

            var roles = _roleService.GetByIdsAndCompanyId(ids, companyId);
            if (!roles.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundRole);
            }

            foreach(var role  in roles)
            {
                if (role.TypeId != (int)AccountType.DynamicRole)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgFailedDeleteBasicRole));
                }
            }

            var accounts = _accountService.GetCompanyAccountByRoleIds(ids, companyId);
            if (accounts != null && accounts.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.roleIsAssignedAndCannotDelete));
            }

            _roleService.DeleteMultiple(roles);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, RoleResource.lblRole.ToLower()));
        }


        /// <summary>
        /// Add a default role to existing companies.
        /// </summary>
        /// <returns></returns>
        /// <response code="201">Create new role default</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiDefaultRoles)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult AddDefault()
        {
            _roleService.AddDefault();

            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, RoleResource.lblRole.ToLower()));
        }


        /// <summary>
        /// Update default permissions.
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiDefaultRoles)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdateDefaultPermission()
        {
            _roleService.UpdateDefaultPermission();

            return new ApiSuccessResult(StatusCodes.Status200OK,
                    string.Format(MessageResource.MessageUpdateSuccess, RoleResource.lblRole.ToLower(), ""));
        }

        /// <summary>
        /// Update role setting default
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiChangeRoleSettingDefault)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ChangeDefaultRoleSettingForUser(int id)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var role = _roleService.GetByIdAndCompanyId(id, companyId);
            if (role == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundRole);
            
            _roleService.ChangeDefaultSettingRoleCompany(id, companyId);
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSuccess);
        }
    }
}