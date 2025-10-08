using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataAccess.Models
{
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Model { get; set; }
        public int VehicleType { get; set; }
        public string PlateNumber { get; set; }
        public string Color { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int CompanyId { get; set; }
        public int? UserId { get; set; }
        public int? VisitId { get; set; }
        public bool IsDeleted { get; set; }

        public User User { get; set; }
        public Visit Visit { get; set; }
        public Company Company { get; set; }
    }
}