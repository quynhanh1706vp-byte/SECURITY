using System;
using System.Collections.Generic;
using System.Text;

namespace DeMasterProCloud.DataAccess.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string Content { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool Status { get; set; }
        public int CompanyId { get; set; }
        public int ReceiveId { get; set; }
        public string ResourceName { get; set; }
        public string ResourceParam { get; set; }
        public string RelatedUrl { get; set; }
    }
}
