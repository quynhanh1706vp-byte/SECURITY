using System;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.Repository;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Service
{
    public interface IShortenLinkService
    {
        string GetFullPathByShortPath(string shortPath);
    }
    
    public class ShortenLinkService : IShortenLinkService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger _logger;

        public ShortenLinkService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<ShortenLinkService>();
        }

        public string GetFullPathByShortPath(string shortPath)
        {
            try
            {
                return _unitOfWork.ShortenLinkRepository.GetFullPathByShortPath(shortPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetFullPathByShortPath");
                return null;
            }
        }
    }
}