using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.LicenseHelpers;
using DeMasterProCloud.DataModel.SystemInfo;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.Service.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeMasterProCloud.Service
{
    public interface ISystemInfoService
    {
        bool CheckLicenseVerified();
        DataLicenseInfoModel GetDataLicenseInfo();
        string VerifyLicense(VerifyLicenseModel model);
        List<string> MigrationAvatarUser(string oldUrl, string newUrl);
        object MigrationLinkImageEventLog(string oldUrl, string newUrl, string companyCode, DateTime start, DateTime end);
        void VerifyLicenseTesting();
    }
    
    public class SystemInfoService : ISystemInfoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public SystemInfoService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<SystemInfoService>();
        }
        
        public bool CheckLicenseVerified()
        {
            return _unitOfWork.SystemInfoRepository.CheckLicenseVerified();
        }

        public DataLicenseInfoModel GetDataLicenseInfo()
        {
            return _unitOfWork.SystemInfoRepository.GetDataLicenseInfo();
        }
        
        public string VerifyLicense(VerifyLicenseModel model)
        {
            try
            {
                // get machine info
                var dataVerify = new DataVerifyLicenseModel()
                {
                    LicenseKey = model.LicenseKey,
                    MachineInfo = new MachineInfoModel()
                    {
                        MacAddress = MachineHelpers.GetLocalMacAddress(),
                        MachineName = MachineHelpers.GetLocalMachineName(),
                        OsIdentifier = MachineHelpers.GetLocalOsIdentifier(),
                        OsDescription = MachineHelpers.GetLocalOsDescription(),
                    }
                };

                string key = RSACryptography.GetDeMasterProKey();
                if (string.IsNullOrEmpty(key))
                    return MessageResource.msgDemasterKeyInvalid;

                string url = _configuration.GetSection(Constants.VerifyLicenseSetting.DefineHostVerifyLicense).Get<string>() + Constants.VerifyLicenseSetting.ApiLicenseKeyActive;
                JObject stuff = null;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        string json = Helpers.JsonConvertCamelCase(dataVerify);
                        var dataBody = new StringContent(json, Encoding.UTF8, "application/json");
                        var response = httpClient.PostAsync(url, dataBody);
                        response.Wait();
                        var result = response.Result;
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();
                        string responseData = readTask.Result;
                        stuff = JObject.Parse(responseData);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message + ex.StackTrace);
                    return MessageResource.SystemError;
                }
                if (stuff != null  && stuff["statusCode"] != null && stuff["statusCode"].ToString() != "200")
                {
                    return stuff["message"] != null ? stuff["message"].ToString() : MessageResource.SystemError;
                }
                else if(stuff != null && stuff["data"] != null)
                {
                    var dataLicenseInfo = DecryptDataLicenseInfo(stuff["data"].ToObject<List<byte>>(), key);
                    if (dataLicenseInfo == null || dataVerify.MachineInfo == null || dataLicenseInfo.MachineInfo.MacAddress != dataVerify.MachineInfo.MacAddress)
                    {
                        return MessageResource.SystemError;
                    }
                    else
                    {
                        _unitOfWork.SystemInfoRepository.UpdateLicense(JsonConvert.SerializeObject(stuff["data"].ToObject<List<byte>>()));
                        _unitOfWork.Save();

                        SetupAfterVerifySuccess(dataLicenseInfo);
                    }

                    return null;
                }
                else
                {
                    return MessageResource.msgVerifyLicenseFalied;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyLicense");
                return MessageResource.SystemError;
            }
        }

        private DataLicenseInfoModel DecryptDataLicenseInfo(List<byte> dataEncrypted, string key)
        {
            try
            {
                var dataEncrypts = RSACryptography.SplitDataDecryptByKeySize(dataEncrypted);
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
                _logger.LogError(ex.Message + ex.StackTrace);
                return null;
            }
        }

        private void SetupAfterVerifySuccess(DataLicenseInfoModel model)
        {
            // update config DefaultPlugin (file appsettings.json)
            try
            {
                string textSetting = File.ReadAllText("appsettings.json");
                var dataSetting = JObject.Parse(textSetting);
                if(dataSetting["DefaultPlugin"] == null) dataSetting.Add("DefaultPlugin");
                dataSetting["DefaultPlugin"] = JToken.Parse(JsonConvert.SerializeObject(model.ListOfPlugIn));
                File.WriteAllText("appsettings.json", dataSetting.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }
        
        public List<string> MigrationAvatarUser(string oldUrl, string newUrl)
        {
            try
            {
                List<string> resultDebug = new List<string>();
                List<Company> companies = _unitOfWork.CompanyRepository.GetCompanies();
                foreach (var company in companies)
                {
                    int index = 0;
                    var users = _unitOfWork.AppDbContext.User.Where(m => !m.IsDeleted && m.CompanyId == company.Id && m.Avatar.Contains(oldUrl));
                    foreach (var user in users)
                    {
                        index += 1;
                        user.Avatar = user.Avatar.Replace(oldUrl, newUrl);
                        _unitOfWork.UserRepository.Update(user);
                    }
                    _unitOfWork.Save();
                    resultDebug.Add($"Update avatar company {company.Name} - {company.Code}: {index} users");
                }

                return resultDebug;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MigrationAvatarUser");
                return new List<string>();
            }
        }

        public object MigrationLinkImageEventLog(string oldUrl, string newUrl, string companyCode, DateTime start, DateTime end)
        {
            try
            {
                Dictionary<string, int> result = new Dictionary<string, int>();

                var companies = _unitOfWork.CompanyRepository.GetCompanies();
                if (!string.IsNullOrEmpty(companyCode))
                {
                    companies = companies.Where(m => m.Code == companyCode).ToList();
                }
                foreach (var company in companies)
                {
                    var buildingDefault = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(company.Id);
                    start = start.ConvertToSystemTime(buildingDefault?.TimeZone);
                    end = end.ConvertToSystemTime(buildingDefault?.TimeZone);

                    var data = _unitOfWork.AppDbContext.EventLog.Where(m =>
                        (m.ImageCamera != null || (!string.IsNullOrEmpty(m.ResultCheckIn) && m.ResultCheckIn.Contains(oldUrl)))
                        && start <= m.EventTime && m.EventTime <= end && m.CompanyId == company.Id).ToList();

                    var eventLogs = data.Where(m =>
                        (!string.IsNullOrEmpty(m.ImageCamera) && m.ImageCamera.Contains(oldUrl))
                        || (!string.IsNullOrEmpty(m.ResultCheckIn) && m.ResultCheckIn.Contains(oldUrl))).ToList();

                    int count = eventLogs.Count;
                    int index = 1;

                    foreach (var eventLog in eventLogs)
                    {
                        if(!string.IsNullOrEmpty(eventLog.ImageCamera)) eventLog.ImageCamera = eventLog.ImageCamera.Replace(oldUrl, newUrl);
                        if(!string.IsNullOrEmpty(eventLog.ResultCheckIn)) eventLog.ResultCheckIn = eventLog.ResultCheckIn.Replace(oldUrl, newUrl);
                        _unitOfWork.AppDbContext.EventLog.Update(eventLog);
                        if (index % 500 == 0)
                        {
                            _unitOfWork.Save();
                        }
                        Console.WriteLine($"Company {company.Name}: {index++} / {count}");
                    }

                    _unitOfWork.Save();
                    result.Add(company.Name, count);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in MigrationLinkImageEventLog");
                return new Dictionary<string, int>();
            }
        }
        
        public void VerifyLicenseTesting()
        {
            try
            {
                var data = _unitOfWork.SystemInfoRepository.GetDataLicenseInfo();
                Console.WriteLine("============================================");
                Console.WriteLine("++> Data License <++");
                Console.WriteLine(JsonConvert.SerializeObject(data));
                Console.WriteLine("++> MAC Address <++");
                Console.WriteLine(MachineHelpers.GetLocalMacAddress());
                Console.WriteLine("============================================");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }
    }
}