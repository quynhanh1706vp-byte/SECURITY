namespace DeMasterProCloud.Service.Protocol
{
    public class LFaceRegisterProtocolData : ProtocolData<LFaceRegisterDetail>
    {
        
    }

    public class LFaceRegisterDetail
    {
        public int UserId { get; set; }
        public string Avatar { get; set; }
    }
    
    public class LFaceRegisterResponseProtocolData : ProtocolData<LFaceRegisterResponseDetail>
    {
        
    }

    public class LFaceRegisterResponseDetail
    {
        public string UserId { get; set; }
        public string CardId { get; set; }
        public string Message { get; set; }
    }
    
    public class TBFaceRegisterResponseProtocolData : ProtocolData<TBFaceRegisterResponseDetail>
    {
        
    }

    public class TBFaceRegisterResponseDetail
    {
        public string UserId { get; set; }
        public string CardId { get; set; }
        public string Message { get; set; }
    }
}