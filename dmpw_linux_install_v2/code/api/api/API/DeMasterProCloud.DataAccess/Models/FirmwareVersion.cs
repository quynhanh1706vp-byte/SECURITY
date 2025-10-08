using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.DataAccess.Models
{
    public class FirmwareVersion
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public string FileName { get; set; }
        public string LinkFile { get; set; }
        public short DeviceType { get; set; }
        public string Note { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
    }
}
