using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.User;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.Building
{
    public class BuildingModel
    {
        [JsonIgnore]
        public int Id { get; set; }
        [Display(Name = nameof(BuildingResource.lblBuildingName), ResourceType = typeof(BuildingResource))]
        public string Name { get; set; }
        
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Location { get; set; }
        public string TimeZone { get; set; }
        public int? ParentId { get; set; }
    }

    public class BuildingDataModel : BuildingModel
    {
        public BuildingDataModel()
        {
            ParentBuildings = new List<BuildingListModel>();
        }

        public List<BuildingListModel> ParentBuildings {  get; set; }
    }

    public class BuildingListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class BuildingListItemModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string TimeZone { get; set; }
        
        public IList<BuildingListItemModel> Children { get; set; }
        public int ParentId { get; set; }
        public string ParentName { get; set; }

        public List<BuildingDoorModel> DoorList { get; set; }
    }
    
    public class BuildingDoorModel
    {
        public int Id { get; set; }
        public string DoorName { get; set; }
        public string DeviceAddress { get; set; }
        public string DeviceType { get; set; }
        public string ActiveTz { get; set; }
        public string PassageTz { get; set; }
        public int ConnectionStatus { get; set; }
        public int OperationTypeId { get; set; }
        public short VerifyModeId { get; set; }
        public string OperationType { get; set; }
        public string VerifyMode { get; set; }
        /// <summary>
        /// This is door's status.
        /// </summary>
        public string DoorStatus { get; set; }

        /// <summary>
        /// Id value of DoorStatus
        /// </summary>
        public int DoorStatusId { get; set; }

        public string Image { get; set; }

        /// <summary>
        /// FW version of device
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Last communication time
        /// </summary>
        public string LastCommunicationTime { get; set; }
        public bool AutoAcceptVideoCall { get; set; }
        public bool EnableVideoCall { get; set; }
    }

    public class BuildingUnAssignDoorModel
    {
        public int Id { get; set; }
        public string Building { get; set; }
        public string DoorName { get; set; }
        public string DeviceAddress { get; set; }
        public int OperationTypeId { get; set; }
        public int VerifyModeId { get; set; }
        public string VerifyMode { get; set; }
        public string OperationType { get; set; }

    }

    public class BuildingDoorMasterModel
    {
        /// <summary>
        /// index value
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// A name of the building.
        /// </summary>
        public string BuildingName { get; set; }

        /// <summary>
        /// List of doors
        /// </summary>
        public List<MasterCardModel> Doors { get; set; }

        /// <summary>
        /// List of masters
        /// </summary>
        public List<UserListModel> Masters { get; set; }
    }
}
