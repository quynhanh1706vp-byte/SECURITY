using System;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Timezone;
using DeMasterProCloud.Service.Protocol;
using Newtonsoft.Json;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Define timezone Mapping
    /// </summary>
    public class AccessTimeMapping : Profile
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public AccessTimeMapping()
        {
            CreateMap<AccessTimeModel, AccessTime>()
            .ForMember(dest => dest.Name, opt => opt.Condition(c => c.Position == 1));
            CreateMap<AccessTime, AccessTimeModel>();

            CreateMap<AccessTime, UpdateTimezoneProtocolDetailData>()
                .ForMember(dest => dest.TimezonePosition, opt => opt.MapFrom(src => src.Position))
                .ForMember(dest => dest.ScheduleCount, opt => opt.MapFrom(src => Constants.Settings.NumberTimezoneOfDay))

                .ForPath(dest => dest.Monday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.MonTime1)))
                .ForPath(dest => dest.Monday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.MonTime2)))
                .ForPath(dest => dest.Monday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.MonTime3)))
                .ForPath(dest => dest.Monday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.MonTime4)))

                .ForPath(dest => dest.Tuesday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.TueTime1)))
                .ForPath(dest => dest.Tuesday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.TueTime2)))
                .ForPath(dest => dest.Tuesday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.TueTime3)))
                .ForPath(dest => dest.Tuesday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.TueTime4)))

                .ForPath(dest => dest.Wednesday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.WedTime1)))
                .ForPath(dest => dest.Wednesday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.WedTime2)))
                .ForPath(dest => dest.Wednesday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.WedTime3)))
                .ForPath(dest => dest.Wednesday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.WedTime4)))

                .ForPath(dest => dest.Thursday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.ThurTime1)))
                .ForPath(dest => dest.Thursday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.ThurTime2)))
                .ForPath(dest => dest.Thursday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.ThurTime3)))
                .ForPath(dest => dest.Thursday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.ThurTime4)))

                .ForPath(dest => dest.Friday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.FriTime1)))
                .ForPath(dest => dest.Friday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.FriTime2)))
                .ForPath(dest => dest.Friday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.FriTime3)))
                .ForPath(dest => dest.Friday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.FriTime4)))

                .ForPath(dest => dest.Saturday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.SatTime1)))
                .ForPath(dest => dest.Saturday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.SatTime2)))
                .ForPath(dest => dest.Saturday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.SatTime3)))
                .ForPath(dest => dest.Saturday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.SatTime4)))

                .ForPath(dest => dest.Sunday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.SunTime1)))
                .ForPath(dest => dest.Sunday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.SunTime2)))
                .ForPath(dest => dest.Sunday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.SunTime3)))
                .ForPath(dest => dest.Sunday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.SunTime4)))

                .ForPath(dest => dest.Holiday1.Interval1,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType1Time1)))
                .ForPath(dest => dest.Holiday1.Interval2,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType1Time2)))
                .ForPath(dest => dest.Holiday1.Interval3,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType1Time3)))
                .ForPath(dest => dest.Holiday1.Interval4,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType1Time4)))

                .ForPath(dest => dest.Holiday2.Interval1,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType2Time1)))
                .ForPath(dest => dest.Holiday2.Interval2,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType2Time2)))
                .ForPath(dest => dest.Holiday2.Interval3,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType2Time3)))
                .ForPath(dest => dest.Holiday2.Interval4,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType2Time4)))

                .ForPath(dest => dest.Holiday3.Interval1,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType3Time1)))
                .ForPath(dest => dest.Holiday3.Interval2,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType3Time2)))
                .ForPath(dest => dest.Holiday3.Interval3,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType3Time3)))
                .ForPath(dest => dest.Holiday3.Interval4,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType3Time4)));

            CreateMap<AccessTime, TimezoneProtocolDetailData>()
                .ForMember(dest => dest.TimezonePosition, opt => opt.MapFrom(src => src.Position));

            CreateMap<AccessTime, AccessTimeDetailModel>()
                .ForPath(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForPath(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
                .ForPath(dest => dest.Monday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.MonTime1)))
                .ForPath(dest => dest.Monday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.MonTime2)))
                .ForPath(dest => dest.Monday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.MonTime3)))
                .ForPath(dest => dest.Monday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.MonTime4)))

                .ForPath(dest => dest.Tuesday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.TueTime1)))
                .ForPath(dest => dest.Tuesday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.TueTime2)))
                .ForPath(dest => dest.Tuesday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.TueTime3)))
                .ForPath(dest => dest.Tuesday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.TueTime4)))

                .ForPath(dest => dest.Wednesday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.WedTime1)))
                .ForPath(dest => dest.Wednesday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.WedTime2)))
                .ForPath(dest => dest.Wednesday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.WedTime3)))
                .ForPath(dest => dest.Wednesday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.WedTime4)))

                .ForPath(dest => dest.Thursday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.ThurTime1)))
                .ForPath(dest => dest.Thursday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.ThurTime2)))
                .ForPath(dest => dest.Thursday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.ThurTime3)))
                .ForPath(dest => dest.Thursday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.ThurTime4)))

                .ForPath(dest => dest.Friday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.FriTime1)))
                .ForPath(dest => dest.Friday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.FriTime2)))
                .ForPath(dest => dest.Friday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.FriTime3)))
                .ForPath(dest => dest.Friday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.FriTime4)))

                .ForPath(dest => dest.Saturday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.SatTime1)))
                .ForPath(dest => dest.Saturday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.SatTime2)))
                .ForPath(dest => dest.Saturday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.SatTime3)))
                .ForPath(dest => dest.Saturday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.SatTime4)))

                .ForPath(dest => dest.Sunday.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.SunTime1)))
                .ForPath(dest => dest.Sunday.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.SunTime2)))
                .ForPath(dest => dest.Sunday.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.SunTime3)))
                .ForPath(dest => dest.Sunday.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.SunTime4)))

                .ForPath(dest => dest.Holiday1.Interval1,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType1Time1)))
                .ForPath(dest => dest.Holiday1.Interval2,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType1Time2)))
                .ForPath(dest => dest.Holiday1.Interval3,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType1Time3)))
                .ForPath(dest => dest.Holiday1.Interval4,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType1Time4)))

                .ForPath(dest => dest.Holiday2.Interval1,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType2Time1)))
                .ForPath(dest => dest.Holiday2.Interval2,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType2Time2)))
                .ForPath(dest => dest.Holiday2.Interval3,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType2Time3)))
                .ForPath(dest => dest.Holiday2.Interval4,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType2Time4)))
                .ForPath(dest => dest.Holiday3.Interval1,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType3Time1)))
                .ForPath(dest => dest.Holiday3.Interval2,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType3Time2)))
                .ForPath(dest => dest.Holiday3.Interval3,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType3Time3)))
                .ForPath(dest => dest.Holiday3.Interval4,
                    opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType3Time4)));

            CreateMap<UpdateTimezoneProtocolDetailData, LoadTimezoneProtocolDetailData>()
                .ForPath(dest => dest.Monday.Interval1, opt => opt.MapFrom(src => src.Monday.Interval1))
                .ForPath(dest => dest.Monday.Interval2, opt => opt.MapFrom(src => src.Monday.Interval2))
                .ForPath(dest => dest.Monday.Interval3, opt => opt.MapFrom(src => src.Monday.Interval3))
                .ForPath(dest => dest.Monday.Interval4, opt => opt.MapFrom(src => src.Monday.Interval4))

                .ForPath(dest => dest.Tuesday.Interval1, opt => opt.MapFrom(src => src.Tuesday.Interval1))
                .ForPath(dest => dest.Tuesday.Interval2, opt => opt.MapFrom(src => src.Tuesday.Interval2))
                .ForPath(dest => dest.Tuesday.Interval3, opt => opt.MapFrom(src => src.Tuesday.Interval3))
                .ForPath(dest => dest.Tuesday.Interval4, opt => opt.MapFrom(src => src.Tuesday.Interval4))

                .ForPath(dest => dest.Wednesday.Interval1, opt => opt.MapFrom(src => src.Wednesday.Interval1))
                .ForPath(dest => dest.Wednesday.Interval2, opt => opt.MapFrom(src => src.Wednesday.Interval2))
                .ForPath(dest => dest.Wednesday.Interval3, opt => opt.MapFrom(src => src.Wednesday.Interval3))
                .ForPath(dest => dest.Wednesday.Interval4, opt => opt.MapFrom(src => src.Wednesday.Interval4))

                .ForPath(dest => dest.Thursday.Interval1, opt => opt.MapFrom(src => src.Thursday.Interval1))
                .ForPath(dest => dest.Thursday.Interval2, opt => opt.MapFrom(src => src.Thursday.Interval2))
                .ForPath(dest => dest.Thursday.Interval3, opt => opt.MapFrom(src => src.Thursday.Interval3))
                .ForPath(dest => dest.Thursday.Interval4, opt => opt.MapFrom(src => src.Thursday.Interval4))

                .ForPath(dest => dest.Friday.Interval1, opt => opt.MapFrom(src => src.Friday.Interval1))
                .ForPath(dest => dest.Friday.Interval2, opt => opt.MapFrom(src => src.Friday.Interval2))
                .ForPath(dest => dest.Friday.Interval3, opt => opt.MapFrom(src => src.Friday.Interval3))
                .ForPath(dest => dest.Friday.Interval4, opt => opt.MapFrom(src => src.Friday.Interval4))

                .ForPath(dest => dest.Saturday.Interval1, opt => opt.MapFrom(src => src.Saturday.Interval1))
                .ForPath(dest => dest.Saturday.Interval2, opt => opt.MapFrom(src => src.Saturday.Interval2))
                .ForPath(dest => dest.Saturday.Interval3, opt => opt.MapFrom(src => src.Saturday.Interval3))
                .ForPath(dest => dest.Saturday.Interval4, opt => opt.MapFrom(src => src.Saturday.Interval4))

                .ForPath(dest => dest.Sunday.Interval1, opt => opt.MapFrom(src => src.Sunday.Interval1))
                .ForPath(dest => dest.Sunday.Interval2, opt => opt.MapFrom(src => src.Sunday.Interval2))
                .ForPath(dest => dest.Sunday.Interval3, opt => opt.MapFrom(src => src.Sunday.Interval3))
                .ForPath(dest => dest.Sunday.Interval4, opt => opt.MapFrom(src => src.Sunday.Interval4))

                .ForPath(dest => dest.Holiday1.Interval1,
                    opt => opt.MapFrom(src => src.Holiday1.Interval1))
                .ForPath(dest => dest.Holiday1.Interval2,
                    opt => opt.MapFrom(src => src.Holiday1.Interval2))
                .ForPath(dest => dest.Holiday1.Interval3,
                    opt => opt.MapFrom(src => src.Holiday1.Interval3))
                .ForPath(dest => dest.Holiday1.Interval4,
                    opt => opt.MapFrom(src => src.Holiday1.Interval4))

                .ForPath(dest => dest.Holiday2.Interval1,
                    opt => opt.MapFrom(src => src.Holiday2.Interval1))
                .ForPath(dest => dest.Holiday2.Interval2,
                    opt => opt.MapFrom(src => src.Holiday2.Interval2))
                .ForPath(dest => dest.Holiday2.Interval3,
                    opt => opt.MapFrom(src => src.Holiday2.Interval3))
                .ForPath(dest => dest.Holiday2.Interval4,
                    opt => opt.MapFrom(src => src.Holiday2.Interval4))

                .ForPath(dest => dest.Holiday3.Interval1,
                    opt => opt.MapFrom(src => src.Holiday3.Interval1))
                .ForPath(dest => dest.Holiday3.Interval2,
                    opt => opt.MapFrom(src => src.Holiday3.Interval2))
                .ForPath(dest => dest.Holiday3.Interval3,
                    opt => opt.MapFrom(src => src.Holiday3.Interval3))
                .ForPath(dest => dest.Holiday3.Interval4,
                    opt => opt.MapFrom(src => src.Holiday3.Interval4));
        }

        /// <summary>
        /// Convert json string to datetime
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public string ConvertToDayDetail(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return "";
            }
            var dayDetail = JsonConvert.DeserializeObject<DayDetail>(jsonString);
            var from = !string.IsNullOrEmpty(dayDetail.From.ToString())
                ? TimeSpan.FromMinutes(dayDetail.From).ToString(Constants.DateTimeFormat.Hhmm)
                : "";
            var to = !string.IsNullOrEmpty(dayDetail.From.ToString())
                ? TimeSpan.FromMinutes(dayDetail.To).ToString(Constants.DateTimeFormat.Hhmm)
                : "";
            return from + to;
        }
    }
}
