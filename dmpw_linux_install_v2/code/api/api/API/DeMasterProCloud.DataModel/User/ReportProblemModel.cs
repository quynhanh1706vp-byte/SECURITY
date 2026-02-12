using System;

namespace DeMasterProCloud.DataModel.User
{
    public class ReportProblemModel
    {
        public string Email { get; set; }
        public string CameraId { get; set; }
        public string DeviceAddress { get; set; }
        public string Detail { get; set; }
        public DateTime Time { get; set; }
        public int Type { get; set; }
    }
    
    public class ReportProblemListModel
    {
        public string Email { get; set; }
        public string CameraId { get; set; }
        public string CameraName { get; set; }
        public string DeviceAddress { get; set; }
        public string DeviceName { get; set; }
        public string Detail { get; set; }
        public DateTime Time { get; set; }
        public string TypeDescription { get; set; }
        public int Type { get; set; }
    }
}