using FluentValidation;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.Service;
using DeMasterProCloud.DataModel.Role;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>   A Role validation. </summary>
    ///
    /// <remarks>   Edward, 2020-06-30. </remarks>

    public class RoleValidation : BaseValidation<RoleModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Edward, 2020-06-30. </remarks>
        ///
        /// <param name="roleService">   The role service.  </param>
        /// <param name="httpContextAccessor"></param>

        public RoleValidation(IRoleService roleService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            RuleFor(reg => reg.RoleName)
                .NotEmpty()
                .WithMessage(string.Format(MessageResource.Required, RoleResource.lblRoleName))
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, RoleResource.lblRoleName, "100"))
                .Must((reg, c) => !roleService.IsExist(GetRoleIdFromRoute(), reg.RoleName?.Trim()))
                .WithMessage(string.Format(MessageResource.Exist, RoleResource.lblRoleName))
                .Must(ContainsOnlySafeCharacters)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, RoleResource.lblRoleName));

            RuleFor(reg => reg.Description)
                .MaximumLength(1000)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, RoleResource.lblDescription, "1000"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, RoleResource.lblDescription));
        }

        private int GetRoleIdFromRoute()
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
