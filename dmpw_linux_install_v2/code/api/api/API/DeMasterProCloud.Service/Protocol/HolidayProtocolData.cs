using System;
using System.Collections.Generic;

namespace DeMasterProCloud.Service.Protocol
{
    /// <summary>
    /// Update holiday protocol data
    /// </summary>
    public class HolidayProtocolData : ProtocolData<HolidayProtocolDataHeader>
    {
    }

    public class HolidayProtocolDataHeader
    {
        public HolidayProtocolDataHeader()
        {
            Holidays = new List<HolidayProtocolDataDetail>();
        }

        public int Total { get; set; }
        public List<HolidayProtocolDataDetail> Holidays { get; set; }
    }

    public class HolidayProtocolDataDetail
    {
        public int HolidayType { get; set; }
        public int Recurring { get; set; }
        public List<Dates> HolidayDate { get; set; }
    }

    public class Dates
    {
        public string Date { get; set; }
    }

    /// <summary>
    /// Response from icu
    /// </summary>
    public class HolidayResponseProtocolData : ProtocolData<HolidayResponseProtocolDataHeader>
    {
    }

    public class HolidayResponseProtocolDataHeader
    {
    }

    /// <summary>
    ///Send load holiday protocol data
    /// </summary>
    public class SendLoadHolidayProtocolData : ProtocolData<SendLoadHolidayHeader>
    {
    }
    public class SendLoadHolidayHeader { }

    /// <summary>
    /// Load holiday protocol data
    /// </summary>
    public class LoadHolidayProtocolData : ProtocolData<LoadHolidayHeader>
    {
    }

    public class LoadHolidayHeader
    {
        public int Total { get; set; }
        public List<LoadHolidayDetail> Holidays { get; set; }
    }

    public class LoadHolidayDetail
    {
        public int HolidayType { get; set; }
        public int Recurring { get; set; }
        public string Date { get; set; }
    }

    public class SendHolidayProtocol : ProtocolData<SendHolidayHeader>
    {
    }

    public class SendHolidayHeader
    {
        public int Total { get; set; }
        public List<SendHolidayDetail> Holidays { get; set; }
    }

    public class SendHolidayDetail
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int HolidayType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Recursive { get; set; }
    }
}
