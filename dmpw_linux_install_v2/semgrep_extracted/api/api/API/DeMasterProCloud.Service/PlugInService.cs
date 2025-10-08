using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace DeMasterProCloud.Service
{
    public interface IPluginService : IPaginationService<PlugInModel>
    {
        new IQueryable<PlugInModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        bool CheckPluginCondition(string addOn, int companyId);

        PlugIn GetPluginCompany(int companyId, int id);

        int Update(int id, PlugIns model);
    }
    public class PlugInService : IPluginService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly ICompanyService _companyService;
        //private readonly IRoleService _roleService;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;

        public PlugInService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor,
            ICompanyService companyService, IRoleService roleService, IConfiguration configuration, ILogger<AccountService> logger)
        {
            _unitOfWork = unitOfWork;
            _companyService = companyService;
            //_roleService = roleService;
            _httpContext = httpContextAccessor.HttpContext;
            _logger = logger;
            _configuration = configuration;

        }

        public IQueryable<PlugInModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var data = _unitOfWork.AppDbContext.PlugIn
                    .Select(m => new PlugInModel()
                    {
                        Id = m.Id,
                        CompanyId = m.CompanyId,
                        Solutions = m.PlugIns,
                    });

                totalRecords = data.Count();


                recordsFiltered = data.Count();
                data = data.OrderBy($"{sortColumn} {sortDirection}"); // ColumnDefines.Company
                data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginated");
                totalRecords = 0;
                recordsFiltered = 0;
                return Enumerable.Empty<PlugInModel>().AsQueryable();
            }
        }

        public bool CheckPluginCondition(string addOn, int companyId)
        {
            try
            {
                var solution = _unitOfWork.PlugInRepository.Get(c => c.CompanyId == companyId);

                if (solution != null)
                {
                    PlugIns addOnSolution = JsonConvert.DeserializeObject<PlugIns>(solution.PlugIns);
                    foreach (PropertyInfo pi in addOnSolution.GetType().GetProperties())
                    {
                        if (pi.Name == addOn)
                        {
                            bool value = (bool)addOnSolution.GetType().GetProperty(addOn).GetValue(addOnSolution, null);
                            if (value)
                            {
                                return true;
                            }
                        }

                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckPluginCondition");
                return false;
            }
        }

        public PlugIn GetPluginCompany(int companyId, int id)
        {
            try
            {
                var plugIn = _unitOfWork.PlugInRepository.Get(c => c.CompanyId == companyId && c.Id == id);
                return plugIn;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPluginCompany");
                return null;
            }
        }

        public int Update(int id, PlugIns model)
        {
            var solutionId = 0;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var plugIn = _unitOfWork.PlugInRepository.Get(c => c.Id == id);
                        var json = JsonConvert.SerializeObject(model);
                        plugIn.PlugIns = json;
                        _unitOfWork.PlugInRepository.Update(plugIn);
                        _unitOfWork.Save();

                        transaction.Commit();
                        solutionId = plugIn.Id;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
            return solutionId;
        }
    }
}