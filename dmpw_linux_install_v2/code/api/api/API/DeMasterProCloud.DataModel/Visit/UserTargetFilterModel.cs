using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Visit
{
    public class UserTargetFilterModel : FilterModel
    {
        public string Name { get; set; }
        public List<int> DepartmentIds { get; set; }
        public int CompanyId { get; set; }
        public bool BothUserNoHasAccount { get; set; }
    }
}