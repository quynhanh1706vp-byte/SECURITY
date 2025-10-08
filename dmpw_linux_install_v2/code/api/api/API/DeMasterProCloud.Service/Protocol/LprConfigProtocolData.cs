using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.Service.Protocol
{
    public class LprConfigProtocolData : ProtocolData<LprConfigProtocolHeader>
    {
    }

    public class LprConfigProtocolHeader
    {
        public string DeviceAddress { get; set; }
    }
}
