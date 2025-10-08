using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel.Category;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DeMasterProCloud.DataModel.Vehicle
{
    public class VehicleModel
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public int VehicleType { get; set; }
        public string PlateNumber { get; set; }
        public string Color { get; set; }
    }
    public class VehicleListModel : VehicleModel
    {
        public string UserName { get; set; }
        public int? UserId { get; set; }
        public int? VisitId { get; set; }
    }

    public class PersonalVehicleModel
    {
        public int Id { get; set; }

        public string DepartmentName { get; set; }

        public string UserName { get; set; }

        public string PlateNumber { get; set; }

        public string Model { get; set; }
        public int VehicleType { get; set; }

        public string Color { get; set; }

        public string VehicleName { get; set; }

        public bool ExistBlackBox { get; set; }

        public bool IsAllowed { get; set; }

        public int VehicleRule { get; set; }

        public int VehicleRuleOption { get; set; }
    }

    public class PersonalVehicleOptionModel : PersonalVehicleModel
    {
        public IEnumerable<EnumModel> VehicleRules { get; set; }

        public IEnumerable<EnumModel> DayOfWeekOptions { get; set; }

        //public IEnumerable<EnumModel> EvenOddOptions { get; set; }
    }


    public class PersonalVehicleListModel
    {
        public int Id { get; set; }

        public string DepartmentName { get; set; }

        public string UserName { get; set; }

        public string PlateNumber { get; set; }

        public string Model { get; set; }
        public int VehicleType { get; set; }

        public string Color { get; set; }

        public bool ExistBlackBox { get; set; }

        public bool IsAllowed { get; set; }
        public int VehicleRule { get; set; }
    }


    public class VisitVehicleModel
    {
        public string PlateNumber { get; set; }

        public string Model { get; set; }
        public int VehicleType { get; set; }

        public string Color { get; set; }
    }
    
    public class VehicleInOutStatusListModel
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string UserName { get; set; }

        public string WorkTypeName { get; set; }

        public string DepartmentName { get; set; }

        public string Reason { get; set; }

        public DateTime LastEventTime { get; set; }

        public string PlateNumber { get; set; }

        public string Model { get; set; }

        public string Color { get; set; }

        public string VehicleName { get; set; }
    }

    public class DashBoardVehicleModel
    {
        public int TotalVehicles { get; set; }
        public int TotalVehiclesIn { get; set; }
        public int TotalVehiclesOut { get; set; }
        public int TotalVehiclesOfUser { get; set; }
        public int TotalVehiclesOfVisit { get; set; }
    }
}