using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Visit
{
    public class VisitFilterModel : FilterModel
    {
        public string StartDateFrom { get; set; }
        public string EndDateFrom { get; set; }
        public string AccessDateFrom { get; set; }
        public string AccessDateTo { get; set; }
        public string VisitorName { get; set; }
        public string BirthDay { get; set; }
        public string VisitorDepartment { get; set; }
        public string Position { get; set; }
        public string VisiteeSite { get; set; }
        public string VisitReason { get; set; }
        public string VisiteeName { get; set; }
        public string Phone { get; set; }
        public string ApproverName1 { get; set; }
        public string ApproverName2 { get; set; }
        public string RejectReason { get; set; }
        public string CardId { get; set; }
        public string Search { get; set; }
        public List<int> ProcessStatus { get; set; }
        public bool IsOnyVisitTarget { get; set; }
        public string ExportType { get; set; }
    }
}