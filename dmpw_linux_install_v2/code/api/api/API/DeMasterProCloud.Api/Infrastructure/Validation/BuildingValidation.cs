using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Building;
using DeMasterProCloud.Service;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// Building validation
    /// </summary>
    public class BuildingValidation : BaseValidation<BuildingModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Building validation.
        /// </summary>
        /// <param name="buildingService"></param>
        /// <param name="httpContextAccessor"></param>
        public BuildingValidation(IBuildingService buildingService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            RuleFor(reg => reg.Name)
                .NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, BuildingResource.lblBuildingName))
                .MaximumLength(50)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, BuildingResource.lblBuildingName, "50"))
                .Must((reg, strings) => !buildingService.IsExistedBuildingName(GetBuildingIdFromRoute(), reg.Name?.Trim()))
                .WithMessage(reg => string.Format(MessageResource.Exist, reg.Name))
                .Must(ContainsOnlySafeCharacters)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, BuildingResource.lblBuildingName));
            
            RuleFor(reg => reg.Address)
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, BuildingResource.lblAddress, "100"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, BuildingResource.lblAddress));
        }

        private int GetBuildingIdFromRoute()
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
