using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using DeMasterProCloud.DataModel.User;

namespace DeMasterProCloud.DataModel.Attendance
{
    public class AttendanceModel
    {
        public int Type { get; set; }
        public DateTime ClockInD { get; set; }
        public DateTime ClockOutD { get; set; }
    }

    public class AttendanceListReportModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double Start { get; set; }      // Epoch
        public double End { get; set; }        // Epoch
        
        public double ClockIn { get; set; }      // Epoch
        public double ClockOut { get; set; }        // Epoch
        public int Type { get; set; }
        public int UserId { get; set; }
        public int DepartmentId { get; set; }
        public string UserName { get; set; }
        public double TotalWorkingTime { get; set; }
        [Column(TypeName = "jsonb")]
        public string WorkingTime { get; set; }
        public int? EditedBy { get; set; }
        public string EditorName { get; set; }
    }
    
    public class AttendanceListModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public double Start { get; set; }      // Epoch
        public double End { get; set; }        // Epoch
        
        public double ClockIn { get; set; }      // Epoch
        public double ClockOut { get; set; }        // Epoch
        public int Type { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public double TotalWorkingTime { get; set; }
        [Column(TypeName = "jsonb")]
        public string WorkingTime { get; set; }
        
        public DataAccess.Models.User User { get; set; }
        
    }

    public class SettingRecheckAttendance
    {
        public bool UpdateAttendanceRealTime { get; set; }
        public int TimeRecheckAttendance { get; set; } 
    }
    
    public class AttendanceReportMonthModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int TotalDaysNormal { get; set; }
        public int TotalDays { get; set; }
    }
}