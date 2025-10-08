using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Api
{
    public class CompanyApiModel
    {
        public CompanyApiModel()
        {
            Companies = new List<CompanyApiDetailModel>();
        }
        [Required]
        public List<CompanyApiDetailModel> Companies { get; set; }
    }

    public class CompanyApiDetailModel
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        [JsonProperty(PropertyName = "is_first")]
        public int IsFirst { get; set; }
    }

    public class CompanyLoginApiModel
    {
        [JsonProperty(PropertyName = "phone_id")]
        [Required]
        public string PhoneId { get; set; }

        [JsonProperty(PropertyName = "password")]
        [Required]
        public string Password { get; set; }
        [JsonProperty(PropertyName = "serial_number")]
        [Required]
        public string SerialNumber { get; set; }
        [JsonProperty(PropertyName = "phone_name")]
        public string PhoneName { get; set; }
        [JsonProperty(PropertyName = "company_id")]
        [Required]
        public int CompanyId { get; set; }
    }
}
