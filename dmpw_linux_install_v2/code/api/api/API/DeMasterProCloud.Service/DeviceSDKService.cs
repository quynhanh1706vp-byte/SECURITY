using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.DeviceSDK;
using DeMasterProCloud.DataModel.EventLog;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Service.RabbitMqQueue;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeMasterProCloud.Service;

public interface IDeviceSDKService
{
    public SDKTokenModel Login();
    public SDKTokenModel RefreshToken();
    public List<SDKDeviceListModel> GetAllDevices(bool recall = true);
    public SDKDevicePagingListModel GetDevices(int pageNumber, int pageSize, bool recall = true);
    public SDKDeviceInfoModel GetDeviceInfo(string deviceAddress, bool recall = true);
    public bool SetTime(string deviceAddress, string timezone, bool recall = true);
    public bool SetAccessTime(string deviceAddress, List<SDKAccessTimeDetailModel> listTimezones, bool recall = true);
    public bool SetHoliday(string deviceAddress, List<SDKHolidayDetailModel> holidays, int total, bool recall = true);
    public bool DeviceInstruction(string deviceAddress, string command, bool recall = true);
    public bool OpenDoor(string deviceAddress, SDKOpenDoorModel model, bool recall = true);
    public void AddCard(string deviceAddress, List<SDKCardModel> cards, bool recall = true);
    public void DeleteCard(string deviceAddress, List<SDKCardModel> cards, bool recall = true);
    public Task AddCardAsync(string deviceAddress, List<SDKCardModel> cards, bool recall = true);
    public Task DeleteCardAsync(string deviceAddress, List<SDKCardModel> cards, bool recall = true);
    public bool UpdateDeviceConfig(SDKUpdateDeviceConfigModel model, bool recall = true);
    public bool LoadTimeZoneDevice(string deviceAddress, List<int> listPositions, bool recall = true);
    public bool GetUserInfor(string deviceAddress, string cardId, bool recall = true);
    public bool SubscribeWebhook(bool recall = true);
    public void ReceiveDataWebhook(SDKDataWebhookModel model);
}

public class DeviceSDKService : IDeviceSDKService
{
    private readonly SDKSettingModel _sdkConfig;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public DeviceSDKService(IConfiguration configuration)
    {
        _sdkConfig = configuration.GetSection(Constants.SDKDevice.DefineConfig).Get<SDKSettingModel>();
        _mapper = MapperInstance.Mapper;
        _configuration = configuration;
        _logger = ApplicationVariables.LoggerFactory.CreateLogger<DeviceSDKService>();
    }

