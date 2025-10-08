using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DeMasterProCloud.Api.Infrastructure.Filters
{
    /// <summary>
    /// Authorize filter
    /// </summary>
    public class AuthorizeFilter : AuthorizeAttribute, IAuthorizationFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string authHeader = context.HttpContext.Request.Headers[Constants.Auth.Authorization];
            if (authHeader == null || !authHeader.StartsWith(Constants.Auth.BearerHeader))
            {
                context.Result = new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidToken,
                    MessageResource.InvalidToken);
                return;
            }

            // Extract credentials
            var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<JwtOptionsModel>>();
            var bearerToken = authHeader.Substring(Constants.Auth.BearerHeader.Length)
                .Trim();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidIssuer = options.Value.Issuer,
                ValidAudience = options.Value.Issuer,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SecretKey)),
                ValidateLifetime = true
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal =
                tokenHandler.ValidateToken(bearerToken, tokenValidationParameters, out var securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                context.Result = new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidToken,
                    MessageResource.InvalidToken);
            }
        }
    }
}
