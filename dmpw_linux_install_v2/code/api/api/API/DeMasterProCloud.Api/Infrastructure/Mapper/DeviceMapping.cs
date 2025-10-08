using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.AccessGroupDevice;
using DeMasterProCloud.DataModel.Building;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.DeviceMessage;
using DeMasterProCloud.Service.Protocol;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using DeMasterProCloud.DataModel.AccessGroup;

namespace DeMasterProCloud.Api.Infrastructure.Mapper
{
    /// <summary>
    /// Mapping define for Device
    /// </summary>
    public class DeviceMapping : Profile
    {
        /// <summary>
        /// Ctor DeviceMapping
        /// </summary>
        public DeviceMapping()
        {
            Random rnd = new Random();

            // TODO update device attribute
            CreateMap<IcuDevice, DeviceListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.DoorStatusId, opt => opt.MapFrom(src => src.DoorStatusId))
                .ForMember(dest => dest.DoorActiveTimeZone,
                    opt => opt.MapFrom(src => (src.ActiveTzId == 1 || src.ActiveTzId == 2) ? ((DefaultTimezoneType)src.ActiveTzId).GetDescription() : src.ActiveTz.Name))
                .ForMember(dest => dest.DoorPassageTimeZone,
                    opt => opt.MapFrom(src => (src.PassageTzId == 1 || src.PassageTzId == 2) ? ((DefaultTimezoneType)src.PassageTzId).GetDescription() : src.PassageTz.Name))
                .ForMember(dest => dest.VerifyMode, opt => opt.MapFrom(src => ((VerifyMode)src.VerifyMode).GetDescription()))
                .ForMember(dest => dest.OperationType, opt => opt.MapFrom(src => ((OperationType)src.OperationType).GetDescription()))
                .ForMember(dest => dest.BioStationMode,
                    opt => opt.MapFrom(src => ((BioStationMode)src.BioStationMode).GetDescription()))
                .ForMember(dest => dest.Status,
                    opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.FirmwareVersion))
                .ForMember(dest => dest.ConnectionStatus, opt => opt.MapFrom(src => src.ConnectionStatus))
                .ForMember(dest => dest.LastCommunicationTime,
                    opt => opt.MapFrom(src => src.LastCommunicationTime.ConvertDefaultDateTimeToString(Constants.DateTimeFormatDefault)))
                .ForMember(dest => dest.NumberOfNotTransmittingEvent,
                    opt => opt.MapFrom(src => src.NumberOfNotTransmittingEvent))
                .ForMember(dest => dest.RegisterIdNumber, opt => opt.MapFrom(src => src.RegisterIdNumber))
                .ForMember(dest => dest.Building, opt => opt.MapFrom(src => src.Building.Name))
                .ForMember(dest => dest.DeviceType,
                    opt => opt.MapFrom(src => ((DeviceType)src.DeviceType).GetDescription()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.DeviceType))
                .ForMember(dest => dest.OutCardReader, opt => opt.MapFrom(src => src.RoleReader0 == 1 ? src.VersionReader0 : src.RoleReader1 == 1 ? src.VersionReader1 : ""))
                .ForMember(dest => dest.DoorStatus, opt => opt.MapFrom(src => src.DoorStatus))
                .ForMember(dest => dest.InCardReader, opt => opt.MapFrom(src => src.RoleReader0 == 0 ? src.VersionReader0 : src.RoleReader1 == 0 ? src.VersionReader1 : ""))
                .ForMember(dest => dest.NfcModule, opt => opt.MapFrom(src => src.NfcModuleVersion))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company.Name))
                .ForMember(dest => dest.CompanyCode, opt => opt.MapFrom(src => src.Company.Code))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
                .ForMember(dest => dest.UseAlarmRelay, opt => opt.MapFrom(src => src.UseAlarmRelay))
                .ForMember(dest => dest.EnableVideoCall, opt => opt.MapFrom(src => src.EnableVideoCall))
                .ForMember(dest => dest.AutoAcceptVideoCall, opt => opt.MapFrom(src => src.AutoAcceptVideoCall))
                .ForMember(dest => dest.OperationTypeId, opt => opt.MapFrom(src => src.OperationType))
                .ForMember(dest => dest.VerifyModeId, opt => opt.MapFrom(src => src.VerifyMode))
                .ForMember(dest => dest.AlarmStatus, opt => opt.MapFrom(src => src.AlarmStatus == (short)AlarmState.On));

            // TODO update device attribute
            CreateMap<DeviceModel, IcuDevice>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.DoorName))
                .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => src.DeviceType))

                .ForMember(dest => dest.VerifyMode, opt => opt.MapFrom(src => src.VerifyMode))
                .ForMember(dest => dest.BioStationMode, opt => opt.MapFrom(src => src.BioStationMode))
                .ForMember(dest => dest.BackupPeriod, opt => opt.MapFrom(src => src.BackupPeriod))

                .ForMember(dest => dest.CompanyId, opt => opt.MapFrom(src => src.CompanyId == 0 || src.CompanyId == null ? null : src.CompanyId))
                .ForMember(dest => dest.BuildingId, opt => opt.MapFrom(src => src.BuildingId == 0 ? null : src.BuildingId))
                .ForMember(dest => dest.ActiveTzId, opt => opt.MapFrom(src => src.ActiveTimezoneId == 0 ? null : src.ActiveTimezoneId))
                .ForMember(dest => dest.PassageTzId, opt => opt.MapFrom(src => src.PassageTimezoneId == 0 ? null : src.PassageTimezoneId))

                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
                .ForMember(dest => dest.ServerIp, opt => opt.MapFrom(src => src.ServerIp))
                .ForMember(dest => dest.ServerPort, opt => opt.MapFrom(src => src.ServerPort))

                .ForMember(dest => dest.OperationType, opt => opt.MapFrom(src => EnumHelper.ToEnumList<OperationType>().Select(m => m.Id).ToList().Contains((int)src.OperationType) ? src.OperationType : (short) OperationType.Entrance ))

                .ForMember(dest => dest.RoleReader0, opt => opt.MapFrom(src => src.RoleReader0))
                .ForMember(dest => dest.RoleReader1, opt => opt.MapFrom(src => src.RoleReader1))

                .ForMember(dest => dest.LedReader0, opt => opt.MapFrom(src => src.LedReader0))
                .ForMember(dest => dest.LedReader1, opt => opt.MapFrom(src => src.LedReader1))

                .ForMember(dest => dest.UseCardReader, opt =>
                {
                    opt.MapFrom(src => src.DeviceType == (int)DeviceType.ITouchPop
                                      || src.DeviceType == (int)DeviceType.ITouchPopX ? src.UseCardReader : null);
                })

                .ForMember(dest => dest.BuzzerReader0, opt => opt.MapFrom(src => src.BuzzerReader0))
                .ForMember(dest => dest.BuzzerReader1, opt => opt.MapFrom(src => src.BuzzerReader1))

                .ForMember(dest => dest.SensorType, opt => opt.MapFrom(src => src.SensorType))
                .ForMember(dest => dest.OpenDuration, opt => opt.MapFrom(src => src.LockOpenDuration))
                .ForMember(dest => dest.MaxOpenDuration, opt => opt.MapFrom(src => src.MaxOpenDuration))
                .ForMember(dest => dest.SensorDuration, opt => opt.MapFrom(src => src.SensorDuration))
                .ForMember(dest => dest.SensorAlarm, opt => opt.MapFrom(src => src.Alarm))
                .ForMember(dest => dest.CloseReverseLockFlag, opt => opt.MapFrom(src => src.CloseReverseLock))

                .ForMember(dest => dest.PassbackRule, opt => opt.MapFrom(src => src.Passback))

                .ForMember(dest => dest.MPRCount, opt => opt.MapFrom(src => src.MPRCount == 0 ? null : src.MPRCount))
                .ForMember(dest => dest.MPRInterval, opt => opt.MapFrom(src => src.MPRInterval))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
                .ForMember(dest => dest.UseAlarmRelay, opt => opt.MapFrom(src => src.UseAlarmRelay))
                .ForMember(dest => dest.AutoAcceptVideoCall, opt => opt.MapFrom(src => src.AutoAcceptVideoCall))
                .ForMember(dest => dest.EnableVideoCall, opt => opt.MapFrom(src => src.EnableVideoCall))
                .ForMember(dest => dest.DeviceManagerIds, opt => opt.MapFrom(src => src.DeviceManagerIds == null ? "[]" : JsonConvert.SerializeObject(src.DeviceManagerIds)))
                .ForMember(dest => dest.DependentDoors, opt => opt.MapFrom(src => src.DependentDoors == null ? "[]" : JsonConvert.SerializeObject(src.DependentDoors)))
                .ForMember(dest => dest.ConnectionStatus, opt => opt.Ignore())

                .ForMember(dest => dest.MacAddress, opt =>
                {
                    opt.Condition(src => src.MacAddress != null);
                    opt.MapFrom(src => src.MacAddress);
                })

                //.ForMember(dest => dest.DeviceBuzzer, opt => opt.MapFrom(src => src.DeviceBuzzer))

                .Include<DeviceDataModel, IcuDevice>();
            CreateMap<DeviceDataModel, IcuDevice>();
            CreateMap<DeviceDataModel, DeviceDetailModel>();

            // TODO update device attribute
            CreateMap<IcuDevice, DeviceModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => src.DeviceType))

                .ForMember(dest => dest.VerifyMode, opt => opt.MapFrom(src => src.VerifyMode))
                .ForMember(dest => dest.BioStationMode, opt => opt.MapFrom(src => src.BioStationMode))
                .ForMember(dest => dest.BackupPeriod, opt => opt.MapFrom(src => src.BackupPeriod))

                .ForMember(dest => dest.CompanyId, opt =>
                {
                    opt.Condition(src => src.Company != null);
                    opt.MapFrom(src => src.Company.Id);
                })
                .ForMember(dest => dest.BuildingId, opt => opt.MapFrom(src => src.Building.Id))
                .ForMember(dest => dest.ActiveTimezoneId, opt => opt.MapFrom(src => src.ActiveTzId))
                .ForMember(dest => dest.PassageTimezoneId, opt => opt.MapFrom(src => src.PassageTzId))

                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
                .ForMember(dest => dest.ServerIp, opt => opt.MapFrom(src => src.ServerIp))
                .ForMember(dest => dest.ServerPort, opt => opt.MapFrom(src => src.ServerPort))

                .ForMember(dest => dest.OperationType, opt => opt.MapFrom(src => src.OperationType))

                .ForMember(dest => dest.RoleReader0, opt => opt.MapFrom(src => src.RoleReader0))
                .ForMember(dest => dest.RoleReader1, opt => opt.MapFrom(src => src.RoleReader1))

                .ForMember(dest => dest.LedReader0, opt => opt.MapFrom(src => src.LedReader0))
                .ForMember(dest => dest.LedReader1, opt => opt.MapFrom(src => src.LedReader1))

                .ForMember(dest => dest.UseCardReader, opt => opt.MapFrom(src => src.UseCardReader))

                .ForMember(dest => dest.BuzzerReader0, opt => opt.MapFrom(src => src.BuzzerReader0))
                .ForMember(dest => dest.BuzzerReader1, opt => opt.MapFrom(src => src.BuzzerReader1))

                .ForMember(dest => dest.SensorType, opt => opt.MapFrom(src => src.SensorType))
                .ForMember(dest => dest.LockOpenDuration, opt => opt.MapFrom(src => src.OpenDuration))
                .ForMember(dest => dest.MaxOpenDuration, opt => opt.MapFrom(src => src.MaxOpenDuration))
                .ForMember(dest => dest.SensorDuration, opt => opt.MapFrom(src => src.SensorDuration))
                .ForMember(dest => dest.Alarm, opt => opt.MapFrom(src => src.SensorAlarm))
                .ForMember(dest => dest.CloseReverseLock, opt => opt.MapFrom(src => src.CloseReverseLockFlag))
                .ForMember(dest => dest.Passback, opt => opt.MapFrom(src => src.PassbackRule))

                .ForMember(dest => dest.MPRCount, opt => opt.MapFrom(src => src.MPRCount))
                .ForMember(dest => dest.MPRInterval, opt => opt.MapFrom(src => src.MPRInterval))

                .ForMember(dest => dest.MacAddress, opt => opt.MapFrom(src => src.MacAddress))

                .ForMember(dest => dest.DeviceBuzzer, opt => opt.MapFrom(src => src.DeviceBuzzer))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
                .ForMember(dest => dest.UseAlarmRelay, opt => opt.MapFrom(src => src.UseAlarmRelay))
                .ForMember(dest => dest.EnableVideoCall, opt => opt.MapFrom(src => src.EnableVideoCall))
                .ForMember(dest => dest.AutoAcceptVideoCall, opt => opt.MapFrom(src => src.AutoAcceptVideoCall))
                .ForMember(dest => dest.DeviceManagerIds, opt => opt.MapFrom(src => src.DeviceManagerIds == null ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(src.DeviceManagerIds)))
                .ForMember(dest => dest.DependentDoors, opt => opt.MapFrom(src => src.DependentDoors == null ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(src.DependentDoors)))
                .Include<IcuDevice, DeviceDataModel>();
            CreateMap<IcuDevice, DeviceDataModel>()
                .ForMember(dest => dest.DeviceManagerIds, opt => opt.MapFrom(src => src.DeviceManagerIds == null ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(src.DeviceManagerIds)))
                .ForMember(dest => dest.DependentDoors, opt => opt.MapFrom(src => src.DependentDoors == null ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(src.DependentDoors)))
                .ForMember(dest => dest.BuildingId, opt => opt.MapFrom(src => src.BuildingId));

            // TODO update device attribute
            CreateMap<IcuDevice, IcuDeviceProtocolDetailData>()
                .ForMember(dest => dest.AntiPassback,
                    opt => opt.MapFrom(src => src.PassbackRule))
                .ForMember(dest => dest.VerifyMode,
                    opt => opt.MapFrom(src => src.VerifyMode))
                .ForMember(dest => dest.BioStationMode,
                    opt => opt.MapFrom(src => src.BioStationMode))
                .ForMember(dest => dest.LockOpenDuration,
                    opt => opt.MapFrom(src => src.OpenDuration))
                .ForMember(dest => dest.DoorSensorType,
                    opt => opt.MapFrom(src => src.SensorType))
                .ForMember(dest => dest.SensorDuration,
                    opt => opt.MapFrom(src => src.SensorDuration))
                .ForMember(dest => dest.SensorAlarm,
                    opt => opt.MapFrom(src => src.SensorAlarm))
                .ForMember(dest => dest.CloseReverseLockFlag,
                    opt => opt.MapFrom(src => src.CloseReverseLockFlag))
                .ForMember(dest => dest.MPRCount,
                    opt => opt.MapFrom(src => src.MPRCount))
                .ForMember(dest => dest.MPRInterval,
                    opt => opt.MapFrom(src => src.MPRInterval))
                .ForMember(dest => dest.DoorSensorType,
                    opt => opt.MapFrom(src => src.SensorType))
                .ForMember(dest => dest.BackupPeriod,
                    opt => opt.MapFrom(src => src.BackupPeriod))
                .ForMember(dest => dest.qrAesKey,
                    opt => opt.MapFrom(src => src.Company.SecretCode))
                .ForMember(dest => dest.DeviceBuzzer,
                    opt => opt.MapFrom(src => src.DeviceBuzzer))
                .ForMember(dest => dest.CompanyCode,
                    opt => opt.MapFrom(src => src.Company.Code));

            CreateMap<IcuDevice, AccessGroupDeviceDoor>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TzId, opt => opt.MapFrom(src => src.ActiveTz.Id))
                .ForMember(dest => dest.Timezone, opt => opt.MapFrom(src => src.ActiveTz.Name))
                .ForMember(dest => dest.Building, opt => opt.MapFrom(src => src.Building.Name));

            CreateMap<IcuDevice, AccessGroupDeviceUnAssignDoor>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Building, opt => opt.MapFrom(src => src.Building.Name))
                .ForMember(dest => dest.OperationType, opt => opt.MapFrom(src => src.OperationType))
                .ForMember(dest => dest.VerifyMode, opt => opt.MapFrom(src => src.VerifyMode))
                .ForMember(dest => dest.AccessTimeName, opt => opt.MapFrom(src => src.ActiveTz.Name))
                .ForMember(dest => dest.TzId, opt => opt.MapFrom(src => src.ActiveTz.Id));
            CreateMap<IcuDevice, AccessGroupDeviceAssignDoor>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Building, opt => opt.MapFrom(src => src.Building.Name))
                .ForMember(dest => dest.OperationType, opt => opt.MapFrom(src => src.OperationType))
                .ForMember(dest => dest.VerifyMode, opt => opt.MapFrom(src => src.VerifyMode))
                .ForMember(dest => dest.AccessTimeName, opt => opt.MapFrom(src => src.ActiveTz.Name))
                .ForMember(dest => dest.TzId, opt => opt.MapFrom(src => src.ActiveTz.Id));

            CreateMap<DeviceInfoResponse, IcuDevice>()
                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.Data.IpAddress))
                .ForMember(dest => dest.FirmwareVersion, opt => opt.MapFrom(src => src.Data.Version))
                .ForMember(dest => dest.MacAddress, opt => opt.MapFrom(src => src.Data.MacAddress))
                .ForMember(dest => dest.RegisterIdNumber, opt => opt.MapFrom(src => src.Data.UserCount));

            CreateMap<IcuDevice, BuildingDoorModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.DoorStatus, opt => opt.MapFrom(src => src.DoorStatus))
                .ForMember(dest => dest.DoorStatusId, opt => opt.MapFrom(src => src.DoorStatusId))
                .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => ((DeviceType)src.DeviceType).GetDescription()))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.FirmwareVersion))
                .ForMember(dest => dest.AutoAcceptVideoCall, opt => opt.MapFrom(src => src.AutoAcceptVideoCall))
                .ForMember(dest => dest.EnableVideoCall, opt => opt.MapFrom(src => src.EnableVideoCall))
                .ForMember(dest => dest.LastCommunicationTime, opt => opt.MapFrom(src => src.LastCommunicationTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.ActiveTz, opt => opt.MapFrom(src => src.ActiveTz.Name))
                .ForMember(dest => dest.PassageTz, opt => opt.MapFrom(src => src.PassageTz.Name))
                .ForMember(dest => dest.Image, opt => opt.MapFrom(src => src.Image))
                .ForMember(dest => dest.OperationTypeId, opt => opt.MapFrom(src => src.OperationType))
                .ForMember(dest => dest.VerifyModeId, opt => opt.MapFrom(src => src.VerifyMode))
                .ForMember(dest => dest.VerifyMode, opt => opt.MapFrom(src => ((VerifyMode)src.VerifyMode).GetDescription()))
                .ForMember(dest => dest.OperationType, opt => opt.MapFrom(src => ((OperationType)src.OperationType).GetDescription()));

            CreateMap<IcuDevice, BuildingUnAssignDoorModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Building, opt => opt.MapFrom(src => src.Building.Name))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.OperationTypeId, opt => opt.MapFrom(src => src.OperationType))
                .ForMember(dest => dest.VerifyModeId, opt => opt.MapFrom(src => src.VerifyMode))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.VerifyMode, opt => opt.MapFrom(src => ((VerifyMode)src.VerifyMode).GetDescription()))
                .ForMember(dest => dest.OperationType, opt => opt.MapFrom(src => ((OperationType)src.OperationType).GetDescription()));

            CreateMap<IcuDevice, DeviceStatusDetail>()
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.ConnectionStatus))
                .ForMember(dest => dest.DeviceType,
                    opt => opt.MapFrom(src => ((DeviceType)src.DeviceType).GetDescription()))
                .ForMember(dest => dest.EventCount, opt => opt.MapFrom(src => src.NumberOfNotTransmittingEvent))
                .ForMember(dest => dest.UserCount, opt => opt.MapFrom(src => src.RegisterIdNumber))
                .ForMember(dest => dest.LastCommunicationTime,
                    opt => opt.MapFrom(src => src.LastCommunicationTime.ToString(Constants.DateTimeFormat.MmDdYyyyHHmmss)))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.FirmwareVersion))
                .ForMember(dest => dest.InReader, opt => opt.MapFrom(src => src.VersionReader0))
                .ForMember(dest => dest.OutReader, opt => opt.MapFrom(src => src.VersionReader1))
                .ForMember(dest => dest.NfcModule, opt => opt.MapFrom(src => src.NfcModuleVersion))
                .ForMember(dest => dest.DoorStatus, opt => opt.MapFrom(src => src.DoorStatus))
                .ForMember(dest => dest.DoorStatusId, opt => opt.MapFrom(src => src.DoorStatusId));

            CreateMap<IcuDevice, AccessibleDoorModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ActiveTz, opt => opt.MapFrom(src => src.ActiveTz.Name))
                .ForMember(dest => dest.PassageTz, opt => opt.MapFrom(src => src.PassageTz.Name))
                .ForMember(dest => dest.VerifyMode,
                    opt => opt.MapFrom(src => ((VerifyMode)src.VerifyMode).GetDescription()))
                .ForMember(dest => dest.AntiPassback, opt => opt.MapFrom(src => src.PassbackRule))
                .ForMember(dest => dest.DeviceType,
                    opt => opt.MapFrom(src => ((DeviceType)src.DeviceType).GetDescription()))
                .ForMember(dest => dest.Mpr, opt => opt.MapFrom(src => src.MPRCount));

            CreateMap<IcuDevice, CheckDeviceSettingModel>()
                .ForMember(dest => dest.ActiveTimezone, opt => opt.MapFrom(src => src.ActiveTz.Name))
                .ForMember(dest => dest.PassageTimezone, opt => opt.MapFrom(src => src.PassageTz.Name))

                .ForMember(dest => dest.VerifyMode, opt => opt.MapFrom(src => ((VerifyMode)src.VerifyMode).GetDescription()))
                .ForMember(dest => dest.BackupPeriod, opt => opt.MapFrom(src => src.BackupPeriod))

                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
                .ForMember(dest => dest.ServerPort, opt => opt.MapFrom(src => src.ServerPort))
                .ForMember(dest => dest.ServerIp, opt => opt.MapFrom(src => src.ServerIp))

                .ForMember(dest => dest.AntiPassback, opt => opt.MapFrom(src => ((PassbackRules)src.PassbackRule).GetDescription()))

                .ForMember(dest => dest.LockOpenDuration, opt => opt.MapFrom(src => src.OpenDuration))
                .ForMember(dest => dest.DoorSensorType, opt => opt.MapFrom(src => ((SensorType)src.SensorType).GetDescription()))
                .ForMember(dest => dest.SensorDuration, opt => opt.MapFrom(src => src.SensorDuration ?? 0))

                .ForMember(dest => dest.RoleReader0, opt => opt.MapFrom(src => src.RoleReader0 == 0 ? "In" : src.RoleReader0 == 1 ? "Out" : ""))
                .ForMember(dest => dest.RoleReader1, opt => opt.MapFrom(src => src.RoleReader1 == 0 ? "In" : src.RoleReader1 == 1 ? "Out" : ""))

                .ForMember(dest => dest.LedReader0, opt => opt.MapFrom(src => (src.DeviceType == (short)DeviceType.Icu300N 
                                                                            || src.DeviceType == (short)DeviceType.Icu300NX
                                                                            || src.DeviceType == (short)DeviceType.Icu400) ? src.LedReader0 == 0 ? "Blue" : src.LedReader0 == 1 ? "Red" : "" : null))
                .ForMember(dest => dest.LedReader1, opt => opt.MapFrom(src => src.LedReader1 == 0 ? "Blue" : src.LedReader1 == 1 ? "Red" : ""))
                .ForMember(dest => dest.UseCardReader, opt => opt.MapFrom(src => src.DeviceType == (short)DeviceType.ITouchPop ? src.UseCardReader == 0 ? "Use" : src.UseCardReader == 1 ? "Not use" : "" : null))

                .ForMember(dest => dest.BuzzerReader0, opt => opt.MapFrom(src => src.BuzzerReader0 == 0 ? "ON" : src.BuzzerReader0 == 1 ? "OFF" : ""))
                .ForMember(dest => dest.BuzzerReader1, opt => opt.MapFrom(src => src.BuzzerReader1 == 0 ? "ON" : src.BuzzerReader1 == 1 ? "OFF" : ""))

                .ForMember(dest => dest.MPRCount, opt => opt.MapFrom(src => src.MPRCount))
                .ForMember(dest => dest.MPRInterval, opt => opt.MapFrom(src => src.MPRInterval))

                .ForMember(dest => dest.Valid, opt => opt.MapFrom(src => src.Status == 0 ? "Valid" : "Invalid"))

                .ForMember(dest => dest.DeviceType, opt => opt.MapFrom(src => src.DeviceType))

                .ForMember(dest => dest.SensorAlarm, opt => opt.MapFrom(src => src.SensorAlarm))
                .ForMember(dest => dest.CloseReverseLockFlag, opt => opt.MapFrom(src => src.CloseReverseLockFlag));

            CreateMap<IcuDevice, DeviceListModelForUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress));

            CreateMap<IcuDevice, RecoveryDeviceModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress))
                .ForMember(dest => dest.BuildingName, opt => opt.MapFrom(src => src.Building.Name))
                //.ForMember(dest => dest.ProcessId, opt => opt.MapFrom(src => (new Random()).Next(1000000000).ToString()));
                .ForMember(dest => dest.ProcessId, opt => opt.MapFrom(src => rnd.Next(1000000000).ToString()));
            
            CreateMap<IcuDevice, DoorListModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.IsDelete, opt => opt.MapFrom(src => src.IsDeleted));

            CreateMap<IcuDevice, MasterCardModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.DoorName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DeviceAddress, opt => opt.MapFrom(src => src.DeviceAddress));

            CreateMap<DeviceSettingResponseDetail, DeviceSettingDetail>()
                .ForMember(dest => dest.VerifyMode, opt => opt.MapFrom(src => src.VerifyMode))
                .ForMember(dest => dest.BackupPeriod, opt => opt.MapFrom(src => src.BackupPeriod))

                .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
                .ForMember(dest => dest.ServerIp, opt => opt.MapFrom(src => src.ServerIp))
                .ForMember(dest => dest.ServerPort, opt => opt.MapFrom(src => src.ServerPort))

                .ForMember(dest => dest.AntiPassback, opt => opt.MapFrom(src => src.AntiPassback))

                .ForMember(dest => dest.DoorSensorType, opt => opt.MapFrom(src => src.DoorSensorType))
                .ForMember(dest => dest.LockOpenDuration, opt => opt.MapFrom(src => src.LockOpenDuration))
                .ForMember(dest => dest.SensorDuration, opt => opt.MapFrom(src => src.SensorDuration))

                .ForMember(dest => dest.Valid, opt => opt.MapFrom(src => src.Valid == 0 ? "Valid" : "Invalid"))

                .ForMember(dest => dest.SensorAlarm, opt => opt.MapFrom(src => src.SensorAlarm))
                .ForMember(dest => dest.CloseReverseLockFlag, opt => opt.MapFrom(src => src.CloseReverseLockFlag))

                .ForMember(dest => dest.MPRCount, opt => opt.MapFrom(src => src.MPRCount))
                .ForMember(dest => dest.MPRInterval, opt => opt.MapFrom(src => src.MPRInterval))

                .ForMember(dest => dest.CardCount, opt => opt.MapFrom(src => src.CardCount));


            CreateMap<SystemLog, DeviceHistoryModel>()
                .ForMember(dest => dest.EventTime, opt => opt.MapFrom(src => src.OpeTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => ((ActionLogType)src.Action).GetDescription()))
                .ForMember(dest => dest.EventDetails, opt => opt.MapFrom(src => src.Content + "\n" + src.ContentDetails))
                .ForMember(dest => dest.Operator, opt => opt.MapFrom(src => src.CreatedByNavigation.Username))
                .ForMember(dest => dest.AccessTimeUtc, opt => opt.MapFrom(src => src.OpeTime));

            CreateMap<EventLog, DeviceHistoryModel>()
                .ForMember(dest => dest.EventTime, opt => opt.MapFrom(src => src.EventTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)))
                .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => ((EventType)src.EventType).GetDescription()))
                .ForMember(dest => dest.EventDetails, opt => opt.MapFrom(src => ""))
                .ForMember(dest => dest.Operator, opt => opt.MapFrom(src => ""))
                .ForMember(dest => dest.AccessTimeUtc, opt => opt.MapFrom(src => src.EventTime));
            
            CreateMap<IcuDeviceProtocolDetailData, LprProtocolDetailData>()
                .ForMember(dest => dest.AntiPassback,
                    opt => opt.MapFrom(src => src.AntiPassback))
                .ForMember(dest => dest.VerifyMode,
                    opt => opt.MapFrom(src => src.VerifyMode))
                .ForMember(dest => dest.LockOpenDuration,
                    opt => opt.MapFrom(src => src.LockOpenDuration))
                .ForMember(dest => dest.DoorSensorType,
                    opt => opt.MapFrom(src => src.DoorSensorType))
                .ForMember(dest => dest.SensorDuration,
                    opt => opt.MapFrom(src => src.SensorDuration))
                .ForMember(dest => dest.SensorAlarm,
                    opt => opt.MapFrom(src => src.SensorAlarm))
                .ForMember(dest => dest.CloseReverseLockFlag,
                    opt => opt.MapFrom(src => src.CloseReverseLockFlag))
                .ForMember(dest => dest.MPRCount,
                    opt => opt.MapFrom(src => src.MPRCount))
                .ForMember(dest => dest.MPRInterval,
                    opt => opt.MapFrom(src => src.MPRInterval))
                .ForMember(dest => dest.DoorSensorType,
                    opt => opt.MapFrom(src => src.DoorSensorType))
                .ForMember(dest => dest.BackupPeriod,
                    opt => opt.MapFrom(src => src.BackupPeriod))
                .ForMember(dest => dest.qrAesKey,
                    opt => opt.MapFrom(src => src.qrAesKey))
                .ForMember(dest => dest.ActiveTimezone,
                    opt => opt.MapFrom(src => src.ActiveTimezone))
                .ForMember(dest => dest.DeviceBuzzer,
                    opt => opt.MapFrom(src => src.DeviceBuzzer));
            
        }

        private short GetCardReaderLed(short ledIn, short ledOut)
        {
            switch (ledIn)
            {
                case 0 when ledOut == 0:
                    return 0; // In: Blue, Out: Blue
                case 0 when ledOut == 1:
                    return 1; // In: Blue, Out: Red
                case 1 when ledOut == 0:
                    return 2; // In: Red, Out: Blue
                case 1 when ledOut == 1:
                    return 3; // In: Red, Out: Blue
            }

            return 0; //Return default In: Blue, Out: Blue
        }
    }
}