using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using DeMasterProCloud.DataAccess.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DeMasterProCloud.Repository;

public interface IDepartmentDeviceRepository : IGenericRepository<DepartmentDevice>
{
    IQueryable<DepartmentDevice> GetByDepartmentId(int departmentId);
    IQueryable<DepartmentDevice> GetByDepartmentIds(List<int> departmentIds);
    IQueryable<DepartmentDevice> GetByDepartmentIdAndDoorIds(int departmentId, List<int> doors);
    List<int> GetDoorIdsByAccountDepartmentRole(int companyId, int accountId);
}
public class DepartmentDeviceRepository : GenericRepository<DepartmentDevice>, IDepartmentDeviceRepository
{
    private readonly AppDbContext _dbContext;
    private readonly HttpContext _httpContext;
    public DepartmentDeviceRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
    {
        _dbContext = dbContext;
        _httpContext = contextAccessor?.HttpContext;
    }

    public IQueryable<DepartmentDevice> GetByDepartmentId(int departmentId)
    {
        return _dbContext.DepartmentDevice.Include(x => x.Department)
            .Include(x => x.Icu)
            .Where(x => x.DepartmentId == departmentId);
    }

    public IQueryable<DepartmentDevice> GetByDepartmentIds(List<int> departmentIds)
    {
        return _dbContext.DepartmentDevice.Include(x => x.Department)
            .Include(x => x.Icu)
            .Where(x => departmentIds.Contains(x.DepartmentId));
    }

    public IQueryable<DepartmentDevice> GetByDepartmentIdAndDoorIds(int departmentId, List<int> doors)
    {
        return _dbContext.DepartmentDevice.Include(x => x.Department)
            .Include(x => x.Icu)
            .Where(x => x.DepartmentId == departmentId && doors.Contains(x.IcuId));
    }
    /// <summary>
    /// Return list ids of device for account type enable department dynamic role
    /// </summary>
    /// <param name="companyId"></param>
    /// <param name="accountId"></param>
    /// <returns></returns>
    public List<int> GetDoorIdsByAccountDepartmentRole(int companyId, int accountId)
    {
        // get department ids enable level department permission
        var departmentIds = from c in _dbContext.CompanyAccount
            join r in _dbContext.DynamicRole on c.DynamicRoleId equals r.Id
            join u in _dbContext.User on c.AccountId equals u.AccountId
            join d in _dbContext.Department on u.DepartmentId equals d.Id
            where c.CompanyId == companyId & c.AccountId == accountId
                                           & !r.IsDeleted & r.EnableDepartmentLevel & r.CompanyId == companyId
                                           & !u.IsDeleted & u.CompanyId == companyId
                                           & !d.IsDeleted & d.CompanyId == companyId
            select d.Id;
        var ids = departmentIds.ToList();
            
        // get department children
        var parentIds = ids;
        do
        {
            if(!parentIds.Any())
                break;

            var ids1 = parentIds;
            var childrenIds = _dbContext.Department.Where(d => ids1.Contains(d.ParentId.Value)).Select(d => d.Id);
            parentIds = childrenIds.ToList();
            ids.AddRange(parentIds);
        } while (!parentIds.Any());


        var devices = _dbContext.DepartmentDevice.Where(x => ids.Contains(x.DepartmentId)).Select(x => x.IcuId)
            .Distinct();
            
        return devices.ToList();
    }
}