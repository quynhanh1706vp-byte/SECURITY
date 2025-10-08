using System;

namespace DeMasterProCloud.DataModel.VisitArmy
{
    public class VisitCardInfo
    {
        public string CardId { get; set; }
        public int VisitId { get; set; }
        public string ManagementNumber { get; set; }
        public int CardRoleType { get; set; }
        public int CardStatus { get; set; }
        public DateTime AssignedCardDate { get; set; }
        
        public int VisitType { get; set; }
        public string VisitorName { get; set; }
        public DateTime Birthday { get; set; }
        public string Position { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string VisitPlace { get; set; }
        public string VisitPurpose { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public string VisiteeName { get; set; }
        public string DepartmentName { get; set; }
    }
}