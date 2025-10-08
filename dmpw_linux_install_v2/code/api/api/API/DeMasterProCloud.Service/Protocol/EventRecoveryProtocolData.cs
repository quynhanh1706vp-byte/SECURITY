using System;
using System.Collections.Generic;
using System.Text;
using DeMasterProCloud.DataModel.Device;

namespace DeMasterProCloud.Service.Protocol
{
    public class EventRecoveryProtocolData : ProtocolData<EventRecoveryProtocolDataHeader>
    {
    }

    public class EventRecoveryProtocolDataHeader
    {
        public string FromTime { get; set; }
        public string ToTime { get; set; }
        public int FrameIndex { get; set; }
        public string ProcessId { get; set; }
    }

    /// <summary>
    /// Receive event log from icu
    /// </summary>
    public class ReceiveEventRecoveryProtocolData : ProtocolData<ReceiveEventRecoveryHeaderData>
    {
    }

    public class ReceiveEventRecoveryHeaderData
    {
        public ReceiveEventRecoveryHeaderData()
        {
            Events = new List<ReceiveEventRecoveryEventDetailData>();
        }
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public string ProcessId { get; set; }
        public int Total { get; set; }

        public int UtcHour { get; set; }
        public int UtcMinute { get; set; }

        public List<ReceiveEventRecoveryEventDetailData> Events { get; set; }
    }

    public class ReceiveEventRecoveryEventDetailData
    {
        public string AccessTime { get; set; }
        public string CardId { get; set; }
        public int? IssueCount { get; set; }
        public string UserName { get; set; }
        public string UpdateTime { get; set; }
        public string InOut { get; set; }
        public int EventType { get; set; }
        public int IdType { get; set; }
        public double Temperature { get; set; }
    }
}
