using DeMasterProCloud.DataModel.EventLog;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using System.Collections.Generic;

namespace DeMasterProCloud.Service.Protocol
{
    public class ThreadRefreshProtocolData : ProtocolData<RefreshDetailData>
    {

    }

    public class RefreshDetailData
    {
        public List<int> DeviceIds { get; set; }
    }
}