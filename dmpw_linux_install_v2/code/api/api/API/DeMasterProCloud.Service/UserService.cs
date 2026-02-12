using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Bogus;
using Bogus.Extensions;
using ClosedXML.Excel;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Exceptions;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.AccessGroup;
using DeMasterProCloud.DataModel.Account;
using DeMasterProCloud.DataModel.Card;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.EventLog;
using DeMasterProCloud.DataModel.Header;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.DataModel.Timezone;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.DataModel.WorkingModel;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Service.Infrastructure.Header;
using DeMasterProCloud.Service.Protocol;
using DeMasterProCloud.Service.RabbitMqQueue;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace DeMasterProCloud.Service
{
    /// <summary>
    /// User Interface 
    /// </summary>
    public interface IUserService
    {
        Dictionary<string, object> GetInit(int companyId);
        byte[] Export(string type, UserFilterModel filter, out int totalRecords, out int recordsFiltered);
        byte[] ExportUserData(string type, UserFilterModel filter, string pageName, out int totalRecords, out int recordsFiltered);
        byte[] ExportAccessibleDoors(User user, string type, string search, out int totalRecords, out int recordsFiltered, out string fileName, string sortColumn = "Id", string sortDirection = "desc");

        IEnumerable<UserListModel> GetPaginated(UserFilterModel filter, out int totalRecords, out int recordsFiltered, out List<HeaderData> userHeader, string pageName, int departmentId = 0, bool isAssign = true, string dateFormat = Constants.DateTimeFormatDefault, TimeSpan offSet = new TimeSpan());

        IEnumerable<UserModel> GetPaginatedReturnUserModel(UserFilterModel filter, out int totalRecords, out int recordsFiltered);

        //IEnumerable<UserListModel> GetPaginatedUnAssignUser(string filter, int pageNumber, int pageSize, string sortColumn,
        //    string sortDirection, out int totalRecords, out int recordsFiltered, /*out List<HeaderData> userHeader,*/ List<int> isValid = null, int departmentId = 0);
        List<AccessibleDoorModel> GetPaginatedAccessibleDoors(User user, string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        IQueryable<UserListModel> FilterDataWithOrderToList(UserFilterModel filter,
            out int totalRecords, out int recordsFiltered, out List<HeaderData> userHeader, string pageName,
            int departmentId = 0, bool isAssign = true, string dateFormat = Constants.DateTimeFormatDefault, TimeSpan offSet = new TimeSpan());

        void InitData(UserDataModel userModel);
        int Add(UserModel userModel, bool isSend = true);
        User GetById(int id);
        User GetByIdAndCompany(int id, int companyId);
        void Update(UserModel userModel);
        List<int> UpdateMultiple(List<UserModel> userModels);

        Card AddIdentification(User user, CardModel cardModel, bool isSend = true, bool sendAsync = true);
        bool Delete(User user, out string message);
        bool DeleteRange(List<User> users, out string message);
        List<User> GetByIdsAndCompany(List<int> idArr, int companyId);
        Task<ResultImported> ImportFile(string type, MemoryStream stream, int companyId, int accountId, string accountName);
        ResultImported ValidateImportFileHeaders(IFormFile file);
        bool IsCardIdExist(UserModel model);
        bool IsCardExist(short type, int userId);
        bool IsCardIdExist(string cardId);

        //Card AddCardByUser(int userId, CardModel model);

        void DeleteCardByUser(User user, int cardIndex);

        void UpdateCardByUser(int userId, int cardId, CardModel model);
        bool IsKeyPadPasswordExist(int userId, string enctypedKeyPadPassword);
        void GenerateTestData(int numberOfUser);
        void GenerateTestFaceData(int numberOfUser, bool isAccount = false);
        User GetByCardId(string cardId);
        Card GetCardByUser(int userId, int cardId);

        bool IsExistGetCardByUser(int userId, int cardId);
        List<CardModel> GetCardListByUserId(int userId);

        bool IsAccountExist(int id, string username, int? companyId = null);
        Account GetAccountByEmail(string username, int? companyId = null);
        Account GetAccountById(int userId);

        DynamicQr GetDynamicQrCode(int userId, string qrId);

        int GetCardCount(int accessGroupId);

        void AssignUserToDefaultWorkingTime();
        //void SendVerifyAddAccountMailForUser(string email, string token);

        Card GetNFCPhoneIdByUserId(int userId);
        Account GetAccountByUserName(string userName);
        User GetUserByLinkedAccount(int accountId, int companyId);

        bool IsEmailValid(string emailAddress);

        User GetUserByAccountId(int accountId, int companyId);
        User GetUserByEmail(string email, int companyId);
        User GetUserByPhone(string phone, int companyId);

        string ValidationDynamicQr(string dynamicQr);

        bool GetDeviceFromCompany(string rid);
        Card GetQrByUserId(int userId);
        bool IsDuplicatedAccountCreated(int userId, string email);
        bool IsDuplicatedUserCode(int id, string userCode, int? companyId);

        void SendUsersToAllDoors(List<int> userIds, bool isAddUser);
        User GetUserByUserId(int userId);
        List<User> GetUsersByUserIds(List<int> userIds);
        int GetTotalUserByCompany(int companyId);
        List<User> GetUsersByCompany(int companyId);

        List<int> AddMultiple(List<UserModel> userModels, bool isSend = true);

        bool CheckPermissionEditAvatar(int companyId);

        AccessSettingModel GetAccessSettingByCompany();

        void UpdateAccessSettingCompany(AccessSettingModel model);
        void UpdateApprovalUser(int id, short status);

        bool ExistNotApprovedUser();

        bool CanEditData(User user);

        int GetUsersReviewCount();


        void SendDeleteUsersToAllDevice(List<int> userIds, int companyId, List<int> cardTypeList = null);

        IEnumerable<UserListSimpleModel> GetUsersByUserCodes(List<string> userCodes = null);
        IEnumerable<UserListSimpleModel> GetUsersByConditions(UserGetConditionModel conditions);

        List<int> RemoveQR();
        List<int> ConvertExpiredUserInvalid();
        void UpdateAvatar(int userId, string avatar);
        List<Card> GetCardHFaceIdByUserIds(List<int> ids);
        RegisterUserInitModel GetRegisterUserInit(Company company);
        User GetByNationalIdNumber(string idNumber, int companyId);
        ResultRegisterUser RegisterUser(RegisterUserModel model, Company company);

        List<UserInOutStatusListModel> GetInOutStatus(List<EventLogByWorkType> data, string search, int pageNumber, int pageSize, string sortColumn, string sortDirection, out int total, out int filtered);
        Dictionary<string, object> UpdateListAvatarUser(IFormFile file, Company company);

        void SendUpdateUsersToAllDoors(List<User> users, string sender, bool isAddUser, List<Card> cards = null);
        void UpdateAllUserCompanyToApproval(int companyId, int approvalStepNumber);
    }

    /// <summary>
    /// Service provider for user
    /// </summary>
    public class UserService : IUserService
    {
        /// <summary>
        /// String array display in header sheet when export file
        /// </summary>
        /// 


        private readonly string[] _header =
        {
            UserResource.lblUserCode,
            //UserResource.lblAccessGroup,
            UserResource.lblIsMasterCard,
            UserResource.lblCardStatus,
            UserResource.lblCardType,
            UserResource.lblCardId,
            UserResource.lblFirstName,
            UserResource.lblLastName,
            UserResource.lblSex,
            UserResource.lblEmail,
            UserResource.lblBirthday,
            //UserResource.lblIssuedDate,
            UserResource.lblEffectiveDate,
            UserResource.lblExpiredDate,
            UserResource.lblDepartmentName,
            //UserResource.lblDepartmentNumber,
            UserResource.lblEmployeeNumber,
            UserResource.lblPosition,
            //UserResource.lblKeyPadPassword,
            UserResource.lblPostCode,
            UserResource.lblJob,
            UserResource.lblResponsibility,
            UserResource.lblCompanyPhone,
            UserResource.lblHomePhone,
            UserResource.lblAddress,
            UserResource.lblNationality,
            UserResource.lblCity,
            UserResource.lblRemarks,
            UserResource.lblParentDepartment,
            UserResource.lblTypeOfWork,

        };
        private readonly string[] _headerExcel =
        {
            UserResource.lblUserCode,
            //UserResource.lblAccessGroup,
            // UserResource.lblIsMasterCard,
            UserResource.lblCardStatus,
            UserResource.lblIssueCount,
            UserResource.lblCardType,
            UserResource.lblCardId,
            UserResource.lblFirstName,
            // UserResource.lblLastName,
            UserResource.lblSex,
            UserResource.lblEmail,
            UserResource.lblBirthday,
            //UserResource.lblIssuedDate,
            UserResource.lblEffectiveDate,
            UserResource.lblExpiredDate,
            UserResource.lblDepartmentName,
            //UserResource.lblDepartmentNumber,
            UserResource.lblEmployeeNumber,
            UserResource.lblPosition,
            //UserResource.lblKeyPadPassword,
            // UserResource.lblPostCode,
            // UserResource.lblJob,
            // UserResource.lblResponsibility,
            // UserResource.lblCompanyPhone,
            UserResource.lblHomePhone,
            // UserResource.lblAddress,
            // UserResource.lblNationality,
            // UserResource.lblCity,
            // UserResource.lblRemarks,
            // UserResource.lblParentDepartment,
            UserResource.lblTypeOfWork,

        };

        /// <summary>
        /// String array display in header sheet when export file
        /// </summary>
        private readonly string[] _headerForAccessibleDoor =
        {
            DeviceResource.lblIndex,
            DeviceResource.lblDoorName,
            DeviceResource.lblRID,
            DeviceResource.lblDoorActiveTimezone,
            DeviceResource.lblDoorPassageTimezone,
            DeviceResource.lblVerifyMode,
            DeviceResource.lblAntiPassback,
            DeviceResource.lblDeviceType,
            DeviceResource.lblMpr,
         };

        // Inject dependency
        private readonly IUnitOfWork _unitOfWork;

        private readonly HttpContext _httpContext;
        //private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IAccessGroupService _accessGroupService;
        private readonly IAccountService _accountService;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor,
            IAccessGroupService accessGroupService, IConfiguration configuration, ILogger<UserService> logger,
            IAccountService accountService)
        {
            _unitOfWork = unitOfWork;
            _httpContext = contextAccessor.HttpContext;
            _accessGroupService = accessGroupService;
            _configuration = configuration;
            _logger = logger;
            _accountService = accountService;
            _mapper = MapperInstance.Mapper;
        }

        public Dictionary<string, object> GetInit(int companyId)
        {
            try
            {
                Dictionary<string, object> data = new Dictionary<string, object>();

                string newUserCode = $"{_unitOfWork.UserRepository.GetNewUserCode(companyId):000000}";
                data.Add("userCode", newUserCode);

                var checkPlugIn = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId).PlugIns;
                var PlugInsValue = JsonConvert.DeserializeObject<PlugIns>(checkPlugIn);

                // access time
                var accessTimes = _unitOfWork.AccessTimeRepository
                    .Gets(m => m.CompanyId == companyId && !m.IsDeleted)
                    .Select(m => new AccessTimeListModel
                    {
                        Id = m.Id,
                        AccessTimeName = (m.Id == 1 || m.Id == 2) ? ((DefaultTimezoneType)m.Id).GetDescription() : m.Name,
                        Remark = m.Remarks,
                        Position = m.Position
                    });
                data.Add("accessTimes", accessTimes);

                // working type
                var workingTypes = _unitOfWork.WorkingRepository
                    .Gets(m => m.CompanyId == companyId)
                    .Select(m => new WorkingListModel()
                    {
                        Id = m.Id,
                        Name = m.Name,
                        WorkingDay = m.WorkingDay,
                        IsDefault = m.IsDefault
                    });
                data.Add("workingTypes", workingTypes);

                // access setting
                var accessSetting = _unitOfWork.UserRepository.GetAccessSetting(companyId);
                data.Add("accessSetting", accessSetting != null ? _mapper.Map<AccessSettingModel>(accessSetting) : new AccessSettingModel());

                // approved account
                var approvalAccessSetting = _accountService.GetApprovalAccessSetting(companyId);
                foreach (var key in approvalAccessSetting.Keys)
                {
                    data.Add(key, approvalAccessSetting[key]);
                }

                // building tree
                // will call in user controller

                // device fingerprint
                var deviceFingerprints = _unitOfWork.IcuDeviceRepository
                    .Gets(m => !m.IsDeleted && m.CompanyId == companyId && m.DeviceType == (short)DeviceType.Biostation2)
                    .Select(m => new
                    {
                        Id = m.Id,
                        Name = m.Name,
                        DeviceAddress = m.DeviceAddress,
                    });
                data.Add("deviceFingerprints", deviceFingerprints);

                // device face
                var deviceFaces = _unitOfWork.IcuDeviceRepository
                    .Gets(m => !m.IsDeleted && m.CompanyId == companyId && m.DeviceType == (short)DeviceType.IT100)
                    .Select(m => new
                    {
                        Id = m.Id,
                        Name = m.Name,
                        DeviceAddress = m.DeviceAddress,
                    });
                data.Add("deviceFaces", deviceFaces);

                // device EBKN
                var deviceEbkn = _unitOfWork.IcuDeviceRepository
                    .Gets(m => !m.IsDeleted && m.CompanyId == companyId && m.DeviceType == (short)DeviceType.EbknReader)
                    .Select(m => new
                    {
                        Id = m.Id,
                        Name = m.Name,
                        DeviceAddress = m.DeviceAddress,
                    });
                data.Add("deviceEbkns", deviceEbkn);

                // device Dual Face
                var deviceFaceIds = new List<int>()
                {
                    (short)DeviceType.DF970,
                    (short)DeviceType.BA8300,
                    (short)DeviceType.RA08,
                    (short)DeviceType.DQ8500,
                    (short)DeviceType.DQ200,
                    (short)DeviceType.TBVision,
                    (short)DeviceType.T2Face,
                };
                bool isExitDeviceFace = _unitOfWork.IcuDeviceRepository.Count(m => !m.IsDeleted && m.CompanyId == companyId && deviceFaceIds.Contains(m.DeviceType)) > 0;
                var deviceAratek = _unitOfWork.IcuDeviceRepository.Gets(m => !m.IsDeleted && m.CompanyId == companyId &&
                                                                             (m.DeviceType == (short)DeviceType.BA8300 || m.DeviceType == (short)DeviceType.DQ8500))
                    .Select(m => new
                    {
                        Id = m.Id,
                        Name = m.Name,
                        DeviceAddress = m.DeviceAddress,
                    });
                data.Add("deviceAratek", deviceAratek);

                // device EBKN
                var deviceTBVision = _unitOfWork.IcuDeviceRepository
                    .Gets(m => !m.IsDeleted && m.CompanyId == companyId && m.DeviceType == (short)DeviceType.TBVision)
                    .Select(m => new
                    {
                        Id = m.Id,
                        Name = m.Name,
                        DeviceAddress = m.DeviceAddress,
                    });
                data.Add("deviceTBVision", deviceTBVision);

                // card type
                var cardTypes = _unitOfWork.PlugInRepository.GetListCardTypeByPlugIn(companyId);
                if (!deviceFingerprints.Any())
                {
                    cardTypes = cardTypes.Where(m => m.Id != (short)CardType.FingerPrint && m.Id != (short)CardType.BioFaceId).ToList();
                }
                if (!deviceEbkn.Any())
                {
                    cardTypes = cardTypes.Where(m => m.Id != (short)CardType.EbknFaceId && m.Id != (short)CardType.EbknFingerprint).ToList();
                }
                if (!isExitDeviceFace)
                {
                    cardTypes = cardTypes.Where(m => m.Id != (short)CardType.LFaceId && m.Id != (short)CardType.VNID).ToList();
                }
                if (!deviceAratek.Any())
                {
                    cardTypes = cardTypes.Where(m => m.Id != (short)CardType.AratekFingerPrint).ToList();
                }
                if (!deviceFaces.Any())
                {
                    cardTypes = cardTypes.Where(m => m.Id != (short)CardType.FaceId).ToList();
                }
                if (!deviceTBVision.Any())
                {
                    cardTypes = cardTypes.Where(m => m.Id != (short)CardType.TBFace).ToList();
                }
                data.Add("cardTypes", cardTypes);

                // camera
                var cameras = _unitOfWork.CameraRepository.Gets(m => m.CompanyId == companyId)
                    .Select(m => new EnumModel()
                    {
                        Id = m.Id,
                        Name = m.Name,
                    });
                data.Add("cameras", cameras);

                // department
                var departments = _unitOfWork.DepartmentRepository.GetByCompanyId(companyId)
                    .Select(m => new EnumModel()
                    {
                        Id = m.Id,
                        Name = m.DepartName
                    }).ToList();
                // check permission account type dynamic role department level
                // var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(_httpContext.User.GetCompanyId());
                // PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
                if (_httpContext.User.GetAccountType() == (short)AccountType.DynamicRole/* && plugIns.DepartmentAccessLevel*/)
                {
                    var departmentLevelIds = _unitOfWork.DepartmentRepository
                        .GetDepartmentIdsByAccountDepartmentRole(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId());

                    departments = departments.Where(d => departmentLevelIds.Contains(d.Id)).ToList();
                }
                data.Add("departments", departments);

                // genders
                data.Add("genders", EnumHelper.ToEnumList<SexType>());

                // permission type
                data.Add("permissionTypes", EnumHelper.ToEnumList<PermissionType>());

                // pass type
                data.Add("passTypes", EnumHelper.ToEnumList<PassType>());

                // user status
                data.Add("userStatus", EnumHelper.ToEnumList<Status>());

                // work type
                var workTypes = EnumHelper.ToEnumList<WorkType>();
                var setting = _unitOfWork.SettingRepository.GetByKey("work_type_default", companyId);
                if (setting != null)
                {
                    var values = JsonConvert.DeserializeObject<List<string>>(setting.Value).Select(int.Parse).ToList();
                    workTypes = workTypes.Where(d => values.Contains(d.Id)).ToList();
                }
                data.Add("workTypes", workTypes);

                // access group
                var accessGroups = _unitOfWork.AccessGroupRepository
                    .Gets(m => !m.IsDeleted && m.CompanyId == companyId &&
                               m.Type != (short)AccessGroupType.PersonalAccess &&
                               m.Type != (short)AccessGroupType.VisitAccess).AsEnumerable<AccessGroup>()
                    .Select(_mapper.Map<AccessGroupModelForUser>).ToList();
                data.Add("accessGroups", accessGroups);

                // card status
                data.Add("cardStatus", EnumHelper.ToEnumList<CardStatus>().Where(m => m.Id < 5));

                // operation type
                data.Add("operationTypes", EnumHelper.ToEnumList<OperationType>());

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetInit");
                return new Dictionary<string, object>();
            }
        }

        public void InitData(UserDataModel userModel)
        {
            try
            {

                var companyId = _httpContext.User.GetCompanyId();
                if (userModel.DepartmentId == null)
                {
                    var defaultDepartment = _unitOfWork.DepartmentRepository.GetDefautDepartmentByCompanyId(companyId);

                    var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(_httpContext.User.GetCompanyId());
                    PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
                    if (_httpContext.User.GetAccountType() == (short)AccountType.DynamicRole && plugIns.DepartmentAccessLevel)
                    {
                        var departmentLevelIds = _unitOfWork.DepartmentRepository
                            .GetDepartmentIdsByAccountDepartmentRole(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId());
                        userModel.DepartmentId = departmentLevelIds.Contains(defaultDepartment.Id) ? defaultDepartment.Id : departmentLevelIds.FirstOrDefault();
                    }
                    else
                    {
                        if (defaultDepartment != null)
                        {
                            userModel.DepartmentId = defaultDepartment.Id;
                        }
                    }


                }

                var accessGroups = _accessGroupService.GetListAccessGroupsExceptForVisitor()
                    .Select(_mapper.Map<AccessGroupModelForUser>).ToList();

                var accessGroupDefault = _unitOfWork.AccessGroupRepository.GetDefaultAccessGroup(companyId);
                if (userModel.AccessGroupId == 0 && accessGroupDefault != null)
                {
                    userModel.AccessGroupId = accessGroupDefault.Id;
                    var index = accessGroups.FindIndex(x => x.Id == accessGroupDefault.Id);
                    (accessGroups[index], accessGroups[0]) = (accessGroups[0], accessGroups[index]);
                }
                else
                {
                    if (!accessGroups.Select(m => m.Id).ToList().Contains(userModel.AccessGroupId))
                    {
                        // Add PAG
                        var personalAccessGroup = _accessGroupService.GetById(userModel.AccessGroupId);
                        var pagData = _mapper.Map<AccessGroupModelForUser>(personalAccessGroup);

                        if (pagData != null && personalAccessGroup.ParentId.HasValue)
                        {
                            personalAccessGroup = _accessGroupService.GetById(personalAccessGroup.ParentId.Value);
                            pagData.Name = personalAccessGroup.Name + " *";

                            accessGroups.Add(pagData);
                        }

                        if (pagData != null && (pagData.Type == (short)AccessGroupType.NoAccess ||
                                                pagData.Type == (short)AccessGroupType.FullAccess))
                        {
                            var noAccessGroup = _unitOfWork.AccessGroupRepository.GetNoAccessGroup(companyId);
                            var fullAccessGroup = _unitOfWork.AccessGroupRepository.GetFullAccessGroup(companyId);
                            var noAccessGroupData = _mapper.Map<AccessGroupModelForUser>(noAccessGroup);
                            var fullAccessGroupData = _mapper.Map<AccessGroupModelForUser>(fullAccessGroup);

                            accessGroups.Add(noAccessGroupData);
                            accessGroups.Add(fullAccessGroupData);
                        }
                    }
                }
                userModel.AccessGroups = accessGroups.OrderBy(m => m.Name).ToList();

                if (userModel.Id != 0)
                {
                    var cards = _unitOfWork.CardRepository.GetByUserId(companyId, userModel.Id).Where(m => m.CardStatus < (short)CardStatus.WaitingForPrinting);

                    var cardModelList = new List<CardModel>();
                    foreach (var card in cards)
                    {
                        if (card.CardType != (int)CardType.VehicleId && card.CardType != (int)CardType.VehicleMotoBikeId)
                        {
                            var cardModel = _mapper.Map<CardModel>(card);

                            if (card.CardType == (int)CardType.FaceId)
                            {
                                var face = _unitOfWork.AppDbContext.Face.Where(m => m.UserId == userModel.Id).FirstOrDefault();
                                if (face != null)
                                {
                                    cardModel.FaceData = _mapper.Map<FaceModel>(face);
                                }
                            }

                            if (card.CardType == (short)CardType.FingerPrint || card.CardType == (short)CardType.AratekFingerPrint)
                            {
                                var fingerPrint = _unitOfWork.AppDbContext.FingerPrint.Where(m => m.CardId == card.Id);
                                cardModel.FingerPrintData = fingerPrint.AsEnumerable<FingerPrint>().Select(_mapper.Map<FingerPrintModel>).ToList();
                            }

                            cardModelList.Add(cardModel);
                        }
                    }

                    userModel.CardList = cardModelList;
                }
                else//Id is 0
                {
                    var userCode = _unitOfWork.UserRepository.GetNewUserCode(companyId);
                    string newUserCode = $"{userCode:000000}";
                    while (_unitOfWork.UserRepository.GetByUserCode(companyId, newUserCode) != null)
                    {
                        userCode += 1;
                        newUserCode = $"{userCode:000000}";
                    }
                    userModel.UserCode = newUserCode;
                    userModel.WorkingTypeId = _unitOfWork.WorkingRepository.GetWorkingTypeDefault(companyId);
                }

                // avatar
                if (string.IsNullOrEmpty(userModel.Avatar))
                {
                    userModel.Avatar = Helpers.ResizeImage(userModel.Gender ? Constants.Image.DefaultMale : Constants.Image.DefaultFemale);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitData");
            }
        }


        public int GetCardCount(int accessGroupId)
        {
            try
            {

                int cardCount = 0;
                var accessGroup = _unitOfWork.AccessGroupRepository.GetByIdAndCompanyId(_httpContext.User.GetCompanyId(), accessGroupId);

                var users = _unitOfWork.UserRepository.GetAssignUsersByAccessGroupIds(_httpContext.User.GetCompanyId(), new List<int> { accessGroup.Id });

                if (users.Any())
                {
                    foreach (var user in users)
                    {
                        var cards = _unitOfWork.CardRepository.GetByUserId(_httpContext.User.GetCompanyId(), user.Id);
                        cardCount += cards.Count();
                    }

                }


                return cardCount;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCardCount");
                return 0;
            }
        }

        /// <summary>
        /// Check if account is exist
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public bool IsAccountExist(int id, string username, int? companyId = null)
        {
            try
            {

                if (companyId == null)
                {
                    companyId = _httpContext.User.GetCompanyId();
                }

                var userLogin = _unitOfWork.AccountRepository.Get(m =>
                    m.Username.ToLower() == username.ToLower() && !m.IsDeleted &&
                    m.CompanyId == companyId && m.Id != id);

                return userLogin != null;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsAccountExist");
                return false;
            }
        }

        public Account GetAccountByEmail(string username, int? companyId)
        {
            try
            {

                if (companyId == null)
                {
                    companyId = _httpContext.User.GetCompanyId();
                }

                var userLogin = _unitOfWork.AccountRepository.Get(m =>
                    m.Username.ToLower() == username.ToLower() && !m.IsDeleted &&
                    m.CompanyId == companyId);

                return userLogin;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccountByEmail");
                return null;
            }
        }

        public Account GetAccountById(int userId)
        {
            try
            {

                var account = _unitOfWork.AccountRepository.Get(m =>
                    m.Id == userId && !m.IsDeleted);

                return account;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccountById");
                return null;
            }
        }

        public int Add(UserModel userModel, bool isSend = true)
        {
            // variable declaration
            User user = new User();
            var companyId = _httpContext.User.GetCompanyId();
            var company = _unitOfWork.CompanyRepository.GetById(companyId);
            var roleDefault = _unitOfWork.RoleRepository.GetDefaultRoleSettingByCompany(companyId);

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // add automatic user code
                        if (string.IsNullOrEmpty(userModel.UserCode))
                        {
                            int userCode = _unitOfWork.UserRepository.GetNewUserCode(companyId);
                            userModel.UserCode = $"{userCode:000000}";
                        }
                        else
                        {
                            if (IsDuplicatedUserCode(0, userModel.UserCode, companyId))
                            {
                                int userCode = _unitOfWork.UserRepository.GetNewUserCode(companyId);
                                userModel.UserCode = $"{userCode:000000}";
                            }
                        }

                        // mapping model
                        user = _mapper.Map<User>(userModel);

                        // check departmentId and department number user
                        if (!userModel.DepartmentId.HasValue && userModel.DepartmentId == 0 && string.IsNullOrEmpty(userModel.DepartmentNo))
                        {
                            var deptByNumber = _unitOfWork.DepartmentRepository.GetByNumberAndCompany(userModel.DepartmentNo, companyId);
                            if (deptByNumber != null)
                            {
                                user.DepartmentId = deptByNumber.Id;
                            }
                        }

                        // company
                        user.CompanyId = companyId;

                        // account company
                        var account = AddAccountToUser(userModel.Username, user, roleDefault, company);
                        user.AccountId = account?.Id;
                        user.PermissionType = (short)roleDefault.TypeId;

                        // save user in database
                        _unitOfWork.UserRepository.Add(user);
                        _unitOfWork.Save();

                        // Access group new version
                        var newAccessGroup = AddChildAccessGroupToParentOfUser(userModel.DoorList, company, user.Id, userModel.AccessGroupId);
                        if (newAccessGroup != null)
                        {
                            user.AccessGroupId = newAccessGroup.Id;
                            // update access group for user in database
                            _unitOfWork.UserRepository.Update(user);
                            _unitOfWork.Save();
                        }

                        // Save system log
                        if (!user.FirstName.Contains(Constants.Settings.UpdateUserData))
                        {
                            // include access group, card status, department
                            var details = new List<string>();

                            var accessGroup = _unitOfWork.AccessGroupRepository.GetById(user.AccessGroupId);
                            var detail = $"{UserResource.lblAccessGroup} : {accessGroup?.Name}";
                            details.Add(detail);

                            var department = _unitOfWork.DepartmentRepository.GetById(user.DepartmentId);
                            detail = $"{UserResource.lblDepartment} : {department?.DepartName}";
                            details.Add(detail);

                            var content = $"{ActionLogTypeResource.Add} : {user.FirstName + " " + user.LastName} ({UserResource.lblUser})";
                            var contentDetails = $"{UserResource.lblAddNew} :\n{string.Join("\n", details)}";

                            _unitOfWork.SystemLogRepository.Add(user.Id, SystemLogType.User, ActionLogType.Add, content, contentDetails, null, user.CompanyId);
                            _unitOfWork.Save();

                            // automatic add Qr Code
                            AutomationCreateQrCode(user, false);
                            // automatic add NFC Phone Id
                            AutomationCreateNFCPhoneId(user, false);
                        }

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex.Message + ex.StackTrace);
                        throw;
                    }
                }
            });

            // send count review
            var accessSetting = _unitOfWork.UserRepository.GetAccessSetting(user.CompanyId);
            if (accessSetting != null && accessSetting.ApprovalStepNumber != (short)VisitSettingType.NoStep && user.ApproverId1 != 0)
            {
                _accountService.SendCountReviewToFe(user.ApproverId1, user.CompanyId);
            }

            // send cards to device
            if (isSend)
            {
                var devices = _unitOfWork.IcuDeviceRepository.GetDevicesByUserId(user.Id)
                    .Where(m => company.AutoSyncUserData || m.ConnectionStatus == (short)ConnectionStatus.Online)
                    .ToList();
                if (devices.Count > 0)
                {
                    ThreadSendCardToDevice(null, new List<int>() { user.Id }, devices, _httpContext.User.GetUsername());
                }
            }

            return user.Id;
        }

        public List<int> AddMultiple(List<UserModel> userModels, bool isSend = true)
        {
            User user = new User();
            List<int> userIds = new List<int>();

            var userTimeZone = _accountService.GetById(_httpContext.User.GetAccountId()).TimeZone;
            var offSet = userTimeZone.ToTimeZoneInfo().BaseUtcOffset;

            var companyId = _httpContext.User.GetCompanyId();
            var company = _unitOfWork.CompanyRepository.GetById(companyId);
            string password = Helpers.GeneratePasswordDefaultWithCompany(company.Name);
            var settingDefault = new SettingService(_unitOfWork, _configuration);
            var language = settingDefault.GetLanguage(companyId);

            var buildingDefault = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(companyId);
            var timeZone = buildingDefault?.TimeZone ?? TimeZoneInfo.Local.Id;
            var dynamicRoleId = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((short)AccountType.Employee, companyId).FirstOrDefault().Id;

            var checkPlugIn = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId).PlugIns;
            var PlugInsValue = JsonConvert.DeserializeObject<PlugIns>(checkPlugIn);

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    foreach (var userModel in userModels)
                    {
                        try
                        {
                            var oldUser = _unitOfWork.UserRepository.GetByUserCode(companyId, userModel.UserCode);

                            

                            if (oldUser != null)
                            {
                                Console.WriteLine($"This is duplicated  user. {userModel.UserCode}");
                                continue;
                            }

                            
                            // temp set effective date dd/MM/yyyy 00:00:00
                             if (!DateTime.TryParseExact(userModel.EffectiveDate, Constants.Settings.DateTimeFormatDefault, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                            {
                                userModel.EffectiveDate += " 00:00:00";
                            }
                            
                            if (!DateTime.TryParseExact(userModel.ExpiredDate, Constants.Settings.DateTimeFormatDefault, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                            {
                                userModel.ExpiredDate += " 23:59:59";
                            }

                            user = _mapper.Map<User>(userModel);

                            user.EffectiveDate = user.EffectiveDate.Value.ConvertToSystemTime(offSet);
                            user.ExpiredDate = user.ExpiredDate.Value.ConvertToSystemTime(offSet);

                            // check departmentId and department number user
                            if (!userModel.DepartmentId.HasValue && userModel.DepartmentId == 0 && string.IsNullOrEmpty(userModel.DepartmentNo))
                            {
                                var deptByNumber = _unitOfWork.DepartmentRepository.GetByNumberAndCompany(userModel.DepartmentNo, companyId);
                                if (deptByNumber != null)
                                {
                                    user.DepartmentId = deptByNumber.Id;
                                }
                            }

                            user.AccessGroupId = userModel.AccessGroupId;

                            user.CompanyId = companyId;

                            if (string.IsNullOrEmpty(userModel.Email))
                            {
                                // If there is not email address, an account doesn't be created.
                                user.PermissionType = (short)AccountType.Employee;
                                _unitOfWork.UserRepository.Add(user);
                            }
                            else
                            {
                                //user.Email register
                                var account = _unitOfWork.AccountRepository.GetByUserName(userModel.Email);

                                if (account != null)
                                {
                                    // Add CompanyAccount to mark user belong to multiple companies
                                    // If account of the same company, then check
                                    CompanyAccount companyAccount = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompanyAndAccount(company.Id, account.Id);
                                    // Account already exist in the company. 
                                    if (companyAccount != null)
                                    {
                                        // Check if any User already map to this account
                                    }
                                    // Account is not in current company, we add this account to current company with default role
                                    else
                                    {
                                        companyAccount = new CompanyAccount();
                                        companyAccount.AccountId = account.Id;
                                        companyAccount.CompanyId = companyId;
                                        companyAccount.DynamicRoleId = dynamicRoleId;
                                        _unitOfWork.CompanyAccountRepository.Add(companyAccount);
                                    }
                                    user.AccountId = account.Id;
                                    user.PermissionType = (short)AccountType.Employee;

                                    _unitOfWork.UserRepository.Add(user);
                                }
                                else
                                {
                                    var accountModel = new AccountModel()
                                    {
                                        Username = userModel.Email,
                                        CompanyId = companyId,
                                        Password = password,
                                        ConfirmPassword = password,
                                        Role = (short)AccountType.Employee,
                                        PhoneNumber = userModel.HomePhone
                                    };

                                    var newAccount = _mapper.Map<Account>(accountModel);
                                    newAccount.CompanyId = companyId;
                                    newAccount.Language = language;
                                    newAccount.TimeZone = timeZone;
                                    _unitOfWork.AccountRepository.Add(newAccount);
                                    _unitOfWork.Save();

                                    user.AccountId = newAccount.Id;
                                    user.PermissionType = (short)AccountType.Employee;

                                    _unitOfWork.UserRepository.Add(user);

                                    // Add CompanyAccount to mark user belong to multiple companies
                                    CompanyAccount companyAccount = new CompanyAccount();
                                    companyAccount.AccountId = newAccount.Id;
                                    companyAccount.CompanyId = companyId;
                                    companyAccount.DynamicRoleId = dynamicRoleId;
                                    _unitOfWork.CompanyAccountRepository.Add(companyAccount);

                                    _unitOfWork.Save();
                                }
                            }

                            _unitOfWork.Save();

                            // Add Card
                            if (userModel.CardList != null && userModel.CardList.Any(c => c.CardType != (short)CardStatus.DeletedCard))
                            {
                                foreach (var cardModel in userModel.CardList)
                                {
                                    var isCardIdExist = IsCardIdExist(cardModel.CardId);
                                    if (!isCardIdExist)
                                    {
                                        var card = _mapper.Map<Card>(cardModel);
                                        card.UserId = user.Id;
                                        card.CompanyId = companyId;

                                        _unitOfWork.CardRepository.Add(card);
                                    }
                                }
                            }
                            else
                            {
                                // There is not any valid card in this user
                                user.Status = (short)Status.Invalid;
                                _unitOfWork.UserRepository.Update(user);
                            }

                            // Save system log
                            // include accessgroup, cardstatus, Department
                            var details = new List<string>();

                            var accessGroup = _unitOfWork.AccessGroupRepository.GetById(user.AccessGroupId);
                            var detail = $"{UserResource.lblAccessGroup} : {accessGroup?.Name}";
                            details.Add(detail);

                            var department = _unitOfWork.DepartmentRepository.GetById(user.DepartmentId);
                            detail = $"{UserResource.lblDepartment} : {department?.DepartName}";
                            details.Add(detail);

                            var content =
                                $"{ActionLogTypeResource.Add} : {user.FirstName + " " + user.LastName} ({user.EmpNumber})";
                            var contentDetails = $"{UserResource.lblAddNew} :\n{string.Join("\n", details)}";

                            _unitOfWork.SystemLogRepository.Add(user.Id, SystemLogType.User, ActionLogType.Add,
                                content, contentDetails, null, user.CompanyId);

                            //automation add qr code if Plugin qrcode = true
                            if (PlugInsValue.QrCode)
                            {
                                var listUserByAccountID = GetUserByUserId(user.Id);
                                AutomationCreateQrCode(listUserByAccountID, isSend);

                                // automatic add NFC Phone Id
                                AutomationCreateNFCPhoneId(listUserByAccountID, isSend);
                            }

                            userIds.Add(user.Id);

                            _unitOfWork.Save();
                        }
                        catch (Exception e)
                        {
                            //transaction.Rollback();
                            //throw;
                            Console.WriteLine("##-## [ERROR] ADD User Data : " + user.FirstName);
                            Console.WriteLine(e.StackTrace);
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.InnerException.Message);
                        }
                    }

                    transaction.Commit();
                }
            });

            //// Send all new ussers to devices.
            //SendUsersToAllDoors(userIds, true);

            return userIds;
        }

        public void Update(UserModel userModel)
        {
            // variable declaration
            User user = new User();
            var companyId = _httpContext.User.GetCompanyId();
            int accountId = _httpContext.User.GetAccountId();
            var company = _unitOfWork.CompanyRepository.GetCompanyById(companyId);
            var roleDefault = _unitOfWork.RoleRepository.GetDefaultRoleSettingByCompany(companyId);
            var isValid = userModel.Status == (short)Status.Valid;
            var haveToSend = false;
            List<IcuDevice> deletedDevices = null; // old device (system will send message delete card)

            // old device
            user = _unitOfWork.UserRepository.GetById(userModel.Id);
            var oldListAgd = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, user.AccessGroupId).ToList();
            deletedDevices = oldListAgd.Select(m => m.Icu).ToList();
            int oldApprovalStatus = user.ApprovalStatus;

            List<string> changes = new List<string>();
            haveToSend = CheckChange(user, userModel, false, ref changes);

            // new thread send update user to all doors
            string sender = _httpContext.User.GetUsername();

            IWebSocketService webSocketService = new WebSocketService();
            // Create separate UnitOfWork for background device operations to avoid DbContext concurrency issues
            IUnitOfWork backgroundUnitOfWork = DbHelper.CreateUnitOfWork(_configuration);
            var deviceInstructionQueue = new DeviceInstructionQueue(backgroundUnitOfWork, _configuration, webSocketService);
            try
            {
                Console.WriteLine($"[DEBUG] DELETE Check: isValid={isValid}, haveToSend={haveToSend}, deletedDevices.Count={deletedDevices.Count}");
                if (!isValid || haveToSend)
                {
                    Console.WriteLine($"[DEBUG] DELETE: Calling SendInstructionCommon with DeviceIds=[{string.Join(",", deletedDevices.Select(m => m.Id))}]");
                    // delete user info
                    deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                    {
                        MessageType = Constants.Protocol.DeleteUser,
                        MsgId = Guid.NewGuid().ToString(),
                        Sender = sender,
                        UserIds = new List<int>() { user.Id },
                        CompanyCode = company.Code,
                        DeviceIds = deletedDevices.Select(m => m.Id).ToList(),
                    });
                }
                else
                {
                    Console.WriteLine($"[DEBUG] DELETE: Condition not met - skipping DELETE operation");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // update account
                        if (string.IsNullOrEmpty(userModel.Username))
                        {
                            if (user.AccountId != null) user.AccountId = null;
                        }
                        else
                        {
                            if (user.AccountId == null || (user.Account?.Username.ToLower() != userModel.Username.ToLower()))
                            {
                                var account = AddAccountToUser(userModel.Username, user, roleDefault, company);
                                user.AccountId = account?.Id;
                                user.PermissionType = (short)roleDefault.TypeId;
                            }
                        }

                        // update working time of attendance
                        if (user.WorkingTypeId != userModel.WorkingTypeId && userModel.WorkingTypeId.HasValue)
                        {
                            var day = DateTime.Today.AddDays(1);
                            var attendance = _unitOfWork.AttendanceRepository.GetAttendanceAlreadyCreated(user.Id, companyId, day);
                            while (attendance != null)
                            {
                                var workingTypes = _unitOfWork.WorkingRepository.GetById(userModel.WorkingTypeId.Value);
                                var listWorkings = JsonConvert.DeserializeObject<List<WorkingTime>>(workingTypes.WorkingDay);
                                foreach (var timeWork in listWorkings)
                                {
                                    if (timeWork.Name == day.DayOfWeek.ToString())
                                    {
                                        attendance.WorkingTime = JsonConvert.SerializeObject(timeWork);
                                        _unitOfWork.AttendanceRepository.Update(attendance);
                                        break;
                                    }
                                }
                                day = day.AddDays(1);
                                attendance = _unitOfWork.AttendanceRepository.GetAttendanceAlreadyCreated(user.Id, companyId, day);
                            }
                        }

                        // create new person access group
                        var newAccessGroup = AddChildAccessGroupToParentOfUser(userModel.DoorList, company, user.Id, userModel.AccessGroupId);
                        if (newAccessGroup != null)
                        {
                            userModel.AccessGroupId = newAccessGroup.Id;
                            user.AccessGroupId = newAccessGroup.Id;
                        }
                        else if (userModel.AccessGroupId != user.AccessGroupId)
                        {
                            var oldAccessGroup = _unitOfWork.AccessGroupRepository.GetById(user.AccessGroupId);
                            if (oldAccessGroup.Type == (short)AccessGroupType.PersonalAccess)
                            {
                                // temp add access group of user
                                user.AccessGroupId = userModel.AccessGroupId;
                                // delete old access group
                                _unitOfWork.AccessGroupDeviceRepository.Delete(m => m.AccessGroupId == oldAccessGroup.Id);
                                _unitOfWork.AccessGroupRepository.Delete(oldAccessGroup);
                            }
                        }

                        // mapping model -> user
                        _mapper.Map(userModel, user);

                        // check departmentId and department number user
                        if (!userModel.DepartmentId.HasValue && userModel.DepartmentId == 0 && string.IsNullOrEmpty(userModel.DepartmentNo))
                        {
                            var department = _unitOfWork.DepartmentRepository.GetByNumberAndCompany(userModel.DepartmentNo, companyId);
                            if (department != null)
                            {
                                user.DepartmentId = department.Id;
                            }
                        }

                        // approval setting
                        var accessSetting = _unitOfWork.UserRepository.GetAccessSetting(companyId);
                        if (haveToSend)
                        {
                            if (accessSetting.ApprovalStepNumber == (int)VisitSettingType.SecondStep
                                || accessSetting.ApprovalStepNumber == (int)VisitSettingType.FirstStep)
                            {
                                user.ApprovalStatus = (int)ApprovalStatus.ApprovalWaiting1;
                            }
                        }

                        //Save system log
                        if (changes.Any())
                        {
                            var content = $"{ActionLogTypeResource.Update} : {user.FirstName + " " + user.LastName}";
                            if (!string.IsNullOrWhiteSpace(user.EmpNumber)) content += $" ({user.EmpNumber})";
                            var contentDetails = $"{string.Join("\n", changes)}";
                            _unitOfWork.SystemLogRepository.Add(user.Id, SystemLogType.User, ActionLogType.Update, content, contentDetails, null, user.CompanyId);
                            _unitOfWork.Save();
                        }

                        _unitOfWork.UserRepository.Update(user);
                        _unitOfWork.Save();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex.Message + ex.StackTrace);
                        throw;
                    }
                }
            });

            try
            {
                var listApprovedStatus = new List<int>()
                {
                    (int)ApprovalStatus.NotUse,
                    (int)ApprovalStatus.Approved,
                };
                Console.WriteLine($"[DEBUG] ADD Check: isValid={isValid}, user.ApprovalStatus={user.ApprovalStatus}, listApprovedStatus=[{string.Join(",", listApprovedStatus)}]");
                if (isValid && listApprovedStatus.Contains(user.ApprovalStatus))
                {
                    // Get the updated device list for the user after the database transaction
                    // We need to query using the main UnitOfWork since backgroundUnitOfWork may not have the latest data
                    var currentUser = _unitOfWork.UserRepository.GetById(userModel.Id);
                    var currentDevices = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, currentUser.AccessGroupId).Select(m => m.Icu).ToList();
                    
                    Console.WriteLine($"[DEBUG] ADD: Found {currentDevices.Count} devices for user {user.Id}, AccessGroupId={currentUser.AccessGroupId}");
                    Console.WriteLine($"[DEBUG] ADD: DeviceIds=[{string.Join(",", currentDevices.Select(m => m.Id))}]");
                    
                    if (currentDevices.Any())
                    {
                        // Get user's card IDs
                        var userCardIds = _unitOfWork.AppDbContext.Card
                            .Where(card => !card.IsDeleted && card.UserId == user.Id)
                            .Select(card => card.Id)
                            .ToList();
                        Console.WriteLine($"[DEBUG] ADD: Found {userCardIds.Count} cards for user {user.Id}, CardIds=[{string.Join(",", userCardIds)}]");
                        
                        // add user info
                        Console.WriteLine("ADD");
                        deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                        {
                            MessageType = Constants.Protocol.AddUser,
                            MsgId = Guid.NewGuid().ToString(),
                            Sender = sender,
                            UserIds = new List<int>() { user.Id },
                            CardIds = userCardIds,
                            CompanyCode = company.Code,
                            DeviceIds = currentDevices.Select(m => m.Id).ToList(),
                        });
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] ADD: No devices found for user {user.Id} - skipping ADD operation");
                    }
                }
                else
                {
                    Console.WriteLine($"[DEBUG] ADD: Condition not met - skipping ADD operation");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                // Dispose the background UnitOfWork
                backgroundUnitOfWork?.Dispose();
            }
        }

        /// <summary>
        /// update multiple users
        /// </summary>
        /// <param name="userModel"></param>
        public List<int> UpdateMultiple(List<UserModel> userModels)
        {
            User user = new User();
            List<int> userIds = new List<int>();

            var userTimeZone = _accountService.GetById(_httpContext.User.GetAccountId()).TimeZone;
            var offSet = userTimeZone.ToTimeZoneInfo().BaseUtcOffset;

            var companyId = _httpContext.User.GetCompanyId();
            var company = _unitOfWork.CompanyRepository.GetById(companyId);

            var checkPlugIn = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId).PlugIns;
            var haveToSend = false;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    foreach (var userModel in userModels)
                    {
                        try
                        {
                            var isValid = userModel.Status == (short)Status.Valid;

                            user = GetByIdAndCompany(userModel.Id, companyId);

                            if (user == null)
                            {
                                continue;
                            }

                            List<string> changes = new List<string>();

                            var dateTimeFormat = Constants.Settings.DateTimeFormatDefault;

                            if (userModel.EffectiveDate == null)
                            {
                                userModel.EffectiveDate = user.EffectiveDate.ToSettingDateTimeString();
                            }
                            else
                            {
                                if (!DateTime.TryParseExact(userModel.EffectiveDate, dateTimeFormat, null, DateTimeStyles.None, out _))
                                {
                                    // temp set effective date dd/MM/yyyy 00:00:00
                                    userModel.EffectiveDate += " 00:00:00";
                                }

                                var newEffectiveDate = DateTime.ParseExact(userModel.EffectiveDate, dateTimeFormat, null);
                                newEffectiveDate = newEffectiveDate.ConvertToSystemTime(offSet);
                                userModel.EffectiveDate = newEffectiveDate.ToString(dateTimeFormat);
                            }

                            if (userModel.ExpiredDate == null)
                            {
                                userModel.ExpiredDate = user.ExpiredDate.ToSettingDateTimeString();
                            }
                            else
                            {
                                if (!DateTime.TryParseExact(userModel.ExpiredDate, dateTimeFormat, null, DateTimeStyles.None, out _))
                                {
                                    // temp set expire date dd/MM/yyyy 23:59:59
                                    userModel.ExpiredDate += " 23:59:59";
                                }

                                var newExpiredDate = DateTime.ParseExact(userModel.ExpiredDate, dateTimeFormat, null);
                                newExpiredDate = newExpiredDate.ConvertToSystemTime(offSet);
                                userModel.ExpiredDate = newExpiredDate.ToString(dateTimeFormat);
                            }

                            // 1. Compare the accessGroupId value
                            if (user.AccessGroupId != userModel.AccessGroupId)
                            {
                                // 2. Check whether the access group is PAG or not.
                                if (user.AccessGroup.Type == (short)AccessGroupType.PersonalAccess)
                                {
                                    // 3. Check the Parent AccessGroupId value.
                                    if (user.AccessGroup.ParentId == userModel.AccessGroupId)
                                    {
                                        // In this case, it means that the user's accessGroup information is not changed.
                                        // Because the user is just using PAG.
                                        // Update the userModel's AccessGroupId value same as before (old value).
                                        userModel.AccessGroupId = user.AccessGroupId;
                                    }
                                    else
                                    {
                                        // API should process in this case too.
                                        // But we don't have any plan to develop this process yet.
                                        // TODO
                                        var newAccessGroupDeviceIds = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, userModel.AccessGroupId).Select(agd => agd.IcuId).ToList();
                                        var oldAccessGroupDevices = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, user.AccessGroupId, includeParent: false);
                                        var oldAccessGroupDeviceIds = oldAccessGroupDevices.Select(agd => agd.IcuId).ToList();
                                        // Get intersect
                                        var intersectDeviceIds = newAccessGroupDeviceIds.Intersect(oldAccessGroupDeviceIds);
                                        if (intersectDeviceIds.Any())
                                        {
                                            var toBeDeleted = oldAccessGroupDevices.Where(m => intersectDeviceIds.Contains(m.IcuId));
                                            _unitOfWork.AccessGroupDeviceRepository.DeleteRange(toBeDeleted);
                                            _unitOfWork.Save();
                                        }
                                    }
                                }
                            }



                            haveToSend = CheckChangeADD(user, userModel, false, ref changes);

                            if (haveToSend)
                            {
                                userIds.Add(userModel.Id);
                            }

                            // update user
                            if (userModel.Email != user.Email)
                            {
                                if (!string.IsNullOrEmpty(user.Email))
                                {
                                    // Delete old one.
                                    var existingAccount = _unitOfWork.AccountRepository.GetByUserNameWithTracking(user.Email);
                                    if (existingAccount != null && existingAccount.Id == user.AccountId)
                                    {
                                        // Check companyAccount.
                                        CompanyAccount companyAccount = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompanyAndAccount(company.Id, existingAccount.Id);

                                        if (companyAccount != null)
                                        {
                                            _unitOfWork.CompanyAccountRepository.Delete(companyAccount);
                                            _unitOfWork.Save();
                                        }

                                        List<CompanyAccount> companyAccounts = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByAccount(existingAccount.Id);
                                        if (companyAccounts.Count == 0)
                                        {
                                            _unitOfWork.AccountRepository.DeleteFromSystem(existingAccount);
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(userModel.Email))
                                {
                                    var account = _unitOfWork.AccountRepository.GetByUserName(userModel.Email);
                                    if (account != null)
                                    {
                                        user.AccountId = account.Id;

                                        // Add CompanyAccount to mark user belong to multiple companies
                                        // If account of the same company, then check
                                        CompanyAccount companyAccount = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompanyAndAccount(company.Id, account.Id);
                                        // Account already exist in the company. 
                                        if (companyAccount != null)
                                        {
                                            // Check if any User already map to this account
                                            if (user.AccountId != account.Id)
                                            {

                                            }
                                        }
                                        // Account is not in current company, we add this account to current company with default role
                                        else
                                        {
                                            companyAccount = new CompanyAccount
                                            {
                                                AccountId = account.Id,
                                                CompanyId = _httpContext.User.GetCompanyId(),
                                                DynamicRoleId = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((short)AccountType.Employee, companyId).FirstOrDefault().Id
                                            };
                                            _unitOfWork.CompanyAccountRepository.Add(companyAccount);

                                        }
                                        user.AccountId = account.Id;
                                        _unitOfWork.UserRepository.Update(user);

                                        //Update Phone number to user account
                                        account.PhoneNumber = user.HomePhone;
                                        _unitOfWork.AccountRepository.Update(account);

                                    }
                                    else
                                    {
                                        var accountModel = new AccountModel()
                                        {
                                            Username = userModel.Email,
                                            CompanyId = companyId,
                                            Password = Helpers.GenerateRandomPassword(),
                                            ConfirmPassword = Helpers.GenerateRandomPassword(),
                                            //Role = (short)AccountType.Employee
                                            PhoneNumber = userModel.HomePhone
                                        };

                                        var newAccount = _mapper.Map<Account>(accountModel);
                                        newAccount.CompanyId = companyId;
                                        _unitOfWork.AccountRepository.Add(newAccount);
                                        _unitOfWork.Save();

                                        user.AccountId = newAccount.Id;
                                        _unitOfWork.UserRepository.Update(user);

                                        // Add CompanyAccount to mark user belong to multiple companies
                                        CompanyAccount companyAccount = new CompanyAccount
                                        {
                                            AccountId = newAccount.Id,
                                            CompanyId = _httpContext.User.GetCompanyId(),
                                            DynamicRoleId = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)AccountType.Employee, companyId).FirstOrDefault().Id
                                        };

                                        _unitOfWork.CompanyAccountRepository.Add(companyAccount);

                                    }
                                }
                            }
                            else if (!string.IsNullOrEmpty(userModel.Email))
                            {
                                var account1 = _unitOfWork.AccountRepository.GetByUserNameWithTracking(userModel.Email);
                                if (account1 != null && user.AccountId != account1.Id)
                                {
                                    user.AccountId = account1.Id;
                                    _unitOfWork.UserRepository.Update(user);
                                }
                                if (account1 != null)
                                {
                                    //Update Phone number to user account
                                    account1.PhoneNumber = userModel.HomePhone;
                                    _unitOfWork.AccountRepository.Update(account1);
                                }
                            }

                            // Add or update Card
                            if (userModel.CardList != null && userModel.CardList.Any())
                            {
                                bool isSend = false;
                                foreach (var cardModel in userModel.CardList)
                                {
                                    var isCardIdExist = IsCardIdExist(cardModel.CardId);

                                    if (!isCardIdExist)
                                    {
                                        var card = _mapper.Map<Card>(cardModel);
                                        card.UserId = user.Id;
                                        card.CompanyId = companyId;
                                        _unitOfWork.CardRepository.Add(card);

                                        changes.Add(string.Format(UserResource.msgAddNewCertification, ((CardType)cardModel.CardType).GetDescription(), $"{user.FirstName} ({user.UserCode})"));
                                        changes.Add($"{UserResource.lblCardId} : {card.CardId}\n" +
                                                          $"{UserResource.lblIssueCount} : {card.IssueCount}");

                                        isSend = true;
                                    }
                                    else
                                    {
                                        var oldCard = user.Card.FirstOrDefault(m => !m.IsDeleted && m.CardId.ToLower().Equals(cardModel.CardId.ToLower()));
                                        if (oldCard == null)
                                        {
                                            continue;
                                        }

                                        if (oldCard.UserId == user.Id)
                                        {
                                            // update
                                            if (oldCard.IssueCount != cardModel.IssueCount)
                                            {
                                                changes.Add(Helpers.CreateChangedValueContents(UserResource.lblIssueCount, oldCard.IssueCount, cardModel.IssueCount));

                                                oldCard.IssueCount = cardModel.IssueCount;
                                                oldCard.CardType = cardModel.CardType;
                                                isSend = true;
                                            }

                                            if (cardModel.CardStatus == (short)CardStatus.DeletedCard)
                                            {
                                                changes.Add(string.Format(UserResource.msgDeleteCard, oldCard.CardId, $"{user?.FirstName} ({user?.EmpNumber})"));

                                                oldCard.IsDeleted = true;
                                                isSend = true;
                                            }
                                            else if (oldCard.CardStatus != cardModel.CardStatus)
                                            {
                                                changes.Add(Helpers.CreateChangedValueContents(UserResource.lblCardStatus, ((CardStatus) oldCard.CardStatus).GetDescription(), ((CardStatus)cardModel.CardStatus).GetDescription()));

                                                oldCard.CardStatus = cardModel.CardStatus;
                                                isSend = true;
                                            }

                                            _unitOfWork.CardRepository.Update(oldCard);
                                        }
                                    }
                                }
                                if (!userIds.Contains(userModel.Id) && isSend)
                                {
                                    userIds.Add(userModel.Id);
                                }
                            }

                            // update working time of attendance
                            if (user.WorkingTypeId != userModel.WorkingTypeId)
                            {
                                var day = DateTime.Today.AddDays(1);

                                var attendance = _unitOfWork.AttendanceRepository.GetAttendanceAlreadyCreated(user.Id, companyId, day);

                                while (attendance != null)
                                {
                                    var workingTypes = _unitOfWork.WorkingRepository.GetById(userModel.WorkingTypeId.Value);
                                    var listWorkings = JsonConvert.DeserializeObject<List<WorkingTime>>(workingTypes.WorkingDay);

                                    foreach (var timeWork in listWorkings)
                                    {
                                        if (timeWork.Name == day.DayOfWeek.ToString())
                                        {
                                            attendance.WorkingTime = JsonConvert.SerializeObject(timeWork);

                                            _unitOfWork.AttendanceRepository.Update(attendance);

                                            break;
                                        }
                                    }

                                    day = day.AddDays(1);
                                    attendance = _unitOfWork.AttendanceRepository.GetAttendanceAlreadyCreated(user.Id, companyId, day);
                                }
                            }
                            _mapper.Map(userModel, user);

                            // Check existing valid card
                            var existingCard = user.Card.Where(m => !m.IsDeleted && Helpers.GetCardStatusToSend().Contains(m.CardStatus));
                            if (!existingCard.Any())
                            {
                                if (user.Status != (short)Status.Invalid)
                                {
                                    changes.Add(Helpers.CreateChangedValueContents(UserResource.lblStatus, ((Status)user.Status).GetDescription(), (Status.Invalid).GetDescription()));
                                }

                                user.Status = (short)Status.Invalid;
                            }
                            // check departmentId and department number user
                            if (!userModel.DepartmentId.HasValue && userModel.DepartmentId == 0 && string.IsNullOrEmpty(userModel.DepartmentNo))
                            {
                                var department = _unitOfWork.DepartmentRepository.GetByNumberAndCompany(userModel.DepartmentNo, companyId);
                                if (department != null)
                                {
                                    user.DepartmentId = department.Id;
                                }
                            }

                            // Delete faceId if this user is changed to invalid.
                            if (!isValid)
                            {
                                var faceCard = user.Card.FirstOrDefault(m => m.CardType.Equals((int)CardType.FaceId));

                                if (faceCard != null)
                                {
                                    DeleteCardByUser(user, faceCard.Id);
                                }
                            }

                            //Save system log
                            var content = $"{ActionLogTypeResource.Update} : {user.FirstName + " " + user.LastName} ({user.EmpNumber})";

                            var contentDetails = $"{UserResource.lblUpdate}: {string.Join("\n", changes)}";
                            _unitOfWork.SystemLogRepository.Add(user.Id, SystemLogType.User, ActionLogType.Update,
                                content, contentDetails, null, user.CompanyId);

                            _unitOfWork.UserRepository.Update(user);

                            _unitOfWork.Save();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("##-## [ERROR] UPDATE User Data : " + user.FirstName);
                            Console.WriteLine(e.StackTrace);
                            Console.WriteLine(e.Message);
                            Console.WriteLine(e.InnerException?.Message);
                        }
                    }

                    transaction.Commit();
                }
            });

            return userIds;
        }

        public bool Delete(User user, out string message)
        {
            string sender = _httpContext.User.GetUsername();
            bool hasSend = false;
            string messageError = string.Empty;

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // send delete all card of user
                        SendUpdateUsersToAllDoors(new List<User>() { user }, sender, false);
                        hasSend = true;

                        // delete attendance
                        _unitOfWork.AttendanceRepository.Delete(m => m.UserId == user.Id);

                        // Delete all Cards for a User
                        var oldCardList = _unitOfWork.CardRepository.GetByUserId(user.CompanyId, user.Id);
                        if (oldCardList.Count > 0)
                        {
                            // delete face
                            if (oldCardList.FirstOrDefault(m => m.CardType == (short)CardType.FaceId) != null)
                            {
                                _unitOfWork.CardRepository.DeleteFacesByUserId(user.Id);
                            }

                            // delete cards
                            _unitOfWork.CardRepository.DeleteRangeFromSystem(oldCardList);
                        }

                        // Delete vehicle
                        _unitOfWork.VehicleRepository.Delete(m => m.CompanyId == user.CompanyId && m.UserId == user.Id);

                        // Delete companyAccount ( and account if needed )
                        if (user.PermissionType != (short)PermissionType.NotUse && !string.IsNullOrEmpty(user.Email))
                        {
                            var account = _unitOfWork.AccountRepository.GetByUserNameWithTracking(user.Email);
                            if (account != null)
                            {
                                var companyAccount = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompanyAndAccount(user.CompanyId, account.Id);
                                if (companyAccount != null && companyAccount.DynamicRole.TypeId == (short)AccountType.Employee) 
                                    _unitOfWork.CompanyAccountRepository.Delete(companyAccount);

                                var companyAccounts = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByAccount(account.Id);
                                if (companyAccounts.Count == 1)
                                {
                                    _unitOfWork.AccountRepository.DeleteFromSystem(account);
                                }
                            }
                        }

                        // Delete PAG that was used by deleted users (Set accessGroup's IsDeleted to TRUE)
                        if (user.AccessGroup.Type == (short)AccessGroupType.PersonalAccess)
                        {
                            _unitOfWork.AccessGroupRepository.DeleteFromSystem(user.AccessGroup);
                            // Delete AGDs.
                            var agDevices = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(user.CompanyId, user.AccessGroupId, null, false);
                            _unitOfWork.AccessGroupDeviceRepository.DeleteRange(agDevices);
                        }

                        // Delete user (update 'IsDeleted' flag to TRUE)
                        _unitOfWork.UserRepository.DeleteFromSystem(user);

                        if (!user.FirstName.Contains(Constants.Settings.UpdateUserData))
                        {
                            // Save system log
                            var content = $"{ActionLogTypeResource.Delete} : {user.FirstName} {user.LastName} ({UserResource.lblUserCode} : {user.UserCode})";
                            _unitOfWork.SystemLogRepository.Add(user.Id, SystemLogType.User, ActionLogType.Delete, content, null, new List<int> { user.Id }, user.CompanyId);
                        }

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        if (hasSend)
                        {
                            SendUpdateUsersToAllDoors(new List<User>() { user }, sender, true);
                        }

                        messageError = e.Message;
                    }
                }
            });

            message = messageError;
            return string.IsNullOrEmpty(messageError);
        }

        public bool DeleteRange(List<User> users, out string message)
        {
            bool hasSend = false;
            int companyId = _httpContext.User.GetCompanyId();
            string sender = _httpContext.User.GetUsername();
            string messageError = string.Empty;

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // Delete cards and send DELETE USER message
                        SendUpdateUsersToAllDoors(users, sender, false);
                        hasSend = true;

                        // Delete attendances
                        var userIds = users.Select(c => c.Id).ToList();
                        _unitOfWork.AttendanceRepository.Delete(m => userIds.Contains(m.Id));

                        // Delete all information of User
                        foreach (var user in users)
                        {
                            // delete card
                            var oldCardList = _unitOfWork.CardRepository.GetByUserId(user.CompanyId, user.Id);
                            if (oldCardList.Count > 0)
                            {
                                // delete face
                                if (oldCardList.FirstOrDefault(m => m.CardType == (short)CardType.FaceId) != null)
                                {
                                    _unitOfWork.CardRepository.DeleteFacesByUserId(user.Id);
                                }

                                // delete cards
                                _unitOfWork.CardRepository.DeleteRangeFromSystem(oldCardList);
                            }

                            // delete vehicle
                            _unitOfWork.VehicleRepository.Delete(m => m.CompanyId == user.CompanyId && m.UserId == user.Id);

                            // delete account
                            if (user.PermissionType != (short)PermissionType.NotUse && !string.IsNullOrEmpty(user.Email))
                            {
                                var account = _unitOfWork.AccountRepository.GetByUserNameWithTracking(user.Email);
                                if (account != null)
                                {
                                    var companyAccount = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompanyAndAccount(user.CompanyId, account.Id);
                                    if (companyAccount != null && companyAccount.DynamicRole.TypeId == (short)AccountType.Employee) 
                                        _unitOfWork.CompanyAccountRepository.Delete(companyAccount);

                                    var companyAccounts = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByAccount(account.Id);
                                    if (companyAccounts.Count == 1)
                                        _unitOfWork.AccountRepository.DeleteFromSystem(account);
                                }
                            }

                            // Delete PAG that was used by deleted users (Set accessGroup's IsDeleted to TRUE)
                            if (user.AccessGroup.Type == (short)AccessGroupType.PersonalAccess)
                            {
                                _unitOfWork.AccessGroupRepository.DeleteFromSystem(user.AccessGroup);
                                // Delete AGDs.
                                var agDevices = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(user.CompanyId, user.AccessGroupId, null, false);
                                _unitOfWork.AccessGroupDeviceRepository.DeleteRange(agDevices);
                            }

                            // Delete user data form system.
                            _unitOfWork.UserRepository.DeleteFromSystem(user);
                        }

                        //Save system log
                        if (users.Count == 1)
                        {
                            var user = users.First();
                            var content = $"{ActionLogTypeResource.Delete}: {user.FirstName} {user.LastName} ({UserResource.lblUserCode} : {user.UserCode})";

                            _unitOfWork.SystemLogRepository.Add(user.Id, SystemLogType.User, ActionLogType.Delete, content, null, null, user.CompanyId);
                        }
                        else
                        {
                            List<string> userNames = users.Select(m => m.FirstName).ToList();
                            var content = string.Format(ActionLogTypeResource.DeleteMultipleType, UserResource.lblUser);
                            var contentDetails = $"{UserResource.lblUserCount} : {userIds.Count}\n" + $"{UserResource.lblName}: {string.Join(", ", userNames)}";
                            _unitOfWork.SystemLogRepository.Add(userIds.First(), SystemLogType.User, ActionLogType.DeleteMultiple, content, contentDetails, userIds, companyId);
                        }

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        if (hasSend)
                        {
                            SendUsersToAllDoors(users.Select(m => m.Id).ToList(), true);
                        }

                        _unitOfWork.SystemLogRepository.Add(users.First().Id, SystemLogType.User, ActionLogType.DeleteMultiple, e.Message, $"INNER : {e.InnerException?.Message}, LINE : {e.StackTrace}", users.Select(m => m.Id).ToList(), users.First().CompanyId);
                        _unitOfWork.Save();
                        transaction.Commit();
                        messageError = e.Message;
                    }
                }
            });

            message = messageError;
            return string.IsNullOrEmpty(messageError);
        }

        /// <summary>
        /// Get user by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public User GetById(int id)
        {
            try
            {

                var user = _unitOfWork.UserRepository.GetByUserId(_httpContext.User.GetCompanyId(), id);
                return user;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return null;
            }
        }

        /// <summary>
        /// Get by cardid
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public User GetByCardId(string cardId)
        {
            try
            {

                var card = _unitOfWork.CardRepository.Get(m =>
                    m.CardId.Equals(cardId) && m.CompanyId == _httpContext.User.GetCompanyId() && !m.IsDeleted);
                return _unitOfWork.UserRepository.Get(m => m.Id == card.UserId && m.CompanyId == _httpContext.User.GetCompanyId() && !m.IsDeleted);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCardId");
                return null;
            }
        }

        public Card GetCardByUser(int userId, int cardId)
        {
            try
            {

                var card = _unitOfWork.CardRepository.Get(m => m.Id == cardId && m.UserId == userId && !m.IsDeleted);
                return card;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCardByUser");
                return null;
            }
        }

        /// <summary>
        /// Get user by id and company
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public User GetByIdAndCompany(int id, int companyId)
        {
            try
            {

                var data = _unitOfWork.AppDbContext.User
                    .Include(m => m.Department)
                    .Include(m => m.Company)
                    .Include(m => m.AccessGroup)
                    .Include(m => m.Card)
                    .Include(m => m.Face)
                    .Include(m => m.Account.CompanyAccount)
                    .Include(m => m.Vehicle)
                    .Where(m => m.Id == id && !m.IsDeleted);

                if (data.Any() && companyId != 0)
                {
                    data = data.Where(m => m.CompanyId == companyId);
                }

                //return _unitOfWork.AppDbContext.User.Include(m => m.Department).Include(m => m.Company).Include(m => m.AccessGroup).Where(m => m.Id == id && m.CompanyId == companyId && !m.IsDeleted).First();

                return data.FirstOrDefault();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdAndCompany");
                return null;
            }
        }

        /// <summary>
        /// Get list user by ids and company
        /// </summary>
        /// <param name="userIds"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<User> GetByIdsAndCompany(List<int> userIds, int companyId)
        {
            try
            {

                var data = _unitOfWork.AppDbContext.User
                    .Include(m => m.Department)
                    .Include(m => m.Company)
                    .Include(m => m.AccessGroup)
                    .Include(m => m.Card)
                    .Include(m => m.Face)
                    .Include(m => m.Account.CompanyAccount)
                    // .Include(m => m.Attendance)
                    .Include(m => m.Vehicle)
                    .Where(m => userIds.Contains(m.Id) && !m.IsDeleted);

                if (companyId != 0)
                {
                    data = data.Where(m => m.CompanyId == companyId);
                }

                return data.ToList();
                //return _unitOfWork.AppDbContext.User.Include(m => m.Card).Include(m => m.Department).Where(m => userIds.Contains(m.Id) && m.CompanyId == companyId && !m.IsDeleted).ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdsAndCompany");
                return new List<User>();
            }
        }

        ///// <summary>
        ///// Export user data to excel or txt file
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="filter"></param>
        ///// <param name="sortColumn"></param>
        ///// <param name="sortDirection"></param>
        ///// <param name="totalRecords"></param>
        ///// <param name="recordsFiltered"></param>
        ///// <returns></returns>
        public byte[] Export(string type, UserFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            byte[] fileByte;

            try
            {
                var data = FilterDataWithOrder(filter, out totalRecords, out recordsFiltered);

                switch (type.ToLower())
                {
                    case "excel":
                        fileByte = ExportExcel(data);
                        break;
                    case "hancell":
                        fileByte = ExportHancell(data);
                        break;
                    default:
                        fileByte = ExportTxt(data);
                        break;
                }

                List<string> contentsDetails = [];
                if (filter != null && filter.DepartmentIds != null && filter.DepartmentIds.Count > 0)
                {
                    var departments = _unitOfWork.DepartmentRepository.GetByIdsAndCompanyId(filter.DepartmentIds, _httpContext.User.GetCompanyId());
                    var departNames = departments.Select(dept => dept.DepartName).ToList();
                    contentsDetails.Add($"{UserResource.lblDepartment} : {(departNames.Count > 1 ? "<br />" : "")}{string.Join("<br />", departNames)}");
                }

                string contentsDetail = String.Empty;
                if (contentsDetails.Count > 0)
                    contentsDetail = string.Join("\n <br />", contentsDetails);

                var successContent = $"{ActionLogTypeResource.Export} {ActionLogTypeResource.Success}";
                _unitOfWork.SystemLogRepository.Add(0, SystemLogType.User, ActionLogType.Export, 
                    successContent, contentsDetail, null, _httpContext.User.GetCompanyId());
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
                var errorContent = $"{ActionLogTypeResource.Export} {ActionLogTypeResource.Fail}";
                _unitOfWork.SystemLogRepository.Add(0, SystemLogType.User, ActionLogType.Export, 
                    errorContent, null, null, _httpContext.User.GetCompanyId());
                _unitOfWork.Save();
                throw;
            }
            return fileByte;
        }

        ///// <summary>
        ///// Export user data to excel or txt file
        ///// </summary>
        ///// <param name="type"></param>
        ///// <param name="filter"></param>
        ///// <param name="sortColumn"></param>
        ///// <param name="sortDirection"></param>
        ///// <param name="totalRecords"></param>
        ///// <param name="recordsFiltered"></param>
        ///// <returns></returns>
        public byte[] ExportUserData(string type, UserFilterModel filter, string pageName, out int totalRecords, out int recordsFiltered)
        {
            byte[] fileByte;

            try
            {
                string dateFormat = ApplicationVariables.Configuration[Constants.DateServerFormat + ":" + Thread.CurrentThread.CurrentCulture.Name];
                var userTimeZone = _accountService.GetById(_httpContext.User.GetAccountId()).TimeZone;
                var offSet = userTimeZone.ToTimeZoneInfo().BaseUtcOffset;

                var data = GetPaginated(filter, out totalRecords, out recordsFiltered, out _, pageName, dateFormat: dateFormat, offSet: offSet).ToList();
                ExportHelper exportHelper = new(_configuration, _httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId());

                switch (type.ToLower())
                {
                    case "csv":
                        fileByte = exportHelper.ExportDataToCsv(data, pageName);
                        break;
                    case "hancell":
                        fileByte = exportHelper.ExportDataToHancell(data, pageName);
                        break;
                    case "excel":
                    default:
                        fileByte = exportHelper.ExportDataToExcel(data, pageName);
                        break;
                }

                List<string> contentsDetails = [];
                if(filter != null && filter.DepartmentIds != null && filter.DepartmentIds.Count > 0)
                {
                    var departments = _unitOfWork.DepartmentRepository.GetByIdsAndCompanyId(filter.DepartmentIds, _httpContext.User.GetCompanyId());
                    var departNames = departments.Select(dept => dept.DepartName).ToList();
                    contentsDetails.Add($"{UserResource.lblDepartment} : {string.Join("<br />", departNames)}");
                }

                string contentsDetail = String.Empty;
                if (contentsDetails.Count > 0)
                    contentsDetail = string.Join("\n <br />", contentsDetails);

                var successContent = $"{ActionLogTypeResource.Export} {ActionLogTypeResource.Success}";
                _unitOfWork.SystemLogRepository.Add(0, SystemLogType.User, ActionLogType.Export,
                    successContent, contentsDetail, null, _httpContext.User.GetCompanyId());
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
                var errorContent = $"{ActionLogTypeResource.Export} {ActionLogTypeResource.Fail}";
                _unitOfWork.SystemLogRepository.Add(0, SystemLogType.User, ActionLogType.Export,
                    errorContent, null, null, _httpContext.User.GetCompanyId());
                _unitOfWork.Save();
                throw;
            }

            return fileByte;
        }

        public byte[] ExportAccessibleDoors(User user, string type, string search, out int totalRecords,
            out int recordsFiltered, out string fileName, string sortColumn = "Id",
            string sortDirection = "desc")
        {
            try
            {

                // Change language.
                var accountLanguage = _unitOfWork.AppDbContext.Account.FirstOrDefault(m => m.Id == _httpContext.User.GetAccountId()).Language;
                var culture = new CultureInfo(accountLanguage);

                // FE doesn't give "Culture" info.
                Thread.CurrentThread.CurrentUICulture = culture;

                var accessibleDoorModels = FilterAccessibleDoorDataWithOrder(user, search, sortColumn, sortDirection, out totalRecords,
                   out recordsFiltered).ToList();

                var fileByte = type == Constants.Excel
                    ? ExportAccessibleDoorsWithExcel(accessibleDoorModels)
                    : ExportAccessibleDoorsWithTxt(accessibleDoorModels);

                //Save system log
                var companyId = _httpContext.User.GetCompanyId();
                fileName = string.Format(Constants.ExportFileFormat, UserResource.lblExportAccessibleDoor + $"_{user.FirstName + (!string.IsNullOrWhiteSpace(user.LastName) ? "_" + user.LastName : "")}", DateTime.Now);
                fileName = type == "excel" ? $"{fileName}.xlsx" : $"{fileName}.csv";
                var content = $"{CommonResource.lblFileName} : {fileName}";
                var contentDetails = $"{AccountResource.lblUsername} : {user.FirstName + (!string.IsNullOrWhiteSpace(user.LastName) ? " " + user.LastName : "")}";
                _unitOfWork.SystemLogRepository.Add(companyId, SystemLogType.User, ActionLogType.AccessibleDoorExport, content, contentDetails, null, _httpContext.User.GetCompanyId());
                _unitOfWork.Save();

                return fileByte;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExportAccessibleDoors");
                totalRecords = 0;
                recordsFiltered = 0;
                fileName = null;

                return new byte[0];
            }
        }

        ///// <summary>
        ///// Export Accessible doors data to txt file
        ///// </summary>
        ///// <param name="accessibleDoorModels"></param>
        ///// <returns></returns>
        public byte[] ExportAccessibleDoorsWithExcel(List<AccessibleDoorModel> accessibleDoorModels)
        {
            try
            {

                byte[] result;

                //var package = new ExcelPackage();
                using (var package = new ExcelPackage())
                {
                    // add a new worksheet to the empty workbook
                    var worksheet =
                        package.Workbook.Worksheets.Add(UserResource.lblAccessibleDoors); //Worksheet name

                    //First add the headers for Accessible Door sheet
                    for (var i = 0; i < _headerForAccessibleDoor.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = _headerForAccessibleDoor[i];
                    }

                    var recordIndex = 2;
                    foreach (var accessibleDoorModel in accessibleDoorModels)
                    {
                        //For the Accessible Door sheet
                        var colIndex = 1;
                        //worksheet.Cells[recordIndex, colIndex++].Value = accessibleDoorModel.Id;
                        worksheet.Cells[recordIndex, colIndex++].Value = recordIndex - 1;
                        worksheet.Cells[recordIndex, colIndex++].Value = accessibleDoorModel.DoorName;
                        worksheet.Cells[recordIndex, colIndex++].Value = accessibleDoorModel.DeviceAddress;
                        worksheet.Cells[recordIndex, colIndex++].Value = accessibleDoorModel.ActiveTz;
                        worksheet.Cells[recordIndex, colIndex++].Value = accessibleDoorModel.PassageTz;
                        worksheet.Cells[recordIndex, colIndex++].Value = accessibleDoorModel.VerifyMode;
                        worksheet.Cells[recordIndex, colIndex++].Value = accessibleDoorModel.AntiPassback;
                        worksheet.Cells[recordIndex, colIndex++].Value = accessibleDoorModel.DeviceType;
                        worksheet.Cells[recordIndex, colIndex].Value = accessibleDoorModel.Mpr;

                        recordIndex++;
                    }

                    worksheet.Cells.AutoFitColumns(15);

                    result = package.GetAsByteArray();
                }
                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExportAccessibleDoorsWithExcel");
                return new byte[0];
            }
        }

        ///// <summary>
        ///// Export Accessible doors data to txt file
        ///// </summary>
        ///// <param name="accessibleDoorModels"></param>
        ///// <returns></returns>
        public byte[] ExportAccessibleDoorsWithTxt(List<AccessibleDoorModel> accessibleDoorModels)
        {
            try
            {

                // Build the file content

                var cnt = 1;
                var accessibleDoors = accessibleDoorModels.Select(x => new object[]
                {
                    x.Id = cnt++,
                    x.DoorName,
                    $"=\"{x.DeviceAddress}\"",
                    x.ActiveTz,
                    x.PassageTz,
                    x.VerifyMode,
                    x.AntiPassback,
                    x.DeviceType,
                    x.Mpr
                }).ToList();

                var accessibleDoorTxt = new StringBuilder();
                accessibleDoors.ForEach(line => { accessibleDoorTxt.AppendLine(string.Join(",", line)); });

                byte[] buffer = Encoding.UTF8.GetBytes($"{string.Join(",", _headerForAccessibleDoor)}\r\n{accessibleDoorTxt}");
                buffer = Encoding.UTF8.GetPreamble().Concat(buffer).ToArray();

                return buffer;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExportAccessibleDoorsWithTxt");
                return new byte[0];
            }
        }

        public byte[] ExportExcel(IEnumerable<User> data)
        {
            try
            {

                byte[] result;

                //var package = new ExcelPackage();
                using (var package = new ExcelPackage())
                {
                    // add a new worksheet to the empty workbook
                    var worksheet = package.Workbook.Worksheets.Add(UserResource.lblUser); //Worksheet name
                    // add a new worksheet for card data
                    var cardSheet = package.Workbook.Worksheets.Add(UserResource.lblCard);

                    // Get user data to export
                    //var data = FilterDataWithOrder(filter, out totalRecords, out recordsFiltered);
                    //var culture = _httpContext.Request.Query[Constants.Culture];
                    //var formatDate = ApplicationVariables.Configuration[Constants.DateClientFormat + ":" + culture];
                    //var data = FilterDataWithOrder2(filter, out totalRecords, out recordsFiltered, out List<HeaderData> userHeader, Page.User, dateFormat: formatDate);

                    var accountTimezone = GetAccountById(_httpContext.User.GetAccountId()).TimeZone;
                    var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

                    foreach (var i in data)
                    {
                        if (i.BirthDay != null)
                        {
                            DateTime newSelectedDate = i.BirthDay.Value;
                            i.BirthDay = Helpers.ConvertToUserTime(newSelectedDate, offSet);
                        }
                        if (i.ExpiredDate != null)
                        {
                            DateTime newSelectedDate = i.ExpiredDate.Value;
                            i.ExpiredDate = Helpers.ConvertToUserTime(newSelectedDate, offSet);
                        }
                        if (i.EffectiveDate != null)
                        {
                            DateTime newSelectedDate = i.EffectiveDate.Value;
                            i.EffectiveDate = Helpers.ConvertToUserTime(newSelectedDate, offSet);
                        }
                    }

                    var users = data.ToList();

                    //First add the headers for user sheet
                    for (var i = 0; i < _headerExcel.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = _headerExcel[i];
                    }

                    // check plugin camera and add HFaceId
                    if (users.Any() && _unitOfWork.CompanyRepository.CheckCompanyByPlugin(Constants.PlugIn.CameraPlugIn, users[0].CompanyId))
                    //if (users.Any() && _unitOfWork.CompanyRepository.CheckCompanyByPlugin(Constants.PlugIn.CameraPlugIn, _httpContext.User.GetCompanyId()))
                    {
                        worksheet.Cells[1, _headerExcel.Length + 1].Value = CardType.HFaceId.GetDescription();
                    }

                    // Add the headers for card sheet
                    cardSheet.Cells[1, 1].Value = UserResource.lblCardId;
                    cardSheet.Cells[1, 2].Value = UserResource.lblIssueCount;

                    int recordIndex = 2;
                    int cardRow = 2;

                    worksheet.Cells.Style.Numberformat.Format = "@";

                    foreach (var user in users)
                    {
                        //For the User sheet
                        var cards = _unitOfWork.CardRepository.GetByUserId(_httpContext.User.GetCompanyId(), user.Id);
                        if (cards == null || !cards.Any())
                        {
                            var colIndex = 1;
                            //var accessGroup = _unitOfWork.AccessGroupRepository.GetById(user.AccessGroupId);
                            worksheet.Cells[recordIndex, colIndex++].Value = user.UserCode;
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.AccessGroupId < 3 ? ((AccessGroupType)user.AccessGroupId).GetDescription() : accessGroup.Name;
                            // IsMasterCard
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.IsMasterCard;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.BuildingMaster.Any();
                            worksheet.Cells[recordIndex, colIndex++].Value = "";
                            worksheet.Cells[recordIndex, colIndex++].Value = "";
                            worksheet.Cells[recordIndex, colIndex++].Value = "";
                            worksheet.Cells[recordIndex, colIndex++].Value = "";
                            worksheet.Cells[recordIndex, colIndex++].Value = user.FirstName;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.LastName;
                            worksheet.Cells[recordIndex, colIndex++].Value = ((SexType)Convert.ToInt32(user.Sex)).GetDescription();
                            worksheet.Cells[recordIndex, colIndex++].Value = user.Email;

                            // display format
                            //worksheet.Cells[recordIndex, colIndex].Style.Numberformat.Format = "yyyyMMdd";
                            // Value format
                            worksheet.Cells[recordIndex, colIndex++].Value = user.BirthDay?.Date.ToString(Constants.DateTimeFormat.YyyyMMdd);

                            // display format
                            //worksheet.Cells[recordIndex, colIndex].Style.Numberformat.Format = "yyyyMMdd";
                            // Value format
                            worksheet.Cells[recordIndex, colIndex++].Value = user.EffectiveDate?.Date.ToString(Constants.DateTimeFormat.YyyyMMdd);

                            // display format
                            //worksheet.Cells[recordIndex, colIndex].Style.Numberformat.Format = "yyyyMMdd";
                            // Value format
                            worksheet.Cells[recordIndex, colIndex++].Value = user.ExpiredDate?.Date.ToString(Constants.DateTimeFormat.YyyyMMdd);

                            worksheet.Cells[recordIndex, colIndex++].Value = user.Department.DepartName;
                            worksheet.Cells[recordIndex, colIndex++].Value = user.EmpNumber;
                            worksheet.Cells[recordIndex, colIndex++].Value = user.Position;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.PostCode;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.Job;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.Responsibility;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.OfficePhone;
                            worksheet.Cells[recordIndex, colIndex++].Value = user.HomePhone;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.Address;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.Nationality;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.City;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.Remarks;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.Department?.Parent?.DepartName ?? "";
                            worksheet.Cells[recordIndex, colIndex].Value = ((WorkType)Convert.ToInt32(user.WorkType)).GetDescription();
                        }
                        else
                        {
                            var colIndex = 1;
                            var cardIdsString = "";
                            int cardColumn = 1;
                            foreach (var card in cards)
                            {
                                // if (card.CardType == (short)CardType.NFC)
                                // {
                                // }
                                if (cardIdsString == "")
                                {
                                    cardIdsString += card.CardId;
                                }
                                else
                                {
                                    cardIdsString += "," + card.CardId;
                                }
                                // Init column index as 1
                                cardColumn = 1;
                                // Add card data to card sheet
                                cardSheet.Cells[cardRow, cardColumn++].Value = card.CardId;
                                cardSheet.Cells[cardRow, cardColumn].Value = card.IssueCount;
                                cardRow++;
                            }
                            var cardTypeString = "";
                            foreach (var card in cards)
                            {
                                if (cardTypeString == "")
                                {
                                    cardTypeString += Enum.GetName(typeof(CardType), card.CardType);
                                }
                                else
                                {
                                    cardTypeString += "," + Enum.GetName(typeof(CardType), card.CardType);
                                }
                            }
                            //var accessGroup = _unitOfWork.AccessGroupRepository.GetById(user.AccessGroupId);
                            worksheet.Cells[recordIndex, colIndex++].Value = user.UserCode;
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.AccessGroupId < 3 ? ((AccessGroupType)user.AccessGroupId).GetDescription() : accessGroup.Name;
                            // IsMasterCard
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.IsMasterCard;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.BuildingMaster.Any();
                            worksheet.Cells[recordIndex, colIndex++].Value = (short)CardStatus.Normal;
                            worksheet.Cells[recordIndex, colIndex++].Value = "";
                            worksheet.Cells[recordIndex, colIndex++].Value = cardTypeString;
                            worksheet.Cells[recordIndex, colIndex++].Value = cardIdsString;
                            worksheet.Cells[recordIndex, colIndex++].Value = user.FirstName;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.LastName;
                            worksheet.Cells[recordIndex, colIndex++].Value = ((SexType)Convert.ToInt32(user.Sex)).GetDescription();
                            worksheet.Cells[recordIndex, colIndex++].Value = user.Email;

                            // display format
                            //worksheet.Cells[recordIndex, colIndex].Style.Numberformat.Format = "yyyyMMdd";
                            // Value format
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.BirthDay?.ToString(Constants.DateTimeFormat.YyyyMMdd);
                            worksheet.Cells[recordIndex, colIndex++].Value = user.BirthDay?.Date.ToString(Constants.DateTimeFormat.YyyyMMdd);

                            // display format
                            //worksheet.Cells[recordIndex, colIndex].Style.Numberformat.Format = "yyyyMMdd";
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.EffectiveDate?.ToOADate();
                            // Value format
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.EffectiveDate?.ToString(Constants.DateTimeFormat.YyyyMMdd);
                            worksheet.Cells[recordIndex, colIndex++].Value = user.EffectiveDate?.Date.ToString(Constants.DateTimeFormat.YyyyMMdd);

                            // display format
                            //worksheet.Cells[recordIndex, colIndex].Style.Numberformat.Format = "yyyyMMdd";
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.ExpiredDate?.ToOADate();
                            // Value format
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.ExpiredDate?.ToString(Constants.DateTimeFormat.YyyyMMdd);
                            worksheet.Cells[recordIndex, colIndex++].Value = user.ExpiredDate?.Date.ToString(Constants.DateTimeFormat.YyyyMMdd);

                            worksheet.Cells[recordIndex, colIndex++].Value = user.Department.DepartName;
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.Department.DepartNo;
                            worksheet.Cells[recordIndex, colIndex++].Value = user.EmpNumber;
                            worksheet.Cells[recordIndex, colIndex++].Value = user.Position;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.PostCode;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.Job;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.Responsibility;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.OfficePhone;
                            worksheet.Cells[recordIndex, colIndex++].Value = user.HomePhone;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.Address;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.Nationality;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.City;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.Remarks;
                            // worksheet.Cells[recordIndex, colIndex++].Value = user.Department?.Parent?.DepartName ?? "";
                            worksheet.Cells[recordIndex, colIndex].Value = ((WorkType)Convert.ToInt32(user.WorkType)).GetDescription();
                        }

                        // camera plugin
                        if ((string)worksheet.Cells[1, _headerExcel.Length + 1].Value == CardType.HFaceId.GetDescription())
                        {
                            worksheet.Cells[recordIndex, _headerExcel.Length + 1].Value = cards?.FirstOrDefault(m => m.CardType == (short)CardType.HFaceId)?.CardId ?? "";
                        }

                        recordIndex++;
                    }

                    worksheet.Cells.AutoFitColumns();
                    //worksheet.Column(3).Hidden = true;
                    //worksheet.Column(16).Hidden = true;

                    result = package.GetAsByteArray();
                }

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExportExcel");
                return new byte[0];
            }
        }
        
        /// <summary>
        /// Export user data to hancell file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public byte[] ExportHancell(IEnumerable<User> data)
        {
            try
            {

                byte[] result;

                using (XLWorkbook xl = new XLWorkbook())
                {
                    // add a new worksheet to the empty workbook
                    var worksheet = xl.Worksheets.Add($"{UserResource.lblUser}");
                    // add a new worksheet for card data
                    var cardSheet = xl.Worksheets.Add(UserResource.lblCard);

                    //var data = FilterDataWithOrder(filter, out totalRecords, out recordsFiltered);

                    var accountTimezone = GetAccountById(_httpContext.User.GetAccountId()).TimeZone;
                    var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

                    foreach (var i in data)
                    {
                        if (i.BirthDay != null)
                        {
                            DateTime newSelectedDate = i.BirthDay.Value;
                            i.BirthDay = Helpers.ConvertToUserTime(newSelectedDate, offSet);
                        }
                        if (i.ExpiredDate != null)
                        {
                            DateTime newSelectedDate = i.ExpiredDate.Value;
                            i.ExpiredDate = Helpers.ConvertToUserTime(newSelectedDate, offSet);
                        }
                        if (i.EffectiveDate != null)
                        {
                            DateTime newSelectedDate = i.EffectiveDate.Value;
                            i.EffectiveDate = Helpers.ConvertToUserTime(newSelectedDate, offSet);
                        }
                    }
                    var users = data.ToList();

                    //First add the headers for user sheet
                    for (var i = 0; i < _header.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = _header[i];
                    }

                    // Add the headers for card sheet
                    cardSheet.Cell(1, 1).Value = UserResource.lblCardId;
                    cardSheet.Cell(1, 2).Value = UserResource.lblIssueCount;

                    int recordIndex = 2;
                    int cardRow = 2;

                    worksheet.Style.NumberFormat.Format = "@";

                    foreach (var user in users)
                    {
                        //For the User sheet
                        var cards = _unitOfWork.CardRepository.GetByUserId(_httpContext.User.GetCompanyId(), user.Id);
                        if (cards == null || !cards.Any())
                        {
                            var colIndex = 1;
                            //var accessGroup = _unitOfWork.AccessGroupRepository.GetById(user.AccessGroupId);
                            worksheet.Cell(recordIndex, colIndex++).Value = user.UserCode;
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.AccessGroupId < 3 ? ((AccessGroupType)user.AccessGroupId).GetDescription() : accessGroup.Name;
                            // IsMasterCard
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.IsMasterCard;
                            worksheet.Cell(recordIndex, colIndex++).Value = "";
                            worksheet.Cell(recordIndex, colIndex++).Value = Enum.GetName(typeof(CardType), 0);
                            worksheet.Cell(recordIndex, colIndex++).Value = "";
                            worksheet.Cell(recordIndex, colIndex++).Value = user.FirstName;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.LastName;
                            worksheet.Cell(recordIndex, colIndex++).Value = ((SexType)Convert.ToInt32(user.Sex)).GetDescription();
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Email;

                            // display format
                            worksheet.Cell(recordIndex, colIndex).Style.NumberFormat.Format = "yyyy-mm-dd";
                            // Value format
                            worksheet.Cell(recordIndex, colIndex++).Value = user.BirthDay?.Date.ToString(Constants.DateTimeFormat.YyyyyMdDdFormat);

                            // display format
                            worksheet.Cell(recordIndex, colIndex).Style.NumberFormat.Format = "yyyy-mm-dd";
                            // Value format
                            worksheet.Cell(recordIndex, colIndex++).Value = user.EffectiveDate?.Date;

                            // display format
                            worksheet.Cell(recordIndex, colIndex).Style.NumberFormat.Format = "yyyy-mm-dd";
                            // Value format
                            worksheet.Cell(recordIndex, colIndex++).Value = user.ExpiredDate?.Date;

                            worksheet.Cell(recordIndex, colIndex++).Value = user.Department.DepartName;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.EmpNumber;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Position;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.PostCode;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Job;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Responsibility;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.OfficePhone;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.HomePhone;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Address;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Nationality;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.City;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Remarks;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Department?.Parent?.DepartName ?? "";
                            worksheet.Cell(recordIndex, colIndex).Value = ((WorkType)Convert.ToInt32(user.WorkType)).GetDescription();
                        }
                        else
                        {
                            var cardIdsString = "";
                            int cardColumn = 1;
                            foreach (var card in cards)
                            {
                                if (card.CardType == (short)CardType.NFC)
                                {
                                    if (cardIdsString == "")
                                    {
                                        cardIdsString += card.CardId;
                                    }
                                    else
                                    {
                                        cardIdsString += "," + card.CardId;
                                    }
                                    // Init column index as 1
                                    cardColumn = 1;
                                    // Add card data to card sheet
                                    cardSheet.Cell(cardRow, cardColumn++).Value = card.CardId;
                                    cardSheet.Cell(cardRow, cardColumn).Value = card.IssueCount;
                                    cardRow++;
                                }
                            }
                            var colIndex = 1;
                            //var accessGroup = _unitOfWork.AccessGroupRepository.GetById(user.AccessGroupId);
                            worksheet.Cell(recordIndex, colIndex++).Value = user.UserCode;
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.AccessGroupId < 3 ? ((AccessGroupType)user.AccessGroupId).GetDescription() : accessGroup.Name;
                            // IsMasterCard
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.IsMasterCard;
                            worksheet.Cell(recordIndex, colIndex++).Value = (short)CardStatus.Normal;
                            worksheet.Cell(recordIndex, colIndex++).Value = Enum.GetName(typeof(CardType), 0);
                            worksheet.Cell(recordIndex, colIndex++).Value = cardIdsString;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.FirstName;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.LastName;
                            worksheet.Cell(recordIndex, colIndex++).Value = ((SexType)Convert.ToInt32(user.Sex)).GetDescription();
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Email;

                            // display format
                            worksheet.Cell(recordIndex, colIndex).Style.NumberFormat.Format = "yyyy-mm-dd";
                            // Value format
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.BirthDay?.ToString(Constants.DateTimeFormat.YyyyyMdDdFormat);
                            worksheet.Cell(recordIndex, colIndex++).Value = user.BirthDay?.Date.ToString(Constants.DateTimeFormat.YyyyyMdDdFormat);

                            // display format
                            worksheet.Cell(recordIndex, colIndex).Style.NumberFormat.Format = "yyyy-mm-dd";
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.EffectiveDate?.ToOADate();
                            // Value format
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.EffectiveDate?.ToString(Constants.DateTimeFormat.YyyyyMdDdFormat);
                            worksheet.Cell(recordIndex, colIndex++).Value = user.EffectiveDate?.Date;

                            // display format
                            worksheet.Cell(recordIndex, colIndex).Style.NumberFormat.Format = "yyyy-mm-dd";
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.ExpiredDate?.ToOADate();
                            // Value format
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.ExpiredDate?.ToString(Constants.DateTimeFormat.YyyyyMdDdFormat);
                            worksheet.Cell(recordIndex, colIndex++).Value = user.ExpiredDate?.Date;

                            worksheet.Cell(recordIndex, colIndex++).Value = user.Department.DepartName;
                            //worksheet.Cells[recordIndex, colIndex++].Value = user.Department.DepartNo;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.EmpNumber;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Position;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.PostCode;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Job;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Responsibility;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.OfficePhone;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.HomePhone;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Address;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Nationality;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.City;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Remarks;
                            worksheet.Cell(recordIndex, colIndex++).Value = user.Department?.Parent?.DepartName ?? "";
                            worksheet.Cell(recordIndex, colIndex).Value = ((WorkType)Convert.ToInt32(user.WorkType)).GetDescription();
                        }

                        recordIndex++;
                    }

                    using (MemoryStream fs = new MemoryStream())
                    {
                        xl.SaveAs(fs);
                        fs.Position = 0;
                        result = fs.ToArray();
                    }
                }


                //var package = new ExcelPackage();
                using (var package = new ExcelPackage())
                {

                }

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExportHancell");
                return new byte[0];
            }
        }

        ///// <summary>
        ///// Export user data to txt file
        ///// </summary>
        ///// <param name="filter"></param>
        ///// <param name="sortColumn"></param>
        ///// <param name="sortDirection"></param>
        ///// <param name="totalRecords"></param>
        ///// <param name="recordsFiltered"></param>
        ///// <returns></returns>
        public byte[] ExportTxt(IEnumerable<User> data)
        {
            try
            {

                //var data = FilterDataWithOrder(filter, out totalRecords, out recordsFiltered);

                var accountTimezone = GetAccountById(_httpContext.User.GetAccountId()).TimeZone;
                var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

                foreach (var i in data)
                {
                    if (i.BirthDay != null)
                    {
                        DateTime newSelectedDate = i.BirthDay.Value;
                        i.BirthDay = Helpers.ConvertToUserTime(newSelectedDate, offSet);
                    }
                    if (i.ExpiredDate != null)
                    {
                        DateTime newSelectedDate = i.ExpiredDate.Value;
                        i.ExpiredDate = Helpers.ConvertToUserTime(newSelectedDate, offSet);
                    }
                    if (i.EffectiveDate != null)
                    {
                        DateTime newSelectedDate = i.EffectiveDate.Value;
                        i.EffectiveDate = Helpers.ConvertToUserTime(newSelectedDate, offSet);
                    }
                }
                var temp = data.ToList();
                var userCardList = new List<object[]>();

                foreach (var user in temp)
                {
                    //For the User sheet
                    //var cards = user.Card.ToList().Where(m => !m.IsDeleted);
                    var cards = _unitOfWork.CardRepository.GetByUserId(_httpContext.User.GetCompanyId(), user.Id);

                    if (cards == null || !cards.Any())
                    {
                        userCardList.Add(
                            new object[]
                            {
                                $"=\"{user.UserCode}\"",
                                //user.AccessGroup.Name,
                                // IsMasterCard
                                //user.IsMasterCard,
                                "",
                                "",
                                "",
                                user.FirstName,
                                user.LastName,
                                ((SexType)Convert.ToInt32(user.Sex)).GetDescription(),
                                $"=\"{user.Email}\"",
                                user.BirthDay?.ToString(Constants.DateTimeFormat.YyyyMMdd),
                                user.EffectiveDate?.ToString(Constants.DateTimeFormat.YyyyMMdd),
                                user.ExpiredDate?.ToString(Constants.DateTimeFormat.YyyyMMdd),
                                user.Department.DepartName,
                                //user.Department.DepartNo,
                                user.EmpNumber,
                                user.Position,
                                user.PostCode,
                                user.Job,
                                user.Responsibility,
                                user.OfficePhone,
                                user.HomePhone,
                                user.Address,
                                user.Nationality,
                                user.City,
                                user.Remarks,
                                user.Department?.Parent?.DepartName ?? "",
                                ((WorkType)Convert.ToInt32(user.WorkType)).GetDescription()
                            });
                    }
                    else
                    {
                        var cardIdString = "";
                        foreach (var card in cards)
                        {
                            if (card.CardType == (short)CardType.PassCode)
                            {
                                cardIdString = "******";
                            }
                            else
                            {
                                cardIdString = card.CardId;
                            }

                            userCardList.Add(
                            new object[]
                            {
                                $"=\"{user.UserCode}\"",
                                //user.AccessGroup.Name,
                                // IsMasterCard
                                //user.IsMasterCard,
                                ((CardStatus)card.Status).GetDescription(),
                                Enum.GetName(typeof(CardType), 0),
                                $"=\"{cardIdString}\"",
                                user.FirstName,
                                user.LastName,
                                ((SexType)Convert.ToInt32(user.Sex)).GetDescription(),
                                $"=\"{user.Email}\"",
                                user.BirthDay?.ToString(Constants.DateTimeFormat.YyyyMMdd),
                                user.EffectiveDate?.ToString(Constants.DateTimeFormat.YyyyMMdd),
                                user.ExpiredDate?.ToString(Constants.DateTimeFormat.YyyyMMdd),
                                user.Department.DepartName,
                                //user.Department.DepartNo,
                                user.EmpNumber,
                                user.Position,
                                user.PostCode,
                                user.Job,
                                user.Responsibility,
                                user.OfficePhone,
                                user.HomePhone,
                                user.Address,
                                user.Nationality,
                                user.City,
                                user.Remarks,
                                user.Department?.Parent?.DepartName ?? "",
                                ((WorkType)Convert.ToInt32(user.WorkType)).GetDescription()
                            });
                        }
                    }
                }

                // Build the file content
                var userTxt = new StringBuilder();
                userCardList.ForEach(line => userTxt.AppendLine(string.Join(",", line)));

                byte[] buffer = Encoding.UTF8.GetBytes($"{string.Join(",", _header)}\r\n{userTxt}");
                buffer = Encoding.UTF8.GetPreamble().Concat(buffer).ToArray();

                return buffer;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExportTxt");
                return new byte[0];
            }
        }

        public IEnumerable<UserListModel> GetPaginated(UserFilterModel filter, out int totalRecords, out int recordsFiltered, out List<HeaderData> userHeader, string pageName, 
            int departmentId = 0, bool isAssign = true, string dateFormat = Constants.DateTimeFormatDefault, TimeSpan offSet = new TimeSpan())
        {
            try
            {

                var data = FilterDataWithOrderToList(filter, out totalRecords, out recordsFiltered, out userHeader, pageName, departmentId, isAssign, dateFormat, offSet);
                List<UserListModel> result = new List<UserListModel>();

                if (filter.PageSize > 0)
                    result = data.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToList();
                else
                    result = data.ToList();

                List<int> vehicleTypes = new List<int>()
                {
                    (int)CardType.VehicleId,
                    (int)CardType.VehicleMotoBikeId
                };

                List<int> faceTypes = new List<int>()
                {
                    (int)CardType.FaceId,
                    (int)CardType.EbknFaceId,
                    (int)CardType.BioFaceId,
                    (int)CardType.LFaceId,
                    (int)CardType.HFaceId,
                    (int)CardType.TBFace,
                };

                foreach (var user in result)
                {
                    var cardList = GetCardListByUserId(user.Id);

                    user.CardList = cardList.Where(m => !vehicleTypes.Contains(m.CardType) && !faceTypes.Contains(m.CardType) && m.CardStatus < (short)CardStatus.WaitingForPrinting).ToList();
                    user.PlateNumberList = cardList.Where(m => vehicleTypes.Contains(m.CardType)).ToList();
                    user.FaceList = cardList.Where(m => faceTypes.Contains(m.CardType)).ToList();
                }

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginated");
                totalRecords = 0;
                recordsFiltered = 0;
                userHeader = new List<HeaderData>();

                return new List<UserListModel>();
            }
        }
        public IEnumerable<UserModel> GetPaginatedReturnUserModel(UserFilterModel filter, out int totalRecords, out int recordsFiltered)
        {
            try
            {

                var data = _unitOfWork.UserRepository.GetByCompanyId(_httpContext.User.GetCompanyId(), filter.Status);
                List<UserModel> result = new List<UserModel>();
                totalRecords = data.Count();
                recordsFiltered = totalRecords;

                if (filter.PageSize > 0)
                    result = data.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).Select(x => _mapper.Map<UserModel>(x)).ToList();
                else
                    result = data.Select(x => _mapper.Map<UserModel>(x)).ToList();
                List<int> vehicleTypes = new List<int>()
                {
                    (int)CardType.VehicleId,
                    (int)CardType.VehicleMotoBikeId
                };

                List<int> faceTypes = new List<int>()
                {
                    (int)CardType.FaceId,
                    (int)CardType.EbknFaceId,
                    (int)CardType.BioFaceId,
                    (int)CardType.LFaceId,
                    (int)CardType.HFaceId
                };

                foreach (var user in result)
                {
                    var cardList = GetCardListByUserId(user.Id);

                    user.CardList = cardList.ToList();

                }


                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginatedReturnUserModel");
                totalRecords = 0;
                recordsFiltered = 0;

                return new List<UserModel>();
            }
        }

        /// <summary>
        /// Get paginated accessible doors
        /// </summary>
        /// <param name="user"></param>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public List<AccessibleDoorModel> GetPaginatedAccessibleDoors(User user, string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {

                //var companyId = _httpContext.User.GetCompanyId();
                var companyId = user.CompanyId;

                var data = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, user.AccessGroupId)/*.Select(m => m.Icu)*/;

                var accountTypeTemp = _httpContext.User.GetAccountType();
                var accountIdTemp = _httpContext.User.GetAccountId();
                var companyIdTemp = _httpContext.User.GetCompanyId();



                // check plugin
                var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyIdTemp);
                if (plugin != null)
                {

                    PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
                    if (plugIns.DepartmentAccessLevel && accountTypeTemp == (short)AccountType.DynamicRole)
                    {
                        List<int> doorIdList = _unitOfWork.DepartmentDeviceRepository.GetDoorIdsByAccountDepartmentRole(companyIdTemp, accountIdTemp);
                        data = data.Where(x => doorIdList.Contains(x.IcuId));
                    }
                }

                data = data.Include(i => i.Icu.ActiveTz).Include(i => i.Icu.PassageTz);

                //var data = _unitOfWork.IcuDeviceRepository.GetDevicesByAccessGroup(companyId, user.AccessGroupId);

                totalRecords = data.Count();

                //if (!string.IsNullOrEmpty(filter))
                //{
                //    data = data.Where(x =>
                //        x.Name.ToLower().Contains(filter.ToLower()) ||
                //        x.DeviceAddress.ToLower().Contains(filter.ToLower()) ||
                //        ((DeviceType)x.DeviceType).GetDescription().ToLower().Contains(filter.ToLower()) ||
                //        x.ActiveTz.Name.ToLower().Contains(filter.ToLower()) ||
                //        x.PassageTz.Name.ToLower().Contains(filter.ToLower()) ||
                //        ((VerifyMode)x.VerifyMode).GetDescription().ToLower().Contains(filter.ToLower()));
                //}

                if (!string.IsNullOrEmpty(filter))
                {
                    filter = filter.Trim().RemoveDiacritics().ToLower();

                    data = data.Where(x =>
                        x.Icu.Name.RemoveDiacritics().ToLower().Contains(filter)
                         || x.Icu.DeviceAddress.RemoveDiacritics().ToLower().Contains(filter)
                         || x.Icu.ActiveTz.Name.RemoveDiacritics().ToLower().Contains(filter)
                         || x.Icu.PassageTz.Name.RemoveDiacritics().ToLower().Contains(filter));
                }

                // Temporary code
                sortColumn = "Icu." + sortColumn;

                recordsFiltered = data.Count();
                data = data.OrderBy($"{sortColumn} {sortDirection}");
                data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                //var accessibleDoors = data.AsEnumerable<IcuDevice>().Select(Mapper.Map<AccessibleDoorModel>).ToList();
                var accessibleDoors = data.AsEnumerable<AccessGroupDevice>().Select(_mapper.Map<AccessibleDoorModel>).ToList();

                //foreach (var accessDoor in accessibleDoors)
                //{
                //    var accessGroupDevice =
                //        _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupIdAndDeviceId(companyId,
                //            user.AccessGroupId, accessDoor.Id);
                //    accessDoor.AccessGroupTz = accessGroupDevice.Tz.Name;
                //}

                return accessibleDoors;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginatedAccessibleDoors");
                totalRecords = 0;
                recordsFiltered = 0;

                return new List<AccessibleDoorModel>();
            }
        }

        /// <summary>
        /// Filter and order data
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        internal IQueryable<AccessibleDoorModel> FilterAccessibleDoorDataWithOrder(User user, string filter, string sortColumn, string sortDirection,
            out int totalRecords,
            out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var data = _unitOfWork.IcuDeviceRepository.GetDevicesByAccessGroup(companyId, user.AccessGroupId);

            var accountTypeTemp = _httpContext.User.GetAccountType();
            var accountIdTemp = _httpContext.User.GetAccountId();
            var companyIdTemp = _httpContext.User.GetCompanyId();

            if (accountTypeTemp != (short)AccountType.SystemAdmin)
            {
                data = data.Where(m => m.CompanyId == companyIdTemp);
            }

            // check plugin
            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyIdTemp);
            if (plugin != null)
            {

                PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
                if (plugIns.DepartmentAccessLevel && accountTypeTemp == (short)AccountType.DynamicRole)
                {
                    List<int> doorIdList = _unitOfWork.DepartmentDeviceRepository.GetDoorIdsByAccountDepartmentRole(companyIdTemp, accountIdTemp);
                    data = data.Where(x => doorIdList.Contains(x.Id));
                }
            }

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.Where(x =>
                    x.Name.RemoveDiacritics().ToLower().Contains(filter) ||
                    x.DeviceAddress.RemoveDiacritics().ToLower().Contains(filter) ||
                    ((DeviceType)x.DeviceType).GetDescription().RemoveDiacritics().ToLower().Contains(filter) ||
                    x.ActiveTz.Name.RemoveDiacritics().ToLower().Contains(filter) ||
                    x.PassageTz.Name.RemoveDiacritics().ToLower().Contains(filter) ||
                    ((VerifyMode)x.VerifyMode).GetDescription().RemoveDiacritics().ToLower().Contains(filter));
            }

            recordsFiltered = data.Count();
            data = data.OrderBy($"{sortColumn} {sortDirection}");
            return data.AsEnumerable<IcuDevice>().Select(_mapper.Map<AccessibleDoorModel>).AsQueryable();
        }


        internal IQueryable<User> SearchUserDataByFilter(UserFilterModel filter, int departmentId = 0, bool isAssign = true)
        {
            int companyId = _httpContext.User.GetCompanyId();
            int accountId = _httpContext.User.GetAccountId();
            var data = _unitOfWork.UserRepository.GetByCompanyId(companyId, filter.Status);

            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(m => (m.FirstName?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                                       || (m.Position?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                                       || (m.EmpNumber?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)
                                       || m.Card.Any(n => n.CardId?.RemoveDiacritics()?.ToLower()?.Contains(search) == true)).AsQueryable();
            }
            if (filter.ExpiredDate != null && filter.ExpiredDate.Value != DateTime.MinValue)
            {
                data = data.Where(m => filter.ExpiredDate <= m.ExpiredDate.Value);
                if (filter.ExpiredDateEnd != null && filter.ExpiredDateEnd.Value != DateTime.MinValue)
                    data = data.Where(m => m.ExpiredDate <= filter.ExpiredDateEnd.Value);
                else
                    data = data.Where(m => m.ExpiredDate <= filter.ExpiredDate.Value.AddDays(1));
            }

            if (!string.IsNullOrEmpty(filter.FirstName))
            {
                data = data.Where(m => m.FirstName.ToLower().Contains(filter.FirstName.ToLower()));
            }
            if (!string.IsNullOrEmpty(filter.Position))
            {
                data = data.Where(m => m.Position.ToLower().Contains(filter.Position.ToLower()));
            }
            if (!string.IsNullOrEmpty(filter.CardId))
            {
                data = data.Where(m => m.Card.Any(n => n.CardId.ToLower().Contains(filter.CardId.ToLower())));
            }
            if (!string.IsNullOrEmpty(filter.EmpNumber))
            {
                data = data.Where(m => m.EmpNumber.ToLower().Contains(filter.EmpNumber.ToLower()));
            }
            if (filter.DepartmentIds != null && filter.DepartmentIds.Any())
            {
                data = data.Where(m => filter.DepartmentIds.Contains(m.DepartmentId));
            }
            if (filter.WorkTypeIds != null && filter.WorkTypeIds.Any())
            {
                data = data.Where(m => filter.WorkTypeIds.Contains((int)m.WorkType));
            }

            if(filter.AddOns != null)
            {
                if(filter.AddOns.faceAndIris != null && filter.AddOns.faceAndIris.Count >= 0)
                {
                    var typeList = EnumHelper.ToEnumList<CardType_IT100>().Select(e => (short)e.Id).ToList();
                    if(typeList.Except(filter.AddOns.faceAndIris).Count() == 0)
                    {
                        // All selected.
                        // This means that the system should return all user regardless of face data.
                    }
                    else if(filter.AddOns.faceAndIris.Count == 0)
                    {
                        // Not selected.
                        // This means that the system should return all user regardless of face data.
                    }
                    else if(filter.AddOns.faceAndIris.Count > 0)
                    {
                        data = data.Where(m => filter.AddOns.faceAndIris.Contains((short)CardType_IT100.None) || m.Face != null);

                        // Something is selected.
                        data = data.Where(m => (filter.AddOns.faceAndIris.Contains((short)CardType_IT100.None) && string.IsNullOrWhiteSpace(m.Face.First().FaceCode) && string.IsNullOrWhiteSpace(m.Face.First().LeftIrisCode) && string.IsNullOrWhiteSpace(m.Face.First().RightIrisCode))
                        || (filter.AddOns.faceAndIris.Contains((short)CardType_IT100.FaceAndIris) && m.Face.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(m.Face.First().FaceCode) && !string.IsNullOrWhiteSpace(m.Face.First().LeftIrisCode) && !string.IsNullOrWhiteSpace(m.Face.First().RightIrisCode))
                        || (filter.AddOns.faceAndIris.Contains((short)CardType_IT100.FaceOnly) && m.Face.FirstOrDefault() != null && !string.IsNullOrWhiteSpace(m.Face.First().FaceCode) && string.IsNullOrWhiteSpace(m.Face.First().LeftIrisCode) && string.IsNullOrWhiteSpace(m.Face.First().RightIrisCode))
                        || (filter.AddOns.faceAndIris.Contains((short)CardType_IT100.IrisOnly) && m.Face.FirstOrDefault() != null && string.IsNullOrWhiteSpace(m.Face.First().FaceCode) && !string.IsNullOrWhiteSpace(m.Face.First().LeftIrisCode) && !string.IsNullOrWhiteSpace(m.Face.First().RightIrisCode)));
                    }
                }
            }
            
            // check account type dynamic role enable department role
            var accountType = _httpContext.User.GetAccountType();
            if (accountType == (short)AccountType.DynamicRole)
            {
                var departmentIds = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
                if (departmentIds.Any())
                {
                    data = data.Where(x => departmentIds.Contains(x.DepartmentId));
                }
            }

            if (isAssign)
            {
                if (departmentId > 0)
                {
                    data = data.Where(x => x.DepartmentId == departmentId);
                }
            }
            else
            {
                if (departmentId > 0)
                {
                    data = data.Where(x => x.DepartmentId != departmentId);
                }
            }

            return data;
        }


        internal IEnumerable<User> FilterDataWithOrder(UserFilterModel filter, out int totalRecords, out int recordsFiltered,
            int departmentId = 0, bool isAssign = true)
        {
            var data = SearchUserDataByFilter(filter, departmentId, isAssign);

            recordsFiltered = data.Count();
            totalRecords = recordsFiltered;

            // Default sort ( asc - FirstName )
            data = data.OrderBy($"{filter.SortColumn} {filter.SortDirection}");
            if (filter.PageSize > 0)
                data = data.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize);

            return data;
        }

        public IQueryable<UserListModel> FilterDataWithOrderToList(UserFilterModel filter,
            out int totalRecords, out int recordsFiltered, out List<HeaderData> userHeader, string pageName,
            int departmentId = 0, bool isAssign = true, string dateFormat = Constants.DateTimeFormatDefault, TimeSpan offSet = new TimeSpan())
        {
            try
            {

                var data = SearchUserDataByFilter(filter, departmentId, isAssign);

                recordsFiltered = data.Count();
                totalRecords = recordsFiltered;

                int companyId = _httpContext.User.GetCompanyId();
                int accountId = _httpContext.User.GetAccountId();

                userHeader = new List<HeaderData>();
                if (!string.IsNullOrEmpty(pageName))
                {
                    IPageHeader pageHeader = new PageHeader(_configuration, pageName, companyId);
                    userHeader = pageHeader.GetHeaderList(companyId, accountId);
                }

                var resultList = data.Select(u => new UserListModel()
                {
                    Id = u.Id,
                    UserCode = u.UserCode,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    DepartmentName = u.Department != null ? u.Department.DepartName : "",
                    Position = u.Position,
                    EffectiveDate = u.EffectiveDate != null ? u.EffectiveDate.Value.ConvertToUserTime(offSet).ConvertDefaultDateTimeToString(dateFormat)
                        : DateTime.UtcNow.ConvertDefaultDateTimeToString(dateFormat),
                    Status = u.Status,
                    ExpiredDate = u.ExpiredDate != null ? u.ExpiredDate.Value.ConvertToUserTime(offSet).ConvertDefaultDateTimeToString(dateFormat)
                        : DateTime.UtcNow.ConvertDefaultDateTimeToString(dateFormat),
                    WorkTypeName = u.WorkType != null ? ((WorkType)u.WorkType).GetDescription() : "",
                    AccessGroupName = u.AccessGroup != null && u.AccessGroup.ParentId != null && u.AccessGroup.Parent != null ? $"{u.AccessGroup.Parent.Name} *" : u.AccessGroup.Name,
                    ApprovalStatus = ((ApprovalStatus)u.ApprovalStatus).GetDescription(),
                    AccountId = u.AccountId ?? 0,
                    Avatar = u.Avatar,
                    EmployeeNo = u.EmpNumber,
                    NationalIdNumber = u.NationalIdNumber,
                    HomePhone = u.HomePhone,
                });

                string columnName = filter.SortColumn.ToPascalCase();
                var sortHeader = userHeader.FirstOrDefault(m => m.HeaderVariable.Equals(columnName));
                if (!string.IsNullOrEmpty(filter.SortColumn) && totalRecords > 0)
                {
                    if (sortHeader != null && !sortHeader.IsCategory)
                    {
                        resultList = Helpers.SortData<UserListModel>(resultList.AsEnumerable<UserListModel>(), filter.SortDirection, columnName);
                    }
                    else if (sortHeader != null && sortHeader.IsCategory)
                    {
                        if (Int32.TryParse(columnName, out int columnId))
                        {
                            var result = resultList.ToList();

                            if (filter.SortDirection.Equals("asc"))
                                result = result.OrderBy(m => m.CategoryOptions.FirstOrDefault(c => c.Category.Id == columnId)?.OptionName, new NullOrEmptyStringReducer()).ToList();
                            else
                                result = result.OrderByDescending(m => m.CategoryOptions.FirstOrDefault(c => c.Category.Id == columnId)?.OptionName).ToList();

                            resultList = result.AsQueryable();
                        }
                    }
                }

                return resultList;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FilterDataWithOrderToList");
                totalRecords = 0;
                recordsFiltered = 0;
                userHeader = new List<HeaderData>();

                return new List<UserListModel>().AsQueryable();
            }
        }


        public List<CardModel> GetCardListByUserId(int userId)
        {
            try
            {

                var cards = _unitOfWork.CardRepository.GetByUserId(_httpContext.User.GetCompanyId(), userId).Where(m => m.CardStatus < (short)CardStatus.WaitingForPrinting);
                var cardModelList = new List<CardModel>();
                foreach (var card in cards)
                {
                    var cardModel = _mapper.Map<CardModel>(card);
                    if (cardModel.CardType == (short)CardType.PassCode)
                    {
                        cardModel.CardId = Constants.DynamicQr.PassCodeString;
                    }
                    else if (cardModel.CardType == (short)CardType.FaceId)
                    {
                        var faceData = _unitOfWork.CardRepository.GetFacesByUserId((int) card.UserId).FirstOrDefault();
                        if(faceData != null)
                        {
                            cardModel.FaceData = _mapper.Map<FaceModel>(faceData);
                        }
                    }
                    cardModelList.Add(cardModel);
                }
                return cardModelList;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCardListByUserId");
                return new List<CardModel>();
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
                    if (columnCount != _headerExcel.Length)
                    {
                        return new ResultImported()
                        {
                            Result = false,
                            Message = string.Format(MessageResource.msgFileIncorrectFormatColumns, _headerExcel.Length, columnCount)
                        };
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

        public async Task<ResultImported> ImportFile(string type, MemoryStream stream, int companyId, int accountId, string accountName)
        {
            var data = new List<UserImportExportModel>();
            //var fileByte = LoadUsersFromExcelFile(file, data);
            List<ExcelCardData> cardData = new List<ExcelCardData>();

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

                    var whiteListDomain = _configuration.GetSection(Constants.Settings.DefineWhiteListDomain).Get<List<string>>();

                    for (int i = worksheet.Dimension.Start.Row + 1; i <= worksheet.Dimension.End.Row; i++)
                    {
                        var cells = worksheet.Cells;
                        // var cardStatus = Convert.ToString(cells[i, Array.IndexOf(_header, UserResource.lblCardStatus) + 1].Value ?? "");
                        // var cardId = Convert.ToString(cells[i, Array.IndexOf(_header, UserResource.lblCardId) + 1].Value ?? "");
                        var name = Convert.ToString(cells[i, Array.IndexOf(_header, UserResource.lblFirstName) + 1].Value ?? "");

                        // Ignore row that doesnt have name
                        if (name.Equals(""))
                            continue;

                        var item = ReadDataFromExcelSimpleTemplate(worksheet, i, whiteListDomain);
                        data.Add(item);
                    }

                    ExcelWorksheet cardSheet;
                    try
                    {
                        cardSheet = package.Workbook.Worksheets[2];
                        var cells = cardSheet.Cells;

                        for (int i = cardSheet.Dimension.Start.Row + 1; i <= cardSheet.Dimension.End.Row; i++)
                        {
                            var cardId = Convert.ToString(cells[i, 1].Value ?? "").Trim();

                            if (string.IsNullOrWhiteSpace(cardId))
                                continue;

                            var model = new ExcelCardData()
                            {
                                CardId = cardId,
                                IssueCount = Int32.TryParse(Convert.ToString(cells[i, 2].Value).Trim(), out int issCnt) ? issCnt : 0
                            };

                            cardData.Add(model);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"{e.Message}");
                        if (cardData != null && cardData.Any())
                        {
                            Console.WriteLine($"Count of card data : {cardData.Count}");
                            Console.WriteLine($"The last data : {(cardData.Count > 0 ? cardData.LastOrDefault().CardId : "")}");
                        }
                    }
                }

                //var fileName = file.FileName;
                var result = ImportSimpleTemplate(data, companyId, accountId, accountName, cardData);
                //_unitOfWork.Save();
                return result;
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}:{Environment.NewLine} {e.StackTrace}");
                throw;
            }

            ////Save system log
            //var companyId = _httpContext.User.GetCompanyId();
            //var content = string.Format(UserResource.msgImportUserList, DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss"),
            //    _httpContext.User.Identity.Name);
            //_unitOfWork.SystemLogRepository.Add(companyId, SystemLogType.User, ActionLogType.Import, content);
            //_unitOfWork.Save();

            //return fileByte;
            //return fileByte;
        }

        public ResultImported ImportSimpleTemplate(List<UserImportExportModel> listImportUser, int companyId, int accountId, string accountName, List<ExcelCardData> cardData = null)
        {
            IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
            var webSocketService = new WebSocketService();
            var notificationService = new NotificationService(null, null);
            var accountService = new AccountService(unitOfWork, new HttpContextAccessor(), null, _configuration, ApplicationVariables.LoggerFactory.CreateLogger<AccountService>(), null, null, null, null, null, null);
            var userService = new UserService(unitOfWork, new HttpContextAccessor(), null, 
                _configuration, ApplicationVariables.LoggerFactory.CreateLogger<UserService>(), accountService);

            var validUsers = listImportUser.ToList();
            var formatDateTime = "yyyyMMdd HH:mm:ss";
            var formatDate = "yyyyMMdd";

            try
            {
                if (validUsers.Any())
                {
                    var company = unitOfWork.CompanyRepository.GetById(companyId);

                    // check limit add users to company
                    var checkLimit = unitOfWork.CompanyRepository.CheckLimitCountOfUsers(companyId, validUsers.Count);
                    if (!checkLimit.IsAdded)
                    {
                        notificationService.SendMessage(Constants.MessageType.Error,
                            Constants.NotificationType.TransmitDataError, accountName,
                            string.Format(UserResource.msgMaximumAddUser, checkLimit.NumberOfMaximum), company.Id);
                        var errorContent = $"{ActionLogTypeResource.Import} {ActionLogTypeResource.Fail}";
                        var detail = $"{string.Format(UserResource.msgMaximumAddUser, checkLimit.NumberOfMaximum)}";
                        unitOfWork.SystemLogRepository.Add(0, SystemLogType.User, ActionLogType.Import, errorContent, detail, null, companyId, accountId);
                        return new ResultImported()
                        {
                            Result = false,
                            Message = string.Format(UserResource.msgMaximumAddUser, checkLimit.NumberOfMaximum)
                        };
                    }

                    var accountTimezone = unitOfWork.AccountRepository.GetById(accountId).TimeZone;

                    var workingType = unitOfWork.WorkingRepository.GetWorkingTypeDefault(companyId);

                    var passwordDefault = Helpers.GeneratePasswordDefaultWithCompany(company.Name);
                    var roleDefault = unitOfWork.RoleRepository.GetDefaultRoleSettingByCompany(companyId);
                    var result = new ResultImported
                    {
                        Result = false,
                        Message = MessageResource.NotFoundDepartment
                    };

                    var count = 0;
                    int addCount = 0, updateCount = 0;
                    bool isAdd = true;
                    int totalCount = validUsers.Count;

                    List<int> UserIds = new List<int>();

                    // Get all departments
                    var departments = unitOfWork.DepartmentRepository.GetByCompanyId(companyId);
                    var accessGroup = unitOfWork.AppDbContext.AccessGroup.Single(
                                d => d.CompanyId == companyId && d.Type == 1 && !d.IsDeleted);

                    // check point company 
                    var countNewUserImport = 0;
                    foreach (var importedUser in validUsers)
                    {
                        string userCode = "";

                        if (Int32.TryParse(importedUser.UserCode.ToString(), out int userCodeNumber))
                            userCode = String.Format("{0:000000}", userCodeNumber);
                        else
                            userCode = importedUser.UserCode.ToString();

                        if (!userService.IsDuplicatedUserCode(0, userCode, companyId))
                        {
                            countNewUserImport++;
                        }
                    }

                    var seenUserCodes = new HashSet<string>();
                    foreach (var importedUser in validUsers)
                    {
                        if (!string.IsNullOrEmpty(importedUser.UserCode.Value) && seenUserCodes.Contains(importedUser.UserCode.Value))
                        {
                            var errorContent = $"{ActionLogTypeResource.Import} {ActionLogTypeResource.Fail} : {importedUser.FirstName.Value}";
                            var detail = $"{string.Format(MessageResource.msgDuplicatedData, string.Concat(UserResource.lblUserCode,string.Concat("(", importedUser.UserCode.Value, ")")))}";
                            unitOfWork.SystemLogRepository.Add(0, SystemLogType.User, ActionLogType.Import, errorContent, detail, null, companyId, accountId);
                            continue;
                        }
                        else
                        {
                            seenUserCodes.Add(importedUser.UserCode.Value);
                        }
                        User user = null;
                        Department departmentAdded = null;
                        Department parentDepartmentAdded = null;
                        CompanyAccount companyAccountAdded = null;
                        Account accountAdded = null;
                        List<string> duplicateContent = new List<string>();

                        // Check data excel department 
                        if (importedUser.DepartmentName != null && !string.IsNullOrWhiteSpace(importedUser.DepartmentName.Value))
                        {
                            var department = departments.FirstOrDefault(d => d.DepartName.ToLower() == importedUser.DepartmentName?.Value?.ToLower());

                            List<int> cardTypeList = new List<int>();

                            String[] strListCardType = importedUser.CardType?.Value?.Split(',') ?? new string[0];
                            if (strListCardType.Length > 1)
                            {
                                foreach (var cardType in strListCardType)
                                {
                                    var cardTypeTemp = EnumHelper.GetValueByName(typeof(CardType), cardType);
                                    cardTypeList.Add(cardTypeTemp);
                                }
                            }

                            // Create department if there is not the department about the name in excel file.
                            if (department == null)
                            {
                                department = new Department
                                {
                                    DepartName = importedUser.DepartmentName.Value,
                                    CompanyId = companyId,
                                    CreatedBy = accountId,
                                    CreatedOn = DateTime.UtcNow,
                                    UpdatedBy = accountId,
                                    UpdatedOn = DateTime.UtcNow,
                                    IsDeleted = false,
                                    DepartNo = importedUser.DepartmentName.Value,
                                };

                                unitOfWork.DepartmentRepository.Add(department);
                                unitOfWork.Save();

                                departments.Add(department);
                                departmentAdded = department;
                            }

                            // Check department is not null
                            if (department != null)
                            {
                                if (importedUser.UserCode == null || string.IsNullOrEmpty(importedUser.UserCode.Value))
                                {
                                    // var failContent = $"{ActionLogTypeResource.Import} {ActionLogTypeResource.Fail} : {importedUser.FirstName.Value} ({UserResource.lblUser})";
                                    // var failContentDetail = $"{string.Format(MessageResource.Required, UserResource.lblUserCode)}";
                                    //
                                    // unitOfWork.SystemLogRepository.Add(0, SystemLogType.User, ActionLogType.Import,
                                    // failContent, failContentDetail, null, companyId, accountId);
                                    // departments = DeleteDataAdded(parentDepartmentAdded, departmentAdded, companyAccountAdded, accountAdded, departments);
                                    // continue;
                                    importedUser.UserCode.Value = unitOfWork.UserRepository.GetNewUserCode(companyId).ToString();
                                }

                                string userCode = "";

                                if (Int32.TryParse(importedUser.UserCode.ToString(), out int userCodeNumber))
                                    userCode = String.Format("{0:000000}", userCodeNumber);
                                else
                                    userCode = importedUser.UserCode.ToString();

                                if (!userService.IsDuplicatedUserCode(0, userCode, companyId) && !CheckEmailExits(importedUser.Email.Value,unitOfWork, companyId))
                                {
                                    // check user existed with code but card id existed in other user -> set record import fail
                                    string cardExit = CardIdExist(importedUser.CardId.Value, listImportUser, unitOfWork, companyId);
                                    if (!string.IsNullOrEmpty(cardExit))
                                    {
                                        // Write error log message.
                                        var errorContent = $"{ActionLogTypeResource.Import} {ActionLogTypeResource.Fail} : {importedUser.FirstName.Value}";
                                        var detail = $"{string.Format(MessageResource.msgCardIdExisted, string.Concat("(", cardExit, ")"))}";
                                        unitOfWork.SystemLogRepository.Add(0, SystemLogType.User, ActionLogType.Import, errorContent, detail, null, companyId, accountId);
                                        departments = DeleteDataAdded(parentDepartmentAdded, departmentAdded, companyAccountAdded, accountAdded, departments);
                                        continue;
                                    }
                                    // import avatar
                                    if (importedUser.Avatar != null && !string.IsNullOrEmpty(importedUser.Avatar.Value))
                                    {
                                        try
                                        {
                                            string avatarBase64 = Helpers.GetImageBase64FromUrl(importedUser.Avatar.Value);

                                            // Use secure file saving to prevent path traversal attacks
                                            string fileName = $"{importedUser.UserCode}.{Guid.NewGuid().ToString()}.jpg";
                                            string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/avatar";
                                            bool success = FileHelpers.SaveFileImageSecure(avatarBase64, basePath, fileName);

                                            if (success)
                                            {
                                                string path = $"{basePath}/{fileName}";
                                                importedUser.Avatar.SetValue(path);
                                            }
                                            else
                                                importedUser.Avatar.Value = null;
                                        }
                                        catch (Exception e)
                                        {
                                            importedUser.Avatar.Value = null;
                                        }
                                    }

                                    if (string.IsNullOrEmpty(importedUser.Email.Value))
                                    {
                                        user = _mapper.Map<User>(importedUser);

                                        // format date time
                                        user.BirthDay = importedUser.Birthday.Value.ConvertDefaultStringToDateTime(formatDate);
                                        user.EffectiveDate = importedUser.EffectiveDate.Value.ConvertDefaultStringToDateTime(formatDateTime);
                                        user.ExpiredDate = importedUser.ExpiredDate.Value.ConvertDefaultStringToDateTime(formatDateTime);

                                        if (user.EffectiveDate == null || user.ExpiredDate == null)
                                        {
                                            // Write error log message.
                                            var errorContent = $"{ActionLogTypeResource.Import} {ActionLogTypeResource.Fail} : {importedUser.FirstName.Value} ({UserResource.lblUser})";
                                            var detail = $"{string.Format(MessageResource.Required, UserResource.lblEffectiveDate + ", " + UserResource.lblExpiredDate)}";
                                            unitOfWork.SystemLogRepository.Add(user == null ? 0 : user.Id, SystemLogType.User, ActionLogType.Import, errorContent, 
                                                detail, null, companyId, accountId);
                                            departments = DeleteDataAdded(parentDepartmentAdded, departmentAdded, companyAccountAdded, accountAdded, departments);
                                            continue;
                                        }

                                        user.EffectiveDate = user.EffectiveDate.Value.Date.ConvertToSystemTime(accountTimezone);
                                        user.ExpiredDate = user.ExpiredDate.Value.Date.AddSeconds(-1).AddDays(1).ConvertToSystemTime(accountTimezone);

                                        user.UserCode = String.Format("{0:000000}", Convert.ToInt32(user.UserCode));
                                        user.DepartmentId = department.Id;
                                        user.AccessGroupId = accessGroup.Id;
                                        user.CompanyId = companyId;
                                        user.BirthDay = user.BirthDay ?? DateTime.Now;
                                        user.FirstName = importedUser.FirstName.Value;
                                        user.WorkingTypeId = workingType;

                                        // Write Created and Updated data in Thread.
                                        user.CreatedBy = accountId;
                                        user.CreatedOn = DateTime.UtcNow;
                                        user.UpdatedBy = accountId;
                                        user.UpdatedOn = DateTime.UtcNow;

                                        if (importedUser.TypeOfWork?.Value != null)
                                        {
                                            if (int.TryParse(importedUser.TypeOfWork.Value, out int workTypeId))
                                            {
                                                if (Enum.IsDefined(typeof(WorkType), workTypeId))
                                                {
                                                    user.WorkType = (short)workTypeId;
                                                }
                                            }
                                            else
                                            {
                                                var workTypesAllLang = EnumHelper.GetAllDescriptionWithId<WorkType>(UserResource.ResourceManager);
                                                foreach (var workTypes in workTypesAllLang)
                                                {
                                                    if (workTypes.Value.Any(e => e.RemoveAllEmptySpace().ToLower().Equals(importedUser.TypeOfWork.Value.RemoveAllEmptySpace().ToLower())))
                                                    {
                                                        user.WorkType = (short)workTypes.Key;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                        unitOfWork.UserRepository.Add(user);
                                        unitOfWork.Save();

                                        // This is new user.
                                        isAdd = true;
                                        count += 1;
                                        addCount += 1;

                                        // Automatically add QR code
                                        if (!cardTypeList.Contains((short)CardType.QrCode))
                                            userService.AutomationCreateQrCode(user, false, accountId);
                                        // Automatically add NFC Phone Id
                                        if (!cardTypeList.Contains((short)CardType.NFCPhone))
                                            userService.AutomationCreateNFCPhoneId(user, false, accountId);

                                        if (!string.IsNullOrEmpty(importedUser.CardId.Value))
                                        {
                                            String[] strListCard = importedUser.CardId.Value.Split(",");
                                            if (strListCard.Length > 1)
                                            {
                                                foreach (var cardId in strListCard)
                                                {
                                                    var issCnt = cardData.FirstOrDefault(c => c.CardId.ToLower().Equals(cardId.ToLower()))?.IssueCount;
                                                    userService.AddOrUpdateCard(user, importedUser, companyId, cardId, out string message, issCnt);

                                                    if (!string.IsNullOrEmpty(message))
                                                        duplicateContent.Add(message);
                                                }
                                            }
                                            else
                                            {
                                                var issCnt = cardData.FirstOrDefault(c => c.CardId.ToLower().Equals(importedUser.CardId.Value.ToLower()))?.IssueCount;
                                                userService.AddOrUpdateCard(user, importedUser, companyId, importedUser.CardId.Value, out string message, issCnt);

                                                if (!string.IsNullOrEmpty(message))
                                                    duplicateContent.Add(message);
                                            }
                                        }
                                    }
                                    else /*if (EmailIsValid(importedUser.Email.Value))*/ // temp remove verify email valid
                                    {
                                        // Check whether account has already been created in system.
                                        var account = unitOfWork.AppDbContext.Account.FirstOrDefault(a => a.Username.ToLower() == importedUser.Email.Value.ToLower() && !a.IsDeleted);
                                        if (account == null)
                                        {
                                            var currentAccount = unitOfWork.AccountRepository.GetById(accountId);
                                            var accountModel = new AccountModel()
                                            {
                                                Username = importedUser.Email.Value,
                                                CompanyId = companyId,
                                                Password = passwordDefault,
                                                ConfirmPassword = passwordDefault,
                                                Role = (short)roleDefault.TypeId,
                                                PhoneNumber = importedUser.HomePhone.Value,
                                            };

                                            var newAccount = _mapper.Map<Account>(accountModel);
                                            // mapper ignore language.
                                            newAccount.Language = currentAccount.Language;
                                            // mapper ignore timezone.
                                            newAccount.TimeZone = currentAccount.TimeZone;
                                            newAccount.Type = (short)roleDefault.TypeId;

                                            // Write Created and Updated data in Thread.
                                            newAccount.CreatedBy = accountId;
                                            newAccount.CreatedOn = DateTime.UtcNow;
                                            newAccount.UpdatedBy = accountId;
                                            newAccount.UpdatedOn = DateTime.UtcNow;

                                            unitOfWork.AccountRepository.Add(newAccount);
                                            unitOfWork.Save();
                                            accountAdded = newAccount;

                                            // Add Company account.
                                            var newCompanyAccount = new CompanyAccount
                                            {
                                                AccountId = newAccount.Id,
                                                CompanyId = companyId,
                                                DynamicRoleId = roleDefault.Id,
                                                CreatedBy = accountId,
                                                CreatedOn = DateTime.UtcNow,
                                                UpdatedBy = accountId,
                                                UpdatedOn = DateTime.UtcNow
                                            };
                                            unitOfWork.CompanyAccountRepository.Add(newCompanyAccount);
                                            unitOfWork.Save();
                                            companyAccountAdded = newCompanyAccount;
                                            user = _mapper.Map<User>(importedUser);

                                            // format date
                                            user.BirthDay = importedUser.Birthday.Value.ConvertDefaultStringToDateTime(formatDate);
                                            user.EffectiveDate = importedUser.EffectiveDate.Value.ConvertDefaultStringToDateTime(formatDateTime);
                                            user.ExpiredDate = importedUser.ExpiredDate.Value.ConvertDefaultStringToDateTime(formatDateTime);

                                            if (user.EffectiveDate == null || user.ExpiredDate == null)
                                            {
                                                // Write error log message.
                                                var errorContent = $"{ActionLogTypeResource.Import} {ActionLogTypeResource.Fail} : {importedUser.FirstName.Value} ({UserResource.lblUser})";
                                                var details = $"{string.Format(MessageResource.Required, UserResource.lblEffectiveDate + ", " + UserResource.lblExpiredDate)}";
                                                unitOfWork.SystemLogRepository.Add(user == null ? 0 : user.Id, SystemLogType.User, ActionLogType.Import,
                                                errorContent, details, null, companyId, accountId);
                                                departments = DeleteDataAdded(parentDepartmentAdded, departmentAdded, companyAccountAdded, accountAdded, departments);
                                                continue;
                                            }

                                            user.EffectiveDate = user.EffectiveDate.Value.Date.ConvertToSystemTime(accountTimezone);
                                            user.ExpiredDate = user.ExpiredDate.Value.Date.AddSeconds(-1).AddDays(1).ConvertToSystemTime(accountTimezone);

                                            user.UserCode = String.Format("{0:000000}", Convert.ToInt32(user.UserCode));
                                            user.DepartmentId = department.Id;
                                            user.AccessGroupId = accessGroup.Id;
                                            user.CompanyId = companyId;
                                            user.BirthDay = user.BirthDay == null ? DateTime.UtcNow.Date : user.BirthDay.Value.Date;
                                            user.FirstName = importedUser.FirstName.Value;
                                            user.WorkingTypeId = workingType;

                                            user.Email = importedUser.Email.Value;
                                            user.AccountId = newAccount.Id;
                                            user.PermissionType = newAccount.Type;

                                            // Write Created and Updated data in Thread.
                                            user.CreatedBy = accountId;
                                            user.CreatedOn = DateTime.UtcNow;
                                            user.UpdatedBy = accountId;
                                            user.UpdatedOn = DateTime.UtcNow;

                                            if (importedUser.TypeOfWork?.Value != null)
                                            {
                                                if (int.TryParse(importedUser.TypeOfWork.Value, out int workTypeId))
                                                {
                                                    if (Enum.IsDefined(typeof(WorkType), workTypeId))
                                                    {
                                                        user.WorkType = (short)workTypeId;
                                                    }
                                                }
                                                else
                                                {
                                                    var workTypesAllLang = EnumHelper.GetAllDescriptionWithId<WorkType>(UserResource.ResourceManager);
                                                    foreach (var workTypes in workTypesAllLang)
                                                    {
                                                        if (workTypes.Value.Any(e => e.RemoveAllEmptySpace().ToLower().Equals(importedUser.TypeOfWork.Value.RemoveAllEmptySpace().ToLower())))
                                                        {
                                                            user.WorkType = (short)workTypes.Key;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            unitOfWork.UserRepository.Add(user);
                                            unitOfWork.Save();

                                            // This is new user.
                                            isAdd = true;
                                            count += 1;
                                            addCount += 1;

                                            // Automatically add QR code
                                            if (!cardTypeList.Contains((short)CardType.QrCode))
                                                userService.AutomationCreateQrCode(user, false, accountId);
                                            // Automatically add NFC Phone Id
                                            if (!cardTypeList.Contains((short)CardType.NFCPhone))
                                                userService.AutomationCreateNFCPhoneId(user, false, accountId);

                                            // [ERROR] error with some service null exception
                                            // When using async function, should not re-use other service
                                            // send welcome email to employee user
                                            // var token = accountService.GetTokenByAccount(newAccount);
                                            // accountService.SendWelcomeMail(newAccount.Username, user.FirstName, token, companyId);
                                            // add cardId with card type
                                            if (!string.IsNullOrEmpty(importedUser.CardId.Value))
                                            {
                                                String[] strListCard = importedUser.CardId.Value.Split(",");
                                                if (strListCard.Length > 1)
                                                {
                                                    foreach (var cardId in strListCard)
                                                    {
                                                        var issCnt = cardData.FirstOrDefault(c => c.CardId.ToLower().Equals(cardId.ToLower()))?.IssueCount;
                                                        userService.AddOrUpdateCard(user, importedUser, companyId, cardId, out string message, issCnt);

                                                        if (!string.IsNullOrEmpty(message))
                                                            duplicateContent.Add(message);
                                                    }
                                                }
                                                else
                                                {
                                                    var issCnt = cardData.FirstOrDefault(c => c.CardId.ToLower().Equals(importedUser.CardId.Value.ToLower()))?.IssueCount;
                                                    userService.AddOrUpdateCard(user, importedUser, companyId, importedUser.CardId.Value, out string message, issCnt);

                                                    if (!string.IsNullOrEmpty(message))
                                                        duplicateContent.Add(message);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            var userAccount = unitOfWork.AppDbContext.User.Where(u =>
                                                u.CompanyId == companyId && u.AccountId == account.Id && !u.IsDeleted);

                                            // Check account already link to user
                                            if (!userAccount.Any())
                                            {
                                                // Add CompanyAccount to mark user belong to multiple companies
                                                // If account of the same company, then check
                                                CompanyAccount companyAccount = unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompanyAndAccount(companyId, account.Id);
                                                // Account already exist in the company. 
                                                if (companyAccount != null)
                                                {
                                                    // Check if any User already map to this account
                                                }
                                                // Account is not in current company, we add this account to current company with default role
                                                else
                                                {
                                                    companyAccount = new CompanyAccount();
                                                    companyAccount.AccountId = account.Id;
                                                    companyAccount.CompanyId = companyId;
                                                    companyAccount.DynamicRoleId = roleDefault.Id;

                                                    // Write Created and Updated data in Thread.
                                                    companyAccount.CreatedBy = accountId;
                                                    companyAccount.CreatedOn = DateTime.UtcNow;
                                                    companyAccount.UpdatedBy = accountId;
                                                    companyAccount.UpdatedOn = DateTime.UtcNow;

                                                    unitOfWork.CompanyAccountRepository.Add(companyAccount);
                                                    companyAccountAdded = companyAccount;
                                                }

                                                user = _mapper.Map<User>(importedUser);

                                                // format date
                                                user.BirthDay = importedUser.Birthday.Value.ConvertDefaultStringToDateTime(formatDate);
                                                user.EffectiveDate = importedUser.EffectiveDate.Value.ConvertDefaultStringToDateTime(formatDateTime);
                                                user.ExpiredDate = importedUser.ExpiredDate.Value.ConvertDefaultStringToDateTime(formatDateTime);

                                                if (user.EffectiveDate == null || user.ExpiredDate == null)
                                                {
                                                    // Write error log message.
                                                    var errorContent = $"{ActionLogTypeResource.Import} {ActionLogTypeResource.Fail} : {importedUser.FirstName.Value} ({UserResource.lblUser})";
                                                    var detail = $"{string.Format(MessageResource.Required, UserResource.lblEffectiveDate + ", " + UserResource.lblExpiredDate)}";
                                                    unitOfWork.SystemLogRepository.Add(user == null ? 0 : user.Id, SystemLogType.User, ActionLogType.Import,
                                                    errorContent, detail, null, companyId, accountId);
                                                    departments = DeleteDataAdded(parentDepartmentAdded, departmentAdded, companyAccountAdded, accountAdded, departments);
                                                    continue;
                                                }

                                                user.EffectiveDate = user.EffectiveDate.Value.Date.ConvertToSystemTime(accountTimezone);
                                                user.ExpiredDate = user.ExpiredDate.Value.Date.AddSeconds(-1).AddDays(1).ConvertToSystemTime(accountTimezone);

                                                user.UserCode = String.Format("{0:000000}", Convert.ToInt32(user.UserCode));
                                                user.DepartmentId = department.Id;
                                                user.AccessGroupId = accessGroup.Id;
                                                user.CompanyId = companyId;
                                                user.BirthDay = user.BirthDay == null ? DateTime.UtcNow.Date : user.BirthDay.Value.Date;
                                                user.WorkingTypeId = workingType;

                                                user.Email = importedUser.Email.Value;
                                                user.AccountId = account.Id;
                                                user.PermissionType = (short)roleDefault.TypeId;

                                                // Write Created and Updated data in Thread.
                                                user.CreatedBy = accountId;
                                                user.CreatedOn = DateTime.UtcNow;
                                                user.UpdatedBy = accountId;
                                                user.UpdatedOn = DateTime.UtcNow;

                                                if (importedUser.TypeOfWork?.Value != null)
                                                {
                                                    if (int.TryParse(importedUser.TypeOfWork.Value, out int workTypeId))
                                                    {
                                                        if (Enum.IsDefined(typeof(WorkType), workTypeId))
                                                        {
                                                            user.WorkType = (short)workTypeId;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var workTypesAllLang = EnumHelper.GetAllDescriptionWithId<WorkType>(UserResource.ResourceManager);
                                                        foreach (var workTypes in workTypesAllLang)
                                                        {
                                                            if (workTypes.Value.Any(e => e.RemoveAllEmptySpace().ToLower().Equals(importedUser.TypeOfWork.Value.RemoveAllEmptySpace().ToLower())))
                                                            {
                                                                user.WorkType = (short)workTypes.Key;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                                unitOfWork.UserRepository.Add(user);
                                                unitOfWork.Save();

                                                // This is new user.
                                                isAdd = true;
                                                count += 1;
                                                addCount += 1;

                                                // Automatically add QR code
                                                if (!cardTypeList.Contains((short)CardType.QrCode))
                                                    userService.AutomationCreateQrCode(user, false, accountId);
                                                // Automatically add NFC Phone Id
                                                if (!cardTypeList.Contains((short)CardType.NFCPhone))
                                                    userService.AutomationCreateNFCPhoneId(user, false, accountId);

                                                // add cardId with card type
                                                if (!string.IsNullOrEmpty(importedUser.CardId.Value))
                                                {
                                                    String[] strListCard = importedUser.CardId.Value.Split(",");
                                                    if (strListCard.Length > 1)
                                                    {
                                                        foreach (var cardId in strListCard)
                                                        {
                                                            var issCnt = cardData.FirstOrDefault(c => c.CardId.ToLower().Equals(cardId.ToLower()))?.IssueCount;
                                                            userService.AddOrUpdateCard(user, importedUser, companyId, cardId, out string message, issCnt);

                                                            if (!string.IsNullOrEmpty(message))
                                                                duplicateContent.Add(message);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var issCnt = cardData.FirstOrDefault(c => c.CardId.ToLower().Equals(importedUser.CardId.Value.ToLower()))?.IssueCount;
                                                        userService.AddOrUpdateCard(user, importedUser, companyId, importedUser.CardId.Value, out string message, issCnt);

                                                        if (!string.IsNullOrEmpty(message))
                                                            duplicateContent.Add(message);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // This user is exist in system.
                                    // This user should be updated.
                                    user = unitOfWork.UserRepository.GetByUserCode(companyId, userCode);
                                    bool userByEmail = false;
                                    if (user == null && !string.IsNullOrEmpty(importedUser.Email.Value))
                                    {
                                        user = unitOfWork.UserRepository.GetUserByEmail(companyId, importedUser.Email.Value);
                                        userByEmail = true;
                                    }
                                    if (user != null)
                                    {
                                        // check user existed with code but card id existed in other user -> set record import fail
                                        string cardExit = CardIdExist(importedUser.CardId.Value, listImportUser, unitOfWork, companyId, user);
                                        if (!string.IsNullOrEmpty(cardExit))
                                        {
                                            // Write error log message.
                                            var errorContent = $"{ActionLogTypeResource.Import} {ActionLogTypeResource.Fail} : {importedUser.FirstName.Value} ({UserResource.lblUser})";
                                            var detail = $"{string.Format(MessageResource.msgCardIdExisted, string.Concat("(", cardExit, ")"))}";
                                            unitOfWork.SystemLogRepository.Add(user == null ? 0 : user.Id, SystemLogType.User, ActionLogType.Import, errorContent, 
                                                detail, null, companyId, accountId);
                                            departments = DeleteDataAdded(parentDepartmentAdded, departmentAdded, companyAccountAdded, accountAdded, departments);
                                            continue;
                                        }
                                        if (userByEmail && !importedUser.Email.Value.ToLower().Equals(user.Email))
                                        {
                                            var currentAccount = unitOfWork.AccountRepository.GetById(accountId);
                                            var accountModel = new AccountModel()
                                            {
                                                Username = importedUser.Email.Value,
                                                CompanyId = companyId,
                                                Password = passwordDefault,
                                                ConfirmPassword = passwordDefault,
                                                Role = (short)roleDefault.TypeId,
                                                PhoneNumber = importedUser.HomePhone.Value,
                                            };

                                            var newAccount = _mapper.Map<Account>(accountModel);
                                            // mapper ignore language.
                                            newAccount.Language = currentAccount.Language;
                                            // mapper ignore timezone.
                                            newAccount.TimeZone = currentAccount.TimeZone;
                                            newAccount.Type = (short)roleDefault.TypeId;

                                            // Write Created and Updated data in Thread.
                                            newAccount.CreatedBy = accountId;
                                            newAccount.CreatedOn = DateTime.UtcNow;
                                            newAccount.UpdatedBy = accountId;
                                            newAccount.UpdatedOn = DateTime.UtcNow;

                                            unitOfWork.AccountRepository.Add(newAccount);
                                            unitOfWork.Save();

                                            user.AccountId = newAccount.Id;
                                            user.PermissionType = newAccount.Type;
                                            user.Email = importedUser.Email.Value ?? "";
                                        }
                                        if (!string.IsNullOrEmpty(importedUser.UserCode.Value))
                                            user.UserCode = userCode;
                                        
                                        if (!string.IsNullOrEmpty(importedUser.FirstName.Value))
                                            user.FirstName = importedUser.FirstName.Value;

                                        if (importedUser.Sex?.Value != null)
                                            user.Sex = importedUser.Sex.Value.Value;

                                        if (importedUser.Birthday?.Value != null)
                                            user.BirthDay = importedUser.Birthday.Value.ConvertDefaultStringToDateTime(formatDate);

                                        if (importedUser.EffectiveDate?.Value != null)
                                            user.EffectiveDate = importedUser.EffectiveDate.Value.ConvertDefaultStringToDateTime(formatDateTime).Value.Date.ConvertToSystemTime(accountTimezone);
                                        else if (!string.IsNullOrEmpty(importedUser.EffectiveDate.PreValue))
                                            user.EffectiveDate = Int32.TryParse(importedUser.EffectiveDate.PreValue, out int effectiveTime)
                                                ? DateTime.FromOADate(effectiveTime).ConvertToSystemTime(accountTimezone)
                                                : DateTime.ParseExact(importedUser.EffectiveDate.PreValue, Constants.DateTimeFormat.YyyyMMdd, null).ConvertToSystemTime(accountTimezone);

                                        if (importedUser.ExpiredDate?.Value != null)
                                            user.ExpiredDate = importedUser.ExpiredDate.Value.ConvertDefaultStringToDateTime(formatDateTime).Value.Date.AddSeconds(-1).AddDays(1).ConvertToSystemTime(accountTimezone);
                                        else if (!string.IsNullOrEmpty(importedUser.ExpiredDate.PreValue))
                                            user.ExpiredDate = Int32.TryParse(importedUser.ExpiredDate.PreValue, out int expiredDate)
                                                ? DateTime.FromOADate(expiredDate).ConvertToSystemTime(accountTimezone)
                                                : DateTime.ParseExact(importedUser.ExpiredDate.PreValue, Constants.DateTimeFormat.YyyyMMdd, null).AddSeconds(-1).AddDays(1).ConvertToSystemTime(accountTimezone);

                                        if (user.DepartmentId != department.Id)
                                            user.DepartmentId = department.Id;

                                        if (importedUser.EmployeeNumber?.Value != null)
                                            user.EmpNumber = importedUser.EmployeeNumber.Value;

                                        if (!string.IsNullOrEmpty(importedUser.Position.Value))
                                            user.Position = importedUser.Position.Value;



                                        if (!string.IsNullOrEmpty(importedUser.HomePhone.Value))
                                            user.HomePhone = importedUser.HomePhone.Value;

                                        if (importedUser.TypeOfWork?.Value != null)
                                        {
                                            if (int.TryParse(importedUser.TypeOfWork.Value, out int workTypeId))
                                            {
                                                if (Enum.IsDefined(typeof(WorkType), workTypeId))
                                                {
                                                    user.WorkType = (short)workTypeId;
                                                }
                                            }
                                            else
                                            {
                                                var workTypesAllLang = EnumHelper.GetAllDescriptionWithId<WorkType>(UserResource.ResourceManager);
                                                foreach (var workTypes in workTypesAllLang)
                                                {
                                                    if (workTypes.Value.Any(e => e.RemoveAllEmptySpace().ToLower().Equals(importedUser.TypeOfWork.Value.RemoveAllEmptySpace().ToLower())))
                                                    {
                                                        user.WorkType = (short)workTypes.Key;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        // Write Updated data in Thread.
                                        user.UpdatedBy = accountId;
                                        user.UpdatedOn = DateTime.UtcNow;

                                        unitOfWork.UserRepository.Update(user);
                                        //unitOfWork.Save();

                                        // This is an existing user.
                                        isAdd = false;
                                        count += 1;
                                        updateCount += 1;

                                        // Add / Update card data
                                        // Do not delete
                                        if (!string.IsNullOrEmpty(importedUser.CardId.Value))
                                        {
                                            String[] strListCard = importedUser.CardId.Value.Split(",");
                                            if (strListCard.Length > 1)
                                            {
                                                foreach (var cardId in strListCard)
                                                {
                                                    var issCnt = cardData.FirstOrDefault(c => c.CardId.ToLower().Equals(cardId.ToLower()))?.IssueCount;
                                                    userService.AddOrUpdateCard(user, importedUser, companyId, cardId, out string message, issCnt);

                                                    if (!string.IsNullOrEmpty(message))
                                                        duplicateContent.Add(message);
                                                }
                                            }
                                            else
                                            {
                                                var issCnt = cardData.FirstOrDefault(c => c.CardId.ToLower().Equals(importedUser.CardId.Value.ToLower()))?.IssueCount;
                                                userService.AddOrUpdateCard(user, importedUser, companyId, importedUser.CardId.Value, out string message, issCnt);

                                                if (!string.IsNullOrEmpty(message))
                                                    duplicateContent.Add(message);
                                            }
                                        }
                                        // else
                                        // {
                                        //     // delete card
                                        //     var cardList = unitOfWork.CardRepository.GetByUserId(companyId, user.Id);
                                        //     if (cardList.Any())
                                        //     {
                                        //         unitOfWork.CardRepository.DeleteRange(cardList);
                                        //     }
                                        // }

                                        //unitOfWork.Save();
                                    }
                                }

                                if (user != null)
                                {
                                    //Console.WriteLine($"{count} : {user.FirstName} is {(isAdd ? "added" : "updated")} to system.");

                                    if (count % 100 == 0)
                                    {
                                        notificationService.SendMessage(Constants.MessageType.Success,
                                        Constants.NotificationType.TransmitDataSuccess, accountName,
                                        string.Format(MessageResource.msgUserImporting3, count, totalCount), company.Id);
                                    }

                                    UserIds.Add(user.Id);
                                }
                                else
                                {
                                    Console.WriteLine($"##-## {importedUser.FirstName.Value} is failed to {(isAdd ? "added" : "updated")}.");

                                    notificationService.SendMessage(Constants.MessageType.Error,
                                        Constants.NotificationType.TransmitDataError, accountName,
                                        $"{importedUser.FirstName.Value} is failed to add/update.", company.Id);

                                    // delete data added error
                                    departments = DeleteDataAdded(parentDepartmentAdded, departmentAdded, companyAccountAdded, accountAdded, departments);
                                }

                                var successContent = $"{ActionLogTypeResource.Import} {ActionLogTypeResource.Success} : {importedUser.FirstName.Value} ({UserResource.lblUser})";

                                var successContentDetail = duplicateContent.Any() ? string.Join(" ", duplicateContent) : "";

                                // Success to import user.
                                unitOfWork.SystemLogRepository.Add(user == null ? 0 : user.Id, SystemLogType.User, ActionLogType.Import,
                                    successContent, successContentDetail, null, companyId, accountId);
                            }
                        }
                        else
                        {
                            var errorContent = $"{ActionLogTypeResource.Import} {ActionLogTypeResource.Fail} : {importedUser.FirstName.Value}";
                            var detail = $"{string.Format(MessageResource.NotFoundDepartment)}";
                            unitOfWork.SystemLogRepository.Add(0, SystemLogType.User, ActionLogType.Import, errorContent, detail, null, companyId, accountId);
                            continue;
                        }

                        //unitOfWork.Save();
                    }

                    unitOfWork.Save();

                    var accessGroups = unitOfWork.AccessGroupRepository.GetListAccessGroups(companyId)
                        .Where(m => m.Type != (short)AccessGroupType.VisitAccess && m.Type != (short)AccessGroupType.PersonalAccess);

                    List<int> normalDeviceIds = new List<int>();
                    foreach (var eachAccessGroup in accessGroups)
                    {
                        var devices = unitOfWork.IcuDeviceRepository.GetDevicesByAccessGroup(companyId, eachAccessGroup.Id).ToList();
                        if (company != null && !company.AutoSyncUserData)
                            devices = devices.Where(m => m.ConnectionStatus == (short)ConnectionStatus.Online).ToList();

                        var devicesNexpaLPR = devices.Where(m => m.DeviceType == (short)DeviceType.NexpaLPR);
                        foreach (var device in devicesNexpaLPR)
                        {
                            // Send User Data to Device
                            userService.SendAllUserData(device.DeviceAddress, webSocketService, UserIds);
                        }

                        normalDeviceIds.AddRange(devices.Select(m => m.Id));
                    }

                    // send access control (device common instruction)
                    DeviceInstructionQueue deviceInstructionQueue = new DeviceInstructionQueue(unitOfWork, _configuration, webSocketService);
                    normalDeviceIds = normalDeviceIds.Distinct().ToList();
                    deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                    {
                        DeviceIds = normalDeviceIds,
                        Sender = !string.IsNullOrEmpty(accountName) ? accountName : Constants.RabbitMq.SenderDefault,
                        MsgId = Guid.NewGuid().ToString(),
                        MessageType = Constants.Protocol.AddUser,
                        UserIds = UserIds,
                        CompanyCode = company?.Code,
                    });

                    //if (count == validUsers.Count)
                    if ((addCount + updateCount) == totalCount)
                    {
                        result.Result = true;
                        result.Message = String.Format(MessageResource.ImportSuccess, (addCount + updateCount), addCount, updateCount);

                        notificationService.SendMessage(Constants.MessageType.Success,
                                            Constants.NotificationType.TransmitDataSuccess, accountName,
                                            result.Message, company.Id);
                    }
                    else
                    {
                        result.Result = false;
                        result.Message = String.Format(MessageResource.ImportError, (addCount + updateCount), totalCount - (addCount + updateCount));

                        notificationService.SendMessage(Constants.MessageType.Error,
                                            Constants.NotificationType.TransmitDataError, accountName,
                                            result.Message, company.Id);
                    }

                    unitOfWork.Dispose();

                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
                var errorContent = $"{ActionLogTypeResource.Import} {ActionLogTypeResource.Fail}";
                unitOfWork.SystemLogRepository.Add(0, SystemLogType.User, ActionLogType.Import, errorContent, null, null, companyId, accountId);
                throw;
            }
            var res = new ResultImported
            {
                Result = false,
                Message = MessageResource.NoContentInFile
            };

            return res;
        }


        private List<Department> DeleteDataAdded(Department parentDepartmentAdded, Department departmentAdded, CompanyAccount companyAccountAdded, Account accountAdded,
            List<Department> departments)
        {
            List<Department> newDepartments = new List<Department>();
            if (departments != null)
            {
                newDepartments = departments;
            }
            if (parentDepartmentAdded != null)
            {
                _unitOfWork.DepartmentRepository.Delete(m => m.Id == parentDepartmentAdded.Id);
                _unitOfWork.Save();
                newDepartments.Remove(parentDepartmentAdded);
            }
            if (departmentAdded != null)
            {
                _unitOfWork.DepartmentRepository.Delete(m => m.Id == departmentAdded.Id);
                _unitOfWork.Save();
                newDepartments.Remove(departmentAdded);
            }
            if (companyAccountAdded != null)
            {
                _unitOfWork.CompanyAccountRepository.Delete(m => m.Id == companyAccountAdded.Id);
                _unitOfWork.Save();
            }
            if (accountAdded != null)
            {
                _unitOfWork.AccountRepository.Delete(m => m.Id == accountAdded.Id);
                _unitOfWork.Save();
            }

            return newDepartments;
        }

        public void SendAllUserData(string deviceAddress, WebSocketService webSocketService, List<int> userIds = null)
        {
            try
            {

                IcuDevice device = _unitOfWork.IcuDeviceRepository.GetDeviceByAddress(deviceAddress);

                if (device == null)
                {
                    return;
                }

                string sender = _httpContext != null ? _httpContext.User.GetUsername() : Constants.RabbitMq.SenderDefault;
                // Create separate UnitOfWork for background device operations to avoid DbContext concurrency issues
                IUnitOfWork backgroundUnitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                var deviceInstructionQueue = new DeviceInstructionQueue(backgroundUnitOfWork, _configuration, webSocketService);
                var company = _unitOfWork.CompanyRepository.GetById(device.CompanyId ?? 0);
                try
                {
                    deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                    {
                        DeviceIds = new List<int> { device.Id },
                        Sender = sender,
                        MsgId = Guid.NewGuid().ToString(),
                        MessageType = Constants.Protocol.AddUser,
                        UserIds = userIds,
                        CompanyCode = company?.Code,
                    });
                }
                finally
                {
                    backgroundUnitOfWork?.Dispose();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendAllUserData");
            }
        }

        private UserImportExportModel ReadDataFromExcelSimpleTemplate(ExcelWorksheet worksheet, int row, List<string> whiteListDomain)
        {
            var colIndex = 1;
            var cells = worksheet.Cells;
            var model = new UserImportExportModel();

            // User Code
            model.SetUserCode(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // Card status
            model.SetCardStatus(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // issue count
            model.SetIssueCount(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // Card Type
            model.SetCardType(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // Card Id
            model.SetCardId(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // First Name
            model.SetFirstName(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // Sex
            model.SetSex(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // Email
            model.SetEmail(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // Birthday
            model.SetBirthdayDate(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // Effective date
            model.SetEffectiveDate(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // Expired date
            model.SetExpiredDate(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // Department name
            model.SetDepartment(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // Employee number
            model.SetEmployeeNumber(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // Position
            model.SetPosition(Convert.ToString(cells[row, colIndex++].Value));
            // Home phone
            model.SetHomePhone(Convert.ToString(cells[row, colIndex++].Value).Trim());
            // type of work
            model.SetTypeOfWork(Convert.ToString(cells[row, colIndex++].Value).Trim());

            return model;
        }

        private void AddOrUpdateCard(User user, UserImportExportModel importedUser, int companyId, string cardId, out string duplicateContent, int? issueCount = null)
        {
            duplicateContent = "";
            if (!string.IsNullOrEmpty(cardId))
            {
                var card = _unitOfWork.CardRepository.GetByCardId(companyId, cardId);
                if (card == null)
                {
                    // Console.WriteLine(user);

                    String[] listCardId = importedUser.CardId.Value.Split(",");
                    var indexCardId = Array.IndexOf(listCardId, cardId);

                    // card type
                    String[] strListCardType = importedUser.CardType?.Value?.Split(',') ?? new string[0];
                    int cardType = strListCardType.Length > 0
                        ? EnumHelper.GetValueByName(typeof(CardType), strListCardType[indexCardId])
                        : 0;

                    var newCard = new Card()
                    {
                        CompanyId = companyId,
                        CardId = cardId,
                        CardStatus = (short)(importedUser.CardStatus.Value ?? 1),
                        UserId = user.Id,
                        CardType = cardType,
                        IssueCount = (short)(importedUser.IssueCount.Value ?? 0)
                    };

                    _unitOfWork.CardRepository.Add(newCard);
                    _unitOfWork.Save();
                    
                    if (cardType == (short)CardType.VehicleId || cardType == (short)CardType.VehicleMotoBikeId)
                    {
                        // create vehicle
                        var vehicle = new Vehicle()
                        {
                            CompanyId = user.CompanyId,
                            UserId = user.Id,
                            VehicleType = cardType == (short)CardType.VehicleId
                                ? (short)VehicleType.Car
                                : (short)VehicleType.MotoBike,
                            PlateNumber = cardId
                        };
                        _unitOfWork.VehicleRepository.Add(vehicle);
                        _unitOfWork.Save();
                    }
                }
                else
                {
                    if (card.UserId != user.Id)
                        duplicateContent = string.Format(MessageResource.Exist, $"{UserResource.lblCardId}({cardId})");
                    else
                        duplicateContent = "";

                    card.IssueCount = (short)(importedUser.IssueCount.Value ?? 0);
                    card.CardStatus = (short)(importedUser.CardStatus.Value ?? 1);

                    _unitOfWork.CardRepository.Update(card);
                    _unitOfWork.Save();
                }
            }
        }

        /// <summary>
        /// Check whether if card id is existed in the company
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool IsCardIdExist(UserModel model)
        {
            try
            {

                return _unitOfWork.CardRepository.IsCardIdExist(_httpContext.User.GetCompanyId(), model.CardList);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsCardIdExist");
                return false;
            }
        }

        /// <summary>
        /// Check cardId by card type and userId
        /// </summary>
        /// <param name="type"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public bool IsCardExist(short type, int userId)
        {
            try
            {

                return _unitOfWork.CardRepository.IsCardExist(type, userId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsCardExist");
                return false;
            }
        }

        /// <summary>
        /// Check whether user has this card
        /// </summary>
        /// <param name="userId">user index number</param>
        /// <param name="cardId">index number of card, not cardId</param>
        /// <returns> If user has the card, true. 
        ///           If user doesn't have the card, false. </returns>
        public bool IsExistGetCardByUser(int userId, int cardId)
        {
            try
            {

                return _unitOfWork.CardRepository.IsCardIdByUserIdExist(_httpContext.User.GetCompanyId(), userId, cardId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsExistGetCardByUser");
                return false;
            }
        }

        /// <summary>
        /// Check whether if card id is existed in the company
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public bool IsCardIdExist(string cardId)
        {
            try
            {
                return _unitOfWork.CardRepository.IsCardIdExist(_httpContext.User.GetCompanyId(), cardId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsCardIdExist");
                return false;
            }
        }

        /// <summary>
        /// Check whether keypad password is existed in the company
        /// </summary>
        /// <param name="userId"></param>
        /// 
        /// <param name="enctypedKeyPadPassword"></param>
        /// <returns></returns>
        public bool IsKeyPadPasswordExist(int userId, string enctypedKeyPadPassword)
        {
            try
            {

                return _unitOfWork.UserRepository.IsKeyPadPasswordExist(_httpContext.User.GetCompanyId(), userId,
                    enctypedKeyPadPassword);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsKeyPadPasswordExist");
                return false;
            }
        }

        /// <summary>
        /// Generate test data for user
        /// </summary>
        /// <param name="numberOfUser"></param>
        public void GenerateTestData(int numberOfUser)
        {
            try
            {

                var companyId = _httpContext.User.GetCompanyId();

                var defaultAccessGroup =
                    _unitOfWork.AccessGroupRepository.GetDefaultAccessGroup(companyId);

                var defaultDepartment = _unitOfWork.DepartmentRepository.GetDefautDepartmentByCompanyId(companyId);

                var defaultEmployeeId = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)AccountType.Employee, companyId).FirstOrDefault().Id;

                var defaultWorkingTypeId = _unitOfWork.WorkingRepository.GetWorkingTypeDefault(companyId);

                var userCode = _unitOfWork.UserRepository.GetNewUserCode(companyId);

                for (var i = 0; i < numberOfUser; i++)
                {
                    var fakeAccount = new Faker<Account>()
                        .RuleFor(u => u.Password, f => SecurePasswordHasher.Hash("123123"))
                        .RuleFor(u => u.UpdatePasswordOn, f => DateTime.UtcNow)
                        .RuleFor(u => u.Username, f => String.Format(userCode + "@duali.com"));

                    var account = fakeAccount.Generate();
                    _unitOfWork.AccountRepository.Add(account);

                    var fakeCompanyAccount = new Faker<CompanyAccount>()
                        .RuleFor(u => u.CompanyId, f => companyId)
                        .RuleFor(u => u.DynamicRoleId, f => defaultEmployeeId)
                        .RuleFor(u => u.Account, f => account)
                        .RuleFor(u => u.PreferredSystem, f => 0);

                    var companyAccount = fakeCompanyAccount.Generate();
                    _unitOfWork.CompanyAccountRepository.Add(companyAccount);

                    var fakeUser = new Faker<User>()
                        .RuleFor(u => u.CompanyId, f => companyId)
                        .RuleFor(u => u.AccessGroupId, f => defaultAccessGroup.Id)
                        .RuleFor(u => u.Account, f => account)
                        // IsMasterCard
                        //.RuleFor(u => u.IsMasterCard, f => false)
                        //.RuleFor(u => u.CardStatus, f => (short)CardStatus.Normal)
                        .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                        .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
                        .RuleFor(u => u.KeyPadPw, f => Encryptor.Encrypt(f.Random.Replace("########"), _configuration[Constants.Settings.EncryptKey]))
                        //.RuleFor(u => u.CardId, f => f.Random.Replace("****************"))
                        .RuleFor(u => u.Email, f => String.Format(userCode + "@duali.com"))
                        .RuleFor(u => u.DepartmentId, f => defaultDepartment.Id)
                        .RuleFor(u => u.UserCode, f => String.Format("{0:000000}", userCode))
                        .RuleFor(u => u.WorkingTypeId, f => defaultWorkingTypeId)
                        .RuleFor(u => u.EffectiveDate, f => DateTime.UtcNow)
                        .RuleFor(u => u.IssuedDate, f => DateTime.UtcNow)
                        .RuleFor(u => u.ExpiredDate, f => DateTime.UtcNow.AddMonths(12));

                    var user = fakeUser.Generate();
                    _unitOfWork.UserRepository.Add(user);

                    var fakeCard = new Faker<Card>()
                        .RuleFor(u => u.CompanyId, f => companyId)
                        .RuleFor(u => u.CardType, f => 0)
                        .RuleFor(u => u.User, f => user)
                        //.RuleFor(u => u.CardId, f => f.Random.Replace("********"))
                        .RuleFor(u => u.CardId, f => f.Random.Replace("#*#**#*#"))
                        .RuleFor(u => u.IssueCount, f => 0)
                        .RuleFor(u => u.CardStatus, f => (short)0)
                        .RuleFor(u => u.IsMasterCard, f => false)
                        .RuleFor(u => u.Status, f => (short)0)
                        .RuleFor(u => u.IssueCount, f => 0);

                    var card = fakeCard.Generate();
                    _unitOfWork.CardRepository.Add(card);

                    userCode++;
                }

                _unitOfWork.Save();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateTestData");
            }
        }

        /// <summary>
        /// Generate test data for user
        /// </summary>
        /// <param name="numberOfUser"></param>
        public void GenerateTestFaceData(int numberOfUser, bool isAccount = false)
        {
            try
            {

                var companyId = _httpContext.User.GetCompanyId();

                var defaultAccessGroup =
                    _unitOfWork.AccessGroupRepository.GetDefaultAccessGroup(companyId);

                var defaultDepartment = _unitOfWork.DepartmentRepository.GetDefautDepartmentByCompanyId(companyId);

                var defaultEmployeeId = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)AccountType.Employee, companyId).FirstOrDefault().Id;

                var defaultWorkingTypeId = _unitOfWork.WorkingRepository.GetWorkingTypeDefault(companyId);

                var userCode = _unitOfWork.UserRepository.GetNewUserCode(companyId);

                for (var i = 0; i < numberOfUser; i++)
                {
                    Account account = null;

                    if (isAccount)
                    {
                        var fakeAccount = new Faker<Account>()
                        .RuleFor(u => u.Password, f => SecurePasswordHasher.Hash("123123"))
                        .RuleFor(u => u.UpdatePasswordOn, f => DateTime.UtcNow)
                        .RuleFor(u => u.Username, f => String.Format(userCode + "@duali.com"));

                        account = fakeAccount.Generate();
                        _unitOfWork.AccountRepository.Add(account);

                        var fakeCompanyAccount = new Faker<CompanyAccount>()
                            .RuleFor(u => u.CompanyId, f => companyId)
                            .RuleFor(u => u.DynamicRoleId, f => defaultEmployeeId)
                            .RuleFor(u => u.Account, f => account)
                            .RuleFor(u => u.PreferredSystem, f => 0);

                        var companyAccount = fakeCompanyAccount.Generate();
                        _unitOfWork.CompanyAccountRepository.Add(companyAccount);
                    }

                    var fakeUser = new Faker<User>()
                        .RuleFor(u => u.CompanyId, f => companyId)
                        .RuleFor(u => u.AccessGroupId, f => defaultAccessGroup.Id)
                        .RuleFor(u => u.Account, f => account)
                        .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                        .RuleFor(u => u.LastName, (f, u) => f.Name.LastName())
                        .RuleFor(u => u.KeyPadPw, f => Encryptor.Encrypt(f.Random.Replace("########"), _configuration[Constants.Settings.EncryptKey]))
                        .RuleFor(u => u.Email, f => isAccount ? String.Format(userCode + "@duali.com") : "")
                        .RuleFor(u => u.DepartmentId, f => defaultDepartment.Id)
                        .RuleFor(u => u.UserCode, f => String.Format("{0:000000}", userCode))
                        .RuleFor(u => u.WorkingTypeId, f => defaultWorkingTypeId)
                        .RuleFor(u => u.EffectiveDate, f => DateTime.UtcNow)
                        .RuleFor(u => u.IssuedDate, f => DateTime.UtcNow)
                        .RuleFor(u => u.ExpiredDate, f => DateTime.UtcNow.AddMonths(12));

                    var user = fakeUser.Generate();
                    _unitOfWork.UserRepository.Add(user);

                    var fakeCard = new Faker<Card>()
                        .RuleFor(u => u.CompanyId, f => companyId)
                        .RuleFor(u => u.CardType, f => (int)CardType.FaceId)
                        .RuleFor(u => u.User, f => user)
                        .RuleFor(u => u.CardId, f => f.Random.Replace("#*#**#**#"))
                        .RuleFor(u => u.IssueCount, f => 1)
                        .RuleFor(u => u.CardStatus, f => (short)CardStatus.Normal)
                        .RuleFor(u => u.IsMasterCard, f => false)
                        .RuleFor(u => u.Status, f => (short)CardStatus.Normal);

                    var card = fakeCard.Generate();
                    _unitOfWork.CardRepository.Add(card);

                    var fakeFace = new Faker<Face>()
                        .RuleFor(u => u.User, f => user)
                        .RuleFor(u => u.CompanyId, f => companyId)
                        .RuleFor(u => u.FaceCode, f => $"FaceCode_{user.FirstName}")
                        .RuleFor(u => u.FaceImage, f => $"FaceImage_{user.FirstName}")
                        .RuleFor(u => u.FaceSmallImage, f => $"FaceSmallImage_{user.FirstName}")
                        .RuleFor(u => u.LeftIrisImage, f => $"LeftIrisImage_{user.FirstName}")
                        .RuleFor(u => u.RightIrisImage, f => $"RightIrisImage_{user.FirstName}")
                        .RuleFor(u => u.LeftIrisCode, f => $"LeftIrisCode_{user.FirstName}")
                        .RuleFor(u => u.RightIrisCode, f => $"RightIrisCode_{user.FirstName}");

                    var face = fakeFace.Generate();
                    _unitOfWork.AppDbContext.Face.Add(face);

                    userCode++;
                }

                _unitOfWork.Save();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateTestFaceData");
            }
        }

        public void SendDeleteUsersToAllDevice(List<int> userIds, int companyId, List<int> cardTypeList = null)
        {
            try
            {

                var groupMsgId = Guid.NewGuid().ToString();
                var company = _unitOfWork.CompanyRepository.GetById(companyId);
                var devices = _unitOfWork.IcuDeviceRepository.GetByCompany(companyId).ToList();
                if (company != null && !company.AutoSyncUserData)
                    devices = devices.Where(m => m.ConnectionStatus == (short)ConnectionStatus.Online).ToList();

                List<UserLog> allUserLogs = new List<UserLog>();
                var usersToDelete = _unitOfWork.UserRepository.GetByIds(companyId, userIds);

                foreach (var user in usersToDelete)
                {
                    List<UserLog> userLogs = user.Card.Select(_mapper.Map<UserLog>).ToList();

                    allUserLogs.AddRange(userLogs);
                }

                foreach (var device in devices)
                {
                    var cardTypes = cardTypeList ?? Helpers.GetMatchedIdentificationType(device.DeviceType);
                    var totalLogs = allUserLogs.Where(m => cardTypes.Contains(m.CardType)).ToList().SplitList(Helpers.GetMaxSplit(device.DeviceType));

                    if (totalLogs.Any())
                        // Send DELETE. 
                        _accessGroupService.SendAddOrDeleteUser(device.DeviceAddress, userLogs: totalLogs, isAddUser: false);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendDeleteUsersToAllDevice");
            }
        }


        /// <summary>
        /// Send users to all devices.
        /// </summary>
        public void SendUsersToAllDoors(List<int> userIds, bool isAddUser)
        {
            try
            {

                var companyId = _httpContext.User.GetCompanyId();
                var company = _unitOfWork.CompanyRepository.GetById(companyId);
                var devices = _unitOfWork.IcuDeviceRepository.GetDeviceAllInfoByCompany(companyId).ToList();
                if (company != null && !company.AutoSyncUserData)
                    devices = devices.Where(m => m.ConnectionStatus == (short)ConnectionStatus.Online).ToList();

                foreach (var device in devices)
                {
                    var userLogs = _accessGroupService.MakeUserLogData(device, userIds, new List<int>(), !isAddUser);
                    _accessGroupService.SendAddOrDeleteUser(device.DeviceAddress, userLogs: userLogs, isAddUser: isAddUser);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendUsersToAllDoors");
            }
        }

        /// <summary>
        /// Send Identification to device that has specific user
        /// </summary>
        public void SendIdentiToDevice(User user, Card newCard, bool isAdd, List<int> ignoredDeviceIds = null)
        {
            try
            {

                if (user != null && !CanSendData(user))
                {
                    return;
                }

                if (user == null)
                {
                    _logger.LogError("Can not find this user in system");
                    return;
                }

                List<AccessGroupDevice> agDevicesWithType = new List<AccessGroupDevice>();

                switch (newCard.CardType)
                {
                    case (int)CardType.NFC:
                    case (int)CardType.QrCode:
                    case (int)CardType.PassCode:
                    case (int)CardType.NFCPhone:
                    case (int)CardType.FaceId:
                    case (int)CardType.HFaceId:
                    case (int)CardType.FingerPrint:
                    case (int)CardType.LFaceId:
                    case (int)CardType.BioFaceId:
                    case (int)CardType.EbknFingerprint:
                    case (int)CardType.EbknFaceId:
                    case (int)CardType.AratekFingerPrint:
                    case (int)CardType.VehicleId:
                    case (int)CardType.VehicleMotoBikeId:
                        agDevicesWithType = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(user.CompanyId, user.AccessGroupId).ToList();

                        var devices = agDevicesWithType.Select(m => m.Icu).Where(m => ignoredDeviceIds == null || !ignoredDeviceIds.Contains(m.Id)).ToList();

                        agDevicesWithType = agDevicesWithType.Where(m =>
                            devices.Contains(m.Icu)
                            && Helpers.GetMatchedIdentificationType(m.Icu.DeviceType).Contains(newCard.CardType))
                            .ToList();

                        if (agDevicesWithType.Any())
                        {
                            _accessGroupService.SendIdentificationToDevice(agDevicesWithType, user, newCard, isAdd);
                        }

                        break;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendIdentiToDevice");
            }
        }

        /// <summary>
        /// Add new identification to exist user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cardModel"></param>
        /// <returns></returns>
        public Card AddIdentification(User user, CardModel cardModel, bool isSend = true, bool sendAsync = true)
        {
            var card = new Card();
            var contentsDetails = "";

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        card = new Card()
                        {
                            CardId = cardModel.CardId?.ToUpper().RemoveAllEmptySpace(),
                            CardStatus = cardModel.CardStatus,
                            IssueCount = cardModel.IssueCount,
                            CardType = cardModel.CardType,
                            Note = cardModel.Description,
                            UserId = user.Id,
                            CompanyId = user.CompanyId,
                            AccessGroupId = user.AccessGroupId,
                            IsMasterCard = user.IsMasterCard
                        };

                        var identification = "";
                        var checkQrCodeCompany = _unitOfWork.AppDbContext.PlugIn.Where(x => x.CompanyId == user.CompanyId).Select(x => x.PlugIns).FirstOrDefault();
                        var value = JsonConvert.DeserializeObject<PlugIns>(checkQrCodeCompany);
                        switch (cardModel.CardType)
                        {
                            case (short)CardType.NFC:
                                {
                                    identification = "Card";
                                    card.CardId = cardModel.CardId?.ToUpper().RemoveAllEmptySpace();
                                    contentsDetails = $"{UserResource.lblCardId} : {card.CardId}\n" +
                                                      $"{UserResource.lblIssueCount} : {card.IssueCount}";
                                    break;
                                }
                            case (short)CardType.PassCode when value.PassCode:
                                {
                                    identification = "PassCode";
                                    card.CardId = Helpers.GenerateRandomPasswordNumber(Constants.DynamicQr.LenPassCode);
                                    contentsDetails = $"{UserResource.lblCardId} : {card.CardId}\n" +
                                                      $"{UserResource.lblIssueCount} : {card.IssueCount}";
                                    break;
                                }
                            case (short)CardType.QrCode when value.QrCode:
                                {
                                    identification = "QR";
                                    string qrId;
                                    if (!string.IsNullOrEmpty(cardModel.CardId))
                                    {
                                        qrId = cardModel.CardId.RemoveAllEmptySpace();
                                    }
                                    else
                                    {
                                        qrId = GenQrId();
                                        if (user.AccountId != null)
                                        {
                                            List<User> users = _unitOfWork.UserRepository.GetUsersByAccountId((int)user.AccountId);
                                            List<Card> cards = _unitOfWork.CardRepository.GetQrCodeByUsers(users);
                                            if (cards.Count > 0)
                                            {
                                                qrId = cards[0].CardId;
                                            }
                                        }
                                    }

                                    card.CardId = qrId;
                                    break;
                                }
                            case (short)CardType.NFCPhone when value.QrCode:
                                {
                                    identification = "NFCPhone";
                                    string nfcPhoneId;

                                    if (!string.IsNullOrEmpty(cardModel.CardId))
                                    {
                                        nfcPhoneId = cardModel.CardId.RemoveAllEmptySpace();
                                    }
                                    else
                                    {
                                        nfcPhoneId = GenNFCPhoneId();

                                        if (user.AccountId != null)
                                        {
                                            // Get existing user that link to above account
                                            List<User> users = _unitOfWork.UserRepository.GetUsersByAccountId((int)user.AccountId);
                                            // Get list of QR code that link to those above user
                                            List<Card> cards = _unitOfWork.CardRepository.GetNFCPhoneByUsers(users);
                                            if (cards.Count > 0)
                                            {
                                                nfcPhoneId = cards[0].CardId;
                                            }
                                        }
                                    }

                                    card.CardId = nfcPhoneId;
                                    break;
                                }
                            case (short)CardType.FaceId:
                                {
                                    identification = "Face ID";
                                    string faceId = string.IsNullOrEmpty(card.CardId)
                                        ? GenFaceId()
                                        : cardModel.CardId.RemoveAllEmptySpace();

                                    if (user.AccountId != null)
                                    {
                                        // Get existing user that link to above account
                                        List<User> users = _unitOfWork.UserRepository.GetUsersByAccountId((int)user.AccountId);
                                        // Get list of face ID that link to those above user
                                        List<Card> cards = _unitOfWork.CardRepository.GetFaceIdByUsers(users);
                                        if (cards.Count > 0)
                                        {
                                            faceId = cards[0].CardId;
                                        }
                                    }

                                    // Save 'face' data if the faceDataList value is not null.
                                    if (cardModel.FaceData != null)
                                    {
                                        Face face = _mapper.Map<Face>(cardModel.FaceData);
                                        face.CompanyId = user.CompanyId;
                                        face.UserId = user.Id;

                                        _unitOfWork.AppDbContext.Face.Add(face);
                                    }
                                    card.CardId = faceId;
                                    break;
                                }
                            case (short)CardType.HFaceId when value.CameraPlugIn:
                                {
                                    identification = "HFaceId";
                                    card.CardId = cardModel.CardId?.ToUpper().RemoveAllEmptySpace();
                                    card.UpdatedBy = 1;
                                    card.CreatedBy = 1;
                                    break;
                                }
                            case (short)CardType.VehicleId when value.VehiclePlugIn:
                                {
                                    identification = "VehicleId";
                                    card.CardId = cardModel.CardId?.ToUpper().RemoveAllEmptySpace();
                                    card.UpdatedBy = _httpContext != null ? _httpContext.User.GetAccountId() : 0;
                                    card.CreatedBy = _httpContext != null ? _httpContext.User.GetAccountId() : 0;
                                    break;
                                }
                            case (short)CardType.VehicleMotoBikeId when value.VehiclePlugIn:
                                {
                                    identification = "VehicleMotoBikeId";
                                    card.CardId = cardModel.CardId?.ToUpper().RemoveAllEmptySpace();
                                    card.UpdatedBy = _httpContext != null ? _httpContext.User.GetAccountId() : 0;
                                    card.CreatedBy = _httpContext != null ? _httpContext.User.GetAccountId() : 0;
                                    break;
                                }
                            case (short)CardType.FingerPrint:
                                {
                                    identification = "FingerPrint";
                                    card.CardId = user.Id.ToString();
                                    card.UpdatedBy = _httpContext != null ? _httpContext.User.GetAccountId() : 0;
                                    card.CreatedBy = _httpContext != null ? _httpContext.User.GetAccountId() : 0;
                                    break;
                                }
                            case (short)CardType.AratekFingerPrint:
                                {
                                    identification = "AratekFingerPrint";
                                    card.CardId = "AF" + user.Id;
                                    card.UpdatedBy = _httpContext != null ? _httpContext.User.GetAccountId() : 0;
                                    card.CreatedBy = _httpContext != null ? _httpContext.User.GetAccountId() : 0;
                                    break;
                                }
                            case (short)CardType.VNID:
                            {
                                identification = "VNID";
                                card.CardId = cardModel.CardId?.ToUpper().RemoveAllEmptySpace();
                                contentsDetails = $"{UserResource.lblCardId} : {card.CardId}\n" +
                                                  $"{UserResource.lblIssueCount} : {card.IssueCount}";
                                break;
                            }
                        }

                        if (!string.IsNullOrEmpty(card.CardId))
                        {
                            //Check whether the cardId is duplicated.
                            var oldCard = _unitOfWork.CardRepository.GetByCardId(card.CompanyId, card.CardId);
                            if (oldCard == null)
                            {
                                _unitOfWork.CardRepository.Add(card);
                                _unitOfWork.Save();

                                // if card type is fingerprint, we add templates of fingerprint to this card
                                if (card.CardType == (short)CardType.FingerPrint || card.CardType == (short)CardType.AratekFingerPrint)
                                {
                                    if (cardModel.FingerPrintData != null && cardModel.FingerPrintData.Count > 0)
                                    {
                                        _unitOfWork.CardRepository.SetFingerPrintToCard(card.Id, cardModel.FingerPrintData);
                                    }
                                }
                            }

                            //Save system log
                            var content = string.Format(UserResource.msgAddNewCertification, identification, $"{user.FirstName} ({user.UserCode})");
                            _unitOfWork.SystemLogRepository.Add(user.Id, SystemLogType.User, ActionLogType.Update, content, contentsDetails, null, user.CompanyId);

                            _unitOfWork.Save();
                            transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        transaction.Rollback();
                        card = null;
                        isSend = false;
                    }
                }
            });

            // API must send a new card to device that the user is included.
            if (isSend)
            {
                // get devices by user
                var company = _unitOfWork.CompanyRepository.GetById(user.CompanyId);
                var devices = _unitOfWork.AccessGroupDeviceRepository
                    .GetByAccessGroupId(user.CompanyId, user.AccessGroupId)
                    .Select(m => m.Icu)
                    .Where(m => company.AutoSyncUserData || m.ConnectionStatus == (short)ConnectionStatus.Online)
                    .ToList();

                if (devices.Count > 0)
                {
                    string sender = _httpContext != null ? _httpContext.User.GetUsername() : "";
                    ThreadSendCardToDevice(new List<int>() { card.Id }, new List<int>() { user.Id }, devices, sender, sendAsync: sendAsync);
                }
            }

            return card;
        }

        public void AutomationCreateQrCode(User user, bool isSend = true, int? accountId = null)
        {
            try
            {

                var companyId = user.CompanyId;
                var createdBy = accountId ?? _httpContext?.User?.GetAccountId();
                var checkQrCodeCompany = _unitOfWork.AppDbContext.PlugIn.Where(x => x.CompanyId == companyId).Select(x => x.PlugIns).FirstOrDefault();
                var plugIns = JsonConvert.DeserializeObject<PlugIns>(checkQrCodeCompany);
                string qrId;

                // check plugin qr code
                if (!plugIns.QrCode)
                {
                    return;
                }

                // check enable/disable using dynamic qr-code
                var qrCodeSetting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.AutoGenerateQrCode, companyId);
                if (qrCodeSetting == null || !bool.TryParse(Helpers.GetStringFromValueSetting(qrCodeSetting.Value), out bool valueQrCode) || !valueQrCode)
                {
                    return;
                }

                if (user.AccountId.HasValue)
                {
                    // If this user link to an Account that already has QR code, we should make QR code we same id
                    // Get existing user that link to above account
                    List<User> users = _unitOfWork.UserRepository.GetUsersByAccountId((int)user.AccountId);
                    // Get list of QR code that link to those above user
                    List<Card> cards = _unitOfWork.CardRepository.GetQrCodeByUsers(users);

                    qrId = cards.Count > 0 ? cards[0].CardId : GenQrId();
                }
                else
                {
                    qrId = GenQrId();
                }

                Card cardModel = new Card
                {
                    CardId = qrId,
                    CompanyId = companyId,
                    CreatedOn = DateTime.Now,
                    CreatedBy = createdBy ?? 0,
                    UserId = user.Id,
                    CardType = (short)CardType.QrCode
                };

                //Check whether the cardId is duplicated.
                var oldCard = _unitOfWork.CardRepository.GetByCardId(cardModel.CompanyId, cardModel.CardId);
                if (oldCard == null)
                {
                    _unitOfWork.CardRepository.Add(cardModel);
                    _unitOfWork.Save();
                }
                // API must send a new card to device that the user is included.
                if (isSend) SendIdentiToDevice(user, cardModel, true);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AutomationCreateQrCode");
            }
        }

        public void AutomationCreateNFCPhoneId(User user, bool isSend = true, int? accountId = null)
        {
            try
            {

                var companyId = user.CompanyId;
                var createdBy = accountId ?? _httpContext?.User?.GetAccountId();

                var checkQrCodeCompany = _unitOfWork.AppDbContext.PlugIn.Where(x => x.CompanyId == companyId).Select(x => x.PlugIns).FirstOrDefault();
                var value = JsonConvert.DeserializeObject<PlugIns>(checkQrCodeCompany);
                string nfcPhoneId;

                // check plugin nfc (qr code)
                if (!value.QrCode)
                {
                    return;
                }

                // check enable/disable using dynamic qr-code
                var qrCodeSetting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.AutoGenerateQrCode, companyId);
                if (qrCodeSetting == null || !bool.TryParse(Helpers.GetStringFromValueSetting(qrCodeSetting.Value), out bool valueQrCode) || !valueQrCode)
                {
                    return;
                }

                // check automatic generate NFC Phone
                var autoGenNFCPhoneSetting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.AutoGenerateNfcPhone, companyId);
                if (autoGenNFCPhoneSetting == null || !bool.TryParse(Helpers.GetStringFromValueSetting(autoGenNFCPhoneSetting.Value), out bool valueAutoGenNFCPhone) || !valueAutoGenNFCPhone)
                {
                    return;
                }

                if (user.AccountId.HasValue)
                {
                    // If this user is linked to an Account that already has NFC Phone Id, we should make NFC Phone Id same as existing.
                    // Get existing user that link to above account
                    List<User> users = _unitOfWork.UserRepository.GetUsersByAccountId((int)user.AccountId);
                    // Get list of NFC phone Id that link to those above user
                    List<Card> cards = _unitOfWork.CardRepository.GetNFCPhoneByUsers(users);

                    nfcPhoneId = cards.Count > 0 ? cards[0].CardId : GenNFCPhoneId();
                }
                else
                {
                    nfcPhoneId = GenNFCPhoneId();
                }

                Card cardModel = new Card
                {
                    CardId = nfcPhoneId,
                    CompanyId = companyId,
                    CreatedOn = DateTime.Now,
                    CreatedBy = createdBy ?? 0,
                    UserId = user.Id,
                    CardType = (short)CardType.NFCPhone
                };

                //Check whether the cardId is duplicated.
                var oldCard = _unitOfWork.CardRepository.GetByCardId(cardModel.CompanyId, cardModel.CardId);
                if (oldCard == null)
                {
                    _unitOfWork.AppDbContext.Card.Add(cardModel);
                }

                _unitOfWork.AppDbContext.SaveChanges();
                // API must send a new card to device that the user is included.
                if (isSend) SendIdentiToDevice(user, cardModel, true);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AutomationCreateNFCPhoneId");
            }
        }

        public void UpdateCardByUser(int userId, int cardIndex, CardModel model)
        {
            try
            {

                var user = _unitOfWork.UserRepository.GetByUserId(_httpContext.User.GetCompanyId(), userId);
                var card = _unitOfWork.CardRepository.GetById(cardIndex);

                Card oldCard = new Card()
                {
                    CardId = card.CardId,
                    CompanyId = card.CompanyId,
                    CardType = card.CardType,
                    CardStatus = card.CardStatus,
                };

                // CardId should not be changed.
                if (!card.CardId.Equals(model.CardId) && card.CardType != (int)CardType.VehicleId && card.CardType != (int)CardType.VehicleMotoBikeId)
                {
                    throw new Exception(string.Format(MessageResource.msgCannotUpdate, UserResource.lblCardId));
                }

                short oldStatus = card.CardStatus;

                if (userId == 0) card.UserId = null;
                else card.UserId = userId;

                card.CardId = model.CardId;
                card.CompanyId = _httpContext.User.GetCompanyId();
                card.IssueCount = model.IssueCount;
                card.CardStatus = model.CardStatus;
                card.CardType = model.CardType;
                card.Note = model.Description;

                if (card.CardType == (short)CardType.FingerPrint || card.CardType == (short)CardType.AratekFingerPrint)
                {
                    _unitOfWork.CardRepository.SetFingerPrintToCard(card.Id, model.FingerPrintData);
                }

                _unitOfWork.CardRepository.Update(card);
                _unitOfWork.Save();

                var sendType = JudgeSendType(oldStatus, model.CardStatus);
                switch (sendType)
                {
                    case 1:
                        SendIdentiToDevice(user, oldCard, false);
                        break;
                    case 2:
                        SendIdentiToDevice(user, oldCard, false);
                        SendIdentiToDevice(user, card, true);
                        break;
                    case 3:
                        // Nothing to do
                        break;
                    case 4:
                        SendIdentiToDevice(user, card, true);
                        break;
                    case 0:
                        // wrong type case
                        break;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateCardByUser");
            }
        }

        internal int JudgeSendType(short oldStatus, short newStatus)
        {
            List<short> addList = new List<short>()
            {
                (short) CardStatus.Normal,
                (short) CardStatus.Transfer,
                (short) CardStatus.Lost,
                (short) CardStatus.Retire,
                (short) CardStatus.InValid
            };

            List<short> deleteList = new List<short>()
            {
                //(short) CardStatus.Lost,
                //(short) CardStatus.Retire,
                //(short) CardStatus.InValid
            };

            if (addList.Contains(oldStatus))
            {
                if (deleteList.Contains(newStatus))
                {
                    // Send DELETE user
                    return 1;
                }
                else if (addList.Contains(newStatus))
                {
                    // Send DELETE -> ADD user
                    return 2;
                }
                else
                {
                    // wrong case;
                    return 0;
                }
            }
            else if (deleteList.Contains(oldStatus))
            {
                if (deleteList.Contains(newStatus))
                {
                    // Nothing to do
                    return 3;
                }
                else if (addList.Contains(newStatus))
                {
                    // Send ADD user
                    return 4;
                }
                else
                {
                    // wrong case;
                    return 0;
                }
            }
            else
            {
                // wrong case.
                return 0;
            }

        }

        /// <summary>
        /// Delete user's card
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cardIndex">index number of card, not cardId</param>
        public void DeleteCardByUser(User user, int cardIndex)
        {
            try
            {

                var card = _unitOfWork.CardRepository.GetById(cardIndex);

                if (card.CardType == (int)CardType.FaceId)
                {
                    // Delete "Face" data too.
                    var face = _unitOfWork.AppDbContext.Face.Where(m => m.UserId == user.Id);
                    if (face.Any())
                        _unitOfWork.AppDbContext.Face.RemoveRange(face);
                }

                // send deleted identification to device
                string sender = _httpContext != null ? _httpContext.User.GetUsername() : "SYSTEM";
                SendUpdateUsersToAllDoors(new List<User>() { user }, sender, false, new List<Card>() { card });

                // update IsDeleted in card
                //card.IsDeleted = true;
                //card.UpdatedOn = DateTime.UtcNow;
                //_unitOfWork.CardRepository.Update(card);
                _unitOfWork.CardRepository.DeleteFromSystem(card);

                //Save system log
                var content = string.Format(UserResource.msgDeleteCard, card.CardId, $"{user?.FirstName} ({user?.UserCode})");
                _unitOfWork.SystemLogRepository.Add(user?.Id ?? 0, SystemLogType.User, ActionLogType.Update, content, null, null, card.CompanyId);
                _unitOfWork.Save();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteCardByUser");
            }
        }

        public string GenQrId()
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenQrId");
                return null;
            }
        }

        public string GenNFCPhoneId()
        {
            try
            {

                var chars = Constants.Settings.CharacterGenNfcId;
                var stringChars = new char[Constants.Settings.LengthCharacterGenNFCPhoneId];
                var random = new Random();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                var finalString = new String(stringChars);
                if (_unitOfWork.CardRepository.CheckIsExistedQrCode(finalString) != true)
                {
                    GenNFCPhoneId();
                }
                return finalString;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenNFCPhoneId");
                return null;
            }
        }

        public string GenFaceId()
        {
            try
            {

                var chars = Constants.Settings.CharacterGenQrCode;
                var stringChars = new char[Constants.Settings.LengthCharacterGenFaceId];
                var random = new Random();

                for (int i = 0; i < stringChars.Length; i++)
                {
                    stringChars[i] = chars[random.Next(chars.Length)];
                }

                var finalString = new String(stringChars);
                if (_unitOfWork.CardRepository.CheckIsExistedQrCode(finalString) != true)
                {
                    GenFaceId();
                }
                return finalString;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenFaceId");
                return null;
            }
        }

        public bool CheckChangeADD(User user, UserModel model, bool onlyCheck, ref List<string> changes)
        {
            try
            {

                bool haveToSend = false;
                if (model.Id != 0)
                {
                    // Check AccessGroup
                    if (user.AccessGroupId != model.AccessGroupId)
                    {
                        if (!onlyCheck)
                        {
                            string oldAgName = "";
                            string newAgName = "";

                            var oldAg = _unitOfWork.AccessGroupRepository.GetById(user.AccessGroupId);
                            oldAgName = oldAg.Name;
                            if (oldAg.Type == (short)AccessGroupType.PersonalAccess && oldAg.ParentId.HasValue)
                            {
                                oldAg = _unitOfWork.AccessGroupRepository.GetById(oldAg.ParentId.Value);
                                oldAgName = oldAg.Name + " *";
                            }
                            var oldAgd = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(user.CompanyId, user.AccessGroupId);
                            var oldAgdNames = oldAgd.Select(m => m.Icu.Name).ToList();

                            var newAg = _unitOfWork.AccessGroupRepository.GetById(model.AccessGroupId);
                            newAgName = newAg.Name;
                            if (newAg.Type == (short)AccessGroupType.PersonalAccess && newAg.ParentId.HasValue)
                            {
                                newAg = _unitOfWork.AccessGroupRepository.GetById(newAg.ParentId.Value);
                                newAgName = newAg.Name + " *";
                            }
                            var newAgd = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(user.CompanyId, model.AccessGroupId);
                            var newAgdNames = newAgd.Select(m => m.Icu.Name).ToList();

                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblAccessGroup, oldAgName, newAgName));
                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblAccessibleDoors, string.Join("<br />", oldAgdNames), string.Join("<br />", newAgdNames)));
                        }

                        haveToSend = true;
                    }

                    // Check Avatar 
                    if (user.Avatar != model.Avatar)
                    {
                        if (!onlyCheck)
                        {
                            ////Delete previous image
                            //string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                            //if (!string.IsNullOrEmpty(user.Avatar))
                            //{
                            //    FileHelpers.DeleteFileFromLink(user.Avatar.Replace($"{connectionApi}/static/", ""));
                            //}

                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblAvatar, 
                                user.Avatar != null ? user.Avatar.GetFileNameFromPath() : "", 
                                model.Avatar != null ? model.Avatar.GetFileNameFromPath() : ""));
                        }
                        ////If updated only Avatar, do not send user to devices
                        //haveToSend = true;
                    }

                    // Check Department
                    if (user.DepartmentId != model.DepartmentId)
                    {
                        if (!onlyCheck)
                        {
                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblDepartment, 
                                _unitOfWork.DepartmentRepository.GetById(user.DepartmentId).DepartName, 
                                _unitOfWork.DepartmentRepository.GetById(model.DepartmentId ?? 0).DepartName));
                        }

                        haveToSend = true;
                    }

                    // Check EffectiveDate
                    var oldEffectiveDate = user.EffectiveDate.HasValue ? user.EffectiveDate.Value.ConvertDefaultDateTimeToString() : "";
                    if (oldEffectiveDate != model.EffectiveDate)
                    {
                        if(!string.IsNullOrEmpty(oldEffectiveDate))
                        {

                            if (!onlyCheck)
                            {
                                changes.Add(Helpers.CreateChangedValueContents(UserResource.lblEffectiveDate, oldEffectiveDate, model.EffectiveDate));
                            }
                            haveToSend = true;
                        }

                    }

                    // Check ExpiredDate
                    var oldExpiredDate = user.ExpiredDate.HasValue ? user.ExpiredDate.Value.ConvertDefaultDateTimeToString() : "";
                    if (oldExpiredDate != model.ExpiredDate)
                    {
                        if(!string.IsNullOrEmpty(oldEffectiveDate))
                        {

                            if (!onlyCheck)
                            {
                                changes.Add(Helpers.CreateChangedValueContents(UserResource.lblExpiredDate, oldExpiredDate, model.ExpiredDate));
                            }
                        }

                        haveToSend = true;
                    }

                    // Check UserName
                    if (!user.FirstName.Equals(model.FirstName.Trim()))
                    {
                        if (!onlyCheck)
                        {
                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblName, user.FirstName, model.FirstName));
                        }

                        haveToSend = true;
                    }

                    // Check Status
                    // Valid / Invalid
                    if (user.Status != model.Status)
                    {
                        if (!onlyCheck)
                        {
                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblStatus, ((UserStatus)user.Status).GetDescription(), ((UserStatus)model.Status).GetDescription()));
                        }

                        haveToSend = true;
                    }

                    // Check WorkType (if company enable plugin time attendance)
                    if (_unitOfWork.CompanyRepository.CheckCompanyByPlugin(Constants.PlugIn.TimeAttendance, user.CompanyId) && user.WorkType != model.WorkType)
                    {
                        if (!onlyCheck)
                        {
                            var oldWorkTypeName = "";
                            var newWorkTypeName = "";
                            if (user.WorkType != null)
                            {
                                oldWorkTypeName = ((WorkType)user.WorkType).GetDescription();
                            }
                            newWorkTypeName = ((WorkType)model.WorkType).GetDescription();

                            // for user
                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblWorkType, oldWorkTypeName, newWorkTypeName));
                        }

                        haveToSend = true;
                    }
                }
                return haveToSend;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckChangeADD");
                return false;
            }
        }

        public bool CheckChange(User user, UserModel model, bool onlyCheck, ref List<string> changes)
        {
            try
            {

                bool haveToSend = false;

                if (model.Id != 0)
                {
                    // Check UserName
                    if (!user.FirstName.Equals(model.FirstName.Trim()))
                    {
                        if (!onlyCheck)
                        {
                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblName, user.FirstName, model.FirstName));
                        }

                        haveToSend = true;
                    }

                    // Check AccessGroup
                    if (user.AccessGroupId != model.AccessGroupId)
                    {
                        if (!onlyCheck)
                        {
                            string oldAgName = "";
                            string newAgName = "";

                            var oldAg = _unitOfWork.AccessGroupRepository.GetById(user.AccessGroupId);
                            oldAgName = oldAg.Name;
                            if (oldAg.Type == (short)AccessGroupType.PersonalAccess && oldAg.ParentId.HasValue)
                            {
                                oldAg = _unitOfWork.AccessGroupRepository.GetById(oldAg.ParentId.Value);
                                oldAgName = oldAg.Name + " *";
                            }
                            var oldAgd = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(user.CompanyId, user.AccessGroupId);
                            var oldAgdNames = oldAgd.Select(m => m.Icu.Name).ToList();

                            var newAg = _unitOfWork.AccessGroupRepository.GetById(model.AccessGroupId);
                            newAgName = newAg.Name;
                            if (newAg.Type == (short)AccessGroupType.PersonalAccess && newAg.ParentId.HasValue)
                            {
                                newAg = _unitOfWork.AccessGroupRepository.GetById(newAg.ParentId.Value);
                                newAgName = newAg.Name + " *";
                            }
                            var newAgd = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(user.CompanyId, model.AccessGroupId);
                            var newAgdNames = newAgd.Select(m => m.Icu.Name).ToList();

                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblAccessGroup, oldAgName, newAgName));
                        }

                        haveToSend = true;
                    }

                    // Check Door list
                    if (model.DoorList != null && model.DoorList.Any())
                    {
                        var oldDoorList = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(user.CompanyId, user.AccessGroupId).ToList();
                        var oldDoorModel = oldDoorList
                            .Select(m => new DoorModel
                            {
                                DoorId = m.IcuId,
                                AccessTimeId = m.TzId
                            }).ToList();

                        // New DoorModel
                        var newDoorModel = model.DoorList;

                        // Get except
                        // This list should be deleted
                        var oldMinusNew = oldDoorModel.Except(newDoorModel, new DoorModelCompare()).ToList();
                        if (oldMinusNew.Any())
                        {
                            haveToSend = true;
                            var listUnAssign = oldDoorList
                                .Where(m => oldMinusNew.Any(n => n.DoorId == m.IcuId && n.AccessTimeId == m.TzId))
                                .Select(m => m.Icu.Name);

                            changes.Add($"[{UserResource.lblLostAccessPermission}]<br />{string.Join("<br />", listUnAssign)}");
                        }

                        // This list should be added
                        var newMinusOld = newDoorModel.Except(oldDoorModel, new DoorModelCompare()).ToList();
                        if (newMinusOld.Any())
                        {
                            haveToSend = true;
                            var listAssign = _unitOfWork.IcuDeviceRepository.GetByIds(newMinusOld.Select(m => m.DoorId).ToList()).Select(m => m.Name);

                            changes.Add($"[{UserResource.lblAddAccessPermission}]<br />{string.Join("<br />", listAssign)}");
                        }
                    }
                    else
                    {
                        // This case is about non-PAG.
                        // This means that this user is changed to have normal AG, not additional doors.
                        if (user.AccessGroupId != model.AccessGroupId)
                        {
                            haveToSend = true;
                        }
                    }


                    // Check Avatar
                    if (user.Avatar != model.Avatar)
                    {
                        if (!onlyCheck)
                        {
                            string oldAvatar = user.Avatar != null ? user.Avatar.GetFileNameFromPath() : "";
                            string newAvatar = model.Avatar != null ? model.Avatar.GetFileNameFromPath() : "";

                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblAvatar, oldAvatar, newAvatar));
                        }

                        haveToSend = true;
                    }

                    // Check HomePhone
                    if (!string.IsNullOrEmpty(user.HomePhone) && !user.HomePhone.Equals(model.HomePhone))
                    {
                        if (!onlyCheck)
                        {
                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblHomePhone, user.HomePhone, model.HomePhone));
                        }

                        haveToSend = true;
                    }

                    // Check Department
                    if (user.DepartmentId != model.DepartmentId)
                    {
                        if (!onlyCheck)
                        {
                            string oldDeptName = _unitOfWork.DepartmentRepository.GetById(user.DepartmentId).DepartName;
                            string newDeptName = _unitOfWork.DepartmentRepository.GetById(model.DepartmentId ?? 0)?.DepartName;

                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblDepartment, oldDeptName, newDeptName));
                        }

                        haveToSend = true;
                    }

                    // Check EffectiveDate
                    var oldEffectiveDate = user.EffectiveDate.HasValue ? user.EffectiveDate.Value.ConvertDefaultDateTimeToString() : "";
                    if (oldEffectiveDate != model.EffectiveDate)
                    {
                        if(!string.IsNullOrEmpty(oldEffectiveDate))
                        {

                            if (!onlyCheck)
                            {
                                DateTime oldDateTime = DateTime.ParseExact(oldEffectiveDate, Constants.Settings.DateTimeFormatDefault, null);
                                DateTime newDateTime = DateTime.ParseExact(model.EffectiveDate, Constants.Settings.DateTimeFormatDefault, null);
                                var userTimeZone = _accountService.GetById(_httpContext.User.GetAccountId()).TimeZone;
                                var offSet = userTimeZone.ToTimeZoneInfo().BaseUtcOffset;

                                oldDateTime = oldDateTime.ConvertToUserTime(offSet);
                                newDateTime = newDateTime.ConvertToUserTime(offSet);

                                changes.Add(Helpers.CreateChangedValueContents(UserResource.lblEffectiveDate, oldDateTime.ToSettingDateString(), newDateTime.ToSettingDateString()));
                            }

                            haveToSend = true;
                        }
                    }

                    // Check ExpiredDate
                    var oldExpiredDate = user.ExpiredDate.HasValue ? user.ExpiredDate.Value.ConvertDefaultDateTimeToString() : "";
                    if (oldExpiredDate != model.ExpiredDate)
                    {
                        if(!string.IsNullOrEmpty(oldEffectiveDate))
                        {

                            if (!onlyCheck)
                            {
                                DateTime oldDateTime = DateTime.ParseExact(oldExpiredDate, Constants.Settings.DateTimeFormatDefault, null);
                                DateTime newDateTime = DateTime.ParseExact(model.ExpiredDate, Constants.Settings.DateTimeFormatDefault, null);
                                var userTimeZone = _accountService.GetById(_httpContext.User.GetAccountId()).TimeZone;
                                var offSet = userTimeZone.ToTimeZoneInfo().BaseUtcOffset;

                                oldDateTime = oldDateTime.ConvertToUserTime(offSet);
                                newDateTime = newDateTime.ConvertToUserTime(offSet);

                                changes.Add(Helpers.CreateChangedValueContents(UserResource.lblExpiredDate, oldDateTime.ToSettingDateString(), newDateTime.ToSettingDateString()));
                            }

                            haveToSend = true;
                        }

                    }

                    // Check BirthDay
                    var oldBirthDay = user.BirthDay.HasValue ? user.BirthDay.Value.ConvertDefaultDateTimeToString() : "";
                    if (oldBirthDay != model.BirthDay)
                    {
                        if(!string.IsNullOrEmpty(oldBirthDay))
                        {

                            if (!onlyCheck)
                            {
                                string oldDateTimeStr = string.Empty, newDateTimeStr = string.Empty;
                                var userTimeZone = _accountService.GetById(_httpContext.User.GetAccountId()).TimeZone;
                                var offSet = userTimeZone.ToTimeZoneInfo().BaseUtcOffset;

                                if (DateTime.TryParseExact(oldBirthDay, Constants.Settings.DateTimeFormatDefault, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime oldDateTime))
                                {
                                    oldDateTime = oldDateTime.ConvertToUserTime(offSet);
                                    oldDateTimeStr = oldDateTime.ToSettingDateString();
                                }


                                if (DateTime.TryParseExact(model.BirthDay, Constants.Settings.DateTimeFormatDefault, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime newDateTime))
                                {
                                    newDateTime = newDateTime.ConvertToUserTime(offSet);
                                    newDateTimeStr = newDateTime.ToSettingDateString();
                                }

                                changes.Add(Helpers.CreateChangedValueContents(UserResource.lblBirthday, oldDateTimeStr, newDateTimeStr));
                            }

                            haveToSend = true;
                        }    
                    }

                    // Check Status
                    // Valid / Invalid
                    if (user.Status != model.Status)
                    {
                        if (!onlyCheck)
                        {
                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblStatus, ((UserStatus)user.Status).GetDescription(), ((UserStatus)model.Status).GetDescription()));
                        }

                        haveToSend = true;
                    }

                    // Check WorkType
                    if (user.WorkType != model.WorkType)
                    {
                        if (!onlyCheck)
                        {
                            var oldWorkTypeName = "";
                            var newWorkTypeName = "";
                            if (user.WorkType != null)
                            {
                                oldWorkTypeName = ((WorkType)user.WorkType).GetDescription();
                            }
                            newWorkTypeName = ((WorkType)model.WorkType).GetDescription();

                            // for user
                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblWorkType, oldWorkTypeName, newWorkTypeName));
                        }

                        haveToSend = true;
                    }

                    // Check Username (account - mail)
                    if (user.AccountId != null && !string.IsNullOrEmpty(model.Username) && model.Username.Trim().ToLower() != user.Account?.Username)
                    {
                        if (!onlyCheck)
                        {
                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblEmail, user.Account?.Username, model.Username));
                        }

                        haveToSend = true;
                    }

                    // Check working time (if company enable plugin time attendance)
                    if (_unitOfWork.CompanyRepository.CheckCompanyByPlugin(Constants.PlugIn.TimeAttendance, user.CompanyId) && user.WorkingTypeId != model.WorkingTypeId)
                    {
                        if (!onlyCheck)
                        {
                            var oldWorkingTypeName = user.WorkingTypeId == null ? "" : _unitOfWork.WorkingRepository.GetById(user.WorkingTypeId.Value).Name;
                            var newWorkingTypeName = model.WorkingTypeId == null ? "" : _unitOfWork.WorkingRepository.GetById(model.WorkingTypeId.Value).Name;

                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblWorkingType, oldWorkingTypeName, newWorkingTypeName));
                        }

                        haveToSend = true;
                    }

                    // Check EmpNo
                    if (user.EmpNumber != model.EmployeeNumber?.Trim())
                    {
                        if (!onlyCheck)
                        {
                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblEmployeeNumber, user.EmpNumber, model.EmployeeNumber));
                        }
                    }

                    // Check Position
                    if (user.Position != model.Position?.Trim())
                    {
                        if (!onlyCheck)
                        {
                            changes.Add(Helpers.CreateChangedValueContents(UserResource.lblPosition, user.Position, model.Position));
                        }
                    }
                }

                return haveToSend;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckChange");
                return false;
            }
        }

        public DynamicQr GetDynamicQrCode(int userId, string qrId)
        {
            try
            {

                var company = _unitOfWork.CompanyRepository.GetById(_httpContext.User.GetCompanyId());
                var timeNow = DateTime.Now.ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss);
                var secondNow = timeNow.Substring(timeNow.Length - Constants.DynamicQr.SubStringSeconds);

                // Get setting dynamic qr code
                var setting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.TimePeriodQrcode, company.Id);
                int.TryParse(Helpers.GetStringFromValueSetting(setting.Value), out int timePeriod);
                timePeriod = timePeriod == 0 ? 12 : timePeriod;

                // Get setting
                var settingQrCode = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.UseStaticQrCode, company.Id);
                bool.TryParse(Helpers.GetStringFromValueSetting(settingQrCode.Value), out bool useStaticQrCode);
                if (useStaticQrCode)
                {
                    return new DynamicQr()
                    {
                        QrCode = qrId,
                        Duration = timePeriod
                    };
                }

                // Create plain text
                var plaintText = qrId + "_" + DateTime.Now.AddSeconds(timePeriod).ToString(Constants.DateTimeFormat.DdMMyyyyHHmmss) + "_" + secondNow;
                int len = plaintText.Length;
                var salt = Helpers.GenerateSalt(len);
                var data = plaintText + "_" + salt;
                var key = company.SecretCode;

                string text = EncryptStringToBytes(data, Encoding.UTF8.GetBytes(key), Encoding.UTF8.GetBytes(Helpers.ReverseString(Constants.DynamicQr.Key)));
                return new DynamicQr()
                {
                    QrCode = Constants.DynamicQr.NameProject + "_" + text,
                    Duration = timePeriod
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDynamicQrCode");
                return null;
            }
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
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

        public void AssignUserToDefaultWorkingTime()
        {
            try
            {

                var users = _unitOfWork.UserRepository.GetAllUserInSystemNoWorkingTime();
                foreach (var user in users)
                {
                    var workingTime = _unitOfWork.WorkingRepository.GetWorkingTypeDefault(user.CompanyId);
                    user.WorkingTypeId = workingTime;
                    _unitOfWork.UserRepository.Update(user);
                    _unitOfWork.Save();

                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AssignUserToDefaultWorkingTime");
            }
        }

        public Account GetAccountByUserName(string userName)
        {
            try
            {

                return _unitOfWork.AccountRepository.GetByUserName(userName);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccountByUserName");
                return null;
            }
        }

        public User GetUserByLinkedAccount(int accountId, int companyId)
        {
            try
            {

                return _unitOfWork.UserRepository.GetUserByAccountId(accountId, companyId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserByLinkedAccount");
                return null;
            }
        }

        /// <summary>
        /// Check whether email address is valid or not.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public bool IsEmailValid(string emailAddress)
        {
            if (string.IsNullOrEmpty(emailAddress))
                return false;

            try
            {
                MailAddress m = new MailAddress(emailAddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>   Query if 'id' is duplicated user code. </summary>
        /// <remarks>   Edward, 2020-03-27. </remarks>
        /// <param name="userId">   The identifier of user. </param>
        /// <param name="userCode"> The user code. </param>
        /// <returns>   True if duplicated user code, false if not. </returns>
        public bool IsDuplicatedUserCode(int userId, string userCode, int? companyId)
        {
            try
            {

                companyId = companyId ?? _httpContext.User.GetCompanyId();

                int userCount = _unitOfWork.AppDbContext.User.Count(m => m.Id != userId && !m.IsDeleted && m.UserCode == userCode && m.CompanyId == companyId.Value);

                if (userCount == 0)
                    return false;
                else
                    return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsDuplicatedUserCode");
                return false;
            }
        }


        public bool IsDuplicatedAccountCreated(int userId, string email)
        {
            try
            {

                var account = _unitOfWork.AppDbContext.Account.FirstOrDefault(a => a.Username.ToLower() == email.ToLower() && !a.IsDeleted);

                if (account == null)
                {
                    return false;
                }

                var companyAccount = _unitOfWork.AppDbContext.CompanyAccount.FirstOrDefault(a => a.AccountId == account.Id && a.CompanyId == _httpContext.User.GetCompanyId());

                var user = _unitOfWork.UserRepository.GetById(userId);

                if (companyAccount != null && (userId == 0 || (user != null && user.Email != email)))
                {
                    // If account exists in system, it means this email was duplicated.
                    // but if there is not user linked to this account, it means this email should be linked to the user.

                    //20200825 : We only check duplicate for the same company, 1 account can be use in multiple company

                    var userLinkAccount = _unitOfWork.AppDbContext.User.FirstOrDefault(u => u.AccountId == account.Id
                                                                                            && !u.IsDeleted
                                                                                            && u.CompanyId == _httpContext.User.GetCompanyId());

                    if (userLinkAccount == null)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                return false;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsDuplicatedAccountCreated");
                return false;
            }
        }

        public User GetUserByAccountId(int accountId, int companyId)
        {
            try
            {

                var user = _unitOfWork.AppDbContext.User.Include(m => m.Department)
                    .FirstOrDefault(u => u.AccountId == accountId && !u.IsDeleted && u.CompanyId == companyId);
                return user;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserByAccountId");
                return null;
            }
        }

        public User GetUserByEmail(string email, int companyId)
        {
            try
            {

                var user = _unitOfWork.AppDbContext.User.FirstOrDefault(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted && u.CompanyId == companyId);
                return user;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserByEmail");
                return null;
            }
        }

        public User GetUserByPhone(string phone, int companyId)
        {
            try
            {

                var user = _unitOfWork.AppDbContext.User.FirstOrDefault(u => u.HomePhone.ToLower() == phone.ToLower() && !u.IsDeleted && u.CompanyId == companyId);
                return user;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserByPhone");
                return null;
            }
        }

        public User GetUserByUserId(int userId)
        {
            try
            {

                var user = _unitOfWork.AppDbContext.User.Where(u => u.Id == userId && u.CompanyId == _httpContext.User.GetCompanyId() && !u.IsDeleted).FirstOrDefault();
                return user;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserByUserId");
                return null;
            }
        }

        public List<User> GetUsersByUserIds(List<int> userIds)
        {
            try
            {

                var users = _unitOfWork.UserRepository.GetByIds(userIds).ToList();
                foreach (var user in users)
                {
                    if (string.IsNullOrEmpty(user.Avatar))
                    {
                        user.Avatar = user.Sex ? Constants.Image.DefaultMale : Constants.Image.DefaultFemale;
                    }
                }

                return users;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsersByUserIds");
                return new List<User>();
            }
        }

        public string ValidationDynamicQr(string dynamicQr)
        {
            var message = "";

            var company = _unitOfWork.CompanyRepository.GetById(_httpContext.User.GetCompanyId());

            var companySecretCode = company.SecretCode;

            var data = StringToByteArray(dynamicQr.Remove(0, 32));
            var valid = new ValidationQr();
            try
            {
                var strDecrypt = DecryptStringToBytes(data, Encoding.UTF8.GetBytes(companySecretCode),
                    Encoding.UTF8.GetBytes(Helpers.ReverseString(Constants.DynamicQr.Key)));

                String[] parsData = strDecrypt.Split("_");

                var qrId = parsData[0];
                var dateTimeString = parsData[1];


                CultureInfo provider = CultureInfo.InvariantCulture;
                DateTime expireDate = DateTime.ParseExact(dateTimeString, Constants.DateTimeFormat.DdMMyyyyHHmmss, provider);



                var card = _unitOfWork.AppDbContext.Card.Single(i => i.CardId == qrId &&
                                                                     !i.IsDeleted &&
                                                                     i.CompanyId == _httpContext.User.GetCompanyId() &&
                                                                     i.CardType == (short)CardType.QrCode);

                if (card != null)
                {
                    var user = _unitOfWork.UserRepository.GetByUserId(_httpContext.User.GetCompanyId(), card.UserId.Value);
                    message = expireDate >= DateTime.Now
                        ? string.Format(MessageResource.msgValidationUser, user.FirstName)
                        : MessageResource.msgDynamicQrExpired;
                    return message;
                }

            }
            catch
            {
                valid.Messages = MessageResource.msgUserIsInValid;
            }
            return message;
        }

        private static string DecryptStringToBytes(byte[] inputBytes, byte[] Key, byte[] IV)
        {
            byte[] outputBytes = inputBytes;

            // SECURITY: Replaced RijndaelManaged with AES for better security
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                string plaintext = string.Empty;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption. 
                using (MemoryStream msEncrypt = new MemoryStream(outputBytes))
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csEncrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
                return plaintext;
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public bool GetDeviceFromCompany(string rid)
        {
            try
            {

                var device = _unitOfWork.IcuDeviceRepository.GetDeviceByRid(_httpContext.User.GetCompanyId(), rid) != null;
                return device;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDeviceFromCompany");
                return false;
            }
        }

        public Card GetQrByUserId(int userId)
        {
            try
            {

                var identification = _unitOfWork.CardRepository.GetQrCode(userId);
                return identification;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetQrByUserId");
                return null;
            }
        }
        public Card GetNFCPhoneIdByUserId(int userId)
        {
            try
            {

                var identification = _unitOfWork.CardRepository.GetNFCPhoneId(userId);
                return identification;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNFCPhoneIdByUserId");
                return null;
            }
        }

        public int GetTotalUserByCompany(int companyId)
        {
            try
            {

                return _unitOfWork.UserRepository.GetUserByCompany(companyId).Count();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTotalUserByCompany");
                return 0;
            }
        }

        public List<User> GetUsersByCompany(int companyId)
        {
            try
            {

                return _unitOfWork.UserRepository.GetUserByCompany(companyId).ToList();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsersByCompany");
                return new List<User>();
            }
        }

        public bool CheckPermissionEditAvatar(int companyId)
        {
            try
            {

                var setting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.EmployeeEditAvatar, companyId);
                if (bool.TryParse(Helpers.GetStringFromValueSetting(setting.Value), out var value))
                {
                    return value;
                }

                return false;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckPermissionEditAvatar");
                return false;
            }
        }

        public int GetUsersReviewCount()
        {
            try
            {

                var currentAccountId = _httpContext.User.GetAccountId();

                var count = _unitOfWork.AppDbContext.User.Count(v => v.CompanyId == _httpContext.User.GetCompanyId()
                                                                        && ((v.ApprovalStatus == (short)ApprovalStatus.ApprovalWaiting1 && v.ApproverId1 == currentAccountId)
                                                                            || (v.ApprovalStatus == (short)ApprovalStatus.ApprovalWaiting2 && v.ApproverId2 == currentAccountId)
                                                                            || (v.ApprovalStatus == (short)ApprovalStatus.UpdateWaiting1 && v.ApproverId1 == currentAccountId)
                                                                            || (v.ApprovalStatus == (short)ApprovalStatus.UpdateWaiting2 && v.ApproverId2 == currentAccountId))
                                                                        && !v.IsDeleted);

                return count;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsersReviewCount");
                return 0;
            }
        }

        public AccessSettingModel GetAccessSettingByCompany()
        {
            try
            {

                AccessSettingModel result = new AccessSettingModel()
                {

                };

                var accessSetting = _unitOfWork.AppDbContext.AccessSetting.Where(m => m.CompanyId == _httpContext.User.GetCompanyId()).FirstOrDefault();

                if (accessSetting != null)
                {
                    result = _mapper.Map<AccessSettingModel>(accessSetting);

                    List<string> firstApprovalAccounts = new List<string>();
                    List<string> secondApprovalAccounts = new List<string>();
                    var companyId = _httpContext.User.GetCompanyId();

                    if (!string.IsNullOrEmpty(accessSetting.FirstApproverAccounts))
                    {
                        var firstIds = JsonConvert.DeserializeObject<List<int>>(accessSetting.FirstApproverAccounts);

                        foreach (var id in firstIds)
                        {
                            if (_accountService.IsExistCompanyAccountbyAccountId(id, companyId))
                                firstApprovalAccounts.Add(id.ToString());
                        }
                    }

                    if (!string.IsNullOrEmpty(accessSetting.SecondApproverAccounts))
                    {
                        var secondIds = JsonConvert.DeserializeObject<List<int>>(accessSetting.SecondApproverAccounts);

                        foreach (var id in secondIds)
                        {
                            if (_accountService.IsExistCompanyAccountbyAccountId(id, companyId))
                                secondApprovalAccounts.Add(id.ToString());
                        }
                    }

                    result.FirstApproverAccounts = "[" + string.Join(",", firstApprovalAccounts) + "]";
                    result.SecondApproverAccounts = "[" + string.Join(",", secondApprovalAccounts) + "]";

                    if (string.IsNullOrEmpty(accessSetting.ListFieldsEnable))
                        accessSetting.ListFieldsEnable = JsonConvert.SerializeObject(Helpers.GetSettingVisibleFields(accessSetting.ListFieldsEnable, typeof(UserModel), Constants.Settings.VisitListFieldsIgnored, _logger));
                    if (string.IsNullOrEmpty(accessSetting.VisibleFields))
                        accessSetting.VisibleFields = JsonConvert.SerializeObject(Helpers.GetSettingVisibleFields(accessSetting.VisibleFields, typeof(AccessSettingModel)));
                }

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccessSettingByCompany");
                return null;
            }
        }


        public void UpdateAccessSettingCompany(AccessSettingModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var companyId = _httpContext.User.GetCompanyId();
                        var setting = _unitOfWork.UserRepository.GetAccessSetting(companyId);
                        setting.CompanyId = companyId;
                        setting.FirstApproverAccounts = model.FirstApproverAccounts;
                        setting.SecondApproverAccounts = model.SecondApproverAccounts;
                        setting.ApprovalStepNumber = model.ApprovalStepNumber;
                        setting.EnableAutoApproval = model.EnableAutoApproval;
                        setting.AllowDeleteRecord = model.AllowDeleteRecord;
                        setting.AllLocationWarning = model.AllLocationWarning;
                        setting.DeviceIdCheckIn = model.DeviceIdCheckIn;
                        setting.ListFieldsEnable = JsonConvert.SerializeObject(model.ListFieldsEnable);

                        _unitOfWork.UserRepository.UpdateAccessSetting(setting);
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


        public void UpdateApprovalUser(int id, short status)
        {
            User user = null;
            List<UserModel> tempUsersModel = new List<UserModel>();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        user = _unitOfWork.UserRepository.GetById(id);

                        var oldStatus = user.ApprovalStatus;
                        user.ApprovalStatus = status;
                        int accountCreated = user.CreatedBy;
                        if (_httpContext?.User != null)
                        {
                            accountCreated = _httpContext.User.GetAccountId();
                        }
                        user.UpdatedBy = accountCreated;
                        _unitOfWork.UserRepository.Update(user);


                        // Check user's vehicle.
                        var vehicles = _unitOfWork.VehicleRepository.GetListVehicleByUser(user.Id);
                        if (vehicles != null && vehicles.Any())
                        {
                            int vehicleStatus = (int)VehicleStatus.Disallow;

                            switch (status)
                            {
                                case (short)ApprovalStatus.Approved:
                                    vehicleStatus = (int)VehicleStatus.Allow;
                                    break;
                            }

                            // vehicles.ForEach(m => m.Status = vehicleStatus);
                        }

                        // Check Update_User_Data
                        if ((oldStatus == (short)ApprovalStatus.UpdateWaiting1 || oldStatus == (short)ApprovalStatus.UpdateWaiting2) && status == (short)ApprovalStatus.Approved)
                        {
                            // This case is about approved user about updating information.

                            // This data is list of temp user data.
                            // These temp data is included all changes.
                            var updateUserData = _unitOfWork.UserRepository.GetUsersByFirstName(Constants.Settings.UpdateUserData + user.Id);
                            if (updateUserData.Any())
                            {
                                updateUserData = updateUserData.OrderBy(m => m.Id).ToList();

                                foreach (var tempUser in updateUserData)
                                {
                                    var testUserModel = _mapper.Map<UserModel>(tempUser);
                                    testUserModel.Id = id;
                                    testUserModel.Status = (short)UserStatus.Use;
                                    testUserModel.FirstName = user.FirstName;
                                    tempUsersModel.Add(testUserModel);
                                }

                                // Delete all temp data.
                                _unitOfWork.UserRepository.DeleteRange(updateUserData);
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

            // send count review
            _accountService.SendCountReviewToFe(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId());

            if (user != null)
            {
                if (tempUsersModel.Any())
                {
                    foreach (var userModel in tempUsersModel)
                    {
                        Update(userModel);
                    }
                }

                List<short> validCardStatus = new List<short>()
                {
                    (short) CardStatus.Normal,
                };

                var cards = _unitOfWork.CardRepository.GetByUserId(user.CompanyId, user.Id);
                cards = cards.Where(m => validCardStatus.Contains(m.Status) && validCardStatus.Contains(m.CardStatus)).ToList();

                if (cards.Any() && status == (short)ApprovalStatus.Approved)
                {
                    IWebSocketService webSocketService = new WebSocketService();
                    // Create separate UnitOfWork for background device operations to avoid DbContext concurrency issues
                    IUnitOfWork backgroundUnitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                    var deviceInstructionQueue = new DeviceInstructionQueue(backgroundUnitOfWork, _configuration, webSocketService);
                    var company = _unitOfWork.CompanyRepository.GetById(user.CompanyId);
                    var newListAgd = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(company.Id, user.AccessGroupId).ToList();
                    var devices = newListAgd.Select(m => m.Icu).ToList();
                    try
                    {
                        deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                        {
                            MessageType = Constants.Protocol.AddUser,
                            MsgId = Guid.NewGuid().ToString(),
                            Sender = _httpContext.User.GetUsername(),
                            UserIds = new List<int>() { user.Id },
                            CompanyCode = company.Code,
                            DeviceIds = devices.Select(m => m.Id).ToList(),
                        });
                    }
                    finally
                    {
                        backgroundUnitOfWork?.Dispose();
                    }
                }
            }
        }


        public bool ExistNotApprovedUser()
        {
            try
            {

                var companyId = _httpContext.User.GetCompanyId();

                List<int> approvalList = new List<int>()
                {
                    (int) ApprovalStatus.ApprovalWaiting1,
                    (int) ApprovalStatus.ApprovalWaiting2,
                };

                var users = _unitOfWork.AppDbContext.User.Where(m => approvalList.Contains(m.ApprovalStatus) && m.CompanyId == companyId && !m.IsDeleted);

                return users.Any();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExistNotApprovedUser");
                return false;
            }
        }



        public bool CanSendData(User user)
        {
            try
            {

                if (user.ApprovalStatus == (int)ApprovalStatus.ApprovalWaiting1
                    || user.ApprovalStatus == (int)ApprovalStatus.ApprovalWaiting2
                    || user.ApprovalStatus == (int)ApprovalStatus.Rejected)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CanSendData");
                return false;
            }
        }

        public bool CanEditData(User user)
        {
            try
            {

                if (user.ApprovalStatus == (int)ApprovalStatus.ApprovalWaiting1
                    || user.ApprovalStatus == (int)ApprovalStatus.ApprovalWaiting2)
                {
                    return false;
                }
                else
                {
                    return true;
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CanEditData");
                return false;
            }
        }



        /// <summary>
        /// Get user data by list of user code data
        /// </summary>
        /// <param name="userCodes"></param>
        /// <returns></returns>
        public IEnumerable<UserListSimpleModel> GetUsersByUserCodes(List<string> userCodes = null)
        {
            try
            {

                var companyId = _httpContext.User.GetCompanyId();
                var data = _unitOfWork.UserRepository.GetByUserCodes(companyId, userCodes);

                var result = data.Select(_mapper.Map<UserListSimpleModel>);

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsersByUserCodes");
                return new List<UserListSimpleModel>();
            }
        }


        /// <summary>
        /// Get user data by some conditions
        /// The conditions can be added by 3rd party module's needs
        /// </summary>
        /// <param name="userCodes"></param>
        /// <returns></returns>
        public IEnumerable<UserListSimpleModel> GetUsersByConditions(UserGetConditionModel conditions)
        {
            try
            {

                var companyId = _httpContext.User.GetCompanyId();
                var data = _unitOfWork.UserRepository.GetByConditions(companyId, conditions?.UserCodes, conditions?.CardIds);

                var result = data.Select(_mapper.Map<UserListSimpleModel>);

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsersByConditions");
                return new List<UserListSimpleModel>();
            }
        }

        public Dictionary<string, object> UpdateListAvatarUser(IFormFile file, Company company)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            try
            {
                if (company != null)
                {
                    var listUsers = _unitOfWork.UserRepository.GetUserByCompany(company.Id);

                    string success = "";
                    string error = "";

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        file.CopyTo(memoryStream);
                        byte[] fileBytes = memoryStream.ToArray();
                        string avatarBase64 = "";
                        var userName = file.FileName.Split(".");
                        try
                        {
                            avatarBase64 = Convert.ToBase64String(fileBytes);
                        }
                        catch (Exception ex)
                        {
                            error = $"Error converting file {userName[0]} to Base64: {ex.Message}";
                        }

                        if (!string.IsNullOrEmpty(avatarBase64))
                        {
                            // check user
                            var user = listUsers.Where(x => x.FirstName.ToLower().Equals(userName[0].ToLower())).ToList();
                            if (user.Count > 1)
                            {
                                error = $"The user's name {userName[0]} has been duplicated.";
                            }
                            else if (user.Count == 1)
                            {
                                var firstUser = user.FirstOrDefault();
                                if (firstUser != null)
                                {
                                    string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                                    string userCode = !string.IsNullOrEmpty(firstUser.UserCode) ? firstUser.UserCode : "user";

                                    // Use secure file saving to prevent path traversal attacks
                                    string fileName = $"{userCode}.{Guid.NewGuid().ToString()}.jpg";
                                    string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/avatar";
                                    bool successSaveFile = FileHelpers.SaveFileImageSecure(avatarBase64, basePath, fileName, Constants.Image.MaximumImageStored);

                                    if (successSaveFile)
                                    {
                                        string pathServer = $"{basePath}/{fileName}";
                                        firstUser.Avatar = $"{connectionApi}/static/{pathServer}";
                                    }
                                }
                                _unitOfWork.UserRepository.Update(firstUser);
                                success = userName[0];
                                result.Add("UserId", firstUser?.Id ?? 0);
                                result.Add("UserName", firstUser?.FirstName ?? "");
                                result.Add("AvatarBase64", avatarBase64);
                            }
                            else
                            {
                                error = $"Cannot found user map to image {userName[0]}.";
                            }
                        }
                    }

                    _unitOfWork.Save();
                    result.Add("ImageError", error);
                    result.Add("ImageSuccess", success);
                }

            }
            catch (Exception ex)
            {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                result.Add("Error", $"Error system: An error occurred: {ex.Message}. Line {line}");
            }

            return result;
        }

        public void UpdateAvatar(int userId, string avatar)
        {
            try
            {

                var user = _unitOfWork.UserRepository.GetById(userId);
                if (user != null)
                {
                    user.Avatar = avatar;
                    _unitOfWork.UserRepository.Update(user);
                    _unitOfWork.Save();

                    // if existed dual face device. Send access control topic to device (register face)
                    List<int> dualFaceTypes = new List<int>()
                    {
                        (short)DeviceType.DF970,
                        (short)DeviceType.BA8300,
                        (short)DeviceType.RA08,
                        (short)DeviceType.DQ8500,
                        (short)DeviceType.DQ200,
                        (short)DeviceType.TBVision,
                        (short)DeviceType.T2Face,
                    };
                    var company = _unitOfWork.CompanyRepository.GetById(user.CompanyId);
                    var devices = _unitOfWork.AccessGroupDeviceRepository
                        .GetByAccessGroupId(user.CompanyId, user.AccessGroupId)
                        .Select(m => m.Icu)
                        .Where(m => dualFaceTypes.Contains(m.DeviceType) && (company.AutoSyncUserData || m.ConnectionStatus == (short)ConnectionStatus.Online))
                        .ToList();

                    if (devices.Count > 0)
                    {
                        string sender = _httpContext.User.GetUsername();
                        ThreadSendCardToDevice(null, new List<int>() { userId }, devices, sender);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateAvatar");
            }
        }

        public List<Card> GetCardHFaceIdByUserIds(List<int> ids)
        {
            try
            {

                List<Card> cards = new List<Card>();
                foreach (int id in ids)
                {
                    var card = _unitOfWork.CardRepository.GetHFaceIdForUser(id);
                    if (card != null)
                        cards.Add(card);
                }

                return cards;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCardHFaceIdByUserIds");
                return new List<Card>();
            }
        }

        public User GetByNationalIdNumber(string idNumber, int companyId)
        {
            try
            {

                return _unitOfWork.UserRepository.GetByNationalIdNumber(idNumber, companyId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByNationalIdNumber");
                return null;
            }
        }

        public RegisterUserInitModel GetRegisterUserInit(Company company)
        {
            try
            {

                RegisterUserInitModel model = new RegisterUserInitModel();

                // logo
                var logoSetting = _unitOfWork.SettingRepository.GetLogo(company.Id);
                model.Logo = Helpers.GetStringFromValueSetting(logoSetting.Value);
                // language
                var languageSetting = _unitOfWork.SettingRepository.GetLanguage(company.Id);
                model.Language = Helpers.GetStringFromValueSetting(languageSetting.Value);
                // building
                var buildingDefault = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(company.Id);
                model.Timezone = buildingDefault.TimeZone;
                // company name
                model.CompanyName = company.Name;

                return model;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRegisterUserInit");
                return null;
            }
        }

        public ResultRegisterUser RegisterUser(RegisterUserModel model, Company company)
        {
            ResultRegisterUser accountInfo = null;
            var accessSetting = _unitOfWork.UserRepository.GetAccessSetting(company.Id);
            User user = new User();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // mapping data
                        user = _mapper.Map<User>(model);
                        // init param default
                        user.CompanyId = company.Id;
                        // working type
                        user.WorkingTypeId = _unitOfWork.WorkingRepository.GetWorkingTypeDefault(company.Id);
                        // department
                        user.DepartmentId = _unitOfWork.DepartmentRepository.GetDefautDepartmentByCompanyId(company.Id).Id;
                        // status
                        user.Status = (short)Status.Valid;
                        // user code
                        user.UserCode = $"{_unitOfWork.UserRepository.GetNewUserCode(company.Id):000000}";
                        // access setting and approval status
                        var firstApproverAccounts = JsonConvert.DeserializeObject<List<int>>(accessSetting.FirstApproverAccounts ?? "[]");
                        var secondApproverAccounts = JsonConvert.DeserializeObject<List<int>>(accessSetting.SecondApproverAccounts ?? "[]");
                        if (accessSetting.ApprovalStepNumber == (short)VisitSettingType.FirstStep)
                        {
                            user.ApproverId1 = model.ApproverId == 0 ? firstApproverAccounts.First() : model.ApproverId;
                            user.ApprovalStatus = (short)ApprovalStatus.ApprovalWaiting1;
                        }
                        else if (accessSetting.ApprovalStepNumber == (short)VisitSettingType.SecondStep)
                        {
                            user.ApproverId1 = model.ApproverId == 0 ? firstApproverAccounts.First() : model.ApproverId;
                            user.ApproverId2 = model.ApproverId == 0 ? secondApproverAccounts.First() : model.ApproverId;
                            user.ApprovalStatus = (short)ApprovalStatus.ApprovalWaiting2;
                        }

                        // create new account
                        var roleDefault = _unitOfWork.RoleRepository.GetDefaultRoleSettingByCompany(company.Id);
                        var account = AddAccountToUser(model.HomePhone, user, roleDefault, company);
                        user.AccountId = account?.Id;
                        user.PermissionType = (short)roleDefault.TypeId;
                        accountInfo = new ResultRegisterUser { Username = account?.Username, Password = Helpers.GeneratePasswordDefaultWithCompany(company.Name) };

                        // save user in database
                        _unitOfWork.UserRepository.Add(user);
                        _unitOfWork.Save();

                        // Access group new version
                        var accessGroupDefault = _unitOfWork.AccessGroupRepository.GetDefaultAccessGroup(company.Id);
                        var listDoor = _unitOfWork.IcuDeviceRepository.GetDevicesByAccessGroup(company.Id, accessGroupDefault.Id);
                        // New Access Group
                        var newAccessGroup = new AccessGroup
                        {
                            Name = Constants.Settings.NameAccessGroupPersonal + user.Id,
                            CompanyId = company.Id,
                            Type = (short)AccessGroupType.PersonalAccess,
                            ParentId = accessGroupDefault.Id
                        };
                        _unitOfWork.AccessGroupRepository.Add(newAccessGroup);
                        _unitOfWork.Save();
                        user.AccessGroupId = newAccessGroup.Id;
                        // Assign doors
                        foreach (var door in listDoor)
                        {
                            var agD = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupIdAndDeviceId(company.Id, accessGroupDefault.Id, door.Id);
                            var detailModel = new AccessGroupDevice
                            {
                                IcuId = door.Id,
                                TzId = agD.TzId,
                                AccessGroupId = newAccessGroup.Id
                            };
                            _unitOfWork.AccessGroupDeviceRepository.Add(detailModel);
                        }
                        _unitOfWork.Save();

                        // add user
                        _unitOfWork.UserRepository.Add(user);
                        _unitOfWork.Save();

                        // System log
                        if (!user.FirstName.Contains(Constants.Settings.UpdateUserData))
                        {
                            // Save system log
                            var details = new List<string>();
                            var detail = $"{UserResource.lblAccessGroup} : {newAccessGroup?.Name}";
                            details.Add(detail);

                            var department = _unitOfWork.DepartmentRepository.GetById(user.DepartmentId);
                            detail = $"{UserResource.lblDepartment} : {department?.DepartName}";
                            details.Add(detail);

                            var content = $"{ActionLogTypeResource.Add} : {user.FirstName + " " + user.LastName} ({UserResource.lblUser})";
                            var contentDetails = $"{UserResource.lblAddNew} :\n{string.Join("\n", details)}";

                            _unitOfWork.SystemLogRepository.Add(user.Id, SystemLogType.User, ActionLogType.Add, content, contentDetails, null, user.CompanyId);

                            // automatic add Qr Code
                            AutomationCreateQrCode(user, false);
                            // automatic add NFC Phone Id
                            AutomationCreateNFCPhoneId(user, false);
                        }

                        _unitOfWork.Save();
                        transaction.Commit();
                        accountInfo.UserId = user.Id;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        transaction.Rollback();
                        accountInfo = null;
                    }
                }
            });

            // send count review
            if (user.Id != 0 && accessSetting != null && accessSetting.ApprovalStepNumber != (short)VisitSettingType.NoStep && user.ApproverId1 != 0)
            {
                _accountService.SendCountReviewToFe(user.ApproverId1, user.CompanyId);
            }

            // send cards to device
            if (user.Id != 0 && CanSendData(user))
            {
                var devices = _unitOfWork.IcuDeviceRepository.GetDevicesByUserId(user.Id)
                    .Where(m => company.AutoSyncUserData || m.ConnectionStatus == (short)ConnectionStatus.Online)
                    .ToList();
                if (devices.Count > 0)
                {
                    ThreadSendCardToDevice(null, new List<int>() { user.Id }, devices, _httpContext.User.GetUsername());
                }
            }

            return accountInfo;
        }

        public List<int> RemoveQR()
        {
            try
            {

                var companyId = _httpContext.User.GetCompanyId();
                var cards = _unitOfWork.CardRepository.GetByCompanyId(companyId);

                List<int> userIds = new List<int>();

                if (cards != null)
                {
                    var qrData = cards.Where(card => card.CardType == (int)CardType.QrCode && card.UserId != null && card.VisitId == null);

                    if (qrData != null)
                    {
                        userIds = qrData.Select(card => card.UserId.Value).ToList();

                        // Delete All Qr Code
                        SendDeleteUsersToAllDevice(userIds, companyId, new List<int> { (int)CardType.QrCode });

                        _unitOfWork.CardRepository.DeleteRange(qrData);
                        _unitOfWork.Save();
                    }
                }

                return userIds;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RemoveQR");
                return new List<int>();
            }
        }

        public List<int> ConvertExpiredUserInvalid()
        {
            try
            {

                var companyId = _httpContext.User.GetCompanyId();
                var users = _unitOfWork.UserRepository.GetByCompanyId(companyId, new List<int>() { (int)Status.Valid }).Where(user => user.ExpiredDate <= DateTime.UtcNow);

                List<int> userIds = new List<int>();

                if (users != null)
                {
                    var userList = users.ToList();

                    userIds = userList.Select(user => user.Id).ToList();

                    // Send Delete All Card Message to devices.
                    SendDeleteUsersToAllDevice(userIds, companyId);

                    userList.ForEach(user => user.Status = (short)Status.Invalid);

                    foreach (var user in userList)
                    {
                        _unitOfWork.UserRepository.Update(user);
                    }
                    _unitOfWork.Save();
                }

                return userIds;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ConvertExpiredUserInvalid");
                return new List<int>();
            }
        }

        public List<UserInOutStatusListModel> GetInOutStatus(List<EventLogByWorkType> data, string search, int pageNumber, int pageSize, string sortColumn, string sortDirection, out int total, out int filtered)
        {
            try
            {

                if (data != null && data.Any())
                {
                    List<UserInOutStatusListModel> eventLogsByUser = new List<UserInOutStatusListModel>();

                    foreach (var eachData in data)
                    {
                        var workTypeName = ((Army_WorkType)eachData.WorkType).GetDescription();

                        eventLogsByUser.AddRange(eachData.EventLogsByUser.Select(d => new UserInOutStatusListModel()
                        {
                            Id = d.UserId,
                            UserName = d.UserName,
                            WorkTypeName = workTypeName,
                            DepartmentName = d.DepartmentName,
                            MilitaryNumber = d.MilitaryNumber,
                            LastEventTime = d.EventLogs.FirstOrDefault().EventTime,
                            Reason = d.Reason,
                            CardList = d.CardList,
                            PlateNumberList = d.VehicleList.Select(v => new CardModel()
                            {
                                CardId = v.PlateNumber
                            }).ToList()
                        }).ToList());
                    }

                    // Search filter
                    if (!string.IsNullOrEmpty(search))
                    {
                        search = search.Trim().ToLower();
                        eventLogsByUser = eventLogsByUser.Where(d => d.UserName.ToLower().Contains(search)
                        || d.DepartmentName.ToLower().Contains(search)
                        || d.PlateNumberList.Any(p => p.CardId.ToLower().Contains(search))
                        || d.CardList.Any(c => c.CardId.ToLower().Contains(search))
                        || d.WorkTypeName.ToLower().Contains(search)
                        || d.Reason.ToLower().Contains(search)).ToList();
                    }

                    total = eventLogsByUser.Count();
                    filtered = eventLogsByUser.Count();

                    // Sort
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        string columnName = sortColumn.ToPascalCase();
                        eventLogsByUser = Helpers.SortData<UserInOutStatusListModel>(eventLogsByUser.AsEnumerable<UserInOutStatusListModel>(), sortDirection, columnName).ToList();
                    }

                    // Paging
                    if (pageSize > 0)
                    {
                        eventLogsByUser = eventLogsByUser.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    return eventLogsByUser;
                }
                else
                {
                    total = 0;
                    filtered = 0;

                    return new List<UserInOutStatusListModel>();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetInOutStatus");
                total = 0;
                filtered = 0;
                return new List<UserInOutStatusListModel>();
            }
        }

        private Account AddAccountToUser(string userName, User user, DynamicRole roleDefault, Company company)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                var account = _unitOfWork.AccountRepository.GetByUserName(userName);
                if (account != null)
                {
                    CompanyAccount companyAccount = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompanyAndAccount(company.Id, account.Id);
                    // Account already exist in the company.
                    // Check if any User already map to this account
                    // Account is not in current company, we add this account to current company with default role
                    if (companyAccount == null)
                    {
                        companyAccount = new CompanyAccount();
                        companyAccount.AccountId = account.Id;
                        companyAccount.CompanyId = company.Id;
                        companyAccount.DynamicRoleId = roleDefault.Id;
                        _unitOfWork.CompanyAccountRepository.Add(companyAccount);
                    }
                }
                else
                {
                    string password = Helpers.GeneratePasswordDefaultWithCompany(company.Name);
                    var languageSetting = _unitOfWork.SettingRepository.GetLanguage(company.Id);
                    string language = Helpers.GetStringFromValueSetting(languageSetting?.Value);
                    var buildingDefault = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(company.Id);
                    var accountModel = new AccountModel()
                    {
                        Username = userName,
                        CompanyId = company.Id,
                        Password = password,
                        ConfirmPassword = password,
                        Role = (short)roleDefault.TypeId,
                        Language = language,
                        PhoneNumber = user.HomePhone,
                    };

                    var newAccount = _mapper.Map<Account>(accountModel);

                    newAccount.CompanyId = company.Id;
                    newAccount.Language = language;
                    newAccount.TimeZone = buildingDefault?.TimeZone;
                    newAccount.Type = (short)roleDefault.TypeId;
                    _unitOfWork.AccountRepository.Add(newAccount);
                    _unitOfWork.Save();

                    // Add CompanyAccount to mark user belong to multiple companies
                    CompanyAccount companyAccount = new CompanyAccount();
                    companyAccount.AccountId = newAccount.Id;
                    companyAccount.CompanyId = company.Id;
                    companyAccount.DynamicRoleId = roleDefault.Id;
                    _unitOfWork.CompanyAccountRepository.Add(companyAccount);
                    _unitOfWork.Save();

                    List<int> statusAllowSendMail = new List<int>
                    {
                        (short)ApprovalStatus.NotUse,
                        (short)ApprovalStatus.Approved,
                    };

                    _unitOfWork.Save();
                    account = newAccount;
                }

                return account;
            }

            return null;
        }

        private AccessGroup AddChildAccessGroupToParentOfUser(List<DoorModel> doorList, Company company, int userId, int parentAccessGroupId)
        {
            var accessGroup = _unitOfWork.AccessGroupRepository.GetById(parentAccessGroupId);
            if (doorList != null && doorList.Any())
            {
                var agDevices = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(company.Id, parentAccessGroupId)
                    .Select(m => new DoorModel()
                    {
                        DoorId = m.IcuId,
                        AccessTimeId = m.TzId,
                    }).ToList();
                var additionalDoorList = doorList.Select(m => new DoorModel()
                {
                    DoorId = m.DoorId,
                    AccessTimeId = m.AccessTimeId,
                }).Except(agDevices, new DoorModelCompare()).ToList();

                // New Access Group
                AccessGroup newAccessGroup = null;
                if (accessGroup != null && accessGroup.Type == (short)AccessGroupType.PersonalAccess)
                {
                    newAccessGroup = accessGroup;

                    // remove old agd unassigned
                    var deletedAgd = agDevices.Except(doorList.Select(m => new DoorModel()
                    {
                        DoorId = m.DoorId,
                        AccessTimeId = m.AccessTimeId,
                    }), new DoorModelCompare()).ToList();
                    if (deletedAgd.Any())
                    {
                        foreach (var item in deletedAgd)
                        {
                            _unitOfWork.AccessGroupDeviceRepository.Delete(m =>
                                m.IcuId == item.DoorId &&
                                m.TzId == item.AccessTimeId &&
                                m.AccessGroupId == accessGroup.Id);
                        }
                        _unitOfWork.Save();
                    }
                }
                else
                {
                    if (additionalDoorList.Any())
                    {
                        newAccessGroup = new AccessGroup
                        {
                            Name = Constants.Settings.NameAccessGroupPersonal + userId,
                            CompanyId = company.Id,
                            Type = (short)AccessGroupType.PersonalAccess,
                            ParentId = parentAccessGroupId
                        };
                        _unitOfWork.AccessGroupRepository.Add(newAccessGroup);
                        _unitOfWork.Save();
                    }
                }

                if (additionalDoorList.Any() && newAccessGroup != null)
                {
                    foreach (var door in additionalDoorList)
                    {
                        var detailDoor = _unitOfWork.IcuDeviceRepository.GetById(door.DoorId);
                        var detailModel = new AccessGroupDevice
                        {
                            IcuId = detailDoor.Id,
                            TzId = doorList.First(m => m.DoorId == door.DoorId).AccessTimeId,
                            AccessGroupId = newAccessGroup.Id
                        };
                        _unitOfWork.AccessGroupDeviceRepository.Add(detailModel);
                        _unitOfWork.Save();
                    }

                    return newAccessGroup;
                }
            }

            return null;
        }

        internal void SendCardToDevice(List<int> cardIds, List<int> userIds, List<IcuDevice> devices, string sender, string messageType = Constants.Protocol.AddUser)
        {
            try
            {
                IWebSocketService webSocketService = new WebSocketService();
                IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                var deviceInstructionQueue = new DeviceInstructionQueue(unitOfWork, _configuration, webSocketService);
                try
                {
                    var company = _unitOfWork.CompanyRepository.GetById(devices.First().CompanyId.Value);
                    deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                    {
                        MessageType = messageType,
                        MsgId = Guid.NewGuid().ToString(),
                        Sender = sender,
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
                finally
                {
                    unitOfWork.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ThreadSendCardToDevice(List<int> cardIds, List<int> userIds, List<IcuDevice> devices, string sender, string messageType = Constants.Protocol.AddUser, bool sendAsync = true)
        {
            if (!sendAsync)
            {
                Console.WriteLine($"[ThreadSendCardToDevice] Send card to device - SYNC");
                SendCardToDevice(cardIds, userIds, devices, sender, messageType = Constants.Protocol.AddUser);
            }
            else
            {
                new Thread(() =>
                {
                    Console.WriteLine($"[ThreadSendCardToDevice] Send card to device - ASYNC");
                    SendCardToDevice(cardIds, userIds, devices, sender, messageType = Constants.Protocol.AddUser);
                }).Start();
            }
        }

        public void SendUpdateUsersToAllDoors(List<User> users, string sender, bool isAddUser, List<Card> cards = null)
        {
            try
            {

                // send device common instruction to v2 devices
                // send access control to v1 devices
                IWebSocketService webSocketService = new WebSocketService();
                // Create separate UnitOfWork for background device operations to avoid DbContext concurrency issues
                IUnitOfWork backgroundUnitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                var deviceInstructionQueue = new DeviceInstructionQueue(backgroundUnitOfWork, _configuration, webSocketService);
                var groupUsers = users.GroupBy(m => m.AccessGroupId);
                foreach (var groupUser in groupUsers)
                {
                    var companyId = groupUser.First().CompanyId;
                    var company = _unitOfWork.CompanyRepository.GetById(companyId);
                    var devices = _unitOfWork.AccessGroupDeviceRepository
                        .GetByAccessGroupId(companyId, groupUser.Key)
                        .Select(m => m.Icu).ToList();

                    List<int> cardIdFilters = new List<int>();
                    if (cards != null)
                    {
                        cardIdFilters = cards.Where(m => m.UserId.HasValue && groupUser.Any(u => u.Id == m.UserId.Value))
                            .Select(m => m.Id).ToList();
                    }

                    deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                    {
                        MessageType = isAddUser ? Constants.Protocol.AddUser : Constants.Protocol.DeleteUser,
                        MsgId = Guid.NewGuid().ToString(),
                        Sender = sender,
                        UserIds = groupUser.Select(m => m.Id).ToList(),
                        CardIds = cardIdFilters,
                        CardFilterIds = cardIdFilters,
                        CompanyCode = company.Code,
                        DeviceIds = devices.Select(m => m.Id).Distinct().ToList(),
                    });
                }

                // Dispose background UnitOfWork
                backgroundUnitOfWork?.Dispose();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendUpdateUsersToAllDoors");
            }
        }

        public void UpdateAllUserCompanyToApproval(int companyId, int approvalStepNumber)
        {
            var approvalStepOld = _unitOfWork.UserRepository.GetAccessSetting(companyId);
            if (approvalStepOld != null && approvalStepOld.ApprovalStepNumber == (short)VisitSettingType.NoStep
                                        && approvalStepNumber == (short)VisitSettingType.FirstStep)
            {
                _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
                {
                    using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            var users = _unitOfWork.UserRepository.GetByCompanyId(companyId).Where(x => x.ApprovalStatus != (int)ApprovalStatus.Approved);
                            foreach (var user in users)
                            {
                                user.ApprovalStatus = (int)ApprovalStatus.Approved;
                                _unitOfWork.UserRepository.Update(user);
                            }

                            _unitOfWork.Save();
                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                });
            }
        }

        private string CardIdExist(string cardData, List<UserImportExportModel> dataImport, IUnitOfWork unitOfWork, int companyId, User user = null)
        {
            var result = new HashSet<string>();
            if (string.IsNullOrEmpty(cardData))
            {
                return null;
            }
            var cards = cardData.Split(',');
            foreach (var card in cards)
            {
                if (dataImport.Where(x => x.CardId.Value != null).Count(x => x.CardId.Value.Equals(card)) >= 2)
                {
                    result.Add(card);
                }

                if (user != null)
                {
                    var cardInDataBase = unitOfWork.AppDbContext.Card.FirstOrDefault(x => !x.IsDeleted && x.CompanyId == companyId && x.CardId == card && x.UserId != user.Id);
                    if (cardInDataBase != null)
                    {
                        result.Add(card);
                    }
                }
                else
                {
                    var cardInDataBase = unitOfWork.CardRepository.GetByCardId(companyId, card);
                    if (cardInDataBase != null)
                    {
                        result.Add(card);
                    }
                }

            }
            return string.Join(",", result);
        }
        private bool CheckEmailExits(string email, IUnitOfWork unitOfWork, int companyId)
        {
            if (!string.IsNullOrEmpty(email))
            {
                var account = unitOfWork.AppDbContext.Account.FirstOrDefault(a => a.Username.ToLower() == email.ToLower() && !a.IsDeleted);
                if (account != null)
                {
                    var userAccount = unitOfWork.AppDbContext.User.Where(u =>
                        u.CompanyId == companyId && u.AccountId == account.Id && !u.IsDeleted);
                    return userAccount.Any();
                }
            }

            return false;
        }
    }
}


