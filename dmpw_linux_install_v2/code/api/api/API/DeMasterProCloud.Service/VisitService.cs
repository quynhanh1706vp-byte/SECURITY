
using AutoMapper;
using Bogus.Extensions;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Exceptions;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.AccessGroup;
using DeMasterProCloud.DataModel.AccessGroupDevice;
using DeMasterProCloud.DataModel.Account;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.DataModel.Role;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.DataModel.Visit;
using DeMasterProCloud.DataModel.VisitArmy;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Service.Protocol;
using DeMasterProCloud.Service.RabbitMqQueue;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;
using CardType = DeMasterProCloud.Common.Infrastructure.CardType;
using Company = DeMasterProCloud.DataAccess.Models.Company;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using iText.Html2pdf;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using AngleSharp.Html.Parser;
using AngleSharp;
using iText.Layout.Font;


namespace DeMasterProCloud.Service
{
    /// <summary>
    /// Visit Interface 
    /// </summary>
    public interface IVisitService
    {
        List<VisitReportModel> VisitReport(string from,
            string to, List<int> doorIds, List<int> DeviceReaderIds, string visitorName, string cardId, string search, List<int> inOutType, List<int> eventType,
            string visiteeSite, List<int> cardStatus, List<int> cardTypes, List<int> visitorTypes, string birthDay, string visitorDepartment, List<int> building, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);
        
        List<VisitListModel> GetPaginated(VisitFilterModel filter, out int totalRecords, out int recordsFiltered);

        Dictionary<string, object> GetInit();
        Visit Add(VisitModel visitModel, string groupId = null, bool isSend = true);
        byte[] GetFileExcelImportVisitTemplate(int companyId);
        VisitTargetRegister GetInitVisitForm(int companyId);
        Visit RegisterVisit(VisitModel visitModel, int companyId, int createdBy, bool isSend = true);
        string CheckLocationWarning(string locationVisitor, string allOfLocation, int companyId);
        void PreRegister(VisitModel visitModel);
        void Update(VisitModel visitModel, bool isSend = true);
        //void ChangeStatus(int statusId, List<int> visitIds, string reason);
        Visit GetById(int id);
        Visit GetByIdWithTimezone(int id);
        List<Visit> GetByIds(List<int> ids);
        void InitData(VisitDataModel visitModel);
        VisitReportViewModel InitReportData();
        byte[] Export(VisitFilterModel filter, out int totalRecords, out int recordsFiltered);

        byte[] ExportVisitReport(string type, string from,
            string to, List<int> doorIds, List<int> DeviceReaderIds, string visitorName, string cardId, string search, List<int> inOutType, List<int> eventType,
            string visiteeSite, List<int> cardStatus, List<int> cardTypes, string birthDay, string visitorDepartment, List<int> building, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered);

        Card GetCardByCardId(string cardId);
        VisitSetting GetVisitSettingCompany();
        VisitSetting GetVisitSettingByCompanyId(int companyId);
        void UpdateVisitSettingCompany(VisitSettingModel model);
        void UpdateApprovalVisitor(int id, short status, bool isGroup = false);

        Card GetCardByVisitor(int id);
        int AssignedCardVisitor(int visitId, CardModel model, bool isSend = true);
        void AssignedQRVisitor(int visitId, string email);

        void DeletedAssignedCardVisitor(int id);
        bool AssignedDoorVisitor(int id, List<int> door);

        bool RejectVisitor(int id, RejectedModel model);

        void FinishVisitor(int id, bool isGroup, int companyId, int accountId);

        IQueryable<VisitListHistoryModel> GetPaginatedVisitHistoryLog(int id, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        bool IsCardIdExist(string cardId, int companyId);

        int LengthVisitNeedToReview();
        int GetRequestApprovalCount(int companyId, int accountId);

        void ReturnCardVisitor(string cardId, string reason);
        void ReturnCardVisitor(int visitId, string cardId, string reason);

        void ReturnVisitor(List<int> visitIds, string reason);

        void Delete(int visitId);
        void DeleteRange(List<int> visitIds);

        int GetLastApproval(int visitId);
        IEnumerable<EventLog> GetHistoryVisitor(int id, int pageNumber, int pageSize, out int totalRecords, out int recordsFiltered);
        int GetTodayRegisteredVisitorsCount(int companyId, DateTime day);

        VisitQRCodeModel CreateQRCodeForVisitor(int visitorId);
        string GetStaticQrCode(Visit visit);
        void UpdateSettingVisibleFieldsByCompanyId(int companyId, Dictionary<string, bool> model);
        VisitCardInfo GetAllInfoByCardVisit(string cardId);

        void AddFirstApproverSetting(List<int> ids, int companyId);
        void AddSecondApproverSetting(List<int> ids, int companyId);
        void AddCheckManagerSetting(List<int> ids, int companyId);

        void DeleteFirstApproverSetting(List<int> ids, int companyId);
        void DeleteSecondApproverSetting(List<int> ids, int companyId);
        void DeleteCheckManagerSetting(List<int> ids, int companyId);

        VisitDataModel GetVisitByQrCodeEncrypt(string qrCode, int companyId);

        List<VisitorCardModel> GetVisitorsByCardIds(List<string> CardIds);

        List<VisitorCardModel> GetVisitorCardByVisitIds(List<int> visitIds);
        bool IsExistedVisitee(int visiteeId);
        void UpdateFieldLayoutRegister(FieldLayoutRegister model, int companyId);
        List<UserListModel> GetUserTarget(UserTargetFilterModel filter, out int recordsFiltered, out int recordsTotal);
        Visit GetByPhoneAndCompanyId(string phone, int companyId);
        string PrepareQrCodeForVisitor(Visit visitor, short cardStatus = (short)CardStatus.InValid);
        List<Visit> GetGroupsByIdWithTimezone(int id);
        void WriteSystemLog(int logObjId, ActionLogType type, string content, string contentDetails);
        string GenerateImageQRCode(string qrCode);
        List<Card> GetCardHFaceIdByVisitIds(List<int> ids);
        Task<ResultImported> ImportFile(string type, MemoryStream stream, int companyId, int accountId, string accountName);
        ResultImported ValidateImportFileHeaders(IFormFile file);
        Dictionary<bool, List<string>> ImportMultiVisitors(List<ImportMultiVisitModel> models, int companyId);
        void SendCardToDeviceRegisterMeeting(Visit visit, int companyId);
        int GetTotalVisitByCompany(int companyId);
        List<CardModel> GetCardListByVisitId(int visitId);
        void DeleteCardByVisit(Visit visit, int cardId);
    }


    /// <summary>
    /// Service provider for user
    /// </summary>
    public class VisitService : IVisitService
    {

        // Inject dependency
        private readonly IUnitOfWork _unitOfWork;

        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly IJwtHandler _jwtHandler;
        private readonly ILogger _logger;
        private readonly IAccessGroupService _accessGroupService;
        private readonly IAccountService _accountService;
        private readonly ISettingService _settingService;
        private readonly IDeviceService _deviceService;
        private readonly INotificationService _notificationService;
        private readonly ICompanyService _companyService;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IWebSocketService _webSocketService;


        public VisitService(IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor, IAccessGroupService accessGroupService, INotificationService notificationService,
            IAccountService accountService, IDeviceService deviceService,
            IConfiguration configuration, ILogger<VisitService> logger, IJwtHandler jwtHandler, ICompanyService companyService,
            IUserService userService, IWebSocketService webSocketService)
        {
            _unitOfWork = unitOfWork;
            _httpContext = contextAccessor.HttpContext;
            _configuration = configuration;
            _jwtHandler = jwtHandler;
            _logger = logger;
            _accessGroupService = accessGroupService;
            _accountService = accountService;
            _deviceService = deviceService;
            _notificationService = notificationService;
            _companyService = companyService;
            _userService = userService;
            _mapper = MapperInstance.Mapper;
            _settingService = new SettingService(_unitOfWork, _configuration);
            _webSocketService = webSocketService;
        }

        public VisitService(IUnitOfWork unitOfWork, IConfiguration configuration, IAccessGroupService accessGroupService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _accessGroupService = accessGroupService;
            _mapper = MapperInstance.Mapper;
        }

        // header of data template
        string[] templateHeaders =
        {
            VisitResource.lblVisitName,
            VisitResource.lblApproval1,
            VisitResource.lblBirthDay,
            "Doors",
            UserResource.lblEmail,
            VisitResource.lblAddress,
            VisitResource.lblStartDate,
            VisitResource.lblEndDate,
            VisitResource.lblPosition,
            VisitResource.lblPhone,
            VisitResource.lblNationalIdNumber,
            VisitResource.lblReason,
            VisitResource.lblVisitType,
            "VisiteeDepartment",
            "VisiteeDepartmentId",
            "VisiteeEmpNumber",
            "VisiteeId",
            "VisiteeName",
            "VisiteeSite",
            "VisitorDepartment",
            "VisitorEmpNumber",
        };

        // header of data define
        string[] templateHeaders2 =
        {
            "ApproverId",
            "ApproverName",
            "DoorId",
            "DoorName",
            "Visit Type",
            "Visitee DepartmentId",
            "Visitee Department Name",
            "VisiteeId",
            "VisiteeName",
        };
        private readonly string[] _header2 =
        {
            //한아름 수정
            VisitResource.lblIDX,//Idx
            VisitResource.lblAccessTime,
            VisitResource.lblUserName,
            VisitResource.lblBirthDay,
            VisitResource.lblDepartment,
            VisitResource.lblCardID,
            VisitResource.lblRID,
            VisitResource.lblDoorName,
            VisitResource.IblSite2,
            VisitResource.lblEventDetail,
            VisitResource.lblIssueCount,
            VisitResource.lblCardStatus,
            VisitResource.lblInOut,
            EventLogResource.lblBodyTemperature,
        };

        public Dictionary<string, object> GetInit()
        {
            try
            {
                Dictionary<string, object> data = new Dictionary<string, object>();

                // visit type
                data.Add("visitType", EnumHelper.ToEnumList<VisitType>());

                // visit status
                data.Add("visitStatus", EnumHelper.ToEnumList<VisitChangeStatusType>());

                // visit setting
                var visitSetting = GetVisitSettingCompany();
                VisitSettingInitModel visitSettingData = new();
                List<AccountListModel> accounts = new();
                // Null check.
                if (visitSetting != null)
                {
                    visitSettingData = _mapper.Map<VisitSettingInitModel>(visitSetting);

                    if (!string.IsNullOrWhiteSpace(visitSetting.FirstApproverAccounts))
                    {
                        // account aprroval
                        List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.FirstApproverAccounts);
                        accounts = _unitOfWork.AppDbContext.Account
                            .Include(m => m.User).ThenInclude(m => m.Department)
                            .Where(m => accountIds.Contains(m.Id) && !m.IsDeleted).Select(m => new AccountListModel
                            {
                                Id = m.Id,
                                Email = m.Username,
                                CompanyName = m.Company.Name
                            }).ToList();
                    }
                }

                data.Add("visitSetting", visitSettingData);
                data.Add("accounts", accounts);

                // all of building tree
                var buildings = new List<AccessGroupDeviceDoor>();
                var user = _userService.GetUserByAccountId(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId());
                if (user != null)
                {
                    buildings = _accessGroupService.GetPaginatedForDoors(user.AccessGroupId, "", null, 1, 99999, "Id", "desc",
                        out _, out _);
                }
                data.Add("buildings", buildings);

                // user visit target
                var filter = new UserFilterModel()
                {
                    SortColumn = "FirstName",
                    SortDirection = "asc",
                };

                var usersVisit = _userService.FilterDataWithOrderToList(filter, out _,out _, out _, "", 0).ToList();
                data.Add("visitTarget", usersVisit);

                var operationTypes = EnumHelper.ToEnumList<OperationType>();
                data.Add("operationTypes", operationTypes);

                var verifyModes = EnumHelper.ToEnumList<VerifyMode>();
                data.Add("verifyModes", verifyModes);

                var identificationType = EnumHelper.ToEnumList<IdentificationType>();
                data.Add("identificationType", identificationType);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetInit");
                return new Dictionary<string, object>();
            }
        }

        public VisitReportViewModel InitReportData()
        {
            try
            {
                var model =
                new VisitReportViewModel
                {
                    EventTypeList = EnumHelper.ToEnumList<EventType>(),
                    InOutList = EnumHelper.ToEnumList<Antipass>(),
                    ListCardStatus = EnumHelper.ToEnumList<CardStatus>(),
                };

                var companyId = _httpContext.User.GetCompanyId();
                var buildings = _unitOfWork.BuildingRepository.GetByCompanyId(companyId);
                var doors = _unitOfWork.IcuDeviceRepository.GetDoors(companyId);
                var departments = _unitOfWork.DepartmentRepository.GetByCompanyId(companyId);

                if (doors != null)
                {
                    model.DoorList = doors.Select(m => new SelectListItemModel
                    {
                        Id = m.Id,
                        Name = m.Name
                    }).ToList();
                }

                model.BuildingList = buildings.Select(m => new SelectListItemModel
                {
                    Id = m.Id,
                    Name = m.Name
                }).ToList();

                model.DepartmentList = departments.Select(m => new SelectListItemModel()
                {
                    Id = m.Id,
                    Name = m.DepartName
                }).ToList();

                model.IdentificationType = EnumHelper.ToEnumList<IdentificationType>();

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitReportData");
                return new VisitReportViewModel();
            }
        }

        /// <summary>
        /// Add a visit
        /// </summary>
        /// <param name="visitModel"></param>
        public Visit Add(VisitModel visitModel, string groupId = null, bool isSend = true)
        {
            Visit visit = null;
            VisitHistory logVisitHistory = null;

            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();
            var visitSetting = GetVisitSettingCompany();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var currentAccount = _unitOfWork.AccountRepository.Get(m =>
                            m.Id == accountId && !m.IsDeleted);
                        if (!string.IsNullOrEmpty(currentAccount.Language))
                        {
                            var currentCulture = new CultureInfo(currentAccount.Language);
                            Thread.CurrentThread.CurrentUICulture = currentCulture;
                        }

                        visit = _mapper.Map<Visit>(visitModel);
                        visit.Id = 0;

                        if (visitModel.VisitorName.Equals(Constants.Settings.PinpadVisitorName) && !string.IsNullOrEmpty(visitModel.Doors))
                        {
                            var doorIds = JsonConvert.DeserializeObject<List<int>>(visitModel.Doors);

                            var deviceAddress = _unitOfWork.IcuDeviceRepository.GetByIds(doorIds).Select(m => m.DeviceAddress);

                            visit.VisitorName += string.Join(",", deviceAddress);

                            var deviceVisitor = _unitOfWork.AppDbContext.Visit.Where(m => m.VisitorName.Equals(visit.VisitorName) && !m.IsDeleted);

                            if (deviceVisitor.Any())
                            {
                                List<int> visitIds = deviceVisitor.Select(x => x.Id).ToList();

                                _unitOfWork.AppDbContext.Card.Where(m => m.VisitId.HasValue && visitIds.Contains(m.VisitId.Value)).Delete();

                                deviceVisitor.Delete();
                            }
                        }

                        visit.ApproverId1 = visitModel.ApproverId1;
                        visit.ApplyDate = DateTime.UtcNow;
                        visit.CompanyId = companyId;
                        visit.IsDeleted = false;
                        visit.CreatedBy = accountId;
                        visit.Status = (short)VisitChangeStatusType.Waiting;

                        // Add group Id information.
                        if (!string.IsNullOrEmpty(groupId))
                        {
                            visit.GroupId = groupId;
                        }

                        if(visit.AccessGroupId == 0)
                        {
                            var defaultAG = _unitOfWork.AccessGroupRepository.GetDefaultAccessGroup(companyId);
                            if (defaultAG != null)
                                visit.AccessGroup = defaultAG;
                        }

                        _unitOfWork.VisitRepository.Add(visit);
                        _unitOfWork.Save();

                        var accessGroup = new AccessGroup
                        {
                            Name = Constants.Settings.NameAccessGroupVisitor + visit.Id,
                            CompanyId = companyId,
                            Type = (short)AccessGroupType.VisitAccess
                        };
                        _unitOfWork.AccessGroupRepository.Add(accessGroup);
                        _unitOfWork.Save();

                        visit.AccessGroupId = accessGroup.Id;
                        _unitOfWork.VisitRepository.Update(visit);
                        _unitOfWork.Save();

                        logVisitHistory = new VisitHistory
                        {
                            VisitorId = visit.Id,
                            CompanyId = companyId,
                            OldStatus = null,
                            NewStatus = visit.Status,
                            UpdatedBy = accountId,
                            CreatedOn = DateTime.UtcNow
                        };
                        _unitOfWork.AppDbContext.VisitHistory.Add(logVisitHistory);
                        _unitOfWork.Save();
                        
                        if (visitModel.NationalIdCard != null)
                        {
                            var nationalIdCard = _mapper.Map<NationalIdCard>(visitModel.NationalIdCard);
                            var cardNationalId = _unitOfWork.AppDbContext.NationalIdCard.FirstOrDefault(x => x.CCCD == nationalIdCard.CCCD);
                            if (cardNationalId == null)
                            {
                                nationalIdCard.Id = 0;
                                nationalIdCard.Avatar = visitModel.Avatar;
                                nationalIdCard.VisitId = visit.Id;
                                _unitOfWork.VisitRepository.AddNationalIdCardForVisit(nationalIdCard);
                                _unitOfWork.Save();
                            }
                            
                            visitModel.CardList ??= new List<CardModel>();
                            var cardVNId = _unitOfWork.CardRepository.GetByCardId(companyId, visitModel.NationalIdNumber);
                            if (cardVNId != null)
                            {
                                Console.WriteLine($"VIND exited in system: {cardVNId.CardId}");
                            }
                            else
                            {
                                visitModel.CardList.Add(new CardModel()
                                {
                                    CardId = visitModel.NationalIdNumber,
                                    IssueCount = 0,
                                    CardType = (short)CardType.VNID,
                                });
                            }
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

            // Assign list doors
            AssignDoors(visit.AccessGroupId, visitModel.Doors, visitSetting.AccessGroupId, companyId);
            // Add card list
            if (visitModel.CardList != null && visitModel.CardList.Any())
            {
                foreach (var item in visitModel.CardList)
                {
                    var card = new Card
                    {
                        CardId = item.CardId,
                        IssueCount = item.IssueCount,
                        CompanyId = visit.CompanyId,
                        VisitId = visit.Id,
                        CardType = item.CardType
                    };
                    _unitOfWork.CardRepository.Add(card);
                    _unitOfWork.Save();
                    
                    if (item.CardType == (short)CardType.VehicleId || item.CardType == (short)CardType.VehicleMotoBikeId)
                    {
                        // create vehicle
                        var vehicle = new Vehicle()
                        {
                            CompanyId = visit.CompanyId,
                            VisitId = visit.Id,
                            VehicleType = item.CardType == (short)CardType.VehicleId
                                ? (short)VehicleType.Car
                                : (short)VehicleType.MotoBike,
                            PlateNumber = item.CardId
                        };
                        _unitOfWork.VehicleRepository.Add(vehicle);
                        _unitOfWork.Save();
                    }
                }
            }
            if (!string.IsNullOrEmpty(visitModel.CardId))
            {
                int cardType = visit.VisitorName.Contains(Constants.Settings.PinpadVisitorName) ? (int)CardType.PassCode : (int)CardType.NFC;

                // Create a new card for visitor
                CardModel newCard = new CardModel
                {
                    CardId = visitModel.CardId,
                    CardType = cardType,
                    CardStatus = (short)CardStatus.Normal,
                };

                AssignedCardVisitor(visit.Id, newCard);
            }
            // send count review
            if (visitSetting.ApprovalStepNumber != (short)VisitSettingType.NoStep && visit.ApproverId1 != 0)
            {
                _accountService.SendCountReviewToFe(visit.ApproverId1, companyId);
            }

            if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.NoStep)
            {
                UpdateApprovalVisitor(visit.Id, (short)VisitChangeStatusType.Approved);
            }

            //Save system log 
            var content =
                VisitResource.msgAddVisit;
            _unitOfWork.SystemLogRepository.Add(companyId, SystemLogType.VisitManagement, ActionLogType.Add, content, null, null, companyId);
            _unitOfWork.Save();


            return visit;
        }

