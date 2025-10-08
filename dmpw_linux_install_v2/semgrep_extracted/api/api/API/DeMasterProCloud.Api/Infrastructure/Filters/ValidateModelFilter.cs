using System.Threading.Tasks;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DeMasterProCloud.Api.Infrastructure.Filters
{
    /// <summary>
    /// Validate model filter
    /// </summary>
    public class ValidateModelFilter : IAsyncActionFilter
    {
        // private readonly IOptions<MvcJsonOptions> _jsonOptions;
        //
        // /// <summary>
        // /// Ctor
        // /// </summary>
        // /// <param name="jsonOptions"></param>
        // public ValidateModelFilter(IOptions<MvcJsonOptions> jsonOptions)
        // {
        //     _jsonOptions = jsonOptions;
        // }

        /// <summary>
        /// OnActionExecutionAsync
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
                context.HttpContext.Response.ContentType = "application/json";
                var response = new ApiErrorResultModel(context.HttpContext.Response.StatusCode);
                var payload = Helpers.JsonConvertCamelCase(response);
                //var payload = JsonConvert.SerializeObject(response);
                await context.HttpContext.Response.WriteAsync(payload);
            }
            await next(); // the actual action
        }
    }
}
