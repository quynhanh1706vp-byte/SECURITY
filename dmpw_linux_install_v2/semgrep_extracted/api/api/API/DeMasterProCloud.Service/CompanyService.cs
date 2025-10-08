using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Bogus.Extensions;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Company;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Attendance;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.DataModel.SystemInfo;
using DeMasterProCloud.Service.Infrastructure;
using System.Threading;
using DeMasterProCloud.DataModel.Device;
using Org.BouncyCastle.Asn1.BC;

namespace DeMasterProCloud.Service
{
    public interface ICompanyService
    {
        bool IsValidCompany(Company company);
        string MakeCompanyCode();
        int Add(CompanyModel model);
        void Update(CompanyModel model);
        void Delete(Company company);
        void DeleteRange(List<Company> companies);
        Company GetById(int? id);
        Company GetByCode(string code);
        List<Company> Gets();
        void AssignToCompany(List<int> deviceIds, int companyId);
        string AssignCameraToCompany(List<UnregistedDevice> devices, int companyId);

        IQueryable<CompanyListModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        List<Company> GetExpiredCompaniesByDay(int numberOfDay = 0, bool bExactDay = true);
        void UpdateCompaniesToExpire(List<Company> companies);

        //void SendCompanyAccountMail(Company company, string originalPass);
        //void SendExpiredCompanyMail(List<Company> companies, int numberOfExpiredDay);
        Company GetRootCompany();
        bool IsExistCompanyCode(string companyCode);
        bool IsExistedCompanyAccount(int companyId, string userName);
        List<Company> GetByIds(List<int> ids);
        List<Company> GetCompanies();
        void UpdateLogo(string base64String);
        string GetCurrentLogo(Company company);
        CompanyDataModel GetDefaultInfoById(int companyId);
        string GetCodeById(int companyId);
        void ResetAesKeyCompanies();
        void ResetAesKeyCompany(int companyId);
        PlugInListModel GetPluginByCompany(int companyId);
        bool CheckPluginByCompany(int companyId, string plugIn);
        PlugIn GetPluginByCompanyAllowShowing(int companyId);
        int UpdatePluginByCompany(int companyId, PlugIns model);
        Building GetDefaultBuildingByCompany(int companyId);
        void UpdateSettingRecheckAttendance(int companyId, SettingRecheckAttendance model);
        CheckLimitAddedModel CheckLimitCountOfUsers(int companyId, int numberAdded);

        void UpdateEncryptSetting(EncryptSettingModel model);
        EncryptSettingModel GetEncryptSetting(int id);

        void UpdateExpiredPWSetting(ExpiredPWSettingModel model);
        ExpiredPWSettingModel GetExpiredPWSetting(int id);

        string TestENC(string plainText);
        string TestDEC(string encText);
        CompanyViewDetailModel GetCompanyViewDetailById(int companyId);
        List<LanguageDetailModel> GetListLanguageForCompany(int companyId);
        bool CheckIpAllowInCompany(string ipAddress, int companyId);
        void ResetAllowIpSetting(int companyId);
    }

    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly HttpContext _httpContext;
        private readonly IDeviceService _deviceService;
        private readonly IRoleService _roleService;
        private readonly ISettingService _settingService;
        private readonly ICameraService _cameraService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public CompanyService(IUnitOfWork unitOfWork, IConfiguration configuration, ISettingService settingService, ICameraService cameraService,
            IHttpContextAccessor contextAccessor, IDeviceService deviceService, IRoleService roleService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _httpContext = contextAccessor.HttpContext;
            _deviceService = deviceService;
            _roleService = roleService;
            _settingService = settingService;
            _cameraService = cameraService;
            _mapper = MapperInstance.Mapper;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<CompanyService>();
        }

