using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.AccessGroup;
using DeMasterProCloud.DataModel.AccessGroupDevice;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.Header;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Access group controller
    /// </summary>
    [Produces("application/json")]
    [CheckAddOnAttribute(Constants.PlugIn.AccessControl)]

    public class AccessGroupController : Controller
    {
        private readonly IAccessGroupService _accessGroupService;
        private readonly HttpContext _httpContext;
        private readonly IVisitService _visitService;
        private readonly IAccessGroupDeviceService _accessGroupDeviceService;
        private readonly IDepartmentService _departmentService;
        private readonly IMapper _mapper;
        private readonly IPluginService _pluginService;

        /// <summary>
        /// Constructor for inject services
        /// </summary>
        /// <param name="accessGroupService"></param>
        /// <param name="contextAccessor"></param>
        /// <param name="visitService"> service of visit </param>
        /// <param name="departmentService"></param>
        /// <param name="accessGroupDeviceService"></param>
        /// <param name="mapper"></param>
        /// <param name="pluginService"></param>
        public AccessGroupController(IAccessGroupService accessGroupService, IHttpContextAccessor contextAccessor,
            IVisitService visitService, IDepartmentService departmentService,
            IAccessGroupDeviceService accessGroupDeviceService, IMapper mapper, IPluginService pluginService)
        {
            _accessGroupService = accessGroupService;
            _httpContext = contextAccessor.HttpContext;
            _visitService = visitService;
            _departmentService = departmentService;
            _accessGroupDeviceService = accessGroupDeviceService;
            _mapper = mapper;
            _pluginService = pluginService;
        }

        /// <summary>
        /// List all access group with pagination
        /// </summary>
        /// <param name="search">Query string that filter Access Group by Name.</param>
        /// <param name="doorIds">Door Ids</param>
        /// <param name="userIds">User Ids</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessGroups)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessGroup)]
        public IActionResult Gets(string search, List<int> doorIds, List<int> userIds, int pageNumber = 1, int pageSize = 10, string sortColumn = "Name",
            string sortDirection = "asc")
        {
            sortColumn = Helpers.CheckPropertyInObject<AccessGroupListModel>(sortColumn, "Name", ColumnDefines.AccessGroup);
            sortDirection = "asc";

            var accessGroups = _accessGroupService
                .GetPaginated(search, doorIds, userIds, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered);

            var pagingData = new PagingData<AccessGroupListModel>
            {
                Data = accessGroups,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get a access group by id
        /// </summary>
        /// <param name="id">Access Group Id</param>
        /// <returns></returns>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not found: Access group Id does not exist.</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessGroupsId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessGroup)]
        public IActionResult Get(int id)
        {
            var accessGroup = _accessGroupService.GetById(id);
            if (accessGroup == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound);
            }
            var model = _mapper.Map<AccessGroupListModel>(accessGroup);
            return Ok(model);
        }

        /// <summary>
        /// Get list of doors with access group by id
        /// </summary>
        /// <param name="id">Access Group Id</param>
        /// <param name="search">Query string that filter Doors by Device Address, Name, Timezone or Building</param>
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
        [Route(Constants.Route.ApiAccessGroupsDoors)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessGroup)]
        public IActionResult GetAssignDoors(int id, string search, List<int> operationType = null, int pageNumber = 1, int pageSize = 10, string sortColumn = null,
            string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<AccessGroupDeviceDoor>(sortColumn, "DeviceAddress", ColumnDefines.AccessGroupForDoors);
            var accessGroups = _accessGroupService
                .GetPaginatedForDoors(id, search, operationType, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered);
            
            var pagingData = new PagingData<AccessGroupDeviceDoor>
            {
                Data = accessGroups,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get list of doors unassigned with access group by id
        /// </summary>
        /// <param name="id">Access Group Id</param>
        /// <param name="search">Query string that filter Doors by Device Address, Name or Building</param>
        /// <param name="operationType"></param>
        /// <param name="buildingIds"></param>
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
        [Route(Constants.Route.ApiAccessGroupsUnAssignedDoors)]
        [Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessGroup)]
        public IActionResult GetUnAssignDoors(int id, string search, List<int> operationType = null, List<int> buildingIds = null, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id",
            string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<AccessGroupDeviceUnAssignDoor>(sortColumn, "Id", ColumnDefines.AccessGroupForUnAssignDoors);
            
            var accessGroups = _accessGroupService
                .GetPaginatedForUnAssignDoors(id, search, operationType, buildingIds, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered);
            
            var pagingData = new PagingData<AccessGroupDeviceUnAssignDoor>
            {
                Data = accessGroups,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get list of all doors by accessGroupId for visit
        /// </summary>
        /// <param name="id">Access Group Id</param>
        /// <param name="search">Query string that filter Doors by Building Name</param>
        /// <param name="pageNumber">Page number start from 1</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="onlyDefaultDoor">If using onlyDefaultDoor = true, list device response: default door config page visit setting</param>
        /// <returns></returns>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessGroupsAllDoorsForVisit)]
        [Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUnAssignDoorsForVisit(int id, string search, int pageNumber = 1, int pageSize = 10, string sortColumn = null,
            string sortDirection = "desc", bool onlyDefaultDoor = false)
        {
            var unassignedDoorsTree = _accessGroupService
                .GetPaginatedAllDoorsForVisit(id, search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered);

            if (onlyDefaultDoor)
            {
                var accessGroupDevice = _accessGroupDeviceService.GetByAccessGroupId(id);
                var doorDefault = accessGroupDevice.Select(x => x.IcuId);
                List<Node> newData = new List<Node>();
                foreach (var uNode in unassignedDoorsTree)
                {
                    uNode.Devices = uNode.Devices.Where(m => doorDefault.Contains(m.Id)).ToList();
                    if (uNode.Devices.Any())
                    {
                        newData.Add(uNode);
                    }
                }

                unassignedDoorsTree = newData;
            }

            var pagingData = new PagingData<Node>
            {
                Data = unassignedDoorsTree,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get list of all users with access group by id
        /// </summary>
        /// <param name="id">Access Group Id</param>
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
        [Route(Constants.Route.ApiAccessGroupsUsersList)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessGroup)]
        public IActionResult GetAssignUsers(int id, string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "FirstName",
            string sortDirection = "desc")
        {
            if (!int.TryParse(sortColumn, out var index))
            {
                sortColumn = Helpers.CheckPropertyInObject<UserForAccessGroup>(sortColumn, "FirstName");
            }
            
            var users = _accessGroupService
                .GetPaginatedForUsers(id, search, pageNumber, pageSize, sortColumn, sortDirection, 
                out var recordsTotal, out var recordsFiltered, out List<HeaderData> userHeaders, Page.AccessGroup + Page.User);

            var pagingData = new PagingData<UserForAccessGroup, HeaderData>
            {
                Data = users,
                Header = userHeaders,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }

        /// <summary>
        /// Get list of all users unassigned with access group by id
        /// </summary>
        /// <param name="id">Access Group Id</param>
        /// <param name="search">Query string that filter Users by UserName, Department Name or Access Group Name</param>
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
        [Route(Constants.Route.ApiAccessGroupsUnAssignedUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessGroup)]
        public IActionResult GetUnAssignUsers(int id, string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "FirstName",
            string sortDirection = "desc")
        {
            //sortColumn = Helpers.CheckPropertyInObject<UnAssignUserForAccessGroup>(sortColumn, "FullName", ColumnDefines.AccessGroupForUnAssignUsers);
            if (!int.TryParse(sortColumn, out var index))
            {
                sortColumn = Helpers.CheckPropertyInObject<UserForAccessGroup>(sortColumn, "FirstName");
            }

            var users = _accessGroupService
                .GetPaginatedForUsers(id, search, pageNumber, pageSize, sortColumn, sortDirection,
                out var recordsTotal, out var recordsFiltered, out List<HeaderData> userHeaders, Page.AccessGroup + Page.UnAssignUser, false);

            var pagingData = new PagingData<UserForAccessGroup, HeaderData>
            {
                Data = users,
                Header = userHeaders,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }

        /// <summary>
        /// Add new access group
        /// </summary>
        /// <param name="model">JSON model for Access Group</param>
        /// <returns></returns>
        /// <response code="201">Create new access group success</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Unprocessable Entity: Access Group Name exist</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPost]
        [Route(Constants.Route.ApiAccessGroups)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.AccessGroup)]
        public IActionResult Add([FromBody]AccessGroupModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            if (_httpContext.User.GetAccountType() != (short)AccountType.PrimaryManager && model.IsDefault)
            {
                return new ApiSuccessResult(StatusCodes.Status403Forbidden, AccessGroupResource.msgNotPermissionAddDefaultAg);
            }
            _accessGroupService.Add(model);
            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, AccessGroupResource.lblAccessGroup.ToLower()));
        }

        /// <summary>
        /// Edit a access group
        /// </summary>
        /// <param name="id">Access Group Id</param>
        /// <param name="model">JSON model for Access Group</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Access Group Name exist.</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Access Group Id bot found from DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPut]
        [Route(Constants.Route.ApiAccessGroupsId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessGroup)]
        public IActionResult Edit(int id, [FromBody]AccessGroupModel model)
        {
            model.Id = id;

            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }

            var accessGroup = _accessGroupService.GetById(id);
            if (accessGroup == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound);
            }

            if(accessGroup.Type != (short)AccessGroupType.NormalAccess && !(!accessGroup.IsDefault && model.IsDefault))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                        string.Format(AccessGroupResource.msgCanNotUpdateAccessGroup, accessGroup.Name));
            }
            if (_httpContext.User.GetAccountType() != (short)AccountType.PrimaryManager && model.IsDefault)
            {
                return new ApiSuccessResult(StatusCodes.Status403Forbidden, AccessGroupResource.msgNotPermissionChangeDefaultAg);
            }
            _accessGroupService.Update(model);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, AccessGroupResource.lblAccessGroup.ToLower(), ""));
        }

        /// <summary>
        /// Delete a access group
        /// </summary>
        /// <param name="id">Access Group Id</param>
        /// <returns></returns>
        /// <response code="200">Delete a access group success</response>
        /// <response code="400">Bad Request: Access Group could not delete</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Access Group Id bot found from DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpDelete]
        [Route(Constants.Route.ApiAccessGroupsId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.AccessGroup)]
        public IActionResult Delete(int id)
        {
            var accessGroupModel = _accessGroupService.GetById(id);
            if (accessGroupModel == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound);
            }

            var name = _accessGroupService.GetAGNameCanNotDelete(new List<int> { id });
            if (!string.IsNullOrWhiteSpace(name))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    string.Format(AccessGroupResource.msgCanNotDeleteAccessGroup, name));
            }

            var accessGroup = _mapper.Map<AccessGroup>(accessGroupModel);
            _accessGroupService.Delete(accessGroup);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, AccessGroupResource.lblAccessGroup));
        }

        /// <summary>
        /// Delete list access group
        /// </summary>
        /// <param name="ids">list ids of access group</param>
        /// <returns></returns>
        /// <response code="200">Delete list access group success</response>
        /// <response code="400">Bad Request: List of Access Group Ids include one or more Access Group could not delete</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: List ids do not exist</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpDelete]
        [Route(Constants.Route.ApiAccessGroups)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.AccessGroup)]
        public IActionResult DeleteMultiple(List<int> ids)
        {
            var accessGroups = _accessGroupService.GetByIds(ids);
            if (!accessGroups.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound);
            }

            var name = _accessGroupService.GetAGNameCanNotDelete(ids);
            if (!string.IsNullOrWhiteSpace(name))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    string.Format(AccessGroupResource.msgCanNotDeleteAccessGroup, name));
            }

            _accessGroupService.DeleteRange(accessGroups);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteMultipleSuccess, AccessGroupResource.lblAccessGroup));
        }

        /// <summary>
        /// Assign users to an access group
        /// </summary>
        /// <param name="id">       Access group identifier. </param>
        /// <param name="userIds">  List of identifiers for the users. </param>
        /// <returns>   An IActionResult. </returns>
        /// <response code="400">Bad Request: Access group Id does not exist, list of User Ids empty or user assigned for access group</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPost]
        [Route(Constants.Route.ApiAccessGroupsAssignUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessGroup)]
        public IActionResult AssignUsers(int id, [FromBody]List<int> userIds)
        {
            if (id == 0 || userIds == null || !userIds.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.msgEmptyUser);
            }

            var companyId = _httpContext.User.GetCompanyId();
            // check door is already in department with department level
            var isFullAg = _accessGroupService.IsFullAccessGroup(companyId, id);
            bool usersInvalid = _accessGroupService.CheckUserInDepartment(companyId, _httpContext.User.GetAccountId(), userIds);
            var enablePlugin = _pluginService.CheckPluginCondition(Constants.PlugIn.DepartmentAccessLevel, companyId);
            if (!usersInvalid || (isFullAg && enablePlugin && _httpContext.User.GetAccountType() == (short)AccountType.DynamicRole))
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, AccessGroupResource.msgCannotAssignUsers);
            }

            var result = _accessGroupService.AssignUsers(id, userIds);
            if (!string.IsNullOrWhiteSpace(result))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, result);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, AccessGroupResource.msgAssignUsers);
        }

        /// <summary>
        /// Assign doors to an access group
        /// </summary>
        /// <param name="id">Access Group Id</param>
        /// <param name="model">JSON model for (doorId, timezoneId and companyId)</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Access group Id does not exist, model of the doors empty</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPost]
        [Route(Constants.Route.ApiAccessGroupsAssignDoors)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessGroup)]
        public IActionResult AssignDoors(int id, [FromBody]AccessGroupAssignDoor model)
        {
            if (id == 0 || model == null || !model.Doors.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.msgEmptyDoorAssign);
            }

            var accessGroup = _accessGroupService.GetById(id);
            var newModel = new AccessGroupAssignDoor();
            int companyId = _httpContext.User.GetCompanyId();
            int accountId = _httpContext.User.GetAccountId();

            if (accessGroup.Name.Contains(Constants.Settings.NameAccessGroupVisitor))
            {
                string[] str = accessGroup.Name.Split("-");
                var visit = _visitService.GetById(Int32.Parse(str[1]));
                if (visit.CreatedBy != accountId)
                {
                    var lastApproval = _visitService.GetLastApproval(visit.Id);
                    if (lastApproval != 0)
                    {
                        if (lastApproval != accountId)
                        {
                            return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.assingedVisitAuthor);
                        }
                    }
                    else
                    {
                        return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.assingedVisitNotApproval);
                    }
                }

                var visitSetting = _visitService.GetVisitSettingCompany();
                var accessGroupDevice = _accessGroupDeviceService.GetByAccessGroupId(visitSetting.AccessGroupId);
                foreach (var door in model.Doors)
                {
                    var detailModel = new AccessGroupAssignDoorDetail()
                    {
                        DoorId = door.DoorId,
                        TzId = accessGroupDevice.FirstOrDefault(x => x.IcuId == door.DoorId)?.TzId 
                               ?? accessGroupDevice.Select(x =>x.TzId).FirstOrDefault(),
                        CompanyId = companyId
                    };

                    newModel.Doors.Add(detailModel);
                }
            }
            else
            {
                foreach (var door in model.Doors)
                {
                    var detailModel = new AccessGroupAssignDoorDetail()
                    {
                        DoorId = door.DoorId,
                        TzId = door.TzId,
                        CompanyId = companyId
                    };

                    newModel.Doors.Add(detailModel);
                }
            }
            // check door is already in department with department level
            bool doorsInvalid = _departmentService.CheckDoorInDepartment(companyId, accountId, newModel.Doors.Select(x => x.DoorId).ToList());
            if (!doorsInvalid)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, AccessGroupResource.msgCannotAssignDevices);
            }
            var result = _accessGroupService.AssignDoors(id, newModel);
            if (!string.IsNullOrWhiteSpace(result))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, result);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, AccessGroupResource.msgAssignDoors);
        }

        /// <summary>
        /// Remove doors from an access group
        /// </summary>
        /// <param name="id">Access Group Id</param>
        /// <param name="doorIds">List of ids with doors</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Access group Id does not exist, model of the doors empty</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpDelete]
        [Route(Constants.Route.ApiAccessGroupsUnAssignDoors)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessGroup)]
        public IActionResult UnAssignDoors(int id, [FromBody]List<int> doorIds)
        {
            if (id == 0 || doorIds == null || !doorIds.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.msgEmptyDoorAssign);
            }

            var accessGroup = _accessGroupService.GetById(id);
            if (accessGroup.Name.Contains(Constants.Settings.NameAccessGroupVisitor))
            {
                string[] str = accessGroup.Name.Split("-");
                var visit = _visitService.GetById(Int32.Parse(str[1]));
                var lastApproval = _visitService.GetLastApproval(visit.Id);
                if (lastApproval != 0)
                {
                    if (lastApproval != _httpContext.User.GetAccountId())
                    {
                        return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.assingedVisitAuthor);
                    }
                }
                else
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.assingedVisitNotApproval);
                }
            }
            if (accessGroup != null && accessGroup.Type == (short)AccessGroupType.FullAccess)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.msgCanNotDeleteDevice);
            }
            // check door is already in department with department level
            bool doorsInvalid = _departmentService.CheckDoorInDepartment(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId(), doorIds);
            if (!doorsInvalid)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, AccessGroupResource.msgCannotUnAssignDevices);
            }
            
            _accessGroupService.UnAssignDoors(id, doorIds);
            return new ApiSuccessResult(StatusCodes.Status200OK, AccessGroupResource.msgUnAssignDoors);
        }

        /// <summary>
        /// Remove users from an access group
        /// </summary>
        /// <param name="id">Access Group Id</param>
        /// <param name="userIds">List of ids with users</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Access group Id does not exist, list of User Ids empty or user assigned for access group</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpDelete]
        [Route(Constants.Route.ApiAccessGroupsUnAssignUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessGroup)]
        public IActionResult UnAssignUsers(int id, [FromBody]List<int> userIds)
        {
            if (id == 0 || userIds == null || !userIds.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.msgEmptyUser);
            }

            var accessGroup = _accessGroupService.GetById(id);
            if(accessGroup == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, AccessGroupResource.msgNotFoundAccessGroup);
            }

            if (accessGroup.IsDefault)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.msgUnassignDefaultGroup);
            }
            // check user is already in department with department level
            bool usersInvalid = _accessGroupService.CheckUserInDepartment(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId(), userIds);
            if (!usersInvalid)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, AccessGroupResource.msgCannotUnAssignUsers);
            }
            _accessGroupService.UnAssignUsers(id, userIds);
            return new ApiSuccessResult(StatusCodes.Status200OK, AccessGroupResource.msgUnAssignUsers);
        }

        /// <summary>
        /// Delete devices from all access groups
        /// </summary>
        /// <param name="doorIds"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route(Constants.Route.ApiUnAssignDoorsAllAccessGroup)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessGroup)]
        public IActionResult RemoveDeviceFromAllAccessGroup([FromBody] List<int> doorIds)
        {
            if (doorIds == null || !doorIds.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.msgEmptyDoors);
            }
            
            _accessGroupService.RemoveDeviceFromAllAccessGroup(doorIds);
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.msgDeleteUnregisteredDevice);
        }

        /// <summary>
        /// Add access group -> assign door with access time names (apply to HVQY)
        /// </summary>
        /// <param name="accessTimeNames"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiAddAccessGroupWithAccessTimeNames)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.AccessGroup)]
        public IActionResult AddAccessGroupByAccessTimeName([FromBody] List<string> accessTimeNames)
        {
            if (accessTimeNames == null || !accessTimeNames.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.msgEmptyDoors);
            }

            _accessGroupService.AddAccessGroupWithAccessTime(accessTimeNames);
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSuccess);
        }
    }
}