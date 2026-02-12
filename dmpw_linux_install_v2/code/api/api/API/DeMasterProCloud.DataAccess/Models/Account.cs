using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class Account
    {
        public Account()
        {
            SystemLog = new HashSet<SystemLog>();
            Department = new HashSet<Department>();
            User = new HashSet<User>();
            CompanyAccount = new HashSet<CompanyAccount>();
            IcuDevice = new HashSet<IcuDevice>();
            HeaderSetting = new HashSet<HeaderSetting>();
            DataListSetting = new HashSet<DataListSetting>();
        }

        public int Id { get; set; }
        public int? CompanyId { get; set; }
        public int? DynamicRoleId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Password { get; set; }
        public bool RootFlag { get; set; }
        //public short Status { get; set; }
        public short Type { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsDeleted { get; set; }
        public virtual Company Company { get; set; }
        //public string Remarks { get; set; }
        public string TimeZone { get; set; }
        public string Language { get; set; }
        public int PreferredSystem { get; set; }
        public ICollection<SystemLog> SystemLog { get; set; }
        public ICollection<Department> Department { get; set; }
        public string RefreshToken { get; set; }
        public DateTime CreateDateRefreshToken { get; set; }
        public string CurrentLoginInfo { get; set; }
        public DateTime UpdatePasswordOn { get; set; }
        public string LoginConfig { get; set; }
        public DynamicRole DynamicRole { get; set; }
        public string DeviceToken { get; set; }
        public ICollection<User> User { get; set; }
        public ICollection<CompanyAccount> CompanyAccount { get; set; }
        public ICollection<IcuDevice> IcuDevice { get; set; }

        public ICollection<HeaderSetting> HeaderSetting { get; set; }

        /// <summary>
        /// For data list setting (ex. Header)
        /// </summary>
        public ICollection<DataListSetting> DataListSetting { get; set; }
    }
}
