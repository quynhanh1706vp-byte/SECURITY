using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DeMasterProCloud.DataModel.Setting;

namespace DeMasterProCloud.DataModel.Device
{
    /// <summary>   A data Model for the controller device list. </summary>
    /// <remarks>   Edward, 2020-02-28. </remarks>
    public class ControllerDeviceListModel
    {
        /// <summary>   Gets or sets the identifier. </summary>
        /// <value> The identifier. \n
        ///         This is index number of database.</value>
        [JsonProperty("id")]
        public int Id { get; set; }
        public int CompanyId { get; set; }

        /// <summary>   Gets or sets the name of the door. </summary>
        /// <value> The name of the door. </value>
        [Display(Name = nameof(DeviceResource.lblDoorName), ResourceType = typeof(DeviceResource))]
        public string DoorName { get; set; }

        /// <summary>   Gets or sets the device address. </summary>
        /// <value> The device address. </value>
        [Display(Name = nameof(DeviceResource.lblDeviceAddress), ResourceType = typeof(DeviceResource))]
        public string DeviceAddress { get; set; }

        /// <summary>   Gets or sets the door active time zone. </summary>
        /// <value> The door active time zone. </value>
        [Display(Name = nameof(DeviceResource.lblDoorActiveTimezone), ResourceType = typeof(DeviceResource))]
        public string DoorActiveTimeZone { get; set; }

        /// <summary>   Gets or sets the door passage time zone. </summary>
        /// <value> The door passage time zone. </value>
        [Display(Name = nameof(DeviceResource.lblDoorPassageTimezone), ResourceType = typeof(DeviceResource))]
        public string DoorPassageTimeZone { get; set; }

        /// <summary>   Gets or sets the verify mode. </summary>
        /// <value> The verify mode. </value>
        [Display(Name = nameof(DeviceResource.lblVerifyMode), ResourceType = typeof(DeviceResource))]
        public string VerifyMode { get; set; }

        /// <summary>   Gets or sets the status. </summary>
        /// <value> The status. </value>
        [Display(Name = nameof(DeviceResource.lblStatus), ResourceType = typeof(DeviceResource))]
        public short Status { get; set; }

        /// <summary>   Gets or sets the connection status. </summary>
        /// <value> The connection status. </value>
        [Display(Name = nameof(DeviceResource.lblConnectionStatus), ResourceType = typeof(DeviceResource))]
        public short ConnectionStatus { get; set; }

        /// <summary>   Gets or sets the version. </summary>
        /// <value> The version. </value>
        [Display(Name = nameof(DeviceResource.lblVersion), ResourceType = typeof(DeviceResource))]
        public string Version { get; set; }

        /// <summary>   Gets or sets the last communication time. </summary>
        /// <value> The last communication time. </value>
        [Display(Name = nameof(DeviceResource.lblLastCommunicationTime), ResourceType = typeof(DeviceResource))]
        public string LastCommunicationTime { get; set; }
        [Display(Name = nameof(DeviceResource.lblCreateTimeOnlineDevice), ResourceType = typeof(DeviceResource))]
        public string CreateTimeOnlineDevice { get; set; }
        [Display(Name = nameof(DeviceResource.lblUpTimeOnlineDevice), ResourceType = typeof(DeviceResource))]
        public int UpTimeOnlineDevice { get; set; }
        [Display(Name = nameof(DeviceResource.lblCreatedOn), ResourceType = typeof(DeviceResource))]
        public DateTime CreatedOn { get; set; }
        /// <summary>   Gets or sets the number of not transmitting events. </summary>
        /// <value> The total number of not transmitting event. </value>
        [Display(Name = nameof(DeviceResource.lblNumberOfNotTransmittingEvent), ResourceType = typeof(DeviceResource))]
        public int NumberOfNotTransmittingEvent { get; set; }

        /// <summary>   Gets or sets the register identifier number. </summary>
        /// <value> The register identifier number. </value>
        [Display(Name = nameof(DeviceResource.lblRegisterIdNumber), ResourceType = typeof(DeviceResource))]
        public int RegisterIdNumber { get; set; }

        /// <summary>   Gets or sets from database identifier number. </summary>
        /// <value> from database identifier number. </value>
        [Display(Name = nameof(DeviceResource.lblFromDbIdNumber), ResourceType = typeof(DeviceResource))]
        public int FromDbIdNumber { get; set; }

