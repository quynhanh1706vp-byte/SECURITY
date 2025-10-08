
namespace DeMasterProCloud.DataModel.DeviceReader
{
    public class DeviceReaderModel
    {
        public int Id { get; set; }
      
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public int IcuDeviceId { get; set; }
    }

    public class DeviceReaderListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public int IcuDeviceId { get; set; }
        public string DeviceType { get; set; }
        public short Status { get; set; }
        public string DeviceName { get; set; }
        public string BuildingName { get; set; }
        public int CameraId { get; set; }
        public int DeviceReaderId { get; set; }
        public string DeviceAddress { get; set; }
        public bool IsCamera { get; set; }
    }

  
    public class DeviceReaderDetail
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public int IcuDeviceId { get; set; }
    }
}
