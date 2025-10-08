using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.Department
{
    public class DepartmentListModel
    {
        public int Id { get; set; }
        public string DepartmentNumber { get; set; }
        public string DepartmentName { get; set; }
        public string ParentDepartment { get; set; }
        public int MaxNumberCheckout { get; set; }
        public double MaxPercentCheckout { get; set; }
        public bool IsRoot { get; set; }
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }

        /// <summary>   Gets or sets the identifier of the department manager. 
        ///             This value is needed to display existing information when editing department. </summary>
        /// <value> The identifier of the department manager. </value>
        public string DepartmentManagerId { get; set; }
        public string DepartmentManager { get; set; }
        
        public int NumberUser { get; set; }
    }


    public class DepartmentListItemModel
    {
        public int Id { get; set; }
        public string DepartmentNumber { get; set; }
        public string DepartmentName { get; set; }
        public string ParentDepartment { get; set; }
        public int MaxNumberCheckout { get; set; }
        public double MaxPercentCheckout { get; set; }
        public bool IsRoot { get; set; }
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }

        /// <summary>   Gets or sets the identifier of the department manager. 
        ///             This value is needed to display existing information when editing department. </summary>
        /// <value> The identifier of the department manager. </value>
        public string DepartmentManagerId { get; set; }
        public string DepartmentManager { get; set; }

        public int NumberUser { get; set; }
        public IList<DepartmentListItemModel> Children { get; set; }
        public int ParentId { get; set; }


        /// <summary>
        /// A name of accessGroup 
        /// </summary>
        public string AccessGroupName { get; set; }

        /// <summary>
        /// an identifier of accessGroup.
        /// </summary>
        public int? AccessGroupId { get; set; }
    }
}
