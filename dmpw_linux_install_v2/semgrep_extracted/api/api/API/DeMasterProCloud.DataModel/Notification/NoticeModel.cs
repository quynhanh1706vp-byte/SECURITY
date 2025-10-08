using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Notification
{
    public class NoticeModel
    {
        public int TotalVisitRequest { get; set; }
        public int TotalAccessRequest { get; set; }
        public List<NotificationData> Notices { get; set; }
    }
}