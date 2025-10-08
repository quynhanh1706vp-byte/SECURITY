using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bogus.Extensions;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Exceptions;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.DataModel.Vehicle;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Service.RabbitMqQueue;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace DeMasterProCloud.Service
{
    public interface IVehicleService
    {
        DashBoardVehicleModel GetDashBoardVehicles(DateTime dataAccess);
        List<VehicleListModel> GetListVehiclesByUser(int userId);
        VehicleListModel GetVehiclesById(int vehicleId);
        List<VehicleListModel> GetListVehiclesOfUser(string search, bool isUser, int pageNumber, int pageSize, string sortColumn, string sortDirection, out int recordsTotal, out int recordsFiltered);
        List<VehicleListModel> GetListVehiclesByVisit(int visitId);
        void AddForUser(int userId, VehicleModel model);
        int AddForVisit(int visitId, VehicleModel model);
        void Update(VehicleModel vehicle, int id);
        void Delete(int id);
        Vehicle GetByVehicleId(int id);
        Vehicle GetByPlateNumber(string plateNumber, int companyId);

        List<Vehicle> GetPersonalVehicleByVehicleIds(List<int> ids);
        Vehicle GetAllByVehicleId(int id);

        void EditPersonalVehicle(VehicleModel model);

        void DeletePersonalVehicleMulti(List<Vehicle> vehicles);
        void ApprovePersonalVehicle(Vehicle vehicle);

        Task<ResultImported> ImportFile(string type, MemoryStream stream, int companyId, int accountId, string accountName, bool isUser);
        ResultImported ValidateImportFileHeaders(IFormFile file);
        public byte[] GetFileExcelImportVehicleTemplate(int companyId, bool isUser);
        byte[] Export(string type, bool isUser, int companyId, out int totalRecords, out int recordsFiltered);
    }

    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly HttpContext _httpContext;
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private readonly IVisitService _visitService;
        private readonly IMapper _mapper;
        private readonly IEventLogService _eventLogService;
        private readonly INotificationService _notificationService;

        private readonly string[] _header =
        {
            AccountResource.lblUsername,
            EventLogResource.lblPlateNumber,
            EventLogResource.lblVehicleModel,
            EventLogResource.lblVehicleType,
            UserResource.lblColor,
        };
        public VehicleService(IUnitOfWork unitOfWork, IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor, IUserService userService, IVisitService visitService, IEventLogService eventLogService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _httpContext = httpContextAccessor.HttpContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<VehicleService>();
            _userService = userService;
            _visitService = visitService;
            _mapper = MapperInstance.Mapper;
            _eventLogService = eventLogService;
            _notificationService = notificationService;
        }

        public DashBoardVehicleModel GetDashBoardVehicles(DateTime dataAccess)
        {
            var result = new DashBoardVehicleModel();
            var companyId = _httpContext.User.GetCompanyId();
            
            // get count vehicle of user, visit
            var allVehicles = _unitOfWork.VehicleRepository.GetAllVehicleOfCompany(companyId).ToList();
            result.TotalVehicles = allVehicles.Count;
            result.TotalVehiclesOfUser = allVehicles.Count(x => x.UserId != null);
            result.TotalVehiclesOfVisit = allVehicles.Count(x => x.VisitId != null);
            
            // get vehicle in, out.
            var eventlogVehicle = _eventLogService.GetVehicleNormalAccessByDay(companyId, dataAccess);
            result.TotalVehiclesIn = eventlogVehicle
                .GroupBy(u => u.CardId)
                .Select(g => g.FirstOrDefault(x => x.Antipass.ToLower().Equals("in")))
                .Count(x => x != null);
            result.TotalVehiclesOut = eventlogVehicle
                .GroupBy(u => u.CardId)
                .Select(g => g.FirstOrDefault(x => x.Antipass.ToLower().Equals("out")))
                .Count(x => x != null);
            
            return result;
        }

        public List<VehicleListModel> GetListVehiclesByUser(int userId)
        {
            var vehicles = _unitOfWork.VehicleRepository.GetListVehicleByUser(userId);
            return _mapper.Map < List<VehicleListModel>>(vehicles);
        }

        public VehicleListModel GetVehiclesById(int vehicleId)
        {
            var vehicle = _unitOfWork.VehicleRepository.GetVehicleById(vehicleId);
            return _mapper.Map<VehicleListModel>(vehicle);
        }

        public List<VehicleListModel> GetListVehiclesOfUser(string search, bool isUser, int pageNumber, int pageSize, string sortColumn, string sortDirection, out int recordsTotal, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();

            var vehicles = _unitOfWork.VehicleRepository.GetAllVehicle(isUser, companyId);

            recordsTotal = vehicles.Count();

            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = search.Trim().RemoveDiacritics().ToLower();

                vehicles = vehicles.AsEnumerable().Where(m => (m.PlateNumber?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) ?? false)
                    || (m.Model?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) ?? false)
                    || (m.User != null && m.User.FirstName?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) == true)
                    || (m.Visit != null && m.Visit.VisitorName?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) == true)
                    || (m.Color?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) ?? false)
                    || (GetLocalizedVehicleTypeDescription(m.VehicleType)?.RemoveDiacritics()?.ToLower()?.Contains(normalizedSearch) ?? false)).AsQueryable();
            }
        
            recordsFiltered = vehicles.Count();
        
            List<VehicleListModel> result = new List<VehicleListModel>();
        
            result = _mapper.Map<List<VehicleListModel>>(vehicles.ToList());
        
            var resultQuerable = result.AsQueryable();
        
            if (!string.IsNullOrEmpty(sortColumn))
            {
                sortColumn = sortColumn.ToPascalCase();
        
                resultQuerable = Helpers.SortData(resultQuerable.AsEnumerable(), sortDirection, sortColumn);
            }
        
            if (resultQuerable.FirstOrDefault()?.GetType().GetProperty(sortColumn) != null)
            {
                if (sortDirection.Equals("desc"))
                {
                    resultQuerable = resultQuerable.OrderByDescending(c => c.GetType().GetProperty(sortColumn).GetValue(c, null));
                }
                else if (sortDirection.Equals("asc"))
                {
                    resultQuerable = resultQuerable.OrderBy(c => c.GetType().GetProperty(sortColumn).GetValue(c, null));
                }
            }
        
            if (pageSize > 0)
                resultQuerable = resultQuerable.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        
            return resultQuerable.ToList();
        }

        public void AddForUser(int userId, VehicleModel model)
        {
            try
            {
                var vehicle = _mapper.Map<Vehicle>(model);

                var companyId = _httpContext.User.GetCompanyId();
                vehicle.UserId = userId;
                vehicle.UpdatedBy = _httpContext.User.GetAccountId();
                vehicle.UpdatedOn = DateTime.UtcNow;
                vehicle.CompanyId = companyId;

                var user = _userService.GetByIdAndCompany(userId, companyId);

                _unitOfWork.VehicleRepository.Add(vehicle);
                _unitOfWork.Save();
                AddIdentification(vehicle.PlateNumber, vehicle.VehicleType, vehicle.CompanyId, null, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddForUser");
            }
        }

        public void Update(VehicleModel model, int id)
        {
            try
            {
                var vehicle = _unitOfWork.VehicleRepository.GetById(id);
                string cardOld = vehicle.PlateNumber;
                vehicle.Color = model.Color;
                vehicle.Model = model.Model;
                vehicle.VehicleType = model.VehicleType;
                vehicle.PlateNumber = model.PlateNumber.Replace(" ", "");
                vehicle.UpdatedBy = _httpContext.User.GetAccountId();
                vehicle.UpdatedOn = DateTime.Now;
                _unitOfWork.VehicleRepository.Update(vehicle);
                _unitOfWork.Save();

                if (vehicle.UserId.HasValue)
                {
                    var user = _userService.GetByIdAndCompany(vehicle.UserId.Value, _httpContext.User.GetCompanyId());
                    var cards = _userService.GetCardListByUserId(user.Id);
                    var cardVehicle = cards.FirstOrDefault(m => (m.CardType == (short)CardType.VehicleId ||  m.CardType == (short)CardType.VehicleMotoBikeId) && m.CardId == cardOld);
                    if (cardVehicle != null)
                    {
                        _userService.DeleteCardByUser(user, cardVehicle.Id);
                    }

                    AddIdentification(vehicle.PlateNumber, vehicle.VehicleType, vehicle.CompanyId,null, user);
                }
                else if (vehicle.VisitId.HasValue)
                {
                    Visit visit = _unitOfWork.VisitRepository.GetById(vehicle.VisitId.Value);
                    var cards = _visitService.GetCardListByVisitId(visit.Id);
                    var cardVehicle = cards.FirstOrDefault(m => (m.CardType == (short)CardType.VehicleId ||  m.CardType == (short)CardType.VehicleMotoBikeId) && m.CardId == cardOld);
                    if (cardVehicle != null)
                    {
                        _visitService.DeleteCardByVisit(visit, cardVehicle.Id);
                    }
                    AddIdentification(vehicle.PlateNumber, vehicle.VehicleType, vehicle.CompanyId, visit, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Update");
            }
        }

        public void Delete(int id)
        {
            try
            {
                var vehicle = _unitOfWork.VehicleRepository.GetById(id);
                var company = _unitOfWork.CompanyRepository.GetById(_httpContext.User.GetCompanyId());
                if (vehicle.UserId.HasValue)
                {
                    var user = _userService.GetByIdAndCompany(vehicle.UserId.Value, company.Id);
                    var cards = _userService.GetCardListByUserId(user.Id);
                    //var cardVehicle = cards.FirstOrDefault(m => m.CardType == (short)CardType.VehicleId);
                    var cardVehicle = cards.FirstOrDefault(m => (m.CardType == (short)CardType.VehicleId ||  m.CardType == (short)CardType.VehicleId)
                                                                && m.CardId.ToLower().Equals(vehicle.PlateNumber.ToLower()));
                    if (cardVehicle != null)
                    {
                        _userService.DeleteCardByUser(user, cardVehicle.Id);
                        var devices = _unitOfWork.AccessGroupDeviceRepository
                            .GetByAccessGroupId(company.Id, user.AccessGroupId)
                            .Select(x=>x.Icu)
                            .Where(m => company.AutoSyncUserData || m.ConnectionStatus == (short)ConnectionStatus.Online)
                            .ToList();

                        if (devices.Count > 0)
                        {
                            ThreadSendVehicleToDevice(new List<int>{cardVehicle.Id},null, new List<int>{user.Id},devices,
                                _httpContext.User.GetUsername(), Constants.Protocol.DeleteVehicle);
                            ThreadSendCardToDevice(new List<int>{cardVehicle.Id}, null, new List<int>{user.Id},devices,
                                _httpContext.User.GetUsername(), company, Constants.Protocol.DeleteUser);
                        }
                    }
                }
                else if (vehicle.VisitId.HasValue)
                {
                    var card = _unitOfWork.CardRepository.GetByCardId(company.Id, vehicle.PlateNumber);
                    // send vehicle to device
                    Visit visit = _unitOfWork.VisitRepository.GetById(vehicle.VisitId.Value);
                    var devices = _unitOfWork.AccessGroupDeviceRepository
                        .GetByAccessGroupId(company.Id, visit.AccessGroupId)
                        .Select(x=>x.Icu)
                        .Where(m => company.AutoSyncUserData || m.ConnectionStatus == (short)ConnectionStatus.Online)
                        .ToList();
                    if (devices.Count > 0)
                    {
                        ThreadSendVehicleToDevice(new List<int>{card.Id}, new List<int>{visit.Id}, null,devices,
                            _httpContext.User.GetUsername(), Constants.Protocol.DeleteVehicle);
                        // send plate number same card to ICU device
                        ThreadSendCardToDevice(new List<int>{card.Id}, new List<int>{visit.Id}, null,devices,
                            _httpContext.User.GetUsername(), company, Constants.Protocol.DeleteUser);
                    }

                    _unitOfWork.CardRepository.Delete(card);
                    _unitOfWork.Save();
                }

                _unitOfWork.VehicleRepository.Delete(vehicle);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
            }
        }

        public Vehicle GetByVehicleId(int id)
        {
            return _unitOfWork.VehicleRepository.GetById(id);
        }

        public Vehicle GetAllByVehicleId(int id)
        {
            return _unitOfWork.VehicleRepository.GetAllById(id).FirstOrDefault();
        }

        public List<Vehicle> GetPersonalVehicleByVehicleIds(List<int> ids)
        {
            var vehicles = _unitOfWork.VehicleRepository.GetVehicleByIds(ids);

            return vehicles.ToList();
        }

        public Vehicle GetByPlateNumber(string plateNumber, int companyId)
        {
            plateNumber = plateNumber.Replace(" ", "");
            return _unitOfWork.VehicleRepository.GetByPlateNumber(plateNumber, companyId);
        }

        public List<VehicleListModel> GetListVehiclesByVisit(int visitId)
        {
             var vehicles = _unitOfWork.VehicleRepository.GetListVehicleByVisit(visitId);
             return _mapper.Map<List<VehicleListModel>>(vehicles);
        }

        public int AddForVisit(int visitId, VehicleModel model)
        {
            try
            {
                var vehicle = _mapper.Map<Vehicle>(model);
                var companyId = _httpContext.User.GetCompanyId();
                vehicle.VisitId = visitId;
                vehicle.UpdatedBy = _httpContext.User.GetAccountId();
                vehicle.UpdatedOn = DateTime.Now;
                vehicle.CompanyId = companyId;

                var visitor = _unitOfWork.VisitRepository.GetByVisitId(companyId, visitId);

                _unitOfWork.VehicleRepository.Add(vehicle);
                _unitOfWork.Save();
                AddIdentification(vehicle.PlateNumber, vehicle.VehicleType, vehicle.CompanyId, visitor, null);

                return vehicle.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddForVisit");
                return 0;
            }
        }

        private void AddIdentification(string cardId, int vehicleType, int companyId, Visit visit, User user, string action = Constants.Protocol.AddUser)
        {
            IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
            IUserService userService = new UserService(unitOfWork, new HttpContextAccessor(), null,
                _configuration, ApplicationVariables.LoggerFactory.CreateLogger<UserService>(), null);
            var cardDb = unitOfWork.CardRepository.GetByCardId(companyId, cardId);
            if (cardDb != null && (cardDb.CardType == (short)CardType.VehicleId || cardDb.CardType == (short)CardType.VehicleMotoBikeId))
            {
                // card existed in database
                return;
            }
            var company = unitOfWork.CompanyRepository.GetById(companyId);
            if (user != null)
            {
                userService.AddIdentification(user, new CardModel()
                {
                    CardId = cardId,
                    //CardStatus = userArmy == null ? (short)CardStatus.Normal : (short)CardStatus.InValid,
                    CardStatus = (short)CardStatus.Normal,
                    CardType = vehicleType == (short)VehicleType.MotoBike ? (short)CardType.VehicleMotoBikeId : (short)CardType.VehicleId
                });
                // send vehicle to device
                var devices = unitOfWork.AccessGroupDeviceRepository
                    .GetByAccessGroupId(user.CompanyId, user.AccessGroupId)
                    .Select(m => m.Icu)
                    .Where(m => company.AutoSyncUserData || m.ConnectionStatus == (short)ConnectionStatus.Online)
                    .ToList();
                
                var cardIds = user.Card.Where(x => x.CardId.ToLower() == cardId.ToLower()).Select(x => x.Id).ToList();
                if (devices.Count > 0)
                {
                    ThreadSendCardToDevice(cardIds,null,new List<int>{user.Id}, devices, Constants.RabbitMq.SenderDefault, company, action);
                }
            }
            else if (visit != null)
            {
                var card = new Card
                {
                    CardId = cardId,
                    IssueCount = 0,
                    CompanyId = visit.CompanyId,
                    VisitId = visit.Id,
                    CardType = vehicleType == (short)VehicleType.MotoBike ? (short)CardType.VehicleId : (short)CardType.VehicleMotoBikeId,
                    AccessGroupId = visit.AccessGroupId,
                    ValidFrom = visit.StartDate,
                    ValidTo = visit.EndDate,
                    CreatedBy = 1,
                    CreatedOn = DateTime.Now,
                    UpdatedBy = 1,
                    UpdatedOn = DateTime.Now
                };
                unitOfWork.CardRepository.Add(card);
                unitOfWork.Save();

                // send vehicle to device
                var devices = unitOfWork.AccessGroupDeviceRepository
                    .GetByAccessGroupId(visit.CompanyId, visit.AccessGroupId)
                    .Select(m => m.Icu)
                    .Where(m => company.AutoSyncUserData || m.ConnectionStatus == (short)ConnectionStatus.Online)
                    .ToList();
                
                var cardIds = visit.Card.Where(x => x.CardId.ToLower() == cardId.ToLower()).Select(x => x.Id).ToList();
                if (devices.Count > 0)
                {
                    ThreadSendCardToDevice(cardIds,new List<int>{visit.Id}, null,devices,Constants.RabbitMq.SenderDefault, company, action);
                }
            }
        }
        private void ThreadSendVehicleToDevice(List<int> cardIds,List<int> visitIds, List<int> userIds ,List<IcuDevice> devices, string sender, 
            string messageType = Constants.Protocol.AddVehicle)
        {
            Thread thread = new Thread(() =>
            {
                IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                IWebSocketService webSocketService = new WebSocketService();
                var accessControlQueue = new AccessControlQueue(unitOfWork, webSocketService);
                try
                {
                    // device
                    foreach (var device in devices)
                    {
                        accessControlQueue.SendUserInfo(new UserInfoQueueModel()
                        {
                            DeviceAddress = device.DeviceAddress,
                            DeviceId = device.Id,
                            MessageType = messageType,
                            MsgId = Guid.NewGuid().ToString(),
                            Sender = sender,
                            VisitIds = visitIds,
                            CardIds = cardIds,
                            UserIds = userIds
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    unitOfWork.Dispose();
                }
            });
            thread.Start();
            thread.Join();
        }
        private void ThreadSendCardToDevice(List<int> cardIds,List<int> visitIds, List<int> userIds ,List<IcuDevice> devices, string sender, Company company,
            string messageType = Constants.Protocol.AddUser)
        {
            Thread thread = new Thread(() =>
            {
                IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                IWebSocketService webSocketService = new WebSocketService();
                try
                {
                    var deviceInstructionQueue = new DeviceInstructionQueue(unitOfWork, _configuration, webSocketService);
                    deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                    {
                        MessageType = messageType,
                        MsgId = Guid.NewGuid().ToString(),
                        Sender = sender,
                        VisitIds = visitIds,
                        UserIds = userIds,
                        CardIds = cardIds,
                        CardFilterIds = cardIds,
                        CompanyCode = company.Code,
                        DeviceIds = devices.Select(m => m.Id).Distinct().ToList(),
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            });
            thread.Start();
            thread.Join();
        }
        
        /// <summary>
        /// Edit personal vehicle info
        /// </summary>
        /// <returns></returns>
        public void EditPersonalVehicle(VehicleModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var companyId = _httpContext.User.GetCompanyId();
                        var vehicle = _unitOfWork.VehicleRepository.GetById(model.Id);
                        var card = _unitOfWork.CardRepository.GetByCardId(companyId, vehicle.PlateNumber);

                        if (card != null)
                        {
                            if (vehicle.UserId != null)
                            {
                                var user = _unitOfWork.UserRepository.GetById(vehicle.UserId.Value);
                                var devices = new List<IcuDevice>();
                                var company = _unitOfWork.CompanyRepository.GetById(companyId);
                                if (user != null)
                                {
                                    // send vehicle to device
                                    devices = _unitOfWork.AccessGroupDeviceRepository
                                        .GetByAccessGroupId(user.CompanyId, user.AccessGroupId)
                                        .Select(m => m.Icu)
                                        .Where(m => company.AutoSyncUserData || m.ConnectionStatus == (short)ConnectionStatus.Online)
                                        .ToList();
                                    ThreadSendCardToDevice(new List<int>(){card.Id}, null,new List<int>{user.Id},devices,_httpContext.User.GetUsername(), company, Constants.Protocol.DeleteUser);
                                }

                                if (!card.CardId.Equals(model.PlateNumber))
                                {
                                    // Server should update card too, when the plate number is changed.
                                    card.CardId = model.PlateNumber;

                                    _unitOfWork.CardRepository.Update(card);
                                    _unitOfWork.Save();
                                }

                                CardModel cardModel = new CardModel()
                                {
                                    CardId = model.PlateNumber,
                                    Id = card.Id,
                                    CardStatus = card.CardStatus,
                                    CardType = model.VehicleType == (int)VehicleType.MotoBike ? (int)CardType.VehicleMotoBikeId : (int)CardType.VehicleId,
                                    IssueCount = card.IssueCount
                                };

                                vehicle.PlateNumber = model.PlateNumber;
                                vehicle.Model = model.Model;
                                vehicle.Color = model.Color;
                                vehicle.VehicleType = model.VehicleType;

                                _unitOfWork.VehicleRepository.Update(vehicle);

                                _unitOfWork.Save();

                                _userService.UpdateCardByUser(vehicle.UserId.Value, card.Id, cardModel);
                                transaction.Commit();
                                if (user != null)
                                {
                                    // send vehicle to device
                                    ThreadSendCardToDevice(new List<int>(){card.Id}, null,new List<int>{user.Id},devices,_httpContext.User.GetUsername(), company);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[ERROR] EDIT PERSONAL VEHICLE. Message : {e.Message}, StackTrace : {e.StackTrace}");

                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }


        /// <summary>
        /// Approve personal vehicle info
        /// </summary>
        /// <returns></returns>
        public void ApprovePersonalVehicle(Vehicle vehicle)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        _unitOfWork.VehicleRepository.Update(vehicle);

                        _unitOfWork.Save();

                        var card = _unitOfWork.CardRepository.GetByCardId(vehicle.CompanyId, vehicle.PlateNumber);

                        CardModel cardModel = new CardModel()
                        {
                            CardId = card.CardId,
                            Id = card.Id,
                            CardStatus = (int)CardStatus.Normal,
                            CardType = card.CardType,
                            IssueCount = card.IssueCount
                        };

                        _userService.UpdateCardByUser(vehicle.UserId.Value, card.Id, cardModel);

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
        /// Delete Multiple personal vehicle info
        /// </summary>
        /// <returns></returns>
        public void DeletePersonalVehicleMulti(List<Vehicle> vehicles)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var vehicle in vehicles)
                        {
                            Delete(vehicle.Id);
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
        }

        public ResultImported ValidateImportFileHeaders(IFormFile file)
        {
            try
            {
                using (var package = new ExcelPackage(FileHelpers.ConvertToStream(file)))
                {
                    ExcelWorksheet worksheet;
                    int columnCount;
                    try
                    {
                        worksheet = package.Workbook.Worksheets[1];
                        columnCount = worksheet.Dimension.End.Column;
                    }
                    catch (Exception)
                    {
                        return new ResultImported()
                        {
                            Result = false,
                            Message = MessageResource.msgFileIncorrectFormat
                        };
                    }

                    // Validate header columns match expected structure
                    if (columnCount != _header.Length)
                    {
                        return new ResultImported()
                        {
                            Result = false,
                            Message = string.Format(MessageResource.msgFileIncorrectFormatColumns, _header.Length, columnCount)
                        };
                    }

                    // Validate actual header values match expected headers
                    for (int col = 1; col <= columnCount; col++)
                    {
                        var actualHeader = Convert.ToString(worksheet.Cells[1, col].Value ?? "").Trim();
                        var expectedHeader = _header[col - 1];

                        if (!actualHeader.Equals(expectedHeader, StringComparison.OrdinalIgnoreCase))
                        {
                            return new ResultImported()
                            {
                                Result = false,
                                Message = string.Format(MessageResource.msgFileIncorrectFormatHeader, expectedHeader, col, actualHeader)
                            };
                        }
                    }

                    return new ResultImported() { Result = true, Message = MessageResource.msgFileIncorrectFormat };
                }
            }
            catch (Exception ex)
            {
                return new ResultImported()
                {
                    Result = false,
                    Message = string.Format(MessageResource.msgFileIncorrectFormat, ex.Message)
                };
            }
        }

        public async Task<ResultImported> ImportFile(string type, MemoryStream stream, int companyId, int accountId, string accountName, bool isUser)
        {
            var data = new List<VehicleImportModel>();
            IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
            try
            {
                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet;
                    try
                    {
                        worksheet = package.Workbook.Worksheets[1];
                    }
                    catch (Exception)
                    {
                        throw new InvalidFormatException();
                    }

                    if (isUser)
                    {
                        var users = unitOfWork.UserRepository.GetByCompanyId(companyId);

                        for (int i = worksheet.Dimension.Start.Row + 1; i <= worksheet.Dimension.End.Row; i++)
                        {
                            var cells = worksheet.Cells;
                            var name = Convert.ToString(cells[i, Array.IndexOf(_header, AccountResource.lblUsername) + 1].Value ?? "");

                            // Ignore row that doesnt have name
                            if (string.IsNullOrEmpty(name))
                                continue;

                            var item = ReadDataUserFromExcel(worksheet, i, users);
                            data.Add(item);
                        }
                    }
                    else
                    {
                        var visits = unitOfWork.VisitRepository.GetVisitInvalidInCompanyId(companyId);

                        for (int i = worksheet.Dimension.Start.Row + 1; i <= worksheet.Dimension.End.Row; i++)
                        {
                            var cells = worksheet.Cells;
                            var name = Convert.ToString(cells[i, Array.IndexOf(_header, AccountResource.lblUsername) + 1].Value ?? "");

                            // Ignore row that doesnt have name
                            if (string.IsNullOrEmpty(name))
                                continue;

                            var item = ReadDataVisitFromExcel(worksheet, i, visits);
                            data.Add(item);
                        }
                    }
                }
                var result = Import(unitOfWork, data, companyId, isUser);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}:{Environment.NewLine} {e.StackTrace}");
                throw;
            }
        }
        private VehicleImportModel ReadDataUserFromExcel(ExcelWorksheet worksheet, int row, IQueryable<User> users)
        {
            var colIndex = 1;
            var cells = worksheet.Cells;
            var model = new VehicleImportModel();

            var userCode = Convert.ToString(cells[row, colIndex++].Value)?.Trim().Split(" - ")[1];

            var matchedUser = users.FirstOrDefault(x => x.UserCode == userCode);
            if (matchedUser != null)
            {
                model.SetUserId(matchedUser.Id.ToString());
            }
            else
            {
                model.SetUserId("0");
            }

            model.SetPlateNumber(Convert.ToString(cells[row, colIndex++].Value)?.Trim());
            model.SetModel(Convert.ToString(cells[row, colIndex++].Value)?.Trim());
            model.SetVehicleType(Convert.ToString(cells[row, colIndex++].Value)?.Trim());
            model.SetColor(Convert.ToString(cells[row, colIndex++].Value)?.Trim());

            return model;
        }
        private VehicleImportModel ReadDataVisitFromExcel(ExcelWorksheet worksheet, int row, IQueryable<Visit> visits)
        {
            var colIndex = 1;
            var cells = worksheet.Cells;
            var model = new VehicleImportModel();

            var visitId = Convert.ToString(cells[row, colIndex++].Value)?.Trim().Split(" - ")[1];

            var matchedVisit = visits.FirstOrDefault(x => x.Id.ToString() == visitId);
            if (matchedVisit != null)
            {
                model.SetUserId(matchedVisit.Id.ToString());
            }
            else
            {
                model.SetUserId("0");
            }

            model.SetPlateNumber(Convert.ToString(cells[row, colIndex++].Value)?.Trim());
            model.SetModel(Convert.ToString(cells[row, colIndex++].Value)?.Trim());
            model.SetVehicleType(Convert.ToString(cells[row, colIndex++].Value)?.Trim());
            model.SetColor(Convert.ToString(cells[row, colIndex++].Value)?.Trim());

            return model;
        }

        public ResultImported Import(IUnitOfWork unitOfWork,List<VehicleImportModel> listImportVehicles, int companyId, bool isUser)
        {
            Vehicle vehicle = null;
            int addCount = 0, updateCount = 0, count = 0;
            int totalCount = listImportVehicles.Count;
            var start = DateTime.Now;
            try
            {
                foreach (var vehicleData in listImportVehicles)
                {
                    unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
                    {
                        using (var transaction = unitOfWork.AppDbContext.Database.BeginTransaction())
                        {
                            try
                            {
                                // Skip records with invalid data
                                if (string.IsNullOrWhiteSpace(vehicleData.PlateNumber?.Value) || vehicleData.UserId?.Value == null || vehicleData.UserId.Value == 0)
                                {
                                    count += 1;
                                    return; // Skip this record
                                }

                                var vehicleExits = unitOfWork.VehicleRepository.GetByPlateNumber(vehicleData.PlateNumber.Value, companyId);
                                if (vehicleExits == null) // add
                                {
                                    vehicle = _mapper.Map<Vehicle>(vehicleData);
                                    if (isUser)
                                    {
                                        vehicle.UserId = vehicleData.UserId.Value;
                                        var user = unitOfWork.UserRepository.GetById(vehicle.UserId ?? 0);
                                        if (user != null)
                                        {
                                            AddIdentification(vehicleData.PlateNumber.Value, vehicleData.VehicleType.Value ?? (int)VehicleType.MotoBike, companyId, null, user);
                                        }
                                    }
                                    else
                                    {
                                        vehicle.VisitId = vehicleData.UserId.Value;
                                        var visit = unitOfWork.VisitRepository.GetById(vehicle.VisitId ?? 0);
                                        if (visit != null)
                                        {
                                            AddIdentification(vehicleData.PlateNumber.Value, vehicleData.VehicleType.Value ?? (int)VehicleType.MotoBike, companyId, visit, null);
                                        }
                                    }

                                    vehicle.CompanyId = companyId;

                                    unitOfWork.VehicleRepository.Add(vehicle);
                                    unitOfWork.Save();
                                    addCount++;
                                    count += 1;
                                }
                                else // update
                                {
                                    if (isUser)
                                    {
                                        vehicleExits.UserId = vehicleData.UserId.Value;

                                        var user = unitOfWork.UserRepository.GetById(vehicleExits.UserId ?? 0);
                                        if (user != null)
                                        {
                                            AddIdentification(vehicleExits.PlateNumber, vehicleExits.VehicleType, companyId, null, user, Constants.Protocol.DeleteUser);
                                            AddIdentification(vehicleData.PlateNumber.Value, vehicleData.VehicleType.Value ?? (int)VehicleType.MotoBike, companyId, null, user);
                                        }
                                    }
                                    else
                                    {
                                        vehicleExits.VisitId = vehicleData.UserId.Value;

                                        var visit = unitOfWork.VisitRepository.GetById(vehicleExits.VisitId ?? 0);
                                        if (visit != null)
                                        {
                                            AddIdentification(vehicleExits.PlateNumber, vehicleExits.VehicleType, companyId, visit, null, Constants.Protocol.DeleteUser);
                                            AddIdentification(vehicleData.PlateNumber.Value, vehicleData.VehicleType.Value ?? (int)VehicleType.MotoBike, companyId, visit, null);
                                        }
                                    }

                                    vehicleExits.PlateNumber = vehicleData.PlateNumber.Value;
                                    vehicleExits.VehicleType = vehicleData.VehicleType.Value ?? (int)VehicleType.Car;
                                    vehicleExits.Model = vehicleData.Model.Value;
                                    vehicleExits.Color = vehicleData.Color.Value;

                                    unitOfWork.VehicleRepository.Update(vehicleExits);
                                    unitOfWork.Save();
                                    updateCount++;
                                    count += 1;
                                }
                                
                                if (count % 100 == 0)
                                {
                                    _notificationService.SendMessage(Constants.MessageType.Success,
                                        Constants.NotificationType.TransmitDataSuccess, "SYSTEM",
                                        string.Format(MessageResource.msgVehicleImporting, count, count, totalCount), companyId);
                                }

                                transaction.Commit();
                                
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                                throw;
                            }
                        }
                    });
                }
            }
            catch (Exception e)
            {
                return new ResultImported
                {
                    Result = false,
                    Message = String.Format(MessageResource.ImportVehicleAllocationError, (addCount + updateCount), addCount, (totalCount - addCount - updateCount))
                };
            }

            Console.WriteLine("[Import Vehicle: ]       {0}", DateTime.Now.Subtract(start).TotalMilliseconds);
            if ((addCount + updateCount) == totalCount)
            {
                _notificationService.SendMessage(Constants.MessageType.Success,
                    Constants.NotificationType.TransmitDataSuccess, "SYSTEM",
                    String.Format(MessageResource.ImportSuccess, (addCount + updateCount), addCount, updateCount), companyId);

                return new ResultImported
                {
                    Result = true,
                    Message = String.Format(MessageResource.ImportSuccess, (addCount + updateCount), addCount, updateCount)
                };
            }

            _notificationService.SendMessage(Constants.MessageType.Error,
                Constants.NotificationType.TransmitDataError, "SYSTEM",
                String.Format(MessageResource.ImportVehicleAllocationError, (addCount + updateCount), addCount, (totalCount - addCount - updateCount)), companyId);

            return new ResultImported
            {
                Result = false,
                Message = String.Format(MessageResource.ImportVehicleAllocationError, (addCount + updateCount), addCount, totalCount - addCount - updateCount)
            };
        }

        public byte[] GetFileExcelImportVehicleTemplate(int companyId, bool isUser)
        {
            try
            {
                using (var package = new ExcelPackage())
            {
                // === Sheet 1: VEHICLES ===
                var worksheet = package.Workbook.Worksheets.Add("Vehicles");

                // Add headers
                worksheet.Row(1).Style.Font.Bold = true;
                for (int i = 0; i < _header.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = _header[i];
                }

                // === Sheet 2: DEFINE USER ===
                var defineSheet = package.Workbook.Worksheets.Add("DefineData");

                if (isUser)
                {
                    defineSheet.Row(1).Style.Font.Bold = true;
                    defineSheet.Cells[1, 1].Value = "UserName";
                    defineSheet.Cells[1, 2].Value = "UserId";

                    var users = _unitOfWork.UserRepository.GetByCompanyId(companyId).OrderBy(x => x.FirstName).ToList();

                    // Fill user data vào DefineUser
                    for (int i = 0; i < users.Count; i++)
                    {
                        defineSheet.Cells[i + 2, 1].Value = string.Concat(users[i].FirstName, " - " , users[i].UserCode);
                        defineSheet.Cells[i + 2, 2].Value = users[i].Id;
                    }

                    // Tạo named range cho DisplayName để làm dropdown
                    string nameRange = "UserNameList";
                    var nameRangeAddress = ExcelCellBase.GetAddress(2, 1, users.Count + 1, 1); // A2:A{N}
                    package.Workbook.Names.Add(nameRange, defineSheet.Cells[nameRangeAddress]);

                    for (int i = 2; i <= 100; i++)
                    {
                        var validation = worksheet.DataValidations.AddListValidation($"A{i}");
                        validation.Formula.ExcelFormula = nameRange;
                        validation.ShowErrorMessage = true;
                        validation.Error = "Vui lòng chọn nhân viên từ danh sách.";
                    }
                }
                else
                {
                    defineSheet.Row(1).Style.Font.Bold = true;
                    defineSheet.Cells[1, 1].Value = "VisitName";
                    defineSheet.Cells[1, 2].Value = "VisitId";

                    var visits = _unitOfWork.VisitRepository.GetVisitInvalidInCompanyId(companyId).OrderBy(x => x.VisitorName).ToList();

                    // Fill visit data into DefineData sheet
                    for (int i = 0; i < visits.Count; i++)
                    {
                        defineSheet.Cells[i + 2, 1].Value = string.Concat(visits[i].VisitorName, " - " , visits[i].Id);
                        defineSheet.Cells[i + 2, 2].Value = visits[i].Id;
                    }

                    // Create named range for visit names dropdown
                    string nameRange = "VisitNameList";
                    if (visits.Count > 0)
                    {
                        var nameRangeAddress = ExcelCellBase.GetAddress(2, 1, visits.Count + 1, 1); // A2:A{N}
                        package.Workbook.Names.Add(nameRange, defineSheet.Cells[nameRangeAddress]);
                    }
                    else
                    {
                        // Create a single cell range when no visits exist
                        var nameRangeAddress = ExcelCellBase.GetAddress(2, 1, 2, 1); // A2:A2
                        package.Workbook.Names.Add(nameRange, defineSheet.Cells[nameRangeAddress]);
                    }

                    for (int i = 2; i <= 100; i++)
                    {
                        var validation = worksheet.DataValidations.AddListValidation($"A{i}");
                        validation.Formula.ExcelFormula = nameRange;
                        validation.ShowErrorMessage = true;
                        validation.Error = "Vui lòng chọn khách mời từ danh sách.";
                    }
                }
                
                // === Sheet 3: DEFINE VEHICLE TYPE ===
                var defineSheetVehicleType = package.Workbook.Worksheets.Add("VehicleType");

                defineSheetVehicleType.Row(1).Style.Font.Bold = true;
                defineSheetVehicleType.Cells[1, 1].Value = "Loại xe";
                defineSheetVehicleType.Cells[1, 2].Value = "Mô tả";

                // Fill user data vào Define vehicle type
                defineSheetVehicleType.Cells[2, 1].Value = 0;
                defineSheetVehicleType.Cells[2, 2].Value = DeviceResource.lblCar;
                defineSheetVehicleType.Cells[3, 1].Value = 1;
                defineSheetVehicleType.Cells[3, 2].Value = DeviceResource.lblMotoBike;
                
                // Sample data
                for (int i = 0; i < 3; i++)
                {
                    int col = 2;
                    worksheet.Cells[i + 2, col++].Value = $"88K1234{i}";
                    worksheet.Cells[i + 2, col++].Value = $"Honda";
                    worksheet.Cells[i + 2, col++].Value = 0;
                    worksheet.Cells[i + 2, col++].Value = $"Red";
                }

                worksheet.Cells.AutoFitColumns();
                defineSheet.Cells.AutoFitColumns();

                    return package.GetAsByteArray();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFileExcelImportVehicleTemplate");
                return new byte[0];
            }
        }

        public byte[] Export(string type, bool isUser, int companyId, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                byte[] fileByte;

                try
                {
                    var data = GetDataWithOrder(isUser, companyId, out totalRecords, out recordsFiltered);

                    switch (type.ToLower())
                    {
                        default:
                            fileByte = ExportExcel(data);
                            break;
                    }

                    string contentsDetail = String.Empty;
                    var successContent = $"{ActionLogTypeResource.Export} {ActionLogTypeResource.Success}";
                    _unitOfWork.SystemLogRepository.Add(0, SystemLogType.Vehicle, ActionLogType.Export,
                        successContent, contentsDetail, null, _httpContext.User.GetCompanyId());
                    _unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message + ex.StackTrace);
                    var errorContent = $"{ActionLogTypeResource.Export} {ActionLogTypeResource.Fail}";
                    _unitOfWork.SystemLogRepository.Add(0, SystemLogType.Vehicle, ActionLogType.Export,
                        errorContent, null, null, _httpContext.User.GetCompanyId());
                    _unitOfWork.Save();
                    throw;
                }
                return fileByte;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Export");
                totalRecords = 0;
                recordsFiltered = 0;
                return new byte[0];
            }
        }

        private IEnumerable<Vehicle> GetDataWithOrder(bool isUser, int companyId, out int totalRecords, out int recordsFiltered)
        {
            var data = _unitOfWork.VehicleRepository.GetAllVehicle(isUser, companyId);

            recordsFiltered = data.Count();
            totalRecords = recordsFiltered;

            // Default sort ( asc - PlateNumber )
            data = data.OrderBy(x => x.PlateNumber);

            return data;
        }
        private byte[] ExportExcel(IEnumerable<Vehicle> data)
        {
            byte[] result;
            var companyId = _httpContext.User.GetCompanyId();
            var vehicles = data.ToList();

            using (var package = new ExcelPackage())
            {
                // === Sheet 1: VEHICLES ===
                var worksheet = package.Workbook.Worksheets.Add("Vehicles");

                // Add headers
                worksheet.Row(1).Style.Font.Bold = true;
                for (int i = 0; i < _header.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = _header[i];
                }

                // Fill actual vehicle data
                for (int i = 0; i < vehicles.Count; i++)
                {
                    var vehicle = vehicles[i];
                    int row = i + 2; // Start from row 2 (after header)
                    int col = 1;

                    // Username (User or Visit name)
                    if (vehicle.UserId.HasValue && vehicle.User != null)
                    {
                        worksheet.Cells[row, col++].Value = string.Concat(vehicle.User.FirstName, " - ", vehicle.User.UserCode);
                    }
                    else if (vehicle.VisitId.HasValue && vehicle.Visit != null)
                    {
                        worksheet.Cells[row, col++].Value = string.Concat(vehicle.Visit.VisitorName, " - ", vehicle.Visit.Id);
                    }
                    else
                    {
                        worksheet.Cells[row, col++].Value = "";
                    }

                    // Plate Number
                    worksheet.Cells[row, col++].Value = vehicle.PlateNumber ?? "";

                    // Vehicle Model
                    worksheet.Cells[row, col++].Value = vehicle.Model ?? "";

                    // Vehicle Type
                    worksheet.Cells[row, col++].Value = vehicle.VehicleType;

                    // Color
                    worksheet.Cells[row, col++].Value = vehicle.Color ?? "";
                }

                // === Sheet 2: DEFINE USER/VISIT DATA ===
                var defineSheet = package.Workbook.Worksheets.Add("DefineData");

                // Determine if this is for users or visits based on the data
                bool isUser = vehicles.Any(v => v.UserId.HasValue);

                if (isUser)
                {
                    defineSheet.Row(1).Style.Font.Bold = true;
                    defineSheet.Cells[1, 1].Value = "UserName";
                    defineSheet.Cells[1, 2].Value = "UserId";

                    var users = _unitOfWork.UserRepository.GetByCompanyId(companyId).OrderBy(x => x.FirstName).ToList();

                    // Fill user data
                    for (int i = 0; i < users.Count; i++)
                    {
                        defineSheet.Cells[i + 2, 1].Value = string.Concat(users[i].FirstName, " - ", users[i].UserCode);
                        defineSheet.Cells[i + 2, 2].Value = users[i].Id;
                    }

                    // Create named range for dropdown validation
                    if (users.Count > 0)
                    {
                        string nameRange = "UserNameList";
                        var nameRangeAddress = ExcelCellBase.GetAddress(2, 1, users.Count + 1, 1);
                        package.Workbook.Names.Add(nameRange, defineSheet.Cells[nameRangeAddress]);

                        for (int i = 2; i <= Math.Max(100, vehicles.Count + 10); i++)
                        {
                            var validation = worksheet.DataValidations.AddListValidation($"A{i}");
                            validation.Formula.ExcelFormula = nameRange;
                            validation.ShowErrorMessage = true;
                            validation.Error = "Vui lòng chọn nhân viên từ danh sách.";
                        }
                    }
                }
                else
                {
                    defineSheet.Row(1).Style.Font.Bold = true;
                    defineSheet.Cells[1, 1].Value = "VisitName";
                    defineSheet.Cells[1, 2].Value = "VisitId";

                    var visits = _unitOfWork.VisitRepository.GetVisitInvalidInCompanyId(companyId).OrderBy(x => x.VisitorName).ToList();

                    // Fill visit data
                    for (int i = 0; i < visits.Count; i++)
                    {
                        defineSheet.Cells[i + 2, 1].Value = string.Concat(visits[i].VisitorName, " - ", visits[i].Id);
                        defineSheet.Cells[i + 2, 2].Value = visits[i].Id;
                    }

                    // Create named range for dropdown validation
                    if (visits.Count > 0)
                    {
                        string nameRange = "UserNameList";
                        var nameRangeAddress = ExcelCellBase.GetAddress(2, 1, visits.Count + 1, 1);
                        package.Workbook.Names.Add(nameRange, defineSheet.Cells[nameRangeAddress]);

                        for (int i = 2; i <= Math.Max(100, vehicles.Count + 10); i++)
                        {
                            var validation = worksheet.DataValidations.AddListValidation($"A{i}");
                            validation.Formula.ExcelFormula = nameRange;
                            validation.ShowErrorMessage = true;
                            validation.Error = "Vui lòng chọn khách mời từ danh sách.";
                        }
                    }
                }

                // === Sheet 3: DEFINE VEHICLE TYPE ===
                var defineSheetVehicleType = package.Workbook.Worksheets.Add("VehicleType");

                defineSheetVehicleType.Row(1).Style.Font.Bold = true;
                defineSheetVehicleType.Cells[1, 1].Value = "Loại xe";
                defineSheetVehicleType.Cells[1, 2].Value = "Mô tả";

                // Fill vehicle type definitions
                defineSheetVehicleType.Cells[2, 1].Value = 0;
                defineSheetVehicleType.Cells[2, 2].Value = DeviceResource.lblCar;
                defineSheetVehicleType.Cells[3, 1].Value = 1;
                defineSheetVehicleType.Cells[3, 2].Value = DeviceResource.lblMotoBike;

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();
                defineSheet.Cells.AutoFitColumns();
                defineSheetVehicleType.Cells.AutoFitColumns();

                result = package.GetAsByteArray();
            }

            return result;
        }

        /// <summary>
        /// Gets the localized description for VehicleType based on current culture
        /// </summary>
        /// <param name="vehicleType">The vehicle type enum value</param>
        /// <returns>Localized description string</returns>
        private string GetLocalizedVehicleTypeDescription(int vehicleType)
        {
            try
            {
                // Get culture from the current HTTP context instead of thread culture
                // This ensures culture flows properly in async operations
                var currentCulture = GetCurrentCultureFromContext();
                var resourceManager = new ResourceManager(typeof(DeviceResource));

                // Convert int to VehicleType enum
                var vehicleTypeEnum = (VehicleType)vehicleType;

                // Get the localized description based on the VehicleType enum
                switch (vehicleTypeEnum)
                {
                    case VehicleType.Car:
                        return resourceManager.GetString(DeviceResource.lblCar, currentCulture) ?? "Car";
                    case VehicleType.MotoBike:
                        return resourceManager.GetString(DeviceResource.lblMotoBike, currentCulture) ?? "MotoBike";
                    default:
                        return vehicleTypeEnum.ToString();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting localized vehicle type description: {ex.Message}");
                return ((VehicleType)vehicleType).ToString();
            }
        }

        /// <summary>
        /// Gets the current culture from HTTP context or falls back to thread culture
        /// </summary>
        /// <returns>Current CultureInfo</returns>
        private CultureInfo GetCurrentCultureFromContext()
        {
            try
            {
                // First try to get culture from HTTP context
                if (_httpContext?.Features?.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>() != null)
                {
                    var requestCultureFeature = _httpContext.Features.Get<Microsoft.AspNetCore.Localization.IRequestCultureFeature>();
                    return requestCultureFeature.RequestCulture.UICulture;
                }

                // Fallback to thread culture
                return Thread.CurrentThread.CurrentUICulture;
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Could not get culture from context: {ex.Message}. Using default culture.");
                return CultureInfo.InvariantCulture;
            }
        }
    }
}