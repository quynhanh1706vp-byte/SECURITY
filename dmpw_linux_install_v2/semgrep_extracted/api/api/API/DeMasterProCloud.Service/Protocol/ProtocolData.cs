using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DeMasterProCloud.Service.Protocol
{
    public class ProtocolData<T>
    {
        public string MsgId { get; set; }
        public string Sender { get; set; }
        public string Type { get; set; }

        public T Data { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
        }
    }

    public class BasicProtocolData : ProtocolData<BasicResponseHeader>
    {
    }

    public class BasicResponseHeader
    {
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public int Total { get; set; }
    }
}
