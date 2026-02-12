using System;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Service
{
    /// <summary>
    /// AccessLevel service interface
    /// </summary>
    public interface IAccessGroupDeviceService
    {
        IQueryable<AccessGroupDevice> GetByAccessGroupId(int accessGroupId);
        IQueryable<AccessGroupDevice> GetUnAssignAccessGroupId(int accessGroupId);
        List<AccessGroupDevice> GetByTimezoneId(int timezoneId);
    }

    public class AccessGroupDeviceService : IAccessGroupDeviceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly ILogger _logger;

        public AccessGroupDeviceService(IUnitOfWork unitOfWork, IHttpContextAccessor contextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContext = contextAccessor.HttpContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<AccessGroupDeviceService>();
        }

        /// <summary>
        /// Get by access group id
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <returns></returns>
        public IQueryable<AccessGroupDevice> GetByAccessGroupId(int accessGroupId)
        {
            try
            {
                return _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(_httpContext.User.GetCompanyId(), accessGroupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByAccessGroupId");
                return Enumerable.Empty<AccessGroupDevice>().AsQueryable();
            }
        }

        /// <summary>
        /// Get unassign access group id
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <returns></returns>
        public IQueryable<AccessGroupDevice> GetUnAssignAccessGroupId(int accessGroupId)
        {
            try
            {
                return _unitOfWork.AccessGroupDeviceRepository.GetByUnAssignAccessGroupId(_httpContext.User.GetCompanyId(), accessGroupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUnAssignAccessGroupId");
                return Enumerable.Empty<AccessGroupDevice>().AsQueryable();
            }
        }

        /// <summary>
        /// Get by timezone id
        /// </summary>
        /// <param name="timezoneId"></param>
        /// <returns></returns>
        public List<AccessGroupDevice> GetByTimezoneId(int timezoneId)
        {
            try
            {
                return _unitOfWork.AccessGroupDeviceRepository.GetByTimezoneId(_httpContext.User.GetCompanyId(),
                    timezoneId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByTimezoneId");
                return new List<AccessGroupDevice>();
            }
        }
    }
}
