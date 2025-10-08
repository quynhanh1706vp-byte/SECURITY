using AutoMapper;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Attendance;


namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Attendance Mapping
    /// </summary>
    public class AttendanceMapping : Profile
    {
        /// <summary>
        /// Attendance Mapping
        /// </summary>
        public AttendanceMapping()
        {
            CreateMap<AttendanceModel, Attendance>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));
            
            CreateMap<LeaveRequestSetting, LeaveSettingModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.NumberDayOffYear, opt => opt.MapFrom(src => src.NumberDayOffYear))
                .ForMember(dest => dest.NumberDayOffPreviousYear, opt => opt.MapFrom(src => src.NumberDayOffPreviousYear));
        }
    }
}