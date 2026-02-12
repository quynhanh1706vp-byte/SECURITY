using FluentValidation;
using System;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Company;
using DeMasterProCloud.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// company validation
    /// </summary>
    public class CompanyValidation : BaseValidation<CompanyModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// company validation
        /// </summary>
        /// <param name="companyService"></param>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        public CompanyValidation(ICompanyService companyService, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            RuleFor(reg => reg.Name).NotEmpty()
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, CompanyResource.lblCompanyName, "100"))
                .Must(ContainsOnlySafeCharacters)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, CompanyResource.lblCompanyName));
            //RuleFor(reg => reg.ExpiredFrom).NotEmpty()
            //    .Must((reg, c) => DateTimeHelper.IsDateTime(reg.ExpiredFrom))
            //    .WithMessage(string.Format(MessageResource.InvalidDateFormat, CompanyResource.lblExpiredDateFrom,
            //        Helpers.GetDateServerFormat()));
            //RuleFor(reg => reg.ExpiredTo).NotEmpty()
            //    .Must((reg, c) => DateTimeHelper.IsDateTime(reg.ExpiredFrom))
            //    .WithMessage(string.Format(MessageResource.InvalidDateFormat, CompanyResource.lblExpiredDateTo,
            //        Helpers.GetDateServerFormat()))
            //    .Must((reg, strings) => Helpers.CompareDate(reg.ExpiredFrom, reg.ExpiredTo))
            //    .WithMessage(reg =>string.Format(MessageResource.GreaterThan, CompanyResource.lblExpiredDateTo, reg.ExpiredFrom));
            //RuleFor(m => Convert.ToDateTime(m.ExpiredTo))
            //.GreaterThanOrEqualTo(m => Convert.ToDateTime(m.ExpiredFrom));

            //RuleFor(reg => reg.Username).NotEmpty()
            //    .MaximumLength(50)
            //    .EmailAddress()
            //    .Must((reg, strings) => !companyService.IsExistedCompanyAccount(GetCompanyIdFromRoute(), reg.Username))
            //    .WithMessage(string.Format(MessageResource.Exist, AccountResource.lblUsername));

            //RuleFor(reg => reg.Password).NotEmpty()
            //    .When(reg => reg.Id == 0)
            //    .MinimumLength(6)
            //    .Must((reg, c) => !Helpers.IsUnicode(reg.Password))
            //    .WithMessage(
            //        string.Format(MessageResource.CannotIncludeUnicodeCharacter, AccountResource.lblPassword));

            //RuleFor(reg => reg.ConfirmPassword).NotEmpty()
            //    .When(reg => reg.Id == 0)
            //    .MinimumLength(6)
            //    .Equal(m => m.Password).WithMessage(string.Format(MessageResource.Compare,
            //        CompanyResource.lblConfirmPassword, CompanyResource.lblPassword));

            //RuleFor(reg => reg.Contact)/*.NotEmpty()*/
            //    .MaximumLength(100);

            RuleFor(reg => reg.Logo).Must(m =>
                    Helpers.IsValidImage(Helpers.GetFileExtension(m),
                        configuration[Constants.AllowImageType].Split(",")))
                .When(m => !string.IsNullOrWhiteSpace(m.Logo))
                .WithMessage(string.Format(MessageResource.InvalidImageExtension, CompanyResource.lblCompanyLogo,
                    configuration[Constants.AllowImageType]));

            RuleFor(reg => reg.MiniLogo).Must(m =>
                    Helpers.IsValidImage(Helpers.GetFileExtension(m),
                        configuration[Constants.AllowImageType].Split(",")))
                .When(m => !string.IsNullOrWhiteSpace(m.MiniLogo))
                .WithMessage(string.Format(MessageResource.InvalidImageExtension, CompanyResource.lblCompanyMiniLogo,
                    configuration[Constants.AllowImageType]));
        }

        private int GetCompanyIdFromRoute()
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
