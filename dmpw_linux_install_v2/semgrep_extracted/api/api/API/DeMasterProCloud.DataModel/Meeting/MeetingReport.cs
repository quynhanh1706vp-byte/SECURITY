using System;
using System.Collections.Generic;
using DeMasterProCloud.DataModel.EventLog;

namespace DeMasterProCloud.DataModel.Meeting
{
    public class MeetingReport
    {
        public int Total { get; set; }
        public int TotalAbsence { get; set; }
        public int TotalAttendance { get; set; }
        public List<RecordMeetingReport> Users { get; set; }
    }

    public class RecordMeetingReport
    {
        public string Name { get; set; }
        public string FirstTimeAccess { get; set; }
        public bool IsAttendance { get; set; }
    }
}