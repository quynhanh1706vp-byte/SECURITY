using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Device
{
    public class CameraFilterModel : FilterModel
    {
        public string Search { get; set; }
        public int CompanyId { get; set; }
        public List<int> Types { get; set; }
        public List<int> DeviceIds { get; set; }
    }
}