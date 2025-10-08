

using System;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DeMasterProCloud.Repository
{
    public interface IAccessWorkShiftRepository : IGenericRepository<AccessWorkShift>
    {    
        
    }
    
    public class AccessWorkShiftRepository : GenericRepository<AccessWorkShift>, IAccessWorkShiftRepository
    {
        private readonly AppDbContext _dbContext;
        
        public AccessWorkShiftRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
        }

    }
}