        public void ThreadSendCardToDevice(List<int> cardIds, List<int> visitIds, List<IcuDevice> devices, string sender, string messageType = Constants.Protocol.AddUser)
        {
            if (messageType == Constants.Protocol.AddUser && (devices == null || devices.Count == 0))
            {
                // List devices empty. So can not send card to device
                return;
            }

            new Thread(() =>
            {
                IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                IWebSocketService webSocketService = new WebSocketService();
                var deviceInstructionQueue = new DeviceInstructionQueue(unitOfWork, _configuration, webSocketService);
                var company = unitOfWork.CompanyRepository.GetById(devices.FirstOrDefault()?.CompanyId ?? 0);

                try
                {
                    // device
                    deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                    {
                        DeviceIds = devices.Select(m => m.Id).ToList(),
                        MessageType = messageType,
                        MsgId = Guid.NewGuid().ToString(),
                        Sender = sender,
                        VisitIds = visitIds,
                        // CardIds = cardIds,
                        CardFilterIds = cardIds,
                        CompanyCode = company?.Code,
                    });
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

        /// <summary>
        /// Assign door when create/edit visit
        /// </summary>
        /// <param name="accessGroupId">access group id</param>
        /// <param name="doorAssign">string json doors assign</param>
        /// <param name="settingAccessGroupId">Access group of VisitSetting</param>
        /// <param name="companyId"></param>
        private void AssignDoors(int accessGroupId, string doorAssign, int settingAccessGroupId, int companyId = 0)
        {
            if (string.IsNullOrEmpty(doorAssign) || accessGroupId == 0) return;
            if (companyId == 0 && _httpContext?.User != null)
            {
                companyId = _httpContext.User.GetCompanyId();
            }
            var doorIds = JsonConvert.DeserializeObject<List<int>>(doorAssign);
            var doors = _deviceService.GetDoorList();
            doors = doors.Where(d => doorIds.Contains(d.Id));
            if (doorIds.Count > 0)
            {
                var accessGroupDevice =
                    _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, settingAccessGroupId);

                var newModel = new AccessGroupAssignDoor();
                foreach (var door in doors)
                {
                    var detailModel = new AccessGroupAssignDoorDetail()
                    {
                        DoorId = door.Id,
                        //TzId = door.ActiveTzId ?? tzId,
                        TzId = accessGroupDevice.FirstOrDefault(x => x.IcuId == door.Id)?.TzId
                               ?? accessGroupDevice.FirstOrDefault()?.TzId ?? 0,
                        CompanyId = companyId
                    };

                    newModel.Doors.Add(detailModel);
                }

                _accessGroupService.AssignDoors(accessGroupId, newModel);
            }
        }

        /// <summary>
        /// Assign doors for visitor register
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="doorAssign"></param>
        /// <param name="accessibleDoors"></param>
        private void AssignDoorsVisitor(int accessGroupId, string doorAssign, IQueryable<AccessGroupDevice> accessibleDoors)
        {
            if (string.IsNullOrEmpty(doorAssign) || accessGroupId == 0) return;
            var doorIds = JsonConvert.DeserializeObject<List<int>>(doorAssign);
            if (doorIds.Count > 0)
            {
                var doors = _deviceService.GetByIds(doorIds);
                foreach (var door in doors)
                {
                    var accessGroupDevice = new AccessGroupDevice
                    {
                        AccessGroupId = accessGroupId,
                        IcuId = door.Id,
                        TzId = accessibleDoors.FirstOrDefault(x => x.IcuId == door.Id)?.TzId
                               ?? accessibleDoors.FirstOrDefault()?.TzId ?? 0,
                    };
                    _unitOfWork.AccessGroupDeviceRepository.Add(accessGroupDevice);
                }
            }
        }

        public VisitTargetRegister GetInitVisitForm(int companyId)
        {
            try
            {
                VisitTargetRegister model = new VisitTargetRegister();

                List<string> keys = new List<string>()
                {
                    Constants.Settings.Logo,
                    Constants.Settings.Language
                };
                List<Setting> settings = _unitOfWork.SettingRepository.GetByKeys(keys, companyId);
                if (settings.Count == keys.Count)
                {
                    model.Logo = Helpers.GetStringFromValueSetting(settings.FirstOrDefault(m => m.Key == Constants.Settings.Logo)?.Value);
                    model.Language = Helpers.GetStringFromValueSetting(settings.FirstOrDefault(m => m.Key == Constants.Settings.Language)?.Value);
                }

                // door default
                var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);
                var defaultDoorIds =
                    _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, visitSetting.AccessGroupId)
                        .Select(x => x.IcuId).ToList();
                if (defaultDoorIds != null && defaultDoorIds.Any())
                {
                    var device = _unitOfWork.IcuDeviceRepository.GetByIds(defaultDoorIds);
                    model.RidDefault = device.FirstOrDefault()?.DeviceAddress;
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetInitVisitForm");
                return new VisitTargetRegister();
            }
        }

        /// <summary>
        /// Add visit with company id, created by
        /// </summary>
        /// <param name="visitModel"></param>
        /// <param name="companyId"></param>
        /// <param name="createdBy"></param>
        /// <returns></returns>
        public Visit RegisterVisit(VisitModel visitModel, int companyId, int createdBy, bool isSend = true)
        {
            Visit visit = null;
            VisitHistory logVisitHistory = null;
            var visitSetting = GetVisitSettingByCompanyId(companyId);

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        visit = string.IsNullOrEmpty(visitModel.NationalIdNumber) ? null : _unitOfWork.VisitRepository.GetByNationIdNumber(visitModel.NationalIdNumber, companyId);
                        if (visit == null)
                        {
                            visit = _mapper.Map<Visit>(visitModel);
                            visit.ApproverId1 = visitModel.ApproverId1;
                            visit.ApplyDate = visit.StartDate;
                            visit.CreatedOn = DateTime.UtcNow;
                            visit.CompanyId = companyId;
                            visit.IsDeleted = false;
                            visit.CreatedBy = createdBy;
                            visit.UpdatedBy = createdBy;
                            visit.Status = createdBy == 0 ? (short)VisitChangeStatusType.Approved :(short)VisitChangeStatusType.Waiting;
                            if (visitModel.Id > 0)
                            {
                                visit.AliasId = visitModel.Id.ToString();
                                visit.Id = 0;
                            }

                            // access group default
                            if (visit.AccessGroupId == 0 || _unitOfWork.AccessGroupRepository.GetByIdAndCompanyId(visit.CompanyId, visit.AccessGroupId) == null)
                            {
                                if (visitSetting.AccessGroupId > 0)
                                {
                                    visit.AccessGroupId = visitSetting.AccessGroupId;    
                                }
                                else
                                {
                                    visit.AccessGroupId = _unitOfWork.AccessGroupRepository.GetDefaultAccessGroup(visit.CompanyId).Id;
                                }
                            }
                        
                            _unitOfWork.VisitRepository.Add(visit);
                            _unitOfWork.Save();
                        }
                        else
                        {
                            visit.StartDate = visitModel.StartDate.ConvertDefaultStringToDateTime() ?? DateTime.UtcNow;
                            visit.EndDate = visitModel.EndDate.ConvertDefaultStringToDateTime() ?? DateTime.UtcNow;
                            if (visit.EndDate <= visit.StartDate)
                            {
                                visit.EndDate = visit.StartDate.AddDays(1);
                            }
                            if (visitModel.Id > 0)
                            {
                                visit.AliasId = visitModel.Id.ToString();
                            }
                            visit.ApproverId1 = visitModel.ApproverId1;
                            visit.ApplyDate = visit.StartDate;
                            visit.UpdatedOn = DateTime.UtcNow;
                            visit.CompanyId = companyId;
                            visit.IsDeleted = false;
                            visit.CreatedBy = createdBy;
                            visit.UpdatedBy = createdBy;
                            visit.VisitReason = visitModel.VisitReason;
                            visit.VisitorDepartment = visitModel.VisitorDepartment;
                            visit.Status = createdBy == 0 ? (short)VisitChangeStatusType.Approved :(short)VisitChangeStatusType.Waiting;
                            // if (visitModel.Id > 0)
                            // {
                            //     visit.AliasId = visitModel.Id.ToString();
                            // }
                            _unitOfWork.VisitRepository.Update(visit);
                            _unitOfWork.Save();
                        }

                        // check event-log not link to visitor by alias Id
                        if (visitModel.Id > 0)
                        {
                            var eventLogs = _unitOfWork.AppDbContext.EventLog
                                .Where(m => m.EventTime >= visit.StartDate && m.EventTime < visit.EndDate
                                                 && m.CompanyId == companyId && m.CardId.Contains($"_{Constants.LFaceConfig.PrefixVisitId}{visitModel.Id}"))
                                .ToList();

                            foreach (var eventLog in eventLogs)
                            {
                                eventLog.UserName = visit.VisitorName;
                                eventLog.VisitId = visit.Id;
                                _unitOfWork.Save();
                            }
                        }
                        
                        var accessGroup = new AccessGroup
                        {
                            Name = Constants.Settings.NameAccessGroupVisitor + visit.Id,
                            CompanyId = companyId,
                            Type = (short)AccessGroupType.VisitAccess
                        };
                        _unitOfWork.AccessGroupRepository.Add(accessGroup);
                        _unitOfWork.Save();

                        visit.AccessGroupId = accessGroup.Id;
                        _unitOfWork.VisitRepository.Update(visit);
                        _unitOfWork.Save();

                        logVisitHistory = new VisitHistory
                        {
                            VisitorId = visit.Id,
                            CompanyId = companyId,
                            OldStatus = null,
                            NewStatus = (short)VisitChangeStatusType.Waiting,
                            CreatedOn = DateTime.UtcNow,
                            UpdatedBy = createdBy
                        };

                        _unitOfWork.AppDbContext.VisitHistory.Add(logVisitHistory);
                        _unitOfWork.Save();

                        // Add card list
                        if (!string.IsNullOrEmpty(visitModel.NationalIdNumber))
                        {
                            if (visitModel.CardList == null)
                            {
                                visitModel.CardList = new List<CardModel>();
                            }
                            var cardVNId = _unitOfWork.CardRepository.GetByCardId(companyId, visitModel.NationalIdNumber);
                            if (cardVNId != null)
                            {
                                Console.WriteLine($"VIND exited in system: {cardVNId.CardId}");
                            }
                            else
                            {
                                visitModel.CardList.Add(new CardModel()
                                {
                                    CardId = visitModel.NationalIdNumber,
                                    IssueCount = 0,
                                    CardType = (short)CardType.VNID,
                                });
                            }
                        }
                        if (visitModel.CardList != null && visitModel.CardList.Any())
                        {
                            foreach (var item in visitModel.CardList)
                            {
                                var card = new Card
                                {
                                    CardId = item.CardId,
                                    IssueCount = item.IssueCount,
                                    CompanyId = visit.CompanyId,
                                    VisitId = visit.Id,
                                    CardType = item.CardType
                                };
                                _unitOfWork.CardRepository.Add(card);
                                _unitOfWork.Save();

                                if (item.CardType == (short)CardType.VehicleId || item.CardType == (short)CardType.VehicleMotoBikeId)
                                {
                                    // create vehicle
                                    var vehicle = new Vehicle()
                                    {
                                        CompanyId = visit.CompanyId,
                                        VisitId = visit.Id,
                                        VehicleType = item.CardType == (short)CardType.VehicleId
                                            ? (short)VehicleType.Car
                                            : (short)VehicleType.MotoBike,
                                        PlateNumber = item.CardId
                                    };
                                    _unitOfWork.VehicleRepository.Add(vehicle);
                                    _unitOfWork.Save();
                                }
                            }
                        }

                        if (visitModel.NationalIdCard != null)
                        {
                            var nationalIdCard = _mapper.Map<NationalIdCard>(visitModel.NationalIdCard);
                            var cardNationalId = _unitOfWork.AppDbContext.NationalIdCard.FirstOrDefault(x => x.CCCD == nationalIdCard.CCCD);
                            if (cardNationalId == null)
                            {
                                nationalIdCard.Id = 0;
                                nationalIdCard.VisitId = visit.Id;
                                nationalIdCard.Avatar = visitModel.Avatar;
                                _unitOfWork.VisitRepository.AddNationalIdCardForVisit(nationalIdCard);
                                _unitOfWork.Save();
                            }
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

            var accessibleDoors = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, visitSetting.AccessGroupId);
            string textJsonDoorDefault = null;
            if (accessibleDoors.Any())
            {
                var resultDoorsDefault = accessibleDoors.Select(m => m.IcuId);
                textJsonDoorDefault = JsonConvert.SerializeObject(resultDoorsDefault);
            }

            if (visitSetting.EnableAutoApproval)
            {
                UpdateApprovalVisitor(visit.Id, (short)VisitChangeStatusType.Approved);
                // Assign list doors
                AssignDoors(visit.AccessGroupId, textJsonDoorDefault, visitSetting.AccessGroupId, companyId);
            }
            else
            {
                // Assign doors
                AssignDoorsVisitor(visit.AccessGroupId, textJsonDoorDefault, accessibleDoors);
                // send count review
                if (visit.ApproverId1 != 0)
                {
                    _accountService.SendCountReviewToFe(visit.ApproverId1, companyId);
                }
            }

            return visit;
        }

        public string CheckLocationWarning(string locationVisitor, string allOfLocation, int companyId)
        {
            try
            {
                if (string.IsNullOrEmpty(allOfLocation) || string.IsNullOrEmpty(locationVisitor))
                {
                    return null;
                }

                locationVisitor = locationVisitor.ReplaceSpacesString().ToUpper();
                allOfLocation = allOfLocation.ReplaceSpacesString().ToUpper();

                List<string> allLocations = allOfLocation.Split("\n").ToList();
                string messageVirus = "";
                foreach (string allLocation in allLocations)
                {
                    List<string> locations = allLocation.Split(",").ToList();
                    if (locations.Count >= 2)
                    {
                        int countCheck = 0;
                        foreach (string location in locations)
                        {
                            if (locationVisitor.RemoveDiacritics().Contains(location.RemoveDiacritics().Trim()))
                            {
                                countCheck++;
                            }
                        }

                        if (countCheck >= 2)
                        {
                            messageVirus += $"<br/>- {allLocation}";
                        }
                    }
                }

                if (!string.IsNullOrEmpty(messageVirus))
                {
                    VisitTargetRegister model = GetInitVisitForm(companyId);
                    string message = VisitResource.ResourceManager.GetString("LocationVirus", new CultureInfo(model.Language)) + $"{messageVirus}";
                    return message;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckLocationWarning");
                return null;
            }
        }

        public void PreRegister(VisitModel visitModel)
        {
            Visit visit = null;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        visit = _mapper.Map<Visit>(visitModel);
                        visit.Status = (short)VisitChangeStatusType.PreRegister;
                        visit.ApplyDate = DateTime.Now;
                        visit.CompanyId = Constants.DefaultCompanyId;
                        visit.IsDeleted = false;


                        if (visitModel.CardStatus == (short)VisitingCardStatusType.NotUse)
                        {
                            visit.Status = (short)VisitChangeStatusType.AutoApproved;
                        }
                        else
                        {
                            visit.Status = (short)VisitChangeStatusType.PreRegister;
                        }

                        _unitOfWork.VisitRepository.Add(visit);
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
        public void SendCardToDeviceRegisterMeeting(Visit visit, int companyId)
        {
           
            try
            {
                var devices = _unitOfWork.AccessGroupDeviceRepository
                .GetByAccessGroupId(companyId, visit.AccessGroupId)
                .Select(x => x.Icu).ToList();
                var card = _unitOfWork.CardRepository.GetByVisitId(companyId, visit.Id).Select(x => x.Id).ToList();
                if (devices.Count > 0)
                {
                    ThreadSendCardToDevice(card, new List<int> { visit.Id }, devices, Constants.InvitationLinkSender.MeetingSender);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
                
            
        }
        public int GetTotalVisitByCompany(int companyId)
        {
            try
            {
                return _unitOfWork.VisitRepository.GetVisitInvalidInCompanyId(companyId).Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTotalVisitByCompany");
                return 0;
            }
        }

        public List<CardModel> GetCardListByVisitId(int visitId)
        {
            try
            {
                var cards = _unitOfWork.CardRepository.GetByVisitId(_httpContext.User.GetCompanyId(), visitId).Where(m => m.CardStatus < (short)CardStatus.WaitingForPrinting);
                var cardModelList = new List<CardModel>();
                foreach (var card in cards)
                {
                    var cardModel = _mapper.Map<CardModel>(card);
                    cardModelList.Add(cardModel);
                }
                return cardModelList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCardListByVisitId");
                return new List<CardModel>();
            }
        }

        public void DeleteCardByVisit(Visit visit, int cardIndex)
        {
            try
            {
                var card = _unitOfWork.CardRepository.GetById(cardIndex);

                // send deleted identification to device
                string sender = _httpContext != null ? _httpContext.User.GetUsername() : "SYSTEM";
                SendUpdateVisitsToAllDoors(new List<Visit>() { visit }, sender, false, new List<Card>() { card });

                // update IsDeleted in card
                _unitOfWork.CardRepository.DeleteFromSystem(card);

                //Save system log
                var content = string.Format(UserResource.msgDeleteCard, card.CardId, $"{visit?.VisitorName}");
                _unitOfWork.SystemLogRepository.Add(visit?.Id ?? 0, SystemLogType.VisitManagement, ActionLogType.Update, content, null, null, card.CompanyId);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteCardByVisit");
            }
        }

        /// <summary>
        /// Update visit
        /// </summary>
        /// <param name="visitModel"></param>
        /// <returns></returns>
        public void Update(VisitModel visitModel, bool isSend = true)
        {
            Visit visit = null;
            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var accountTimezone = _unitOfWork.AccountRepository.Get(m => m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted)?.TimeZone;
                        var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

                        visit = _unitOfWork.VisitRepository.GetByVisitId(companyId, visitModel.Id);

                        var oldAccessGroupId = visit.AccessGroupId;

                        _mapper.Map(visitModel, visit);

                        if (visitModel.AccessGroupId == 0)
                        {
                            visit.AccessGroupId = oldAccessGroupId;
                        }

                        visit.ApproverId1 = visitModel.ApproverId1;

                        _unitOfWork.VisitRepository.Update(visit);
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

            var visitSetting = GetVisitSettingCompany();
            // Assign list doors
            if (!string.IsNullOrEmpty(visitModel.Doors))
            {
                // Doors compare with origin door list.
                List<int> doorIds = JsonConvert.DeserializeObject<List<int>>(visitModel.Doors);

                var agds = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(visit.CompanyId, visit.AccessGroupId);
                if (agds != null && agds.Any())
                {
                    List<int> originDoorIds = agds.Select(m => m.IcuId).ToList();

                    List<int> toBeDeletedDoors = originDoorIds.Except(doorIds).ToList();
                    _accessGroupService.UnAssignDoors(visit.AccessGroupId, toBeDeletedDoors);

                    List<int> toBeAddedDoors = doorIds.Except(originDoorIds).ToList();
                    visitModel.Doors = JsonConvert.SerializeObject(toBeAddedDoors);
                }

                AssignDoors(visit.AccessGroupId, visitModel.Doors, visitSetting.AccessGroupId);
            }
            // send cards to device
            if (isSend)
            {
                var devices = _unitOfWork.AccessGroupDeviceRepository
                    .GetByAccessGroupId(companyId, visit.AccessGroupId)
                    .Select(x => x.Icu).ToList();
                var card = _unitOfWork.CardRepository.GetByVisitId(companyId, visit.Id).Select(x => x.Id).ToList();
                if (devices.Count > 0)
                {
                    ThreadSendCardToDevice(card, new List<int> { visit.Id }, devices, _httpContext.User.GetUsername(), Constants.Protocol.AddUser);
                }
            }

             //Save system log 
            var content = VisitResource.msgUpdateVisit;
            _unitOfWork.SystemLogRepository.Add(companyId, SystemLogType.VisitManagement, ActionLogType.Update, content, null, null, companyId);
            _unitOfWork.Save();
        }

        /// <summary>
        /// Get pinpad info by device
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public PinPadModel GetPinFromDevice(int deviceId)
        {
            try
            {
                PinPadModel result = new PinPadModel
                {
                    PinNumber = "",
                    StartDate = DateTime.Now.ToSettingDateString() + " 00:00:00",
                    EndDate = DateTime.Now.ToSettingDateString() + " 23:59:59"
                };

                var device = _unitOfWork.IcuDeviceRepository.GetByIdAndCompanyId(_httpContext.User.GetCompanyId(), deviceId);

                if (device == null) { return null; }

                var visit = _unitOfWork.AppDbContext.Visit.FirstOrDefault(m => m.VisitorName.Equals(Constants.Settings.PinpadVisitorName + device.DeviceAddress) && !m.IsDeleted);

                if (visit == null) { return result; }

                var card = _unitOfWork.AppDbContext.Card.FirstOrDefault(m => m.VisitId == visit.Id && !m.IsDeleted);

                if (card == null) { return result; }

                result.PinNumber = card.CardId;
                result.StartDate = visit.StartDate.ToSettingDateTimeString();
                result.EndDate = visit.EndDate.ToSettingDateTimeString();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPinFromDevice");
                return null;
            }
        }

        public string GetStaticQrCode(Visit visit)
        {
            try
            {
                var cardQrCode = _unitOfWork.CardRepository.GetQrCodeForVisitor(visit.Id);

                if (cardQrCode != null)
                {
                    return GetStaticQrCodeForVisitor(visit, cardQrCode.CardId);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStaticQrCode");
                return null;
            }
        }
        public string GetStaticQrCodeExport(Visit visit, IUnitOfWork unitOfWork)
        {
            try
            {
                var cardQrCode = unitOfWork.CardRepository.GetQrCodeForVisitor(visit.Id);

                if (cardQrCode != null)
                {
                    return GetStaticQrCodeForVisitorExport(visit, cardQrCode.CardId, unitOfWork);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStaticQrCodeExport");
                return null;
            }
        }

        /// <summary>
        /// Update pinpad information
        /// 1. Pin number  
        /// 2. valid date
        /// </summary>
        /// <param name="device"> device data </param>
        /// <param name="model"> PinPad model data ( PinNumber, StartDate, EndDate ) </param>
        public bool UpdatePinPad(IcuDevice device, PinPadModel model)
        {
            try
            {
                var visit = _unitOfWork.AppDbContext.Visit
                            .Where(m => m.VisitorName.Contains(Constants.Settings.PinpadVisitorName + device.DeviceAddress)
                            && m.CompanyId == _httpContext.User.GetCompanyId()
                            && !m.IsDeleted).FirstOrDefault();

                if (visit == null) { return false; }

                var cards = _unitOfWork.AppDbContext.Card.Where(m => m.VisitId == visit.Id && !m.IsDeleted);

                // Delete if there is an old card
                if (cards.Any())
                {
                    foreach (var card in cards)
                    {
                        // Send Delete card message to device
                        SendIdentificationToDevice(visit, card, false);
                    }

                    _unitOfWork.CardRepository.DeleteRange(cards);

                    _unitOfWork.Save();
                }

                // Add a new card and assign a card to visitor
                int cardType = visit.VisitorName.Contains(Constants.Settings.PinpadVisitorName) ? (int)CardType.PassCode : (int)CardType.NFC;

                CardModel newCard = new CardModel()
                {
                    CardId = model.PinNumber,
                    CardType = cardType,
                    CardStatus = (short)CardStatus.Normal,
                };

                AssignedCardVisitor(visit.Id, newCard, true);

                _unitOfWork.Save();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get visit by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Visit GetById(int id)
        {
            try
            {
                var visit = _unitOfWork.VisitRepository.GetByVisitId(_httpContext.User.GetCompanyId(), id);

                return visit;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return null;
            }
        }

        public Visit GetByIdWithTimezone(int id)
        {
            try
            {
                return _unitOfWork.VisitRepository.GetByVisitId(_httpContext.User.GetCompanyId(), id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdWithTimezone");
                return null;
            }
        }

        public List<Visit> GetGroupsByIdWithTimezone(int id)
        {
            try
            {
                List<Visit> result = new List<Visit>();

                var timezone = _unitOfWork.AccountRepository.Get(m => m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted).TimeZone;
                var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;

                var companyId = _httpContext.User.GetCompanyId();

                var visit = _unitOfWork.VisitRepository.GetByVisitId(companyId, id);

                if (visit != null)
                {
                    if (!string.IsNullOrEmpty(visit.GroupId))
                    {
                        result = _unitOfWork.VisitRepository.GetByGroupIdAndStatus(visit.GroupId, visit.Status);
                    }
                    else
                    {
                        result = new List<Visit>() { visit };
                    }

                    foreach (var eachVisitor in result)
                    {
                        eachVisitor.StartDate = Helpers.ConvertToUserTime(eachVisitor.StartDate, offSet);
                        eachVisitor.EndDate = Helpers.ConvertToUserTime(eachVisitor.EndDate, offSet);
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetGroupsByIdWithTimezone");
                return new List<Visit>();
            }
        }

        public Visit GetByCardId(string cardId)
        {
            try
            {
                return _unitOfWork.VisitRepository.GetByCardId(_httpContext.User.GetCompanyId(), cardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCardId");
                return null;
            }
        }

        public Visit GetByCardIdExceptThis(int id, string cardId)
        {
            try
            {
                return _unitOfWork.VisitRepository.GetByCardIdExceptThis(_httpContext.User.GetCompanyId(), id, cardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCardIdExceptThis");
                return null;
            }
        }

        public List<Visit> GetByIds(List<int> ids)
        {
            try
            {
                return _unitOfWork.VisitRepository.GetByVisitIds(_httpContext.User.GetCompanyId(), ids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIds");
                return new List<Visit>();
            }
        }

        /// <inheritdoc />
        public void InitData(VisitDataModel visitModel)
        {
            try
            {
                var visitSetting = GetVisitSettingCompany();
            visitModel.VisitTypes = EnumHelper.ToEnumList<VisitType>();

            visitModel.ListCardStatus = EnumHelper.ToEnumList<VisitingCardStatusType>();
            visitModel.ListIdentificationType = EnumHelper.ToEnumList<IdentificationType>();

            var companyId = _httpContext.User.GetCompanyId();

            // var accessGroupDefault =
            //     _unitOfWork.AccessGroupRepository.GetDefaultAccessGroup(companyId);
            if (visitModel.AccessGroupId == 0)
            {
                visitModel.AccessGroupId = visitSetting.AccessGroupId;
            }

            if (visitModel.Id == 0 && string.IsNullOrEmpty(visitModel.CardId))
            {
                var cardId = _httpContext.Request.Query["cardId"].ToString();
                if (!string.IsNullOrEmpty(cardId))
                {
                    visitModel.CardId = cardId;
                }

                visitModel.VisitType = 1;
            }


            // add field doors assign
            if (visitModel.AccessGroupId != 0)
            {
                List<int> doors = new List<int>();
                var accessGroupDevices = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(_httpContext.User.GetCompanyId(), visitModel.AccessGroupId);
                foreach (var item in accessGroupDevices)
                {
                    doors.Add(item.IcuId);
                }
                visitModel.Doors = JsonConvert.SerializeObject(doors);

                if (doors.Count > 0)
                {
                    var building = _unitOfWork.BuildingRepository.GetByDeviceId(doors[0]);
                    visitModel.BuildingName = building.Name;
                    visitModel.BuildingAddress = building.Address;
                }
            }

            // building default
            var buildingDefault = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(companyId);
            if (string.IsNullOrEmpty(visitModel.BuildingName))
            {
                visitModel.BuildingName = buildingDefault.Name;
                visitModel.BuildingAddress = buildingDefault.Address;
            }

            if (visitModel.StartDate == null || visitModel.EndDate == null)
            {
                DateTime startUtc;
                DateTime endUtc;

                var timezoneId = _httpContext.User.GetTimezone();

                try
                {
                    if (!string.IsNullOrEmpty(timezoneId))
                    {
                        var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);

                        var today = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tzInfo).Date;

                        var startLocal = today.AddHours(9);
                        var endLocal = today.AddHours(18);

                        startUtc = TimeZoneInfo.ConvertTimeToUtc(startLocal, tzInfo);
                        endUtc = TimeZoneInfo.ConvertTimeToUtc(endLocal, tzInfo);
                    }
                    else
                    {
                        throw new Exception("No timezone provided");
                    }
                }
                catch
                {
                    var todayUtc = DateTime.UtcNow.Date;
                    startUtc = todayUtc.AddHours(9);
                    endUtc = todayUtc.AddHours(18);
                }

                visitModel.StartDate = startUtc.ConvertDefaultDateTimeToString();
                visitModel.EndDate = endUtc.ConvertDefaultDateTimeToString();
            }
            if (visitModel.BirthDay == null)
            {
                visitModel.BirthDay = DateTime.Now.ConvertDefaultDateTimeToString(Constants.DateTimeFormat.DdMMYyyy);
            }

            // avatar
            if (string.IsNullOrEmpty(visitModel.Avatar))
            {
                visitModel.Avatar = Constants.Image.DefaultMale;
            }
            // avatar visitee
            if (visitModel.VisiteeId.HasValue)
            {
                var visitee = _unitOfWork.UserRepository.GetByUserId(companyId, visitModel.VisiteeId.Value);
                if (visitee != null)
                {
                    visitModel.VisiteeAvatar = visitee.Avatar ?? Constants.Image.DefaultMale;
                    visitModel.VisiteeName = visitee.FirstName;
                    visitModel.VisiteeDepartment = visitModel.VisiteeDepartment;
                    visitModel.VisiteeSite = visitee.Department?.DepartName;
                    visitModel.VisiteeDepartmentId = visitee.DepartmentId;
                }
            }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitData");
            }
        }
        public void InitDataExport(VisitDataModel visitModel, IUnitOfWork unitOfWork, int companyId)
        {
            try
            {
                var visitSetting = GetVisitSettingCompanyExport(unitOfWork, companyId);
            visitModel.VisitTypes = EnumHelper.ToEnumList<VisitType>();

            visitModel.ListCardStatus = EnumHelper.ToEnumList<VisitingCardStatusType>();


            // var accessGroupDefault =
            //     unitOfWork.AccessGroupRepository.GetDefaultAccessGroup(companyId);
            if (visitModel.AccessGroupId == 0)
            {
                visitModel.AccessGroupId = visitSetting.AccessGroupId;
            }

            if (visitModel.Id == 0 && string.IsNullOrEmpty(visitModel.CardId))
            {
                var cardId = _httpContext.Request.Query["cardId"].ToString();
                if (!string.IsNullOrEmpty(cardId))
                {
                    visitModel.CardId = cardId;
                }

                visitModel.VisitType = 1;
            }


            // add field doors assign
            if (visitModel.AccessGroupId != 0)
            {
                List<int> doors = new List<int>();
                var accessGroupDevices = unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, visitModel.AccessGroupId);
                foreach (var item in accessGroupDevices)
                {
                    doors.Add(item.IcuId);
                }
                visitModel.Doors = JsonConvert.SerializeObject(doors);

                if (doors.Count > 0)
                {
                    var building = unitOfWork.BuildingRepository.GetByDeviceId(doors[0]);
                    visitModel.BuildingName = building.Name;
                    visitModel.BuildingAddress = building.Address;
                }
            }

            // building default
            var buildingDefault = unitOfWork.BuildingRepository.GetDefaultByCompanyId(companyId);
            if (string.IsNullOrEmpty(visitModel.BuildingName))
            {
                visitModel.BuildingName = buildingDefault.Name;
                visitModel.BuildingAddress = buildingDefault.Address;
            }

            if (visitModel.StartDate == null || visitModel.EndDate == null)
            {
                // visit date utc
                visitModel.StartDate = (DateTime.Now.Date + new TimeSpan(9, 0, 0)).ConvertDefaultDateTimeToString();
                visitModel.EndDate = (DateTime.Now.Date + new TimeSpan(18, 0, 0)).ConvertDefaultDateTimeToString();
            }
            if (visitModel.BirthDay == null)
            {
                visitModel.BirthDay = DateTime.Now.ConvertDefaultDateTimeToString(Constants.DateTimeFormat.DdMMYyyy);
            }

            // avatar
            if (string.IsNullOrEmpty(visitModel.Avatar))
            {
                visitModel.Avatar = Constants.Image.DefaultMale;
            }
            // avatar visitee
            if (visitModel.VisiteeId.HasValue)
            {
                var visitee = unitOfWork.UserRepository.GetByUserId(companyId, visitModel.VisiteeId.Value);
                if (visitee != null)
                {
                    visitModel.VisiteeAvatar = visitee.Avatar ?? Constants.Image.DefaultMale;
                    visitModel.VisiteeName = visitee.FirstName;
                    visitModel.VisiteeDepartment = visitModel.VisiteeDepartment;
                    visitModel.VisiteeSite = visitee.Department?.DepartName;
                    visitModel.VisiteeDepartmentId = visitee.DepartmentId;
                }
            }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitDataExport");
            }
        }

        public List<VisitListModel> GetPaginated(VisitFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var data = FilterDataWithOrder(filter, out totalRecords, out recordsFiltered);

            int currentAccountId = _httpContext.User.GetAccountId();
            int currentCompanyId = _httpContext.User.GetCompanyId();
            Account currentAccount = _unitOfWork.AccountRepository.GetById(currentAccountId);
            User currentUser = _unitOfWork.UserRepository.GetByAccountId(currentAccountId, currentCompanyId);

            short currentAccountType = _httpContext.User.GetAccountType();
            if (!filter.IsOnyVisitTarget)
            {
                List<short> typesAllowGetAll = new List<short>()
                {
                    (short)AccountType.PrimaryManager,
                };

                if (typesAllowGetAll.Contains(currentAccountType)) filter.IsOnyVisitTarget = false;
                else filter.IsOnyVisitTarget = true;
            }

            if (filter.IsOnyVisitTarget)
            {
                if (currentAccount != null)
                {
                    data = data.Where(m => m.CreatedBy == currentAccount.Id || m.ApproverId1 == currentAccount.Id ||
                                           (currentUser == null || m.VisiteeId == currentUser.Id));
                }

                recordsFiltered = data.Count();
            }

            data = data.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);
            data = data.Include(m => m.AccessGroup).ThenInclude(m => m.AccessGroupDevice).ThenInclude(m => m.Icu);

            var accountTimezone = currentAccount.TimeZone;
            var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

            List<VisitListModel> result = new List<VisitListModel>();
            foreach (var visit in data)
            {
                visit.IsDecision = false;
                if (visit.Status != (short)VisitChangeStatusType.Approved && visit.Status != (short)VisitChangeStatusType.AutoApproved)
                {
                    var visitSetting = GetVisitSettingCompany();
                    if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.NoStep)
                    {
                        if (visit.Status == (short)VisitChangeStatusType.Waiting)
                        {
                            if (visit.ApproverId1 == currentAccountId)
                            {
                                visit.IsDecision = true;
                            }
                        }
                    }
                    if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.FirstStep)
                    {
                        if (visit.Status == (short)VisitChangeStatusType.Waiting)
                        {
                            if (visit.ApproverId1 == currentAccountId)
                            {
                                visit.IsDecision = true;
                            }
                        }
                    }
                    if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.SecondStep)
                    {
                        if (visit.Status == (short)VisitChangeStatusType.Waiting)
                        {
                            if (visit.ApproverId1 == currentAccountId)
                            {
                                visit.IsDecision = true;
                            }
                        }
                        else if (visit.Status == (short)VisitChangeStatusType.Approved1)
                        {
                            var secondApprovalAccounts = JsonConvert.DeserializeObject<List<int>>(visitSetting.SecondsApproverAccounts);
                            if (secondApprovalAccounts.Contains(currentAccountId))
                            {
                                visit.IsDecision = true;
                            }
                        }
                    }
                }

                var card = visit.Card.Where(m => !m.IsDeleted).Select(m => m.CardId);
                visit.CardId = string.Join(",", card);

                if (!string.IsNullOrEmpty(visit.Avatar))
                {
                    visit.Avatar = visit.Avatar;
                }
                else if (!string.IsNullOrEmpty(visit.VisitorName))
                {
                    visit.Avatar = Constants.Image.DefaultMale;
                }

                string visiteeAvatar = null;
                if (visit.VisiteeId.HasValue)
                {
                    var visitee = _unitOfWork.UserRepository.GetByUserId(currentCompanyId, visit.VisiteeId.Value);
                    if (visitee != null)
                    {
                        visit.VisiteeName = visitee.FirstName;
                        visit.VisiteeDepartment = visit.VisiteeDepartment;
                        visit.VisiteeSite = visitee.Department?.DepartName ?? visit.VisiteeSite;
                        visiteeAvatar = visitee.Avatar ?? Constants.Image.DefaultMale;
                    }
                }

                var visitListModel = _mapper.Map<VisitListModel>(visit);
                visitListModel.VisiteeAvatar = visiteeAvatar;
                result.Add(visitListModel);
            }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginated");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<VisitListModel>();
            }
        }

        internal IQueryable<Visit> FilterDataWithOrder(VisitFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var timezone = _unitOfWork.AccountRepository.Get(m => m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted)?.TimeZone;
            var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;

            var data = _unitOfWork.VisitRepository.GetByCompanyId(companyId);
            data = data.Include(m => m.Card);
            data = data.Where(m => !m.VisitorName.Contains(Constants.Settings.PinpadVisitorName));

            totalRecords = data.Count();
            
            if (!string.IsNullOrEmpty(filter.Search))
            {

                data = data.Where(m => m.VisitorName.RemoveDiacritics().ToLower().Contains(filter.Search.RemoveDiacritics().ToLower()) 
                                       || m.Card.Any(n => n.CardId.RemoveDiacritics().ToLower().Contains(filter.Search.RemoveDiacritics().ToLower())));
            }
            if (!string.IsNullOrEmpty(filter.StartDateFrom))
            {
                var startDateFrom = (filter.StartDateFrom.ConvertDefaultStringToDateTime() ?? DateTime.UtcNow);
                data = data.Where(m => startDateFrom <= m.StartDate);
            }

            if (!string.IsNullOrEmpty(filter.EndDateFrom))
            {
                var endDateFrom = (filter.EndDateFrom.ConvertDefaultStringToDateTime() ?? DateTime.UtcNow);
                data = data.Where(m => m.StartDate <= endDateFrom);
            }

            if (!string.IsNullOrEmpty(filter.AccessDateFrom))
            {
                var opeDateTimeFrom = filter.AccessDateFrom.ConvertDefaultStringToDateTime() ?? DateTime.UtcNow;
                data = data.Where(m => !(m.EndDate < opeDateTimeFrom.ConvertToSystemTime(offSet)));
            }

            if (!string.IsNullOrEmpty(filter.AccessDateTo))
            {
                var opeDateTimeTo = filter.AccessDateTo.ConvertDefaultStringToDateTime() ?? DateTime.UtcNow;
                data = data.Where(m => !(m.StartDate > opeDateTimeTo.ConvertToSystemTime(offSet)));
            }

            if (!string.IsNullOrEmpty(filter.VisitorName))
            {
                data = data.Where(m => m.VisitorName.ToLower().Contains(filter.VisitorName.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.BirthDay))
            {
                DateTime dateBirthDayMin = DateTime.ParseExact(filter.BirthDay, Constants.Settings.DateTimeFormatDefault, CultureInfo.InvariantCulture).Date;
                DateTime dateBirthDayMax = dateBirthDayMin.AddDays(1);
                data = data.Where(m => dateBirthDayMin <= m.BirthDay && m.BirthDay < dateBirthDayMax);
            }

            if (!string.IsNullOrEmpty(filter.VisitorDepartment))
            {
                data = data.Where(m => m.VisitorDepartment.ToLower().Contains(filter.VisitorDepartment.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.Position))
            {
                data = data.Where(m => m.Position.ToLower().Contains(filter.Position.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.VisiteeSite))
            {
                data = data.Where(m => m.VisiteeSite.ToLower().Contains(filter.VisiteeSite.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.VisitReason))
            {
                data = data.Where(m => m.VisitReason.ToLower().Contains(filter.VisitReason.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.VisiteeName))
            {
                data = data.Where(m => m.VisiteeName.ToLower().Contains(filter.VisiteeName.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.Phone))
            {
                data = data.Where(m => m.Phone.Contains(filter.Phone));
            }

            if (filter.ProcessStatus != null && filter.ProcessStatus.Any())
            {
                data = data.Where(m => filter.ProcessStatus.Contains(m.Status));
            }

            if (!string.IsNullOrEmpty(filter.CardId))
            {
                data = data.Where(m => m.Card.Any(n => n.CardId.ToLower().Contains(filter.CardId.ToLower())));
            }

            recordsFiltered = data.Count();

            data = data.OrderBy($"{filter.SortColumn} {filter.SortDirection}");

            return data;
        }

        public List<VisitReportModel> VisitReport(string from,
            string to, List<int> doorIds, List<int> DeviceReaderIds, string visitorName,
            string cardId, string search, List<int> inOutType, List<int> eventType,
            string visiteeSite, List<int> cardStatus, List<int> cardTypes, List<int> visitorTypes, string birthDay, string visitorDepartment, List<int> building, int pageNumber,
            int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var result = FilterDataWithOrderForVisitReport(from, to, doorIds, DeviceReaderIds, visitorName,
                    cardId, search, inOutType, eventType, visiteeSite, cardStatus, cardTypes, visitorTypes, birthDay, visitorDepartment, building, pageNumber, pageSize,
                    sortColumn, sortDirection, out totalRecords, out recordsFiltered);

                var dataList = result.Select(_mapper.Map<VisitReportModel>);

                var idx = (pageNumber - 1) * pageSize + 1;
                foreach (var visitReport in dataList)
                {
                    visitReport.Id = idx++;

                    var buildingName = GetBuildingNameByRid(visitReport.Device);
                    visitReport.Building = buildingName;
                    visitReport.CardStatus = ((CardStatus)GetCardStatus(visitReport.CardId)).GetDescription();


                    visitReport.ApproverName1 = _unitOfWork.AccountRepository.GetById(visitReport.ApproverId1)?.Username ?? "";

                }

                return dataList.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VisitReport");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<VisitReportModel>();
            }
        }

        /// <summary>
        /// Filter data
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="doorIds"></param>
        /// <param name="visitorName"></param>
        /// <param name="cardId"></param>
        /// <param name="inOutType"></param>
        /// <param name="eventType"></param>
        /// <param name="visiteeSite"></param>
        /// <param name="cardStatus"></param>
        /// <param name="cardTypes"></param>
        /// <param name="birthDay"></param>
        /// <param name="visitorDepartment"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public IQueryable<EventLog> FilterDataWithOrderForVisitReport(string from,
            string to, List<int> doorIds, List<int> DeviceReaderIds, string visitorName,
            string cardId, string search, List<int> inOutType, List<int> eventType,
            string visiteeSite, List<int> cardStatus, List<int> cardTypes, List<int> visitorTypes, string birthDay, string visitorDepartment, List<int> building, int pageNumber,
            int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            int accountId = _httpContext.User.GetAccountId();
            int companyId = _httpContext.User.GetCompanyId();
            var data = _unitOfWork.AppDbContext.EventLog
                .Include(m => m.Visit)
                .Include(m => m.Icu).ThenInclude(m => m.DeviceReader)
                .Include(m => m.Icu.Building)
                .Include(m => m.Company)
                .AsQueryable();

            Account currentAccount = _unitOfWork.AccountRepository.GetById(accountId);
            User currentUser = _unitOfWork.UserRepository.GetByAccountId(accountId, companyId);

            short currentAccountType = _httpContext.User.GetAccountType();
            List<short> typesAllowGetAll = new List<short>()
            {
                (short)AccountType.PrimaryManager,
            };

            if (!typesAllowGetAll.Contains(currentAccountType))
            {
                if (currentAccount != null)
                {
                    data = data.Where(m => m.Visit.CreatedBy == currentAccount.Id || m.Visit.ApproverId1 == currentAccount.Id ||
                                           (currentUser == null || m.Visit.VisiteeId == currentUser.Id));
                }

                recordsFiltered = data.Count();
            }
            data = data.Where(m => m.CompanyId == _httpContext.User.GetCompanyId() && m.IsVisit);
            

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(search))
            {
                data = data.Where(m => m.Visit.VisitorName.ToLower().Contains(search.Trim().ToLower()) 
                                       || m.CardId.ToLower().Contains(search.Trim().ToLower()));
            }
            if (!string.IsNullOrEmpty(from))
            {
                var accessDateTimeFrom = from.ConvertDefaultStringToDateTime() ?? DateTime.UtcNow;
                data = data.Where(m => m.EventTime >= accessDateTimeFrom);
            }

            if (!string.IsNullOrEmpty(to))
            {
                var accessDateTimeTo = to.ConvertDefaultStringToDateTime() ?? DateTime.UtcNow;
                data = data.Where(m => m.EventTime <= accessDateTimeTo);
            }

            if (eventType != null && eventType.Any())
            {
                data = data.Where(m => eventType.Contains(m.EventType));
            }

            if (!string.IsNullOrEmpty(visitorName))
            {
                data = data.Where(m => m.Visit.VisitorName.ToLower().Contains(visitorName.ToLower()));
            }

            // [TEMP]: search by string In, Out
            if (inOutType != null && inOutType.Any())
            {
                List<string> inOutTypeDescription = new List<string>();

                foreach (var type in inOutType)
                {
                    // inOutTypeDescription.Add(((Antipass)type).GetDescription());
                    inOutTypeDescription.Add(Enum.GetName(typeof(Antipass), type));
                }

                data = data.Where(m => inOutTypeDescription.Contains(m.Antipass));
            }

            if (!string.IsNullOrEmpty(cardId))
            {
                data = data.Where(m => m.CardId.ToLower().Contains(cardId.ToLower()));
            }

            if (doorIds != null && doorIds.Any())
            {
                data = data.Where(m => doorIds.Contains(m.IcuId));
            }

            if (DeviceReaderIds != null && DeviceReaderIds.Any())
            {
                data = data.Where(m => m.Icu != null && m.Icu.DeviceReader != null && m.Icu.DeviceReader.Any(d => d != null && DeviceReaderIds.Contains(d.Id)));
            }

            if (!string.IsNullOrEmpty(visiteeSite))
            {
                data = data.Where(m => m.Visit.VisiteeSite.ToLower().Contains(visiteeSite.ToLower()));
            }

            if (!string.IsNullOrEmpty(birthDay))
            {
                data = data.Where(m => m.Visit.BirthDay.ToSettingDateString().Contains(birthDay));
            }

            if (!string.IsNullOrEmpty(visitorDepartment))
            {
                int.TryParse(visitorDepartment, out var departmentId);
                var department = _unitOfWork.DepartmentRepository.GetById(departmentId);
                if (department != null)
                {
                    data = data.Where(m => m.Visit.VisiteeSite.ToLower().Contains(department.DepartName.ToLower()));
                }
            }

            if (building != null && building.Any())
            {
                data = data.Where(m => building.Contains(m.Icu.BuildingId ?? 0));
            }
            
            if (cardTypes != null && cardTypes.Any())
            {
                data = data.Where(m => cardTypes.Contains(m.CardType));
            }

            if (visitorTypes != null && visitorTypes.Any())
            {
                List<string> visitorTypesStr = visitorTypes.Select(d => d.ToString()).ToList();
                data = data.Where(m => visitorTypesStr.Contains(m.Visit.VisitType));
            }

            recordsFiltered = data.Count();

            data = data.OrderBy($"{sortColumn} {sortDirection}");
            if (pageSize > 0)
                data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return data;
        }

        public int GetCardStatus(string cardId)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var user = _unitOfWork.UserRepository.GetByCardId(companyId, cardId);

                if (user != null)
                {
                    return _unitOfWork.CardRepository.GetCardStatusByUserIdAndCardId(companyId, user.Id, cardId);
                }
                return (int)(CardStatus.Normal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCardStatus");
                return 0;
            }
        }

        public string GetBuildingNameByRid(string deviceAddress)
        {
            try
            {
                return _unitOfWork.BuildingRepository.GetBuildingNameByRid(_httpContext.User.GetCompanyId(), deviceAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetBuildingNameByRid");
                return null;
            }
        }

        public byte[] ExportTxt(VisitFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            int accountId = _httpContext.User.GetAccountId();
            var companyId = _httpContext.User.GetCompanyId();
            var accountTimezone = _unitOfWork.AccountRepository.Get(m =>
                   m.Id == accountId && !m.IsDeleted).TimeZone;
            var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;
            
            List<string> headers = new List<string>(_header);

            var data = FilterDataWithOrder(filter, out totalRecords, out recordsFiltered);

            Account currentAccount = _unitOfWork.AccountRepository.GetById(accountId);
            User currentUser = _unitOfWork.UserRepository.GetByAccountId(accountId, companyId);

            short currentAccountType = _httpContext.User.GetAccountType();
            if (!filter.IsOnyVisitTarget)
            {
                List<short> typesAllowGetAll = new List<short>()
                {
                    (short)AccountType.PrimaryManager,
                };

                if (typesAllowGetAll.Contains(currentAccountType)) filter.IsOnyVisitTarget = false;
                else filter.IsOnyVisitTarget = true;
            }

            if (filter.IsOnyVisitTarget)
            {
                if (currentAccount != null)
                {
                    data = data.Where(m => m.CreatedBy == currentAccount.Id || m.ApproverId1 == currentAccount.Id ||
                                               (currentUser == null || m.VisiteeId == currentUser.Id));
                }

                recordsFiltered = data.Count();
            }
            
            var visits = data.ToList().Select(x => new object[]
            {
                x.Id,
                x.ApplyDate?.ConvertToUserTime(offSet),
                x.VisitorName,
                x.BirthDay.ToSettingDateString(),
                x.VisitorDepartment,
                x.Position,
                x.StartDate.ConvertToUserTime(offSet).ConvertDefaultDateTimeToString(),
                x.EndDate.ConvertToUserTime(offSet).ConvertDefaultDateTimeToString(),
                x.VisiteeSite,
                x.VisitReason,
                x.VisiteeName,
                x.Phone,
                x.Address,
                x.NationalIdNumber,
                x.VisitingCardState,
                _unitOfWork.AccountRepository.GetByIdAndCompanyId(companyId,x.ApproverId1)?.Username,
                x.RejectReason,
                string.Join(" || ", x.Card.Where(m => !m.IsDeleted).Select(m => m.CardId))

            }).ToList();
            // Build the file content 
            var visitTxt = new StringBuilder();
            visits.ForEach(line =>
            {
                List<object> newObject = new List<object>();
                newObject.AddRange(line);
                visitTxt.AppendLine(string.Join(",", newObject));
            });

            byte[] buffer = Encoding.UTF8.GetBytes($"{string.Join(",", headers)}\r\n{visitTxt}");
            buffer = Encoding.UTF8.GetPreamble().Concat(buffer).ToArray();

            return buffer;
        }
        public byte[] Export(VisitFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            var fileByte = filter.ExportType == Constants.Excel
                ? ExportExcel(filter, out totalRecords, out recordsFiltered)
                : ExportTxt(filter, out totalRecords, out recordsFiltered);

            //Save system log 
            var companyId = _httpContext.User.GetCompanyId();
            var content =
                VisitResource.msgExportVisitList + ":" + DateTime.Now.ToSettingDateTimeString() + "(" +
                VisitResource.msgLogin + ":" + _httpContext.User.GetUsername() + ")";
            _unitOfWork.SystemLogRepository.Add(companyId, SystemLogType.Department, ActionLogType.Export, content, null, null, companyId);
            _unitOfWork.Save();

            return fileByte;
        }
        private readonly string[] _header =
        {

            VisitResource.lblID,
            VisitResource.lblApplyDate,
            VisitResource.lblVisitName,
            VisitResource.lblBirthDay,
            VisitResource.lblDepartment,
            VisitResource.lblPosition,
            VisitResource.lblStartDate,
            VisitResource.lblEndDate,
            VisitResource.lblSite,
            VisitResource.lblReason,
            VisitResource.lblVisitTarget,
            VisitResource.lblPhone,
            VisitResource.lblAddress,
            VisitResource.lblNationalIdNumber,
            VisitResource.lblVisitStatus,
            VisitResource.lblApproval1,
            VisitResource.lblRejectReason,
            VisitResource.lblRejectCardID,

        };

        public byte[] ExportExcel(VisitFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            var accountId = _httpContext.User.GetAccountId();
            var companyId = _httpContext.User.GetCompanyId();
            
            var accountTimezone = _unitOfWork.AccountRepository.Get(m =>
                  m.Id == accountId && !m.IsDeleted).TimeZone;
            var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;
            
            byte[] result;
            using (var package = new ExcelPackage())
            {
                // add a new worksheet to the empty workbook
                var worksheet = package.Workbook.Worksheets.Add(DepartmentResource.lblDepartment); //Worksheet name
                var data = FilterDataWithOrder(filter, out totalRecords, out recordsFiltered);
                Account currentAccount = _unitOfWork.AccountRepository.GetById(accountId);
                User currentUser = _unitOfWork.UserRepository.GetByAccountId(accountId, companyId);

                short currentAccountType = _httpContext.User.GetAccountType();
                if (!filter.IsOnyVisitTarget)
                {
                    List<short> typesAllowGetAll = new List<short>()
                    {
                        (short)AccountType.PrimaryManager,
                    };

                    if (typesAllowGetAll.Contains(currentAccountType)) filter.IsOnyVisitTarget = false;
                    else filter.IsOnyVisitTarget = true;
                }

                if (filter.IsOnyVisitTarget)
                {
                    if (currentAccount != null)
                    {
                        data = data.Where(m => m.CreatedBy == currentAccount.Id || m.ApproverId1 == currentAccount.Id ||
                                               (currentUser == null || m.VisiteeId == currentUser.Id));
                    }

                    recordsFiltered = data.Count();
                }

                var visits = data.ToList();

                //First add the headers for user sheet 
                for (var i = 0; i < _header.Length; i++)
                {
                    //worksheet.Cells[1, i + 1].Value = _header[i];
                    worksheet.Cells[1, i + 1].Value = _header[i];
                }

                var recordIndex = 2;

                foreach (var visit in visits)
                {
                    var approver1 =
                        _unitOfWork.AccountRepository.GetByIdAndCompanyId(companyId, visit.ApproverId1);

                    //For the Department sheet
                    var colIndex = 1;
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.Id;
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.ApplyDate?.ConvertToUserTime(offSet).ToSettingDateTimeString();
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.VisitorName;
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.BirthDay.ToSettingDateString();
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.VisitorDepartment;
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.Position;
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.StartDate.ConvertToUserTime(offSet).ToSettingDateTimeString();
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.EndDate.ConvertToUserTime(offSet).ToSettingDateTimeString();
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.VisiteeSite;
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.VisitReason;
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.VisiteeName;
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.Phone;
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.Address;
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.NationalIdNumber;
                    worksheet.Cells[recordIndex, colIndex++].Value = ((VisitChangeStatusType)visit.Status).GetDescription();
                    worksheet.Cells[recordIndex, colIndex++].Value = approver1?.Username;
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.RejectReason;
                    worksheet.Cells[recordIndex, colIndex++].Value = string.Join(",", visit.Card.Where(m => !m.IsDeleted).Select(m => m.CardId));
                    recordIndex++;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }

        public byte[] ExportVisitReport(string type, string from,
            string to, List<int> doorIds, List<int> DeviceReaderIds, string visitorName, string cardId, string search, List<int> inOutType, List<int> eventType,
            string visiteeSite, List<int> cardStatus, List<int> cardTypes, string birthDay, string visitorDepartment, List<int> building, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered)
        {
            var fileByte = type == Constants.Excel
                ? ExportVisitReportExcel(from, to, doorIds, DeviceReaderIds,
                    visitorName, cardId, search, inOutType, eventType, visiteeSite,
                    cardStatus, cardTypes, birthDay, visitorDepartment, building, sortColumn, sortDirection, out totalRecords,
                    out recordsFiltered)
                : ExportVisitReportTxt(from, to, doorIds, DeviceReaderIds,
                    visitorName, cardId, search, inOutType, eventType, visiteeSite,
                    cardStatus, cardTypes, birthDay, visitorDepartment, building, sortColumn, sortDirection, out totalRecords,
                    out recordsFiltered);

            return fileByte;
        }

        public byte[] ExportVisitReportExcel(string accessDateFrom,
           string accessDateTo, List<int> doorIds, List<int> DeviceReaderIds, string visitorName, string cardId, string search, List<int> inOutType, List<int> eventType,
           string visiteeSite, List<int> cardStatus, List<int> cardTypes, string birthDay, string visitorDepartment, List<int> building, string sortColumn, string sortDirection, out int totalRecords,
           out int recordsFiltered)
        {
            var accountId = _httpContext.User.GetAccountId();
            var accountTimezone = _unitOfWork.AccountRepository.Get(m =>
                  m.Id == accountId && !m.IsDeleted).TimeZone;
            var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

            byte[] result;
            using (var package = new ExcelPackage())
            {
                // add a new worksheet to the empty workbook
                var worksheet =
                    package.Workbook.Worksheets.Add(DepartmentResource.lblDepartment); //Worksheet name

                var visits = FilterDataWithOrderForVisitReport(accessDateFrom,
                        accessDateTo, doorIds, DeviceReaderIds,
                        visitorName, cardId, search, inOutType, eventType, visiteeSite,
                        cardStatus, cardTypes, null, birthDay, visitorDepartment, building, 0, 0,
                        sortColumn, sortDirection, out totalRecords, out recordsFiltered).ToList();

                //First add the headers for user sheet 
                for (var i = 0; i < _header2.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = _header2[i];
                }
                var recordIndex = 2;

                foreach (var visit in visits)
                {
                    //For the Department sheet
                    var colIndex = 1;

                    worksheet.Cells[recordIndex, colIndex++].Value = recordIndex - 1;//idx
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.EventTime.ConvertToUserTime(offSet).ToSettingDateTimeString();//출입시간
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.Visit?.VisitorName;//이름
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.Visit?.BirthDay.ToSettingDateString();//생년월일
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.Visit?.VisitorDepartment;//소속
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.CardId;//카드ID
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.Icu.DeviceAddress;//RID
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.Icu.Name;//출입문 이름 
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.Visit?.VisiteeSite;//장소
                    worksheet.Cells[recordIndex, colIndex++].Value = ((EventType)visit.EventType).GetDescription();//이벤트 종류
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.IssueCount;//발급차수
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.Visit != null ? ((CardStatus)visit.Visit?.CardStatus).GetDescription() : "";//카드상태 
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.Antipass;//입/출
                    worksheet.Cells[recordIndex, colIndex++].Value = visit.BodyTemperature;

                    recordIndex++;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }

        public byte[] ExportVisitReportTxt(string accessDateFrom,
            string accessDateTo, List<int> doorIds, List<int> DeviceReaderIds, string visitorName, string cardId, string search, List<int> inOutType, List<int> eventType,
            string visiteeSite, List<int> cardStatus, List<int> cardTypes, string birthDay, string visitorDepartment, List<int> building, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered)
        {
            int index = 1;
            var accountId = _httpContext.User.GetAccountId();
            
            var accountTimezone = _unitOfWork.AccountRepository.Get(m =>
                  m.Id == accountId && !m.IsDeleted).TimeZone;
            var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

            var visit = FilterDataWithOrderForVisitReport(accessDateFrom, accessDateTo, doorIds, DeviceReaderIds, visitorName,
                cardId, search, inOutType, eventType, visiteeSite, cardStatus, cardTypes, null, birthDay,
                visitorDepartment, building, 0, 0,
                sortColumn, sortDirection, out totalRecords, out recordsFiltered)
                .ToList().Select(x => new object[]
                {
                    index++,

                    x.EventTime.ConvertToUserTime(offSet).ToSettingDateTimeString(),//출입시간 
                    x.Visit?.VisitorName,//이름 
                    x.Visit?.BirthDay.ToSettingDateString(),//생년월일 
                    x.Visit?.VisitorDepartment,//소속 
                    $"=\"{x.CardId}\"",//카드ID 
                    $"=\"{x.Icu?.DeviceAddress}\"",
                    x.Icu?.Name,//출입문 이름  
                    x.Visit?.VisiteeSite,//장소 
                    ((EventType)x.EventType).GetDescription(),//이벤트 종류 
                    x.IssueCount,//발급차수 
                    x.Visit != null ? ((CardStatus)x.Visit?.CardStatus).GetDescription() : "",
                    x.Antipass,//입/출 
                    x.BodyTemperature
                }).ToList();;
            // Build the file content 
            var visitTxt = new StringBuilder();
            visit.ForEach(line =>
            {
                visitTxt.AppendLine(string.Join(",", line));
            });

            byte[] buffer = Encoding.UTF8.GetBytes($"{string.Join(",", _header2)}\r\n{visitTxt}");
            buffer = Encoding.UTF8.GetPreamble().Concat(buffer).ToArray();

            return buffer;
        }

        public Card GetCardByCardId(string cardId)
        {
            return _unitOfWork.CardRepository.GetByCardId(_httpContext.User.GetCompanyId(), cardId);
        }

        public VisitSetting GetVisitSettingCompany()
        {
            var companyId = _httpContext.User.GetCompanyId();

            var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);

            List<int> defaultDoors = new List<int>();

            var accessGroupDevice = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, visitSetting.AccessGroupId);
            if (accessGroupDevice != null)
            {
                defaultDoors = accessGroupDevice.Select(m => m.IcuId).Distinct().ToList();
            }

            visitSetting.DefaultDoors = JsonConvert.SerializeObject(defaultDoors);
            visitSetting.ListFieldsEnable = JsonConvert.SerializeObject(Helpers.GetSettingVisibleFields(visitSetting.ListFieldsEnable, typeof(VisitModel), Constants.Settings.VisitListFieldsIgnored, _logger));
            visitSetting.VisibleFields = JsonConvert.SerializeObject(Helpers.GetSettingVisibleFields(visitSetting.VisibleFields, typeof(VisitSettingModel)));
            visitSetting.VisitCheckManagerAccounts = visitSetting.VisitCheckManagerAccounts ?? "[]";

            return visitSetting;
        }

        public VisitSetting GetVisitSettingCompanyExport(IUnitOfWork unitOfWork, int companyId)
        {

            var visitSetting = unitOfWork.VisitRepository.GetVisitSetting(companyId);

            List<int> defaultDoors = new List<int>();

            var accessGroupDevice = unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, visitSetting.AccessGroupId);
            if (accessGroupDevice != null)
            {
                defaultDoors = accessGroupDevice.Select(m => m.IcuId).Distinct().ToList();
            }

            visitSetting.DefaultDoors = JsonConvert.SerializeObject(defaultDoors);
            visitSetting.ListFieldsEnable = JsonConvert.SerializeObject(Helpers.GetSettingVisibleFields(visitSetting.ListFieldsEnable, typeof(VisitModel), Constants.Settings.VisitListFieldsIgnored, _logger));
            visitSetting.VisibleFields = JsonConvert.SerializeObject(Helpers.GetSettingVisibleFields(visitSetting.VisibleFields, typeof(VisitSettingModel)));
            visitSetting.VisitCheckManagerAccounts = visitSetting.VisitCheckManagerAccounts ?? "[]";

            return visitSetting;
        }

        /// <summary>
        /// Get Visit Setting by company id
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public VisitSetting GetVisitSettingByCompanyId(int companyId)
        {
            var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);
            visitSetting.ListFieldsEnable = JsonConvert.SerializeObject(Helpers.GetSettingVisibleFields(visitSetting.ListFieldsEnable, typeof(VisitModel), Constants.Settings.VisitListFieldsIgnored, _logger));
            visitSetting.VisibleFields = JsonConvert.SerializeObject(Helpers.GetSettingVisibleFields(visitSetting.VisibleFields, typeof(VisitSettingModel)));
            return visitSetting;
        }

        public void UpdateVisitSettingCompany(VisitSettingModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var companyId = _httpContext.User.GetCompanyId();
                        var setting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);
                        setting.CompanyId = companyId;
                        setting.ApprovalStepNumber = model.ApprovalStepNumber;
                        setting.AccessGroupId = model.AccessGroupId;
                        setting.GroupDevices = Helpers.JsonConvertCamelCase(model.GroupDevices);
                        setting.OutSide = model.OutSide;
                        setting.EnableAutoApproval = model.EnableAutoApproval;
                        setting.EnableCaptCha = model.EnableCaptCha;
                        setting.AllowDeleteRecord = model.AllowDeleteRecord;
                        setting.AllowEditRecord = model.AllowEditRecord;
                        setting.AllLocationWarning = model.AllLocationWarning;
                        setting.DeviceIdCheckIn = model.DeviceIdCheckIn;
                        setting.AllowGetUserTarget = model.AllowGetUserTarget;
                        setting.ListFieldsEnable = JsonConvert.SerializeObject(model.ListFieldsEnable);
                        setting.FieldRequired = JsonConvert.SerializeObject(model.FieldRequired ?? new List<string>());
                        setting.PersonalInvitationLink = model.PersonalInvitationLink;
                        setting.AllowSelectDoorWhenCreateNew = model.AllowSelectDoorWhenCreateNew;
                        setting.InsiderAutoApproved = model.InsiderAutoApproved;
                        setting.AllowSendKakao = model.AllowSendKakao;
                        setting.ListVisitPurpose = model.ListVisitPurpose;
                        setting.OnlyAccessSingleBuilding = model.OnlyAccessSingleBuilding;

                        // update permission employee invite visitor
                        setting.AllowEmployeeInvite = model.AllowEmployeeInvite;
                        var roleEmployees = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)AccountType.Employee, companyId);
                        if (roleEmployees.Any())
                        {
                            var roleEmployee = roleEmployees.FirstOrDefault();
                            var permissionEmployee = JsonConvert.DeserializeObject<List<PermissionGroupModel>>(roleEmployee.PermissionList)
                                .ToList();

                            // set all permission visit enable = model.AllowEmployeeInvite
                            foreach (var permissionGroupModel in permissionEmployee)
                            {
                                if (permissionGroupModel.Title.ToLower().Contains("visit"))
                                {
                                    foreach (var permission in permissionGroupModel.Permissions)
                                    {
                                        permission.IsEnabled = model.AllowEmployeeInvite;
                                    }
                                }
                            }

                            roleEmployee.PermissionList = JsonConvert.SerializeObject(permissionEmployee);
                            _unitOfWork.RoleRepository.Update(roleEmployee);
                        }

                        _unitOfWork.VisitRepository.UpdateVisitSetting(setting);
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
        /// Update visitor's status.
        /// </summary>
        /// <param name="id"> visitor identifier </param>
        /// <param name="status"> status to update </param>
        /// <param name="isGroup"></param>
        public void UpdateApprovalVisitor(int id, short status, bool isGroup = false)
        {
            List<Visit> visitorHanet = new List<Visit>();
            // List<int> secondApproverIds = new List<int>();
            var visit = _unitOfWork.VisitRepository.GetById(id);
            var visitSetting = GetVisitSettingByCompanyId(visit.CompanyId);
            var accountId = _httpContext.User.GetAccountId();
            var companyId = _httpContext.User.GetCompanyId();
            var start = DateTime.Now;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var originVisitor = _unitOfWork.VisitRepository.GetById(id);

                        var visitors = _unitOfWork.VisitRepository.GetByGroupId(originVisitor.GroupId);

                        if (!isGroup)
                        {
                            visitors = null;
                        }

                        if (visitors == null || !visitors.Any())
                        {
                            visitors = new List<Visit>() { originVisitor };
                        }
                        else
                        {
                            visitors = visitors.Where(m => m.Status == originVisitor.Status).ToList();
                        }

                        foreach (var visitor in visitors)
                        {
                            ApprovalVisit(visitor, visit, status, visitSetting, accountId, visitorHanet);
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
            Console.WriteLine("[Approval Visit: ]       {0}", DateTime.Now.Subtract(start).TotalMilliseconds);
            var start1 = DateTime.Now;

            // send count review
            _accountService.SendCountReviewToFe(accountId, companyId);

            // Pushing to visitee
            ThreadPushNotifyToVisitee(visit);

            // send cards to device
            var devices = _unitOfWork.AccessGroupDeviceRepository
                .GetByAccessGroupId(_httpContext.User.GetCompanyId(), visit.AccessGroupId)
                .Select(x => x.Icu).ToList();
            var card = _unitOfWork.CardRepository.GetByVisitId(companyId, visit.Id).Select(x => x.Id).ToList();
            if (devices.Count > 0)
            {
                ThreadSendCardToDevice(card, new List<int> { visit.Id }, devices, _httpContext.User.GetUsername());
            }
            Console.WriteLine("[Approval Visit, Device: ]       {0}", DateTime.Now.Subtract(start1).TotalMilliseconds);
        }

        public void ApprovalVisit(Visit visitor, Visit visit, short status, VisitSetting visitSetting, int accountId, List<Visit> visitorHanet)
        {
            visitor.ApprovDate1 = DateTime.Now;
            var oldStatus = visitor.Status;
            visitor.Status = status;
            int accountCreated = visitor.CreatedBy;
            if (_httpContext?.User != null)
            {
                accountCreated = accountId;
            }
            visitor.UpdatedBy = accountCreated;
            _unitOfWork.VisitRepository.Update(visitor);

            var log = new VisitHistory
            {
                VisitorId = visitor.Id,
                CompanyId = visitor.CompanyId,
                OldStatus = oldStatus,
                NewStatus = visitor.Status,
                UpdatedBy = accountCreated,
                CreatedOn = DateTime.UtcNow
            };

            // ONLY FOR AutoApproved
            // This status is only used in army.
            if (status == (short)VisitChangeStatusType.AutoApproved)
            {
                string approverName = "";

                int apprpoverId = visitor.ApproverId1;
                Account approver = _unitOfWork.AccountRepository.GetById(apprpoverId);
                if (approver != null)
                {
                    approverName = _unitOfWork.UserRepository.GetNameByUsernameAndCompanyId(approver.Username, visitor.CompanyId);
                }

                log.Reason = $"{VisitResource.lblApproval1} : {approverName}";
            }

            _unitOfWork.AppDbContext.VisitHistory.Add(log);
            _unitOfWork.Save();

            var building = _unitOfWork.BuildingRepository.GetByCompanyId(visitor.CompanyId).FirstOrDefault();
            var checkPlugInCompany = _unitOfWork.AppDbContext.PlugIn.Where(x => x.CompanyId == visitor.CompanyId).Select(x => x.PlugIns).FirstOrDefault();
            Console.WriteLine("[Update Visit: ]       {0}", DateTime.Now.Subtract(visitor.ApprovDate1).TotalMilliseconds);
            var starts = DateTime.Now;
            // Check QR Plug-in
            if (JsonConvert.DeserializeObject<PlugIns>(checkPlugInCompany).QrCode
                && (status == (short)VisitChangeStatusType.Approved || status == (short)VisitChangeStatusType.AutoApproved))
            {
                Card qrCode = _unitOfWork.CardRepository.GetQrCodeForVisitor(visitor.Id);

                if (qrCode != null)
                {
                    qrCode.CardStatus = (short)CardStatus.Normal;
                    qrCode.ValidFrom = visit.StartDate;
                    qrCode.ValidTo = visit.EndDate;

                    _unitOfWork.CardRepository.Update(qrCode);
                    _unitOfWork.Save();
                }
                else
                {
                    // Make QR
                    string qrId = GenQrId();

                    // Assign QR code to visitor
                    qrCode = new Card
                    {
                        CardId = qrId,
                        IssueCount = 0,
                        CompanyId = visitor.CompanyId,
                        VisitId = visit.Id,
                        CardType = (short)CardType.QrCode,
                        ValidFrom = visit.StartDate,
                        ValidTo = visit.EndDate
                    };

                    _unitOfWork.CardRepository.Add(qrCode);
                    _unitOfWork.Save();
                }
            }

            // Camera Plugin. Visitor Hanet
            if (status == (short)VisitChangeStatusType.Approved || status == (short)VisitChangeStatusType.AutoApproved)
            {
                visitorHanet.Add(visitor);
            }
            Console.WriteLine("[Plugin QR: ]       {0}", DateTime.Now.Subtract(starts).TotalMilliseconds);
        }

        public Card GetCardByVisitor(int id)
        {
            return _unitOfWork.AppDbContext.Card.FirstOrDefault(c => !c.IsDeleted && c.VisitId == id);
        }

        /// <summary>
        /// Assign card to visitor
        /// </summary>
        /// <param name="visitId"> identifier of visitor </param>
        /// <param name="model"> CardModel </param>
        /// <returns></returns>
        public int AssignedCardVisitor(int visitId, CardModel model, bool isSend = true)
        {
            int cardId = 0;
            var visitor = GetById(visitId);

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // Add card
                        var card = new Card
                        {
                            CardId = model.CardId,
                            IssueCount = model.IssueCount,
                            CompanyId = visitor.CompanyId,
                            VisitId = visitId,
                            CardType = model.CardType
                        };
                        _unitOfWork.CardRepository.Add(card);
                        _unitOfWork.Save();

                        var oldStatus = visitor.Status;
                        visitor.Status = (short)VisitChangeStatusType.CardIssued;
                        visitor.CardId = model.CardId;

                        var log = new VisitHistory
                        {
                            VisitorId = visitor.Id,
                            CompanyId = visitor.CompanyId,
                            OldStatus = oldStatus,
                            NewStatus = visitor.Status,
                            UpdatedBy = _httpContext.User.GetAccountId(),
                            CreatedOn = DateTime.UtcNow,
                            Reason = $"{VisitResource.lblCardID} : {model.CardId}"
                        };
                        _unitOfWork.AppDbContext.VisitHistory.Add(log);

                        _unitOfWork.VisitRepository.Update(visitor);
                        _unitOfWork.Save();

                        transaction.Commit();
                        cardId = card.Id;

                        // send to device.
                        if (isSend)
                        {
                            SendIdentificationToDevice(visitor, card, true);
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });

            return cardId;
        }

        public string GetStaticQrCodeForVisitor(Visit visitor, string qrId)
        {
            var settingQrCode = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.UseStaticQrCode, visitor.CompanyId);
            bool.TryParse(Helpers.GetStringFromValueSetting(settingQrCode.Value), out bool useStaticQrCode);
            if (useStaticQrCode)
            {
                return qrId;
            }

            var company = _unitOfWork.CompanyRepository.GetById(visitor.CompanyId);

            var timeStart = visitor.StartDate.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
            var secondStart = timeStart.Substring(timeStart.Length - Constants.DynamicQr.SubStringSeconds);
            // Create plain text
            var plaintText = qrId + "_" + visitor.EndDate.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss) + "_" + secondStart;

            int len = plaintText.Length;
            var salt = Helpers.GenerateSalt(len);

            var data = plaintText + "_" + salt;

            var key = company.SecretCode;
            var text = EncryptStringToBytes(data, Encoding.UTF8.GetBytes(key),
                Encoding.UTF8.GetBytes(Helpers.ReverseString(Constants.DynamicQr.Key)));

            return Constants.DynamicQr.NameProject + "_" + text;
        }
        public string GetStaticQrCodeForVisitorExport(Visit visitor, string qrId, IUnitOfWork unitOfWork)
        {
            var settingQrCode = unitOfWork.SettingRepository.GetByKey(Constants.Settings.UseStaticQrCode, visitor.CompanyId);
            bool.TryParse(Helpers.GetStringFromValueSetting(settingQrCode.Value), out bool useStaticQrCode);
            if (useStaticQrCode)
            {
                return qrId;
            }

            var company = unitOfWork.CompanyRepository.GetById(visitor.CompanyId);

            var timeStart = visitor.StartDate.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
            var secondStart = timeStart.Substring(timeStart.Length - Constants.DynamicQr.SubStringSeconds);
            // Create plain text
            var plaintText = qrId + "_" + visitor.EndDate.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss) + "_" + secondStart;

            int len = plaintText.Length;
            var salt = Helpers.GenerateSalt(len);

            var data = plaintText + "_" + salt;

            var key = company.SecretCode;
            var text = EncryptStringToBytes(data, Encoding.UTF8.GetBytes(key),
                Encoding.UTF8.GetBytes(Helpers.ReverseString(Constants.DynamicQr.Key)));

            return Constants.DynamicQr.NameProject + "_" + text;
        }

        /// <summary>
        /// Assign QR to visitor
        /// </summary>
        /// <param name="visitId"> identifier of visitor </param>
        /// <param name="email"> email address </param>
        /// <returns></returns>
        public void AssignedQRVisitor(int visitId, string email)
        {
            var visitor = GetById(visitId);
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(email) && _accountService.IsEmailValid(email))
                        {
                            visitor.Email = email;
                            _unitOfWork.VisitRepository.Update(visitor);
                            _unitOfWork.Save();

                            // Send QR to visitor through email.
                            string qrId = GenQrId();

                            // Add QR
                            var qrCode = new Card
                            {
                                CardId = qrId,
                                IssueCount = 0,
                                CompanyId = visitor.CompanyId,
                                VisitId = visitId,
                                CardType = (short)CardType.QrCode
                            };
                            _unitOfWork.CardRepository.Add(qrCode);
                            _unitOfWork.Save();
                        }

                        transaction.Commit();

                        var agDevices =
                            _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(visitor.CompanyId,
                                visitor.AccessGroupId);

                        if (agDevices != null && agDevices.Any())
                        {
                            foreach (var agDevice in agDevices)
                            {
                                _accessGroupService.SendVisitor(agDevice, true, visitor);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        private void SendIdentificationToDevice(Visit visitor, Card newCard, bool isAdd)
        {
            if (visitor == null)
            {
                _logger.LogError("Can not find this visitor in system");
                return;
            }

            // var agDevices = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(_httpContext.User.GetCompanyId(), visitor.AccessGroupId);
            // if (agDevices != null && agDevices.Any())
            // {
            //     foreach (var agDevice in agDevices)
            //     {
            //         _accessGroupService.SendIdentificationToDeviceVisitor(agDevice, visitor, newCard, isAdd);
            //     }
            // }
            var agDevices = _unitOfWork.AccessGroupDeviceRepository
                .GetByAccessGroupId(_httpContext.User.GetCompanyId(), visitor.AccessGroupId).Select(x => x.Icu).ToList();
            ThreadSendCardToDevice(new List<int> { newCard.Id }, new List<int> { visitor.Id }, agDevices, _httpContext.User.GetUsername(),
                !isAdd ? Constants.Protocol.DeleteUser : Constants.Protocol.AddUser);
        }

        /// <summary>
        /// Generate QR
        /// </summary>
        /// <returns></returns>
        internal string GenQrId()
        {
            var chars = Constants.Settings.CharacterGenQrCode;
            var stringChars = new char[Constants.Settings.LengthCharacterGenQrCode];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            if (_unitOfWork.CardRepository.CheckIsExistedQrCode(finalString) != true)
            {
                GenQrId();
            }
            return finalString;
        }

        /// <summary>
        /// Encrypt QR code
        /// </summary>
        /// <param name="companySecretCode"></param>
        /// <param name="qrId"></param>
        /// <returns></returns>
        internal byte[] EncryptQrCode(string companySecretCode, string qrId)
        {
            // SECURITY: Replaced RijndaelManaged with AES for better security
            Aes aes = Aes.Create();
            aes.BlockSize = Constants.AutoRenew.BlockSize;
            aes.KeySize = Constants.AutoRenew.KeySize;
            aes.Mode = CipherMode.ECB;

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData genQrCode =
                qrGenerator.CreateQrCode(qrId,
                    QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(genQrCode);
            byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(20);
            return qrCodeAsPngByteArr;
        }

        /// <summary>
        /// Assign doors to visitor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="doors"></param>
        /// <returns></returns>
        public bool AssignedDoorVisitor(int id, List<int> doors)
        {
            var visitor = GetById(id);
            var card = _unitOfWork.AppDbContext.Card.Where(c => !c.IsDeleted && c.VisitId == id).ToList();

            if (card.Count > 1 || card.Count == 0)
            {
                return false;
            }
            else
            {
                _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
                {
                    using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var companyId = _httpContext.User.GetCompanyId();

                            var accessGroup = new AccessGroup
                            {
                                Name = Constants.Settings.NameAccessGroupVisitor + id,
                                CompanyId = companyId,
                                Type = (short)AccessGroupType.VisitAccess
                            };
                            _unitOfWork.AccessGroupRepository.Add(accessGroup);
                            _unitOfWork.Save();

                            visitor.AccessGroupId = accessGroup.Id;
                            _unitOfWork.VisitRepository.Update(visitor);
                            _unitOfWork.Save();

                            foreach (var door in doors)
                            {
                                var detailDoor = _unitOfWork.IcuDeviceRepository.GetById(door);
                                var detailModel = new AccessGroupDevice
                                {
                                    IcuId = detailDoor.Id,
                                    TzId = detailDoor.ActiveTzId.Value,
                                    AccessGroupId = visitor.AccessGroupId
                                };
                                _unitOfWork.AccessGroupDeviceRepository.Add(detailModel);
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
                SendIdentificationToDevice(visitor, card.FirstOrDefault(), true);
                return true;
            }
        }

        public bool RejectVisitor(int id, RejectedModel model)
        {
            bool success = true;
            var visitor = GetById(id);
            var accountId = _httpContext.User.GetAccountId();
            var companyId = _httpContext.User.GetCompanyId();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var oldStatus = visitor.Status;

                        visitor.RejectDate = DateTime.Now;
                        visitor.UpdatedOn = DateTime.Now;
                        visitor.UpdatedBy = accountId;
                        if (!string.IsNullOrEmpty(model.Reason))
                        {
                            visitor.RejectReason = model.Reason;
                        }
                        visitor.Status = (short)VisitChangeStatusType.Rejected;
                        _unitOfWork.VisitRepository.Update(visitor);

                        var log = new VisitHistory
                        {
                            VisitorId = visitor.Id,
                            CompanyId = companyId,
                            OldStatus = oldStatus,
                            NewStatus = visitor.Status,
                            UpdatedBy = accountId,
                            CreatedOn = DateTime.UtcNow,
                            Reason = model.Reason
                        };
                        _unitOfWork.AppDbContext.VisitHistory.Add(log);

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex.Message + ex.StackTrace);
                        success = false;

                    }
                }
            });

            if (success)
            {
                // send count review
                _accountService.SendCountReviewToFe(accountId, companyId);
            }

            return success;
        }

        public IQueryable<VisitListHistoryModel> GetPaginatedVisitHistoryLog(int id, int pageNumber, int pageSize,
            string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();

            List<int> groupVisitorIds = new List<int>();
            var visitor = _unitOfWork.VisitRepository.GetById(id);
            if (visitor != null)
            {
                var visitors = _unitOfWork.VisitRepository.GetByGroupIdAndStatus(visitor.GroupId, visitor.Status);

                if (visitors != null && visitors.Any())
                {
                    groupVisitorIds = visitors.Select(v => v.Id).ToList();

                    groupVisitorIds.Remove(visitor.Id);
                }
            }

            List<int> personalStatus = new List<int>()
                {
                    (int)VisitChangeStatusType.CardIssued,
                    (int)VisitChangeStatusType.CardReturned,
                    (int)VisitChangeStatusType.Finished,
                };

            var data = _unitOfWork.AppDbContext.VisitHistory
                .Where(c => c.CompanyId == companyId && (c.VisitorId == id || (groupVisitorIds.Contains(c.VisitorId) && personalStatus.Contains(c.NewStatus))))
                .Select(m => new VisitListHistoryModel()
                {
                    Id = m.Id,
                    CompanyId = m.CompanyId,
                    VisitorId = m.VisitorId,
                    OldStatus = m.OldStatus,
                    OldStatusString = m.OldStatus != null ? ((VisitChangeStatusType)m.OldStatus).GetDescription() : "",
                    NewStatusString = ((VisitChangeStatusType)m.NewStatus).GetDescription(),
                    NewStatus = m.NewStatus,
                    CreatedOn = m.CreatedOn,
                    UpdatedBy = m.UpdatedBy,
                    Reason = m.Reason
                });

            totalRecords = data.Count();

            recordsFiltered = data.Count();
            data = data.OrderBy($"{sortColumn} {sortDirection}");
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return data;
        }

        public void FinishVisitor(int id, bool isGroup, int companyId, int accountId)
        {
            var start1 = DateTime.UtcNow;

            var visitor = _unitOfWork.VisitRepository.GetByVisitId(companyId, id);
            List<Visit> visitors = _unitOfWork.VisitRepository.GetByGroupId(visitor.GroupId);

            if (!isGroup)
            {
                visitors = null;
            }

            if (visitors == null || !visitors.Any())
            {
                visitors = new List<Visit>() { visitor };
            }
            else
            {
                visitors = visitors.Where(m => m.Status == visitor.Status).ToList();
            }

            var company = _unitOfWork.CompanyRepository.GetById(companyId);
            var devices = _unitOfWork.IcuDeviceRepository.GetDeviceAllInfoByCompany(companyId).ToList();
            if (company != null && !company.AutoSyncUserData)
                devices = devices.Where(m => m.ConnectionStatus == (short)ConnectionStatus.Online).ToList();

            var start2 = DateTime.UtcNow;
            Console.WriteLine($"#################### GET DEVICE TIME : {(start2 - start1).TotalMilliseconds} (milliseconds)");
            Console.WriteLine($"#########################################################################################");

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var oldStatus = visitor.Status;
                        var newStatus = (short)VisitChangeStatusType.Finished;

                        switch (oldStatus)
                        {
                            case (short)VisitChangeStatusType.Waiting:
                            case (short)VisitChangeStatusType.Approved1:
                            case (short)VisitChangeStatusType.Approved:
                            case (short)VisitChangeStatusType.AutoApproved:
                            case (short)VisitChangeStatusType.CardReturned:
                                newStatus = (short)VisitChangeStatusType.Finished;
                                break;
                            case (short)VisitChangeStatusType.CardIssued:
                                newStatus = (short)VisitChangeStatusType.FinishedWithoutReturnCard;
                                break;
                        }

                        foreach (var eachVisitor in visitors)
                        {
                            if (eachVisitor == null) continue;
                            start1 = DateTime.UtcNow;

                            // Steps to finish visit
                            // 1. Send DELETE messages to AGD
                            // (But now, server sends messages to all devices)
                            foreach (var device in devices)
                            {
                                var cardTypes = Helpers.GetMatchedIdentificationType(device.DeviceType);
                                if(eachVisitor.Card.Any(m => cardTypes.Contains(m.CardType)))
                                {
                                    List<UserLog> userLogs = eachVisitor.Card.Where(m => cardTypes.Contains(m.CardType)).Select(_mapper.Map<UserLog>).ToList();
                                    _accessGroupService.SendAddOrDeleteUser(device.DeviceAddress, userLogs.SplitList(Helpers.GetMaxSplit(device.DeviceType)), false);
                                }
                            }

                            start2 = DateTime.UtcNow;
                            Console.WriteLine($"#################### SEND DELETE : {(start2 - start1).TotalMilliseconds} (milliseconds)");

                            // 2. Delete visitor's AG
                            // To be exact, change the IsDeleted column of the AG to TRUE.
                            _unitOfWork.AccessGroupRepository.DeleteFromSystem(eachVisitor.AccessGroup);
                            // 2-1. Delete visitor's AGDs.
                            var agDevices = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(eachVisitor.CompanyId, eachVisitor.AccessGroupId, null, false);
                            _unitOfWork.AccessGroupDeviceRepository.DeleteRange(agDevices);

                            var start3 = DateTime.UtcNow;
                            Console.WriteLine($"#################### AG -> DELETE : {(start3 - start2).TotalMilliseconds} (milliseconds)");
                            Console.WriteLine($"#########################################################################################");

                            // get latest checkout of visitor
                            if (newStatus == (short)VisitChangeStatusType.Finished)
                            {
                                var eventLogLast = _unitOfWork.EventLogRepository.GetLastEventOfVisitor(eachVisitor.CompanyId, eachVisitor.Id, eachVisitor.StartDate, eachVisitor.EndDate, EventType.NormalAccess, "out");
                                if (eventLogLast != null)
                                {
                                    eachVisitor.EndDate = eventLogLast.EventTime;
                                }
                            }
                            
                            // 3. Update visitor's status to FINISHED.
                            eachVisitor.Status = newStatus;
                            _unitOfWork.VisitRepository.Update(eachVisitor);

                            // 4. Add VisitHistory
                            var log = new VisitHistory
                            {
                                VisitorId = eachVisitor.Id,
                                CompanyId = companyId,
                                OldStatus = oldStatus,
                                NewStatus = newStatus,
                                UpdatedBy = accountId,
                                CreatedOn = DateTime.UtcNow,
                                Reason = accountId == 0 ? Constants.RabbitMq.SenderDefault : ""
                            };
                            _unitOfWork.AppDbContext.VisitHistory.Add(log);

                            // 5. Delete Vehicle information
                            var vehicles = _unitOfWork.VehicleRepository.GetListVehicleByVisit(eachVisitor.Id).ToList();
                            _unitOfWork.VehicleRepository.DeleteRangeFromSystem(vehicles);

                            // 6. Delete Cards information
                            var cards = _unitOfWork.CardRepository.GetByVisitId(companyId, eachVisitor.Id).ToList();
                            _unitOfWork.CardRepository.DeleteRangeFromSystem(cards);
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
        }

        public void DeletedAssignedCardVisitor(int id)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var cards = _unitOfWork.AppDbContext.Card.Where(c => !c.IsDeleted && c.VisitId == id && c.CardType == (int)CardType.NFC);
                        if (cards.Any())
                        {
                            foreach (var card in cards)
                            {
                                _unitOfWork.CardRepository.Delete(card);
                            }
                            _unitOfWork.Save();
                            transaction.Commit();
                        }
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
        /// Check if the card already exists.
        /// </summary>
        /// <param name="cardId"> card Id(Uid, etc..) </param>
        /// <returns></returns>
        public bool IsCardIdExist(string cardId, int companyId)
        {
            var data = _unitOfWork.AppDbContext.Card.Where(c => !c.IsDeleted && c.CardId == cardId && c.CompanyId == companyId &&
                                                                (c.UserId.HasValue || c.VisitId.HasValue));

            return data.Any();
        }

        /// <summary>
        /// Count visitor that is waiting to review(approval).
        /// </summary>
        /// <returns></returns>
        public int LengthVisitNeedToReview()
        {
            var currentAccountId = _httpContext.User.GetAccountId();

            var count = _unitOfWork.AppDbContext.Visit.Count(v => v.CompanyId == _httpContext.User.GetCompanyId()
                                                                     && (v.Status == (short)VisitChangeStatusType.Waiting && v.ApproverId1 == currentAccountId)
                                                                     && !v.IsDeleted);

            return count;
        }

        /// <summary>
        /// Get the number of visitors waiting for approval
        /// </summary>
        /// <returns></returns>
        public int GetRequestApprovalCount(int companyId, int accountId)
        {
            var visitorCount = _unitOfWork.VisitRepository.GetByFirstApprovalId(companyId, accountId).Count();

            return visitorCount;
        }

        public void ReturnCardVisitor(string cardId, string reason)
        {
            var companyId = _httpContext.User.GetCompanyId();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var cards = _unitOfWork.AppDbContext.Card.Where(c => !c.IsDeleted && c.CardId == cardId && c.CompanyId == companyId);
                        foreach (var card in cards)
                        {
                            var visit = _unitOfWork.AppDbContext.Visit.FirstOrDefault(c => !c.IsDeleted && c.Id == card.VisitId && c.CompanyId == companyId);

                            if (visit != null)
                            {
                                if (visit.Status != (short)VisitChangeStatusType.Finished && visit.Status != (short)VisitChangeStatusType.CardReturned)
                                {
                                    SendIdentificationToDevice(visit, card, false);
                                    var oldStatus = visit.Status;
                                    //visit.Status = (short)VisitChangeStatusType.Finished;
                                    visit.Status = (short)VisitChangeStatusType.CardReturned;
                                    var log = new VisitHistory
                                    {
                                        VisitorId = visit.Id,
                                        CompanyId = companyId,
                                        OldStatus = oldStatus,
                                        NewStatus = visit.Status,
                                        UpdatedBy = _httpContext.User.GetAccountId(),
                                        CreatedOn = DateTime.UtcNow,
                                        Reason = reason
                                    };
                                    _unitOfWork.AppDbContext.VisitHistory.Add(log);
                                    _unitOfWork.CardRepository.Delete(card);
                                    _unitOfWork.VisitRepository.Update(visit);
                                }
                            }

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
        }

        public void ReturnVisitor(List<int> visitIds, string reason)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var visitId in visitIds)
                        {
                            var visitor = _unitOfWork.AppDbContext.Visit.FirstOrDefault(c => !c.IsDeleted && c.Id == visitId);
                            if (visitor != null)
                            {
                                if (visitor.Status != (short)VisitChangeStatusType.Finished && visitor.Status != (short)VisitChangeStatusType.CardReturned)
                                {
                                    // send delete all card of visit
                                    SendUpdateVisitsToAllDoors(new List<Visit> { visitor }, _httpContext.User.GetUsername(), false);
                                    var cards = _unitOfWork.AppDbContext.Card.Where(c => !c.IsDeleted && c.VisitId == visitId);

                                    foreach (var card in cards)
                                    {
                                        // SendIdentificationToDevice(visitor, card, false);
                                        _unitOfWork.CardRepository.Delete(card);
                                    }

                                    var oldStatus = visitor.Status;
                                    //visitor.Status = (short)VisitChangeStatusType.Finished;
                                    visitor.Status = (short)VisitChangeStatusType.CardReturned;
                                    var log = new VisitHistory
                                    {
                                        VisitorId = visitor.Id,
                                        CompanyId = _httpContext.User.GetCompanyId(),
                                        OldStatus = oldStatus,
                                        NewStatus = visitor.Status,
                                        UpdatedBy = _httpContext.User.GetAccountId(),
                                        CreatedOn = DateTime.UtcNow,
                                        Reason = reason
                                    };
                                    _unitOfWork.AppDbContext.VisitHistory.Add(log);
                                    _unitOfWork.VisitRepository.Update(visitor);
                                }
                            }
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
        }

        public void ReturnCardVisitor(int visitId, string cardId, string reason)
        {
            var companyId = _httpContext.User.GetCompanyId();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var cards = _unitOfWork.AppDbContext.Card.Where(c => !c.IsDeleted && c.CardId.ToLower().Equals(cardId.ToLower()) && c.VisitId == visitId && c.CompanyId == companyId);
                        foreach (var card in cards)
                        {
                            var visit = _unitOfWork.AppDbContext.Visit.FirstOrDefault(c => !c.IsDeleted && c.Id == card.VisitId && c.CompanyId == companyId);

                            if (visit != null)
                            {
                                if (visit.Status != (short)VisitChangeStatusType.Finished && visit.Status != (short)VisitChangeStatusType.CardReturned)
                                {
                                    var oldStatus = visit.Status;
                                    //visit.Status = (short)VisitChangeStatusType.Finished;
                                    visit.Status = (short)VisitChangeStatusType.CardReturned;
                                    var log = new VisitHistory
                                    {
                                        VisitorId = visit.Id,
                                        CompanyId = companyId,
                                        OldStatus = oldStatus,
                                        NewStatus = visit.Status,
                                        UpdatedBy = _httpContext.User.GetAccountId(),
                                        CreatedOn = DateTime.UtcNow,
                                        Reason = reason
                                    };
                                    _unitOfWork.AppDbContext.VisitHistory.Add(log);

                                    _unitOfWork.CardRepository.DeleteFromSystem(card);

                                    _unitOfWork.VisitRepository.Update(visit);

                                    SendIdentificationToDevice(visit, card, false);
                                }
                            }
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
        }


        public void Delete(int visitId)
        {
            string sender = _httpContext.User.GetUsername();
            bool hasSend = false;
            var visit = _unitOfWork.AppDbContext.Visit.Single(v => v.Id == visitId && !v.IsDeleted);

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var cards = _unitOfWork.AppDbContext.Card.Where(m => m.VisitId == visitId);

                        // send delete all card of visit
                        SendUpdateVisitsToAllDoors(new List<Visit> { visit }, sender, false);
                        hasSend = true;

                        var accessGroup = _unitOfWork.AppDbContext.AccessGroup.FirstOrDefault(a => a.Id == visit.AccessGroupId && a.CompanyId == _httpContext.User.GetCompanyId());
                        if (accessGroup != null)
                        {
                            var agDevices = _unitOfWork.AppDbContext.AccessGroupDevice.Where(ag => ag.AccessGroupId == accessGroup.Id);
                            if (agDevices.Any())
                                _unitOfWork.AccessGroupDeviceRepository.DeleteRange(agDevices);
                        }
                        if (cards.Any())
                            _unitOfWork.CardRepository.DeleteRange(cards);
                        if (visit != null)
                            _unitOfWork.VisitRepository.Delete(visit);
                        if (accessGroup != null)
                            _unitOfWork.AccessGroupRepository.Delete(accessGroup);
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        transaction.Rollback();
                        if (hasSend)
                        {
                            SendVisitsToAllDoors(new List<int> { visit.Id }, true);
                        }
                    }
                }
            });

             //Save system log 
            var companyId = _httpContext.User.GetCompanyId();
            var content =
                VisitResource.msgDeleteVisit;
            _unitOfWork.SystemLogRepository.Add(companyId, SystemLogType.VisitManagement, ActionLogType.Delete, content, null, null, companyId);
            _unitOfWork.Save();
        }

        public void DeleteRange(List<int> visitIds)
        {
            bool hasSend = false;
            int companyId = _httpContext.User.GetCompanyId();
            string sender = _httpContext.User.GetUsername();
            var visits = _unitOfWork.AppDbContext.Visit.Where(v => visitIds.Contains(v.Id) && !v.IsDeleted);
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // Delete cards and send DELETE VISIT message
                        SendUpdateVisitsToAllDoors(visits.ToList(), sender, false);
                        hasSend = true;

                        foreach (var visit in visits)
                        {
                            var cards = _unitOfWork.AppDbContext.Card.Where(m => m.VisitId == visit.Id);
                            var accessGroup = _unitOfWork.AppDbContext.AccessGroup.FirstOrDefault(a => a.Id == visit.AccessGroupId && a.CompanyId == _httpContext.User.GetCompanyId());
                            if (accessGroup != null)
                            {
                                var agDevices = _unitOfWork.AppDbContext.AccessGroupDevice.Where(ag => ag.AccessGroupId == accessGroup.Id);
                                if (agDevices.Any())
                                    _unitOfWork.AccessGroupDeviceRepository.DeleteRange(agDevices);
                            }
                            if (cards.Any())
                                _unitOfWork.CardRepository.DeleteRange(cards);
                            if (visit != null)
                                _unitOfWork.VisitRepository.Delete(visit);
                            if (accessGroup != null)
                                _unitOfWork.AccessGroupRepository.Delete(accessGroup);
                        }

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        transaction.Rollback();
                        if (hasSend)
                        {
                            SendVisitsToAllDoors(visits.Select(m => m.Id).ToList(), true);
                        }
                    }
                }
            });

