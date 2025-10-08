using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Role;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DeMasterProCloud.DataAccess.Migrations;

namespace DeMasterProCloud.Repository
{
    public interface IDataListSettingRepository : IGenericRepository<DataListSetting>
    {
        DataListSetting GetHeaderByCompanyAndAccount(int companyId, int accountId);
        DataListSetting GetMonitoringDoorListSetting(int companyId, int accountId);

        DataListSetting GetMonitoringEventListSetting(int companyId, int accountId);

        DataListSetting GetBuildingListSetting(int companyId, int accountId);
    }

    public class DataListSettingRepository : GenericRepository<DataListSetting>, IDataListSettingRepository
    {
        private readonly AppDbContext _dbContext;

        public DataListSettingRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
        }

        private DataListSetting GetDataLIstSettingByType(int companyId, int accountId, int type)
        {
            var setting = _dbContext.DataListSetting.FirstOrDefault(m => m.CompanyId == companyId && m.AccountId == accountId && m.DataType == type);
            return setting;
        }

        /// <summary>
        /// Get Header setting value.
        /// </summary>
        /// <param name="companyId"> An identifier of company </param>
        /// <param name="accountId"> An identifier of account </param>
        /// <returns></returns>
        public DataListSetting GetHeaderByCompanyAndAccount(int companyId, int accountId)
        {
            //var headerSetting = _dbContext.DataListSetting.FirstOrDefault(m => m.CompanyId == companyId && m.AccountId == accountId && m.DataType == (int)ListDataType.Header);
            var headerSetting = GetDataLIstSettingByType(companyId, accountId, (int)ListDataType.Header);
            return headerSetting;
        }

        /// <summary>
        /// Get DoorList setting value (in Monitoring page).
        /// </summary>
        /// <param name="companyId"> An identifier of company </param>
        /// <param name="accountId"> An identifier of account </param>
        /// <returns></returns>
        public DataListSetting GetMonitoringDoorListSetting(int companyId, int accountId)
        {
            var doorListSetting = GetDataLIstSettingByType(companyId, accountId, (int)ListDataType.DoorListSettings);
            return doorListSetting;
        }

        /// <summary>
        /// Get EventList setting value (in Monitoring page).
        /// </summary>
        /// <param name="companyId"> An identifier of company </param>
        /// <param name="accountId"> An identifier of account </param>
        /// <returns></returns>
        public DataListSetting GetMonitoringEventListSetting(int companyId, int accountId)
        {
            var eventListSetting = GetDataLIstSettingByType(companyId, accountId, (int)ListDataType.MonitoringEvents);
            return eventListSetting;
        }


        /// <summary>
        /// Get BuildingList setting value (in Entry page).
        /// </summary>
        /// <param name="companyId"> An identifier of company </param>
        /// <param name="accountId"> An identifier of account </param>
        /// <returns></returns>
        public DataListSetting GetBuildingListSetting(int companyId, int accountId)
        {
            var buildingListSetting = GetDataLIstSettingByType(companyId, accountId, (int)ListDataType.BuildingListSettings);
            return buildingListSetting;
        }
    }
}