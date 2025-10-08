using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataAccess.Models
{
    public class WorkShift
    {
        public WorkShift()
        {
            AccessWorkShift = new HashSet<AccessWorkShift>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public ICollection<AccessWorkShift> AccessWorkShift { get; set; }
    }
}