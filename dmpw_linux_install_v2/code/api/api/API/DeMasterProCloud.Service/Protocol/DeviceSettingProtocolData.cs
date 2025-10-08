using System;
using System.Collections.Generic;
using System.Text;

namespace DeMasterProCloud.Service.Protocol
{
    public class DeviceSettingProtocolData : ProtocolData<DeviceSettingProtocolDataHeader>
    {
    }

    public class DeviceSettingProtocolDataHeader
    {
    }

    /// <summary>
    /// Device setting receive from icu
    /// </summary>
    public class DeviceSettingResponse : ProtocolData<DeviceSettingResponseDetail>
    {
    }

    public class DeviceSettingResponseDetail
    {
        public int ReaderCount { get; set; }
        public List<int> ReaderConfig { get; set; }

        public int VerifyMode { get; set; }
        public int BackupPeriod { get; set; }

        public int Valid { get; set; }  // Maybe not use.

        public int? ActiveTimezone { get; set; }
        public int? PassageTimezone { get; set; }
        
        public string IpAddress { get; set; }
        public string ServerPort { get; set; }
        public string ServerIp { get; set; }

        public int AntiPassback { get; set; }
        
        public int DoorSensorType { get; set; }
        public int LockOpenDuration { get; set; }
        public int SensorDuration { get; set; }

        public int MPRCount { get; set; }
        public int MPRInterval { get; set; }

        public bool SensorAlarm { get; set; }
        public bool CloseReverseLockFlag { get; set; }
        
        public int CardCount { get; set; }
    }

    /// <summary>
    /// Device setting send to webapp
    /// </summary>
    public class SendDeviceSettingProtocol : ProtocolData<DeviceSettingDetail>
    {
    }

    public class DeviceSettingDetail
    {
        //public string ActiveTimezone { get; set; }
        //public string PassageTimezone { get; set; }
        //public int VerifyMode { get; set; }
        //public string IpAddress { get; set; }
        //public string ServerPort { get; set; }
        //public string ServerIp { get; set; }
        //public int AntiPassback { get; set; }
        //public int HardPassback { get; set; }
        //public int LockOpenDuration { get; set; }
        //public int DoorSensorType { get; set; }
        //public int StatusDelay { get; set; }
        //public int DefaultLed { get; set; }
        //public int AuthCount { get; set; }
        //public int AuthInterval { get; set; }
        //public int Alarm { get; set; }
        //public int CloseReverseLock { get; set; }
        //public int cardCount { get; set; }

        //public int ReaderCount { get; set; }
        //public List<int> ReaderConfig { get; set; }
        
        public string RoleReader0 { get; set; }
        public string RoleReader1 { get; set; }

        public string LedReader0 { get; set; }
        public string LedReader1 { get; set; }
        public string UseCardReader { get; set; }

        public string BuzzerReader0 { get; set; }
        public string BuzzerReader1 { get; set; }

        public int VerifyMode { get; set; }
        public int BackupPeriod { get; set; }

        public string Valid { get; set; }

        public string ActiveTimezone { get; set; }
        public string PassageTimezone { get; set; }

        public string IpAddress { get; set; }
        public string ServerPort { get; set; }
        public string ServerIp { get; set; }

        public int AntiPassback { get; set; }

        public int DoorSensorType { get; set; }
        public int LockOpenDuration { get; set; }
        public int SensorDuration { get; set; }

        public int MPRCount { get; set; }
        public int MPRInterval { get; set; }

        public bool SensorAlarm { get; set; }
        public bool CloseReverseLockFlag { get; set; }

        public int CardCount { get; set; }
    }
}
