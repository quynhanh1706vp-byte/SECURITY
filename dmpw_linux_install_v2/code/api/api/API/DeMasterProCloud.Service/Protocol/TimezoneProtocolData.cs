using System.Collections.Generic;

namespace DeMasterProCloud.Service.Protocol
{
    /// <summary>
    /// Update timezone config send to icu
    /// </summary>
    public class UpdateTimezoneProtocolData : ProtocolData<UpdateTimezoneProtocolHeaderData>
    {
    }

    public class UpdateTimezoneProtocolHeaderData
    {
        public UpdateTimezoneProtocolHeaderData()
        {
            Timezone = new List<UpdateTimezoneProtocolDetailData>();
        }
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public int Total { get; set; }
        public List<UpdateTimezoneProtocolDetailData> Timezone { get; set; }
    }

    public class UpdateTimezoneProtocolDetailData
    {
        public int TimezonePosition { get; set; }
        public int ScheduleCount { get; set; }
        public IntervalTime Monday { get; set; }
        public IntervalTime Tuesday { get; set; }
        public IntervalTime Wednesday { get; set; }
        public IntervalTime Thursday { get; set; }
        public IntervalTime Friday { get; set; }
        public IntervalTime Saturday { get; set; }
        public IntervalTime Sunday { get; set; }
        public IntervalTime Holiday1 { get; set; }
        public IntervalTime Holiday2 { get; set; }
        public IntervalTime Holiday3 { get; set; }
    }

    public class IntervalTime
    {
        public string Interval1 { get; set; }
        public string Interval2 { get; set; }
        public string Interval3 { get; set; }
        public string Interval4 { get; set; }
    }

    /// <summary>
    /// Delete timezone sending config to ICU
    /// </summary>
    public class TimezoneProtocolData : ProtocolData<TimezoneProtocolHeaderData>
    {
    }

    public class TimezoneProtocolHeaderData
    {
        public TimezoneProtocolHeaderData()
        {
            Timezone = new List<TimezoneProtocolDetailData>();
        }
        public int Total { get; set; }
        public List<TimezoneProtocolDetailData> Timezone { get; set; }
    }

    public class TimezoneProtocolDetailData
    {
        public int TimezonePosition { get; set; }

    }

    /// <summary>
    /// Send timezone to icu
    /// </summary>
    public class LoadTimezoneProtocolData : ProtocolData<LoadTimezoneProtocolHeaderData>
    {
    }

    public class LoadTimezoneProtocolHeaderData
    {
        public LoadTimezoneProtocolHeaderData()
        {
            Timezones = new List<LoadTimezoneProtocolDetailData>();
        }
        public int Total { get; set; }
        public List<LoadTimezoneProtocolDetailData> Timezones { get; set; }
    }

    public class LoadTimezoneProtocolDetailData
    {
        //public string Type { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public IntervalTime Monday { get; set; }
        public IntervalTime Tuesday { get; set; }
        public IntervalTime Wednesday { get; set; }
        public IntervalTime Thursday { get; set; }
        public IntervalTime Friday { get; set; }
        public IntervalTime Saturday { get; set; }
        public IntervalTime Sunday { get; set; }
        public IntervalTime Holiday1 { get; set; }
        public IntervalTime Holiday2 { get; set; }
        public IntervalTime Holiday3 { get; set; }
    }
}
