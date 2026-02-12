using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.WorkShift;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace DeMasterProCloud.Repository
{
    
    public interface IWorkShiftRepository : IGenericRepository<WorkShift>
    {
       bool IsNameExist(WorkShiftModel model);
    }
    
    public class WorkShiftRepository : GenericRepository<WorkShift>, IWorkShiftRepository
    {
        private readonly AppDbContext _dbContext;
        
        public WorkShiftRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
        }
        
        public bool IsNameExist(WorkShiftModel model)
        {
            return _dbContext.WorkShift.Any(x => x.Name.ToLower().Equals(model.Name.ToLower()) && x.Id != model.Id);
        }
    }
}