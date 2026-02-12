using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bogus.Extensions;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.DeviceSDK;
using DeMasterProCloud.DataModel.Notification;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.DataModel.Visit;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Service.Protocol;
using DeMasterProCloud.Service.RabbitMqQueue;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace DeMasterProCloud.Service
{
    public interface ICameraService
    {
        Dictionary<string, object> GetInit(int companyId);

        List<CameraListModel> GetPaginated(CameraFilterModel filterModel, out int totalRecords,
            out int recordsFiltered);

        Camera GetById(int id);
        Camera GetDetailById(int id);
        Camera GetByCameraId(string cameraId);
        bool Add(CameraModel model);
        bool Update(Camera camera, int deviceIdResetConfig);
        void Delete(Camera camera);
        bool CheckPlugInCamera(int companyId);
        void RecheckEventLog(int companyId, DateTime start, DateTime end);
        IEnumerable<DeviceHistoryModel> GetHistory(int deviceId, int pageNumber, int pageSize, out int totalRecords);
        string ReceiveWebhookFromTsCamera(IFormFile faceImage, IFormFile vehicleImage);
    }

    public class CameraService : ICameraService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly HttpContext _httpContext;
        private readonly IDeviceSDKService _deviceSdkService;
        private readonly IWebSocketService _webSocketService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        #region parameter hanet camera

        private readonly int _hanetTypePersonEmployee = 0;
        private readonly int _hanetTypePersonCustomer = 1;
        private readonly int _hanetTypePersonCameraAlarm = 3;
        private readonly int _hanetTypePersonTakeFacePicture = 6;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        public CameraService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration, IDeviceSDKService deviceSdkService, IWebSocketService webSocketService)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _deviceSdkService = deviceSdkService;
            _webSocketService = webSocketService;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<CameraService>();
            _mapper = MapperInstance.Mapper;
        }

        public Dictionary<string, object> GetInit(int companyId)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            
            // types camera
            var solution = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
            var plugin = JsonConvert.DeserializeObject<PlugIns>(solution.PlugIns);
            var types = EnumHelper.ToEnumList<CameraType>();
            if (!plugin.CameraPlugIn) types = types.Where(m => m.Id != (int) CameraType.CameraHanet).ToList();
            if (!plugin.VehiclePlugIn) types = types.Where(m => m.Id != (int) CameraType.CameraLPR).ToList();
            dic.Add("types", types);
            
            return dic;
        }
        
        public List<CameraListModel> GetPaginated(CameraFilterModel filterModel, out int totalRecords, out int recordsFiltered)
        {
            var data = _unitOfWork.CameraRepository.GetCamerasByCompany(filterModel.CompanyId).AsQueryable();
            totalRecords = data.Count();
            recordsFiltered = totalRecords;
            if (!string.IsNullOrEmpty(filterModel.Search))
            {
                data = data.Where(c => c.Name.ToLower().Contains(filterModel.Search.ToLower()));
            }

            if (filterModel.Types != null && filterModel.Types.Any())
            {
                data = data.Where(m => filterModel.Types.Contains(m.Type));
            }
            if (filterModel.DeviceIds != null && filterModel.DeviceIds.Any())
            {
                data = data.Where(m => filterModel.DeviceIds.Contains(m.IcuId));
            }
            
            recordsFiltered = data.Count();
            data = data.OrderBy($"{filterModel.SortColumn} {filterModel.SortDirection}");
            data = data.Skip((filterModel.PageNumber - 1) * filterModel.PageSize).Take(filterModel.PageSize);
            return data.ToList();
        }

        public Camera GetById(int id)
        {
            return _unitOfWork.CameraRepository.GetById(id);
        }

        public Camera GetDetailById(int id)
        {
            return _unitOfWork.AppDbContext.Camera.Include(m => m.IcuDevice).FirstOrDefault(m => m.Id == id);
        }

        public Camera GetByCameraId(string cameraId)
        {
            return _unitOfWork.CameraRepository.GetByCameraId(cameraId);
        }

        public bool Add(CameraModel model)
        {
            var currentSetting = GetCameraSetting(_httpContext.User.GetCompanyId());
            var camera = new Camera()
            {
                Name = model.Name,
                PlaceID = currentSetting.PlaceId,
                IcuDeviceId = model.IcuId == 0 ? (int?) null : model.IcuId,
                CompanyId = model.CompanyId != 0 ? model.CompanyId : _httpContext.User.GetCompanyId(),
                CameraId = model.CameraId,
                VideoLength = model.VideoLength,
                RoleReader = model.RoleReader,
                SaveEventUnknownFace = model.SaveEventUnknownFace,
                CheckEventFromWebHook = model.CheckEventFromWebHook,
                UrlStream = model.UrlStream,
                Type = model.Type,
                Similarity = model.Similarity,
                VoiceAlarm = model.VoiceAlarm,
                LightAlarm = model.LightAlarm,
            };
            camera.CreatedBy = _httpContext.User.GetAccountId();
            camera.CreatedOn = DateTime.UtcNow;
            try
            {
                _unitOfWork.CameraRepository.Add(camera);
                _unitOfWork.Save();

                if (camera.IcuDeviceId.HasValue)
                {
                    SendDeviceConfig(camera.IcuDeviceId.Value);
                }
                
                VmsAddCamera(camera);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
                return false;
            }
        }

        public bool Update(Camera camera, int deviceIdResetConfig)
        {
            camera.UpdatedBy = _httpContext.User.GetAccountId();
            camera.UpdatedOn = DateTime.Now;
            try
            {
                _unitOfWork.CameraRepository.Update(camera);
                _unitOfWork.Save();
                
                if(camera.IcuDeviceId.HasValue)
                    SendDeviceConfig(camera.IcuDeviceId.Value);
                
                // reset config camera device
                if(deviceIdResetConfig != 0)
                    SendDeviceConfig(deviceIdResetConfig);
                
                // HandleResponseCamera(camera);
                VmsDeleteCamera(camera);
                VmsAddCamera(camera);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogError($"CompanyId: {camera.CompanyId} -  CameraId: {camera.CameraId}");
                _logger.LogError(e.Message);
                _logger.LogWarning(e.StackTrace);
                return false;
            }
        }

        public void Delete(Camera camera)
        {
            int deviceId = camera.IcuDeviceId ?? 0;
            try
            {
                _unitOfWork.CameraRepository.Delete(camera);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
            }

            try
            {
                VmsDeleteCamera(camera);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
            }
            if(deviceId != 0)
            {
                try
                {
                    SendDeviceConfig(deviceId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Delete");
                }
            }
        }

        public CameraSetting GetCameraSetting(int companyId)
        {
            // Check plugin.
            IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
            CameraSetting cameraSetting = new CameraSetting();
            try
            {
                var plugIns = unitOfWork.AppDbContext.PlugIn.Where(x => x.CompanyId == companyId).Select(x => x.PlugIns).FirstOrDefault();
                if (!string.IsNullOrEmpty(plugIns))
                {
                    var value = JsonConvert.DeserializeObject<PlugIns>(plugIns);
                    if (value.CameraPlugIn)
                    {
                        var setting = unitOfWork.SettingRepository.GetByKey(Constants.Settings.CameraSetting, companyId);
                        var model = JsonConvert.DeserializeObject<CameraSetting>(setting.Value);
                        if (string.IsNullOrEmpty(model.Server))
                        {
                            model.Server = Constants.HanetApiCamera.HostServer;
                        }
                        cameraSetting = model;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                cameraSetting = new CameraSetting();
            }

            unitOfWork.Dispose();
            return cameraSetting;
        }

        public bool CheckPlugInCamera(int companyId)
        {
            var solution = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
            var plugin = JsonConvert.DeserializeObject<PlugIns>(solution.PlugIns);
            if (plugin.CameraPlugIn)
            {
                var cameraSetting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.CameraSetting, companyId);
                if (!string.IsNullOrEmpty(cameraSetting.Value))
                {
                    var valueSetting = JsonConvert.DeserializeObject<CameraSetting>(cameraSetting.Value);
                    return valueSetting.IsEnableAutoSync;
                }
            }
            return false;
        }

        public void RecheckEventLog(int companyId, DateTime start, DateTime end)
        {
            List<Company> companies = new List<Company>();
            if (companyId == 0)
            {
                _unitOfWork.CompanyRepository.GetCompaniesByPlugin(Constants.PlugIn.CameraPlugIn);
            }
            else
            {
                var company = _unitOfWork.CompanyRepository.GetById(companyId);
                if (company != null)
                {
                    var valid = _unitOfWork.CompanyRepository.CheckCompanyByPlugin(Constants.PlugIn.CameraPlugIn, companyId);
                    if (valid)
                        companies.Add(company);
                }
            }

            foreach (var company in companies)
            {
                var eventLogs = _unitOfWork.AppDbContext.EventLog
                    .Where(m => m.CompanyId == company.Id && m.CardType == (short)CardType.HFaceId && m.ImageCamera != null && start <= m.EventTime && m.EventTime <= end);
                var cameras = _unitOfWork.CameraRepository.GetCamerasByCompany(company.Id);
                
                foreach (var eventLog in eventLogs)
                {
                    if(eventLog.ImageCamera == "[]") continue;
                    
                    int delay = Constants.HanetApiCamera.DefaultDelayCallApi / 2;
                    if (cameras.Count > 0 && cameras[0].VideoLength > 0)
                    {
                        delay = cameras[0].VideoLength / 2;
                    }
                    long from = eventLog.EventTime.AddSeconds(-delay).ConvertToTimeSpanUnix();
                    long to = eventLog.EventTime.AddSeconds(+delay).ConvertToTimeSpanUnix();
                    
                    var eventCheckInModels = JsonConvert.DeserializeObject<List<CameraEventCheckInModel>>(eventLog.ImageCamera);
                    eventCheckInModels = eventCheckInModels.Where(m => from <= m.CheckinTime && m.CheckinTime <= to).ToList();
                    if (eventCheckInModels.Any())
                    {
                        eventLog.ImageCamera = JsonConvert.SerializeObject(eventCheckInModels);
                    }
                    else
                    {
                        eventLog.ImageCamera = JsonConvert.SerializeObject("[]");
                    }
                    _unitOfWork.EventLogRepository.Update(eventLog);
                    _unitOfWork.Save();
                }
            }
        }

        public IEnumerable<DeviceHistoryModel> GetHistory(int cameraId, int pageNumber, int pageSize, out int totalRecords)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var data = _unitOfWork.AppDbContext.EventLog
                .Where(c => c.CompanyId == companyId)
                .Where(c => c.CameraId == cameraId && c.CameraId.HasValue)
                .Where(c => c.EventType == (short)EventType.CommunicationSucceed
                            || c.EventType == (short)EventType.CommunicationFailed)
                .OrderByDescending(c => c.EventTime);
            
            totalRecords = data.Count();
            return data.Skip((pageNumber - 1) * pageSize).Take(pageSize).Select(c => new EventLog
                {
                    EventTime = c.EventTime,
                    EventType = c.EventType
                }).AsEnumerable()
                .Select(_mapper.Map<DeviceHistoryModel>);
        }

        private void SendDeviceConfig(int deviceId)
        {
            IWebSocketService webSocketService = new WebSocketService();
            string sender = _httpContext.User.GetUsername();
            try
            {
                DeviceInstructionQueue deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, webSocketService);
                var device = _unitOfWork.IcuDeviceRepository.GetById(deviceId);
                if (device != null)
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
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
            
        }

        private void AddIdentificationFaceId(IUnitOfWork unitOfWork, int userId, string cardId, bool isUser = true)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                _logger.LogError("CardId empty. Can not create new card HFaceId" + (isUser ? "userId = " : "visitorId = ") + userId);
                return;
            }
            
            IAccessGroupService accessGroupService = new AccessGroupService(unitOfWork, _deviceSdkService,
                _configuration, ApplicationVariables.LoggerFactory.CreateLogger<AccessGroupService>(),
                new HttpContextAccessor(), null);
            
            // delete all old card face id
            var cardFaceIds = unitOfWork.AppDbContext.Card
                .Where(m => !m.IsDeleted && m.CardType == (short) CardType.HFaceId && m.CardId.ToLower() != cardId.ToLower()
                       && ((isUser && m.UserId == userId) || (!isUser && m.VisitId == userId)));
            if (cardFaceIds.Any())
            {
                foreach (var cardFaceId in cardFaceIds)
                {
                    cardFaceId.IsDeleted = true;
                    cardFaceId.UpdatedOn = DateTime.Now;
                    try
                    {
                        unitOfWork.CardRepository.Update(cardFaceId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in AddIdentificationFaceId");
                    }
                }
                try
                {
                    unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AddIdentificationFaceId");
                }
            }

            var checkCardFace = unitOfWork.CardRepository.Get(m => !m.IsDeleted && m.CardId.ToLower() == cardId.ToLower());
            if (checkCardFace != null)
            {
                _logger.LogWarning($"Card face existed in system cardId = {cardId}");
                return;
            }

            if (isUser) // Add Card for user
            {
                IUserService userService = new UserService(unitOfWork, new HttpContextAccessor(), accessGroupService,
                    _configuration, ApplicationVariables.LoggerFactory.CreateLogger<UserService>(), null);
                var user = userService.GetByIdAndCompany(userId, 0);
                userService.AddIdentification(user, new CardModel()
                {
                    CardId = cardId,
                    CardStatus = 0,
                    CardType = (short) CardType.HFaceId
                });
            }
            else // Add Card for visitor
            {
                var visitor = unitOfWork.VisitRepository.GetById(userId);
                List<short> statusVisitNormal = new List<short>()
                {
                    (short)VisitChangeStatusType.Approved,
                    (short)VisitChangeStatusType.AutoApproved,
                };
                var faceId = new Card
                {
                    CardId = cardId,
                    IssueCount = 0,
                    CompanyId = visitor.CompanyId,
                    VisitId = visitor.Id,
                    CardType = (short)CardType.HFaceId,
                    AccessGroupId = visitor.AccessGroupId,
                    ValidFrom = visitor.StartDate,
                    ValidTo = visitor.EndDate,
                    CreatedBy = 1,
                    CreatedOn = DateTime.Now,
                    UpdatedBy = 1,
                    UpdatedOn = DateTime.Now,
                    CardStatus = statusVisitNormal.Contains(visitor.Status) ? (short)CardStatus.Normal : (short)CardStatus.InValid,
                };
                try
                {
                    unitOfWork.CardRepository.Add(faceId);
                    unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AddIdentificationFaceId");
                }

                var agDevices = unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(visitor.CompanyId, visitor.AccessGroupId);

                if (agDevices != null && agDevices.Any())
                {
                    foreach (var agDevice in agDevices)
                    {
                        accessGroupService.SendVisitor(agDevice, true, visitor);
                    }
                }
            }
        }
        
        private EventLog SaveEventLogWithTsCamera(List<DataImageCamera> eventCheckInModel, Card card, User user, 
            Visit visit, IcuDevice device, short eventType, int roleReader, DateTime timeWebhook, double similarity, string vehicleCardId = null, bool bothCheckVehicle = false)
        {
            DateTime now = DateTime.UtcNow;
            var cardList = new List<OtherCardListModel>();
            DateTime eventTime = new DateTime(timeWebhook.Year, timeWebhook.Month, timeWebhook.Day, timeWebhook.Hour, timeWebhook.Minute, timeWebhook.Second);
            try
            {
                if (bothCheckVehicle && vehicleCardId != null)
                {
                    cardList.Add(new OtherCardListModel()
                    {
                        CardId = vehicleCardId,
                        CardType = (short)CardType.VehicleId
                    });
                }
                
                string userName = "Unknown";
                string roleCamera = roleReader == (short) RoleRules.In ? "In" : "Out";
                if (user != null)
                {
                    userName = user.FirstName;
                }
                else if (visit != null)
                {
                    userName = visit.VisitorName;
                }
                else
                {
                    try
                    {
                        var language = Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetLanguage(device.CompanyId.Value).Value);
                        var culture = new CultureInfo(language);
                        userName = EventLogResource.ResourceManager.GetString("lblUnknownFace", culture);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                }

                // if (card != null && user != null)
                // {
                //     cardList.Add(new OtherCardListModel()
                //     {
                //         CardId = eventCheckInModel.FirstOrDefault()?.PersonID,
                //         CardType = card.CardType
                //     });
                // }
                var eventLog = new EventLog()
                {
                    EventTime = eventTime,
                    EventType = eventType,
                    CardType = (short)CardType.LFaceId,
                    CardId = card?.CardId,
                    CreatedOn = now,
                    UserId = user?.Id,
                    UserName = userName,
                    DeptId = user?.DepartmentId,
                    DoorName = device.Name,
                    IcuId = device.Id,
                    Icu = device,
                    Antipass = roleCamera,
                    ImageCamera = JsonConvert.SerializeObject(eventCheckInModel),
                    CompanyId = device.CompanyId,
                    ResultCheckIn = eventCheckInModel.FirstOrDefault()?.Link,
                    VisitId = visit?.Id,
                    IsVisit = visit != null,
                    SearchScore = similarity,
                    DelayOpenDoorByCamera = Math.Round((now - eventTime).TotalSeconds, 0),
                    OtherCardId = JsonConvert.SerializeObject(cardList),
                };
                
                _unitOfWork.EventLogRepository.Add(eventLog);
                _unitOfWork.Save();
                
                // pushing notification by setting
                HelperService helperService = new HelperService(_configuration);
                helperService.PushNotificationSettingByEventLogsAsync(device, new List<EventLog>() {eventLog});
                
                return eventLog;
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException)
                {
                    _logger.LogWarning("[EVENT-EXCEPTION] This is duplicated event. Server cannot store this event.");
                    _logger.LogWarning("[EVENT-EXCEPTION] Device Address : " + device.DeviceAddress);
                    _logger.LogWarning("[EVENT-EXCEPTION] Access Time    : " + eventTime);
                    _logger.LogWarning("[EVENT-EXCEPTION] EventModel     : " + JsonConvert.SerializeObject(eventCheckInModel));
                }
                else
                {
                    Console.WriteLine("[Tung Son checkin exception]: " + ex.Message + ex.StackTrace);   
                }
                return null;
            }
        }
        private EventLog SaveEventLogWithCamera(CheckinDataWebhookModel data, CameraEventCheckInModel eventCheckInModel, User user, Visit visit, IcuDevice device, short eventType, int roleReader, DateTime timeWebhook)
        {
            DateTime now = DateTime.Now;
            DateTime eventTime = new DateTime(timeWebhook.Year, timeWebhook.Month, timeWebhook.Day, timeWebhook.Hour, timeWebhook.Minute, timeWebhook.Second);
            try
            {
                string userName = "Unknown";
                string roleCamera = roleReader == (short) RoleRules.In ? "In" : "Out";
                if (user != null)
                {
                    userName = user.FirstName;
                }
                else if (visit != null)
                {
                    userName = visit.VisitorName;
                }
                else
                {
                    try
                    {
                        var language = Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetLanguage(device.CompanyId.Value).Value);
                        var culture = new CultureInfo(language);
                        userName = EventLogResource.ResourceManager.GetString("lblUnknownFace", culture);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                }

                var eventLog = new EventLog()
                {
                    EventTime = eventTime,
                    EventType = eventType,
                    CardType = (short)CardType.HFaceId,
                    CardId = Constants.HanetApiCamera.PrefixCardId + eventCheckInModel.PersonID,
                    CreatedOn = now,
                    UserId = user?.Id,
                    UserName = userName,
                    DeptId = user?.DepartmentId,
                    DoorName = device.Name,
                    IcuId = device.Id,
                    Icu = device,
                    Antipass = roleCamera,
                    ImageCamera = $"[{JsonConvert.SerializeObject(eventCheckInModel)}]",
                    CompanyId = device.CompanyId,
                    ResultCheckIn = eventCheckInModel.Avatar,
                    VisitId = visit?.Id,
                    IsVisit = visit != null,
                    BodyTemperature = data.Temp / 10,
                };
                
                _unitOfWork.EventLogRepository.Add(eventLog);
                _unitOfWork.Save();
                
                // pushing notification by setting
                HelperService helperService = new HelperService(_configuration);
                helperService.PushNotificationSettingByEventLogsAsync(device, new List<EventLog>() {eventLog});
                
                return eventLog;
            }
            catch (Exception ex)
            {
                if (ex is DbUpdateException)
                {
                    _logger.LogWarning("[EVENT-EXCEPTION] This is duplicated event. Server cannot store this event.");
                    _logger.LogWarning("[EVENT-EXCEPTION] Device Address : " + device.DeviceAddress);
                    _logger.LogWarning("[EVENT-EXCEPTION] Access Time    : " + eventTime);
                    _logger.LogWarning("[EVENT-EXCEPTION] Card Id        : " + eventCheckInModel.PersonID);
                    _logger.LogWarning("[EVENT-EXCEPTION] Event Type     : HFaceId");
                }
                else
                {
                    Console.WriteLine("[Hanet checkin exception]: " + ex.Message + ex.StackTrace);   
                }
                return null;
            }
        }

        private async Task SendInstructionToIcu(IcuDevice device, int openPeriod)
        {
            var deviceOpenInstruction = new SDKOpenDoorModel
            {
                OpenPeriod = openPeriod,
            };
            _deviceSdkService.OpenDoor(device.DeviceAddress, deviceOpenInstruction);
        }

        private async Task SendNotificationToFontEnd(EventLog eventLog, IcuDevice icuDevice, int companyId, User user, Visit visit)
        {
            // Convert data send to FE
            SendEventLogDetailData sendEventLog = _mapper.Map<SendEventLogDetailData>(eventLog);
            sendEventLog.Id = Guid.NewGuid();
            sendEventLog.Device = icuDevice.DeviceAddress;
            sendEventLog.DeviceAddress = icuDevice.DeviceAddress;
            sendEventLog.UnixTime = (sendEventLog.AccessTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
            sendEventLog.DoorName = icuDevice.Name;
            sendEventLog.EventDetailCode = eventLog.EventType;
            sendEventLog.IssueCount = eventLog.IssueCount.ToString();
            sendEventLog.InOut = eventLog.Antipass;
            sendEventLog.EventType = eventLog.EventType;
            
            if (Enum.TryParse(eventLog.Antipass, true, out Antipass inOutType))
            {
                sendEventLog.InOutType = (int)inOutType;
            }

            if (visit != null)
            {
                sendEventLog.Department = visit?.VisitorDepartment;
                sendEventLog.DepartmentName = visit?.VisitorDepartment;
                sendEventLog.UserName = visit?.VisitorName;
                sendEventLog.ExpireDate = visit?.StartDate.ConvertDefaultDateTimeToString();
                sendEventLog.VisitId = visit?.Id;
                sendEventLog.Position = visit?.Position;
                if (icuDevice.BuildingId != null) sendEventLog.BuildingId = icuDevice.BuildingId.Value;
                sendEventLog.UserType = (short)UserType.Visit;
                if (string.IsNullOrEmpty(visit.Avatar))
                {
                    sendEventLog.Avatar = Helpers.ResizeImage(Constants.Image.DefaultMale);
                }
                else
                {
                    sendEventLog.Avatar = visit.Avatar;
                }
                if (!string.IsNullOrEmpty(visit.VisitType))
                {
                    sendEventLog.WorkType = int.Parse(visit.VisitType);
                    sendEventLog.WorkTypeName = ((VisitType)int.Parse(visit.VisitType)).GetDescription();
                }
            }
            else if (user != null)
            {
                sendEventLog.ParentDepartmentId = user?.Department?.ParentId ?? 0;
                sendEventLog.Parent = user?.Department?.Parent?.DepartName;
                sendEventLog.Department = user.Department == null ? "" : user.Department.DepartName;
                sendEventLog.DepartmentName = user.Department == null ? "" : user.Department.DepartName;
                sendEventLog.UserName = user?.FirstName + " " + user?.LastName;
                sendEventLog.ExpireDate = user?.ExpiredDate?.ConvertDefaultDateTimeToString();
                sendEventLog.UserId = user?.Id;
                sendEventLog.Position = user?.Position;
                if (icuDevice.BuildingId != null) sendEventLog.BuildingId = icuDevice.BuildingId.Value;
                sendEventLog.UserType = (short)UserType.Normal;
                if (string.IsNullOrEmpty(user.Avatar))
                {
                    sendEventLog.Avatar = Helpers.ResizeImage(user.Sex ? Constants.Image.DefaultMale : Constants.Image.DefaultFemale);
                }
                else
                {
                    sendEventLog.Avatar = user.Avatar;
                }
                if (user.WorkType.HasValue)
                {
                    sendEventLog.WorkType = user.WorkType.Value;
                    sendEventLog.WorkTypeName = ((WorkType)user.WorkType.Value).GetDescription();
                }
            }
            else
            {
                sendEventLog.Avatar = eventLog.ResultCheckIn;
            }

            _webSocketService.SendWebSocketToFE(Constants.Protocol.EventLogType, companyId, sendEventLog);
        }

        public async Task SendInfoVisitToFe(Visit visit, Company company, int cameraId, string imageBase64, string messageType = Constants.Protocol.NotificationCamera)
        {
            try
            {
                var model = new VisitDataModel();

                if (visit != null) // visitor
                {
                    // convert start, end date visit
                    IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                    var building = unitOfWork.BuildingRepository.GetDefaultByCompanyId(company.Id);
                    var offSet = building.TimeZone.ToTimeZoneInfo().BaseUtcOffset;

                    model = _mapper.Map<VisitDataModel>(visit);

                    // add field doors assign
                    if (visit.AccessGroupId != 0)
                    {
                        List<int> doors = new List<int>();
                        var accessGroupDevices = unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(visit.CompanyId, visit.AccessGroupId);
                        foreach (var item in accessGroupDevices)
                        {
                            doors.Add(item.IcuId);
                        }

                        model.Doors = JsonConvert.SerializeObject(doors);
                    }
                    
                    model.StartDate = visit.StartDate.ConvertToUserTime(offSet).ConvertDefaultDateTimeToString();
                    model.EndDate = visit.EndDate.ConvertToUserTime(offSet).ConvertDefaultDateTimeToString();
                    unitOfWork.Dispose();
                }

                model.Avatar = imageBase64;

                var notification = new NotificationProtocolDataDetail
                {
                    MessageType =
                        string.IsNullOrEmpty(imageBase64) && messageType == Constants.Protocol.NotificationCamera
                            ? Constants.MessageType.Error
                            : Constants.MessageType.Success,
                    NotificationType = "CAMERA",
                    User = visit?.VisitorName,
                    Message = Helpers.JsonConvertCamelCase(model)
                };
                _webSocketService.SendWebSocketToFE(messageType, company.Id, notification);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        public void ReceiveDataQrCodeWebhook(QrCodeDataWebhookModel data)
        {
            if (string.IsNullOrEmpty(data.Qrcode) || data.Qrcode.Split('|').Length < 6)
            {
                _logger.LogWarning($"Data Qr code wrong ({data.Qrcode})");
                return;
            }
            
            var camera = _unitOfWork.CameraRepository.GetByCameraId(data.DeviceID);
            if (camera == null)
            {
                _logger.LogWarning($"Camera not found {data.DeviceID}");
                return;
            }
            
            var company = _unitOfWork.CompanyRepository.GetById(camera.CompanyId);
            string[] dataQrCode = data.Qrcode.Split('|');
            var visitor = _unitOfWork.VisitRepository.GetByNationIdNumber(dataQrCode[0], company.Id);
            string imageBase64 = "";

            if (visitor == null)
            {
                visitor = new Visit()
                {
                    VisitorName = dataQrCode[2],
                    Address = dataQrCode[5],
                    BirthDay = DateTime.ParseExact(dataQrCode[3], Constants.DateTimeFormat.DdMdYyyy, null),
                    NationalIdNumber = dataQrCode[0],
                };
            }
            else
            {
                imageBase64 = Helpers.GetImageBase64FromUrl(visitor.Avatar);
            }
            
            var building = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(company.Id);
            var offSet = building.TimeZone.ToTimeZoneInfo().BaseUtcOffset;
            visitor.StartDate = DateTime.UtcNow;
            visitor.EndDate = DateTime.UtcNow.ConvertToUserTime(offSet).Date.AddDays(1).AddSeconds(-1).ConvertToSystemTime(offSet);
            
            // send visitor by fe
            SendInfoVisitToFe(visitor, company, camera.Id, imageBase64, Constants.Protocol.NotificationCameraQrCode);
        }

        public string ReceiveWebhookFromTsCamera(IFormFile file, IFormFile vehicleImage)
        {
            try
            {
                Console.WriteLine("[RECEIVE IMAGE EVENT-LOG]: " + file.FileName);
                string[] fieldsInFileName = file.FileName.Split('-');

                // check file format
                if (fieldsInFileName.Length < 3 || fieldsInFileName[2].Length < 8)
                {
                    _logger.LogWarning("File Name wrong format");
                    return "File Name wrong format";
                }
                string similarityStr = fieldsInFileName[3].Insert(1, ".");
                if (!double.TryParse(similarityStr, out var similarity))
                {
                    similarity = 0;
                }
                similarity = Math.Round(similarity, 3);
                // check camera
                Camera camera = _unitOfWork.CameraRepository.GetByCameraId(fieldsInFileName[0]);
                if (camera == null || camera.IcuDeviceId == 0)
                {
                    _logger.LogWarning($"Camera not found {fieldsInFileName[0]}");
                    return $"Camera not found {fieldsInFileName[0]}";
                }

                var device = camera.IcuDevice;

                // check plugin camera for company
                var solution = _unitOfWork.PlugInRepository.GetPlugInByCompany(camera.CompanyId);
                var plugin = JsonConvert.DeserializeObject<PlugIns>(solution.PlugIns);
                if (!plugin.CameraPlugIn)
                {
                    _logger.LogWarning("Disable plugin camera");
                    return "Disable plugin camera";
                }

                var connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                if (string.IsNullOrEmpty(connectionApi))
                {
                    return "Server error config";
                }

                string folderImageDay = fieldsInFileName[2].Substring(0, 8);
                var company = _unitOfWork.CompanyRepository.GetCompanyById(camera.CompanyId);
                // Save file
                if (!Directory.Exists($"{Constants.Settings.DefineFolderImages}/{company.Code}/{folderImageDay}"))
                {
                    Directory.CreateDirectory(
                        $"{Constants.Settings.DefineFolderImages}/{company.Code}/{folderImageDay}");
                }

                // Use secure file saving to prevent path traversal attacks
                bool isSuccess = FileHelpers.SaveFileByIFormFileSecure(file, $"{Constants.Settings.DefineFolderImages}/{company.Code}/{folderImageDay}", file.FileName);
                if (!isSuccess)
                    return "Invalid file name or save failed";

                string linkFile = $"{Constants.Settings.DefineFolderImages}/{company.Code}/{folderImageDay}/{file.FileName}";

                // init data model
                List<DataImageCamera> eventCheckInModel = new List<DataImageCamera>()
                {
                    new DataImageCamera()
                    {
                        FileName = file.FileName,
                        Link = connectionApi + "/static/" + linkFile,
                    }
                };

                // Initial: check type camera alarm
                short eventTypeUnknown = (short)EventType.UnknownPerson;

                string imageBase64 = Helpers.GetImageBase64FromUrl(connectionApi + "/static/" + linkFile);
                string hanetCardId = fieldsInFileName[1];

                var card = _unitOfWork.CardRepository.GetByCardId(company.Id, hanetCardId);
                var buildingDevice = _unitOfWork.BuildingRepository.GetById(device.BuildingId.Value);
                TimeZoneInfo timeZoneInfo = buildingDevice.TimeZone.ToTimeZoneInfo();
                
                var offSet = timeZoneInfo.BaseUtcOffset;
                var timeWebhook =
                    (fieldsInFileName[2].ConvertDefaultStringToDateTime(Constants.DateTimeFormat.DdMMyyyyHHmmss) ??
                     DateTime.UtcNow).ConvertToSystemTime(offSet);
                _logger.LogInformation($"Time webhook {timeWebhook.ToString()}");
                var eventLogs = _unitOfWork.AppDbContext.EventLog
                    .Where(m => m.CardId == hanetCardId && m.CompanyId == company.Id
                                                        && (m.ImageCamera == null || m.ImageCamera == "[]")
                                                        && timeWebhook.AddSeconds(-Constants.HanetApiCamera
                                                            .TimeoutWebhook) <= m.EventTime
                                                        && timeWebhook.AddSeconds(+Constants.HanetApiCamera
                                                            .TimeoutWebhook) >= m.EventTime);
                
                User user = null;
                Visit visit = null;
                Card vehicleCard = null;
                string vehicleCardId = null;

                if (vehicleImage != null)
                {
                    // Use secure file saving to prevent path traversal attacks
                    bool vehicleSuccess = FileHelpers.SaveFileByIFormFileSecure(vehicleImage, $"{Constants.Settings.DefineFolderImages}/{company.Code}/{folderImageDay}", vehicleImage.FileName);
                    if (!vehicleSuccess)
                        return "Invalid vehicle file name or save failed";

                    string linkFileVehicle = $"{Constants.Settings.DefineFolderImages}/{company.Code}/{folderImageDay}/{vehicleImage.FileName}";
                    
                    string[] fileNameVehicle = vehicleImage.FileName.Split('-');
                    var dataImage = new DataImageCamera
                    {
                        FileName = vehicleImage.FileName,
                        Link = $"{connectionApi}/static/{linkFileVehicle}"
                    };

                    if (fileNameVehicle.Length >= 3)
                    {
                        var cameraVehicle = _unitOfWork.CameraRepository.GetByCameraId(fileNameVehicle[0]);
                        if (cameraVehicle != null)
                        {
                            vehicleCard = _unitOfWork.CardRepository.GetByCardId(company.Id, fileNameVehicle[1]);
                            vehicleCardId = fileNameVehicle[1];
                        }
                    }

                    eventCheckInModel.Add(dataImage);
                }

                // Page camera visit
                if (card != null && card.VisitId.HasValue)
                {
                    visit = _unitOfWork.VisitRepository.GetByVisitId(company.Id, card.VisitId.Value);
                    if (visit != null)
                    {
                        if (DateTime.UtcNow > visit.EndDate)
                        {
                            visit.StartDate = DateTime.UtcNow;
                            visit.EndDate = DateTime.UtcNow;
                        }
                    }

                    // send info to fe
                    _ = SendInfoVisitToFe(visit, company, camera.Id, imageBase64);
                }
                else if (card != null && card.UserId.HasValue)
                {
                    user = _unitOfWork.AppDbContext.User.Include(m => m.Department)
                        .ThenInclude(x => x.Parent)
                        .FirstOrDefault(m => m.Id == card.UserId.Value && !m.IsDeleted);
                }

                // update avatar event-log
                if (eventLogs.Any())
                {
                    _logger.LogInformation($"Update Image checkin of eventlog, count {eventLogs.Count()}");
                    foreach (var eventLog in eventLogs)
                    {
                        eventLog.ResultCheckIn = imageBase64;
                        eventLog.ImageCamera = $"[{JsonConvert.SerializeObject(eventCheckInModel)}]";
                        eventLog.ResultCheckIn = eventCheckInModel.FirstOrDefault()?.Link;
                        eventLog.SearchScore = similarity;
                        eventLog.DelayOpenDoorByCamera = Math.Round((timeWebhook - eventLog.EventTime).TotalSeconds, 0);
                        _unitOfWork.EventLogRepository.Update(eventLog);
                        _ = SendNotificationToFontEnd(eventLog, device, company.Id, user, visit);
                    }

                    _unitOfWork.Save();
                }
                else // create new event-log
                {
                    _logger.LogInformation($"Add new Event log with time checkin: {timeWebhook.ToString()}");
                    if (user == null && visit == null)
                    {
                        if (camera.SaveEventUnknownFace)
                        {
                            var eventLogknown = SaveEventLogWithTsCamera(eventCheckInModel, card, null, null, device,
                                eventTypeUnknown, camera.RoleReader, timeWebhook, similarity);
                            if (eventLogknown == null)
                                return "Can not found event log";

                            _ = SendNotificationToFontEnd(eventLogknown, device, company.Id, null, null);
                        }
                    }
                    else
                    {
                        IDeviceService deviceService = new DeviceService(_unitOfWork);
                        var eventType = deviceService.CheckPermissionUserOrVisitOpenDoor(user, visit, device, DateTime.UtcNow, vehicleCard, vehicleImage != null);
                        var eventLog = SaveEventLogWithTsCamera(eventCheckInModel, card, user, visit, device, eventType, camera.RoleReader, timeWebhook, similarity, vehicleCardId, vehicleImage != null);
                        if (eventLog == null)
                            return "Can not found event log";

                        // send event-log monitoring to font-end
                        _ = SendNotificationToFontEnd(eventLog, device, company.Id, user, visit);

                        if (eventType == (short)EventType.NormalAccess)
                        {
                            // send command open door
                            int openDuration = device.OpenDuration ?? 3;
                            if (device.MaxOpenDuration.HasValue && openDuration > device.MaxOpenDuration.Value)
                                openDuration = device.MaxOpenDuration.Value;
                            _ = SendInstructionToIcu(device, openDuration);
                            // calculator attendance
                            if (user != null && plugin.TimeAttendance && company.UpdateAttendanceRealTime)
                            {
                                int timeFormatId = 0;

                                var attendanceSetting =
                                    _unitOfWork.AttendanceRepository.GetAttendanceSetting(camera.CompanyId);
                                if (attendanceSetting != null)
                                {
                                    timeFormatId = attendanceSetting.TimeFormatId;
                                }

                                var attendanceService = new AttendanceService(new HttpContextAccessor(), _configuration);
                                _ = attendanceService.AddClockInOutAsync(user, eventLog.Antipass, eventLog.EventTime,
                                    timeZoneInfo, timeFormatId);
                            }
                        }
                    }
                }

                return String.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return e.Message;
            }
        }

        #region vms

        private void VmsAddCamera(Camera camera)
        {
            try
            {
                if (string.IsNullOrEmpty(camera.UrlStream))
                {
                    return;
                }
                
                string url = _configuration.GetSection(Constants.VmsConfig.DefineHost).Get<string>() +
                             Constants.VmsConfig.ApiCameras;
                var data = new VmsCameraModel()
                {
                    CameraId = camera.CameraId,
                    Name = camera.Name,
                    UrlStream = camera.UrlStream,
                    VideoLength = camera.VideoLength
                };
                var response = Helpers.PostJson(url, data, _httpContext.GetTokenBearer());
                if (!string.IsNullOrEmpty(response?["urlStream"]?.ToString()))
                {
                    camera.VmsUrlStream = response?["urlStream"]?.ToString();
                    _unitOfWork.CameraRepository.Update(camera);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        
        private void VmsDeleteCamera(Camera camera)
        {
            try
            {
                string url = _configuration.GetSection(Constants.VmsConfig.DefineHost).Get<string>() +
                             Constants.VmsConfig.ApiCameras + $"?cameraId={camera.CameraId}";
                var response = Helpers.DeleteJson(url, null, _httpContext.GetTokenBearer());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion
    }
}