        /// <summary>
        /// Add company
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public int Add(CompanyModel model)
        {
            var companyId = 0;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //Add company
                        var company = _mapper.Map<Company>(model);

                        if (!model.LogoEnable)
                        {
                            company.Logo = null;
                        }

                        if (!model.MiniLogoEnable)
                        {
                            company.MiniLogo = null;
                        }

                        //Add company code.
                        company.Code = !string.IsNullOrEmpty(model.Code) ? model.Code : MakeCompanyCode();
                        //Add company name.
                        company.Name = !string.IsNullOrEmpty(model.Name) ? model.Name : null;
                        //Add company Remark.
                        company.Remarks = !string.IsNullOrEmpty(model.Remarks) ? model.Remarks : null;
                        //Add EventLogStorageDuration
                        company.EventLogStorageDurationInFile = model.EventLogStorageDurationInDb == 0 ? 24 : model.EventLogStorageDurationInDb;
                        company.EventLogStorageDurationInDb = model.EventLogStorageDurationInFile == 0 ? 24 : model.EventLogStorageDurationInFile;
                        company.TimeLimitStoredImage = model.TimeLimitStoredImage == 0 ? 365 : model.TimeLimitStoredImage;
                        company.TimeLimitStoredVideo = model.TimeLimitStoredVideo == 0 ? 365 : model.TimeLimitStoredVideo;
                        company.LimitCountOfUser = model.LimitCountOfUser;
                        company.EnableReCheckImageCamera = model.EnableReCheckImageCamera;
                        company.TimeLimitCheckImageCamera = model.TimeLimitCheckImageCamera;
                        //Add secretCode system info
                        company.SecretCode = _unitOfWork.SystemInfoRepository.GetSecretCodeInSystem();

                        _unitOfWork.CompanyRepository.Add(company);
                        _unitOfWork.Save();

                        //Add default department
                        _unitOfWork.DepartmentRepository.AddDefault(company);

                        //Add default access time.
                        _unitOfWork.AccessTimeRepository.AddDefaultTimezone(company.Id);
                        _unitOfWork.Save();

                        //Add default Access group
                        _unitOfWork.AccessGroupRepository.AddDefault(company, _configuration);
                        _unitOfWork.Save();

                        //Add default building.
                        var timeZone = _unitOfWork.AppDbContext.Account.FirstOrDefault(m => m.Id == _httpContext.User.GetAccountId())?.TimeZone ?? Constants.DefaultTimeZone;
                        //_unitOfWork.BuildingRepository.AddDefault(company);
                        _unitOfWork.BuildingRepository.AddDefault(company, timeZone);
                        _unitOfWork.Save();
                        
                        //Add default working type.
                        _unitOfWork.WorkingRepository.AddWorkingTypeDefault(company.Id);
                        _unitOfWork.Save();
                        
                        //Add plug-Ins
                        _unitOfWork.PlugInRepository.AddPlugInDefault(company.Id, _configuration);
                        _unitOfWork.Save();

                        //Add default Access setting.
                        _unitOfWork.UserRepository.CreateDefaultAccessSetting(company.Id);
                        _unitOfWork.Save();

                        //Add default visit setting.
                        _unitOfWork.VisitRepository.CreateDefaultVisitSetting(company.Id);
                        _unitOfWork.Save();
                        
                        //Add attendance setting
                        _unitOfWork.AppDbContext.AttendanceSetting.Add(new AttendanceSetting()
                        {
                            CompanyId = company.Id,
                            ApproverAccounts = "[]",
                            
                            InReaders = "[]",
                            OutReaders = "[]"
                        });
                        _unitOfWork.Save();

                        //Add default setting.
                        var settings = _configuration.GetSection(Constants.Settings.FileSettings)?.Get<List<FileSetting>>();
                        if (settings != null && settings.Any())
                        {
                            foreach (var unApplySetting in settings)
                            {
                                if (unApplySetting.Key.Equals("logo"))
                                {
                                    // save image logo
                                    if (unApplySetting.Values[0].IsTextBase64())
                                    {
                                        string hostApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();

                                        // Use secure file saving to prevent path traversal attacks
                                        string fileName = $"{Constants.Settings.Logo}.jpg";
                                        string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/setting";
                                        bool isSaveImage = FileHelpers.SaveFileImageSecure(unApplySetting.Values[0], basePath, fileName);
                                        string path = $"{basePath}/{fileName}";
                                        unApplySetting.Values[0] = $"{hostApi}/static/{path}";
                                    }
                                }
                                if (unApplySetting.Key.Equals("qr_logo"))
                                {
                                    // save image qr logo
                                    if (unApplySetting.Values[0].IsTextBase64())
                                    {
                                        string hostApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();

                                        // Use secure file saving to prevent path traversal attacks
                                        string fileName = $"{Constants.Settings.QRLogo}.jpg";
                                        string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/setting";
                                        bool isSaveImage = FileHelpers.SaveFileImageSecure(unApplySetting.Values[0], basePath, fileName);
                                        string path = $"{basePath}/{fileName}";
                                        unApplySetting.Values[0] = $"{hostApi}/static/{path}";
                                    }
                                }
                                var valueSetting = JsonConvert.SerializeObject(unApplySetting.Values);
                                var setting = new Setting
                                {
                                    Key = unApplySetting.Key,
                                    Value = valueSetting,
                                    CompanyId = company.Id
                                };
                                _unitOfWork.SettingRepository.Add(setting);
                            }
                            _unitOfWork.Save();
                        }
                        
                        //Add language list default
                        _unitOfWork.SettingRepository.Add(new Setting()
                        {
                            CompanyId = company.Id,
                            Key = Constants.Settings.KeyListLanguageOfCompany,
                            Value = (model.ListLanguage == null || model.ListLanguage.Count == 0)
                                ? JsonConvert.SerializeObject(Constants.Settings.ListLanguageDefault)
                                : JsonConvert.SerializeObject(model.ListLanguage.Select(_mapper.Map<LanguageModel>))
                        });
                        _unitOfWork.Save();
                        
                        //Add default role.
                        _roleService.AddDefaultRole(company.Id);
                        
                        // Add default camera setting
                        _unitOfWork.SettingRepository.Add(new Setting()
                        {
                            Key = Constants.Settings.CameraSetting,
                            Value = Helpers.JsonConvertCamelCase(new CameraSetting()),
                            CompanyId = company.Id
                        });
                        
                        // Add default login settings from configuration
                        var configLoginSettings = _configuration.GetSection(Constants.Settings.FileSettings)?.Get<List<FileSetting>>();
                        var loginSettingKeys = new[]
                        {
                            Constants.Settings.KeyChangeInFirstTime,
                            Constants.Settings.KeyHaveUpperCase,
                            Constants.Settings.KeyHaveNumber,
                            Constants.Settings.KeyHaveSpecial,
                            Constants.Settings.KeyTimeNeedToChange,
                            Constants.Settings.KeyMaximumTimeUsePassword,
                            Constants.Settings.KeyMaximumEnterWrongPassword,
                            Constants.Settings.KeyTimeoutWhenWrongPassword
                        };

                        if (configLoginSettings != null && configLoginSettings.Any())
                        {
                            foreach (var settingKey in loginSettingKeys)
                            {
                                var configSetting = configLoginSettings.FirstOrDefault(s => s.Key == settingKey);
                                if (configSetting != null)
                                {
                                    var setting = new Setting
                                    {
                                        Key = configSetting.Key,
                                        Value = JsonConvert.SerializeObject(configSetting.Values),
                                        CompanyId = company.Id
                                    };
                                    _unitOfWork.SettingRepository.Add(setting);
                                }
                            }
                        }
                        _unitOfWork.Save();
                        
                        _unitOfWork.CompanyRepository.Update(company);
                        _unitOfWork.Save();

                        //Save system log
                        //var content = $"{ActionLogTypeResource.Add}: {company.Code} ({CompanyResource.lblCompanyCode})";
                        var content = CompanyResource.lblAddNew;
                        var contentsDetails = $"{CompanyResource.lblCompanyName} : {company.Name} (code : {company.Code})";
                        _unitOfWork.SystemLogRepository.Add(company.Id, SystemLogType.Company, ActionLogType.Add, content, contentsDetails, null, company.Id);
                        
                        _unitOfWork.Save();
                        transaction.Commit();
                        companyId = company.Id;
                        //Send mail
                        //SendCompanyAccountMail(company, model.Password);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in Add");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
            return companyId;
        }

