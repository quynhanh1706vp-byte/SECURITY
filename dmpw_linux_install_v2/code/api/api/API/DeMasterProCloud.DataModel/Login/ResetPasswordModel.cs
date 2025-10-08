using System.ComponentModel.DataAnnotations;
using DeMasterProCloud.Common.Resources;

namespace DeMasterProCloud.DataModel.Login
{
    public class ResetPasswordModel
    {
        [Display(Name = nameof(AccountResource.lblNewPassword), ResourceType = typeof(AccountResource))]
        public string NewPassword { get; set; }
        [Display(Name = nameof(AccountResource.lblConfirmNewPassword), ResourceType = typeof(AccountResource))]
        public string ConfirmNewPassword { get; set; }
        [Display(Name = nameof(AccountResource.lblToken), ResourceType = typeof(AccountResource))]
        public string Token { get; set; }
    }
}
