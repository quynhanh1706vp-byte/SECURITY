using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// UnregistedDevices repository interface
    /// </summary>
    public interface IUnregistedDeviceRepository : IGenericRepository<UnregistedDevice>
    {
        IQueryable<UnregistedDevice> GetByCompanyId(int companyId);
        UnregistedDevice GetByDeviceAddress(string deviceAddress);
        UnregistedDevice GetByMacAddress(string macAddr);
        UnregistedDevice GetByIdAndCompanyId(int companyId, int id);
    }

    /// <summary>
    /// UnregistedDevices repository
    /// </summary>
    public class UnregistedDeviceRepository : GenericRepository<UnregistedDevice>, IUnregistedDeviceRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        public UnregistedDeviceRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<UnregistedDeviceRepository>();
        }

        /// <summary>
        /// Get all unregisted devices by companyId
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IQueryable<UnregistedDevice> GetByCompanyId(int companyId)
        {
            try
            {
                return _dbContext.UnregistedDevice.Where(x => x.CompanyId == companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCompanyId");
                return Enumerable.Empty<UnregistedDevice>().AsQueryable();
            }
        }

        /// <summary>
        /// Get by device address and companyId
        /// </summary>
        /// <param name="deviceAddress"></param>
        /// <returns></returns>
        public UnregistedDevice GetByDeviceAddress(string deviceAddress)
        {
            try
            {
                return _dbContext.UnregistedDevice.FirstOrDefault(x =>
                    x.DeviceAddress == deviceAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByDeviceAddress");
                return null;
            }
        }

        /// <summary>
        /// Get by Mac address
        /// </summary>
        /// <param name="macAddress"></param>
        /// <returns></returns>
        public UnregistedDevice GetByMacAddress(string macAddr)
        {
            try
            {
                return _dbContext.UnregistedDevice.FirstOrDefault(x =>
                    x.MacAddress == macAddr);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByMacAddress");
                return null;
            }
        }

        /// <summary>
        /// Get by id and companyId
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public UnregistedDevice GetByIdAndCompanyId(int companyId, int id)
        {
            try
            {
                return _dbContext.UnregistedDevice.FirstOrDefault(x =>
                    x.Id == id && x.CompanyId == companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdAndCompanyId");
                return null;
            }
        }
    }
}
