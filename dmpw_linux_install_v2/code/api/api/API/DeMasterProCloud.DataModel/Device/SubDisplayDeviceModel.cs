using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace DeMasterProCloud.DataModel.Device
{
    public class SubDisplayDeviceModel
    {
        public List<SubDisplayPerson> PersonInfos { get; set; }
    }

    public class SubDisplayVehicle
    {
        public string VehicleModel { get; set; }
        public string PlateNumber { get; set; }
        public string VehicleColor { get; set; }
        public string VehicleType { get; set; }
        public string VehicleClass { get; set; }
        public string VehicleRule { get; set; }
        public bool ExistBlackBox { get; set; }
        public string VehicleStatus { get; set; }

        public string Building { get; set; }
        public string AccessTime { get; set; }
        public string Image { get; set; }
    }

    public class SubDisplayPerson
    {
        public string Name { get; set; }
        public string Grade { get; set; }
        public string Position { get; set; }
        // public string Job { get; set; }
        public string Department { get; set; }
        // public string PersonStatus { get; set; }
        public string Avatar { get; set; }
        
        public int IcuId { get; set; }
        public string DeviceAddress { get; set; }
        public string DeviceName { get; set; }
        public string Building { get; set; }
        // public string AccessTime { get; set; }
        public DateTime EventTime { get; set; }
        // public string EventTypeDescription { get; set; }
        public List<EventModel> EventTypeDescriptions { get; set; }
        public int EventType { get; set; }
        // public string Image { get; set; }
        
        public bool IsVisit { get; set; }
        public string VisitorName { get; set; }
        public List<string> VisitorTypes { get; set; }
        public string VisitorReason { get; set; }
        public string VisitorRank { get; set; }

        public string PlateNumber { get; set; }
        public string ResultCheckIn { get; set; }
        public string InOut { get; set; }
        public int InOutType { get; set; }
        //visit
        public string Address { get; set; }
        public string QrCode { get; set; }
    }

    public class EventModel
    {
        public int Culture { get; set; }
        public string EventName { get; set; }
    }

    public class GenerateTokenSubDisplayModel
    {
        public int Type { get; set; }
        public List<int> DeviceIds { get; set; }
        public int Count { get; set; }
        public bool IsUseDesign { get; set; }
        public string LocationOriginMonitoring { get; set; }
        public List<int> EventType { get; set; }
        public bool EnableDisplayListVisitor { get; set; }
        public bool EnableDisplayAbnormalEvent { get; set; }
        public bool EnableDisplayCanteenEvent { get; set; }
        public string TimeReset { get; set; }
        public string TimeStartCheckIn { get; set; }
        public string TimeEndCheckIn { get; set; }
        public string TimeStartCheckOut { get; set; }
        public string TimeEndCheckOut { get; set; }
        public bool EnableIgnoreDuplicatedEvent { get; set; }
        public List<int> ParentDepartment { get; set; }
        public bool IsCheckTeacherOut { get; set; }
    }

    public class DataTokenScreenMonitoring
    {
        public int CompanyId { get; set; }
        public List<int> DeviceIds { get; set; }
        public string Timezone { get; set; }
        public bool EnableDisplayListVisitor { get; set; }
        public string TimeReset { get; set; }
        public string TimeStartCheckIn { get; set; }
        public string TimeEndCheckIn { get; set; }
        public string TimeStartCheckOut { get; set; }
        public string TimeEndCheckOut { get; set; }
        public List<int> ParentDepartment { get; set; }
        public bool IsCheckTeacherOut { get; set; }
    }
}