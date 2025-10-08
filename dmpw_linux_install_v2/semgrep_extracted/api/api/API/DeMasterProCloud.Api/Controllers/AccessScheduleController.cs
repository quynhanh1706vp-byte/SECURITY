using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.AccessSchedule;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;

namespace DeMasterProCloud.Api.Controllers
{
    [Produces("application/json")]
    public class AccessScheduleController : Controller
    {
        private readonly IAccessScheduleService _accessScheduleService;
        private readonly IDeviceService _deviceService;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly HttpContext _httpContext;
        private readonly IDepartmentService _departmentService;
        private readonly IAccessGroupService _accessGroupService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessScheduleService"></param>
        /// <param name="deviceService"></param>
        /// <param name="userService"></param>
        /// <param name="accountService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="departmentService"></param>
        /// <param name="accessGroupService"></param>
        public AccessScheduleController(IAccessScheduleService accessScheduleService, IDeviceService deviceService, IUserService userService, 
            IAccountService accountService, IHttpContextAccessor httpContextAccessor, IDepartmentService departmentService,
            IAccessGroupService accessGroupService)
        {
            _accessScheduleService = accessScheduleService;
            _deviceService = deviceService;
            _userService = userService;
            _accountService = accountService;
            _httpContext = httpContextAccessor.HttpContext;
            _departmentService = departmentService;
            _accessGroupService = accessGroupService;
        }


        /// <summary>
        /// Get init accessSchedule page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessSchedulesInit)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessSchedule)]
        public IActionResult GetInit()
        {
            var data = _accessScheduleService.GetInit();
            
           
            
            return Ok(data);
        }

        /// <summary>
        /// Get list of accessSchedule with company
        /// </summary>
        /// <param name="content">Content of accessSchedule</param>
        /// <param name="userIds">List of user ids</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessSchedules)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessSchedule)]
        public IActionResult Gets(string content, List<int> userIds, int pageNumber = 1, int pageSize = 10, string sortColumn = "Content", string sortDirection = "asc")
        {
           
            var models = _accessScheduleService.GetPaginated(content, userIds, pageNumber, pageSize, sortColumn, sortDirection,
                out var recordsTotal, out var recordsFiltered);
            
            var pagingData = new PagingData<AccessScheduleListModel>
            {
                Data = models,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }
        
        /// <summary>
        /// Get accessSchedule by id
        /// </summary>
        /// <param name="id">AccessSchedule Id</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessSchedulesId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessSchedule)]
        public IActionResult Get(int id)
        {
            var model = _accessScheduleService.GetDetailByAccessScheduleId(id);
            if (model == null) return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundAccessSchedule);
            
            return Ok(model);
        }
        
