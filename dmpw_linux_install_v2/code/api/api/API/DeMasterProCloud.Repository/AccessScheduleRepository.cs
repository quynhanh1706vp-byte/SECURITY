

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.AccessSchedule;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DeMasterProCloud.Repository
{
    public interface IAccessScheduleRepository : IGenericRepository<AccessSchedule>
    {
       
        AccessScheduleModel GetByAccessScheduleId(int id);
       
        AccessSchedule GetAccessScheduleRunning();
        IQueryable<AccessSchedule> GetAccessSchedulesOverlap(AccessScheduleModel model);
        
        IQueryable<UserAccessSchedule> GetUserAssign(int id);
        
    }
    
    public class AccessScheduleRepository : GenericRepository<AccessSchedule>, IAccessScheduleRepository
    {
        private readonly AppDbContext _dbContext;
        
        public AccessScheduleRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
        }



        public AccessScheduleModel GetByAccessScheduleId(int id)
        {
            var accessSchedule = _dbContext.AccessSchedule.Include(x => x.AccessWorkShift).Include(x=> x.UserAccessSchedule).ThenInclude(x=> x.User).FirstOrDefault(m => !m.IsDeleted && m.Id == id);
            if (accessSchedule == null) return null;

            return new AccessScheduleModel()
            {
                Id = accessSchedule.Id,
                Content = accessSchedule.Content,
                StartTime = accessSchedule.StartTime.ConvertDefaultDateTimeToString(),
                EndTime = accessSchedule.EndTime.ConvertDefaultDateTimeToString(),
                UserQuantity = accessSchedule.UserQuantity,
                DepartmentId = accessSchedule.DepartmentId,
               
                UserIds = _dbContext.UserAccessSchedule.Where(x => x.AccessScheduleId == accessSchedule.Id).Select(x => x.UserId).ToList(),
                
            };
        }

       

        public AccessSchedule GetAccessScheduleRunning()
        {
            DateTime now = DateTime.Now;
            return _dbContext.AccessSchedule.Where(m => !m.IsDeleted && m.EndTime >= now)
                .OrderBy(m => m.StartTime).FirstOrDefault();
        }

        public IQueryable<AccessSchedule> GetAccessSchedulesOverlap(AccessScheduleModel model)
        {
            var startTime = model.StartTime.ConvertDefaultStringToDateTime();
            var endTime = model.EndTime.ConvertDefaultStringToDateTime();
            return _dbContext.AccessSchedule.Where(m =>
                (model.Id == 0 || m.Id != model.Id)
                && !m.IsDeleted
                &&
                (
                    (m.StartTime <= startTime && startTime< m.EndTime)
                    || (m.StartTime < endTime && endTime <= m.EndTime) || (m.StartTime >= startTime && endTime >= m.EndTime)
                )
                && (model.UserIds == null || model.UserIds.Count == 0 ||
                    m.UserAccessSchedule.Any(ua => model.UserIds.Contains(ua.UserId)))
                && (model.WorkShiftIds == null || model.WorkShiftIds.Count == 0 ||
                    m.AccessWorkShift.Any(aws => model.WorkShiftIds.Contains(aws.WorkShiftId))));
        }


        public IQueryable<UserAccessSchedule> GetUserAssign(int id)
        {
            return _dbContext.UserAccessSchedule.Include(x => x.User).Include(x => x.AccessSchedule)
                .Where(x => x.AccessScheduleId == id && !x.AccessSchedule.IsDeleted);
        }

    }
}