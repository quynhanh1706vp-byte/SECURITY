using System;
using System.Collections.Generic;
using System.Text;

namespace DeMasterProCloud.DataModel.Notification
{
    public class Notification_
    {

    }

    public class NotificationData
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int TypeId { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int ReceiveId { get; set; }
        public bool Status { get; set; }
        public int CompanyId { get; set; }
        public string RelatedUrl { get; set; }
        public string TransType { get; set; }
    }

    public class NotificationUpdate
    {
        public bool Status { get; set; }
    }

    public class NotificationMapping
    {
        public string visitor_name { get; set; }
        public string visit_date { get; set; }
        public string status { get; set; }
        public string building { get; set; }
        public string userOpen { get; set; }
        public string IcuName { get; set; }
        public string DeviceAddress { get; set; }
        public string Command { get; set; }
        public string buildingName { get; set; }
        public string userRelease { get; set; }
        public string Type { get; set; }
    }

    public class NotificationNoticeModel
    {
        public string Content { get; set; }
        public List<int> UserIds { get; set; }
        
    }
}
