using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Vehicle;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    public class VehicleMapping : Profile
    {
        public VehicleMapping()
        {
            CreateMap<VehicleModel, Vehicle>()
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
                .ForMember(dest => dest.VehicleType, opt => opt.MapFrom(src => src.VehicleType))
                .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber.Replace(" ", "")));
            

            CreateMap<Vehicle, VehicleModel>()
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
                .ForMember(dest => dest.VehicleType, opt => opt.MapFrom(src => src.VehicleType))
                .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber));
            
            CreateMap<VehicleImportModel, Vehicle>()
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color.Value))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model.Value))
                .ForMember(dest => dest.VehicleType, opt => opt.MapFrom(src => src.VehicleType.Value))
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.VisitId, opt => opt.Ignore())
                .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber.Value));

            CreateMap<Vehicle, VehicleListModel>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserId != null ? src.User.FirstName : src.Visit.VisitorName))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.VisitId, opt => opt.MapFrom(src => src.VisitId))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
                .ForMember(dest => dest.VehicleType, opt => opt.MapFrom(src => src.VehicleType))
                .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber));


            CreateMap<Vehicle, PersonalVehicleListModel>()
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
                .ForMember(dest => dest.VehicleType, opt => opt.MapFrom(src => src.VehicleType))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.UserName, opt =>
                {
                    opt.Condition(src => src.User != null);
                    opt.MapFrom(src => src.User.FirstName);
                })
                .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber));


            CreateMap<Vehicle, PersonalVehicleModel>()
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
                .ForMember(dest => dest.VehicleType, opt => opt.MapFrom(src => src.VehicleType))
                .ForMember(dest => dest.UserName, opt =>
                {
                    opt.Condition(src => src.User != null);
                    opt.MapFrom(src => src.User.FirstName);
                })
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber))
                .Include<Vehicle, PersonalVehicleOptionModel>();
            CreateMap<Vehicle, PersonalVehicleOptionModel>();


            CreateMap<PersonalVehicleModel, Vehicle>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
                .ForMember(dest => dest.VehicleType, opt => opt.MapFrom(src => src.VehicleType))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber));

            CreateMap<VisitVehicleModel, Vehicle>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model))
                .ForMember(dest => dest.VehicleType, opt => opt.MapFrom(src => src.VehicleType))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Color))
                .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.PlateNumber));
        }
    }
}