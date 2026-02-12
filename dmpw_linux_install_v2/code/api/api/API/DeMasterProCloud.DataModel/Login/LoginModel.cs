using System.ComponentModel.DataAnnotations;
using DeMasterProCloud.Common.Resources;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Login
{
    public class LoginModel
    {
        [Display(Name = nameof(AccountResource.lblUsername), ResourceType = typeof(AccountResource))]
        public string Username { get; set; }
        [Display(Name = nameof(AccountResource.lblPassword), ResourceType = typeof(AccountResource))]
        public string Password { get; set; }
        [JsonIgnore]
        [Display(Name = nameof(AccountResource.lblRemeberMe), ResourceType = typeof(AccountResource))]
        public bool Remember { get; set; }
        [JsonIgnore]
        public string ReturnUrl { get; set; }
        public bool EnableRemoveOldSession { get; set; }

    }

    public class LoginModelWithCompany
    {
        public int CompanyId { get; set; }
        public string TemporaryToken { get; set; }
    }

    public class FirebaseLoginModel
    {
        public string IdToken { get; set; }
    }
}
