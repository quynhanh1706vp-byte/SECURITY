using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Attendance
{
    public class AttendanceSettingModel
    {
        public int Id { get; set; }
        public string ApprovarAccounts { get; set; }
        public int TimeFormatId { get; set; }
        public List<TimeformatTypeModel> TimeFormatList { get; set; }
        public bool EnableNotifyCheckinLate { get; set; }
    }

    public class AttendanceRegister
    {
        public int Type { get; set; }
        public DateTime StartD { get; set; }
        public DateTime EndD { get; set; }
        public string Reason { get; set; }
    }
    
    public class LeaveRequestModel
    {
        public int UserId { get; set; }
        public int Type { get; set; }
        public DateTime StartD { get; set; }
        public DateTime EndD { get; set; }
        public string Reason { get; set; }
    }

    public class AttendanceLeaveModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string DepartmentName { get; set; }
        public string Avatar { get; set; }
        public int Type { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Reason { get; set; }
        public string RejectReason { get; set; }
        public int Status { get; set; }
        public string Creator { get; set; }
    }

    public class ActionApproval
    {
        public bool IsAccept { get; set; }
        public string RejectReason { get; set; }
    }

    public class TimeformatTypeModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }
}