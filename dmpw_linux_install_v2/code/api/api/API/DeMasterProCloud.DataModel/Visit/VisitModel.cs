using System;
using System.Collections.Generic;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel.Card;
using DeMasterProCloud.DataModel.User;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Visit
{
    public class VisitModel
    {
        public int Id { get; set; }
        public string VisitorName { get; set; }
        public int VisitType { get; set; }
        public string BirthDay { get; set; }
        public string VisitorDepartment { get; set; }
        public string VisitorEmpNumber { get; set; }
        public string Position { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string VisiteeSite { get; set; }
        public string VisitReason { get; set; }
        public int? VisiteeId { get; set; }
        public string VisiteeName { get; set; }
        public int? VisiteeDepartmentId { get; set; }
        public string VisiteeDepartment { get; set; }
        public string VisiteeEmpNumber { get; set; }
        public int? LeaderId { get; set; }
        public string LeaderName { get; set; }
        public string Phone { get; set; }
        public string InvitePhone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public bool IsDecision { get; set; }

        public short CardStatus { get; set; }
        
        public int ApproverId1 { get; set; }
        public int ApproverId2 { get; set; }
        public int ApproverId { get; set; }

        public string CardId { get; set; }
        public int AccessGroupId { get; set; }
        public string ProcessStatus { get; set; }
        
        public string Avatar { get; set; }
        public string ImageCardIdFont { get; set; }
        public string ImageCardIdBack { get; set; }
        public int IdentificationType { get; set; }
        public string IdentificationTypeName { get; set; }
        public string NationalIdNumber { get; set; }
        public string DynamicQrCode { get; set; }

        public string GReCaptchaResponse { get; set; }
        public string Doors { get; set; }
        public string Floors { get; set; }
        public List<SymptomDetail> Covid19 { get; set; }
        public string BuildingName { get; set; }
        public string BuildingAddress { get; set; }
        // time start date, end date UTC
        // public DateTime StartTime { get; set; }
        // public DateTime EndTime { get; set; }
        public int StatusCode { get; set; }
        public string AllowedBelonging { get; set; }
        public string VisiteeAvatar { get; set; }
        public bool Gender { get; set; }
        public string PlaceIssueIdNumber { get; set; }
        public DateTime DateIssueIdNumber { get; set; }
        public string UnitName { get; set; }
        public int CreatedBy { get; set; }

        /// <summary>
        /// I don't know what is the difference with VisiteeSite.
        /// But there is a column 'VisitPlace' in DB, So I added below variable to add data to DB.
        /// </summary>
        public string VisitPlace { get; set; }
        public int SizeRotateAvatar { get; set; }
        public string RoomNumber { get; set; }
        public string RoomDoorCode { get; set; }
        public string VisiteePhone { get; set; }
        public string VisiteeEmail { get; set; }

        public string DocumentLabel { get; set; }
        public string DocumentNumber { get; set; }
        public int DocumentType { get; set; }
        
        // for mobiapp
        public List<CardModel> CardList { get; set; }
        public NationalIdCardModel NationalIdCard { get; set; }
    }

    public class VisitDataModel : VisitModel
    {
        public IEnumerable<EnumModel> VisitTypes { get; set; }
        public IEnumerable<EnumModel> ListCardStatus { get; set; }
        public IEnumerable<EnumModel> ListIdentificationType { get; set; }
        //public List<AccessGroupModelForUser> AccessGroups { get; set; }
        //public IEnumerable<EnumModel> ListVisitPreregisterCardStatus { get; set; }
    }

    public class VisitListModel
    {
        public int Id { get; set; }
        public string ApplyDate { get; set; }
        public string VisitorName { get; set; }
        public string BirthDay { get; set; }
        public string VisitorDepartment { get; set; }
        public string Position { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int VisiteeId { get; set; }
        public string VisiteeSite { get; set; }
        public string VisitReason { get; set; }
        public string VisiteeName { get; set; }
        public string Phone { get; set; }
        public string InvitePhone { get; set; }

        public string ProcessStatus { get; set; }
        public string Approver1 { get; set; }
        public string Approver2 { get; set; }
        public string RejectReason { get; set; }
        public string CardId { get; set; }
        public int AccessGroupId { get; set; }
        public bool IsDecision { get; set; }
        public string Avatar { get; set; }
        public string VisiteeAvatar { get; set; }
        public int IdentificationType { get; set; }
        public string IdentificationTypeName { get; set; }
        public string NationalIdNumber { get; set; }
        public double BodyTemperature { get; set; }
        public string AllowedBelonging { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        // time start date, end date UTC
        // public DateTime StartTime { get; set; }
        // public DateTime EndTime { get; set; }
        public string Doors { get; set; }
        public int StatusCode { get; set; }

        public int VisitType { get; set; }
        public string VisitPlace { get; set; }
        public string CreatedName { get; set; }
        public int CreatedBy { get; set; }

        public bool AutoApproved { get; set; }
        public string RoomNumber { get; set; }
        public string RoomDoorCode { get; set; }
        public string Address { get; set; }
        public bool Sex { get; set; }
        public string DocumentLabel { get; set; }
        public string DocumentNumber { get; set; }
        public int DocumentType { get; set; }
    }

    public class VisitReportModel
    {
        public int Id { get; set; }
        public int VisitId { get; set; }
        public string VisiteeName { get; set; }
        public string VisiteeDepartment { get; set; }
        public string ApproverName1 { get; set; }
        public string ApproverName2 { get; set; }
        public int ApproverId1 { get; set; }
        public int ApproverId2 { get; set; }
        public string AccessTime { get; set; }
        public string UserName { get; set; }
        public string BirthDay { get; set; }
        public string Department { get; set; }
        public string CardId { get; set; }
        public string Device { get; set; }
        public string DoorName { get; set; }
        public string Building { get; set; }
        public string InOut { get; set; }
        public string EventDetail { get; set; }
        public int IssueCount { get; set; }
        public string CardStatus { get; set; }
        public string CardType { get; set; }
        public string Avatar { get; set; }
        public string ImageCamera { get; set; }
        public string OtherCardId { get; set; }
        public string ResultCheckIn { get; set; }
        public double BodyTemperature { get; set; }
        public double DelayOpenDoorByCamera { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class VisitOperationTime
    {
        public string OpeDateFrom { get; set; }
        public string OpeDateTo { get; set; }
    }

    public class VisitViewModel
    {
        public string OpeDateFrom { get; set; }
        public string OpeDateTo { get; set; }
        public string OpeTimeFrom { get; set; }
        public string OpeTimeTo { get; set; }
        public string VisitorName { get; set; }
        public string BirthDay { get; set; }
        public string VisitorDepartment { get; set; }
        public string Position { get; set; }
        public string VisiteeSite { get; set; }
        public string VisitReason { get; set; }
        public string VisiteeName { get; set; }
        public string Phone{ get; set; }
        public string InvitePhone { get; set; }

        public string ApproverName1 { get; set; }
        public string ApproverName2 { get; set; }
        public string RejectReason { get; set; }
        public string CardId { get; set; }
        public IEnumerable<EnumModel> ProcessStatus { get; set; }
        public IEnumerable<EnumModel> VisitArmyTypes { get; set; }
    }


    public class VisitReportViewModel
    {
        public string AccessDateFrom { get; set; }
        public string AccessDateTo { get; set; }
        public string AccessTimeFrom { get; set; }
        public string AccessTimeTo { get; set; }

        public string UserName { get; set; }
        public string BirthDay { get; set; }
        public string Department { get; set; }
        public string CardId { get; set; }
        public string Device { get; set; }
        public string DoorName { get; set; }
        public string Building { get; set; }
        public string InOutType { get; set; }
        public string EventDetails { get; set; }
        public int IssueCount { get; set; }
        private string CardStatus { get; set; }


        //public IEnumerable<SelectListItem> CardTypeList { get; set; }
        
        public IEnumerable<EnumModel> InOutList { get; set; }
        public IEnumerable<EnumModel> EventTypeList { get; set; }
        public IEnumerable<SelectListItemModel> DoorList { get; set; }
        public IEnumerable<EnumModel> ListCardStatus { get; set; }
        public IEnumerable<SelectListItemModel> BuildingList { get; set; }
        public IEnumerable<SelectListItemModel> DepartmentList { get; set; }
        public IEnumerable<EnumModel> IdentificationType { get; set; }
    }

    public class VisitPreRegisterModel
    {
        [JsonIgnore] public int Id { get; set; }
        public string VisitorName { get; set; }
        public string VisitorEmpNumber { get; set; }
        public string BirthDay { get; set; }
        public string VisitorDepartment { get; set; }
        
        public string Position { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string VisiteeSite { get; set; }
        public string VisitReason { get; set; }
        public string VisiteeName { get; set; }
        public string VisiteeDepartment { get; set; }
        

        public string Phone { get; set; }
        public string InvitePhone { get; set; }

        public string Address { get; set; }

        

        public short CardStatus { get; set; }

        public IEnumerable<EnumModel> ListCardStatus { get; set; }
    }

    public class GetBackVisit
    {
        public List<int> VisitIds { get; set; }
        public string CardId { get; set; }
        public string Reason { get; set; }
    }

    public class VisitCardReturnModel
    {
        public int VisitId { get; set; }
        public string CardId { get; set; }
        public string Reason { get; set; }
    }


    public class VisitTargetRegister
    {
        public int TargetId { get; set; }
        public string TargetName { get; set; }
        public string TargetDepartment { get; set; }
        public string SiteKey { get; set; }
        public string Language { get; set; }
        public string Logo { get; set; }
        public string CompanyName { get; set; }
        public string CompanyCode { get; set; }
        public List<SymptomDetail> Symptoms { get; set; }
        public string ListFieldsEnable { get; set; }
        public string LabelListField { get; set; }
        public string BodyTemplate { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public string Background { get; set; }
        public List<string> FieldRegisterLeft { get; set; }
        public List<string> FieldRegisterRight { get; set; }
        public List<string> FieldRequired { get; set; }
        public bool AllowGetUserTarget { get; set; }
        public bool IsEnableVideoDoorBell { get; set; }
        public string RidDefault { get; set; }
        public string TimeZone { get; set; }
        public List<string> ListVisitPurpose { get; set; }
        public List<EnumModel> Departments { get; set; }
    }

    public class VisitEmailInvite
    {
        public string Link { get; set; }
        public List<string> Emails { get; set; }
    }

    public class VisitQRCodeModel
    {
        public int Id { get; set; }
        public string VisitorName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string InvitePhone { get; set; }
        public string Address { get; set; }
        public string CompanyName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string QRCode { get; set; }
        public string VisiteeName { get; set; }
        public string VisiteeDepartment { get; set; }
        public string Avatar { get; set; }
        public string Language { get; set; }
    }


    public class VisitorCardModel
    {
        public int Id { get; set; }
        public string CardId { get; set; }
        public int IssueCount { get; set; }
        public string VisitorName { get; set; }
        public int VisitId { get; set; }
    }

    public class ImportMultiVisitModel
    {
        public string Barcode { get; set; }
        public string Name { get; set; }
        public List<string> AccessGroupNames { get; set; }
        public string ValidDateFrom { get; set; }
        public string ValidDateTo { get; set; }
        public string Action { get; set; }
    }
}