using AutoMapper;
using Bogus.Extensions;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Account;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Company;
using DeMasterProCloud.DataModel.Login;
using DeMasterProCloud.DataModel.Role;
using DeMasterProCloud.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using DeMasterProCloud.DataModel.SystemLog;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.DataModel.Header;

namespace DeMasterProCloud.Service
{
    /// <summary>
    /// UserLogin service interface
    /// </summary>
    public interface IAccountService
    {
        int Add(AccountModel model);

        void Update(AccountModel model);
        void UpdateProfile(AccountModel model);

        void UpdateTimeZone(AccountTimeZoneModel model, string userTimeZone = "");

        void Delete(CompanyAccount companyAccount);

        void DeleteRange(List<CompanyAccount> companyAccounts);

        void ChangePassword(Account account);

        IQueryable<AccountListModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered, List<int> companyIds, bool? ignoreApprovalVisit);
        Account GetById(int id);

        bool IsExist(string username);

        Account GetAuthenticatedAccount(LoginModel model);

        bool IsAllowDelete(Account account, Account accountLogin);

        Account GetAccountLogin(ClaimsPrincipal user);

        Account GetAccountByCurrentCompany(int id);

        AccountDataModel InitData(AccountDataModel model);

        Account GetAccountByEmail(string email);
        CompanyAccount GetCompanyAccountByEmail(string email, int companyId);

        bool HasDefaultAccount(/*int companyId*/);

        Dictionary<string, List<string>> GetTimeZone();

        Dictionary<string, string> GetInfomationSystem();

        IQueryable<CompanyAccount> GetPaginatedPrimaryAccount(string filter, int pageNumber, int pageSize, int sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        IEnumerable<EnumModel> GetAccountType();
        bool IsExistCompanyAccountbyAccountId(int accountId, int companyId);
        Account GetAccountByPhoneNumber(string phoneNumber);
        List<CompanyAccount> GetCompanyAccountByRoleId(int roleId, int companyId);
        List<CompanyAccount> GetCompanyAccountByRoleIds(List<int> roleIds, int companyId);

        List<Company> GetCompanyList(Account account);
        List<Company> GetPaginatedCompanyList(Account account, string search, int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered);

        List<AccountListModel> GetApprovalAccount();
        List<AccountListModel> GetApprovalAccount(string search, int type, bool isVisit, string sortColumn, string sortDirection, int pageNumber, int pageSize, out int filteredRecord, out int totalRecord);
        bool IsEmailValid(string emailAddress);
        int GetSystemTotalAccount();
        List<EnumModel> GetPreferredSystem();
        TokenModel CreateAuthToken(Account account, Company company, bool updateRefreshToken = true);
        TokenModel CreateAuthTokenAdmin(Account account, bool updateRefreshToken = true);
        void AddAccountToCompany(int accountId, int companyId, short roleId);
        //IQueryable<Account> GetPaginatedCompany(string search, int pageNumber, int pageSize, int sortColumn, string sortDirection, out int recordsTotal, out int recordsFiltered, List<int> companyIds);
        Account GetAccountsById(int accountId);
        CompanyAccount GetCompanyAccountById(int id);
        CompanyAccount GetCompanyAccountByIdAndCompany(int id, int companyId);
        List<CompanyAccount> GetCompanyAccountsByIds(List<int> idArr, int companyId);
        void ResetDefaultTimezoneByCompanyId(int companyId);

        List<AccountListModel> GetAccessApprovalAccount();

        LoginInfoModel GetLastLogin(int accountId, int companyId);
        List<AccountListModel> GetPaginatedAccountListByIds(string search, List<int> ids, int companyId, string pageName, int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int recordsFiltered, out int recordsTotal, out List<HeaderData> userHeader);

        TokenModel GetTokenWithAccountAndHashKey(AccountGetTokenModel model, int companyId);
        Dictionary<string, List<AccountListModel>> GetApprovalAccessSetting(int companyId);
        bool CheckAccountActivated(Account account, int companyId);
        Dictionary<string, int> GetAccountCountReview(int accountId, int companyId);
        void SendCountReviewToFe(int accountId, int companyId);

        void SetRootAccount(Account account, bool isRoot);
        void Logout(int accountId);
        bool CheckSessionInvalidLogin(Account account);
        void SaveCurrentLogin(int accountId);
        string HashJsonToQrCode(Account account, int companyId);
        void UpdateDeviceTokenForAccount(int accountId, string deviceToken);
        bool VerifyPassWord(string passWord);
        PasswordValidationResult ValidatePasswordComplexity(string password);
        PasswordStatusResult GetPasswordStatus(Account account, int companyId);
        bool IsPasswordExpired(Account account, int companyId);
        void UpdatePasswordChangedConfig(Account account);
        bool IsAccountLocked(LoginConfigModel loginConfig, LoginSettingModel loginSetting);
        LoginConfigModel ParseLoginConfig(string loginConfigJson);
        string GetAccountLockoutMessage(LoginSettingModel loginSetting);
        (bool isValid, string errorMessage, bool passwordChangeRequired, string warningMessage) ValidateAccountForLogin(Account account, int companyId);
    }

    /// <summary>
    /// Implement Account service inteface
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly ICompanyService _companyService;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IJwtHandler _jwtHandler;
        private readonly ISettingService _settingService;
        private readonly IRoleService _roleService;
        private readonly JwtOptionsModel _options;
        private readonly IDepartmentService _departmentService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Ctor for account service
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="companyService"></param>
        /// <param name="mailService"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        /// <param name="jwtHandler"></param>
        /// <param name="settingService"></param>
        /// <param name="roleService"></param>
        /// <param name="options"></param>
        /// <param name="departmentService"></param>
        /// <param name="mailTemplateService"></param>
        /// <param name="notificationService"></param>
        public AccountService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor,
            ICompanyService companyService,
            IConfiguration configuration, ILogger<AccountService> logger, IJwtHandler jwtHandler,
            ISettingService settingService, IRoleService roleService, IOptions<JwtOptionsModel> options,
            IDepartmentService departmentService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _companyService = companyService;
            _httpContext = httpContextAccessor.HttpContext;
            _logger = logger;
            _configuration = configuration;

            _jwtHandler = jwtHandler;
            _settingService = settingService;

