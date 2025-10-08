using AutoMapper;
using Bogus.Extensions;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.UnregistedDevice;
using DeMasterProCloud.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using DeMasterProCloud.Api.Infrastructure.Mapper;

namespace DeMasterProCloud.Service
{
    public interface IUnregistedDeviceService
    {
        IQueryable<UnregistedDeviceModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);
        List<UnregistedDevice> GetAll();
        List<UnregistedDevice> GetByIds(List<int> ids);
        List<int> AddMissingDevice(List<UnregistedDevice> missingDevices);
        void DeleteMultiple(List<int> ids);
    }

    public class UnregistedDeviceService : IUnregistedDeviceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly IDeviceService _deviceService;
        private ISendMessageService _sendMessageService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public UnregistedDeviceService(IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor, IConfiguration configuration, IDeviceService deviceService)
        {
            _unitOfWork = unitOfWork;
            _httpContext = contextAccessor.HttpContext;
            _configuration = configuration;
            _deviceService = deviceService;
            _mapper = MapperInstance.Mapper;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<UnregistedDeviceService>();
        }

        /// <summary>
        /// Get paginated unregisted device
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public IQueryable<UnregistedDeviceModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                //var data = _unitOfWork.UnregistedDevicesRepository.GetByCompanyId(_httpContext.User.GetCompanyId())
                //    .AsEnumerable<UnregistedDevice>().Select(Mapper.Map<UnregistedDeviceModel>).AsQueryable();

                var data = _unitOfWork.UnregistedDevicesRepository.GetAll()
                    .AsEnumerable<UnregistedDevice>().Select(_mapper.Map<UnregistedDeviceModel>).AsQueryable();

                totalRecords = data.Count();

                if (!string.IsNullOrEmpty(filter))
                {
                    filter = filter.Trim().RemoveDiacritics().ToLower();
                    data = data.Where(x => x.DeviceAddress.RemoveDiacritics().ToLower().Contains(filter) ||
                                           x.IpAddress.RemoveDiacritics().ToLower().Contains(filter));
                }

                recordsFiltered = data.Count();

                data = data.OrderBy($"{sortColumn} {sortDirection}");
                data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginated");
                totalRecords = 0;
                recordsFiltered = 0;
                return Enumerable.Empty<UnregistedDeviceModel>().AsQueryable();
            }
        }

        /// <summary>
        /// Get all by company id
        /// </summary>
        /// <returns></returns>
        public List<UnregistedDevice> GetAll()
        {
            try
            {
                return _unitOfWork.UnregistedDevicesRepository.GetAll().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll");
                return new List<UnregistedDevice>();
            }
        }

        public List<UnregistedDevice> GetByIds(List<int> ids)
        {
            try
            {
                return _unitOfWork.AppDbContext.UnregistedDevice.Where(m => ids.Contains(m.Id)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIds");
                return new List<UnregistedDevice>();
            }
        }

        private int CheckDeviceType(string deviceType)
        {
            try
            {
                deviceType = deviceType.ToLower();

                foreach (var element in Enum.GetValues(typeof(DeviceType)))
                {
                    if (deviceType.Equals(element.GetName().ToLower()))
                    {
                        return (int) element;
                    }
                }

                if (deviceType.Contains(DeviceType.Icu300N.GetDescription().ToLower()))
                {
                    return (int)DeviceType.Icu300N;
                }
                else if (deviceType.Contains(DeviceType.Icu300NX.GetDescription().ToLower()))
                {
                    return (int)DeviceType.Icu300NX;
                }
                else if (deviceType.Contains(DeviceType.Icu400.GetDescription().ToLower()))
                {
                    return (int)DeviceType.Icu400;
                }
                else if (deviceType.Contains(DeviceType.ITouchPop.GetDescription().ToLower()))
                {
                    return (int)DeviceType.ITouchPop;
                }
                else if (deviceType.Contains(DeviceType.ITouchPopX.GetDescription().ToLower()))
                {
                    return (int)DeviceType.ITouchPopX;
                }
                else if (deviceType.Contains(DeviceType.DQMiniPlus.GetDescription().ToLower()))
                {
                    return (int)DeviceType.DQMiniPlus;
                }
                else if (deviceType.Contains(DeviceType.IT100.GetDescription().ToLower()))
                {
                    return (int)DeviceType.IT100;
                }
                else if (deviceType.Contains(DeviceType.FV6000.GetDescription().ToLower()))
                {
                    return (int)DeviceType.FV6000;
                }
                else if (deviceType.Contains(DeviceType.ITouch30A.GetDescription().ToLower()))
                {
                    return (int)DeviceType.ITouch30A;
                }
                else if (deviceType.Contains(DeviceType.DP636X.GetDescription().ToLower()))
                {
                    return (int)DeviceType.DP636X;
                }
                else if (deviceType.Contains(DeviceType.Biostation2.GetDescription().ToLower()))
                {
                    return (int)DeviceType.Biostation2;
                }
                else
                {
                    // 0 = ICU-300N
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckDeviceType");
                return 0;
            }
        }

        /// <summary>
        /// Add all missing device
        /// </summary>
        /// <param name="missingDevices"></param>
        public List<int> AddMissingDevice(List<UnregistedDevice> missingDevices)
        {
            try
            {
                List<int> deviceIds = new List<int>();
                foreach (var md in missingDevices)
                {
                    int deviceId = 0;
                    switch (md.RegisterType)
                    {
                        case (int)Registertype.NewDevice:
                            deviceId = AddOrUpdateDevice(md);
                            deviceIds.Add(deviceId);
                            break;
                        case (int)Registertype.Replace:
                            deviceId = AddOrUpdateDevice(md);
                            deviceIds.Add(deviceId);
                            break;
                        case (int)Registertype.Relocation:
                            DeleteOldDeviceFromSystem(md);
                            deviceId = AddOrUpdateDevice(md);
                            deviceIds.Add(deviceId);
                            break;
                    }
                }

                //Clear missing device
                _unitOfWork.UnregistedDevicesRepository.Delete(m => missingDevices.Select(n => n.Id).Contains(m.Id));
                _unitOfWork.Save();
                return deviceIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddMissingDevice");
                return new List<int>();
            }
        }

        /// <summary>
        /// Delete multiple unregistered device(s).
        /// </summary>
        /// <param name="ids"> list of unregistered device(s)'s identifier </param>
        public void DeleteMultiple(List<int> ids)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var unregisteredDevices = _unitOfWork.AppDbContext.UnregistedDevice.Where(m => ids.Contains(m.Id)).ToList();

                        _unitOfWork.UnregistedDevicesRepository.DeleteRange(unregisteredDevices);
                        _unitOfWork.Save();

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// This functions is to register a device to our system.
        /// </summary>
        /// <param name="unregisteredDevice"> Unregistered devices to be registered </param>
        private int AddOrUpdateDevice(UnregistedDevice unregisteredDevice)
        {
            try
            {
                IcuDevice oldDevice = _deviceService.GetByDeviceAddress(unregisteredDevice.DeviceAddress);

                if (oldDevice != null)
                {
                    DeviceModel device = _mapper.Map<DeviceModel>(oldDevice);

                    device.MacAddress = unregisteredDevice.MacAddress;
                    device.IpAddress = unregisteredDevice.IpAddress;
                    device.OperationType = (short)unregisteredDevice.OperationType;
                    device.CompanyId = oldDevice.CompanyId ?? null;
                    device.BuildingId = oldDevice.BuildingId ?? null;

                    _deviceService.Update(device);
                    return device.Id;
                }
                else
                {
                    var device = new DeviceModel
                    {
                        DeviceAddress = unregisteredDevice.DeviceAddress,
                        IpAddress = unregisteredDevice.IpAddress,
                        DoorName = "Door " + unregisteredDevice.DeviceAddress,

                        MPRCount = Constants.Settings.DefaultMprAuthCount,
                        VerifyMode = Constants.Settings.DefaultVerifyMode,
                        LockOpenDuration = Constants.Settings.DefaultLockOpenDurationSeconds,
                        SensorType = Constants.Settings.DefaultSensorType,

                        DeviceType = CheckDeviceType(unregisteredDevice.DeviceType),

                        MacAddress = unregisteredDevice.MacAddress
                    };

                    switch (device.DeviceType)
                    {
                        case (int)DeviceType.Icu300N:
                        case (int)DeviceType.Icu300NX:
                        case (int)DeviceType.Icu400:
                        case (int)DeviceType.Icu500:
                            device.UseCardReader = null;

                            device.RoleReader1 = (short)RoleRules.Out;
                            device.LedReader1 = (short)CardReaderLed.Blue;

                            device.BuzzerReader0 = (short)BuzzerReader.ON;
                            device.BuzzerReader1 = (short)BuzzerReader.ON;

                            break;
                        case (int)DeviceType.ITouchPop:
                        case (int)DeviceType.ITouchPopX:
                        case (int)DeviceType.DQMiniPlus:
                        case (int)DeviceType.IT100:
                        case (int)DeviceType.ITouch30A:
                        case (int)DeviceType.DP636X:
                            device.UseCardReader = (short)UseCardReader.NotUse;

                            device.RoleReader1 = null;
                            device.LedReader1 = null;

                            device.BuzzerReader0 = null;
                            device.BuzzerReader1 = null;

                            break;
                        default:
                            break;
                    }

                    device.DeviceBuzzer = (short)BuzzerReader.ON;

                    var deviceId = _deviceService.Add(device);

                    // Send auth_start message.
                    if (unregisteredDevice.Status == (short)ConnectionStatus.LostKey)
                    {
                        // 1. get TIMESTAMP
                        string timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
                        // 2. make K-AES
                        int ret = DmpAuth.DMP_auth_makekey(Helpers.HexStringToByteArray(unregisteredDevice.DeviceAddress), Helpers.HexStringToByteArray(timeStamp));

                        if (ret == (int)Status.Valid)
                        {
                            _sendMessageService = new SendMessageService(_configuration);

                            // 4. Send message to device
                            // _sendMessageService.SendAuthStartMessage(unregisteredDevice.DeviceAddress, timeStamp);

                            var icuDevice = _deviceService.GetById(deviceId);

                            icuDevice.ConnectionStatus = unregisteredDevice.Status;
                            _unitOfWork.IcuDeviceRepository.Update(icuDevice);
                            _unitOfWork.Save();

                            _sendMessageService.SendDeviceStatusToFE(icuDevice);
                        }
                    }

                    return deviceId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddOrUpdateDevice");
                return 0;
            }
        }

        /// <summary>
        /// This function is to delete old device from our system.
        /// (* In fact, not deleted. It is just changed value about 'IsDeleted' attribute. )
        /// </summary>
        /// <param name="unregisteredDevice"> Unregistered devices to be deleted </param>
        private void DeleteOldDeviceFromSystem(UnregistedDevice unregisteredDevice)
        {
            try
            {
                IcuDevice oldDevice = _unitOfWork.IcuDeviceRepository.GetDeviceByMacAddress(unregisteredDevice.MacAddress);

                if(oldDevice != null && oldDevice.DeviceAddress != unregisteredDevice.DeviceAddress)
                    _deviceService.Delete(oldDevice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteOldDeviceFromSystem");
            }
        }
    }
}