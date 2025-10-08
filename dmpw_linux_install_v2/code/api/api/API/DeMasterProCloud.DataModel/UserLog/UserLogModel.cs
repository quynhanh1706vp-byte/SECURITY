using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DeMasterProCloud.DataModel.Api;

namespace DeMasterProCloud.DataModel.UserLog
{
    //public class UserLogProtocolModel : ProtocolData
    //{
    //    public int Total { get; set; }
    //    public List<UserLogProtocolDetailModel> Data { get; set; }
    //}
    /// <summary>
    /// UserLog model
    /// </summary>
    public class UserLogProtocolDetailModel
    {
        [JsonProperty(PropertyName = "icu_address")]
        public string IcuAddress { get; set; }
        [JsonProperty(PropertyName = "index")]
        public long Index { get; set; }

        [JsonProperty(PropertyName = "position")]
        public int Position { get; set; }

        [JsonProperty(PropertyName = "card_id")]
        public string CardId { get; set; }
        [JsonProperty(PropertyName = "effective_date")]
        public string EffectiveDate { get; set; }

        [JsonProperty(PropertyName = "expired_date")]
        public string ExpiredDate { get; set; }

        [JsonProperty(PropertyName = "timezone_pos")]
        public int TimezonePos { get; set; }

        [JsonProperty(PropertyName = "mpr_pos")]
        public int MprPos { get; set; }

        [JsonProperty(PropertyName = "key_pad_pw")]
        public string KeyPadPw { get; set; }
        [JsonProperty(PropertyName = "action_type")]
        public int ActionType { get; set; }
    }


    public class UserLogModel
    {
        public int IcuId { get; set; }
        public int Position { get; set; }
        public int TzPosition { get; set; }
        public short MprPos { get; set; }
    }
}
