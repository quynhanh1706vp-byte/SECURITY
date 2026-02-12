using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;

namespace DeMasterProCloud.DataModel.Api
{
    public class ApiModelError
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; set; }

        public string Message { get; set; }

        public ApiModelError(string field, string message)
        {
            Field = field != string.Empty ? field : null;
            Message = message;
        }
    }

    public class ApiErrorResultModel
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public List<ApiModelError> Errors { get; set; }

        public ApiErrorResultModel(ModelStateDictionary modelState)
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity;
            Errors = modelState.Keys
                .SelectMany(key => modelState[key].Errors.Select(x => new ApiModelError(key.ToCamelCase(), x.ErrorMessage)))
                .ToList();

            if (Errors != null && Errors.Any())
            {
                Message = Errors.First().Message;
            }
            else
            {
                Message = MessageResource.ValidationFailed;
            }
        }

        public ApiErrorResultModel(int metaCode, string message = null)
        {
            StatusCode = metaCode;
            Message = message ?? GetDefaultMessageForStatusCode(metaCode);
            Errors = new List<ApiModelError>()
            {
                new ApiModelError("", message)
            };
        }
        
        public ApiErrorResultModel()
        {

        }

        private static string GetDefaultMessageForStatusCode(int statusCode)
        {
            switch (statusCode)
            {
                case StatusCodes.Status400BadRequest:
                    return MessageResource.BadRequest;
                case StatusCodes.Status401Unauthorized:
                    return MessageResource.Unauthorized;
                case StatusCodes.Status403Forbidden:
                    return MessageResource.Forbidden;
                case StatusCodes.Status404NotFound:
                    return MessageResource.ResourceNotFound;
                case StatusCodes.Status408RequestTimeout:
                    return MessageResource.RequestTimeout;
                case StatusCodes.Status422UnprocessableEntity:
                    return MessageResource.UnprocessableEntity;
                case 500:
                    return MessageResource.SystemError;
                default:
                    return MessageResource.AnUnexpectedErrorOccurred;
            }
        }
    }

    public class ValidationFailedResult : ObjectResult
    {
        public ValidationFailedResult(ModelStateDictionary modelState)
            : base(new ApiErrorResultModel(modelState))
        {
            StatusCode = StatusCodes.Status422UnprocessableEntity;
        }
    }

    public class ApiUnauthorizedResult : ObjectResult
    {
        public ApiUnauthorizedResult(int metaCode, string message)
            : base(new ApiErrorResultModel(metaCode, message))
        {
            StatusCode = StatusCodes.Status401Unauthorized;
        }
    }

    public class ApiErrorResult : ObjectResult
    {
        public ApiErrorResult(int metaCode, string message = null)
            : base(new ApiErrorResultModel(metaCode, message))
        {
            StatusCode = metaCode;
        }
    }

    public class ApiSuccessResult : ObjectResult
    {
        public ApiSuccessResult(int metaCode, string message = null, string data = null)
            : base(new ApiSuccessResultModel(metaCode, message, data))
        {
            StatusCode = metaCode;
        }
    }

    public class ApiSuccessResultModel
    {
        public ApiSuccessResultModel(int metaCode, string message, string data )
        {
            StatusCode = metaCode;
            //Message = message?.FirstCharToUpper();
            Message = message;
            Data = data?.FirstCharToUpper();
        }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        
        public string Data { get; set; }
    }
}