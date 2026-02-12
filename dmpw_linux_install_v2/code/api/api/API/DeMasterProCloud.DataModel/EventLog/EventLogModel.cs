using System.Collections.Generic;
using DeMasterProCloud.Common.Infrastructure;

namespace DeMasterProCloud.DataModel.EventLog
{
    public class EventLogViewModel
    {
        public string AccessDateFrom { get; set; }
        public string AccessDateTo { get; set; }
        public string AccessTimeFrom { get; set; }
        public string AccessTimeTo { get; set; }
        public short? EventType { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string InOutType { get; set; }
        public string CardId { get; set; }
        public string DoorName { get; set; }
        public short? CardType { get; set; }
        //public IEnumerable<SelectListItem> CardTypeList { get; set; }
        public int? Company { get; set; }
        public IEnumerable<EnumModel> InOutList { get; set; }
        public IEnumerable<EnumModel> EventTypeList { get; set; }
        public IEnumerable<EnumModel> VerifyModeList { get; set; }
        public IEnumerable<SelectListItemModel> DoorList { get; set; }
        public IEnumerable<SelectListItemModel> BuildingList { get; set; }
        public IEnumerable<SelectListItemModel> DepartmentList { get; set; }
        public IEnumerable<SelectListItemModel> CompanyItems { get; set; }
    }

    public class VehicleEventLogViewModel
    {
        public string AccessDateFrom { get; set; }
        public string AccessDateTo { get; set; }
        public string AccessTimeFrom { get; set; }
        public string AccessTimeTo { get; set; }
        public string UserName { get; set; }
        public string PlateNumber { get; set; }

        public short? EventType { get; set; }
        public short? InOut { get; set; }
        public short? DepartmentId { get; set; }
        public short? IsApproved { get; set; }
        public short? BuildingId { get; set; }
        public short? VehicleClassificationId { get; set; }

        public VehicleEventLogViewOptionModel ItemLists { get; set; }
    }

    public class VehicleEventLogViewOptionModel
    {
        public IEnumerable<EnumModel> InOutList { get; set; }
        public IEnumerable<EnumModel> EventTypeList { get; set; }
        public IEnumerable<EnumModel> VehicleClassificationList { get; set; }
        public IEnumerable<EnumModel> ApproveList { get; set; }
        public IEnumerable<SelectListItemModel> BuildingList { get; set; }
        public IEnumerable<SelectListItemModel> DoorList { get; set; }
        public IEnumerable<SelectListItemModel> DepartmentList { get; set; }
    }

    public class EventLogReportViewModel
    {
        public string AccessDateFrom { get; set; }
        public string AccessDateTo { get; set; }
        public string AccessTimeFrom { get; set; }
        public string AccessTimeTo { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public short? EventType { get; set; }
        public string UserCode { get; set; }
        public string UserName { get; set; }
        public string InOutType { get; set; }
        public string CardId { get; set; }
        public string DoorName { get; set; }
        public short? CardType { get; set; }
        //public IEnumerable<SelectListItem> CardTypeList { get; set; }
        public int? Company { get; set; }
        public IEnumerable<EnumModel> InOutList { get; set; }
        public IEnumerable<EnumModel> EventTypeList { get; set; }
        public IEnumerable<EnumModel> VerifyModeList { get; set; }
        public IEnumerable<SelectListItemModel> DoorList { get; set; }
        public IEnumerable<SelectListItemModel> BuildingList { get; set; }
        public IEnumerable<SelectListItemModel> DepartmentList { get; set; }
        public IEnumerable<SelectListItemModel> CompanyItems { get; set; }
        public IEnumerable<EnumModel> CardTypeList { get; set; }
        public IEnumerable<EnumModel> WorkTypeList { get; set; }
    }


    public class EventReportPdfModel
    {
        public string EventTime { get; set; }
        public int? UserCode { get; set; }
        public string FirstName { get; set; }
        public string CardId { get; set; }
        public string DeviceAddress { get; set; }
        public string DoorName { get; set; }
        public string CardType { get; set; }
        public string InOutStatus { get; set; }
        public string EventType { get; set; }
        public string Company { get; set; }
    }

    public class EventLogAccessTimeModel
    {
        public string AccessDateFrom { get; set; }
        public string AccessDateTo { get; set; }
        public string AccessTimeFrom { get; set; }
        public string AccessTimeTo { get; set; }
    }

    public class EventTypeListModel
    {
        public List<EnumModel> EventTypeList { get; set; }
    }

    public class EventCountByDeviceModel
    {
        public int DeviceId { get; set; }
        public int Count { get; set; }
        //public string ProcessId { get; set; }
    }
    public class EventRecoveryProgressModel
    {
        public int DeviceId { get; set; }
        public string ProcessId { get; set; }
    }

    public class EventLogHistory
    {
        public string InOut { get; set; }
        public string AccessTime { get; set; }
        public string CardId { get; set; }
        public int EventDetail { get; set; }
        public string DoorName { get; set; }
        public double UnixTime { get; set; }
        public string Avatar { get; set; }
        public string ImageCamera { get; set; }
        public string OtherCardId { get; set; }
        public string ResultCheckIn { get; set; }
        public int CardTypeId { get; set; }
        public string CardType { get; set; }
        public string Building { get; set; }
    }

    public class BodyTemperatureModel
    {
        public double BodyTemperature { get; set; }
    }

    public class UploadImageEventModel
    {
        public string Id { get; set; }
        public string Hash { get; set; }
        public string Image { get; set; } // string base64
        public string EventTime { get; set; } // Format ddMMyyyyHHmmss (time of device)
        public string CardId { get; set; }
        public List<UpdateImageMultiModeModel> CardRequestList { get; set; }
    }
    
    public class UpdateImageMultiModeModel
    {
        public string Image { get; set; }
        public string CardId { get; set; }
        public int CardType { get; set; }
    }
    public class UploadImageVisitCheckinModel
    {
        public string Id { get; set; }
        public string Hash { get; set; }
        public string Image { get; set; } // string base64
        public int VisitId { get; set; }
    }

    public class ParentDepartmentMonitoring
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int UserLeaveRequest { get; set; }
        public int UserNotLeaveRequest { get; set; }
        public int UserAvailable { get; set; }
        public int TotalUser { get; set; }
        public bool IsTeacher { get; set; }
    }

    /// <summary>
    /// Event model for sqlite (in device)
    /// </summary>
    public class SqliteEventModel
    {
        /// <summary>
        /// Access time value.
        /// Format = yyyyMMddHHmmss
        /// </summary>
        /// <example>
        /// 20231010154820
        /// </example>
        public string EventTime { get; set; }

        /// <summary>
        /// CardId value.
        /// </summary>
        public string CardId { get; set; }

        /// <summary>
        /// Type of event
        /// </summary>
        public short EventType { get; set; }

        /// <summary>
        /// Issue count of card
        /// </summary>
        public short IssCnt { get; set; }

        /// <summary>
        /// CARD type
        /// </summary>
        /// <example>
        /// 0 = CARD
        /// 1 = QR
        /// 2 = Pass code
        /// ...
        /// </example>
        public short IdType { get; set; }

        /// <summary>
        /// In or Out
        /// </summary>
        public string InOutType { get; set; }
    }

}
