using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.WorkingModel;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Working type controller
    /// </summary>
    [Produces("application/ms-excel", "application/json", "application/text")]
    [CheckAddOn(Constants.PlugIn.TimeAttendance)]
    public class WorkingTypeController : Controller 
    {
        private readonly IWorkingService _workingService;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly IAttendanceService _attendanceService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;


        /// <summary>
        /// Working type controller
        /// </summary>
        /// <param name="workingService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        /// <param name="attendanceService"></param>
        /// <param name="userService"></param>
        /// <param name="mapper"></param>
        public WorkingTypeController(IWorkingService workingService, IHttpContextAccessor httpContextAccessor, IConfiguration configuration, 
            IAttendanceService attendanceService, IUserService userService, IMapper mapper)
        {
            _workingService = workingService;
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration;
            _attendanceService = attendanceService;
            _userService = userService;
            _mapper = mapper;
        }
        
        /// <summary>
        /// Get WorkingType(s)
        /// </summary>
        /// <param name="search">Not use</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiGetWorkingTypeCompany)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.WorkingTime)]
        public IActionResult Get(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id",
            string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<WorkingListModel>(sortColumn, "Name", ColumnDefines.Company);
            var workingTypes = _workingService
                .GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered).AsEnumerable().ToList();

            var pagingData = new PagingData<WorkingListModel>
            {
                Data = workingTypes,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }
        
        /// <summary>
        /// Get Working Type by id
        /// </summary>
        /// <param name="id">Working type id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiGetWorkingTypeCompanyDetail)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.WorkingTime)]
        public IActionResult GetDetail(int id)
        {
            //var workingType = _workingService.GetWorkingType(id, _httpContext.User.GetCompanyId());
            var workingType = _workingService.GetWorkingTypeDetail(id, _httpContext.User.GetCompanyId());

            if(workingType == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundWorkingType);
            }

            return Ok(workingType);
        }
        
        /// <summary>
        /// Add new a working type
        /// </summary>
        /// <param name="model">JSON model for WorkingType</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiAddWorkingTypeCompany)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.WorkingTime)]
        public IActionResult Add([FromBody] WorkingModel model)
        {
            var listWorking = JsonConvert.DeserializeObject<List<WorkingTime>>(model.WorkingDay);
            var listDay = new List<string>();
            
            if(listWorking != null)
            {
                foreach (var s in listWorking)
                {
                    // Try validate Working Type
                    bool validType = Helpers.TryValidateWorkingType(s.Type);
                    if (validType != true)
                    {
                        return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AttendanceResource.msgValidationWorkingType);
                    }

                    // Try validate Working Time
                    bool validTime = Helpers.TryValidateWorkingTime(s.Start, s.End, s.StartTimeWorking);
                    if (validTime != true)
                    {
                        return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AttendanceResource.msgValidationWorkingTime);
                    }

                    // Try validate Working Day
                    bool validDay = Helpers.TryValidateWorkingDay(s.Name);
                    if (validDay != true)
                    {
                        return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AttendanceResource.msgValidationWorkingDay);
                    }

                    listDay.Add(s.Name);
                }

                // Try validate Working Day Distinct
                bool validDistinctDay = Helpers.TryValidateWorkingDayDistinct(listDay);
                if (validDistinctDay != true)
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AttendanceResource.msgValidationWorkingDay);
                }
            }
            else
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.BadRequest);
            }

            // Try validate lunchTime
            if (model.CheckLunchTime && !string.IsNullOrWhiteSpace(model.LunchTime))
            {
                var lunchTime = JsonConvert.DeserializeObject<LunchTime>(model.LunchTime);

                bool validLunchTime = Helpers.TryValidateWorkingTime(lunchTime.Start, lunchTime.End);
                if (validLunchTime != true)
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AttendanceResource.msgValidationLunchTime);
                }
            }

            // Try validate Working Day Existed
            var existed = _workingService.CheckWorkingTypeExisted(model.Name);
            if(existed != null)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AttendanceResource.msgWorkingTimeNameExisted);
            }
            
            var working = _workingService.Add(model);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageAddSuccess, UserResource.lblWorkingType, ""), working.ToString());
        }

        /// <summary>
        ///     updates working type. 
        /// </summary>
        /// <remarks>   Edward, 2020-03-11. </remarks>
        /// <param name="id">       The identifier of working type. </param>
        /// <param name="model">    workingTime model. </param>
        /// <returns>   An IActionResult. </returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: working type does not exist in DB</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiUpdateWorkingTypeCompany)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.WorkingTime)]
        public IActionResult Edit(int id, [FromBody] WorkingModel model)
        {
            var listWorking = JsonConvert.DeserializeObject<List<WorkingTime>>(model.WorkingDay);
            var listDay = new List<string>();
            var workingType = _workingService.GetWorkingType(id, _httpContext.User.GetCompanyId());
            
            // Check working Type already existed
            if (workingType == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundWorkingType);
            }

            foreach (var s in listWorking)
            {
                // Try validate Working Type
                bool validType = Helpers.TryValidateWorkingType(s.Type);
                if (validType != true)
                {
                    return new ValidationFailedResult(ModelState);
                }

                // Try validate Working Time
                bool validTime = Helpers.TryValidateWorkingTime(s.Start, s.End, s.StartTimeWorking);
                if (validTime != true)
                {
                    return new ValidationFailedResult(ModelState);
                }

                // Try validate Working Day
                bool validDay = Helpers.TryValidateWorkingDay(s.Name);
                if (validDay != true)
                {
                    return new ValidationFailedResult(ModelState);
                }

                listDay.Add(s.Name);
            }

            // Try validate Working Day Distinct
            bool validDistinctDay = Helpers.TryValidateWorkingDayDistinct(listDay);
            if (validDistinctDay != true)
            {
                return new ValidationFailedResult(ModelState);
            }

            // Try validate Working Day Existed
            //var existed = _workingService.CheckWorkingTypeExisted(model.Name);
            //if(existed.Id != id)
            //{
            //    return new ValidationFailedResult(ModelState);
            //}
            var existed = _workingService.CheckNameWorkingTime(model.Name, id);
            if (!existed)
            {
                return new ValidationFailedResult(ModelState);
            }

            _workingService.Update(id, model);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, UserResource.lblWorkingType, ""));

        }
        
        /// <summary>
        /// Deleted Working Type
        /// </summary>
        /// <param name="id">Working type id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: working type does not exist in DB</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiGetWorkingTypeCompanyDetail)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.WorkingTime)]
        public IActionResult DeletedWorkingType(int id)
        {
            var workingType = _workingService.GetWorkingType(id, _httpContext.User.GetCompanyId());
            
            if (workingType != null)
            {
                if (workingType.IsDefault)
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity , UserResource.DeleteDefaultWorkingType);
                }
                var users = _workingService.GetUserUsingWorkingType(id, _httpContext.User.GetCompanyId());
                if (users.Any())
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity , UserResource.DeleteWorkingTypeUserWorkingOn);
                }
                _workingService.Delete(workingType);
                
            }
            else
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundWorkingType);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, UserResource.lblWorkingType));
            
        }
        
        /// <summary>
        /// Assign Multiple User to Default working time
        /// </summary>
        /// <param name="id">Working type id</param>
        /// <param name="listUser">List of User</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: List userId of string listUser does not exist in DB</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPatch]
        [Route(Constants.Route.AssignMultipleUsersToWorkingTime)]
        //[Authorize(Policy = Constants.Policy.PrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.User)]
        public IActionResult AssignMultipleUserToDefaultWorkingTime(int id, string listUser)
        {
            if (!string.IsNullOrWhiteSpace(listUser))
            {
                String[] strListId = listUser.Split(",");
                if (strListId.Any())
                {
                    foreach (var userId in strListId)
                    {
                        var user = _userService.GetByIdAndCompany(Int32.Parse(userId), _httpContext.User.GetCompanyId());
                        if (user == null)
                        {
                            return new ApiErrorResult(StatusCodes.Status404NotFound,
                                string.Format(MessageResource.UserIsNotValidation, userId));
                        }
                    }
                }
                else
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity);
                }
                
                var workingType = _workingService.GetWorkingType(id, _httpContext.User.GetCompanyId());
                if (workingType != null)
                {
                    _workingService.AssignMultipleUserToWorkingTime(id, listUser);
                    return new ApiSuccessResult(StatusCodes.Status200OK,
                        string.Format(MessageResource.MessageUpdateSuccess, strListId.Count(), UserResource.lblUser));
                }
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundWorkingType);
            }
            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity);
        }
    }
}