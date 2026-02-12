using System;
using System.Collections.Generic;
using System.Text;
using DeMasterProCloud.DataModel.Device;

namespace DeMasterProCloud.Service.Protocol
{
    public class EventCountProtocolData : ProtocolData<EventCountProtocolDataHeader>
    {
    }
    
    public class EventCountProtocolDataHeader
    {
        public string ProcessId { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
    }

    public class EventCountResponse : ProtocolData<EventCountResponseDetail>
    {
    }

    public class EventCountResponseDetail
    {
        public string ProcessId { get; set; }
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Send event log to webapp
    /// </summary>
    public class SendEventCountModelData : ProtocolData<SendEventCountDetailData>
    {
    }

    //public class SendEventCountHeaderData
    //{
    //    //public SendEventCountDetailData Data { get; set; }
    //}

    public class SendEventCountDetailData
    {
        public string ProcessId { get; set; }
        public int DeviceId { get; set; }
        public int DeviceEventCount { get; set; }
        public int DbEventCount { get; set; }
        public string FromTime { get; set; }
        
    }
}
