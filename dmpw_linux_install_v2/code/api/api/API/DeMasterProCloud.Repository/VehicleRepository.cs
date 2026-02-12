using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Vehicle;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MoreLinq.Extensions;

namespace DeMasterProCloud.Repository
{
    public interface IVehicleRepository : IGenericRepository<Vehicle>
    {
        IQueryable<Vehicle> GetAllVehicleOfCompany(int companyId);
        IQueryable<Vehicle> GetAllVehicle(bool isUser, int companyId);
        IQueryable<Vehicle> GetListVehicleByUser(int userId);
        IQueryable<Vehicle> GetListVehicleByVisit(int visitId);
        Vehicle GetByPlateNumber(string plateNumber, int companyId);
        IQueryable<Vehicle> GetByPlateNumbers(List<string> plateNumbers, int companyId);
        IQueryable<Vehicle> GetVehicleByIds(List<int> ids);

        IQueryable<Vehicle> GetAllById(int id);
        Vehicle GetVehicleById(int id);
    }
    
    public class VehicleRepository : GenericRepository<Vehicle>, IVehicleRepository
    {
        private readonly AppDbContext _dbContext;
        
        public VehicleRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
        }

        public IQueryable<Vehicle> GetAllVehicleOfCompany(int companyId)
        {
            return _dbContext.Vehicle.Include(m => m.User).Include(m => m.Visit)
                .Where(m => !m.IsDeleted && m.CompanyId == companyId);
        }
        public IQueryable<Vehicle> GetAllVehicle(bool isUser, int companyId)
        {
            return _dbContext.Vehicle.Include(m => m.User).Include(m => m.Visit)
                .Where(m => !m.IsDeleted && m.CompanyId == companyId && isUser ? m.UserId != null : m.VisitId != null);
        }
        public IQueryable<Vehicle> GetListVehicleByUser(int userId)
        {
            return _dbContext.Vehicle.Where(m => !m.IsDeleted && m.UserId == userId);
        }

        public IQueryable<Vehicle> GetListVehicleByVisit(int visitId)
        {
            return _dbContext.Vehicle.Where(m => !m.IsDeleted && m.VisitId == visitId);
        }

        public Vehicle GetByPlateNumber(string plateNumber, int companyId)
        {
            return _dbContext.Vehicle
                .FirstOrDefault(m => !m.IsDeleted && m.PlateNumber.ToLower() == plateNumber.ToLower() && m.CompanyId == companyId);
        }

        public IQueryable<Vehicle> GetByPlateNumbers(List<string> plateNumbers, int companyId)
        {
            return _dbContext.Vehicle
                .Where(m => !m.IsDeleted && plateNumbers.Any(d => d.Equals(m.PlateNumber, System.StringComparison.OrdinalIgnoreCase)) && m.CompanyId == companyId);
        }

        public IQueryable<Vehicle> GetVehicleByIds(List<int> ids)
        {
            var vehicles = _dbContext.Vehicle.Where(m => !m.IsDeleted && ids.Contains(m.Id));

            return vehicles;
        }

        public IQueryable<Vehicle> GetAllById(int id)
        {
            var vehicle = _dbContext.Vehicle.Include(m => m.User).Where(m => !m.IsDeleted && m.Id == id);

            return vehicle;
        }

        public Vehicle GetVehicleById(int id)
        {
            var vehicle = _dbContext.Vehicle.Include(m => m.User).FirstOrDefault(m => !m.IsDeleted && m.Id == id);
            return vehicle;
        }
    }
}