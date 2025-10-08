using System;
using Microsoft.Extensions.Logging;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Repository
{
    public interface IShortenLinkRepository : IGenericRepository<ShortenLink>
    {
        string GetShortPathByFullPath(string fullPath);
        string GetFullPathByShortPath(string shortPath);
        string GenerateShortLink(string locationOrigin, string fullPath);
    }
    
    public class ShortenLinkRepository : GenericRepository<ShortenLink>, IShortenLinkRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        
        public ShortenLinkRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<ShortenLinkRepository>();
        }

        public string GetShortPathByFullPath(string fullPath)
        {
            var link = _dbContext.ShortenLink.FirstOrDefault(m => m.FullPath == fullPath);
            return link?.ShortPath;
        }

        public string GetFullPathByShortPath(string shortPath)
        {
            var link = _dbContext.ShortenLink.FirstOrDefault(m => m.ShortPath == shortPath);
            return link?.FullPath;
        }
        
        public string GenerateShortLink(string locationOrigin, string fullPath)
        {
            string shortPath = GetShortPathByFullPath(fullPath);
            Random random = new Random();
            while (string.IsNullOrEmpty(shortPath))
            {
                shortPath = "";
                for (int i = 0; i < Constants.CountRandomLinkShorten; i++)
                {
                    shortPath += Constants.AlphabetFull[random.Next(0, Constants.AlphabetFull.Length - 1)];
                }

                var testShortPath = GetFullPathByShortPath(shortPath);
                if (string.IsNullOrEmpty(testShortPath))
                {
                    try
                    {
                        _dbContext.Add(new ShortenLink()
                        {
                            FullPath = fullPath,
                            ShortPath = shortPath,
                            LocationOrigin = locationOrigin,
                        });
                        _dbContext.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in GenerateShortLink");
                    }
                }
                else
                {
                    shortPath = "";
                }
            }

            return shortPath;
        }
    }
}