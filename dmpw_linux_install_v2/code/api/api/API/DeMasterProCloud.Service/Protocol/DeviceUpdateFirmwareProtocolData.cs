using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.Service.Protocol
{
    public class DeviceUpdateFirmwareProtocolData : ProtocolData<DeviceUpdateFirmware>
    {
    }

    public class DeviceUpdateFirmware
    {
        public string Version { get; set; }
        public string FileName { get; set; }
        public string LinkDownLoad { get; set; }
    }
}
