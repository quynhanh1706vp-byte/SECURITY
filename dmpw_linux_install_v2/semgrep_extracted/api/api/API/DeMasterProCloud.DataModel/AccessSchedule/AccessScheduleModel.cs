using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.AccessSchedule
{
    public class AccessScheduleModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public List<int> DoorIds { get; set; }
        public int UserQuantity { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int? DepartmentId { get; set; }
        public List<int> UserIds { get; set; }
        public List<int> WorkShiftIds { get; set; }
    }
    public class AccessScheduleDetailModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string DoorIds { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int? DepartmentId { get; set; }
        public int UserQuantity { get; set; }


        public List<UserAccessScheduleModel> Users { get; set; }
        public List<WorkShiftAccessScheduleModel> WorkShifts { get; set; }
        
        public List<DoorModel> Doors { get; set; }
    }

    
    public class UserAccessScheduleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Avatar { get; set; }
        public string DepartmentName { get; set; }
    }
    public class AccessScheduleUserInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string DepartmentName { get; set; }
    }
    public class WorkShiftAccessScheduleModel
    {
        public int Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Name { get; set; }
    }

    public class DoorModel
    {
        public int Id { get; set; }
        public string DoorName { get; set; }
    }
 
    
    public class AccessScheduleListModel
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public int UserQuantity { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
       
    }


   
}