        /// <summary>
        /// Update company
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void Update(CompanyModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //Update company
                        var company = _unitOfWork.CompanyRepository.GetById(model.Id);
                        var existingName = company.Name;

                        List<string> changes = new List<string>();

                        var isChange = CheckChange(company, model, ref changes);

                        if (!model.LogoEnable)
                        {
                            company.Logo = null;
                        }

                        if (!model.MiniLogoEnable)
                        {
                            company.MiniLogo = null;
                        }

                        if (!string.IsNullOrEmpty(model.Name))
                        {
                            company.Name = model.Name;
                        }
                        if (!string.IsNullOrEmpty(model.Code) && company.Code != model.Code)
                        {
                            company.Code = model.Code;
                        }
                        if (!string.IsNullOrEmpty(model.Remarks))
                        {
                            company.Remarks = model.Remarks;
                        }
                        if (!string.IsNullOrEmpty(model.WebsiteUrl))
                        {
                            company.WebsiteUrl = model.WebsiteUrl;
                        }
                        if (!string.IsNullOrEmpty(model.ContactWEmail))
                        {
                            company.ContactWEmail = model.ContactWEmail;
                        }
                        if (!string.IsNullOrEmpty(model.Phone))
                        {
                            company.Phone = model.Phone;
                        }
                        if (!string.IsNullOrEmpty(model.Industries))
                        {
                            company.Industries = model.Industries;
                        }
                        if (!string.IsNullOrEmpty(model.Location))
                        {
                            company.Location = model.Location;
                        }
                        company.EventLogStorageDurationInDb = model.EventLogStorageDurationInDb == 0 ? 24 : model.EventLogStorageDurationInDb;
                        company.EventLogStorageDurationInFile = model.EventLogStorageDurationInFile == 0 ? 24 : model.EventLogStorageDurationInFile;
                        company.TimeLimitStoredImage = model.TimeLimitStoredImage == 0 ? 365 : model.TimeLimitStoredImage;
                        company.TimeLimitStoredVideo = model.TimeLimitStoredVideo == 0 ? 365 : model.TimeLimitStoredVideo;
                        company.AutoSyncUserData = model.AutoSyncUserData;
                        company.LimitCountOfUser = model.LimitCountOfUser;
                        company.EnableReCheckImageCamera = model.EnableReCheckImageCamera;
                        company.TimeLimitCheckImageCamera = model.TimeLimitCheckImageCamera;

                        if(company.CardBit != model.CardBit)
                        {
                            // Update CardBit value.
                            company.CardBit = model.CardBit;
                        }
                        
                        _unitOfWork.CompanyRepository.Update(company);
                        
                        // update default language
                        var listLanguageDefault = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.KeyListLanguageOfCompany, company.Id);
                        if (model.ListLanguage == null || model.ListLanguage.Count == 0)
                        {
                            listLanguageDefault.Value = JsonConvert.SerializeObject(Constants.Settings.ListLanguageDefault);
                            _unitOfWork.SettingRepository.Update(listLanguageDefault);
                            _unitOfWork.Save();
                        }
                        else
                        {
                            listLanguageDefault.Value = JsonConvert.SerializeObject(model.ListLanguage);
                            _unitOfWork.SettingRepository.Update(listLanguageDefault);
                            _unitOfWork.Save();

                            if (CheckPluginByCompany(company.Id, Constants.PlugIn.CustomizeLanguage))
                            {
                                var settingLanguage = _unitOfWork.SettingRepository.GetLanguage(company.Id);
                                settingLanguage.Value = $"[\"{model.ListLanguage.First().Tag}\"]";
                                _unitOfWork.SettingRepository.Update(settingLanguage);
                            }
                        }
                        
                        //Save system log
                        var content = $"{CompanyResource.lblUpdateCompany}\n{CompanyResource.lblCompanyName} : {existingName}";
                        var contentsDetails = string.Join("\n", changes);
                        _unitOfWork.SystemLogRepository.Add(company.Id, SystemLogType.Company, ActionLogType.Update, content, contentsDetails, null, company.Id);

                        _unitOfWork.Save();
                        transaction.Commit();

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in Update");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Assign a door(s) to company
        /// This function is for Assign company button.
        /// </summary>
        /// <param name="deviceIds"></param>
        /// <param name="companyId"></param>
        public void AssignToCompany(List<int> deviceIds, int companyId)
        {
            _deviceService.AssignToCompany(deviceIds, companyId);
        }
        /// <summary>
        /// Assign a camera(s) to company
        /// </summary>
        /// <param name="devices"></param>
        /// <param name="companyId"></param>
        public string AssignCameraToCompany(List<UnregistedDevice> devices, int companyId)
        {
            List<string> cameraError = new List<string>();
            // add camera in company
            foreach (var camera in devices)
            {
                var oldCamera = _cameraService.GetByCameraId(camera.DeviceAddress);
                if (oldCamera == null)
                {
                    CameraModel newCamera = new CameraModel()
                    {
                        CompanyId = companyId,
                        Name = camera.DeviceAddress,
                        Type = (short)CameraType.CameraDC,
                        RoleReader = (short)RoleRules.In,
                        CameraId = camera.DeviceAddress,
                        CheckEventFromWebHook = true
                    };
                    _cameraService.Add(newCamera);
                }
                else
                {
                    cameraError.Add(oldCamera.CameraId);
                }
            }

            //Clear unregistered camera
            var unregisteredDeviceIds = devices.Where(x => !cameraError.Contains(x.DeviceAddress)).Select(n => n.Id);
            _unitOfWork.UnregistedDevicesRepository.Delete(m => unregisteredDeviceIds.Contains(m.Id));
            _unitOfWork.Save();

            return cameraError.Count > 0
                ? string.Format(MessageResource.msgCameraIsExisted, string.Join(", ", cameraError))
                : string.Empty;
        }

        /// <summary>
        /// Update logo
        /// </summary>
        /// <param name="base64String"></param>
        public void UpdateLogo(string base64String)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var company = _unitOfWork.CompanyRepository.GetById(_httpContext.User.GetCompanyId());
                        company.Logo = System.Text.Encoding.UTF8.GetBytes(base64String);
                        _unitOfWork.CompanyRepository.Update(company);
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in UpdateLogo");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Get logo by company
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public string GetCurrentLogo(Company company)
        {
            var base64Logo = "";
            if (company.Logo != null)
            {
                base64Logo = System.Text.Encoding.UTF8.GetString(company.Logo);
            }

            return base64Logo;
        }