        /// <summary>   Gets or sets the building. </summary>
        /// <value> The building. </value>
        [Display(Name = nameof(DeviceResource.lblBuilding), ResourceType = typeof(DeviceResource))]
        public string Building { get; set; }

        /// <summary>   Gets or sets the type of the device. </summary>
        /// <value> The type of the device. </value>
        [Display(Name = nameof(DeviceResource.lblDeviceType), ResourceType = typeof(DeviceResource))]
        public string DeviceType { get; set; }

        /// <summary>   Gets or sets the in card reader. </summary>
        /// <value> The in card reader. </value>
        public string InCardReader { get; set; }

        /// <summary>   Gets or sets the out card reader. </summary>
        /// <value> The out card reader. </value>
        public string OutCardReader { get; set; }

        /// <summary>   Gets or sets the nfc module. </summary>
        /// <value> The nfc module. </value>
        public string NfcModule { get; set; }

        /// <summary>   Gets or sets the door status. </summary>
        /// <value> The door status. </value>
        public string DoorStatus { get; set; }

        /// <summary>   Gets or sets the name of the company. </summary>
        /// <value> The name of the company. </value>
        public string CompanyName { get; set; }

        /// <summary>   Gets or sets the company code. </summary>
        /// <value> The company code. </value>
        public string CompanyCode { get; set; }
        public int TotalTime { get; set; }
        public int UpTime { get; set; }
        /// <summary> Meal Service Time for this device </summary>
        public string MealServiceTime { get; set; }

        /// <summary> Get current Meal Service schedule </summary>
        public string CurrentMealService { get; set; }
        public string Image { get; set; }
    }

    /// <summary>   A data Model for the controller device. </summary>
    /// <remarks>   Edward, 2020-01-21. </remarks>
    public class ControllerDeviceModel
    {
        public int Id { get; set; }

        /// <summary>   Gets or sets the device address. </summary>
        /// <value> The device address. </value>
        public string ControllerAddress { get; set; }

        /// <summary>   Gets or sets the name of the door. </summary>
        /// <value> The name of the door. </value>
        public string DoorName { get; set; }

        /// <summary>   Gets or sets the type of the device. </summary>
        /// <value> The type of the device.\n
        ///         ICU-300N or iTouchPop2A. </value>
        public int ControllerType { get; set; }

        /// <summary>   Gets or sets the identifier of the company. </summary>
        /// <value> The identifier of the company. </value>
        public int? CompanyId { get; set; }

        /// <summary>
        /// Code value of the company
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>   Gets or sets the IP address. </summary>
        /// <value> The IP address. </value>
        public string IpAddress { get; set; }

        /// <summary>   Gets or sets the anti-passback. </summary>
        /// <value> The anti-passback value.\n
        ///         0 = Not use, 1 = Soft APB, 2 = Hard APB </value>
        public int? Passback { get; set; }

        public string MacAddress { get; set; }

        public List<ReaderModel> Readers { get; set; }
    }

    public class ControllerDeviceDataModel : ControllerDeviceModel
    {
        public IEnumerable<SelectListItemModel> ActiveTimezoneItems { get; set; }
        public IEnumerable<SelectListItemModel> PassageTimezoneItems { get; set; }
        public IEnumerable<SelectListItemModel> BuildingItems { get; set; }
        public IEnumerable<SelectListItemModel> AccessTzItems { get; set; }
        public IEnumerable<SelectListItemModel> CompanyItems { get; set; }
        public IEnumerable<EnumModel> VerifyModeItems { get; set; }
        public IEnumerable<EnumModel> RoleItems { get; set; }
        public IEnumerable<EnumModel> UseCardReaderItems { get; set; }
        public IEnumerable<EnumModel> BuzzerReaderItems { get; set; }
        public IEnumerable<EnumModel> PassbackItems { get; set; }
        public IEnumerable<EnumModel> SensorTypeItems { get; set; }
        public IEnumerable<EnumModel> CardReaderLedItems { get; set; }
        public IEnumerable<EnumModel> DeviceTypeItems { get; set; }
        public IEnumerable<EnumModel> OperationTypeItems { get; set; }
    }


    public class ReaderModel
    {
        public int Id { get; set; }
        public string DeviceAddress { get; set; }
        public string DeviceName { get; set; }
    }
}