using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.SystemInfo;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DeMasterProCloud.Api.Controllers
{
    public class SystemInfoController : Controller
    {
        private readonly HttpContext _httpContext;
        private readonly ISystemInfoService _systemInfoService;
        private readonly IConfiguration _configuration;

        public SystemInfoController(IHttpContextAccessor httpContextAccessor, ISystemInfoService systemInfoService, IConfiguration configuration)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _systemInfoService = systemInfoService;
            _configuration = configuration;
        }
        
        /// <summary>
        /// Verify server license
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiVerifyLicenseKey)]
        public IActionResult VerifyServerLicense([FromBody] VerifyLicenseModel model)
        {
            string msgError = _systemInfoService.VerifyLicense(model);
            if (string.IsNullOrEmpty(msgError))
            {
                return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.msgVerifyLicenseSuccess);
            }
            else
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, msgError);
            }
        }


        /// <summary>
        /// Update link avatar
        /// </summary>
        /// <param name="oldUrl">Ex: https://xxx.xxx.xxx.xxx</param>
        /// <param name="newUrl">Ex: https://yyy.yyy.yyy.yyy</param>
        /// <returns></returns>
        [HttpPatch]
        [Route(Constants.Route.ApiSystemMigrationResetAvatar)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult MigrationAvatarUser(string oldUrl, string newUrl)
        {
            if (string.IsNullOrWhiteSpace(oldUrl) || string.IsNullOrWhiteSpace(newUrl))
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, "url not null");
            }
            
            var result = _systemInfoService.MigrationAvatarUser(oldUrl, newUrl);
            return Ok(result);
        }

        /// <summary>
        /// Migrate url link image event-log
        /// </summary>
        /// <param name="oldUrl"></param>
        /// <param name="newUrl"></param>
        /// <param name="companyCode"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route(Constants.Route.ApiSystemMigrationLinkEventLog)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult MigrationLinkImageEventLog(string oldUrl, string newUrl, string companyCode, DateTime start, DateTime end)
        {
            return Ok(_systemInfoService.MigrationLinkImageEventLog(oldUrl, newUrl, companyCode, start, end));
        }
        
        // Testing verify license
        [HttpGet]
        [Route(Constants.Route.ApiVerifyLicenseKeyTest)]
        [Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult VerifyLicenseTesting()
        {
            _systemInfoService.VerifyLicenseTesting();
            return Ok();
        }
    }
}