        /// <summary>
        /// Get default infomation about company by Id
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public CompanyDataModel GetDefaultInfoById(int companyId)
        {
            var companyInfo = new CompanyDataModel
            {
                BuildingItems = _unitOfWork.BuildingRepository.GetMany(c => !c.IsDeleted && c.CompanyId == companyId)
                                        .Select(m => new SelectListItemModel { Id = m.Id, Name = m.Name }).ToList(),
                DefaultBuilding = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(companyId).Id,

                ActiveTzItems = _unitOfWork.AccessTimeRepository.GetMany(c => !c.IsDeleted && c.CompanyId == companyId)
                                        .Select(m => new SelectListItemModel { Id = m.Id, Name = m.Name }).ToList(),
                DefaultActiveTz = _unitOfWork.AccessTimeRepository.GetDefaultTzByCompanyId(Constants.Tz24hPos, companyId).Id,

                PassiveTzItems = _unitOfWork.AccessTimeRepository.GetMany(c => !c.IsDeleted && c.CompanyId == companyId)
                                        .Select(m => new SelectListItemModel { Id = m.Id, Name = m.Name }).ToList(),
                DefaultPassiveTz = _unitOfWork.AccessTimeRepository.GetDefaultTzByCompanyId(Constants.TzNotUsePos, companyId).Id,
            };

            return companyInfo;
        }
        

        /// <summary>
        /// Get logo by company
        /// </summary>
        /// <param name="cocompanyIdmpany"></param>
        /// <returns></returns>
        public string GetCodeById(int companyId)
        {
            return _unitOfWork.CompanyRepository.GetCompanyCodeByCompanyId(companyId);
        }
        

        /// <summary>
        /// Delete company
        /// </summary>
        /// <param name="company"></param>
        public void Delete(Company company)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // Delete Company data from system
                        _unitOfWork.CompanyRepository.DeleteFromSystem(company);

                        // delete company account
                        var companyAccounts = _unitOfWork.AppDbContext.CompanyAccount.Where(c => c.CompanyId == company.Id);
                        if (companyAccounts.Any())
                        {
                            _unitOfWork.CompanyAccountRepository.DeleteRange(companyAccounts);
                        }

                        // check list account in company
                        // check company account include: account
                        var accounts = _unitOfWork.AppDbContext.Account.Where(a => a.CompanyId == company.Id);
                        foreach (var account in accounts)
                        {
                            var companyAccountWithAccount = _unitOfWork.AppDbContext.CompanyAccount.Where(c =>
                                c.AccountId == account.Id &&
                                companyAccounts.FirstOrDefault(x => x.Id == c.Id) == null);
                            if (!companyAccountWithAccount.Any())
                            {
                                _unitOfWork.AccountRepository.DeleteFromSystem(account);
                            }
                        }

                        // delete company's building.
                        var buildings = _unitOfWork.AppDbContext.Building.Where(m => m.CompanyId == company.Id);
                        _unitOfWork.BuildingRepository.DeleteRangeFromSystem(buildings);
                        
                        //Save system log
                        var content = $"{CompanyResource.msgDelete}";
                        var contentsDetails = $"{CompanyResource.lblCompanyName} : {company.Name} (code : {company.Code})";
                        _unitOfWork.SystemLogRepository.Add(company.Id, SystemLogType.Company, ActionLogType.Delete, content, contentsDetails, null, company.Id);

                        // delete account rabbit mq
                        var deviceUsers = _unitOfWork.AppDbContext.IcuDevice.Where(m => m.CompanyId == company.Id).Select(
                            d => ((DeviceType) d.DeviceType).GetName() + "_" + d.DeviceAddress
                        ).ToList();

                        // delete icu device
                        var IcuDeviceList = _unitOfWork.AppDbContext.IcuDevice.Where(m => m.CompanyId == company.Id);
                        foreach (var icuDeviceTemp in IcuDeviceList)
                        {
                            icuDeviceTemp.Company = null;
                            icuDeviceTemp.CompanyId = null;
                            icuDeviceTemp.UpdatedOn = DateTime.UtcNow;
                            icuDeviceTemp.UpdatedBy = _httpContext.User.GetAccountId();
                            _unitOfWork.IcuDeviceRepository.Update(icuDeviceTemp);
                        }


                        Helpers.DeleteAccountRabbitMq(_configuration, new List<string>(){company.Code}, deviceUsers);
                        
                        _unitOfWork.Save();
                        transaction.Commit();

                        //Push company changed (account changed) notify
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in Delete");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Delete a list of company
        /// </summary>
        /// <param name="companies"></param>
        public void DeleteRange(List<Company> companies)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // list of account rabbit mq deleted
                        List<string> accountCompanyDelete = new List<string>();
                        List<string> accountDeviceDelete = new List<string>();
                        
                        foreach (var company in companies)
                        {
                            //company.IsDeleted = true;
                            //company.UpdatedOn = DateTime.UtcNow;

                            // delete company account
                            var companyAccounts = _unitOfWork.AppDbContext.CompanyAccount.Where(c => c.CompanyId == company.Id);
                            if (companyAccounts.Any())
                            {
                                _unitOfWork.CompanyAccountRepository.DeleteRange(companyAccounts);
                            }

                            // check list account in company
                            // check company account include: account
                            var accounts = _unitOfWork.AppDbContext.Account.Where(a => a.CompanyId == company.Id);
                            foreach (var account in accounts)
                            {
                                var companyAccountWithAccount = _unitOfWork.AppDbContext.CompanyAccount.Where(c =>
                                    c.AccountId == account.Id &&
                                    companyAccounts.FirstOrDefault(x => x.Id == c.Id) == null);
                                if (!companyAccountWithAccount.Any())
                                {
                                    _unitOfWork.AccountRepository.DeleteFromSystem(account);
                                }
                            }

                            // delete company's building.
                            var buildings = _unitOfWork.AppDbContext.Building.Where(m => m.CompanyId == company.Id);
                            _unitOfWork.BuildingRepository.DeleteRangeFromSystem(buildings);

                            //Save system log
                            var content = $"{CompanyResource.msgDelete}";
                            var contentsDetails = $"{CompanyResource.lblCompanyName} : {company.Name} (code : {company.Code})";
                            _unitOfWork.SystemLogRepository.Add(company.Id, SystemLogType.Company, ActionLogType.Delete, content, contentsDetails, null, company.Id);

                            //_unitOfWork.CompanyRepository.Update(company);
                            
                            // init account rabbit mq
                            var deviceUsers = _unitOfWork.AppDbContext.IcuDevice.Where(m => m.CompanyId == company.Id).Select(
                                d => ((DeviceType) d.DeviceType).GetName() + "_" + d.DeviceAddress
                            ).ToList();
                            accountCompanyDelete.Add(company.Code);
                            accountDeviceDelete.AddRange(deviceUsers);

                            // delete icu device
                            var IcuDeviceList = _unitOfWork.AppDbContext.IcuDevice.Where(m => m.CompanyId == company.Id);
                            foreach (var icuDeviceTemp in IcuDeviceList)
                            {
                                icuDeviceTemp.Company = null;
                                icuDeviceTemp.CompanyId = null;
                                icuDeviceTemp.UpdatedOn = DateTime.UtcNow;
                                icuDeviceTemp.UpdatedBy = _httpContext.User.GetAccountId();
                                _unitOfWork.IcuDeviceRepository.Update(icuDeviceTemp);
                            }
                        }

