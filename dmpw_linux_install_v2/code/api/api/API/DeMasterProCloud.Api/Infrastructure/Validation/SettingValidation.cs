using FluentValidation;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.Service;

namespace DeMasterProCloud.Api.Infrastructure.Validation
{
    /// <summary>
    /// Setting validation
    /// </summary>
    public class SettingValidation : AbstractValidator<SettingModel>
    {
        /// <summary>
        /// Setting validation
        /// </summary>
        /// <param name="settingService"></param>
        public SettingValidation(ISettingService settingService)
        {
            RuleFor(reg => reg.Value).NotEmpty();
            //RuleFor(reg => reg.Key).NotEmpty().MaximumLength(50);
            //RuleFor(reg => reg.Key)
            //    .Must((reg, c) => !settingService.IsKeyExist(reg))
            //    .WithMessage(string.Format(MessageResource.Exist, SettingResource.lblKey));
            //RuleFor(reg => reg.Key).MaximumLength(50);
            //RuleFor(reg => reg.Category).NotEmpty().MaximumLength(255);
        }
    }
}
