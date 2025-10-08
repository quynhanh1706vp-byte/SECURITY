using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DeMasterProCloud.Common.Resources;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Timezone
{
    public class AccessTimeModel
    {
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]
        public int Position { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int CompanyId { get; set; }
        public List<DayDetail> Monday { get; set; }
        public List<DayDetail> Tuesday { get; set; }
        public List<DayDetail> Wednesday { get; set; }
        public List<DayDetail> Thursday { get; set; }
        public List<DayDetail> Friday { get; set; }
        public List<DayDetail> Saturday { get; set; }
        public List<DayDetail> Sunday { get; set; }
        public List<DayDetail> HolidayType1 { get; set; }
        public List<DayDetail> HolidayType2 { get; set; }
        public List<DayDetail> HolidayType3 { get; set; }
        public string Remarks { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public class AccessTimeListModel
    {
        public int Id { get; set; }
        public string AccessTimeName { get; set; }
        public string Remark { get; set; }
        public int Position { get; set; }
    }

    public class DayDetail
    {
        public int From { get; set; }
        public int To { get; set; }
    }

    public class AccessTimeInfoModel
    {
        public string MsgId { get; set; }
        public List<AccessTimeDetailModel> AccessTimes { get; set; }
        //public TimezoneDetailModel PassageTz { get; set; }
    }

    public class AccessTimeDetailModel
    {
        public string Name { get; set; }
        public int Position { get; set; }
        public string Type { get; set; }
        public AccessTimeIntervalTime Monday { get; set; }
        public AccessTimeIntervalTime Tuesday { get; set; }
        public AccessTimeIntervalTime Wednesday { get; set; }
        public AccessTimeIntervalTime Thursday { get; set; }
        public AccessTimeIntervalTime Friday { get; set; }
        public AccessTimeIntervalTime Saturday { get; set; }
        public AccessTimeIntervalTime Sunday { get; set; }
        public AccessTimeIntervalTime Holiday1 { get; set; }
        public AccessTimeIntervalTime Holiday2 { get; set; }
        public AccessTimeIntervalTime Holiday3 { get; set; }
    }

    public class AccessTimeIntervalTime
    {
        public string Interval1 { get; set; }
        public string Interval2 { get; set; }
        public string Interval3 { get; set; }
        public string Interval4 { get; set; }
    }

    public class AssignAccessTime
    {
        public List<DeviceAssignModel> DeviceList { get; set; }
        public int AccessTimeId { get; set; }
        public List<string> UserCodes { get; set; }
    }

    public class DeviceAssignModel
    {
        public int Id { get; set; }

        public string DeviceAddress { get; set; }
        public string DoorName { get; set; }
        
    }
}
