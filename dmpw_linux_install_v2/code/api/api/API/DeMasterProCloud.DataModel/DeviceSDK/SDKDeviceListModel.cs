using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.DeviceSDK;

public class SDKDevicePagingListModel
{
    public int Page { get; set; }
    public int Total { get; set; }
    public List<SDKDeviceListModel> Data { get; set; }
}

public class SDKDeviceListModel
{
    public string DeviceAddress { get; set; }
    public int Status { get; set; }
    public string IpAddress { get; set; }
    public string DeviceType { get; set; }
    public string MacAddress { get; set; }
    public string OperationMode { get; set; }
    public string ClientId { get; set; }
}

public class SDKDeviceInfoModel
{
    public string DeviceAddress { get; set; }
    public string MacAddress { get; set; }

    public string Version { get; set; }
    public string Reader0Version { get; set; }
    public string Reader1Version { get; set; }
    public string NfcModuleVersion { get; set; }
    public Dictionary<string, string> ExtraVersion { get; set; }

    public string IpAddress { get; set; }
    public string Subnet { get; set; }
    public string Gateway { get; set; }
    public string ServerIp { get; set; }
    public string ServerPort { get; set; }
    public string DeviceTime { get; set; }
    public int EventCount { get; set; }
    public int UserCount { get; set; }
    public int EventNotTransCount { get; set; }
    public int OperationMode { get; set; }
    public List<SDKIntegratedDeviceModel> IntegratedDevices { get; set; }
}

public class SDKIntegratedDeviceModel
{
    public string Type { get; set; }
    public string Id { get; set; }
    public int Role { get; set; }
}

public class SDKAccessTimeModel
{
    public int Total { get; set; }
    public List<SDKAccessTimeDetailModel> Timezone { get; set; }
}

public class SDKAccessTimeDetailModel
{
    public int TimezonePosition { get; set; }
    public int ScheduleCount { get; set; }
    public SDKIntervalTime Monday { get; set; }
    public SDKIntervalTime Tuesday { get; set; }
    public SDKIntervalTime Wednesday { get; set; }
    public SDKIntervalTime Thursday { get; set; }
    public SDKIntervalTime Friday { get; set; }
    public SDKIntervalTime Saturday { get; set; }
    public SDKIntervalTime Sunday { get; set; }
    public SDKIntervalTime Holiday1 { get; set; }
    public SDKIntervalTime Holiday2 { get; set; }
    public SDKIntervalTime Holiday3 { get; set; }
}

public class SDKIntervalTime
{
    public string Interval1 { get; set; }
    public string Interval2 { get; set; }
    public string Interval3 { get; set; }
    public string Interval4 { get; set; }
}

public class SDKHolidayModel
{
    public int Total { get; set; }
    public List<SDKHolidayDetailModel> Timezone { get; set; }
}

public class SDKHolidayDetailModel
{
    public int HolidayType { get; set; }
    public int Recurring { get; set; }
    public List<SDKHolidayDates> HolidayDate { get; set; }
}

public class SDKHolidayDates
{
    public string Date { get; set; }
}

public class SDKOpenDoorModel
{
    public int OpenPeriod { get; set; }
    public string OpenUntilTime { get; set; }
}

public class SDKUserProtocolHeaderData
{
    public SDKUserProtocolHeaderData()
    {
        Users = new List<SDKCardModel>();
    }
    public int Total { get; set; }
    public int UpdateFlag { get; set; }

    public int FrameIndex { get; set; }
    public int TotalIndex { get; set; }

    public List<SDKCardModel> Users { get; set; }
}
public class SDKCardModel
{
    public string EmployeeNumber { get; set; }
    public string UserName { get; set; }
    public string CardId { get; set; }
    public int IssueCount { get; set; }
    public short AdminFlag { get; set; }
    public string EffectiveDate { get; set; }
    public string ExpireDate { get; set; }
    public int CardStatus { get; set; }
    public string UserId { get; set; }
    public int IdType { get; set; }
    public string DepartmentName { get; set; }
    public int Timezone { get; set; }
    public int AntiPassBack { get; set; }
    public string Password { get; set; }
    public int AccessGroupId { get; set; }
    public string Position { get; set; }
    public string Avatar { get; set; }
    public SDKFaceDataList FaceData { get; set; }
    public List<int> FloorIndex { get; set; }
    public string WorkType { get; set; }
    public string Grade { get; set; }
    public List<string> FingerTemplates { get; set; }
}

public class SDKFaceDataList
{
    public string LeftIrisImage { get; set; }
    public string RightIrisImage { get; set; }
    public string FaceImage { get; set; }
    public string FaceSmallImage { get; set; }
    public string LeftIrisCode { get; set; }
    public string RightIrisCode { get; set; }
    public string FaceCode { get; set; }
}

public class SDKUpdateDeviceConfigModel
{
    public string DeviceAddress { get; set; }
    public int ReaderCount { get; set; }
    public List<int> ReaderConfig { get; set; }
    public short VerifyMode { get; set; }
    public int? ActiveTimezone { get; set; }
    public int? PassageTimezone { get; set; }
    public int LockOpenDuration { get; set; }
    public bool CloseReverseLockFlag { get; set; }
    public short Valid { get; set; }
    public short AntiPassback { get; set; }
    public bool SensorAlarm { get; set; }
    public int SensorDuration { get; set; }
    public int MPRCount { get; set; }
    public int MPRInterval { get; set; }
    public int BackupPeriod { get; set; }
    public short DoorSensorType { get; set; }
    public string QrAesKey { get; set; }
    public short DeviceBuzzer { get; set; }
    public List<SDKCameraDeviceConfig> Cameras { get; set; }
    public int OperationMode { get; set; }
    public string CompanyCode { get; set; }
    public bool UseStaticQrCode { get; set; }
    public string Password { get; set; }
    public string ListCameras { get; set; }
}

public class SDKCameraDeviceConfig
{
    public string CameraId { get; set; }
    public int RoleReader { get; set; }
    public bool SaveEventUnknownFace { get; set; }
    public int Similarity { get; set; } 
    public bool VoiceAlarm { get; set; } 
    public bool LightAlarm { get; set; }
}

public class LprSDKUpdateDeviceConfigModel : SDKUpdateDeviceConfigModel
{
    public bool IsTwoPart { get; set; }
    public string TwoPartTimeFrom { get; set; }
    public string TwoPartTimeTo { get; set; }
}