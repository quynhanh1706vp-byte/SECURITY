
using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class User
    {
        public User()
        {
            // These are the DB tables those are using UserId value as foreign key
            EventLog = new HashSet<EventLog>();
            Card = new HashSet<Card>();
            Face = new HashSet<Face>();
            Vehicle = new HashSet<Vehicle>();
          
            UserAccessSchedule = new HashSet<UserAccessSchedule>();
        }

        public int Id { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
        public string UserCode { get; set; }
        public string City { get; set; }

        public int CompanyId { get; set; }
        public int? AccountId { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        public int DepartmentId { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public string EmpNumber { get; set; }
        public string FirstName { get; set; }
        public string HomePhone { get; set; }
        public DateTime? IssuedDate { get; set; }
        public string Job { get; set; }
        public string KeyPadPw { get; set; }
        public string LastName { get; set; }
        public string Nationality { get; set; }
        public string OfficePhone { get; set; }
        public string Position { get; set; }
        public string PostCode { get; set; }
        public string Remarks { get; set; }
        public string Responsibility { get; set; }
        public string Rfu { get; set; }
        public bool Sex { get; set; }
        public int AccessGroupId { get; set; }
        public bool IsMasterCard { get; set; }
        public string Email { get; set; }
        public short PassType { get; set; }
        public short? WorkType { get; set; }
        public short PermissionType { get; set; }

        public bool IsSystemUseApply { get; set; }
        public string SystemUseApplyReason { get; set; }
        public string SystemUsePassword { get; set; }
        public bool IsSystemUseApproval { get; set; }
        public string SystemAuth { get; set; }
        public bool IsAccountLock { get; set; }
        public DateTime? SystemUseApplyDate { get; set; }
        public string Grade { get; set; }
        public string NationalIdNumber { get; set; }

        /// <summary>
        /// First approver's company account Id 
        /// </summary>
        public int ApproverId1 { get; set; }

        /// <summary>
        /// Second approver's company account Id 
        /// </summary>
        public int ApproverId2 { get; set; }

        /// <summary>
        /// Status of approval
        /// </summary>
        public int ApprovalStatus { get; set; }

        public short Status { get; set; }
        public DateTime? BirthDay { get; set; }
        public bool IsDeleted { get; set; }

        public int? WorkingTypeId { get; set; }
        public string AliasDataInfo { get; set; }

        public WorkingType WorkingType { get; set; }
        public Company Company { get; set; }
        public Department Department { get; set; }
        public Account Account { get; set; }
        public ICollection<EventLog> EventLog { get; set; }
        public AccessGroup AccessGroup { get; set; }
        public NationalIdCard? NationalIdCard { get; set; }
        public ICollection<Card> Card { get; set; }
        public ICollection<Attendance> Attendance { get; set; }
        public ICollection<AttendanceLeave> AttendanceLeave { get; set; }
        public ICollection<Face> Face { get; set; }
        public ICollection<Vehicle> Vehicle { get; set; }

        public ICollection<UserAccessSchedule> UserAccessSchedule { get; set; }
    }
}
