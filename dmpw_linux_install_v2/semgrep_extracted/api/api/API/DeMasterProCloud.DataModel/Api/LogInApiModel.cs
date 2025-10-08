using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Api
{
    /// <summary>
    /// Class for response user login info
    /// </summary>
    public class LoginResponseModel
    {
        [JsonProperty(PropertyName = "user_type")]
        public int UserType { get; set; }

        [JsonProperty(PropertyName = "time_sync")]
        public int SyncTimeFlag { get; set; }

        [JsonProperty(PropertyName = "utc_time")]
        public string UtcTime { get; set; }
    }
}