            //Save system log 
            var content = VisitResource.msgDeleteVisit;
            _unitOfWork.SystemLogRepository.Add(companyId, SystemLogType.VisitManagement, ActionLogType.DeleteMultiple, content, null, null, companyId);
            _unitOfWork.Save();
        }

        public int GetLastApproval(int visitId)
        {
            var companyId = _httpContext.User.GetCompanyId();

            var visitors1 = _unitOfWork.AppDbContext.VisitHistory.Where(v => v.CompanyId == companyId
                                                                     && v.VisitorId == visitId);

            var visitor = visitors1.ToList().Last();

            //var visitors= _unitOfWork.AppDbContext.VisitHistory.Last(v => v.CompanyId == companyId
            //                                                         //&& v.NewStatus == (short) VisitChangeStatusType.Approved 
            //                                                         && v.VisitorId == visitId);
            if (visitor != null)
            {
                return visitor.UpdatedBy;
            }

            return 0;
        }

        public IEnumerable<EventLog> GetHistoryVisitor(int id, int pageNumber, int pageSize, out int totalRecords, out int recordsFiltered)
        {
            var data = _unitOfWork.VisitRepository.GetHistoryVisitor(id, pageNumber, pageSize, out totalRecords, out recordsFiltered).ToList();

            return data;
        }