        /// <summary>
        /// Add new accessSchedule
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessSchedules)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.AccessSchedule)]
        public IActionResult Add([FromBody] AccessScheduleModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            var (hasOverlap, overlappingUserNames) = _accessScheduleService.CheckAccessScheduleOverlapWithUsers(model);
            if (hasOverlap)
            {
                var userNamesStr = overlappingUserNames.Any() ? string.Join(", ", overlappingUserNames) : "";
                var errorMessage = string.IsNullOrEmpty(userNamesStr)
                    ? MessageResource.msgAccessScheduleTimeOverlap
                    : $"{MessageResource.msgAccessScheduleTimeOverlap} ({userNamesStr})";

                return new ApiErrorResult(StatusCodes.Status400BadRequest, errorMessage);
            }
            
            if (model.UserIds != null && model.UserIds.Any())
            {
                var users = _userService.GetUsersByUserIds(model.UserIds);
                if(!users.Any()) return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.NotNullValidator, UserResource.lblUser));
            }
            
            int accessScheduleId = _accessScheduleService.Add(model);
            if (accessScheduleId == 0) return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.MessageAddNewFailed, AccessScheduleResource.lblAccessSchedule));
            
            return new ApiSuccessResult(StatusCodes.Status201Created, string.Format(MessageResource.MessageAddSuccess, AccessScheduleResource.lblAccessSchedule));
        }
        
        /// <summary>
        /// Edit accessSchedule
        /// </summary>
        /// <param name="id">AccessSchedule Id</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessSchedulesId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessSchedule)]
        public IActionResult Edit(int id, [FromBody] AccessScheduleModel model)
        {
            model.Id = id;
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }
            var accessSchedule = _accessScheduleService.GetById(id);
            if (accessSchedule == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundAccessSchedule);
            }

            var (hasOverlap, overlappingUserNames) = _accessScheduleService.CheckAccessScheduleOverlapWithUsers(model);
            if (hasOverlap)
            {
                var userNamesStr = overlappingUserNames.Any() ? string.Join(", ", overlappingUserNames) : "";
                var errorMessage = string.IsNullOrEmpty(userNamesStr)
                    ? MessageResource.msgAccessScheduleTimeOverlap
                    : $"{MessageResource.msgAccessScheduleTimeOverlap} ({userNamesStr})";

                return new ApiErrorResult(StatusCodes.Status400BadRequest, errorMessage);
            }
            
           
            
            if (model.UserIds != null && model.UserIds.Any())
            {
                var users = _userService.GetUsersByUserIds(model.UserIds);
                if(!users.Any()) return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.NotNullValidator, DeviceResource.lblDevice));
            }
            
            bool result = _accessScheduleService.Edit(accessSchedule, model);
            if (!result)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.MessageUpdateFailed, AccessScheduleResource.lblAccessSchedule));
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, AccessScheduleResource.lblAccessSchedule, model.Content));
        }
        
        /// <summary>
        /// Delete accessSchedule
        /// </summary>
        /// <param name="id">AccessSchedule Id</param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessSchedulesId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.AccessSchedule)]
        public IActionResult Delete(int id)
        {
            var accessSchedule = _accessScheduleService.GetById(id);
            if (accessSchedule == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundAccessSchedule);
            
            bool result = _accessScheduleService.Delete(accessSchedule);
            if (!result)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessScheduleResource.DeletedFailed);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, AccessScheduleResource.lblAccessSchedule));
        }
        
        /// <summary>
        /// Delete list accessSchedules
        /// </summary>
        /// <param name="ids">List of ids accessSchedule</param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessSchedules)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.AccessSchedule)]
        public IActionResult DeleteRange(List<int> ids)
        {
            var accessSchedules = _accessScheduleService.GetByIds(ids);
            if (!accessSchedules.Any())
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundAccessSchedule);
            
            bool result = _accessScheduleService.DeleteRange(accessSchedules);
            if (!result)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessScheduleResource.DeletedFailed);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, AccessScheduleResource.lblAccessSchedule,"(s)"));
        }
       

       

        /// <summary>
        /// Get all users of accessSchedule
        /// </summary>
        /// <param name="id">AccessSchedule Id</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessSchedulesIdUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessSchedule)]
        public IActionResult GetUsersOfAccessSchedule(int id)
        {
            var accessSchedule = _accessScheduleService.GetByAccessScheduleId(id);
            if(accessSchedule == null) return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundAccessSchedule);
            
            List<AccessScheduleUserInfo> users = _accessScheduleService.GetUsersByAccessSchedule(accessSchedule);
            return Ok(users);
        }
        /// <summary>
        /// Get list of all users with accessSchedule by id
        /// </summary>
        /// <param name="id">AccessSchedule Id</param>
        /// <param name="search">Query string that filter Users by UserName, Department Name or CardId</param>
        /// <param name="pageNumber">Page number start from 1</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string of the column</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessSchedulesAssignUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessSchedule)]
        public IActionResult GetAssignUsers(int id, string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "FirstName",
            string sortDirection = "asc")
        {
            var accessSchedules = _accessScheduleService.GetPaginatedForUsers(id, new List<int>(),search, pageNumber, pageSize, sortColumn, sortDirection, 
                out var recordsTotal, out var recordsFiltered);

            var pagingData = new PagingData<UserForAccessSchedule>
            {
                Data = accessSchedules,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }

        /// <summary>
        /// Get list of all users unassigned with accessSchedule by id
        /// </summary>
        /// <param name="id">AccessSchedule Id</param>
        /// <param name="idsIgnore"></param>
        /// <param name="search">Query string that filter Users by UserName, Department Name or AccessSchedule Name</param>
        /// <param name="pageNumber">Page number start from 1</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessSchedulesUnAssignedUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessSchedule)]
        public IActionResult GetUnAssignUsers(int id, List<int> idsIgnore ,string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "FirstName",
            string sortDirection = "asc")
        {
            var accessSchedules = _accessScheduleService
                .GetPaginatedForUsers(id, idsIgnore,search, pageNumber, pageSize, sortColumn, sortDirection,
                out var recordsTotal, out var recordsFiltered, false);

            var pagingData = new PagingData<UserForAccessSchedule>
            {
                Data = accessSchedules,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }
        
       
        
        
        
        /// <summary>
        /// Assign users to an accessSchedule
        /// </summary>
        /// <param name="id">       accessSchedule identifier. </param>
        /// <param name="userIds">  List of identifiers for the users. </param>
        /// <returns>   An IActionResult. </returns>
        /// <response code="400">Bad Request: AccessSchedule Id does not exist, list of User Ids empty or user assigned for accessSchedule</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPost]
        [Route(Constants.Route.ApiAccessSchedulesAssignUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessSchedule)]
        public IActionResult AssignUsers(int id, [FromBody]List<int> userIds)
        {
            if (id == 0 || userIds == null || !userIds.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessScheduleResource.msgEmptyUser);
            }
            var accessSchedule = _accessScheduleService.GetById(id);
            if (accessSchedule == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundAccessSchedule);
            }
            
            var companyId = _httpContext.User.GetCompanyId();
            
            // check door is already in department with department level
            bool usersInvalid = _accessGroupService.CheckUserInDepartment(companyId, _httpContext.User.GetAccountId(), userIds);
            if (!usersInvalid)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, AccessScheduleResource.msgCannotAssignUsers);
            }
            var result = _accessScheduleService.AssignUsersToAccessSchedule(id, userIds);
            if (!string.IsNullOrEmpty(result))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, result);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, AccessScheduleResource.msgAssignUsers);
        }

       
        [HttpDelete]
        [Route(Constants.Route.ApiAccessScheduleUnAssignUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessSchedule)]
        public IActionResult UnAssignUser(int id, [FromBody]List<int> userIds)
        {
            if (id == 0 || userIds == null || !userIds.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessScheduleResource.msgEmptyUser);
            }

            var result = _accessScheduleService.UnAssignUsersToAccessSchedule(id, userIds);
            if (!string.IsNullOrEmpty(result))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, result);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, AccessScheduleResource.msgUnAssignUsers);
        }

    }
}