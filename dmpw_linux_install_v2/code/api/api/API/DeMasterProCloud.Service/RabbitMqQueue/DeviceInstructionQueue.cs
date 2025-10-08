using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using AutoMapper;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.DeviceSDK;
using DeMasterProCloud.DataModel.FirmwareVersion;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Service.Protocol;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Constants = DeMasterProCloud.Common.Infrastructure.Constants;

namespace DeMasterProCloud.Service.RabbitMqQueue
{
    public class DeviceInstructionQueue
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDeviceSDKService _deviceSDKService;
        private readonly ISendMessageService _sendMessageService;
        private readonly IConfiguration _configuration;
        private readonly IWebSocketService _webSocketService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly SDKSettingModel _sdkConfig;
        private static readonly object _dbLock = new object();

        public DeviceInstructionQueue(IUnitOfWork unitOfWork, IConfiguration configuration, IWebSocketService webSocketService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _deviceSDKService = new DeviceSDKService(ApplicationVariables.Configuration);
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<DeviceInstructionQueue>();
            _mapper = MapperInstance.Mapper;
            _webSocketService = webSocketService;
            _sendMessageService = new SendMessageService(ApplicationVariables.Configuration);
            _sdkConfig = configuration.GetSection(Constants.SDKDevice.DefineConfig).Get<SDKSettingModel>();
        }

