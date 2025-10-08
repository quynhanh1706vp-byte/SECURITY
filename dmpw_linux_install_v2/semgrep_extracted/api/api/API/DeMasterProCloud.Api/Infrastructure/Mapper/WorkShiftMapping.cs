using System;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.WorkShift;
using DeMasterProCloud.DataModel.WorkShift;
using Newtonsoft.Json;

namespace DeMasterProCloud.Api.Infrastructure.Mapper;

public class WorkShiftMapping : Profile
{
    /// <summary>
    /// MealType mapping.
    /// </summary>
    public WorkShiftMapping()
    {
        CreateMap<WorkShift, WorkShiftListModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime));
        
        CreateMap<WorkShiftModel, WorkShift>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime));
            
        CreateMap<WorkShift, WorkShiftDetailModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartTime))
            .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.EndTime));
            
            
    }
}