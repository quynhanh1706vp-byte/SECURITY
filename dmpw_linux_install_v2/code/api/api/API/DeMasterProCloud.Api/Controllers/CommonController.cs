using System;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Common Controller
    /// </summary>
    [Produces("application/json")]
    public class CommonController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly HttpContext _httpContext;


        /// <summary>
        /// Common controller
        /// </summary>
        /// <param name="settingService"></param>
        /// <param name="httpContextAccessor"></param>
        public CommonController(ISettingService settingService, IHttpContextAccessor httpContextAccessor)
        {
            _settingService = settingService;
            _httpContext = httpContextAccessor.HttpContext;
        }

        /// <summary>
        /// Get current Logo
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCurrentLogo)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdminAndEmployee, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetCurrentLogo()
        {
            var currentQRLogo = _settingService.GetCurrentLogo(_httpContext.User.GetCompanyId());
            return Ok(currentQRLogo);
        }

        /// <summary>
        /// Get current Logo
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCurrentQRLogo)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdminAndEmployee, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetCurrentQRLogo()
        {
            var currentLogo = _settingService.GetCurrentQRLogo(_httpContext.User.GetCompanyId());
            return Ok(currentLogo);
        }

        /// <summary>
        /// Get Common Setting
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCommonSettings)]
        [Authorize(Policy = Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Get()
        {
            var settings = _settingService.GetAll(_httpContext.User.GetCompanyId()).GroupBy(x => x.Category)
                .Select(x => new SettingByCategoryModel
                {
                    Category = x.Key,
                    Settings = x.Select(m => m).ToList()
                })
                .ToList();
            return Ok(settings);
        }
    }
}