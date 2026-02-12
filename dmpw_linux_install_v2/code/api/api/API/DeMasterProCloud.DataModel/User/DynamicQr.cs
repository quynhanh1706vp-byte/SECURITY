namespace DeMasterProCloud.DataModel.User
{
    public class DynamicQr
    {
        public string QrCode { get; set; }
        public int Duration { get; set; }
    }

    public class NFCPhoneID
    {
        public string NfcPhoneId { get; set; }
    }

    public class ValidationQr 
    {
        public string Messages { get; set; }
    }
}