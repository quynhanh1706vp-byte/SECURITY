using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DeMasterProCloud.DataModel.Api
{
    public class DeviceRequestModel
    {
        [JsonProperty(PropertyName = "phone_id")]
        [Required]
        public string PhoneId { get; set; }
        [JsonProperty(PropertyName = "password")]
        [Required]
        public string Password { get; set; }
        [JsonProperty(PropertyName = "phone_name")]
        [Required]
        public string PhoneName { get; set; }

        [JsonProperty(PropertyName = "app_version")]
        [Required]
        public string AppVersion { get; set; }

        [JsonProperty(PropertyName = "serial_number")]
        [StringLength(50, MinimumLength = 1)]
        [Required]
        public string SerialNumber { get; set; }
        [JsonProperty(PropertyName = "company_id")]
        [Required]
        public int CompanyId { get; set; }

        [JsonProperty(PropertyName = "lat")]
        public string Lat { get; set; }

        [JsonProperty(PropertyName = "lng")]
        public string Lng { get; set; }
    }

    public class WgSettingRequestModel
    {
        [JsonProperty(PropertyName = "icu_address")]
        public string IcuAddress { get; set; }
        public long Index { get; set; }
    }

    public class WgSettingResponseModel
    {
        [JsonProperty(PropertyName = "icu_address")]
        public string IcuAddress { get; set; }
        public long Index { get; set; }
        [JsonProperty(PropertyName = "odd_parity_start_bit")]
        public int OddParityStartBit { get; set; }
        [JsonProperty(PropertyName = "odd_parity_bits_count")]
        public int OddParityBitsCount { get; set; }
        [JsonProperty(PropertyName = "event_parity_start_bit")]
        public int EventParityStartBit { get; set; }
        [JsonProperty(PropertyName = "event_parity_bits_count")]
        public int EventParityBitsCount { get; set; }
        [JsonProperty(PropertyName = "card_id_start_bit")]
        public int CardIdStartBit { get; set; }
        [JsonProperty(PropertyName = "card_id_bits_count")]
        public int CardIdBitsCount { get; set; }
        [JsonProperty(PropertyName = "facility_code_start_bit")]
        public int FacilityCodeStartBit { get; set; }
        [JsonProperty(PropertyName = "facility_code_bits_count")]
        public int FacilityCodeBitsCount { get; set; }
    }

    public class IcuAddressModel
    {
        [JsonProperty(PropertyName = "icu_address")]
        public string IcuAddress { get; set; }
    }
}
