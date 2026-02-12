using System;
using System.Collections.Generic;
using System.Text;
using DeMasterProCloud.Common.Infrastructure;

namespace DeMasterProCloud.DataModel.MessageLog
{
    public class MessageLogModel
    {
        public string MsgId { get; set; }
        public string Topic { get; set; }
        public string PayLoad { get; set; }
        public string Status { get; set; }
        public string PublishTime { get; set; }
        public string ResponseTime { get; set; }
        public string ProtocolType { get; set; }
        public string GroupMsgId { get; set; }
    }

    public class MessageLogTime
    {
        public string FromPublishTime { get; set; }
        public string ToPublishTime { get; set; }
        public string FromResponseTime { get; set; }
        public string ToResponseTime { get; set; }
    }


    public class MessageDataModel
    {
        public IEnumerable<EnumModel> ProtocolTypes { get; set; }
        public IEnumerable<EnumModel> Topics { get; set; }
        public IEnumerable<EnumModel> Status { get; set; }
    }
}
