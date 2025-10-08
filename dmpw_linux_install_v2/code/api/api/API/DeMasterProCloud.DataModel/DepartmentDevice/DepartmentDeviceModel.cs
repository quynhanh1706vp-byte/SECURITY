using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.DepartmentDevice;

public class DepartmentDeviceModel
{
    public int Id { get; set; }
    public string DeviceAddress { get; set; }
    public string DoorName { get; set; }
    public int TzId { get; set; }
    public string AccessTime { get; set; }
}
// public class DepartmentAssignDoor
// {
//     public DepartmentAssignDoor()
//     {
//         Doors = new List<DepartmentAssignDoorDetail>();
//     }
//     public List<DepartmentAssignDoorDetail> Doors { get; set; }
// }
// public class DepartmentAssignDoorDetail
// {
//     public int DoorId { get; set; }
//     public int TzId { get; set; }
//     public int CompanyId { get; set; }
// }