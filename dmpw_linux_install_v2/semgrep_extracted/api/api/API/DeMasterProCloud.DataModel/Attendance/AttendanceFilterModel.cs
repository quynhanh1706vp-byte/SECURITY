using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Attendance
{
    // public class LeaveAttendanceFilterModel : FilterModel
    // {
    //     public string Search { get; set; }
    //     public string AttendanceType { get; set; }
    //     public DateTime Start { get; set; }
    //     public DateTime End { get; set; }
    // }

    public class LeaveReportFilterModel : FilterModel
    {
        public string Search { get; set; }
        public List<int> DepartmentIds { get; set; }
        public string Year { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string AttendanceType { get; set; }
    }
}