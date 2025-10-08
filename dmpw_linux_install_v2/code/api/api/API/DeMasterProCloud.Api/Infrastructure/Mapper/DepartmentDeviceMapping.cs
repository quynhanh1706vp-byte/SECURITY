using AutoMapper;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.DepartmentDevice;

namespace DeMasterProCloud.Api.Infrastructure.Mapper;

public class DepartmentDeviceMapping : Profile
{
    public DepartmentDeviceMapping()
    {
        CreateMap<DepartmentDevice, DepartmentDeviceModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IcuId))
            .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.Icu.DeviceAddress))
            .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Icu.Name));

        CreateMap<IcuDevice, DepartmentDeviceModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
            .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name));
    }
}