using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class Visit
    {
        public Visit()
        {
            EventLog = new HashSet<EventLog>();
            Card = new HashSet<Card>();
            Vehicle = new HashSet<Vehicle>();
        }

        public int Id { get; set; }
        public DateTime? ApplyDate { get; set; }
        public short Status { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }

        public string VisitorName { get; set; }
        public string VisitType { get; set; }
        public DateTime BirthDay { get; set; }
        public string VisitorDepartment { get; set; }
        public string VisitorEmpNumber { get; set; }
        public string Position { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
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
        public string Address { get; set; }
        public bool IsDecision { get; set; }
        public short VisitingCardState { get; set; }
        public int ApproverId1 { get; set; }
        public int ApproverId2 { get; set; }
        public string CardId { get; set; }
        public int IssueCount { get; set; }
        public short CardStatus { get; set; }
        public int AccessGroupId { get; set; }

        public string RejectReason { get; set; }
        public DateTime ApprovDate1 { get; set; }
        public DateTime ApprovDate2 { get; set; }
        public string RejectorId { get; set; }
        public DateTime RejectDate { get; set; }
        
        
        public string UserCode { get; set; }
        public DateTime IssuedDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public int IssuerId { get; set; }
        public int ReclaimerId { get; set; }
        
        public string Avatar { get; set; }
        public string ImageCardIdFont { get; set; }
        public string ImageCardIdBack { get; set; }
        public int IdentificationType { get; set; }
        public string IdentificationTypeName { get; set; }
        public string NationalIdNumber { get; set; }
        
        public string AllowedBelonging { get; set; }
        public bool Gender { get; set; }
        public string PlaceIssueIdNumber { get; set; }
        public DateTime DateIssueIdNumber { get; set; }
        
        public string Email { get; set; }
        
        // Id for partner
        public string AliasId { get; set; }

        public string VisitPlace { get; set; }
        public string UnitName { get; set; }

        /// <summary>
        /// This value is for group management when you apply for multiple visitors at once.
        /// </summary>
        public string GroupId { get; set; }

        public string RoomNumber { get; set; }
        public string RoomDoorCode { get; set; }

        public string DocumentLabel { get; set; }
        public string DocumentNumber { get; set; }
        public int DocumentType { get; set; }

        public Company Company { get; set; }
        public AccessGroup AccessGroup { get; set; }
        public NationalIdCard? NationalIdCard { get; set; }
        public ICollection<Card> Card { get; set; }
        public ICollection<EventLog> EventLog { get; set; }
        public ICollection<Vehicle> Vehicle { get; set; }

    }
}
