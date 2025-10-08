using System.Collections.Generic;
using DeMasterProCloud.DataModel.User;

namespace DeMasterProCloud.Service.Protocol
{
    public class NotificationProtocolData : ProtocolData<NotificationProtocolDataDetail>
    {
    }

    public class NotificationProtocolDataDetail
    {
        public string MessageType { get; set; }
        public string NotificationType { get; set; }
        public string User { get; set; }
        public string Message { get; set; }

        public string RelatedUrl { get; set; }

        public bool Keep { get; set; }
    }

    public class AddUserToDeviceProtocolData : ProtocolData<AddUserToDeviceProtocolDetail>
    {
    }
    public class AddUserToDeviceProtocolDetail
    {
        public int FrameIndex { get; set; }
        public int TotalIndex { get; set; }
        public int Total { get; set; }
        public List<UserEbknModel> Users { get; set; }
    }


    public class NotificationVisitorProtocolData : ProtocolData<NotificationVisitorProtocolDataDetail>
    {
    }

    public class NotificationVisitorProtocolDataDetail
    {
        public string Type { get; set; }
        public string Content { get; set; }
        public string CreatedOn { get; set; }
        public string ReceiveId { get; set; }
        public string CompanyId { get; set; }
        public string Status { get; set; }
        public string NotificationType { get; set; }
    }
    
    public class NotificationToUserProtocolData : ProtocolData<NotificationToUserProtocolDataDetail>
    {
    }

    public class NotificationToUserProtocolDataDetail
    {
        public string Type { get; set; }
        public string Content { get; set; }
        public string CreatedOn { get; set; }
        public string ReceiveId { get; set; }
        public string CompanyId { get; set; }
        public string Status { get; set; }
        public string NotificationType { get; set; }
    }
}
