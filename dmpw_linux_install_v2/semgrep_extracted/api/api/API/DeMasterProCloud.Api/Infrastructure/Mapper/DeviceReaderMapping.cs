using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.DeviceReader;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    public class DeviceReaderMapping : Profile
    {
        public DeviceReaderMapping()
        {
            CreateMap<DeviceReader, DeviceReaderListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DeviceReaderId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
                .ForMember(dest => dest.MacAddress, opt => opt.MapFrom(src => src.MacAddress))
                .ForMember(dest => dest.IcuDeviceId, opt => opt.MapFrom(src => src.IcuDeviceId))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.IcuDevice.DeviceAddress))
                .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => ((DeviceType)(src.IcuDevice != null ? src.IcuDevice.DeviceType : 0)).GetDescription()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IcuDevice != null ? src.IcuDevice.ConnectionStatus : 0))
                .ForMember(dest => dest.BuildingName, opt => opt.MapFrom(src => src.IcuDevice != null && src.IcuDevice.Building != null ? src.IcuDevice.Building.Name : string.Empty))
                .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.IcuDevice != null ? src.IcuDevice.Name : string.Empty));

            CreateMap<DeviceReaderModel, DeviceReader>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
                .ForMember(dest => dest.MacAddress, opt => opt.MapFrom(src => src.MacAddress))
                .ForMember(dest => dest.IcuDeviceId, opt => opt.MapFrom(src => src.IcuDeviceId));
            
            CreateMap<Camera, DeviceReaderListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IcuDevice.IpAddress))
                .ForMember(dest => dest.MacAddress, opt => opt.MapFrom(src => src.IcuDevice.MacAddress))
                .ForMember(dest => dest.IcuDeviceId, opt => opt.MapFrom(src => src.IcuDeviceId))
                .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => ((CameraType)(src.Type)).GetDescription()))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.CameraId))
                .ForMember(dest => dest.CameraId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IsCamera, opt => opt.MapFrom(src => src.Id > 0))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IcuDevice != null ? src.IcuDevice.ConnectionStatus : 0))
                .ForMember(dest => dest.BuildingName, opt => opt.MapFrom(src => src.IcuDevice != null && src.IcuDevice.Building != null ? src.IcuDevice.Building.Name : string.Empty))
                .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.IcuDevice != null ? src.IcuDevice.Name : string.Empty));

        }
    }
}
