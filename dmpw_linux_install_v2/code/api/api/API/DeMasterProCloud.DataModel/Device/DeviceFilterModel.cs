using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Device;

public class DeviceFilterModel : FilterModel
{
    public string Filter { get; set; }
    public List<int> OperationTypes { get; set; }
    public List<int> CompanyIds { get; set; }
    public List<int> ConnectionStatus { get; set; }
    public List<int> Status { get; set; }
    public List<int> DeviceTypes { get; set; }
    public List<int> BuildingIds { get; set; }
    public List<int> IgnoreIds { get; set; }
}