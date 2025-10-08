namespace DeMasterProCloud.Service.Protocol
{
    public class TsFaceRegisterProtocolData : ProtocolData<TsFaceRegisterDetail>
    {
        
    }

    public class TsFaceRegisterDetail
    {
        public int UserId { get; set; }
        public string Avatar { get; set; }
    }
    
    public class TsFaceRegisterResponseProtocolData : ProtocolData<TsFaceRegisterResponseDetail>
    {
        
    }

    public class TsFaceRegisterResponseDetail
    {
        public string UserId { get; set; }
        public string CardId { get; set; }
        public string Message { get; set; }
    }
}