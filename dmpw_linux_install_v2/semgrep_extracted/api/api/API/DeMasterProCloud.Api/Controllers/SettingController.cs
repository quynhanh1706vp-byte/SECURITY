using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.Service;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.DataModel.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
using DeMasterProCloud.DataModel.Header;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Login;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Setting controller
    /// </summary>
    [Produces("application/json")]
    public class SettingController : Controller
    {
        private readonly ISettingService _settingService;
        private readonly IConfiguration _configuration;
        private readonly HttpContext _httpContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// Ctor of setting
        /// </summary>
        /// <param name="settingService"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        public SettingController(ISettingService settingService, IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _settingService = settingService;
            _configuration = configuration;
            _httpContext = httpContextAccessor.HttpContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Get the setting
        /// </summary>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiSettings)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Setting)]
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

        /// <summary>   Get the setting. </summary>
        /// <remarks>   Edward, 2020-01-29. </remarks>
        /// <param name="id">   The identifier. </param>
        /// <returns>   An IActionResult. </returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Setting service does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiSettingsId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.Setting)]
        public IActionResult Get(int id)
        {
            var setting = _settingService.GetById(id);
            if (setting == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundSetting);
            }
            var model = _mapper.Map<SettingEditModel>(setting);
            return Ok(model);
        }

        /// <summary>   (An Action that handles HTTP PUT requests) edits. </summary>
        /// <remarks>   Edward, 2020-01-29. </remarks>
        /// <param name="id">       The identifier. </param>
        /// <param name="model">    The model. </param>
        /// <returns>   An IActionResult. </returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Setting service does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiSettingsId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.Setting)]
        public IActionResult Edit(int id, [FromBody]SettingModel model)
        {
            var setting = _settingService.GetById(id);
            if (setting == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundSetting);
            }

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, SettingResource.lblSetting, ""));
        }

        /// <summary>   (An Action that handles HTTP PATCH requests) edit multiple. </summary>
        /// <remarks>   Edward, 2020-01-29. </remarks>
        /// <param name="models">   The models. </param>
        /// <returns>   An IActionResult. </returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: List of Setting Services in Model JSOn does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPatch]
        [Route(Constants.Route.ApiSettings)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.Setting)]
        public IActionResult EditMultiple([FromBody]List<SettingModel> models)
        {
            if (models == null || !models.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, string.Format(MessageResource.Required, SettingResource.lblSetting));
            }
            
            // check pagination setting (pageSize)
            var settingPagination = _settingService.GetByKey(Constants.Settings.PaginationPage, _httpContext.User.GetCompanyId()) ?? new Setting();
            var modelPagination = models.FirstOrDefault(m => m.Id == settingPagination.Id);
            if (modelPagination is { Value: not null })
            {
                int maximumPageSize = _configuration.GetSection("QuerySetting:MaxPageSize").Get<int>();
                maximumPageSize = maximumPageSize == 0 ? Constants.DefaultPaggingQuery : maximumPageSize;
                if (int.TryParse(modelPagination.Value.FirstOrDefault(), out int pagination))
                {
                    if (pagination > maximumPageSize)
                    {
                        return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.LessThan, SettingResource.pagination_title, maximumPageSize));
                    }
                }
                else
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.Number, SettingResource.pagination_title));
                }
            }

            try
            {
                _settingService.Update(models, _httpContext.User.GetCompanyId());
                return new ApiSuccessResult(StatusCodes.Status200OK,
                    string.Format(MessageResource.MessageUpdateSuccess, SettingResource.lblSetting, ""));
            }
            catch (System.Exception e)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, e.Message);
            }
        }


        /// <summary>   (An Action that handles HTTP PATCH requests) edit header setting. </summary>
        /// <remarks>   Edward, 2022-02-08. </remarks>
        /// <param name="headerData">   The header data model. </param>
        /// <returns>   An IActionResult. </returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: List of Setting Services in Model JSOn does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPatch]
        [Route(Constants.Route.ApiHeaderSettings)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.Setting)]
        public IActionResult EditHeaderSetting([FromBody] List<HeaderData> headerData)
        {
            if(headerData == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, string.Format(MessageResource.Required, MessageResource.lblHeader));
            }

            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();

            //if (!_settingService.UpdateHeaderSetting(headerData, companyId, accountId))
            //{
            //    return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.MsgFail);
            //}

            _settingService.UpdateHeaderSetting(headerData, companyId, accountId);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, SettingResource.lblSetting, ""));
        }

        [HttpGet]
        [Route(Constants.Route.ApiSettingsPassword)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // [CheckPermission(ActionName.View + Page.Setting)]
        public IActionResult GetPasswordSetting()
        {

            var settings = _settingService.GetLoginSetting(_httpContext.User.GetCompanyId());
            
            return Ok(settings);
        }
        
        [HttpPut]
        [Route(Constants.Route.ApiSettingsPassword)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.Setting)]
        public IActionResult UpdatePasswordSetting([FromBody] LoginSettingModel model)
        {
            try
            {
                ApplicationVariables.TempPasswordSetting = JsonConvert.SerializeObject(model);
                _settingService.UpdateLoginSetting(model, _httpContext.User.GetCompanyId());
            }
            catch (Exception)
            {
                return new ApiSuccessResult(StatusCodes.Status422UnprocessableEntity, "Update setting failed");
            }
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSuccess);
        }
        
        // /// <summary>
        // /// create new account rabbit mq
        // /// </summary>
        // /// <param name="model"></param>
        // /// <returns></returns>
        // [HttpPost]
        // [Route(Constants.Route.ApiAddAccountRabbitmq)]
        // [AllowAnonymous]
        // // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // public IActionResult AddAccountRabbitMq([FromBody]List<AccountRabbitModel> model)
        // {
        //     try
        //     {
        //         _rabbitMqService.AddNewAccountRabbitMq(model);
        //         return new ApiSuccessResult(StatusCodes.Status200OK);
        //     }
        //     catch (Exception e)
        //     {
        //         return new ApiErrorResult(StatusCodes.Status400BadRequest, e.Message);
        //     }
        // }
        // /// <summary>
        // /// delete account rabbit mq
        // /// </summary>
        // /// <param name="userName"></param>
        // /// <returns></returns>
        // [HttpDelete]
        // [Route(Constants.Route.ApiDeleteAccountRabbitmq)]
        // [AllowAnonymous]
        // // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // public IActionResult DeleteAccountRabbitMq(List<string> userName)
        // {
        //     try
        //     {
        //         _rabbitMqService.DeleteNewAccountRabbitMq(userName);
        //         return new ApiSuccessResult(StatusCodes.Status200OK);
        //     }
        //     catch (Exception e)
        //     {
        //         return new ApiErrorResult(StatusCodes.Status400BadRequest, e.Message);
        //     }
        // }
    }
}