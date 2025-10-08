using System;
using System.Collections.Generic;
using DeMasterProCloud.DataModel.PlugIn;

namespace DeMasterProCloud.DataModel.SystemInfo
{
    public class LicenseInfoModel
    {
        public bool LicenseVerified { get; set; }
    }
    
    public class DataLicenseInfoModel
    {
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        
        public MachineInfoModel MachineInfo { get; set; }
        public int CountOfDevices { get; set; }

        public string LicenseNumber { get; set; }
        public string LicenseKey { get; set; }
        
        public DateTime EffectiveDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public List<PlugInSettingModel> ListOfPlugIn { get; set; }
    }
}