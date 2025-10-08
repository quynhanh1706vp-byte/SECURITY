using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.WebSocketModel
{
    public class UserInforModel : ResponseData
    {
        public string TerminalType { get; set; }
        public string TerminalID { get; set; }
        public string ProductName { get; set; }
        public string DeviceSerialNo { get; set; }
        public string DeviceUID { get; set; }
        public string UserID { get; set; }
        public string Name { get; set; }
        public string Privilege { get; set; }
        public string Depart { get; set; }
        public string Enabled { get; set; }
        public string TimeSet1 { get; set; }
        public string TimeSet2 { get; set; }
        public string TimeSet3 { get; set; }
        public string TimeSet4 { get; set; }
        public string TimeSet5 { get; set; }
        public string PWD { get; set; }
        public string Fingers { get; set; }
        public string FaceEnrolled { get; set; }
        public string More { get; set; }
        public List<string> FingerPrints;
        public string Face;
        public string Photo { get; set; }
        public string LogImage { get; set; }
    }

    public class GetNextUserModel : RequestData
    {
        public string UserID { get; set; }
    }
    public class GetFingerInforModel : RequestData
    {
        public string UserID { get; set; }
        public string FingerNo { get; set; }
        public string FingerOnly { get; set; }
    }
    public class GetFaceInforModel : RequestData
    {
        public string UserID { get; set; }
    }
}