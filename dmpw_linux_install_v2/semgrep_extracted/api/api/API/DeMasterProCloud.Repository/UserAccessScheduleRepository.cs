

using System;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DeMasterProCloud.Repository
{
    public interface IUserAccessScheduleRepository : IGenericRepository<UserAccessSchedule>
    {    
        
    }
    
    public class UserAccessScheduleRepository : GenericRepository<UserAccessSchedule>, IUserAccessScheduleRepository
    {
        private readonly AppDbContext _dbContext;
        
        public UserAccessScheduleRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
        }

    }
}