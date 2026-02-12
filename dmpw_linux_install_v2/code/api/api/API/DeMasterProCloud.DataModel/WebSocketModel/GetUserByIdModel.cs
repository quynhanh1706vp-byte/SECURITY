namespace DeMasterProCloud.DataModel.WebSocketModel
{
    public class GetUserByIdModel : RequestData
    {
        public string UserID { get; set; }
    }
    public class GetFingerModel : GetUserByIdModel
    {
        public string FingerNo { get; set; }
        public string FingerOnly { get; set; }
    }
    public class SetFaceModel : GetUserByIdModel
    {
        public string Privilege { get; set; }
        public string DuplicationCheck { get; set; }
        public string FaceData { get; set; }
    }
    public class SetUserPhotoModel : GetUserByIdModel
    {
        public string PhotoSize { get; set; }
        public string PhotoData { get; set; }
    }
    public class SetFingerModel : GetUserByIdModel
    {
        public string Privilege { get; set; }
        public string FingerNo { get; set; }
        public string DuplicationCheck { get; set; }
        public string Duress { get; set; }
        public string FingerData { get; set; }
    }

    public class DeviceStatusModel : RequestData
    {
        public string ParamName { get; set; }
    }
    public class DeviceInforModel : RequestData
    {
        public string ParamName { get; set; }
        public string Value { get; set; }
    }
    public class DeviceStatusResponseModel
    {
        public string TerminalType { get; set; }
        public string TerminalID { get; set; }
        public string ProductName { get; set; }
        public string DeviceSerialNo { get; set; }
        public string DeviceUID { get; set; }
        public string Response { get; set; }
        public string ParamName { get; set; }
        public string Value { get; set; }
    }

    public class EmptyAllDataResponseModel : ResponseData
    {
        public string TerminalType { get; set; }
        public string TerminalID { get; set; }
        public string DeviceSerialNo { get; set; }
    }

    public class SetTimeModel : RequestData
    {
        public string Time { get; set; }
    }

    public class GetFirstGlog : RequestData
    {
        public string BeginLogPos { get; set; }
        public string UserID { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class GetNextGlog : RequestData
    {
        public string BeginLogPos { get; set; }
    }
}