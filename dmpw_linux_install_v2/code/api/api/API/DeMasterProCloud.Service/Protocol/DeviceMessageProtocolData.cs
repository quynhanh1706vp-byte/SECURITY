using System.Collections.Generic;


namespace DeMasterProCloud.Service.Protocol
{
    public class DeviceMessageProtocolData : ProtocolData<DeviceMessageProtocolDataHeader>
    {
    }

    public class DeviceMessageProtocolDataHeader
    {
        public DeviceMessageProtocolDataHeader()
        {
            Messages = new List<DeviceMessageProtocolDataDetail>();
        }
        public List<DeviceMessageProtocolDataDetail> Messages { get; set; }
    }

    public class DeviceMessageProtocolDataDetail
    {
        public string MessageId { get; set; }
        public string Content { get; set; }
    }

    /// <summary>
    /// Device Message response
    /// </summary>
    public class DeviceMessageResponseProtocolData : ProtocolData<DeviceMessageResponseDetail>
    {
    }

    public class DeviceMessageResponseDetail
    {
        public string Status { get; set; }
    }
}
