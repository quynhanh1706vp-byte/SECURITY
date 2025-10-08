using DeMasterProCloud.DataAccess.Models;
using Microsoft.AspNetCore.Http;

namespace DeMasterProCloud.Repository;

public interface IDeviceReaderRepository : IGenericRepository<DeviceReader>
{
    
}

public class DeviceReaderRepository : GenericRepository<DeviceReader>, IDeviceReaderRepository
{
    private readonly AppDbContext _dbContext;
    public DeviceReaderRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
    {
        _dbContext = dbContext;
    }
}