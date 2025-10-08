using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.AccessGroup
{
    public class AccessGroupModel
    {
        // [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
    }
    public class AccessGroupListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public short Type { get; set; }
        public int TotalDoors { get; set; }
        public int TotalUsers { get; set; }
    }

    public class AccessGroupAssignDoor
    {
        public AccessGroupAssignDoor()
        {
            Doors = new List<AccessGroupAssignDoorDetail>();
        }
        public List<AccessGroupAssignDoorDetail> Doors { get; set; }
    }
    public class AccessGroupAssignDoorDetail
    {
        public int DoorId { get; set; }
        public int TzId { get; set; }
        public int CompanyId { get; set; }
    }

    public class DoorListModel
    {
        public int Id { get; set; }
        public string DoorName { get; set; }
        public bool IsDelete { get; set; }
    }

}
