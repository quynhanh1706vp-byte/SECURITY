using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DeMasterProCloud.Service
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CheckAddOnAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _addOn;
        
        public CheckAddOnAttribute(string addOn)
        {
            _addOn = addOn;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                string authorHeader = context.HttpContext.Request.Headers["Authorization"];
                if (string.IsNullOrEmpty(authorHeader))
                {
                    List<string> pathAnonymous = new List<string>()
                    {
                        Constants.Route.ApiRegisterVisitAuthentication,
                        Constants.Route.ApiQRCodeSMS,
                        Constants.Route.ApiRegisterVisitUsersTarget,
                        Constants.Route.ApiVisitGetInfoIdentity,
                        Constants.Route.ApiAnonymousGetVisitByPhone,
                        Constants.Route.ApiVisitsDepartmentsTarget,
                        Constants.Route.ApiRegisterVisitAuthentication,
                    };
                    if (context.HttpContext.Request.Path.HasValue && pathAnonymous.Contains(context.HttpContext.Request.Path.Value))
                    {
                        return;
                    }

                    // link: /register-visit
                    string companyCode = context.HttpContext.Request.Query["companyCode"];
                    if (!string.IsNullOrEmpty(companyCode))
                    {
                        ICompanyService companyService = (ICompanyService) context.HttpContext.RequestServices.GetService(typeof(ICompanyService));
                        var company = companyService.GetByCode(companyCode);
                        if(company != null)
                            return;
                    }
                    else if (!string.IsNullOrEmpty(context.HttpContext.Request.Query["tokenVisit"]))
                    {
                        var tokenVisit = new JwtSecurityTokenHandler().ReadToken(context.HttpContext.Request.Query["tokenVisit"]) as JwtSecurityToken;
                        companyCode = tokenVisit.Claims.First(claim => claim.Type == Constants.ClaimName.CompanyCode).Value;
                        ICompanyService companyService = (ICompanyService) context.HttpContext.RequestServices.GetService(typeof(ICompanyService));
                        var company = companyService.GetByCode(companyCode);
                        if(company != null)
                            return;
                    }
                    context.Result = new ApiErrorResult(StatusCodes.Status404NotFound);
                }

                if (string.IsNullOrEmpty(authorHeader) || authorHeader.Length < 7)
                {
                    Console.WriteLine($"authHeader data is wrong.");
                    Console.WriteLine($"Route value [Controller] : {context.ActionDescriptor.RouteValues["controller"]}");
                    Console.WriteLine($"Route value [API] : {context.ActionDescriptor.RouteValues["action"]}");

                    context.Result = new ApiErrorResult(StatusCodes.Status403Forbidden);
                    return;
                }

                var token = authorHeader.Remove(0, 7);
                if (token.ToLower() == "token_fake") // FE send fake
                {
                    return;
                }

                var handler = new JwtSecurityTokenHandler();
                var tokenS = handler.ReadToken(token) as JwtSecurityToken;
                var accountType = tokenS.Claims.First(claim => claim.Type == Constants.ClaimName.AccountType).Value;
                if (Int32.Parse(accountType) != (short) AccountType.SystemAdmin)
                {
                    var companyId = tokenS.Claims.First(claim => claim.Type == Constants.ClaimName.CompanyId).Value;

                    IPluginService service = (IPluginService)context.HttpContext.RequestServices.GetService(typeof(IPluginService));
                    var valid = service.CheckPluginCondition(_addOn, Int32.Parse(companyId));
                    if (valid)
                    {
                        return;
                    }

                    context.Result = new ApiErrorResult(StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger<CheckAddOnAttribute>();
                logger.LogError(ex, "Error in OnAuthorization");
                context.Result = new ApiErrorResult(StatusCodes.Status403Forbidden);
            }
        }
    }

    public class CheckMultipleAddOnAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string [] _addOn;
        
        public CheckMultipleAddOnAttribute(string [] addOn)
        {
            _addOn = addOn;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                string authorHeader = context.HttpContext.Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorHeader) || authorHeader.Length < 7)
                {
                    Console.WriteLine($"authHeader data is wrong.");
                    Console.WriteLine($"Route value [Controller] : {context.ActionDescriptor.RouteValues["controller"]}");
                    Console.WriteLine($"Route value [API] : {context.ActionDescriptor.RouteValues["action"]}");

                    context.Result = new ApiErrorResult(StatusCodes.Status403Forbidden);
                    return;
                }

                var token = authorHeader.Remove(0, 7);

                var handler = new JwtSecurityTokenHandler();

                var tokenS = handler.ReadToken(token) as JwtSecurityToken;

                var accountType = tokenS.Claims.First(claim => claim.Type == Constants.ClaimName.AccountType).Value;

                if (Int32.Parse(accountType) != (short) AccountType.SystemAdmin)
                {
                    var companyId = tokenS.Claims.First(claim => claim.Type == Constants.ClaimName.CompanyId).Value;

                    IPluginService service = (IPluginService)context.HttpContext.RequestServices.GetService(typeof(IPluginService));
                    bool validPlugIn = false;
                    foreach (var eachAddOn in _addOn)
                    {
                        validPlugIn = service.CheckPluginCondition(eachAddOn, Int32.Parse(companyId));
                        if (validPlugIn)
                        {
                            return;
                        }
                    }


                    context.Result = new ApiErrorResult(StatusCodes.Status404NotFound);
                }
                return;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger<CheckMultipleAddOnAttribute>();
                logger.LogError(ex, "Error in OnAuthorization");
                context.Result = new ApiErrorResult(StatusCodes.Status403Forbidden);
            }
        }
    }

    /// <summary>
    /// Class for checking permission
    /// </summary>
    public class CheckPermission : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _permissionName;

        public CheckPermission(string permissionName)
        {
            _permissionName = permissionName;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                string authorHeader = context.HttpContext.Request.Headers["Authorization"];

                if(string.IsNullOrEmpty(authorHeader) || authorHeader.Length < 7)
                {
                    Console.WriteLine($"authHeader data is wrong.");
                    Console.WriteLine($"Route value [Controller] : {context.ActionDescriptor.RouteValues["controller"]}");
                    Console.WriteLine($"Route value [API] : {context.ActionDescriptor.RouteValues["action"]}");

                    context.Result = new ApiErrorResult(StatusCodes.Status403Forbidden);
                    return;
                }

                var token = authorHeader.Remove(0, 7);
                if (token.ToLower() == "token_fake") // FE send fake
                {
                    return;
                }

                var handler = new JwtSecurityTokenHandler();
                var tokenS = handler.ReadToken(token) as JwtSecurityToken;
                var accountType = tokenS.Claims.First(claim => claim.Type == Constants.ClaimName.AccountType).Value;
                if (Int32.Parse(accountType) != (short)AccountType.SystemAdmin && Int32.Parse(accountType) != (short)AccountType.PrimaryManager)
                {
                    var companyId = tokenS.Claims.First(claim => claim.Type == Constants.ClaimName.CompanyId).Value;
                    var accountId = tokenS.Claims.First(claim => claim.Type == Constants.ClaimName.AccountId).Value;

                    IRoleService roleService = (IRoleService)context.HttpContext.RequestServices.GetService(typeof(IRoleService));
                    var valid = roleService.CheckPermissionEnabled(_permissionName, Int32.Parse(accountId), Int32.Parse(companyId));
                    if (valid) { return; }
                    context.Result = new ApiErrorResult(StatusCodes.Status403Forbidden);
                }
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger<CheckPermission>();
                logger.LogError(ex, "Error in OnAuthorization");
                context.Result = new ApiErrorResult(StatusCodes.Status403Forbidden);
            }
        }

    }

    /// <summary>
    /// Class for checking permission
    /// </summary>
    public class CheckMultiPermission : AuthorizeAttribute, IAuthorizationFilter
    {
        // true -> AND condition
        // false -> OR condition
        private readonly bool _isAnd;
        private readonly string[] _permissions;

        public CheckMultiPermission(string[] permissions, bool isAnd)
        {
            _permissions = permissions;
            _isAnd = isAnd;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                string authorHeader = context.HttpContext.Request.Headers["Authorization"];

                if (string.IsNullOrEmpty(authorHeader) || authorHeader.Length < 7)
                {
                    Console.WriteLine($"authHeader data is wrong.");
                    Console.WriteLine($"Route value [Controller] : {context.ActionDescriptor.RouteValues["controller"]}");
                    Console.WriteLine($"Route value [API] : {context.ActionDescriptor.RouteValues["action"]}");

                    context.Result = new ApiErrorResult(StatusCodes.Status403Forbidden);
                    return;
                }

                var token = authorHeader.Remove(0, 7);

                var handler = new JwtSecurityTokenHandler();

                var tokenS = handler.ReadToken(token) as JwtSecurityToken;

                var accountType = tokenS.Claims.First(claim => claim.Type == Constants.ClaimName.AccountType).Value;

                if (Int32.Parse(accountType) != (short)AccountType.SystemAdmin && Int32.Parse(accountType) != (short)AccountType.PrimaryManager)
                {
                    var companyId = tokenS.Claims.First(claim => claim.Type == Constants.ClaimName.CompanyId).Value;
                    var accountId = tokenS.Claims.First(claim => claim.Type == Constants.ClaimName.AccountId).Value;

                    IRoleService roleService = (IRoleService)context.HttpContext.RequestServices.GetService(typeof(IRoleService));

                    List<bool> resultList = new List<bool>();

                    foreach (var permission in _permissions)
                    {
                        var valid = roleService.CheckPermissionEnabled(permission, Int32.Parse(accountId), Int32.Parse(companyId));

                        resultList.Add(valid);
                    }

                    if (resultList.Contains(!_isAnd) == _isAnd)
                        context.Result = new ApiErrorResult(StatusCodes.Status403Forbidden);
                    else
                        return;
                }

                return;
            }
            catch (Exception ex)
            {
                var logger = ApplicationVariables.LoggerFactory.CreateLogger<CheckMultiPermission>();
                logger.LogError(ex, "Error in OnAuthorization");
                context.Result = new ApiErrorResult(StatusCodes.Status403Forbidden);
            }
        }
    }
}