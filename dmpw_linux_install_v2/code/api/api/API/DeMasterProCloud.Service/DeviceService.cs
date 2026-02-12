using AutoMapper;
using Bogus;
using Bogus.Extensions;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.AccessGroup;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.Timezone;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Protocol;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using User = DeMasterProCloud.DataAccess.Models.User;
using System.Threading;
using DeMasterProCloud.DataModel.SystemInfo;
using DeMasterProCloud.Service.Infrastructure;
using Constants = DeMasterProCloud.Common.Infrastructure.Constants;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.DataModel.EventLog;
using DeMasterProCloud.DataModel.Vehicle;
using System.IO.Compression;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.DataModel.DeviceSDK;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.Service.RabbitMqQueue;

namespace DeMasterProCloud.Service
{
    public interface IDeviceService : IPaginationService<IcuDevice>
    {
        int Add(DeviceModel model);

        DeviceDataModel InitData(DeviceDataModel model, int companyId = 0);

        DeviceInitModel InitializeData();

        void Update(DeviceModel model);

        IcuDevice GetByIdAndCompany(int id, int companyId);
        IcuDevice GetByDeviceAddress(string deviceAddress);
        bool HasExistName(int id, string deviceAddress);

        IcuDevice GetById(int id);

        List<IcuDevice> GetByIds(List<int> ids);

        void Delete(IcuDevice device);

        List<IcuDevice> GetByIdsAndCompany(List<int> idArr, int companyId, bool isAll = false);
        void DeleteRange(List<IcuDevice> devices);

        bool IsDeviceAddressExist(DeviceModel model);

        void UpdateDeviceStatus(IcuDevice device, bool status);

        void SendDeviceInfo(string deviceAddress);

        int SendInstruction(List<IcuDevice> devices, DeviceInstruction model);

        bool HasTimezone(int timezoneId, int companyId);

        IEnumerable<IcuDevice> GetDoorList();

        void Reinstall(List<IcuDevice> devices, bool isAddDevice = false, List<ReinstallDeviceDetail> reinstallDevices = null);

        void CopyDevices(IcuDevice deviceCopy, List<IcuDevice> devices);

        DeviceTypeList GetListDeviceType(List<IcuDevice> devices);

        void UploadFile(IFormFileCollection files);
        TransmitInfoModel GetTransmitAllData();

        List<DoorStatusModel> InitDoorStatus(IOrderedEnumerable<EnumModel> enumModels);
        List<IcuDevice> GetOnlineDevices(List<IcuDevice> devices, ref string firstOffAddr);

        IQueryable<IcuDevice> GetPaginatedDeviceValid(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered, List<int> companyId, List<int> connectionStatus, List<int> deviceType);
        IEnumerable<RecentlyDisconnectedDeviceModel> getRecentlyDisconnectedDevices(int pageNumber, int pageSize, int sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        void TransmitData(TransmitDataModel models, List<IcuDevice> devices);

        UserInfoModel GetUserInfo(IcuDevice device, string cardId);

        UserInfoByCardIdModel GetUserInfoByCardId(IcuDevice device, string cardId);
        UserInfoByCardIdModel CheckUserInfoInDevice(IcuDevice device, string cardId, bool deviceCheck = true);

        void RequestBackup();
        
        void StopProcess(List<IcuDevice> devices, List<string> processIds);

        void SendDeviceConfig(IcuDevice device, string protocolType);

        List<IcuDevice> GetByCompanyId(int companyId);

        IEnumerable<DeviceHistoryModel> GetHistory(IcuDevice device, int pageNumber, int pageSize, out int totalRecords);

        IQueryable<DeviceListModel> GetPaginatedDevices(DeviceFilterModel filter, out int totalRecords, out int recordsFiltered);

        IQueryable<IcuDevice> GetPaginatedDevices(int pageNumber, int pageSize,
            out int totalRecords, out int recordsFiltered, List<short> operationTypes, List<int> companyId, List<int> connectionStatus);

        IQueryable<IcuDevice> GetDevicesForDashBoard(short operationType, List<int> companyId);

        void UpdateUpTimeToDevice(int deviceId);
        int ReUpdateUpTimOnlineDevice();
        void ReUpdateUpTimOnlineDeviceById(int id);
        object ListFilterDeviceMonitoring();
        object getInit();
        bool IsAbleDeviceVerify(DeviceModel model);
        void ChangedConnectionStatus(IcuDevice device, short status, IModel channel = null, List<string> columns = null);
        short CheckPermissionUserOrVisitOpenDoor(User user, Visit visit, IcuDevice device, DateTime timeUtc, Card vehicleCard = null, bool bothCheckVehicle = false);
        void AssignToCompany(List<int> deviceIds, int companyId);
        List<IcuDevice> GetByRidsAndCompany(List<string> ridArr, int companyId);
        List<IcuDevice> GetDevicesByBuildingIds(List<int> buildingIds, int companyId, bool includeChildren = false);
        SubDisplayDeviceModel GetSubDisplayDeviceInfoByToken(string tokenDevice);
        DataTokenScreenMonitoring GetDataByTokenMonitoring(string token);
        string AssignUsersToDevice(int deviceId, List<int> userIds);
        string UnassignUsersFromDevice(int deviceId, List<int> userIds, bool ok);
        CheckLimitAddedModel CheckLimitDevicesAdded(int numberOfAdded);
        List<VehicleInOutStatusListModel> GetInOutStatus(List<EventLogByWorkType> data, string search, int pageNumber, int pageSize, string sortColumn, string sortDirection, out int total, out int filtered);

        void UploadFileLogDevice(IcuDevice device, string msgId, IFormFile file);
        byte[] GetFileLogOfDevice(IcuDevice device, string fileName = null);
        bool SendDeviceRequest(string msgId, string command, IcuDevice device);
        List<FileLogListModel> GetAllLogsOfDevice(IcuDevice device);

        List<EnumModelWithValue> GetRegisteredCardsByType(int id);

        int CountRegisteredIdByDeviceId(int deviceId);
    }

    public class DeviceService : IDeviceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IConnection _connection;
        private readonly IAccessGroupService _accessGroupService;
        private readonly IWebSocketService _webSocketService;
        private readonly IDeviceSDKService _deviceSdkService;

        private readonly ISendMessageService _sendMessageService;
        private readonly IMapper _mapper = MapperInstance.Mapper;
        public DeviceService(IUnitOfWork unitOfWork, IAccessGroupService accessGroupService)
        {
            _unitOfWork = unitOfWork;
            _accessGroupService = accessGroupService;
            _sendMessageService = new SendMessageService(_configuration);
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<DeviceService>();
        }

