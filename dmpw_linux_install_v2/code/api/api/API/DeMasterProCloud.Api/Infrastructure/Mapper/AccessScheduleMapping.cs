using System;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.AccessSchedule;
using DeMasterProCloud.DataModel.AccessSchedule;
using Newtonsoft.Json;

namespace DeMasterProCloud.Api.Infrastructure.Mapper;

public class AccessScheduleMapping : Profile
{
    /// <summary>
    /// MealType mapping.
    /// </summary>
    public AccessScheduleMapping()
    {
        CreateMap<AccessSchedule, AccessScheduleListModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.UserQuantity, opt => opt.MapFrom(src => src.UserQuantity))
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.DepartName : string.Empty))
            
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault)))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault)));
        
        CreateMap<AccessScheduleModel, AccessSchedule>()
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.UserQuantity, opt => opt.MapFrom(src => src.UserQuantity))
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ConvertDefaultStringToDateTime(Constants.DateTimeFormatDefault) ?? DateTime.UtcNow))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ConvertDefaultStringToDateTime(Constants.DateTimeFormatDefault) ?? DateTime.UtcNow))
            .ForMember(dest => dest.DoorIds, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.DoorIds)));
            
        CreateMap<AccessSchedule, AccessScheduleDetailModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Content, opt => opt.MapFrom(src => src.Content))
            .ForMember(dest => dest.DoorIds, opt => opt.MapFrom(src => src.DoorIds))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault)))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault)))
            .ForMember(dest => dest.DepartmentId, opt => opt.MapFrom(src => src.DepartmentId))
            .ForMember(dest => dest.UserQuantity, opt => opt.MapFrom(src => src.UserQuantity));
            
            
    }
}