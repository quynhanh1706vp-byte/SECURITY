using System;
using AutoMapper;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Holiday;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.Service.Protocol;
using System.Collections.Generic;
using System.Linq;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Define HolidayMapping
    /// </summary>
    public class HolidayMapping : Profile
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public HolidayMapping()
        {
            CreateMap<HolidayModel, Holiday>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CompanyId, opt => opt.Ignore());

            CreateMap<Holiday, HolidayModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.Remarks ?? string.Empty));

            CreateMap<Holiday, HolidayListModel>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.HolidayType, opt => opt.MapFrom(src => ((HolidayType)src.Type).GetDescription()))
                .ForMember(dest => dest.RecursiveDisp, opt => opt.MapFrom(src => src.Recursive ? CommonResource.lblYes : CommonResource.lblNo));

            CreateMap<Holiday, HolidayProtocolDataDetail>()
                .ForMember(dest => dest.HolidayType, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Recurring, opt => opt.MapFrom(src => src.Recursive ? 1 : 0))
                .ForMember(dest => dest.HolidayDate,
                    opt => opt.MapFrom(src => GetListDates(src.StartDate, src.EndDate)));

            CreateMap<Holiday, HolidayDetail>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.HolidayType, opt => opt.MapFrom(src => ((HolidayType)src.Type).GetDescription()))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.Recursive, opt => opt.MapFrom(src => src.Recursive ? 1 : 0));

            CreateMap<LoadHolidayDetail, SendHolidayDetail>()
                .ForMember(dest => dest.HolidayType,
                    opt => opt.MapFrom(src => src.HolidayType))
                .ForMember(dest => dest.Recursive, opt => opt.MapFrom(src => src.Recurring));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<Dates> GetListDates(DateTime startDate, DateTime endDate)
        {
            var holidayDates = new List<Dates>();
            var listDate = DateTimeHelper.GetListRangeDate(startDate, endDate);
            if (listDate.Any())
            {
                foreach (var date in listDate)
                {
                    var dates = new Dates { Date = date.ToString(Constants.DateTimeFormat.DdMMyyyy) };
                    holidayDates.Add(dates);
                }
            }
            return holidayDates;
        }

    }
}
