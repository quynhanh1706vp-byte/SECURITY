using System.Collections.Generic;

namespace DeMasterProCloud.Service.Protocol
{
    public class AccessGroupProtocolData: ProtocolData<AccessGroupDetail>
    {
    }

    public class AccessGroupDetail
    {
        public AccessGroupDetail()
        {
            AccessGroups = new List<AccessGroupDataDetail>();
        }
    
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public int Total { get; set; }
        public List<AccessGroupDataDetail> AccessGroups { get; set; }
    }

    public class AccessGroupDataDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AccessGroupResponseProtocolData : ProtocolData<AccessGroupResponseDetail>
    {
    }

    public class AccessGroupResponseDetail
    {
        public string Status { get; set; }
        public int StatusCode { get; set; }
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public int Total { get; set; }
    }
}