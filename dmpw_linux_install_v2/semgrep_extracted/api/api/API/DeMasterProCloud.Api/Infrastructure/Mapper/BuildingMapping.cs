using System.Web;
using AutoMapper;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Building;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Create mapping for building
    /// </summary>
    public class BuildingMapping : Profile
    {
        /// <summary>
        /// Ctor for mapping Building
        /// </summary>
        public BuildingMapping()
        {
            CreateMap<Building, BuildingListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name));

            CreateMap<BuildingModel, Building>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
                .ForMember(dest => dest.ParentId, opt =>
                {
                    //opt.Condition(m => m.ParentId != null && m.ParentId > 0);
                    opt.MapFrom(src => src.ParentId);
                });
            CreateMap<Building, BuildingModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.PostalCode))
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.TimeZone, opt => opt.MapFrom(src => src.TimeZone))
                .ForMember(dest => dest.ParentId, opt =>
                {
                    //opt.Condition(m => m.ParentId != null && m.ParentId > 0);
                    opt.MapFrom(src => src.ParentId);
                });

            CreateMap<BuildingModel, BuildingDataModel>();
        }
    }
}
