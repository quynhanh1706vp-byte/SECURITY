using FluentValidation;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.Service;
using DeMasterProCloud.DataModel.User;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// Device Validation
    /// </summary>
    public class DeviceValidation : BaseValidation<DeviceModel>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        /// <summary>
        /// Device Validation
        /// </summary>
        /// <param name="deviceService"></param>
        public DeviceValidation(IDeviceService deviceService, IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            RuleFor(reg => reg.DeviceAddress).NotEmpty()
                .WithMessage(string.Format(MessageResource.MessageCanNotBlank, DeviceResource.lblDeviceAddress))
                .MaximumLength(20)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, DeviceResource.lblDeviceAddress, "20"))
                .Must((reg, c) => !deviceService.HasExistName(GetAccessGroupIdFromRoute(), reg.DeviceAddress))
                .WithMessage(DeviceResource.msgDeviceAddressInUse)
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, DeviceResource.lblDeviceAddress));

            RuleFor(reg => reg.DoorName).NotEmpty()
                .MaximumLength(100)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, DeviceResource.lblDoorName, "100"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, VisitResource.lblDoorName));

            //RuleFor(reg => reg.ActiveTimezoneId).NotEmpty();

            RuleFor(reg => reg.VerifyMode).NotNull()
                .Must((reg, d) => deviceService.IsAbleDeviceVerify(reg))
                .WithMessage(DeviceResource.msgCannotUseVerifyMode);

            RuleFor(reg => reg.MPRCount).NotEmpty().InclusiveBetween(1, 10)
                .WithMessage(string.Format(MessageResource.RangeFromTo, DeviceResource.lblCondition));

            RuleFor(reg => reg.MPRInterval).InclusiveBetween(1, 180)
                .WithMessage(string.Format(MessageResource.RangeFromToInterval, DeviceResource.lblMprInterval));

            //RuleFor(reg => reg.IpAddress).NotEmpty()/*.When(reg => reg.NetworkEnable)*/
            //    .Must((reg, strings) => Helpers.IsIpAddress(reg.IpAddress))
            //    .WithMessage(reg => string.Format(MessageResource.InvalidIpAddress, reg.IpAddress))
            //    /*.When(reg => reg.NetworkEnable)*/;

            RuleFor(reg => reg.LockOpenDuration).NotNull().InclusiveBetween(1, 254);

            RuleFor(reg => reg.SensorDuration).InclusiveBetween(1, 254)
                //.Must((reg, c) => (reg.StatusDelay >= reg.LockOpenDuration))
                //.WithMessage(string.Format(MessageResource.GreaterThanOrEqual, DeviceResource.lblStatusDelay, DeviceResource.lblLockOpenDuration))
                .When(reg => reg.Alarm == true);
            //.When(reg => reg.SensorType != (short)SensorType.None);

            //RuleFor(reg => reg.CloseReverseLock).Must((reg, c) => reg.CloseReverseLock)
            //    .WithMessage(string.Format(MessageResource.Required, DeviceResource.lblCloseReverseLock))
            //    .When(reg => reg.SensorType != (short)SensorType.None);

            // [Edward] 2020.03.04
            // Add validation to prevent the different card readers have same role.
            RuleFor(reg => reg.RoleReader1)
                .Must((reg, c) => reg.RoleReader0 != reg.RoleReader1)
                .WithMessage(string.Format(DeviceResource.msgDuplicateRole))
                .When(reg => reg.UseCardReader == 0 || reg.DeviceType == (short)DeviceType.Icu300N || reg.DeviceType == (short)DeviceType.Icu300NX || reg.DeviceType == (short)DeviceType.Icu400);
        }
        private int GetAccessGroupIdFromRoute()
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

    public class ConfigLocalMqttModelValidation : BaseValidation<ConfigLocalMqttModel>
    {
        public ConfigLocalMqttModelValidation()
        {
            RuleFor(reg => reg.LocalMqtt.Host)
                .MaximumLength(19)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, "Host", "20"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, "Host"));
            RuleFor(reg => reg.LocalMqtt.UserName)
                .MaximumLength(9)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, "UserName","10"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, "UserName"));
            RuleFor(reg => reg.LocalMqtt.Password)
                .MaximumLength(9)
                .WithMessage(string.Format(MessageResource.LengthNotGreaterThan, "Password", "10"))
                .Must(NotContainsCode)
                .WithMessage(string.Format(MessageResource.msgInvalidInput, "Password"));
        }
    }
}