        static string EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            // SECURITY: Replaced RijndaelManaged with AES for better security
            // Create an AES object with the specified key and IV
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            // Return the encrypted bytes from the memory stream.
            var result = Combine(IV, encrypted);
            return BitConverter.ToString(result).Replace("-", "");
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        public int GetTodayRegisteredVisitorsCount(int companyId, DateTime day)
        {
            return _unitOfWork.VisitRepository.GetByDayRegisteredVisitorsCount(companyId, day);
        }

        /// <summary>
        /// Generate string base 64 image QR-Code for visitor
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="visitorId"></param>
        /// <returns></returns>
        public VisitQRCodeModel CreateQRCodeForVisitor(int visitorId)
        {
            // get visitor in database
            var visitor = _unitOfWork.AppDbContext.Visit.FirstOrDefault(v => v.Id == visitorId && !v.IsDeleted);
            if (visitor == null) return null;

            // get card qr-code
            var cards = _unitOfWork.CardRepository.GetByVisitId(visitor.CompanyId, visitorId).Where(x => x.CardType == (short)CardType.QrCode).ToList();
            if (!cards.Any()) return null;
            var card = cards[0];

            var checkQrCodeCompany = _unitOfWork.AppDbContext.PlugIn.Where(x => x.CompanyId == visitor.CompanyId).Select(x => x.PlugIns).FirstOrDefault();
            if (JsonConvert.DeserializeObject<PlugIns>(checkQrCodeCompany).QrCode)
            {
                // get timezone
                var buildings = _unitOfWork.BuildingRepository.GetByCompanyId(visitor.CompanyId);
                var building = buildings.ToList()[0];

                TimeZoneInfo info = building.TimeZone.ToTimeZoneInfo();

                DateTime endTimeVisitor = TimeZoneInfo.ConvertTimeToUtc(visitor.EndDate, info);

                // generate qr-code with cardId
                string dynamicQrCode = GetStaticQrCodeForVisitor(visitor, card.CardId);
                var company = _unitOfWork.CompanyRepository.GetById(visitor.CompanyId);
                byte[] qrCodeAsPngByteArr = EncryptQrCode(company.SecretCode, dynamicQrCode);

                if (visitor.VisiteeId.HasValue)
                {
                    var visitee = _unitOfWork.UserRepository.GetByUserId(visitor.CompanyId, visitor.VisiteeId.Value);
                    if (visitee != null)
                    {
                        visitor.VisiteeName = visitee.FirstName;
                        visitor.VisiteeDepartment = visitor.VisiteeDepartment;
                        visitor.VisiteeSite = visitee.Department?.DepartName ?? visitor.VisiteeSite;
                    }
                }

                return new VisitQRCodeModel()
                {
                    Id = visitorId,
                    VisitorName = visitor.VisitorName,
                    Phone = visitor.Phone,
                    Email = visitor.Email,
                    Address = building.Address,
                    CompanyName = company.Name,
                    StartDate = visitor.StartDate,
                    EndDate = visitor.EndDate,
                    VisiteeName = visitor.VisiteeName,
                    VisiteeDepartment = visitor.VisiteeDepartment,
                    Avatar = visitor.Avatar,
                    QRCode = "data:image/*;base64," + Convert.ToBase64String(qrCodeAsPngByteArr),
                    Language = Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetLanguage(company.Id).Value),
                };
            }

