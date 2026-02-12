using System;
using System.Collections.Generic;
using DeMasterProCloud.DataModel.Device;
using DocumentFormat.OpenXml.Office2010.ExcelAc;

namespace DeMasterProCloud.DataModel.EventLog
{
    public class EventLogFilterModel : FilterModel
    {
        public DataTokenScreenMonitoring DataTokenMonitoring { get; set; }
        public List<int> EventTypes { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public List<int> PersonTypes { get; set; }
        public string Search { get; set; }
        public List<string> InOutTypes { get; set; }
        public List<int> InOutIds { get; set; }
        public string CardId { get; set; }
        public List<int> DoorIds { get; set; }
        public List<int> BuildingIds { get; set; }
        public List<int> DepartmentIds { get; set; }
        public List<int> VerifyModeIds { get; set; }
        public int CompanyId { get; set; }
        public List<int> IsValid { get; set; }
        public int AccountId { get; set; }
        public short AccountType { get; set; }
        public List<int> CardTypes { get; set; }
        public List<int> VehicleClassificationIds { get; set; }
        public List<int> WorkTypeIds { get; set; }
        public List<int> ObjectType { get; set; }
    }
    
    public class EventLogRelatedFilterModel : FilterModel
    {
        public int UserId { get; set; }
        public int VisitId { get; set; }
        public int UnknownPersonId { get; set; }
        public int EventLogId { get; set; }
        public List<int> EventTypes { get; set; }
        public DataAccess.Models.EventLog EventLog { get; set; }
    }

    public class EventLogExportFilterModel : FilterModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string UserName { get; set; }
        public string CardId { get; set; }
        public int CompanyId { get; set; }
        public string Culture { get; set; }
        public string Type { get; set; }
        public List<int> EventTypes { get; set; }
        public List<int> InOutType { get; set; }
        public List<int> DoorIds { get; set; }
        public List<int> BuildingIds { get; set; }
        public List<int> DepartmentIds { get; set; }
        public List<int> CardType { get; set; }
        public List<int> IsValid { get; set; }
        public List<int> WorkType { get; set; }
        public TimeSpan OffSet { get; set; }
        public int TotalRecord { get; set; }
        public int ExportAccountId { get; set; }
        public List<int> ObjectType { get; set; }
    }
}