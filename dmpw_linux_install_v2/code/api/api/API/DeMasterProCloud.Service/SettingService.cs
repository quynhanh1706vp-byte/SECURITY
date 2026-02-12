using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AutoMapper;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Header;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Header;
using DeMasterProCloud.DataModel.Login;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DeMasterProCloud.Service
{
    /// <summary>
    /// Setting service interface
    /// </summary>
    public interface ISettingService
    {
        Setting GetById(int id);
        Setting GetByKey(string key, int companyId);
        string GetLanguage(int companyId);
        string GetLanguage(int accountId, int companyId);
        void Update(SettingModel model);
        void Update(List<SettingModel> models, int companyId);
        LocalMqttSetting GetLocalMqttSetting(int companyId);
        List<FileSetting> GetAll(int companyId);
        string GetValueFromKey(string key, int companyId);
        void Set(int companyId);
        LogoModel GetCurrentLogo(int companyId);
        LogoModel GetCurrentQRLogo(int companyId);

        void UpdateHeaderSetting(List<HeaderData> headerData, int companyId, int accountId);

        List<HeaderData> GetHeaderData<T>();
        List<HeaderData> GetUserHeaderData(string pageName, string[] headerList, int companyId, int accountId);

        List<HeaderData> GetHeaderData<T>(string[] headerList, int companyId);
        LoginSettingModel GetLoginSetting(int companyId);
        void UpdateLoginSetting(LoginSettingModel model, int companyId);
    }

    public class SettingService : ISettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public SettingService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _mapper = MapperInstance.Mapper;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<SettingService>();
        }

        /// <summary>
        /// Get by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Setting GetById(int id)
        {
            return _unitOfWork.SettingRepository.GetById(id);
        }

        /// <summary>
        /// Get by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Setting GetByKey(string key, int companyId)
        {
            return _unitOfWork.SettingRepository.GetByKey(key, companyId);
        }

        public string GetLanguage(int companyId)
        {
            return Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetLanguage(companyId).Value);
        }

        public string GetLanguage(int accountId, int companyId)
        {
            var account = _unitOfWork.AccountRepository.GetById(accountId);
            if (account != null && !string.IsNullOrEmpty(account.Language))
            {
                return account.Language;
            }
            else
            {
                return Helpers.GetStringFromValueSetting(_unitOfWork.SettingRepository.GetLanguage(companyId).Value);
            }
        }

        /// <summary>
        /// Add a setting
        /// </summary>
        /// <param name="model"></param>
        public void Update(SettingModel model)
        {
            var setting = GetById(model.Id);
            var oldValue = setting.Value;
            var newValue = model.Value;
            _mapper.Map(model, setting);
            try
            {
                _unitOfWork.SettingRepository.Update(setting);

                //Save system log
                var content = string.Format(SettingResource.msgChangeSetting, setting.Key,
                    oldValue, newValue);
                _unitOfWork.SystemLogRepository.Add(1, SystemLogType.SystemSetting,
                                ActionLogType.Update, content, null, new List<int> { setting.Id });

                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Update");
            }
        }

        /// <summary>
        /// Update a list of setting
        /// </summary>
        /// <param name="models"></param>
        public void Update(List<SettingModel> models, int companyId)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var strSettings = new List<string>();
                        var ids = new List<int>();

                        var company = _unitOfWork.CompanyRepository.GetById(companyId);
                        var settings = _unitOfWork.SettingRepository.GetByCompanyId(companyId);

                        foreach (var model in models)
                        {
                            //var setting = _unitOfWork.SettingRepository.GetById(model.Id);
                            var setting = settings.FirstOrDefault(s => s.Id == model.Id);

                            if (setting == null)
                            {
                                continue;
                            }

                            if (setting.Key.Equals("work_type_default"))
                            {
                                var oldValues = JsonConvert.DeserializeObject<List<String>>(setting.Value);
                                var toBeDeleted = oldValues.Except(model.Value).Select(m => Int32.Parse(m)).ToList();

                                var oldUsers = _unitOfWork.AppDbContext.User.Where(u => !u.IsDeleted && u.CompanyId == setting.CompanyId && toBeDeleted.Contains((int)u.WorkType));
                                if (oldUsers.Any())
                                {
                                    var typeNames = oldUsers.GroupBy(u => u.WorkType).Select(u => ((WorkType)u.Key).GetDescription()).ToList();
                                    throw new Exception(string.Format(MessageResource.CannotUpdateWorkTypeList, string.Join(", ", typeNames)));
                                }
                            }
                            else if (setting.Key.Equals("army_work_type_default"))
                            {
                                var oldValues = JsonConvert.DeserializeObject<List<String>>(setting.Value);
                                var toBeDeleted = oldValues.Except(model.Value).Select(m => Int32.Parse(m)).ToList();

                                var oldUsers = _unitOfWork.AppDbContext.User.Where(u => !u.IsDeleted && u.CompanyId == setting.CompanyId && toBeDeleted.Contains((int)u.WorkType));
                                if (oldUsers.Any())
                                {
                                    var typeNames = oldUsers.GroupBy(u => u.WorkType).Select(u => ((Army_WorkType)u.Key).GetDescription()).ToList();
                                    throw new Exception(string.Format(MessageResource.CannotUpdateWorkTypeList, string.Join(", ", typeNames)));
                                }
                            }
                            if (setting.Key.Equals("logo"))
                            {
                                // save image logo
                                if (model.Value[0].IsTextBase64())
                                {
                                    string hostApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();

                                    JArray jsonArray = JsonConvert.DeserializeObject<JArray>(setting.Value);
                                    // delete old link
                                    if (!string.IsNullOrEmpty(jsonArray[0].ToString()) && jsonArray[0].ToString().Contains(hostApi))
                                    {
                                        FileHelpers.DeleteFileFromLink(jsonArray[0].ToString().Replace($"{hostApi}/static/", ""));
                                    }

                                    // Use secure file saving to prevent path traversal attacks
                                    string fileName = $"{Constants.Settings.Logo}.jpg";
                                    string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/setting";
                                    bool isSaveImage = FileHelpers.SaveFileImageSecure(model.Value[0], basePath, fileName);
                                    string path = $"{basePath}/{fileName}";
                                    model.Value[0] = $"{hostApi}/static/{path}";
                                }
                            }
                            if (setting.Key.Equals("qr_logo"))
                            {
                                // save image qr logo
                                if (model.Value[0].IsTextBase64())
                                {
                                    string hostApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();

                                    JArray jsonArray = JsonConvert.DeserializeObject<JArray>(setting.Value);
                                    // delete old link
                                    if (!string.IsNullOrEmpty(jsonArray[0].ToString()) && jsonArray[0].ToString().Contains(hostApi))
                                    {
                                        FileHelpers.DeleteFileFromLink(jsonArray[0].ToString().Replace($"{hostApi}/static/", ""));
                                    }

                                    // Use secure file saving to prevent path traversal attacks
                                    string fileName = $"{Constants.Settings.QRLogo}.jpg";
                                    string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/setting";
                                    bool isSaveImage = FileHelpers.SaveFileImageSecure(model.Value[0], basePath, fileName);
                                    string path = $"{basePath}/{fileName}";
                                    model.Value[0] = $"{hostApi}/static/{path}";
                                }
                            }

                            var newValue = JsonConvert.SerializeObject(model.Value);

                            if (setting.Value.Equals(newValue))
                            {

                            }
                            else
                            {
                                setting.Value = newValue;
                                _unitOfWork.SettingRepository.Update(setting);
                                _unitOfWork.Save();

                                strSettings.Add(setting.Key + ":" + setting.Value);
                                ids.Add(setting.Id);
                            }
                        }

                        if (strSettings.Any())
                        {
                            //Save system log
                            var content = SettingResource.msgChangeSettingMultiple;
                            var contentDetails = $"{SettingResource.lblSetting}: {string.Join("\n", strSettings)}";

                            _unitOfWork.SystemLogRepository.Add(1, SystemLogType.SystemSetting,
                                ActionLogType.Update, content, contentDetails, ids);
                        }
                        
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

        public LocalMqttSetting GetLocalMqttSetting(int companyId)
        {
            LocalMqttSetting setting = new LocalMqttSetting();

            try
            {
                setting.Host = GetValueFromKey(Constants.Settings.LocalMqttHost, companyId);
                var portString = GetValueFromKey(Constants.Settings.LocalMqttPort, companyId);
                if (!string.IsNullOrEmpty(portString))
                {
                    int port = 0;
                    if (int.TryParse(portString, out port))
                    {
                        setting.Port = port;
                    }
                }
                setting.UserName = GetValueFromKey(Constants.Settings.LocalMqttUserName, companyId);
                setting.Password = GetValueFromKey(Constants.Settings.LocalMqttPassword, companyId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return setting;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<FileSetting> GetAll(int companyId)
        {
            var settings = _configuration.GetSection(Constants.Settings.FileSettings)?.Get<List<FileSetting>>();
            if (settings != null && settings.Any())
            {
                var settingKeys = settings.Select(m => m.Key).ToList();
                var dbSettings = _unitOfWork.SettingRepository.GetByKeys(settingKeys, companyId);

                var resourceManager = SettingResource.ResourceManager;
                var eventResourceManager = EventLogResource.ResourceManager;
                var userResourceManager = UserResource.ResourceManager;

                if (dbSettings != null && dbSettings.Any())
                {
                    foreach (var setting in settings)
                    {
                        var dbSetting = dbSettings.First(m => m.Key == setting.Key);
                        setting.Id = dbSetting.Id;
                        setting.Category = resourceManager.GetString(setting.Category);
                        setting.Title = resourceManager.GetString(setting.Title);
                        setting.Description = resourceManager.GetString(setting.Description);
                        if (!string.IsNullOrEmpty(dbSetting.Value) && dbSetting.Value != "null")
                            setting.Values = JsonConvert.DeserializeObject<string[]>(dbSetting.Value).Where(m => !string.IsNullOrEmpty(m)).ToArray();
                        
                        if (setting.Options != null && setting.Options.Any())
                        {
                            if (setting.Key.Equals("monitoring_event_type_default"))
                            {
                                foreach (var option in setting.Options)
                                {
                                    option.Key = option.Key;
                                    option.Value = eventResourceManager.GetString(option.Value);
                                }
                            }
                            else if (setting.Key.Equals("work_type_default") || setting.Key.Equals("army_work_type_default"))
                            {
                                foreach (var option in setting.Options)
                                {
                                    option.Key = option.Key;
                                    option.Value = userResourceManager.GetString(option.Value);
                                }
                            }
                            else if (setting.Key.Equals("event_list_push_notification"))
                            {
                                foreach (var option in setting.Options)
                                {
                                    option.Key = option.Key;
                                    option.Value = eventResourceManager.GetString(option.Value);
                                }
                            }
                            else if (setting.Key.Equals("language"))
                            {
                                var listLanguageSetting = _unitOfWork.SettingRepository.GetByKey(Constants.Settings.KeyListLanguageOfCompany, companyId);
                                var listLanguage = JsonConvert.DeserializeObject<List<LanguageModel>>(listLanguageSetting.Value);
                                setting.Options = setting.Options.Where(m => listLanguage.FirstOrDefault(n => n.Tag == m.Key) != null).ToList();
                                foreach (var option in setting.Options)
                                {
                                    option.Key = option.Key;
                                    option.Value = resourceManager.GetString(option.Value);
                                }
                            }
                            else
                            {
                                foreach (var option in setting.Options)
                                {
                                    option.Key = option.Key;
                                    option.Value = resourceManager.GetString(option.Value);
                                }
                            }
                        }
                    }

                    var strPlugIn = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId).PlugIns;
                    settings.Remove(settings.FirstOrDefault(m => m.Key.Equals("army_work_type_default")));
                }
            }
            return settings;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Set(int companyId)
        {
            var settings = _configuration.GetSection(Constants.Settings.FileSettings)?.Get<List<FileSetting>>();
            if (settings != null && settings.Any())
            {
                var dbSettings = _unitOfWork.SettingRepository.GetAll().Select(m => m.Key).ToList();
                if (dbSettings.Any())
                {
                    settings = settings.Where(m => dbSettings.All(x => x != m.Key)).ToList();
                }
                foreach (var unApplySetting in settings)
                {
                    var setting = new Setting
                    {
                        Key = unApplySetting.Key,
                        Value = JsonConvert.SerializeObject(unApplySetting.Values),
                        CompanyId = companyId
                    };
                    _unitOfWork.SettingRepository.Add(setting);
                }
                _unitOfWork.Save();
            }
        }

        /// <summary>
        /// Get value from key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValueFromKey(string key, int companyId)
        {
            var value = "";
            var setting = GetByKey(key, companyId);
            if (setting != null)
            {
                value = Helpers.GetStringFromValueSetting(setting.Value);
            }
            return value;
        }

        /// <summary>
        /// Get current logo
        /// </summary>
        /// <returns></returns>
        public LogoModel GetCurrentLogo(int companyId)
        {
            if (companyId == 0)
            {
                var settings = _configuration.GetSection(Constants.Settings.FileSettings)?.Get<List<FileSetting>>();
                foreach (var unApplySetting in settings)
                {
                    if (unApplySetting.Key == Constants.Settings.Logo)
                    {
                        var logoUnknow = new LogoModel
                        {
                            Logo = unApplySetting.Values.FirstOrDefault()
                        };
                        return logoUnknow;
                    }
                }
            }
            var logo = _unitOfWork.SettingRepository.GetLogo(companyId);
            var logoModel = new LogoModel
            {
                Logo = Helpers.GetStringFromValueSetting(logo?.Value)
            };
            return logoModel;
        }


        /// <summary>
        /// Get current QR logo
        /// </summary>
        /// <returns></returns>
        public LogoModel GetCurrentQRLogo(int companyId)
        {
            if (companyId == 0)
            {
                var settings = _configuration.GetSection(Constants.Settings.FileSettings)?.Get<List<FileSetting>>();
                foreach (var unApplySetting in settings)
                {
                    if (unApplySetting.Key == Constants.Settings.QRLogo)
                    {
                        var logoUnknow = new LogoModel
                        {
                            Logo = unApplySetting.Values.FirstOrDefault()
                        };
                        return logoUnknow;
                    }
                }
            }
            var qrLogo = _unitOfWork.SettingRepository.GetQRLogo(companyId);
            JArray jsonArray = JsonConvert.DeserializeObject<JArray>(qrLogo.Value);

            var logoModel = new LogoModel
            {
                Logo = jsonArray[0].ToString()
            };
            return logoModel;
        }

        public void UpdateHeaderSetting(List<HeaderData> headerData, int companyId, int accountId)
        {
            var pageName = headerData.First().PageName;

            headerData = headerData.Where(h => !string.IsNullOrEmpty(h.HeaderVariable)).ToList();

            var headerSetting = _unitOfWork.DataListSettingRepository.GetHeaderByCompanyAndAccount(companyId, accountId);
            if (headerSetting == null)
            {
                headerSetting = new DataListSetting()
                {
                    AccountId = accountId,
                    CompanyId = companyId,
                    DataType = (int)ListDataType.Header
                };

                _unitOfWork.DataListSettingRepository.Add(headerSetting);
                _unitOfWork.Save();
            }

            if (string.IsNullOrEmpty(headerSetting.DataList))
            {
                int i = 1;

                var headerInfos = new List<HeaderInfo>();
                foreach (var headerInfo in headerData)
                {
                    headerInfos.Add(new HeaderInfo()
                    {
                        HeaderId = headerInfo.HeaderId,
                        HeaderOrder = i++,
                        HeaderVariable = headerInfo.HeaderVariable,
                        IsVisible = true
                    });
                }

                var headerValue = new List<HeaderSettingModel>()
                {
                    new HeaderSettingModel()
                    {
                        PageName = pageName,
                        Headers = headerInfos
                    }
                };

                headerSetting.DataList = JsonConvert.SerializeObject(headerValue);

                _unitOfWork.DataListSettingRepository.Update(headerSetting);
                _unitOfWork.Save();
            }

            var headerListInDb = headerSetting.DataList;
            var headerSettingModels = JsonConvert.DeserializeObject<List<HeaderSettingModel>>(headerListInDb);
            var pageHeaders = headerSettingModels.FirstOrDefault(m => m.PageName == pageName);

            if (pageHeaders == null)
            {
                int i = 1;

                var headerInfos = new List<HeaderInfo>();
                foreach (var headerInfo in headerData)
                {
                    headerInfos.Add(new HeaderInfo()
                    {
                        HeaderId = headerInfo.HeaderId,
                        HeaderOrder = i++,
                        HeaderVariable = headerInfo.HeaderVariable,
                        IsVisible = true
                    });
                }

                pageHeaders = new HeaderSettingModel()
                {
                    PageName = pageName,
                    Headers = headerInfos
                };

                headerSettingModels.Add(pageHeaders);
                headerSetting.DataList = JsonConvert.SerializeObject(headerSettingModels);

                _unitOfWork.DataListSettingRepository.Update(headerSetting);
                _unitOfWork.Save();
            }
            else
            {
                var deleteFromDB = pageHeaders.Headers.Select(m => m.HeaderVariable).Except(headerData.Select(m => m.HeaderVariable));
                var addToDB = headerData.Select(m => m.HeaderVariable).Except(pageHeaders.Headers.Select(m => m.HeaderVariable));

                if (deleteFromDB.Any() || addToDB.Any())
                {
                    headerSettingModels.Remove(pageHeaders);

                    if (deleteFromDB.Any())
                        pageHeaders.Headers = pageHeaders.Headers.Where(m => !deleteFromDB.Contains(m.HeaderVariable)).ToList();

                    if (addToDB.Any())
                    {
                        foreach (var newHeader in addToDB)
                        {
                            var header = headerData.Find(m => m.HeaderVariable.Equals(newHeader));
                            pageHeaders.Headers.Add(new HeaderInfo()
                            {
                                HeaderId = header.HeaderId,
                                HeaderVariable = header.HeaderVariable,
                                IsVisible = header.IsVisible,
                                HeaderOrder = pageHeaders.Headers.Max(m => m.HeaderOrder) + 1
                            });
                        }
                    }

                    headerSettingModels.Add(pageHeaders);
                    headerSetting.DataList = JsonConvert.SerializeObject(headerSettingModels);

                    _unitOfWork.DataListSettingRepository.Update(headerSetting);
                    _unitOfWork.Save();
                }

                var diffIdList = headerData.Select(m => m.HeaderId).Except(pageHeaders.Headers.Select(m => m.HeaderId));
                if (diffIdList.Any())
                {
                    headerSettingModels.Remove(pageHeaders);

                    foreach (var diff in diffIdList)
                    {
                        var header = headerData.Find(h => h.HeaderId == diff);
                        var headerInDb = pageHeaders.Headers.FirstOrDefault(m => m.HeaderVariable.Equals(header.HeaderVariable));
                        if (headerInDb != null)
                            headerInDb.HeaderId = header.HeaderId;
                    }

                    headerSettingModels.Add(pageHeaders);
                    headerSetting.DataList = JsonConvert.SerializeObject(headerSettingModels);

                    _unitOfWork.DataListSettingRepository.Update(headerSetting);
                    _unitOfWork.Save();
                }
            }

            headerSettingModels = headerSettingModels.Where(m => !string.IsNullOrEmpty(m.PageName)).ToList();

            foreach (var headerInfo in headerData)
            {
                pageHeaders = headerSettingModels.FirstOrDefault(m => m.PageName.Equals(headerInfo.PageName));

                if (pageHeaders != null)
                {
                    var pageHeader = pageHeaders.Headers.FirstOrDefault(m => m.HeaderId.Equals(headerInfo.HeaderId));

                    if (headerInfo.IsCategory)
                        pageHeader = pageHeaders.Headers.FirstOrDefault(m => m.HeaderVariable.Equals(headerInfo.HeaderVariable));

                    if (pageHeader != null)
                    {
                        pageHeader.IsVisible = headerInfo.IsVisible;

                        var preHeader = pageHeaders.Headers.FirstOrDefault(m => m.HeaderOrder.Equals(headerInfo.HeaderOrder));
                        if (preHeader != null)
                            preHeader.HeaderOrder = pageHeader.HeaderOrder;

                        pageHeader.HeaderOrder = headerInfo.HeaderOrder;
                    }
                }
            }

            headerSetting.DataList = JsonConvert.SerializeObject(headerSettingModels);

            _unitOfWork.DataListSettingRepository.Update(headerSetting);
            _unitOfWork.Save();
        }


        public List<HeaderData> GetHeaderData<T>()
        {
            List<HeaderData> headers = new List<HeaderData>();

            foreach (var column in ColumnDefines.UserHeader)
            {
                foreach (var element in Enum.GetValues(typeof(T)))
                {
                    if (column.Equals(element.GetName()))
                    {
                        HeaderData header = new HeaderData()
                        {
                            HeaderId = (int)element,
                            HeaderName = element.GetDescription(),
                            HeaderVariable = element.GetName(),
                            IsCategory = false
                        };

                        headers.Add(header);
                    }
                }
            }

            return headers;
        }


        /// <summary>
        /// Create header data about user information.
        /// </summary>
        /// <param name="pageName"> page name(string) data </param>
        /// <param name="headerNameList"> string array about header data </param>
        /// <param name="companyId"> index of company </param>
        /// <param name="accountId"> index of account </param>
        /// <returns></returns>
        public List<HeaderData> GetUserHeaderData(string pageName, string[] headerNameList, int companyId, int accountId)
        {
            List<HeaderData> headers = new List<HeaderData>();

            if (companyId == 0)
            {
                return headers;
            }

            var strPlugIn = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId).PlugIns;
            var plugIns = JsonConvert.DeserializeObject<PlugIns>(strPlugIn);

            int i = 0;

            foreach (var column in headerNameList)
            {
                object item = Array.Find((UserHeaderColumns[])Enum.GetValues(typeof(UserHeaderColumns)), m => m.GetName().Equals(column));

                if (item != null && column.Equals(item.GetName()))
                {
                    // Check vehicle plugin
                    if ((int)item == (int)UserHeaderColumns.PlateNumberList && !plugIns.VehiclePlugIn)
                    {
                        continue;
                    }

                    // Check army plugin (MilitaryNo / EmployeeNo)
                    if ((int)item == (int)UserHeaderColumns.MilitaryNo)
                    {
                        continue;
                    }

                    // Check Access Approval
                    if ((int)item == (int)UserHeaderColumns.ApprovalStatus)
                    {
                        var accessSetting = _unitOfWork.AppDbContext.AccessSetting.Where(m => m.CompanyId == companyId).FirstOrDefault();
                        // don't show approval status when not using approval setting
                        if (accessSetting.ApprovalStepNumber == (short)VisitSettingType.NoStep)
                            continue;
                    }

                    HeaderData header = new HeaderData()
                    {
                        PageName = pageName,
                        HeaderId = (int)item,
                        HeaderOrder = i++,
                        HeaderName = item.GetDescription(),
                        HeaderVariable = item.GetName(),
                        IsCategory = false
                    };

                    headers.Add(header);
                }
            }

            // check headerSetting
            headers = CheckHeaderSetting(pageName, headers, companyId, accountId);

            return headers;
        }

        /// <summary>
        /// This function is to compare the header data with headerSetting in DB data.
        /// </summary>
        /// <param name="pageName"> page name (string) data </param>
        /// <param name="headers"> List of header data that is defined in source code. (default header data) </param>
        /// <param name="companyId"> index of company </param>
        /// <param name="accountId"> index of account </param>
        /// <returns></returns>
        private List<HeaderData> CheckHeaderSetting(string pageName, List<HeaderData> headers, int companyId, int accountId)
        {
            try
            {
                var headerSetting = _unitOfWork.HeaderRepository.GetByCompanyAndAccount(companyId, accountId);
                if (headerSetting == null)
                {
                    return headers;
                }

                if (string.IsNullOrEmpty(headerSetting.HeaderList))
                {
                    return headers;
                }

                var headerListInDb = headerSetting.HeaderList;
                var headerSettingModels = JsonConvert.DeserializeObject<List<HeaderSettingModel>>(headerListInDb);
                var pageHeaders = headerSettingModels.FirstOrDefault(m => m.PageName == pageName);

                if (pageHeaders == null)
                {
                    return headers;
                }

                var headerInfoList = pageHeaders.Headers;

                foreach (var headerInfo in headerInfoList)
                {
                    var header = headers.FirstOrDefault(m => m.HeaderVariable == headerInfo.HeaderVariable);

                    if (header != null)
                    {
                        header.HeaderOrder = headerInfo.HeaderOrder;
                        header.IsVisible = headerInfo.IsVisible;
                    }
                }

                headers = headers.OrderBy(m => m.HeaderOrder).ToList();

                return headers;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Header Setting : {e.Message}");
                Console.WriteLine($"[ERROR] Header Setting : {e.StackTrace}");
                Console.WriteLine($"[ERROR] Header Setting : {e.InnerException?.Message}");

                return headers;
            }
        }


        public List<HeaderData> GetHeaderData<T>(string[] headerList, int companyId)
        {
            List<HeaderData> headers = new List<HeaderData>();

            foreach (var column in headerList)
            {
                object test = Array.Find((T[])Enum.GetValues(typeof(T)), m => m.GetName().Equals(column));

                foreach (var element in Enum.GetValues(typeof(T)))
                {
                    if (column.Equals(element.GetName()))
                    {
                        HeaderData header = new HeaderData()
                        {
                            HeaderId = (int)element,
                            HeaderName = element.GetDescription(),
                            HeaderVariable = element.GetName(),
                            IsCategory = false
                        };

                        headers.Add(header);
                    }
                }
            }

            CompanyService companyService = new CompanyService(_unitOfWork, _configuration, null, null, new HttpContextAccessor(), null, null);
            if (companyService.CheckPluginByCompany(companyId, Constants.PlugIn.VehiclePlugIn))
            {
                HeaderData header = new HeaderData()
                {
                    HeaderId = (int)UserArmyHeaderColumn.PlateNumberList,
                    HeaderName = UserArmyHeaderColumn.PlateNumberList.GetDescription(),
                    HeaderVariable = UserArmyHeaderColumn.PlateNumberList.GetName(),
                    IsCategory = false
                };

                headers.Add(header);
            }

            return headers;
        }
        
        public LoginSettingModel GetLoginSetting(int companyId)
        {
            return _unitOfWork.SettingRepository.GetLoginSetting(companyId);
        }

        public void UpdateLoginSetting(LoginSettingModel model, int companyId)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var settingsToUpdate = new List<(string Key, string Value)>();

                        settingsToUpdate.Add((Constants.Settings.KeyChangeInFirstTime, JsonConvert.SerializeObject(new[] { model.ChangeInFirstTime.ToString().ToLower() })));
                        settingsToUpdate.Add((Constants.Settings.KeyHaveUpperCase, JsonConvert.SerializeObject(new[] { model.HaveUpperCase.ToString().ToLower() })));
                        settingsToUpdate.Add((Constants.Settings.KeyHaveNumber, JsonConvert.SerializeObject(new[] { model.HaveNumber.ToString().ToLower() })));
                        settingsToUpdate.Add((Constants.Settings.KeyHaveSpecial, JsonConvert.SerializeObject(new[] { model.HaveSpecial.ToString().ToLower() })));
                        settingsToUpdate.Add((Constants.Settings.KeyTimeNeedToChange, JsonConvert.SerializeObject(new[] { model.TimeNeedToChange.ToString() })));
                        settingsToUpdate.Add((Constants.Settings.KeyMaximumTimeUsePassword, JsonConvert.SerializeObject(new[] { model.MaximumTimeUsePassword.ToString() })));
                        settingsToUpdate.Add((Constants.Settings.KeyMaximumEnterWrongPassword, JsonConvert.SerializeObject(new[] { model.MaximumEnterWrongPassword.ToString() })));
                        settingsToUpdate.Add((Constants.Settings.KeyTimeoutWhenWrongPassword, JsonConvert.SerializeObject(new[] { model.TimeoutWhenWrongPassword.ToString() })));

                        foreach (var (Key, Value) in settingsToUpdate)
                        {
                            var setting = _unitOfWork.SettingRepository.GetByKey(Key, companyId);
                            if (setting == null)
                            {
                                continue;
                            }
                            setting.Value = Value;
                            _unitOfWork.SettingRepository.Update(setting);
                        }

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }
    }
}
