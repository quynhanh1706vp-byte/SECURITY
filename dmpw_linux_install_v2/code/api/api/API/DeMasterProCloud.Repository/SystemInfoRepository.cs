using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.LicenseHelpers;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.SystemInfo;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DeMasterProCloud.Repository
{
    public interface ISystemInfoRepository : IGenericRepository<SystemInfo>
    {
        bool CheckLicenseVerified();
        CheckLimitAddedModel CheckLimitDevicesAdded(int numberOfAdded = 1);
        DataLicenseInfoModel GetDataLicenseInfo();
        void UpdateLicense(string licenseInfo);
        SystemInfo GetSystemInfo();
        string GetSecretCodeInSystem();
    }
    
    public class SystemInfoRepository : GenericRepository<SystemInfo>, ISystemInfoRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        
        public SystemInfoRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<SystemInfoRepository>();
        }

        public bool CheckLicenseVerified()
        {
            if (Constants.VerifyLicenseSetting.IgnoredCheckFromLicense)
            {
                return true;
            }
            
            var dataLicenseInfo = GetDataLicenseInfo();
            if (dataLicenseInfo == null) return false;
            
            return dataLicenseInfo.EffectiveDate < DateTime.Now && DateTime.Now < dataLicenseInfo.ExpiredDate && dataLicenseInfo.MachineInfo.MacAddress == MachineHelpers.GetLocalMacAddress();            
            
        }

        public CheckLimitAddedModel CheckLimitDevicesAdded(int numberOfAdded = 1)
        {
            if (Constants.VerifyLicenseSetting.IgnoredCheckFromLicense)
            {
                return new CheckLimitAddedModel(){IsAdded = true};
            }
            
            var data = GetDataLicenseInfo();
            if (data != null)
            {
                int countDeviceInDb = _dbContext.IcuDevice.Count(m => !m.IsDeleted);
                return new CheckLimitAddedModel()
                {
                    IsAdded = countDeviceInDb + numberOfAdded <= data.CountOfDevices,
                    NumberOfCurrent = countDeviceInDb,
                    NumberOfMaximum = data.CountOfDevices,
                };
            }
            else
            {
                return new CheckLimitAddedModel()
                {
                    IsAdded = false,
                };
            }
        }
        
        public DataLicenseInfoModel GetDataLicenseInfo()
        {
            var info = _dbContext.SystemInfo.OrderByDescending(m => m.Id).FirstOrDefault();
            if (info == null)
            {
                return null;
            }
            else
            {
                try
                {
                    string key = RSACryptography.GetDeMasterProKey();
                    var dataEncrypts = RSACryptography.SplitDataDecryptByKeySize(JsonConvert.DeserializeObject<List<byte>>(info.LicenseInfo));
                    List<byte> licenseInfo = new List<byte>();
                    foreach (var dataEncrypt in dataEncrypts)
                    {
                        var dataDecrypt = RSACryptography.DecryptData(dataEncrypt, key);
                        licenseInfo.AddRange(dataDecrypt);
                    }
                    var dataLicenseInfo = JsonConvert.DeserializeObject<DataLicenseInfoModel>(Encoding.UTF8.GetString(licenseInfo.ToArray()));
                
                    return dataLicenseInfo;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
        }
        
        public void UpdateLicense(string licenseInfo)
        {
            var info = _dbContext.SystemInfo.OrderByDescending(m => m.Id).FirstOrDefault();
            if(info == null)
            {
                info = new SystemInfo()
                {
                    LicenseInfo = licenseInfo,
                    SecretCode = GetSecretCodeInSystem(),
                    UpdatedOn = DateTime.Now,
                };
            }
            else
            {
                info.Id = 0;
                info.LicenseInfo = licenseInfo;
                info.UpdatedOn = DateTime.Now;
            }

            try
            {
                _dbContext.SystemInfo.Add(info);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateLicense");
            }
        }

        public SystemInfo GetSystemInfo()
        {
            return _dbContext.SystemInfo.OrderByDescending(m => m.Id).FirstOrDefault();
        }

        public string GetSecretCodeInSystem()
        {
            var systemInfo = _dbContext.SystemInfo.OrderByDescending(m => m.Id).FirstOrDefault();
            if (systemInfo != null && !string.IsNullOrEmpty(systemInfo.SecretCode))
            {
                return systemInfo.SecretCode;
            }
            else
            {
                var company = _dbContext.Company.FirstOrDefault(m => !m.IsDeleted && !string.IsNullOrEmpty(m.SecretCode));
                if (company != null)
                {
                    return company.SecretCode;
                }
                
                return Helpers.GenerateCompanyKey();
            }
        }
    }
}