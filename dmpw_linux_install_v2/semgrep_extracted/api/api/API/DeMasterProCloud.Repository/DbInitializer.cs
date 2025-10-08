using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using DeMasterProCloud.Common.Resources;
using Microsoft.Extensions.Localization;
using System.Threading;
using System.Globalization;
using DeMasterProCloud.DataModel.Setting;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// the class to seed date to database server when application start up.
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            //Database migrate
            unitOfWork.AppDbContext.Database.Migrate();

            // Add/Update a event list
            bool updateEvents;
            try
            {
                updateEvents = configuration.GetSection("UpdateEventList").Get<bool>();
            }
            catch (Exception e)
            {
                var events = unitOfWork.EventRepository.GetAll();
                if(!events.Any())
                    updateEvents = true;
                else
                    updateEvents = false;
            }

            if (updateEvents)
                unitOfWork.EventRepository.AddDefaultEventList();
            else
            {
                var events = unitOfWork.EventRepository.GetAll();
                if (!events.Any() || events.Count()/4 != EnumHelper.ToEnumList<EventType>().Count)
                {
                    unitOfWork.EventRepository.AddDefaultEventList();
                }
            }

            // Temporary
            // Create Default access setting for existing companies.
            var companies = unitOfWork.AppDbContext.Company.Include(m => m.AccessSetting).Where(m => !m.IsDeleted);
            foreach(var company in companies)
            {
                if(company.AccessSetting == null)
                {
                    unitOfWork.UserRepository.CreateDefaultAccessSetting(company.Id);
                }
            }

            // Temporary
            // Not used PAG should be deleted (IsDelteted = true)
            // deleted users
            var noUsePAGs = unitOfWork.AppDbContext.User
                                                .Include(u => u.AccessGroup)
                                                .Where(u => u.IsDeleted && u.AccessGroup.Type == (short)AccessGroupType.PersonalAccess && !u.AccessGroup.IsDeleted)
                                                .Select(u => u.AccessGroup)
                                                .ToList();
            // Remove not used AGs
            if(noUsePAGs != null && noUsePAGs.Count != 0)
            {
                unitOfWork.AccessGroupRepository.DeleteRangeFromSystem(noUsePAGs);
                foreach(var noUserPAG in noUsePAGs)
                {
                    // Delete AGDs.
                    var agDevices = unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(noUserPAG.CompanyId, noUserPAG.Id, null, false);
                    unitOfWork.AccessGroupDeviceRepository.DeleteRange(agDevices);
                }
            }

            // Temporary
            var pmsCompanies = unitOfWork.AppDbContext.Company.Include(m => m.DynamicRole).Where(m => !m.IsDeleted);
            foreach(var company in pmsCompanies)
            {
                if(!company.DynamicRole.Any(role => role.RoleSettingDefault))
                {
                    // Set default 
                    var empRole = company.DynamicRole.FirstOrDefault(m => m.TypeId == (int)AccountType.Employee);
                    if (empRole != null) empRole.RoleSettingDefault = true;

                    unitOfWork.RoleRepository.Update(empRole);
                }
            }

            // If there is not default accessGroup
            var agCompanies = unitOfWork.AppDbContext.Company.Include(m => m.AccessGroup).Where(m => !m.IsDeleted);
            foreach (var company in agCompanies)
            {
                if (!company.AccessGroup.Any(ag => ag.IsDefault))
                {
                    // Set default 
                    var fullAG = company.AccessGroup.FirstOrDefault(m => m.Type == (int)AccessGroupType.FullAccess);
                    if (fullAG != null)
                    {
                        fullAG.IsDefault = true;
                        unitOfWork.AccessGroupRepository.Update(fullAG);

                        // Get company setting for language.
                        string language = Helpers.GetStringFromValueSetting(unitOfWork.SettingRepository.GetLanguage(company.Id).Value);
                        var originCulture = Thread.CurrentThread.CurrentCulture;
                        CultureInfo newCulture = new CultureInfo(language);
                        CultureInfo.CurrentCulture = newCulture;
                        CultureInfo.CurrentUICulture = newCulture;

                        var contents = AccessGroupResource.msgNoDefaultAG;
                        var contentsDetail = $"{AccessGroupResource.lblDefaultAccessGroup} : {fullAG.Name}";
                        
                        unitOfWork.SystemLogRepository.Add(fullAG.Id, SystemLogType.AccessGroup,
                                    ActionLogType.Update, contents, contentsDetail, null, company.Id);

                        CultureInfo.CurrentCulture = originCulture;
                        CultureInfo.CurrentUICulture = originCulture;
                    }
                }
            }

            //var mastersByCompany = unitOfWork.UserRepository.GetAllUserInSystemExceptInvalid().Where(m => m.IsMasterCard).GroupBy(m => m.CompanyId);

            //if (mastersByCompany.Any())
            //{
            //    // Update master -> building master
            //    foreach(var masters in mastersByCompany)
            //    {
            //        foreach (var master in masters)
            //        {
            //            // Get all buildings in the company.
            //            var buildings = unitOfWork.BuildingRepository.GetByCompanyId(masters.Key).Include(m => m.BuildingMaster);
            //            foreach (var building in buildings)
            //            {
            //                var buildingMaster = building.BuildingMaster.FirstOrDefault(m => m.UserId == master.Id);

            //                if (buildingMaster == null)
            //                {
            //                    buildingMaster = new BuildingMaster()
            //                    {
            //                        BuildingId = building.Id,
            //                        UserId = master.Id
            //                    };

            //                    unitOfWork.AppDbContext.BuildingMaster.Add(buildingMaster);
            //                }
            //            }

            //            master.IsMasterCard = false;
            //            unitOfWork.UserRepository.Update(master);
            //        }

            //    }
            //}

            // Temporary
            // Update HeaderSetting -> DataListSetting
            // Set old value's type to 1(header data).
            var oldHeaderSettings = unitOfWork.AppDbContext.HeaderSetting.ToList();
            foreach (var oldHeaderSetting in oldHeaderSettings)
            {
                var oldDataListSetting = unitOfWork.AppDbContext.DataListSetting.FirstOrDefault(d => d.DataType == (int)ListDataType.Header && d.AccountId == oldHeaderSetting.AccountId);
                if(oldDataListSetting == null)
                {
                    var dataListSetting = new DataListSetting()
                    {
                        Id = oldHeaderSetting.Id,
                        AccountId = oldHeaderSetting.AccountId,
                        CompanyId = oldHeaderSetting.CompanyId,
                        DataList = oldHeaderSetting.HeaderList,
                        DataType = (int)ListDataType.Header,
                        Name = oldHeaderSetting.Name,
                    };

                    unitOfWork.DataListSettingRepository.Add(dataListSetting);
                }
            }

            unitOfWork.Save();

            // Temporary
            // Create default login settings for existing companies
            var allCompanies = unitOfWork.AppDbContext.Company.Where(m => !m.IsDeleted);
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

            var configSettings = configuration.GetSection(Constants.Settings.FileSettings)?.Get<List<FileSetting>>();

            foreach (var company in allCompanies)
            {
                foreach (var settingKey in loginSettingKeys)
                {
                    var existingSetting = unitOfWork.SettingRepository.GetByKey(settingKey, company.Id);
                    if (existingSetting == null && configSettings != null)
                    {
                        var defaultSetting = configSettings.FirstOrDefault(s => s.Key == settingKey);
                        if (defaultSetting != null)
                        {
                            var setting = new Setting
                            {
                                Key = defaultSetting.Key,
                                Value = JsonConvert.SerializeObject(defaultSetting.Values),
                                CompanyId = company.Id
                            };
                            unitOfWork.SettingRepository.Add(setting);
                        }
                    }
                }
            }

            unitOfWork.Save();

            // Temporary
            // Initialize default LoginConfig for existing accounts
            var accountsWithoutLoginConfig = unitOfWork.AppDbContext.Account
                .Where(a => !a.IsDeleted && string.IsNullOrEmpty(a.LoginConfig));

            foreach (var account in accountsWithoutLoginConfig)
            {
                var loginConfig = new DeMasterProCloud.DataModel.Account.LoginConfigModel
                {
                    IsFirstLogin = false, // Existing accounts are not first login
                    TimeChangedPassWord = account.UpdatePasswordOn != DateTime.MinValue ? account.UpdatePasswordOn : DateTime.UtcNow,
                    LoginFailCount = 0,
                    LastTimeLoginFail = DateTime.MinValue
                };

                account.LoginConfig = JsonConvert.SerializeObject(loginConfig);
                unitOfWork.AccountRepository.Update(account);
            }

            unitOfWork.Save();

            // Look for any company.
            if (!unitOfWork.AppDbContext.Company.Any())
            {
                // Add a system admin as a default account.
                unitOfWork.AccountRepository.AddDefaultAccount(
                    configuration[Constants.Settings.DefaultAccountUsername],
                    configuration[Constants.Settings.DefaultAccountPassword]);



                unitOfWork.Save();
            }
        }
    }
}
