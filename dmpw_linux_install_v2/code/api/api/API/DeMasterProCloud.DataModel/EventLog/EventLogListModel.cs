using DeMasterProCloud.DataModel.Category;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.EventLog
{
    /// <summary>
    /// EventLog List class
    /// </summary>
    public class EventLogListModel
    {
        public int Id { get; set; }
        public int EventLogId { get; set; }
        public int? IcuId { get; set; }
        public int? UserId { get; set; }
        public string Avatar { get; set; }
        public int? VisitId { get; set; }
        public string AccessTime { get; set; }
        public string EventTime { get; set; }
        public string UserName { get; set; }
        public string Department { get; set; }
        public string DepartmentName { get; set; }
        public int ParentDepartmentId { get; set; }
        public string Parent { get; set; }
        public string CardId { get; set; }
        public bool IsRegisteredCard { get; set; }
        public string Device { get; set; }
        public string DeviceAddress { get; set; }
        public string DoorName { get; set; }
        public string Building { get; set; }
        public string VerifyMode { get; set; }
        public string CardType { get; set; }
        public int CardTypeId { get; set; }
        public string ExpireDate { get; set; }
        /// <summary>
        /// This value is about user's workType.
        /// Or this value can be Visitor's type when the event is about visitor.
        /// </summary>
        public short Type { get; set; }
        public string InOut { get; set; }
        public int InOutType { get; set; }
        public short EventType { get; set; }
        public string EventDetail { get; set; }
        public int IssueCount { get; set; }
        public string CardStatus { get; set; }
        public string ImageCamera { get; set; }
        public string OtherCardId { get; set; }
        public string ResultCheckIn { get; set; }
        public string Videos { get; set; }
        public double BodyTemperature { get; set; }
        public string AllowedBelonging { get; set; }
        public double UnixTime { get; set; }

        /// <summary>
        /// Variables that distinguish user types of events
        /// 0 : Nomal
        /// 1 : Visit
        /// </summary>
        public int? UserType { get; set; }
        public string PersonTypeArmy { get; set; }
        public List<int> DeviceManagerIds { get; set; }

        public int? WorkType { get; set; }

        public string WorkTypeName { get; set; }


        public string Memo { get; set; }
        public int ObjectType { get; set; }
        
        // field of visit
        public int VisitTargetId { get; set; }
        public string VisitTargetName { get; set; }
        public string VisitTargetDepartment { get; set; }
    }


    public class VehicleEventLogListModel
    {
        public int Id { get; set; }
        public string EventTime { get; set; }

        public double UnixTime { get; set; }

        public string PlateNumber { get; set; }
        public string Model { get; set; }
        public string DoorName { get; set; }
        public string UserName { get; set; }
        public string DepartmentName { get; set; }
        public string InOut { get; set; }
        public short EventDetailCode { get; set; }
        public string EventDetail { get; set; }
        public int? UserId { get; set; }
        public int? VisitId { get; set; }

        public string VehicleType { get; set; }
        public string VehicleColor { get; set; }
        public string VehicleImage { get; set; }

        public List<UserCategoryDataModel> CategoryOptions { get; set; }
        public List<int> DeviceManagerIds { get; set; }
        public int ObjectType { get; set; }

        //public int EventLogId { get; set; }
        //public int? IcuId { get; set; }
        //public string Avatar { get; set; }
        //public bool IsRegisteredCard { get; set; }
        //public string Device { get; set; }
        public string Building { get; set; }
        //public string VerifyMode { get; set; }
        public string CardType { get; set; }
        public int CardTypeId { get; set; }
        //public string ExpireDate { get; set; }
        //[JsonIgnore]
        //public short Type { get; set; }
        //public int IssueCount { get; set; }
        //public string CardStatus { get; set; }
        //public string ImageCamera { get; set; }
        //public string ResultCheckIn { get; set; }
        //public double BodyTemperature { get; set; }
        //public double UnixTime { get; set; }

        /// <summary>
        /// Variables that distinguish user types of events
        /// 0 : Nomal
        /// 1 : Visit
        /// </summary>
        //public int? UserType { get; set; }
    }
}
