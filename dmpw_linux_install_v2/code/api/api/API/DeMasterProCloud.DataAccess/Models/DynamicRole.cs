using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class DynamicRole
    {
        public DynamicRole()
        {
            Account = new HashSet<Account>();
        }

        public int Id { get; set; }
        public int TypeId { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }
        public string PermissionList { get; set; }
        public bool EnableDepartmentLevel { get; set; }
        public bool RoleSettingDefault { get; set; }
        public string Description { get; set; }
   
        public DateTime UpdatedOn { get; set; }
        public Company Company { get; set; }

        public bool IsDeleted { get; set; }

        public ICollection<Account> Account { get; set; }
        public ICollection<CompanyAccount> CompanyAccount { get; set; }
    }
}
