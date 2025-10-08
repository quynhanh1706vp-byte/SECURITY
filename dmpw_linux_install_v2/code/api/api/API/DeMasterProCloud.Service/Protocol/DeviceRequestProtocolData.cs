using DeMasterProCloud.DataModel.Setting;
using System.Collections.Generic;

namespace DeMasterProCloud.Service.Protocol
{
    /// <summary>
    /// Device instruction response
    /// </summary>
    public class DeviceRequestResponse : ProtocolData<DeviceDataRequestReponseDetail>
    {
    }

    public class DeviceDataRequestReponseDetail
    {
        public List<int> TransmitIds { get; set; }
    }

    public class RequestDataProtocolData : ProtocolData<RequestDataDetail>
    {
    }

    public class RequestDataDetail
    {
        public string Command { get; set; }
    }
}
