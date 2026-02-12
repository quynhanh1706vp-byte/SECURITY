using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.User
{
    public class AccessSettingModel
    {
        public string FirstApproverAccounts { get; set; }
        public string SecondApproverAccounts { get; set; }
        //public int? AccessTimeId { get; set; }
        //public string DefaultDoors { get; set; }
        //public List<GroupDeviceItem> GroupDevices { get; set; }
        public int ApprovalStepNumber{ get; set; }
        //public bool OutSide{ get; set; }
        //public bool AllowEmployeeInvite { get; set; }
        //public bool EnableCaptCha { get; set; }
        public bool EnableAutoApproval { get; set; }
        public bool AllowDeleteRecord { get; set; }
        public string AllLocationWarning { get; set; }
        public int DeviceIdCheckIn { get; set; }
        public Dictionary<string, bool> ListFieldsEnable { get; set; }
    }

    //public class GroupDeviceItem
    //{
    //    public string Name { get; set; }
    //    public List<int> DeviceIds { get; set; }
    //}
}