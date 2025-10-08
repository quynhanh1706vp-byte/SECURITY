using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DeMasterProCloud.Service.Protocol
{
    public class MultipleServiceProtocolData
    {
        public string GroupMsgId { get; set; }
        public string PayLoad { get; set; }
        public string MsgId { get; set; }
        public string Topic { get; set; }
        public string ProcessId { get; set; }
        public string Username { get; set; }
        public DateTime PublishedTime { get; set; }
        public int Retry { get; set; }
        public int MessageIndex { get; set; }
        public int TotalMessages { get; set; }
        public string ActionType { get; set; }

        public string Sender { get; set; }
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }
    }
}
