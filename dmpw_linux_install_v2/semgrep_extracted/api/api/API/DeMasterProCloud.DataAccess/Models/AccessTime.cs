using System;
using System.Collections.Generic;

namespace DeMasterProCloud.DataAccess.Models
{
    public partial class AccessTime
    {
        public AccessTime()
        {
            //DoorAccessTz = new HashSet<IcuDevice>();
            DoorActive = new HashSet<IcuDevice>();
            DoorPassage = new HashSet<IcuDevice>();

            //AccessSetting = new HashSet<AccessSetting>();
        }

        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public int Position { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string FriTime1 { get; set; }
        public string FriTime2 { get; set; }
        public string FriTime3 { get; set; }
        public string FriTime4 { get; set; }
        public string HolType1Time1 { get; set; }
        public string HolType1Time2 { get; set; }
        public string HolType1Time3 { get; set; }
        public string HolType1Time4 { get; set; }
        public string HolType2Time1 { get; set; }
        public string HolType2Time2 { get; set; }
        public string HolType2Time3 { get; set; }
        public string HolType2Time4 { get; set; }
        public string HolType3Time1 { get; set; }
        public string HolType3Time2 { get; set; }
        public string HolType3Time3 { get; set; }
        public string HolType3Time4 { get; set; }
        public string MonTime1 { get; set; }
        public string MonTime2 { get; set; }
        public string MonTime3 { get; set; }
        public string MonTime4 { get; set; }
        public string Remarks { get; set; }
        public string SatTime1 { get; set; }
        public string SatTime2 { get; set; }
        public string SatTime3 { get; set; }
        public string SatTime4 { get; set; }
        public string SunTime1 { get; set; }
        public string SunTime2 { get; set; }
        public string SunTime3 { get; set; }
        public string SunTime4 { get; set; }
        public string ThurTime1 { get; set; }
        public string ThurTime2 { get; set; }
        public string ThurTime3 { get; set; }
        public string ThurTime4 { get; set; }
        public string TueTime1 { get; set; }
        public string TueTime2 { get; set; }
        public string TueTime3 { get; set; }
        public string TueTime4 { get; set; }
        public string WedTime1 { get; set; }
        public string WedTime2 { get; set; }
        public string WedTime3 { get; set; }
        public string WedTime4 { get; set; }
        public bool IsDeleted { get; set; }

        public Company Company { get; set; }
        //public ICollection<IcuDevice> DoorAccessTz { get; set; }
        public ICollection<IcuDevice> DoorActive { get; set; }
        public ICollection<IcuDevice> DoorPassage { get; set; }
        public ICollection<AccessGroupDevice> AccessGroupDevice { get; set; }

        //public ICollection <AccessSetting> AccessSetting { get; set; }
    }
}
