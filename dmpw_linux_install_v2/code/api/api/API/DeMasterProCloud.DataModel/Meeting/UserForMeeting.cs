using System.Collections.Generic;
using DeMasterProCloud.DataModel.Category;

namespace DeMasterProCloud.DataModel.Meeting;

public class UserForMeeting
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string UserCode { get; set; }
    public string DepartmentName { get; set; }
    public string WorkTypeName { get; set; }
}