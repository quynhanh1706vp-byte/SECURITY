using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Api
{
    public class EventLogDetailRequestModel
    {
        public long Index { get; set; }
        [JsonProperty(PropertyName = "card_id")]
        public string CardId { get; set; }

        [JsonProperty(PropertyName = "event_time")]
        public string EventTime { get; set; }

        [Required]
        public short Status { get; set; }
        [JsonProperty(PropertyName = "in_out")]
        [Required]
        public string InOut { get; set; }

        [Required]
        public short Type { get; set; }
        public string Password { get; set; }
    }
}
