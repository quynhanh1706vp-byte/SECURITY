using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.DataModel.FirmwareVersion
{
    public class FirmwareVersionFilterModel : FilterModel
    {
        public string Version { get; set; }
        public List<int> DeviceTypes { get; set; }
    }
}
