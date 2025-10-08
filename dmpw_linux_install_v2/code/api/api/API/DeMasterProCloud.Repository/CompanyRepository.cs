using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using Newtonsoft.Json;
using DeMasterProCloud.DataModel.PlugIn;
using System.Reflection;
using DeMasterProCloud.DataAccess.Migrations;
using DeMasterProCloud.DataModel.SystemInfo;
using Microsoft.Extensions.Configuration;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// Interface for Company repository
    /// </summary>
    public interface ICompanyRepository : IGenericRepository<Company>
    {
        string MakeCompanyCode();
        Company GetCompanyById(int? id);
        Company GetCompanyByCode(string code);
        List<Company> GetExpiredCompaniesByDay(int numberOfDay = 0, bool bExactDay = true);
        void UpdateCompaniesToExpire(List<Company> companies);
        Company GetRootCompany();
        List<Company> GetByIds(List<int> ids);
        List<Company> GetCompanies();
        String GetCompanyCodeByCompanyId(int companyId);
        List<Company> GetCompaniesByPlugin(string pluginName);
        bool CheckCompanyByPlugin(string pluginName, int companyId);
        CheckLimitAddedModel CheckLimitCountOfUsers(int companyId, int numberAdded);
        string GetDevicePasswordByCompany(int companyId);
    }

    /// <summary>
    /// Company repository
    /// </summary>
    public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        public CompanyRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<CompanyRepository>();
        }

        /// <summary>
        /// Get company by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Company GetCompanyById(int? id)
        {
            try
            {
                return _dbContext.Company/*.Include(c => c.Account)*/
                    .FirstOrDefault(c =>
                        c.Id == id /*&& c.Account.FirstOrDefault(m => m.RootFlag).CompanyId == id*/ &&
                        !c.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompanyById");
                return null;
            }
        }

        /// <summary>
        /// Get company by code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Company GetCompanyByCode(string code)
        {
            try
            {
                return _dbContext.Company.FirstOrDefault(c => c.Code == code && !c.IsDeleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompanyByCode");
                return null;
            }
        }

        ///// <summary>
        ///// Get companies
        ///// </summary>
        ///// <param></param>
        ///// <returns></returns>
        //public List<Company> GetCompanies()
        //{
        //    return _dbContext.Company.Where(c => !c.IsDeleted).ToList();
        //}

        /// <summary>
        /// Get company by list of id
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<Company> GetByIds(List<int> ids)
        {
            try
            {
                return GetMany(c => ids.Contains(c.Id) && !c.IsDeleted).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIds");
                return new List<Company>();
            }
        }

        /// <summary>
        /// Make company code
        /// </summary>
        /// <returns></returns>
        public string MakeCompanyCode()
        {
            try
            {
                var companies = _dbContext.Company.ToList();
                var companyCode = Helpers.GenerateCompanyCode();
                while (companies.Any(c => c.Code == companyCode))
                {
                    companyCode = Helpers.GenerateCompanyCode();
                }
                return companyCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MakeCompanyCode");
                return string.Empty;
            }
        }

        /// <summary>
        /// Get company code
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public string GetCompanyCodeByCompanyId(int companyId)
        {
            try
            {
                var company = _dbContext.Company.FirstOrDefault(c => c.Id == companyId);

                return company.Code;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompanyCodeByCompanyId");
                return string.Empty;
            }
        }

        /// <summary>
        /// Get list of company that enabled specific plugin.
        /// </summary>
        /// <param name="pluginName"></param>
        /// <returns></returns>
        public List<Company> GetCompaniesByPlugin(string pluginName)
        {
            try
            {
                List<Company> enabledCompanies = new List<Company>();

                var plugInTuples = _dbContext.PlugIn.Include(m => m.Company).Where(m => !m.Company.IsDeleted);

                foreach (var plugin in plugInTuples)
                {
                    var solution = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);

                    foreach (PropertyInfo pi in solution.GetType().GetProperties())
                    {
                        if (pi.Name == pluginName)
                        {
                            bool value = (bool)solution.GetType().GetProperty(pluginName).GetValue(solution, null);
                            if (value)
                                enabledCompanies.Add(plugin.Company);
                        }
                    }
                }

                return enabledCompanies;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompaniesByPlugin");
                return new List<Company>();
            }
        }

        public bool CheckCompanyByPlugin(string pluginName, int companyId)
        {
            try
            {
                var plugin = _dbContext.PlugIn.Include(m => m.Company).Where(m => !m.Company.IsDeleted && m.CompanyId == companyId).FirstOrDefault();
                if (plugin != null)
                {
                    var solution = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);

                    foreach (PropertyInfo pi in solution.GetType().GetProperties())
                    {
                        if (pi.Name == pluginName)
                        {
                            bool value = (bool)solution.GetType().GetProperty(pluginName).GetValue(solution, null);
                            if (value)
                                return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckCompanyByPlugin");
                return false;
            }
        }

        public CheckLimitAddedModel CheckLimitCountOfUsers(int companyId, int numberAdded)
        {
            try
            {
                var company = _dbContext.Company.FirstOrDefault(m => m.Id == companyId && !m.IsDeleted);
                var countUser = _dbContext.User.Count(m => m.CompanyId == companyId && !m.IsDeleted);
                return new CheckLimitAddedModel
                {
                    IsAdded = company != null && (company.LimitCountOfUser == 0  || countUser + numberAdded <= company.LimitCountOfUser),
                    NumberOfCurrent = countUser,
                    NumberOfMaximum = company?.LimitCountOfUser ?? 0,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckLimitCountOfUsers");
                return new CheckLimitAddedModel
                {
                    IsAdded = false,
                    NumberOfCurrent = 0,
                    NumberOfMaximum = 0
                };
            }
        }

        public string GetDevicePasswordByCompany(int companyId)
        {
            try
            {
                var setting = _dbContext.Setting.FirstOrDefault(s => s.CompanyId == companyId && s.Key == Constants.Settings.KeyDevicePassword);
                if (setting != null)
                    return Helpers.GetStringFromValueSetting(setting.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            // Get default device password from configuration
            var defaultPassword = ApplicationVariables.Configuration?[Constants.Settings.DefaultDevicePassword];

            if (string.IsNullOrEmpty(defaultPassword))
            {
                Console.WriteLine("[SECURITY WARNING] DefaultDevicePassword is not configured in appsettings");
                throw new InvalidOperationException("Default device password configuration is missing or empty.");
            }

            return defaultPassword;
        }


        /// <summary>
        /// Get expired companies by day
        /// </summary>
        /// <param name="numberOfDay"></param>
        /// <param name="bExactDay"></param>
        /// <returns></returns>
        public List<Company> GetExpiredCompaniesByDay(int numberOfDay, bool bExactDay)
        {
            try
            {
                var companies = _dbContext.Company.Include(c => c.Account)
                    .Where(c => !c.RootFlag && !c.IsDeleted)
                    .AsEnumerable();
                companies = bExactDay
                    ? companies.Where(c => DateTime.Now.AddDays(numberOfDay).Date.Subtract(c.ExpiredTo.Date).Days == 0)
                    : companies.Where(c => DateTime.Now.AddDays(numberOfDay).Date.Subtract(c.ExpiredTo.Date).Days >= 0);
                return companies.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetExpiredCompaniesByDay");
                return new List<Company>();
            }
        }

        /// <summary>
        /// Update companies to expired
        /// </summary>
        /// <param name="companies"></param>
        public void UpdateCompaniesToExpire(List<Company> companies)
        {
            //Update companies to expired
            foreach (var company in companies)
            {
                //company.IsD = (short)Status.Invalid;
                Update(company);
            }
        }

        /// <summary>
        /// Get root company
        /// </summary>
        /// <returns></returns>
        public Company GetRootCompany()
        {
            try
            {
                return Get(m => m.RootFlag);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetRootCompany");
                return null;
            }
        }

        /// <summary>
        /// Get all companies
        /// </summary>
        /// <returns></returns>
        public List<Company> GetCompanies()
        {
            try
            {
                return GetMany(c =>!c.IsDeleted).OrderBy(c => c.Id).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompanies");
                return new List<Company>();
            }
        }
    }
}