using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DeMasterProCloud.DataModel.Login
{
    /// <summary>
    /// Login Api Model class
    /// </summary>
    public class LoginApiModel
    {
        [JsonProperty(PropertyName = "company_code")]
        [Required]
        public string CompanyCode { get; set; }
        [JsonProperty(PropertyName = "username")]
        [Required]
        public string UserName { get; set; }

        [JsonProperty(PropertyName = "password")]
        [Required]
        public string Password { get; set; }
    }
}
