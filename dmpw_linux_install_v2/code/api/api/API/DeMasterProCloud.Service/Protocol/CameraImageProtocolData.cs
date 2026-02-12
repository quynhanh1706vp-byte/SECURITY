namespace DeMasterProCloud.Service.Protocol
{
    public class CameraImageProtocolData : ProtocolData<CameraImageDetail>
    {
        
    }

    public class CameraImageDetail
    {
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public string Extension { get; set; }
        public string Type { get; set; }
        public string FileName { get; set; }
        public string Data { get; set; }
    }
}