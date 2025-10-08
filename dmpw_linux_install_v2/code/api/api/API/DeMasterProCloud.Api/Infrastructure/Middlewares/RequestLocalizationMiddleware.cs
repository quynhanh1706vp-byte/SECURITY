using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Account;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MoreLinq;
using Namotion.Reflection;
using Newtonsoft.Json;

namespace DeMasterProCloud.Api.Infrastructure.Middlewares
{
    /// <summary>
    /// Request Localization middleware
    /// </summary>
    public class RequestLocalizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly IOptions<List<string>> _options;
        private readonly int _maxPageSize;
        private readonly List<string> _urlIgnored;
        private readonly LoginSessionConfig _loginSessionConfig;

        /// <summary>
        /// Request localization in middle ware
        /// </summary>
        /// <param name="next"></param>
        /// <param name="configuration"></param>
        /// <param name="options"></param>
        public RequestLocalizationMiddleware(RequestDelegate next, IConfiguration configuration, IOptions<List<string>> options)
        {
            _next = next;
            _configuration = configuration;
            _options = options;
            _maxPageSize = configuration.GetSection("QuerySetting:MaxPageSize").Get<int>();
            if (_maxPageSize == 0) _maxPageSize = Constants.DefaultPaggingQuery;
            _urlIgnored = configuration.GetSection("QuerySetting:UrlIgnored").Get<List<string>>() ?? new List<string>();
            _loginSessionConfig = configuration.GetSection("LoginSessionConfig").Get<LoginSessionConfig>();
        }

        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            SetCurrentCulture(context);
            CheckPageSizeQuery(context);
            TrimFromQuery(context);
            bool isUpdated = CheckAndUpdateLoginInfo(context);
            if (isUpdated)
                await _next(context);
        }

        /// <summary>
        /// Set culture for each api request
        /// </summary>
        /// <param name="context"></param>
        private void SetCurrentCulture(HttpContext context)
        {
            var reqCulture = context.Request.Query["culture"].ToString();
            if (string.IsNullOrEmpty(reqCulture) || !_options.Value.Contains(reqCulture))
            {
                ////Set default culture en-US
                //reqCulture = _options.Value[0];

                //Set default culture account's culture
                if (context.Request.Headers.TryGetValue("Authorization", out var authorization))
                {
                    // Try to get current logged in account's language.
                    string token = authorization.ToString().Replace("Bearer", "").Trim();
                    var jwt = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
                    if (jwt == null)
                    {
                        throw new Exception("JWT NULL" + token);
                    }
                    var accountIdValue = jwt.Claims.FirstOrDefault(m => m.Type == Constants.ClaimName.AccountId)?.Value;

                    if (int.TryParse(accountIdValue, out int accountId))
                    {
                        using (IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration))
                        {
                            var account = unitOfWork.AccountRepository.GetById(accountId);

                            if (account != null)
                            {
                                reqCulture = account.Language;
                            }
                        }
                    }
                }
            }
            
            var culture = new CultureInfo(reqCulture);
            //context.Response.Cookies.Append(
            //    CookieRequestCultureProvider.DefaultCookieName,
            //    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            //    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(30) }
            //);

            CultureInfo.CurrentCulture = new CultureInfo(culture.Name);
            CultureInfo.CurrentUICulture = culture;
        }

        private void CheckPageSizeQuery(HttpContext context)
        {
            context.Request.Query.TryGetValue("pageSize", out var strPageSize);
            string path = context.Request.Path;
            if (!_urlIgnored.Contains(path) && !string.IsNullOrEmpty(strPageSize))
            {
                int pageSize = 0;
                int.TryParse(strPageSize, out pageSize);
                if (pageSize > _maxPageSize)
                {
                    // context.Response.ContentType = "application/json";
                    // var response = new ApiErrorResult(StatusCodes.Status400BadRequest, $"Maximum page size for url {path}: 100 records");
                    // var payload = JsonConvert.SerializeObject(response);
                    // context.Response.WriteAsync(payload);
                    string queryString = context.Request.QueryString.ToString();
                    context.Request.QueryString = new QueryString(queryString.Replace($"pageSize={pageSize}", $"pageSize={_maxPageSize}"));
                }
            }
        }

        private static void TrimFromQuery(HttpContext context)
        {
            //List<string> paramList = new List<string>() {
            //    "search",
            //    "filter",
            //    "searchAll",
            //    "userName",
            //    "cardId",
            //    "type",
            //};

            var paramList = context.Request.Query.Keys;
            foreach(var paramName in paramList)
            {
                CheckTextFilterQuery(context, paramName);
            }
        }

        private static void CheckTextFilterQuery(HttpContext context, string paramName)
        {
            context.Request.Query.TryGetValue(paramName, out var strValue);
            if (!string.IsNullOrWhiteSpace(strValue) && Helpers.HaveEmptySpaceInStr(strValue))
            {
                var encodedStr = KoreanHelpers.EncodingUTF8OnlyKorean(strValue);

                var newSearch = encodedStr.Trim().Replace(" ", "%20");
                var oldSearch = encodedStr.Replace(" ", "%20");
                if (!newSearch.Equals(oldSearch))
                {
                    string queryString = context.Request.QueryString.ToString();
                    context.Request.QueryString = new QueryString(queryString.Replace($"{paramName}={oldSearch}", $"{paramName}={newSearch}"));
                }
            }

            //context.Request.Query.TryGetValue("search", out var strSearch);
            //if (!string.IsNullOrWhiteSpace(strSearch))
            //{
            //    var encodedStr = KoreanHelpers.EncodingUTF8OnlyKorean(strSearch);

            //    var newSearch = encodedStr.Trim().Replace(" ", "%20");
            //    var oldSearch = encodedStr.Replace(" ", "%20");
            //    string queryString = context.Request.QueryString.ToString();
            //    context.Request.QueryString = new QueryString(queryString.Replace($"search={oldSearch}", $"search={newSearch}"));
            //}
            
            //context.Request.Query.TryGetValue("filter", out var strFilter);
            //if (!string.IsNullOrWhiteSpace(strFilter))
            //{
            //    var encodedStr = KoreanHelpers.EncodingUTF8OnlyKorean(strFilter);

            //    var newSearch = encodedStr.Trim().Replace(" ", "%20");
            //    var oldSearch = encodedStr.Replace(" ", "%20");
            //    string queryString = context.Request.QueryString.ToString();
            //    context.Request.QueryString = new QueryString(queryString.Replace($"filter={oldSearch}", $"filter={newSearch}"));
            //}

            //context.Request.Query.TryGetValue("searchAll", out var strSearchAll);
            //if (!string.IsNullOrWhiteSpace(strSearchAll))
            //{
            //    var encodedStr = KoreanHelpers.EncodingUTF8OnlyKorean(strSearchAll);

            //    var newSearch = encodedStr.Trim().Replace(" ", "%20");
            //    var oldSearch = encodedStr.Replace(" ", "%20");
            //    string queryString = context.Request.QueryString.ToString();
            //    context.Request.QueryString = new QueryString(queryString.Replace($"searchAll={oldSearch}", $"searchAll={newSearch}"));
            //}
        }

        // return true: updated, false: something wrong (session expired, account is being used on another device)
        private bool CheckAndUpdateLoginInfo(HttpContext context)
        {
            if (_loginSessionConfig == null || !_loginSessionConfig.EnableSingleIpAddress)
            {
                return true;
            }

            try
            {
                if (context.Request.Headers.TryGetValue("Authorization", out var authorization))
                {
                    string token = authorization.ToString().Replace("Bearer", "").Trim();
                    var jwt = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
                    if (jwt == null)
                    {
                        throw new Exception("JWT NULL" + token);
                    }

                    var accountIdValue = jwt.Claims.FirstOrDefault(m => m.Type == Constants.ClaimName.AccountId)?.Value;
                    string ipAddress = context.GetIpAddressRequest();

                    if (int.TryParse(accountIdValue, out int accountId) && !string.IsNullOrEmpty(ipAddress))
                    {
                        IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                        CurrentLoginInfoModel currentLoginInfo = unitOfWork.AccountRepository.GetCurrentLoginInfo(accountId);
                        if (currentLoginInfo != null)
                        {
                            // check enable single ip address
                            if (!_loginSessionConfig.EnableSingleIpAddress)
                            {
                                return true;
                            }

                            if (currentLoginInfo.IpAddress == ipAddress)
                            {
                                // check session expired date
                                if ((DateTime.Now - currentLoginInfo.ActiveTime).TotalMinutes > _loginSessionConfig.SessionExpiredTime)
                                {
                                    // Session expired
                                    context.Response.ContentType = "application/json";
                                    context.Response.Headers.Add("Access-Control-Allow-Methods", context.Request.Method);
                                    context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                    var response = new ApiErrorResult(StatusCodes.Status401Unauthorized, AccountResource.msgSessionExpired);
                                    //var payload = JsonConvert.SerializeObject(response);
                                    var payload = Helpers.JsonConvertCamelCase(response);
                                    context.Response.WriteAsync(payload);
                                    return false;
                                }
                            }
                            else
                            {
                                // account using for other ip address
                                context.Response.ContentType = "application/json";
                                context.Response.Headers.Add("Access-Control-Allow-Methods", context.Request.Method);
                                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                var response = new ApiErrorResult(StatusCodes.Status401Unauthorized, AccountResource.msgAccountUseOther);
                                //var payload = JsonConvert.SerializeObject(response);
                                var payload = Helpers.JsonConvertCamelCase(response);
                                context.Response.WriteAsync(payload);
                                return false;
                            }
                        }
                        
                        unitOfWork.AccountRepository.SaveCurrentLogin(accountId, new CurrentLoginInfoModel()
                        {
                            IpAddress = ipAddress,
                            ActiveTime = DateTime.Now,
                        });

                        unitOfWork.Save();
                        unitOfWork.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return true;
        }
    }
}
