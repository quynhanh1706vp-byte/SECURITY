using DeMasterProCloud.DataModel.Setting;
using System.Collections.Generic;

namespace DeMasterProCloud.Service.Protocol
{
    /// <summary>
    /// Device instruction send to icu
    /// </summary>
    public class DeviceInstructionProtocolData : ProtocolData<DeviceInstructionDetail>
    {
    }

    public class DeviceInstructionDetail
    {
        public string Command { get; set; } //open or closed
        public string UserName { get; set; }
    }

    /// <summary>
    /// Device instruction 
    /// </summary>
    ///
    public class DeviceInstructionOpenProtocolData : ProtocolData<DeviceInstructionOpenDetail>
    {
    }
    public class DeviceInstructionOpenDetail
    {
        public DeviceInstructionOpenDetail()
        {
            Options = new Option1();
        }
        public string Command { get; set; }
        public string UserName { get; set; }
        public Option1 Options { get; set; }
    }

    public class Option1
    {
        public int OpenPeriod { get; set; }
        public string OpenUntilTime { get; set; }
    }


    public class ControllerInstructionOpenProtocolData : DeviceInstructionOpenProtocolData
    {
        public List<string> DeviceAddresses { get; set; }
    }

    public class ControllerInstructionProtocolData : DeviceInstructionProtocolData
    {
        public List<string> DeviceAddresses { get; set; }
    }


    /// <summary>
    /// Device instruction settime
    /// </summary>
    public class DeviceInstructionSetTimeProtocolData : ProtocolData<DeviceInstructionSetTimeDetail>
    {
        public List<string> Receivers { get; set; }
    }

    public class DeviceInstructionSetTimeDetail
    {
        public DeviceInstructionSetTimeDetail()
        {
            Options = new Option2();
        }
        public string Command { get; set; } //open or closed
        public string UserName { get; set; }
        
        public int utcHour { get; set; }
        public int utcMinute { get; set; }
        public Option2 Options { get; set; }
    }

    public class Option2
    {
        public string Time { get; set; }
        public bool IsSchedule { get; set; }
    }

    /// <summary>
    /// Device instruction settime
    /// </summary>
    public class DeviceInstructionUpdateDeviceStateProtocolData : ProtocolData<DeviceInstructionUpdateDeviceStateDetail>
    {
    }

    public class DeviceInstructionUpdateDeviceStateDetail
    {
        public DeviceInstructionUpdateDeviceStateDetail()
        {
            Options = new OptionState();
        }
        public string Command { get; set; } //open or closed
        public string UserName { get; set; }
        public OptionState Options { get; set; }
    }

    public class OptionState
    {
        public int State { get; set; }
    }

    /// <summary>
    /// Device instruction response
    /// </summary>
    public class DeviceInstructionResponse : ProtocolData<DeviceDataInstructionReponseDetail>
    {
    }

    public class DeviceDataInstructionReponseDetail
    {
        public string DeviceAddress { get; set; }
        public string DoorStatus { get; set; }
        public string Status { get; set; }
    }
    
    public class DeviceInstructionActiveLicenseResponse : ProtocolData<DeviceDataInstructionActiveLicenseReponseDetail>
    {
    }

    public class DeviceDataInstructionActiveLicenseReponseDetail
    {
        public string Message { get; set; }
        public string Serial { get; set; }
        public string Key { get; set; }
        public string ActivationCode { get; set; }
        public string DeviceId { get; set; }
        public string MacAddress { get; set; }
    }

    /// <summary>
    /// Device instruction response
    /// </summary>
    public class SendDoorStatusProtocol : ProtocolData<DoorStatusDetail>
    {
    }

    public class DoorStatusDetail
    {
        public int IcuId { get; set; }
        public string Status { get; set; }
    }


    /// <summary>
    /// Door Status message
    /// </summary>
    public class DoorStatusProtocol : ProtocolData<DoorStatusIntDetail>
    {
    }
    // TODO rename later
    public class DoorStatusIntDetail
    {
        public string DeviceAddress { get; set; }
        // 0 : Closed & Lock
        // 1 : Closed & Unlocked
        // 2 : Opened
        // 3 : Fored Opened
        // 4 : Passage Opened
        // 5 : Emergency Opened
        // 6 : Emergency Closed
        public int DoorState { get; set; }
        public int AlarmState { get; set; }
    }
    
    public class DeviceInstructionLocalMqttProtocolData : ProtocolData<DeviceInstructionLocalMqttDetail>
    {
    }

    public class DeviceInstructionLocalMqttDetail
    {
        public string Command { get; set; }
        public string UserName { get; set; }
        public LocalMqttSetting LocalMqtt { get; set; }
    }
    
    public class DeviceInstructionActiveLicenseProtocolData : ProtocolData<DeviceInstructionActiveLicenseDetail>
    {
    }

    public class DeviceInstructionActiveLicenseDetail
    {
        public string Command { get; set; }
        public string UserName { get; set; }
        public ActiveFaceSetting Data { get; set; }
    }


    public class DeviceDebugModeData : ProtocolData<DeviceDebugModeDetail>
    {
    }
    public class DeviceDebugModeDetail
    {
        public DeviceDebugModeDetail()
        {
        }

        public int DebugMode { get; set; }
    }
}
