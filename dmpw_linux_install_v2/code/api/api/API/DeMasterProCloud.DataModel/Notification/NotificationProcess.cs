using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Notification
{
    public class NotificationProcess
    {
        public double Process { get; set; }
        public Dictionary<string, ReportResultProcess> Report { get; set; }
    }

    public class ReportResultProcess
    {
        public int Success { get; set; }
        public int Error { get; set; }
        public List<DataError> DataErrors { get; set; }
    }

    public class DataError
    {
        public string Avatar { get; set; }
        public string RecordsNameError { get; set; }
        public string MessageError { get; set; }
    }

    public class HanetSyncError
    {
        public int StatusUpdate { get; set; }
        public string Message { get; set; }
    }
}