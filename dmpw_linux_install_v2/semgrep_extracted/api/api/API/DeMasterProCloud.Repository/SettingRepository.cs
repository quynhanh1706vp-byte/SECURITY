using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataModel.Login;
using System;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// Interface for Setting repository
    /// </summary>
    public interface ISettingRepository : IGenericRepository<Setting>
    {
        Setting GetByKey(string key, int companyId);
        List<Setting> GetByKeys(List<string> keys, int companyId);
        Setting GetLogo(int companyId);
        Setting GetQRLogo(int companyId);


        Setting GetLanguage(int companyId);
        bool IsKeyExist(SettingModel model);
        List<Setting> GetCompaniesWithAutoRenew();
        
        Setting GetCompaniesPeriodAutoRenew(int companyId);

        List<Setting> GetByCompanyId(int companyId);
        int GetDefaultPaginationNumber(int companyId);
        LoginSettingModel GetLoginSetting(int companyId);
    }

    /// <summary>
    /// Setting repository
    /// </summary>
    public class SettingRepository : GenericRepository<Setting>, ISettingRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        public SettingRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<SettingRepository>();
        }

        /// <summary>
        /// Get setting by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Setting GetByKey(string key, int companyId)
        {
            try
            {
                return Get(c => c.Key == key && c.CompanyId == companyId);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetByKey");
                return null;
            }
        }

        /// <summary>
        /// Get setting by keys
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<Setting> GetByKeys(List<string> keys, int companyId)
        {
            try
            {
                return GetMany(c => keys.Any(m =>m == c.Key) && c.CompanyId == companyId).ToList();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetByKeys");
                return new List<Setting>();
            }
        }

        /// <summary>
        /// Get settings by companyId
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<Setting> GetByCompanyId (int companyId)
        {
            try
            {
                //return GetMany(c => c.CompanyId == companyId).ToList();

                var settings = _dbContext.Setting.Where(c => c.CompanyId == companyId).ToList();

                return settings;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCompanyId");
                return new List<Setting>();
            }
        }

        public Setting GetLogo(int companyId)
        {
            try
            {
                var logo = Get(c => c.Key == "logo" && c.CompanyId == companyId);
                return logo;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetLogo");
                return null;
            }
        }

        /// <summary>
        /// Get QR Logo image from DB
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Setting GetQRLogo(int companyId)
        {
            try
            {
                var logo = Get(c => c.Key == Constants.Settings.QRLogo && c.CompanyId == companyId);
                return logo;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetQRLogo");
                return null;
            }
        }



        /// <summary>
        /// Check if key is exists
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool IsKeyExist(SettingModel model)
        {
            try
            {
                return _dbContext.Setting.Any(m => m.Key == model.Key && m.Id != model.Id);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in IsKeyExist");
                return false;
            }
        }

        public Setting GetLanguage(int companyId)
        {
            try
            {
                var setting = Get(c => c.Key == "language" && c.CompanyId == companyId);
                return setting;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetLanguage");
                return null;
            }
        }
        
        public List<Setting> GetCompaniesWithAutoRenew()
        {
            try
            {
                var companies = GetMany(c => c.Key == "auto_renew_qr_code").ToList();
                return companies;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompaniesWithAutoRenew");
                return new List<Setting>();
            }
        }
        
        public Setting GetCompaniesPeriodAutoRenew(int companyId)
        {
            try
            {
                var companies = Get(c => c.Key == "qr_code_auto_renew_period" && c.CompanyId == companyId);
                return companies;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompaniesPeriodAutoRenew");
                return null;
            }
        }

        public int GetDefaultPaginationNumber(int companyId)
        {
            try
            {
                var setting = _dbContext.Setting.FirstOrDefault(m => m.Key == Constants.Settings.PaginationPage && m.CompanyId == companyId);
                if (setting != null)
                {
                    var value = Helpers.GetStringFromValueSetting(setting.Value);
                    if (int.TryParse(value, out int paginationNumber))
                    {
                        return paginationNumber;
                    }
                }
                return Constants.DefaultPaginationNumber;
            }
            catch
            {
                return Constants.DefaultPaginationNumber;
            }
        }

        public LoginSettingModel GetLoginSetting(int companyId)
        {
            LoginSettingModel setting = new LoginSettingModel();
            try
            {
                if (companyId != 0)
                {
                    
                    setting.ChangeInFirstTime = Helpers.GetBoolFromValueSetting(GetByKey(Constants.Settings.KeyChangeInFirstTime, companyId).Value);
                    setting.HaveUpperCase = Helpers.GetBoolFromValueSetting(GetByKey(Constants.Settings.KeyHaveUpperCase, companyId).Value);
                    setting.HaveNumber = Helpers.GetBoolFromValueSetting(GetByKey(Constants.Settings.KeyHaveNumber, companyId).Value);
                    setting.HaveSpecial = Helpers.GetBoolFromValueSetting(GetByKey(Constants.Settings.KeyHaveSpecial, companyId).Value);
                    setting.TimeNeedToChange = Helpers.GetIntFromValueSetting(GetByKey(Constants.Settings.KeyTimeNeedToChange, companyId).Value);
                    setting.MaximumTimeUsePassword = Helpers.GetIntFromValueSetting(GetByKey(Constants.Settings.KeyMaximumTimeUsePassword, companyId).Value);
                    setting.MaximumEnterWrongPassword = Helpers.GetIntFromValueSetting(GetByKey(Constants.Settings.KeyMaximumEnterWrongPassword, companyId).Value);
                    setting.TimeoutWhenWrongPassword = Helpers.GetIntFromValueSetting(GetByKey(Constants.Settings.KeyTimeoutWhenWrongPassword, companyId).Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                setting = new LoginSettingModel();
            }

            return setting;
        }
    }
}