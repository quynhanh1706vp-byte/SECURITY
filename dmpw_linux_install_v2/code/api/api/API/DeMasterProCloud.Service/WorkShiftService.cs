using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.EventLog;
using DeMasterProCloud.DataModel.WorkShift;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.DataModel.Visit;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Protocol;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using Newtonsoft.Json;

namespace DeMasterProCloud.Service
{
    public interface IWorkShiftService
    {
        Dictionary<string, object> GetInit();

        List<WorkShiftListModel> GetPaginated(string name, List<int> userIds, int pageNumber, int pageSize,
            string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered);


        WorkShift GetById(int id);
        List<WorkShift> GetByIds(List<int> ids);
        WorkShiftModel GetByWorkShiftId(int id);
        WorkShiftDetailModel GetDetailByWorkShiftId(int id);


        int Add(WorkShiftModel model);
        void Edit(WorkShift workShift, WorkShiftModel model);
        void Delete(WorkShift workShift);
        void DeleteRange(List<WorkShift> workShifts);
        bool IsNameExist(WorkShiftModel model);
        bool IsWorkShiftUsedInAccessSchedule(int workShiftId);
        bool AreWorkShiftsUsedInAccessSchedule(List<int> workShiftIds);
       
    }
    
    public class WorkShiftService : IWorkShiftService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly HttpContext _httpContext;
        private readonly IAccessGroupService _accessGroupService;
        private readonly IVisitService _visitService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        
        public WorkShiftService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, 
            IConfiguration configuration, IAccessGroupService accessGroupService, IVisitService visitService, ISettingService settingService)
        {
            _accessGroupService = accessGroupService;
            _httpContext = httpContextAccessor.HttpContext;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _visitService = visitService;
            _settingService = settingService;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<WorkShiftService>();
            _mapper = MapperInstance.Mapper;
        }

        public Dictionary<string , object> GetInit()
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            
           

            

            return data;
        }
         public bool IsNameExist(WorkShiftModel model)
        {
            return _unitOfWork.WorkShiftRepository.IsNameExist(model);
        }

       
        
        public List<WorkShiftListModel> GetPaginated(string name, List<int> userIds, int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var workShifts = _unitOfWork.AppDbContext.WorkShift.Include(x => x.AccessWorkShift).ThenInclude(x => x.AccessSchedule).ThenInclude(x => x.UserAccessSchedule).Where(x => true);
                totalRecords = workShifts.Count();
                if (totalRecords == 0)
                {
                    recordsFiltered = 0;
                    return new List<WorkShiftListModel>();
                }

                var data = workShifts;
                if (!string.IsNullOrEmpty(name))
                {
                    data = data.Where(m => m.Name.ToLower().Contains(name.Trim().ToLower()));
                }

                if (userIds.Any())
                {
                    data = data.Where(m => m.AccessWorkShift != null && m.AccessWorkShift.Any(x => x.AccessSchedule != null && x.AccessSchedule.UserAccessSchedule != null && x.AccessSchedule.UserAccessSchedule.Any(y => userIds.Contains(y.UserId))));
                }


                recordsFiltered = data.Count();
                // data = data.OrderBy($"{sortColumn} {sortDirection}");
                var result = _mapper.Map<List<WorkShiftListModel>>(data);
                string columnName = sortColumn.ToLower();
                if (!string.IsNullOrEmpty(sortColumn) && totalRecords > 0)
                {
                    result = Helpers.SortData<WorkShiftListModel>(result.AsEnumerable<WorkShiftListModel>(), sortDirection, columnName).ToList();
                }
                result = result.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginated");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<WorkShiftListModel>();
            }
        }

        
        
        public WorkShift GetById(int id)
        {
            return _unitOfWork.WorkShiftRepository.GetById(id);
        }

        public List<WorkShift> GetByIds(List<int> ids)
        {
            try
            {
                return _unitOfWork.AppDbContext.WorkShift.Where(m => ids.Contains(m.Id)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIds");
                return new List<WorkShift>();
            }
        }

        public WorkShiftModel GetByWorkShiftId(int id)
        {
            var workShift = _unitOfWork.AppDbContext.WorkShift.FirstOrDefault(m => m.Id == id);
            if (workShift == null) return null;
            var result = _mapper.Map<WorkShiftModel>(workShift);
            return result;
        }

        public WorkShiftDetailModel GetDetailByWorkShiftId(int id)
        {
            var workShift = _unitOfWork.AppDbContext.WorkShift.FirstOrDefault(m => m.Id == id);
            if (workShift == null) return null;

            var result = _mapper.Map<WorkShiftDetailModel>(workShift);

            

            return result;
        }

       

    
        public int Add(WorkShiftModel model)
        {
            var result = 0;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {

                    // add workShift
                    WorkShift workShift = _mapper.Map<WorkShift>(model);
                    
                    try
                    {
                       

                        _unitOfWork.AppDbContext.WorkShift.Add(workShift);
                        _unitOfWork.Save();
                        transaction.Commit();
                        
                        
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        result = 0;
                        transaction.Rollback();
                    }
                    result = workShift.Id;
                }
            });
            
            return result;
        }

        public void Edit(WorkShift workShift, WorkShiftModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {

                        // edit workShift
                        _mapper.Map(model, workShift);


                        _unitOfWork.WorkShiftRepository.Update(workShift);
                        _unitOfWork.Save();
                       
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        transaction.Rollback();
                    }
                }
            });
        }

        public void Delete(WorkShift workShift)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                    



                        // delete user workShift, visit workShift
                        _unitOfWork.AppDbContext.AccessWorkShift.RemoveRange(workShift.AccessWorkShift);
                        _unitOfWork.Save();

                        _unitOfWork.AppDbContext.WorkShift.Remove(workShift);
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        transaction.Rollback();
                    }
                }
            });

            
        }

        public void DeleteRange(List<WorkShift> workShifts)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        int companyId = _httpContext.User.GetCompanyId();
                        var userName = _httpContext.User.GetUsername();
                        // un-assign users
                        foreach (var workShift in workShifts)
                        {
                            

                            _unitOfWork.AppDbContext.AccessWorkShift.RemoveRange(workShift.AccessWorkShift);
                            _unitOfWork.AppDbContext.WorkShift.Remove(workShift);
                            _unitOfWork.Save();
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        transaction.Rollback();
                    }
                }
            });
        }

        public bool IsWorkShiftUsedInAccessSchedule(int workShiftId)
        {
            var companyId = _httpContext.User.GetCompanyId();
            return _unitOfWork.AppDbContext.AccessWorkShift
                .Any(aws => aws.WorkShiftId == workShiftId && aws.AccessSchedule.CompanyId == companyId);
        }

        public bool AreWorkShiftsUsedInAccessSchedule(List<int> workShiftIds)
        {
            var companyId = _httpContext.User.GetCompanyId();
            return _unitOfWork.AppDbContext.AccessWorkShift
                .Any(aws => workShiftIds.Contains(aws.WorkShiftId) && aws.AccessSchedule.CompanyId == companyId);
        }

    }
}