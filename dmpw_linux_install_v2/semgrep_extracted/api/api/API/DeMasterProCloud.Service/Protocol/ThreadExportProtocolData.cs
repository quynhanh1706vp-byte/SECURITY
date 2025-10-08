using DeMasterProCloud.DataModel.EventLog;

namespace DeMasterProCloud.Service.Protocol
{
    public class ThreadExportProtocolData : ProtocolData<ExportToFileDetailData>
    {
        
    }

    public class ExportToFileDetailData
    {
        public string ExportAccount { get; set; }
        public string ExportType { get; set; }
        public EventLogExportFilterModel Filter { get; set; }
    }
}