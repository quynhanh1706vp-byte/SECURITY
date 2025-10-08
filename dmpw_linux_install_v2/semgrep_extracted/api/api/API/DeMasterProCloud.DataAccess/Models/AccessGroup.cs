using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataAccess.Models
{
    public class AccessGroup
    {
        public AccessGroup()
        {
            User = new HashSet<User>();
            AccessGroupDevice = new HashSet<AccessGroupDevice>();
            Visit = new HashSet<Visit>();
            Child = new HashSet<AccessGroup>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public int CompanyId { get; set; }
        public short Type { get; set; }
        public int? ParentId { get; set; }


        public AccessGroup Parent { get; set; }
        public ICollection<AccessGroup> Child { get; set; }

        public Company Company { get; set; }

        public ICollection<User> User { get; set; }
        public ICollection<AccessGroupDevice> AccessGroupDevice { get; set; }
        public ICollection<Visit> Visit { get; set; }
    }
}
