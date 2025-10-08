namespace DeMasterProCloud.DataModel.WebSocketModel
{
    public class LoginResponse : ResponseData
    {
        public string Response { get; set; }
        public string DeviceSerialNo { get; set; }
    }

    public class LoginRequest : RequestData
    {
        public string DeviceSerialNo { get; set; }
        public string Token { get; set; }
    }

    public class AdminLogV2
    {
        public string TerminalType { get; set; }
        public string TerminalID { get; set; }
        public string ProductName { get; set; }
        public string DeviceSerialNo { get; set; }
        public string DeviceUID { get; set; }
        public string Event { get; set; }
        public string TransID { get; set; }
        public string LogID { get; set; }
        public string Time { get; set; }
        public string AdminID { get; set; }
        public string UserID { get; set; }
        public string Action { get; set; }
        public string Stat { get; set; }
    } 
    public class KeepAlive
    {
        public string TerminalType { get; set; }
        public string TerminalID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string Event { get; set; }
        public string DevTime { get; set; }
    }

    public class GetFaceDataResponse : ResponseData
    {
        public string TerminalType { get; set; }
        public string TerminalID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string UserID { get; set; }
        public string FaceEnrolled { get; set; }
        public string FaceData { get; set; }
    } 
    public class GetPhotoDataResponse : ResponseData
    {
        public string TerminalType { get; set; }
        public string TerminalID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string UserID { get; set; }
        public string PhotoData { get; set; }
    }
    public class GetFingerDataResponse : ResponseData
    {
        public string TerminalType { get; set; }
        public string TerminalID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string UserID { get; set; }
        public string FingerNo { get; set; }
        public string Duress { get; set; }
        public string FingerData { get; set; }
    }
}
        