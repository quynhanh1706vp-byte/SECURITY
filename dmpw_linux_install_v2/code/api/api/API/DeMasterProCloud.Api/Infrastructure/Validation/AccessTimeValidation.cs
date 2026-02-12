using FluentValidation;
using DeMasterProCloud.DataModel.Timezone;
using DeMasterProCloud.Service;
using DeMasterProCloud.Common.Resources;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// AccessTime validation
    /// </summary>
    public class AccessTimeValidation : BaseValidation<AccessTimeModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// AccessTime validation
        /// </summary>
        /// <param name="accessTimeService"></param>
        /// <param name="httpContextAccessor"></param>
        public AccessTimeValidation(IAccessTimeService accessTimeService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            RuleFor(reg => reg.Name).NotEmpty()
                .MaximumLength(50)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, AccessTimeResource.lblTimezoneName, 50))
                .Must((reg, strings) => !accessTimeService.IsExistedName(GetAccessTimeIdFromRoute(), reg.Name?.Trim()))
                .WithMessage(reg => string.Format(MessageResource.Exist, reg.Name))
                .Must(NotContainsCode)
                .WithMessage(reg => string.Format(MessageResource.msgInvalidInput, reg.Name));

            RuleFor(reg => reg.Remarks)
                .MaximumLength(1000)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, AccessTimeResource.lblRemark, 1000))
                .Must(NotContainsCode)
                .WithMessage(reg => string.Format(MessageResource.msgInvalidInput, reg.Remarks));
        }

        private int GetAccessTimeIdFromRoute()
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
