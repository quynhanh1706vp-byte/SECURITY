using System;
using System.ComponentModel.DataAnnotations;
using DeMasterProCloud.Common.Resources;

namespace DeMasterProCloud.DataModel.Login
{
    public class ForgotPasswordModel
    {
        [Display(Name = nameof(AccountResource.lblEmail), ResourceType = typeof(AccountResource))]
        public string Email { get; set; }
    }
}
