using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class DeviceReader
    {
            

        public int Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public int IcuDeviceId { get; set; }
        public IcuDevice IcuDevice { get; set; }
    }
}
