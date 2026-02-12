using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Vehicle;
using DeMasterProCloud.Service;
using FluentValidation;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    public class VehicleValidation : BaseValidation<VehicleModel>
    {
        /// <summary>
        /// vehicle validation.
        /// </summary>
        /// <param name="vehicleService"></param>
        public VehicleValidation(IVehicleService vehicleService)
        {
            RuleFor(reg => reg.PlateNumber)
                .NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, EventLogResource.lblPlateNumber))
                .MaximumLength(15)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, EventLogResource.lblPlateNumber, "15"))
                .Must(ContainsOnlySafeCharacters)
                .WithMessage(string.Format(MessageResource.msgInvalidInput + "Ex: 29B17788", EventLogResource.lblPlateNumber));
            
            RuleFor(reg => reg.Color)
                .MaximumLength(50)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, UserResource.lblColor, "50"))
                .Must(ContainsOnlySafeCharacters)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, UserResource.lblColor));
            
            RuleFor(reg => reg.Model)
                .MaximumLength(50)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, EventLogResource.lblVehicleModel, "50"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, EventLogResource.lblVehicleModel));
        }
    }
}