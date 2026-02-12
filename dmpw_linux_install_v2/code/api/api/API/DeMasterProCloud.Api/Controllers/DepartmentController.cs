using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using Microsoft.AspNetCore.Mvc;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.Service;
using DeMasterProCloud.DataModel.Department;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.DepartmentDevice;
using DeMasterProCloud.DataModel.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
using System.Diagnostics;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Department controller
    /// </summary>
    [Produces("application/json")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly HttpContext _httpContext;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        /// <summary>
        /// Ctor of department
        /// </summary>
        /// <param name="departmentService"></param>
        /// <param name="userService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="mapper"></param>
        public DepartmentController(IDepartmentService departmentService, IUserService userService,
            IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _departmentService = departmentService;
            _httpContext = httpContextAccessor.HttpContext;
            _userService = userService;
            _mapper = mapper;
        }

        /// <summary>
        /// get (a) department(s) by search filter
        /// </summary>
        /// <param name="search">Query string that filter Department by Name, Department Number or Name of Parent.</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by index of the column. Example </param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending </param>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDepartments)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Department)]
        public IActionResult Gets(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "DepartmentNumber",
            string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<DepartmentListModel>(sortColumn, "DepartmentNumber",
                new[] {"DepartmentNumber", "DepartmentName", "NumberUser", "DepartmentManager"});
            var departments = _departmentService
                .GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered);

            var pagingData = new PagingData<DepartmentListModel>
            {
                Data = departments,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }


        /// <summary>
        /// Get department as hierarchy form.
        /// </summary>
        /// <param name="search">Query string that filter Department by Name, Department Number or Name of Parent.</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by index of the column. Example </param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDepartmentsChild)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Department)]
        public IActionResult GetDepartmentHierarchy(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "DepartmentName", string sortDirection = "asc")
        {
            var filter = new DepartmentFilterModel()
            {
                Search = search,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = SortColumnMapping.DepartmentColumn(sortColumn),
                SortDirection = sortDirection,
            };
            var departments = _departmentService.GetListDepartment(filter, out var recordsTotal, out var recordsFiltered);
            var pagingData = new PagingData<DepartmentListItemModel>
            {
                Data = departments.ToList(),
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }

        /// <summary>
        /// get a department by Id
        /// </summary>
        /// <param name="id">Department Id</param>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Department does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDepartmentsId)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Department)]
        public IActionResult Get(int id)
        {
            DepartmentDataModel model = new DepartmentDataModel();

            if(id != 0)
            {
                var department = _departmentService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
                if (department == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.NotFoundDepartment);
                }

                model = _mapper.Map<DepartmentDataModel>(department);
            }

            _departmentService.InitDepartment(model);
            return Ok(model);
        }

        ///// <summary>
        ///// get a department by number
        ///// </summary>
        ///// <param name="number">Department Id</param>
        ///// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        ///// <response code="404">Not Found: Department does not exist in DB</response>
        ///// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        //[HttpGet]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //[Route(Constants.Route.ApiDepartmentsNumber)]
        //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[CheckPermission(ActionName.View + Page.Department)]
        //public IActionResult GetByNumber(string number)
        //{
        //    var department = _departmentService.GetByNumberAndCompany(number, _httpContext.User.GetCompanyId());
        //    if (department == null)
        //    {
        //        return new ApiErrorResult(StatusCodes.Status404NotFound);
        //    }

        //    var model = Mapper.Map<DepartmentModel>(department);

        //    return Ok(model);
        //}

        /// <summary>
        /// add a department
        /// </summary>
        /// <param name="model">JSON model for Department</param>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Company account does not exist in DB</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiDepartments)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.Department)]
        public IActionResult Add([FromBody]DepartmentModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }
            // check department create is child department of account login
            if (!_departmentService.CheckPermissionDepartmentLevel(model.ParentId ?? 0))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgNotPermissionAddAnotherDept));
            }
            
            // auto generate department number (by lenght departments)
            if (string.IsNullOrWhiteSpace(model.Number))
            {
                model.Number = _departmentService.GetNewAutoDepartmentNumber(_httpContext.User.GetCompanyId()).ToString();
            }

            if (model.DepartmentManagerId == null)
            {
                model.DepartmentManagerId = 0;
            }

            if (model.DepartmentManagerId != 0)
            {
                var user = _userService.GetById(model.DepartmentManagerId.Value);

                if (user == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, $"{MessageResource.NoUserInformation}");
                }

                if (user.AccountId == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, $"{MessageResource.CannotDesignateDeptManager} ({MessageResource.NoLoginAccount})");
                }

                var account = _departmentService.GetAccount(user.AccountId.Value);
                if (account != null)
                {
                    var companyAccount = _departmentService.GetCompanyAccount(account.Id, _httpContext.User.GetCompanyId());
                    if (companyAccount == null)
                    {
                        return new ApiErrorResult(StatusCodes.Status404NotFound, $"{MessageResource.CannotDesignateDeptManager} ({MessageResource.NoLoginAccount})");
                    }

                    model.DepartmentManagerId = account.Id;
                }
                else
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, $"{MessageResource.CannotDesignateDeptManager} ({MessageResource.NoLoginAccount})");
                }
            }
            else
            {
                model.DepartmentManagerId = null;
            }


            var department = _departmentService.Add(model);
            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, DepartmentResource.lblDepartment.ToLower()), department.ToString());
        }

        /// <summary>
        /// edit a department
        /// </summary>
        /// <param name="id">Department Id</param>
        /// <param name="model">JSON model for Department</param>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Account Company or department not found</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiDepartmentsId)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.Department)]
        public IActionResult Edit(int id, [FromBody]DepartmentModel model)
        {
            model.Id = id;
            if(model.DepartmentManagerId == null)
            {
                model.DepartmentManagerId = 0;
            }
            
            if (id == model.ParentId)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, BuildingResource.msgCannotSetParentToIt);
            }
            
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }

            //var department = _departmentService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
            //if (department == null)
            //{
            //    return new ApiErrorResult(StatusCodes.Status404NotFound);
            //}

            if (model.DepartmentManagerId != 0)
            {
                var user = _userService.GetById(model.DepartmentManagerId.Value);

                if (user == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, $"{MessageResource.NoUserInformation}");
                }

                if(user.AccountId == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, $"{MessageResource.CannotDesignateDeptManager} ({MessageResource.NoLoginAccount})");
                }

                var account = _departmentService.GetAccount(user.AccountId.Value);
                if (account != null)
                {
                    var companyAccount = _departmentService.GetCompanyAccount(account.Id, _httpContext.User.GetCompanyId());
                    if (companyAccount == null)
                    {
                        return new ApiErrorResult(StatusCodes.Status404NotFound, $"{MessageResource.CannotDesignateDeptManager} ({MessageResource.NoLoginAccount})");
                    }

                    model.DepartmentManagerId = account.Id;
                }
                else
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, $"{MessageResource.CannotDesignateDeptManager} ({MessageResource.NoLoginAccount})");
                }
            }
            else
            {
                model.DepartmentManagerId = null;
            }

            // check parentid
            var checkUpdateDepartment = _departmentService.CheckEditDepartment(model);
            if (model.ParentId == 0)
            {
                model.ParentId = null;
            }

            if (checkUpdateDepartment == true)
            {
                _departmentService.Update(model);
            }
            else
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                string.Format(MessageResource.ParentIdInvalid, DepartmentResource.lblDepartment.ToLower(), ""));
            }

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, DepartmentResource.lblDepartment.ToLower(), ""));
        }

        /// <summary>
        /// delete a department by id
        /// </summary>
        /// <param name="id">Department Id</param>
        /// <response code="400">Bad Request: Length of List company with department empty</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Department does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiDepartmentsId)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.Department)]
        public IActionResult Delete(int id)
        {
            var currentCompanyId = _httpContext.User.GetCompanyId();

            var department = _departmentService.GetByIdAndCompany(id, currentCompanyId);
            if (department == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.NotFoundDepartment);
            }

            if (_departmentService.GetByCompanyId(currentCompanyId).Count() <= 1 || _departmentService.IsUserExist(id))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, $"{DepartmentResource.msgNotDeleted} ({MessageResource.ExistUser} {MessageResource.or} {MessageResource.LastDepartment})");
            }

            _departmentService.Delete(department);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, DepartmentResource.lblDepartment.ToLower()));
        }
        /// <summary>
        /// </summary>
        /// <param name="id">Department Id</param>
        /// <param name="search"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <response code="400">Bad Request: Length of List company with department empty</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Department does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiDepartmentsUsersUnAssign)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetUserUnAssign(int id, string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "FullName",
            string sortDirection = "desc")
        {
            var currentCompanyId = _httpContext.User.GetCompanyId();

            var department = _departmentService.GetByIdAndCompany(id, currentCompanyId);
            if (department == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.NotFoundDepartment);
            }

            if (_departmentService.GetByCompanyId(currentCompanyId).Count() <= 1 || _departmentService.IsUserExist(id))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, $"{DepartmentResource.msgNotDeleted} ({MessageResource.ExistUser} {MessageResource.or} {MessageResource.LastDepartment})");
            }

            var lstUsers = _userService.GetUsersByCompany(currentCompanyId);
            
            if (lstUsers != null && lstUsers.Count() > 0)
            {
                lstUsers = lstUsers.Where(x => x.DepartmentId != id).ToList();
            }

            return Ok(lstUsers);
        }
        /// <summary>
        /// </summary>
        /// <param name="id">Department Id</param>
        /// <param name="userIds">List user Id</param>
        /// <response code="400">Bad Request: Length of List company with department empty</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Department does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiDepartmentsIdAssign)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.User)]
        public IActionResult AssignUsers(int id,[FromBody]List<int> userIds)
        {
            var currentCompanyId = _httpContext.User.GetCompanyId();

            var department = _departmentService.GetByIdAndCompany(id, currentCompanyId);
            if (department == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.NotFoundDepartment);
            }
            
            var result = _departmentService.AssignUsers(id, userIds);
            
            if (result != "Ok")
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, result);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, DepartmentResource.msgAssignUsersToDepartment);
        }
        
        
        /// <summary>
        /// </summary>
        /// <param name="id">Department Id</param>
        /// <param name="userIds">List user Id</param>
        /// <response code="400">Bad Request: Length of List company with department empty</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Department does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiDepartmentsIdUnAssign)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.User)]
        public IActionResult UnAssignUsers(int id,[FromBody]List<int> userIds)
        {
            var currentCompanyId = _httpContext.User.GetCompanyId();

            var department = _departmentService.GetByIdAndCompany(id, currentCompanyId);
            if (department == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.NotFoundDepartment);
            }
            
            var departments = _departmentService.GetByCompanyId(currentCompanyId);
            if (departments.Count <= 1)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, $"{DepartmentResource.msgNotUnassignUserToDepartment} ({MessageResource.LastDepartment})");
            }
            
            var defaultDepartment = departments.Where(m => m.Id != id).OrderBy(m => m.Id).First();
            var result = _departmentService.AssignUsers(defaultDepartment.Id, userIds);
            
            if (result != "Ok")
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, result);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, DepartmentResource.msgUnassignSuccess + $" ({defaultDepartment.DepartName})");
        }

        /// <summary>
        /// Delete departments by id list
        /// </summary>
        /// <param name="ids">list to ids on the device to delete</param>
        /// <response code="400">Bad Request: Length of List company with department empty</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Departments does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiDepartments)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.Department)]
        public IActionResult DeleteMultiple(List<int> ids)
        {
            var currentCompanyId = _httpContext.User.GetCompanyId();

            if (!ids.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.NotSelected, DepartmentResource.lblDepartment.ToLower()));
            }

            var departments = _departmentService.GetByIdsAndCompany(ids, currentCompanyId);
            if (departments == null || !departments.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.NotFoundDepartment));
            }

            foreach (var id in ids)
            {
                if (_departmentService.GetByCompanyId(currentCompanyId).Count() <= 1 || _departmentService.IsUserExist(id))
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, $"{DepartmentResource.msgNotDeleted} ({MessageResource.ExistUser} {MessageResource.or} {MessageResource.LastDepartment})");
                }
            }

            _departmentService.DeleteRange(departments);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteMultipleSuccess, DepartmentResource.lblDepartment.ToLower()));
        }

        /// <summary>
        /// Get list of all departments as hierarchy
        /// </summary>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiDepartmentsHierarchy)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Department)]
        public IActionResult GetListDepartment()
        {
            var departments = _departmentService.GetDeparmentHierarchy();
            return Json(departments);
        }
        /// <summary>
        /// Get list parent department
        /// </summary>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiGetListParentDepartment)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Department)]
        public IActionResult GetListParentDepartment()
        {
            var companyId = _httpContext.User.GetCompanyId();
            var departments = _departmentService.GetListParentDepartment(companyId);
            return Json(departments);
        }

        /// <summary>
        /// Import departments data
        /// </summary>
        /// <param name="file">file stored list of departments to create</param>
        /// <param name="type">type file import (excel, csv)</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: File extension not valid or data in file wrong</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: File not found</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiDepartmentsImport)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.Department)]
        public IActionResult ImportDepartment(IFormFile file, string type = "excel")
        {
            if (file.Length == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgFailLengthFile);
            }
            var fileType = FileHelpers.GetFileExtension(file);
            var excelExtension = new[] { ".xls", ".xlsx" };
            var txtExtension = new[] { ".csv" };
            if (type.Equals("excel", StringComparison.OrdinalIgnoreCase) && !excelExtension.Contains(fileType) ||
                type.Equals("csv", StringComparison.OrdinalIgnoreCase) && !txtExtension.Contains(fileType))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    string.Format(MessageResource.msgErrorFileExtension, type));
            }

            var result = _departmentService.ImportFile(type, file, out int total, out int fail);

            if (!result)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    string.Format(MessageResource.msgFailedToImportDetails, fail, total));
            }
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.msgSuccessToImportDetails, total));

            // TODO
            // return true -> all datas are imported normally.
            // return false -> There is failed to import data.
        }


        /// <summary>
        /// Export departments data
        /// </summary>
        /// <param name="type"> file type </param>
        /// <param name="filter">Query string that filter Department by Name, Department Number or Name of Parent.</param>
        /// <param name="sortColumn">Sort Column by index of the column. Example </param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Produces("application/csv", "application/ms-excel")]
        [Route(Constants.Route.ApiDepartmentsExport)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Export + Page.Department)]
        public IActionResult ExportDepartment(string type = "excel", string filter = "", string sortColumn = "DepartmentNumber",
            string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<DepartmentListModel>(sortColumn, "DepartmentNumber",
                new[] {"DepartmentNumber", "DepartmentName", "NumberUser", "DepartmentManager"});
            var fileData = _departmentService.Export(type, filter, sortColumn, sortDirection, out int totalRecords,
                out int recordsFiltered);

            if (totalRecords == 0 || recordsFiltered == 0)
            {
                return new ApiSuccessResult(StatusCodes.Status200OK,
                    string.Format(MessageResource.MessageExportDataIsZero, UserResource.lblUser.ToLower()));
            }

            var filename = string.Format(Constants.ExportFileFormat, "export_department", DateTime.Now);
            var fullName = type == "excel" ? $"{filename}.xlsx" : $"{filename}.csv";
            return File(fileData, type.Equals("excel") ? "application/ms-excel" : "application/csv", fullName);
        }

        
        /// <summary>
        /// Get list device assigned in department
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="search"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiDepartmentDeviceAssigned)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckAddOn(Constants.PlugIn.DepartmentAccessLevel)]
        [CheckPermission(ActionName.View + Page.Department)]
        public IActionResult DepartmentDeviceAssigned(int departmentId, string search, int pageNumber, int pageSize, string sortColumn, string sortDirection)
        {
            sortColumn = Helpers.CheckPropertyInObject<DepartmentDeviceModel>(sortColumn, "Id", ColumnDefines.DepartmentDeviceForUnAssignDoors);
            var fileData = _departmentService.GetDepartmentDevices(departmentId, search, pageNumber, pageSize, sortColumn, sortDirection, out int totalRecords,
                out int recordsFiltered);

            var pagingData = new PagingData<DepartmentDeviceModel>
            {
                Data = fileData,
                Meta = { RecordsTotal = totalRecords, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }
        
        /// <summary>
        /// Get list device unAssigned in department
        /// </summary>
        /// <param name="departmentId"></param>
        /// <param name="search"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiDepartmentDeviceUnAssigned)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckAddOn(Constants.PlugIn.DepartmentAccessLevel)]
        [CheckPermission(ActionName.View + Page.Department)]
        public IActionResult DepartmentDeviceUnAssigned(int departmentId, string search, int pageNumber, int pageSize, string sortColumn, string sortDirection)
        {
            sortColumn = Helpers.CheckPropertyInObject<DepartmentDeviceModel>(sortColumn, "Id", ColumnDefines.DepartmentDeviceForUnAssignDoors);
            var fileData = _departmentService.GetDepartmentDevicesUnassign(departmentId, search, pageNumber, pageSize, sortColumn, sortDirection, out int totalRecords,
                out int recordsFiltered);

            var pagingData = new PagingData<DepartmentDeviceModel>
            {
                Data = fileData,
                Meta = { RecordsTotal = totalRecords, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }
        
        /// <summary>
        /// Assign door to department
        /// </summary>
        /// <param name="id"></param>
        /// <param name="doorIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiAssignedDeptDevice)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckAddOn(Constants.PlugIn.DepartmentAccessLevel)]
        [CheckPermission(ActionName.Edit + Page.Department)]
        public IActionResult DepartmentAssignDoors(int id, [FromBody]List<int> doorIds)
        {
            if (id == 0 || doorIds == null || !doorIds.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.msgEmptyDoorAssign);
            }

            var result = _departmentService.AssignDepartmentDevice(id, doorIds);
            if (!result)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, DepartmentResource.msgAssignDoors);
        }

        /// <summary>
        /// Unassign door of department
        /// </summary>
        /// <param name="id"></param>
        /// <param name="doorIds"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiUnAssignedDeptDevice)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckAddOn(Constants.PlugIn.DepartmentAccessLevel)]
        [CheckPermission(ActionName.Edit + Page.Department)]
        public IActionResult DepartmentUnAssignDoors(int id, [FromBody] List<int> doorIds)
        {
            if (id == 0 || doorIds == null || !doorIds.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessGroupResource.msgEmptyDoorAssign);
            }

            var result = _departmentService.UnAssignDepartmentDevice(id, doorIds);
            if (!result)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, DepartmentResource.msgUnAssignDoors);
        }


        /// <summary>
        /// Generate test data
        /// </summary>
        /// <param name="numberOfDept">Number of departments to be created</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route("/departments/create-test-data")]
        [Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CreateTestData(int numberOfDept)
        {
            if (numberOfDept == 0)
            {
                return Ok(new { message = "No department data is created!" });
            }

            var stopWatch = Stopwatch.StartNew();
            _departmentService.GenerateTestData(numberOfDept);
            stopWatch.Stop();

            Trace.WriteLine($"Elapsed time {stopWatch.ElapsedMilliseconds} ms");
            return Ok(new
            {
                message =
                    $"{numberOfDept} department(s) data were created successfully in {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds).TotalSeconds} seconds!"
            });
        }
    }
}