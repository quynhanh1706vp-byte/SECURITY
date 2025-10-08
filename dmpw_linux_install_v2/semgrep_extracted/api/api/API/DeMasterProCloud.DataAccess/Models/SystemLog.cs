using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class SystemLog
    {
        public long Id { get; set; }
        public int Action { get; set; }
        public int? CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public string Content { get; set; }
        public string ContentDetails { get; set; }
        public string ContentIds { get; set; }
        public int CreatedBy { get; set; }
        public DateTime OpeTime { get; set; }
        public int Type { get; set; }

        public virtual Company Company { get; set; }
        public Account CreatedByNavigation { get; set; }
    }
}
