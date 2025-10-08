using FluentValidation;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.DeviceReader;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// DeviceReader validation
    /// </summary>
    public class DeviceReaderValidation : BaseValidation<DeviceReaderModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// DeviceReader validation
        /// </summary>
        /// <param name="deviceReaderService"></param>
        /// <param name="httpContextAccessor"></param>
        public DeviceReaderValidation(IDeviceReaderService deviceReaderService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            RuleFor(reg => reg.Name).NotEmpty()
                .MaximumLength(50)
                .Must((reg, strings) => !deviceReaderService.IsExistedName(GetDeviceReaderIdFromRoute(), reg.Name?.Trim()))
                .WithMessage(reg => string.Format(MessageResource.Exist, reg.Name));
        }

        private int GetDeviceReaderIdFromRoute()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Request.RouteValues.TryGetValue("id", out var idValue) == true)
            {
                if (int.TryParse(idValue?.ToString(), out var id))
                {
                    return id;
                }
            }
            return 0; // Default for Add operations
        }
    }
}
