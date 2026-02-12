using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Api
{
    public class EmergencyRequestModel
    {
        [JsonProperty(PropertyName = "password")]
        [Required]
        public string Password { get; set; }
    }
}
