namespace DeMasterProCloud.DataModel.WebSocketModel
{
    public class RegistrationResponse : ResponseData
    {
        public string DeviceSerialNo { get; set; }
        public string Token { get; set; }
    }
    public class RegistrationRequest : RequestData
    {
        public string ProductName { get; set; }
        public string TerminalType { get; set; }
        public string DeviceSerialNo { get; set; }
        public string CloudId { get; set; }
    }
}