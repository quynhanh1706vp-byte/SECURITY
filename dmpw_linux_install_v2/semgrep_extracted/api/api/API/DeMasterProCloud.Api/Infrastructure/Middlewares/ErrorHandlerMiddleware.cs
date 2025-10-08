using System;
using System.Threading.Tasks;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Email;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace DeMasterProCloud.Api.Infrastructure.Middlewares
{
    /// <summary>
    /// Custom error handle middleware
    /// </summary>
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        /// <param name="mailService"></param>
        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IConfiguration configuration)
        {
            _next = next;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="context"></param>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var errorMessage = $"{exception.Message}{Environment.NewLine}{exception.StackTrace}";
                _logger.LogError(errorMessage);
                await HandleErrorAsync(context, MessageResource.SystemError);
            }
        }

        private Task HandleErrorAsync(HttpContext context, string errorMessage)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            var response = new ApiErrorResultModel(context.Response.StatusCode, errorMessage);
            var payload = Helpers.JsonConvertCamelCase(response);
            //var payload = JsonConvert.SerializeObject(response);
            return context.Response.WriteAsync(payload);
        }
    }
}
