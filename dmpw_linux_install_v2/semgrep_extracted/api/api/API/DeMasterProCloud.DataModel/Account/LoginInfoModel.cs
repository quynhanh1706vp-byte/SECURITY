using System;

namespace DeMasterProCloud.DataModel.Account
{
    public class LoginInfoModel
    {
        public DateTime Time { get; set; }
        public string IpAddress { get; set; }
    }

    public class CurrentLoginInfoModel
    {
        public string IpAddress { get; set; }
        public DateTime ActiveTime { get; set; }
    }

    public class LoginSessionConfig
    {
        public bool EnableSingleIpAddress { get; set; }
        public int SessionExpiredTime { get; set; }
    }
    
    public class HashQRCode
    {
        public string CardId { get; set; }
        public int Duration { get; set; }
        public string SecretKey { get; set; }
        public bool UseStaticQrCode { get; set; }
        public string SecretIV { get; set; }
    }
}