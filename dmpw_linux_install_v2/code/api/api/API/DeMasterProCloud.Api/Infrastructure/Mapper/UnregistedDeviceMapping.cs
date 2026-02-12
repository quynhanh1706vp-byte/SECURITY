using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.UnregistedDevice;
using DeMasterProCloud.Service.Protocol;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// UnregistedDevice mapping
    /// </summary>
    public class UnregistedDeviceMapping : Profile
    {
        /// <summary>
        /// UnregisteredDevice mapping
        /// </summary>
        public UnregistedDeviceMapping()
        {
            CreateMap<UnregistedDevice, UnregistedDeviceModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Connectionstatus, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
                .ForMember(dest => dest.MacAddress, opt => opt.MapFrom(src => src.MacAddress))
                .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => src.DeviceType))
                .ForMember(dest => dest.OperationType, opt => opt.MapFrom(src => ((OperationType)src.OperationType).GetDescription() ));

            CreateMap<UnregistedDeviceModel, UnregistedDevice>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress));

            CreateMap<DeviceConnectionStatusProtocolData, UnregistedDevice>()
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.Data.DeviceAddress))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Data.Status))
                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.Data.IpAddress))
                .ForMember(dest => dest.MacAddress, opt => opt.MapFrom(src => src.Data.MacAddress))
                .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => src.Data.DeviceType));

            CreateMap<UnregistedDevice, DeviceModel>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => "Door" + " " + src.DeviceAddress))
                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress));

        }
    }
}