        public DeviceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _sendMessageService = new SendMessageService(_configuration);
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<DeviceService>();
        }
        public DeviceService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _sendMessageService = new SendMessageService(_configuration);
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<DeviceService>();
        }

        public DeviceService(IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor,
            IConfiguration configuration, IAccessGroupService accessGroupService, IWebSocketService webSocketService, IDeviceSDKService deviceSdkService)
        {
            _unitOfWork = unitOfWork;
            _httpContext = contextAccessor.HttpContext;
            _configuration = configuration;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<DeviceService>();
            _accessGroupService = accessGroupService;
            _connection = ApplicationVariables.GetAvailableConnection(configuration);
            _webSocketService = webSocketService;
            _deviceSdkService = deviceSdkService;
        }

        /// <summary>
        /// Initial data
        /// </summary>
        /// <param name="model"></param>
        public DeviceDataModel InitData(DeviceDataModel model, int companyId = 0)
        {
            try
            {
                var currentCompanyId = _httpContext.User.GetCompanyId();

                if (companyId != 0)
            {
                var timezoneList = _unitOfWork.AccessTimeRepository
                    .GetMany(m => !m.IsDeleted && m.CompanyId == companyId).Select(
                        m =>
                            new SelectListItemModel
                            {
                                Id = m.Id,
                                Name = m.Name
                            }).ToList();
                model.ActiveTimezoneItems = timezoneList;

                if (model.ActiveTimezoneId == 0 && model.ActiveTimezoneItems.Any())
                {
                    model.ActiveTimezoneId = _unitOfWork.AccessTimeRepository
                        .GetDefaultTzByCompanyId(Constants.Settings.DefaultPositionActiveTimezone, companyId).Id;
                }

                model.PassageTimezoneItems = timezoneList;

                if (model.PassageTimezoneId == null || model.PassageTimezoneId == 0 && model.PassageTimezoneItems.Any())
                {
                    model.PassageTimezoneId = _unitOfWork.AccessTimeRepository
                        .GetDefaultTzByCompanyId(Constants.Settings.DefaultPositionPassageTimezone, companyId).Id;
                }

                model.BuildingItems = _unitOfWork.BuildingRepository
                    .GetByCompanyId(companyId)
                    .Select(
                        m =>
                        new SelectListItemModel
                        {
                            Id = m.Id,
                            Name = m.Name
                        }).ToList();

                model.AccessTzItems = _unitOfWork.AccessTimeRepository
                    .GetMany(m => !m.IsDeleted && m.CompanyId == companyId)
                    .Select(
                        m =>
                        new SelectListItemModel
                        {
                            Id = m.Id,
                            Name = m.Name
                        }).ToList();

                if (model.BuildingId == 0 && model.BuildingItems.Any())
                {
                    model.BuildingId = _unitOfWork.BuildingRepository
                        .GetDefaultByCompanyId(companyId).Id;
                }

                model.CompanyId = companyId;
            }
            else
            {
                var defaultList = _unitOfWork.AccessTimeRepository
                    .GetMany(m => !m.IsDeleted && m.CompanyId == companyId).Select(
                        m =>
                            new SelectListItemModel
                            {
                                Id = m.Id,
                                Name = m.Name
                            }).ToList();

                var noneList = new SelectListItemModel
                {
                    Id = 0,
                    Name = "None"
                };

                defaultList.Add(noneList);

                model.ActiveTimezoneItems = defaultList;
                model.ActiveTimezoneId = 0;

                model.PassageTimezoneItems = defaultList;
                model.PassageTimezoneId = 0;

                model.BuildingItems = defaultList;
                model.BuildingId = 0;

                model.AccessTzItems = defaultList;
            }

            if (currentCompanyId != 0)
            {
                model.CompanyItems = _unitOfWork.CompanyRepository.GetMany(m => !m.IsDeleted && m.Id == companyId).Select(m => new SelectListItemModel { Id = m.Id, Name = m.Name }).ToList();
            }
            else
            {
                model.CompanyItems = _unitOfWork.CompanyRepository.GetMany(m => !m.IsDeleted).Select(m => new SelectListItemModel { Id = m.Id, Name = m.Name }).ToList();
            }


            var verifyModeList = EnumHelper.ToEnumList<SingleVerify>();
            verifyModeList = verifyModeList.Concat(EnumHelper.ToEnumList<SingleVerifyOR>()).ToList();
            verifyModeList = verifyModeList.Concat(EnumHelper.ToEnumList<MultiVerify>()).ToList();
            //model.VerifyModeItems = EnumHelper.ToEnumList<VerifyMode>();
            model.VerifyModeItems = verifyModeList;
            if (model.VerifyMode == 0 && model.VerifyModeItems.Any())
            {
                model.VerifyMode = (int)SingleVerify.Card;
            }
            

            model.DeviceTypeItems = EnumHelper.ToEnumList<DeviceType>();
            var listDeviceType = new List<EnumModelDeviceType>();
            foreach(var device in model.DeviceTypeItems)
            {
                var deviceTemp = new EnumModelDeviceType();
                deviceTemp.Id = device.Id;
                deviceTemp.Name = device.Name;
                switch (device.Id)
                {
                    case (int)DeviceType.Icu300N:
                    case (int)DeviceType.Icu300NX:
                    case (int)DeviceType.Icu400:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerify.PassCode,
                            (int) SingleVerifyOR.CardOrQR,
                            (int) SingleVerifyOR.CardOrPassCode,
                            (int) MultiVerify.CardAndQR,
                            (int) MultiVerify.CardAndPassCode 
                            }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.ITouchPop:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int) SingleVerify.Card,
                            (int) SingleVerify.PassCode,
                            (int) SingleVerifyOR.CardOrPassCode,
                            (int) MultiVerify.CardAndPassCode
                            }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.ITouchPopX:
                    case (int)DeviceType.PM85:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerify.PassCode,
                            (int) SingleVerifyOR.CardOrQR,
                            (int) SingleVerifyOR.CardOrPassCode,
                            (int) SingleVerifyOR.CardOrQROrPassCode,
                            (int) MultiVerify.CardAndQR,
                            (int) MultiVerify.CardAndPassCode
                            }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.DQMiniPlus:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerifyOR.CardOrQR,
                            (int) MultiVerify.CardAndQR
                            }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.IT100:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int)SingleVerify.Card,
                            (int)SingleVerify.Face,
                            (int)SingleVerifyOR.FaceOrIris,
                            (int)SingleVerifyOR.CardOrFaceOrIris,
                            /*(int)MultiVerify.FaceAndIris,*/
                            }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.DesktopApp:
                        deviceTemp.VerifyModeList = verifyModeList;
                        break;
                    case (int)DeviceType.NexpaLPR:
                        deviceTemp.VerifyModeList = verifyModeList;
                        break;
                    case (int)DeviceType.FV6000:
                    case (int)DeviceType.XStation2:
                        deviceTemp.VerifyModeList = verifyModeList;
                        break;
                    case (int)DeviceType.Biostation2:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int) SingleVerify.Card,
                            (int) SingleVerify.FingerprintOnly,
                            (int) SingleVerifyOR.CardOrFingerprint,
                            (int) MultiVerify.CardAndPassCode,
                            (int) MultiVerify.CardAndFingerprint,
                            (int) MultiVerify.FingerprintAndPassCode
                            }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.Biostation3:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) MultiVerify.CardAndFace,
                            (int) MultiVerify.CardAndPassCode,
                            (int) MultiVerify.CardAndFaceAndPassCode,
                            (int) MultiVerify.FaceAndPassCode,
                            }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.EbknReader:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int) SingleVerify.Face,
                            (int) SingleVerify.FingerprintOnly,
                            (int) MultiVerify.FingerprintAndFace,
                            (int) MultiVerify.FaceAndPassCode,
                            (int) SingleVerifyOR.FaceOrFingerprintOrPassCode,
                            (int) MultiVerify.FingerprintAndPassCode,
                            }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.BA8300:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int) SingleVerify.Face,
                            (int) SingleVerify.FingerprintOnly,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerifyOR.CardOrFingerprint,
                            (int) SingleVerifyOR.CardOrQR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) MultiVerify.PlateNumberAndVNID,
                            }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.DF970:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerifyOR.CardOrQR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) SingleVerify.Vehicle,
                            (int) SingleVerify.VNID,
                            (int) MultiVerify.VehicleAndFace,
                            (int) MultiVerify.PlateNumberAndVNID,
                            }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.Icu500:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerifyOR.CardOrQR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) SingleVerify.Vehicle,
                            (int) SingleVerify.VNID,
                            (int) MultiVerify.PlateNumberAndVNID,
                        }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.T2Face:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerifyOR.CardOrQR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) SingleVerify.VNID,
                        }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.RA08:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                        }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.DQ8500:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() {
                            (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) MultiVerify.PlateNumberAndVNID,
                        }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.DQ200:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() {
                            (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) MultiVerify.PlateNumberAndVNID,
                        }.Contains(m.Id)).ToList();
                        break;
                    case (int)DeviceType.TBVision:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() {
                            (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) MultiVerify.PlateNumberAndVNID,
                        }.Contains(m.Id)).ToList();
                        break;
                    default:
                        deviceTemp.VerifyModeList = verifyModeList.FindAll(m => new List<int>() { 
                            (int)SingleVerify.Card
                            }.Contains(m.Id)).ToList();
                        break;
                }

                // add
                listDeviceType.Add(deviceTemp);
            }
            model.DeviceTypeList = listDeviceType;


            

          
            model.BioStationModeItems = EnumHelper.ToEnumList<BioStationMode>();

            if (model.BackupPeriod == 0)
            {
                model.BackupPeriod = Constants.Settings.DefaultBackupPeriod;
            }

            model.OperationTypeItems = EnumHelper.ToEnumList<OperationType>();

            model.PassbackItems = EnumHelper.ToEnumList<PassbackRules>();
            model.Passback = model.Passback ?? 0;

            model.RoleItems = EnumHelper.ToEnumList<RoleRules>();
            if (model.Id == 0 && model.RoleItems.Any())
            {
                model.RoleReader0 = (short)model.RoleItems.First().Id;
                model.RoleReader1 = 1;
            }
            else
            {
                model.RoleReader0 = model.RoleReader0 == null ? 0 : model.RoleReader0;
                model.RoleReader1 = model.RoleReader1 == null ? 1 : model.RoleReader1;
            }

            //Init card reader led items
            model.CardReaderLedItems = EnumHelper.ToEnumList<CardReaderLed>();
            if (model.Id == 0 && model.CardReaderLedItems.Any())
            {
                model.LedReader0 = (short)model.CardReaderLedItems.First().Id;
                model.LedReader1 = (short)model.CardReaderLedItems.First().Id;
            }
            else
            {
                model.LedReader0 = model.LedReader0 == null ? 0 : model.LedReader0;
                model.LedReader1 = model.LedReader1 == null ? 0 : model.LedReader1;
            }

            model.BuzzerReaderItems = EnumHelper.ToEnumList<BuzzerReader>();
            if (model.Id == 0 && model.BuzzerReaderItems.Any())
            {
                model.BuzzerReader0 = (short)BuzzerReader.ON;
                model.BuzzerReader1 = (short)BuzzerReader.ON;

                model.DeviceBuzzer = (short)BuzzerReader.ON;
            }
            else
            {
                model.BuzzerReader0 = model.BuzzerReader0 == null ? (short)BuzzerReader.ON : model.BuzzerReader0;
                model.BuzzerReader1 = model.BuzzerReader1 == null ? (short)BuzzerReader.ON : model.BuzzerReader1;
            }

            model.UseCardReaderItems = EnumHelper.ToEnumList<UseCardReader>();
            model.UseCardReader = model.UseCardReader ?? (short)1;

            // Init
            // A. If click the add button, model.Id == 0
            // B. If double-click the device(for editing), model.Id == device.Id
            if (model.Id == 0)
            {
                model.SensorType = Constants.Settings.DefaultSensorType;

                model.LockOpenDuration = Constants.Settings.DefaultLockOpenDurationSeconds;
            }

            model.SensorTypeItems = EnumHelper.ToEnumList<SensorType>();
            if (model.SensorType != 0 && model.SensorType != 1 && model.SensorType != 2)
            {
                model.SensorType = Constants.Settings.DefaultSensorType;
            }

            if (model.SensorDuration == 0 || model.SensorDuration == null)
            {
                model.SensorDuration = Constants.Settings.DefaultSensorDuration;
            }

            if (model.MPRCount == null || model.MPRCount < Constants.Settings.DefaultMprAuthCount)
            {
                model.MPRCount = Constants.Settings.DefaultMprAuthCount;
            }

            if (model.MPRInterval == 0)
            {
                model.MPRInterval = Constants.Settings.DefaultMprInterval;
            }

            // accountIds
            if (model.CompanyId.HasValue)
            {
                model.AccountIds = _unitOfWork.AppDbContext.CompanyAccount
                    .Include(m => m.Account).ThenInclude(n => n.User)
                    .Where(m => m.CompanyId == model.CompanyId.Value && m.AccountId.HasValue)
                    .Select(m => new EnumModel()
                    {
                        Id = m.AccountId.Value,
                        Name = m.Account.User.FirstOrDefault(n => n.CompanyId == model.CompanyId.Value) == null
                            ? $"{m.Account.Username}"
                            : $"{m.Account.User.FirstOrDefault(n => n.CompanyId == model.CompanyId.Value).FirstName} ({m.Account.Username})"
                    });

                model.DependentDoorsIds = _unitOfWork.AppDbContext.IcuDevice.Where(x =>
                    x.CompanyId == model.CompanyId.Value && x.Id != model.Id
                    && !x.IsDeleted).Select(x => new EnumModel()
                    {
                        Id = x.Id,
                        Name = x.Name
                    });
            }

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitData");
                return null;
            }
        }

        public int Add(DeviceModel model)
        {
            IcuDevice deviceSaved = null;
            int deviceId = 0;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var device = _mapper.Map<IcuDevice>(model);
                        device.Status = (short)Status.Valid;

                        if (device.CompanyId == null)
                        {
                            device.ActiveTzId = null;
                            device.PassageTzId = null;
                            device.BuildingId = null;
                        }

                        _unitOfWork.IcuDeviceRepository.Add(device);
                        _unitOfWork.Save();
                        deviceSaved = device;

                        _unitOfWork.Save();
                        transaction.Commit();
                        deviceId = device.Id;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        deviceId = 0;
                        throw;
                    }
                }
            });

            if (deviceSaved.CompanyId != null)
            {
                // Assign company
                AssignToCompany(new List<int> { deviceSaved.Id }, deviceSaved.CompanyId ?? 0);
            }

            if (deviceSaved.DeviceType != (short)DeviceType.DesktopApp)
            {
                //Send device to rabbit mq server
                var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
                deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                {
                    MsgId = Guid.NewGuid().ToString(),
                    DeviceId = deviceSaved.Id,
                    DeviceAddress = deviceSaved.DeviceAddress,
                    MessageType = Constants.Protocol.DeviceInstruction,
                    Command = Constants.CommandType.UpdateDeviceState
                });
            }
            
            return deviceId;
        }

        /// <summary>
        /// Assign the door(s) to company
        /// This function is used on the Edit or Add page of the device settings.
        /// </summary>
        /// <param name="deviceIds">list of device id</param>
        /// <param name="companyId">company id to assign</param>
        public void AssignToCompany(List<int> deviceIds, int companyId)
        {
            List<IcuDevice> AssignedDevices = null;

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var devices = GetByIds(deviceIds);
                        var company = _unitOfWork.CompanyRepository.GetById(companyId);

                        foreach (var eachDevice in devices)
                        {
                            var existCompanyId = eachDevice.CompanyId;

                            // Check whether the company is changed or not.
                            if (existCompanyId == companyId)
                            {
                                continue;
                            }

                            //Assign device
                            eachDevice.CompanyId = companyId;
                            eachDevice.Company = company;

                            var timezones = _unitOfWork.AccessTimeRepository.GetByCompany(companyId);

                            // Active timezone
                            if (eachDevice.ActiveTzId == null || eachDevice.ActiveTzId == 0)
                            {
                                eachDevice.ActiveTzId = _unitOfWork.AccessTimeRepository
                                                .GetDefaultTzByCompanyId(Constants.Settings.DefaultPositionActiveTimezone, companyId).Id;
                            }
                            else
                            {
                                if (timezones.Where(m => m.Id == eachDevice.ActiveTzId) == null || !timezones.Where(m => m.Id == eachDevice.ActiveTzId).Any())
                                {
                                    eachDevice.ActiveTzId = timezones.Where(m => m.Position == Constants.Settings.DefaultPositionActiveTimezone).FirstOrDefault().Id;
                                }
                            }

                            // Passage timezone
                            if (eachDevice.PassageTzId == null || eachDevice.PassageTzId == 0)
                            {
                                eachDevice.PassageTzId = _unitOfWork.AccessTimeRepository
                                                .GetDefaultTzByCompanyId(Constants.Settings.DefaultPositionPassageTimezone, companyId).Id;
                            }
                            else
                            {
                                if (timezones.Where(m => m.Id == eachDevice.PassageTzId) == null || !timezones.Where(m => m.Id == eachDevice.PassageTzId).Any())
                                {
                                    eachDevice.PassageTzId = timezones.Where(m => m.Position == Constants.Settings.DefaultPositionPassageTimezone).FirstOrDefault().Id;
                                }
                            }

                            var buildings = _unitOfWork.BuildingRepository.GetByCompanyId(companyId);

                            // Building
                            if (eachDevice.BuildingId == null || eachDevice.BuildingId == 0)
                            {
                                eachDevice.BuildingId = buildings.OrderBy(c => c.Id).FirstOrDefault().Id;
                            }
                            else
                            {
                                if (buildings.Where(m => m.Id == eachDevice.BuildingId) == null || !buildings.Where(m => m.Id == eachDevice.BuildingId).Any())
                                {
                                    eachDevice.BuildingId = buildings.OrderBy(c => c.Id).FirstOrDefault().Id;
                                }
                            }

                            _unitOfWork.IcuDeviceRepository.Update(eachDevice);

                            // If the device is already assigned to a company, save the unassign system logs to an existing company.
                            if (existCompanyId != null)
                            {
                                var contents = $"{ActionLogTypeResource.UnassignDoor} : {eachDevice.Name}";

                                _unitOfWork.SystemLogRepository.Add(eachDevice.Id, SystemLogType.DeviceSetting,
                                ActionLogType.Delete, contents, null, null, existCompanyId);

                                _unitOfWork.Save();
                            }
                        }

                        AssignedDevices = devices;

                        //Save system log
                        var device = devices.First();
                        var content = $"{ActionLogTypeResource.AssignDoor}";
                        var deviceAddresses = devices.Select(c => c.DeviceAddress).ToList();
                        var contentDetails = $"{DeviceResource.lblDeviceCount} : {devices.Count()} \n"
                        + $"{DeviceResource.lblDeviceAddress}: {string.Join(", ", deviceAddresses)}";

                        _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceSetting, ActionLogType.Add,
                            content, contentDetails, deviceIds, companyId);

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
            
            foreach (var device in AssignedDevices)
            {
                // Assign to Full Access Group.
                AssignToFullAccessGroup(device.Id, companyId);
            }

        }

        /// <summary>
        /// Update device
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void Update(DeviceModel model)
        {
            IcuDevice device = null;
            int? oldCompanyId = null;
            int? oldBuildingId = null;

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        device = GetById(model.Id);
                        List<string> changes = new List<string>();
                        CheckChange(device, model, ref changes);
                        
                        var existingName = device.Name;
                        oldCompanyId = device.CompanyId;
                        oldBuildingId = device.BuildingId;

                        _mapper.Map(model, device);

                        if (device.UseCardReader == null)
                        {
                            device.UseCardReader = 1;
                        }

                        // If device type is iTouchPop2A or iTouchPopX or DQ-MINI useCardReader should be checked.
                        if (device.DeviceType == (short)DeviceType.ITouchPop
                            || device.DeviceType == (short)DeviceType.ITouchPopX
                            //|| device.DeviceType == (short)DeviceType.DQMiniPlus
                            || device.DeviceType == (short)DeviceType.IT100)
                        {
                            if (device.UseCardReader == 1)
                            {
                                device.RoleReader1 = null;
                                device.LedReader1 = null;
                                device.BuzzerReader1 = null;
                            }
                        }

                        _unitOfWork.IcuDeviceRepository.Update(device);

                        // Save system log
                        if (device.CompanyId != null)
                        {
                            var content = string.Format(DeviceResource.lblUpdateDevice, existingName);
                            var contentsDetails = string.Join("\n", changes);
                            _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceSetting, ActionLogType.Update,
                                content, contentsDetails, null, device.CompanyId);
                        }

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

            // Assign company
            if (device.CompanyId != null && oldCompanyId != device.CompanyId)
            {
                // Assign company
                AssignToCompany(new List<int> { device.Id }, device.CompanyId ?? 0);
            }

            // A building info has changed. BE should send device info message.
            string sender = _httpContext.User.GetUsername();
            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
            if (device.BuildingId != null && oldBuildingId != device.BuildingId)
            {
                string groupMsgId = Guid.NewGuid().ToString();
                deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                {
                    DeviceAddress = device.DeviceAddress,
                    DeviceId = device.Id,
                    MessageType = Constants.Protocol.DeviceInstruction,
                    MsgId = groupMsgId,
                    Sender = sender,
                    
                    IsSaveSystemLog = true,
                    Command = Constants.CommandType.SetTime,
                    UserName = sender,
                });
                
                deviceInstructionQueue.SendLoadDeviceInfo(new LoadDeviceInfoQueueModel()
                {
                    DeviceAddress = device.DeviceAddress,
                    DeviceId = device.Id,
                    MessageType = Constants.Protocol.LoadDeviceInfo,
                    MsgId = groupMsgId,
                    Sender = sender,
                });
            }

            var company = _unitOfWork.CompanyRepository.GetById(device.CompanyId ?? 0);
            if (company != null && company.AutoSyncUserData && device.ConnectionStatus == (short)ConnectionStatus.Online)
            {
                // Get setting
                bool useStaticQrCode = false;
                if (device.CompanyId.HasValue)
                {
                    var setting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.UseStaticQrCode, device.CompanyId.Value);
                    bool.TryParse(Helpers.GetStringFromValueSetting(setting?.Value), out useStaticQrCode);
                }
                
                //Send device config
                deviceInstructionQueue.SendDeviceConfig(new ConfigQueueModel()
                {
                    DeviceAddress = device.DeviceAddress,
                    DeviceId = device.Id,
                    MessageType = Constants.Protocol.UpdateDeviceConfig,
                    MsgId = Guid.NewGuid().ToString(),
                    Sender = sender,
                    UseStaticQrCode = useStaticQrCode
                });
            }
        }

        /// <summary>
        /// Delete device
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public void Delete(IcuDevice device)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //Delete device
                        device.Status = (short)Status.Invalid;
                        _unitOfWork.IcuDeviceRepository.DeleteFromSystem(device);

                        //Save system log
                        if (device.Company != null)
                        {
                            var content =
                                $"{ActionLogTypeResource.Delete} : {device.Name} ({device.DeviceAddress})";

                            _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceSetting, ActionLogType.Delete,
                                content, null, null, device.CompanyId);
                        }

                        _unitOfWork.Save();
                        transaction.Commit();

                        // delete account rabbitmq
                        string userDeviceRabbit = ((DeviceType)device.DeviceType).GetName() + "_" + device.DeviceAddress;
                        Helpers.DeleteAccountRabbitMq(_configuration, new List<string>() { }, new List<string>() { userDeviceRabbit });
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
            //Send device to rabbit mq server
            // [Edward] 2019.09.18
            // 존재 이유가 불명확하여 임시 주석
            //SendDeviceInstruction(device, Constants.CommandType.UpdateDeviceState);
            UnBindDeviceFromQueue(device.DeviceAddress);
        }

        /// <summary>
        /// Delete a list of device
        /// </summary>
        /// <param name="devices"></param>
        /// <returns></returns>
        public void DeleteRange(List<IcuDevice> devices)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        List<string> usersDeviceRabbit = new List<string>();
                        foreach (var device in devices)
                        {
                            //Delete device
                            device.Status = (short)Status.Invalid;
                            _unitOfWork.IcuDeviceRepository.DeleteFromSystem(device);
                            
                            if (device.Company != null)
                            {
                                var content = $"{ActionLogTypeResource.Delete} : {device.Name} ({device.DeviceAddress})";

                                _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceSetting, ActionLogType.Delete,
                                    content, null, null, device.CompanyId);
                            }

                            usersDeviceRabbit.Add(((DeviceType)device.DeviceType).GetName() + "_" + device.DeviceAddress);
                        }

                        ////Save system log
                        //foreach (var device in devices)
                        //{
                        //    if (device.Company != null)
                        //    {
                        //        var content = $"{ActionLogTypeResource.Delete}: {device.Name} ({device.DeviceAddress})";

                        //        _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceSetting,
                        //            ActionLogType.Add, content, null, null, device.CompanyId);
                        //    }
                        //}

                        _unitOfWork.Save();
                        transaction.Commit();

                        // delete account rabbitmq
                        Helpers.DeleteAccountRabbitMq(_configuration, new List<string>() { }, usersDeviceRabbit);
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });

            //Send device to rabbit mq server
            foreach (var device in devices)
            {
                UnBindDeviceFromQueue(device.DeviceAddress);
            }
        }

        /// <summary>
        /// Get data with pagination
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public IQueryable<IcuDevice> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var data = _unitOfWork.AppDbContext.IcuDevice
                .Include(m => m.ActiveTz)
                .Include(m => m.PassageTz)
                .Include(m => m.Building)
                .Include(m => m.Company)
                .Where(m => !m.IsDeleted);

            if (_httpContext.User.GetAccountType() != (short)AccountType.SystemAdmin)
            {
                data = data.Where(m => m.CompanyId == _httpContext.User.GetCompanyId());
            }

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.Where(x => x.Name.RemoveDiacritics().ToLower().Contains(filter)
                                       || x.DeviceAddress.RemoveDiacritics().ToLower().Contains(filter)
                                       || x.Building.Name.RemoveDiacritics().ToLower().Contains(filter));
            }

            recordsFiltered = data.Count();
            data = data.OrderBy($"{sortColumn} {sortDirection}");
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var accountTimezone = _unitOfWork.AccountRepository.Get(m =>
                    m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted).TimeZone;
            var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

            foreach (var device in data)
            {
                //device.LastCommunicationTime = Helpers.ConvertToUserTime(device.LastCommunicationTime, accountTimezone);
                device.LastCommunicationTime = Helpers.ConvertToUserTime(device.LastCommunicationTime, offSet);
            }
            return data;
        }

        /// <summary>
        /// Get data with pagination
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public IQueryable<IcuDevice> GetPaginatedDeviceValid(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered, List<int> companyId, List<int> connectionStatus, List<int> deviceType)
        {
            var allData = _unitOfWork.AppDbContext.IcuDevice
                .Include(m => m.ActiveTz)
                .Include(m => m.PassageTz)
                .Include(m => m.Building)
                .Include(m => m.Company)
                .Where(m => !m.IsDeleted && m.Status == (short)Status.Valid);

            if (_httpContext.User.GetAccountType() != (short)AccountType.SystemAdmin)
            {
                allData = allData.Where(m => m.CompanyId == _httpContext.User.GetCompanyId());
            }

            totalRecords = allData.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().RemoveDiacritics().ToLower();
                allData = allData.Where(x => x.Name.RemoveDiacritics().ToLower().Contains(filter)
                                       || x.DeviceAddress.RemoveDiacritics().ToLower().Contains(filter));
            }

            if (companyId.Count() > 0)
            {
                allData = allData.Where(x => companyId.Contains(Convert.ToInt32(x.CompanyId)));
            }
            if (connectionStatus.Count() > 0)
            {
                allData = allData.Where(x => connectionStatus.Contains(Convert.ToInt32(x.ConnectionStatus)));
            }
            if (deviceType.Count() > 0)
            {
                allData = allData.Where(x => deviceType.Contains(Convert.ToInt32(x.DeviceType)));
            }

            recordsFiltered = allData.Count();
            allData = allData.OrderBy($"{sortColumn} {sortDirection}");
            allData = allData.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var accountTimezone = _unitOfWork.AccountRepository.Get(m =>
                    m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted).TimeZone;
            var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

            foreach (var device in allData)
            {
                //device.LastCommunicationTime = Helpers.ConvertToUserTime(device.LastCommunicationTime, accountTimezone);
                device.LastCommunicationTime = Helpers.ConvertToUserTime(device.LastCommunicationTime, offSet);
            }

            return allData;
        }

        public object ListFilterDeviceMonitoring()
        {
            try
            {
                // List company
                var lstCompany = _unitOfWork.CompanyRepository.Gets(x => x.IsDeleted == false).Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToList();

                // List connection status
                var lstConnection = EnumHelper.ToEnumList<ConnectionStatus>();

                // List device type
                var lstDeviceType = EnumHelper.ToEnumList<DeviceType>();

                // List operation type
                var lstOperationType = EnumHelper.ToEnumList<OperationType>();

                var data = new
                {
                    listCompany = lstCompany,
                    listConnection = lstConnection,
                    listDeviceType = lstDeviceType,
                    listOperationType = lstOperationType
                };
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ListFilterDeviceMonitoring");
                return null;
            }
        }
        public object getInit()
        {
            try
            {
                var accountType = _httpContext.User.GetAccountType();
                // List company
                var lstCompany = _unitOfWork.CompanyRepository.Gets(x => x.IsDeleted == false).Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToList();
                if(accountType == (short)AccountType.SystemAdmin || accountType == (short)AccountType.SuperAdmin)
                {
                }
                else
                {
                    lstCompany = _unitOfWork.CompanyRepository.Gets(x => false).Select(x => new
                    {
                        x.Id,
                        x.Name
                    }).ToList();

                }

                // List connection status
                var lstConnection = EnumHelper.ToEnumList<ConnectionStatus>();

                // List device type
                var lstDeviceType = EnumHelper.ToEnumList<DeviceType>();

                // List operation type
                var lstOperationType = EnumHelper.ToEnumList<OperationType>();

                // List verify mode
                var lstVerifyMode = EnumHelper.ToEnumList<VerifyMode>();

                var data = new
                {
                    listCompany = lstCompany,
                    listConnection = lstConnection,
                    listDeviceType = lstDeviceType,
                    listOperationType = lstOperationType,
                    lstVerifyMode = lstVerifyMode,
                };
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in getInit");
                return null;
            }
        }
        public int CountRegisteredIdByDeviceId(int deviceId)
        {
            try
            {
                var device = _unitOfWork.IcuDeviceRepository.GetByIcuId(deviceId);
                if (device.CompanyId == null)
                {
                    return 0;
                }

                return _unitOfWork.CardRepository.GetCardAvailableInDevice(deviceId).Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CountRegisteredIdByDeviceId");
                return 0;
            }
        }

        public void UpdateDeviceStatus(IcuDevice device, bool status)
        {
            var actionType = ActionLogType.InvalidDoor;
            if (status)
            {
                device.Status = (short)Status.Valid;
                actionType = ActionLogType.ValidDoor;
            }
            else
            {
                device.Status = (short)Status.Invalid;
            }

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        _unitOfWork.IcuDeviceRepository.Update(device);

                        //Save system log
                        var content = device.Status == (short)Status.Valid
                        ? string.Format(DeviceResource.msgValidDoor, device.Name)
                        : string.Format(DeviceResource.msgInvalidDoor, device.Name);
                        _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceSetting, actionType,
                            content, null, null, device.CompanyId ?? 0);

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
            if (device.DeviceType != (short)DeviceType.DesktopApp)
            {
                var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
                deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                {
                    MsgId = Guid.NewGuid().ToString(),
                    DeviceId = device.Id,
                    DeviceAddress = device.DeviceAddress,
                    MessageType = Constants.Protocol.DeviceInstruction,
                    Command = Constants.CommandType.UpdateDeviceState
                });
                //if (actionType == ActionLogType.ValidDoor)
                //{
                //    Reinstall(new List<IcuDevice> { device }, true);
                //}
            }
        }

        /// <summary>
        /// Get devide by id and company
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IcuDevice GetByIdAndCompany(int id, int companyId)
        {
            try
            {
                if (companyId != 0)
                {
                    var devices = _unitOfWork.AppDbContext.IcuDevice
                    .Include(m => m.Company)
                    .Include(m => m.ActiveTz)
                    .Where(m => m.Id == id && !m.IsDeleted);

                    return devices.FirstOrDefault(m => m.CompanyId == companyId && !m.Company.IsDeleted);
                }
                else
                {
                    var devices = _unitOfWork.AppDbContext.IcuDevice
                    .Include(m => m.Company)
                    .Include(m => m.ActiveTz)
                    .Where(m => m.Id == id && !m.IsDeleted);

                    return devices.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdAndCompany");
                return null;
            }
        }

        public IcuDevice GetByDeviceAddress(string deviceAddress)
        {
            try
            {
                return _unitOfWork.AppDbContext.IcuDevice.Include(m => m.Company).FirstOrDefault(m => m.DeviceAddress.ToLower().Equals(deviceAddress.ToLower()) && !m.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByDeviceAddress");
                return null;
            }
        }

        public bool HasExistName(int id, string deviceAddress)
        {
            try
            {
                return _unitOfWork.AppDbContext.IcuDevice.Include(m => m.Company)
                    .Any(m => m.DeviceAddress.ToLower().Equals(deviceAddress.ToLower()) && !m.IsDeleted && m.Id != id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HasExistName");
                return false;
            }
        }

        public IcuDevice GetById(int id)
        {
            try
            {
                return _unitOfWork.AppDbContext.IcuDevice.FirstOrDefault(m => id == m.Id && !m.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return null;
            }
        }

        /// <summary>
        /// Get devide by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<IcuDevice> GetByIds(List<int> ids)
        {
            try
            {
                return _unitOfWork.AppDbContext.IcuDevice.Include(m => m.Company)
                    .Where(m =>
                        ids.Contains(m.Id) && !m.IsDeleted).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIds");
                return new List<IcuDevice>();
            }
        }

        /// <summary>
        /// Check if device address is exist
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool IsDeviceAddressExist(DeviceModel model)
        {
            try
            {
                return _unitOfWork.AppDbContext.IcuDevice.Include(m => m.Company).Any(m =>
                    m.DeviceAddress == model.DeviceAddress && m.Id != model.Id && !m.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsDeviceAddressExist");
                return false;
            }
        }

        /// <summary>
        /// Check Device type and verify mode
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool IsAbleDeviceVerify(DeviceModel model)
        {
            try
            {
                switch (model.DeviceType)
                {
                case (int)DeviceType.Icu300N:
                case (int)DeviceType.Icu300NX:
                case (int)DeviceType.Icu400:
                    if (new List<int>() { (int) SingleVerify.Card,
                                        (int) SingleVerify.QR,
                                        (int) SingleVerify.PassCode,
                                        (int) SingleVerifyOR.CardOrQR,
                                        (int) SingleVerifyOR.CardOrPassCode,
                                        (int) MultiVerify.CardAndQR,
                                        (int) MultiVerify.CardAndPassCode }.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.ITouchPop:
                    if (new List<int>() { (int) SingleVerify.Card,
                                        (int) SingleVerify.PassCode,
                                        (int) SingleVerifyOR.CardOrPassCode,
                                        (int) MultiVerify.CardAndPassCode }.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.ITouchPopX:
                case (int)DeviceType.PM85:
                    if (new List<int>() { (int) SingleVerify.Card,
                                        (int) SingleVerify.QR,
                                        (int) SingleVerify.PassCode,
                                        (int) SingleVerifyOR.CardOrQR,
                                        (int) SingleVerifyOR.CardOrPassCode,
                                        (int) SingleVerifyOR.CardOrQROrPassCode,
                                        (int) MultiVerify.CardAndQR,
                                        (int) MultiVerify.CardAndPassCode }.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.DQMiniPlus:
                    if (new List<int>() { (int) SingleVerify.Card,
                                        (int) SingleVerify.QR,
                                        (int) SingleVerifyOR.CardOrQR,
                                        (int) MultiVerify.CardAndQR}.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.IT100:
                    if (new List<int>() { (int)SingleVerify.Card,
                                          (int)SingleVerify.Face,
                                          (int)SingleVerifyOR.FaceOrIris,
                                          (int)SingleVerifyOR.CardOrFaceOrIris,
                                          /*(int)MultiVerify.FaceAndIris,*/}.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.DesktopApp:
                    return true;
                case (int)DeviceType.NexpaLPR:
                    return true;
                case (int)DeviceType.FV6000:
                case (int)DeviceType.XStation2:
                    return true;
                case (int)DeviceType.Biostation2:
                    if (new List<int>() { (int) SingleVerify.Card,
                            (int) SingleVerify.FingerprintOnly,
                            (int) SingleVerifyOR.CardOrFingerprint,
                            (int) MultiVerify.CardAndPassCode,
                            (int) MultiVerify.CardAndFingerprint,
                            (int) MultiVerify.FingerprintAndPassCode}.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.Biostation3:
                    if (new List<int>() { (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) MultiVerify.CardAndFace,
                            (int) MultiVerify.CardAndPassCode,
                            (int) MultiVerify.CardAndFaceAndPassCode,
                            (int) MultiVerify.FaceAndPassCode,
                        }.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.EbknReader:
                    if (new List<int>() { (int) SingleVerify.Face,
                            (int) SingleVerify.FingerprintOnly,
                            (int) MultiVerify.FingerprintAndFace,
                            (int) MultiVerify.FaceAndPassCode,
                            (int) SingleVerifyOR.FaceOrFingerprintOrPassCode,
                            (int) MultiVerify.FingerprintAndPassCode,
                        }.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.BA8300:
                    if (new List<int>() { (int) SingleVerify.Face,
                            (int) SingleVerify.FingerprintOnly,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerifyOR.CardOrFingerprint,
                            (int) SingleVerifyOR.CardOrQR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) MultiVerify.PlateNumberAndVNID,
                        }.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.DF970:
                    if (new List<int>() { (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerify.Vehicle,
                            (int) SingleVerify.VNID,
                            (int) SingleVerifyOR.CardOrQR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) MultiVerify.VehicleAndFace,
                            (int) MultiVerify.PlateNumberAndVNID,
                        }.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.Icu500:
                    if (new List<int>() { (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.Vehicle,
                            (int) SingleVerify.VNID,
                            (int) SingleVerify.QR,
                            (int) SingleVerifyOR.CardOrQR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) MultiVerify.PlateNumberAndVNID,
                        }.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.T2Face:
                    if (new List<int>() { (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerify.VNID,
                            (int) SingleVerifyOR.CardOrQR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                        }.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.RA08:
                    if (new List<int>() { (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerify.Face,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) MultiVerify.PlateNumberAndVNID,
                        }.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.DQ8500:
                    if (new List<int>() { (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerify.Face,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) MultiVerify.PlateNumberAndVNID,
                        }.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.DQ200:
                    if (new List<int>() { (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerify.Face,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                            (int) MultiVerify.PlateNumberAndVNID,
                        }.Contains(model.VerifyMode)) { return true; }
                    break;
                case (int)DeviceType.TBVision:
                    if (new List<int>() { (int) SingleVerify.Face,
                            (int) SingleVerify.Card,
                            (int) SingleVerify.QR,
                            (int) SingleVerifyOR.CardOrQrOrFingerprintOrFaceId,
                        }.Contains(model.VerifyMode)) { return true; }
                    break;
                default:
                    if (new List<int>() { (int)SingleVerify.Card }.Contains(model.VerifyMode)) { return true; }
                    break;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsAbleDeviceVerify");
                return false;
            }
        }

        /// <summary>
        /// Get list of device by ids and company
        /// </summary>
        /// <param name="idArr"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<IcuDevice> GetByIdsAndCompany(List<int> idArr, int companyId, bool isAll = false)
        {
            try
            {
                IQueryable<IcuDevice> devices;

                if (isAll)
                {
                    devices = _unitOfWork.IcuDeviceRepository.GetDeviceAllInfoByCompany(companyId).Where(m => idArr.Contains(m.Id));
                }
                else
                {
                    devices = _unitOfWork.IcuDeviceRepository.GetDevicesByCompany(companyId).Where(m => idArr.Contains(m.Id));
                }

                if (companyId != 0)
                {
                    return devices.Where(m => m.CompanyId == companyId && !m.Company.IsDeleted).ToList();
                }
                else
                {
                    return devices.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdsAndCompany");
                return new List<IcuDevice>();
            }
        }

        /// <summary>
        /// Get list of device by rids and company
        /// </summary>
        /// <param name="ridArr"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<IcuDevice> GetByRidsAndCompany(List<string> ridArr, int companyId)
        {
            try
            {
                var devices = _unitOfWork.AppDbContext.IcuDevice.Include(m => m.Company).Include(m => m.ActiveTz).Include(m => m.Building)
                   .Where(m =>
                       ridArr.Contains(m.DeviceAddress) && !m.IsDeleted);

                if (companyId != 0)
                {
                    return devices.Where(m => m.CompanyId == companyId && !m.Company.IsDeleted).ToList();
                }
                else
                {
                    return devices.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByRidsAndCompany");
                return new List<IcuDevice>();
            }
        }
        public List<IcuDevice> GetByCompanyId(int companyId)
        {
            try
            {
                var devices = _unitOfWork.AppDbContext.IcuDevice.Include(m => m.Company).Include(m => m.ActiveTz).Include(m => m.Building)
                    .Where(m => !m.IsDeleted);

                if (companyId != 0)
                {
                    return devices.Where(m => m.CompanyId == companyId && !m.Company.IsDeleted).ToList();
                }
                else
                {
                    return devices.ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCompanyId");
                return new List<IcuDevice>();
            }
        }

        /// <summary>
        /// Generate icu devices
        /// </summary>
        /// <param name="numberOfDevice"></param>
        /// <returns></returns>
        public List<IcuDevice> GenerateTestData(int numberOfDevice)
        {
            var icuDevices = new List<IcuDevice>();
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var random = new Random();
                        for (var i = 0; i < numberOfDevice; i++)
                        {
                            var icuAddress = new string(Enumerable.Repeat(Constants.HexNumber, 6)
                                .Select(s => s[random.Next(s.Length)]).ToArray());
                            var fakeIcu = new Faker<IcuDevice>()
                                .RuleFor(u => u.DeviceAddress, f => icuAddress)
                                .RuleFor(u => u.Name, f => icuAddress)
                                .RuleFor(u => u.CompanyId, f => 1)
                                .RuleFor(u => u.BuildingId, f => 1)
                                .RuleFor(u => u.ActiveTzId, f => 1)
                                .RuleFor(u => u.LedReader0, f => (short)1)
                                .RuleFor(u => u.LedReader1, f => (short)1)
                                .RuleFor(u => u.IpAddress, (f, u) => f.Internet.Ip());

                            var icuDevice = fakeIcu.Generate();
                            _unitOfWork.IcuDeviceRepository.Add(icuDevice);
                            _unitOfWork.Save();

                            icuDevices.Add(icuDevice);
                        }
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
            return icuDevices;
        }

        /// <summary>
        /// Publish a message to get device info
        /// <param name="deviceAddress"></param>
        /// <param name="groupMsgId"></param>
        /// <param name="groupIndex"></param>
        /// <param name="groupLength"></param>
        /// <param name="processId"></param>
        /// </summary>
        public void SendDeviceInfo(string deviceAddress)
        {
            string sender = _httpContext.User.GetUsername();
            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
            deviceInstructionQueue.SendLoadDeviceInfo(new LoadDeviceInfoQueueModel()
            {
                MsgId = Guid.NewGuid().ToString(),
                DeviceAddress = deviceAddress,
                MessageType = Constants.Protocol.LoadDeviceInfo,
                Sender = sender,
            });
        }
        
        /// <summary>
        /// Notify to unbind device
        /// </summary>
        /// <param name="deviceAddress"></param>
        internal void UnBindDeviceFromQueue(string deviceAddress)
        {
            // var icuAddress = new IcuAddress
            // {
            //     Address = deviceAddress
            // };
            // if (_connection != null && _connection.IsOpen)
            // {
            //     using (var channel = _connection.CreateModel())
            //     {
            //         channel.QueueDelete(queueName);
            //     }
            // }
        }

        public void RequestBackup()
        {
            var devices = _unitOfWork.IcuDeviceRepository.GetDevicesByCompany(_httpContext.User.GetCompanyId()).ToList();

            foreach (var device in devices)
            {
                var deviceRoutingKey = $"{Constants.RabbitMq.EventLogTopic}.{device.DeviceAddress}";
                if (_connection != null && _connection.IsOpen)
                {
                    using (var channel = _connection.CreateModel())
                    {
                        channel.QueueDelete(deviceRoutingKey);
                    }
                }
            }
        }
        
        public void SendDeviceConfig(IcuDevice device, string protocolType)
        {
            string sender = _httpContext?.User.GetUsername() ?? Constants.RabbitMq.SenderDefault;
            DeviceInstructionQueue deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
            // Get setting
            bool useStaticQrCode = false;
            if (device.CompanyId.HasValue)
            {
                var setting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.UseStaticQrCode, device.CompanyId.Value);
                bool.TryParse(Helpers.GetStringFromValueSetting(setting?.Value), out useStaticQrCode);
            }
            
            deviceInstructionQueue.SendDeviceConfig(new ConfigQueueModel()
            {
                DeviceAddress = device.DeviceAddress,
                DeviceId = device.Id,
                MessageType = Constants.Protocol.UpdateDeviceConfig,
                MsgId = Guid.NewGuid().ToString(),
                Sender = sender,
                UseStaticQrCode = useStaticQrCode
            });
        }
        

        /// <summary>
        /// Send device instruction with multiple device
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="model"></param>
        public int SendInstruction(List<IcuDevice> devices, DeviceInstruction model)
        {
            int errorDeviceCnt = 0;
            string groupMsgId = Helpers.GenerateGroupMsgId();
            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
            string sender = _httpContext?.User?.GetUsername() ?? Constants.RabbitMq.SenderDefault;

            foreach (var device in devices)
            {
                if (model.Command == Constants.CommandType.SetTime)
                {
                    // Send set time message
                    deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                    {
                        DeviceAddress = device.DeviceAddress,
                        DeviceId = device.Id,
                        MessageType = Constants.Protocol.DeviceInstruction,
                        MsgId = groupMsgId,
                        Sender = sender,
                        MessageIndex = 1,
                        MessageTotal = 2,
                        IsSaveSystemLog = true,
                        Command = Constants.CommandType.SetTime,
                        UserName = sender,
                    });
                    // Send Device Info
                    deviceInstructionQueue.SendLoadDeviceInfo(new LoadDeviceInfoQueueModel()
                    {
                        DeviceAddress = device.DeviceAddress,
                        MessageType = Constants.Protocol.LoadDeviceInfo,
                        MsgId = groupMsgId,
                        Sender = sender,
                        MessageIndex = 2,
                        MessageTotal = 2,
                    });
                }
                else
                {
                    // Send device instruction
                    deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                    {
                        DeviceAddress = device.DeviceAddress,
                        DeviceId = device.Id,
                        MessageType = Constants.Protocol.DeviceInstruction,
                        MsgId = groupMsgId,
                        Sender = sender,
                        IsSaveSystemLog = true,
                        Command = model.Command,
                        UserName = sender,
                        OpenPeriod = model.OpenPeriod,
                        OpenUtilTime = string.IsNullOrEmpty(model.OpenUntilTime)
                            ? DateTime.MinValue
                            : DateTime.ParseExact(model.OpenUntilTime, Constants.DateTimeFormat.ddMMyyyyHHmmsszzz, null),
                        LocalMqtt = model.LocalMqtt,
                    });
                }
            }
            return errorDeviceCnt;
        }

        /// <summary>
        /// Assign a door to full access group
        /// </summary>
        /// <param name="doorId"></param>
        /// <param name="tzId"></param>
        public void AssignToFullAccessGroup(int doorId, int companyId)
        {
            var fullAccessGroup =
                _unitOfWork.AccessGroupRepository.GetFullAccessGroup(companyId);

            var tzId = _unitOfWork.AccessTimeRepository.GetByPositionAndCompany(Constants.Tz24hPos, companyId).Id;

            var accessGroupId = fullAccessGroup.Id;
            var assignDoorDetail = new AccessGroupAssignDoorDetail
            {
                DoorId = doorId,
                TzId = tzId,
                CompanyId = companyId
            };
            var assignDoor = new AccessGroupAssignDoor
            {
                Doors = new List<AccessGroupAssignDoorDetail> { assignDoorDetail }
            };
            _accessGroupService.AssignDoors(accessGroupId, assignDoor, false);
        }


        /// <summary>
        /// Check if door has a timezone
        /// </summary>
        /// <param name="timezoneId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public bool HasTimezone(int timezoneId, int companyId)
        {
            var result = false;
            var isUseForDoor = _unitOfWork.IcuDeviceRepository.HasTimezone(timezoneId, companyId);
            var isUseForAg = _unitOfWork.AccessGroupDeviceRepository.HasTimezone(timezoneId);
            if (isUseForDoor || isUseForAg)
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Get valid door by company ( Logged in account )
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IcuDevice> GetDoorList()
        {
            var companyId = _httpContext.User.GetCompanyId();
            var devices = _unitOfWork.AppDbContext.IcuDevice.Where(m => m.Status == (short)Status.Valid).OrderBy(c => c.Name);

            if (companyId != 0)
            {
                devices = devices.Where(m => m.CompanyId == companyId).OrderBy(c => c.Name);
            }

            return devices;
        }
        
        public void Reinstall(List<IcuDevice> devices, bool isAddDevice = false, List<ReinstallDeviceDetail> onlineDeviceDetails = null)
        {
            try
            {
                List<short> messageDeviceTypes = Helpers.GetDisplayDevices();
                var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
                var accessControlQueue = new AccessControlQueue(_unitOfWork, _webSocketService);
                string sender = _httpContext.User.GetUsername();
                foreach (var device in devices)
                {
                    if (device.CompanyId == null)
                    {
                        _logger.LogError("Device can't do it until Device is assigned to the company.");
                        continue;
                    }

                    var company = _unitOfWork.CompanyRepository.GetById(device.CompanyId.Value);
                    if (company != null && !company.AutoSyncUserData && device.ConnectionStatus != (short)ConnectionStatus.Online)
                    {
                        continue;
                    }
                    
                    // init parameters
                    var processId = TransmitDataModel.GetProcessIdFromDeviceId(onlineDeviceDetails, device.Id) ?? Guid.NewGuid().ToString();
                    
                    // Calculate total number of message should be sent (for progress bar)
                    int groupLength = 0;
                    groupLength += 1; // get load device info - the last message
                    int lenghtMessageUserInfo = 0;
                    // current time
                    groupLength += 1;
                    // device setting
                    groupLength += 1;
                    // timezone setting
                    var timezones = _unitOfWork.AccessTimeRepository.GetByCompany(device.CompanyId);
                    groupLength += timezones.Count;
                    // holiday setting
                    groupLength += 1;
                    // user info
                    var countCard = CountRegisteredIdByDeviceId(device.Id);
                    int maxSplitMessage = Helpers.GetMaxSplit(device.DeviceType);
                    lenghtMessageUserInfo = countCard / maxSplitMessage;
                    lenghtMessageUserInfo = lenghtMessageUserInfo * maxSplitMessage < countCard ? lenghtMessageUserInfo + 1 : lenghtMessageUserInfo;
                    groupLength += lenghtMessageUserInfo;
                    // access group
                    if (device.DeviceType == (short)DeviceType.Biostation2 || device.DeviceType == (short)DeviceType.Biostation3)
                    {
                        groupLength += 1;
                    }
                    // delete all events
                    groupLength += 1;
                    // delete all users
                    groupLength += 1;
                    // update user data
                    if (!(device.DeviceType == (short)DeviceType.Icu300N
                          || device.DeviceType == (short)DeviceType.Icu300NX
                          || device.DeviceType == (short)DeviceType.DesktopApp
                          || device.DeviceType == (short)DeviceType.ITouchPop
                          || device.DeviceType == (short)DeviceType.NexpaLPR
                          || device.DeviceType == (short)DeviceType.FV6000
                          || device.DeviceType == (short)DeviceType.XStation2))
                    {
                        groupLength += 1;
                    }
                    // device messages
                    if (messageDeviceTypes.Contains(device.DeviceType))
                    {
                        groupLength += 1;
                    }
                    
                    // send timezones
                    int currentIndex = 1;
                    if (timezones.Any())
                    {
                        deviceInstructionQueue.SendUpdateTimezone(new UpdateTimezoneQueueModel()
                        {
                            MsgId = processId,
                            DeviceId = device.Id,
                            DeviceAddress = device.DeviceAddress,
                            Sender = sender,
                            MessageType = Constants.Protocol.UpdateTimezone,
                            MessageIndex = currentIndex,
                            MessageTotal = groupLength,
                        });
                        currentIndex += timezones.Count;
                    }
                    
                    // Send all holiday
                    deviceInstructionQueue.SendHoliday(new HolidayQueueModel()
                    {
                        DeviceAddress = device.DeviceAddress,
                        DeviceId = device.Id,
                        MessageType = Constants.Protocol.UpdateHoliday,
                        MsgId = processId,
                        Sender = sender,
                        MessageIndex = currentIndex,
                        MessageTotal = groupLength,
                    });
                    currentIndex += 1;
                    // Get setting
                    bool useStaticQrCode = false;
                    if (device.CompanyId.HasValue)
                    {
                        var setting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.UseStaticQrCode, device.CompanyId.Value);
                        bool.TryParse(Helpers.GetStringFromValueSetting(setting?.Value), out useStaticQrCode);
                    }

                    // Send update device config
                    deviceInstructionQueue.SendDeviceConfig(new ConfigQueueModel()
                    {
                        DeviceAddress = device.DeviceAddress,
                        DeviceId = device.Id,
                        MessageType = Constants.Protocol.UpdateDeviceConfig,
                        MsgId = processId,
                        Sender = sender,
                        MessageIndex = currentIndex,
                        MessageTotal = groupLength,
                        UseStaticQrCode = useStaticQrCode
                    });
                    currentIndex += 1;
                    
                    // Send delete all user
                    deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                    {
                        DeviceAddress = device.DeviceAddress,
                        DeviceId = device.Id,
                        MessageType = Constants.Protocol.DeviceInstruction,
                        MsgId = processId,
                        Sender = sender,
                        Command = Constants.CommandType.DeleteAllUsers,
                        UserName = sender,
                        MessageIndex = currentIndex,
                        MessageTotal = groupLength,
                    });
                    currentIndex += 1;
                    
                    // Send delete all event
                    deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                    {
                        DeviceAddress = device.DeviceAddress,
                        DeviceId = device.Id,
                        MessageType = Constants.Protocol.DeviceInstruction,
                        MsgId = processId,
                        Sender = sender,
                        Command = Constants.CommandType.DeleteAllEvents,
                        UserName = sender,
                        MessageIndex = currentIndex,
                        MessageTotal = groupLength,
                    });
                    currentIndex += 1;

                    // Send assign user
                    if (countCard > 0)
                    {
                        accessControlQueue.SendUserInfo(new UserInfoQueueModel()
                        {
                            DeviceAddress = device.DeviceAddress,
                            DeviceId = device.Id,
                            MessageType = Constants.Protocol.AddUser,
                            MsgId = processId,
                            Sender = sender,
                            UserIds = null,
                            VisitIds = null,
                            MessageIndex = currentIndex,
                            MessageTotal = groupLength,
                        });
                        currentIndex += lenghtMessageUserInfo;
                    }

                    if (!(device.DeviceType == (short)DeviceType.Icu300N
                        || device.DeviceType == (short)DeviceType.Icu300NX
                        || device.DeviceType == (short)DeviceType.DesktopApp
                        || device.DeviceType == (short)DeviceType.ITouchPop
                        || device.DeviceType == (short)DeviceType.NexpaLPR
                        || device.DeviceType == (short)DeviceType.FV6000
                        || device.DeviceType == (short)DeviceType.XStation2))
                    {
                        // Send Update all user
                        deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                        {
                            DeviceAddress = device.DeviceAddress,
                            DeviceId = device.Id,
                            MessageType = Constants.Protocol.DeviceInstruction,
                            MsgId = processId,
                            Sender = sender,
                            Command = Constants.CommandType.UpdateAllUsers,
                            UserName = sender,
                            MessageIndex = currentIndex,
                            MessageTotal = groupLength,
                        });
                        currentIndex += 1;
                    }
                    
                    // Send Device Info
                    deviceInstructionQueue.SendLoadDeviceInfo(new LoadDeviceInfoQueueModel()
                    {
                        MsgId = processId,
                        DeviceAddress = device.DeviceAddress,
                        MessageType = Constants.Protocol.LoadDeviceInfo,
                        Sender = sender,
                    });
                    currentIndex += 1;
                    
                    // get device info
                    deviceInstructionQueue.SendLoadDeviceInfo(new LoadDeviceInfoQueueModel()
                    {
                        DeviceAddress = device.DeviceAddress,
                        MsgId = processId,
                        Sender = sender,
                        MessageIndex = currentIndex,
                        MessageTotal = groupLength,
                        MessageType = Constants.Protocol.LoadDeviceInfo,
                    });
                    currentIndex += 1;
                    
                    // Save System Log
                    if (!isAddDevice)
                    {
                        // This case(!isAddDevice) is for reinstall, not install.
                        var content = DeviceResource.msgReinstallDeviceSetting;
                        List<string> details = new List<string>()
                        {
                            $"{DeviceResource.lblDeviceAddress} : {device.DeviceAddress}",
                            $"{DeviceResource.lblDoorName} : {device.Name}"
                        };
                        var contentsDetails = string.Join("\n", details);
                        _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceSetting, ActionLogType.Reinstall,content, contentsDetails, null, device.CompanyId);
                        _unitOfWork.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Copy the devices
        /// </summary>
        /// <param name="deviceCopy"></param>
        /// <param name="devices"></param>
        public void CopyDevices(IcuDevice deviceCopy, List<IcuDevice> devices)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var device in devices)
                        {
                            // TODO update device attribute
                            device.DeviceType = deviceCopy.DeviceType;
                            device.VerifyMode = deviceCopy.VerifyMode;
                            device.BackupPeriod = deviceCopy.BackupPeriod;

                            if (deviceCopy.Company != null && deviceCopy.CompanyId != null)
                            {
                                device.Company = deviceCopy.Company;
                                device.CompanyId = deviceCopy.CompanyId;
                            }
                            device.BuildingId = deviceCopy.BuildingId;
                            device.ActiveTzId = deviceCopy.ActiveTzId;
                            device.PassageTzId = deviceCopy.PassageTzId;

                            device.OperationType = deviceCopy.OperationType;

                            device.RoleReader0 = deviceCopy.RoleReader0;
                            device.RoleReader1 = deviceCopy.RoleReader1;

                            device.LedReader0 = deviceCopy.LedReader0;
                            device.LedReader1 = deviceCopy.LedReader1;

                            device.BuzzerReader0 = deviceCopy.BuzzerReader0;
                            device.BuzzerReader1 = deviceCopy.BuzzerReader1;

                            device.UseCardReader = deviceCopy.UseCardReader;

                            device.DeviceBuzzer = deviceCopy.DeviceBuzzer;

                            device.SensorType = deviceCopy.SensorType;
                            device.OpenDuration = deviceCopy.OpenDuration;
                            device.SensorDuration = deviceCopy.SensorDuration;
                            device.SensorAlarm = deviceCopy.SensorAlarm;
                            device.CloseReverseLockFlag = deviceCopy.CloseReverseLockFlag;
                            device.PassbackRule = deviceCopy.PassbackRule;

                            device.MPRCount = deviceCopy.MPRCount;
                            device.MPRInterval = deviceCopy.MPRInterval;

                            device.Status = deviceCopy.Status;

                            _unitOfWork.IcuDeviceRepository.Update(device);

                            //Save system log
                            var content = string.Format(DeviceResource.msgCopyDeviceSetting, deviceCopy.Name, device.Name);

                            var contentsDetails = $"{DeviceResource.lblDoorName} : {deviceCopy.Name} ({DeviceResource.lblDeviceAddress} : {deviceCopy.DeviceAddress})\n" +
                                                $"{DeviceResource.lblDoorName} : {device.Name} ({DeviceResource.lblDeviceAddress} : {device.DeviceAddress})";

                            _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.DeviceSetting, ActionLogType.CopyDoorSetting,
                                content, contentsDetails, null, device.CompanyId);

                            _unitOfWork.Save();
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
            
            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
            string sender = _httpContext.User.GetUsername();
            foreach (var device in devices)
            {
                // Get setting
                bool useStaticQrCode = false;
                if (device.CompanyId.HasValue)
                {
                    var setting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.UseStaticQrCode, device.CompanyId.Value);
                    bool.TryParse(Helpers.GetStringFromValueSetting(setting?.Value), out useStaticQrCode);
                }
                deviceInstructionQueue.SendDeviceConfig(new ConfigQueueModel()
                {
                    DeviceAddress = device.DeviceAddress,
                    DeviceId = device.Id,
                    MessageType = Constants.Protocol.UpdateDeviceConfig,
                    MsgId = Guid.NewGuid().ToString(),
                    Sender = sender,
                    UseStaticQrCode = useStaticQrCode
                });
            }
        }

        /// <summary>
        /// Get device type list
        /// </summary>
        /// <returns></returns>
        public DeviceTypeList GetListDeviceType(List<IcuDevice> devices)
        {
            DeviceTypeModel listIcuDeviceTypeModel = new DeviceTypeModel();
            DeviceTypeModel listITouchDeviceTypeModel = new DeviceTypeModel();

            var listDeviceType = new List<DeviceTypeModel>();

            FileDetail icuMainType = null,
                    iTouchMainType = null;

            foreach (var device in devices)
            {
                // When device type is icu300N
                if (device.DeviceType == (short)DeviceType.Icu300N 
                    || device.DeviceType == (short)DeviceType.Icu300NX)
                {
                    listIcuDeviceTypeModel.Name = ((DeviceType)device.DeviceType).GetDescription();
                    if (icuMainType == null)
                    {
                        icuMainType = new FileDetail
                        {
                            //IcuIds = icuIds,
                            Target = DeviceUpdateTarget.Icu300N.GetDescription(),
                            Remark = "ICU firmware(.bin)"
                        };
                        icuMainType.IcuIds.Add(device.Id);
                        listIcuDeviceTypeModel.FileList.Add(icuMainType);
                    }
                    else
                    {
                        var indexOf = listIcuDeviceTypeModel.FileList.IndexOf(listIcuDeviceTypeModel.FileList.Find(x =>
                            x.Target == DeviceUpdateTarget.Icu300N.GetDescription()));
                        if (!listIcuDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listIcuDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }

                    var readerVersion0 = !string.IsNullOrEmpty(device.VersionReader0) ? device.VersionReader0.Split("_")[0] : string.Empty;

                    // reader0
                    if (!string.IsNullOrEmpty(readerVersion0))
                    {
                        var filedetail = listIcuDeviceTypeModel.FileList.Find(x =>
                            x.Target == device.VersionReader0);
                        if (filedetail == null)
                        {
                            var newFileDetail = new FileDetail
                            {
                                Target = readerVersion0,
                                Remark = "Reader firmware(.bin)"
                            };
                            newFileDetail.IcuIds.Add(device.Id);
                            listIcuDeviceTypeModel.FileList.Add(newFileDetail);
                        }
                        else
                        {
                            var indexOf = listIcuDeviceTypeModel.FileList.IndexOf(filedetail);
                            if (!listIcuDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                            {
                                listIcuDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                            }
                        }
                    }

                    var readerVersion1 = !string.IsNullOrEmpty(device.VersionReader1) ? device.VersionReader1.Split("_")[0] : string.Empty;

                    // reader1
                    // If reader1 is same as reader0, don't send the reader1 to list
                    if (!string.IsNullOrEmpty(readerVersion1) && !readerVersion1.Equals(readerVersion0))
                    {
                        var outCardReader = listIcuDeviceTypeModel.FileList.Find(x =>
                            x.Target == device.VersionReader1);
                        if (outCardReader == null)
                        {
                            var newOutReader = new FileDetail
                            {
                                Target = readerVersion1,
                                Remark = "Reader firmware(.bin)"
                            };
                            newOutReader.IcuIds.Add(device.Id);
                            listIcuDeviceTypeModel.FileList.Add(newOutReader);
                        }
                        else
                        {
                            var indexOf = listIcuDeviceTypeModel.FileList.IndexOf(outCardReader);
                            if (!listIcuDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                            {
                                listIcuDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                            }
                        }
                    }
                }
                if (device.DeviceType == (short)DeviceType.Icu400)
                {
                    listIcuDeviceTypeModel.Name = ((DeviceType)device.DeviceType).GetDescription();
                    if (icuMainType == null)
                    {
                        icuMainType = new FileDetail
                        {
                            //IcuIds = icuIds,
                            Target = DeviceUpdateTarget.Icu400.GetDescription(),
                            Remark = "ICU firmware(.bin)"
                        };
                        icuMainType.IcuIds.Add(device.Id);
                        listIcuDeviceTypeModel.FileList.Add(icuMainType);
                    }
                    else
                    {
                        var indexOf = listIcuDeviceTypeModel.FileList.IndexOf(listIcuDeviceTypeModel.FileList.Find(x =>
                            x.Target == DeviceUpdateTarget.Icu400.GetDescription()));
                        if (!listIcuDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listIcuDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }

                    var readerVersion0 = !string.IsNullOrEmpty(device.VersionReader0) ? device.VersionReader0.Split("_")[0] : string.Empty;

                    // reader0
                    if (!string.IsNullOrEmpty(readerVersion0))
                    {
                        var filedetail = listIcuDeviceTypeModel.FileList.Find(x =>
                            x.Target == device.VersionReader0);
                        if (filedetail == null)
                        {
                            var newFileDetail = new FileDetail
                            {
                                Target = readerVersion0,
                                Remark = "Reader firmware(.bin)"
                            };
                            newFileDetail.IcuIds.Add(device.Id);
                            listIcuDeviceTypeModel.FileList.Add(newFileDetail);
                        }
                        else
                        {
                            var indexOf = listIcuDeviceTypeModel.FileList.IndexOf(filedetail);
                            if (!listIcuDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                            {
                                listIcuDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                            }
                        }
                    }

                    var readerVersion1 = !string.IsNullOrEmpty(device.VersionReader1) ? device.VersionReader1.Split("_")[0] : string.Empty;

                    // reader1
                    // If reader1 is same as reader0, don't send the reader1 to list
                    if (!string.IsNullOrEmpty(readerVersion1) && !readerVersion1.Equals(readerVersion0))
                    {
                        var outCardReader = listIcuDeviceTypeModel.FileList.Find(x =>
                            x.Target == device.VersionReader1);
                        if (outCardReader == null)
                        {
                            var newOutReader = new FileDetail
                            {
                                Target = readerVersion1,
                                Remark = "Reader firmware(.bin)"
                            };
                            newOutReader.IcuIds.Add(device.Id);
                            listIcuDeviceTypeModel.FileList.Add(newOutReader);
                        }
                        else
                        {
                            var indexOf = listIcuDeviceTypeModel.FileList.IndexOf(outCardReader);
                            if (!listIcuDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                            {
                                listIcuDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                            }
                        }
                    }

                    // Tar file
                    FileDetail tarFile = new FileDetail
                    {
                        Target = DeviceUpdateTarget.Tar.GetDescription(),
                        Remark = "Tar file (.tar or .zip)"
                    };
                    tarFile.IcuIds.Add(device.Id);
                    listIcuDeviceTypeModel.FileList.Add(tarFile);
                }
                // When device type is iTouchPop2A
                else if (device.DeviceType == (short)DeviceType.ITouchPop)
                {
                    listITouchDeviceTypeModel.Name = ((DeviceType)device.DeviceType).GetDescription();

                    // main firmware ( APK )
                    if (iTouchMainType == null)
                    {
                        iTouchMainType = new FileDetail
                        {
                            Target = DeviceUpdateTarget.ITouchPop2A.GetDescription(),
                            Remark = "ITouchPop2A firmware(.apk)"
                        };
                        iTouchMainType.IcuIds.Add(device.Id);
                        listITouchDeviceTypeModel.FileList.Add(iTouchMainType);
                    }
                    else
                    {
                        var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(listITouchDeviceTypeModel.FileList.Find(x =>
                            x.Target == DeviceUpdateTarget.ITouchPop2A.GetDescription()));
                        if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }

                    // RF module
                    if (!string.IsNullOrEmpty(device.NfcModuleVersion))
                    {
                        var nfcFileType = listITouchDeviceTypeModel.FileList.Find(x =>
                            x.Target == device.NfcModuleVersion);
                        if (nfcFileType == null)
                        {
                            var nfcAbcm = new FileDetail
                            {
                                Target = DeviceUpdateTarget.Abcm.GetDescription(),
                                Remark = "RF firmware(.bin)"
                            };
                            nfcAbcm.IcuIds.Add(device.Id);
                            listITouchDeviceTypeModel.FileList.Add(nfcAbcm);
                        }
                        else
                        {
                            var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(nfcFileType);
                            if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                            {
                                listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                            }
                        }
                    }

                    // Extra
                    if (!string.IsNullOrEmpty(device.ExtraVersion))
                    {
                        var extraAbcm = listITouchDeviceTypeModel.FileList.Find(x =>
                            x.Target == device.ExtraVersion);

                        if (extraAbcm == null)
                        {
                            extraAbcm = new FileDetail
                            {
                                Target = device.ExtraVersion,
                                Remark = "Some description"
                            };
                            extraAbcm.IcuIds.Add(device.Id);
                            listITouchDeviceTypeModel.FileList.Add(extraAbcm);
                        }
                        else
                        {
                            var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(extraAbcm);
                            if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                            {
                                listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                            }
                        }
                    }
                }
                // When device type is iTouch30A
                else if (device.DeviceType == (short)DeviceType.ITouch30A)
                {
                    listITouchDeviceTypeModel.Name = ((DeviceType)device.DeviceType).GetDescription();

                    // main firmware ( APK )
                    if (iTouchMainType == null)
                    {
                        iTouchMainType = new FileDetail
                        {
                            Target = DeviceUpdateTarget.ITouch30A.GetDescription(),
                            Remark = "ITouch30A firmware(.apk)"
                        };
                        iTouchMainType.IcuIds.Add(device.Id);
                        listITouchDeviceTypeModel.FileList.Add(iTouchMainType);
                    }
                    else
                    {
                        var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(listITouchDeviceTypeModel.FileList.Find(x =>
                            x.Target == DeviceUpdateTarget.ITouch30A.GetDescription()));
                        if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }

                    // RF module
                    if (!string.IsNullOrEmpty(device.NfcModuleVersion))
                    {
                        var nfcFileType = listITouchDeviceTypeModel.FileList.Find(x =>
                            x.Target == device.NfcModuleVersion);
                        if (nfcFileType == null)
                        {
                            var nfcAbcm = new FileDetail
                            {
                                Target = DeviceUpdateTarget.Abcm.GetDescription(),
                                Remark = "RF firmware(.bin)"
                            };
                            nfcAbcm.IcuIds.Add(device.Id);
                            listITouchDeviceTypeModel.FileList.Add(nfcAbcm);
                        }
                        else
                        {
                            var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(nfcFileType);
                            if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                            {
                                listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                            }
                        }
                    }

                    // Extra
                    if (!string.IsNullOrEmpty(device.ExtraVersion))
                    {
                        var extraAbcm = listITouchDeviceTypeModel.FileList.Find(x =>
                            x.Target == device.ExtraVersion);

                        if (extraAbcm == null)
                        {
                            extraAbcm = new FileDetail
                            {
                                Target = device.ExtraVersion,
                                Remark = "Some description"
                            };
                            extraAbcm.IcuIds.Add(device.Id);
                            listITouchDeviceTypeModel.FileList.Add(extraAbcm);
                        }
                        else
                        {
                            var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(extraAbcm);
                            if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                            {
                                listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                            }
                        }
                    }
                }
                // When device type is iTouchPopX
                else if (device.DeviceType == (short)DeviceType.ITouchPopX)
                {
                    listITouchDeviceTypeModel.Name = ((DeviceType)device.DeviceType).GetDescription();

                    // main firmware ( APK )
                    if (iTouchMainType == null)
                    {
                        iTouchMainType = new FileDetail
                        {
                            Target = DeviceUpdateTarget.ITouchPopX.GetDescription(),
                            Remark = "ITouchPopX firmware (.bin)"
                        };
                        iTouchMainType.IcuIds.Add(device.Id);
                        listITouchDeviceTypeModel.FileList.Add(iTouchMainType);
                    }
                    else
                    {
                        var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(listITouchDeviceTypeModel.FileList.Find(x =>
                            x.Target == DeviceUpdateTarget.ITouchPopX.GetDescription()));
                        if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }

                    // Module
                    var moduleFileType = listITouchDeviceTypeModel.FileList.Find(x =>
                        x.Target == device.NfcModuleVersion);
                    if (moduleFileType == null)
                    {
                        var module = new FileDetail
                        {
                            Target = DeviceUpdateTarget.Module.GetDescription(),
                            Remark = "Module (.ko)"
                        };
                        module.IcuIds.Add(device.Id);
                        listITouchDeviceTypeModel.FileList.Add(module);
                    }
                    else
                    {
                        var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(moduleFileType);
                        if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }

                    // Library
                    var library = listITouchDeviceTypeModel.FileList.Find(x =>
                        x.Target == device.ExtraVersion);

                    if (library == null)
                    {
                        library = new FileDetail
                        {
                            Target = DeviceUpdateTarget.Library.GetDescription(),
                            Remark = "Library (.so)"
                        };
                        library.IcuIds.Add(device.Id);
                        listITouchDeviceTypeModel.FileList.Add(library);
                    }
                    else
                    {
                        var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(library);
                        if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }

                    // Tar file
                    FileDetail tarFile = new FileDetail
                    {
                        Target = DeviceUpdateTarget.Tar.GetDescription(),
                        Remark = "Tar file (.tar or .zip)"
                    };
                    tarFile.IcuIds.Add(device.Id);
                    listITouchDeviceTypeModel.FileList.Add(tarFile);

                }
                // When device type is DP-636X
                else if (device.DeviceType == (short)DeviceType.DP636X)
                {
                    listITouchDeviceTypeModel.Name = ((DeviceType)device.DeviceType).GetDescription();

                    // main firmware ( APK )
                    if (iTouchMainType == null)
                    {
                        iTouchMainType = new FileDetail
                        {
                            Target = DeviceUpdateTarget.DP636X.GetDescription(),
                            Remark = "DP-636X firmware"
                        };
                        iTouchMainType.IcuIds.Add(device.Id);
                        listITouchDeviceTypeModel.FileList.Add(iTouchMainType);
                    }
                    else
                    {
                        var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(listITouchDeviceTypeModel.FileList.Find(x =>
                            x.Target == DeviceUpdateTarget.DP636X.GetDescription()));
                        if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }

                    // Module
                    var moduleFileType = listITouchDeviceTypeModel.FileList.Find(x =>
                        x.Target == device.NfcModuleVersion);
                    if (moduleFileType == null)
                    {
                        var module = new FileDetail
                        {
                            Target = DeviceUpdateTarget.Module.GetDescription(),
                            Remark = "Module(.ko)"
                        };
                        module.IcuIds.Add(device.Id);
                        listITouchDeviceTypeModel.FileList.Add(module);
                    }
                    else
                    {
                        var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(moduleFileType);
                        if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }

                    // Library
                    var library = listITouchDeviceTypeModel.FileList.Find(x =>
                        x.Target == device.ExtraVersion);

                    if (library == null)
                    {
                        library = new FileDetail
                        {
                            Target = DeviceUpdateTarget.Library.GetDescription(),
                            Remark = "Library(.so)"
                        };
                        library.IcuIds.Add(device.Id);
                        listITouchDeviceTypeModel.FileList.Add(library);
                    }
                    else
                    {
                        var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(library);
                        if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }

                    // Tar file
                    FileDetail tarFile = new FileDetail
                    {
                        Target = DeviceUpdateTarget.Tar.GetDescription(),
                        Remark = "Tar file(.tar or .zip)"
                    };
                    tarFile.IcuIds.Add(device.Id);
                    listITouchDeviceTypeModel.FileList.Add(tarFile);

                }
                // when the device type is DQ-MINI
                else if (device.DeviceType == (short)DeviceType.DQMiniPlus)
                {
                    listITouchDeviceTypeModel.Name = ((DeviceType)device.DeviceType).GetDescription();

                    // main firmware ( bin )
                    if (iTouchMainType == null)
                    {
                        iTouchMainType = new FileDetail
                        {
                            Target = DeviceUpdateTarget.DQMiniPlus.GetDescription(),
                            Remark = "DQ-MINI firmware(.bin)"
                        };
                        iTouchMainType.IcuIds.Add(device.Id);
                        listITouchDeviceTypeModel.FileList.Add(iTouchMainType);
                    }
                    else
                    {
                        var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(listITouchDeviceTypeModel.FileList.Find(x =>
                            x.Target == DeviceUpdateTarget.DQMiniPlus.GetDescription()));
                        if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }

                    // Tar file
                    FileDetail tarFile = new FileDetail
                    {
                        Target = DeviceUpdateTarget.Tar.GetDescription(),
                        Remark = "Tar file (.tar or .zip)"
                    };
                    tarFile.IcuIds.Add(device.Id);
                    listITouchDeviceTypeModel.FileList.Add(tarFile);
                }
                // when the device type is iT100
                else if (device.DeviceType == (short)DeviceType.IT100)
                {
                    listITouchDeviceTypeModel.Name = ((DeviceType)device.DeviceType).GetDescription();

                    // main firmware ( bin )
                    if (iTouchMainType == null)
                    {
                        iTouchMainType = new FileDetail
                        {
                            Target = DeviceUpdateTarget.IT100.GetDescription(),
                            Remark = "iT100 firmware(.apk)"
                        };
                        iTouchMainType.IcuIds.Add(device.Id);
                        listITouchDeviceTypeModel.FileList.Add(iTouchMainType);
                    }
                    else
                    {
                        var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(listITouchDeviceTypeModel.FileList.Find(x =>
                            x.Target == DeviceUpdateTarget.IT100.GetDescription()));
                        if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }
                }
                // when the device type is PM85
                else if (device.DeviceType == (short)DeviceType.PM85)
                {
                    listITouchDeviceTypeModel.Name = ((DeviceType)device.DeviceType).GetDescription();

                    // main firmware ( bin )
                    if (iTouchMainType == null)
                    {
                        iTouchMainType = new FileDetail
                        {
                            Target = DeviceUpdateTarget.PM85.GetDescription(),
                            Remark = "PM85 firmware(.apk)"
                        };
                        iTouchMainType.IcuIds.Add(device.Id);
                        listITouchDeviceTypeModel.FileList.Add(iTouchMainType);
                    }
                    else
                    {
                        var indexOf = listITouchDeviceTypeModel.FileList.IndexOf(listITouchDeviceTypeModel.FileList.Find(x =>
                            x.Target == DeviceUpdateTarget.PM85.GetDescription()));
                        if (!listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Contains(device.Id))
                        {
                            listITouchDeviceTypeModel.FileList[indexOf].IcuIds.Add(device.Id);
                        }
                    }
                }
            }

            if (listIcuDeviceTypeModel.FileList.Any())
            {
                listDeviceType.Add(listIcuDeviceTypeModel);
            }

            if (listITouchDeviceTypeModel.FileList.Any())
            {
                listDeviceType.Add(listITouchDeviceTypeModel);
            }
            return new DeviceTypeList { DeviceTypes = listDeviceType };
        }

        /// <summary>
        /// Upload data
        /// </summary>
        /// <param name="files"></param>
        public void UploadFile(IFormFileCollection files)
        {
            string sender = _httpContext.User.GetUsername();
            string language = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId())?.Language;
            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
            foreach (var file in files)
            {
                deviceInstructionQueue.UploadFile(new UploadFileQueueModel()
                {
                    Sender = sender,
                    Language = language,
                    MessageType = Constants.Protocol.FileDownLoad,
                    File = file
                });
            }
        }
        
        /// <summary>
        /// Get user
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        private List<SendUserDetail> GetUser(IcuDevice device)
        {
            var userDetails = new List<SendUserDetail>();
            var fileName = FileHelpers.GetLastestFileName(Directory.GetCurrentDirectory(), device.DeviceAddress, Constants.ExportType.LoadAllUser);
            if (File.Exists(fileName))
            {
                var lines = File.ReadAllLines(fileName);
                for (var i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Split(",");
                    var user = new SendUserDetail
                    {
                        CardId = line[0],
                        UserName = line[1].Trim(),
                        Department = line[2],
                        EmployeeNumber = line[3],
                        ExpireDate = line[4],
                        IssueCount = Convert.ToInt32(line[5]),
                        IsMasterCard = Convert.ToInt32(line[6]),
                        EffectiveDate = line[7],
                        CardStatus = line[8],
                        Password = line[9],
                        Timezone = line[10]
                    };
                    userDetails.Add(user);
                }
            }

            return userDetails;
        }

        /// <summary>
        /// Get user info
        /// </summary>
        /// <param name="device"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public UserInfoModel GetUserInfo(IcuDevice device, string cardId)
        {
            SendUserDetail userDb = null, userDevice = null;
            var userIcusDetails = GetUser(device);
            var user = _unitOfWork.UserRepository.GetByCardIdIncludeDepartment(device.CompanyId, cardId);
            if (user != null)
            {
                var card = _unitOfWork.CardRepository.GetByUserId(device.CompanyId ?? 0, user.Id).Where(m => m.CardId.Equals(cardId)).FirstOrDefault();

                userDb = _mapper.Map<SendUserDetail>(user);
                userDb.CardId = card?.CardId;
                userDb.IssueCount = card != null ? card.IssueCount : 0;
                userDb.CardStatus = card != null ? ((CardStatus)card.CardStatus).GetDescription() : "";
                var agDevice = _unitOfWork.AccessGroupDeviceRepository.GetByIcuId(device.CompanyId ?? 0,
                    device.Id).FirstOrDefault(x => x.AccessGroupId == user.AccessGroupId);
                if (agDevice != null) userDb.Timezone = agDevice.Tz.Name;
            }
            if (userIcusDetails.Any())
            {
                userDevice = userIcusDetails.FirstOrDefault(x => x.CardId == cardId);
            }

            var model = new UserInfoModel
            {
                UserDb = userDb,
                UserDevice = userDevice
            };
            return model;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="device"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public UserInfoByCardIdModel GetUserInfoByCardId(IcuDevice device, string cardId)
        {
            SendUserDetail userDb = null;
            //var msgId = MakeLoadUserByCardIdProtocolData(device, cardId);

            var card = _unitOfWork.AppDbContext.Card.Where(m => m.CompanyId == device.CompanyId && m.CardId.ToLower().Equals(cardId.ToLower()) && !m.IsDeleted).FirstOrDefault();

            if (card != null)
            {
                DateTime effectiveDate = new DateTime();
                DateTime expiredDate = new DateTime();

                if (card.VisitId != null)
                {
                    var visitor = _unitOfWork.AppDbContext.Visit.Where(m => m.Id == card.VisitId && !m.IsDeleted).FirstOrDefault();

                    effectiveDate = visitor.StartDate;
                    expiredDate = visitor.EndDate;

                    userDb = _mapper.Map<SendUserDetail>(visitor);
                }
                else if (card.UserId != null)
                {
                    var user = _unitOfWork.AppDbContext.User.Include(u => u.Department).Where(m => m.Id == card.UserId && !m.IsDeleted).FirstOrDefault();

                    effectiveDate = user.EffectiveDate.Value;
                    expiredDate = user.ExpiredDate.Value;

                    userDb = _mapper.Map<SendUserDetail>(user);

                    var agDevice = _unitOfWork.AccessGroupDeviceRepository.GetByIcuId(device.CompanyId ?? 0,
                        device.Id).FirstOrDefault(x => x.AccessGroupId == user.AccessGroupId);
                    if (agDevice != null) userDb.Timezone = agDevice.Tz.Name;
                }

                var buildingTimezone = _unitOfWork.BuildingRepository.Get(m => m.Id == device.BuildingId && !m.IsDeleted).TimeZone;
                var offSet = buildingTimezone.ToTimeZoneInfo().BaseUtcOffset;

                effectiveDate = Helpers.ConvertToUserTime(effectiveDate, offSet);
                expiredDate = Helpers.ConvertToUserTime(expiredDate, offSet);

                userDb.CardId = cardId;
                userDb.CardStatus = ((CardStatus)card.CardStatus).GetDescription();
                userDb.IssueCount = card.IssueCount;
                userDb.EffectiveDate = effectiveDate.ToString(Constants.DateTimeFormat.DdMdYyyyFormat);
                userDb.ExpireDate = expiredDate.ToString(Constants.DateTimeFormat.DdMdYyyyFormat);
            }

            var model = new UserInfoByCardIdModel
            {
                MsgId = MakeLoadUserByCardIdProtocolData(device, cardId),
                User = userDb
            };
            return model;
        }

        /// <summary>
        /// Get user information by card Id
        /// </summary>
        /// <param name="device"></param>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public UserInfoByCardIdModel CheckUserInfoInDevice(IcuDevice device, string cardId, bool deviceCheck = true)
        {
            SendUserDetail userDb = null;

            string msgId = Guid.NewGuid().ToString();

            if (deviceCheck)
                msgId = MakeLoadUserByCardIdProtocolData(device, cardId);

            var card = _unitOfWork.AppDbContext.Card.Where(m => m.CardId.ToLower().Equals(cardId.ToLower()) && !m.IsDeleted).FirstOrDefault();

            if (card != null)
            {
                var accountTimezone = _unitOfWork.AccountRepository.Get(m =>
                   m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted).TimeZone;
                var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

                if (card.VisitId != null)
                {
                    var visitor = _unitOfWork.AppDbContext.Visit.Where(m => m.Id == card.VisitId && !m.IsDeleted).FirstOrDefault();

                    var agDevice = _unitOfWork.AccessGroupDeviceRepository.GetByIcuId(device.CompanyId ?? 0,
                        device.Id).FirstOrDefault(x => x.AccessGroupId == visitor.AccessGroupId);

                    if (agDevice != null)
                    {
                        userDb = _mapper.Map<SendUserDetail>(visitor);

                        // [Temporary code - Edward]
                        // This function was made only for SWC project. And I need to parse this data form as ddMMyyyyHHmmss.
                        // But I can't change the mapper because the mapper being used at other places too.
                        // So I Added below 2 lines for SWC project.
                        //visitor.StartDate = Helpers.ConvertToUserTime(visitor.StartDate, accountTimezone);
                        //visitor.EndDate = Helpers.ConvertToUserTime(visitor.EndDate, accountTimezone);
                        visitor.StartDate = Helpers.ConvertToUserTime(visitor.StartDate, offSet);
                        visitor.EndDate = Helpers.ConvertToUserTime(visitor.EndDate, offSet);

                        userDb.EffectiveDate = visitor.StartDate.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
                        userDb.ExpireDate = visitor.EndDate.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
                    }
                }
                else if (card.UserId != null)
                {
                    var user = _unitOfWork.AppDbContext.User.Where(m => m.Id == card.UserId && !m.IsDeleted).FirstOrDefault();

                    var agDevice = _unitOfWork.AccessGroupDeviceRepository.GetByIcuId(device.CompanyId ?? 0,
                        device.Id).FirstOrDefault(x => x.AccessGroupId == user.AccessGroupId);

                    if (agDevice != null)
                    {
                        userDb = _mapper.Map<SendUserDetail>(user);

                        userDb.Timezone = agDevice.Tz.Name;

                        // [Temporary code - Edward]
                        // This function was made only for SWC project. And I need to parse this data form as ddMMyyyyHHmmss.
                        // But I can't change the mapper because the mapper being used at other places too.
                        // So I Added below 2 line for SWC project.
                        //user.EffectiveDate = Helpers.ConvertToUserTime(user.EffectiveDate.Value, accountTimezone);
                        //user.ExpiredDate = Helpers.ConvertToUserTime(user.ExpiredDate.Value, accountTimezone);
                        user.EffectiveDate = Helpers.ConvertToUserTime(user.EffectiveDate.Value, offSet);
                        user.ExpiredDate = Helpers.ConvertToUserTime(user.ExpiredDate.Value, offSet);

                        userDb.EffectiveDate = user.EffectiveDate.Value.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
                        userDb.ExpireDate = user.ExpiredDate.Value.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
                    }
                }
            }

            var model = new UserInfoByCardIdModel
            {
                MsgId = msgId,
                User = userDb
            };
            return model;
        }

        /// <summary>
        /// Make holiday protocol data
        /// </summary>
        /// <param name="device"></param>
        /// <param name="cardId"></param>
        private string MakeLoadUserByCardIdProtocolData(IcuDevice device, string cardId)
        {
            _deviceSdkService.GetUserInfor(device.DeviceAddress, cardId);
            return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Transmit data
        /// </summary>
        /// <param name="models"></param>
        /// <param name="devices"></param>
        public void TransmitData(TransmitDataModel models, List<IcuDevice> devices)
        {
            if (models.IsAllDevice)
            {
                devices = _unitOfWork.IcuDeviceRepository.GetDeviceAllInfoByCompanyE(_httpContext.User.GetCompanyId()).ToList();
            }

            if (devices != null && devices.Any())
            {
                var actionType = Constants.ActionType.TransmitData;
                string sender = _httpContext?.User.GetUsername() ?? Constants.RabbitMq.SenderDefault;
                foreach (var device in devices)
                {
                    if (device.Building == null)
                    {
                        device.Building = _unitOfWork.BuildingRepository.GetByIdAndCompanyId(device.CompanyId.Value, device.BuildingId.Value);
                    }
                    
                    // init parameters
                    var content = $"{DeviceResource.msgTransmitData} : {device.Name}({device.DeviceAddress})";
                    List<string> details = new List<string>();
                    var contentsDetail = "";
                    var processId = TransmitDataModel.GetProcessIdFromDeviceId(models.Devices, device.Id);
                    
                    // Calculate total number of message should be sent (for progress bar)
                    int groupLength = 0;
                    int lenghtMessageUserInfo = 0;
                    int lenghtTimezone = 0;
                    groupLength += 1; // get device info - the last message
                    foreach (var transId in models.TransmitIds)
                    {
                        switch (transId)
                        {
                            case (short)TransmitType.CurrentTime:
                            {
                                groupLength += 1;
                                details.Add(DeviceResource.lblTransmitCurrentTime);
                                break;   
                            }
                            case (short)TransmitType.DeviceSetting:
                            {
                                groupLength += 1;
                                details.Add(DeviceResource.lblTransmitDeviceSetting);
                                break;
                            }
                            case (short)TransmitType.TimezoneSetting:
                            {
                                lenghtTimezone = _unitOfWork.AccessTimeRepository.GetByCompany(device.CompanyId).Count();
                                groupLength += lenghtTimezone;
                                details.Add(DeviceResource.lblTransmitTimezoneSetting);
                                break;
                            }
                            case (short)TransmitType.HolidaySetting:
                            {
                                groupLength += 1;
                                details.Add(DeviceResource.lblTransmitHolidaySetting);
                                break;   
                            }
                            case (short)TransmitType.UserInfo:
                            {
                                // message send user info
                                int countCard = 0;
                                if (models.UserIds != null && models.UserIds.Count > 0)
                                {
                                    countCard = _unitOfWork.CardRepository.GetCardAvailableInDevice(device.Id).Count(m => m.UserId.HasValue && models.UserIds.Contains(m.UserId.Value));
                                }
                                else
                                {
                                    countCard = _unitOfWork.CardRepository.GetCardAvailableInDevice(device.Id).Count();
                                }
                                
                                int maxSplitMessage = Helpers.GetMaxSplit(device.DeviceType);
                                lenghtMessageUserInfo = countCard / maxSplitMessage;
                                lenghtMessageUserInfo = lenghtMessageUserInfo * maxSplitMessage < countCard ? lenghtMessageUserInfo + 1 : lenghtMessageUserInfo;
                                groupLength += lenghtMessageUserInfo;
                                
                                // access group
                                if ((models.IsDeleteAllUser && device.DeviceType == (short)DeviceType.Biostation2) || (device.DeviceType == (short)DeviceType.Biostation3))
                                {
                                    groupLength += 1;
                                }
                                
                                if (models.IsDeleteAllUser)
                                {
                                    // Delete_all_users
                                    groupLength += 1;

                                    if (!(device.DeviceType == (short)DeviceType.Icu300N
                                        || device.DeviceType == (short)DeviceType.Icu300NX
                                        || device.DeviceType == (short)DeviceType.DesktopApp
                                        || device.DeviceType == (short)DeviceType.ITouchPop
                                        || device.DeviceType == (short)DeviceType.NexpaLPR
                                        || device.DeviceType == (short)DeviceType.FV6000
                                        || device.DeviceType == (short)DeviceType.XStation2
                                        || device.DeviceType == (short)DeviceType.Biostation3
                                        || device.DeviceType == (short)DeviceType.Biostation2))
                                    {
                                        // + 1 update user data
                                        groupLength += 1;
                                    }
                                }
                                
                                details.Add(DeviceResource.lblTransmitUserInfo);
                                break;
                            }
                        }
                    }
                    
                    //Save system log
                    contentsDetail = string.Join("\n", details);
                    _unitOfWork.SystemLogRepository.Add(device.Id, SystemLogType.TransmitAllData, ActionLogType.Transmit, content, contentsDetail, null, device.CompanyId);
                    _unitOfWork.Save();

                    // new thread send messages to device
                    new Thread(() =>
                    {
                        IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                        try
                        {
                            int currentIndex = 1;
                            var deviceInstructionQueue = new DeviceInstructionQueue(unitOfWork, _configuration, _webSocketService);
                            var accessControlQueue = new AccessControlQueue(unitOfWork, _webSocketService);
                            foreach (var transId in models.TransmitIds)
                            {
                                switch (transId)
                                {
                                    case (short)TransmitType.CurrentTime:
                                    {
                                        deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                                        {
                                            DeviceAddress = device.DeviceAddress,
                                            DeviceId = device.Id,
                                            MessageType = Constants.Protocol.DeviceInstruction,
                                            MsgId = processId,
                                            Sender = sender,
                                            MessageIndex = currentIndex,
                                            MessageTotal = groupLength,
                                            IsSaveSystemLog = false,
                                            Command = Constants.CommandType.SetTime,
                                            UserName = sender,
                                        });
                                        ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
                                        {
                                            Type = Constants.SDKDevice.WebhookProcessData,
                                            Data = new SDKProcessFeModel()
                                            {
                                                MsgId = processId,
                                                Index = currentIndex,
                                                Total = groupLength,
                                            }
                                        }), device.CompanyId ?? 0);
                                        currentIndex++;
                                        break;
                                    }
                                    case (short)TransmitType.DeviceSetting:
                                    {
                                        // Get setting
                                        bool useStaticQrCode = false;
                                        if (device.CompanyId.HasValue)
                                        {
                                            var setting = unitOfWork.SettingRepository.GetByKey(Constants.Settings.UseStaticQrCode, device.CompanyId.Value);
                                            bool.TryParse(Helpers.GetStringFromValueSetting(setting?.Value), out useStaticQrCode);
                                        }
                                        
                                        deviceInstructionQueue.SendDeviceConfig(new ConfigQueueModel()
                                        {
                                            DeviceAddress = device.DeviceAddress,
                                            DeviceId = device.Id,
                                            MessageType = Constants.Protocol.UpdateDeviceConfig,
                                            MsgId = processId,
                                            Sender = sender,
                                            MessageIndex = currentIndex,
                                            MessageTotal = groupLength,
                                            UseStaticQrCode = useStaticQrCode
                                        });
                                        
                                        ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
                                        {
                                            Type = Constants.SDKDevice.WebhookProcessData,
                                            Data = new SDKProcessFeModel()
                                            {
                                                MsgId = processId,
                                                Index = currentIndex,
                                                Total = groupLength,
                                            }
                                        }), device.CompanyId ?? 0);
                                        currentIndex++;
                                        break;
                                    }
                                    case (short)TransmitType.TimezoneSetting:
                                    {
                                        deviceInstructionQueue.SendUpdateTimezone(new UpdateTimezoneQueueModel()
                                        {
                                            DeviceAddress = device.DeviceAddress,
                                            DeviceId = device.Id,
                                            MessageType = Constants.Protocol.UpdateTimezone,
                                            MsgId = processId,
                                            Sender = sender,
                                            MessageIndex = currentIndex,
                                            MessageTotal = groupLength,
                                        });
                                        currentIndex += lenghtTimezone;
                                        ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
                                        {
                                            Type = Constants.SDKDevice.WebhookProcessData,
                                            Data = new SDKProcessFeModel()
                                            {
                                                MsgId = processId,
                                                Index = currentIndex,
                                                Total = groupLength,
                                            }
                                        }), device.CompanyId ?? 0);
                                        break;
                                    }
                                    case (short)TransmitType.HolidaySetting:
                                    {
                                        deviceInstructionQueue.SendHoliday(new HolidayQueueModel()
                                        {
                                            DeviceAddress = device.DeviceAddress,
                                            DeviceId = device.Id,
                                            MessageType = Constants.Protocol.UpdateHoliday,
                                            MsgId = processId,
                                            Sender = sender,
                                            MessageIndex = currentIndex,
                                            MessageTotal = groupLength,
                                        });
                                        ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
                                        {
                                            Type = Constants.SDKDevice.WebhookProcessData,
                                            Data = new SDKProcessFeModel()
                                            {
                                                MsgId = processId,
                                                Index = currentIndex,
                                                Total = groupLength,
                                            }
                                        }), device.CompanyId ?? 0);
                                        currentIndex++;
                                        break;
                                    }
                                    case (short)TransmitType.UserInfo:
                                    {
                                        if (models.IsDeleteAllUser || lenghtMessageUserInfo == 0)
                                        {
                                            deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                                            {
                                                DeviceAddress = device.DeviceAddress,
                                                DeviceId = device.Id,
                                                MessageType = Constants.Protocol.DeviceInstruction,
                                                MsgId = processId,
                                                Sender = sender,
                                                Command = Constants.CommandType.DeleteAllUsers,
                                                UserName = sender,
                                                MessageIndex = currentIndex,
                                                MessageTotal = groupLength,
                                            });
                                            currentIndex += 1;
                                            ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
                                            {
                                                Type = Constants.SDKDevice.WebhookProcessData,
                                                Data = new SDKProcessFeModel()
                                                {
                                                    MsgId = processId,
                                                    Index = groupLength,
                                                    Total = groupLength,
                                                }
                                            }), device.CompanyId ?? 0);
                                        }

                                        List<int> userIds = null;
                                        if (models.UserIds != null && models.UserIds.Count > 0)
                                        {
                                            userIds = models.UserIds;
                                        }

                                        if (device.DeviceType != (short)DeviceType.EbknReader)
                                        {
                                            accessControlQueue.SendUserInfo(new UserInfoQueueModel()
                                            {
                                                DeviceAddress = device.DeviceAddress,
                                                DeviceId = device.Id,
                                                MessageType = Constants.Protocol.AddUser,
                                                MsgId = processId,
                                                Sender = sender,
                                                UserIds = userIds,
                                                VisitIds = null,
                                                MessageIndex = currentIndex,
                                                MessageTotal = groupLength,
                                            });
                                        }
                                        currentIndex += lenghtMessageUserInfo;

                                        if (models.IsDeleteAllUser)
                                        {
                                            if (!(device.DeviceType == (short)DeviceType.Icu300N
                                                  || device.DeviceType == (short)DeviceType.Icu300NX
                                                  || device.DeviceType == (short)DeviceType.DesktopApp
                                                  || device.DeviceType == (short)DeviceType.ITouchPop
                                                  || device.DeviceType == (short)DeviceType.NexpaLPR
                                                  || device.DeviceType == (short)DeviceType.FV6000
                                                  || device.DeviceType == (short)DeviceType.XStation2
                                                  || device.DeviceType == (short)DeviceType.Biostation3
                                                  || device.DeviceType == (short)DeviceType.Biostation2))
                                            {
                                                deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                                                {
                                                    DeviceAddress = device.DeviceAddress,
                                                    DeviceId = device.Id,
                                                    MessageType = Constants.Protocol.DeviceInstruction,
                                                    MsgId = processId,
                                                    Sender = sender,
                                                    Command = Constants.CommandType.UpdateAllUsers,
                                                    UserName = sender,
                                                    MessageIndex = currentIndex,
                                                    MessageTotal = groupLength,
                                                });
                                                currentIndex += 1;
                                                ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
                                                {
                                                    Type = Constants.SDKDevice.WebhookProcessData,
                                                    Data = new SDKProcessFeModel()
                                                    {
                                                        MsgId = processId,
                                                        Index = currentIndex,
                                                        Total = groupLength,
                                                    }
                                                }), device.CompanyId ?? 0);
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            
                            // get device info
                            deviceInstructionQueue.SendLoadDeviceInfo(new LoadDeviceInfoQueueModel()
                            {
                                DeviceAddress = device.DeviceAddress,
                                MsgId = processId,
                                Sender = sender,
                                MessageIndex = currentIndex,
                                MessageTotal = groupLength,
                                MessageType = Constants.Protocol.LoadDeviceInfo,
                            });
                            ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
                            {
                                Type = Constants.SDKDevice.WebhookProcessData,
                                Data = new SDKProcessFeModel()
                                {
                                    MsgId = processId,
                                    Index = groupLength,
                                    Total = groupLength,
                                }
                            }), device.CompanyId ?? 0);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                        finally
                        {
                            unitOfWork.Dispose();
                        }
                    }).Start();
                }
            }
        }

        /// <summary>
        /// Init data
        /// </summary>
        /// <returns></returns>
        public DeviceInitModel InitializeData()
        {
            var data = new DeviceInitModel
            {
                VerifyModeItems = EnumHelper.ToEnumList<VerifyMode>(),
                SensorTypeItems = EnumHelper.ToEnumList<SensorType>(),
                PassbackItems = EnumHelper.ToEnumList<PassbackRules>(),
                HolidayItems = EnumHelper.ToEnumList<HolidayType>()
            };
            return data;
        }
        public List<IcuDevice> GetOnlineDevices(List<IcuDevice> devices, ref string offlineDeviceAddr)
        {
            List<IcuDevice> onlineDevices = new List<IcuDevice>();
            List<string> offlineDevices = new List<string>();

            foreach (var device in devices)
            {
                if (device.ConnectionStatus == (short)ConnectionStatus.Offline)
                {
                    offlineDevices.Add(device.DeviceAddress);
                }
                else
                {
                    onlineDevices.Add(device);
                }
            }

            offlineDeviceAddr = string.Join(", ", offlineDevices);

            return onlineDevices;
        }
        

        /// <summary>
        /// Stop Firmware updating
        /// </summary>
        /// <param name="devices">device list to stop</param>
        /// <param name="processIds">process id list</param>
        /// <returns></returns>
        public void StopProcess(List<IcuDevice> devices, List<string> processIds)
        {
            if (devices.Any())
            {
                foreach (var device in devices)
                {
                    var deviceAddress = device.DeviceAddress;
                }
            }
        }

        /// <summary>
        /// Checking if there are any changes.
        /// </summary>
        /// <param name="device">Device that contains existing information</param>
        /// <param name="model">Model that contains new information</param>
        /// <param name="changes">List of changes</param>
        /// <returns></returns>
        internal bool CheckChange(IcuDevice device, DeviceModel model, ref List<string> changes)
        {
            if (model.Id != 0)
            {
                if (device.Name != model.DoorName)
                {
                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblDoorName, device.Name, model.DoorName));
                }

                if (device.DeviceType != model.DeviceType)
                {
                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblDeviceType, ((DeviceType)device.DeviceType).GetDescription(), ((DeviceType)model.DeviceType).GetDescription()));
                }

                if (device.VerifyMode != model.VerifyMode)
                {
                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblVerifyMode, ((VerifyMode)device.VerifyMode).GetDescription(), ((VerifyMode)model.VerifyMode).GetDescription()));
                }

                if (device.BackupPeriod != model.BackupPeriod)
                {
                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblBackupPeriod, device.BackupPeriod, model.BackupPeriod));
                }

                if (device.ActiveTzId != model.ActiveTimezoneId && device.ActiveTzId != null && model.ActiveTimezoneId != null)
                {
                    string oldValue = _unitOfWork.AccessTimeRepository.GetById(device.ActiveTzId ?? 0)?.Name;
                    string newValue = _unitOfWork.AccessTimeRepository.GetById(model.ActiveTimezoneId ?? 0)?.Name;

                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblDoorActiveTimezone, oldValue, newValue));

                    //var tz = _unitOfWork.AccessTimeRepository.GetById(device.ActiveTzId ?? 0);
                    //if (tz != null)
                    //{
                    //    changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblDoorActiveTimezone,
                    //        _unitOfWork.AccessTimeRepository.GetById(device.ActiveTzId ?? 0).Name,
                    //        _unitOfWork.AccessTimeRepository.GetById(model.ActiveTimezoneId ?? 0).Name));
                    //}
                    //else
                    //{
                    //    changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblDoorActiveTimezone,
                    //        null, _unitOfWork.AccessTimeRepository.GetById(model.ActiveTimezoneId ?? 0).Name));
                    //}
                }

                if (device.PassageTzId != model.PassageTimezoneId && device.PassageTzId != null && model.PassageTimezoneId != null)
                {
                    string oldValue = _unitOfWork.AccessTimeRepository.GetById(device.PassageTzId ?? 0)?.Name;
                    string newValue = _unitOfWork.AccessTimeRepository.GetById(model.PassageTimezoneId ?? 0)?.Name;

                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblDoorPassageTimezone, oldValue, newValue));

                    //var pz = _unitOfWork.AccessTimeRepository.GetById(device.PassageTzId ?? 0);
                    //if (pz != null)
                    //{
                    //    changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblDoorPassageTimezone,
                    //        _unitOfWork.AccessTimeRepository.GetById(device.PassageTzId ?? 0).Name,
                    //        _unitOfWork.AccessTimeRepository.GetById(model.PassageTimezoneId ?? 0).Name));
                    //}
                    //else
                    //{
                    //    changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblDoorPassageTimezone,
                    //        null,
                    //        _unitOfWork.AccessTimeRepository.GetById(model.PassageTimezoneId ?? 0).Name));
                    //}
                }

                if (device.OperationType != model.OperationType)
                {
                    //changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblOperationType,
                    //    ((OperationType)device.OperationType).GetDescription(), ((OperationType)model.OperationType).GetDescription()));

                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblOperationType, ((OperationType)device.OperationType).GetDescription(), ((OperationType)model.OperationType).GetDescription()));
                }

                //if (device.RoleReader0 != model.RoleReader0)
                //{
                //    changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblOperationType,
                //        ((OperationType)device.OperationType).GetDescription(), ((OperationType)model.OperationType).GetDescription()));
                //}

                if (device.SensorType != model.SensorType)
                {
                    //changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblDoorSensorType,
                    //     ((SensorType)device.SensorType).GetDescription(), ((SensorType)model.SensorType).GetDescription()));

                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblDoorSensorType, ((SensorType)device.SensorType).GetDescription(), ((SensorType)model.SensorType).GetDescription()));
                }

                if (device.MaxOpenDuration != model.MaxOpenDuration)
                {
                    //changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblOperationType,
                    //    ((OperationType)device.OperationType).GetDescription(), ((OperationType)model.OperationType).GetDescription()));

                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblMaxOpenDuration, device.MaxOpenDuration, model.MaxOpenDuration));
                }

                if (device.OpenDuration != model.LockOpenDuration)
                {
                    //changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblLockOpenDuration,
                    //     device.OpenDuration ?? 0, model.LockOpenDuration));

                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblLockOpenDuration, device.OpenDuration, model.LockOpenDuration));
                }

                if (device.SensorDuration != model.SensorDuration)
                {
                    //changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblStatusDelay,
                    //     device.SensorDuration ?? 0, model.SensorDuration ?? 0));

                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblStatusDelay, device.SensorDuration, model.SensorDuration));
                }

                if (device.SensorAlarm != model.Alarm)
                {
                    //changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblAlarm,
                    //     device.SensorAlarm ? CommonResource.Use : CommonResource.NotUse, model.Alarm ? CommonResource.Use : CommonResource.NotUse));

                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblAlarm, device.SensorAlarm ? CommonResource.Use : CommonResource.NotUse, model.Alarm ? CommonResource.Use : CommonResource.NotUse));
                }

                if (device.CloseReverseLockFlag != model.CloseReverseLock)
                {
                    //changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblCloseReverseLock,
                    //     device.CloseReverseLockFlag ? CommonResource.Use : CommonResource.NotUse, model.CloseReverseLock ? CommonResource.Use : CommonResource.NotUse));

                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblCloseReverseLock, device.CloseReverseLockFlag ? CommonResource.Use : CommonResource.NotUse, model.CloseReverseLock ? CommonResource.Use : CommonResource.NotUse));
                }

                if (device.PassbackRule != model.Passback)
                {
                    //changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblAntiPassback,
                    //     ((PassbackRules)device.PassbackRule).GetDescription(), ((PassbackRules)model.Passback).GetDescription()));

                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblAntiPassback, ((PassbackRules)device.PassbackRule).GetDescription(), ((PassbackRules)model.Passback).GetDescription()));
                }

                if (device.MPRCount != model.MPRCount)
                {
                    //changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblDoorSensorType,
                    //     device.MPRCount, model.MPRCount ?? 1));

                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblDoorSensorType, device.MPRCount, model.MPRCount));
                }

                if (device.MPRInterval != model.MPRInterval)
                {
                    //changes.Add(string.Format(MessageResource.msgChangeInfo, DeviceResource.lblDoorSensorType,
                    //     device.MPRInterval, model.MPRInterval));

                    changes.Add(Helpers.CreateChangedValueContents(DeviceResource.lblDoorSensorType, device.MPRInterval, model.MPRInterval));
                }
            }

            return changes.Count() > 0;
        }

        /// <summary>   View history. \n
        ///             This function is for displaying the device history on web.</summary>
        /// <param name="device"> Device to check history. </param>
        /// <param name="pageNumber"> page number </param>
        /// <param name="pageSize"> data count in one page </param>
        /// <param name="totalRecords"> total records count </param>
        /// <remarks>   Edward, 2020-02-29. </remarks>
        public IEnumerable<DeviceHistoryModel> GetHistory(IcuDevice device, int pageNumber, int pageSize, out int totalRecords)
        {
            var companyId = device.CompanyId;
            var startSysTem = DateTime.Now;
            var systemLogDataLogs = _unitOfWork.AppDbContext.SystemLog
                .Include(m => m.CreatedByNavigation)
                .Where(c => c.CompanyId == companyId)
                .Where(c => c.Type == (short)SystemLogType.DeviceMonitoring
                            || c.Type == (short)SystemLogType.DeviceSetting
                            || c.Type == (short)SystemLogType.DeviceUpdate
                            || c.Type == (short)SystemLogType.TransmitAllData
                            || c.Type == (short)SystemLogType.Emergency).AsEnumerable<SystemLog>();

            var systemLogData = systemLogDataLogs.Where(c =>
                    c.ContentIds != null &&
                    (JObject.Parse(c.ContentIds)["Id"].ToString().Equals(device.Id + "")
                    || JObject.Parse(c.ContentIds)["assigned_ids"].ToString().Contains(device.Id + "")))
                .Select(c => new SystemLog
                {
                    //OpeTime = Helpers.ConvertToUserTimeZoneReturnDate(c.OpeTime, accountTimezone),
                    OpeTime = c.OpeTime,
                    CreatedByNavigation = c.CreatedByNavigation,
                    Action = c.Action,
                    Content = c.Content,
                    ContentDetails = c.ContentDetails
                }).Select(_mapper.Map<DeviceHistoryModel>);
            Console.WriteLine("[SystemLog: ]       {0}", DateTime.Now.Subtract(startSysTem).TotalMilliseconds);
            var startEvent = DateTime.Now;
            var listEventType = new List<int>
            {
                (short)EventType.CommunicationSucceed,
                (short)EventType.CommunicationFailed,
                (short)EventType.ApplicationIsRunning,
                (short)EventType.PassageAccessTimeOn,
                (short)EventType.PassageAccessTimeOff,
                (short)EventType.FirmwareApplicationUpdate,
                (short)EventType.FirmwareDownloadFailed,
                (short)EventType.DeviceInstructionDeleteAllEvent,
                (short)EventType.DeviceInstructionDeleteAllUser,
                (short)EventType.DeviceInstructionSendAllUser,
                (short)EventType.DeviceInstructionForceClose,
                (short)EventType.DeviceInstructionForceOpen,
                (short)EventType.DeviceInstructionOpen,
                (short)EventType.DeviceInstructionRelease,
                (short)EventType.DeviceInstructionReset,
                (short)EventType.DeviceInstructionSettime,
                (short)EventType.ExitApplication,
                (short)EventType.DeviceIsRunning,
            };
            var eventLogData = _unitOfWork.AppDbContext.EventLog
                .Where(c => c.CompanyId == companyId)
                .Where(c => c.IcuId == device.Id && !c.CameraId.HasValue)
                .Where(c => listEventType.Contains(c.EventType))
                .Select(c => new EventLog
                {
                    //EventTime = Helpers.ConvertToUserTimeZoneReturnDate(c.EventTime, accountTimezone),
                    EventTime = c.EventTime,
                    EventType = c.EventType
                })
                .Select(_mapper.Map<DeviceHistoryModel>);
            Console.WriteLine("[EventLog: ]       {0}", DateTime.Now.Subtract(startEvent).TotalMilliseconds);
            var startpagging = DateTime.Now;
            
            totalRecords = systemLogData.Count() + eventLogData.Count();
            List<DeviceHistoryModel> data = systemLogData.Concat(eventLogData).ToList();
            data = data.OrderByDescending(c => c.AccessTimeUtc).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            
            Console.WriteLine("[Order: ]       {0}", DateTime.Now.Subtract(startpagging).TotalMilliseconds);
            var startpagging1 = DateTime.Now;
            
            data.ForEach(m => m.EventDetails = !string.IsNullOrEmpty(m.EventDetails) ? m.EventDetails.Replace("&lt;br /&gt;", "\n") : m.EventDetails);
            Console.WriteLine("[Replace: ]       {0}", DateTime.Now.Subtract(startpagging1).TotalMilliseconds);
            Console.WriteLine("[Pagging: ]       {0}", DateTime.Now.Subtract(startpagging).TotalMilliseconds);
            return data;
        }

        public IQueryable<DeviceListModel> GetPaginatedDevices(DeviceFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            var data = _unitOfWork.AppDbContext.IcuDevice
                .Include(m => m.ActiveTz)
                .Include(m => m.PassageTz)
                .Include(m => m.Building)
                .Include(m => m.Company)
                .Where(m => !m.IsDeleted);

            var accountTypeTemp = _httpContext.User.GetAccountType();
            var accountIdTemp = _httpContext.User.GetAccountId();
            var companyIdTemp = _httpContext.User.GetCompanyId();

            if (accountTypeTemp != (short)AccountType.SystemAdmin)
            {
                data = data.Where(m => m.CompanyId == companyIdTemp);
            }

            // check plugin
            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyIdTemp);
            if(plugin != null)
            {
                PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
                if (plugIns.DepartmentAccessLevel && accountTypeTemp == (short)AccountType.DynamicRole)
                {
                    List<int> doorIdList = _unitOfWork.DepartmentDeviceRepository.GetDoorIdsByAccountDepartmentRole(companyIdTemp, accountIdTemp);
                    data = data.Where(x => doorIdList.Contains(x.Id));
                }
            }    
            
            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter.Filter))
            {
                data = data.Where(x => x.Name.ToLower().Contains(filter.Filter.ToLower())
                                       || x.DeviceAddress.ToLower().Contains(filter.Filter.ToLower())
                                       || x.Building.Name.ToLower().Contains(filter.Filter.ToLower()));
            }

            if (filter.IgnoreIds != null && filter.IgnoreIds.Count > 0)
            {
                data = data.Where(x => !filter.IgnoreIds.Contains(x.Id));
            }
            if (filter.OperationTypes == null || !filter.OperationTypes.Any())
            {
                filter.OperationTypes = new List<int>()
                {
                    (int) OperationType.Entrance,
                    (int) OperationType.BusReader,
                    (int) OperationType.FireDetector,
                    (int) OperationType.Reception,
                };
            }
            else if (filter.OperationTypes.Any(m => m.Equals((int)OperationType.Entrance)) && !filter.OperationTypes.Any(m => m.Equals((int)OperationType.FireDetector)))
            {
                filter.OperationTypes.Add((int)OperationType.FireDetector);
            }
            
            data = data.Where(x => filter.OperationTypes.Contains((int)x.OperationType));
            
            if (filter.CompanyIds != null && filter.CompanyIds.Count > 0)
            {
                data = data.Where(x => filter.CompanyIds.Contains(x.CompanyId.Value) || (filter.CompanyIds.Contains(0) && !x.CompanyId.HasValue));
            }
            if (filter.ConnectionStatus != null && filter.ConnectionStatus.Count > 0)
            {
                data = data.Where(x => filter.ConnectionStatus.Contains(x.ConnectionStatus));
            }
            if (filter.Status != null && filter.Status.Count > 0)
            {
                data = data.Where(x => filter.Status.Contains(x.Status));
            }
            if (filter.DeviceTypes != null && filter.DeviceTypes.Count > 0)
            {
                data = data.Where(x => filter.DeviceTypes.Contains(x.DeviceType));
            }
            if (filter.BuildingIds != null && filter.BuildingIds.Count > 0)
            {
                data = data.Where(x => x.BuildingId.HasValue && filter.BuildingIds.Contains(x.BuildingId.Value));
            }

            recordsFiltered = data.Count();
            //data = !string.IsNullOrEmpty(filter.SortColumn) ? data.OrderBy($"{filter.SortColumn} {filter.SortDirection}") : data.OrderBy(c => c.DeviceAddress);
            
            //if (filter.PageSize > 0)
            //    data = data.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);
            
            var result = data.AsEnumerable().Select(_mapper.Map<DeviceListModel>).AsQueryable();

            result = Helpers.SortData(result.AsEnumerable<DeviceListModel>(), filter.SortDirection, filter.SortColumn);
            if (filter.PageSize > 0)
                result = result.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);

            var dataResult = new List<DeviceListModel>();
            foreach (var device in result)
            {
                var totalTime = Math.Abs(Math.Ceiling(DateTime.Now.Subtract(device.CreatedOn).TotalMinutes));
                device.TotalTime = Convert.ToInt32(totalTime);
                device.UpTime = Convert.ToInt32(Math.Ceiling((decimal)(device.UpTimeOnlineDevice * 100 / totalTime)));
                //device.FromDbIdNumber = _unitOfWork.CardRepository.GetCardAvailableInDevice(device.Id).Count();
                DirectoryInfo logInfo = new DirectoryInfo($"{Constants.Settings.DefineFolderDataLogs}/{device.CompanyCode}/{device.DeviceAddress}");
                if (!logInfo.Exists)
                {
                    device.FileLogStatus = (short)LogFileDeviceStatus.NoFile;
                }
                else
                {
                    var fileLogs = logInfo.GetFiles();
                    device.FileLogStatus = fileLogs.Length == 0
                        ? (short)LogFileDeviceStatus.NoFile
                        : fileLogs.Any(m => m.CreationTimeUtc.Date == DateTime.UtcNow.Date)
                            ? (short)LogFileDeviceStatus.NewFile
                            : (short)LogFileDeviceStatus.OldFile;
                }
                dataResult.Add(device);
            }

            return new EnumerableQuery<DeviceListModel>(dataResult);
        }

        public IQueryable<IcuDevice> GetPaginatedDevices(int pageNumber, int pageSize,
            out int totalRecords, out int recordsFiltered, List<short> operationTypes, List<int> companyId, List<int> connectionStatus)
        {
            var data = _unitOfWork.AppDbContext.IcuDevice
                .Include(m => m.ActiveTz)
                .Include(m => m.PassageTz)
                .Include(m => m.Building)
                .Include(m => m.Company)
                .Where(m => !m.IsDeleted);

            if (_httpContext.User.GetAccountType() != (short)AccountType.SystemAdmin)
            {
                data = data.Where(m => m.CompanyId == _httpContext.User.GetCompanyId());
            }

            totalRecords = data.Count();

            if (operationTypes != null && !operationTypes.Any())
            {
                data = data.Where(x => operationTypes.Contains(x.OperationType));
            }

            if (companyId.Count() > 0)
            {
                data = data.Where(x => companyId.Contains(Convert.ToInt32(x.CompanyId.Value)));
            }
            if (connectionStatus.Count() > 0)
            {
                data = data.Where(x => connectionStatus.Contains(Convert.ToInt32(x.ConnectionStatus)));
            }

            recordsFiltered = data.Count();

            // Default sort ( asc - Device Address )
            data = data.OrderBy(c => c.DeviceAddress);

            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var accountTimezone = _unitOfWork.AccountRepository.Get(m =>
                    m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted).TimeZone;
            var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

            foreach (var device in data)
            {
                //device.LastCommunicationTime = Helpers.ConvertToUserTime(device.LastCommunicationTime, accountTimezone);
                device.LastCommunicationTime = Helpers.ConvertToUserTime(device.LastCommunicationTime, offSet);
            }
            return data;
        }

        public IQueryable<IcuDevice> GetDevicesForDashBoard(short operationType, List<int> companyId)
        {
            var data = _unitOfWork.AppDbContext.IcuDevice
                .Include(m => m.ActiveTz)
                .Include(m => m.PassageTz)
                .Include(m => m.Building)
                .Include(m => m.Company)
                .Where(m => !m.IsDeleted);

            if (_httpContext.User.GetAccountType() != (short)AccountType.SystemAdmin)
            {
                if (companyId == null) companyId = new List<int>();
                companyId.Add(_httpContext.User.GetCompanyId());
            }


            var accountTypeTemp = _httpContext.User.GetAccountType();
            var accountIdTemp = _httpContext.User.GetAccountId();
            var companyIdTemp = _httpContext.User.GetCompanyId();

            if (accountTypeTemp != (short)AccountType.SystemAdmin)
            {
                data = data.Where(m => m.CompanyId == companyIdTemp);
            }

            // check plugin
            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyIdTemp);
            if(plugin != null)
            {

                PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
                if (plugIns.DepartmentAccessLevel && accountTypeTemp == (short)AccountType.DynamicRole)
                {
                    List<int> doorIdList = _unitOfWork.DepartmentDeviceRepository.GetDoorIdsByAccountDepartmentRole(companyIdTemp, accountIdTemp);
                    data = data.Where(x => doorIdList.Contains(x.Id));
                }
            }

            data = data.Where(x => x.OperationType == operationType);

            if (companyId.Any())
            {
                data = data.Where(x => x.CompanyId.HasValue && companyId.Contains(x.CompanyId.Value));
            }

            // Default sort ( asc - Device Address )
            data = data.OrderBy(c => c.DeviceAddress);

            return data;
        }


        public void UpdateUpTimeToDevice(int deviceId)
        {
            string deviceAddress = "";

            try
            {
                var start = DateTime.Now;
                var device = _unitOfWork.IcuDeviceRepository.GetByIcuId(deviceId);
                deviceAddress = device.DeviceAddress;

                // event log online/offline of icu from CreateTimeOnlineDevice to Now
                List<EventLog> eventLogData = _unitOfWork.IcuDeviceRepository.GetEventLogData(deviceId).ToList();
                var lastSuccessTimes = _unitOfWork.AppDbContext.EventLog.Where(x => x.IcuId == device.Id && x.EventType == (short)EventType.CommunicationSucceed);
                EventLog lastSuccessTime = null;
                if (lastSuccessTimes.Any())
                {
                    lastSuccessTime = new EventLog()
                    {
                        Id = lastSuccessTimes.First().Id,
                        IcuId = lastSuccessTimes.First().IcuId,
                        EventTime = lastSuccessTimes.Max(m => m.EventTime)
                    };
                }
                string createTimeOnlineDevice = "";
                //Get Monitoring up time to the device
                List<MonitoringUpTimeDevice> listUpTime = new List<MonitoringUpTimeDevice>();

                float count = 0;
                double timeOnline = 0;
                if (eventLogData.Any())
                {
                    var listEventLogData = eventLogData.Where(x => x.EventTime >= x.CreatedOn).OrderBy(x => x.EventTime).ToList();
                    for (int i = 0; i < eventLogData.Count; i++)
                    {
                        var item = listEventLogData[i];

                        var checkData = listUpTime.OrderByDescending(x => x.id).FirstOrDefault();
                        if (checkData != null && checkData.TimeFailed == null)
                        {
                            if (item.EventType == (short)EventType.CommunicationFailed)
                            {
                                checkData.TimeFailed = Convert.ToDateTime(item.EventTime);
                            }
                        }
                        else
                        {
                            if (item.EventType == (short)EventType.CommunicationSucceed)
                            {
                                MonitoringUpTimeDevice timeUp = new MonitoringUpTimeDevice();
                                timeUp.id = checkData == null ? (i + 1) : (checkData.id + 1);
                                timeUp.TimeSuccess = Convert.ToDateTime(item.EventTime);
                                timeUp.TimeFailed = null;
                                listUpTime.Add(timeUp);
                            }
                        }
                    }

                    var exitsFailed = listUpTime.FirstOrDefault(x => x.TimeFailed == null);
                    var exitsSuccess = listUpTime.OrderBy(x => x.id).FirstOrDefault();

                    if (exitsFailed != null)
                    {
                        exitsFailed.TimeFailed = device.LastCommunicationTime;
                    }
                    else
                    {
                        if (device.ConnectionStatus == (short)ConnectionStatus.Online && exitsSuccess != null)
                        {
                            _logger.LogWarning("##-## DeviceAddress : " + deviceAddress);
                            _logger.LogWarning("##-## listUpTime is null ? : " + !listUpTime.Any());
                            _logger.LogWarning("##-## exitsSuccess is null ? : " + ((exitsSuccess == null) ? "Null" : "Not Null"));
                            _logger.LogWarning("##-## exitsSuccess.TimeSuccess : " + exitsSuccess.TimeSuccess);
                            _logger.LogWarning("##-## VALUE : " + Math.Abs(Convert.ToDateTime(DateTime.Now).Subtract(Convert.ToDateTime(exitsSuccess.TimeSuccess)).TotalMinutes));

                            timeOnline = Math.Abs(Convert.ToDateTime(DateTime.Now).Subtract(Convert.ToDateTime(exitsSuccess.TimeSuccess)).TotalMinutes);
                        }
                    }
                    var lstUpTime = listUpTime.ToList();

                    foreach (var item in lstUpTime)
                    {
                        var countUpTimeDevice = Math.Abs(Convert.ToDateTime(item.TimeFailed).Subtract(Convert.ToDateTime(item.TimeSuccess)).TotalMinutes);
                        count += (float)countUpTimeDevice;
                    }

                }
                else
                {
                    if (device.ConnectionStatus == (short)ConnectionStatus.Online && lastSuccessTime != null)
                    {
                        if (string.IsNullOrEmpty(device.CreateTimeOnlineDevice))
                        {
                            timeOnline = Math.Abs(Convert.ToDateTime(DateTime.Now).Subtract(device.CreatedOn).TotalMinutes);
                        }

                        else if (Convert.ToDateTime(device.CreateTimeOnlineDevice) < lastSuccessTime.EventTime)
                        {
                            timeOnline = Math.Abs(Convert.ToDateTime(DateTime.Now).Subtract(lastSuccessTime.EventTime).TotalMinutes);

                        }

                        else if (Convert.ToDateTime(device.CreateTimeOnlineDevice) > lastSuccessTime.EventTime)
                        {
                            timeOnline = Math.Abs(Convert.ToDateTime(DateTime.Now).Subtract(Convert.ToDateTime(device.CreateTimeOnlineDevice)).TotalMinutes);

                        }

                    }
                    createTimeOnlineDevice = DateTime.Now.ToString();
                }
                device.CreateTimeOnlineDevice = createTimeOnlineDevice == "" ? DateTime.Now.ToString() : createTimeOnlineDevice;
                device.UpTimeOnlineDevice = device.UpTimeOnlineDevice + ((int)Math.Ceiling(count)) + Convert.ToInt32(timeOnline);

                _unitOfWork.IcuDeviceRepository.Update(device);
                _unitOfWork.Save();
                var end = DateTime.Now;
            }
            catch (Exception e)
            {
                _logger.LogError("!! EXCEPTION !! - Update UpTime\n##_DeviceAddress : " + deviceAddress + "\nCAUSED BY : " + e.InnerException + "\nWhere? : " + e.StackTrace);
            }
        }


        public double GetDeviceOffTimeInLast24Hours(int deviceId, out int count)
        {
            count = 0;

            //TODO: return offline time
            List<EventLog> eventLogData = _unitOfWork.IcuDeviceRepository.GetRecentEventLogData(deviceId).ToList();
            double timeOffline = 0;
            EventLog checkpoint = null;
            if (eventLogData.Count() > 0)
            {
                for (int i = 0; i < eventLogData.Count; i++)
                {
                    EventLog eventLog = eventLogData[i];
                    if (eventLog.EventType == (short)EventType.CommunicationFailed)
                    {
                        count++;

                        if (checkpoint == null)
                        {
                            checkpoint = eventLog;
                        }
                    }
                    else
                    {
                        if (checkpoint != null)
                        {
                            if (eventLog.EventTime > checkpoint.EventTime)
                            {
                                timeOffline += eventLog.EventTime.Subtract(checkpoint.EventTime).TotalSeconds;
                                checkpoint = null;
                            }
                        }

                    }

                }
                // If the last event is communication failed then we added time from the last event to now as off time
                if (checkpoint != null)
                {
                    timeOffline += DateTime.Now.Subtract(checkpoint.EventTime).TotalSeconds;
                }
            }

            return timeOffline;
        }

        public int ReUpdateUpTimOnlineDevice()
        {
            int Count = 0;
            _unitOfWork.IcuDeviceRepository.ReUpdateUpTimOnlineDevice();
            foreach (var item in _unitOfWork.AppDbContext.IcuDevice.Where(c => c.IsDeleted == false))
            {
                UpdateUpTimeToDevice(item.Id);
                Count++;
            }
            return Count;
        }
        public void ReUpdateUpTimOnlineDeviceById(int id)
        {
            _unitOfWork.IcuDeviceRepository.ReUpdateUpTimOnlineDeviceById(id);
            UpdateUpTimeToDevice(id);
        }


        // Return list of device that disconnected in the last 24 hours
        // Inlude total disconnect time, current status

        public IEnumerable<RecentlyDisconnectedDeviceModel> getRecentlyDisconnectedDevices(int pageNumber, int pageSize, int sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            IEnumerable<RecentlyDisconnectedDeviceModel> devices = _unitOfWork.IcuDeviceRepository.getRecentlyDisconnectedDevices();

            foreach (RecentlyDisconnectedDeviceModel device in devices)
            {
                int count = 0;
                device.TotalOfflineSeconds = GetDeviceOffTimeInLast24Hours(device.Id, out count);
                device.Count = count;
            }
            totalRecords = devices.Count();

            recordsFiltered = devices.Count();
            devices = devices.OrderByDescending(x => x.TotalOfflineSeconds);

            devices = devices.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return devices;
        }
        
        public void ChangedConnectionStatus(IcuDevice device, short status, IModel channel = null, List<string> columns = null)
        {
            if (device == null) return;

            if (status == (short)ConnectionStatus.Online)
            {
                device.LastCommunicationTime = DateTime.UtcNow;
                //_unitOfWork.IcuDeviceRepository.Update(device);

                if (columns != null && columns.Any())
                    _unitOfWork.IcuDeviceRepository.UpdateSpecific(device, columns);
                else
                    _unitOfWork.IcuDeviceRepository.Update(device);
            }

            if (device.ConnectionStatus == status)
            {
                // nothing to do in current.
                _sendMessageService.SendDeviceStatusToFE(device, device.DoorStatus ?? "", channel, _unitOfWork);
            }
            else
            {
                // To do in here.
                // 1. Update connection status
                // 2. Send connection status to FE
                // 3. Make eventLog
                // 4. Send communication eventLog to FE
                // 5. Make and send notification

                // ================= 1. Update connection status =================
                device.ConnectionStatus = (short)status;
                if (columns != null && columns.Any())
                    _unitOfWork.IcuDeviceRepository.UpdateSpecific(device, columns);
                else
                    _unitOfWork.IcuDeviceRepository.Update(device);

                //////////////////////////////////////////////////////////////////

                // ================= 2. Send connection status to FE =================
                _sendMessageService.SendDeviceStatusToFE(device, device.DoorStatus ?? "", channel, _unitOfWork);
                //////////////////////////////////////////////////////////////////////

                if (device.CompanyId != null)
                {
                    // ================= 3. Make eventLog =================
                    var eventType = EventType.CommunicationSucceed;
                    if (status == (short)ConnectionStatus.Offline)
                    {
                        eventType = EventType.CommunicationFailed;
                    }

                    var eventLog = new EventLog
                    {
                        Icu = device,
                        IcuId = device.Id,
                        DoorName = device.Name,
                        CompanyId = device.CompanyId,
                        EventType = (int)eventType,
                        EventTime = DateTime.UtcNow
                    };
                    _unitOfWork.EventLogRepository.Add(eventLog);
                    ////////////////////////////////////////////////////// 

                    // ================= 4. Send communication eventLog to FE =================
                    var eventLogDetail = _mapper.Map<EventLogDetailModel>(eventLog);
                    ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
                    {
                        Type = Constants.SDKDevice.WebhookEventLogType,
                        Data = eventLogDetail,
                    }));
                    ///////////////////////////////////////////////////////////////////////////

                    // ================= 5. Pushing notification by setting =================
                    // HelperService helperService = new HelperService(_configuration);
                    // helperService.PushNotificationSettingByEventLogsAsync(device, new List<EventLog>() { eventLog });
                    ///////////////////////////////////////////////////////////////////////////
                }
                else
                {
                    _logger.LogInformation("This device doesn't have any company. We cannot make eventlog and notification.");
                }
            }

            _unitOfWork.Save();
        }

        public short CheckPermissionUserOrVisitOpenDoor(User user, Visit visit, IcuDevice device, DateTime timeUtc, Card vehicleCard = null, bool bothCheckVehicle = false)
        {
            if (device.VerifyMode == (short)VerifyMode.VehicleAndFace)
            {
                if (bothCheckVehicle)
                {
                    if (vehicleCard == null)
                    {
                        return (short)EventType.UnregisteredVehicle;
                    }
                    else
                    {
                        if (user != null && vehicleCard.UserId != user.Id)
                        {
                            return (short)EventType.FailTwoFactorAuth;
                        }
                    
                        if (visit != null && vehicleCard.VisitId != visit.Id)
                        {
                            return (short)EventType.FailTwoFactorAuth;
                        }
                    }
                }
                else
                {
                    return (short)EventType.FailTwoFactorAuth;
                }
            }
            
            
            AccessGroupDevice accessGroupDevice;
            // check expired user or visit
            if (user != null)
            {
                if (user.ExpiredDate < timeUtc)
                {
                    _logger.LogWarning($"User {user.Id} - {user.FirstName} expired!");
                    return (short)EventType.ExpirationDate;
                }
                accessGroupDevice = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupIdAndDeviceId(user.CompanyId, user.AccessGroupId, device.Id);
            }
            else if (visit != null)
            {
                if (visit.StartDate <= timeUtc && timeUtc <= visit.EndDate)
                {
                    accessGroupDevice = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupIdAndDeviceId(visit.CompanyId, visit.AccessGroupId, device.Id);
                }
                else if (timeUtc < visit.StartDate)
                {
                    _logger.LogWarning($"Visitor {visit.Id} - {visit.VisitorName} effective date not started!");
                    return (short)EventType.EffectiveDateNotStarted;
                }
                else
                {
                    _logger.LogWarning($"Visitor {visit.Id} - {visit.VisitorName} expired!");
                    return (short)EventType.ExpirationDate;
                }
            }
            else
            {
                accessGroupDevice = null;
            }

            // check time permission
            if (accessGroupDevice != null)
            {
                var accessTime = _unitOfWork.AccessTimeRepository.GetById(accessGroupDevice.TzId);
                var building = _unitOfWork.BuildingRepository.GetById(device.BuildingId.Value);

                TimeZoneInfo zone = building.TimeZone.ToTimeZoneInfo();

                var timeInDevice = TimeZoneInfo.ConvertTimeFromUtc(timeUtc.ToUniversalTime(), zone);
                var minuteInDay = timeInDevice.Hour * 60 + timeInDevice.Minute;

                string[] dayTimes = { "SunTime", "MonTime", "TueTime", "WedTime", "ThurTime", "FriTime", "SatTime" };
                string dayTime = dayTimes[(short)timeInDevice.DayOfWeek];
                short eventType = (short)EventType.ExpirationDate;
                for (int i = 1; i <= 4; i++)
                {
                    var jsonTime = accessTime.GetType().GetProperty(dayTime + i)?.GetValue(accessTime);
                    if (jsonTime != null)
                    {
                        var detailTime = JsonConvert.DeserializeObject<DayDetail>(jsonTime.ToString());
                        if (detailTime.From <= minuteInDay && detailTime.To >= minuteInDay)
                        {
                            eventType = (short)EventType.NormalAccess;
                            break;
                        }
                        else if (minuteInDay < detailTime.From)
                        {
                            eventType = (short)EventType.EffectiveDateNotStarted;
                        }
                        else
                        {
                            eventType = (short)EventType.ExpirationDate;
                        }
                    }
                }

                return eventType;
            }
            else
            {
                _logger.LogWarning($"Access Group Device null for user {user?.FirstName} ({user?.Id})");
                return (short)EventType.NoDoorActiveTime;
            }
        }
        
        public List<IcuDevice> GetDevicesByBuildingIds(List<int> buildingIds, int companyId, bool includeChildren = false)
        {
            var devices = _unitOfWork.IcuDeviceRepository.GetDevicesByBuildingIds(buildingIds, companyId).ToList();

            if (includeChildren)
            {
                var childBuildingIds = _unitOfWork.AppDbContext.Building.Where(m => m.ParentId != null && buildingIds.Contains(m.ParentId.Value)).Select(m => m.Id).ToList();
                if (childBuildingIds.Any())
                {
                    childBuildingIds = childBuildingIds.Except(buildingIds).ToList();

                    var childDevices = GetDevicesByBuildingIds(childBuildingIds, companyId, includeChildren);

                    childDevices = childDevices.Except(devices).ToList();

                    devices.AddRange(childDevices);
                }
            }

            return devices;
        }
        
        public SubDisplayDeviceModel GetSubDisplayDeviceInfoByToken(string tokenDevice)
        {
            var jwt = new JwtSecurityTokenHandler().ReadToken(tokenDevice) as JwtSecurityToken;
            if (jwt == null)
            {
                throw new Exception("JWT NULL");
            }

            var listDeviceAddress = JsonConvert.DeserializeObject<List<string>>(jwt.Claims
                .First(claim => claim.Type == Constants.ClaimName.ListDeviceAddress).Value);
            SubDisplayDeviceModel data = new SubDisplayDeviceModel() { PersonInfos = new List<SubDisplayPerson>() };
            
            foreach (var item in listDeviceAddress)
            {
                try
                {
                    var eventLog = _unitOfWork.AppDbContext.EventLog
                        //.Include(m => m.Icu)
                        .Include(m => m.Icu).ThenInclude(n => n.Building)
                        .Include(m => m.User).ThenInclude(n => n.Department)
                        .Include(m => m.User)
                        .Include(m => m.Visit)
                        .OrderByDescending(m => m.EventTime)
                        .FirstOrDefault(m =>
                            m.Icu.DeviceAddress == item &&
                            (m.Antipass.ToLower() == "in" || m.Antipass.ToLower() == "out"));
                    if (eventLog != null)
                    {
                        var info = GetLastEventInfo(eventLog);
                        if (info == null)
                            throw new Exception("Event log null");
                        else
                            data.PersonInfos.Add(info);
                    }
                    else
                    {
                        var icuEventNull = _unitOfWork.IcuDeviceRepository.GetDeviceByAddress(item);
                        data.PersonInfos.Add(new SubDisplayPerson()
                        {
                            DeviceAddress = item,
                            DeviceName = icuEventNull?.Name,
                            EventTime = DateTime.MinValue
                        });
                    }
                }
                catch (Exception e)
                {
                    _logger.LogWarning("item: " + e.Message);
                    _logger.LogWarning("item: " + e.StackTrace);
                    var icuError = _unitOfWork.IcuDeviceRepository.GetDeviceByAddress(item);
                    data.PersonInfos.Add(new SubDisplayPerson()
                    {
                        DeviceAddress = item,
                        DeviceName = icuError?.Name,
                        EventTime = DateTime.MinValue
                    });
                }
            }

            return data;
        }
        
        private SubDisplayPerson GetLastEventInfo(EventLog eventLog)
        {
            if (eventLog == null) return null;

            // Get image
            string image = "";
            if (eventLog.CardType == (short)CardType.VehicleId || eventLog.CardType == (short)CardType.VehicleMotoBikeId)
            {
                if (!string.IsNullOrEmpty(eventLog.ImageCamera))
                {
                    var vehicleImage = JsonConvert.DeserializeObject<List<DataImageCamera>>(eventLog.ImageCamera);
                    image = vehicleImage.Any() ? vehicleImage.First().Link : "";
                }
            }
            else
            {
                image = eventLog.IsVisit ? eventLog.Visit?.Avatar : eventLog.User?.Avatar;
            }

            // Get other information
            string deptName = "";
            string name = "";
            string plateNumber = "";
            List<string> visitTypes = new List<string>();
            if (eventLog.VisitId != null)
            {
                name = eventLog.Visit?.VisitorName;
                deptName = eventLog.Visit?.VisiteeDepartment;

                if (eventLog.Visit != null && int.TryParse(eventLog.Visit.VisitType, out var numberVisitType))
                {
                    visitTypes = EnumHelper.GetAllDescriptions<VisitArmyType>(numberVisitType, VisitResource.ResourceManager);
                    if (!visitTypes.Any())
                    {
                        visitTypes.Add(((VisitArmyType)numberVisitType).GetDescription());
                    }
                }
            }
            else if (eventLog.UserId != null)
            {
                name = eventLog.User?.FirstName;
                deptName = eventLog.User?.Department?.DepartName;
            }

            if (eventLog.CardType == (short)CardType.VehicleId || eventLog.CardType == (short)CardType.VehicleMotoBikeId)
            {
                plateNumber = eventLog.CardId;
            }
            Enum.TryParse(eventLog.Antipass, true, out Antipass inOutType);

            return new SubDisplayPerson()
            {
                PlateNumber = plateNumber,
                Position = eventLog.IsVisit ? eventLog.Visit?.Position : eventLog.User?.Position,
                Name = name,
                Department = deptName,
                Building = eventLog.Icu.Building.Name,
                EventTime = eventLog.EventTime,
                Avatar = image,
                DeviceAddress = eventLog.Icu.DeviceAddress,
                DeviceName = eventLog.Icu.Name,
                EventTypeDescriptions = _unitOfWork.EventRepository.GetAllEventTypeByNumber(eventLog.EventType).Select(m => new EventModel { Culture = m.Culture, EventName = m.EventName }).ToList(),
                EventType = eventLog.EventType, 
                InOut = eventLog.Antipass,
                InOutType = (short)inOutType,

                IsVisit = eventLog.IsVisit,
                VisitorName = eventLog.Visit?.VisitorName,
                VisitorReason = eventLog.Visit?.VisitReason,
                VisitorTypes = visitTypes,
            };
        }

        public DataTokenScreenMonitoring GetDataByTokenMonitoring(string token)
        {
            var jwt = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            if (jwt == null)
            {
                return null;
            }

            try
            {
                DataTokenScreenMonitoring data = new DataTokenScreenMonitoring();
                var claimTypes = jwt.Claims.Select(m => m.Type).ToList();
                if (claimTypes.Contains(Constants.ClaimName.CompanyId))
                    data.CompanyId = JsonConvert.DeserializeObject<int>(jwt.Claims.First(claim => claim.Type == Constants.ClaimName.CompanyId).Value);
                if (claimTypes.Contains(Constants.ClaimName.DeviceIds))
                    data.DeviceIds = JsonConvert.DeserializeObject<List<int>>(jwt.Claims.First(claim => claim.Type == Constants.ClaimName.DeviceIds).Value);
                if (claimTypes.Contains(Constants.ClaimName.Timezone))
                    data.Timezone = jwt.Claims.First(claim => claim.Type == Constants.ClaimName.Timezone).Value;
                if (claimTypes.Contains(Constants.ClaimName.TimeReset))
                    data.TimeReset = jwt.Claims.First(claim => claim.Type == Constants.ClaimName.TimeReset).Value;
                if (claimTypes.Contains(Constants.ClaimName.TimeStartCheckIn))
                    data.TimeStartCheckIn = jwt.Claims.First(claim => claim.Type == Constants.ClaimName.TimeStartCheckIn).Value;
                if (claimTypes.Contains(Constants.ClaimName.TimeEndCheckIn))
                    data.TimeEndCheckIn = jwt.Claims.First(claim => claim.Type == Constants.ClaimName.TimeEndCheckIn).Value;
                if (claimTypes.Contains(Constants.ClaimName.TimeStartCheckOut))
                    data.TimeStartCheckOut = jwt.Claims.First(claim => claim.Type == Constants.ClaimName.TimeStartCheckOut).Value;
                if (claimTypes.Contains(Constants.ClaimName.TimeEndCheckOut))
                    data.TimeEndCheckOut = jwt.Claims.First(claim => claim.Type == Constants.ClaimName.TimeEndCheckOut).Value;
                if (claimTypes.Contains(Constants.ClaimName.IsCheckTeacherOut))
                    data.IsCheckTeacherOut = JsonConvert.DeserializeObject<bool>(jwt.Claims.First(claim => claim.Type == Constants.ClaimName.IsCheckTeacherOut).Value);
                if (claimTypes.Contains(Constants.ClaimName.ParentDepartment))
                    data.ParentDepartment = JsonConvert.DeserializeObject<List<int>>(jwt.Claims.First(claim => claim.Type == Constants.ClaimName.ParentDepartment).Value);
                var claimEnableDisplayListVisitor = jwt.Claims.FirstOrDefault(claim => claim.Type == Constants.ClaimName.EnableDisplayListVisitor);
                if (claimEnableDisplayListVisitor != null)
                    data.EnableDisplayListVisitor = bool.Parse(claimEnableDisplayListVisitor.Value);
                var devices = _unitOfWork.IcuDeviceRepository.GetByIds(data.DeviceIds).Where(m => m.CompanyId == data.CompanyId);
                if (devices.Count() != data.DeviceIds.Count())
                {
                    return null;
                }

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }

            return null;
        }

        public CheckLimitAddedModel CheckLimitDevicesAdded(int numberOfAdded)
        {
            return _unitOfWork.SystemInfoRepository.CheckLimitDevicesAdded(numberOfAdded);
        }
        
        public string AssignUsersToDevice(int deviceId, List<int> userIds)
        {
            string resultString = "";
            var companyId = _httpContext.User.GetCompanyId();
            var accessTime = _unitOfWork.AccessTimeRepository.GetDefaultTzByCompanyId(Constants.Tz24hPos, companyId);
            var currentTime = DateTime.UtcNow;
            var currentAccountId = _httpContext.User.GetAccountId();

            var usersToAssign = _unitOfWork.UserRepository.GetByIds(companyId, userIds);

            var device = _unitOfWork.IcuDeviceRepository.GetByIcuId(deviceId);

            foreach (var eachUser in usersToAssign)
            {
                _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
                {
                    using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var accessGroup = eachUser.AccessGroup;

                            if (accessGroup == null)
                            {
                                accessGroup = _unitOfWork.AccessGroupRepository.GetById(eachUser.AccessGroupId);
                            }

                            if (accessGroup.Type != (short)AccessGroupType.PersonalAccess)
                            {
                                AccessGroup newPAG = new AccessGroup()
                                {
                                    CompanyId = companyId,
                                    CreatedBy = currentAccountId,
                                    UpdatedBy = currentAccountId,
                                    CreatedOn = currentTime,
                                    UpdatedOn = currentTime,
                                    IsDefault = false,
                                    Type = (short)AccessGroupType.PersonalAccess,
                                    Name = Constants.Settings.NameAccessGroupPersonal + eachUser.Id,
                                    ParentId = accessGroup.Id,
                                    IsDeleted = false,
                                };

                                _unitOfWork.AccessGroupRepository.Add(newPAG);
                                _unitOfWork.Save();

                                eachUser.AccessGroupId = newPAG.Id;
                                _unitOfWork.UserRepository.Update(eachUser);
                                _unitOfWork.Save();
                            }

                            if (!eachUser.AccessGroup.AccessGroupDevice.Where(m => m.IcuId == deviceId).Any())
                            {
                                AccessGroupDevice newAGD = new AccessGroupDevice()
                                {
                                    AccessGroupId = eachUser.AccessGroupId,
                                    CreatedBy = currentAccountId,
                                    UpdatedBy = currentAccountId,
                                    CreatedOn = currentTime,
                                    UpdatedOn = currentTime,
                                    IcuId = deviceId,
                                    TzId = accessTime.Id
                                };

                                _unitOfWork.AccessGroupDeviceRepository.Add(newAGD);
                            }

                            // Save system log
                            var contents = $"{ActionLogTypeResource.AssignUser} : {eachUser.FirstName}";
                            var contentsDetail = $"{DeviceResource.lblDoorName} : {device.Name}";

                            _unitOfWork.SystemLogRepository.Add(deviceId, SystemLogType.RegisteredUser,
                            ActionLogType.AssignUser, contents, contentsDetail, null, companyId);

                            _unitOfWork.Save();

                            transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            _logger.LogError($"[ERROR] (Assign user to device) message : {e.Message}");
                            _logger.LogError($"[Error] (Assign user to device) message(inner) : {e.InnerException?.Message}");
                            _logger.LogError($"[Error] (Assign user to device) location : {e.StackTrace}");

                            transaction.Rollback();
                        }
                    }
                });
            }

            var groupMsgId = Guid.NewGuid().ToString();
            var logs = _accessGroupService.MakeUserLogData(device, userIds, null);

            // Send ADD
            _accessGroupService.SendAddOrDeleteUser(device.DeviceAddress, userLogs: logs, isAddUser: true);

            return resultString;
        }
        
        public string UnassignUsersFromDevice(int deviceId, List<int> userIds, bool ok)
        {
            string resultString = "";
            List<string> names = new List<string>();
            var companyId = _httpContext.User.GetCompanyId();
            var accessTime = _unitOfWork.AccessTimeRepository.GetDefaultTzByCompanyId(Constants.Tz24hPos, companyId);
            var currentTime = DateTime.UtcNow;
            var currentAccountId = _httpContext.User.GetAccountId();

            var usersToUnassign = _unitOfWork.UserRepository.GetByIds(companyId, userIds);

            foreach (var eachUser in usersToUnassign)
            {
                _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
                {
                    using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var accessGroup = eachUser.AccessGroup;
                            List<int> accessGroupDeviceIds;
                            AccessGroupDevice accessGroupDevice = null;

                            if (accessGroup == null)
                            {
                                accessGroup = _unitOfWork.AccessGroupRepository.GetById(eachUser.AccessGroupId);
                                accessGroupDevice = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupIdAndDeviceId(companyId, eachUser.AccessGroupId, deviceId);
                                accessGroupDeviceIds = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, eachUser.AccessGroupId, null, false).Select(m => m.IcuId).ToList();
                            }
                            else
                            {
                                accessGroupDevice = eachUser.AccessGroup.AccessGroupDevice.FirstOrDefault(m => m.IcuId == deviceId && m.AccessGroupId == eachUser.AccessGroupId);
                                accessGroupDeviceIds = eachUser.AccessGroup.AccessGroupDevice.Select(m => m.IcuId).ToList();
                            }

                            if (accessGroup.Type == (short)AccessGroupType.PersonalAccess)
                            {
                                if (accessGroupDevice == null)
                                {
                                    if (!ok)
                                    {
                                        names.Add(eachUser.FirstName);

                                        if (names.Count >= 5)
                                        {
                                            resultString = string.Format(MessageResource.ChangedAccessGroup, $"({string.Join(", ", names)} {CommonResource.Exceed})");
                                        }
                                        else
                                        {
                                            resultString = string.Format(MessageResource.ChangedAccessGroup, $"({string.Join(", ", names)})");
                                        }
                                    }
                                    else
                                    {
                                        // If 'accessGroupDevice' is NULL, this means that this device is in Parent's AG.
                                        // 1. Check whether there is an type 0 accecc group except No Access Group.
                                        // 2. Create a new type 0 access group if there is not type 0 access group.
                                        // 3. Update PAG's parentId as type 0 access group.
                                        // 4. 

                                        var noAccessGroups = _unitOfWork.AppDbContext.AccessGroup.Where(m => m.Type == (short)AccessGroupType.NoAccess && m.CompanyId == companyId).ToList();
                                        if (noAccessGroups.Count == 1)
                                        {
                                            // Create a new type 0 access group if there is not type 0 access group.
                                            AccessGroup newNoAccessGroup = new AccessGroup()
                                            {
                                                CompanyId = companyId,
                                                CreatedBy = currentAccountId,
                                                UpdatedBy = currentAccountId,
                                                CreatedOn = currentTime,
                                                UpdatedOn = currentTime,
                                                IsDeleted = false,
                                                Type = (short)AccessGroupType.NoAccess,
                                                IsDefault = false,
                                                Name = AccessGroupResource.lblCustomGroup
                                            };

                                            _unitOfWork.AccessGroupRepository.Add(newNoAccessGroup);
                                            _unitOfWork.Save();

                                            noAccessGroups.Add(newNoAccessGroup);
                                        }

                                        var customNAG = noAccessGroups.Last();

                                        var parentAG = _unitOfWork.AccessGroupRepository.GetByIdAndCompanyId(companyId, accessGroup.ParentId.Value);
                                        foreach (var parentAGD in parentAG.AccessGroupDevice)
                                        {
                                            if (parentAGD.IcuId == deviceId)
                                            {
                                                continue;
                                            }

                                            if (accessGroupDeviceIds.Contains(parentAGD.IcuId))
                                            {
                                                continue;
                                            }

                                            AccessGroupDevice newPAGD = new AccessGroupDevice()
                                            {
                                                CreatedBy = currentAccountId,
                                                UpdatedBy = currentAccountId,
                                                CreatedOn = currentTime,
                                                UpdatedOn = currentTime,
                                                AccessGroupId = accessGroup.Id,
                                                IcuId = parentAGD.IcuId,
                                                TzId = parentAGD.TzId
                                            };

                                            _unitOfWork.AccessGroupDeviceRepository.Add(newPAGD);
                                            _unitOfWork.Save();
                                        }

                                        accessGroup.ParentId = customNAG.Id;
                                        _unitOfWork.AccessGroupRepository.Update(accessGroup);

                                        _unitOfWork.Save();
                                    }
                                }
                                else
                                {
                                    if (ok)
                                    {
                                        // If 'accessGroupDevice' is NOT NULL, this means that this device is in PAGD. So, API just remove this accessGroupDevice.
                                        _unitOfWork.AccessGroupDeviceRepository.Delete(accessGroupDevice);

                                        _unitOfWork.Save();
                                    }
                                }
                            }
                            else
                            {
                                if (!ok)
                                {
                                    names.Add(eachUser.FirstName);

                                    if (names.Count >= 5)
                                    {
                                        resultString = string.Format(MessageResource.ChangedAccessGroup, $"({string.Join(", ", names)} {CommonResource.Exceed})");
                                    }
                                    else
                                    {
                                        resultString = string.Format(MessageResource.ChangedAccessGroup, $"({string.Join(", ", names)})");
                                    }
                                }
                                else
                                {
                                    // If this user is using normal AG, API should make PAG for this user.
                                    var noAccessGroups = _unitOfWork.AppDbContext.AccessGroup.Where(m => m.Type == (short)AccessGroupType.NoAccess && m.CompanyId == companyId).ToList();
                                    if (noAccessGroups.Count == 1)
                                    {
                                        // Create a new type 0 access group if there is not type 0 access group.
                                        AccessGroup newNoAccessGroup = new AccessGroup()
                                        {
                                            CompanyId = companyId,
                                            CreatedBy = currentAccountId,
                                            UpdatedBy = currentAccountId,
                                            CreatedOn = currentTime,
                                            UpdatedOn = currentTime,
                                            IsDeleted = false,
                                            Type = (short)AccessGroupType.NoAccess,
                                            IsDefault = false,
                                            Name = AccessGroupResource.lblCustomGroup
                                        };

                                        _unitOfWork.AccessGroupRepository.Add(newNoAccessGroup);
                                        _unitOfWork.Save();

                                        noAccessGroups.Add(newNoAccessGroup);
                                    }

                                    var customNAG = noAccessGroups.Last();

                                    AccessGroup newPAG = new AccessGroup()
                                    {
                                        CompanyId = companyId,
                                        CreatedBy = currentAccountId,
                                        UpdatedBy = currentAccountId,
                                        CreatedOn = currentTime,
                                        UpdatedOn = currentTime,
                                        IsDeleted = false,
                                        Type = (short)AccessGroupType.PersonalAccess,
                                        IsDefault = false,
                                        Name = Constants.Settings.NameAccessGroupPersonal + eachUser.Id,
                                        ParentId = customNAG.Id
                                    };

                                    _unitOfWork.AccessGroupRepository.Add(newPAG);
                                    _unitOfWork.Save();

                                    foreach (var aGD in accessGroup.AccessGroupDevice)
                                    {
                                        if (aGD.IcuId == deviceId)
                                        {
                                            continue;
                                        }

                                        AccessGroupDevice newPAGD = new AccessGroupDevice()
                                        {
                                            CreatedBy = currentAccountId,
                                            UpdatedBy = currentAccountId,
                                            CreatedOn = currentTime,
                                            UpdatedOn = currentTime,
                                            AccessGroupId = newPAG.Id,
                                            IcuId = aGD.IcuId,
                                            TzId = aGD.TzId
                                        };

                                        _unitOfWork.AccessGroupDeviceRepository.Add(newPAGD);
                                        _unitOfWork.Save();
                                    }

                                    eachUser.AccessGroupId = newPAG.Id;
                                    _unitOfWork.UserRepository.Update(eachUser);

                                    _unitOfWork.Save();
                                }
                            }

                            if (ok)
                            {
                                // Save system log
                                var contents = $"{ActionLogTypeResource.UnassignUser} : {eachUser.FirstName}";
                                var device = _unitOfWork.IcuDeviceRepository.GetById(deviceId);
                                var contentsDetail = $"{DeviceResource.lblDoorName} : {device.Name}";

                                _unitOfWork.SystemLogRepository.Add(deviceId, SystemLogType.RegisteredUser,
                                ActionLogType.UnassignUser, contents, contentsDetail, null, companyId);

                                _unitOfWork.Save();
                            }

                            transaction.Commit();
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();

                            Console.WriteLine($"##-## [ERROR] {e.Message}");
                            Console.WriteLine($"##-## [ERROR] {e.InnerException?.Message}");
                        }
                    }
                });

                if (!ok && names.Count >= 5)
                {
                    break;
                }
            }

            if (!ok && names.Count == 0)
            {
                return UnassignUsersFromDevice(deviceId, userIds, true);
            }

            if (ok)
            {
                var groupMsgId = Guid.NewGuid().ToString();
                var device = _unitOfWork.IcuDeviceRepository.GetByIcuId(deviceId);

                List<UserLog> allUserLogs = new List<UserLog>();
                var cardTypes = Helpers.GetMatchedIdentificationType(device.DeviceType);

                foreach (var user in usersToUnassign)
                {
                    List<UserLog> userLogs = user.Card.Where(m => !m.IsDeleted && cardTypes.Contains(m.CardType)).Select(_mapper.Map<UserLog>).ToList();

                    allUserLogs.AddRange(userLogs);
                }

                var totalLogs = allUserLogs.SplitList(Helpers.GetMaxSplit(device.DeviceType));

                // Send DELETE. 
                _accessGroupService.SendAddOrDeleteUser(device.DeviceAddress, userLogs: totalLogs, isAddUser: false);
            }

            return resultString;
        }

        public List<DoorStatusModel> InitDoorStatus(IOrderedEnumerable<EnumModel> enumModels)
        {
            List<DoorStatusModel> result = new List<DoorStatusModel>();

            foreach (var enumItem in enumModels)
            {
                DoorStatusModel model = new DoorStatusModel()
                {
                    Id = enumItem.Id,
                    Name = enumItem.Name,
                    BackgroundColor = Helpers.GetBackgroundColorByDoorStatus(enumItem.Id, out string fontColorCode),
                    FontColor = fontColorCode
                };

                result.Add(model);
            }

            return result;
        }

        public void SetAttendanceReaders(bool inOut, List<int> deviceIds)
        {
            if (deviceIds == null) deviceIds = new List<int>();

            // Get attendanceSetting value from DB.
            var companyId = _httpContext.User.GetCompanyId();
            var attendanceSetting = _unitOfWork.AttendanceRepository.GetAttendanceSetting(companyId);

            // Update readers
            if (inOut)
                attendanceSetting.InReaders = JsonConvert.SerializeObject(deviceIds);
            else
                attendanceSetting.OutReaders = JsonConvert.SerializeObject(deviceIds);

            _unitOfWork.AttendanceRepository.UpdateAttendanceSetting(attendanceSetting);
            _unitOfWork.Save();
        }
        
        public List<EnumModel> GetAttendanceReaders()
        {
            // Get attendanceSetting value from DB.
            var companyId = _httpContext.User.GetCompanyId();
            var attendanceSetting = _unitOfWork.AttendanceRepository.GetAttendanceSetting(companyId);

            if (string.IsNullOrEmpty(attendanceSetting.InReaders)) attendanceSetting.InReaders = "[]";
            if (string.IsNullOrEmpty(attendanceSetting.OutReaders)) attendanceSetting.OutReaders = "[]";

            var inReaderIds = JsonConvert.DeserializeObject<List<int>>(attendanceSetting.InReaders);
            var outReaderIds = JsonConvert.DeserializeObject<List<int>>(attendanceSetting.OutReaders);

            var deviceIds = inReaderIds.Union(outReaderIds);
            var readers = _unitOfWork.IcuDeviceRepository.GetByIds(deviceIds.ToList());

            List<EnumModel> result = new List<EnumModel>();
            foreach (var readerId in inReaderIds)
            {
                var reader = readers.FirstOrDefault(d => d.Id == readerId);
                if (reader != null)
                    result.Add(new EnumModel()
                    {
                        Id = readerId,
                        Name = reader.Name
                    });
            }

            foreach (var readerId in outReaderIds)
            {
                var reader = readers.FirstOrDefault(d => d.Id == readerId);
                if (reader != null)
                    result.Add(new EnumModel()
                    {
                        Id = readerId,
                        Name = reader.Name
                    });
            }

            return result;
        }
        
        public List<VehicleInOutStatusListModel> GetInOutStatus(List<EventLogByWorkType> data, string search, int pageNumber, int pageSize, string sortColumn, string sortDirection, out int total, out int filtered)
        {
            if (data != null && data.Any())
            {
                List<VehicleInOutStatusListModel> eventLogsByVehicle = new List<VehicleInOutStatusListModel>();

                foreach (var eachData in data)
                {
                    var workTypeName = ((Army_WorkType)eachData.WorkType).GetDescription();

                    eventLogsByVehicle.AddRange(eachData.EventLogsByUser.Select(d => new VehicleInOutStatusListModel()
                    {
                        UserId = d.UserId,
                        UserName = d.UserName,
                        WorkTypeName = workTypeName,
                        DepartmentName = d.DepartmentName,
                        LastEventTime = d.EventLogs.FirstOrDefault().EventTime,
                        Reason = d.Reason,
                        Id = d.VehicleList.FirstOrDefault(v => v.PlateNumber.ToLower().Equals(d.EventLogs.FirstOrDefault().CardId.ToLower())).Id,
                        PlateNumber = d.VehicleList.FirstOrDefault(v => v.PlateNumber.ToLower().Equals(d.EventLogs.FirstOrDefault().CardId.ToLower())).PlateNumber,
                        Color = d.VehicleList.FirstOrDefault(v => v.PlateNumber.ToLower().Equals(d.EventLogs.FirstOrDefault().CardId.ToLower())).Color,
                        Model = d.VehicleList.FirstOrDefault(v => v.PlateNumber.ToLower().Equals(d.EventLogs.FirstOrDefault().CardId.ToLower())).Model
                    }).ToList());
                }

                // Search filter
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim().RemoveDiacritics().ToLower();
                    eventLogsByVehicle = eventLogsByVehicle.Where(d => (d.UserName?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                    || (d.DepartmentName?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                    || (d.PlateNumber?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                    || (d.Model?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                    || (d.Color?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                    || (d.VehicleName?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                    || (d.Reason?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                    || (d.WorkTypeName?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                    ).ToList();
                }

                total = eventLogsByVehicle.Count();
                filtered = eventLogsByVehicle.Count();

                // Sort
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    string columnName = sortColumn.ToPascalCase();
                    eventLogsByVehicle = Helpers.SortData<VehicleInOutStatusListModel>(eventLogsByVehicle.AsEnumerable<VehicleInOutStatusListModel>(), sortDirection, columnName).ToList();
                }

                // Paging
                if (pageSize > 0)
                {
                    eventLogsByVehicle = eventLogsByVehicle.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }

                return eventLogsByVehicle;
            }
            else
            {
                total = 0;
                filtered = 0;

                return new List<VehicleInOutStatusListModel>();
            }
        }

        public void UploadFileLogDevice(IcuDevice device, string msgId, IFormFile file)
        {
            try
            {
                if (!device.CompanyId.HasValue)
                {
                    _logger.LogWarning($"Device {device.DeviceAddress} not assign to company. So it will not save data log");
                    return;
                }
                var company = _unitOfWork.CompanyRepository.GetById(device.CompanyId.Value);
                string language = Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetLanguage(company.Id).Value) ?? Constants.DefaultLanguage;

                // Validate filename for security
                if (!FileHelpers.IsValidPathParameter(file.FileName))
                {
                    _logger.LogWarning($"Device {device.DeviceAddress}: invalid filename {file.FileName}");
                    return;
                }

                string extension = FileHelpers.GetFileExtensionByFileName(file.FileName);
                string generatedFileName = $"log_{device.DeviceAddress}_{DateTime.UtcNow.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss)}_UTC{extension}";
                bool result = FileHelpers.SaveFileByIFormFileSecure(file, $"{Constants.Settings.DefineFolderDataLogs}/{company.Code}/{device.DeviceAddress}", generatedFileName);
                if (!result)
                {
                    _logger.LogWarning($"Device {device.DeviceAddress}: save data log {file.FileName} error");
                }
                else
                {
                    var notification = new NotificationProtocolData
                    {
                        MsgId = msgId,
                        Sender = Constants.RabbitMq.SenderDefault,
                        Type = Constants.Protocol.NotificationNewFileLogDevice,
                        Data = new NotificationProtocolDataDetail
                        {
                            MessageType = Constants.MessageType.Success,
                            NotificationType = Constants.NotificationType.UploadSuccess,
                            Message = string.Format(DeviceResource.ResourceManager.GetString("msgNotificationNewFile", new CultureInfo(language)), device.Name),
                        }
                    };
                    
                    IWebSocketService webSocketService = new WebSocketService();
                    webSocketService.SendWebSocketToFE(Constants.Protocol.NotificationNewFileLogDevice, device.CompanyId.Value, notification);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }

        public byte[] GetFileLogOfDevice(IcuDevice device, string fileName = null)
        {;
            try
            {
                if (!device.CompanyId.HasValue)
                {
                    _logger.LogWarning($"Device {device.DeviceAddress} not assign to company. So it will not save data log");
                    return null;
                }
                
                var company = _unitOfWork.CompanyRepository.GetById(device.CompanyId.Value);
                string pathFolder = $"{Constants.Settings.DefineFolderDataLogs}/{company.Code}/{device.DeviceAddress}";
                if (!Directory.Exists(pathFolder))
                {
                    return null;
                }
                else
                {
                    if (string.IsNullOrEmpty(fileName))
                    {
                        string pathTempFile = $"{Constants.Settings.DefineFolderDataLogs}/{company.Code}/{Guid.NewGuid().ToString()}.zip";
                        ZipFile.CreateFromDirectory(pathFolder, pathTempFile);
                        var data = File.ReadAllBytes(pathTempFile);
                        File.Delete(pathTempFile);
                        return data;
                    }
                    else
                    {
                        var securePath = FileHelpers.GetSecurePath(pathFolder, fileName);
                        if (securePath == null)
                        {
                            _logger.LogWarning($"Invalid file name provided for device {device.DeviceAddress}: {fileName}");
                            return null;
                        }

                        if (File.Exists(securePath))
                        {
                            return File.ReadAllBytes(securePath);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
                return null;
            }
        }
        
        /// <summary>
        /// Get all transmit data model
        /// </summary>
        /// <returns></returns>
        public TransmitInfoModel GetTransmitAllData()
        {
            var data = EnumHelper.ToEnumList<TransmitType>();
            return new TransmitInfoModel { Data = data };
        }

        public bool SendDeviceRequest(string msgId, string command, IcuDevice device)
        {
            try
            {
                DeviceInstructionQueue deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
                deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                {
                    MsgId = msgId,
                    MessageType = Constants.Protocol.DeviceInstruction,
                    DeviceId = device.Id,
                    DeviceAddress = device.DeviceAddress,
                    Sender = Constants.RabbitMq.SenderDefault,
                    Command = command,
                });
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public List<FileLogListModel> GetAllLogsOfDevice(IcuDevice device)
        {
            List<FileLogListModel> data = new List<FileLogListModel>();
            if (device.CompanyId.HasValue)
            {
                var company = _unitOfWork.CompanyRepository.GetById(device.CompanyId.Value);
                string pathFolder = $"{Constants.Settings.DefineFolderDataLogs}/{company.Code}/{device.DeviceAddress}";
                DirectoryInfo directoryInfo = new DirectoryInfo(pathFolder);
                if (directoryInfo.Exists)
                {
                    string hostApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                    string linkDefault = $"{hostApi}{Constants.Route.ApiDevicesIdFileLogs}".Replace("{id}", device.Id.ToString());
                    int index = 1;
                    data = directoryInfo.GetFiles()
                        .OrderByDescending(m => m.CreationTimeUtc)
                        .Where(m => !m.FullName.Contains(Constants.FileConfig.FileNameRequestLog))
                        .Select(m =>
                        {
                            string fileName = m.FullName.Split("/").Last();
                            return new FileLogListModel()
                            {
                                Id = index++,
                                FileName = fileName,
                                CreatedOn = m.CreationTimeUtc.ConvertDefaultDateTimeToString(),
                                Size = m.Length,
                                LinkDownload = $"{linkDefault}?fileName={fileName}",
                            };
                        }).ToList();
                }
            }

            return data;
        }


        public List<EnumModelWithValue> GetRegisteredCardsByType(int id)
        {
            var device = _unitOfWork.IcuDeviceRepository.GetByIcuId(id);
            if (device == null) return null;

            var companyId = _httpContext.User.GetCompanyId();
            var validCardTypes = Helpers.GetMatchedIdentificationType(device.DeviceType);
            var userData = _unitOfWork.UserRepository.GetAssignUsersByDeviceId(companyId, new List<int>() { id });
            var cards = userData.Include(u => u.Card).SelectMany(u => u.Card).Where(c => !c.IsDeleted && validCardTypes.Contains(c.CardType)).ToList();

            var cardsByTypes = cards.GroupBy(c => c.CardType).Select(c => new EnumModelWithValue()
            {
                Id = c.Key,
                Name = ((CardType)c.Key).GetDescription(),
                Value = c.Count().ToString()
            }).ToList();

            return cardsByTypes;
        }
    }
}