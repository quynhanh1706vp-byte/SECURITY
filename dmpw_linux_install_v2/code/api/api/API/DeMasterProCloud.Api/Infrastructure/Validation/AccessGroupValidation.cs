using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.AccessGroup;
using DeMasterProCloud.Service;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// AccessGroup validation
    /// </summary>
    public class AccessGroupValidation : BaseValidation<AccessGroupModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// AccessGroup validation
        /// </summary>
        /// <param name="accessGroupService"></param>
        /// <param name="httpContextAccessor"></param>
        public AccessGroupValidation(IAccessGroupService accessGroupService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            RuleFor(reg => reg.Name).NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, AccessGroupResource.lblAccessGroupName))
                .MaximumLength(30)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, AccessGroupResource.lblAccessGroupName, "30"))
                .Must((reg, c) => !accessGroupService.HasExistName(GetAccessGroupIdFromRoute(), reg.Name?.Trim()))
                .WithMessage(string.Format(MessageResource.Exist, AccessGroupResource.lblAccessGroupName))
                .Must(ContainsOnlySafeCharacters)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, AccessGroupResource.lblAccessGroupName));
        }

        private int GetAccessGroupIdFromRoute()
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
