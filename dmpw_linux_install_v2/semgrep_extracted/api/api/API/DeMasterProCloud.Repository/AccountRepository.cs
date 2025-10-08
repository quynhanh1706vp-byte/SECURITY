using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using System;
using Microsoft.Extensions.Options;
using DeMasterProCloud.DataModel.Api;
using System.Collections.Generic;
using DeMasterProCloud.DataModel.Account;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Repository
{

    /// <summary>
    /// UserLogin repository interface
    /// </summary>
    public interface IAccountRepository : IGenericRepository<Account>
    {
        Account GetRootAccountByCompany(int companyId);
        Account AddOrUpdateDefaultAccount(Company company, string userName, string password);
        Account AddDefaultAccount(string userName, string password);
        bool IsExist(int id, string username, int companyId);
        Account GetByIdAndCompanyId(int companyId, int id);
        Account GetByUserNameAndCompanyId(int companyId, string username);
        Account GetByUserName(string username);
        Account GetByUserNameWithTracking(string userName);

        void AddTokenAndRefreshToken( string refreshToken, Account model, int expiryRefreshToken);
        Account GetAccountByRefreshToken(string refreshToken);
        string GetRefreshTokenByUserName(string userName);

        IQueryable<CompanyAccount> GetCompanyAccountByRoleIdandCompanyId(int roleId, int companyId);
        IQueryable<CompanyAccount> GetCompanyAccountByRoleIdsandCompanyId(List<int> roleIds, int companyId);

        Account GetByIdWithRole(int accountId);
        //IQueryable<Account> GetAccountsByCompanyId(int companyId);
        int GetSystemTotalAccount();
        Account GetByPhoneNumber(string phoneNumber);
        List<Account> GetAccountsByCompany(int companyId);
        void SaveCurrentLogin(int accountId, CurrentLoginInfoModel model);
        CurrentLoginInfoModel GetCurrentLoginInfo(int accountId);
    }

    /// <summary>
    /// UserLogin repository
    /// </summary>
    public class AccountRepository : GenericRepository<Account>, IAccountRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        public AccountRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<AccountRepository>();
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
                return Get(c => c.CompanyId == companyId
                //&& c.RootFlag
                && !c.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRootAccountByCompany");
                return null;
            }
        }

        /// <summary>
        /// Add or update a default account
        /// </summary>
        /// <param name="company"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public Account AddOrUpdateDefaultAccount(Company company, string userName, string password)
        {
            try
            {
                var account = Get(c => c.CompanyId == company.Id
                && c.RootFlag
                && !c.IsDeleted);
                if (account == null)
                {
                    account = new Account
                    {
                        Username = userName,
                        Password = SecurePasswordHasher.Hash(password),
                        CompanyId = company.Id,
                        Type = (short)AccountType.SuperAdmin,
                        RootFlag = true
                    };
                    Add(account);
                }
                else
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        account.Password = SecurePasswordHasher.Hash(password);
                    }
                    account.Username = userName;
                }

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddOrUpdateDefaultAccount");
                return null;
            }
        }

        /// <summary>
        /// Add or update a default account
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public Account AddDefaultAccount(string userName, string password)
        {
            try
            {
                var account = Get(c =>
                c.RootFlag
                && !c.IsDeleted);
                if (account == null)
                {
                    account = new Account
                    {
                        Username = userName,
                        Password = SecurePasswordHasher.Hash(password),
                        Language = Constants.DefaultLanguage,
                        TimeZone = Constants.DefaultTimeZone,
                        Type = (short)AccountType.SystemAdmin,
                        RootFlag = true
                    };
                    Add(account);
                }
                else
                {
                    if (!string.IsNullOrEmpty(password))
                    {
                        account.Password = SecurePasswordHasher.Hash(password);
                    }
                    account.Username = userName;
                }

                return account;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddDefaultAccount");
                return null;
            }
        }

        /// <summary>
        /// Get by account id
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public new Account GetById(int accountId)
        {
            try
            {
                return _dbContext.Account.Include(c => c.Company)
                    .FirstOrDefault(c => c.Id == accountId && !c.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return null;
            }
        }

        /// <summary>
        /// Get by account id incdluding company and dynamic role.
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public Account GetByIdWithRole(int accountId)
        {
            try
            {
                return _dbContext.Account.Include(c => c.Company).Include(c => c.DynamicRole)
                    .FirstOrDefault(c => c.Id == accountId && !c.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdWithRole");
                return null;
            }
        }

        /// <summary>
        /// Check whether if account is exists
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public bool IsExist(int id, string username, int companyId)
        {
            try
            {
                var userLogin = Get(m =>
                    m.Username.ToLower() == username.ToLower() && !m.IsDeleted &&
                    m.CompanyId == companyId && m.Id != id);

                return userLogin != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsExist");
                return false;
            }
        }

        /// <summary>
        /// Get by companyid and id
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public Account GetByIdAndCompanyId(int companyId, int id)
        {
            try
            {
                return _dbContext.Account.AsNoTracking().FirstOrDefault(x => x.Id == id && x.CompanyId == companyId && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdAndCompanyId");
                return null;
            }
        }

        public Account GetByUserNameAndCompanyId(int companyId, string username)
        {
            try
            {
                return _dbContext.Account.AsNoTracking().FirstOrDefault(x => x.Username.ToLower().Equals(username.ToLower()) && x.CompanyId == companyId && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByUserNameAndCompanyId");
                return null;
            }
        }

        public void AddTokenAndRefreshToken(string refreshToken, Account model, int expiryRefreshToken)
        {

            try
            {
                var accountLogin = _dbContext.Account.Where(x => x.Username.ToLower() == model.Username.ToLower() && !x.IsDeleted).Select(x => x.Id).FirstOrDefault();
                Account acc = _dbContext.Account.FirstOrDefault(item => item.Id == accountLogin);
                //var createDateRefreshToken = acc.CreateDateRefreshToken;
                //if(createDateRefreshToken == null)
                //{
                //    createDateRefreshToken = DateTime.Now;
                //}    
                //if(acc.RefreshToken == "" || acc.RefreshToken == null || createDateRefreshToken.AddMonths(expiryRefreshToken) < DateTime.Now)
                //{
                //    acc.RefreshToken = refreshToken;
                //    acc.CreateDateRefreshToken = DateTime.Now;
                //}

                acc.RefreshToken = refreshToken;
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddTokenAndRefreshToken");
            }

        }

        public Account GetAccountByRefreshToken(string refreshToken)
        {
            try
            {
                Console.WriteLine("Refresh token: {0}", refreshToken);
                return _dbContext.Account.AsNoTracking().FirstOrDefault(x => x.RefreshToken == refreshToken && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccountByRefreshToken");
                return null;
            }
        }

        public string GetRefreshTokenByUserName(string userName)
        {
            try
            {
                return _dbContext.Account.AsNoTracking().FirstOrDefault(x => x.Username.ToLower() == userName.ToLower() && !x.IsDeleted)?.RefreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRefreshTokenByUserName");
                return null;
            }
        }


        public IQueryable<CompanyAccount> GetCompanyAccountByRoleIdandCompanyId(int roleId, int companyId)
        {
            try
            {
                var data = _dbContext.CompanyAccount
                    .Include(x => x.Account).Where(m => m.DynamicRoleId == roleId && m.CompanyId == companyId && !m.Account.IsDeleted);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompanyAccountByRoleIdandCompanyId");
                return new List<CompanyAccount>().AsQueryable();
            }
        }

        public IQueryable<CompanyAccount> GetCompanyAccountByRoleIdsandCompanyId(List<int> roleIds, int companyId)
        {
            try
            {
                var data = _dbContext.CompanyAccount.Include(x => x.Account)
                                                .Where(m => m.DynamicRoleId != null
                                                && roleIds.Contains(m.DynamicRoleId.Value) && m.CompanyId == companyId && !m.Account.IsDeleted);

                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompanyAccountByRoleIdsandCompanyId");
                return new List<CompanyAccount>().AsQueryable();
            }
        }

        ///// <summary>
        ///// Get all accounts that belongs to specific company.
        ///// </summary>
        ///// <param name="conpanyId"></param>
        ///// <returns></returns>
        //public IQueryable<Account> GetAccountsByCompanyId(int companyId)
        //{
        //    //var data = _dbContext.Account.Where(m => m.CompanyId == companyId && !m.IsDeleted);
        //    var data = (from a in _dbContext.Company
        //                join b in _dbContext.CompanyAccount on a.Id equals b.CompanyId
        //                join c in _dbContext.Account on b.AccountId equals c.Id
        //                where a.Id == companyId && !c.IsDeleted
        //                select new Account
        //                {
        //                    Id = c.Id,
        //                    Username  = c.Username,
        //                    CompanyId = a.Id,
        //                    DynamicRoleId = b.DynamicRoleId,
        //                    DynamicRole = _dbContext.DynamicRole.Where(d=>d.Id==b.DynamicRoleId).FirstOrDefault()
        //                });
        //    return data;
        //}

        /// <summary>
        /// Get total account.
        /// </summary>
        /// <returns></returns>
        public int GetSystemTotalAccount()
        {
            try
            {
                var data = _dbContext.Account.Where(m => !m.IsDeleted);

                return data.Count();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSystemTotalAccount");
                return 0;
            }
        }

        /// <summary>
        /// Get Account object with userName (AsNoTracking)
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Account GetByUserName(string username)
        {
            try
            {
                if(string.IsNullOrWhiteSpace(username)) return null;

                return _dbContext.Account.AsNoTracking().FirstOrDefault(x => x.Username.ToLower().Equals(username.ToLower()) && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByUserName");
                return null;
            }
        }

        /// <summary>
        /// Get Account object with userName
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Account GetByUserNameWithTracking(string userName)
        {
            try
            {
                return _dbContext.Account.FirstOrDefault(x => x.Username.ToLower().Equals(userName.ToLower()) && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByUserNameWithTracking");
                return null;
            }
        }

        public Account GetByPhoneNumber(string phoneNumber)
        {
            try
            {
                return _dbContext.Account.FirstOrDefault(x => x.PhoneNumber.Equals(phoneNumber) && !x.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByPhoneNumber");
                return null;
            }
        }

        public List<Account> GetAccountsByCompany(int companyId)
        {
            try
            {
                return _dbContext.Account.Where(m => !m.IsDeleted && m.CompanyId == companyId).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccountsByCompany");
                return new List<Account>();
            }
        }

        public void SaveCurrentLogin(int accountId, CurrentLoginInfoModel model)
        {
            var account = _dbContext.Account.FirstOrDefault(m => m.Id == accountId && !m.IsDeleted);
            if (account != null)
            {
                account.CurrentLoginInfo = model == null ? null : JsonConvert.SerializeObject(model);
                try
                {
                    _dbContext.Account.Update(account);
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SaveCurrentLogin");
                }
            }
        }

        public CurrentLoginInfoModel GetCurrentLoginInfo(int accountId)
        {
            try
            {
                var account = _dbContext.Account.FirstOrDefault(m => m.Id == accountId && !m.IsDeleted);
                if (account != null && !string.IsNullOrEmpty(account.CurrentLoginInfo) && !account.RootFlag)
                {
                    return JsonConvert.DeserializeObject<CurrentLoginInfoModel>(account.CurrentLoginInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCurrentLoginInfo");
            }
            
            return null;
        }
    }
}