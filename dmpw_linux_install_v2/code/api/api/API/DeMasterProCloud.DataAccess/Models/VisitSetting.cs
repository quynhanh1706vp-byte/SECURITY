using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class VisitSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int? AccessTimeId { get; set; }
        [Column(TypeName = "jsonb")]
        public string FirstApproverAccounts { get; set; }
        public string SecondsApproverAccounts { get; set; }
        public string VisitCheckManagerAccounts { get; set; }
        [Column(TypeName = "jsonb")]
        public string DefaultDoors { get; set; }
        public int AccessGroupId { get; set; }
        [Column(TypeName = "jsonb")]
        public string GroupDevices { get; set; }
        public int ApprovalStepNumber{ get; set; }
        public bool OutSide { get; set; } = false;
        public bool AllowEmployeeInvite { get; set; }
        public bool EnableCaptCha { get; set; }
        public bool EnableAutoApproval { get; set; }
        public bool AllowDeleteRecord { get; set; }
        public bool AllowEditRecord { get; set; }
        public string AllLocationWarning { get; set; }
        public int DeviceIdCheckIn { get; set; }
        public string ListFieldsEnable { get; set; }
        public string VisibleFields { get; set; }
        public string FieldRegisterLeft { get; set; }
        public string FieldRegisterRight { get; set; }
        public string FieldRequired { get; set; }
        public bool AllowGetUserTarget { get; set; }
        public string PersonalInvitationLink { get; set; }
        public bool AllowSelectDoorWhenCreateNew { get; set; }
        public bool AllowSendKakao { get; set; }
        public bool InsiderAutoApproved { get; set; }
        public bool OnlyAccessSingleBuilding { get; set; }
        public string ListVisitPurpose { get; set; }

        public Company Company { get; set; }
        public AccessTime AccessTime { get; set; }
    }
}