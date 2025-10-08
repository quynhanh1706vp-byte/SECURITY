using System;

namespace DeMasterProCloud.DataAccess.Models
{
    public class UnregistedDevice
    {
        public int Id { get; set; }
        public string DeviceAddress { get; set; }
        public short Status { get; set; }
        public string IpAddress { get; set; }
        public string DeviceType { get; set; }
        public int? CompanyId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string MacAddress { get; set; }

        public int OperationType { get; set; }

        public int RegisterType { get; set; }

        public Company Company { get; set; }
    }
}
