using Newtonsoft.Json;

namespace DeMasterProCloud.DataModel.EventLog
{

    /// <summary>
    /// EventLogReport List class
    /// </summary>
    public class EventLogReportListModel
    {
        public int Id { get; set; }
        public int? IcuId { get; set; }
        public int? UserId { get; set; }
        public int? VisitId { get; set; }
        public string AccessTime { get; set; }
        public string EventTime { get; set; }
        public string UserName { get; set; }
        public string BirthDay { get; set; }
        public string EmployeeNumber { get; set; }
        public string Department { get; set; }
        public string DepartmentName { get; set; }
        public string CardId { get; set; }
        public string DeviceAddress { get; set; }
        public string DoorName { get; set; }
        public string Building { get; set; }
        public string UserCode { get; set; }

        public string InOut { get; set; }
        public string EventDetail { get; set; }

        /// <summary>
        /// EventLog index value
        /// </summary>
        public int EventLogId { get; set; }

        public int IssueCount { get; set; }
        public string CardStatus { get; set; }
        public string CardType { get; set; }
        
        public string Action { get; set; }
        public string ImageCamera { get; set; }
        public string OtherCardId { get; set; }
        public string VehicleImage { get; set; }
        public string Videos { get; set; }
        public string VideosVehicle { get; set; }
        public string ResultCheckIn { get; set; }
        public double BodyTemperature { get; set; }
        public double DelayOpenDoorByCamera { get; set; }
        public string Avatar { get; set; }

        /// <summary>
        /// WorkType 
        /// </summary>
        public int? WorkType { get; set; }
        public string WorkTypeName { get; set; }

        /// <summary>
        /// Event Log memo data
        /// </summary>
        public string Memo { get; set; }
        public double Distance { get; set; }
        public double SearchScore { get; set; }
        public double LivenessScore { get; set; }
        public double ObjectType { get; set; }
    }



    public class VehicleReportListModel
    {
        public int Id { get; set; }
        public int? IcuId { get; set; }
        public int? UserId { get; set; }
        public int? VisitId { get; set; }
        public string AccessTime { get; set; }
        public string UserName { get; set; }
        public string BirthDay { get; set; }
        public string EmployeeNumber { get; set; }
        public string Department { get; set; }
        public string CardId { get; set; }
        public string DeviceAddress { get; set; }
        public string DoorName { get; set; }
        public string Building { get; set; }
        public string UserCode { get; set; }

        public string InOut { get; set; }
        public string EventDetail { get; set; }
        public int IssueCount { get; set; }
        public string CardStatus { get; set; }
        public string CardType { get; set; }

        public string Action { get; set; }
        public string ImageCamera { get; set; }
        public string VehicleImage { get; set; }
        public string Videos { get; set; }
        public string ResultCheckIn { get; set; }
        public double BodyTemperature { get; set; }
    }
}
