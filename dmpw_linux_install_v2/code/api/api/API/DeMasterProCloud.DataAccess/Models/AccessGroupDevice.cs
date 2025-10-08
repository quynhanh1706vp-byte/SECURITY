using System;

namespace DeMasterProCloud.DataAccess.Models
{
    public class AccessGroupDevice
    {
        public int AccessGroupId { get; set; }
        public int IcuId { get; set; }
        public IcuDevice Icu { get; set; }
        public AccessGroup AccessGroup { get; set; }
        public AccessTime Tz { get; set; }
        public int TzId { get; set; }
        
        // public int? ElevatorFloorId { get; set; }
        // public virtual ElevatorFloor ElevatorFloor { get; set; }
        public string FloorIds { get; set; }
        
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