            return null;
        }

        public void UpdateSettingVisibleFieldsByCompanyId(int companyId, Dictionary<string, bool> model)
        {
            var visitSetting = GetVisitSettingByCompanyId(companyId);
            visitSetting.VisibleFields = JsonConvert.SerializeObject(model);
            _unitOfWork.VisitRepository.UpdateVisitSetting(visitSetting);
        }

        public VisitCardInfo GetAllInfoByCardVisit(string cardId)
        {
            int companyId = _httpContext.User.GetCompanyId();
            var cardVisit = _unitOfWork.AppDbContext.Card.Include(m => m.Visit)
                .Include(m => m.Visit).ThenInclude(m => m.Vehicle)
                .FirstOrDefault(m => !m.IsDeleted && m.CompanyId == companyId && m.CardId.ToLower() == cardId.ToLower());
            if (cardVisit == null)
            {
                return null;
            }
            else
            {
                var accountTimezone = _unitOfWork.AccountRepository.Get(m => m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted).TimeZone;
                var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;
                var model = _mapper.Map<VisitCardInfo>(cardVisit);
                model.StartDate = model.StartDate.ConvertToUserTime(offSet);
                model.EndDate = model.EndDate.ConvertToUserTime(offSet);

                var visitHistory = _unitOfWork.AppDbContext.VisitHistory.FirstOrDefault(m =>
                    m.CompanyId == companyId && m.VisitorId == model.VisitId &&
                    m.NewStatus == (short)VisitChangeStatusType.CardIssued);

                if (visitHistory != null)
                    model.AssignedCardDate = visitHistory.CreatedOn.ConvertToUserTime(offSet); ;

                return model;
            }
        }

        public void AddFirstApproverSetting(List<int> ids, int companyId)
        {
            var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);
            List<int> accountIds = new List<int>();
            if (!string.IsNullOrEmpty(visitSetting.FirstApproverAccounts))
            {
                accountIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.FirstApproverAccounts);
            }

            ids = ids.Where(m => !accountIds.Contains(m)).ToList();
            accountIds.AddRange(ids);
            visitSetting.FirstApproverAccounts = JsonConvert.SerializeObject(accountIds);
            _unitOfWork.VisitRepository.UpdateVisitSetting(visitSetting);
            _unitOfWork.Save();
        }

        public void AddSecondApproverSetting(List<int> ids, int companyId)
        {
            var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);
            List<int> accountIds = new List<int>();
            if (!string.IsNullOrEmpty(visitSetting.SecondsApproverAccounts))
            {
                accountIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.SecondsApproverAccounts);
            }

            ids = ids.Where(m => !accountIds.Contains(m)).ToList();
            accountIds.AddRange(ids);
            visitSetting.SecondsApproverAccounts = JsonConvert.SerializeObject(accountIds);
            _unitOfWork.VisitRepository.UpdateVisitSetting(visitSetting);
            _unitOfWork.Save();
        }

        public void AddCheckManagerSetting(List<int> ids, int companyId)
        {
            var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);
            List<int> accountIds = new List<int>();
            if (!string.IsNullOrEmpty(visitSetting.VisitCheckManagerAccounts))
            {
                accountIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.VisitCheckManagerAccounts);
            }

            ids = ids.Where(m => !accountIds.Contains(m)).ToList();
            accountIds.AddRange(ids);
            visitSetting.VisitCheckManagerAccounts = JsonConvert.SerializeObject(accountIds);
            _unitOfWork.VisitRepository.UpdateVisitSetting(visitSetting);
            _unitOfWork.Save();
        }

        public void DeleteFirstApproverSetting(List<int> ids, int companyId)
        {
            var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);
            List<int> accountIds = new List<int>();
            if (!string.IsNullOrEmpty(visitSetting.FirstApproverAccounts))
            {
                accountIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.FirstApproverAccounts);
            }

            accountIds = accountIds.Where(m => !ids.Contains(m)).ToList();
            visitSetting.FirstApproverAccounts = JsonConvert.SerializeObject(accountIds);
            _unitOfWork.VisitRepository.UpdateVisitSetting(visitSetting);
            _unitOfWork.Save();
        }

        public void DeleteSecondApproverSetting(List<int> ids, int companyId)
        {
            var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);
            List<int> accountIds = new List<int>();
            if (!string.IsNullOrEmpty(visitSetting.SecondsApproverAccounts))
            {
                accountIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.SecondsApproverAccounts);
            }

            accountIds = accountIds.Where(m => !ids.Contains(m)).ToList();
            visitSetting.SecondsApproverAccounts = JsonConvert.SerializeObject(accountIds);
            _unitOfWork.VisitRepository.UpdateVisitSetting(visitSetting);
            _unitOfWork.Save();
        }

        public void DeleteCheckManagerSetting(List<int> ids, int companyId)
        {
            var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);
            List<int> accountIds = new List<int>();
            if (!string.IsNullOrEmpty(visitSetting.VisitCheckManagerAccounts))
            {
                accountIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.VisitCheckManagerAccounts);
            }

            accountIds = accountIds.Where(m => !ids.Contains(m)).ToList();
            visitSetting.VisitCheckManagerAccounts = JsonConvert.SerializeObject(accountIds);
            _unitOfWork.VisitRepository.UpdateVisitSetting(visitSetting);
            _unitOfWork.Save();
        }

        public VisitDataModel GetVisitByQrCodeEncrypt(string qrCode, int companyId)
        {
            Company company = _unitOfWork.CompanyRepository.GetById(companyId);
            string textEncrypt = qrCode.Replace(Constants.DynamicQr.NameProject + "_", "");
            string textDecrypt = DecryptStringFromBytes(textEncrypt, Encoding.UTF8.GetBytes(company.SecretCode),
                Encoding.UTF8.GetBytes(Helpers.ReverseString(Constants.DynamicQr.Key)));

            try
            {
                if (!string.IsNullOrEmpty(textDecrypt))
                {
                    string[] texts = textDecrypt.Split("_");
                    int count = texts.Length;
                    if (count >= 3)
                    {
                        if (texts[count - 1].Contains("XX")) count = count - 1;
                        count = count - 3;
                        string qr = texts[count].Substring(texts[count].Length - Constants.Settings.LengthCharacterGenQrCode);
                        var card = _unitOfWork.CardRepository.GetByCardId(companyId, qr);
                        if (card.CardType == (int)CardType.QrCode && card.VisitId.HasValue)
                        {
                            var visit = _unitOfWork.VisitRepository.GetByVisitId(companyId, card.VisitId.Value);
                            if (visit != null)
                            {
                                VisitDataModel model;
                                TimeSpan offSet;

                                var account = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId());
                                if (!string.IsNullOrEmpty(account?.TimeZone))
                                {
                                    offSet = account.TimeZone.ToTimeZoneInfo().BaseUtcOffset;
                                }
                                else
                                {
                                    var building = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(company.Id);
                                    offSet = building.TimeZone.ToTimeZoneInfo().BaseUtcOffset;
                                }

                                visit.StartDate = visit.StartDate.ConvertToUserTime(offSet);
                                visit.EndDate = visit.EndDate.ConvertToUserTime(offSet);

                                model = _mapper.Map<VisitDataModel>(visit);

                                // add field doors assign
                                if (visit.AccessGroupId != 0)
                                {
                                    List<int> doors = new List<int>();
                                    var accessGroupDevices = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(visit.CompanyId, visit.AccessGroupId);
                                    foreach (var item in accessGroupDevices)
                                    {
                                        doors.Add(item.IcuId);
                                    }

                                    model.Doors = JsonConvert.SerializeObject(doors);
                                }

                                return model;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }

            return null;
        }

        private string DecryptStringFromBytes(string plainText, byte[] key, byte[] iv)
        {
            string result = "";
            try
            {
                // Check arguments. 
                if (plainText == null || plainText.Length <= 0)
                    throw new ArgumentNullException(nameof(plainText));
                if (key == null || key.Length <= 0)
                    throw new ArgumentNullException(nameof(key));
                if (iv == null || iv.Length <= 0)
                    throw new ArgumentNullException(nameof(iv));

                // SECURITY: Replaced RijndaelManaged with AES for better security
                // Create an AES object with the specified key and IV
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    // Create a decryptor to perform the stream transform
                    ICryptoTransform decrypt = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for encryption.
                    using (MemoryStream ms = new MemoryStream(StringToByteArray(plainText)))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Read))
                        {
                            using (StreamReader sw = new StreamReader(cs))
                            {

                                //Write all data to the stream.
                                result = sw.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message + e.StackTrace);
                result = null;
            }

            return result;
        }

        private static byte[] StringToByteArray(string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public List<VisitorCardModel> GetVisitorsByCardIds(List<string> cardIds)
        {
            List<VisitorCardModel> result = new List<VisitorCardModel>();
            var companyId = _httpContext.User.GetCompanyId();

            int index = 0;

            foreach (var cardId in cardIds)
            {
                var cleanCardId = cardId.RemoveAllEmptySpace();

                var card = _unitOfWork.AppDbContext.Card
                    .Include(c => c.Visit)
                    .FirstOrDefault(c => c.VisitId != null && !c.IsDeleted && c.CardType == (int)CardType.NFC && c.CardId.ToLower().Equals(cleanCardId.ToLower()));

                if (card != null)
                {
                    VisitorCardModel data = new VisitorCardModel()
                    {
                        Id = index++,
                        CardId = cleanCardId,
                        IssueCount = card.IssueCount,
                        VisitorName = card.Visit.VisitorName,
                        VisitId = card.VisitId.Value
                    };

                    result.Add(data);
                }
            }

            return result;
        }

        public List<VisitorCardModel> GetVisitorCardByVisitIds(List<int> visitIds)
        {
            List<VisitorCardModel> result = new List<VisitorCardModel>();
            var companyId = _httpContext.User.GetCompanyId();

            int index = 0;

            foreach (var id in visitIds)
            {
                Visit visit = _unitOfWork.VisitRepository.GetById(id);

                if (visit != null)
                {
                    var card = _unitOfWork.AppDbContext.Card
                        .Include(c => c.Visit)
                        .FirstOrDefault(c => c.VisitId != null && !c.IsDeleted && c.CardType == (int)CardType.NFC && c.VisitId == visit.Id);

                    if (card != null)
                    {
                        VisitorCardModel data = new VisitorCardModel()
                        {
                            Id = index++,
                            CardId = card.CardId,
                            IssueCount = card.IssueCount,
                            VisitorName = visit.VisitorName,
                            VisitId = visit.Id
                        };

                        result.Add(data);
                    }
                }
            }

            return result;
        }

        public bool IsExistedVisitee(int visiteeId)
        {
            return _unitOfWork.UserRepository.GetById(visiteeId) != null;
        }
        
        public void UpdateFieldLayoutRegister(FieldLayoutRegister model, int companyId)
        {
            var setting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);
            setting.FieldRegisterLeft = JsonConvert.SerializeObject(model.FieldRegisterLeft);
            setting.FieldRegisterRight = JsonConvert.SerializeObject(model.FieldRegisterRight);
            _unitOfWork.VisitRepository.UpdateVisitSetting(setting);
            _unitOfWork.Save();
        }

        public List<UserListModel> GetUserTarget(UserTargetFilterModel filter, out int recordsFiltered, out int recordsTotal)
        {
            var data = _unitOfWork.UserRepository.GetByCompanyId(filter.CompanyId);
            if (!filter.BothUserNoHasAccount)
            {
                data = data.Where(m => m.AccountId.HasValue);
            }
            recordsTotal = data.Count();

            if (!string.IsNullOrEmpty(filter.Name))
            {
                data = data.Where(m => m.FirstName.ToLower().Contains(filter.Name.ToLower()));
            }
            if (filter.DepartmentIds != null && filter.DepartmentIds.Any())
            {
                data = data.Where(m => filter.DepartmentIds.Contains(m.DepartmentId));
            }

            recordsFiltered = data.Count();

            data = data.OrderBy($"{filter.SortColumn} {filter.SortDirection}");
            if (filter.PageSize > 0)
                data = data.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);

            return data.AsEnumerable<User>().Select(_mapper.Map<UserListModel>).ToList();
        }

        public Visit GetByPhoneAndCompanyId(string phone, int companyId)
        {
            return _unitOfWork.VisitRepository.GetByPhoneAndCompanyId(phone, companyId);
        }

        public string PrepareQrCodeForVisitor(Visit visitor, short cardStatus = (short)CardStatus.InValid)
        {
            var cardQrCode = _unitOfWork.CardRepository.GetQrCodeForVisitor(visitor.Id);

            if (cardQrCode != null)
            {
                return GetStaticQrCodeForVisitor(visitor, cardQrCode.CardId);
            }
            else
            {
                string qrId = GenQrId();
                var qrCode = new Card
                {
                    CardId = qrId,
                    IssueCount = 0,
                    CompanyId = visitor.CompanyId,
                    VisitId = visitor.Id,
                    CardType = (short)CardType.QrCode,
                    ValidFrom = visitor.StartDate,
                    ValidTo = visitor.EndDate,
                    CardStatus = cardStatus,
                };

                _unitOfWork.CardRepository.Add(qrCode);
                _unitOfWork.Save();

                return GetStaticQrCodeForVisitor(visitor, qrId);
            }
        }
        

        /// <summary>
        /// Write systemLog
        /// </summary>
        /// <param name="logObjId"></param>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <param name="contentDetails"></param>
        public void WriteSystemLog(int logObjId, ActionLogType type, string content, string contentDetails)
        {
            _unitOfWork.SystemLogRepository.Add(logObjId, SystemLogType.VisitManagement, type, content, contentDetails, null, _httpContext.User.GetCompanyId());
            _unitOfWork.Save();
        }

        public string GenerateImageQRCode(string textQrCode)
        {
            try
            {
                using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
                using (QRCodeData qrCodeData = qrGenerator.CreateQrCode(textQrCode, QRCodeGenerator.ECCLevel.Q))
                using (QRCode qrCode = new QRCode(qrCodeData))
                {
                    Bitmap bmp = qrCode.GetGraphic(20);
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        bmp.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                        string path = $"{Constants.Settings.DefineFolderImages}/temp/{DateTime.Now.ToString(Constants.DateTimeFormat.YyyyMMdd)}/{Guid.NewGuid().ToString()}.jpg";
                        FileHelpers.SaveFileByBytes(memoryStream.ToArray(), path);
                        string connectionApi = "";
                        try
                        {
                            connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApiforQR).Get<string>();
                        }
                        catch (Exception)
                        {
                            connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                        }
                        return $"{connectionApi}/static/{path}";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return null;
        }

        public List<Card> GetCardHFaceIdByVisitIds(List<int> ids)
        {
            List<Card> cards = new List<Card>();
            foreach (int id in ids)
            {
                var card = _unitOfWork.CardRepository.GetHFaceIdForVisitor(id);
                if (card != null)
                    cards.Add(card);
            }

            return cards;
        }

        private void ThreadPushNotifyToVisitee(Visit visit)
        {
            new Thread(() =>
            {
                PushNotifyToVisitee(visit);
            }).Start();
        }

        private async void PushNotifyToVisitee(Visit visit)
        {
            IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
            INotificationService notificationService = new NotificationService(unitOfWork, _configuration);
            bool showLogError = true;

            try
            {
                if (visit.Status != (short)VisitChangeStatusType.Approved)
                {
                    showLogError = false;
                    throw new Exception($"[PushNotifyToVisitee]: Status of visitor ({visit.VisitorName}) not approved.");
                }
                if (!visit.VisiteeId.HasValue)
                {
                    showLogError = false;
                    throw new Exception($"[PushNotifyToVisitee]: Visitee of visitor ({visit.VisitorName}) null.");
                }

                var userVisitee = unitOfWork.UserRepository.GetById(visit.VisiteeId.Value);
                if (userVisitee.IsDeleted || !userVisitee.AccountId.HasValue)
                {
                    showLogError = false;
                    throw new Exception($"[PushNotifyToVisitee]: Visitee of visitor invalid.");
                }

                var accountVisitee = unitOfWork.AccountRepository.GetById(userVisitee.AccountId.Value);
                if (accountVisitee == null)
                {
                    showLogError = false;
                    throw new Exception($"[PushNotifyToVisitee]: Account Visitee of visitor invalid.");
                }
                var company = unitOfWork.CompanyRepository.GetById(userVisitee.CompanyId);

                var accountCultureInfo = new CultureInfo(accountVisitee.Language);
                var topic = $"{company.Code}_{accountVisitee.Id}";
                DateTime startTime = visit.StartDate.ConvertToUserTime(accountVisitee.TimeZone);
                DateTime endTime = visit.EndDate.ConvertToUserTime(accountVisitee.TimeZone);
                string dateFormat = ApplicationVariables.Configuration[Constants.DateTimeWithoutSecServerFormat + ":" + accountVisitee.Language];
                if (string.IsNullOrEmpty(dateFormat))
                {
                    dateFormat = Constants.DateTimeFormat.MmDdYyyyHHmm;
                }
                string time = "";
                if (startTime.Date == endTime.Date)
                {
                    time = $"{startTime.ToString(dateFormat)} - {endTime.ToString(Constants.DateTimeFormat.HHmm)}";
                }
                else
                {
                    time = $"{startTime.ToString(dateFormat)} - {endTime.ToString(dateFormat)}";
                }
                var fbMessage = new Message()
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = VisitResource.ResourceManager.GetString("titlePushNotificationToVisitee", accountCultureInfo),
                        Body = string.Format(VisitResource.ResourceManager.GetString("bodyPushNotificationToVisitee", accountCultureInfo), visit.VisitorName, time),
                    },
                    Topic = topic,
                    Data = new Dictionary<string, string>()
                    {
                        {"type", "info"},
                    }
                };

                var messaging = FirebaseMessaging.DefaultInstance;
                var res = await messaging.SendAsync(fbMessage);

                notificationService.AddNotification(new DeMasterProCloud.DataAccess.Models.Notification()
                {
                    CompanyId = userVisitee.CompanyId,
                    Type = (short)NotificationType.NotificationAccess,
                    CreatedOn = DateTime.UtcNow,
                    Status = false,
                    ReceiveId = accountVisitee.Id,
                    Content = fbMessage.Notification.Body,
                    RelatedUrl = $"/visit?id={visit.Id}"
                }, (short)NotificationType.NotificationVisit);

            }
            catch (Exception ex)
            {
                if (showLogError)
                {
                    _logger.LogError(ex.Message + ex.StackTrace);
                }
                else
                {
                    _logger.LogWarning(ex.Message);
                }
            }

            unitOfWork.Dispose();
        }

        private void SendUpdateVisitsToAllDoors(List<Visit> visits, string sender, bool isAddUser, List<Card> cards = null)
        {
            foreach (var visit in visits)
            {
                var companyId = visit.CompanyId;
                var company = _unitOfWork.CompanyRepository.GetById(companyId);
                IWebSocketService webSocketService = new WebSocketService();
                var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, webSocketService);
                List<int> cardIdFilters = null;
                if (cards != null)
                {
                    cardIdFilters = cards.Where(m => m.VisitId == visit.Id).Select(m => m.Id).ToList();
                }

                var agDevices = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, visit.AccessGroupId);
                if (agDevices != null && agDevices.Any())
                {
                    var devicesInAg = agDevices.Select(m => m.Icu).ToList();
                    if (company != null && !company.AutoSyncUserData)
                        devicesInAg = devicesInAg.Where(m => m.ConnectionStatus == (short)ConnectionStatus.Online).ToList();
                    
                    List<int> deviceIds = new List<int>();
                    foreach (var agDevice in agDevices)
                    {
                        var device = devicesInAg.FirstOrDefault(m => m.Id == agDevice.IcuId);
                        if (device == null) continue;

                        deviceIds.Add(device.Id);
                    }

                    deviceIds = deviceIds.Distinct().ToList();
                    deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                    {
                        DeviceIds = deviceIds,
                        MessageType = isAddUser ? Constants.Protocol.AddUser : Constants.Protocol.DeleteUser,
                        MsgId = Guid.NewGuid().ToString(),
                        Sender = sender,
                        VisitIds = new List<int>() { visit.Id },
                        // CardIds = cardIdFilters,
                        CardFilterIds = cardIdFilters,
                        CompanyCode = company?.Code,
                    });
                }
            }
        }
        /// <summary>
        /// Send visits to all devices.
        /// </summary>
        private void SendVisitsToAllDoors(List<int> visitIds, bool isAddUser)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var company = _unitOfWork.CompanyRepository.GetById(companyId);
            var devices = _unitOfWork.IcuDeviceRepository.GetDeviceAllInfoByCompany(companyId).ToList();
            if (company != null && !company.AutoSyncUserData)
                devices = devices.Where(m => m.ConnectionStatus == (short)ConnectionStatus.Online).ToList();

            var groupMsgId = Guid.NewGuid().ToString();
            foreach (var device in devices)
            {
                var userLogs = _accessGroupService.MakeUserLogData(device, new List<int>(), visitIds, !isAddUser);
                _accessGroupService.SendAddOrDeleteUser(device.DeviceAddress, userLogs: userLogs, isAddUser: isAddUser);
            }
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
                    if (columnCount != templateHeaders.Length)
                    {
                        return new ResultImported()
                        {
                            Result = false,
                            Message = string.Format(MessageResource.msgFileIncorrectFormatColumns, templateHeaders.Length, columnCount)
                        };
                    }

                    // Validate actual header values match expected headers
                    for (int col = 1; col <= columnCount; col++)
                    {
                        var actualHeader = Convert.ToString(worksheet.Cells[1, col].Value ?? "").Trim();
                        var expectedHeader = templateHeaders[col - 1].Trim();

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
                    Message = MessageResource.msgFileIncorrectFormat
                };
            }
        }

        public async Task<ResultImported> ImportFile(string type, MemoryStream stream, int companyId, int accountId, string accountName)
        {
            var data = new List<VisitorImportModel>();
            // List<ExcelCardData> cardData = new List<ExcelCardData>();
            try
            {
                using (var package = new ExcelPackage(stream))
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
                        throw new InvalidFormatException();
                    }

                    var accessGroupVisitSettingId = _unitOfWork.VisitRepository.GetVisitSetting(companyId)?.AccessGroupId;
                    if (accessGroupVisitSettingId == null)
                    {
                        accessGroupVisitSettingId = _unitOfWork.AccessGroupRepository.GetDefaultAccessGroup(companyId).CompanyId;
                    }

                    for (int i = worksheet.Dimension.Start.Row + 1; i <= worksheet.Dimension.End.Row; i++)
                    {
                        var cells = worksheet.Cells;
                        var name = Convert.ToString(cells[i, Array.IndexOf(_header, VisitResource.lblVisitName) + 1].Value ?? "");

                        // Ignore row that doesnt have name
                        if (string.IsNullOrEmpty(name))
                            continue;

                        var item = ReadDataFromExcel(worksheet, i);
                        item.SetAccessGroupId($"{accessGroupVisitSettingId}");
                        data.Add(item);
                    }
                }
                var result = Import(data, companyId, accountId, accountName);
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}:{Environment.NewLine} {e.StackTrace}");
                throw;
            }
        }

        public ResultImported Import(List<VisitorImportModel> listImportVisitors, int companyId, int accountId, string accountName, string groupId = null)
        {
            List<Visit> visitorHanet = new List<Visit>();
            Visit visit = null;
            VisitHistory logVisitHistory = null;
            var count = 0;
            int addCount = 0, updateCount = 0;
            var notificationService = new NotificationService(null, null);
            bool isAdd = true;
            int totalCount = listImportVisitors.Count;
            var start = DateTime.Now;
            try
            {
                var currentAccount = _unitOfWork.AccountRepository.Get(m => m.Id == accountId && !m.IsDeleted);
                var accountTimezone = currentAccount.TimeZone;
                if (!string.IsNullOrEmpty(currentAccount.Language))
                {
                    var currentCulture = new CultureInfo(currentAccount.Language);
                    Thread.CurrentThread.CurrentUICulture = currentCulture;
                }

                var visitSetting = GetVisitSettingCompany();
                var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

                foreach (var visitData in listImportVisitors)
                {
                    _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
                    {
                        using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                        {
                            try
                            {
                                if (!string.IsNullOrEmpty(currentAccount.Language))
                                {
                                    var currentCulture = new CultureInfo(currentAccount.Language);
                                    Thread.CurrentThread.CurrentUICulture = currentCulture;
                                }

                                visit = _mapper.Map<Visit>(visitData);
                                visit.StartDate = visit.StartDate.ConvertToSystemTime(offSet);
                                visit.EndDate = visit.EndDate.ConvertToSystemTime(offSet);
                                visit.VisitType = $"{(short)VisitType.OutSider}";

                                if (visitData.VisitorName.Equals(Constants.Settings.PinpadVisitorName) &&
                                    !string.IsNullOrEmpty(visitData.Doors.Value))
                                {
                                    var doorIds = JsonConvert.DeserializeObject<List<int>>(visitData.Doors.Value);

                                    var deviceAddress = _unitOfWork.IcuDeviceRepository.GetByIds(doorIds)
                                        .Select(m => m.DeviceAddress);

                                    visit.VisitorName += string.Join(",", deviceAddress);

                                    var deviceVisitor = _unitOfWork.AppDbContext.Visit.Where(m =>
                                        m.VisitorName.Equals(visit.VisitorName) && !m.IsDeleted);

                                    if (deviceVisitor.Any())
                                    {
                                        List<int> visitIds = deviceVisitor.Select(x => x.Id).ToList();

                                        _unitOfWork.AppDbContext.Card.Where(m =>
                                            m.VisitId.HasValue && visitIds.Contains(m.VisitId.Value)).Delete();

                                        deviceVisitor.Delete();
                                    }
                                }

                                // DateTime.TryParse(visitData.BirthDay.PreValue, out var birthDay);
                                // visit.BirthDay = birthDay;
                                var approvalUser =
                                    _unitOfWork.AccountRepository.GetByIdAndCompanyId(companyId,
                                        visitData.ApproverId1?.Value ?? 0);
                                visit.ApproverId1 = approvalUser?.Id ?? accountId;
                                visit.ApplyDate = DateTime.UtcNow;
                                visit.CompanyId = companyId;
                                visit.IsDeleted = false;
                                visit.CreatedBy = accountId;
                                visit.Status = (short)VisitChangeStatusType.Waiting;

                                // Add group Id information.
                                if (!string.IsNullOrEmpty(groupId))
                                {
                                    visit.GroupId = groupId;
                                }

                                _unitOfWork.VisitRepository.Add(visit);
                                _unitOfWork.Save();
                                addCount++;
                                
                                isAdd = true;
                                count += 1;
                                addCount += 1;

                                var accessGroup = new AccessGroup
                                {
                                    Name = Constants.Settings.NameAccessGroupVisitor + visit.Id,
                                    CompanyId = companyId,
                                    Type = (short)AccessGroupType.VisitAccess
                                };
                                _unitOfWork.AccessGroupRepository.Add(accessGroup);
                                _unitOfWork.Save();

                                visit.AccessGroupId = accessGroup.Id;
                                _unitOfWork.VisitRepository.Update(visit);
                                _unitOfWork.Save();

                                logVisitHistory = new VisitHistory
                                {
                                    VisitorId = visit.Id,
                                    CompanyId = companyId,
                                    OldStatus = null,
                                    NewStatus = visit.Status,
                                    UpdatedBy = accountId,
                                    CreatedOn = DateTime.UtcNow
                                };
                                _unitOfWork.AppDbContext.VisitHistory.Add(logVisitHistory);
                                _unitOfWork.Save();
                                
                                if (count % 100 == 0)
                                {
                                    notificationService.SendMessage(Constants.MessageType.Success,
                                        Constants.NotificationType.TransmitDataSuccess, accountName,
                                        string.Format(MessageResource.msgUserImporting3, count, totalCount), companyId);
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
                    
                    var addVisit1 = DateTime.Now;

                    // Assign list doors
                    AssignDoors(visit.AccessGroupId, visitData.Doors.Value, visitSetting.AccessGroupId, companyId);

                    if (visitData.CardId != null)
                    {
                        int cardType = visit.VisitorName.Contains(Constants.Settings.PinpadVisitorName)
                            ? (int)CardType.PassCode
                            : (int)CardType.NFC;

                        // Create a new card for visitor
                        CardModel newCard = new CardModel
                        {
                            CardId = visitData.CardId.Value,
                            CardType = cardType,
                            CardStatus = (short)CardStatus.Normal,
                        };

                        AssignedCardVisitor(visit.Id, newCard);
                    }

                    Console.WriteLine("[Assign Door Visit: ]       {0}",
                        DateTime.Now.Subtract(addVisit1).TotalMilliseconds);
                    // send count review
                    if (visitSetting.ApprovalStepNumber != (short)VisitSettingType.NoStep && visit.ApproverId1 != 0)
                    {
                        _accountService.SendCountReviewToFe(visit.ApproverId1, companyId);
                    }

                    var addVisit = DateTime.Now;
                    if (visitSetting.ApprovalStepNumber == (short)VisitSettingType.NoStep)
                    {
                        // UpdateApprovalVisitor(visit.Id, (short)VisitChangeStatusType.Approved);
                        var originVisitor = _unitOfWork.VisitRepository.GetById(visit.Id);

                        var visitors = _unitOfWork.VisitRepository.GetByGroupId(originVisitor.GroupId);

                        if (visitors == null || !visitors.Any())
                        {
                            visitors = new List<Visit>() { originVisitor };
                        }
                        else
                        {
                            visitors = visitors.Where(m => m.Status == originVisitor.Status).ToList();
                        }

                        foreach (var visitor in visitors)
                        {
                            ApprovalVisit(visitor, visit, (short)VisitChangeStatusType.Approved, visitSetting,
                                accountId, visitorHanet);
                        }

                        // Pushing to visitee
                        ThreadPushNotifyToVisitee(visit);

                        // send cards to device
                        var devices = _unitOfWork.AccessGroupDeviceRepository
                            .GetByAccessGroupId(companyId, visit.AccessGroupId)
                            .Select(x => x.Icu).ToList();
                        var card = _unitOfWork.CardRepository.GetByVisitId(companyId, visit.Id).Select(x => x.Id)
                            .ToList();
                        if (devices.Count > 0)
                        {
                            ThreadSendCardToDevice(card, new List<int> { visit.Id }, devices, accountName);
                        }
                    }

                    Console.WriteLine("[UpdateApprovalVisitor: ]       {0}", DateTime.Now.Subtract(addVisit).TotalMilliseconds);
                }

                // send count review
                _accountService.SendCountReviewToFe(accountId, companyId);
            }
            catch (Exception e)
            {
                return new ResultImported
                {
                    Result = false,
                    Message = String.Format(MessageResource.ImportError, addCount + updateCount, totalCount - addCount - updateCount)
                };
            }

            Console.WriteLine("[Import Visit: ]       {0}", DateTime.Now.Subtract(start).TotalMilliseconds);
            if (addCount == totalCount)
            {
                notificationService.SendMessage(Constants.MessageType.Success,
                    Constants.NotificationType.TransmitDataSuccess, accountName,
                    String.Format(MessageResource.ImportSuccess, addCount, updateCount), companyId);
                return new ResultImported
                {
                    Result = true,
                    Message = String.Format(MessageResource.ImportSuccess, addCount, updateCount)
                };
            }

            notificationService.SendMessage(Constants.MessageType.Error,
                Constants.NotificationType.TransmitDataError, accountName,
                String.Format(MessageResource.ImportError, addCount + updateCount, (totalCount - addCount - updateCount)), companyId);
            return new ResultImported
            {
                Result = false,
                Message = String.Format(MessageResource.ImportError, addCount + updateCount, (totalCount - addCount - updateCount))
            };
        }

        private VisitorImportModel ReadDataFromExcel(ExcelWorksheet worksheet, int row)
        {
            var colIndex = 1;
            var cells = worksheet.Cells;
            var model = new VisitorImportModel();

            // access GroupId
            model.SetVisitorName(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetApproverId1(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetBirthDay(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetDoors(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetEmail(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetAddress(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetStartDate(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetEndDate(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetPosition(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetPhone(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetNationalIdNumber(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetVisitReason(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetVisitType(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetVisiteeDepartment(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetVisiteeDepartmentId(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetVisiteeEmpNumber(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetVisiteeId(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetVisiteeName(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetVisiteeSite(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetVisitorDepartment(Convert.ToString(cells[row, colIndex++].Value).Trim());
            model.SetVisitorEmpNumber(Convert.ToString(cells[row, colIndex++].Value).Trim());

            return model;
        }

        public byte[] GetFileExcelImportVisitTemplate(int companyId)
        {
            using (var package = new ExcelPackage())
            {
                #region add worksheet data visit template
                var worksheet = package.Workbook.Worksheets.Add(VisitResource.lblVisit);

                // add the headers for book sheet
                worksheet.Row(1).Style.Font.Bold = true;
                for (int i = 0; i < templateHeaders.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = templateHeaders[i];
                }

                // add data sample
                var recordIndex = 2;
                for (int i = 1; i <= 3; i++)
                {
                    var colIndex = 1;
                    worksheet.Cells[recordIndex, colIndex++].Value = $"{VisitResource.lblVisitName}{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"10.05.1998";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"[{i},{i + 2},{i + 3}]";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"test{i}@duali.com";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"{VisitResource.lblAddress}{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = (DateTime.Now.Date + new TimeSpan(9, 0, 0)).ToString(Constants.DateTimeFormatDefault);
                    worksheet.Cells[recordIndex, colIndex++].Value = (DateTime.Now.Date + new TimeSpan(18, 0, 0)).ToString(Constants.DateTimeFormatDefault);
                    worksheet.Cells[recordIndex, colIndex++].Value = $"{VisitResource.lblPosition}{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = "0977666222";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"{i}{i + 1}{i + 2}";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"{VisitResource.lblReason}{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"VisiteeDepartment{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"VisiteeName{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"VisiteeSite{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"VisitorDepartment{i}";
                    worksheet.Cells[recordIndex, colIndex++].Value = $"{i}";

                    recordIndex++;
                }
                worksheet.Cells.AutoFitColumns();
                #endregion

                #region add worksheet define data
                var worksheet2 = package.Workbook.Worksheets.Add("Define Data Map");

                // add the headers for book sheet
                worksheet2.Row(1).Style.Font.Bold = true;
                for (int i = 0; i < templateHeaders2.Length; i++)
                {
                    worksheet2.Cells[1, i + 1].Value = templateHeaders2[i];
                }
                // add data approver
                var approvers = _unitOfWork.AccountRepository.GetAccountsByCompany(companyId);
                recordIndex = 2;
                foreach (var item in approvers)
                {
                    var colIndex = 1;
                    worksheet2.Cells[recordIndex, colIndex++].Value = $"{item.Id}";
                    worksheet2.Cells[recordIndex, colIndex].Value = $"{item.Username}";
                    recordIndex++;
                }

                //add data Door
                var doors = _unitOfWork.IcuDeviceRepository.GetDoors(companyId);
                recordIndex = 2;
                foreach (var item in doors)
                {
                    var colIndex = 3;
                    worksheet2.Cells[recordIndex, colIndex++].Value = $"{item.Id}";
                    worksheet2.Cells[recordIndex, colIndex].Value = $"{item.Name}";
                    recordIndex++;
                }

                //add data visit Type
                recordIndex = 2;
                worksheet2.Cells[recordIndex++, 5].Value = $"Insider = 0";
                worksheet2.Cells[recordIndex, 5].Value = $"OutSider = 1";

                //add data department 
                var departments = _unitOfWork.DepartmentRepository.GetByCompanyId(companyId);
                recordIndex = 2;
                foreach (var item in departments)
                {
                    var colIndex = 6;
                    worksheet2.Cells[recordIndex, colIndex++].Value = $"{item.Id}";
                    worksheet2.Cells[recordIndex, colIndex].Value = $"{item.DepartName}";
                    recordIndex++;
                }
                //add data visitee 
                var visitees = _unitOfWork.UserRepository.GetByCompanyId(companyId);
                recordIndex = 2;
                foreach (var item in visitees)
                {
                    var colIndex = 8;
                    worksheet2.Cells[recordIndex, colIndex++].Value = $"{item.Id}";
                    worksheet2.Cells[recordIndex, colIndex].Value = $"{item.FirstName}";
                    recordIndex++;
                }
                worksheet2.Cells.AutoFitColumns();
                #endregion

                return package.GetAsByteArray();
            }
        }

        public Dictionary<bool, List<string>> ImportMultiVisitors(List<ImportMultiVisitModel> models, int companyId)
        {
            var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);
            var accessGroupDefault = _unitOfWork.AccessGroupRepository.GetById(visitSetting.AccessGroupId);
            Dictionary<int, List<int>> agAndCardsAddToDevice = new Dictionary<int, List<int>>();
            Dictionary<int, List<int>> agAndCardsDeleteToDevice = new Dictionary<int, List<int>>();
            Dictionary<bool, List<string>> result = new Dictionary<bool, List<string>>();
            result.Add(false, new List<string>());

            // check point company
            var visitCount = 0;
            foreach (var model in models)
            {
                if (model.Action.ToLower() == "delete")
                {
                }
                else
                {
                    // check card already exist
                    Card card = _unitOfWork.CardRepository.GetByCardId(companyId, model.Barcode);
                    if (card == null)
                    {
                        visitCount++;
                    }
                }

            }

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var model in models)
                        {
                            if (model.Action.ToLower() == "delete")
                            {
                                // set endDate by now
                                var card = _unitOfWork.CardRepository.GetByCardId(companyId, model.Barcode);
                                if (card?.VisitId != null)
                                {
                                    var visit = _unitOfWork.VisitRepository.GetById(card.VisitId.Value);
                                    if (visit != null)
                                    {
                                        // set end date of visit
                                        short oldStatus = visit.Status;
                                        visit.EndDate = DateTime.UtcNow;
                                        visit.Status = (short)VisitChangeStatusType.Finished;
                                        _unitOfWork.VisitRepository.Update(visit);
                                        _unitOfWork.Save();
                                        VisitHistory logVisitHistory = new VisitHistory
                                        {
                                            VisitorId = visit.Id,
                                            CompanyId = companyId,
                                            OldStatus = oldStatus,
                                            NewStatus = visit.Status,
                                            CreatedOn = DateTime.UtcNow,
                                            UpdatedOn = DateTime.Now,
                                        };
                                        _unitOfWork.AppDbContext.VisitHistory.Add(logVisitHistory);
                                        _unitOfWork.Save();
                                        // delete card
                                        _unitOfWork.CardRepository.DeleteFromSystem(card);
                                        _unitOfWork.Save();

                                        // send delete card
                                        var accessGroupModel = _unitOfWork.AccessGroupRepository.GetByNameAndCompanyId(companyId, model.Name);
                                        int accessGroupDeleteId = accessGroupModel?.Id ?? visit.AccessGroupId;
                                        if (!agAndCardsDeleteToDevice.Keys.Contains(accessGroupDeleteId))
                                        {
                                            agAndCardsDeleteToDevice.Add(accessGroupDeleteId, new List<int>());
                                        }
                                        agAndCardsDeleteToDevice[accessGroupDeleteId].Add(card.Id);
                                    }
                                }
                            }
                            else
                            {
                                // check card already exist
                                Card card = _unitOfWork.CardRepository.GetByCardId(companyId, model.Barcode);
                                if (card == null)
                                {
                                    // add visit
                                    Visit visit = new Visit()
                                    {
                                        VisitorName = model.Name,
                                        AccessGroupId = visitSetting.AccessGroupId,
                                        StartDate = model.ValidDateFrom.ConvertDefaultStringToDateTime() ?? DateTime.UtcNow,
                                        EndDate = model.ValidDateTo.ConvertDefaultStringToDateTime() ?? DateTime.MaxValue,
                                        Status = (short)VisitChangeStatusType.Approved,
                                        CompanyId = companyId,
                                    };
                                    _unitOfWork.VisitRepository.Add(visit);
                                    _unitOfWork.Save();
                                    // add access group visit
                                    AccessGroup accessGroupForVisit = new AccessGroup
                                    {
                                        Name = Constants.Settings.NameAccessGroupVisitor + visit.Id,
                                        CompanyId = companyId,
                                        Type = (short)AccessGroupType.VisitAccess
                                    };
                                    _unitOfWork.AccessGroupRepository.Add(accessGroupForVisit);
                                    _unitOfWork.Save();
                                    // update access group for visit
                                    visit.AccessGroupId = accessGroupForVisit.Id;
                                    _unitOfWork.VisitRepository.Update(visit);
                                    _unitOfWork.Save();

                                    // add access group device for visit
                                    List<int> accessGroupAddIds = new List<int>();
                                    if (model.AccessGroupNames == null || !model.AccessGroupNames.Any())
                                    {
                                        model.AccessGroupNames = new List<string>() { accessGroupDefault.Name };
                                    }
                                    foreach (var itemAccessGroupName in model.AccessGroupNames)
                                    {
                                        var accessGroupModel = _unitOfWork.AccessGroupRepository.GetByNameAndCompanyId(companyId, itemAccessGroupName);
                                        if (accessGroupModel != null)
                                        {
                                            var accessGroupDevices = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, accessGroupModel.Id);
                                            foreach (var item in accessGroupDevices)
                                            {
                                                var oldAgd = _unitOfWork.AppDbContext.AccessGroupDevice.FirstOrDefault(m =>
                                                        m.AccessGroupId == visit.AccessGroupId && m.IcuId == item.IcuId && m.TzId == item.TzId);
                                                if (oldAgd != null)
                                                {
                                                    List<int> oldFloorIds = new List<int>();
                                                    List<int> newFloorIds = new List<int>();
                                                    try
                                                    {
                                                        oldFloorIds = string.IsNullOrEmpty(oldAgd.FloorIds)
                                                            ? new List<int>()
                                                            : JsonConvert.DeserializeObject<List<int>>(oldAgd.FloorIds) ?? new List<int>();
                                                    }
                                                    catch
                                                    {
                                                        oldFloorIds = new List<int>();
                                                    }
                                                    try
                                                    {
                                                        newFloorIds = string.IsNullOrEmpty(item.FloorIds)
                                                            ? new List<int>()
                                                            : JsonConvert.DeserializeObject<List<int>>(item.FloorIds) ?? new List<int>();
                                                    }
                                                    catch
                                                    {
                                                        newFloorIds = new List<int>();
                                                    }

                                                    if (oldFloorIds.Count > 0)
                                                    {
                                                        newFloorIds.AddRange(oldFloorIds);
                                                    }

                                                    newFloorIds = newFloorIds.Distinct().ToList();
                                                    oldAgd.FloorIds = JsonConvert.SerializeObject(newFloorIds);
                                                    _unitOfWork.AccessGroupDeviceRepository.Update(oldAgd);
                                                    _unitOfWork.Save();
                                                }
                                                else
                                                {
                                                    AccessGroupDevice agd = _mapper.Map<AccessGroupDevice>(item);
                                                    agd.AccessGroupId = visit.AccessGroupId;
                                                    _unitOfWork.AccessGroupDeviceRepository.Add(agd);
                                                    _unitOfWork.Save();
                                                }
                                            }

                                            accessGroupAddIds.Add(accessGroupModel.Id);
                                        }
                                    }

                                    // add log of visit
                                    VisitHistory logVisitHistory = new VisitHistory
                                    {
                                        VisitorId = visit.Id,
                                        CompanyId = companyId,
                                        OldStatus = null,
                                        NewStatus = visit.Status,
                                        CreatedOn = DateTime.UtcNow,
                                        UpdatedOn = DateTime.UtcNow,
                                    };
                                    _unitOfWork.AppDbContext.VisitHistory.Add(logVisitHistory);
                                    _unitOfWork.Save();
                                    // add card for visit
                                    card = new Card
                                    {
                                        CardId = model.Barcode,
                                        IssueCount = 0,
                                        CompanyId = companyId,
                                        VisitId = visit.Id,
                                        CardType = (short)CardType.QrCode,
                                        ValidFrom = visit.StartDate,
                                        ValidTo = visit.EndDate
                                    };

                                    _unitOfWork.CardRepository.Add(card);
                                    _unitOfWork.Save();

                                    // send add card
                                    foreach (var accessGroupAddId in accessGroupAddIds)
                                    {
                                        if (!agAndCardsAddToDevice.Keys.Contains(accessGroupAddId))
                                        {
                                            agAndCardsAddToDevice.Add(accessGroupAddId, new List<int>());
                                        }
                                        agAndCardsAddToDevice[accessGroupAddId].Add(card.Id);
                                    }
                                }
                                else
                                {
                                    result[false].Add(string.Format(MessageResource.Exist, string.Concat(CardIssuingResource.lblCardId, ": ", model.Barcode)));
                                }

                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        transaction.Rollback();
                        result[false].Add(ex.Message);
                    }
                }
            });

            // thread send card to device
            // send delete
            foreach (var key in agAndCardsDeleteToDevice.Keys)
            {
                var devices = _unitOfWork.AccessGroupRepository.GetDevicesByAccessGroupId(key);
                if (devices.Any())
                {
                    ThreadSendCardToDevice(agAndCardsDeleteToDevice[key], null, devices, "", Constants.Protocol.DeleteUser);
                }
            }
            // send add
            foreach (var key in agAndCardsAddToDevice.Keys)
            {
                var devices = _unitOfWork.AccessGroupRepository.GetDevicesByAccessGroupId(key);
                if (devices.Any())
                {
                    ThreadSendCardToDevice(agAndCardsAddToDevice[key], null, devices, "", Constants.Protocol.AddUser);
                }
            }
            
            return result;
        }
    }
}