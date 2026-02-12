using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Setting
{
    public class ConfigLocalMqttModel
    {
        public List<int> DeviceIds { get; set; }
        public LocalMqttSetting LocalMqtt { get; set; }
    }
    
    public class LocalMqttSetting
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    
    public class ConfigActiveFace
    {
        public List<int> DeviceIds { get; set; }
        public ActiveFaceSetting ActiveFace { get; set; }
    }

    public class ActiveFaceSetting
    {
        public string Serial { get; set; }
        public string Key { get; set; }
        public string ActivationCode { get; set; }
    }
}