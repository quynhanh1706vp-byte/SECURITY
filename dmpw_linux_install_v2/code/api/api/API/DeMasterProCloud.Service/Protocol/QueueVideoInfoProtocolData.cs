using DeMasterProCloud.DataModel.EventLog;

namespace DeMasterProCloud.Service.Protocol
{
    public class QueueVideoInfoProtocolData : ProtocolData<QueueVideoInfo>
    {
        
    }
    
    public class QueueVideoInfo
    {
        public string PathInput { get; set; }
        public string PathOutput { get; set; }
        public int DeviceId { get; set; }
    }

    public class FfmpegConfig
    {
        public int CpuUsage { get; set; }
        public int Crf { get; set; }
    }
}