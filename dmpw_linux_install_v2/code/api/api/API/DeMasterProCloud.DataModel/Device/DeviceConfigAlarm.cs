using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Device
{
    public class DeviceConfigAlarm
    {
        
    }
    
    public class DeviceConfigAlarmModel
    {
        public bool Status { get; set; }
        public List<int> DeviceIds { get; set; }
    }
}