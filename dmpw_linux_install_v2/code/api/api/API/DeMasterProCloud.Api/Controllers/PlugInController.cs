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
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.PlugIn;
using Newtonsoft.Json;


namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Controller of plugin function
    /// </summary>
    [Produces("application/ms-excel", "application/json", "application/text")]
    [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PluginController : Controller
    {
        private readonly IPluginService _pluginService;
        private readonly HttpContext _httpContext;


        /// <summary>
        /// plug-in controller
        /// </summary>
        /// <param name="pluginService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        /// <param name="companyService"></param>
        /// <param name="mapper"></param>
        public PluginController(IPluginService pluginService, IHttpContextAccessor httpContextAccessor)
        {
            _pluginService = pluginService;
            _httpContext = httpContextAccessor.HttpContext;
        }
        
        /// <summary>
        /// Get Solution(s)
        /// </summary>
        /// <param name="search">Query string that filter plugin</param>
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
        [Route(Constants.Route.ApiGetPlugInCompany)]
        public IActionResult Get(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id",
            string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<PlugInModel>(sortColumn, "Name", ColumnDefines.Company);
            var solutions = _pluginService
                .GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                    out var recordsFiltered).AsEnumerable().ToList();

            var pagingData = new PagingData<PlugInModel>
            {
                Data = solutions,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }
        
        /// <summary>
        /// Update Plugin
        /// </summary>
        /// <param name="id">Plugin id</param>
        /// <param name="model">JSON model for Plugin</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Plugin Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiUpdatePlugInCompany)]
        public IActionResult Update(int id, [FromBody] PlugIns model)
        {
            var plugIn = _pluginService.GetPluginCompany(_httpContext.User.GetCompanyId(), id);
            if (plugIn != null)
            {
                var plugInId = _pluginService.Update(id, model);

                return new ApiSuccessResult(StatusCodes.Status200OK,
                    string.Format(MessageResource.MessageEnabledSuccess, UserResource.lblPlugIn, plugInId));
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundPlugin);
        }

    }
}