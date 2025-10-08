using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.EventLog;
using DeMasterProCloud.DataModel.Visit;
using DeMasterProCloud.Service;
using DeMasterProCloud.Service.Protocol;
using Newtonsoft.Json;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Mapping define for EventLog
    /// </summary>
    public class EvenLogMapping : Profile
    {

        /// <summary>
        /// Ctor for EvenLogMapping
        /// </summary>
        public EvenLogMapping()
        {
            CreateMap<EventLog, EventLogListModel>()
                .ForMember(dest => dest.EventLogId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AccessTime, opt => opt.MapFrom(src => src.EventTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.EventTime, opt => opt.MapFrom(src => src.EventTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.UnixTime, opt => opt.MapFrom(src => src.EventTime.ToSettingDateTimeUnique()))
                .ForMember(dest => dest.Building, opt => opt.MapFrom(src => src.Icu.Building.Name))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => ((CardStatus)src.CardStatus).GetDescription()))
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => ((CardType)src.CardType).GetDescription()))
                .ForMember(dest => dest.CardTypeId, opt => opt.MapFrom(src => src.CardType))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.User != null && src.User.Department != null ? src.User.Department.DepartName : ""))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.User != null && src.User.Department != null ? src.User.Department.DepartName : ""))
                .ForMember(dest => dest.Device, opt => opt.MapFrom(src => src.Icu != null ? src.Icu.DeviceAddress : ""))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.Icu != null ? src.Icu.DeviceAddress : ""))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.DoorName) ? (src.Icu != null ? src.Icu.Name : "") : src.DoorName))
                .ForMember(dest => dest.EventDetail, opt => opt.MapFrom(src => ((EventType)src.EventType).GetDescription()))
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => (short)src.EventType))
                .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.IsVisit ? src.Visit.EndDate.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault) : (src.User.ExpiredDate != null ? (src.User.ExpiredDate ?? DateTime.Now).ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault) : null)))
                .ForMember(dest => dest.IcuId, opt => opt.MapFrom(src => src.IcuId))
                .ForMember(dest => dest.InOut, opt => opt.MapFrom(src => Constants.AntiPass.ToList().FindIndex(x => x.Equals(src.Antipass, StringComparison.OrdinalIgnoreCase)) != -1 ? ((Antipass)Enum.Parse(typeof(Antipass), src.Antipass, true)).GetDescription() : ""))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.WorkType, opt => opt.MapFrom(src => src.IsVisit ? 0 : src.User.WorkType))
                .ForMember(dest => dest.WorkTypeName, opt => opt.MapFrom(src => src.IsVisit ? (src.Visit != null && !string.IsNullOrWhiteSpace(src.Visit.VisitType) ? ((VisitType)int.Parse(src.Visit.VisitType)).GetDescription() : "") : src.User != null && src.User.WorkType.HasValue ? ((WorkType)src.User.WorkType).GetDescription() : ""))
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src => src.VisitId != null ? (short)UserType.Visit : src.UserId != null ? (short)UserType.Normal : src.UserId))
                .ForMember(dest => dest.VerifyMode, opt => opt.MapFrom(src => src.Icu != null ? ((VerifyMode)src.Icu.VerifyMode).GetDescription() : string.Empty))
                .ForMember(dest => dest.VisitId, opt => opt.MapFrom(src => src.VisitId))
                .ForMember(dest => dest.VisitTargetId, opt => opt.MapFrom(src => src.Visit != null ? src.Visit.VisiteeId : 0))
                .ForMember(dest => dest.VisitTargetName, opt => opt.MapFrom(src => src.Visit != null ? src.Visit.VisiteeName : ""))
                .ForMember(dest => dest.VisitTargetDepartment, opt => opt.MapFrom(src => src.Visit != null ? src.Visit.VisiteeDepartment : ""))
                .ForMember(dest => dest.ImageCamera, opt => opt.MapFrom(src => src.ImageCamera))
                .ForMember(dest => dest.OtherCardId, opt => opt.MapFrom(src => src.OtherCardId))
                .ForMember(dest => dest.ResultCheckIn, opt => opt.MapFrom(src => src.ResultCheckIn))
                .ForMember(dest => dest.BodyTemperature, opt => opt.MapFrom(src => src.BodyTemperature))
                .ForMember(dest => dest.Videos, opt => opt.MapFrom(src => src.Videos))
                .ForMember(dest => dest.AllowedBelonging, opt => opt.MapFrom(src => src.Visit.AllowedBelonging))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.IsVisit ? src.Visit != null ? src.Visit.Avatar : "" : src.User != null ? src.User.Avatar : ""))
                .ForMember(dest => dest.ObjectType, opt => opt.MapFrom(src => src.UserId != null ? (int)ObjectTypeEvent.User : src.VisitId != null ? (int)ObjectTypeEvent.Visit : (int)ObjectTypeEvent.Warning))
                .ForMember(dest => dest.DeviceManagerIds, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.Icu.DeviceManagerIds) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(src.Icu.DeviceManagerIds)));

            CreateMap<EventLog, EventLogReportListModel>()
                .ForMember(dest => dest.EventLogId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AccessTime, opt => opt.MapFrom(src => src.EventTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.EventTime, opt => opt.MapFrom(src => src.EventTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.BirthDay, opt => opt.MapFrom(src => src.User != null && src.User.BirthDay != null ? src.User.BirthDay.ToSettingDateString() : ""))
                .ForMember(dest => dest.Building, opt => opt.MapFrom(src => src.Icu.Building.Name))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => ((CardStatus)src.CardStatus).GetDescription()))
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => ((CardType)src.CardType).GetDescription()))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.User != null && src.User.Department != null ? src.User.Department.DepartName : ""))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.User != null && src.User.Department != null ? src.User.Department.DepartName : ""))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.Icu != null ? src.Icu.DeviceAddress : ""))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => string.IsNullOrWhiteSpace(src.DoorName) ? (src.Icu != null ? src.Icu.Name : "") : src.DoorName))
                .ForMember(dest => dest.EventDetail, opt => opt.MapFrom(src => ((EventType)src.EventType).GetDescription()))
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src => src.User != null ? src.User.EmpNumber : ""))
                .ForMember(dest => dest.IcuId, opt => opt.MapFrom(src => src.IcuId))
                .ForMember(dest => dest.InOut, opt => opt.MapFrom(src => Constants.AntiPass.ToList().FindIndex(x => x.Equals(src.Antipass, StringComparison.OrdinalIgnoreCase)) != -1 ? ((Antipass)Enum.Parse(typeof(Antipass), src.Antipass, true)).GetDescription() : ""))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.WorkType, opt => opt.MapFrom(src => src.IsVisit ? 0 : src.User.WorkType))
                .ForMember(dest => dest.WorkTypeName, opt => opt.MapFrom(src => src.IsVisit ? (src.Visit != null && !string.IsNullOrWhiteSpace(src.Visit.VisitType) ? ((VisitType)int.Parse(src.Visit.VisitType)).GetDescription() : "") : src.User != null && src.User.WorkType.HasValue ? ((WorkType)src.User.WorkType).GetDescription() : ""))
                .ForMember(dest => dest.VisitId, opt => opt.MapFrom(src => src.VisitId))
                .ForMember(dest => dest.ImageCamera, opt => opt.MapFrom(src => (src.CardType != (short)CardType.VehicleId && src.CardType != (short)CardType.VehicleMotoBikeId) ? src.ImageCamera : null))
                .ForMember(dest => dest.VehicleImage, opt => opt.MapFrom(src =>
                    (src.CardType == (short)CardType.VehicleId || src.CardType == (short)CardType.VehicleMotoBikeId) ? src.ImageCamera : null))
                .ForMember(dest => dest.ResultCheckIn, opt => opt.MapFrom(src => src.ResultCheckIn))
                .ForMember(dest => dest.OtherCardId, opt => opt.MapFrom(src => src.OtherCardId))
                .ForMember(dest => dest.BodyTemperature, opt => opt.MapFrom(src => src.BodyTemperature))
                .ForMember(dest => dest.Videos, opt => opt.MapFrom(src => (src.CardType != (short)CardType.VehicleId && src.CardType != (short)CardType.VehicleMotoBikeId) ? src.Videos : null))
                .ForMember(dest => dest.VideosVehicle, opt => opt.MapFrom(src =>
                    (src.CardType == (short)CardType.VehicleId || src.CardType == (short)CardType.VehicleMotoBikeId) ? src.Videos : null))
                .ForMember(dest => dest.DelayOpenDoorByCamera, opt => opt.MapFrom(src => src.DelayOpenDoorByCamera))
                .ForMember(dest => dest.Distance, opt => opt.MapFrom(src => src.Distance))
                .ForMember(dest => dest.SearchScore, opt => opt.MapFrom(src => src.SearchScore))
                .ForMember(dest => dest.LivenessScore, opt => opt.MapFrom(src => src.LivenessScore))
                .ForMember(dest => dest.ObjectType, opt => opt.MapFrom(src => src.UserId != null ? (int)ObjectTypeEvent.User : src.VisitId != null ? (int)ObjectTypeEvent.Visit : (int)ObjectTypeEvent.Warning))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User != null ? src.User.Avatar : (src.Visit != null ? src.Visit.Avatar : "")));

            CreateMap<EventLog, VisitReportModel>()
                .ForMember(dest => dest.AccessTime,
                    opt => opt.MapFrom(src => src.EventTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.Visit != null ? src.Visit.VisitorName : null))
                .ForMember(dest => dest.BirthDay,
                    opt => opt.MapFrom(src => src.Visit != null ? src.Visit.BirthDay.ToSettingDateString() : null))
                .ForMember(dest => dest.Department,
                    opt => opt.MapFrom(src => src.Visit != null ? src.Visit.VisitorDepartment : null))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.Device,
                    opt => opt.MapFrom(src => src.Icu != null ? src.Icu.DeviceAddress : null))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.DoorName))
                //Building Name...
                //.ForMember(dest => dest.InOut, opt => opt.MapFrom(src => Constants.AntiPass.Contains(src.Antipass)
                //    ? ((Antipass) Enum.Parse(typeof(Antipass), src.Antipass)).GetDescription()
                //    : EventLogResource.lblUnknown))
                .ForMember(dest => dest.InOut, opt => opt.MapFrom(src => Constants.AntiPass.ToList().FindIndex(x => x.Equals(src.Antipass, StringComparison.OrdinalIgnoreCase)) != -1
                    ? ((Antipass)Enum.Parse(typeof(Antipass), src.Antipass, true)).GetDescription()
                    : EventLogResource.lblUnknown))
                .ForMember(dest => dest.EventDetail,
                    opt => opt.MapFrom(src => ((EventType)src.EventType).GetDescription()))
                .ForMember(dest => dest.VisitId,
                    opt => opt.MapFrom(src => src.VisitId))
                .ForMember(dest => dest.VisiteeName, opt => opt.MapFrom(src => src.Visit != null ? src.Visit.VisiteeName : null))
                .ForMember(dest => dest.VisiteeDepartment, opt => opt.MapFrom(src => src.Visit != null ? src.Visit.VisiteeDepartment : null))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.Visit != null ? src.Visit.StartDate.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault) : null))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.Visit != null ? src.Visit.EndDate.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault) : null))
                .ForMember(dest => dest.ApproverId1, opt => opt.MapFrom(src => src.Visit != null ? src.Visit.ApproverId1 : 0))
                .ForMember(dest => dest.ApproverId2, opt => opt.MapFrom(src => src.Visit != null ? src.Visit.ApproverId2 : 0))
                .ForMember(dest => dest.CardType,
                    opt => opt.MapFrom(src => Enum.GetName(typeof(CardType), src.CardType)))
                .ForMember(dest => dest.Avatar,
                    opt => opt.MapFrom(src => src.VisitId.HasValue ? src.Visit.Avatar : src.ResultCheckIn))
                .ForMember(dest => dest.ImageCamera,
                    opt => opt.MapFrom(src => src.ImageCamera))
                .ForMember(dest => dest.OtherCardId,
                    opt => opt.MapFrom(src => src.OtherCardId))
                .ForMember(dest => dest.ResultCheckIn,
                    opt => opt.MapFrom(src => src.ResultCheckIn))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.BodyTemperature, opt => opt.MapFrom(src => src.BodyTemperature))
                .ForMember(dest => dest.DelayOpenDoorByCamera, opt => opt.MapFrom(src => src.DelayOpenDoorByCamera));
                //Card Status...
                
            CreateMap<SendEventLogDetailData, EventLogWebhook>()
                .ForMember(dest => dest.EventLogId, opt => opt.MapFrom(src => src.EventLogId))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department))
                .ForMember(dest => dest.BodyTemperature, opt => opt.MapFrom(src => src.BodyTemperature))
                .ForMember(dest => dest.EventDetail, opt => opt.MapFrom(src => src.EventDetail))
                .ForMember(dest => dest.Device, opt => opt.MapFrom(src => src.Device))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => src.CardType))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.DoorName))
                .ForMember(dest => dest.ExpireDate, opt => opt.MapFrom(src => src.ExpireDate))
                .ForMember(dest => dest.InOut, opt => opt.MapFrom(src => src.InOut))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.UnixTime, opt => opt.MapFrom(src => src.UnixTime))
                .ForMember(dest => dest.EventDetailCode, opt => opt.MapFrom(src => src.EventDetailCode))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.VisitId, opt => opt.MapFrom(src => src.VisitId))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.NationalIdNumber, opt => opt.MapFrom(src => src.NationalIdNumber))
                .ForMember(dest => dest.AccessTime, opt => opt.MapFrom(src => src.AccessTime));


            CreateMap<EventLog, SendEventLogDetailData>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
                .ForMember(dest => dest.EventLogId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IcuId, opt => opt.MapFrom(src => src.IcuId))
                .ForMember(dest => dest.AccessTime,
                    opt => opt.MapFrom(src => src.EventTime))
                .ForMember(dest => dest.UnixTime,
                    opt => opt.MapFrom(src => (src.EventTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.Device,
                    opt => opt.MapFrom(src => src.Icu != null ? src.Icu.DeviceAddress : null))
                .ForMember(dest => dest.DeviceAddress,
                    opt => opt.MapFrom(src => src.Icu != null ? src.Icu.DeviceAddress : null))
                .ForMember(dest => dest.DoorName,
                    opt => opt.MapFrom(src => src.Icu != null ? src.Icu.Name : null))
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => ((CardType) src.CardType).GetDescription()))
                .ForMember(dest => dest.CardTypeId, opt => opt.MapFrom(src => src.CardType))
                .ForMember(dest => dest.BuildingId,
                    opt => opt.MapFrom(src => src.Icu != null  && src.Icu.BuildingId != null ? src.Icu.BuildingId : null))
                .ForMember(dest => dest.Building,
                    opt => opt.MapFrom(src => src.Icu != null  && src.Icu.Building != null ? src.Icu.Building.Name : null))
                .ForMember(dest => dest.EventDetailCode,
                    opt => opt.MapFrom(src => src.EventType))
                .ForMember(dest => dest.DeviceManagerIds, opt => opt.MapFrom(src => 
                    string.IsNullOrWhiteSpace(src.Icu.DeviceManagerIds) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(src.Icu.DeviceManagerIds)))
                .ForMember(dest => dest.EventDetail,
                    opt => opt.MapFrom(src => ((EventType) src.EventType).GetDescription()))
                .ForMember(dest => dest.BodyTemperature, opt => opt.MapFrom(src => src.BodyTemperature))
                .ForMember(dest => dest.AllowedBelonging, opt => opt.MapFrom(src => src.Visit != null ? src.Visit.AllowedBelonging : null))
                .ForMember(dest => dest.InOut, opt => opt.MapFrom(src => Constants.AntiPass.ToList().FindIndex(x => x.Equals(src.Antipass, StringComparison.OrdinalIgnoreCase)) != -1
                    ? ((Antipass)Enum.Parse(typeof(Antipass), src.Antipass, true)).GetDescription()
                    : EventLogResource.lblUnknown))
                .ForMember(dest => dest.IsVisit, opt => opt.MapFrom(src => src.IsVisit))
                .ForMember(dest => dest.OtherCardId, opt => opt.MapFrom(src => src.OtherCardId))
                .ForMember(dest => dest.Distance, opt => opt.MapFrom(src => src.Distance))
                .ForMember(dest => dest.LivenessScore, opt => opt.MapFrom(src => src.LivenessScore))
                .ForMember(dest => dest.ImageCamera, opt => opt.MapFrom(src => src.ImageCamera))
                .ForMember(dest => dest.SearchScore, opt => opt.MapFrom(src => src.SearchScore))
                .ForMember(dest => dest.Videos, opt => opt.MapFrom(src => src.Videos))
                .ForMember(dest => dest.ObjectType, opt => opt.MapFrom(src => src.UserId != null ? (int)ObjectTypeEvent.User : src.VisitId != null ? (int)ObjectTypeEvent.Visit : (int)ObjectTypeEvent.Warning))
                .ForMember(dest => dest.NationalIdNumber, opt => opt.MapFrom(src => src.User != null ? src.User.NationalIdNumber : src.Visit != null ? src.Visit.NationalIdNumber : null));

            CreateMap<DataImageCamera, CameraEventCheckInModel>()
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Link))
                .ForMember(dest => dest.PersonName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.UserId.HasValue ? "0" : null));



            CreateMap<EventLogListModel, VehicleEventLogListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EventTime, opt => opt.MapFrom(src => src.EventTime))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.PlateNumber, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.DoorName))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.VisitId, opt => opt.MapFrom(src => src.VisitId))
                .ForMember(dest => dest.InOut, opt => opt.MapFrom(src => src.InOut))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.DepartmentName))
                .ForMember(dest => dest.Building, opt => opt.MapFrom(src => src.Building))
                .ForMember(dest => dest.CardTypeId, opt => opt.MapFrom(src => src.CardTypeId))
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => src.CardType))
                .ForMember(dest => dest.VehicleImage, opt => opt.MapFrom(src => src.ImageCamera))
                .ForMember(dest => dest.EventDetailCode, opt => opt.MapFrom(src => src.EventType))
                .ForMember(dest => dest.EventDetail, opt => opt.MapFrom(src => ((EventType)src.EventType).GetDescription()))
                .ForMember(dest => dest.ObjectType, opt => opt.MapFrom(src => src.UserId != null ? (int)ObjectTypeEvent.User : src.VisitId != null ? (int)ObjectTypeEvent.Visit : (int)ObjectTypeEvent.Warning))
                .ForMember(dest => dest.DeviceManagerIds, opt => opt.MapFrom(src => src.DeviceManagerIds));

            var eventTypeAbNormal = new List<int>()
            {

            };
            
            CreateMap<EventLog, EventLogDetailModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.IcuId, opt => opt.MapFrom(src => src.IcuId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.VisitId, opt => opt.MapFrom(src => src.VisitId))
                .ForMember(dest => dest.AccessTime, opt => opt.MapFrom(src => 
                    src.Icu != null && src.Icu.Building != null ? src.EventTime.ConvertToUserTime(src.Icu.Building.TimeZone).ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)
                                                                : src.EventTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.BirthDay, opt => opt.MapFrom(src => 
                    src.IsVisit ? src.Visit != null ? src.Visit.BirthDay.ToSettingDateString() : "" 
                                : src.User != null ? src.User.BirthDay.ToSettingDateString() : ""))
                .ForMember(dest => dest.EmployeeNumber, opt => opt.MapFrom(src =>
                    src.IsVisit ? src.Visit != null ? src.Visit.VisitorEmpNumber : ""
                                : src.User != null ? src.User.EmpNumber : ""))
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src =>
                    src.IsVisit ? src.Visit != null ? src.Visit.VisitorDepartment : ""
                                : src.User != null && src.User.Department != null ? src.User.Department.DepartName : ""))
                .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.Icu != null ? src.Icu.DeviceAddress : ""))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Icu != null ? src.Icu.Name : src.DoorName))
                .ForMember(dest => dest.Building, opt => opt.MapFrom(src => src.Icu != null && src.Icu.Building != null ? src.Icu.Building.Name : ""))
                .ForMember(dest => dest.UserCode, opt => opt.MapFrom(src => src.User != null ? src.User.UserCode : ""))
                .ForMember(dest => dest.InOut, opt => opt.MapFrom(src => src.Antipass))
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => src.EventType))
                .ForMember(dest => dest.EventDetail, opt => opt.MapFrom(src => ((EventType)src.EventType).GetDescription()))
                .ForMember(dest => dest.IssueCount, opt => opt.MapFrom(src => src.IssueCount))
                .ForMember(dest => dest.CardStatus, opt => opt.MapFrom(src => src.CardStatus))
                .ForMember(dest => dest.CardType, opt => opt.MapFrom(src => ((CardType)src.CardType).GetDescription()))
                .ForMember(dest => dest.ImageCamera, opt => opt.MapFrom(src =>
                    (src.CardType != (short)CardType.VehicleId && src.CardType != (short)CardType.VehicleMotoBikeId) ? src.ImageCamera : null))
                .ForMember(dest => dest.VehicleImage, opt => opt.MapFrom(src =>
                    (src.CardType == (short)CardType.VehicleId || src.CardType == (short)CardType.VehicleMotoBikeId) ? src.ImageCamera : null))
                .ForMember(dest => dest.OtherCardId, opt => opt.MapFrom(src => src.OtherCardId))
                .ForMember(dest => dest.Videos, opt => opt.MapFrom(src => (src.CardType != (short)CardType.VehicleId && src.CardType != (short)CardType.VehicleMotoBikeId) ? src.Videos : null))
                .ForMember(dest => dest.VideosVehicle, opt => opt.MapFrom(src =>
                    (src.CardType == (short)CardType.VehicleId || src.CardType == (short)CardType.VehicleMotoBikeId) ? src.Videos : null))
                .ForMember(dest => dest.ResultCheckIn, opt => opt.MapFrom(src => src.ResultCheckIn))
                .ForMember(dest => dest.BodyTemperature, opt => opt.MapFrom(src => src.BodyTemperature))
                .ForMember(dest => dest.DelayOpenDoorByCamera, opt => opt.MapFrom(src => src.DelayOpenDoorByCamera))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User != null ? src.User.Avatar : (src.Visit != null ? src.Visit.Avatar : null)))
                .ForMember(dest => dest.UnixTime, opt => opt.MapFrom(src => src.EventTime.ToSettingDateTimeUnique()))
                .ForMember(dest => dest.CardTypeId, opt => opt.MapFrom(src => src.CardType))
                .ForMember(dest => dest.ObjectType, opt => opt.MapFrom(src => src.UserId != null ? (int)ObjectTypeEvent.User : src.VisitId != null ? (int)ObjectTypeEvent.Visit : (int)ObjectTypeEvent.Warning))
                .ForMember(dest => dest.DoorImage, opt => opt.MapFrom(src => src.Icu.Image));
        }
    }
}