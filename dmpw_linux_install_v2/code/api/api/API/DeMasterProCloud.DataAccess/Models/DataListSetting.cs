using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class DataListSetting
    {
        public DataListSetting()
        {

        }

        /// <summary>
        /// index of data
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// name value
        /// (not used)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// identifier of company
        /// </summary>
        public int? CompanyId { get; set; }

        /// <summary>
        /// identifier of account
        /// </summary>
        public int AccountId { get; set; }

        /// <summary>
        /// Data List
        /// </summary>
        public string DataList { get; set; }

        /// <summary>
        /// Type of data used
        /// </summary>
        /// <example>
        /// 1: Header data
        /// 2: Door List in Monitoring page
        /// 3: Event List in Monitoring page
        /// </example>
        public int DataType { get; set; }

        /// <summary>
        /// DateTime value when the data is updated
        /// </summary>
        public DateTime UpdatedOn { get; set; }

        public Company Company { get; set; }

        public Account Account { get; set; }
    }
}
