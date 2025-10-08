using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DeMasterProCloud.Common.Infrastructure;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.WorkingModel
{
    public class WorkingModel
    {
        public string Name { get; set; }
        public int CompanyId { get; set; }
        
        [Column(TypeName = "jsonb")]
        public string WorkingDay { get; set; }
        
        public bool IsDefault { get; set; }

        [Column(TypeName = "jsonb")]
        public string LunchTime { get; set; }
        public bool CheckLunchTime { get; set; }
        public bool CheckClockOut { get; set; }
        public bool UseClockOutDevice { get; set; }

        public int WorkingHourType { get; set; }

        public IEnumerable<EnumModel> WorkingHourTypeItems { get; set; }
    }


    public class WorkingListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        
        [Column(TypeName = "jsonb")]
        public string WorkingDay { get; set; }
        
        public bool IsDefault { get; set; }
    }
    public class WorkingTime
    {
        [JsonProperty("Name")]
        public string Name { get; set; }
        
        [JsonProperty("StartTimeWorking")]
        public string StartTimeWorking { get; set; }
        
        [JsonProperty("Start")]
        public string Start { get; set; }
        
        [JsonProperty("End")]
        public string End { get; set; }
        
        [JsonProperty("Type")]
        public string Type { get; set; }
    }

    public class LunchTime
    {
        [JsonProperty("Start")]
        public string Start { get; set; }

        [JsonProperty("End")]
        public string End { get; set; }
    }
}