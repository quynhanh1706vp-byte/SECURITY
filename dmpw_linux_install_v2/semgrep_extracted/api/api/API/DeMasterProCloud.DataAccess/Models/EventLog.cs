using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class EventLog
    {
        public EventLog()
        {
        }

        public int Id { get; set; }
        public string Antipass { get; set; }
        public string CardId { get; set; }
        public int IssueCount { get; set; }
        public short CardType { get; set; }
        public short CardStatus { get; set; }

        public string OtherCardId { get; set; }
        public int? CompanyId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? DeptId { get; set; }
        public string DoorName { get; set; }
        public DateTime EventTime { get; set; }
        public int EventType { get; set; }
        public int IcuId { get; set; }
        public long Index { get; set; }
        public string KeyPadPw { get; set; }
        public string UserName { get; set; }
        [Column(TypeName = "jsonb")]
        public string ImageCamera { get; set; }
        public string ResultCheckIn { get; set; }
        public string Videos { get; set; }
        public double BodyTemperature { get; set; }
        public double DelayOpenDoorByCamera { get; set; }
        public int? UserId { get; set; }
        public bool IsVisit { get; set; }
        public int? VisitId { get; set; }
        public int? CameraId { get; set; }
        public double Distance { get; set; }
        public double SearchScore { get; set; }
        public double LivenessScore { get; set; }        
        [JsonIgnore]
        public Company Company { get; set; }
        [JsonIgnore]
        public IcuDevice Icu { get; set; }
        [JsonIgnore]
        public User User { get; set; }
        [JsonIgnore]
        public Visit Visit { get; set; }
        [JsonIgnore]
        public Camera Camera { get; set; }
    }
}
