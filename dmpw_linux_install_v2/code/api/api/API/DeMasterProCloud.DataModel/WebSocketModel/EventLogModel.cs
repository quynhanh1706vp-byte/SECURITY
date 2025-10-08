namespace DeMasterProCloud.DataModel.WebSocketModel
{
    public class EventLogModel
    {
        
    }

    public class FirstGlogResponseModel : ResponseData
    {
        public string TerminalType { get; set; }
        public string TerminalID { get; set; }
        public string DeviceSerialNo { get; set; }
        public string LogID { get; set; }
        public string Time { get; set; }
        public string UserID { get; set; }
        public string Action { get; set; }
        public string AttendStat { get; set; }
        public string APStat { get; set; }
        public string JobCode { get; set; }
        public string Photo { get; set; }
        public string LogImage { get; set; }
    }
}