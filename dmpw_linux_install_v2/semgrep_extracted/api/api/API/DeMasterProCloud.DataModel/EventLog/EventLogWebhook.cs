using System;

namespace DeMasterProCloud.DataModel.EventLog
{
    public class EventLogWebhook
    {
        public Guid Id { get; set; }
        public object EventLogId { get; set; }
        public int? UserId { get; set; }
        public int? VisitId { get; set; }
        public DateTime AccessTime { get; set; }
        public string Device { get; set; }
        public string DoorName { get; set; }
        public string EventDetail { get; set; }
        public int EventDetailCode { get; set; }
        public string ExpireDate { get; set; }
        public int? CardStatus { get; set; }
        public string CardId { get; set; }
        public string CardType { get; set; }
        public string IssueCount { get; set; }
        public string UserName { get; set; }
        public string UserCode { get; set; }
        // public string Avatar { get; set; }
        // public string ResultCheckIn { get; set; }
        public string Department { get; set; }
        public string InOut { get; set; }
        public double UnixTime { get; set; }
        public double BodyTemperature { get; set; }
        public string NationalIdNumber { get; set; }
    }
}