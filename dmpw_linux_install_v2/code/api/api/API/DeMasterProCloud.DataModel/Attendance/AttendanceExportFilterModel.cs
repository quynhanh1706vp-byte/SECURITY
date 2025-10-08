using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Attendance
{
    public class AttendanceExportFilterModel : FilterModel
    {
        public List<int> DepartmentIds { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int CompanyId { get; set; }
        public string Timezone { get; set; }
        public string Language { get; set; }
    }
}