using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.User
{
    public class UserFilterModel : FilterModel
    {
        public string FirstName { get; set; }
        public string Position { get; set; }
        public string CardId { get; set; }
        public string Search { get; set; }
        public List<int> DepartmentIds { get; set; }
        public string EmpNumber { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public DateTime? ExpiredDateEnd { get; set; }
        public List<int> WorkTypeIds { get; set; }

        public List<int> Status { get; set; }

        /// <summary>
        /// A list of category option identifier
        /// </summary>
        public List<int> Category { get; set; }

        public UserFilterAddOns AddOns { get; set; }
    }

    public class UserFilterAddOns
    {   
        public List<short> faceAndIris {  get; set; }
    }
}