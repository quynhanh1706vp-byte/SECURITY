using System;
using Microsoft.Extensions.Logging;
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
    public interface IHeaderRepository : IGenericRepository<HeaderSetting>
    {
        HeaderSetting GetByCompanyAndAccount(int companyId, int accountId);
    }

    public class HeaderRepository : GenericRepository<HeaderSetting>, IHeaderRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;

        public HeaderRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<HeaderRepository>();
        }

        public HeaderSetting GetByCompanyAndAccount(int companyId, int accountId)
        {
            try
            {
                var headerSetting = _dbContext.HeaderSetting.FirstOrDefault(m => m.CompanyId == companyId && m.AccountId == accountId);

                return headerSetting;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCompanyAndAccount");
                return null;
            }
        }
    }
}