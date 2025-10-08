using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel.User;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Department
{
    public class DepartmentModel
    {
        [JsonIgnore]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public int? ParentId { get; set; }
        public int? DepartmentManagerId { get; set; }
        public int MaxNumberCheckout { get; set; }
        public double MaxPercentCheckout { get; set; }
        //public IEnumerable<SelectListItem> ParentDepartments { get; set; }

        /// <summary>
        /// for connection between department and accessGroup
        /// </summary>
        public int? AccessGroupId { get; set; }
    }

    public class DepartmentDataModel : DepartmentModel
    {
        public IEnumerable<EnumModel> AccessGroups { get; set; }
    }

    public class DepartmentImportExportModel
    {
        public Field<string> Name { get; set; }
        public Field<string> Number { get; set; }
        public Field<int?> ParentId { get; set; }
        public Field<int> MaxNumberCheckout { get; set; }
        public Field<int> MaxPercentCheckout { get; set; }
        public bool IsValid { get; set; }
    }


    public class DepartmentSimpleDataModel
    {
        /// <summary>
        /// identifier of department (DB idx)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// number of department
        /// </summary>
        public string DepartmentNumber { get; set; }

        /// <summary>
        /// name of department
        /// </summary>
        public string DepartmentName { get; set; }
    }
}
