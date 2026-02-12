
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Timezone;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using Microsoft.Extensions.Configuration;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// AccessTime Controller
    /// </summary>
    [Produces("application/json")]
    [CheckAddOn(Constants.PlugIn.AccessControl)]
    public class AccessTimeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IAccessTimeService _accessTimeService;
        private readonly IDeviceService _deviceService;
        private readonly HttpContext _httpContext;


        /// <summary>
        /// Timezone controller
        /// </summary>
        /// <param name="accessTimeService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="deviceService"></param>
        /// <param name="configuration"></param>
        public AccessTimeController(IConfiguration configuration, IAccessTimeService accessTimeService, IHttpContextAccessor httpContextAccessor, IDeviceService deviceService)
        {
            _configuration = configuration;
            _accessTimeService = accessTimeService;
            _httpContext = httpContextAccessor.HttpContext;
            _deviceService = deviceService;
        }

        /// <summary>
        /// List all Timezones with pagination
        /// </summary>
        /// <param name="search">Query string that filter Access Group by AccessTime or Remark.</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessTimes)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //[CheckPermission(ActionName.View + Page.Timezone)]
        [CheckMultiPermission(new string[] { (ActionName.View + Page.Timezone), (ActionName.Add + Page.VisitManagement) }, false)]
        public IActionResult Gets(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = null,
            string sortDirection = "desc")
        {
            var timezones = _accessTimeService.GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection,
                out var recordsTotal,
                out var recordsFiltered);

            var pagingData = new PagingData<AccessTimeListModel>
            {
                Data = timezones,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get detail timezone by id
        /// </summary>
        /// <param name="id">Timezone id</param>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="404">Not found: Timezone Id does not exist</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessTimesId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Timezone)]
        public IActionResult Get(int id)
        {
            var model = new AccessTimeModel();
            if (id != 0)
            {
                var timezone = _accessTimeService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
                if (timezone == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, AccessTimeResource.msgNotFound);
                }
                model = _accessTimeService.InitData(timezone);
            }
            return Ok(model);
        }

        /// <summary>
        /// Add a new timezone
        /// </summary>
        /// <param name="model">JSON model for Timezone (from-to are installed by minute). Ex: 0h00 - 23h59m --> from 0 - to 1439</param>
        /// <response code="400">Bad Request: Length of timezone with company must lesser than max number of timezone setting</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPost]
        [Route(Constants.Route.ApiAccessTimes)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.Timezone)]
        public IActionResult Add([FromBody] AccessTimeModel model)
        {
            var maxTimezone = Constants.Settings.DefaultMaxTimezone;
            if (!string.IsNullOrWhiteSpace(_configuration[Constants.Settings.MaxTimezone]))
            {
                maxTimezone = Convert.ToInt32(_configuration[Constants.Settings.MaxTimezone]);
            }
            var timezoneCount = _accessTimeService.GetTimezoneCount(_httpContext.User.GetCompanyId());
            if (timezoneCount >= maxTimezone)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    string.Format(AccessTimeResource.msgLimitTimezone, maxTimezone));
            }

            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            _accessTimeService.Add(model);
            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, AccessTimeResource.lblAccessTime.ToLower()));
        }


        /// <summary>
        /// Edit timezone
        /// </summary>
        /// <param name="id">timezone id to edit</param>
        /// <param name="model">JSON model for Timezone (from-to are installed by minute). Ex: 0h00 - 23h59m --> from 0 - to 1439</param>
        /// <response code="400">Bad Request: position of timezone equals default passage or default active</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="404">Not found: Timezone Id does not exist</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPut]
        [Route(Constants.Route.ApiAccessTimesId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.Timezone)]
        public IActionResult Edit(int id, [FromBody] AccessTimeModel model)
        {
            model.Id = id;
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }
            var timezone = _accessTimeService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
            if (timezone == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, AccessTimeResource.msgNotFound);
            }

            //Check whether it's default timezone
            if (timezone.Position == Constants.Settings.DefaultPositionPassageTimezone ||
                timezone.Position == Constants.Settings.DefaultPositionActiveTimezone)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessTimeResource.msgCannotUpdateDefaultTz);
            }

            _accessTimeService.Update(model, timezone);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, AccessTimeResource.lblAccessTime.ToLower(), ""));
        }

        /// <summary>
        /// Delete timezone
        /// </summary>
        /// <param name="id">timezone id to delete</param>
        /// <response code="400">Bad Request: position of timezone equals default passage or default active</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="404">Not found: Timezone Id does not exist</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpDelete]
        [Route(Constants.Route.ApiAccessTimesId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.Timezone)]
        public IActionResult Delete(int id)
        {
            var timezone = _accessTimeService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
            if (timezone == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, AccessTimeResource.msgNotFound);
            }

            if (_deviceService.HasTimezone(timezone.Id, _httpContext.User.GetCompanyId()))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessTimeResource.msgCannotDeleteTimezone);
            }

            //Check whether it's default timezone
            if (timezone.Position == Constants.Settings.DefaultPositionPassageTimezone ||
                timezone.Position == Constants.Settings.DefaultPositionActiveTimezone)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessTimeResource.msgCannotDeleteDefaultTz);
            }

            _accessTimeService.Delete(timezone);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, AccessTimeResource.lblAccessTime.ToLower()));
        }

        /// <summary>
        /// Timezone multiple delete
        /// </summary>
        /// <param name="ids">list of timezone ids to delete</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Exist a or more timezone cannot delete</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="404">Not found: List of timezone does not exist</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpDelete]
        [Route(Constants.Route.ApiAccessTimes)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.Timezone)]
        public IActionResult DeleteMultiple(List<int> ids)
        {
            var timezones = _accessTimeService.GetByIdsAndCompany(ids, _httpContext.User.GetCompanyId());
            if (!timezones.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, AccessTimeResource.msgNotFound);
            }

            var companyId = _httpContext.User.GetCompanyId();
            foreach (var timezone in timezones)
            {
                //Check whether it's default timezone
                if (timezone.Position == Constants.Settings.DefaultPositionPassageTimezone ||
                    timezone.Position == Constants.Settings.DefaultPositionActiveTimezone)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessTimeResource.msgCannotDeleteDefaultTz);
                }

                if (_deviceService.HasTimezone(timezone.Id, companyId))
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, AccessTimeResource.msgCannotDeleteTimezone);
                }
            }

            _accessTimeService.DeleteRange(timezones);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteMultipleSuccess, AccessTimeResource.lblAccessTime.ToLower()));
        }


        /// <summary>
        /// Add a new access time for hvqy
        /// </summary>
        /// <param name="model">JSON model for Timezone (from-to are installed by minute). Ex: 0h00 - 23h59m --> from 0 - to 1439</param>
        /// <response code="400">Bad Request: Length of timezone with company must lesser than max number of timezone setting</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPost]
        [Route(Constants.Route.ApiAccessTimesHvqy)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult AddAccessTimeHvqy([FromBody] AccessTimeModel model)
        {
            var maxTimezone = Constants.Settings.DefaultMaxTimezone;
            if (!string.IsNullOrWhiteSpace(_configuration[Constants.Settings.MaxTimezone]))
            {
                maxTimezone = Convert.ToInt32(_configuration[Constants.Settings.MaxTimezone]);
            }
            var timezoneCount = _accessTimeService.GetTimezoneCount(_httpContext.User.GetCompanyId());
            if (timezoneCount >= maxTimezone)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    string.Format(AccessTimeResource.msgLimitTimezone, maxTimezone));
            }

            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            var result = _accessTimeService.Add(model);
            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, AccessTimeResource.lblAccessTime.ToLower()), result.ToString());
        }


        /// <summary>
        /// Assign a access time for user
        /// </summary>
        /// <param name="model">JSON model for Timezone (from-to are installed by minute). Ex: 0h00 - 23h59m --> from 0 - to 1439</param>
        /// <response code="400">Bad Request: Length of timezone with company must lesser than max number of timezone setting</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPost]
        [Route(Constants.Route.ApiAssignAccessTimesHvqy)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult AssignAccessTimeHvqy([FromBody] AssignAccessTime model)
        {
            
            var result = _accessTimeService.AssignAccessTimeForUser(model);
            if (result == false)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, AccessTimeResource.msgNotFound);
            }
            else
            {
                
                return new ApiSuccessResult(StatusCodes.Status200OK,MessageResource.MessageUpdateSuccess);
            }
        }


        /// <summary>
        /// Assign a access time for user
        /// </summary>
        /// <param name="model">JSON model for Timezone (from-to are installed by minute). Ex: 0h00 - 23h59m --> from 0 - to 1439</param>
        /// <response code="400">Bad Request: Length of timezone with company must lesser than max number of timezone setting</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Lack of String Bearer Token</response>
        [HttpPost]
        [Route(Constants.Route.ApiUnAssignAccessTimesHvqy)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UnAssignAccessTimeHvqy([FromBody] AssignAccessTime model)
        {
            
            var result = _accessTimeService.UnAssignAccessTimeFromUser(model);
            if (result == false)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, AccessTimeResource.msgNotFound);
            }
            else
            {
                
                return new ApiSuccessResult(StatusCodes.Status200OK,MessageResource.MessageUpdateSuccess);
            }
        }
    }

    
}
