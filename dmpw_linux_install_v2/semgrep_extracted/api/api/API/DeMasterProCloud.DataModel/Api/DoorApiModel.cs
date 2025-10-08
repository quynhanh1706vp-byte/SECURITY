using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Api
{
    public class DoorListResponseModel
    {
        [JsonProperty(PropertyName = "door_id")]
        public int DoorId { get; set; }
        [JsonProperty(PropertyName = "door_name")]
        public string DoorName { get; set; }
        [JsonProperty(PropertyName = "icu_address")]
        public string IcuAddress { get; set; }
        [JsonProperty(PropertyName = "last_sync_time")]
        public long LastSyncTime { get; set; }
        [JsonProperty(PropertyName = "is_enabled")]
        public int IsEnabled { get; set; }
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }
    }

    public class DoorSettingRequestModel
    {
        [Required]
        [JsonProperty(PropertyName = "icu_address")]
        public string IcuAddress { get; set; }
        [Required]
        [JsonProperty(PropertyName = "index")]
        public long Index { get; set; }
    }

    public class OpenDoorRequestModel
    {
        [Required]
        [JsonProperty(PropertyName = "icu_address")]
        public string IcuAddress { get; set; }
        [Required]
        [JsonProperty(PropertyName = "date_time")]
        public string DateTime { get; set; }
    }

    public class OpenDoorResponseModel
    {
        [JsonProperty(PropertyName = "encrypted_data")]
        public string EncryptedData { get; set; }
    }

    public class DoorSettingResponseModel
    {
        [JsonProperty(PropertyName = "icu_address")]
        public string IcuAddress { get; set; }
        [JsonProperty(PropertyName = "index")]
        public long Index { get; set; }
        [JsonProperty(PropertyName = "buzzer")]
        public int Buzzer { get; set; }
        [JsonProperty(PropertyName = "pass_back_rule")]
        public short PassbackRule { get; set; }
        [JsonProperty(PropertyName = "inout_set")]
        public short InOutSet { get; set; }
        [JsonProperty(PropertyName = "hard_pass_back")]
        public int HardPassback { get; set; }
        [JsonProperty(PropertyName = "verify_mode")]
        public short VerifyMode { get; set; }
        [JsonProperty(PropertyName = "status_delay")]
        public int StatusDelay { get; set; }
        [JsonProperty(PropertyName = "active_timezone")]
        public int ActiveTimezone { get; set; }
        [JsonProperty(PropertyName = "passage_timezone")]
        public int? PassageTimezone { get; set; }
        [JsonProperty(PropertyName = "open_duration")]
        public int? OpenDuration { get; set; }
        [JsonProperty(PropertyName = "sensor_type")]
        public short SensorType { get; set; }
        [JsonProperty(PropertyName = "close_reverse_lock")]
        public int CloseReverseLock { get; set; }
        [JsonProperty(PropertyName = "led")]
        public short Led { get; set; }
        [JsonProperty(PropertyName = "condition")]
        public int Condition { get; set; }
        [JsonProperty(PropertyName = "duration")]
        public int Duration { get; set; }
        [JsonProperty(PropertyName = "mpr_enable")]
        public int MprEnable { get; set; }
        [JsonProperty(PropertyName = "status")]
        public int Status { get; set; }
    }
}