    public SDKTokenModel Login()
    {
        try
        {
            SDKLoginModel req = _mapper.Map<SDKLoginModel>(_sdkConfig);
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiLogin}";
            var res = Helpers.PostJson(url, req);
            var data = JsonConvert.DeserializeObject<SDKResponseModel<SDKTokenModel>>(res.ToString())?.Data;
            if (data != null)
            {
                ApplicationVariables.SDKToken = data.AuthToken;
                ApplicationVariables.SDKRefreshToken = data.RefreshToken;
                ApplicationVariables.SDKUserName = data.FullName;
            }
            return data;  
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return null;
        }
    }

    public SDKTokenModel RefreshToken()
    {
        try
        {
            string token = ApplicationVariables.SDKToken;
            string refreshToken = ApplicationVariables.SDKRefreshToken;
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiRefreshToken}?refreshToken={refreshToken}&expiredToken={token}";
            var res = Helpers.PostJson(url);
            var data = JsonConvert.DeserializeObject<SDKResponseModel<SDKTokenModel>>(res.ToString())?.Data;
            if (data != null)
            {
                ApplicationVariables.SDKToken = data.AuthToken;
                ApplicationVariables.SDKRefreshToken = data.RefreshToken;
                ApplicationVariables.SDKUserName = data.FullName;
            }
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return null;
        }
    }

    public List<SDKDeviceListModel> GetAllDevices(bool recall = true)
    {
        List<SDKDeviceListModel> data = new List<SDKDeviceListModel>();
        try
        {
            int pageNumber = 1;
            int pageSize = Constants.SDKDevice.MaxPageSize;
            int total = 0;
            do
            {
                var dataTemp = GetDevices(pageNumber, pageSize, recall);
                if (dataTemp.Total <= 0)
                {
                    break;
                }
                else
                {
                    data.AddRange(dataTemp.Data);
                    total = dataTemp.Total;
                    pageNumber += 1;
                }
            } while (data.Count < total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            data = new List<SDKDeviceListModel>();
        }
        return data;
    }
    
    public SDKDevicePagingListModel GetDevices(int pageNumber, int pageSize, bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiDeviceInSubnet}";
            string token = ApplicationVariables.SDKToken;
            var res = Helpers.PostJson(url, new
            {
                pageNumber = pageNumber,
                pageSize = pageSize,
            }, token);
            if (recall && CheckAndRefreshToken(res))
            {
                return GetDevices(pageNumber, pageSize, false);
            }

            return JsonConvert.DeserializeObject<SDKResponseModel<SDKDevicePagingListModel>>(res.ToString())?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return new SDKDevicePagingListModel();
        }
    }

    public SDKDeviceInfoModel GetDeviceInfo(string deviceAddress, bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiGetDeviceInfo}";
            string token = ApplicationVariables.SDKToken;
            var res = Helpers.PostJson(url, new
            {
                deviceAddress = deviceAddress,
            }, token);
            if (recall && CheckAndRefreshToken(res))
            {
                return GetDeviceInfo(deviceAddress, false);
            }

            return JsonConvert.DeserializeObject<SDKResponseModel<SDKDeviceInfoModel>>(res.ToString())?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return null;
        }
    }

    public bool SetTime(string deviceAddress, string timezone, bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiSetCurrentTime}";
            string token = ApplicationVariables.SDKToken;
            var res = Helpers.PostJson(url, new
            {
                deviceAddress = deviceAddress,
                timezone = timezone,
            }, token);
            if (recall && CheckAndRefreshToken(res))
            {
                return SetTime(deviceAddress, timezone, false);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return false;
        }
    }

    public bool SetAccessTime(string deviceAddress, List<SDKAccessTimeDetailModel> listTimezones, bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiUpdateTimeZone}";
            string token = ApplicationVariables.SDKToken;
            var res = Helpers.PostJson(url, new
            {
                deviceAddress = deviceAddress,
                listTimezones = listTimezones,
            }, token);
            if (recall && CheckAndRefreshToken(res))
            {
                return SetAccessTime(deviceAddress, listTimezones, false);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return false;
        }
    }

    public bool SetHoliday(string deviceAddress, List<SDKHolidayDetailModel> holidays, int total, bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiUpdateHoliday}";
            string token = ApplicationVariables.SDKToken;
            var res = Helpers.PostJson(url, new
            {
                deviceAddress = deviceAddress,
                holidays = holidays,
                total = total,
            }, token);
            if (recall && CheckAndRefreshToken(res))
            {
                return SetHoliday(deviceAddress, holidays, total, false);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return false;
        }
    }

    public bool DeviceInstruction(string deviceAddress, string command, bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiDeviceInstruction}";
            string token = ApplicationVariables.SDKToken;
            var res = Helpers.PostJson(url, new
            {
                deviceAddress = deviceAddress,
                command = command,
            }, token);
            if (recall && CheckAndRefreshToken(res))
            {
                return DeviceInstruction(deviceAddress, command, false);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return false;
        }
    }

    public bool OpenDoor(string deviceAddress, SDKOpenDoorModel model, bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiOpenDoor}";
            string token = ApplicationVariables.SDKToken;
            var res = Helpers.PostJson(url, new
            {
                deviceAddress = deviceAddress,
                openPeriod = model.OpenPeriod,
                openUntilTime = model.OpenUntilTime,
            }, token);
            if (recall && CheckAndRefreshToken(res))
            {
                return OpenDoor(deviceAddress, model, false);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return false;
        }
    }

    public void AddCard(string deviceAddress, List<SDKCardModel> cards, bool recall = true)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiAddCard}";
                string token = ApplicationVariables.SDKToken;

                var payload = new { deviceAddress, cards };
                var res = await Helpers.PostJsonAsync(url, payload, token);

                if (CheckAndRefreshToken(res))
                {
                    AddCard(deviceAddress, cards);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fire-and-forget DeleteCard");
            }
        });
    }

    public void DeleteCard(string deviceAddress, List<SDKCardModel> cards, bool recall = true)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiDeleteCard}";
                string token = ApplicationVariables.SDKToken;

                var payload = new { deviceAddress, cards };
                var res = await Helpers.PostJsonAsync(url, payload, token);

                if (CheckAndRefreshToken(res))
                {
                    DeleteCard(deviceAddress, cards);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fire-and-forget DeleteCard");
            }
        });
    }
    
    public async Task AddCardAsync(string deviceAddress, List<SDKCardModel> cards, bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiAddCard}";
            string token = ApplicationVariables.SDKToken;

            var payload = new { deviceAddress, cards };
            var res = await Helpers.PostJsonAsync(url, payload, token);

            if (CheckAndRefreshToken(res) && recall)
            {
                await AddCardAsync(deviceAddress, cards, false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AddCardAsync");
            throw;
        }
    }

    public async Task DeleteCardAsync(string deviceAddress, List<SDKCardModel> cards, bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiDeleteCard}";
            string token = ApplicationVariables.SDKToken;

            var payload = new { deviceAddress, cards };
            var res = await Helpers.PostJsonAsync(url, payload, token);

            if (CheckAndRefreshToken(res) && recall)
            {
                await DeleteCardAsync(deviceAddress, cards, false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteCardAsync");
            throw;
        }
    }

    public bool UpdateDeviceConfig(SDKUpdateDeviceConfigModel model, bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiUpdateDeviceConfig}";
            string token = ApplicationVariables.SDKToken;
            var res = Helpers.PostJson(url, model, token);
            if (recall && CheckAndRefreshToken(res))
            {
                return UpdateDeviceConfig(model, false);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return false;
        }
    }
    public bool LoadTimeZoneDevice(string deviceAddress, List<int> listPositions, bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiDeviceLoadTimeZone}";
            string token = ApplicationVariables.SDKToken;
            var res = Helpers.PostJson(url, new
            {
                deviceAddress = deviceAddress,
                listPositions = listPositions
            }, token);
            if (recall && CheckAndRefreshToken(res))
            {
                return LoadTimeZoneDevice(deviceAddress, listPositions, false);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return false;
        }
    }

    public bool GetUserInfor(string deviceAddress, string cardId, bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiDeviceLoadUserInfo}";
            string token = ApplicationVariables.SDKToken;
            var res = Helpers.PostJson(url, new
            {
                deviceAddress = deviceAddress,
                cardId = cardId
            }, token);
            if (recall && CheckAndRefreshToken(res))
            {
                return GetUserInfor(deviceAddress, cardId, false);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return false;
        }
    }

    public bool SubscribeWebhook(bool recall = true)
    {
        try
        {
            string url = $"{_sdkConfig.Host}{Constants.SDKDevice.ApiDeviceReceiveEventLog}";
            string token = ApplicationVariables.SDKToken;
            string hostApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Value;
            string receiveEventLogUrl = $"{hostApi}{Constants.Route.ApiWebhookReceiveEventLog}";
            var res = Helpers.PostJson(url, new
            {
                url = receiveEventLogUrl,
            }, token);
            if (recall && CheckAndRefreshToken(res))
            {
                return SubscribeWebhook(false);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
            return false;
        }
    }

    public void ReceiveDataWebhook(SDKDataWebhookModel model)
    {
        try
        {
            switch (model.Type)
            {
                case Constants.SDKDevice.WebhookDeviceConnectionType:
                    WebhookDeviceConnection(JsonConvert.DeserializeObject<SDKDeviceConnectionModel>(Helpers.JsonConvertCamelCase(model.Data)));
                    break;
                case Constants.SDKDevice.WebhookEventLogType:
                    WebhookEventLog(JsonConvert.DeserializeObject<SDKEventLogHeaderData>(Helpers.JsonConvertCamelCase(model.Data)));
                    break;
                case Constants.SDKDevice.WebhookDoorStatusType:
                    WebhookDoorStatus(JsonConvert.DeserializeObject<SDKDoorStatusModel>(Helpers.JsonConvertCamelCase(model.Data)));
                    break;
                case Constants.SDKDevice.WebhookDeviceRequestType:
                    WebhookDeviceRequest(JsonConvert.DeserializeObject<SDKDeviceRequestModel>(Helpers.JsonConvertCamelCase(model.Data)));
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void WebhookDeviceConnection(SDKDeviceConnectionModel model)
    {
        IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
        try
        {
            // check device type camera
            if (model.DeviceType.ToUpper().Contains("CAMERA"))
            {
                Camera camera = unitOfWork.CameraRepository.GetByCameraId(model.DeviceAddress);
                if(camera != null)
                {
                    unitOfWork.CameraRepository.ChangedConnectionStatus(camera, (short)model.Status);
                }
                else
                {
                    AddOrUpdateUnregisteredDevice(unitOfWork, _mapper, model, model.MacAddress, (int)Registertype.NewDevice);
                }
            }
            
            // check device
            var deviceAddress = model.DeviceAddress;
            var device = unitOfWork.IcuDeviceRepository.GetDeviceByAddress(deviceAddress, false);
            var lastCommunicationTime = device?.LastCommunicationTime ?? DateTime.MinValue;
            // Check mac address
            var macAddress = model.MacAddress;
            var deviceByMac = unitOfWork.IcuDeviceRepository.GetDeviceByMacAddress(macAddress);
            var status = model.Status;
            
            if (deviceByMac == null)
            {
                _logger.LogInformation($"[{deviceAddress}] This is a new Mac address. Mac address is " + macAddress);
                
                // Check validation
                // if deviceAddress is FFFFFF, this means installer(or customer) didn't change device's default RID info.
                if (status == (short)ConnectionStatus.Offline || deviceAddress.Equals(Constants.DefaultDeviceAddress))
                {
                    if (status == (short)ConnectionStatus.Offline)
                    {
                        if (device != null)
                        {
                            IDeviceService deviceService = new DeviceService(unitOfWork);
                            deviceService.ChangedConnectionStatus(device, (short)status);
                        }

                        _logger.LogInformation("This message is DISCONNECTED (status = 0) message. So, we don't store to 'UnregisteredDevice' table in DB. Device address is " + deviceAddress);
                        unitOfWork.Dispose();
                        return;
                    }

                    _logger.LogInformation("This device has wrong RID or disconnected. So, we don't store to 'UnregisteredDevice' table in DB. Device address is " + deviceAddress);
                    unitOfWork.Dispose();
                    return;
                }

                // There isn't a device that has same Mac address in database.
                if (device == null)
                {
                    // There isn't a device that has same RID in database.
                    // [ New Device ] - This device is a new device in this system.
                    _logger.LogInformation("This is a new device. Device address is " + deviceAddress);
                    
                    AddOrUpdateUnregisteredDevice(unitOfWork, _mapper, model, macAddress, (int)Registertype.NewDevice);
                    unitOfWork.Dispose();
                    return;
                }
                else
                {
                    // There is a device that has same RID in database.
                    // [ Replace Device ] - This device is a new device to replace existing old device.
                    _logger.LogInformation($"[{deviceAddress}] This is a replace device. We should reinstall in this case.");
                    AddOrUpdateUnregisteredDevice(unitOfWork, _mapper, model, macAddress, (int)Registertype.Replace);

                    // Connection status should be updated about this IcuDevice.
                    IDeviceService deviceService = new DeviceService(unitOfWork);
                    deviceService.ChangedConnectionStatus(device, (short)status);
                    unitOfWork.Dispose();
                    return;
                }
            }
            else
            {
                // There is a device that has same Mac address in database.
                if (device == null)
                {
                    // There isn't a device that has same RID in database.
                    // [ Relocation Device ] - This device is an existing device but installed elsewhere. (with new RID)
                    _logger.LogInformation($"[{deviceAddress}] This is a relocation device. We should reinstall in this case.");
                    _logger.LogInformation($"[{deviceAddress}] Device is null.");
                        
                    AddOrUpdateUnregisteredDevice(unitOfWork, _mapper, model, macAddress, (int)Registertype.Relocation);
                    unitOfWork.Dispose();
                    return;
                }
                else
                {
                    _logger.LogInformation("Device's MacAddress : " + device.MacAddress + ", Message's MacAddress : " + macAddress + ", Device's =? Message's => " + device.MacAddress.Equals(macAddress));
                    _logger.LogInformation("Device's DeviceAddress : " + device.DeviceAddress + ", Message's DeviceAddress : " + deviceAddress + ", Device's =? Message's => " + device.DeviceAddress.Equals(deviceAddress));

                    if ((device.MacAddress != macAddress) || (deviceByMac.DeviceAddress != deviceAddress))
                    {
                        // [ Relocation Device ] - This device is an existing device but RID was changed.
                        _logger.LogInformation($"[{deviceAddress}] This is a relocation device. We should reinstall in this case.");
                            
                        AddOrUpdateUnregisteredDevice(unitOfWork, _mapper, model, macAddress, (int)Registertype.Relocation);
                        unitOfWork.Dispose();
                        return;
                    }
                    
                    if (((device.MacAddress == macAddress) && (deviceByMac.DeviceAddress == deviceAddress)) || status == (short)ConnectionStatus.Offline)
                    {
                        // There is a device that has same RID in database.
                        // We should check status.
                        // * Because of there isn't MAC address in disconnect message.
                        
                        // update device's information
                        IDeviceService deviceService = new DeviceService(unitOfWork, _configuration);
                        deviceService.ChangedConnectionStatus(device, (short) status);
                        
                        // If device online after greater MessageTTLInDevice offline. We will transmit all data user info
                        if(device.CompanyId.HasValue && status == (short)ConnectionStatus.Online && lastCommunicationTime.AddMilliseconds(Constants.RabbitMq.MessageInDeviceTLL) < DateTime.UtcNow)
                        {
                            var newDataOfDevice = unitOfWork.IcuDeviceRepository.GetDeviceAllInfoByCompany(device.CompanyId.Value).FirstOrDefault(m => m.Id == device.Id);
                            if (newDataOfDevice != null && newDataOfDevice.Company.AutoSyncUserData)
                            {
                                new Thread(() =>
                                {
                                    Thread.Sleep(Constants.RabbitMq.DelayTimeReInstallWhenOnline);
                                    IUnitOfWork unitOfWorkThread = DbHelper.CreateUnitOfWork(ApplicationVariables.Configuration);
                                    IDeviceService deviceServiceThread = new DeviceService(unitOfWorkThread, ApplicationVariables.Configuration);
                                    try
                                    {
                                        deviceServiceThread.TransmitData(new TransmitDataModel()
                                        {
                                            IsDeleteAllUser = true,
                                            Devices = new List<ReinstallDeviceDetail>()
                                            {
                                                new ReinstallDeviceDetail()
                                                {
                                                    DeviceId = newDataOfDevice.Id,
                                                    ProcessId = Guid.NewGuid().ToString(),
                                                }
                                            },
                                            TransmitIds = new List<int>()
                                            {
                                                (int)TransmitType.CurrentTime,
                                                (int)TransmitType.DeviceSetting,
                                                (int)TransmitType.TimezoneSetting,
                                                (int)TransmitType.HolidaySetting,
                                                (int)TransmitType.UserInfo
                                            },
                                        }, new List<IcuDevice>() { newDataOfDevice });
                                    }
                                    catch (Exception exThreadSyncAllData)
                                    {
                                        Console.WriteLine(exThreadSyncAllData);
                                    }
                                    finally
                                    {
                                        unitOfWorkThread.Dispose();
                                    }
                                }).Start();
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
        }
        finally
        {
            unitOfWork.Dispose();
        }
    }
    
    private void WebhookEventLog(SDKEventLogHeaderData model)
    {
        IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(ApplicationVariables.Configuration);
        try
        {
            var device = unitOfWork.IcuDeviceRepository.GetDeviceByAddress(model.Events[0]?.DeviceAddress);
            if (device == null)
                throw new Exception($"Device not found by RID ={model.Events[0]?.DeviceAddress}");
            
            var building = unitOfWork.BuildingRepository.GetById(device.BuildingId ?? 0) ?? new Building();
            device.Building = building;
            
            // save event log
            var eventLogResult = WebhookAddEventLog(unitOfWork, model.Events[0], device);

            // send to fe
            if (eventLogResult != null)
                WebhookSendEventLogToFe(eventLogResult, device);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
        }
        finally
        {
            unitOfWork.Dispose();
        }
    }
    private void WebhookDoorStatus(SDKDoorStatusModel model)
    {
        IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(ApplicationVariables.Configuration);
        try
        {
            var icuDevice = unitOfWork.IcuDeviceRepository.GetDeviceByAddress(model.DeviceAddress);
            if (icuDevice == null)
                throw new Exception($"Device not found by RID = {model.DeviceAddress}");
            
            // save door status
            icuDevice.ConnectionStatus = (short)IcuStatus.Connected;
            icuDevice.LastCommunicationTime = DateTime.Now;

            if (!string.IsNullOrEmpty(model.DoorState.ToString()))
            {
                switch (model.DoorState)
                {
                    case (int)DoorStatus.ClosedAndLock:
                        icuDevice.DoorStatus = DoorStatus.ClosedAndLock.GetDescription();
                        break;

                    case (int)DoorStatus.ClosedAndUnlocked:
                        icuDevice.DoorStatus = DoorStatus.ClosedAndUnlocked.GetDescription();
                        break;

                    case (int)DoorStatus.Opened:
                        icuDevice.DoorStatus = DoorStatus.Opened.GetDescription();
                        break;

                    case (int)DoorStatus.ForceOpened:
                        icuDevice.DoorStatus = DoorStatus.ForceOpened.GetDescription();
                        break;

                    case (int)DoorStatus.PassageOpened:
                        icuDevice.DoorStatus = DoorStatus.PassageOpened.GetDescription();
                        break;

                    case (int)DoorStatus.EmergencyOpened:
                        icuDevice.DoorStatus = DoorStatus.EmergencyOpened.GetDescription();
                        break;

                    case (int)DoorStatus.EmergencyClosed:
                        icuDevice.DoorStatus = DoorStatus.EmergencyClosed.GetDescription();
                        break;

                    case (int)DoorStatus.Unknown:
                        icuDevice.DoorStatus = DoorStatus.Unknown.GetDescription();
                        break;

                    case (int)DoorStatus.Lock:
                        icuDevice.DoorStatus = DoorStatus.Lock.GetDescription();
                        break;

                    case (int)DoorStatus.Unlock:
                        icuDevice.DoorStatus = DoorStatus.Unlock.GetDescription();
                        break;
                    default:
                        icuDevice.DoorStatus = model.DoorState.ToString();
                        break;
                }
            }
            unitOfWork.IcuDeviceRepository.Update(icuDevice);
            unitOfWork.Save();

            ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
            {
                Type = Constants.SDKDevice.WebhookDoorStatusType,
                Data = model
            }), icuDevice.CompanyId ?? 0);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
        }
        finally
        {
            unitOfWork.Dispose();
        }
    }
    private void WebhookDeviceRequest(SDKDeviceRequestModel model)
    {
        IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(ApplicationVariables.Configuration);
        try
        {
            var icuDevice = unitOfWork.IcuDeviceRepository.GetDeviceByAddress(model.DeviceAddress);
            if (icuDevice == null)
                throw new Exception($"Device not found by RID = {model.DeviceAddress}");

            IDeviceService deviceService = new DeviceService(unitOfWork, _configuration);
            deviceService.SendDeviceRequest(Guid.NewGuid().ToString(), model.RequestType, icuDevice);

            



        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
        }
        finally
        {
            unitOfWork.Dispose();
        }
    }
    
    private EventLog WebhookAddEventLog(IUnitOfWork unitOfWork, SDKEventLogModel eventLogDetail, IcuDevice icuDevice)
    {
        EventLog eventLogResult = null;
        
        try
        {
            //Get user from cloud database by card id
            User user = null;
            Visit visit = null;
            Card card = null;
            Vehicle vehicle = null;
            short cardStatus = 0;
            string eventUserName = "";
            
            if (!string.IsNullOrEmpty(eventLogDetail.CardId))
            {
                switch (eventLogDetail.IdType)
                {
                    case (int)CardType.VehicleId:
                    case (int)CardType.VehicleMotoBikeId:
                    {
                        vehicle = unitOfWork.VehicleRepository.GetByPlateNumber(eventLogDetail.CardId, icuDevice.CompanyId.Value);
                        if (vehicle != null)
                        {
                            user = unitOfWork.UserRepository.GetByPlateNumberIncludeDepartment(icuDevice.CompanyId, eventLogDetail.CardId);
                            visit = unitOfWork.VisitRepository.GetByPlateNumber(icuDevice.CompanyId, eventLogDetail.CardId);
                        }
                        break;
                    }
                    case (int)CardType.NFC:
                    case (int)CardType.PassCode:
                    case (int)CardType.FaceId:
                    case (int)CardType.HFaceId:
                    case (int)CardType.QrCode:
                    case (int)CardType.NFCPhone:
                    case (int)CardType.Vein:
                    case (int)CardType.FingerPrint:
                    case (int)CardType.LFaceId:
                    case (int)CardType.EbknFaceId:
                    case (int)CardType.EbknFingerprint:
                    case (int)CardType.AratekFingerPrint:
                    case (int)CardType.VNID:
                    case (int)CardType.TBFace:
                    {
                        card = unitOfWork.CardRepository.GetByCardId(icuDevice.CompanyId, eventLogDetail.CardId);
                        if (card != null)
                        {
                            user = unitOfWork.UserRepository.GetByCardIdIncludeDepartment(icuDevice.CompanyId, eventLogDetail.CardId);
                            visit = unitOfWork.VisitRepository.GetByCardId(icuDevice.CompanyId, eventLogDetail.CardId);
                            cardStatus = card.Status;
                        }
                        else
                        {
                            // check suprema device (have NFC but not fingerprint)
                            if (eventLogDetail.IdType == (int)CardType.NFC &&
                                (icuDevice.DeviceType == (int)DeviceType.Biostation2 || icuDevice.DeviceType == (int)DeviceType.Biostation3)
                                && int.TryParse(eventLogDetail.CardId, out int userIdOfSuprema))
                            {
                                user = unitOfWork.UserRepository.GetByIdIncludeDepartment(icuDevice.CompanyId, userIdOfSuprema);
                            }
                        }
                        break;
                    }
                    case (int)CardType.BioFaceId:
                        if (int.TryParse(eventLogDetail.CardId, out int userId))
                        {
                            var userById = unitOfWork.UserRepository.GetByIdIncludeDepartment(icuDevice.CompanyId, userId);
                            if (userById != null)
                            {
                                user = userById;
                                card = unitOfWork.CardRepository.GetByUserId(icuDevice.CompanyId ?? 0, userId).FirstOrDefault(x => x.CardType == (int)CardType.BioFaceId);
                                cardStatus = card.Status;
                                eventLogDetail.CardId = card != null ? card.CardId : eventLogDetail.CardId;
                                eventLogDetail.InOut = icuDevice.RoleReader0 == 0 ? Constants.Attendance.In : Constants.Attendance.Out;
                            }
                        }
                        break;
                }
            }

            var eventTime = DateTime.ParseExact(eventLogDetail.AccessTime, Constants.DateTimeFormat.DdMMyyyyHHmmss, null).ConvertToSystemTime(icuDevice.Building.TimeZone);
            if (eventLogDetail.Temperature > 0) // update temperature
            {
                var eventLogUpdateTemperature = unitOfWork.AppDbContext.EventLog.FirstOrDefault(
                    m => m.IcuId == icuDevice.Id
                              && m.CardId == eventLogDetail.CardId
                              && m.EventType == eventLogDetail.EventType
                              && m.EventTime == eventTime);
                if (eventLogUpdateTemperature != null && (eventLogDetail.Temperature - eventLogUpdateTemperature.BodyTemperature) > 0)
                {
                    eventLogUpdateTemperature.BodyTemperature = eventLogDetail.Temperature;
                    unitOfWork.EventLogRepository.Update(eventLogUpdateTemperature);
                    unitOfWork.Save();
                    return null;
                }
            }

            var eventLog = unitOfWork.EventLogRepository.GetUniqueEventLog(icuDevice.CompanyId ?? 0, icuDevice.Id, eventTime, eventLogDetail.CardId);
            if (eventLog == null)
            {
                _logger.LogWarning($"[ADD-EVENT-LOGS]: UserID = {user?.Id}, VisitID = {visit?.Id}");
                var newEventLog = new EventLog();
                newEventLog.Icu = icuDevice;
                newEventLog.IcuId = icuDevice.Id;
                newEventLog.DoorName = icuDevice.Name;
                newEventLog.CompanyId = icuDevice.CompanyId;
                newEventLog.CardId = eventLogDetail.CardId;
                newEventLog.IssueCount = eventLogDetail.IssueCount;
                newEventLog.Antipass = eventLogDetail.InOut;
                newEventLog.EventType = eventLogDetail.EventType;
                newEventLog.EventTime = eventTime;
                newEventLog.CardType = (short)eventLogDetail.IdType;
                newEventLog.CardStatus = cardStatus;
                newEventLog.BodyTemperature = eventLogDetail.Temperature;
                newEventLog.Distance = eventLogDetail.Distance;
                newEventLog.SearchScore = eventLogDetail.SearchScore;
                newEventLog.LivenessScore = eventLogDetail.LivenessScore;
                
                if (visit != null)
                {
                    newEventLog.UserName = visit?.VisitorName;
                    newEventLog.DeptId = 0;
                    newEventLog.IsVisit = true;
                    newEventLog.VisitId = visit?.Id;
                }
                else
                {
                    newEventLog.UserId = user?.Id;
                    newEventLog.UserName = user == null ? eventUserName : user.FirstName + user.LastName;
                    newEventLog.DeptId = user?.DepartmentId;
                    newEventLog.IsVisit = false;
                }
                // update other cardId with case multi verify mode
                newEventLog.OtherCardId = string.IsNullOrEmpty(eventLogDetail.OtherCardId) ? null : eventLogDetail.OtherCardId;

                // check duplicate EventLog have same cardId and type is HFaceID (or event type unknown face)
                if (newEventLog.CardType == (short)CardType.HFaceId && newEventLog.EventType == (short)EventType.UnknownPerson)
                {
                    DateTime minTimeWebhook = eventTime.AddSeconds(-Constants.HanetApiCamera.TimeoutWebhook);
                    DateTime maxTimeWebhook = eventTime.AddSeconds(Constants.HanetApiCamera.TimeoutWebhook);
                    if (unitOfWork.AppDbContext.EventLog.Any(m => m.IcuId == icuDevice.Id 
                       && (m.EventType == (short)EventType.UnknownPerson && (m.CardId == eventLogDetail.CardId && m.CardType == (short)CardType.HFaceId))
                       && minTimeWebhook <= m.EventTime && m.EventTime <= maxTimeWebhook))
                    {
                        return null;
                    }
                }

                unitOfWork.EventLogRepository.Add(newEventLog);
                unitOfWork.Save();
                eventLogResult = newEventLog;
                
            }
            else
            {
                _logger.LogWarning($"[UPDATE-EVENT-LOGS]: UserID = {user?.Id}, VisitID = {visit?.Id}");
                eventLog.Icu = icuDevice;
                eventLog.IcuId = icuDevice.Id;
                eventLog.DoorName = icuDevice.Name;
                eventLog.CompanyId = icuDevice.CompanyId;
                eventLog.CardId = eventLogDetail.CardId;
                eventLog.IssueCount = eventLogDetail.IssueCount;
                eventLog.Antipass = eventLogDetail.InOut;
                eventLog.EventType = eventLogDetail.EventType;
                eventLog.EventTime = eventTime;
                eventLog.CardStatus = cardStatus;
                eventLog.BodyTemperature = eventLogDetail.Temperature;
                eventLog.Distance = eventLogDetail.Distance;
                eventLog.SearchScore = eventLogDetail.SearchScore;
                eventLog.LivenessScore = eventLogDetail.LivenessScore;
                eventLog.CardType = (short)eventLogDetail.IdType;
                
                if (visit != null)
                {
                    eventLog.UserName = visit?.VisitorName;
                    eventLog.DeptId = 0;
                    eventLog.IsVisit = true;
                    eventLog.VisitId = visit?.Id;
                }
                else
                {
                    eventLog.UserId = user?.Id;
                    eventLog.UserName = user == null ? eventUserName : user.FirstName + user.LastName;
                    eventLog.DeptId = user?.DepartmentId;
                    eventLog.IsVisit = false;
                }
                // update other cardId with case multi verify mode
                eventLog.OtherCardId = string.IsNullOrEmpty(eventLogDetail.OtherCardId) ? null : eventLogDetail.OtherCardId;

                // check duplicate EventLog have same cardId and type is HFaceID (or event type unknown face)
                if (eventLog.CardType == (short)CardType.HFaceId && eventLog.EventType == (short)EventType.UnknownPerson)
                {
                    DateTime minTimeWebhook = eventTime.AddSeconds(-Constants.HanetApiCamera.TimeoutWebhook);
                    DateTime maxTimeWebhook = eventTime.AddSeconds(Constants.HanetApiCamera.TimeoutWebhook);
                    if (unitOfWork.AppDbContext.EventLog.Any(m => m.IcuId == icuDevice.Id 
                       && (m.EventType == (short)EventType.UnknownPerson && (m.CardId == eventLogDetail.CardId && m.CardType == (short)CardType.HFaceId))
                       && minTimeWebhook <= m.EventTime && m.EventTime <= maxTimeWebhook))
                    {
                        return null;
                    }
                }
                unitOfWork.EventLogRepository.Update(eventLog);
                unitOfWork.Save();
                eventLogResult = eventLog;
            }
            
            eventLogResult.Visit = visit;
            eventLogResult.User = user;
        }
        catch (Exception ex)
        {
            if (ex is DbUpdateException)
            {
                _logger.LogWarning("[EVENT-EXCEPTION] This is duplicated event. Server cannot store this event.");
                _logger.LogWarning("[EVENT-EXCEPTION] Device Address : " + eventLogDetail.DeviceAddress);
                _logger.LogWarning("[EVENT-EXCEPTION] Access Time    : " + eventLogDetail.AccessTime);
                _logger.LogWarning("[EVENT-EXCEPTION] Card Id        : " + eventLogDetail.CardId);
                _logger.LogWarning("[EVENT-EXCEPTION] Event Type     : " + eventLogDetail.EventType);
            }
            else if (ex is FormatException)
            {
                _logger.LogWarning("[EVENT-EXCEPTION] This is wrong format event. Server cannot store this event.");
                _logger.LogWarning("[EVENT-EXCEPTION] Event Log Message : " + JsonConvert.SerializeObject(eventLogDetail, Formatting.Indented));                  }
            else
            {
                _logger.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if(ex.InnerException != null)
                {
                    _logger.LogError($"{ex.InnerException?.Message}");
                }
                
                _logger.LogError("[EVENT-EXCEPTION] Device Address : " + eventLogDetail.DeviceAddress);
                _logger.LogError("[EVENT-EXCEPTION] Access Time    : " + eventLogDetail.AccessTime);
                _logger.LogError("[EVENT-EXCEPTION] Card Id        : " + eventLogDetail.CardId);
                _logger.LogError("[EVENT-EXCEPTION] Event Type     : " + eventLogDetail.EventType);
            }

            // Clear changes.
            unitOfWork.Clear();
        }
    
        return eventLogResult;
    }

    private void WebhookSendEventLogToFe(EventLog eventLog, IcuDevice device)
    {
        try
        {
            EventLogDetailModel data = _mapper.Map<EventLogDetailModel>(eventLog);
            
            
            ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
            {
                Type = Constants.SDKDevice.WebhookEventLogType,
                Data = data
            }), device.CompanyId ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
        }
    }
    
    private static void AddOrUpdateUnregisteredDevice(IUnitOfWork unitOfWork, IMapper mapper, SDKDeviceConnectionModel deviceInfo, string macAddress, int registerType)
    {
        var logger = ApplicationVariables.LoggerFactory.CreateLogger<DeviceSDKService>();
        var unregisteredDeviceByMac = unitOfWork.UnregistedDevicesRepository.GetByMacAddress(macAddress);
        if (unregisteredDeviceByMac == null)
        {
            // DB doesn't have an unregistered device with the same Mac address.
            // => New Device
            var unregisteredDevice = mapper.Map<UnregistedDevice>(deviceInfo);
            unregisteredDevice.RegisterType = registerType;
            try
            {
                unitOfWork.UnregistedDevicesRepository.Add(unregisteredDevice);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddOrUpdateUnregisteredDevice");
            }
        }
        else
        {
            unregisteredDeviceByMac.DeviceAddress = deviceInfo.DeviceAddress;
            unregisteredDeviceByMac.IpAddress = deviceInfo.IpAddress;
            unregisteredDeviceByMac.Status = (short)deviceInfo.Status;
            try
            {
                unitOfWork.UnregistedDevicesRepository.Update(unregisteredDeviceByMac);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in AddOrUpdateUnregisteredDevice");
            }
        }

        try
        {
            unitOfWork.Save();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in AddOrUpdateUnregisteredDevice");
        }
    }

    private bool CheckAndRefreshToken(JObject response)
    {
        try
        {
            if (response["statusCode"]?.ToString() == StatusCodes.Status401Unauthorized.ToString())
            {
                var data = RefreshToken();
                return data != null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message + ex.StackTrace);
        }
        return false;
    }
}