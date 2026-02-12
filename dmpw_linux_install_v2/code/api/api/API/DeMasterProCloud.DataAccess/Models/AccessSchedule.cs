using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataAccess.Models
{
    public class AccessSchedule
    {
       
        public AccessSchedule()
        {
            UserAccessSchedule = new HashSet<UserAccessSchedule>();
            AccessWorkShift = new HashSet<AccessWorkShift>();
        }
        
        public int Id { get; set; }
        public string Content { get; set; }
        public string DoorIds { get; set; }
        public bool IsDeleted { get; set; }

        public int UserQuantity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
 
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }
 
        public int CompanyId { get; set; }
        public Company Company { get; set; }


       
        
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public ICollection<AccessWorkShift> AccessWorkShift { get; set; }
        public ICollection<UserAccessSchedule> UserAccessSchedule { get; set; }
    }
}