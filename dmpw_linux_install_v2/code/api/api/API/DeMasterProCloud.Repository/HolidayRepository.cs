using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// Interface for Holiday repository
    /// </summary>
    public interface IHolidayRepository : IGenericRepository<Holiday>
    {
        List<Holiday> GetHolidayByCompany(int companyId);
        Holiday GetHolidayByIdAndCompany(int companyId, int holidayId);
        Holiday GetHolidayByNameAndCompany(int companyId, string name);
        int GetHolidayCount(int companyId);
    }

    /// <summary>
    /// Holiday repository
    /// </summary>
    public class HolidayRepository : GenericRepository<Holiday>, IHolidayRepository
    {
        private readonly ILogger _logger;
        public HolidayRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<HolidayRepository>();
        }

        /// <summary>
        /// Get holidays by company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<Holiday> GetHolidayByCompany(int companyId)
        {
            try
            {
                if(companyId != 0)
                {
                    return GetMany(c => c.CompanyId == companyId && !c.IsDeleted).ToList();
                }
                else
                {
                    return GetMany(c => !c.IsDeleted).ToList();
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetHolidayByCompany");
                return new List<Holiday>();
            }
        }

        /// <summary>
        /// Get number of holiday in a company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public int GetHolidayCount(int companyId)
        {
            try
            {
                return GetMany(c => c.CompanyId == companyId && !c.IsDeleted)
                    .Select(c => c.Id)
                    .Count();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetHolidayCount");
                return 0;
            }
        }

        /// <summary>
        /// Get holiday by id and company
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="holidayId"></param>
        /// <returns></returns>
        public Holiday GetHolidayByIdAndCompany(int companyId, int holidayId)
        {
            try
            {
                return Get(c => c.CompanyId == companyId && c.Id == holidayId
                && !c.IsDeleted);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetHolidayByIdAndCompany");
                return null;
            }
        }

        /// <summary>
        /// Get holiday by name and company
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Holiday GetHolidayByNameAndCompany(int companyId, string name)
        {
            try
            {
                return Get(c => c.CompanyId == companyId && c.Name.ToLower() == name.ToLower()
                        && !c.IsDeleted);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in GetHolidayByNameAndCompany");
                return null;
            }
        }
    }
}