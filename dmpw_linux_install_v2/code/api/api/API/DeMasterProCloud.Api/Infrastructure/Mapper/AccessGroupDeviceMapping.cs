using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.AccessGroupDevice;
using DeMasterProCloud.DataModel.Device;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// AccessGroupDevice Mapping
    /// </summary>
    public class AccessGroupDeviceMapping : Profile
    {
        /// <summary>
        /// AccessGroupDevice mapping
        /// </summary>
        public AccessGroupDeviceMapping()
        {
            CreateMap<AccessGroupDevice, AccessGroupDeviceDoor>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IcuId))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.Icu.DeviceAddress))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Icu.Name))
                .ForMember(dest => dest.TzId, opt => opt.MapFrom(src => src.Tz.Id))
                .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Tz.Name))
                .ForMember(dest => dest.Building, opt => opt.MapFrom(src => src.Icu.Building.Name));

            CreateMap<AccessGroupDevice, AccessibleDoorModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.IcuId))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Icu.Name))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.Icu.DeviceAddress))
                .ForMember(dest => dest.ActiveTz, opt => opt.MapFrom(src => src.Icu.ActiveTz.Name))
                .ForMember(dest => dest.PassageTz, opt => opt.MapFrom(src => src.Icu.PassageTz.Name))
                .ForMember(dest => dest.AccessGroupTz, opt => opt.MapFrom(src => src.Tz.Name))
                .ForMember(dest => dest.VerifyMode,
                    opt => opt.MapFrom(src => ((VerifyMode)src.Icu.VerifyMode).GetDescription()))
                .ForMember(dest => dest.AntiPassback, opt => opt.MapFrom(src => src.Icu.PassbackRule))
                .ForMember(dest => dest.DeviceType,
                    opt => opt.MapFrom(src => ((DeviceType)src.Icu.DeviceType).GetDescription()))
                .ForMember(dest => dest.Mpr, opt => opt.MapFrom(src => src.Icu.MPRCount));
        }
    }
}
