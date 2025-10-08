using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.DataModel.EventLog
{
    public class EventMemoModel
    {
        public int Id { get; set; }
        public int EventLogId { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int UpdatedBy { get; set; }
    }

    public class EventMemoListModel
    {
        public int Id { get; set; }
        public int EventLogId { get; set; }
        public string Description { get; set; }
    }
}
