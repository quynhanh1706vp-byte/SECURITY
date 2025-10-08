using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataAccess.Models
{
    public class AttendanceLeave
    {
        public AttendanceLeave()
        {
            AttendanceLeaveRequest = new HashSet<AttendanceLeaveRequest>();
        }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public int EditedBy { get; set; }
        public int CompanyId { get; set; }
        public int Type { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Reason { get; set; }
        public string RejectReason { get; set; }
        public int Status { get; set; }
        public int CreatedBy { get; set; }

        public virtual User User { get; set; }
        public virtual Company Company { get; set; }
        public ICollection<AttendanceLeaveRequest> AttendanceLeaveRequest { get; set; }
    }
}