namespace DeMasterProCloud.DataAccess.Models
{
    public class AttendanceLeaveRequest
    {
        public int Id { get; set; }
        
        public int AttendanceId { get; set; }
        public Attendance Attendance { get; set; }
        
        public int AttendanceLeaveId { get; set; }
        public AttendanceLeave AttendanceLeave { get; set; }
    }
}