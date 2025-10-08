using System.Collections.Generic;

namespace DeMasterProCloud.Service.Protocol
{
    public class DeviceInfoProtocolData : ProtocolData<DeviceInfoProtocolDataHeader>
    {
    }

    public class DeviceInfoProtocolDataHeader
    {
        public DeviceInfoProtocolDataHeader() { }
    }

    public class DeviceInfoResponse : ProtocolData<DeviceInfoResponseDetail>
    {
    }

    public class DeviceInfoResponseDetail
    {
        public string DeviceAddress { get; set; }
        public string MacAddress { get; set; }

        public string Version { get; set; }
        public string Reader0Version { get; set; }
        public string Reader1Version { get; set; }
        public string NfcModuleVersion { get; set; }
        public Dictionary<string, string> ExtraVersion { get; set; }

        public string IpAddress { get; set; }
        public string Subnet { get; set; }
        public string Gateway { get; set; }
        public string ServerIp { get; set; }
        public string ServerPort { get; set; }
        public string DeviceTime { get; set; }
        public int EventCount { get; set; }
        public int UserCount { get; set; }
        public int EventNotTransCount { get; set; }
        public int OperationMode { get; set; }
        public List<IntegratedDevice> IntegratedDevices { get; set; }
    }

    public class IntegratedDevice
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public int Role { get; set; }
    }


    public class ControllerInfoResponse : ProtocolData<ControllerInfoResponseDetail>
    {

    }

    public class ControllerInfoResponseDetail
    {
        public List<DeviceInfoResponseDetail> Readers { get; set; }
    }
}
