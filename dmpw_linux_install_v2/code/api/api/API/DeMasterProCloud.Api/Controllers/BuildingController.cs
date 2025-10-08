using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Building;
using DeMasterProCloud.DataModel.Header;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Service;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Service.Infrastructure.Header;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Building controller
    /// </summary>
    [Produces("application/json")]
    //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BuildingController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IBuildingService _buildingService;
        private readonly HttpContext _httpContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// Building controller
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="buildingService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="mapper"></param>
        public BuildingController(IConfiguration configuration, IBuildingService buildingService,
            IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _configuration = configuration;
            _buildingService = buildingService;
            _httpContext = httpContextAccessor.HttpContext;
            _mapper = mapper;
        }

        /// GET /buildings : return all existing buildings with paging and sorting
        /// <summary>
        /// Get list of Buildings with pagination
        /// </summary>
        /// <param name="search">Query string that filter Building by Name</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiBuildings)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[CheckPermission(ActionName.View + Page.Building)]
        [CheckMultiPermission(new string[] { (ActionName.View + Page.Building), (ActionName.Add + Page.VisitManagement) }, false)]
        public IActionResult Gets(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id",
            string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<BuildingListModel>(sortColumn, "Id", ColumnDefines.BuildingList);
            var buildings = _buildingService.GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered).ToList();

            var pagingData = new PagingData<BuildingListModel>
            {
                Data = buildings,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// GET /buildings : return all existing buildings with paging and sorting
        /// <summary>
        /// Get Building by id
        /// </summary>
        /// <param name="id">identified of Building</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="404">Not Found: Id Building does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiBuildingsId)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Building)]
        public IActionResult Get(int id)
        {
            BuildingModel model = new BuildingModel();
            
            if(id != 0)
            {
                var building = _buildingService.GetById(id);
                if (building == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
                }
                model = _mapper.Map<BuildingModel>(building);
            }

            var result = _buildingService.InitData(model);
            return Ok(result);
        }


        /// GET /buildings/{id} : return all existing doors of a building with paging and sorting
        /// <summary>
        /// Get list of doors list in a building(by id building)
        /// </summary>
        /// <param name="id">Identified of Building</param>
        /// <param name="search">Query string that filter Door in Building by door name, device address, activeTZ or PassageTZ</param>
        /// <param name="operationType"></param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiBuildingsDoorListById)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Building)]
        public IActionResult GetDoors(int id, string search, short operationType = 0, int pageNumber = 1, int pageSize = 10, string sortColumn = null,
            string sortDirection = "desc")
        {
            var doors = _buildingService.GetPaginatedDoors(id, search, operationType, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered).ToList();

            var pagingData = new PagingData<BuildingDoorModel>
            {
                Data = doors,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }



        /// GET /buildings/{id} : return all existing doors of a building with paging and sorting
        /// <summary>
        /// Get list of doors list in a building(by id building)
        /// </summary>
        /// <param name="ids"> list of building identifier </param>
        /// <param name="search">Query string that filter Door in Building by door name, device address, activeTZ or PassageTZ</param>
        /// <param name="operationType"></param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiBuildingsDoorList)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Building)]
        public IActionResult GetBuildingsDoors(List<int> ids, string search, short operationType = 0, int pageNumber = 1, int pageSize = 10, string sortColumn = null,
            string sortDirection = "desc")
        {
            var doors = _buildingService.GetPaginatedDoors(ids, search, operationType, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered);

            IPageHeader pageHeader = new PageHeader(_configuration, Page.Building + Page.Device, _httpContext.User.GetCompanyId());
            var header = pageHeader.GetHeaderList(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId());

            var pagingData = new PagingData<BuildingDoorModel, HeaderData>
            {
                Data = doors,
                Header = header,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }


        /// <summary>
        /// Export list of doors in buildings (by building ids)
        /// </summary>
        /// <param name="ids"> list of building identifier </param>
        /// <param name="search">Query string that filter Door in Building by door name, device address, activeTZ or PassageTZ</param>
        /// <param name="operationType"></param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="type"> file type : excel, csv, ... </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiExportBuildingsDoorList)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Building)]
        public IActionResult ExportBuildingsDoors(List<int> ids, string search, short operationType = 0, string sortColumn = null, string sortDirection = "desc", string type = "excel")
        {
            if (ids.Count != 0)
            {
                ids.Sort();
                var doors = _buildingService.GetPaginatedDoors(ids, search, operationType, 0, 0, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered);

                ExportHelper exportHelper = new ExportHelper(_configuration, _httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId());
                var exportResult = exportHelper.ExportDataToFile(doors, Page.Building + Page.Device, type);

                var filename = string.Format(Constants.ExportFileFormat, UserResource.lblExport + "_BuildingDoors", DateTime.Now);
                string fullName = $"{filename}.{exportResult.Extension}";

                if (recordsTotal == 0 || recordsFiltered == 0)
                {
                    return new ApiSuccessResult(StatusCodes.Status200OK,
                        string.Format(MessageResource.MessageExportDataIsZero, UserResource.lblUser.ToLower()));
                }

                // Write systemLog.
                string sysLogMsg = $"{CommonResource.lblFileName} : {fullName}";
                var buildingNames = _buildingService.GetByIds(ids).Select(b => b.Name).ToList();
                string sysLogMsgDetail = $"{BuildingResource.lblBuildingName} :<br />{string.Join("<br />", buildingNames)}";
                _buildingService.WriteSystemLog(ids, ActionLogType.ExportDoor, sysLogMsg, sysLogMsgDetail);

                return File(exportResult.FileByte, exportResult.FileMIME, fullName);
            }

            return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    string.Format(MessageResource.RecordNotFound));
        }

        /// <summary>
        /// Get list of accessible-doors list in a building(by id building)
        /// </summary>
        /// <param name="id">Identified of Building</param>
        /// <param name="search">Query string that filter Door in Building by door name, device address, activeTZ or PassageTZ</param>
        /// <param name="operationType"></param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiBuildingsAccessibleDoorList)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Building)]
        public IActionResult GetAccessibleDoors(int id, string search, short operationType = 0, int pageNumber = 1, int pageSize = 10, string sortColumn = null,
            string sortDirection = "desc")
        {
            var doors = _buildingService.GetPaginatedAccessibleDoors(id, search, operationType, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered).ToList();

            var pagingData = new PagingData<BuildingDoorModel>
            {
                Data = doors,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }


        /// <summary>
        /// Get all unassigned doors in specific building with pagination.
        /// </summary>
        /// <param name="id">Building Id</param>
        /// <param name="search">Query string that filter Door in Building by door name, device address or building name</param>
        /// <param name="operationType"> Flag indicating what mode of operation the device is in </param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiBuildingsUnAssignDoors)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Building)]
        public IActionResult GetUnAssignDoors(int id, string search, short operationType = 0, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id",
            string sortDirection = "desc")
        {
            var doors = _buildingService.GetPaginatedUnAssignDoors(id, search, operationType, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                out var recordsFiltered).ToList();

            var pagingData = new PagingData<BuildingUnAssignDoorModel>
            {
                Data = doors,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// POST /buildings/creation : Create new building
        /// <summary>
        /// Add building
        /// </summary>
        /// <param name="model">JSON model for Building</param>
        /// <returns></returns>
        /// <response code="201">Create new a building success</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="422">Unprocessable Entity: Model Json not valid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpPost]
        [Route(Constants.Route.ApiBuildings)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.Building)]
        public IActionResult Add([FromBody] BuildingModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            int buildingId = _buildingService.Add(model);
            return new ApiSuccessResult(StatusCodes.Status201Created,
               string.Format(MessageResource.MessageAddSuccess, BuildingResource.lblBuilding.ToLower()), buildingId.ToString());
        }

        /// POST /buildings/door-creation/{id} : Create new door
        /// <summary>
        /// Add door(s) to a building
        /// </summary>
        /// <param name="id">Building Id</param>
        /// <param name="doorIds">List of door ids</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: id must be non-zero and list of door ids with mode not empty</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpPost]
        [Route(Constants.Route.ApiBuildingsAssignDoors)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.Building)]
        public IActionResult AddDoors(int id, [FromBody] List<int> doorIds)
        {
            if (id == 0 || doorIds == null || doorIds.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgNoDoorsToAssign);
            }

            var building = _buildingService.GetById(id);
            _buildingService.AddDoors(building, doorIds);

            string message = string.Format(BuildingResource.msgAssignDoors, building.Name);

            return new ApiSuccessResult(StatusCodes.Status201Created, message);
        }


        /// PUT /buildings/{id}/edit-building : Edit a building's name
        /// <summary>
        /// Edit information of a building
        /// </summary>
        /// <param name="id">Building Id</param>
        /// <param name="model">JSON model for Building</param>
        /// <returns></returns>
        /// <response code="400">Bad request: Model or Id Building not empty</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpPut]
        [Route(Constants.Route.ApiBuildingsId)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.Building)]
        public IActionResult Edit(int id, [FromBody] BuildingModel model)
        {
            if (id == 0 || model == null)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgNotFoundBuilding);
            }

            if (id == model.ParentId)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgCannotSetParentToIt);
            }

            model.Id = id;
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }
            _buildingService.Update(id, model);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                    string.Format(MessageResource.MessageUpdateSuccess, BuildingResource.lblBuilding.ToLower(), ""));
        }


        /// DELETE /buildings/{id}/delete : delete a building
        /// <summary>
        /// Delete a building
        /// </summary>
        /// <param name="id">Building Id</param>
        /// <returns></returns>
        /// <response code="400">Bad request: Id must be non-zero and differance default of building id(head quarter = 1)</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpDelete]
        [Route(Constants.Route.ApiBuildingsId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.Building)]
        public IActionResult Delete(int id)
        {
            if (id == 0)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgNotFoundBuilding);
            }

            var buildings = _buildingService.GetByCompanyId(_httpContext.User.GetCompanyId());
            if(buildings.Count == 1 && buildings.First().Id == id)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgCannotDeleteDefaultBuilding);
            }

            //if (id == Constants.DefaultBuildingId)//default building - Head Quarter
            //{
            //    return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgCannotDeleteDefaultBuilding);
            //}

            _buildingService.Delete(id);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                    string.Format(MessageResource.MessageDeleteSuccess, BuildingResource.lblBuilding.ToLower()));


        }

        /// <summary>
        /// Delete list access group
        /// </summary>
        /// <param name="ids">List of Building Ids</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: List of Ids must including Default building id = 1</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="404">Not Found: List of Ids do not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpDelete]
        [Route(Constants.Route.ApiBuildings)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.Building)]
        public IActionResult DeleteMultiple(List<int> ids)
        {
            var buildings = _buildingService.GetByIds(ids);
            if (buildings.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, BuildingResource.msgNotFoundBuilding);
            }

            var devices = buildings.SelectMany(x => x.IcuDevice).ToList();
            if (devices.Count > 0)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgCannotDeleteBuildingIncludeDevice);
            }

            var compBuildings = _buildingService.GetByCompanyId(_httpContext.User.GetCompanyId());
            if (compBuildings.Count == 1 && ids.Contains(compBuildings.First().Id))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgCannotDeleteDefaultBuilding);
            }

            //if (ids.Contains(Constants.DefaultBuildingId))
            //{
            //    return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgCannotDeleteDefaultBuilding);
            //}

            var childBuildingIds = _buildingService.GetChildBuildingByParentIds(ids).Select(m => m.Id).ToList();
            if (childBuildingIds.Count != 0 && !childBuildingIds.All(m => ids.Contains(m)))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgCannotDeleteTopLevelBuilding);
            }

            _buildingService.DeleteRange(buildings);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteMultipleSuccess, BuildingResource.lblBuilding.ToLower()));
        }

        /// <summary>
        /// Unassign door from a building
        /// </summary>
        /// <param name="id">Building Id</param>
        /// <param name="doorIds">List of door Ids(assigned for building)</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: id must be non-zero and list of door ids with mode not empty</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Token Bearer</response>
        [HttpDelete]
        [Route(Constants.Route.ApiBuildingsUnAssignDoors)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.Building)]
        public IActionResult DeleteDoors(int id, [FromBody] List<int> doorIds)
        {
            if (id == 0 || doorIds == null || doorIds.Count == 0)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgNoDoorsToAssign);
            }

            if (_buildingService.IsDefaultBuilding(id))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgCannotDeleteDoorsOfDefaultBuilding);
            }

            _buildingService.UnAssignDoors(id, doorIds);

            //return new ApiSuccessResult(StatusCodes.Status200OK, BuildingResource.msgUnAssignDoors);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, DeviceResource.lblDevice.ToLower()));
        }

        /// <summary>
        /// Get list building tree
        /// </summary>
        /// <param name="search">Query string that filter Building by Name</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiBuildingsChild)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[CheckPermission(ActionName.View + Page.Building)]
        [CheckMultiPermission(new string[] { (ActionName.View + Page.Building), (ActionName.Add + Page.VisitManagement) }, false)]
        public IActionResult GetBuildingHierarchy(string search, int pageNumber = 1, int pageSize = 0, string sortColumn = "name",
            string sortDirection = "asc", short operationType = 0)
        {
            var data = _buildingService.GetListBuildingTree(search, operationType, out var recordsTotal, out var recordsFiltered, pageNumber, pageSize, sortColumn, sortDirection);
            var pagingData = new PagingData<BuildingListItemModel>
            {
                Data = data,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }

        /// <summary>
        /// Get building with level
        /// </summary>
        /// <param name="level">
        /// Level building: 0 - Building, 1 - Zone, 2 - Place
        /// </param>
        /// <param name="search">Query string that filter Building by Name</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiBuildingsLevel)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Building)]
        public IActionResult GetListBuildingWithLevel(int level, string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "name", string sortDirection = "asc")
        {
            var data = _buildingService.GetListBuildingWithLevel(level, search, out var recordsTotal, out var recordsFiltered, pageNumber, pageSize, sortColumn, sortDirection);
            var pagingData = new PagingData<BuildingListModel>
            {
                Data = data,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }
    }
}