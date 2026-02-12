using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.AccessSchedule;
using DeMasterProCloud.Service;
using FluentValidation;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    public class AccessScheduleValidation : BaseValidation<AccessScheduleModel>
    {
        /// <summary>
        /// AccessSchedule validation
        /// </summary>
        /// <param name="accessScheduleService"></param>
        public AccessScheduleValidation(IAccessScheduleService accessScheduleService)
        {
            RuleFor(reg => reg.Content).NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, AccessScheduleResource.lblContent))
                .MaximumLength(200)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, AccessScheduleResource.lblContent, "200"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, AccessScheduleResource.lblContent));
        }
    }
}

