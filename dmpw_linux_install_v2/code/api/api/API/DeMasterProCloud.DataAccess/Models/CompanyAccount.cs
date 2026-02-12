using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class CompanyAccount
    {
        public int Id { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }

        public int PreferredSystem { get; set; }

        public Company Company { get; set; }
        [ForeignKey("Company")]
        public int? CompanyId { get; set; }

        public Account Account { get; set; }
        [ForeignKey("Account")]
        public int? AccountId { get; set; }

        public DynamicRole DynamicRole { get; set; }
        [ForeignKey("DynamicRole")]
        public int? DynamicRoleId { get; set; }
    }
}
    