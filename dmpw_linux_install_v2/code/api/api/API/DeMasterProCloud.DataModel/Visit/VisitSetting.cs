using System.Collections.Generic;
using DeMasterProCloud.DataModel.Account;

namespace DeMasterProCloud.DataModel.Visit
{
    public class VisitSettingModel
    {
        public string FirstApproverAccounts { get; set; }
        public string SecondsApproverAccounts { get; set; }
        public int? AccessTimeId { get; set; }
        public List<int> DefaultDoors { get; set; }
        public int AccessGroupId { get; set; }
        public List<GroupDeviceItem> GroupDevices { get; set; }
        public int ApprovalStepNumber{ get; set; }
        public bool OutSide{ get; set; }
        public bool AllowEmployeeInvite { get; set; }
        public bool EnableCaptCha { get; set; }
        public bool EnableAutoApproval { get; set; }
        public bool InsiderAutoApproved { get; set; }
        public bool AllowDeleteRecord { get; set; }
        public bool AllowEditRecord { get; set; }
        public string AllLocationWarning { get; set; }
        public int DeviceIdCheckIn { get; set; }
        public Dictionary<string, bool> ListFieldsEnable { get; set; }
        public List<string> FieldRequired { get; set; }
        public bool AllowGetUserTarget { get; set; }
        public string PersonalInvitationLink { get; set; }
        public bool AllowSelectDoorWhenCreateNew { get; set; }
        public bool AllowSendKakao { get; set; }
        public string ListVisitPurpose { get; set; }
        public bool OnlyAccessSingleBuilding { get; set; }
    }

    public class ApprovedModel
    {
        public bool Approved { get; set; }
        public string Reason { get; set; }
    }
    
    public class RejectedModel
    {
        public string Reason { get; set; }
    }
    
    public class TakeBackCardModel
    {
        public string CardId { get; set; }
        public string Reason { get; set; }
    }

    public class GroupDeviceItem
    {
        public string Name { get; set; }
        public List<int> DeviceIds { get; set; }
    }

    public class VisitSettingInitModel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int? AccessTimeId { get; set; }
        public string FirstApproverAccounts { get; set; }
        public string SecondsApproverAccounts { get; set; }
        public string VisitCheckManagerAccounts { get; set; }
        public List<int> DefaultDoors { get; set; }
        public int AccessGroupId { get; set; }
        public string GroupDevices { get; set; }
        public int ApprovalStepNumber{ get; set; }
        public bool OutSide { get; set; } = false;
        public bool AllowEmployeeInvite { get; set; }
        public bool EnableCaptCha { get; set; }
        public bool EnableAutoApproval { get; set; }
        public bool InsiderAutoApproved { get; set; }
        public bool AllowDeleteRecord { get; set; }
        public bool AllowEditRecord { get; set; }
        public string AllLocationWarning { get; set; }
        public int DeviceIdCheckIn { get; set; }
        public string ListFieldsEnable { get; set; }
        public string VisibleFields { get; set; }
        public string PersonalInvitationLink { get; set; }

        public bool OnlyAccessSingleBuilding { get; set; }
        
        public List<AccountListModel> FirstApprovers { get; set; }
        public List<AccountListModel> SecondApprovers { get; set; }
        public List<AccountListModel> VisitCheckManagers { get; set; }
        public List<string> FieldRegisterLeft { get; set; }
        public List<string> FieldRegisterRight { get; set; }
        public List<string> FieldRequired { get; set; }
        public bool AllowGetUserTarget { get; set; }
        public bool AllowSelectDoorWhenCreateNew { get; set; }
        public bool AllowSendKakao { get; set; }
        public string ListVisitPurpose { get; set; }
    }

    public class FieldLayoutRegister
    {
        public List<string> FieldRegisterLeft { get; set; }
        public List<string> FieldRegisterRight { get; set; }
    }
}