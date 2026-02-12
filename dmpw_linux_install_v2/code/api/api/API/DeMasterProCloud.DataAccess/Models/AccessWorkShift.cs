using System;

namespace DeMasterProCloud.DataAccess.Models;

public class AccessWorkShift
{
    public int WorkShiftId { get; set; }
    public int AccessScheduleId { get; set; }
    public AccessSchedule AccessSchedule { get; set; }
    public WorkShift WorkShift { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
}