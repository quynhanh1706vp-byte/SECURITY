using System;

namespace DeMasterProCloud.DataModel.EventLog
{
    public class EventLogBackupModel
    {
        public int Id { get; set; }
        public DateTime AccessTime { get; set; }
        public string UserName { get; set; }
        public DateTime? BirthDay { get; set; }
        public string UserCode { get; set; }
        public string Department { get; set; }
        public string CardId { get; set; }
        public string RID { get; set; }
        public string DoorName { get; set; }
        public string Building { get; set; }
        public string InOut { get; set; }
        public string EventDetail { get; set; }
        public int IssueCount { get; set; }
        public short CardStatus { get; set; }
        public string CardType { get; set; }
        public double BodyTemperature { get; set; }
        public bool IsVisit { get; set; }
        public string VisitorPhone { get; set; }
        public string VisitorEmail { get; set; }
        public string VisitorAddress { get; set; }
        public string VisitorNationalNumberId { get; set; }
        public string Images { get; set; }
        public string Videos { get; set; }
    }
}