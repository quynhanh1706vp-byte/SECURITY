using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.DeviceSDK;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Service.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeMasterProCloud.Service.RabbitMqQueue
{
    public class AccessControlQueue
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDeviceSDKService _deviceSDKService;
        private readonly IWebSocketService _webSocketService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly SemaphoreSlim _concurrencyLimiter;
        private readonly IConfiguration _configuration;
        private const int MaxConcurrentDeviceCalls = 5;

        public AccessControlQueue(IUnitOfWork unitOfWork, IWebSocketService webSocketService)
        {
            _unitOfWork = unitOfWork;
            _webSocketService = webSocketService;
            _configuration = ApplicationVariables.Configuration;
            _deviceSDKService = new DeviceSDKService(_configuration);
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<AccessControlQueue>();
            _mapper = MapperInstance.Mapper;
            _concurrencyLimiter = new SemaphoreSlim(MaxConcurrentDeviceCalls, MaxConcurrentDeviceCalls);
        }

        public void SendUserInfo(UserInfoQueueModel model)
        {
            Console.WriteLine($"[DEBUG] SendUserInfo called: MessageType={model.MessageType}, DeviceId={model.DeviceId}, DeviceAddress={model.DeviceAddress}, UserIds=[{string.Join(",", model.UserIds ?? new List<int>())}]");
            Task.Run(async () =>
            {
                // Create a new UnitOfWork instance for this background task to avoid DbContext concurrency issues
                IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                try
                {
                    await SendUserInfoAsync(model, unitOfWork);
                }
                finally
                {
                    unitOfWork?.Dispose();
                }
            });
        }
        
        public async Task SendUserInfoAsync(UserInfoQueueModel model, IUnitOfWork unitOfWork = null)
        {
            Console.WriteLine($"[DEBUG] SendUserInfoAsync started: MessageType={model.MessageType}, TotalData={(model.TotalData != null ? "Present" : "Null")}");
            
            // Use the provided unitOfWork or fall back to the instance unitOfWork
            var workingUnitOfWork = unitOfWork ?? _unitOfWork;
            List<int> dualFaceTypeAratek = new List<int>()
            {
                (short)DeviceType.BA8300,
                (short)DeviceType.DQ8500,
            };
            
            if (model.TotalData == null)
            {
                try
                {
                    // dual face device
                    List<int> dualFaceTypes = new List<int>()
                    {
                        (short)DeviceType.DF970,
                        (short)DeviceType.Icu500,
                        (short)DeviceType.BA8300,
                        (short)DeviceType.RA08,
                        (short)DeviceType.DQ8500,
                        (short)DeviceType.DQ200,
                        (short)DeviceType.TBVision,
                        (short)DeviceType.T2Face,
                    };
                    List<int> userIdsForDualFace = new List<int>();
                    var device = _unitOfWork.IcuDeviceRepository.GetByIcuId(model.DeviceId);
                    if (device == null)
                        throw new Exception($"Can not get device by id = {model.DeviceId}");
                    if (!device.CompanyId.HasValue)
                        throw new Exception($"Device {device.DeviceAddress} not assign to company");

                    if (dualFaceTypes.Contains(device.DeviceType))
                    {
                        if (model.UserIds != null && model.UserIds.Any())
                        {
                            userIdsForDualFace.AddRange(model.UserIds);
                        }
                        else
                        {
                            if (model.VisitIds == null || !model.VisitIds.Any())
                            {
                                if (model.CardIds == null || !model.CardIds.Any())
                                {
                                    userIdsForDualFace.AddRange(workingUnitOfWork.UserRepository.GetByCompanyId(device.CompanyId.Value).Select(m => m.Id));
                                }
                                else
                                {
                                    userIdsForDualFace.AddRange(workingUnitOfWork.CardRepository.GetByIds(device.CompanyId.Value, model.CardIds).Where(m => m.UserId.HasValue).Select(m => m.UserId.Value));
                                }
                            }
                        }
                    }

                    //var building = device.BuildingId.HasValue ? _unitOfWork.BuildingRepository.GetById(device.BuildingId.Value) : new Building();
                    var building = device.Building ?? (device.BuildingId != null ? workingUnitOfWork.BuildingRepository.GetById(device.BuildingId.Value) : new Building());
                    var plugIn = workingUnitOfWork.PlugInRepository.GetPlugInByCompany(device.CompanyId.Value);
                    var pluginPolicy = JsonConvert.DeserializeObject<PlugIns>(plugIn.PlugIns);

                    // filter cards
                    DateTime startFilterCards = DateTime.UtcNow;
                    var data = workingUnitOfWork.CardRepository.GetCardAvailableInDevice(model.DeviceId);
                    if (model.UserIds != null && model.UserIds.Any())
                    {
                        data = data.Where(m => m.UserId.HasValue && model.UserIds.Contains(m.UserId.Value));
                    }
                    if (model.VisitIds != null && model.VisitIds.Any())
                    {
                        data = data.Where(m => m.VisitId.HasValue && model.VisitIds.Contains(m.VisitId.Value));
                    }
                    if (model.CardIds != null && model.CardIds.Any())
                    {
                        data = data.Where(m => model.CardIds.Contains(m.Id));
                    }

                    var cardIds = data.Select(m => m.Id).Distinct().ToList();
                    // in case set isDeleted in database, but not send to device
                    if (model.CardIds != null && model.CardIds.Any())
                    {
                        cardIds.AddRange(model.CardIds);
                        cardIds = cardIds.Distinct().ToList();
                    }
                    DateTime endFilterCards = DateTime.UtcNow;
                    Console.WriteLine($"[AccessControlQueue][SendUserInfo][{model.DeviceAddress}]: Time filter {data.Count()} cards: {endFilterCards.Subtract(startFilterCards).TotalMilliseconds} (ms)");

                    // publish message
                    DateTime startPublishMessage = DateTime.UtcNow;
                    int maxSplitMessage = Helpers.GetMaxSplit(device.DeviceType);
                    int countData = data.Count();
                    int lengthMessage = countData / maxSplitMessage;
                    lengthMessage = lengthMessage * maxSplitMessage < countData ? lengthMessage + 1 : lengthMessage;
                    
                    // Process messages in parallel with controlled concurrency
                    var messageTasks = new List<Task>();
                    var cardsToAdd = new ConcurrentBag<Card>();
                    
                    for (int i = 0; i < lengthMessage; i++)
                    {
                        int messageIndex = i; // Capture loop variable
                        var task = ProcessMessageAsync(messageIndex, maxSplitMessage, countData, cardIds, device, building, model, pluginPolicy, dualFaceTypeAratek, cardsToAdd);
                        messageTasks.Add(task);
                    }
                    
                    await Task.WhenAll(messageTasks);
                    
                    // Batch save all cards
                    if (cardsToAdd.Any())
                    {
                        foreach (var card in cardsToAdd)
                        {
                            workingUnitOfWork.CardRepository.Add(card);
                        }
                        await workingUnitOfWork.SaveAsync();
                    }

                    // send fake user to device dual face
                    if (userIdsForDualFace.Count > 0)
                    {
                        await SendFakeUserToDeviceDualFaceAsync(userIdsForDualFace, device, building, workingUnitOfWork);
                    }

                    DateTime endPublishMessage = DateTime.UtcNow;
                    Console.WriteLine($"[AccessControlQueue][SendUserInfo][{model.DeviceAddress}]: Time publish {lengthMessage} message: {endPublishMessage.Subtract(startPublishMessage).TotalMilliseconds} (ms)");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message + ex.StackTrace);
                }
            }
            else
            {
                if (model.TotalData.DeviceIds == null || !model.TotalData.DeviceIds.Any())
                    return;

                if (model.TotalData.CardIds == null || !model.TotalData.CardIds.Any())
                {
                    List<int> cardIds = new List<int>();
                    if (model.UserIds != null && model.UserIds.Any())
                    {
                        var cardIdsOfUser = workingUnitOfWork.AppDbContext.Card
                            .Where(card => !card.IsDeleted && card.UserId != null && model.UserIds.Contains(card.UserId.Value))
                            .Select(card => card.Id);
                        if (cardIdsOfUser.Any())
                        {
                            cardIds.AddRange(cardIdsOfUser);
                        }
                    }
                    if (model.VisitIds != null && model.VisitIds.Any())
                    {
                        var cardIdsOfVisit= workingUnitOfWork.AppDbContext.Card
                            .Where(card => !card.IsDeleted && card.VisitId != null && model.VisitIds.Contains(card.VisitId.Value))
                            .Select(card => card.Id);
                        if (cardIdsOfVisit.Any())
                        {
                            cardIds.AddRange(cardIdsOfVisit);
                        }
                    }
                    if(model.CardIds != null && model.CardIds.Any())
                        cardIds.AddRange(model.CardIds);

                    model.TotalData.CardIds = cardIds.Distinct().ToList();
                }

                // Get CardInfoBasic list
                // 1. Get list of card data from DB.
                Console.WriteLine($"[DEBUG] TotalData.CardIds count: {model.TotalData.CardIds?.Count ?? 0}, CardIds: [{string.Join(",", model.TotalData.CardIds ?? new List<int>())}]");
                var cards = workingUnitOfWork.AppDbContext.Card
                            .Include(card => card.User.AccessGroup.Parent)
                            .Include(card => card.User.Face)
                            .Include(card => card.User.Department)
                            .Include(card => card.Visit.AccessGroup.Parent)
                            .Where(card => model.TotalData.CardIds.Contains(card.Id)
                                    && !card.IsDeleted);
                Console.WriteLine($"[DEBUG] Found {cards.Count()} cards from database");
                // 2. Convert it to CardInfoBasic list model
                var cardInfos = cards.Select(_mapper.Map<CardInfoBasic>).ToList();
                Console.WriteLine($"[DEBUG] Converted to {cardInfos.Count} CardInfoBasic items");
                cardInfos.ForEach(m =>
                {
                    m.FaceData = (m.IdType == (int)CardType.FaceId && m.User != null) ? _mapper.Map<FaceDataList>(m.User.Face.FirstOrDefault()) : null;
                });

                // Get device list
                Console.WriteLine($"[DEBUG] TotalData.DeviceIds count: {model.TotalData.DeviceIds?.Count ?? 0}, DeviceIds: [{string.Join(",", model.TotalData.DeviceIds ?? new List<int>())}]");
                var devices = workingUnitOfWork.AppDbContext.IcuDevice
                            .Include(device => device.AccessGroupDevice).ThenInclude(agd => agd.AccessGroup)
                            .Include(device => device.AccessGroupDevice).ThenInclude(agd => agd.Tz)
                            .Where(device => model.TotalData.DeviceIds.Contains(device.Id)
                                            && !device.IsDeleted)
                            .ToList();
                Console.WriteLine($"[DEBUG] Found {devices.Count} devices from database");
                
                if (!devices.Any())
                {
                    Console.WriteLine($"[DEBUG] No devices found - exiting SendUserInfoAsync");
                    return;
                }
                
                // Get plugin info
                var plugIn = workingUnitOfWork.PlugInRepository.GetPlugInByCompany(devices.First().CompanyId.Value);
                var pluginPolicy = JsonConvert.DeserializeObject<PlugIns>(plugIn.PlugIns);
                Console.WriteLine($"[DEBUG] Plugin policy loaded successfully");
                // Group by building data
                var groupBuildingDevices = devices.GroupBy(d => d.BuildingId);
                Console.WriteLine($"[DEBUG] Grouped into {groupBuildingDevices.Count()} buildings");
                foreach (var buildingDeviceList in groupBuildingDevices)
                {
                    Console.WriteLine($"[DEBUG] Processing building {buildingDeviceList.Key} with {buildingDeviceList.Count()} devices");
                    // Get building master info
                    var building = buildingDeviceList.First().Building ?? (buildingDeviceList.Key != null ? workingUnitOfWork.BuildingRepository.GetById(buildingDeviceList.Key.Value) : new Building());
                    TimeZoneInfo cstZone = building.TimeZone.ToTimeZoneInfo();
                    TimeSpan offSet = cstZone.BaseUtcOffset;

                    var cardData = cardInfos.Select(item => (CardInfoBasic)item.Clone()).ToList();
                    cardData.ForEach(m =>
                    {
                        m.AdminFlag = 0;
                        m.EffectiveDate = m.EffectiveDateUtc.HasValue
                            ? m.EffectiveDateUtc.Value.ConvertToUserTime(offSet).ToString(Constants.DateTimeFormat.DdMMyyyyHHmm)
                            : "";
                        m.ExpireDate = m.ExpireDateUtc.HasValue
                            ? m.ExpireDateUtc.Value.ConvertToUserTime(offSet).ToString(Constants.DateTimeFormat.DdMMyyyyHHmm)
                            : "";
                    });

                    var groupTypeDevices = buildingDeviceList.GroupBy(d => d.DeviceType);
                    Console.WriteLine($"[DEBUG] Building has {groupTypeDevices.Count()} device types: [{string.Join(",", groupTypeDevices.Select(g => g.Key))}]");
                    foreach (var groupTypeDevice in groupTypeDevices)
                    {
                        Console.WriteLine($"[DEBUG] Processing device type {groupTypeDevice.Key} with {groupTypeDevice.Count()} devices");
                        var cardTypes = Helpers.GetMatchedIdentificationType(groupTypeDevice.Key);
                        var sendData = cardData.Where(card => cardTypes.Contains(card.IdType)).ToList();
                        Console.WriteLine($"[DEBUG] Device type {groupTypeDevice.Key}: cardTypes=[{string.Join(",", cardTypes)}], filtered sendData count={sendData.Count}");
                        int maxSplitMessage = Helpers.GetMaxSplit(groupTypeDevice.Key);

                        // Process devices in parallel
                        Console.WriteLine($"[DEBUG] Starting parallel processing of {groupTypeDevice.Count()} devices with {sendData.Count} cards");
                        var deviceTasks = groupTypeDevice.Select(async device =>
                        {
                            Console.WriteLine($"[DEBUG] Processing device {device.DeviceAddress} (ID: {device.Id})");
                            sendData.ForEach(m => m.Timezone = device.AccessGroupDevice.Any(agd => agd.AccessGroupId == m.AccessGroupId)
                                                            ? device.AccessGroupDevice.FirstOrDefault(agd => agd.AccessGroupId == m.AccessGroupId).Tz.Position
                                                            : device.AccessGroupDevice.Any(agd => agd.AccessGroup.ParentId == m.AccessGroupId)
                                                                ? device.AccessGroupDevice.FirstOrDefault(agd => agd.AccessGroup.ParentId == m.AccessGroupId).Tz.Position
                                                                : Constants.Tz24hPos);

                            int countData = sendData.Count;
                            int lengthMessage = countData / maxSplitMessage;
                            lengthMessage = lengthMessage * maxSplitMessage < countData ? lengthMessage + 1 : lengthMessage;
                            
                            var cardsToAdd = new ConcurrentBag<Card>();
                            var messageTasks = new List<Task>();
                            
                            for (int i = 0; i < lengthMessage; i++)
                            {
                                int messageIndex = i;
                                var task = ProcessDeviceMessageAsync(messageIndex, maxSplitMessage, countData, sendData, device, model, pluginPolicy, dualFaceTypeAratek, cardsToAdd);
                                messageTasks.Add(task);
                            }
                            
                            await Task.WhenAll(messageTasks);
                            
                            // Batch save cards for this device
                            if (cardsToAdd.Any())
                            {
                                foreach (var card in cardsToAdd)
                                {
                                    workingUnitOfWork.CardRepository.Add(card);
                                }
                                await workingUnitOfWork.SaveAsync();
                            }
                        });
                        
                        await Task.WhenAll(deviceTasks);
                    }
                }
            }
        }
        private async Task ProcessMessageAsync(int messageIndex, int maxSplitMessage, int countData, List<int> cardIds, 
            IcuDevice device, Building building, UserInfoQueueModel model, PlugIns pluginPolicy, 
            List<int> dualFaceTypeAratek, ConcurrentBag<Card> cardsToAdd)
        {
            Console.WriteLine($"[DEBUG] ProcessMessageAsync started: messageIndex={messageIndex}, device={device.DeviceAddress}, messageType={model.MessageType}");
            await _concurrencyLimiter.WaitAsync();
            try
            {
                int countCard = maxSplitMessage;
                if (messageIndex == (countData / maxSplitMessage) && (messageIndex + 1) * maxSplitMessage > countData)
                {
                    countCard = countData - messageIndex * maxSplitMessage;
                }

                // Create thread-safe UnitOfWork for this task
                using var threadUnitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                var cardListPaging = threadUnitOfWork.CardRepository.GetCardDetailByIds(
                    cardIds.Skip(messageIndex * maxSplitMessage).Take(countCard).ToList()).ToList();
                var listCards = ConvertToCardInfoThreadSafe(cardListPaging, device, building, threadUnitOfWork);
                
                if (model.OverwriteFloorIndex != null)
                {
                    listCards.ForEach(m => m.FloorIndex = model.OverwriteFloorIndex);
                }
                
                var dataCard = GetMessageSendToDevice(model, device, pluginPolicy, listCards, messageIndex, 
                    countData / maxSplitMessage + (countData % maxSplitMessage > 0 ? 1 : 0));
                
                Console.WriteLine($"[DEBUG] ProcessMessageAsync: About to call SDK - MessageType={model.MessageType}, Device={device.DeviceAddress}, Cards={dataCard.Count}");
                
                if (model.MessageType.Equals(Constants.Protocol.DeleteUser))
                {
                    Console.WriteLine($"[DEBUG] About to call DeleteCardAsync for device {device.DeviceAddress}");
                    await _deviceSDKService.DeleteCardAsync(device.DeviceAddress, dataCard);
                    Console.WriteLine($"[DEBUG] DeleteCardAsync completed for device {device.DeviceAddress}");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] About to call AddCardAsync for device {device.DeviceAddress}");
                    await _deviceSDKService.AddCardAsync(device.DeviceAddress, dataCard);
                    Console.WriteLine($"[DEBUG] AddCardAsync completed for device {device.DeviceAddress}");
                    
                    // Process UserIds and VisitIds for face cards
                    await ProcessFaceCardsAsync(model, device, dualFaceTypeAratek, cardsToAdd, threadUnitOfWork);
                }
                
                // Send progress update to frontend
                if (model.MessageTotal > 0)
                {
                    ApplicationVariables.SendMessageToAllClients(Helpers.JsonConvertCamelCase(new SDKDataWebhookModel()
                    {
                        Type = Constants.SDKDevice.WebhookProcessData,
                        Data = new SDKProcessFeModel()
                        {
                            MsgId = model.MsgId,
                            Index = model.MessageIndex + messageIndex,
                            Total = model.MessageTotal,
                        }
                    }), device.CompanyId ?? 0);
                }
            }
            finally
            {
                _concurrencyLimiter.Release();
            }
        }
        
        private async Task ProcessDeviceMessageAsync(int messageIndex, int maxSplitMessage, int countData, 
            List<CardInfoBasic> sendData, IcuDevice device, UserInfoQueueModel model, PlugIns pluginPolicy, 
            List<int> dualFaceTypeAratek, ConcurrentBag<Card> cardsToAdd)
        {
            Console.WriteLine($"[DEBUG] ProcessDeviceMessageAsync started: device={device.DeviceAddress}, messageType={model.MessageType}, cardCount={sendData.Count}");
            await _concurrencyLimiter.WaitAsync();
            try
            {
                int countCard = maxSplitMessage;
                if (messageIndex == (countData / maxSplitMessage) && (messageIndex + 1) * maxSplitMessage > countData)
                {
                    countCard = countData - messageIndex * maxSplitMessage;
                }

                var listCards = sendData.Skip(messageIndex * maxSplitMessage).Take(countCard).ToList();
                if (model.OverwriteFloorIndex != null)
                {
                    listCards.ForEach(m => m.FloorIndex = model.OverwriteFloorIndex);
                }
                
                var dataCard = GetMessageSendToDevice(model, device, pluginPolicy, listCards, messageIndex, 
                    countData / maxSplitMessage + (countData % maxSplitMessage > 0 ? 1 : 0));
                
                if (model.MessageType.Equals(Constants.Protocol.DeleteUser))
                {
                    Console.WriteLine($"[DEBUG] ProcessDeviceMessageAsync: Calling _deviceSDKService.DeleteCardAsync for device {device.DeviceAddress}");
                    await _deviceSDKService.DeleteCardAsync(device.DeviceAddress, dataCard);
                    Console.WriteLine($"[DEBUG] ProcessDeviceMessageAsync: DeleteCardAsync completed for device {device.DeviceAddress}");
                }
                else
                {
                    Console.WriteLine($"[DEBUG] ProcessDeviceMessageAsync: Calling _deviceSDKService.AddCardAsync for device {device.DeviceAddress}");
                    await _deviceSDKService.AddCardAsync(device.DeviceAddress, dataCard);
                    Console.WriteLine($"[DEBUG] ProcessDeviceMessageAsync: AddCardAsync completed for device {device.DeviceAddress}");
                    
                    // Create thread-safe UnitOfWork for face card processing
                    using var threadUnitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                    await ProcessFaceCardsAsync(model, device, dualFaceTypeAratek, cardsToAdd, threadUnitOfWork);
                }
            }
            finally
            {
                _concurrencyLimiter.Release();
            }
        }
        
        private async Task ProcessFaceCardsAsync(UserInfoQueueModel model, IcuDevice device, 
            List<int> dualFaceTypeAratek, ConcurrentBag<Card> cardsToAdd, IUnitOfWork threadUnitOfWork)
        {
            if (model.UserIds != null)
            {
                foreach (var item in model.UserIds)
                {
                    var cardId = string.Concat(
                        (dualFaceTypeAratek.Contains(device.DeviceType)
                            ? Constants.SDKDevice.PrefixLFaceAratek
                            : Constants.SDKDevice.PrefixLFacePass), item);
                    
                    var isCardLFace = threadUnitOfWork.CardRepository.IsCardIdExist(device.CompanyId.Value, cardId);
                    if (!isCardLFace)
                    {
                        var card = new Card()
                        {
                            CardId = cardId,
                            AccessGroupId = 2,
                            IsMasterCard = false,
                            UserId = item,
                            IssueCount = 0,
                            CardType = (short)CardType.LFaceId,
                            CardStatus = (short)CardStatus.Normal,
                            CompanyId = device.CompanyId.Value,
                            UpdatedBy = 1,
                            CreatedBy = 1,
                            CreatedOn = DateTime.UtcNow,
                            UpdatedOn = DateTime.UtcNow,
                        };
                        
                        cardsToAdd.Add(card);
                        SendToFE(device.CompanyId ?? 0, "System", string.Concat("Add card face success: ", cardId), true);
                    }
                }
            }

            if (model.VisitIds != null)
            {
                foreach (var item in model.VisitIds)
                {
                    var cardId = string.Concat(
                        (dualFaceTypeAratek.Contains(device.DeviceType)
                            ? Constants.SDKDevice.PrefixLFaceAratek
                            : Constants.SDKDevice.PrefixLFacePass), Constants.LFaceConfig.PrefixVisitId, item);
                    
                    var isCardLFace = threadUnitOfWork.CardRepository.IsCardIdExist(device.CompanyId.Value, cardId);
                    if (!isCardLFace)
                    {
                        var card = new Card()
                        {
                            CardId = cardId,
                            AccessGroupId = 2,
                            IsMasterCard = false,
                            VisitId = item,
                            IssueCount = 0,
                            CardType = (short)CardType.LFaceId,
                            CardStatus = (short)CardStatus.Normal,
                            CompanyId = device.CompanyId.Value,
                            UpdatedBy = 1,
                            CreatedBy = 1,
                            CreatedOn = DateTime.UtcNow,
                            UpdatedOn = DateTime.UtcNow,
                        };
                        
                        cardsToAdd.Add(card);
                        SendToFE(device.CompanyId ?? 0, "System", string.Concat("Add card face success: ", card.CardId), true);
                    }
                }
            }
        }

        private void SendToFE(int companyId, string sender, string msgBody, bool isSuccess)
        {
            try
            {
                var data = new NotificationProtocolDataDetail
                {
                    MessageType = isSuccess ? Constants.MessageType.Success : Constants.MessageType.Error,
                    NotificationType = isSuccess
                        ? Constants.NotificationType.TransmitDataSuccess
                        : Constants.NotificationType.TransmitDataError,
                    User = sender,
                    Message = msgBody,
                    RelatedUrl = "",
                    Keep = false,
                };
                _webSocketService.SendWebSocketToFE(Constants.SDKDevice.WebhookNotification, companyId, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }
        public List<CardInfoBasic> ConvertToCardInfoThreadSafe(List<Card> cards, IcuDevice device, Building building, IUnitOfWork unitOfWork)
        {
            var cardInfos = cards.Select(_mapper.Map<CardInfoBasic>).ToList();
            cardInfos.ForEach(m =>
            {
                try
                {
                    m.AdminFlag = 0;
                    m.AntiPassBack = m.CardStatus;
                    m.FaceData = (m.IdType == (int)CardType.FaceId && m.User != null) ? _mapper.Map<FaceDataList>(m.User.Face.FirstOrDefault()) : null;
                    m.EffectiveDate = m.EffectiveDateUtc.HasValue
                        ? m.EffectiveDateUtc.Value.ConvertToUserTime(building.TimeZone).ToString(Constants.DateTimeFormat.DdMMyyyyHHmm)
                        : "";
                    m.ExpireDate = m.ExpireDateUtc.HasValue
                        ? m.ExpireDateUtc.Value.ConvertToUserTime(building.TimeZone).ToString(Constants.DateTimeFormat.DdMMyyyyHHmm)
                        : "";

                    // access group device
                    AccessGroupDevice agD = null;
                    if (m.User != null)
                    {
                        agD = unitOfWork.AccessGroupDeviceRepository.GetDetail(m.User.AccessGroupId, device.Id);
                        if (agD == null)
                        {
                            var parentAg = unitOfWork.AccessGroupRepository.GetParentsByIds(new List<int> { m.User?.AccessGroupId ?? 0 }).FirstOrDefault();
                            if (parentAg != null)
                            {
                                // parent agD
                                agD = unitOfWork.AccessGroupDeviceRepository.GetDetail(parentAg.Id, device.Id);
                            }
                        }

                        m.UserId = m.User?.Id.ToString();
                        m.DepartmentName = m.User?.Department?.DepartName ?? "";
                    }
                    else if (m.Visit != null)
                    {
                        agD = unitOfWork.AccessGroupDeviceRepository.GetDetail(m.Visit.AccessGroupId, device.Id);
                        if (agD == null)
                        {
                            var parentAg = unitOfWork.AccessGroupRepository.GetParentsByIds(new List<int> { m.Visit?.AccessGroupId ?? 0 }).FirstOrDefault();
                            if (parentAg != null)
                            {
                                // parent agD
                                agD = unitOfWork.AccessGroupDeviceRepository.GetDetail(parentAg.Id, device.Id);
                            }
                        }

                        m.UserId = m.Visit?.AliasId ?? m.Visit?.Id.ToString();
                    }

                    if (agD != null)
                    {
                        m.Timezone = agD.Tz?.Position ?? 1;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            return cardInfos;
        }
        
        public List<CardInfoBasic> ConvertToCardInfo(List<Card> cards, IcuDevice device, Building building)
        {
            var cardInfos = cards.Select(_mapper.Map<CardInfoBasic>).ToList();
            cardInfos.ForEach(m =>
            {
                try
                {
                    m.AdminFlag = 0;
                    m.AntiPassBack = m.CardStatus;
                    m.FaceData = (m.IdType == (int)CardType.FaceId && m.User != null) ? _mapper.Map<FaceDataList>(m.User.Face.FirstOrDefault()) : null;
                    m.EffectiveDate = m.EffectiveDateUtc.HasValue
                        ? m.EffectiveDateUtc.Value.ConvertToUserTime(building.TimeZone).ToString(Constants.DateTimeFormat.DdMMyyyyHHmm)
                        : "";
                    m.ExpireDate = m.ExpireDateUtc.HasValue
                        ? m.ExpireDateUtc.Value.ConvertToUserTime(building.TimeZone).ToString(Constants.DateTimeFormat.DdMMyyyyHHmm)
                        : "";

                    // access group device
                    AccessGroupDevice agD = null;
                    if (m.User != null)
                    {
                        agD = _unitOfWork.AccessGroupDeviceRepository.GetDetail(m.User.AccessGroupId, device.Id);
                        if (agD == null)
                        {
                            var parentAg = _unitOfWork.AccessGroupRepository.GetParentsByIds(new List<int> { m.User?.AccessGroupId ?? 0 }).FirstOrDefault();
                            if (parentAg != null)
                            {
                                // parent agD
                                agD = _unitOfWork.AccessGroupDeviceRepository.GetDetail(parentAg.Id, device.Id);
                            }
                        }

                        m.UserId = m.User?.Id.ToString();
                        m.DepartmentName = m.User?.Department?.DepartName ?? "";
                    }
                    else if (m.Visit != null)
                    {
                        agD = _unitOfWork.AccessGroupDeviceRepository.GetDetail(m.Visit.AccessGroupId, device.Id);
                        if (agD == null)
                        {
                            var parentAg = _unitOfWork.AccessGroupRepository.GetParentsByIds(new List<int> { m.Visit?.AccessGroupId ?? 0 }).FirstOrDefault();
                            if (parentAg != null)
                            {
                                // parent agD
                                agD = _unitOfWork.AccessGroupDeviceRepository.GetDetail(parentAg.Id, device.Id);
                            }
                        }

                        m.UserId = m.Visit?.AliasId ?? m.Visit?.Id.ToString();
                    }

                    if (agD != null)
                    {
                        m.Timezone = agD.Tz?.Position ?? 1;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            return cardInfos;
        }

        public List<SDKCardModel> GetMessageSendToDevice(UserInfoQueueModel model, IcuDevice device, PlugIns pluginPolicy,
            List<CardInfoBasic> listCards, int frameIndex, int totalIndex)
        {
            string msgId = Helpers.CreateMsgIdProcess(model.MsgId, model.MessageIndex + frameIndex, model.MessageTotal);
            if (model.DeviceIds != null && model.DeviceIds.Any())
            {
                List<string> receivers = _unitOfWork.IcuDeviceRepository.GetByIds(model.DeviceIds).Select(m => m.DeviceAddress).ToList();
                var dataResult = listCards.AsEnumerable().Select(_mapper.Map<SDKCardModel>).ToList();

                if (model.MessageType.Equals(Constants.Protocol.DeleteUser))
                {
                    dataResult.ForEach(data =>
                    {
                        data.FaceData = null;
                        data.FingerTemplates = null;
                    });
                }
                return dataResult;
            }
            else
            {
                switch (device.DeviceType)
                {
                    case (short)DeviceType.Icu300N:
                    {
                        return listCards.AsEnumerable().Select(_mapper.Map<SDKCardModel>).ToList();
                    }
                    case (short)DeviceType.BA8300:
                    case (short)DeviceType.DF970:
                    case (short)DeviceType.RA08:
                    case (short)DeviceType.DQ8500:
                    case (short)DeviceType.DQ200:
                    case (short)DeviceType.TBVision:
                    case (short)DeviceType.Icu500:
                    case (short)DeviceType.T2Face:
                    {
                        List<SDKCardModel> usersData = new List<SDKCardModel>();
                        foreach (var item in listCards)
                        {
                            var itemConverted = _mapper.Map<SDKCardModel>(item);
                            if (item.Visit != null)
                            {
                                itemConverted.UserId = string.IsNullOrEmpty(item.Visit.AliasId) ? $"{Constants.LFaceConfig.PrefixVisitId}{item.Visit.Id}" : $"{Constants.LFaceConfig.PrefixVisitId}{item.Visit.AliasId}";
                            }
                            usersData.Add(itemConverted);
                        }
                        return usersData;
                    }
                    default:
                    {
                        // listCards.AsEnumerable().Select(_mapper.Map<UserProtocolDetailData>).ToList()
                        var dataResult = listCards.AsEnumerable().Select(_mapper.Map<SDKCardModel>).ToList();

                        if (model.MessageType.Equals(Constants.Protocol.DeleteUser))
                        {
                            dataResult.ForEach(data =>
                            {
                                data.FaceData = null;
                                data.FingerTemplates = null;
                            });
                        }
                        return dataResult;
                    }
                }
            }
        }

        private async Task SendFakeUserToDeviceDualFaceAsync(List<int> userIds, IcuDevice device, Building building, IUnitOfWork unitOfWork = null)
        {
            // Use provided UnitOfWork or create a new thread-safe one
            var threadUnitOfWork = unitOfWork ?? DbHelper.CreateUnitOfWork(_configuration);
            var shouldDispose = unitOfWork == null;
            
            var users = threadUnitOfWork.AppDbContext.User.Include(m => m.Department)
                                .Where(m => userIds.Contains(m.Id) && !string.IsNullOrEmpty(m.Avatar)).ToList();
            var agds = device.CompanyId.HasValue
                ? threadUnitOfWork.AccessGroupDeviceRepository.GetByIcuId(device.CompanyId.Value, device.Id).ToList()
                : new List<AccessGroupDevice>();
            var timezones = threadUnitOfWork.AccessTimeRepository.GetByCompany(device.CompanyId)
                .OrderBy(m => m.Id).Select(m => m.Id).ToArray();
            
            var facePassDevice = new List<short>()
            {
                (short)DeviceType.DF970,
                (short)DeviceType.DQ200,
                (short)DeviceType.RA08,
                (short)DeviceType.T2Face,
            };

            foreach (var item in users)
            {
                var agd = agds.FirstOrDefault(n => n.AccessGroupId == item.AccessGroupId);
                var user = new SDKCardModel()
                {
                    Avatar = item.Avatar,
                    UserId = item.Id.ToString(),
                    CardId = $"{(facePassDevice.Contains(device.DeviceType) ? Constants.SDKDevice.PrefixLFacePass : Constants.SDKDevice.PrefixLFaceAratek)}{item.Id}",
                    IdType = (short)CardType.LFaceId,
                    UserName = item.FirstName,
                    Timezone = agd != null ? Array.IndexOf(timezones, agd.TzId) : 0,
                    Grade = item.Grade,
                    WorkType = ((WorkType)(item.WorkType ?? 0)).GetDescription(),
                    DepartmentName = item.Department.DepartName,
                    EffectiveDate = item.EffectiveDate.HasValue
                        ? item.EffectiveDate.Value.ConvertToUserTime(building.TimeZone)
                            .ToString(Constants.DateTimeFormat.DdMMyyyyHHmm)
                        : "",
                    ExpireDate = item.ExpiredDate.HasValue
                        ? item.ExpiredDate.Value.ConvertToUserTime(building.TimeZone)
                            .ToString(Constants.DateTimeFormat.DdMMyyyyHHmm)
                        : "",
                };
                DateTime d1 = DateTime.UtcNow;
                await _deviceSDKService.AddCardAsync(device.DeviceAddress, [user]);
                DateTime d2 = DateTime.UtcNow;
                Console.WriteLine($"[TIME ADD LFACE][{user.CardId}]: {(d2 - d1).TotalMilliseconds}(ms)");
                var lFaceCard = threadUnitOfWork.CardRepository.GetByCardId(device.CompanyId.Value, user.CardId);
                if (lFaceCard == null)
                {
                    var card = new Card()
                    {
                        CardId = user.CardId,
                        UserId = item.Id,
                        AccessGroupId = user.AccessGroupId,
                        IsMasterCard = item.IsMasterCard,
                        IssueCount = 0,
                        CardType = (short)CardType.LFaceId,
                        CardStatus = (short)CardStatus.Normal,
                        CompanyId = device.CompanyId.Value,
                        UpdatedBy = 1,
                        CreatedBy = 1,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow,
                    };
                    threadUnitOfWork.CardRepository.Add(card);
                    threadUnitOfWork.Save();
                        
                    SendToFE(device.CompanyId ?? 0, "System", string.Concat("Add card face success: ", card.CardId), true);
                }
            }
            
            // Dispose the UnitOfWork if we created it locally
            if (shouldDispose && threadUnitOfWork != null)
            {
                threadUnitOfWork.Dispose();
            }
        }
        
        public void Dispose()
        {
            _concurrencyLimiter?.Dispose();
        }
    }
}