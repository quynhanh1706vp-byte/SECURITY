using System;
using Bogus.Extensions;
using FluentValidation;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Account;
using DeMasterProCloud.DataModel.Login;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// Validation for account model
    /// </summary>
    public class AccountValidation : BaseValidation<AccountModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="accountService"></param>
        /// <param name="httpContextAccessor"></param>
        public AccountValidation(IAccountService accountService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            // RuleFor(reg => reg.Username).NotEmpty().NotNull()
            //     .WithMessage(string.Format(MessageResource.EmailEmpty))
            //     .EmailAddress()
            //     .MaximumLength(50)
            //     //.Must((reg, c) => !accountService.IsExist(reg.Username))
            //     .When(reg => reg.Id == 0 && reg.Role != 0)
            //     .WithMessage(string.Format(MessageResource.Exist, AccountResource.lblEmail));


            RuleFor(reg => reg.Password).NotEmpty()
                .When(reg => reg.Id == 0 && reg.Role != 0 && !string.IsNullOrWhiteSpace(reg.ConfirmPassword))
                .MinimumLength(8)
                .MaximumLength(50);

            RuleFor(reg => reg.ConfirmPassword).NotEmpty()
                .When(reg => reg.Id == 0 && reg.Role != 0 && !string.IsNullOrWhiteSpace(reg.Password))
                .MinimumLength(6)
                .MaximumLength(50)
                .Equal(reg => reg.Password).WithMessage(string.Format(MessageResource.Compare,
                    AccountResource.lblPassword, AccountResource.lblConfirmPassword));

            //RuleFor(reg => reg.Role).NotNull()
            //    .Must((reg, c) => accountService.IsValidAccountType(reg.Role))
            //    .WithMessage(AccountResource.msgInvalidAccountType);

            RuleFor(reg => reg.TimeZone)
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, AccessTimeResource.lblTimezone, "100"))
                .Must((reg, c) => Helpers.IsValidTimeZone(reg.TimeZone))
                .WithMessage(string.Format(MessageResource.msgInvalidInput, AccessTimeResource.lblTimezone))
                .When(reg => !String.IsNullOrWhiteSpace(reg.TimeZone));
        }

        private int GetAccountIdFromRoute()
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

    /// <summary>
    /// Forget password validation
    /// </summary>
    public class ForgotPasswordValidation : BaseValidation<ForgotPasswordModel>
    {
        /// <summary>
        /// Forgot password validation
        /// </summary>
        public ForgotPasswordValidation()
        {
            RuleFor(reg => reg.Email).NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, AccountResource.lblEmail))
                .EmailAddress()
                .WithMessage(string.Format(MessageResource.InvalidEmailFormat, AccountResource.lblEmail))
                .MaximumLength(50)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, AccountResource.lblEmail, "50"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, AccountResource.lblEmail));
        }
    }

    /// <summary>
    /// Reset Password Validation
    /// </summary>
    public class ResetPasswordValidation : BaseValidation<ResetPasswordModel>
    {
        /// <summary>
        /// Reset Password Validation
        /// </summary>
        public ResetPasswordValidation()
        {
            RuleFor(reg => reg.NewPassword).NotEmpty()
                .MinimumLength(6)
                .MaximumLength(50);

            RuleFor(reg => reg.ConfirmNewPassword).NotEmpty()
                .MinimumLength(6)
                .MaximumLength(50)
                .Equal(reg => reg.NewPassword).WithMessage(string.Format(MessageResource.Compare,
                    AccountResource.lblNewPassword, AccountResource.lblConfirmNewPassword));

            RuleFor(reg => reg.Token).NotEmpty();
        }
    }
    
    /// <summary>
    /// Reset Password Validation
    /// </summary>
    public class ChangePasswordModelValidation : BaseValidation<ChangePasswordModel>
    {
        /// <summary>
        /// Reset Password Validation
        /// </summary>
        public ChangePasswordModelValidation(IAccountService accountService)
        {
            RuleFor(reg => reg.NewPassword).NotEmpty()
                .MinimumLength(6)
                .MaximumLength(50);

            RuleFor(reg => reg.ConfirmNewPassword).NotEmpty()
                .MinimumLength(6)
                .MaximumLength(50)
                .Equal(reg => reg.NewPassword).WithMessage(string.Format(MessageResource.Compare,
                    AccountResource.lblNewPassword, AccountResource.lblConfirmNewPassword));

            RuleFor(reg => reg.Password).NotEmpty();

            //RuleFor(reg => reg.Username).NotEmpty();

            RuleFor(reg => reg.NewPassword).NotEqual(reg => reg.Password).WithMessage(string.Format(AccountResource.msgSamePasswordBefore));
            

            //RuleFor(reg => reg.Password)
            //    .Must((reg, c) => accountService.GetAuthenticatedAccount(new LoginModel() 
            //    {
            //        Username = reg.Username,
            //        Password = reg.Password
            //    }) != null)
            //    .WithMessage(AccountResource.msgWrongPassword);
        }
    }
   
   
    public class ChangePasswordLoginModelValidation : BaseValidation<ChangePasswordLoginModel>
    {
        /// <summary>
        /// Reset Password Validation
        /// </summary>
        public ChangePasswordLoginModelValidation(IAccountService accountService)
        {
            RuleFor(reg => reg.NewPassword).NotEmpty()
                .MinimumLength(6)
                .MaximumLength(50);

            RuleFor(reg => reg.ConfirmNewPassword).NotEmpty()
                .MinimumLength(6)
                .MaximumLength(50)
                .Equal(reg => reg.NewPassword).WithMessage(string.Format(MessageResource.Compare,
                    AccountResource.lblNewPassword, AccountResource.lblConfirmNewPassword));
        }
    }
}