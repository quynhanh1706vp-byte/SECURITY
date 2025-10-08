using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataModel.PlugIn
{
    public class PlugInModel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        
        [Column(TypeName = "jsonb")]
        public string Solutions { get; set; }
        
        [Column(TypeName = "jsonb")]
        public string PlugInsDescription { get; set; }
        
    }

    public class PlugInListModel
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string PlugIns { get; set; }
        public string PlugInsDescription { get; set; }
        public string PlugInsTitle { get; set; }
    }

    public class PlugInSettingModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnable { get; set; }
        public bool IsShowing { get; set; }
    }
    
    public class PlugIns
    {
        public PlugIns()
        {
            Common=true;
            AccessControl = false;
            VisitManagement = false;
            TimeAttendance = false;
            ScreenMessage = false;
            QrCode = false;
            PassCode = false;
            CameraPlugIn = false;
            CameraPlugIn2 = false;
            VehiclePlugIn = false;
            Vein = false;
            DepartmentAccessLevel = false;
            CustomizeLanguage = false;
        }

        public bool Common { get; set; }
        public bool AccessControl { get; set; }
        public bool VisitManagement { get; set; }
        public bool TimeAttendance { get; set; }
        public bool ScreenMessage { get; set; }
        public bool QrCode { get; set; }
        public bool PassCode { get; set; }
        public bool CameraPlugIn { get; set; }
        public bool CameraPlugIn2 { get; set; }
        public bool VehiclePlugIn { get; set; }
        public bool Vein { get; set; }

        public bool DepartmentAccessLevel { get; set; }
        public bool CustomizeLanguage { get; set; }
    }

    public class PlugInsDescription
    {
        public string Common { get; set; }
        public string AccessControl { get; set; }
        public string VisitManagement { get; set; }
        public string TimeAttendance { get; set; }
        public string CanteenManagement { get; set; }
        public string CardIssuing { get; set; }
        public string ScreenMessage { get; set; }
        public string QrCode { get; set; }
        public string PassCode { get; set; }
        public string CameraPlugIn { get; set; }

        public string ArmyManagement { get; set; }



    }
}