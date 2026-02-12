namespace DeMasterProCloud.DataModel.SystemInfo
{
    public class MachineInfoModel
    {
        public string MachineName { get; set; }
        public string MacAddress { get; set; }
        public string OsIdentifier { get; set; }
        public string OsDescription { get; set; }
    }

    public class DataVerifyLicenseModel
    {
        public MachineInfoModel MachineInfo { get; set; }
        public string LicenseKey { get; set; }
    }
    
    public class VerifyLicenseModel
    {
        public string LicenseKey { get; set; }
    }
}