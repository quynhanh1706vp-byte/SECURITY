namespace DeMasterProCloud.Service.Protocol
{
    public class BioFaceResponseProtocolData: ProtocolData<BioFaceResponseProtocolDetail>
    {
    
    }
    public class BioFaceResponseProtocolDetail
    {
        public bool Status { get; set; }
        public string UserId { get; set; }
    }
}