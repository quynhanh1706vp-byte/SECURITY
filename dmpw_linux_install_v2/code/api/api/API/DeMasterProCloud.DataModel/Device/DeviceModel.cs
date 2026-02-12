using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DeMasterProCloud.DataModel.Setting;

namespace DeMasterProCloud.DataModel.Device
{
    /// <summary>   A data Model for the device list. </summary>
    /// <remarks>   Edward, 2020-02-28. </remarks>
    public class DeviceListModel
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
        
        /// <summary>   Gets or sets the Bio Station Mode. </summary>
        /// <value> The Bio Station Mode. </value>
        [Display(Name = nameof(DeviceResource.lblBioStationMode), ResourceType = typeof(DeviceResource))]
        public string BioStationMode { get; set; }

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

        /// <summary>
        /// Id value of door status.
        /// </summary>
        public int DoorStatusId { get; set; }

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
        public bool UseAlarmRelay { get; set; }
        public bool AlarmStatus { get; set; }
        public List<string> BuildingParents { get; set; }
        public short Type { get; set; }
        public bool AutoAcceptVideoCall { get; set; }
        public bool EnableVideoCall { get; set; }
        public int FileLogStatus { get; set; }
        public string OperationType { get; set; }
        public int OperationTypeId { get; set; }
        public int VerifyModeId { get; set; }
    }

    /// <summary>   A data Model for the device. </summary>
    /// <remarks>   Edward, 2020-01-21. </remarks>
    public class DeviceModel
    {
        public int Id { get; set; }

        /// <summary>   Gets or sets the device address. </summary>
        /// <value> The device address. </value>
        public string DeviceAddress { get; set; }

        /// <summary>   Gets or sets the name of the door. </summary>
        /// <value> The name of the door. </value>
        public string DoorName { get; set; }

        /// <summary>   Gets or sets the type of the device. </summary>
        /// <value> The type of the device.\n
        ///         ICU-300N or iTouchPop2A. </value>
        public int DeviceType { get; set; }

        /// <summary>   Gets or sets the verify mode. </summary>
        /// <value> The verify mode. </value>
        public int VerifyMode { get; set; }
        
        /// <summary>   Gets or sets the BioStation Mode. </summary>
        /// <value> The BioStation Mode. </value>
        public int BioStationMode { get; set; }

        /// <summary>   Gets or sets the backup period. </summary>
        /// <value> The backup period. </value>
        public int BackupPeriod { get; set; }

        /// <summary>   Gets or sets the identifier of the company. </summary>
        /// <value> The identifier of the company. </value>
        public int? CompanyId { get; set; }

        /// <summary>   Gets or sets the identifier of the building. </summary>
        /// <value> The identifier of the building. </value>
        public int? BuildingId { get; set; }

        /// <summary>   Gets or sets the active timezone identifier. </summary>
        /// <value> The identifier of the active timezone. </value>
        public int? ActiveTimezoneId { get; set; }

        /// <summary>   Gets or sets the identifier of the passage timezone. </summary>
        /// <value> The identifier of the passage timezone. </value>
        public int? PassageTimezoneId { get; set; }

        /// <summary>   Gets or sets the IP address. </summary>
        /// <value> The IP address. </value>
        public string IpAddress { get; set; }

        /// <summary>   Gets or sets the server IP. </summary>
        /// <value> The server IP. </value>
        public string ServerIp { get; set; }

        /// <summary>   Gets or sets the server port. </summary>
        /// <value> The server port. </value>
        public int ServerPort { get; set; }

        /// <summary>   Gets or sets the type of the operation. </summary>
        /// <value> The type of the operation.\n
        ///         nomal open or nomal close. </value>
        public short OperationType { get; set; }

        /// <summary>   Gets or sets the first reader's role. </summary>
        /// <value> The first reader's role.\n
        ///         0 = In, 1 = Out </value>
        public short? RoleReader0 { get; set; } = 0;

        /// <summary>   Gets or sets the second reader's role. </summary>
        /// <value> The second reader's role.\n
        ///         This value is normally used by ICU-300N.\n
        ///         Sometimes it is also used by iTouchPop2A that has additional card reader.\n
        ///         0 = In, 1 = Out </value>
        public short? RoleReader1 { get; set; } = 1;

        /// <summary>   Gets or sets the first reader's LED. </summary>
        /// <value> The first reader's LED.\n
        ///         This value is normally used on the card reader of ICU-300N.\n
        ///         0 = Blue, 1 = Red </value>
        public short? LedReader0 { get; set; } = 0;

        /// <summary>   Gets or sets the second reader's LED. </summary>
        /// <value> The second reader's LED.\n
        ///         This value is normally used on the card reader of ICU-300N.\n
        ///         0 = Blue, 1 = Red </value>
        public short? LedReader1 { get; set; } = 0;

        /// <summary>   Gets or sets the first reader's buzzer. </summary>
        /// <value> Determines whether first card reader uses buzzer.\n
        ///         0 = On, 1 = Off </value>
        public short? BuzzerReader0 { get; set; } = 0;

        /// <summary>   Gets or sets the second reader's buzzer. </summary>
        /// <value> Determines whether second card reader uses buzzer.\n
        ///         0 = On, 1 = Off </value>
        public short? BuzzerReader1 { get; set; } = 0;

        /// <summary>   Gets or sets the use card reader. </summary>
        /// <value> Determines whether the iTouchPop2A uses an additional card reader.\n
        ///         0 = Use, 1 = Not use </value>
        public int? UseCardReader { get; set; }

        /// <summary>   Gets or sets the anti-passback. </summary>
        /// <value> The anti-passback value.\n
        ///         0 = Not use, 1 = Soft APB, 2 = Hard APB </value>
        public int? Passback { get; set; }

        /// <summary>   Gets or sets the type of the sensor. </summary>
        /// <value> The type of the sensor. </value>
        public int SensorType { get; set; }
        public int LockOpenDuration { get; set; }
        public int? MaxOpenDuration { get; set; }
        public int? SensorDuration { get; set; }
        public bool Alarm { get; set; }
        public bool CloseReverseLock { get; set; }

        public int? MPRCount { get; set; }
        public int MPRInterval { get; set; }

        public string MacAddress { get; set; }

        /// <summary>   Gets or sets the device buzzer. </summary>
        /// <value> The device buzzer. \n
        ///         This is used for ICU-300N. \n
        ///         0 = OFF, 1 = ON</value>
        public short DeviceBuzzer { get; set; }
        /// <summary> Meal Service Time for this device </summary>
        public int? MealServiceTimeId { get; set; }
        public string Image { get; set; }
        public bool UseAlarmRelay { get; set; }
        public List<int> DeviceManagerIds { get; set; }
        public List<int> DependentDoors { get; set; }
        // public string DoorStatus { get; set; }
        // public short ConnectionStatus { get; set; }
        public bool AutoAcceptVideoCall { get; set; }
        public bool EnableVideoCall { get; set; }

        // For Nexpa LPR
        public bool IsTwoPart { get; set; }
        public string TwoPartTimeFrom { get; set; }
        public string TwoPartTimeTo { get; set; }

        public int? ControllerId { get; set; }
    }

    public class DeviceDataModel : DeviceModel
    {
        public IEnumerable<SelectListItemModel> ActiveTimezoneItems { get; set; }
        public IEnumerable<SelectListItemModel> PassageTimezoneItems { get; set; }
        public IEnumerable<SelectListItemModel> BuildingItems { get; set; }
        public IEnumerable<SelectListItemModel> AccessTzItems { get; set; }
        public IEnumerable<SelectListItemModel> CompanyItems { get; set; }
        public IEnumerable<EnumModel> VerifyModeItems { get; set; }
        public IEnumerable<EnumModel> BioStationModeItems { get; set; }
        public IEnumerable<EnumModel> RoleItems { get; set; }
        public IEnumerable<EnumModel> UseCardReaderItems { get; set; }
        public IEnumerable<EnumModel> BuzzerReaderItems { get; set; }
        public IEnumerable<EnumModel> PassbackItems { get; set; }
        public IEnumerable<EnumModel> SensorTypeItems { get; set; }
        public IEnumerable<EnumModel> CardReaderLedItems { get; set; }
        public IEnumerable<EnumModel> DeviceTypeItems { get; set; }
        public IEnumerable<EnumModelDeviceType> DeviceTypeList { get; set; }
        public IEnumerable<EnumModel> OperationTypeItems { get; set; }
        public IEnumerable<EnumModel> AccountIds { get; set; }
        public IEnumerable<EnumModel> DependentDoorsIds { get; set; }
    }

    public class DeviceDetailModel : DeviceModel
    {
        public IEnumerable<SelectListItemModel> ActiveTimezoneItems { get; set; }
        public IEnumerable<SelectListItemModel> PassageTimezoneItems { get; set; }
        public IEnumerable<SelectListItemModel> BuildingItems { get; set; }
        public IEnumerable<SelectListItemModel> AccessTzItems { get; set; }
        public IEnumerable<SelectListItemModel> CompanyItems { get; set; }
        public IEnumerable<EnumModel> VerifyModeItems { get; set; }
        public IEnumerable<EnumModel> BioStationModeItems { get; set; }
        public IEnumerable<EnumModel> RoleItems { get; set; }
        public IEnumerable<EnumModel> UseCardReaderItems { get; set; }
        public IEnumerable<EnumModel> BuzzerReaderItems { get; set; }
        public IEnumerable<EnumModel> PassbackItems { get; set; }
        public IEnumerable<EnumModel> SensorTypeItems { get; set; }
        public IEnumerable<EnumModel> CardReaderLedItems { get; set; }
        public IEnumerable<EnumModel> DeviceTypeItems { get; set; }
        public IEnumerable<EnumModelDeviceType> DeviceTypeList { get; set; }
        public IEnumerable<EnumModel> OperationTypeItems { get; set; }
        public IEnumerable<EnumModel> AccountIds { get; set; }
        public IEnumerable<EnumModel> DependentDoorsIds { get; set; }
        public string DoorStatus { get; set; }
        public short ConnectionStatus { get; set; }
    }

    /// <summary>   A data Model for the device history. </summary>
    /// <remarks>   Edward, 2020-02-29. </remarks>
    public class DeviceHistoryModel
    {
        /// <summary>   Gets or sets the event time. </summary>
        /// <value> The event time. </value>
        public string EventTime { get; set; }

        /// <summary>   Gets or sets the type of the event. </summary>
        /// <value> The type of the event. </value>
        public string EventType { get; set; }

        /// <summary>   Gets or sets the operator. </summary>
        /// <value> The operator. </value>
        public string Operator { get; set; }

        /// <summary>   Gets or sets the event details. </summary>
        /// <value> The event details. </value>
        public string EventDetails { get; set; }
        public DateTime AccessTimeUtc { get; set; }
    }



    public class MonitoringUpTimeDevice
    {
        public int id { get; set; }
        public DateTime TimeSuccess { get; set; }
        public DateTime? TimeFailed { get; set; }
    }

    public class IcuAddress
    {
        public string Address { get; set; }
    }

    public class Data
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class DeviceStatus
    {
        public bool Status { get; set; }
    }

    public class ReinstallModel
    {
        public int Id { get; set; }
        public List<int> Ids { get; set; }
    }

    /// <summary>   A reinstall device detail. </summary>
    /// <remarks>   Edward, 2020-01-20. </remarks>
    public class ReinstallDeviceDetail
    {
        /// <summary>   Gets or sets the identifier of the device. </summary>
        /// <value> The identifier of the device. </value>
        public int DeviceId { get; set; }

        /// <summary>   Gets or sets the identifier of the process.
        ///             This is made by caller to show percentage through progress bar.</summary>
        /// <value> The identifier of the process. </value>
        public string ProcessId { get; set; }
    }

    /// <summary>   A class of device instruction information. </summary>
    /// <remarks>   Edward, 2020-01-20. </remarks>
    public class DeviceInstruction
    {
        /// <summary>   Gets or sets the identifiers. </summary>
        /// <value> The list of identifiers. </value>
        public List<int> Ids { get; set; }

        /// <summary>   Gets or sets the command. </summary>
        /// <value> The instruction command. </value>
        public string Command { get; set; }

        /// <summary>   Gets or sets the open period. </summary>
        /// <value> The open period when command to open the door. </value>
        public int OpenPeriod { get; set; }

        /// <summary>   Gets or sets the open until time. </summary>
        /// <value> Variable for opening door(s) up to a specific time. </value>
        public string OpenUntilTime { get; set; }
        
        public LocalMqttSetting LocalMqtt { get; set; }
    }

    
    /// <summary>   A class of device instruction information. </summary>
    /// <remarks>   Edward, 2020-01-20. </remarks>
    public class DeviceInstructionRID
    {
        /// <summary>   Gets or sets the identifiers. </summary>
        /// <value> The list of identifiers. </value>
        public List<string> Rids { get; set; }

        /// <summary>   Gets or sets the command. </summary>
        /// <value> The instruction command. </value>
        public string Command { get; set; }

        /// <summary>   Gets or sets the open period. </summary>
        /// <value> The open period when command to open the door. </value>
        public int OpenPeriod { get; set; }

        /// <summary>   Gets or sets the open until time. </summary>
        /// <value> Variable for opening door(s) up to a specific time. </value>
        public string OpenUntilTime { get; set; }
    }



    public class DeviceTypeList
    {
        public DeviceTypeList()
        {
            DeviceTypes = new List<DeviceTypeModel>();
        }

        public List<DeviceTypeModel> DeviceTypes { get; set; }
    }

    public class DeviceTypeModel
    {
        public DeviceTypeModel()
        {
            FileList = new List<FileDetail>();
        }

        public string Name { get; set; }
        public List<FileDetail> FileList { get; set; }
    }

    public class FileDetail
    {
        public FileDetail()
        {
            IcuIds = new List<int>();
        }

        public List<int> IcuIds { get; set; }
        public string Target { get; set; }
        public string Remark { get; set; }
    }

    public class ProcessInfo
    {
        public string Target { get; set; }
        public string DeviceAddress { get; set; }
    }

    public class AccessibleDoorModel
    {
        public int Id { get; set; }
        public string DoorName { get; set; }
        public string DeviceAddress { get; set; }
        public string ActiveTz { get; set; }
        public string PassageTz { get; set; }
        public string AccessGroupTz { get; set; }
        public string VerifyMode { get; set; }
        public int AntiPassback { get; set; }
        public string DeviceType { get; set; }
        public int Mpr { get; set; }
    }

    public class CheckDeviceInfoDetail
    {
        public string MsgId { get; set; }
        public CheckDeviceSettingModel Data { get; set; }
    }

    public class CheckDeviceSettingModel
    {
        public int DeviceType { get; set; } // Not for display.

        public string RoleReader0 { get; set; }
        public string RoleReader1 { get; set; }

        public string LedReader0 { get; set; }
        public string LedReader1 { get; set; }
        public string UseCardReader { get; set; }

        public string BuzzerReader0 { get; set; }
        public string BuzzerReader1 { get; set; }

        public string VerifyMode { get; set; }
        public string BioStationMode { get; set; }
        public int BackupPeriod { get; set; }

        public string Valid { get; set; }

        public string ActiveTimezone { get; set; }
        public string PassageTimezone { get; set; }

        public string IpAddress { get; set; }
        public string ServerPort { get; set; }
        public string ServerIp { get; set; }

        public string AntiPassback { get; set; }

        public string DoorSensorType { get; set; }
        public int LockOpenDuration { get; set; }
        public int SensorDuration { get; set; }

        public int MPRCount { get; set; }
        public int MPRInterval { get; set; }

        public bool SensorAlarm { get; set; }
        public bool CloseReverseLockFlag { get; set; }

        public int CardCount { get; set; }
    }

    public class DeviceListModelForUser
    {
        public int Id { get; set; }
        public string DoorName { get; set; }
        public string DeviceAddress { get; set; }
    }

    public class RecoveryDeviceModel
    {
        public int Id { get; set; }
        public string DoorName { get; set; }
        public string DeviceAddress { get; set; }
        public string BuildingName { get; set; }
        public int? DB { get; set; }
        public string ProcessId { get; set; }
        //public int? FromDeviceCount { get; set; }
        //public int? CountInDbFromDeviceFirstDate { get; set; }
        //public string FirstDateFromDevice { get; set; }
    }

    public class TransmitInfoModel
    {
        public List<EnumModel> Data { get; set; }
    }

    public class TransmitDataModel
    {
        public List<ReinstallDeviceDetail> Devices { get; set; }
        public List<int> TransmitIds { get; set; }
        public bool IsAllDevice { get; set; }
        public bool IsDeleteAllUser { get; set; }

        /// <summary>
        /// This list has identifier of user who has information that will be sent to device.
        /// </summary>
        public List<int> UserIds { get; set; }

        public static String GetProcessIdFromDeviceId(List<ReinstallDeviceDetail> details, int deviceId)
        {
            String result = null;

            if (details != null)
            {
                foreach (var detail in details)
                {
                    if (detail.DeviceId == deviceId)
                    {
                        result = detail.ProcessId;
                    }
                }
            }
            return result;
        }
    }

    public class MasterCardModel
    {
        public int Id { get; set; }
        public string DoorName { get; set; }
        public string DeviceAddress { get; set; }
        public int McNumber { get; set; }
    }

    public class DeviceMsgIdModel
    {
        public string MessageId { get; set; }
    }

    public class UserMasterCardModelDetail
    {
        public string CardId { get; set; }
        public string UserName { get; set; }
    }

    public class UserMasterCardModel
    {
        public List<UserMasterCardModelDetail> UserMasterCard { get; set; }
        public string MsgId { get; set; }
    }

    public class DeviceInitModel
    {
        public IEnumerable<EnumModel> VerifyModeItems { get; set; }
        public IEnumerable<EnumModel> BioStationModeItems { get; set; }
        public IEnumerable<EnumModel> SensorTypeItems { get; set; }
        public IEnumerable<EnumModel> PassbackItems { get; set; }
        public IEnumerable<EnumModel> HolidayItems { get; set; }
    }

    public class RecentlyDisconnectedDeviceModel
    {
        public int Id { get; set; }
        public string DoorName { get; set; }
        public string DeviceAddress { get; set; }
        public double TotalOfflineSeconds { get; set; }
        public int ConnectionStatus { get; set; }
        public string Company { get; set; }

        public int Count { get; set; }
    }

    public class DeviceRidModel
    {
        public string Rid { get; set; }
    }

    public class FileLogListModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string CreatedOn { get; set; }
        public long Size { get; set; }
        public string LinkDownload { get; set; }
    }
}