using DeMasterProCloud.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using DeMasterProCloud.Common.Infrastructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeMasterProCloud.Repository
{
    public interface IFirmwareVersionRepository : IGenericRepository<FirmwareVersion>
    {
        IQueryable<FirmwareVersion> Gets();
    }

    public class FirmwareVersionRepository : GenericRepository<FirmwareVersion>, IFirmwareVersionRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;

        public FirmwareVersionRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<FirmwareVersionRepository>();
        }

        public IQueryable<FirmwareVersion> Gets()
        {
            try
            {
                return _dbContext.FirmwareVersion;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Gets");
                return Enumerable.Empty<FirmwareVersion>().AsQueryable();
            }
        }
    }
}
