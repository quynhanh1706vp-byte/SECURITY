using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Holiday;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Holiday controller
    /// </summary>
    [Produces("application/json")]
    [CheckMultipleAddOnAttribute(new string [] { Constants.PlugIn.TimeAttendance, Constants.PlugIn.AccessControl })]
    public class HolidayController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHolidayService _holidayService;
        private readonly HttpContext _httpContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// Holiday controller
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="holidayService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="mapper"></param>
        public HolidayController(IConfiguration configuration, IHolidayService holidayService,
            IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _configuration = configuration;
            _holidayService = holidayService;
            _httpContext = httpContextAccessor.HttpContext;
            _mapper = mapper;
        }

        /// GET /holidays : return all existing holidays with paging and sorting
        /// <summary>
        /// Return Json object for all holiday list
        /// </summary>
        /// <param name="search">Query string that filter Holiday by Name or Type</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiHolidays)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Holiday)]
        public IActionResult Gets(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "Name",
            string sortDirection = "desc")
        {
            var holidays = _holidayService.GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered).AsEnumerable().Select(_mapper.Map<HolidayListModel>).ToList();

            var pagingData = new PagingData<HolidayListModel>
            {
                Data = holidays,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// POST /holidays : Create new holiday
        /// <summary>
        /// Add new a holiday
        /// </summary>
        /// <param name="models">JSON model for Holiday</param>
        /// <returns></returns>
        /// <response code="201">Create new a holiday</response>
        /// <response code="400">Bad Request: count of holidays must littler than max of holidays</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON not valid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiHolidays)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.Holiday)]
        public IActionResult Add([FromBody]List<HolidayModel> models)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            // If this request passed validation, It means that there are not overlaped holiday about existing holidays.
            // We should check whether there are overlaped holidays in this request.
            foreach (var holiday in models)
            {
                foreach (var model in models.Where(m => !m.Equals(holiday)))
                {
                    if (DateTime.Parse(holiday.StartDate).Date.Subtract(DateTime.Parse(model.EndDate).Date).Days <= 0
                    && DateTime.Parse(model.StartDate).Date.Subtract(DateTime.Parse(holiday.EndDate).Date).Days <= 0)
                    {
                        return new ApiErrorResult(StatusCodes.Status400BadRequest, HolidayResource.msgOverlapDurationTime);
                    }
                }
            }

            //32 is MAX Holiday
            //I'll change it with define constant to constants.cs
            var maxHoliday = Constants.Settings.DefaultMaxHoliday;
            if (!string.IsNullOrWhiteSpace(_configuration[Constants.Settings.MaxHoliday]))
            {
                maxHoliday = Convert.ToInt32(_configuration[Constants.Settings.MaxHoliday]);
            }

            //var holidayCount =
            //    Convert.ToDateTime(model.EndDate).Subtract(Convert.ToDateTime(model.StartDate)).TotalDays +
            //    _holidayService.GetHolidayCount(_httpContext.User.GetCompanyId());

            var count = 0;
            foreach (var holiday in models)
            {
                var listDate = DateTimeHelper.GetListRangeDate(DateTime.Parse(holiday.StartDate), DateTime.Parse(holiday.EndDate));
                count += listDate.Count;
            }
            var holidayCount = count + _holidayService.GetHolidayCount(_httpContext.User.GetCompanyId());

            if (holidayCount >= maxHoliday)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    string.Format(HolidayResource.msgLimitHoliday, maxHoliday));
            }

            var isSuccess = _holidayService.Add(models);

            if (!isSuccess)
            {
                return new ApiSuccessResult(StatusCodes.Status400BadRequest,
                    string.Format(MessageResource.MessageAddNewFailed, HolidayResource.lblHoliday));
            }
            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, HolidayResource.lblHoliday));
        }

        /// GET /holidays/{ID} : Get details of one holiday
        /// <summary>
        /// Get holiday detail information by id
        /// </summary>
        /// <param name="id">Holiday Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Holiday Id not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiHolidaysId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Holiday)]
        public IActionResult Get(int id)
        {
            var model = new HolidayModel();
            if (id != 0)
            {
                var holiday = _holidayService.GetHolidayByIdAndCompany(_httpContext.User.GetCompanyId(), id);

                if (holiday == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundHoliday);
                }
                model = _mapper.Map<HolidayModel>(holiday);
            }
            _holidayService.InitData(model);
            return Ok(model);
        }

        /// PUT /holidays/{ID} : Update a holiday
        /// <summary>
        /// Update holiday
        /// </summary>
        /// <param name="id">Holiday Id</param>
        /// <param name="model">JSON model for holiday</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Holiday Id not exist in DB</response>
        /// <response code="422">Unprocessable Entity: Data of Model JSON not valid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiHolidaysId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.Holiday)]
        public IActionResult Edit(int id, [FromBody]HolidayModel model)
        {
            model.Id = id;
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }
            
            var holiday = _holidayService.GetHolidayByIdAndCompany(_httpContext.User.GetCompanyId(), id);
            if (holiday == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundHoliday);
            }

            _holidayService.Update(model, holiday);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, HolidayResource.lblHoliday, holiday.Name));
        }

        /// DELETE /holidays : Delete multiple holidays by list of ids
        /// <summary>
        /// Delete holidays by multi id
        /// </summary>
        /// <param name="ids">List of holiday ids</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: List of Holiday Ids not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiHolidays)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.Holiday)]
        public IActionResult DeleteMultiple(List<int> ids)
        {
            var holidays = _holidayService.GetByIds(ids)
                .ToList();
            if (!holidays.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundHoliday);
            }

            _holidayService.DeleteRange(holidays);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteMultipleSuccess, HolidayResource.lblHoliday));
        }

        /// DELETE /holidays/{ID} : Delete 1 holiday by ID
        /// <summary>
        /// Delete holiday by Id
        /// </summary>
        /// <param name="id">Holiday Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Holiday Id not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiHolidaysId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.Holiday)]
        public IActionResult Delete(int id)
        {
            var holiday = _holidayService.GetHolidayByIdAndCompany(_httpContext.User.GetCompanyId(), id);
            if (holiday == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundHoliday);
            }

            _holidayService.Delete(holiday);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, HolidayResource.lblHoliday));
        }
    }
}