        public void SendLoadDeviceInfo(LoadDeviceInfoQueueModel model)
        {
            try
            {
                var device = _unitOfWork.IcuDeviceRepository.GetDeviceByAddress(model.DeviceAddress);
                if (device == null)
                    throw new Exception($"Can not get device by id = {model.DeviceAddress}");

                // publish message
                var operationTypeList = EnumHelper.ToEnumList<OperationType>().Select(m => m.Id).ToList();
                var response = _deviceSDKService.GetDeviceInfo(model.DeviceAddress);
                if (response != null)
                {
                    bool isChangedInfo = CheckChangeDeviceInfo(device, response);
                    device.FirmwareVersion = response.Version;
                    device.VersionReader0 = response.Reader0Version;
                    device.VersionReader1 = response.Reader1Version;
                    device.NfcModuleVersion = response.NfcModuleVersion;
                    device.ExtraVersion = response.ExtraVersion == null ? "" : MakeVersionString(response.ExtraVersion);
                    device.IpAddress = string.IsNullOrEmpty(response.IpAddress) ? device.IpAddress : response.IpAddress;
                    device.RegisterIdNumber = response.UserCount;
                    device.ServerIp = response.ServerIp;
                    device.ServerPort = string.IsNullOrEmpty(response.ServerPort) ? 0 : Convert.ToInt32(response.ServerPort);
                    device.NumberOfNotTransmittingEvent = response.EventNotTransCount;
                    device.EventCount = response.EventCount;
                    device.OperationType = operationTypeList.Contains(response.OperationMode) ? (short)response.OperationMode : (short)OperationType.Entrance;
                    device.MacAddress = string.IsNullOrEmpty(device.MacAddress) ? response.MacAddress : device.MacAddress;
                    device.LastCommunicationTime = DateTime.UtcNow;
                    
                    // integrated device
                    if (response.IntegratedDevices != null && response.IntegratedDevices.Any())
                    {
                        foreach (var integratedDevice in response.IntegratedDevices)
                        {
                            if (integratedDevice.Type == Constants.HanetApiCamera.TypeCameraLoadInfo)
                            {
                                var camera = _unitOfWork.CameraRepository.Get(m => m.CameraId == integratedDevice.Id);
                                if(camera != null)
                                {
                                    if (camera.CompanyId != device.CompanyId)
                                    {
                                        _logger.LogWarning($"Sync camera hanet exception. Camera {camera.CameraId} existed in other company (id = {camera.CompanyId})");
                                    }
                                    else
                                    {
                                        if (camera.IcuDeviceId != device.Id)
                                        {
                                            camera.IcuDeviceId = device.Id;
                                        }

                                        camera.RoleReader = integratedDevice.Role;
                                        _unitOfWork.CameraRepository.Update(camera);
                                        lock (_dbLock) { _unitOfWork.Save(); }
                                    }
                                }
                                else
                                {
                                    var setting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.CameraSetting, device.CompanyId.Value);
                                    var settingCamera = JsonConvert.DeserializeObject<CameraSetting>(setting.Value);
                                    camera = new Camera()
                                    {
                                        Name = integratedDevice.Id,
                                        CameraId = integratedDevice.Id,
                                        RoleReader = integratedDevice.Role,
                                        CompanyId = device.CompanyId.Value,
                                        IcuDeviceId = device.Id,
                                        Type = (short)CameraType.CameraHanet,
                                        PlaceID = settingCamera.PlaceId
                                    };
                                    _unitOfWork.CameraRepository.Add(camera);
                                    lock (_dbLock) { _unitOfWork.Save(); }
                                }
                            }
                        }
                    }

                    lock (_dbLock)
                    {
                        _unitOfWork.IcuDeviceRepository.Update(device);
                        _unitOfWork.Save();
                    }
                    
                    // change connection status: save event-log and send to fe
                    if (device.ConnectionStatus != (short)ConnectionStatus.Online)
                    {
                        IDeviceService deviceService = new DeviceService(_unitOfWork);
                        deviceService.ChangedConnectionStatus(device, (short)ConnectionStatus.Online);
                    }
                    else if (isChangedInfo)
                    {
                        _sendMessageService.SendDeviceStatusToFE(device, device.DoorStatus, null, _unitOfWork);
                    }

                    int countCardInDb = _unitOfWork.CardRepository.GetCardAvailableInDevice(device.Id).Count();
                    if (countCardInDb != device.RegisterIdNumber)
                    {
                        Console.WriteLine($"[WARNING DEVICE INFO]: the device {device.DeviceAddress} (FW: {device.FirmwareVersion}) not synced: {device.RegisterIdNumber}/{countCardInDb}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }
        private bool CheckChangeDeviceInfo(IcuDevice oldInfo, SDKDeviceInfoModel newInfo)
        {
            if (!string.Equals(oldInfo.FirmwareVersion ?? "", newInfo.Version ?? "", StringComparison.Ordinal))
                return true;
            if (!string.Equals(oldInfo.VersionReader0 ?? "", newInfo.Reader0Version ?? "", StringComparison.Ordinal))
                return true;
            if (!string.Equals(oldInfo.VersionReader1 ?? "", newInfo.Reader1Version ?? "", StringComparison.Ordinal))
                return true;
            if (!string.Equals(oldInfo.NfcModuleVersion ?? "", newInfo.NfcModuleVersion ?? "", StringComparison.Ordinal))
                return true;

            string newExtraVersion = newInfo.ExtraVersion == null ? "" : MakeVersionString(newInfo.ExtraVersion);
            if (!string.Equals(oldInfo.ExtraVersion ?? "", newExtraVersion, StringComparison.Ordinal))
                return true;

            if (!string.Equals(oldInfo.IpAddress ?? "", newInfo.IpAddress ?? "", StringComparison.Ordinal))
                return true;
            if (oldInfo.RegisterIdNumber != newInfo.UserCount)
                return true;
            if (!string.Equals(oldInfo.ServerIp ?? "", newInfo.ServerIp ?? "", StringComparison.Ordinal))
                return true;

            int newPort = string.IsNullOrEmpty(newInfo.ServerPort) ? 0 : Convert.ToInt32(newInfo.ServerPort);
            if (oldInfo.ServerPort != newPort)
                return true;

            if (oldInfo.NumberOfNotTransmittingEvent != newInfo.EventNotTransCount)
                return true;
            if (oldInfo.EventCount != newInfo.EventCount)
                return true;
            if (oldInfo.OperationType != newInfo.OperationMode)
                return true;
            if (!string.Equals(oldInfo.MacAddress ?? "", newInfo.MacAddress ?? "", StringComparison.Ordinal))
                return true;

            return false;
        }
        private string MakeVersionString(Dictionary<string, string> dictionary)
        {
            List<string> versions = new List<string>();

            foreach (KeyValuePair<string, string> keyValues in dictionary)
            {
                versions.Add("[" + keyValues.Key + "] " + keyValues.Value);
            }
            return string.Join("\n", versions);
        }

        public void SendDeviceConfig(ConfigQueueModel model)
        {
            try
            {
                var device = _unitOfWork.IcuDeviceRepository.GetById(model.DeviceId);
                if (device == null)
                    throw new Exception($"Can not get device by id = {model.DeviceId}");

                // publish message
                var message = MakeDeviceProtocolData(model, device);
                _deviceSDKService.UpdateDeviceConfig(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }

        public void SendUpdateTimezone(UpdateTimezoneQueueModel model)
        {
            try
            {
                var device = _unitOfWork.IcuDeviceRepository.GetById(model.DeviceId);
                if (device == null)
                    throw new Exception($"Can not get device by id = {model.DeviceId}");
                if (!device.CompanyId.HasValue)
                    throw new Exception($"Device {device.DeviceAddress} not assign to company");

                var timezones = _unitOfWork.AccessTimeRepository.GetByCompany(device.CompanyId).OrderBy(m => m.Id);
                foreach (var timezone in timezones)
                {
                    _deviceSDKService.SetAccessTime(device.DeviceAddress,[_mapper.Map<SDKAccessTimeDetailModel>(timezone)]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }
        public void SendHoliday(HolidayQueueModel model)
        {
            try
            {
                int companyId = _unitOfWork.IcuDeviceRepository.GetDeviceByAddress(model.DeviceAddress)?.CompanyId ?? 0;
                var holidays = _unitOfWork.HolidayRepository.GetHolidayByCompany(companyId);
                int countAllHoliday = 0;
                foreach (var holiday in holidays)
                {
                    var listDate = DateTimeHelper.GetListRangeDate(holiday.StartDate, holiday.EndDate);
                    countAllHoliday += listDate.Count;
                }
                var holidaysData = _mapper.Map<List<SDKHolidayDetailModel>>(holidays);
                _deviceSDKService.SetHoliday(model.DeviceAddress, holidaysData, countAllHoliday);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }

        public void SendDeviceInstruction(InstructionQueueModel model)
        {
            try
            {
                string message;
                var content = "";
                var contentsDetails = "";
                List<string> details;

                var device = _unitOfWork.IcuDeviceRepository.GetById(model.DeviceId);
                if (device == null)
                    throw new Exception($"Can not get device by id = {model.DeviceId}");

                var building = device.BuildingId.HasValue ? _unitOfWork.BuildingRepository.GetById(device.BuildingId.Value) : null;
                string timezone = building?.TimeZone ?? "Etc/UTC";

                if (model.Command == Constants.CommandType.Open)
                {
                    var systemLogs = _unitOfWork.AppDbContext.SystemLog
                            .Include(m => m.CreatedByNavigation)
                            .Include(m => m.Company)
                            .Where(m => m.CompanyId == device.CompanyId && m.Type == (int)SystemLogType.Emergency && m.Content.Contains(device.DeviceAddress))
                            .OrderByDescending(m => m.OpeTime)
                            .FirstOrDefault();

                    var lastEmergencyCommand = systemLogs?.Action ?? (int)ActionLogType.Release;
                    if (lastEmergencyCommand != (int)ActionLogType.Release)
                    {
                        // Forced open or close state.
                        return;
                    }
                }

                // init message send to device
                switch (model.Command)
                {
                    case Constants.CommandType.Open:
                    {
                        if (device.MaxOpenDuration.HasValue)
                        {
                            if (model.OpenPeriod > device.MaxOpenDuration.Value)
                            {
                                model.OpenPeriod = device.MaxOpenDuration.Value;
                            }

                            if (model.OpenUtilTime != DateTime.MinValue && model.OpenUtilTime.Subtract(DateTime.UtcNow).TotalSeconds > device.MaxOpenDuration.Value)
                            {
                                model.OpenUtilTime = DateTime.UtcNow.AddSeconds(device.MaxOpenDuration.Value);
                            }
                        }

                        string openUntilTime = "";
                        if (model.OpenUtilTime != DateTime.MinValue)
                        {
                            openUntilTime = Helpers.ConvertDateTimeToStringByTimeZone(model.OpenUtilTime, building?.TimeZone);
                        }
                        
                        bool isSuccess = _deviceSDKService.OpenDoor(device.DeviceAddress, new SDKOpenDoorModel()
                        {
                            OpenPeriod = model.OpenPeriod,
                            OpenUntilTime = openUntilTime,
                        });

                        //Save system log
                        if (isSuccess && model.IsSaveSystemLog)
                        {
                            content = string.Format(DeviceResource.msgSendDeviceCommand, DeviceResource.lblOpenDoor);
                            details = new List<string>()
                            {
                                $"{DeviceResource.lblCommand} : {DeviceResource.lblOpenDoor}",
                                $"{DeviceResource.lblDeviceAddress} : {device.DeviceAddress}",
                                $"{DeviceResource.lblDoorName} : {device.Name}",
                                $"{DeviceResource.lblOpenPeriod } : {model.OpenPeriod}",
                                $"{DeviceResource.lblOpenUntilTime } : {openUntilTime}"
                            };
                            contentsDetails = string.Join("<br />", details);

                            _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceMonitoring, ActionLogType.DoorOpen, content, contentsDetails, null, device.CompanyId);
                            lock (_dbLock) { _unitOfWork.Save(); }
                        }
                        
                        return;
                    }
                    case Constants.CommandType.SetTime:
                    {
                        bool isSuccess = _deviceSDKService.SetTime(device.DeviceAddress, timezone);

                        //Save system log
                        if (isSuccess && model.IsSaveSystemLog)
                        {
                            content = string.Format(DeviceResource.msgSendDeviceCommand, ActionLogTypeResource.SetTime);
                            details = new List<string>()
                            {
                                $"{DeviceResource.lblCommand} : {DeviceResource.lblTransmitCurrentTime}",
                                $"{DeviceResource.lblDeviceAddress} : {device.DeviceAddress}",
                                $"{DeviceResource.lblDoorName} : {device.Name}"
                            };
                            contentsDetails = string.Join("<br />", details);
                            _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceMonitoring, ActionLogType.Sync, content, contentsDetails, null, device.CompanyId);
                            lock (_dbLock) { _unitOfWork.Save(); }
                        }
                        return;
                    }
                    case Constants.CommandType.UpdateDeviceState:
                    {
                        _deviceSDKService.DeviceInstruction(device.DeviceAddress, model.Command);
                        return;
                    }
                    case Constants.CommandType.UpdateFirmware:
                    {

                        // send to SDK
                        string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiUpdateFirmware}";
                        string token = ApplicationVariables.SDKToken;
                        var data = new FirmwareVersionDeviceModel()
                        {
                            DeviceAddress = device.DeviceAddress,
                            DeviceType = device.DeviceType,
                            Sender = model.Sender,
                            RoleReader0 = device.RoleReader0,
                            RoleReader1 = device.RoleReader1,
                            VersionReader0 = device.VersionReader0,
                            VersionReader1 = device.VersionReader1,
                            ExtraVersion = device.ExtraVersion,
                            UrlUploadFileResponse = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>(),
                            LinkFile = model.LinkFile,
                            Version = model.Version,
                            Target = model.Target,
                            FwType = model.FwType,
                        };
                               
                        Helpers.UploadFileMultipartFormData(url, model.FileName, model.FileData, data, token).GetAwaiter().GetResult();
                        
                        //Save system log
                        if (model.IsSaveSystemLog)
                        {
                            content = string.Format(DeviceResource.msgSendDeviceCommand, model.Command);
                            details = new List<string>()
                        {

                            $"{DeviceResource.lblDeviceAddress} : {device.DeviceAddress}",
                            $"{DeviceResource.lblDoorName} : {device.Name}",
                            $"{CommonResource.lblFileName} : {model.FileName}"
                        };
                            contentsDetails = string.Join("<br />", details);

                            _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceUpdate, ActionLogType.UpdateDoor, content, contentsDetails, null, device.CompanyId);
                            lock (_dbLock) { _unitOfWork.Save(); }
                        }
                        break;
                    }
                    case Constants.CommandType.RequestFirmwareVersion:
                    {
                        var firmware = _unitOfWork.FirmwareVersionRepository.Gets().Where(m => device.DeviceType == m.DeviceType).OrderByDescending(n => n.CreatedOn).FirstOrDefault();
                      
                        // send device instruction
                        string linkFile = "";

                        if (device.CompanyId.HasValue)
                        {
                            var companyOfDevice = _unitOfWork.CompanyRepository.GetById(device.CompanyId.Value);
                            string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                            string textUtcNow = DateTime.UtcNow.ToString(Constants.DateTimeFormat.DdMMyyyyHH);
                            string hash = CryptographyHelper.GetMD5Hash($"{firmware.FileName}{companyOfDevice.Code}{textUtcNow}");
                            linkFile = $"{connectionApi}{Constants.Route.ApiFirmwareVersionDeviceDownload}?fileName={firmware.FileName}&hash={hash}";
                        }

                        // send to SDK
                        string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiUpdateFirmware}";
                        string token = ApplicationVariables.SDKToken;
                        var data = new FirmwareVersionDeviceModel()
                        {
                            DeviceAddress = device.DeviceAddress,
                            DeviceType = device.DeviceType,
                            Sender = model.Sender,
                            RoleReader0 = device.RoleReader0,
                            RoleReader1 = device.RoleReader1,
                            VersionReader0 = device.VersionReader0,
                            VersionReader1 = device.VersionReader1,
                            ExtraVersion = device.ExtraVersion,
                            UrlUploadFileResponse = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>(),
                            LinkFile = linkFile,
                         
                            Version = firmware.Version,

                            Target = firmware.FileName.Split("_")[0],
                            FwType = firmware.FileName.Split("_")[0]
                        };
                               
                        Helpers.UploadFileMultipartFormData(url, firmware.FileName, new byte[]{}, data, token).GetAwaiter().GetResult();
                        
                        //Save system log
                        if (model.IsSaveSystemLog)
                        {
                            content = string.Format(DeviceResource.msgSendDeviceCommand, model.Command);
                            details = new List<string>()
                        {

                            $"{DeviceResource.lblDeviceAddress} : {device.DeviceAddress}",
                            $"{DeviceResource.lblDoorName} : {device.Name}",
                            $"{CommonResource.lblFileName} : {model.FileName}"
                        };
                            contentsDetails = string.Join("<br />", details);

                            _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceUpdate, ActionLogType.UpdateDoor, content, contentsDetails, null, device.CompanyId);
                            lock (_dbLock) { _unitOfWork.Save(); }
                        }
                        break;
                    }
                    case Constants.CommandType.SendConfigLocalMqtt:
                    {
                        var data = new DeviceInstructionLocalMqttProtocolData()
                        {
                            MsgId = Helpers.CreateMsgIdProcess(model.MsgId, model.MessageIndex, model.MessageTotal, Constants.CommandType.SendConfigLocalMqtt),
                            Sender = model.Sender,
                            Type = model.MessageType,
                            Data = new DeviceInstructionLocalMqttDetail()
                            {
                                Command = model.Command,
                                UserName = model.UserName,
                                LocalMqtt = model.LocalMqtt,
                            }
                        };

                        message = data.ToString();
                        break;
                    }
                    case Constants.CommandType.ActiveFaceLicense:
                    {
                        var data = new DeviceInstructionActiveLicenseProtocolData()
                        {
                            MsgId = Helpers.CreateMsgIdProcess(model.MsgId, model.MessageIndex, model.MessageTotal, Constants.CommandType.ActiveFaceLicense),
                            Sender = model.Sender,
                            Type = model.MessageType,
                            Data = new DeviceInstructionActiveLicenseDetail()
                            {
                                Command = model.Command,
                                UserName = model.UserName,
                                Data = new ActiveFaceSetting()
                                {
                                    Serial = model.Serial,
                                    Key = model.Key,
                                    ActivationCode = model.ActivationCode,
                                },
                            }
                        };

                        message = data.ToString();
                        break;
                    }
                    case Constants.CommandType.RequestLogFile:
                    {
                        var data = new RequestDataProtocolData()
                        {
                            MsgId = Helpers.CreateMsgIdProcess(model.MsgId, model.MessageIndex, model.MessageTotal, Constants.CommandType.RequestLogFile),
                            Type = model.MessageType,
                            Sender = model.Sender,
                            Data = new RequestDataDetail()
                            {
                                Command = Constants.CommandType.RequestLogFile,
                            }
                        };
                        _deviceSDKService.DeviceInstruction(device.DeviceAddress, Constants.CommandType.RequestLogFile);
                        if (device.CompanyId.HasValue)
                        {
                            var company = _unitOfWork.CompanyRepository.GetById(device.CompanyId.Value);
                            string pathFileRequestDevice = $"{Constants.Settings.DefineFolderDataLogs}/{company.Code}/{device.DeviceAddress}/{Constants.FileConfig.FileNameRequestLog}";
                            FileHelpers.AddTextToFile(pathFileRequestDevice, data.MsgId + "\n");
                            string language = Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetLanguage(company.Id).Value) ?? Constants.DefaultLanguage;
                            
                            new Thread(() =>
                            {
                                try
                                {
                                    int timeout = ApplicationVariables.Configuration.GetSection("Limit:TimeoutRequestDevice").Get<int>();
                                    timeout = timeout > 0 ? timeout : 60;
                                    Thread.Sleep(1000 * timeout);
                                    
                                    string contentFile = File.ReadAllText(pathFileRequestDevice);
                                    if (contentFile.Contains(data.MsgId))
                                    {
                                        contentFile = contentFile.Replace(data.MsgId + "\n", "");
                                        File.WriteAllText(pathFileRequestDevice, contentFile);
                                        
                                        var notification = new NotificationProtocolDataDetail
                                        {
                                            MessageType = Constants.MessageType.Error,
                                            NotificationType = Constants.NotificationType.SendDeviceInstructionError,
                                            Message = DeviceResource.ResourceManager.GetString("msgDeviceNotResponse",
                                                    new CultureInfo(language)) + $" ({device.Name})",
                                        };
                                        _webSocketService.SendWebSocketToFE(Constants.Protocol.NotificationNewFileLogDevice,company.Id, notification);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }).Start();
                        }
                        break;
                    }
                    default:
                    {
                        _deviceSDKService.DeviceInstruction(device.DeviceAddress, model.Command);

                        if (model.IsSaveSystemLog)
                        {
                            switch (model.Command)
                            {
                                case Constants.CommandType.Reset:
                                {
                                    content = string.Format(DeviceResource.msgSendDeviceCommand, DeviceResource.lblReset);
                                    details = new List<string>
                                    {
                                        $"{DeviceResource.lblCommand} : {DeviceResource.lblReset}",
                                        $"{DeviceResource.lblDeviceAddress} : {device.DeviceAddress}",
                                        $"{DeviceResource.lblDoorName} : {device.Name}"
                                    };
                                    contentsDetails = string.Join("<br />", details);
                                    _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceMonitoring, ActionLogType.Reset, content, contentsDetails, null, device.CompanyId);
                                    lock (_dbLock) { _unitOfWork.Save(); }
                                    break;
                                }
                                case Constants.CommandType.ForceOpen:
                                {
                                    content = string.Format(DeviceResource.msgSendDeviceCommand, DeviceResource.lblForceOpen);
                                    details = new List<string>
                                    {
                                        $"{DeviceResource.lblCommand} : {DeviceResource.lblForceOpen}",
                                        $"{DeviceResource.lblDeviceAddress} : {device.DeviceAddress}",
                                        $"{DeviceResource.lblDoorName} : {device.Name}"
                                    };
                                    contentsDetails = string.Join("<br />", details);
                                    _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.Emergency, ActionLogType.ForcedOpen, content, contentsDetails, null, device.CompanyId);
                                    lock (_dbLock) { _unitOfWork.Save(); }
                                    break;
                                }
                                case Constants.CommandType.ForceClose:
                                {
                                    content = string.Format(DeviceResource.msgSendDeviceCommand, DeviceResource.lblForceClose);
                                    details = new List<string>
                                    {
                                        $"{DeviceResource.lblCommand} : {DeviceResource.lblForceClose}",
                                        $"{DeviceResource.lblDeviceAddress} : {device.DeviceAddress}",
                                        $"{DeviceResource.lblDoorName} : {device.Name}"
                                    };
                                    contentsDetails = string.Join("<br />", details);
                                    _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.Emergency, ActionLogType.ForcedClose, content, contentsDetails, null, device.CompanyId);
                                    lock (_dbLock) { _unitOfWork.Save(); }
                                    break;
                                }
                                case Constants.CommandType.Release:
                                {
                                    content = string.Format(DeviceResource.msgSendDeviceCommand, DeviceResource.lblRelease);
                                    details = new List<string>
                                    {
                                        $"{DeviceResource.lblCommand} : {DeviceResource.lblRelease}",
                                        $"{DeviceResource.lblDeviceAddress} : {device.DeviceAddress}",
                                        $"{DeviceResource.lblDoorName} : {device.Name}"
                                    };
                                    contentsDetails = string.Join("<br />", details);
                                    _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.Emergency, ActionLogType.Release, content, contentsDetails, null, device.CompanyId);
                                    lock (_dbLock) { _unitOfWork.Save(); }
                                    break;
                                }
                                case Constants.CommandType.TurnOnAlarm:
                                case Constants.CommandType.TurnOffAlarm:
                                {
                                    var command = DeviceResource.lblAlarm + (model.Command == Constants.CommandType.TurnOnAlarm ? " on" : " off");
                                    content = string.Format(DeviceResource.msgSendDeviceCommand, command);
                                    details = new List<string>
                                    {
                                        $"{DeviceResource.lblCommand} : {command}",
                                        $"{DeviceResource.lblDeviceAddress} : {device.DeviceAddress}",
                                        $"{DeviceResource.lblDoorName} : {device.Name}"
                                    };
                                    contentsDetails = string.Join("<br />", details);
                                    _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceSetting, ActionLogType.UpdateDoor, content, contentsDetails, null, device.CompanyId);
                                    lock (_dbLock) { _unitOfWork.Save(); }
                                    break;
                                }
                            }
                        }

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }

        public void SendInstructionCommon(InstructionCommonModel model)
        {
            try
            {
                Console.WriteLine($"[DEBUG] SendInstructionCommon: MessageType={model.MessageType}, DeviceIds=[{string.Join(",", model.DeviceIds ?? new List<int>())}], UserIds=[{string.Join(",", model.UserIds ?? new List<int>())}]");
                
                var company = _unitOfWork.CompanyRepository.GetCompanyByCode(model.CompanyCode);
                if (company == null)
                    throw new Exception($"Can not get company by code = {model.CompanyCode}");

                IWebSocketService webSocketService = new WebSocketService();
                var accessControlQueue = new AccessControlQueue(_unitOfWork, webSocketService);

                // split device
                var newSplitDevice = SplitMessageSend(model);
                if (newSplitDevice != null && newSplitDevice.Any())
                {
                    foreach (var item in newSplitDevice)
                    {
                        if (item.DeviceType == -1 || item.CompanyCode.Equals("TEMP_DATA"))
                            continue;

                        SendInstructionCommon(item);
                    }
                    return;
                }

                // devices - elevator. It will use topic access_control
                var deviceElevators = new List<IcuDevice>();

                // devices - firmware version 1
                if (model.DeviceIds != null && model.DeviceIds.Any())
                {
                    Console.WriteLine($"[DEBUG] Filtering devices: Total DeviceIds={model.DeviceIds.Count}");
                    var devicesVer1 = _unitOfWork.IcuDeviceRepository
                        .Gets(m => model.DeviceIds.Contains(m.Id) && !m.IsDeleted && m.CompanyId == company.Id
                                              && (string.IsNullOrEmpty(m.FirmwareVersion) || !m.FirmwareVersion.Contains(Constants.FirmwareVersion2)))
                        .ToList();
                    Console.WriteLine($"[DEBUG] Found {devicesVer1.Count} firmware v1 devices: [{string.Join(",", devicesVer1.Select(d => $"{d.Id}({d.FirmwareVersion})"))}]");

                    if (deviceElevators.Any())
                    {
                        devicesVer1.AddRange(deviceElevators);
                        devicesVer1 = devicesVer1.Distinct().ToList();
                    }

                    if (devicesVer1.Any())
                    {
                        Console.WriteLine($"[DEBUG] Processing {devicesVer1.Count} v1 devices for {model.MessageType}");
                        switch (model.MessageType)
                        {
                            case Constants.Protocol.DeleteUser:
                            case Constants.Protocol.AddUser:
                                {
                                    Console.WriteLine($"[DEBUG] Calling accessControlQueue.SendUserInfo for {model.MessageType}");
                                    accessControlQueue.SendUserInfo(new UserInfoQueueModel()
                                    {
                                        MsgId = model.MsgId,
                                        MessageType = model.MessageType,
                                        Sender = model.Sender,
                                        MessageIndex = model.MessageIndex,
                                        MessageTotal = model.MessageTotal,
                                        UserIds = model.UserIds,
                                        VisitIds = model.VisitIds,
                                        CardIds = model.CardIds,
                                        TotalData = new TotalData()
                                        {
                                            CardIds = model.CardIds,
                                            DeviceIds = devicesVer1.Select(d => d.Id).ToList()
                                        }
                                    });
                                    break;
                                }

                            case Constants.Protocol.LoadDeviceInfo:
                                {
                                    foreach (var deviceVer1 in devicesVer1)
                                    {
                                        SendLoadDeviceInfo(new LoadDeviceInfoQueueModel()
                                        {
                                            MsgId = model.MsgId,
                                            Sender = model.Sender,
                                            MessageType = Constants.Protocol.LoadDeviceInfo,
                                            DeviceAddress = deviceVer1.DeviceAddress,
                                            MessageIndex = model.MessageIndex,
                                            MessageTotal = model.MessageTotal,
                                        });
                                    }
                                    break;
                                }
                            case Constants.Protocol.SetTime:
                                {
                                    foreach (var deviceVer1 in devicesVer1)
                                    {
                                        SendDeviceInstruction(new InstructionQueueModel()
                                        {
                                            MsgId = model.MsgId,
                                            Sender = model.Sender,
                                            MessageType = Constants.Protocol.DeviceInstruction,
                                            Command = Constants.CommandType.SetTime,
                                            DeviceId = deviceVer1.Id,
                                            DeviceAddress = deviceVer1.DeviceAddress,
                                            UserName = model.Sender,
                                            MessageIndex = model.MessageIndex,
                                            MessageTotal = model.MessageTotal,
                                        });
                                    }
                                    break;
                                }
                        }
                    }
                }
                else
                {
                    List<string> messageTypes = new List<string>()
                    {
                        Constants.Protocol.DeleteUser,
                        Constants.Protocol.LoadDeviceInfo,
                        Constants.Protocol.SetTime,
                    };

                    if (messageTypes.Contains(model.MessageType))
                    {
                        var devicesVer1 = _unitOfWork.IcuDeviceRepository
                            .Gets(m => !m.IsDeleted && m.CompanyId == company.Id
                                       && !string.IsNullOrEmpty(m.FirmwareVersion) && !m.FirmwareVersion.Contains(Constants.FirmwareVersion2))
                            .ToList();

                        foreach (var deviceVer1 in devicesVer1)
                        {
                            switch (model.MessageType)
                            {
                                case Constants.Protocol.DeleteUser:
                                    {
                                        accessControlQueue.SendUserInfo(new UserInfoQueueModel()
                                        {
                                            MsgId = model.MsgId,
                                            MessageType = model.MessageType,
                                            Sender = model.Sender,
                                            DeviceId = deviceVer1.Id,
                                            DeviceAddress = deviceVer1.DeviceAddress,
                                            MessageIndex = model.MessageIndex,
                                            MessageTotal = model.MessageTotal,
                                            UserIds = model.UserIds,
                                            VisitIds = model.VisitIds,
                                            CardIds = model.CardIds,
                                        });
                                        break;
                                    }
                                case Constants.Protocol.LoadDeviceInfo:
                                    {
                                        SendLoadDeviceInfo(new LoadDeviceInfoQueueModel()
                                        {
                                            MsgId = model.MsgId,
                                            Sender = model.Sender,
                                            MessageType = Constants.Protocol.LoadDeviceInfo,
                                            DeviceAddress = deviceVer1.DeviceAddress,
                                            MessageIndex = model.MessageIndex,
                                            MessageTotal = model.MessageTotal,
                                        });
                                        break;
                                    }
                                case Constants.Protocol.SetTime:
                                    {
                                        SendDeviceInstruction(new InstructionQueueModel()
                                        {
                                            MsgId = model.MsgId,
                                            Sender = model.Sender,
                                            MessageType = Constants.Protocol.DeviceInstruction,
                                            Command = Constants.CommandType.SetTime,
                                            DeviceId = deviceVer1.Id,
                                            DeviceAddress = deviceVer1.DeviceAddress,
                                            UserName = model.Sender,
                                            MessageIndex = model.MessageIndex,
                                            MessageTotal = model.MessageTotal,
                                        });
                                        break;
                                    }
                            }
                        }
                    }
                }

                // not use device elevator for device instruction common
                //if (deviceElevators.Any() && model.DeviceIds != null && model.DeviceIds.Any())
                //{
                //    var deviceElevatorIds = deviceElevators.Select(m => m.Id).ToList();
                //    model.DeviceIds = model.DeviceIds.Where(m => !deviceElevatorIds.Contains(m)).ToList();
                //}

                var deviceV2Ids = new List<IcuDevice>();

                // Check FW v2 devices.
                if(model.DeviceIds != null && model.DeviceIds.Count > 0)
                {
                    if (deviceElevators.Any())
                    {
                        // Elevator devices don't use 'device_common_instruction' message
                        var deviceElevatorIds = deviceElevators.Select(m => m.Id).ToList();
                        model.DeviceIds = model.DeviceIds.Where(m => !deviceElevatorIds.Contains(m)).ToList();
                    }

                    deviceV2Ids = _unitOfWork.IcuDeviceRepository
                        .Gets(m => model.DeviceIds.Contains(m.Id) 
                                && !m.IsDeleted 
                                && m.CompanyId == company.Id
                                && (!string.IsNullOrEmpty(m.FirmwareVersion) && m.FirmwareVersion.Contains(Constants.FirmwareVersion2)))
                        .ToList();

                    if(deviceV2Ids.Count == 0)
                    {
                        // There is no V2 readers to receive msg.
                        return;
                    }
                }

                // devices - firmware version 2
                switch (model.MessageType)
                {
                    case Constants.Protocol.DeleteUser:
                        {
                            List<int> cardIds = new List<int>();
                            if (model.UserIds != null && model.UserIds.Any())
                            {
                                foreach (var userId in model.UserIds)
                                {
                                    var cardList = _unitOfWork.CardRepository.GetByUserId(company.Id, userId).Select(m => m.Id).ToList();
                                    if (cardList.Any())
                                        cardIds.AddRange(cardList);
                                }
                            }
                            if (model.VisitIds != null && model.VisitIds.Any())
                            {
                                foreach (var visitId in model.VisitIds)
                                {
                                    var cardList = _unitOfWork.CardRepository.GetByVisitId(company.Id, visitId).Select(m => m.Id).ToList();
                                    if (cardList.Any())
                                        cardIds.AddRange(cardList);
                                }
                            }
                            if (model.CardIds != null && model.CardIds.Any())
                            {
                                cardIds.AddRange(model.CardIds);
                            }
                            if (model.CardFilterIds != null && model.CardFilterIds.Any())
                            {
                                cardIds = cardIds.Where(m => model.CardFilterIds.Contains(m)).ToList();
                            }

                            cardIds = cardIds.Distinct().ToList();
                            if (cardIds.Any())
                            {
                                var building = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(company.Id) ?? new Building();
                                var device = _unitOfWork.IcuDeviceRepository.GetByCompany(company.Id).FirstOrDefault() ?? new IcuDevice();

                                int maxSplitMessage = Helpers.GetMaxSplit((short)DeviceType.Icu300N); // default minimum
                                int countData = cardIds.Count;
                                int lengthMessage = countData / maxSplitMessage;
                                lengthMessage = lengthMessage * maxSplitMessage < countData ? lengthMessage + 1 : lengthMessage;

                                for (int i = 0; i < lengthMessage; i++)
                                {
                                    int countCard = maxSplitMessage;
                                    if (i == lengthMessage - 1 && (i + 1) * maxSplitMessage > countData)
                                    {
                                        countCard = countData - i * maxSplitMessage;
                                    }

                                    var cardListPaging = _unitOfWork.CardRepository.GetCardDetailByIds(cardIds.Skip(i * maxSplitMessage).Take(countCard).ToList()).ToList();
                                    var listCards = accessControlQueue.ConvertToCardInfo(cardListPaging, device, building);

                                    listCards.ForEach(m =>
                                    {
                                        m.FingerTemplates = null;
                                        m.FaceData = null;
                                    });
                                }
                            }

                            break;
                        }
                    case Constants.Protocol.AddUser:
                        {
                            List<int> cardIds = new List<int>();
                            if (model.UserIds != null && model.UserIds.Any())
                            {
                                foreach (var userId in model.UserIds)
                                {
                                    var cardList = _unitOfWork.CardRepository.GetByUserId(company.Id, userId).Select(m => m.Id).ToList();
                                    if (cardList.Any())
                                        cardIds.AddRange(cardList);
                                }
                            }
                            if (model.VisitIds != null && model.VisitIds.Any())
                            {
                                foreach (var visitId in model.VisitIds)
                                {
                                    var cardList = _unitOfWork.CardRepository.GetByVisitId(company.Id, visitId).Select(m => m.Id).ToList();
                                    if (cardList.Any())
                                        cardIds.AddRange(cardList);
                                }
                            }
                            if (model.CardIds != null && model.CardIds.Any())
                            {
                                cardIds.AddRange(model.CardIds);
                            }
                            if (model.CardFilterIds != null && model.CardFilterIds.Any())
                            {
                                cardIds = cardIds.Where(m => model.CardFilterIds.Contains(m)).ToList();
                            }
                            if (model.DeviceType.HasValue && cardIds != null)
                            {
                                List<int> cardTypes = Helpers.GetMatchedIdentificationType(model.DeviceType.Value);
                                var ids = cardIds;
                                cardIds = _unitOfWork.CardRepository.Gets(m => ids.Contains(m.Id) && cardTypes.Contains(m.CardType))
                                    .Select(m => m.Id).ToList();
                            }

                            cardIds = cardIds.Distinct().ToList();
                            if (cardIds.Any() && deviceV2Ids.Any())
                            {
                                var device = _unitOfWork.IcuDeviceRepository.GetById(model.DeviceIds.First()) ?? new IcuDevice();
                                var building = _unitOfWork.BuildingRepository.GetById(device.BuildingId ?? 0) ?? new Building();

                                int maxSplitDevice = Constants.MaxReceiver;
                                int countDevice = deviceV2Ids.Count;
                                int lengthSplitDevice = countDevice / maxSplitDevice;
                                lengthSplitDevice = lengthSplitDevice * maxSplitDevice < countDevice ? lengthSplitDevice + 1 : lengthSplitDevice;

                                for (int j = 0; j < lengthSplitDevice; j++)
                                {
                                    int countRange = j == lengthSplitDevice - 1 ? countDevice - j * maxSplitDevice : maxSplitDevice;
                                    int maxSplitMessage = Helpers.GetMaxSplit((short)DeviceType.Icu300N); // default minimum
                                    int countData = cardIds.Count;
                                    int lengthMessage = countData / maxSplitMessage;
                                    lengthMessage = lengthMessage * maxSplitMessage < countData ? lengthMessage + 1 : lengthMessage;

                                    for (int i = 0; i < lengthMessage; i++)
                                    {
                                        int countCard = maxSplitMessage;
                                        if (i == lengthMessage - 1 && (i + 1) * maxSplitMessage > countData)
                                        {
                                            countCard = countData - i * maxSplitMessage;
                                        }

                                        var cardListPaging = _unitOfWork.CardRepository.GetCardDetailByIds(cardIds.Skip(i * maxSplitMessage).Take(countCard).ToList()).ToList();
                                        var listCards = accessControlQueue.ConvertToCardInfo(cardListPaging, device, building);
                                    }
                                }
                            }

                            break;
                        }
                    case Constants.Protocol.LoadDeviceInfo:
                        {
                            foreach (var deviceVer2 in deviceV2Ids)
                            {
                                SendLoadDeviceInfo(new LoadDeviceInfoQueueModel()
                                {
                                    MsgId = model.MsgId,
                                    Sender = model.Sender,
                                    MessageType = Constants.Protocol.LoadDeviceInfo,
                                    DeviceAddress = deviceVer2.DeviceAddress,
                                    MessageIndex = model.MessageIndex,
                                    MessageTotal = model.MessageTotal,
                                });
                            }
                            break;
                        }
                    case Constants.Protocol.SetTime:
                        {
                            var groupDevices = _unitOfWork.IcuDeviceRepository
                                .Gets(m => !m.IsDeleted && m.DeviceType != (short)DeviceType.DesktopApp && m.CompanyId == company.Id
                                           && (model.DeviceIds == null || (model.DeviceIds.Any() && model.DeviceIds.Contains(m.Id)))
                                           && !string.IsNullOrEmpty(m.FirmwareVersion) && m.FirmwareVersion.Contains(Constants.FirmwareVersion2))
                                .Include(m => m.Building)
                                .ToList()
                                .GroupBy(m => m.Building.TimeZone).ToList();

                            foreach (var groupDevice in groupDevices)
                            {
                                var deviceList = groupDevice.ToList();
                                foreach (var device in deviceList)
                                {
                                    _deviceSDKService.SetTime(device.DeviceAddress, groupDevice.Key);
                                }
                            }

                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }

        public void UploadFile(UploadFileQueueModel model)
        {
            try
            {
                string undefinedChar = "_";
                string[] arrNames = model.File.Name.Split("__");
                if (Enum.GetValues(typeof(DeviceUpdateTarget))
                        .Cast<DeviceUpdateTarget>()
                        .All(e => e.GetDescription() != arrNames[0]) || arrNames.Length != 2)
                {
                    throw new Exception($"File name update device invalid FileName = {model.File.Name}");
                }
                var deviceList = arrNames[1].Split("&&");
                foreach (var item in deviceList)
                {
                    var deviceId = item.Split("::")[0];
                    var processId = item.Split("::")[1];

                    var device = _unitOfWork.IcuDeviceRepository.GetByIcuId(int.Parse(deviceId));
                    string firmwareType = "", targetFile = "", nameCompare = "";
                    string target = arrNames[0];
                    switch (device.DeviceType)
                    {
                        case (short)DeviceType.Icu300N:
                        case (short)DeviceType.Icu300NX:
                            {
                                // Make protocol - Main firmware file
                                if (target.Equals(DeviceType.Icu300N.GetDescription()))
                                {
                                    nameCompare = DeviceType.Icu300N.GetDescription() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = IcuFileType.MainFirmware.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                // Make protocol - Reader0 firmware file
                                var cardReader0 = !string.IsNullOrEmpty(device.VersionReader0) ? device.VersionReader0.Split("_")[0] : string.Empty;
                                if (cardReader0.Equals(target))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = device.RoleReader0 == (short)RoleRules.In ? IcuFileType.InReader.GetDescription() : IcuFileType.OutReader.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                // Make protocol - Reader1 firmware file
                                var cardReader1 = !string.IsNullOrEmpty(device.VersionReader1) ? device.VersionReader1.Split("_")[0] : string.Empty;
                                if (cardReader1.Equals(target, StringComparison.OrdinalIgnoreCase))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = device.RoleReader1 == (short)RoleRules.In ? IcuFileType.InReader.GetDescription() : IcuFileType.OutReader.GetDescription();
                                        targetFile = target;
                                    }
                                }
                                break;
                            }
                        case (short)DeviceType.Icu400:
                            {
                                // Make protocol - Main firmware file
                                if (target.Equals(DeviceType.Icu400.GetDescription()))
                                {
                                    nameCompare = DeviceType.Icu400.GetDescription() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = IcuFileType.MainFirmware.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                // Make protocol - Reader0 firmware file
                                var cardReader0 = !string.IsNullOrEmpty(device.VersionReader0) ? device.VersionReader0.Split("_")[0] : string.Empty;
                                if (cardReader0.Equals(target))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = device.RoleReader0 == (short)RoleRules.In ? IcuFileType.InReader.GetDescription() : IcuFileType.OutReader.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                // Make protocol - Reader1 firmware file
                                var cardReader1 = !string.IsNullOrEmpty(device.VersionReader1) ? device.VersionReader1.Split("_")[0] : string.Empty;
                                if (cardReader1.Equals(target, StringComparison.OrdinalIgnoreCase))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = device.RoleReader1 == (short)RoleRules.In ? IcuFileType.InReader.GetDescription() : IcuFileType.OutReader.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                // Tar file for ICU-400
                                if (DeviceUpdateTarget.Tar.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = DeviceType.Icu400.GetDescription() + undefinedChar;
                                    if (model.File.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.Tar.GetDescription();
                                        targetFile = DeviceUpdateTarget.Tar.GetDescription();
                                    }
                                }

                                break;
                            }
                        case (short)DeviceType.ITouchPop:
                            {
                                if (target.Equals(DeviceUpdateTarget.ITouchPop2A.GetDescription()))
                                {
                                    nameCompare = DeviceType.ITouchPop.GetDescription() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.ITouchPop2A.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                if (DeviceUpdateTarget.Abcm.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.Abcm.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                var extraVersion = !string.IsNullOrEmpty(device.ExtraVersion) ? device.ExtraVersion.Split("_")[0] : string.Empty;
                                if (extraVersion.Equals(target))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.SoundTrack01.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                break;
                            }
                        case (short)DeviceType.ITouchPopX:
                            {
                                if (target.Equals(DeviceUpdateTarget.ITouchPopX.GetDescription()))
                                {
                                    nameCompare = DeviceType.ITouchPopX.GetDescription() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.MainFirmware.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                if (DeviceUpdateTarget.Module.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.ITouchPopX.GetDescription();
                                        targetFile = DeviceUpdateTarget.ITouchPopX.GetDescription();
                                    }
                                }

                                if (DeviceUpdateTarget.Library.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.Library.GetDescription();
                                        targetFile = DeviceUpdateTarget.ITouchPopX.GetDescription();
                                    }
                                }

                                if (DeviceUpdateTarget.Tar.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = DeviceType.ITouchPopX.GetDescription() + undefinedChar;
                                    if (model.File.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.Tar.GetDescription();
                                        targetFile = DeviceUpdateTarget.Tar.GetDescription();
                                    }
                                }

                                break;
                            }
                        case (short)DeviceType.DQMiniPlus:
                            {
                                if (target.Equals(DeviceUpdateTarget.DQMiniPlus.GetDescription()))
                                {
                                    nameCompare = DeviceType.DQMiniPlus.GetDescription() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.MainFirmware.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                if (DeviceUpdateTarget.Tar.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    //nameCompare = DeviceType.DQMiniPlus.GetDescription() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.Tar.GetDescription();
                                        targetFile = DeviceUpdateTarget.Tar.GetDescription();
                                    }
                                }

                                break;
                            }
                        case (short)DeviceType.IT100:
                            {
                                if (target.Equals(DeviceUpdateTarget.IT100.GetDescription()))
                                {
                                    nameCompare = DeviceType.IT100.GetDescription() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.IT100.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                break;
                            }
                        case (short)DeviceType.PM85:
                            {
                                if (target.Equals(DeviceUpdateTarget.PM85.GetDescription()))
                                {
                                    nameCompare = DeviceType.PM85.GetDescription() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.PM85.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                break;
                            }
                        case (short)DeviceType.DP636X:
                            {
                                if (target.Equals(DeviceUpdateTarget.DP636X.GetDescription()))
                                {
                                    nameCompare = DeviceType.DP636X.GetDescription() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.MainFirmware.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                if (DeviceUpdateTarget.Module.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.Module.GetDescription();
                                        targetFile = DeviceUpdateTarget.DP636X.GetDescription();
                                    }

                                }

                                if (DeviceUpdateTarget.Library.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.Library.GetDescription();
                                        targetFile = DeviceUpdateTarget.DP636X.GetDescription();
                                    }

                                }

                                if (DeviceUpdateTarget.Tar.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = DeviceType.DP636X.GetDescription() + undefinedChar;
                                    if (model.File.FileName.Contains(nameCompare))
                                    {
                                        firmwareType = ItouchPopFileType.Tar.GetDescription();
                                        targetFile = DeviceUpdateTarget.Tar.GetDescription();
                                    }
                                }

                                break;
                            }
                        case (short)DeviceType.ITouch30A:
                            {
                                if (target.Equals(DeviceUpdateTarget.ITouch30A.GetDescription()))
                                {
                                    nameCompare = DeviceType.ITouch30A.GetDescription() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.ITouch30A.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                if (DeviceUpdateTarget.Abcm.GetDescription().ToLower().Equals(target.ToLower()))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.Abcm.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                var extraVersion = !string.IsNullOrEmpty(device.ExtraVersion) ? device.ExtraVersion.Split("_")[0] : string.Empty;
                                if (extraVersion.Equals(target))
                                {
                                    nameCompare = target.Split("-").Last() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.SoundTrack01.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                break;
                            }
                        case (short)DeviceType.DF970:
                            {
                                if (target.Equals(DeviceUpdateTarget.DF970.GetDescription()))
                                {
                                    nameCompare = DeviceType.DF970.GetDescription() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.DF970.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                break;
                            }
                        case (short)DeviceType.Icu500:
                            {
                                if (target.Equals(DeviceUpdateTarget.Icu500.GetDescription()))
                                {
                                    nameCompare = DeviceType.Icu500.GetDescription() + undefinedChar;
                                    if (model.File.Name.Contains(nameCompare))
                                    {
                                        firmwareType = DeviceUpdateTarget.Icu500.GetDescription();
                                        targetFile = target;
                                    }
                                }

                                break;
                            }
                        case (short)DeviceType.T2Face:
                        {
                            if (target.Equals(DeviceUpdateTarget.T2Face.GetDescription()))
                            {
                                nameCompare = DeviceType.T2Face.GetDescription() + undefinedChar;
                                if (model.File.Name.Contains(nameCompare))
                                {
                                    firmwareType = DeviceUpdateTarget.T2Face.GetDescription();
                                    targetFile = target;
                                }
                            }

                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(firmwareType))
                    {
                        // send list file
                        var dataFileTransfer = MakeDataProtocolFileTransfer(model.File, device, targetFile, firmwareType);
                        int total = dataFileTransfer.Count + 1;
                        for (int i = 1; i < total; i++)
                        {
                            var message = new DeviceUploadFileProtocolData
                            {
                                MsgId = Helpers.CreateMsgIdProcess(processId, i, total),
                                Sender = model.Sender,
                                Type = model.MessageType,
                                Data = dataFileTransfer[i - 1],
                            };

                            // _queueService.Publish($"{Constants.RabbitMq.FileTranferTopic}.{device.DeviceAddress}", message.ToString());
                        }

                        // send device instruction
                        SendDeviceInstruction(new InstructionQueueModel()
                        {
                            DeviceId = device.Id,
                            DeviceAddress = device.DeviceAddress,
                            MessageType = Constants.ActionType.UpdateDevice,
                            Command = Constants.CommandType.UpdateFirmware,
                            FwType = firmwareType,
                            Target = target,
                            Sender = model.Sender,
                            MessageIndex = total,
                            MessageTotal = total,
                            MsgId = processId,
                            FileName = model.File.FileName,
                            IsSaveSystemLog = true
                        });
                    }
                    else
                    {
                        var topic = Constants.RabbitMq.NotificationTopic;
                        var account = _unitOfWork.AccountRepository.GetByUserName(model.Sender);
                        if (account == null || account.Type != (short)AccountType.SystemAdmin)
                        {
                            if (device.CompanyId.HasValue)
                            {
                                var company = _unitOfWork.CompanyRepository.GetById(device.CompanyId.Value);
                                topic = $"{Constants.RabbitMq.NotificationTopic}.{company.Code}";
                                if (string.IsNullOrEmpty(model.Language))
                                {
                                    model.Language = Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetLanguage(company.Id).Value);
                                }
                            }
                        }

                        // send message error
                        var notification = new NotificationProtocolDataDetail
                        {
                            MessageType = Constants.MessageType.Error,
                            NotificationType = Constants.NotificationType.FileTransferError,
                            User = model.Sender,
                            Message = string.Format(
                                MessageResource.ResourceManager.GetString("msgFileNameError",
                                    new CultureInfo(model.Language)), nameCompare),
                        };
                        _webSocketService.SendWebSocketToFE(device.DeviceAddress, device.CompanyId ?? 0, notification);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }

        private SDKUpdateDeviceConfigModel MakeDeviceProtocolData(ConfigQueueModel model, IcuDevice device)
        {
            if (device.Company == null && device.CompanyId != null)
            {
                device.Company = _unitOfWork.CompanyRepository.GetById(device.CompanyId.Value);
            }

            var data = _mapper.Map<SDKUpdateDeviceConfigModel>(device);

            if (device.PassageTzId != null)
            {
                var passageTz = _unitOfWork.AccessTimeRepository.GetByIdAndCompany(device.PassageTzId.Value, device.CompanyId ?? 0);
                data.PassageTimezone = passageTz.Position;
            }

            var activeTz = _unitOfWork.AccessTimeRepository.GetByIdAndCompany(device.ActiveTzId, device.CompanyId ?? 0);
            if (activeTz != null)
            {
                data.ActiveTimezone = activeTz.Position;
            }

            if (device.DeviceType == (short)DeviceType.Icu300N
                || device.DeviceType == (short)DeviceType.Icu300NX
                || device.DeviceType == (short)DeviceType.Icu400)
            {
                data.ReaderCount = 2;
                data.ReaderConfig = new List<int>();

                var configReader0 = device.RoleReader0 << 0;
                configReader0 += device.LedReader0 << 1;
                configReader0 += device.BuzzerReader0 << 2;
                data.ReaderConfig.Add(configReader0 ?? 0);

                var configReader1 = device.RoleReader1 << 0;
                configReader1 += device.LedReader1 << 1;
                configReader1 += device.BuzzerReader1 << 2;
                data.ReaderConfig.Add(configReader1 ?? 0);
            }
            else if (device.DeviceType == (short)DeviceType.ITouchPop
                     || device.DeviceType == (short)DeviceType.ITouchPopX
                     || device.DeviceType == (short)DeviceType.DQMiniPlus
                     || device.DeviceType == (short)DeviceType.IT100)
            {
                data.ReaderCount = device.UseCardReader == 0 ? 2 : 1;
                data.ReaderConfig = new List<int>();

                var configReader0 = device.RoleReader0 << 0;
                configReader0 += device.UseCardReader << 1;
                configReader0 += device.BuzzerReader0 << 2;
                data.ReaderConfig.Add(configReader0 ?? 0);

                if (data.ReaderCount == 2)
                {
                    var configReader1 = device.RoleReader1 << 0;
                    configReader1 += device.LedReader1 << 1;
                    configReader1 += device.BuzzerReader1 << 2;
                    data.ReaderConfig.Add(configReader1 ?? 0);
                }
            }
            // else if (device.DeviceType == (short)DeviceType.NexpaLPR)
            // {
            //     var lprData = _mapper.Map<LprProtocolDetailData>(data);
            //     lprData.IsTwoPart = device.TwoPartSystemId != null;
            //     if (lprData.IsTwoPart && device.TwoPartSystemId.HasValue)
            //     {
            //         var twoPartSystem = _unitOfWork.TwoPartSystemRepository.GetById(device.TwoPartSystemId.Value);
            //         lprData.TwoPartTimeFrom = twoPartSystem.TimeFrom;
            //         lprData.TwoPartTimeTo = twoPartSystem.TimeTo;
            //     }
            //
            //     return lprData;
            // }

            data.QrAesKey = Helpers.EncryptSecretCode(data.QrAesKey);
            data.UseStaticQrCode = model.UseStaticQrCode;

            // get all camera
            data.Cameras = new List<SDKCameraDeviceConfig>();
            var cameras = _unitOfWork.CameraRepository.GetCameraByIcuDeviceId(device.Id);
            foreach (var camera in cameras)
            {
                data.Cameras.Add(new SDKCameraDeviceConfig()
                {
                    CameraId = camera.CameraId,
                    RoleReader = camera.RoleReader,
                    SaveEventUnknownFace = camera.SaveEventUnknownFace,
                    Similarity = camera.Similarity,
                    VoiceAlarm = camera.VoiceAlarm,
                    LightAlarm = camera.LightAlarm,
                });
            }
            data.ListCameras = JsonConvert.SerializeObject(data.Cameras);
            // Operation mode
            data.OperationMode = device.OperationType;
            
            // password of device
            data.Password = _unitOfWork.CompanyRepository.GetDevicePasswordByCompany(device.CompanyId.Value);
            
            return data;
        }
        
        private List<DeviceUploadFileDetail> MakeDataProtocolFileTransfer(IFormFile file, IcuDevice device, string target, string fwType)
        {
            List<DeviceUploadFileDetail> data = new List<DeviceUploadFileDetail>();

            if (file.Length > 0)
            {
                var maxSplitSize = Helpers.GetMaxFileSplit(device.DeviceType);
                var dataFile = FileHelpers.SplitFile(file, maxSplitSize);

                if (dataFile.Count > 0)
                {
                    for (var i = 0; i < dataFile.Count; i++)
                    {
                        var fileUploadDetail = new DeviceUploadFileDetail
                        {
                            FrameIndex = i,
                            TotalIndex = dataFile.Count,
                            Extension = FileHelpers.GetFileExtension(file),

                            Target = (target.ToLower().Equals(DeviceUpdateTarget.ITouchPop2A.GetDescription().ToLower()))
                                     || (target.ToLower().Equals(DeviceUpdateTarget.Abcm.GetDescription().ToLower()))
                                ? FileHelpers.GetFileNameWithoutExtension(file) : target,
                            FwType = fwType,
                            FileName = target.ToLower().Equals(DeviceUpdateTarget.Tar.GetDescription().ToLower()) ? "update" : FileHelpers.GetFileNameWithoutExtension(file),
                            Data = dataFile[i]
                        };

                        data.Add(fileUploadDetail);
                    }
                }
            }

            return data;
        }

        private List<InstructionCommonModel> SplitMessageSend(InstructionCommonModel model)
        {
            try
            {
                if (!model.DeviceType.HasValue && model.MessageType == Constants.Protocol.AddUser && model.DeviceIds != null && model.DeviceIds.Any())
                {
                    // init data
                    List<List<IcuDevice>> result = new List<List<IcuDevice>>();
                    List<List<IcuDevice>> readerOnlyForMasters = new List<List<IcuDevice>>();

                    var company = _unitOfWork.CompanyRepository.GetCompanyByCode(model.CompanyCode);

                    // Get buildings that the devices to be receive data are belong.
                    var allBuildings = _unitOfWork.BuildingRepository.GetByCompanyId(company.Id)
                        .Include(m => m.IcuDevice)
                        .Where(m => m.IcuDevice.Any(n => model.DeviceIds.Contains(n.Id)))
                        .ToList();

                    // user is master
                    List<int> userIdsNeedToCheckMaster = new List<int>();
                    List<int> masterUserIds = new List<int>();
                    if (model.UserIds != null && model.UserIds.Any())
                    {
                        userIdsNeedToCheckMaster.AddRange(model.UserIds);
                    }
                    if (model.CardIds != null && model.CardIds.Any())
                    {
                        var userIdsByCardIds = _unitOfWork.CardRepository.GetByIds(company.Id, model.CardIds)
                            .Where(m => m.UserId.HasValue).Select(m => m.UserId.Value).ToList();
                        if (userIdsByCardIds.Any())
                        {
                            userIdsNeedToCheckMaster.AddRange(userIdsByCardIds);
                        }
                    }

                    // split building (master and timezone)
                    List<List<Building>> splitBuilding = new List<List<Building>>();
                    var listBuildingWithoutMaster = allBuildings.GroupBy(m => m.TimeZone).ToList();
                    foreach (var item in listBuildingWithoutMaster)
                    {
                        splitBuilding.Add(item.Select(m => m).ToList());
                    }

                    foreach (var listBuilding in splitBuilding)
                    {
                        // device type
                        var deviceGroups = _unitOfWork.IcuDeviceRepository
                            .GetByIds(model.DeviceIds)
                            .Where(m => m.BuildingId.HasValue && listBuilding.Select(n => n.Id).Contains(m.BuildingId.Value))
                            .GroupBy(m => m.DeviceType).ToList();
                        foreach (var deviceGroupByType in deviceGroups)
                        {
                            // group by verify mode
                            var deviceGroupByVerifyMode = deviceGroupByType.GroupBy(m => m.VerifyMode).ToList();
                            foreach (var deviceGroup in deviceGroupByVerifyMode)
                            {
                                var deviceIds = deviceGroup.Select(m => m.Id).ToList();
                                List<int> accessGroupIds = new List<int>();
                                List<int> userIds = new List<int>();
                                List<int> visitIds = new List<int>();

                                // timezone
                                if (model.UserIds != null && model.UserIds.Any())
                                {
                                    userIds.AddRange(model.UserIds);
                                }
                                if (model.VisitIds != null && model.VisitIds.Any())
                                {
                                    visitIds.AddRange(model.VisitIds);
                                }
                                List<int> cardIds = new List<int>();
                                if (model.CardIds != null && model.CardIds.Any())
                                {
                                    cardIds.AddRange(model.CardIds);
                                }
                                if (model.CardFilterIds != null && model.CardFilterIds.Any())
                                {
                                    cardIds = cardIds.Where(m => model.CardFilterIds.Contains(m)).ToList();
                                }
                                if (model.DeviceType.HasValue)
                                {
                                    var ids = cardIds;
                                    var cards = _unitOfWork.CardRepository.Gets(m => ids.Contains(m.Id)).ToList();
                                    var userIdsByCard = cards.Where(m => m.UserId.HasValue).Select(m => m.UserId.Value).ToList();
                                    if (userIdsByCard.Count > 0)
                                    {
                                        userIds.AddRange(userIdsByCard);
                                    }
                                    var visitIdsByCard = cards.Where(m => m.VisitId.HasValue).Select(m => m.VisitId.Value).ToList();
                                    if (visitIdsByCard.Count > 0)
                                    {
                                        visitIds.AddRange(visitIdsByCard);
                                    }
                                }

                                if (userIds.Count > 0)
                                {
                                    var accessGroupIdsByUser = _unitOfWork.UserRepository.GetByIds(model.UserIds).Select(m => m.AccessGroupId).ToList();
                                    if (accessGroupIdsByUser.Count > 0)
                                    {
                                        accessGroupIds.AddRange(accessGroupIdsByUser);
                                        var accessGroupParent = _unitOfWork.AccessGroupRepository.GetParentsByIds(accessGroupIdsByUser).Select(m => m.Id).ToList();
                                        if (accessGroupParent.Count > 0)
                                        {
                                            accessGroupIds.AddRange(accessGroupParent);
                                        }
                                    }
                                }
                                if (visitIds.Count > 0)
                                {
                                    var accessGroupIdsByVisit = _unitOfWork.VisitRepository.GetByVisitIds(company.Id, model.VisitIds).Select(m => m.AccessGroupId).ToList();
                                    if (accessGroupIdsByVisit.Count > 0)
                                    {
                                        accessGroupIds.AddRange(accessGroupIdsByVisit);
                                    }
                                }

                                if (accessGroupIds.Count > 0)
                                    accessGroupIds = accessGroupIds.Distinct().ToList();

                                var listAccessGroupDevice = _unitOfWork.AccessGroupDeviceRepository
                                    .Gets(m => accessGroupIds.Contains(m.AccessGroupId) && deviceIds.Contains(m.IcuId))
                                    .Include(m => m.Icu)
                                    .ToList()
                                    .GroupBy(m => m.TzId);

                                foreach (var item in listAccessGroupDevice)
                                {
                                    result.Add(item.Select(m => m.Icu).ToList());
                                }
                            }
                        }
                    }

                    List<InstructionCommonModel> resultModel = new List<InstructionCommonModel>();

                    if (result.Count > 0)
                    {
                        foreach (var item in result)
                        {
                            var newModel = model.Clone();
                            newModel.DeviceType = item.First().DeviceType;
                            newModel.DeviceIds = item.Select(m => m.Id).ToList();
                            resultModel.Add(newModel);
                        }
                    }

                    if(readerOnlyForMasters.Count > 0)
                    {
                        resultModel.Add(new InstructionCommonModel()
                        {
                            DeviceType = -1,
                            CompanyCode = "TEMP_DATA"
                        });

                        var cardIds = _unitOfWork.CardRepository.Gets(c => !c.IsDeleted && c.UserId != null && masterUserIds.Contains(c.UserId.Value)).Select(c => c.Id).ToList();

                        foreach (var item in readerOnlyForMasters)
                        {
                            var newModel = model.Clone();
                            newModel.DeviceType = item.First().DeviceType;
                            newModel.DeviceIds = item.Select(m => m.Id).ToList();
                            newModel.UserIds = masterUserIds;
                            newModel.CardIds = cardIds;
                            resultModel.Add(newModel);
                        }
                    }

                    return resultModel;
                }
                else if (!model.DeviceType.HasValue && model.MessageType == Constants.Protocol.DeleteUser && model.DeviceIds != null && model.DeviceIds.Any() && model.UserIds != null && model.UserIds.Count > 0)
                {
                    // 1. ������ ��ȸ
                    // 2. �ǹ� ID ���� ����
                    // 3. �ܸ��⵵ �ǹ� ID ���� ����
                    // 4. �ǹ� ID �� �������� ��Ī
                    // 5. �ǹ� �����ʹ� �ش� �ǹ� �ܸ��⿡ ���� ���� �� ����

                    // init data
                    List<List<IcuDevice>> normalDevices = new List<List<IcuDevice>>();
                    List<List<IcuDevice>> masterDevices = new List<List<IcuDevice>>();
                    List<InstructionCommonModel> resultModel = new List<InstructionCommonModel>();

                    List<int> userIds = model.UserIds;
                    List<int> masterUserIds = new List<int>();
                    // Get company info to use Company's Id value.
                    var company = _unitOfWork.CompanyRepository.GetCompanyByCode(model.CompanyCode);
                    // Check CardIds.
                    if (model.CardIds != null && model.CardIds.Any())
                    {
                        // Get userIds from card data.
                        var userIdsByCardIds = _unitOfWork.CardRepository.GetByIds(company.Id, model.CardIds)
                            .Where(m => m.UserId.HasValue).Select(m => m.UserId.Value).ToList();
                        if (userIdsByCardIds.Any())
                            userIds.AddRange(userIdsByCardIds);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }

            return null;
        }
    }
}