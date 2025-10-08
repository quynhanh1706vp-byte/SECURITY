using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.Service;
using FluentValidation;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    public class CameraValidation : BaseValidation<CameraModel>
    {
        /// <summary>
        /// camera validation.
        /// </summary>
        /// <param name="cameraService"></param>
        public CameraValidation(ICameraService cameraService)
        {
            RuleFor(reg => reg.CameraId)
                .NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, CameraResource.lblCameraId))
                .MaximumLength(50)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, CameraResource.lblCameraId, "50"))
                .Must(ContainsOnlySafeCharacters)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, CameraResource.lblCameraId));
            
            RuleFor(reg => reg.Name)
                .NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, CameraResource.lblCameraName))
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, CameraResource.lblCameraName, "100"))
                .Must(ContainsOnlySafeCharacters)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, CameraResource.lblCameraName));
            
            RuleFor(reg => reg.VideoLength)
                .InclusiveBetween(1, 9999)
                .WithMessage(string.Format(MessageResource.MustBeGreaterThanZero, CameraResource.lblVideoLength, 0, 9999));
        }
    }
}