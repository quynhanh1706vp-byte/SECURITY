using DeMasterProCloud.DataModel.User;
using System;

namespace DeMasterProCloud.DataModel.Attendance
{
    public class AttendanceLeaveReportModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int TotalOverTime { get; set; }
        public int TotalBusinessTrip { get; set; }
        public int TotalVacation { get; set; }
        public int TotalSickness { get; set; }
        public int TotalRemote { get; set; }
        public int TotalLateIn { get; set; }
        public int TotalEarlyOut { get; set; }
        public int TotalOffDutyBreak { get; set; }
    }


    public class AttendanceLeaveDataModel
    {
        public int Id { get; set; }

        public UserListModel User { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public int Type { get; set; }

        public string Reason { get; set; }
    }

    public class AttendanceLeaveDetailDataModel : AttendanceLeaveDataModel
    {
        public string TypeName { get; set; }
    }
}