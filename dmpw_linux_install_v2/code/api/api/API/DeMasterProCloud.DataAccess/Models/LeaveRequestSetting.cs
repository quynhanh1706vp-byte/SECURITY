namespace DeMasterProCloud.DataAccess.Models
{
    public class LeaveRequestSetting
    {
        public int Id { get; set; }
        public int NumberDayOffYear { get; set; } // Number of Day Off a year
        public int NumberDayOffPreviousYear { get; set; } // Days that allow to add day-off from previous year
        
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}