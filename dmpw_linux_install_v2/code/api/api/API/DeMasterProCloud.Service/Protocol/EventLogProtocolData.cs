using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DeMasterProCloud.Service.Protocol
{
    /// <summary>
    /// Receive event log from icu
    /// </summary>
    public class ReceiveEventLogProtocolData : ProtocolData<ReceiveEventLogHeaderData>
    {

    }

    public class ReceiveEventLogHeaderData
    {
        public ReceiveEventLogHeaderData()
        {
            Events = new List<ReceiveEventLogDetailData>();
        }
        public int Total { get; set; }
        public List<ReceiveEventLogDetailData> Events { get; set; }

        public int UtcHour { get; set; }
        public int UtcMinute { get; set; }
    }

    public class ReceiveEventLogDetailData
    {
        public string DeviceAddress { get; set; }
        public string AccessTime { get; set; }
        public string CardId { get; set; }
        public int? IssueCount { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string UpdateTime { get; set; }
        public string InOut { get; set; }
        public int EventType { get; set; }
        public int IdType { get; set; }
        public int CornerId { get; set; }
        public double Temperature { get; set; }
        public double DelayOpenDoorByCamera { get; set; }

        /// <summary>
        /// This type means whether the event is online or offline.
        /// 0 = Online.
        /// 1 = Offline.
        /// </summary>
        public int SendType { get; set; }

        /// <summary>
        /// This variable is for delivering images directly without going through their own servers.
        /// </summary>
        public string ImageFile { get; set; }
        public double Distance { get; set; }
        public double SearchScore { get; set; }
        public double LivenessScore { get; set; }
        public string OtherCardId { get; set; }
    }

    public class OtherCardListModel
    {
        public int CardType { get; set; }
        public string CardId { get; set; }
    }

    /// <summary>
    /// Send event log to webapp
    /// </summary>
    public class SendEventLogListModelData : ProtocolData<SendEventLogHeaderData>
    {
    }

    public class SendEventLogHeaderData
    {
        public SendEventLogHeaderData()
        {
            Events = new List<SendEventLogDetailData>();
        }
        public int Total { get; set; }
        public List<SendEventLogDetailData> Events { get; set; }
    }

    public class SendEventLogDetailData
    {
        public Guid Id { get; set; }
        public object EventLogId { get; set; }
        public int IcuId { get; set; }
        public int? UserId { get; set; }
        /// <summary>
        /// identifier of visit
        /// </summary>
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
        public int CardTypeId { get; set; }
        public string CardList { get; set; }
        public string IssueCount { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public string ResultCheckIn { get; set; }
        public string Department { get; set; }
        public int ParentDepartmentId { get; set; }
        public string Parent { get; set; }
        public int EventType { get; set; }

        public string InOut { get; set; }
        public int InOutType { get; set; }
        
        public double UnixTime { get; set; }
        
        public int BuildingId { get; set; }
        public double BodyTemperature { get; set; }
        public string AllowedBelonging { get; set; }

        //Field for canteen
        public string DeviceName { get; set; }
        public string DeviceAddress { get; set; }
        public string DepartmentName { get; set; }
        public string CornerName { get; set; }
        public string Building { get; set; }
        public string MealType { get; set; }
        public decimal Amount { get; set; }
        public decimal ExceptionalMealAmount { get; set; }
        public decimal ExceptionalUserAmount { get; set; }
        public int AppliedDiscount { get; set; }

        /// <summary>
        /// Variables that distinguish user types of events
        /// 0 : Nomal
        /// 1 : Visit
        /// </summary>
        public int UserType { get; set; }
        public string PersonTypeArmy { get; set; }
        public List<int> DeviceManagerIds { get; set; }
        public string Position { get; set; }
        public bool IsVisit { get; set; }

        public int? WorkType { get; set; }

        public string WorkTypeName { get; set; }
        public string ImageCamera { get; set; }
        public double Distance { get; set; }
        public double SearchScore { get; set; }
        public double LivenessScore { get; set; }
        public string NationalIdNumber { get; set; }
        public string Videos { get; set; }
        public string OtherCardId { get; set; }
        public int ObjectType { get; set; }
    }

    /// <summary>
    /// Response event log
    /// </summary>
    public class ResponseEventLogProtocolData : ProtocolData<ResponseEventLogHeader>
    {
    }

    public class ResponseEventLogHeader
    {
        public int Total { get; set; }
    }

    public class ResponseEventLogHeaderThirdParty : ResponseEventLogHeader
    {
        public List<ResponseEventLogDetail> Events { get; set; }
    }

    public class ResponseEventLogDetail
    {
        public int EventLogId { get; set; }
        public string AccessTime { get; set; }
        public string DeviceAddress { get; set; }
        public short EventType { get; set; }
        public string CardId { get; set; }
        public string AccessedId { get; set; }
        public short CardType { get; set; }
        public int IssueCount { get; set; }
        public string DepartmentCode { get; set; }
        public string DepartmentName { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public int VisitId { get; set; }
        public string Antipass { get; set; }
        public string EmployeeNumber { get; set; }
        public bool? IsMasterCard { get; set; }
    }

    /// <summary>
    /// Send event log to webapp
    /// </summary>
    public class SendVehicleEventLogListModelData : ProtocolData<SendVehicleEventLogHeaderData>
    {
    }

    public class SendVehicleEventLogHeaderData
    {
        public SendVehicleEventLogHeaderData()
        {
            Events = new List<SendVehicleEventLogDetailData>();
        }
        public int Total { get; set; }
        public List<SendVehicleEventLogDetailData> Events { get; set; }
    }

    public class SendVehicleEventLogDetailData
    {
        public Guid Id { get; set; }
        public int EventLogId { get; set; }
        public int IcuId { get; set; }
        public int? UserId { get; set; }
        public int? VisitId { get; set; }
        //public DateTime EventTime { get; set; }
        public string EventTime { get; set; }
        public string DeviceAddress { get; set; }
        public string DoorName { get; set; }
        public string EventDetail { get; set; }
        public int EventDetailCode { get; set; }
        public string PlateNumber { get; set; }
        public string Model { get; set; }
        public int CardType { get; set; }
        public string UserName { get; set; }
        public string DepartmentName { get; set; }
        public string InOut { get; set; }
        public List<int> DeviceManagerIds { get; set; }
        public string VehicleImage { get; set; }

        public double UnixTime { get; set; }

    }
    
    public class ResponseEventLogIcuProtocolData : ProtocolData<ResponseEventLogIcuDetail>
    {
    }

    public class ResponseEventLogIcuDetail
    {
        public int Total { get; set; }
    }
}
