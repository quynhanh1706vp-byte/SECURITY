using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.WorkShift;
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
    public class WorkShiftController : Controller
    {
        private readonly IWorkShiftService _workShiftService;
        private readonly IDeviceService _deviceService;
        private readonly IUserService _userService;
        private readonly IAccountService _accountService;
        private readonly HttpContext _httpContext;
        private readonly IDepartmentService _departmentService;
        private readonly IAccessGroupService _accessGroupService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="workShiftService"></param>
        /// <param name="deviceService"></param>
        /// <param name="userService"></param>
        /// <param name="accountService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="departmentService"></param>
        /// <param name="accessGroupService"></param>
        public WorkShiftController(IWorkShiftService workShiftService, IDeviceService deviceService, IUserService userService, 
            IAccountService accountService, IHttpContextAccessor httpContextAccessor, IDepartmentService departmentService,
            IAccessGroupService accessGroupService)
        {
            _workShiftService = workShiftService;
            _deviceService = deviceService;
            _userService = userService;
            _accountService = accountService;
            _httpContext = httpContextAccessor.HttpContext;
            _departmentService = departmentService;
            _accessGroupService = accessGroupService;
        }


        /// <summary>
        /// Get init workShift page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiWorkShiftsInit)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessSchedule)]
        public IActionResult GetInit()
        {
            var data = _workShiftService.GetInit();
            
           
            
            return Ok(data);
        }

        /// <summary>
        /// Get list of workShift 
        /// </summary>
        /// <param name="name">Name of workShift</param>
        /// <param name="userIds">List of user ids</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiWorkShifts)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessSchedule)]
        public IActionResult Gets(string name, List<int> userIds, int pageNumber = 1, int pageSize = 10, string sortColumn = "Name", string sortDirection = "asc")
        {
           
            var models = _workShiftService.GetPaginated(name, userIds, pageNumber, pageSize, sortColumn, sortDirection,
                out var recordsTotal, out var recordsFiltered);
            
            var pagingData = new PagingData<WorkShiftListModel>
            {
                Data = models,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }
        
        /// <summary>
        /// Get workShift by id
        /// </summary>
        /// <param name="id">WorkShift Id</param>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiWorkShiftsId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessSchedule)]
        public IActionResult Get(int id)
        {
            var model = _workShiftService.GetDetailByWorkShiftId(id);
            if (model == null) return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundWorkShift);
            
            return Ok(model);
        }
        
        /// <summary>
        /// Add new workShift
        /// </summary>
        /// <param name="model">WorkShift model with StartTime and EndTime in HH:mm format (e.g. "09:00" for 9:00)</param>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiWorkShifts)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.AccessSchedule)]
        public IActionResult Add([FromBody] WorkShiftModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }
           
            int workShiftId = _workShiftService.Add(model);
            if (workShiftId == 0) return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.MessageAddNewFailed, WorkShiftResource.lblWorkShift));
            
            return new ApiSuccessResult(StatusCodes.Status201Created, string.Format(MessageResource.MessageAddSuccess, WorkShiftResource.lblWorkShift));
        }
        
        /// <summary>
        /// Edit workShift
        /// </summary>
        /// <param name="id">WorkShift Id</param>
        /// <param name="model">WorkShift model with StartTime and EndTime in HH:mm format (e.g. "09:00" for 9:00)</param>
        /// <returns></returns>
        [HttpPut]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiWorkShiftsId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessSchedule)]
        public IActionResult Edit(int id, [FromBody] WorkShiftModel model)
        {
            model.Id = id;
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }
            var workShift = _workShiftService.GetById(id);
            if (workShift == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundWorkShift);
            }

            _workShiftService.Edit(workShift, model);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, WorkShiftResource.lblWorkShift, model.Name));
        }
        
        /// <summary>
        /// Delete workShift
        /// </summary>
        /// <param name="id">WorkShift Id</param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiWorkShiftsId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.AccessSchedule)]
        public IActionResult Delete(int id)
        {
            var workShift = _workShiftService.GetById(id);
            if (workShift == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundWorkShift);

            // Check if WorkShift is being used in AccessSchedule
            if (_workShiftService.IsWorkShiftUsedInAccessSchedule(id))
                return new ApiErrorResult(StatusCodes.Status409Conflict,
                    string.Format(MessageResource.msgCannotDeleteBecauseRef, WorkShiftResource.lblWorkShift, AccessScheduleResource.lblAccessSchedule));

            _workShiftService.Delete(workShift);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, WorkShiftResource.lblWorkShift));
        }
        
        /// <summary>
        /// Delete list workShifts
        /// </summary>
        /// <param name="ids">List of ids workShift</param>
        /// <returns></returns>
        [HttpDelete]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiWorkShifts)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.AccessSchedule)]
        public IActionResult DeleteRange(List<int> ids)
        {
            var workShifts = _workShiftService.GetByIds(ids);
            if (!workShifts.Any())
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundWorkShift);

            // Check if any WorkShift is being used in AccessSchedule
            if (_workShiftService.AreWorkShiftsUsedInAccessSchedule(ids))
                return new ApiErrorResult(StatusCodes.Status409Conflict,
                    string.Format(MessageResource.msgCannotDeleteBecauseRef, WorkShiftResource.lblWorkShift, AccessScheduleResource.lblAccessSchedule));

            _workShiftService.DeleteRange(workShifts);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, WorkShiftResource.lblWorkShift,"(s)"));
        }
    }
}