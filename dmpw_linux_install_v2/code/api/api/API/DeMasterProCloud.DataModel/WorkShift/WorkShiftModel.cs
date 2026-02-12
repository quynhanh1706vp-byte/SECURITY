using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataModel.WorkShift
{
    public class WorkShiftModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
       
        public string StartTime { get; set; }
        public string EndTime { get; set; }

    }
    public class WorkShiftDetailModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
      
    }

    
    
    public class WorkShiftListModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
       
    }

}