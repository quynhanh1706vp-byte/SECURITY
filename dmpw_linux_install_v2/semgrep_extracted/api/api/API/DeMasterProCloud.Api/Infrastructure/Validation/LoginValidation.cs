using FluentValidation;
using DeMasterProCloud.DataModel.Login;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// Login validation
    /// </summary>
    public class LoginValidation : AbstractValidator<LoginModel>
    {
        /// <summary>
        /// Login validation
        /// </summary>
        public LoginValidation()
        {
            RuleFor(reg => reg.Username).NotEmpty();

            RuleFor(reg => reg.Password).NotEmpty();
        }
    }
}