            _roleService = roleService;
            _options = options?.Value;
            _departmentService = departmentService;
            _notificationService = notificationService;
            _mapper = MapperInstance.Mapper;
        }

        /// <summary>
        /// Add account
        /// </summary>
        /// <param name="model"></param>
        public int Add(AccountModel model)
        {
            var accountId = 0;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        int companyId = 0;
                        companyId = _httpContext.User.GetCompanyId();
                        Company company = null;
                        if (companyId != 0)
                        {
                            company = _unitOfWork.CompanyRepository.GetCompanyById(companyId);
                        }

                        // auto generate password if password of account model empty.
                        if (string.IsNullOrEmpty(model.Password))
                        {
                            model.Password = company == null ? Helpers.GenerateRandomPassword() : Helpers.GeneratePasswordDefaultWithCompany(company.Name);
                        }

                        var account = _mapper.Map<Account>(model);
                        DynamicRole dynamicRole = null;

                        // Initialize LoginConfig for new account
                        var loginConfig = new LoginConfigModel
                        {
                            IsFirstLogin = true,
                            TimeChangedPassWord = DateTime.UtcNow,
                            LoginFailCount = 0,
                            LastTimeLoginFail = DateTime.MinValue
                        };
                        account.LoginConfig = JsonConvert.SerializeObject(loginConfig);

                        var currentLoggedInAccount = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId());
                        if (currentLoggedInAccount != null)
                        {
                            account.Language = currentLoggedInAccount.Language;
                        }

                        if (_httpContext.User.GetAccountType() != (int)AccountType.SystemAdmin)
                        {
                            account.CompanyId = companyId;

                            dynamicRole = _unitOfWork.RoleRepository.GetById((int)model.Role);
                            //account.DynamicRoleId = dynamicRole.Id;
                            //account.Type = (short)dynamicRole.TypeId;

                            account.UpdatePasswordOn = DateTime.UtcNow;
                        }
                        else
                        {
                            if (account.CompanyId == null || account.CompanyId == 0)
                            {
                                // System admin
                                account.Type = model.Role;
                            }
                            else
                            {
                                companyId = (int)account.CompanyId;
                                account.CompanyId = companyId;

                                dynamicRole = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)model.Role, account.CompanyId.Value).FirstOrDefault();
                                //account.DynamicRoleId = dynamicRole.Id;
                                //account.Type = (short)dynamicRole.TypeId;

                                account.UpdatePasswordOn = DateTime.UtcNow;
                            }
                        }
                        var buildingDefault = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(companyId);
                        account.TimeZone = string.IsNullOrEmpty(account.TimeZone)
                            ? (buildingDefault?.TimeZone ?? TimeZoneInfo.Local.Id)
                            : account.TimeZone;


                        _unitOfWork.AccountRepository.Add(account);

                        // Save for account ID ( new added account )
                        // The new account ID should be used for storing system log.
                        _unitOfWork.Save();

                        // Add Company account object
                        if (companyId != 0)
                        {
                            CompanyAccount companyAccount = new CompanyAccount
                            {
                                AccountId = account.Id,
                                CompanyId = companyId,
                                DynamicRoleId = dynamicRole.Id,
                                //TimeZone = localZone.Id
                            };
                            _unitOfWork.CompanyAccountRepository.Add(companyAccount);
                        }
                        else
                        {
                            CompanyAccount sysCompanyAccount = new CompanyAccount
                            {
                                AccountId = account.Id,
                            };
                            _unitOfWork.CompanyAccountRepository.Add(sysCompanyAccount);
                        }

                        _unitOfWork.Save();

                        if (account.CompanyId.HasValue)
                        {
                            company = _unitOfWork.CompanyRepository.GetCompanyById(companyId);
                            account.Company = company;

                            //// Update PW to encrypted PW.
                            //var companyIds = _unitOfWork.CompanyAccountRepository.GetCompanyIdsByAccount(account.Id).Select(m => m.Value).ToList();
                            //if (companyIds.Any())
                            //{
                            //    var companies = _unitOfWork.CompanyRepository.GetByIds(companyIds);
                            //    if (companies.Any(m => m.UseDataEncrypt))
                            //    {
                            //        try
                            //        {
                            //            var newAccount = _unitOfWork.AccountRepository.GetById(account.Id);

                            //            if (newAccount != null)
                            //            {
                            //                var updateQuery = DbHelper.MakeEncQuery("Account", "Password", account.Password, account.Id);

                            //                Console.WriteLine($"Update Query : {updateQuery}");

                            //                var result = _unitOfWork.AppDbContext.Database.ExecuteSqlRaw(updateQuery);

                            //                _unitOfWork.Save();
                            //            }
                            //            else
                            //            {
                            //                Console.WriteLine($"[DEBUG] Account is null !!");
                            //            }
                            //        }
                            //        catch (Exception e)
                            //        {
                            //            var updateQuery = DbHelper.MakeEncQuery("Account", "Password", account.Password, account.Id);

                            //            // 2022-04-08
                            //            Console.WriteLine($"[ERROR] Encrypt error");
                            //            Console.WriteLine($"{e.Message}");
                            //            Console.WriteLine($"{e.InnerException?.Message}");
                            //            Console.WriteLine($"Update Query : {updateQuery}");
                            //        }
                            //    }
                            //}

                            // make content for storing system log
                            var content = string.Format(AccountResource.lblAddNew);
                            // make contentsDetails for storing system log
                            List<string> details = new List<string>
                            {
                                $"{AccountResource.lblEmail} : {account.Username}",
                                $"{AccountResource.lblRole} : {((AccountType)dynamicRole.TypeId).GetDescription()}"
                            };
                            var contentsDetails = string.Join("\n", details); ;

                            //Save system log
                            _unitOfWork.SystemLogRepository.Add(account.Id, SystemLogType.AccountManagement, ActionLogType.Add,
                                    content, contentsDetails, null, account.CompanyId);

                            _unitOfWork.Save();
                        }

                        //Save and commit
                        _unitOfWork.Save();
                        transaction.Commit();
                        accountId = account.Id;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Failed to add account.");
                        _logger.LogError($"{e.Message}:{Environment.NewLine} {e.StackTrace}");

                        Console.WriteLine($"{e.InnerException?.Message}");

                        transaction.Rollback();

                        throw;
                    }
                }
            });
            return accountId;
        }

        /// <summary>
        /// Update account
        /// Update : password, language, timezone
        /// update company account
        /// update : Role, type
        /// </summary>
        /// <param name="model"></param>
        public void Update(AccountModel model)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();

                var compAccount = GetCompanyAccountById(model.Id);

                var account = GetById(compAccount.AccountId.Value);

                List<string> changes = new List<string>();

                var oldType = 0;

                try
                {
                    if(compAccount.DynamicRoleId != null && compAccount.DynamicRole != null)
                    {
                        oldType = compAccount.DynamicRole.TypeId;
                    }
                }
                catch
                {

                }

                var hasChange = HasChange(compAccount, model, ref changes);
                _mapper.Map(model, account);
                var oldCompanyId = compAccount.CompanyId;

                //Mapper.Map(model, account);

                if (_httpContext.User.GetAccountType() != (int)AccountType.SystemAdmin)
                {
                    compAccount.DynamicRoleId = (int)model.Role;
                }
                else
                {
                    if (model.CompanyId != null && model.CompanyId != 0)
                    {
                        compAccount.CompanyId = model.CompanyId;
                        compAccount.DynamicRoleId = (int)model.Role;
                    }
                }
                _unitOfWork.AccountRepository.Update(account);
                _unitOfWork.CompanyAccountRepository.Update(compAccount);

                _unitOfWork.Save();

                var newCompanyId = compAccount.CompanyId;
                
                //Save system log

                if (oldCompanyId == newCompanyId && oldCompanyId != 0)
                {
                    // In this case, company info has not changed.
                    // check change about extra data.
                    if (hasChange)
                    {
                        // Save the system log type as update.
                        var content = $"{AccountResource.lblUpdate}\n{AccountResource.lblEmail} : {account.Username}";
                        var contentsDetails = string.Join("\n", changes);
                        _unitOfWork.SystemLogRepository.Add(account.Id, SystemLogType.AccountManagement, ActionLogType.Update,
                            content, contentsDetails, null, oldCompanyId);
                    }
                }
                else
                {
                    // In this case, company info has changed.
                    // It is not important if extra data is changed or not.
                    // Just save the system log as "remove" or "add" to old or new company.
                    if (oldCompanyId != null)
                    {
                        // save the "Remove" system log to old company
                        var content = string.Format(AccountResource.lblDelete);
                        var contentsDetails = $"{AccountResource.lblEmail} : {account.Username}";

                        _unitOfWork.SystemLogRepository.Add(compAccount.Id, SystemLogType.AccountManagement, ActionLogType.Delete,
                            content, contentsDetails, null, oldCompanyId);

                        // DeletePrimaryManager(model, Convert.ToInt32(oldCompanyId));
                    }

                    if (newCompanyId != null && newCompanyId != 0)
                    {
                        // save the "Add" system log to new company
                        var content = string.Format(AccountResource.lblAddNew);

                        List<string> details = new List<string>
                        {
                            $"{AccountResource.lblEmail} : {account.Username}",
                            $"{AccountResource.lblRole} : {((AccountType)compAccount.DynamicRole.TypeId).GetDescription()}"
                        };

                        var contentsDetails = string.Join("\n", details);

                        _unitOfWork.SystemLogRepository.Add(account.Id, SystemLogType.AccountManagement, ActionLogType.Add,
                            content, contentsDetails, null, newCompanyId);
                    }
                }

                //Save and commit
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        /// <summary>
        /// Update account profile
        /// Update : password, language, timezone
        /// </summary>
        /// <param name="model"></param>
        public void UpdateProfile(AccountModel model)
        {
            try
            {
                var account = GetById(model.Id);
                var companyAccount = GetCompanyAccountByIdAndCompany(account.Id, _httpContext.User.GetCompanyId());
                //var companyAccount2 = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompanyAndAccount(_httpContext.User.GetCompanyId(), account.Id);

                List<string> changes = new List<string>();

                var hasChange = HasChange(account, model, ref changes);

                _mapper.Map(model, account);

                if (_httpContext.User.GetAccountType() != (int)AccountType.SystemAdmin)
                {
                    account.TimeZone = model.TimeZone ?? account.TimeZone;
                    //companyAccount.TimeZone = model.TimeZone ?? companyAccount.TimeZone;
                    account.Language = model.Language ?? account.Language;
                    //account.PreferredSystem = model.TimeZone == null && model.Language== null ? account.PreferredSystem : model.PreferredSystem;
                    companyAccount.PreferredSystem = model.TimeZone == null && model.Language == null ? account.PreferredSystem : model.PreferredSystem;
                }
                else
                {
                    account.TimeZone = model.TimeZone ?? account.TimeZone;
                    //companyAccount.TimeZone = model.TimeZone ?? companyAccount.TimeZone;
                    account.Language = model.Language ?? account.Language;
                }

                _unitOfWork.AccountRepository.Update(account);
                if (companyAccount != null)
                    _unitOfWork.CompanyAccountRepository.Update(companyAccount);

                _unitOfWork.Save();

                //Save system log
                if (hasChange)
                {
                    // Save the system log type as update.
                    var content = $"{AccountResource.lblUpdate}\n{AccountResource.lblEmail} : {account.Username}";
                    var contentsDetails = string.Join("\n", changes);
                    _unitOfWork.SystemLogRepository.Add(account.Id, SystemLogType.AccountManagement, ActionLogType.Update,
                        content, contentsDetails, null, _httpContext.User.GetCompanyId());
                }

                //Save and commit
                _unitOfWork.Save();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Update account
        /// </summary>
        /// <param name="model"></param>
        public void UpdateTimeZone(AccountTimeZoneModel model, string userTimeZone = "")
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(userTimeZone))
                        {
                            var account = GetById(model.Id);
                            if (account != null)
                            {
                                var companyAccount = GetCompanyAccountByIdAndCompany(account.Id, _httpContext.User.GetCompanyId());

                                if (companyAccount != null)
                                {
                                    //companyAccount.TimeZone = userTimeZone;
                                    account.TimeZone = userTimeZone;

                                    //Save and commit
                                    //_unitOfWork.CompanyAccountRepository.Update(companyAccount);
                                    _unitOfWork.AccountRepository.Update(account);
                                    _unitOfWork.Save();
                                    transaction.Commit();
                                }
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

        /// <summary>
        /// Delete company account
        /// </summary>
        /// <param name="companyAccount"></param>
        public void Delete(CompanyAccount companyAccount)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // check if there are any remained companyAccount about this account.
                        var otherCompanyAccount = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByAccount(companyAccount.AccountId.Value);

                        otherCompanyAccount.Remove(companyAccount);

                        if (otherCompanyAccount.Count == 0)
                        {
                            var account = _unitOfWork.AccountRepository.GetById(companyAccount.AccountId.Value);
                            // Delete account from system.
                            // Set "IsDeleted" to TRUE.
                            _unitOfWork.AccountRepository.DeleteFromSystem(account);
                        }

                        _unitOfWork.CompanyAccountRepository.Delete(companyAccount);

                        //Save system log
                        var content = string.Format(AccountResource.lblDelete);
                        var contentsDetails = $"{AccountResource.lblEmail} : {companyAccount.Account.Username}";
                        _unitOfWork.SystemLogRepository.Add(companyAccount.Id, SystemLogType.AccountManagement, ActionLogType.Delete,
                                content, contentsDetails, null, companyAccount.CompanyId);

                        //Save and commit
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
        /// Delete a list of company accounts
        /// </summary>
        /// <param name="accounts"></param>
        public void DeleteRange(List<CompanyAccount> companyAccounts)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var companyAccount in companyAccounts)
                        {
                            // check if there are any remained companyAccount about this account.
                            var otherCompanyAccount = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByAccount(companyAccount.AccountId.Value);

                            otherCompanyAccount.Remove(companyAccount);

                            if (otherCompanyAccount.Count == 0)
                            {
                                var account = _unitOfWork.AccountRepository.GetById(companyAccount.AccountId.Value);
                                _unitOfWork.AccountRepository.DeleteFromSystem(account);
                            }

                            _unitOfWork.CompanyAccountRepository.Delete(companyAccount);

                            //Save system log
                            var content = string.Format(AccountResource.lblDelete);
                            var contentsDetails = $"{AccountResource.lblEmail} : {companyAccount.Account.Username}";
                            _unitOfWork.SystemLogRepository.Add(companyAccount.Id, SystemLogType.AccountManagement, ActionLogType.Delete,
                                content, contentsDetails, null, companyAccount.CompanyId);

                            _unitOfWork.Save();
                        }

                        //Save and commit
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[FAIL] Failed to delete account. MESSAGE : {e.Message}\nSTACKTRACE : {e.StackTrace}\nDETAIL : {e.InnerException?.Message}");
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Get account by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Account GetById(int id)
        {
            try
            {
                var account = _unitOfWork.AppDbContext.Account.FirstOrDefault(m => m.Id == id && !m.IsDeleted);

                if (account != null)
                {
                    if (account.CompanyId != 0)
                    {
                        var company = _companyService.GetById(account.CompanyId ?? 0);
                        account.Company = company;
                    }
                    else
                    {
                        //account.Company = new Company();
                    }
                }

                //return _unitOfWork.AppDbContext.Account/*.Include(m => m.Company)*/.FirstOrDefault(m =>
                //    m.Id == id && !m.IsDeleted /*&& !m.Company.IsDeleted*/);
                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return null;
            }
        }

        /// <summary>
        /// Get list of company account by ids
        /// </summary>
        /// <param name="idArr"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<CompanyAccount> GetCompanyAccountsByIds(List<int> idArr, int companyId)
        {
            try
            {
                var companyAccounts = _unitOfWork.AppDbContext.CompanyAccount.Include(m => m.Company).Include(m => m.Account).Include(m => m.DynamicRole)
                    .Where(m => idArr.Any(x => x == m.Id));

                if (companyId == 0)
                {
                    return companyAccounts.ToList();
                }
                else
                {
                    return companyAccounts.Where(m => m.CompanyId == companyId).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompanyAccountsByIds");
                return new List<CompanyAccount>();
            }
        }

        /// <summary>
        /// Get data with pagination
        /// </summary>
        /// <param name="filter">username - email</param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <param name="companyIds"></param>
        /// <param name="ignoreApprovalVisit"></param>
        /// <returns></returns>
        public IQueryable<AccountListModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered, List<int> companyIds, bool? ignoreApprovalVisit)
        {
            // Function flow.
            // 1. Get data
            // 2. Filter data
            //  - filter
            //  - companyIds
            // 3. Change data form(return form)
            // 4. Sort data
            // 5. Paginate data

            // ========================== [ 1.Get Data ] ==========================
            IQueryable<CompanyAccount> companyData = _unitOfWork.AppDbContext.CompanyAccount
                .Include(m => m.Company)
                .Include(m => m.DynamicRole)
                .Include(m => m.Account)
                .Where(m => /*m.DynamicRole.TypeId >= accountType &&*/ !m.Account.IsDeleted);

            IQueryable<CompanyAccount> systemData = _unitOfWork.AppDbContext.CompanyAccount
                .Include(m => m.Account)
                .Where(m => m.CompanyId == null);

            totalRecords = companyData.Count();

            // ========================= [ 2.Filter data ] ============================
            // get current companyId ( to filter by companyId )
            var companyId = _httpContext.User.GetCompanyId();
            bool isSystem = true;

            if (companyIds == null || companyIds.Count == 0)
            {
                if (companyId != 0)
                {
                    // this case -> normal company (not from system admin UI)
                    // API should return only this company's data
                    isSystem = false;

                    var currentAccount = _unitOfWork.AppDbContext.CompanyAccount
                        .Include(m => m.DynamicRole)
                        .FirstOrDefault(m => m.AccountId == _httpContext.User.GetAccountId() && m.CompanyId == companyId);
                    companyData = companyData.Where(m => m.DynamicRole.TypeId >= currentAccount.DynamicRole.TypeId);

                    companyIds = new List<int> { companyId };
                }
                else
                {
                    // this case -> system admin UI (some companies selected)
                    // API should return only selected company's data
                    isSystem = true;

                    companyIds = _unitOfWork.AppDbContext.Company.Where(m => !m.IsDeleted).Select(m => m.Id).ToList();
                }
            }

            IQueryable<AccountListModel> result = null;

            var users = _unitOfWork.AppDbContext.User.Include(m => m.Department)
                .Where(m => companyIds.Contains(m.CompanyId) && !m.IsDeleted);

            // check if an user typed filter(some text) or not ( to filter by "filter" )
            if (!string.IsNullOrEmpty(filter))
            {
                // this case -> user typed nothing filter to search
                // search filter list - email, linked user's name
                var normalizedFilter = filter.Trim().RemoveDiacritics().ToLower();

                result = (from A in companyData
                          join B in users
                          on A.AccountId equals B.AccountId into data
                          from C in data.DefaultIfEmpty()
                          where companyIds.Contains(A.CompanyId.Value)
                          && (!isSystem || A.DynamicRole.TypeId == (short)AccountType.PrimaryManager)
                          // [ 3.Change data form -> AccountListModel ]
                          select new AccountListModel
                          {
                              Id = A.Id,
                              AccountId = A.AccountId.Value,
                              Email = A.Account.Username,
                              UserName = (C.FirstName ?? "" + " " + C.LastName ?? "").Trim(),
                              FirstName = (C.FirstName ?? "" + " " + C.LastName ?? "").Trim(),
                              Role = A.DynamicRole.Name,
                              CompanyName = A.Company.Name,
                              Department = C.Department.DepartName,
                              Position = C.Position,
                          }).AsEnumerable()
                          .Where(m => m.Email.RemoveDiacritics().ToLower().Contains(normalizedFilter)
                                   || m.FirstName.RemoveDiacritics().ToLower().Contains(normalizedFilter))
                          .DistinctBy(m => m.Id).AsQueryable();

                systemData = systemData.AsEnumerable().Where(m => m.Account.Username.RemoveDiacritics().ToLower().Contains(normalizedFilter)).AsQueryable();
            }
            else
            {
                result = (from A in companyData
                          join B in users
                          on A.AccountId equals B.AccountId into data
                          from C in data.DefaultIfEmpty()
                          where companyIds.Contains(A.CompanyId ?? 0)
                          && (!isSystem || A.DynamicRole.TypeId == (short)AccountType.PrimaryManager)
                          // [ 3.Change data form -> AccountListModel ]
                          select new AccountListModel
                          {
                              Id = A.Id,
                              AccountId = A.AccountId.Value,
                              Email = A.Account.Username,
                              UserName = (C.FirstName ?? "" + " " + C.LastName ?? "").Trim(),
                              FirstName = (C.FirstName ?? "" + " " + C.LastName ?? "").Trim(),
                              Role = A.DynamicRole.Name,
                              CompanyName = A.Company.Name,
                              Department = C.Department.DepartName,
                              Position = C.Position,
                          }).AsEnumerable().DistinctBy(m => m.Id).AsQueryable();
            }

            if (isSystem)
            {
                var systemResult = systemData.AsEnumerable().Select(_mapper.Map<AccountListModel>).AsQueryable();

                foreach (var systemAccount in systemResult)
                {
                    result = result.Append(systemAccount);
                }
            }

            if (ignoreApprovalVisit != null && (bool)ignoreApprovalVisit)
            {
                IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                var visitService = new VisitService(unitOfWork, new HttpContextAccessor(),
                    null, null, null, null, null, null, null, null, null, null);
                var visitSetting = visitService.GetVisitSettingCompany();
                List<int> accountIgnoreIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.FirstApproverAccounts);
                result = result.Where(x => !accountIgnoreIds.Contains(x.AccountId));
                unitOfWork.Dispose();
            }

            recordsFiltered = result.Count();

            // ============================== [ 4.Sort data ] ================================
            // Default sort ( asc - Email )
            result = result.OrderBy(c => c.Email);

            try
            {
                int int_sortColumn = Int32.Parse(sortColumn);

                int_sortColumn = int_sortColumn > ColumnDefines.AccountListModel.Length - 1 ? 0 : int_sortColumn;
                result = result.OrderBy($"{ColumnDefines.AccountListModel[int_sortColumn]} {sortDirection}");
            }
            catch
            {
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    sortColumn = Char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);

                    if (result.FirstOrDefault()?.GetType().GetProperty(sortColumn) != null)
                    {
                        if (sortDirection.Equals("desc"))
                        {
                            result = result.OrderByDescending(c => c.GetType().GetProperty(sortColumn).GetValue(c, null));
                        }
                        else if (sortDirection.Equals("asc"))
                        {
                            result = result.OrderBy(c => c.GetType().GetProperty(sortColumn).GetValue(c, null));
                        }
                    }
                }
            }

            // ============================ [ 5.Paginate data ] =============================
            if (pageNumber != 0 && pageSize != 0)
                result = result.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return result;

            //// get user by filter as firstname
            //if (string.IsNullOrEmpty(filter))
            //    filter = "";

            //var companyId = _httpContext.User.GetCompanyId();
            //var accountType = _httpContext.User.GetAccountType();
            //List<User> users = _unitOfWork.UserRepository.GetUsersByFirstName(filter);

            //// Add companyId on array companyIds
            //foreach (var user in users)
            //{
            //    if (companyIds.Find(c => c == user.CompanyId) == null)
            //        companyIds.Add(user.CompanyId);
            //}
            ////IQueryable<Account> data = _unitOfWork.AppDbContext.Account.Include(m => m.Company)
            ////    .Where(m => !m.IsDeleted && m.Type >= accountType);

            //// get company account by array companyIds
            //IQueryable<CompanyAccount> companyData = _unitOfWork.AppDbContext.CompanyAccount
            //.Include(m => m.Company).Include(m => m.DynamicRole).Include(m => m.Account)
            //.Where(m => /*m.DynamicRole.TypeId >= accountType &&*/ !m.Account.IsDeleted);

            //if (companyId != 0)
            //{
            //    //data = data.Where(m => m.CompanyId == companyId && m.Company.IsDeleted == false);

            //    companyData = companyData.Where(m => m.CompanyId == companyId);
            //}

            ////totalRecords = data.Count();
            //totalRecords = companyData.Count();

            //if (companyIds.Count() > 0)
            //{
            //    //data = data.Where(x => companyIds.Contains(Convert.ToInt32(x.CompanyId)));

            //    companyData = companyData.Where(x => companyIds.Contains(Convert.ToInt32(x.CompanyId)));
            //}

            //// Filter username company account - filter
            //// Filter accountId in list users filtered 
            //companyData = companyData.Where(x => x.Account.Username.ToUpper().Contains(filter.ToUpper()) || users.Find(u => u.AccountId == x.AccountId) != null);

            ////recordsFiltered = data.Count();
            //recordsFiltered = companyData.Count();

            //foreach (var companyAccount in companyData)
            //{
            //    if (companyAccount.DynamicRoleId == null)
            //    {
            //        _logger.LogError("ACCOUNT ERROR : " + companyAccount.Account.Username);
            //    }

            //    User user = users.Find(u => u.AccountId == companyAccount.AccountId);
            //    if (user != null)
            //    {
            //        if (companyAccount.Account == null)
            //            companyAccount.Account = new Account();
            //        companyAccount.Account.FirstName = user.FirstName;
            //    }
            //}

            ////sortColumn = sortColumn > ColumnDefines.Account.Length - 1 ? 0 : sortColumn;
            ////data = data.OrderBy($"{ColumnDefines.Account[sortColumn]} {sortDirection}");

            //sortColumn = sortColumn > ColumnDefines.CompanyAccount.Length - 1 ? 0 : sortColumn;
            //companyData = companyData.OrderBy($"{ColumnDefines.CompanyAccount[sortColumn]} {sortDirection}");

            ////data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            //companyData = companyData.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            ////return data;
            //return companyData;
        }

        /// <summary>
        /// Check if account is exist ( only name )
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public bool IsExist(string username)
        {
            try
            {
                if (string.IsNullOrEmpty(username)) return true;

                var userLogin = _unitOfWork.AccountRepository.Get(m =>
                    m.Username.ToLower() == username.ToLower() && !m.IsDeleted);

                return userLogin != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsExist");
                return false;
            }
        }

        /// <summary>
        /// Get authentication account
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Account GetAuthenticatedAccount(LoginModel model)
        {
            try
            {
                var account = _unitOfWork.AppDbContext.Account
                    .Include(m => m.Company)
                    .Include(m => m.CompanyAccount).ThenInclude(ca => ca.Company)
                    .FirstOrDefault(m =>
                        m.Username.ToLower() == model.Username.ToLower() /*&& m.Company.Code == model.CompanyCode*/ &&
                        !m.IsDeleted /*&& !m.Company.IsDeleted /*&& m.Status == (short)Status.Valid*/);

                if (account != null)
                {
                    var companyId = account.CompanyId ?? account.CompanyAccount?.FirstOrDefault()?.CompanyId ?? 0;

                    var loginSetting = _unitOfWork.SettingRepository.GetLoginSetting(companyId);
                    var loginConfig = ParseLoginConfig(account.LoginConfig);



                    var password = account.Password;
                    bool isPasswordValid = false;

                    // Check password
                    if (SecurePasswordHasher.VerifySHA256Hash(model.Password, password))
                    {
                        isPasswordValid = true;
                    }
                    else
                    {
                        // Check password
                        if (SecurePasswordHasher.Verify(model.Password, password))
                        {
                            account.Password = SecurePasswordHasher.GetSHA256Hash(model.Password);
                            isPasswordValid = true;
                        }
                    }

                    if (isPasswordValid)
                    {
                        // // Reset login fail count on successful login
                        // loginConfig.LoginFailCount = 0;
                        // account.LoginConfig = JsonConvert.SerializeObject(loginConfig);
                        // _unitOfWork.AccountRepository.Update(account);
                        // _unitOfWork.Save();
                        return account;
                    }
                    else
                    {
                        // Increment login fail count
                        loginConfig.LoginFailCount++;
                        loginConfig.LastTimeLoginFail = DateTime.UtcNow;
                        account.LoginConfig = JsonConvert.SerializeObject(loginConfig);
                        _unitOfWork.AccountRepository.Update(account);
                        _unitOfWork.Save();
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAuthenticatedAccount");
                return null;
            }
        }

        /// <summary>
        /// Get account roles
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public IEnumerable<EnumModel> GetAccountRoles()
        {
            //var accountType = _httpContext.User.GetAccountType();
            return EnumHelper.ToEnumList<AccountType>();
        }

        /// <summary>
        /// Get account role by selected
        /// </summary>
        /// <param name="selected"></param>
        /// <returns></returns>
        public IEnumerable<SelectListItem> GetAccountRoles(short? selected)
        {
            var accountType = _httpContext.User.GetAccountType();
            return EnumHelper.ToSelectList<AccountType>(selected).Where(m =>
                Convert.ToInt16(m.Value) >= accountType);
        }

        /// <summary>
        /// Get root account by company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Account GetRootAccountByCompany(int companyId)
        {
            try
            {
                return _unitOfWork.AccountRepository.GetRootAccountByCompany(companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRootAccountByCompany");
                return null;
            }
        }

        /// <summary>
        /// Get valid account
        /// </summary>
        /// <param name="username"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Account GetValidAccount(string username, int companyId)
        {
            try
            {
                if (companyId == 0)
                {
                    return _unitOfWork.AppDbContext.Account
                    .FirstOrDefault(m => m.Username.ToLower() == username.ToLower() && !m.IsDeleted);
                }

                var account = _unitOfWork.AppDbContext.Account.Include(m => m.Company).Include(m => m.CompanyAccount)
                    .FirstOrDefault(m =>
                        m.Username.ToLower() == username.ToLower() &&
                        !m.IsDeleted && !m.Company.IsDeleted);

                if (account != null && _companyService.IsValidCompany(account.Company))
                {
                    return account;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetValidAccount");
                return null;
            }
        }

        /// <summary>
        /// Check if account is allowed delete
        /// </summary>
        /// <param name="account"></param>
        /// <param name="accountLogin"></param>
        /// <returns></returns>
        public bool IsAllowDelete(Account account, Account accountLogin)
        {
            try
            {
                if (account.Id == accountLogin.Id)
                {
                    return false;
                }

                if (account.RootFlag)
                {
                    return false;
                }

                if (account.CompanyId == null || account.Company == null || account.Company.IsDeleted)
                {
                    return true;
                }

                // [Edward]
                // Added a new condition to delete account.
                // If the account to be deleted is approval account, the account cannot be deleted.
                var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(account.CompanyId.Value);

                if (visitSetting.ApprovalStepNumber == 1)
                {
                    var ids = JsonConvert.DeserializeObject<List<int>>(visitSetting.FirstApproverAccounts);

                    if (ids.Contains(account.Id))
                        return false;
                }
                else if (visitSetting.ApprovalStepNumber == 2)
                {
                    var firstIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.FirstApproverAccounts);
                    var secondIds = JsonConvert.DeserializeObject<List<int>>(visitSetting.SecondsApproverAccounts);

                    if (firstIds.Contains(account.Id) || secondIds.Contains(account.Id))
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsAllowDelete");
                return false;
            }
        }

        /// <summary>
        /// Get login account
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Account GetAccountLogin(ClaimsPrincipal user)
        {
            try
            {
                return GetValidAccount(user.GetUsername(), user.GetCompanyId());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccountLogin");
                return null;
            }
        }

        /// <summary>
        /// Change the password
        /// </summary>
        /// <param name="account"></param>
        public void ChangePassword(Account account)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        _unitOfWork.AccountRepository.Update(account);
                        _unitOfWork.Save();

                        //var companyIds = _unitOfWork.CompanyAccountRepository.GetCompanyIdsByAccount(account.Id).Select(m => m.Value).ToList();
                        //if (companyIds.Any())
                        //{
                        //    var companies = _unitOfWork.CompanyRepository.GetByIds(companyIds);
                        //    if (companies.Any(m => m.UseDataEncrypt))
                        //    {
                        //        try
                        //        {
                        //            var updateQuery = DbHelper.MakeEncQuery("Account", "Password", account.Password, account.Id);
                        //            var result = _unitOfWork.AppDbContext.Database.ExecuteSqlRaw(updateQuery);

                        //            _unitOfWork.Save();
                        //        }
                        //        catch (Exception e)
                        //        {

                        //        }
                        //    }
                        //}

                        //Save system log
                        var content = $"{AccountResource.lblUpdate}\n{AccountResource.lblEmail} : {account.Username}";
                        var contentsDetails = string.Format(AccountResource.msgChangePassword, account.Username);
                        _unitOfWork.SystemLogRepository.Add(account.Id, SystemLogType.AccountManagement, ActionLogType.Update,
                            content, contentsDetails, null, account.CompanyId, account.Id);

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
        /// get token through account infomation
        /// </summary>
        /// <param name="account"> Account </param>
        /// <returns> token </returns>
        public string GetTokenByAccount(Account account)
        {
            try
            {
                string companyCode = "";
                string companyName = "";

                if (account.Type != (short)AccountType.SystemAdmin)
                {
                    var company = account.Company;
                    if (company == null)
                    {
                        company = _companyService.GetById(account.CompanyId);

                    }

                    companyCode = company.Code;
                    companyName = company.Name;
                }

                var claims = new[]
                {
                    new Claim(Constants.ClaimName.Username, account.Username),
                    new Claim(Constants.ClaimName.AccountId, account.Id.ToString()),
                    new Claim(Constants.ClaimName.CompanyId, account.Type == (short)AccountType.SystemAdmin ? "0" :account.CompanyId.ToString()),
                    new Claim(Constants.ClaimName.CompanyCode, account.Type == (short)AccountType.SystemAdmin ? "s000001" : companyCode),
                    new Claim(Constants.ClaimName.CompanyName, account.Type == (short)AccountType.SystemAdmin ? "SystemAdmin" : companyName),
                    new Claim(Constants.ClaimName.AccountType, account.Type.ToString()),
                    new Claim(Constants.ClaimName.Timezone, account.TimeZone)
                };

                var token = _jwtHandler.BuilToken(claims);

                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTokenByAccount");
                return null;
            }
        }

        /// <summary>
        /// Get user name by email address
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public string GetUserNameByEmail(int companyId, string emailAddress)
        {
            try
            {
                if (emailAddress == null)
                {
                    return null;
                }

                var user = _unitOfWork.UserRepository.GetUserByEmail(companyId, emailAddress);

                if (user != null)
                {
                    return $"{user.FirstName} {user.LastName}".Trim();
                }
                else
                {
                    return emailAddress;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserNameByEmail");
                return null;
            }
        }

        /// <summary>
        /// Check if the email is valid or not
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public bool IsEmailValid(string emailAddress)
        {
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

        /// <summary>
        /// Get account by account Id.
        /// This function searches account through accountId only within the current company.
        /// </summary>
        /// <param name="accountId"> identifier of account </param>
        /// <returns></returns>
        public Account GetAccountByCurrentCompany(int accountId)
        {
            try
            {
                int companyId = _httpContext.User.GetCompanyId();

                //return _unitOfWork.AppDbContext.Account.Include(m => m.Company)
                //    .FirstOrDefault(m => m.Id == accountId && !m.IsDeleted && m.CompanyId == companyId);

                return _unitOfWork.AppDbContext.CompanyAccount
                    .Include(m => m.Account)
                    .Where(m => m.AccountId == accountId && !m.Account.IsDeleted && m.CompanyId == companyId)
                    .Select(m => m.Account).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccountByCurrentCompany");
                return null;
            }
        }

        /// <summary>
        /// Initial data (AccountDataModel)
        /// </summary>
        /// <param name="model"></param>
        public AccountDataModel InitData(AccountDataModel model)
        {
            try
            {
                var currentAccountType = _httpContext.User.GetAccountType();
                var currentCompanyId = _httpContext.User.GetCompanyId();

                if (currentAccountType == (short)AccountType.SystemAdmin)
                {
                    // System admin

                    var data = _unitOfWork.AppDbContext.Company
                    .Where(c => !c.IsDeleted)
                    .Select(m => new CompanyListModel
                    {
                        Id = m.Id,
                        Code = m.Code,
                        Name = m.Name,
                        Remarks = m.Remarks
                    });
                    model.CompanyIdList = data;

                    var accountTypes = GetAccountRoles();
                    var accountType = accountTypes.ToList();

                    accountType.Remove(accountType.Where(m => m.Id == (short)AccountType.SuperAdmin).FirstOrDefault());
                    accountType.Remove(accountType.Where(m => m.Id == (short)AccountType.DynamicRole).FirstOrDefault());
                    accountTypes = accountType;

                    model.RoleList = accountTypes;
                }
                else
                {
                    // Company manager

                    model.CompanyIdList = null;

                    var roleList = _roleService.GetRoleList();
                    model.RoleList = roleList.AsEnumerable();

                    if (model.Id == 0)
                    {
                        var role = _unitOfWork.RoleRepository.GetByTypeAndCompanyId((int)AccountType.PrimaryManager, currentCompanyId);
                        model.Role = role.FirstOrDefault().Id;

                        model.CompanyId = currentCompanyId;
                    }
                }

                //model.CompanyId = model.CompanyId.ToString() == null ? currentCompanyId : model.CompanyId;

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitData");
                return null;
            }
        }

        /// <summary>
        /// Checking if there are any changes.
        /// </summary>
        /// <param name="account">Account that contains existing information</param>
        /// <param name="model">Model that contains new information</param>
        /// <param name="changes">List of changes</param>
        /// <returns></returns>
        internal bool HasChange(Account account, AccountModel model, ref List<string> changes)
        {
            if (model.Id != 0)
            {
                if (account.Username.ToLower() != model.Username.ToLower())
                {
                    //changes.Add(string.Format(MessageResource.msgChangeInfo, AccountResource.lblEmail, account.Username, model.Username));
                    changes.Add(Helpers.CreateChangedValueContents(AccountResource.lblEmail, account.Username, model.Username));
                }

                //if (account.Type != model.Role)
                //{
                //    changes.Add(string.Format(MessageResource.msgChangeInfo,
                //                                AccountResource.lblRole,
                //                                ((AccountType)account.Type).GetDescription(),
                //                                ((AccountType)model.Role).GetDescription()));
                //}

                if (!(string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword)))
                {
                    changes.Add(string.Format(AccountResource.msgChangePassword, model.Username));
                }
            }

            return changes.Count() > 0;
        }


        /// <summary>
        /// Checking if there are any changes.
        /// </summary>
        /// <param name="companyAccount">Account that contains existing information</param>
        /// <param name="model">Model that contains new information</param>
        /// <param name="changes">List of changes</param>
        /// <returns></returns>
        internal bool HasChange(CompanyAccount companyAccount, AccountModel model, ref List<string> changes)
        {
            var account = GetById(companyAccount.AccountId.Value);

            if (model.Id != 0)
            {
                if (account.Username.ToLower() != model.Username.ToLower())
                {
                    changes.Add(Helpers.CreateChangedValueContents(AccountResource.lblEmail, account.Username, model.Username));
                }

                if (companyAccount.DynamicRoleId != null && companyAccount.DynamicRoleId != (int)model.Role)
                {
                    var oldDynamicRole = _roleService.GetByIdAndCompanyId(companyAccount.DynamicRoleId.Value, companyAccount.CompanyId.Value);
                    var newDynamicRole = _roleService.GetByIdAndCompanyId(model.Role, companyAccount.CompanyId.Value);

                    changes.Add(string.Format(MessageResource.msgChangeInfo,
                                                AccountResource.lblRole,
                                                oldDynamicRole.Name,
                                                newDynamicRole.Name));
                }

                if (!(string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.ConfirmPassword)))
                {
                    changes.Add(string.Format(AccountResource.msgChangePassword, model.Username));
                }
            }

            return changes.Count() > 0;
        }

        /// <summary>
        /// Check whether account type is valid
        /// </summary>
        /// <param name="accountType"></param>
        /// <returns></returns>
        public bool IsValidAccountType(short accountType)
        {
            if (_httpContext.User.GetAccountType() == (short)AccountType.SystemAdmin)
            {
                var accountTypeIds = EnumHelper.ToEnumList<AccountType>().Select(m => m.Id).ToList();

                return accountTypeIds.Contains((int)accountType);
            }
            else
            {
                var dynamicRoles = _roleService.GetRoleList().Select(m => m.Id).ToList();

                return dynamicRoles.Contains((int)accountType);
            }
        }

        /// <summary>
        /// Get by email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Account GetAccountByEmail(string email)
        {
            try
            {
                return _unitOfWork.AppDbContext.Account.Include(m => m.Company)
                    .FirstOrDefault(m => m.Username.ToLower() == email.ToLower() && !m.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccountByEmail");
                return null;
            }
        }

        /// <summary>
        /// Get companyAccount by email and CompanyId
        /// </summary>
        /// <param name="email"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public CompanyAccount GetCompanyAccountByEmail(string email, int companyId)
        {
            try
            {
                return _unitOfWork.AppDbContext.CompanyAccount.Include(m => m.Company).Include(m => m.Account).Include(m => m.DynamicRole)
                    .FirstOrDefault(m => m.Account.Username.ToLower() == email.ToLower() && m.CompanyId == companyId && !m.Account.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompanyAccountByEmail");
                return null;
            }
        }

        /// <summary>
        /// Save system log about login
        /// </summary>
        /// <param name="account">logged in account</param>
        /// <param name="companyId">company of account</param>
        public void SaveSystemLogLogIn(Account account, int companyId)
        {
            string contentDetails = "";
            // var companyId = account.CompanyId;
            var accountId = account.Id;
            foreach (var header in ColumnDefines.HeaderDeviceOs)
            {
                if (_httpContext.Request.Headers.TryGetValue(header, out var text))
                {
                    contentDetails += header + " : " + text + "\n";
                }
            }

            string ipAddress = _httpContext.GetIpAddressRequest();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                contentDetails += Constants.Settings.HeaderIpv4 + " : " + ipAddress + "\n";
            }

            if (contentDetails.Length > 0)
            {
                contentDetails = contentDetails.Substring(0, contentDetails.Length - 1);
            }
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //Save system log
                        if (companyId != 0)
                        {
                            var content = $"{AccountResource.lblLogin} : {account.Username}";
                            
                            _unitOfWork.SystemLogRepository.Add(account.Id, SystemLogType.AccountManagement, ActionLogType.Login,
                            content, contentDetails, null, companyId, account.Id);
                        }
                        
                        _unitOfWork.AccountRepository.SaveCurrentLogin(accountId, new CurrentLoginInfoModel()
                        {
                            IpAddress = ipAddress,
                            ActiveTime = DateTime.Now,
                        });
                        
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

            new Thread(() =>
            {
                var unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
                {
                    using (var transaction = unitOfWork.AppDbContext.Database.BeginTransaction())
                    {
                        var systemLogs = unitOfWork.SystemLogRepository.GetMany(s =>
                            s.CompanyId == companyId && s.CreatedBy == accountId &&
                            s.ContentDetails != null &&
                            s.Type == (short)SystemLogType.AccountManagement && s.Action == (short)ActionLogType.Login).OrderByDescending(s => s.OpeTime);

                        var systemLogsDevice = unitOfWork.SystemLogRepository.GetMany(s =>
                            s.CompanyId == companyId && s.CreatedBy == accountId &&
                            s.ContentDetails != null && s.ContentDetails.Contains(contentDetails) &&
                            s.Type == (short)SystemLogType.AccountManagement && s.Action == (short)ActionLogType.Login).OrderByDescending(s => s.OpeTime);

                        if (systemLogs.Any())
                        {
                            int count = 0, i = 0;
                            var systemlogUpdate = new SystemLog();
                            foreach (var systemLog in systemLogs)
                            {
                                if (i++ == 0)
                                {
                                    systemlogUpdate = systemLog;
                                    systemlogUpdate.ContentDetails = contentDetails;
                                    continue;
                                }

                                if (string.IsNullOrEmpty(systemLog.ContentDetails)) continue;
                                int indexTotal = systemLog.ContentDetails.IndexOf("total");

                                if (indexTotal >= 0)
                                {
                                    var text = systemLog.ContentDetails.Substring(indexTotal, systemLog.ContentDetails.Length - indexTotal);
                                    text = text.Replace("total : ", "").Trim();
                                    if (systemLog.ContentDetails.IndexOf(contentDetails) >= 0 || systemLogsDevice.Count() > 1)
                                    {
                                        count = int.Parse(text.Trim());
                                    }
                                    else
                                    {
                                        count = 1 + int.Parse(text.Trim());
                                    }
                                }
                                else
                                {
                                    if (systemLog.ContentDetails.IndexOf(contentDetails) >= 0)
                                    {
                                        count = 1;
                                    }
                                    else
                                    {
                                        count = 2;
                                    }
                                }

                                systemlogUpdate.ContentDetails = contentDetails + "\n" + $"total : {count}";
                                unitOfWork.AppDbContext.SystemLog.Update(systemlogUpdate);
                                unitOfWork.Save();
                                transaction.Commit();
                                break;

                            }
                        }
                    }
                });

                unitOfWork.Dispose();

            }).Start();
        }

        public bool HasDefaultAccount(/*int companyId*/)
        {
            var result = true;
            //_unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            //{
            //    using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
            //    {
            //        try
            //        {
            //            var accountCount = _unitOfWork.AppDbContext.Account
            //                                .Include(m => m.Company)
            //                                .Where(m => !m.IsDeleted && m.RootFlag)
            //                                .Count();

            //            if (accountCount == 0)
            //            {
            //                _unitOfWork.AccountRepository.AddDefaultAccount(
            //                    _configuration[Constants.Settings.DefaultAccountUsername],
            //                    _configuration[Constants.Settings.DefaultAccountUsername]);
            //                _unitOfWork.Save();
            //                result = false;

            //                _unitOfWork.Save();
            //            }
            //            transaction.Commit();
            //        }
            //        catch (Exception)
            //        {
            //            transaction.Rollback();
            //            throw;
            //        }
            //    }
            //});

            try
            {
                var accountCount = _unitOfWork.AppDbContext.Account
                                    .Include(m => m.Company)
                                    .Where(m => !m.IsDeleted && m.RootFlag)
                                    .Count();

                if (accountCount == 0)
                {
                    _unitOfWork.AccountRepository.AddDefaultAccount(
                        _configuration[Constants.Settings.DefaultAccountUsername],
                        _configuration[Constants.Settings.DefaultAccountUsername]);
                    _unitOfWork.Save();
                    result = false;

                    _unitOfWork.Save();
                }
                //transaction.Commit();
            }
            catch (Exception)
            {
                //transaction.Rollback();
                throw;
            }

            return result;
        }

        public Dictionary<string, List<string>> GetTimeZone()
        {
            try
            {
                var dictionary = new Dictionary<string, List<string>>();
                foreach (TimeZoneInfo z in TimeZoneInfo.GetSystemTimeZones())
                {
                    List<string> existing;
                    var key = "UTC" + " " + z.BaseUtcOffset.ToString().Replace(":00", "");
                    if (!dictionary.TryGetValue(key.ToUpper(), out existing))
                    {
                        existing = new List<string>();
                        dictionary[key] = existing;
                        existing.Add(z.Id);
                    }
                    else
                    {
                        existing.Add(z.Id);
                    }
                }
                return dictionary;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetTimeZone");
                return new Dictionary<string, List<string>>();
            }
        }

        public Dictionary<string, string> GetUserRabbitCredential(CompanyAccount companyAccount, Company company)
        {
            var host = _configuration.GetSection("QueueConnectionSettingsWebApp:Host");
            var virtualHost = _configuration.GetSection("QueueConnectionSettingsWebApp:VirtualHost");
            var port = _configuration.GetSection("QueueConnectionSettingsWebApp:Port");
            var username = _configuration.GetSection("QueueConnectionSettingsWebApp:UserName");
            var password = _configuration.GetSection("QueueConnectionSettingsWebApp:Password");

            Dictionary<string, string> queueService = new Dictionary<string, string>();
            queueService.Add(host.Key, host.Value);
            queueService.Add(port.Key, port.Value);
            queueService.Add(virtualHost.Key, virtualHost.Value);
            if (companyAccount != null && (short)companyAccount.DynamicRole.TypeId != (short)AccountType.SystemAdmin)
            {
                queueService.Add(username.Key, company.Code);
                queueService.Add(password.Key, Convert.ToBase64String(Encoding.UTF8.GetBytes(company.Code)));
            }
            else
            {
                queueService.Add(username.Key, username.Value);
                queueService.Add(password.Key, password.Value);
            }

            return queueService;
        }

        public bool IsSystemAdmin(int accountId)
        {
            if (accountId == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Dictionary<string, string> GetInfomationSystem()
        {
            var api = _configuration.GetSection("Version:Api");
            var webApp = _configuration.GetSection("Version:WebApp");
            var rabbitMQ = _configuration.GetSection("Version:RabbitMQ");
            var postgresql = _configuration.GetSection("Version:Postgresql");
            var supportdevice1 = _configuration.GetSection("Version:ICU300N");
            var supportdevice2 = _configuration.GetSection("Version:DE950");
            var supportdevice3 = _configuration.GetSection("Version:DE960");
            var supportdevice4 = _configuration.GetSection("Version:DQMini");
            var supportdevice5 = _configuration.GetSection("Version:ITouchPop2A");

            Dictionary<string, string> version = new Dictionary<string, string>();
            version.Add(api.Key, api.Value);
            version.Add(webApp.Key, webApp.Value);
            version.Add(rabbitMQ.Key, rabbitMQ.Value);
            version.Add(postgresql.Key, postgresql.Value);
            version.Add(supportdevice1.Key, supportdevice1.Value);
            version.Add(supportdevice2.Key, supportdevice2.Value);
            version.Add(supportdevice3.Key, supportdevice3.Value);
            version.Add(supportdevice4.Key, supportdevice4.Value);
            version.Add(supportdevice5.Key, supportdevice5.Value);

            // get license information
            var licenseInfo = _unitOfWork.SystemInfoRepository.GetDataLicenseInfo();
            if (licenseInfo != null)
            {
                try
                {
                    version.Add("LicenseNumber", licenseInfo.LicenseNumber);
                    version.Add("CustomerName", licenseInfo.CustomerName);
                    version.Add("CountOfDevices", licenseInfo.CountOfDevices.ToString());
                    string listPlugin = "";
                    licenseInfo.ListOfPlugIn.ForEach(m => listPlugin += m.Name + ", ");
                    if (listPlugin.Length > 2) listPlugin = listPlugin.Substring(0, listPlugin.Length - 2);
                    version.Add("ListOfPlugIn", listPlugin);
                    version.Add("EffectiveDate", licenseInfo.EffectiveDate.ToString(Constants.DateTimeFormat.YyyyMmDd));
                    version.Add("ExpiredDate", licenseInfo.ExpiredDate.ToString(Constants.DateTimeFormat.YyyyMmDd));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            return version;
        }

        public IQueryable<CompanyAccount> GetPaginatedPrimaryAccount(string filter, int pageNumber, int pageSize, int sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();

            //IQueryable<Account> data = _unitOfWork.AppDbContext.Account
            //    .Where(m => !m.IsDeleted && m.Type == (short)AccountType.PrimaryManager);

            IQueryable<CompanyAccount> data = _unitOfWork.AppDbContext.CompanyAccount.Include(m => m.DynamicRole).Include(m => m.Account).Include(m => m.Company)
                .Where(m => m.DynamicRole.TypeId == (int)AccountType.PrimaryManager && !m.Account.IsDeleted);

            if (companyId != 0)
            {
                data = data.Where(m => m.CompanyId == companyId && !m.Company.IsDeleted);
            }

            //data = null;

            foreach (var eachData in data)
            {
                var company = _companyService.GetById(eachData.CompanyId ?? 0);

                if (company != null)
                {
                    eachData.Company = company;
                }
                else
                {
                    eachData.Company = new Company();
                }

                var user = _unitOfWork.UserRepository.GetUserByEmail(companyId, eachData.Account.Username);

                if (user != null)
                {
                    eachData.Account.Username = $"{user.FirstName} {user.LastName}({eachData.Account.Username})";
                }
            }

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                var normalizedFilter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x => x.Account.Username.RemoveDiacritics().ToLower().Contains(normalizedFilter)).AsQueryable();
            }

            recordsFiltered = data.Count();
            sortColumn = sortColumn > ColumnDefines.CompanyAccount.Length - 1 ? 0 : sortColumn;
            data = data.OrderBy($"{ColumnDefines.CompanyAccount[sortColumn]} {sortDirection}");
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            return data;
        }

        /// <summary>
        /// Gets a list of Account that has approval permission about visitor.
        /// </summary>
        /// <returns></returns>
        public List<AccountListModel> GetApprovalAccount()
        {
            int companyId = _httpContext.User.GetCompanyId();
            List<int> ignoredTypes = new List<int>()
            {
                (short)AccountType.Employee,
                (short)AccountType.SecondaryManager,
            };

            var roles = _unitOfWork.RoleRepository.GetByCompanyId(companyId)
                .Where(m => !ignoredTypes.Contains(m.TypeId))
                .Select(m => new RoleDataModel()
                {
                    Id = m.Id,
                    PermissionGroups = JsonConvert.DeserializeObject<List<PermissionGroupDataModel>>(m.PermissionList),
                    RoleName = m.Name
                });

            List<int> approvalRoles = new List<int>();
            foreach (var item in roles)
            {
                var itemRole = item.PermissionGroups.SelectMany(x => x.Permissions).FirstOrDefault(x => x.Title == ActionName.Approve + Page.VisitManagement);
                if (itemRole != null && itemRole.IsEnabled)
                {
                    approvalRoles.Add(item.Id);
                }

                itemRole = item.PermissionGroups.SelectMany(x => x.Permissions).FirstOrDefault(x => x.Title == ActionName.Approve + Page.User);
                if (itemRole != null && itemRole.IsEnabled)
                {
                    approvalRoles.Add(item.Id);
                }
            }

            List<AccountListModel> data = _unitOfWork.AppDbContext.CompanyAccount.Include(m => m.Account).Include(m => m.DynamicRole)
                                       .Where(m => !m.Account.IsDeleted && m.CompanyId == companyId && m.DynamicRoleId != null && approvalRoles.Contains(m.DynamicRoleId.Value))
                                       .Select(m => new AccountListModel()
                                       {
                                           Id = m.AccountId.Value,
                                           Email = m.Account.Username,
                                           TimeZone = m.Account.TimeZone
                                       }).ToList();

            foreach (var account in data)
            {
                var userName = GetUserNameByEmail(companyId, account.Email);
                account.Email = account.Email.Equals(userName) ? account.Email : $"{userName} ({account.Email})";
                account.AccountId = account.Id;

                var user = _unitOfWork.UserRepository.GetUserByAccountId(account.Id, companyId);
                if (user != null)
                {
                    var department = _unitOfWork.DepartmentRepository.GetById(user.DepartmentId);

                    account.FirstName = user.FirstName;
                    account.Position = user.Position;
                    account.Department = department.DepartName;
                    account.DepartmentName = department.DepartName;
                }
            }

            return data;
        }

        /// <summary>
        /// Gets a list of Account that has approval permission.
        /// </summary>
        /// <param name="search"> search filter </param>
        /// <param name="type"> If the type value is bigger than 0, pre-registered approval accounts are excluded from list. </param>
        /// <example>
        /// type 1 : excluded 1st approval
        /// type 2 : excluded 2nd approval
        /// type 3 : excluded visit check manager (only visitor)
        /// </example>
        /// <param name="isVisit"> A flag to distinguish visit or user </param>
        /// <param name="sortColumn"> sort column </param>
        /// <param name="sortDirection"> sort direction </param>
        /// <param name="pageNumber"> page number </param>
        /// <param name="pageSize"> page size </param>
        /// <param name="filteredRecord"></param>
        /// <param name="totalRecord"></param>
        /// <returns></returns>
        public List<AccountListModel> GetApprovalAccount(string search, int type, bool isVisit, string sortColumn, string sortDirection, int pageNumber, int pageSize, out int filteredRecord, out int totalRecord)
        {
            var companyId = _httpContext.User.GetCompanyId();

            var roles = _unitOfWork.AppDbContext.DynamicRole
                            .Where(m => m.CompanyId == companyId && !m.IsDeleted && m.TypeId != (short)AccountType.Employee && m.TypeId != (short)AccountType.SecondaryManager);

            var roleData = roles.Select(m => new RoleDataModel()
            {
                Id = m.Id,
                PermissionGroups = JsonConvert.DeserializeObject<List<PermissionGroupDataModel>>(m.PermissionList),
                RoleName = m.Name
            });

            List<int> approvalRoles = new List<int>();
            foreach (var item in roleData)
            {
                var permittionTitle = isVisit ? ActionName.Approve + Page.VisitManagement : ActionName.Approve + Page.User;

                var itemRole = item.PermissionGroups.SelectMany(x => x.Permissions).FirstOrDefault(x => x.Title == permittionTitle);
                if (itemRole != null && itemRole.IsEnabled)
                {
                    approvalRoles.Add(item.Id);
                }

                //var itemRole = item.PermissionGroups.SelectMany(x => x.Permissions).FirstOrDefault(x => x.Title == ActionName.Approve + Page.VisitManagement);
                //if (itemRole != null && itemRole.IsEnabled)
                //{
                //    approvalRoles.Add(item.Id);
                //}

                //itemRole = item.PermissionGroups.SelectMany(x => x.Permissions).FirstOrDefault(x => x.Title == ActionName.Approve + Page.User);
                //if (itemRole != null && itemRole.IsEnabled)
                //{
                //    approvalRoles.Add(item.Id);
                //}
            }

            List<AccountListModel> data = _unitOfWork.AppDbContext.CompanyAccount
                                        .Include(m => m.Account.User).ThenInclude(user => user.Department)
                                        .Include(m => m.DynamicRole)
                                        .Where(m => !m.Account.IsDeleted
                                                && m.CompanyId == companyId
                                                && m.DynamicRoleId != null
                                                && approvalRoles.Contains(m.DynamicRoleId.Value)
                                               )
                                        .Select(m => new AccountListModel()
                                        {
                                            Id = m.AccountId.Value,
                                            AccountId = m.AccountId.Value,
                                            //Email = m.Account.User != null && m.Account.User.Any(u => !u.IsDeleted && u.CompanyId == companyId) 
                                            //        ? $"{m.Account.User.FirstOrDefault(u => !u.IsDeleted && u.CompanyId == companyId).FirstName} {m.Account.User.FirstOrDefault(u => !u.IsDeleted && u.CompanyId == companyId).LastName}".Trim()
                                            //        : m.Account.Username,
                                            Email = m.Account.Username,
                                            UserName = m.Account.User != null && m.Account.User.Any(u => !u.IsDeleted && u.CompanyId == companyId)
                                                    ? $"{m.Account.User.FirstOrDefault(u => !u.IsDeleted && u.CompanyId == companyId).FirstName} {m.Account.User.FirstOrDefault(u => !u.IsDeleted && u.CompanyId == companyId).LastName}".Trim()
                                                    : "",
                                            TimeZone = m.Account.TimeZone,
                                            FirstName = m.Account.User != null && m.Account.User.Any(u => !u.IsDeleted && u.CompanyId == companyId)
                                                    ? m.Account.User.FirstOrDefault(u => !u.IsDeleted && u.CompanyId == companyId).FirstName
                                                    : "",
                                            Position = m.Account.User != null && m.Account.User.Any(u => !u.IsDeleted && u.CompanyId == companyId)
                                                    ? m.Account.User.FirstOrDefault(u => !u.IsDeleted && u.CompanyId == companyId).Position
                                                    : "",
                                            Department = m.Account.User != null && m.Account.User.Any(u => !u.IsDeleted && u.CompanyId == companyId) && m.Account.User.FirstOrDefault(u => !u.IsDeleted && u.CompanyId == companyId).Department != null
                                                    ? m.Account.User.FirstOrDefault(u => !u.IsDeleted && u.CompanyId == companyId).Department.DepartName
                                                    : "",
                                            DepartmentName = m.Account.User != null && m.Account.User.Any(u => !u.IsDeleted && u.CompanyId == companyId) && m.Account.User.FirstOrDefault(u => !u.IsDeleted && u.CompanyId == companyId).Department != null
                                                    ? m.Account.User.FirstOrDefault(u => !u.IsDeleted && u.CompanyId == companyId).Department.DepartName
                                                    : "",
                                        }).ToList();

            totalRecord = data.Count;

            if (type > 0)
            {
                // If type value is bigger than 0, exclude pre-registered data.
                if (isVisit)
                {
                    // Exclude visitor approval data.
                    var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(companyId);
                    string approvalAccounts = string.Empty;
                    switch (type)
                    {
                        case 1:
                            approvalAccounts = visitSetting.FirstApproverAccounts;
                            break;
                        case 2:
                            approvalAccounts = visitSetting.SecondsApproverAccounts;
                            break;
                        case 3:
                        default:
                            approvalAccounts = visitSetting.VisitCheckManagerAccounts;
                            break;
                    }

                    if (!string.IsNullOrWhiteSpace(approvalAccounts))
                    {
                        // parse string data to list of int
                        List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(approvalAccounts);
                        // Exclude.
                        data = data.Where(d => !accountIds.Contains(d.AccountId)).ToList();
                    }
                }
                else
                {
                    // Exclude user approval data.
                    var accessSetting = _unitOfWork.UserRepository.GetAccessSetting(companyId);
                    string approvalAccounts = string.Empty;
                    switch (type)
                    {
                        case 1:
                            approvalAccounts = accessSetting.FirstApproverAccounts;
                            break;
                        case 2:
                        default:
                            approvalAccounts = accessSetting.SecondApproverAccounts;
                            break;
                    }

                    if (!string.IsNullOrWhiteSpace(approvalAccounts))
                    {
                        // parse string data to list of int
                        List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(approvalAccounts);
                        // Exclude.
                        data = data.Where(d => !accountIds.Contains(d.AccountId)).ToList();
                    }
                }
            }

            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = search.Trim().RemoveDiacritics().ToLower();

                data = data.Where(d => d.Email.RemoveDiacritics().ToLower().Contains(normalizedSearch)
                                     || d.DepartmentName.RemoveDiacritics().ToLower().Contains(normalizedSearch)
                                     || d.Department.RemoveDiacritics().ToLower().Contains(normalizedSearch)
                                     || d.FirstName.RemoveDiacritics().ToLower().Contains(normalizedSearch)).ToList();
            }

            filteredRecord = data.Count;

            IQueryable<AccountListModel> resultList = Helpers.SortData<AccountListModel>(data.AsEnumerable<AccountListModel>(), sortDirection, sortColumn);

            if (pageSize > 0)
                resultList = resultList.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            data = resultList.ToList();

            return data;
        }

        /// <summary>
        /// Gets a list of Account that has approval permission about User.
        /// </summary>
        /// <returns></returns>
        public List<AccountListModel> GetAccessApprovalAccount()
        {
            var companyId = _httpContext.User.GetCompanyId();

            var roles = _unitOfWork.AppDbContext.DynamicRole
                            .Where(m => m.CompanyId == companyId && !m.IsDeleted && m.TypeId != (short)AccountType.Employee && m.TypeId != (short)AccountType.SecondaryManager);

            var roleData = roles.Select(m => new RoleDataModel()
            {
                Id = m.Id,
                PermissionGroups = JsonConvert.DeserializeObject<List<PermissionGroupDataModel>>(m.PermissionList),
                RoleName = m.Name
            });

            List<int> approvalRoles = new List<int>();
            foreach (var item in roleData)
            {
                var itemRole = item.PermissionGroups.SelectMany(x => x.Permissions).FirstOrDefault(x => x.Title == ActionName.Approve + Page.User);
                if (itemRole != null && itemRole.IsEnabled)
                {
                    approvalRoles.Add(item.Id);
                }
            }

            List<AccountListModel> data = _unitOfWork.AppDbContext.CompanyAccount.Include(m => m.Account).Include(m => m.DynamicRole)
                                       .Where(m => !m.Account.IsDeleted && m.CompanyId == companyId && m.DynamicRoleId != null && approvalRoles.Contains(m.DynamicRoleId.Value))
                                       .Select(m => new AccountListModel()
                                       {
                                           Id = m.AccountId.Value,
                                           Email = m.Account.Username,
                                           TimeZone = m.Account.TimeZone
                                           //TimeZone = m.TimeZone
                                       }).ToList();

            foreach (var account in data)
            {
                var userName = GetUserNameByEmail(companyId, account.Email);

                account.Email = account.Email.Equals(userName) ? account.Email : $"{userName} ({account.Email})";

                account.AccountId = account.Id;

                var user = _unitOfWork.UserRepository.GetUserByAccountId(account.Id, _httpContext.User.GetCompanyId());

                if (user != null)
                {
                    var department = _unitOfWork.DepartmentRepository.GetById(user.DepartmentId);

                    account.FirstName = user.FirstName;
                    account.Position = user.Position;

                    account.Department = department.DepartName;
                    account.DepartmentName = department.DepartName;
                }
            }

            return data;
        }

        public IEnumerable<EnumModel> GetAccountType()
        {
            var currentAccountType = _httpContext.User.GetAccountType();

            if (currentAccountType == (short)AccountType.SystemAdmin)
            {
                var accountTypes = GetAccountRoles();
                List<int> accountTypeAllow = new List<int>()
                {
                    (int) AccountType.SystemAdmin,
                    (int) AccountType.PrimaryManager,
                };
                return accountTypes.Where(m => accountTypeAllow.Contains(m.Id));
            }
            else
            {
                var roleList = _roleService.GetRoleList();
                return roleList.AsEnumerable();

            }
        }

        public bool IsExistCompanyAccount(int id, int companyId)
        {
            return _unitOfWork.AppDbContext.CompanyAccount.Include(m => m.Account).FirstOrDefault(m => m.Id == id && m.CompanyId == companyId && !m.Account.IsDeleted) != null;
        }

        public bool IsExistCompanyAccountbyAccountId(int accountId, int companyId)
        {
            return _unitOfWork.AppDbContext.CompanyAccount.Include(m => m.Account).FirstOrDefault(m => m.AccountId == accountId && m.CompanyId == companyId && !m.Account.IsDeleted) != null;
        }

        public void AddTokenAndRefreshTokenToDB(string refreshToken, Account model, int expiryRefreshToken)
        {

            try
            {
                _unitOfWork.AccountRepository.AddTokenAndRefreshToken(refreshToken, model, expiryRefreshToken);

            }
            catch (Exception ex)
            {
                ex.ToString();
            }


        }

        public Account GetAccountByRefreshToken(string refreshToken)
        {
            Account lstAccount = new Account();
            lstAccount = _unitOfWork.AccountRepository.GetAccountByRefreshToken(refreshToken);
            return lstAccount;
        }
        public string GetRefreshTokenByUserName(string userName)
        {
            return _unitOfWork.AccountRepository.GetRefreshTokenByUserName(userName);
        }

        public void AddPrimaryManager(AccountModel model/*(, Account account*/)
        {
            //var lstNoti = _unitOfWork.AppDbContext.Setting.Where(x => x.Key == "list_user_to_notification" && x.CompanyId == account.CompanyId).FirstOrDefault();
            var lstNoti = (from a in _unitOfWork.AppDbContext.Setting where a.Key == "list_user_to_notification" && a.CompanyId == model.CompanyId select a).FirstOrDefault();
            string[] Value = JsonConvert.DeserializeObject<string[]>(lstNoti.Value);
            if (Value.Contains(model.Username))
            {

            }
            else
            {
                int newLength = Value.Length + 1;
                string[] result = new string[newLength];
                for (int i = 0; i < Value.Length; i++)
                {
                    result[i] = Value[i];
                    result[newLength - 1] = model.Username;
                }
                StringBuilder builder = new StringBuilder();
                builder.Append('[');
                foreach (string value in result)
                {
                    builder.Append('"');
                    builder.Append(value);
                    builder.Append('"');
                    builder.Append(',');
                }
                builder.Append(']');

                lstNoti.Value = builder.ToString().TrimEnd(',');
                _unitOfWork.AppDbContext.Setting.Update(lstNoti);
                _unitOfWork.AppDbContext.SaveChanges();
            }
        }

        public void DeletePrimaryManager(AccountModel model, int oldCompanyId)
        {
            var lstNoti = (from a in _unitOfWork.AppDbContext.Setting where a.Key == "list_user_to_notification" && a.CompanyId == oldCompanyId select a).FirstOrDefault();
            string[] Value = JsonConvert.DeserializeObject<string[]>(lstNoti.Value);
            int newLength = Value.Length + 1;
            string[] result = new string[newLength];

            for (int i = 0; i < Value.Length; i++)
            {
                if (Value[i] != model.Username)
                {
                    result[i] = Value[i];
                }

            }
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            foreach (string value in result)
            {
                builder.Append('"');
                builder.Append(value);
                builder.Append('"');
                builder.Append(',');
            }
            builder.Append(']');
            lstNoti.Value = builder.ToString().TrimEnd(',');
            _unitOfWork.AppDbContext.Setting.Update(lstNoti);
            _unitOfWork.AppDbContext.SaveChanges();
        }

        /// <summary>
        /// Get account(s) by role id and company id.
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<CompanyAccount> GetCompanyAccountByRoleId(int roleId, int companyId)
        {
            var data = _unitOfWork.AccountRepository.GetCompanyAccountByRoleIdandCompanyId(roleId, companyId).ToList();

            return data;
        }


        /// <summary>
        /// Get account(s) by list of role id and company id.
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<CompanyAccount> GetCompanyAccountByRoleIds(List<int> roleIds, int companyId)
        {
            var data = _unitOfWork.AccountRepository.GetCompanyAccountByRoleIdsandCompanyId(roleIds, companyId).ToList();

            return data;
        }

        public int GetSystemTotalAccount()
        {
            return _unitOfWork.AccountRepository.GetSystemTotalAccount();
        }

        /// <summary>
        /// Get list of available code to set MealType's code
        /// </summary>
        /// <returns></returns>
        public List<EnumModel> GetPreferredSystem()
        {
            var codeList = EnumHelper.ToEnumList<PreferredSystem>();

            return codeList;
        }

        /// <summary>
        /// Get list of all company that user belong to
        /// </summary>
        /// <returns></returns>

        public List<Company> GetCompanyList(Account account)
        {
            List<int?> companyIds = _unitOfWork.CompanyAccountRepository.GetCompanyIdsByAccount(account.Id);
            var ids = new List<int>();
            foreach (int? companyId in companyIds)
            {
                if (companyId != null)
                    ids.Add((int)companyId);
            }
            var companies = _unitOfWork.CompanyRepository.GetByIds(ids);
            return companies;
        }
        
        /// <summary>
        /// Get list paginated of all company that user belong to
        /// </summary>
        /// <param name="account"></param>
        /// <param name="search"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>

        public List<Company> GetPaginatedCompanyList(Account account, string search, int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered)
        {
            List<int?> companyIds = _unitOfWork.CompanyAccountRepository.GetCompanyIdsByAccount(account.Id);
            var ids = new List<int>();
            foreach (int? companyId in companyIds)
            {
                if (companyId != null)
                    ids.Add((int)companyId);
            }
            
            IEnumerable<Company> companies = _unitOfWork.CompanyRepository.GetByIds(ids).AsQueryable().OrderBy($"{sortColumn} {sortDirection}");

            totalRecords = companies.Count();
            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = search.Trim().RemoveDiacritics().ToLower();
                companies = companies.Where(m => !string.IsNullOrEmpty(m.Name) && m.Name.RemoveDiacritics().ToLower().Contains(normalizedSearch));
            }
            
            recordsFiltered = companies.Count();
            if (pageSize > 0)
            {
                companies = companies.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
            
            return companies.ToList();
        }


        /// <summary>
        /// Return company account that conresponsding to refresh token
        /// </summary>
        /// <returns></returns>

        public CompanyAccount GetCompanyAccountByRefreshToken(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public TokenModel CreateAuthToken(Account account, Company company, bool updateRefreshToken = true)
        {
            try
            {
                var companyAccount = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompanyAndAccount(company.Id, account.Id);
                var user = _unitOfWork.UserRepository.GetUserByAccountId(account.Id, company.Id);
                var claims = new[]
                {
                    new Claim(Constants.ClaimName.sub, account.Username),
                    new Claim(Constants.ClaimName.Username, account.Username),
                    new Claim(Constants.ClaimName.AccountId, account.Id.ToString()),
                    new Claim(Constants.ClaimName.CompanyId, company.Id.ToString()),
                    new Claim(Constants.ClaimName.CompanyCode, company.Code),
                    new Claim(Constants.ClaimName.CompanyName, company.Name),
                    new Claim(Constants.ClaimName.FullName, user?.FirstName ?? ""),
                    new Claim(Constants.ClaimName.AccountType, companyAccount.DynamicRole.TypeId.ToString())
                };
                var token = _jwtHandler.BuilToken(claims);

                SaveSystemLogLogIn(account, company.Id);

                var refreshTokenClaims = new[]
                {
                    new Claim(Constants.ClaimName.CompanyId, company.Id.ToString()),
                    new Claim(Constants.ClaimName.AccountId, account.Id.ToString()),
                };

                string refreshToken = account.RefreshToken;

                // Check if the  refresh token is valid, if not create new one
                bool invalidRefrestoken = false;
                if (refreshToken != null)
                {
                    try
                    {
                        ClaimsPrincipal refreshTokenPrincipal = _jwtHandler.GetPrincipalFromExpiredToken(refreshToken);
                        var accountId = refreshTokenPrincipal.GetAccountId();
                        var companyId = refreshTokenPrincipal.GetCompanyId();
                        if (accountId != account.Id || companyId != company.Id)
                        {
                            invalidRefrestoken = true;
                        }
                    }
                    catch (Exception)
                    {
                        invalidRefrestoken = true;
                    }
                }

                if (updateRefreshToken || _jwtHandler.IsTokenExpired(refreshToken) || invalidRefrestoken || refreshToken == null)
                {
                    refreshToken = _jwtHandler.BuildRefreshToken(refreshTokenClaims);
                    AddTokenAndRefreshTokenToDB(refreshToken, account, _options.expiryRefreshToken);
                }

                //var deptId = user?.DepartmentId ?? 0;
                //var deptName = deptId == 0 ? "" : _departmentService.GetById(deptId).DepartName;
                var deptName = user != null && user.Department != null ? user.Department.DepartName : "";
                var fullName = user == null ? "" : user.FirstName;
                var plugIn = _companyService.GetPluginByCompanyAllowShowing(company.Id);
                var json = JsonConvert.DeserializeObject<Dictionary<string, bool>>(plugIn.PlugIns);

                // Get consolidated password status for warnings (expired passwords already blocked in ValidateAccountForLogin)
                var passwordStatus = GetPasswordStatus(account, company.Id);

                TokenModel result = new TokenModel
                {
                    Status = 1,
                    AuthToken = token,
                    RefreshToken = refreshToken,
                    AccountType = (short)companyAccount.DynamicRole.TypeId,
                    FullName = fullName,
                    CompanyName = company.Name,
                    DepartmentName = deptName,
                    CompanyCode = company.Code,
                    Logo = Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetByKey(Constants.Settings.QRLogo, company.Id).Value),
                    QueueService = GetUserRabbitCredential(companyAccount, company),
                    PlugIn = json,
                    //Permissions = _roleService.GetPermissionsByCompanyAccountId(companyAccount.Id),
                    Permissions = _roleService.GetPermissionsByCompanyAccountId(companyAccount),
                    //Role = _roleService.GetByIdAndCompanyId((int)companyAccount.DynamicRoleId, (int)companyAccount.CompanyId).Name,
                    Role = companyAccount.DynamicRole.Name,
                    EnableDepartmentLevel = companyAccount.DynamicRoleId != null && (_roleService.GetByIdAndCompanyId(companyAccount.DynamicRoleId.Value, company.Id)?.EnableDepartmentLevel ?? false),
                    AccountId = account.Id,
                    UserId = user?.Id ?? 0,
                    ExpireAccessToken = _options.ExpiryMinutes,
                    //UserTimeZone = GetById(account.Id).TimeZone,
                    UserTimeZone = companyAccount.Account.TimeZone,
                    //UserTimeZone = GetCompanyAccountByIdAndCompany(account.Id, company.Id).TimeZone,
                    //UserLanguage = GetById(account.Id).Language,
                    UserLanguage = companyAccount.Account.Language,
                    //UserPreferredSystem = GetCompanyAccountByIdAndCompany(account.Id, company.Id).PreferredSystem,
                    UserPreferredSystem = companyAccount.PreferredSystem,
                    ExpiredDate = user == null ? "" : user.ExpiredDate.ToSettingDateString(),
                    LicenseVerified = true,
                    DefaultPaginationNumber = _unitOfWork.SettingRepository.GetDefaultPaginationNumber(company.Id),
                    // Password change notifications
                    PasswordChangeRequired = passwordStatus.HasWarning,
                    PasswordChangeMessage = passwordStatus.Message,
                };
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAuthToken");
                return null;
            }
        }

        public TokenModel CreateAuthTokenAdmin(Account account, bool updateRefreshToken = true)
        {
            try
            {
                // add current login info
                _unitOfWork.AccountRepository.SaveCurrentLogin(account.Id, new CurrentLoginInfoModel()
                {
                    IpAddress = _httpContext.GetIpAddressRequest(),
                    ActiveTime = DateTime.Now,
                });

                var claims = new[]
                {
                    //new Claim(ClaimTypes.Name, account.Username),
                    new Claim(Constants.ClaimName.sub, account.Username),
                    new Claim(Constants.ClaimName.Username, account.Username),
                    new Claim(Constants.ClaimName.AccountId, account.Id.ToString()),
                    new Claim(Constants.ClaimName.CompanyId, "0" ),
                    new Claim(Constants.ClaimName.CompanyCode, "s000001" ),
                    new Claim(Constants.ClaimName.CompanyName, "SystemAdmin" ),
                    new Claim(Constants.ClaimName.FullName, "SystemAdmin"),
                    new Claim(Constants.ClaimName.AccountType, account.Type.ToString())
                };
                var token = _jwtHandler.BuilToken(claims);
                var refreshTokenClaims = new[]
                {
                    new Claim(Constants.ClaimName.CompanyId, "0"),
                    new Claim(Constants.ClaimName.AccountId, account.Id.ToString()),
                };

                string refreshToken = GetRefreshTokenByUserName(account.Username);

                if (updateRefreshToken || _jwtHandler.IsTokenExpired(refreshToken) || refreshToken == null)
                {
                    refreshToken = _jwtHandler.BuildRefreshToken(refreshTokenClaims);
                    AddTokenAndRefreshTokenToDB(refreshToken, account, _options.expiryRefreshToken);
                }

                AddTokenAndRefreshTokenToDB(refreshToken, account, _options.expiryRefreshToken);
                TokenModel result = new TokenModel
                {
                    Status = 1,
                    AuthToken = token,
                    RefreshToken = refreshToken,
                    AccountType = account.Type,
                    CompanyCode = "0",
                    CompanyName = account.Company == null ? "" : account.Company.Name,
                    DepartmentName = null,
                    FullName = null,
                    QueueService = GetUserRabbitCredential(null, null),
                    ExpireAccessToken = _options.ExpiryMinutes,
                    UserTimeZone = GetById(account.Id).TimeZone,
                    UserLanguage = GetById(account.Id).Language,
                    UserPreferredSystem = GetById(account.Id).PreferredSystem,
                    AccountId = account.Id,
                    LicenseVerified = true,
                };
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateAuthTokenAdmin");
                return null;
            }
        }

        public void AddAccountToCompany(int accountId, int companyId, short roleId)
        {
            try
            {
                // Add Company account object
                if (companyId != 0)
                {
                    CompanyAccount companyAccount = new CompanyAccount
                    {
                        AccountId = accountId,
                        CompanyId = companyId,
                        DynamicRoleId = roleId
                    };

                    _unitOfWork.CompanyAccountRepository.Add(companyAccount);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAccountToCompany");
            }
        }

        public Account GetAccountsById(int accountId)
        {
            try
            {
                return _unitOfWork.AccountRepository.GetById(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccountsById");
                return null;
            }
        }


        public CompanyAccount GetCompanyAccountById(int id)
        {
            try
            {
                return _unitOfWork.CompanyAccountRepository.GetCompanyAccountById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompanyAccountById");
                return null;
            }
        }

        public CompanyAccount GetCompanyAccountByIdAndCompany(int id, int companyId)
        {
            try
            {
                return _unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompanyAndAccount(companyId, id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompanyAccountByIdAndCompany");
                return null;
            }
        }

        public Account GetAccountByPhoneNumber(string phoneNumber)
        {
            try
            {
                return _unitOfWork.AccountRepository.GetByPhoneNumber(phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccountByPhoneNumber");
                return null;
            }
        }

        public void ResetDefaultTimezoneByCompanyId(int companyId)
        {
            try
            {
                var accounts = _unitOfWork.AccountRepository.GetAccountsByCompany(companyId);
                var building = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(companyId);
                if (building != null && !string.IsNullOrEmpty(building.TimeZone))
                {
                    foreach (var account in accounts)
                    {
                        account.TimeZone = building.TimeZone;
                        _unitOfWork.AccountRepository.Update(account);
                    }
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetDefaultTimezoneByCompanyId");
            }
        }

        public LoginInfoModel GetLastLogin(int accountId, int companyId)
        {
            try
            {
                var contentIds = JsonConvert.SerializeObject(new SystemLogIdContent { Id = accountId, AssignedIds = null });
                var systemLogin = _unitOfWork.AppDbContext.SystemLog
                    .Where(m => m.CompanyId == companyId && m.ContentIds == contentIds
                                && m.Type == (short)SystemLogType.AccountManagement && m.Action == (short)ActionLogType.Login)
                    .OrderByDescending(m => m.OpeTime)
                    .Skip(0).Take(10);

                if (systemLogin.Count() > 2)
                {
                    var systemLog = systemLogin.ToList()[1];
                    string ip = "";
                    if (!string.IsNullOrEmpty(systemLog.ContentDetails) && systemLog.ContentDetails.Contains(Constants.Settings.HeaderIpv4))
                    {
                        var headers = systemLog.ContentDetails.Split("\n");
                        foreach (var header in headers)
                        {
                            if (header.Contains(Constants.Settings.HeaderIpv4))
                            {
                                ip = header.Substring((Constants.Settings.HeaderIpv4 + " : ").Length);
                                break;
                            }
                        }
                    }
                    return new LoginInfoModel()
                    {
                        Time = systemLog.OpeTime,
                        IpAddress = ip
                    };
                }

                return new LoginInfoModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetLastLogin");
                return new LoginInfoModel();
            }
        }

        public List<AccountListModel> GetPaginatedAccountListByIds(string search, List<int> ids, int companyId, string pageName, int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int recordsFiltered, out int recordsTotal, out List<HeaderData> userHeader)
        {
            var accountId = _httpContext.User.GetAccountId();

            var accounts = _unitOfWork.AppDbContext.Account
                .Include(m => m.User).ThenInclude(m => m.Department)
                .Include(account => account.CompanyAccount)
                .Where(m => ids.Contains(m.Id) && !m.IsDeleted).ToList();

            recordsTotal = accounts.Count;

            var data = accounts.Select(m => new AccountListModel()
            {
                Id = m.CompanyAccount.FirstOrDefault(n => n.CompanyId == companyId)?.Id ?? 0,
                AccountId = m.Id,
                UserName = m.User.FirstOrDefault(n => n.CompanyId == companyId)?.FirstName,
                FirstName = m.User.FirstOrDefault(n => n.CompanyId == companyId)?.FirstName,
                Email = m.Username,
                Department = m.User.FirstOrDefault(n => n.CompanyId == companyId)?.Department?.DepartName,
                DepartmentName = m.User.FirstOrDefault(n => n.CompanyId == companyId)?.Department?.DepartName,
                Position = m.User.FirstOrDefault(n => n.CompanyId == companyId)?.Position
            });

            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = search.Trim().RemoveDiacritics().ToLower();
                data = data.Where(m =>
                    (!string.IsNullOrEmpty(m.UserName) && m.UserName.RemoveDiacritics().ToLower().Contains(normalizedSearch))
                    || (!string.IsNullOrEmpty(m.FirstName) && m.FirstName.RemoveDiacritics().ToLower().Contains(normalizedSearch))
                    || (!string.IsNullOrEmpty(m.Email) && m.Email.RemoveDiacritics().ToLower().Contains(normalizedSearch)));
            }

            userHeader = _settingService.GetUserHeaderData(Page.VisitSetting + Page.User, ColumnDefines.AccountHeader, companyId, accountId);

            recordsFiltered = data.Count();

            data = data.AsQueryable().OrderBy($"{sortColumn} {sortDirection}");
            if(pageSize > 0)
                data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return data.ToList();
        }

        public TokenModel GetTokenWithAccountAndHashKey(AccountGetTokenModel model, int companyId)
        {
            // verify key
            bool isVerify = true;

            // get token
            if (isVerify)
            {
                var account = _unitOfWork.AppDbContext.Account.Include(m => m.CompanyAccount)
                    .FirstOrDefault(m => m.Username.ToLower() == model.Username.ToLower() && m.CompanyAccount.Any(n => n.CompanyId == companyId));
                if (account == null)
                {
                    return null;
                }

                var company = _unitOfWork.CompanyRepository.GetById(companyId);
                return CreateAuthToken(account, company, false);
            }

            return null;
        }

        public Dictionary<string, List<AccountListModel>> GetApprovalAccessSetting(int companyId)
        {
            var accessSetting = _unitOfWork.UserRepository.GetAccessSetting(companyId);
            List<AccountListModel> firstApprovalAccounts = new List<AccountListModel>();
            List<AccountListModel> secondApprovalAccounts = new List<AccountListModel>();

            if (!string.IsNullOrEmpty(accessSetting.FirstApproverAccounts))
            {
                List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(accessSetting.FirstApproverAccounts);
                firstApprovalAccounts = GetPaginatedAccountListByIds(null, accountIds, companyId, Page.AccessSetting + Page.User, 1, accountIds.Count,
                    "UserName", "asc", out var _, out var _, out List<HeaderData> _);
            }

            if (!string.IsNullOrEmpty(accessSetting.SecondApproverAccounts))
            {
                List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(accessSetting.SecondApproverAccounts);
                secondApprovalAccounts = GetPaginatedAccountListByIds(null, accountIds, companyId, Page.AccessSetting + Page.User, 1, accountIds.Count,
                    "UserName", "asc", out var _, out var _, out List<HeaderData> _);
            }

            return new Dictionary<string, List<AccountListModel>>()
            {
                { "firstApprovalAccounts", firstApprovalAccounts },
                { "secondApprovalAccounts", secondApprovalAccounts },
            };
        }

        public bool CheckAccountActivated(Account account, int companyId)
        {
            List<int> statusActivated = new List<int>()
            {
                (short)ApprovalStatus.NotUse,
                (short)ApprovalStatus.Approved,
            };
            var user = _unitOfWork.UserRepository.GetUserByAccountId(account.Id, companyId);
            return !account.IsDeleted && user == null || statusActivated.Contains(user.ApprovalStatus);
        }

        public Dictionary<string, int> GetAccountCountReview(int accountId, int companyId)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            int visit = 0, user = 0;

            
            visit = _unitOfWork.AppDbContext.Visit.Count(v => v.CompanyId == companyId
                && ((v.Status == (short)VisitChangeStatusType.Waiting && v.ApproverId1 == accountId)
                    || (v.Status == (short)VisitChangeStatusType.Approved1 && v.ApproverId2 == accountId))
                && !v.IsDeleted);

            user = _unitOfWork.AppDbContext.User.Count(v => v.CompanyId == companyId && !v.IsDeleted
                                                            && ((v.ApprovalStatus == (short)ApprovalStatus.ApprovalWaiting1 && v.ApproverId1 == accountId)
                                                                || (v.ApprovalStatus == (short)ApprovalStatus.ApprovalWaiting2 && v.ApproverId2 == accountId)
                                                                || (v.ApprovalStatus == (short)ApprovalStatus.UpdateWaiting1 && v.ApproverId1 == accountId)
                                                                || (v.ApprovalStatus == (short)ApprovalStatus.UpdateWaiting2 && v.ApproverId2 == accountId))
                                                            );
            

            result.Add("visit", visit);
            result.Add("user", user);
            return result;
        }

        public void SendCountReviewToFe(int accountId, int companyId)
        {
            try
            {
                var account = _unitOfWork.AccountRepository.GetById(accountId);
                var company = _unitOfWork.CompanyRepository.GetById(companyId);
                if (account != null && company != null)
                {
                    var data = GetAccountCountReview(accountId, companyId);
                    _notificationService.SendMessage(
                        Constants.MessageType.Success,
                        Constants.NotificationType.NotificationCountReview,
                        account.Username,
                        JsonConvert.SerializeObject(data),
                        company.Id
                    );
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message + e.StackTrace);
            }
        }

        public void Logout(int accountId)
        {
            try
            {
                // clear session
                _unitOfWork.AccountRepository.SaveCurrentLogin(accountId, null);

                // save system-log
                int companyId = _httpContext.User.GetCompanyId();
                if (companyId != 0)
                {
                    var content = $"{AccountResource.lblLogout} : {_httpContext.User.GetUsername()}";

                    _unitOfWork.SystemLogRepository.Add(accountId, SystemLogType.AccountManagement, ActionLogType.Logout,
                        content, Constants.Settings.HeaderIpv4 + " : " + _httpContext.GetIpAddressRequest(), null, companyId, accountId);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Logout");
            }
        }

        public bool CheckSessionInvalidLogin(Account account)
        {
            var loginSessionConfig = _configuration.GetSection("LoginSessionConfig").Get<LoginSessionConfig>();
            if (loginSessionConfig != null && loginSessionConfig.EnableSingleIpAddress)
            {
                try
                {
                    if (!account.RootFlag && !string.IsNullOrEmpty(account.CurrentLoginInfo))
                    {
                        var currentLoginInfo = JsonConvert.DeserializeObject<CurrentLoginInfoModel>(account.CurrentLoginInfo);
                        if (currentLoginInfo.IpAddress != _httpContext.GetIpAddressRequest() &&
                            (DateTime.Now - currentLoginInfo.ActiveTime).TotalMinutes <= loginSessionConfig.SessionExpiredTime)
                        {
                            return false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message + ex.StackTrace);
                }
            }

            return true;
        }
        
        public void SetRootAccount(Account account, bool isRoot)
        {
            try
            {
                account.RootFlag = isRoot;
                _unitOfWork.AccountRepository.Update(account);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SetRootAccount");
            }
        }

        private void UpdateLoginConfigByAccountId(int accountId, LoginConfigModel loginConfigModel)
        {
            var account = _unitOfWork.AccountRepository.GetById(accountId);
            if (account != null)
            {
                account.LoginConfig = JsonConvert.SerializeObject(loginConfigModel);
                
                _unitOfWork.AccountRepository.Update(account);
                _unitOfWork.Save();
            }
        }
        public void SaveCurrentLogin(int accountId)
        {
            try
            {
                _unitOfWork.AccountRepository.SaveCurrentLogin(accountId, new CurrentLoginInfoModel()
                {
                    IpAddress = _httpContext.GetIpAddressRequest(),
                    ActiveTime = DateTime.UtcNow,
                });

                var account = _unitOfWork.AccountRepository.GetById(accountId);
                if (account != null)
                {
                    var loginConfig = ParseLoginConfig(account.LoginConfig);
                    loginConfig.IsFirstLogin = loginConfig.IsFirstLogin;
                    loginConfig.LoginFailCount = 0;
                    loginConfig.LastTimeLoginFail = DateTime.MinValue;
                    account.LoginConfig = JsonConvert.SerializeObject(loginConfig);
                    _unitOfWork.AccountRepository.Update(account);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveCurrentLogin");
            }
        }
        public string HashJsonToQrCode(Account account, int companyId)
        {
            try
            {
                HashQRCode hashQrCode = new HashQRCode();
                var company = _unitOfWork.CompanyRepository.GetById(companyId);
                if (company != null)
                {
                    // Get setting dynamic qr code
                    var setting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.AllowGenerateQrCodeOffline, companyId);
                    bool.TryParse(Helpers.GetStringFromValueSetting(setting.Value), out bool allowGenQrCodeOffilne);

                    // Get setting use static qrcode
                    var settingQrCode = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.UseStaticQrCode, companyId);
                    bool.TryParse(Helpers.GetStringFromValueSetting(settingQrCode.Value), out bool useStaticQrCode);

                    // Get setting time period
                    var timeSetting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.TimePeriodQrcode, companyId);
                    int.TryParse(Helpers.GetStringFromValueSetting(timeSetting.Value), out int timePeriod);

                    hashQrCode.Duration = timePeriod == 0 ? 12 : timePeriod;
                    hashQrCode.SecretKey = company.SecretCode;
                    hashQrCode.UseStaticQrCode = useStaticQrCode;
                    hashQrCode.SecretIV = Helpers.ReverseString(Constants.DynamicQr.Key);
                    if (allowGenQrCodeOffilne)
                    {
                        var user = _unitOfWork.AppDbContext.User.Include(m => m.Department)
                            .FirstOrDefault(u => u.AccountId == account.Id && !u.IsDeleted && u.CompanyId == companyId);
                        if (user != null)
                        {
                            var identification = _unitOfWork.CardRepository.GetQrCode(user.Id);
                            if (identification != null)
                            {
                                hashQrCode.CardId = identification.CardId;
                            }
                        }
                        string responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(hashQrCode);
                        return EncryptStringToBytes(responseJson,Encoding.UTF8.GetBytes(Constants.DynamicQr.Key));
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HashJsonToQrCode");
                return null;
            }
        }

        public void UpdateDeviceTokenForAccount(int accountId, string deviceToken)
        {
            try
            {
                var account = _unitOfWork.AccountRepository.GetById(accountId);
                account.DeviceToken = deviceToken;
                _unitOfWork.AccountRepository.Update(account);
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }

        static string EncryptStringToBytes(string plainText, byte[] Key)
        {
            // Check arguments. 
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            // if (IV == null || IV.Length <= 0)
            //     throw new ArgumentNullException("IV");
            byte[] encrypted;
            // SECURITY: Replaced RijndaelManaged with AES for better security
            // Create an AES object with the specified key
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.PKCS7;
                // aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, null);

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
            return BitConverter.ToString(encrypted).Replace("-", "");
        }

        /// <summary>
        /// Enhanced password validation with detailed error messages - gets companyId from current user context
        /// </summary>
        public PasswordValidationResult ValidatePasswordComplexity(string password)
        {
            var companyId = _httpContext.User.GetCompanyId();
            return ValidatePasswordComplexity(password, companyId);
        }

        /// <summary>
        /// Enhanced password validation with detailed error messages - with explicit companyId
        /// </summary>
        public PasswordValidationResult ValidatePasswordComplexity(string password, int companyId)
        {
            var result = new PasswordValidationResult();
            
            // If companyId is 0, skip password complexity validation
            // This happens during registration or admin operations without company context
            if (companyId == 0)
                return result;
                
            var loginSetting = _unitOfWork.SettingRepository.GetLoginSetting(companyId);
            
            if (loginSetting == null)
                return result;

            // Check uppercase requirement
            if (loginSetting.HaveUpperCase && !password.Any(char.IsUpper))
            {
                result.IsValid = false;
                result.MissingUppercase = true;
                result.ErrorMessages.Add(AccountResource.msgPasswordRequireUppercase ?? "Password must contain at least one uppercase letter.");
            }

            // Check number requirement
            if (loginSetting.HaveNumber && !password.Any(char.IsDigit))
            {   
                result.IsValid = false;
                result.MissingNumber = true;
                result.ErrorMessages.Add(AccountResource.msgPasswordRequireNumber ?? "Password must contain at least one number.");
            }

            // Check special character requirement
            if (loginSetting.HaveSpecial)
            {
                string specialChars = "!@#$%^&*()_+-=[]{}|\\:;\"'<>,.?/~`";
                if (!password.Any(c => specialChars.Contains(c)))
                {
                    result.IsValid = false;
                    result.MissingSpecialChar = true;
                    result.ErrorMessages.Add(AccountResource.msgPasswordRequireSpecialChar ?? "Password must contain at least one special character.");
                }
            }

            return result;
        }

        /// <summary>
        /// Legacy method for backward compatibility - uses current user's company
        /// </summary>
        public bool VerifyPassWord(string passWord)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var validationResult = ValidatePasswordComplexity(passWord, companyId);
            return validationResult.IsValid;
        }

        public LoginConfigModel ParseLoginConfig(string loginConfigJson)
        {
            if (string.IsNullOrEmpty(loginConfigJson))
            {
                return new LoginConfigModel
                {
                    IsFirstLogin = true,
                    TimeChangedPassWord = DateTime.UtcNow,
                    LoginFailCount = 0,
                    LastTimeLoginFail = DateTime.MinValue
                };
            }

            try
            {
                return JsonConvert.DeserializeObject<LoginConfigModel>(loginConfigJson) ?? new LoginConfigModel
                {
                    IsFirstLogin = true,
                    TimeChangedPassWord = DateTime.UtcNow,
                    LoginFailCount = 0,
                    LastTimeLoginFail = DateTime.MinValue
                };
            }
            catch
            {
                return new LoginConfigModel
                {
                    IsFirstLogin = true,
                    TimeChangedPassWord = DateTime.UtcNow,
                    LoginFailCount = 0,
                    LastTimeLoginFail = DateTime.MinValue
                };
            }
        }

        public bool IsAccountLocked(LoginConfigModel loginConfig, LoginSettingModel loginSetting)
        {
            if (loginSetting == null) return false;

            var maxLoginAttempts = loginSetting.MaximumEnterWrongPassword;
            var lockoutMinutes = loginSetting.TimeoutWhenWrongPassword;

            if (maxLoginAttempts > 0 && loginConfig.LoginFailCount >= maxLoginAttempts && loginConfig.LastTimeLoginFail != DateTime.MinValue)
            {
                var timeSinceLastFail = DateTime.UtcNow - loginConfig.LastTimeLoginFail;
                if (timeSinceLastFail.TotalMinutes < lockoutMinutes)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Consolidated method to get password status for an account
        /// </summary>
        public PasswordStatusResult GetPasswordStatus(Account account, int companyId)
        {
            try
            {
                var loginSetting = _unitOfWork.SettingRepository.GetLoginSetting(companyId);
                var loginConfig = ParseLoginConfig(account.LoginConfig);
                var result = new PasswordStatusResult();

                var minutesSinceLastChange = (DateTime.UtcNow - loginConfig.TimeChangedPassWord).TotalMinutes;
                if (loginSetting?.MaximumTimeUsePassword > 0)
                {
                    // Check if password is expired
                    if (minutesSinceLastChange > loginSetting.MaximumTimeUsePassword)
                    {
                        result.IsExpired = true;
                        result.BlockLogin = true;
                        result.Message = AccountResource.msgPasswordExpired ?? "Your password has expired";
                        return result;
                    }
                }
                // Check if password is in warning period
                if (loginSetting?.TimeNeedToChange > 0 &&
                    minutesSinceLastChange > loginSetting.TimeNeedToChange)
                {
                    result.IsInWarningPeriod = true;
                    result.BlockLogin = false;
                    result.Message = AccountResource.msgPasswordNeedToChange ?? "You need to change your password to enhance security";
                }

                // Check first login requirement
                if (loginConfig.IsFirstLogin && loginSetting?.ChangeInFirstTime == true)
                {
                    result.IsFirstLogin = true;
                    result.BlockLogin = false;
                    result.Message = AccountResource.msgPasswordNeedToChange ?? "You need to change your password to enhance security";
                }

                return result;
            }
            catch (System.Exception)
            {
                // Return default PasswordStatusResult if any exception occurs
                return new PasswordStatusResult();
            }
        }

        // Keep these methods for backward compatibility but use consolidated logic
        public bool IsPasswordExpired(Account account, int companyId)
        {
            return GetPasswordStatus(account, companyId).IsExpired;
        }


        public void UpdatePasswordChangedConfig(Account account)
        {
            try
            {
                var loginConfig = ParseLoginConfig(account.LoginConfig);
                loginConfig.IsFirstLogin = false;
                loginConfig.TimeChangedPassWord = DateTime.UtcNow;
                account.LoginConfig = JsonConvert.SerializeObject(loginConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdatePasswordChangedConfig");
            }
        }

        public string GetAccountLockoutMessage(LoginSettingModel loginSetting)
        {
            if (loginSetting == null) return AccountResource.msgAccountLocked ?? "Account is locked";
            
            int lockoutMinutes = (int)loginSetting.TimeoutWhenWrongPassword;
            return lockoutMinutes > 0
                ? string.Format(AccountResource.msgAccountLockedWithDuration ?? "Account locked for {0} minutes", lockoutMinutes)
                : AccountResource.msgAccountLocked ?? "Account is locked";
        }

        public (bool isValid, string errorMessage, bool passwordChangeRequired, string warningMessage) ValidateAccountForLogin(Account account, int companyId)
        {
            try
            {
                if (account == null)
                    return (false, MessageResource.InvalidCredentials, false, string.Empty);

                if (account.IsDeleted)
                    return (false, MessageResource.InvalidCredentials, false, string.Empty);

                var loginConfig = ParseLoginConfig(account.LoginConfig);
                var loginSetting = _unitOfWork.SettingRepository.GetLoginSetting(companyId);

                // Check if account is locked - this blocks login
                if (IsAccountLocked(loginConfig, loginSetting))
                {
                    return (false, GetAccountLockoutMessage(loginSetting), false, string.Empty);
                }

                // Get consolidated password status
                var passwordStatus = GetPasswordStatus(account, companyId);

                // If password blocks login (expired), return error
                if (passwordStatus.BlockLogin)
                {
                    return (false, passwordStatus.Message, false, string.Empty);
                }

                // If there's a warning (first login or approaching expiry), allow login with warning
                return (true, string.Empty, passwordStatus.HasWarning, passwordStatus.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ValidateAccountForLogin");
                return (false, "An error occurred during login validation", false, string.Empty);
            }
        }

    }
}   
