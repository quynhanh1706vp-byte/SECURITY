using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Web;
using Newtonsoft.Json;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.SystemLog;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// Interface for SystemLog repository
    /// </summary>
    public interface ISystemLogRepository : IGenericRepository<SystemLog>
    {
        void Add(int logObjId, SystemLogType sysType, ActionLogType type, string content = null,
            string contentDetails = null, List<int> assignedIds = null, int? companyId = null, int? createdBy = null);
    }

    /// <summary>
    /// SystemLog repository
    /// </summary>
    public class SystemLogRepository : GenericRepository<SystemLog>, ISystemLogRepository
    {
        private readonly HttpContext _httpContext;
        private readonly AppDbContext _dbContext;

        public SystemLogRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext,
            contextAccessor)
        {
            if(contextAccessor != null)
                _httpContext = contextAccessor.HttpContext;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Add system log
        /// </summary>
        /// <param name="logObjId"></param>
        /// <param name="sysType"></param>
        /// <param name="type"></param>
        /// <param name="content"></param>
        /// <param name="assignedIds"></param>
        /// <param name="companyId"></param>
        /// <param name="createdBy"></param>
        /// <param name="contentDetails"></param>
        public void Add(int logObjId, SystemLogType sysType, ActionLogType type, string content = null,
            string contentDetails = null, List<int> assignedIds = null, int? companyId = null, int? createdBy = null)
        {

            if (createdBy == null)
            {
                // If this log in not created by user then assign it to system admin
                if (_httpContext != null) 
                    createdBy = _httpContext.User.GetAccountId();
                if (createdBy == null || createdBy == 0)
                {
                    createdBy = 1;
                }
            }
            
            var systemLog = new SystemLog
            {
                CompanyId = companyId == 0 ? null : companyId,
                OpeTime = DateTime.UtcNow,
                Type = (short)sysType,
                Action = (short)type,
                Content = !string.IsNullOrEmpty(content) ? HttpUtility.HtmlEncode(content) : null,
                ContentDetails = !string.IsNullOrEmpty(contentDetails) ? HttpUtility.HtmlEncode(contentDetails) : null,
                CreatedBy = createdBy ?? _httpContext.User.GetAccountId(),

            };
            var systemLogIdContent = new SystemLogIdContent
            {
                Id = logObjId,
                AssignedIds = assignedIds
            };
            systemLog.ContentIds = JsonConvert.SerializeObject(systemLogIdContent);
            Add(systemLog);
            //_dbContext.SystemLog.Add(systemLog);
            //_dbContext.SaveChanges();
        }
    }
}