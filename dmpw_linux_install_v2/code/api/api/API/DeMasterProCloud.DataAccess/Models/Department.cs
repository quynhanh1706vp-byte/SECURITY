using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class Department
    {
        public Department()
        {
            InverseParent = new HashSet<Department>();
            User = new HashSet<User>();

            Vehicle = new HashSet<Vehicle>();
            AccessSchedule = new HashSet<AccessSchedule>();
        }

        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string DepartName { get; set; }
        public string DepartNo { get; set; }
        public int? ParentId { get; set; }
        //public short Status { get; set; }
        public int UpdatedBy { get; set; }
        public int MaxNumberCheckout { get; set; }
        public double MaxPercentCheckout { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }

        public Company Company { get; set; }
        public Department Parent { get; set; }
        public ICollection<Department> InverseParent { get; set; }
        public ICollection<User> User { get; set; }

        public int? DepartmentManagerId { get; set; }

        public Account DepartmentManager { get; set; }

        public ICollection<Vehicle> Vehicle { get; set; }

        public ICollection<DepartmentDevice> DepartmentDevice { get; set; }
        public ICollection<AccessSchedule> AccessSchedule { get; set; }
    }
}
