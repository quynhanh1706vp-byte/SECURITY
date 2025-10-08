using System;

namespace DeMasterProCloud.DataAccess.Models;

public class UserAccessSchedule
{
    public int AccessScheduleId { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public AccessSchedule AccessSchedule { get; set; }
    public int CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime UpdatedOn { get; set; }
}