using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class HeaderSetting
    {
        public HeaderSetting()
        {

        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int? CompanyId { get; set; }
        public int AccountId { get; set; }

        public string HeaderList { get; set; }
        
        public DateTime UpdatedOn { get; set; }
        public Company Company { get; set; }

        public Account Account { get; set; }
    }
}
