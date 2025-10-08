using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Api
{
    public class TimezoneLogResponseModel
    {
        public string IcuAddress { get; set; }
        public long Index { get; set; }
        public int Position { get; set; }
        public string Mon1 { get; set; }
        public string Mon2 { get; set; }
        public string Mon3 { get; set; }
        public string Tue1 { get; set; }
        public string Tue2 { get; set; }
        public string Tue3 { get; set; }
        public string Wed1 { get; set; }
        public string Wed2 { get; set; }
        public string Wed3 { get; set; }
        public string Thu1 { get; set; }
        public string Thu2 { get; set; }
        public string Thu3 { get; set; }
        public string Fri1 { get; set; }
        public string Fri2 { get; set; }
        public string Fri3 { get; set; }
        public string Sat1 { get; set; }
        public string Sat2 { get; set; }
        public string Sat3 { get; set; }
        public string Sun1 { get; set; }
        public string Sun2 { get; set; }
        public string Sun3 { get; set; }
        [JsonProperty(PropertyName = "hol_type1_time1")]
        public string HolType1Time1 { get; set; }
        [JsonProperty(PropertyName = "hol_type1_time2")]
        public string HolType1Time2 { get; set; }
        [JsonProperty(PropertyName = "hol_type1_time3")]
        public string HolType1Time3 { get; set; }
        [JsonProperty(PropertyName = "hol_type2_time1")]
        public string HolType2Time1 { get; set; }
        [JsonProperty(PropertyName = "hol_type2_time2")]
        public string HolType2Time2 { get; set; }
        [JsonProperty(PropertyName = "hol_type2_time3")]
        public string HolType2Time3 { get; set; }
        [JsonProperty(PropertyName = "hol_type3_time1")]
        public string HolType3Time1 { get; set; }
        [JsonProperty(PropertyName = "hol_type3_time2")]
        public string HolType3Time2 { get; set; }
        [JsonProperty(PropertyName = "hol_type3_time3")]
        public string HolType3Time3 { get; set; }
        [JsonProperty(PropertyName = "action_type")]
        public int ActionType { get; set; }
    }

    public class HolidayLogRequestModel
    {
        public string IcuAddress { get; set; }
        public long Index { get; set; }
    }

    public class HolidayLogResponseModel
    {
        public string IcuAddress { get; set; }
        public long Index { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int Type { get; set; }
        public bool Recurring { get; set; }
    }
}
