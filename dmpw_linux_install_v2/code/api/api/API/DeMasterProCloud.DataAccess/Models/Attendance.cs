using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataAccess.Models
{
    public class Attendance
    {
        public Attendance()
        {
            AttendanceLeaveRequest = new HashSet<AttendanceLeaveRequest>();
        }
        
        public int Id { get; set; }
        public DateTime Date { get; set; }
        //public double Start { get; set; }      // Epoch
        //public double End { get; set; }        // Epoch
        //public double ClockIn { get; set; }      // Epoch
        //public double ClockOut { get; set; }        // Epoch
        
        public DateTime StartD { get; set; }      // Epoch
        public DateTime EndD { get; set; }        // Epoch
        public DateTime ClockInD { get; set; }      // Epoch
        public DateTime ClockOutD { get; set; }

        public string TimeZone { get; set; }

        public double TotalWorkingTime { get; set; }

        public int Type { get; set; }
        public int UserId { get; set; }
        
        public int CompanyId { get; set; }
        
        public virtual User User { get; set; }
        
        public virtual Company Company { get; set; }
        
        [Column(TypeName = "jsonb")]
        public string WorkingTime { get; set; }
        
        public int? EditedBy { get; set; }
        
        public ICollection<AttendanceLeaveRequest> AttendanceLeaveRequest { get; set; }
    }
}