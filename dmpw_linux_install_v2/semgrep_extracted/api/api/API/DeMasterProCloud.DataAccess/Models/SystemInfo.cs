using System;

namespace DeMasterProCloud.DataAccess.Models
{
    public class SystemInfo
    {
        public int Id { get; set; }
        public string LicenseInfo { get; set; }
        public string SecretCode { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}