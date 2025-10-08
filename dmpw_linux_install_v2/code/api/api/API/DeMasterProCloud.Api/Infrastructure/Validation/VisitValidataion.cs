using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using DeMasterProCloud.DataModel.Visit;
using DeMasterProCloud.Service;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// Validation for visitor
    /// </summary>
    public class VisitValidation : BaseValidation<VisitModel>
    {
        /// <summary>
        /// Validation for visitor
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="visitService"></param>
        public VisitValidation(IConfiguration configuration, IVisitService visitService)
        {
            RuleFor(reg => reg.VisitorName).NotEmpty().WithMessage(MessageResource.VisitorNameInValid);
            RuleFor(reg => reg.VisitorName).MaximumLength(100);
            RuleFor(reg => reg.VisitorName).Must(ContainsOnlySafeCharacters)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, VisitResource.lblVisitName))
                .When(reg => !string.IsNullOrWhiteSpace(reg.VisitorName));

            RuleFor(reg => reg.BirthDay).Must((reg, c) => DateTimeHelper.IsDateTime(reg.BirthDay, Constants.DateTimeFormat.DdMMYyyy))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, VisitResource.lblBirthDay, Constants.DateTimeFormat.DdMMYyyy));

            RuleFor(reg => reg.VisitorDepartment)
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, VisitResource.lblDepartment, "100"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, VisitResource.lblDepartment))
                .When(reg => !string.IsNullOrWhiteSpace(reg.VisitorDepartment));
            
            RuleFor(reg => reg.RoomNumber)
                .MaximumLength(50)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, VisitResource.lblRoomNumber, "50"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, VisitResource.lblRoomNumber))
                .When(reg => !string.IsNullOrWhiteSpace(reg.RoomNumber));
            
            RuleFor(reg => reg.DocumentNumber)
                .MaximumLength(50)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, VisitResource.lblDocumentNumber, "50"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, VisitResource.lblDocumentNumber))
                .When(reg => !string.IsNullOrWhiteSpace(reg.DocumentNumber));
            
            RuleFor(reg => reg.VisiteeDepartment)
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, VisitResource.lblVisiteeDepartment, "100"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, VisitResource.lblVisiteeDepartment))
                .When(reg => !string.IsNullOrWhiteSpace(reg.VisiteeDepartment));
            
            RuleFor(reg => reg.Email)
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, AccountResource.lblEmail, "100"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, AccountResource.lblEmail))
                .When(reg => !string.IsNullOrWhiteSpace(reg.Email));

            /*RuleFor(reg => reg.AccessGroupId).NotEmpty();*/

            RuleFor(reg => reg.StartDate).Must((reg, c) => DateTimeHelper.IsDateTime(reg.StartDate, Constants.DateTimeFormatDefault))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, VisitResource.lblStartDate, Constants.DateTimeFormatDefault));

            RuleFor(reg => reg.EndDate).Must((reg, c) => DateTimeHelper.IsDateTime(reg.EndDate, Constants.DateTimeFormatDefault))
                .WithMessage(string.Format(MessageResource.InvalidDateFormat, VisitResource.lblEndDate, Constants.DateTimeFormatDefault));

            // Validate startDate must be less than endDate
            RuleFor(reg => reg)
                .Must(ValidateStartDateBeforeEndDate)
                .WithMessage(MessageResource.msgStartLessEnd)
                .When(reg => !string.IsNullOrWhiteSpace(reg.StartDate) && !string.IsNullOrWhiteSpace(reg.EndDate));

            RuleFor(reg => reg.VisiteeId).Must((m) => !m.HasValue || m.Value == 0 || visitService.IsExistedVisitee(m.Value));
        }

        /// <summary>
        /// Validates that start date is before end date
        /// </summary>
        /// <param name="visit">The visit model to validate</param>
        /// <returns>True if start date is before end date, false otherwise</returns>
        private bool ValidateStartDateBeforeEndDate(VisitModel visit)
        {
            if (string.IsNullOrWhiteSpace(visit.StartDate) || string.IsNullOrWhiteSpace(visit.EndDate))
                return true; // Skip validation if either date is empty

            if (System.DateTime.TryParse(visit.StartDate, out System.DateTime startDate) &&
                System.DateTime.TryParse(visit.EndDate, out System.DateTime endDate))
            {
                return startDate < endDate;
            }

            return true; // Skip validation if dates cannot be parsed
        }

    }


    public class VisitOperationTimeValidation : AbstractValidator<VisitOperationTime>
    {
        public VisitOperationTimeValidation(IConfiguration configuration, IVisitService visitService)
        {
            RuleFor(reg => reg.OpeDateFrom).NotEmpty().WithMessage(string.Format(MessageResource.Required, SystemLogResource.lblOpeDateFrom));
            RuleFor(reg => reg.OpeDateTo).NotEmpty().WithMessage(string.Format(MessageResource.Required, SystemLogResource.lblOpeDateTo));
        }
    }
}
