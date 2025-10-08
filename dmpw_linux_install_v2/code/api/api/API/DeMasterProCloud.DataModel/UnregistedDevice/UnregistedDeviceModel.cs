namespace DeMasterProCloud.DataModel.UnregistedDevice
{
    public class UnregistedDeviceModel
    {
        public int Id { get; set; }
        public string DeviceAddress { get; set; }
        public short Status { get; set; }
        public string IpAddress { get; set; }

        public string DeviceType { get; set; }

        public string OperationType { get; set; }

        public string MacAddress { get; set; }

        public int Connectionstatus { get; set; }

        public int RegisterType { get; set; }
    }
}
