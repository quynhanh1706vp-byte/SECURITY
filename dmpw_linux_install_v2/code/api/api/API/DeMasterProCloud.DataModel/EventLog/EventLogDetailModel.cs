using DeMasterProCloud.DataModel.Attendance;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.DataModel.Vehicle;
using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.EventLog
{
    public class EventLogDetailModel
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
        public int EventType { get; set; }
        public string EventDetail { get; set; }
        public int IssueCount { get; set; }
        public string CardStatus { get; set; }
        public string CardType { get; set; }
        public string ImageCamera { get; set; }
        public string OtherCardId { get; set; }
        public string VehicleImage { get; set; }
        public string Videos { get; set; }
        public string VideosVehicle { get; set; }
        public string ResultCheckIn { get; set; }
        public double BodyTemperature { get; set; }
        public double DelayOpenDoorByCamera { get; set; }
        public string Avatar { get; set; }
        public double UnixTime { get; set; }
        // public string RelatedUrl { get; set; }
        public int CardTypeId { get; set; }
        public string DoorImage { get; set; }
        public List<EventLogDetailModel> RelatedEventLogs { get; set; }
        public int ObjectType { get; set; }
    }



    public class EventLogByWorkTypeCount
    {
        /// <summary>
        /// Work type value
        /// </summary>
        public short WorkType { get; set; }
        /// <summary>
        /// total count of user
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// a number of user who is in company
        /// </summary>
        public int InCnt { get; set; }
        /// <summary>
        /// a number of user who is not in company
        /// </summary>
        public int OutCnt { get; set; }
    }

    public class EventLogByWorkType
    {
        public short WorkType { get; set; }

        public List <EventLogByUser> EventLogsByUser { get; set; }
    }


    public class EventLogByUser
    {
        public int UserId { get; set; }

        public string DepartmentName { get; set; }

        public string Rank { get; set; }

        public string UserName { get; set; }

        public string Leader { get; set; }

        public string Reason { get; set; }

        public string MilitaryNumber { get; set; }

        public List<CardModel> CardList { get; set; }

        public List<VehicleModel> VehicleList { get; set; }

        public List<EventLogSimpleModel> EventLogs { get; set; }

        public List<AttendanceLeaveModel> AttendanceLeave { get; set; }
    }


    public class EventLogSimpleModel
    {
        public int EventLogId { get; set; }

        public string Antipass { get; set; }

        public int EventType { get; set; }

        //public string EventTime { get; set; }
        public DateTime EventTime { get; set; }

        public int IcuId { get; set; }

        public string DeviceAddress { get; set; }

        public string CardId { get; set; }
    }
}