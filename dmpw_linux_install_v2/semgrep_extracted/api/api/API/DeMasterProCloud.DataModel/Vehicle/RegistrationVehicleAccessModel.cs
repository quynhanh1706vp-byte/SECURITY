using System;

namespace DeMasterProCloud.DataModel.Vehicle
{
    public class RegistrationVehicleAccessModel
    {
        public int BuildingId { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        public string PlateNumber { get; set; }
        public int VehicleType { get; set; }
        public string VehicleColor { get; set; }
        public string VehicleModel { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Purpose { get; set; }
        public string Remark { get; set; }
        public string MilitaryNumber { get; set; }
        public short WorkType { get; set; }
    }
    
    public class RegistrationPersonAccessModel
    {
        public int BuildingId { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Purpose { get; set; }
        public string Remark { get; set; }
        public string MilitaryNumber { get; set; }
        public short WorkType { get; set; }
    }
}