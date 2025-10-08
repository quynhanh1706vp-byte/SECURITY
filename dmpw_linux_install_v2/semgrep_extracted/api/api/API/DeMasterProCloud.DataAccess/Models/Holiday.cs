using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class Holiday
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime EndDate { get; set; }
        public string Name { get; set; }
        public bool Recursive { get; set; }
        public string Remarks { get; set; }
        public DateTime StartDate { get; set; }
        //public short Status { get; set; }
        public int Type { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }

        public Company Company { get; set; }
    }
}