                        // Delete from dompanies data from system.
                        _unitOfWork.CompanyRepository.DeleteRangeFromSystem(companies);

                        // delete account rabbit mq
                        Helpers.DeleteAccountRabbitMq(_configuration, accountCompanyDelete, accountDeviceDelete);
                        
                        _unitOfWork.Save();
                        transaction.Commit();

                        //Push company changed (account changed) notify
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in DeleteRange");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Update company to the expiration
        /// </summary>
        /// <param name="companies"></param>
        /// <returns></returns>
        public void UpdateCompaniesToExpire(List<Company> companies)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //Update whole doors of each company to invalid as well.
                        foreach (var company in companies)
                        {
                            // Delete company data from system.
                            _unitOfWork.CompanyRepository.DeleteFromSystem(company);

                            var icuDevices = _unitOfWork.IcuDeviceRepository.GetValidDoorsByCompany(company.Id);
                            foreach (var icuDevice in icuDevices)
                            {
                                icuDevice.Status = (short)Status.Invalid;
                                _unitOfWork.IcuDeviceRepository.Update(icuDevice);
                            }

                            _unitOfWork.Save();
                            transaction.Commit();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in UpdateCompaniesToExpire");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
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
        public IQueryable<CompanyListModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn, string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            // Step 1: Fetch user time zone once
            var userTimeZone = _unitOfWork.AppDbContext.Account
                .Where(c => c.Id == _httpContext.User.GetAccountId() && !c.IsDeleted)
                .Select(c => c.TimeZone)
                .FirstOrDefault();

            // Step 2: Fetch and map the data
            var data = _unitOfWork.AppDbContext.Company
                .Where(c => !c.IsDeleted)
                .Select(m => new CompanyListModel
                {
                    Id = m.Id,
                    Code = m.Code,
                    Name = m.Name,
                    Remarks = m.Remarks,
                    Createdon = m.CreatedOn.ConvertToUserTimeZone(userTimeZone)
                })
                .AsEnumerable() // Step 3: Switch to client-side evaluation
                .Select(m =>
                {
                    // Optionally reformat the date string to ensure correct sorting
                    m.Createdon = DateTime.Parse(m.Createdon).ToString("o");
                    return m;
                })
                .AsQueryable();

            // Step 4: Apply filtering
            totalRecords = data.Count();
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x => x.Code.RemoveDiacritics().ToLower().Contains(filter)
                                       || x.Name.RemoveDiacritics().ToLower().Contains(filter)).AsQueryable();
            }

            // Step 5: Apply sorting
            recordsFiltered = data.Count();
            data = data.OrderBy($"{sortColumn} {sortDirection}");

            // Step 6: Apply pagination
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            // Step 7: Convert back to original format
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            string dateFormat = ApplicationVariables.Configuration[Constants.DateServerFormat + ":" + culture];
            var result = data.AsEnumerable() // Switch to client-side evaluation for final processing
                .Select(m =>
                {
                    // Convert back to the original format used in ConvertToUserTimeZone
                    m.Createdon = DateTime.Parse(m.Createdon).ToString(dateFormat); // Use your date format key
                    return m;
                })
                .AsQueryable();

            

            return result;
        }


        /// <summary>
        /// Get companies
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public List<Company> Gets()
        {
            return _unitOfWork.CompanyRepository.GetCompanies();
        }

        /// <summary>
        /// Get company by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Company GetById(int? id)
        {
            return _unitOfWork.CompanyRepository.GetCompanyById(id);
        }

        /// <summary>
        /// Get Company by code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        /// <returns></returns>
        public Company GetByCode(string code)
        {
            return _unitOfWork.CompanyRepository.GetCompanyByCode(code);
        }

        /// <summary>
        /// Get expired companies by day
        /// </summary>
        /// <param name="numberOfDay"></param>
        /// <param name="bExactDay"></param>
        /// <returns></returns>
        public List<Company> GetExpiredCompaniesByDay(int numberOfDay, bool bExactDay)
        {
            return _unitOfWork.CompanyRepository.GetExpiredCompaniesByDay(numberOfDay, bExactDay);
        }

        /// <summary>
        /// Check if company code is exist
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        public bool IsExistCompanyCode(string companyCode)
        {
            return _unitOfWork.AppDbContext.Company.Any(m =>
                !m.IsDeleted && m.Code == companyCode);
        }

        /// <summary>
        /// Check if company account is exist
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool IsExistedCompanyAccount(int companyId, string userName)
        {
            var account = _unitOfWork.AccountRepository.GetRootAccountByCompany(companyId);
            if (account != null && _unitOfWork.AccountRepository.IsExist(account.Id, userName, companyId))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get list of company by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<Company> GetByIds(List<int> ids)
        {
            return _unitOfWork.CompanyRepository.GetByIds(ids);
        }
        
        /// <summary>
        /// Get root company
        /// </summary>
        /// <returns></returns>
        public Company GetRootCompany()
        {
            return _unitOfWork.CompanyRepository.GetRootCompany();
        }

        /// <summary>
        /// Check company is valid
        /// </summary>
        /// <param name="company"></param>
        /// <returns></returns>
        public bool IsValidCompany(Company company)
        {
            if (company == null)
            {
                return false;
            }

            if (company.RootFlag)
            {
                return true;
            }

            var result = !company.IsDeleted /*&&
                         DateTime.Now.Subtract(company.ExpiredFrom.Date).TotalSeconds >= 0
                         && company.ExpiredTo.Subtract(DateTime.Now.Date).TotalSeconds >= 0*/;
            return result;
        }

        /// <summary>
        /// Get company code
        /// </summary>
        /// <returns></returns>
        public string MakeCompanyCode()
        {
            return _unitOfWork.CompanyRepository.MakeCompanyCode();
        }

        /// <summary>
        /// Get list of company
        /// </summary>
        /// <returns></returns>
        public List<Company> GetCompanies()
        {
            return _unitOfWork.CompanyRepository.GetCompanies();
        }
        
        private static Random random = new Random();
        
        /// <summary>
        /// Checking if there are any changes.
        /// </summary>
        /// <param name="company">Company that contains existing information</param>
        /// <param name="model">Model that contains new information</param>
        /// <param name="changes">List of changes</param>
        /// <returns></returns>
        internal bool CheckChange(Company company, CompanyModel model, ref List<string> changes)
        {
            if (model.Id != 0)
            {
                if (company.Name != model.Name)
                {
                    changes.Add(string.Format(MessageResource.msgChangeInfo, CompanyResource.lblCompanyName, company.Name, model.Name));
                }

                if (company.Remarks != model.Remarks)
                {
                    changes.Add(string.Format(MessageResource.msgChangeInfo, CompanyResource.lblRemarks, company.Remarks, model.Remarks));
                }
            }

            return changes.Count() > 0;
        }

        public void ResetAesKeyCompanies()
        {
            var companies = _unitOfWork.CompanyRepository.GetCompanies();
            // We use same code for all company
            var secretCode = Helpers.GenerateCompanyKey();
            foreach (var company in companies)
            {
                company.SecretCode = secretCode;
                try
                {
                    _unitOfWork.CompanyRepository.Update(company);
                    _unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ResetAesKeyCompanies");
                }
                var devices = _deviceService.GetByCompanyId(company.Id);
                foreach (var device in devices)
                {
                    try
                    {
                        _deviceService.SendDeviceConfig(device, Constants.Protocol.UpdateDeviceConfig);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in ResetAesKeyCompanies");
                    }
                }

            }
        }

        public void ResetAesKeyCompany(int companyId)
        {
            var company = _unitOfWork.CompanyRepository.GetById(companyId);
            company.SecretCode = Helpers.GenerateCompanyKey();
            try
            {
                _unitOfWork.CompanyRepository.Update(company);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetAesKeyCompany");
            }
            var devices = _deviceService.GetByCompanyId(company.Id);
            foreach (var device in devices)
            {
                try
                {
                    _deviceService.SendDeviceConfig(device, Constants.Protocol.UpdateDeviceConfig);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ResetAesKeyCompany");
                }
            }
        }

        public PlugInListModel GetPluginByCompany(int companyId)
        {
            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
            var pluginSettings = _configuration.GetSection("DefaultPlugin")?.Get<List<PlugInSettingModel>>();
            var pluginPolicy = JsonConvert.DeserializeObject<Dictionary<string, bool>>(plugin.PlugIns);
            //var pluginDescription = JsonConvert.DeserializeObject<Dictionary<string, string>>(plugin.PlugInsDescription);
            
            // title
            var title = new Dictionary<string, string>();
            // Description
            var descriptions = new Dictionary<string, string>();

            var language = _settingService.GetLanguage(_httpContext.User.GetAccountId(), companyId);
            var culture = new CultureInfo(language);

            var dbList = pluginPolicy.Select(m => m.Key);
            var settingList = pluginSettings.Select(m => m.Name);

            var removedList = dbList.Except(settingList);

            foreach(var removed in removedList)
            {
                pluginPolicy.Remove(removed);
            }

            foreach (var pluginSetting in pluginSettings)
            {
                if (pluginSetting.IsShowing)
                {
                    title.Add(pluginSetting.Name, SystemResource.ResourceManager.GetString("PlugIn" + pluginSetting.Name, culture));
                    descriptions.Add(pluginSetting.Name, SystemResource.ResourceManager.GetString("DescPlugIn" + pluginSetting.Name, culture));
                }
                else
                {
                    pluginPolicy.Remove(pluginSetting.Name);
                    //pluginDescription.Remove(pluginSetting.Name);
                }
            }

            var plugInListModel = new PlugInListModel()
            {
                Id = plugin.Id,
                CompanyId = plugin.CompanyId,
                PlugIns = JsonConvert.SerializeObject(pluginPolicy),
                //PlugInsDescription = JsonConvert.SerializeObject(pluginDescription),
                PlugInsDescription = JsonConvert.SerializeObject(descriptions),
                PlugInsTitle = JsonConvert.SerializeObject(title)
            };
            
            return plugInListModel;
        }

        public bool CheckPluginByCompany(int companyId, string plugIn)
        {
            var solution = _unitOfWork.PlugInRepository.Get(c => c.CompanyId == companyId);

            if (solution != null)
            {
                PlugIns addOnSolution = JsonConvert.DeserializeObject<PlugIns>(solution.PlugIns);
                foreach (PropertyInfo pi in addOnSolution.GetType().GetProperties())
                {
                    if (pi.Name == plugIn)
                    {
                        bool value = (bool)addOnSolution.GetType().GetProperty(plugIn).GetValue(addOnSolution, null);
                        if (value)
                        {
                            return true;
                        }
                    }

                }
                return false;
            }
            else
            {
                return false;
            }
        }

        public int UpdatePluginByCompany(int companyId, PlugIns model)
        {
            var pluginId = 0;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        List<string> details = new List<string>()
                        {
                            $"company Id : {companyId}"
                        };

                        var pluginSettings = _configuration.GetSection("DefaultPlugin")?.Get<List<PlugInSettingModel>>();
                        foreach (PropertyInfo pi in model.GetType().GetProperties())
                        {
                            details.Add($"{pi.Name} : {model.GetType().GetProperty(pi.Name).GetValue(model, null)}");

                            Console.WriteLine(pi.Name);
                            var pluginSetting = pluginSettings.FirstOrDefault(p => p.IsShowing && p.Name == pi.Name);
                            if (pluginSetting == null)
                            {
                                if (pi.Name.Equals(Constants.PlugIn.Common))
                                {
                                    model.GetType().GetProperty(pi.Name)?.SetValue(model, true);
                                }
                                else
                                {
                                    model.GetType().GetProperty(pi.Name)?.SetValue(model, false);

                                }
                            }
                        }

                        var plugIn = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
                        var json = JsonConvert.SerializeObject(model);
                        plugIn.PlugIns = json;
                        _unitOfWork.PlugInRepository.Update(plugIn);
                        _unitOfWork.Save();

                        // // update Camera setting
                        // if (model.CameraPlugIn)
                        // {
                        //     var settingCamera = _unitOfWork.AppDbContext.Setting.FirstOrDefault(s => s.Key == Constants.Settings.CameraSetting && s.CompanyId == companyId);
                        //     
                        //     if (settingCamera == null)
                        //     {
                        //         var cameraSetting = new CameraSetting();
                        //         var setting = new Setting()
                        //         {
                        //             Key = Constants.Settings.CameraSetting,
                        //             Value = Helpers.JsonConvertCamelCase(cameraSetting),
                        //             CompanyId = companyId
                        //         };
                        //         
                        //         _unitOfWork.AppDbContext.Setting.Add(setting);
                        //     }
                        //     else if (String.IsNullOrEmpty(Helpers.GetStringFromValueSetting(settingCamera.Value)))
                        //     {
                        //         settingCamera.Value = Helpers.JsonConvertCamelCase(new CameraSetting());
                        //
                        //         _unitOfWork.AppDbContext.Setting.Update(settingCamera);
                        //     }
                        // }

                        //Save system log
                        var content = MessageResource.msgChangedPlugIn;
                        var contentsDetails = string.Join("\n", details);

                        // It is better that this information is not shown to the user.
                        // Therefore, this contents have 0 value as companyId, not own company's Id.
                        // In this case, '_httpContext.User.GetCompanyId()' <- this value is 0.
                        _unitOfWork.SystemLogRepository.Add(plugIn.Id, SystemLogType.PlugIn, ActionLogType.Update,
                            content, contentsDetails, null, _httpContext.User.GetCompanyId());

                        _unitOfWork.Save();

                        // Update company's permissions.
                        _roleService.UpdatePermissionsInDB(plugIn, (int)AccountType.PrimaryManager);
                        _roleService.UpdatePermissionsInDB(plugIn, (int)AccountType.SecondaryManager);
                        _roleService.UpdatePermissionsInDB(plugIn, (int)AccountType.Employee);
                        _roleService.UpdatePermissionsInDB(plugIn, (int)AccountType.DynamicRole);

                        transaction.Commit();
                        pluginId = plugIn.Id;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in UpdatePluginByCompany");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
            return pluginId;
        }

        public PlugIn GetPluginByCompanyAllowShowing(int companyId)
        {
            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);

            if(plugin != null)
            {
                var pluginPolicy = JsonConvert.DeserializeObject<Dictionary<string, bool>>(plugin.PlugIns);
                foreach (var keyPlug in pluginPolicy.ToList())
                {
                    if (keyPlug.Value == false)
                    {
                        pluginPolicy.Remove(keyPlug.Key);
                    }
                }
                plugin.PlugIns = JsonConvert.SerializeObject(pluginPolicy);
            }

            return plugin;
        }

        public Building GetDefaultBuildingByCompany(int companyId)
        {
            return _unitOfWork.BuildingRepository.GetDefaultByCompanyId(companyId);
        }

        public void UpdateSettingRecheckAttendance(int companyId, SettingRecheckAttendance model)
        {
            var company = _unitOfWork.CompanyRepository.GetById(companyId);
            company.TimeRecheckAttendance = model.TimeRecheckAttendance;
            company.UpdateAttendanceRealTime = model.UpdateAttendanceRealTime;
            try
            {
                _unitOfWork.CompanyRepository.Update(company);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateSettingRecheckAttendance");
            }
        }

        public CheckLimitAddedModel CheckLimitCountOfUsers(int companyId, int numberAdded)
        {
            return _unitOfWork.CompanyRepository.CheckLimitCountOfUsers(companyId, numberAdded);
        }

        public CompanyViewDetailModel GetCompanyViewDetailById(int companyId)
        {
            try
            {
                var company = _unitOfWork.CompanyRepository.GetById(companyId);
                if (company == null)
                    throw new Exception($"Company not existed in system by id = {companyId}");

                CompanyViewDetailModel data = new CompanyViewDetailModel();
                data.Name = company.Name;
                data.Code = company.Code;
                data.NumberOfUsers = _unitOfWork.UserRepository.Count(m => m.CompanyId == companyId && !m.IsDeleted);
                data.NumberOfAccounts = _unitOfWork.CompanyAccountRepository.Count(m => m.CompanyId == companyId);
                data.NumberOfBuildings = _unitOfWork.BuildingRepository.Count(m => m.CompanyId == companyId && !m.IsDeleted);
                data.NumberOfDevices = _unitOfWork.IcuDeviceRepository.Count(m => m.CompanyId == companyId && !m.IsDeleted);
                
                // plugins
                var companyPlugIn = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
                var plugIns = JsonConvert.DeserializeObject<Dictionary<string, bool>>(companyPlugIn.PlugIns);
                data.EnabledPlugIns = plugIns.Where(m => m.Value).Select(m => m.Key).ToList();
                
                // visitors, event-logs by months in last 12 months
                DateTime utcNow = DateTime.UtcNow;
                data.TotalVisitors = new List<GraphModel>();
                data.TotalEventLogs = new List<GraphModel>();
                for (int i = 0; i < 12; i++)
                {
                    DateTime date = utcNow.AddMonths(-i);
                    DateTime minDate = new DateTime(date.Year, date.Month, 1);
                    DateTime maxDate = minDate.AddMonths(1);
                    
                    data.TotalVisitors.Insert(0, new GraphModel()
                    {
                        Name = minDate.ToString(Constants.DateTimeFormat.MMYYYY),
                        Value = _unitOfWork.VisitRepository.Count(m => m.CompanyId == companyId && !m.IsDeleted && ((minDate <= m.StartDate && m.StartDate <= maxDate) || (minDate <= m.EndDate && m.EndDate <= maxDate))),
                    });
                    
                    data.TotalEventLogs.Insert(0, new GraphModel()
                    {
                        Name = minDate.ToString(Constants.DateTimeFormat.MMYYYY),
                        Value = _unitOfWork.EventLogRepository.Count(m => m.CompanyId == companyId && minDate <= m.EventTime && m.EventTime <= maxDate),
                    });
                }
                
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        /// <summary>
        /// Get List languages for company
        /// </summary>
        /// <param name="companyId">
        /// -1: get default
        /// >0: get by company
        /// ..: get all current
        /// </param>
        /// <returns></returns>
        public List<LanguageDetailModel> GetListLanguageForCompany(int companyId)
        {
            try
            {
                if (companyId > 0)
                {
                    var listLanguageDefault = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.KeyListLanguageOfCompany, companyId);
                    List<LanguageModel> languages = JsonConvert.DeserializeObject<List<LanguageModel>>(listLanguageDefault.Value);
                    var enumLanguages = EnumHelper.ToEnumList<ListLanguage>();
                    List<LanguageDetailModel> data = new List<LanguageDetailModel>();
                    foreach (var language in languages)
                    {
                        var item = _mapper.Map<LanguageDetailModel>(language);
                        item.Name = enumLanguages.FirstOrDefault(m => m.Id == language.Id)?.Name;
                        data.Add(item);
                    }
                    return data;
                }
                else if (companyId == -1)
                {
                    List<LanguageModel> languages = JsonConvert.DeserializeObject<List<LanguageModel>>(JsonConvert.SerializeObject(Constants.Settings.ListLanguageDefault));
                    var enumLanguages = EnumHelper.ToEnumList<ListLanguage>();
                    List<LanguageDetailModel> data = new List<LanguageDetailModel>();
                    foreach (var language in languages)
                    {
                        var item = _mapper.Map<LanguageDetailModel>(language);
                        item.Name = enumLanguages.FirstOrDefault(m => m.Id == language.Id)?.Name;
                        data.Add(item);
                    }
                    return data;
                }
                else
                {
                    List<LanguageModel> languages = JsonConvert.DeserializeObject<List<LanguageModel>>(JsonConvert.SerializeObject(Constants.Settings.ListLanguageCurrent));
                    var enumLanguages = EnumHelper.ToEnumList<ListLanguage>();
                    List<LanguageDetailModel> data = new List<LanguageDetailModel>();
                    foreach (var language in languages)
                    {
                        var item = _mapper.Map<LanguageDetailModel>(language);
                        item.Name = enumLanguages.FirstOrDefault(m => m.Id == language.Id)?.Name;
                        data.Add(item);
                    }
                    return data;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new List<LanguageDetailModel>();
            }
        }

        public bool CheckIpAllowInCompany(string ipAddress, int companyId)
        {
            try
            {
                var allowMobileAccessSetting = _settingService.GetByKey(Constants.Settings.KeyAllowMobileAccessWithoutWhiteList, companyId);
                if (allowMobileAccessSetting != null && bool.TryParse(Helpers.GetStringFromValueSetting(allowMobileAccessSetting.Value), out bool result) && result)
                {
                    foreach (var header in ColumnDefines.HeaderDeviceOs)
                    {
                        if (_httpContext.Request.Headers.TryGetValue(header, out var text) && !string.IsNullOrWhiteSpace(text))
                        {
                            return true;
                        }
                    }
                }
                
                var whiteListIpSetting = _settingService.GetByKey(Constants.Settings.KeyWhiteListIpAddress, companyId);
                if (whiteListIpSetting != null && !string.IsNullOrEmpty(whiteListIpSetting.Value))
                {
                    var whiteListIp = JsonConvert.DeserializeObject<List<string>>(whiteListIpSetting.Value);
                    if (whiteListIp == null || whiteListIp.Count == 0)
                    {
                        return true;
                    }
                    else
                    {
                        if (whiteListIp.Count == 1 && string.IsNullOrEmpty(whiteListIp.First()))
                        {
                            return true;
                        }
                        else
                        {
                            return whiteListIp.Contains(ipAddress);
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }


        public void ResetAllowIpSetting(int companyId)
        {
            try
            {
                var whiteListIpSetting = _settingService.GetByKey(Constants.Settings.KeyWhiteListIpAddress, companyId);
                if (whiteListIpSetting != null && !string.IsNullOrEmpty(whiteListIpSetting.Value))
                {
                    whiteListIpSetting.Value = JsonConvert.SerializeObject(new List<string>() { });

                    _unitOfWork.SettingRepository.Update(whiteListIpSetting);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        } 

        /// <summary>
        /// Update company's encryption setting.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void UpdateEncryptSetting(EncryptSettingModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var company = GetById(model.Id);

                        company.UseDataEncrypt = model.IsEnabled;

                        _unitOfWork.CompanyRepository.Update(company);

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in UpdateEncryptSetting");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }


        public EncryptSettingModel GetEncryptSetting(int id)
        {
            var company = GetById(id);

            EncryptSettingModel result = new EncryptSettingModel()
            {
                Id = id,
                IsEnabled = company.UseDataEncrypt,
            };

            return result;
        }


        /// <summary>
        /// Update company's expired PW setting.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void UpdateExpiredPWSetting(ExpiredPWSettingModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var company = GetById(model.Id);

                        company.UseExpiredPW = model.IsEnabled;
                        company.PwValidPeriod = model.Period;

                        _unitOfWork.CompanyRepository.Update(company);

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in UpdateExpiredPWSetting");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }


        public ExpiredPWSettingModel GetExpiredPWSetting(int id)
        {
            var company = GetById(id);

            ExpiredPWSettingModel result = new ExpiredPWSettingModel()
            {
                Id = id,
                IsEnabled = company.UseExpiredPW,
                Period = company.PwValidPeriod
            };

            return result;
        }





        //========================[TEST]===============================
        public string TestENC(string plainText)
        {
            try
            {
                var encText = _unitOfWork.AppDbContext.Account.Select(m => _unitOfWork.AppDbContext.Enc("normal", plainText, "")).FirstOrDefault();

                if (!string.IsNullOrEmpty(encText))
                {
                    plainText = encText;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Encrypt error");
                Console.WriteLine($"{e.Message}");
                Console.WriteLine($"{e.InnerException?.Message}");

                throw;
            }

            return plainText;
        }


        public string TestDEC(string encText)
        {
            try
            {
                var decPassword = _unitOfWork.AppDbContext.Account
                    .Select(m => _unitOfWork.AppDbContext.Dec("normal", encText, "", 0)).FirstOrDefault();

                if (!string.IsNullOrEmpty(decPassword)) encText = decPassword;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Decrypt error");
                Console.WriteLine($"{e.Message}");
                Console.WriteLine($"{e.InnerException?.Message}");

                throw;
            }

            return encText;
        }
    }
}