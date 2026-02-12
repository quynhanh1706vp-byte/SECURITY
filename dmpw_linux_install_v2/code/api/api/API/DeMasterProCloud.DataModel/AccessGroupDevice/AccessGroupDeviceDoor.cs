namespace DeMasterProCloud.DataModel.AccessGroupDevice
{
    public class AccessGroupDeviceDoor
    {
        public int Id { get; set; }
        public string DeviceAddress { get; set; }
        public string DoorName { get; set; }
        public int TzId { get; set; }
        public string Timezone { get; set; }
        public string Building { get; set; }
        public int Type { get; set; }

        public bool IsParent { get; set; }
    }

    public class AccessGroupDeviceAssignDoor
    {
        public int Id { get; set; }
        public string DeviceAddress { get; set; }
        public string DoorName { get; set; }
        public short OperationType { get; set; }
        public short VerifyMode { get; set; }
        public string Building { get; set; }
        public string AccessTimeName { get; set; }
        public string TzId { get; set; }
        
    }
    public class AccessGroupDeviceUnAssignDoor
    {
        public int Id { get; set; }
        public string DeviceAddress { get; set; }
        public string DoorName { get; set; }
        public short OperationType { get; set; }
        public short VerifyMode { get; set; }
        public string Building { get; set; }
        public string AccessTimeName { get; set; }
        public string TzId { get; set; }
        
    }
}
