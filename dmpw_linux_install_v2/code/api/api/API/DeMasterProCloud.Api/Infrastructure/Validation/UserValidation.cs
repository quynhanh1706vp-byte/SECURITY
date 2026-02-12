using System;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>   A user validation. </summary>
    ///
    /// <remarks>   Edward, 2020-01-30. </remarks>

    public class UserValidation : BaseValidation<UserModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>   Constructor. </summary>
        ///
        /// <remarks>   Edward, 2020-01-30. </remarks>
        ///
        /// <param name="configuration">    The configuration. </param>
        /// <param name="userService">      The user service. </param>
        /// <param name="httpContextAccessor"></param>

        public UserValidation(IConfiguration configuration, IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;

            RuleFor(reg => reg.Address)
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, BuildingResource.lblAddress, "100"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, UserResource.lblAddress));
            RuleFor(reg => reg.DepartmentId).NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, UserResource.lblDepartment));
            RuleFor(reg => reg.AccessGroupId).NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, UserResource.lblAccessGroup));
            RuleFor(reg => reg.UserCode)
                .MaximumLength(30)
                .WithMessage(string.Format(UserResource.msgUserCodeLength))
                .Must((reg, c) => !userService.IsDuplicatedUserCode(GetUserIdFromRoute(), reg.UserCode?.Trim(), null))
                .WithMessage(string.Format(UserResource.msgUserCodeIsBeingUsedByAnother));

            RuleFor(reg => reg.EffectiveDate).Must((reg, c) => DateTimeHelper.IsDateTime(reg.EffectiveDate))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, UserResource.lblEffectiveDate,
                    Helpers.GetDateServerFormat()));

            RuleFor(reg => reg.ExpiredDate).Must((reg, c) => DateTimeHelper.IsDateTime(reg.ExpiredDate))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, UserResource.lblExpiredDate,
                    Helpers.GetDateServerFormat()));

            RuleFor(reg => reg.ExpiredDate).Must((reg, c) => DateTimeHelper.CheckDateTime(reg.ExpiredDate, reg.EffectiveDate))
                .WithMessage(string.Format(MessageResource.InvalidDate, UserResource.lblExpiredDate, UserResource.lblEffectiveDate));

            RuleFor(reg => reg.FirstName).NotEmpty()
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, UserResource.lblFirstName, "100"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, UserResource.lblFirstName));

            RuleFor(reg => reg.BirthDay).Must((reg, c) => DateTimeHelper.IsDateTime(reg.BirthDay))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, UserResource.lblBirthday,
                    Helpers.GetDateServerFormat()))
                .When(reg => !string.IsNullOrWhiteSpace(reg.BirthDay));

            RuleFor(reg => reg.BirthDay).Must(ValidateBirthdayIsInPast)
                .WithMessage(string.Format(MessageResource.msgDateMustBeInPast, UserResource.lblBirthday))
                .When(reg => !string.IsNullOrWhiteSpace(reg.BirthDay) && DateTimeHelper.IsDateTime(reg.BirthDay));

            RuleFor(reg => reg.Nationality)
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, UserResource.lblNationality, "100"));

            RuleFor(reg => reg.City)
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, UserResource.lblCity, "100"));

            RuleFor(reg => reg.Position)
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, UserResource.lblPosition, "100"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, UserResource.lblPosition));

            RuleFor(reg => reg.PostCode)
                .MaximumLength(20)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, UserResource.lblPostCode, "20"));

            RuleFor(reg => reg.HomePhone)
                .MaximumLength(20)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, UserResource.lblHomePhone, "20"));

            RuleFor(reg => reg.OfficePhone)
                .MaximumLength(20)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, UserResource.lblOfficePhone, "20"));
        }

        /// <summary>
        /// Validates that birthday is in the past (less than current date)
        /// </summary>
        /// <param name="birthday">The birthday string to validate</param>
        /// <returns>True if birthday is in the past, false otherwise</returns>
        private bool ValidateBirthdayIsInPast(string birthday)
        {
            if (string.IsNullOrWhiteSpace(birthday))
                return true; // Skip validation if birthday is empty

            if (DateTime.TryParse(birthday, out DateTime birthdayDate))
            {
                return birthdayDate < DateTime.Now;
            }

            return true; // Skip validation if date cannot be parsed
        }

        private int GetUserIdFromRoute()
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

    public class RegisterUserModelValidation : AbstractValidator<RegisterUserModel>
    {
        public RegisterUserModelValidation()
        {
            RuleFor(m => m.FirstName).NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, UserResource.lblFirstName));
            RuleFor(m => m.HomePhone).NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, UserResource.lblHomePhone));
        }
    }
}
