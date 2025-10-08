using System.Collections.Generic;
using DeMasterProCloud.DataModel.Device;

namespace DeMasterProCloud.Service.Protocol
{
    /// <summary>
    /// Sending device config to icu
    /// </summary>
    public class IcuDeviceProtocolData : ProtocolData<IcuDeviceProtocolDetailData>
    {
    }

    //public class IcuDeviceProtocolDetailData
    //{
    //    public short Led { get; set; }
    //    public short Buzzer { get; set; }
    //    public short InOutSet { get; set; }
    //    public string Parameter { get; set; }
    //    public short VerifyMode { get; set; }
    //    public int ActiveTimezone { get; set; }
    //    public int PassageTimezone { get; set; }
    //    //public short OpenDuration { get; set; }
    //    public int LockOpenDuration { get; set; }
    //    public short CloseReverseLockFlag { get; set; }
    //    public short SensorType { get; set; }
    //    public short Valid { get; set; }
    //    public short AntiPassback { get; set; }
    //    public short HardAntiPassback { get; set; }
    //    public int StatusDelay { get; set; }
    //    public int Condition { get; set; }
    //    public int TapRange { get; set; }
    //    public int BackupPeriod { get; set; }
    //    public short DoorSensorType { get; set; }
    //}

    public class IcuDeviceProtocolDetailData
    {
        //public short Led { get; set; }
        //public short Buzzer { get; set; }
        //public short InOutSet { get; set; }
        public int ReaderCount { get; set; }
        public List<int> ReaderConfig { get; set; }
        //public string Parameter { get; set; }
        public short VerifyMode { get; set; }
        public int? ActiveTimezone { get; set; }
        public int? PassageTimezone { get; set; }
        //public short OpenDuration { get; set; }
        public int LockOpenDuration { get; set; }
        public bool CloseReverseLockFlag { get; set; }
        //public short SensorType { get; set; }
        public short Valid { get; set; }
        public short AntiPassback { get; set; }
        //public short HardAntiPassback { get; set; }
        public bool SensorAlarm { get; set; }
        public int SensorDuration { get; set; }
        public int MPRCount { get; set; }
        public int MPRInterval { get; set; }
        public int BackupPeriod { get; set; }
        public short DoorSensorType { get; set; }
        public string qrAesKey { get; set; }

        /// <summary>   Gets or sets the device buzzer. </summary>
        /// <value> The device buzzer. </value>
        public short DeviceBuzzer { get; set; }
        
        public List<CameraDeviceConfig> Cameras { get; set; }
        /// <summary>
        /// this use in java code
        /// </summary>
        public string ListCameras { get; set; }

        /// <summary>
        /// This variable means that the device's operation type.
        /// 0 : Entrance
        /// 1 : Restaurant
        /// 2 : Bus reader
        /// 3 : Fire detector (alarm)
        /// </summary>
        public int OperationMode { get; set; }
        public string CompanyCode { get; set; }
        public int BioStationMode { get; set; }
        public bool UseStaticQrCode { get; set; }
        public bool EnableDoorBell { get; set; }
        public string Password { get; set; }
    }

    public class LprProtocolDetailData : IcuDeviceProtocolDetailData
    {
        public bool IsTwoPart { get; set; }
        public string TwoPartTimeFrom { get; set; }
        public string TwoPartTimeTo { get; set; }
    }

    /// <summary>
    /// Device config response
    /// </summary>
    public class IcuDeviceResponseProtocolData : ProtocolData<IcuDeviceResponseDetail>
    {
    }

    public class IcuDeviceResponseDetail
    {
        public string Status { get; set; }
    }


    /// <summary>
    /// Starting message of authentication
    /// </summary>
    public class DeviceAuthStartProtocolData : ProtocolData<DeviceAuthStartProtocolDetail>
    {
    }

    public class DeviceAuthStartProtocolDetail
    {
        public string DeviceAddress { get; set; }
        public string Time { get; set; }
    }

    /// <summary>
    /// Step message of authentication from device
    /// </summary>
    public class DeviceAuthStepProtocolData : ProtocolData<DeviceAuthStepProtocolDetail>
    {
    }

    public class DeviceAuthStepProtocolDetail
    {
        public string DeviceAddress { get; set; }
        public string Time { get; set; }
        public string Data { get; set; }
    }

    /// <summary>
    /// Certificate message of authentication from device
    /// </summary>
    public class DeviceAuthCertificateProtocolData : ProtocolData<DeviceAuthCertificateProtocolDetail>
    {
    }

    public class DeviceAuthCertificateProtocolDetail
    {
        public string Ca_certificate { get; set; }
        public string Client_certificate { get; set; }
        public string Client_key { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }



    /// <summary>
    /// This data model is for Controller
    /// </summary>
    public class ControllerDeviceProtocolData : ProtocolData<ControllerDeviceProtocolDetailData>
    {
    }

    public class ControllerDeviceProtocolDetailData
    {
        public Dictionary<string, IcuDeviceProtocolDetailData> Devices { get; set; }
    }
}
