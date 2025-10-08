using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DeMasterProCloud.Common.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using DeMasterProCloud.DataModel.Api;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Service
{
    /// <summary>
    /// Class Handle token api
    /// </summary>
    public interface IJwtHandler
    {
        bool IsTokenExpired(string token);
        string BuildRefreshToken(IEnumerable<Claim> claims);

        string BuilToken(IEnumerable<Claim> claims);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
    public class JwtHandler : IJwtHandler
    {
        private readonly JwtOptionsModel _options;
        private readonly ILogger<JwtHandler> _logger;

        public JwtHandler(IOptions<JwtOptionsModel> options)
        {
            _options = options.Value;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<JwtHandler>();
        }
        /// <summary>
        /// Build a token from claims
        /// </summary>
        /// <param name="claims"></param>
        /// <returns></returns>
        public string BuilToken(IEnumerable<Claim> claims)
        {
            var currentTime = DateTime.UtcNow;
            var expiredTime = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)); //Secret
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(_options.Issuer,
                _options.Issuer,
                claims,
                currentTime,
                expiredTime,
                creds);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        /// <summary>
        /// Check if token is expired
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsTokenExpired(string token)
        {
            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadToken(token);
                return jwt.ValidTo.Subtract(DateTime.UtcNow).Minutes < 0;
            } catch (Exception)
            {
                return false;
            }
            
        }

        /// <summary>
        /// Generating a 32 byte long random number and converting it to base64
        /// </summary>
        /// <returns></returns>
        public string BuildRefreshToken(IEnumerable<Claim> claims)
        {
            var currentTime = DateTime.UtcNow;
            var expiredTime = DateTime.UtcNow.AddMinutes(_options.expiryRefreshToken);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)); //Secret
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(_options.Issuer,
                _options.Issuer,
                claims,
                currentTime,
                expiredTime,
                creds);

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        

        /// <summary>
        /// Get principal from expired token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey)),
                    ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                    throw new SecurityTokenException("Invalid token");

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating expired token");
                return null;
            }
        }
    }
}