using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.WorkingModel;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Working type mapping
    /// </summary>
    public class WorkingTypeMapping : Profile
    {
        /// <summary>
        /// Working type mapping
        /// </summary>
        public WorkingTypeMapping()
        {
            CreateMap<WorkingModel, WorkingType>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.WorkingDay, opt => opt.MapFrom(src => src.WorkingDay))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.CheckLunchTime, opt => opt.MapFrom(src =>  src.WorkingHourType == (int)WorkingHourType.TotalInCompany ? src.CheckLunchTime : false))
                .ForMember(dest => dest.LunchTime, opt => opt.MapFrom(src => src.LunchTime))
                .ForMember(dest => dest.CheckClockOut, opt => opt.MapFrom(src => src.CheckClockOut))
                .ForMember(dest => dest.UseClockOutDevice, opt => opt.MapFrom(src => src.UseClockOutDevice))
                .ForMember(dest => dest.WorkingHourType, opt => opt.MapFrom(src => src.UseClockOutDevice ? src.WorkingHourType : (int) WorkingHourType.TotalInCompany));

            CreateMap<WorkingType, WorkingModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId))
                .ForMember(dest => dest.WorkingDay, opt => opt.MapFrom(src => src.WorkingDay))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.CheckLunchTime, opt => opt.MapFrom(src => src.CheckLunchTime))
                .ForMember(dest => dest.LunchTime, opt => opt.MapFrom(src => src.LunchTime))
                .ForMember(dest => dest.CheckClockOut, opt => opt.MapFrom(src => src.CheckClockOut))
                .ForMember(dest => dest.UseClockOutDevice, opt => opt.MapFrom(src => src.UseClockOutDevice))
                .ForMember(dest => dest.WorkingHourType, opt => opt.MapFrom(src => src.WorkingHourType));
        }
    }
}