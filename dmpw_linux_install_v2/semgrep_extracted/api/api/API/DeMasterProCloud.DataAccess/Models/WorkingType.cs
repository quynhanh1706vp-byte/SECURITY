using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataAccess.Models
{
    public class WorkingType
    {
        public WorkingType()
        {
            IsDefault = false;
            UseClockOutDevice = true;
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public int CompanyId { get; set; }

        [Column(TypeName = "jsonb")]
        public string WorkingDay { get; set; }

        public bool IsDefault { get; set; }

        public bool CheckClockOut { get; set; } = true;

        public bool UseClockOutDevice { get; set; } = true;

        public int WorkingHourType { get; set; }

        public bool CheckLunchTime { get; set; }

        [Column(TypeName = "jsonb")]
        public string LunchTime { get; set; }

        public virtual Company Company { get; set; }
        
        public ICollection<User> User { get; set; }
    }

}