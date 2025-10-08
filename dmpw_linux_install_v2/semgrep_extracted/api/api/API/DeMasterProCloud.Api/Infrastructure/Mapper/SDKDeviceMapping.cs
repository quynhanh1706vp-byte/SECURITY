using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.DeviceSDK;
using DeMasterProCloud.DataModel.Timezone;
using DeMasterProCloud.Service.Protocol;
using Newtonsoft.Json;

namespace DeMasterProCloud.Api.Infrastructure.Mapper;

public class SDKDeviceMapping : Profile
{
    public SDKDeviceMapping()
    {
        CreateMap<SDKSettingModel, SDKLoginModel>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.Username))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password));
        
        CreateMap<SDKDeviceConnectionModel, UnregistedDevice>()
            .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
            .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
            .ForMember(dest => dest.MacAddress, opt => opt.MapFrom(src => src.MacAddress))
            .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => src.DeviceType));

        CreateMap<IcuDevice, SDKUpdateDeviceConfigModel>()
            .ForMember(dest => dest.AntiPassback,opt => opt.MapFrom(src => src.PassbackRule))
            .ForMember(dest => dest.VerifyMode,opt => opt.MapFrom(src => src.VerifyMode))
            .ForMember(dest => dest.LockOpenDuration,opt => opt.MapFrom(src => src.OpenDuration))
            .ForMember(dest => dest.DoorSensorType,opt => opt.MapFrom(src => src.SensorType))
            .ForMember(dest => dest.SensorDuration,opt => opt.MapFrom(src => src.SensorDuration))
            .ForMember(dest => dest.SensorAlarm,opt => opt.MapFrom(src => src.SensorAlarm))
            .ForMember(dest => dest.CloseReverseLockFlag,opt => opt.MapFrom(src => src.CloseReverseLockFlag))
            .ForMember(dest => dest.MPRCount,opt => opt.MapFrom(src => src.MPRCount))
            .ForMember(dest => dest.MPRInterval,opt => opt.MapFrom(src => src.MPRInterval))
            .ForMember(dest => dest.BackupPeriod,opt => opt.MapFrom(src => src.BackupPeriod))
            .ForMember(dest => dest.QrAesKey,opt => opt.MapFrom(src => src.Company.SecretCode))
            .ForMember(dest => dest.DeviceBuzzer,opt => opt.MapFrom(src => src.DeviceBuzzer))
            .ForMember(dest => dest.OperationMode,opt => opt.MapFrom(src => src.OperationType))
            .ForMember(dest => dest.CompanyCode,opt => opt.MapFrom(src => src.Company.Code));
        
        CreateMap<SDKUpdateDeviceConfigModel, LprSDKUpdateDeviceConfigModel>()
            .ForMember(dest => dest.AntiPassback,opt => opt.MapFrom(src => src.AntiPassback))
            .ForMember(dest => dest.VerifyMode,opt => opt.MapFrom(src => src.VerifyMode))
            .ForMember(dest => dest.LockOpenDuration,opt => opt.MapFrom(src => src.LockOpenDuration))
            .ForMember(dest => dest.DoorSensorType,opt => opt.MapFrom(src => src.DoorSensorType))
            .ForMember(dest => dest.SensorDuration,opt => opt.MapFrom(src => src.SensorDuration))
            .ForMember(dest => dest.SensorAlarm,opt => opt.MapFrom(src => src.SensorAlarm))
            .ForMember(dest => dest.CloseReverseLockFlag,opt => opt.MapFrom(src => src.CloseReverseLockFlag))
            .ForMember(dest => dest.MPRCount,opt => opt.MapFrom(src => src.MPRCount))
            .ForMember(dest => dest.MPRInterval,opt => opt.MapFrom(src => src.MPRInterval))
            .ForMember(dest => dest.BackupPeriod,opt => opt.MapFrom(src => src.BackupPeriod))
            .ForMember(dest => dest.QrAesKey,opt => opt.MapFrom(src => src.QrAesKey))
            .ForMember(dest => dest.DeviceBuzzer,opt => opt.MapFrom(src => src.DeviceBuzzer))
            .ForMember(dest => dest.OperationMode,opt => opt.MapFrom(src => src.OperationMode))
            .ForMember(dest => dest.CompanyCode,opt => opt.MapFrom(src => src.CompanyCode));

        CreateMap<Holiday, SDKHolidayDetailModel>()
            .ForMember(dest => dest.HolidayType, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Recurring, opt => opt.MapFrom(src => src.Recursive ? 1 : 0))
            .ForMember(dest => dest.HolidayDate, opt => opt.MapFrom(src => GetListDates(src.StartDate, src.EndDate)));
        
        CreateMap<AccessTime, SDKAccessTimeDetailModel>()
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
            .ForPath(dest => dest.Holiday1.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType1Time1)))
            .ForPath(dest => dest.Holiday1.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType1Time2)))
            .ForPath(dest => dest.Holiday1.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType1Time3)))
            .ForPath(dest => dest.Holiday1.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType1Time4)))
            .ForPath(dest => dest.Holiday2.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType2Time1)))
            .ForPath(dest => dest.Holiday2.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType2Time2)))
            .ForPath(dest => dest.Holiday2.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType2Time3)))
            .ForPath(dest => dest.Holiday2.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType2Time4)))
            .ForPath(dest => dest.Holiday3.Interval1, opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType3Time1)))
            .ForPath(dest => dest.Holiday3.Interval2, opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType3Time2)))
            .ForPath(dest => dest.Holiday3.Interval3, opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType3Time3)))
            .ForPath(dest => dest.Holiday3.Interval4, opt => opt.MapFrom(src => ConvertToDayDetail(src.HolType3Time4)));

        CreateMap<CardInfoBasic, SDKCardModel>()
            .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => src.EmployeeNumber))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.DepartmentName))
            .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
            .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
            .ForMember(dest => dest.AdminFlag, opt => opt.MapFrom(src => src.AdminFlag))
            .ForMember(dest => dest.EffectiveDate, opt => opt.MapFrom(src => src.EffectiveDate))
            .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.ExpireDate))
            .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
            .ForMember(dest => dest.AntiPassBack, opt => opt.MapFrom(src => src.AntiPassBack))
            .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.Timezone))
            .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.Password))
            .ForMember(dest => dest.AccessGroupId, opt => opt.MapFrom(src => src.AccessGroupId))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.IdType, opt => opt.MapFrom(src => src.IdType))
            .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Position))
            .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Avatar))
            .ForMember(dest => dest.FaceData, opt => opt.MapFrom(src => src.FaceData))
            .ForMember(dest => dest.FloorIndex, opt => opt.MapFrom(src => src.FloorIndex))
            .ForMember(dest => dest.FingerTemplates, opt => opt.MapFrom(src => src.FingerTemplates));

        CreateMap<EventLog, SDKEventLogFe>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.EventLogId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceId, opt => opt.MapFrom(src => src.IcuId))
                .ForMember(dest => dest.AccessTime, opt => opt.MapFrom(src => src.EventTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.UnixTime, opt => opt.MapFrom(src => src.EventTime.ToSettingDateTimeUnique()))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.Icu != null ? src.Icu.DeviceAddress : null))
                .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.Icu != null ? src.Icu.Name : null))
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => src.CardType != null ? ((CardType) src.CardType).GetDescription() : null))
                .ForMember(dest => dest.CardTypeId, opt => opt.MapFrom(src => src.CardType))
                .ForMember(dest => dest.BuildingId, opt => opt.MapFrom(src => src.Icu != null  && src.Icu.BuildingId != null ? src.Icu.BuildingId : null))
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType))
                .ForMember(dest => dest.EventTypeDescription, opt => opt.MapFrom(src => src.EventType != null ? ((EventType) src.EventType).GetDescription() : null))
                .ForMember(dest => dest.BodyTemperature, opt => opt.MapFrom(src => src.BodyTemperature))
                .ForMember(dest => dest.InOut, opt => opt.MapFrom(src => src.Antipass != null && Constants.AntiPass.ToList().FindIndex(x => x.Equals(src.Antipass, StringComparison.OrdinalIgnoreCase)) != -1
                    ? ((Antipass)Enum.Parse(typeof(Antipass), src.Antipass, true)).GetDescription()
                    : EventLogResource.lblUnknown))
                .ForMember(dest => dest.IsVisit, opt => opt.MapFrom(src => src.IsVisit))
                .ForMember(dest => dest.OtherCardId, opt => opt.MapFrom(src => src.OtherCardId))
                .ForMember(dest => dest.Distance, opt => opt.MapFrom(src => src.Distance))
                .ForMember(dest => dest.LivenessScore, opt => opt.MapFrom(src => src.LivenessScore))
                .ForMember(dest => dest.ImageCamera, opt => opt.MapFrom(src => src.ImageCamera))
                .ForMember(dest => dest.SearchScore, opt => opt.MapFrom(src => src.SearchScore))
                .ForMember(dest => dest.Videos, opt => opt.MapFrom(src => src.Videos))
                .ForMember(dest => dest.NationalIdNumber, opt => opt.MapFrom(src => src.User != null ? src.User.NationalIdNumber : (src.Visit != null ? src.Visit.NationalIdNumber : null)));
        
    }
    
    public List<SDKHolidayDates> GetListDates(DateTime startDate, DateTime endDate)
    {
        var holidayDates = new List<SDKHolidayDates>();
        var listDate = DateTimeHelper.GetListRangeDate(startDate, endDate);
        if (listDate.Any())
        {
            foreach (var date in listDate)
            {
                var dates = new SDKHolidayDates { Date = date.ToString(Constants.DateTimeFormat.DdMMyyyy) };
                holidayDates.Add(dates);
            }
        }
        return holidayDates;
    }
    
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