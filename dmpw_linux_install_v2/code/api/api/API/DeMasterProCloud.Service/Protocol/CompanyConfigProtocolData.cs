using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.Service.Protocol
{
    public class CompanyConfigProtocolData : ProtocolData<CompanyConfigProtocolHeader>
    {
    }

    public class CompanyConfigProtocolHeader
    {
        public int CardBit { get; set; }
    }
}
