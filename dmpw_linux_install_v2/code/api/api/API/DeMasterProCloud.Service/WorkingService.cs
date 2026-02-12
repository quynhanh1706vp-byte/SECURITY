using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Company;
using DeMasterProCloud.DataModel.WorkingModel;
using DeMasterProCloud.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Globalization;
using System.Threading;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Resources;

namespace DeMasterProCloud.Service
{
    public interface IWorkingService : IPaginationService<WorkingListModel>
    {
        new IQueryable<WorkingListModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        WorkingType GetWorkingType(int workingId, int companyId);
        WorkingModel GetWorkingTypeDetail(int id, int companyId);
        int Add(WorkingModel model);
        void Update(int id, WorkingModel model);
        WorkingType CheckWorkingTypeExisted(string workingName);
        List<User> GetUserUsingWorkingType(int workingId, int companyId);
        void Delete(WorkingType workingType);

        void AssignMultipleUserToWorkingTime(int workingTypeId, string listUserId);
        bool CheckNameWorkingTime(string workingName, int id);

        void UpdateAttendanceWorkingTime(int workingTypeId, int userId, int companyId);
    }

    public class WorkingService : IWorkingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly ICompanyService _companyService;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        /// <summary>
        /// Ctor for account service
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="companyService"></param>
        /// <param name="mailService"></param>
        /// <param name="logger"></param>
        public WorkingService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor,
            ICompanyService companyService, IConfiguration configuration, ILogger<AccountService> logger)
        {
            _unitOfWork = unitOfWork;
            _companyService = companyService;
            _httpContext = httpContextAccessor.HttpContext;
            _logger = logger;
            _configuration = configuration;
            _mapper = MapperInstance.Mapper;
        }

        public IQueryable<WorkingListModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var data = _unitOfWork.AppDbContext.WorkingType
                    .Where(c => c.CompanyId == _httpContext.User.GetCompanyId())
                    .Select(m => new WorkingListModel()
                    {
                        Id = m.Id,
                        Name = m.Name,
                        WorkingDay = m.WorkingDay,
                        IsDefault = m.IsDefault
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
                return Enumerable.Empty<WorkingListModel>().AsQueryable();
            }
        }


        public int Add(WorkingModel model)
        {
            var workingId = 0;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var workingType = _mapper.Map<WorkingType>(model);
                        workingType.CompanyId = _httpContext.User.GetCompanyId();
                        var defaultWorkingType = CheckWorkingTypeDefault(_httpContext.User.GetCompanyId());
                        if (!defaultWorkingType)
                        {
                            workingType.IsDefault = true;
                        }
                        else
                        {
                            workingType.IsDefault = false;
                        }
                        _unitOfWork.WorkingRepository.Add(workingType);
                        _unitOfWork.Save();

                        //Save system log
                        var content = AttendanceResource.msgAddNewWorkingType;
                        List<string> details = new List<string>
                        {
                            $"{AttendanceResource.headerName} : {workingType.Name}"
                        };
                        var contentsDetails = string.Join("\n", details);

                        _unitOfWork.SystemLogRepository.Add(workingType.Id, SystemLogType.Attendance, ActionLogType.Add,
                            content, contentsDetails, null, _httpContext.User.GetCompanyId());

                        _unitOfWork.Save();

                        transaction.Commit();
                        workingId = workingType.Id;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                    }
                }
            });
            return workingId;
        }

        public WorkingType CheckWorkingTypeExisted(string workingName)
        {
            try
            {
                var existed = _unitOfWork.WorkingRepository.CheckWorkingTypeExisted(workingName, _httpContext.User.GetCompanyId());
                return existed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckWorkingTypeExisted");
                return null;
            }
        }

        public bool CheckNameWorkingTime(string workingName, int id)
        {
            try
            {
                var checkWorkingTime = _unitOfWork.WorkingRepository.CheckNameWorkingTime(workingName, _httpContext.User.GetCompanyId(), id);
                return checkWorkingTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckNameWorkingTime");
                return false;
            }
        }

        public WorkingType GetWorkingType(int id, int companyId)
        {
            try
            {
                var workingType = _unitOfWork.AppDbContext.WorkingType.FirstOrDefault(m => m.Id == id && m.CompanyId == companyId);

                if (workingType != null)
                {
                    if (workingType.CompanyId != 0)
                    {
                        var company = _companyService.GetById(workingType.CompanyId);
                        workingType.Company = company;
                    }

                }

                return workingType;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetWorkingType");
                return null;
            }
        }

        /// <summary>
        /// Get WorkingType's detail information
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public WorkingModel GetWorkingTypeDetail(int id, int companyId)
        {
            try
            {
                if(id == 0)
                {
                    // init data
                    var workingModel = new WorkingModel()
                    {
                        WorkingHourType = (int)WorkingHourType.OnlyInOffice,
                        WorkingHourTypeItems = EnumHelper.ToEnumList<WorkingHourType>().ToList(),
                        CheckClockOut = false,
                        UseClockOutDevice = false,
                        CheckLunchTime = false,
                        IsDefault = false,
                        CompanyId = companyId,
                    };

                    return workingModel;
                }

                var workingType = _unitOfWork.AppDbContext.WorkingType.FirstOrDefault(m => m.Id == id && m.CompanyId == companyId);

                if (workingType != null)
                {
                    // Change language.
                    var accountLanguage = _unitOfWork.AppDbContext.Account.FirstOrDefault(m => m.Id == _httpContext.User.GetAccountId()).Language;
                    var culture = new CultureInfo(accountLanguage);

                    // FE doesn't give "Culture" info.
                    Thread.CurrentThread.CurrentUICulture = culture;

                    var workingModel = _mapper.Map<WorkingModel>(workingType);

                    workingModel.WorkingHourTypeItems = EnumHelper.ToEnumList<WorkingHourType>().ToList();

                    return workingModel;
                }


                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetWorkingTypeDetail");
                return null;
            }
        }

        private bool CheckWorkingTypeDefault(int companyId)
        {
            var workingType = _unitOfWork.AppDbContext.WorkingType.FirstOrDefault(m => m.CompanyId == companyId && m.IsDefault);

            if (workingType != null)
            {
                return true;
            }
            return false;
        }

        private WorkingType GetWorkingTypeDefault(int companyId)
        {
            var workingType = _unitOfWork.AppDbContext.WorkingType.FirstOrDefault(m => m.CompanyId == companyId && m.IsDefault);

            return workingType;
        }

        public void Update(int id, WorkingModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var workingType = _unitOfWork.WorkingRepository.GetById(id);
                        model.CompanyId = _httpContext.User.GetCompanyId();

                        var workingTypeDefault = GetWorkingTypeDefault(_httpContext.User.GetCompanyId());
                        if (model.IsDefault == true)
                        {
                            if (workingType.Id != workingTypeDefault.Id)
                            {
                                workingTypeDefault.IsDefault = false;

                            }
                        }
                        else
                        {
                            if (workingType.Id == workingTypeDefault.Id)
                            {
                                model.IsDefault = true;
                            }
                        }

                        bool isTimeChanged = false;
                        if(model.WorkingDay != workingType.WorkingDay)
                        {
                            isTimeChanged = true;
                        }

                        _mapper.Map(model, workingType);
                        _unitOfWork.WorkingRepository.Update(workingTypeDefault);
                        _unitOfWork.WorkingRepository.Update(workingType);
                        _unitOfWork.Save();

                        // Update user's attendance (from the next day)
                        if (isTimeChanged)
                        {
                            var users = _unitOfWork.AppDbContext.User.Where(m => m.WorkingTypeId == workingType.Id).ToList();

                            foreach (var user in users)
                            {
                                UpdateAttendanceWorkingTime(workingType.Id, user.Id, user.CompanyId);
                            }
                        }

                        _unitOfWork.Save();



                        // save to system log 
                        var content = AttendanceResource.msgUpdateWorkingType;
                        List<string> details = new List<string>
                        {
                            $"{AttendanceResource.headerName} : {workingType.Name}"
                        };
                        var contentsDetails = string.Join("\n", details);
                        _unitOfWork.SystemLogRepository.Add(workingType.Id, SystemLogType.Attendance, ActionLogType.Update,
                            content, contentsDetails, null, _httpContext.User.GetCompanyId());

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                    }
                }
            });
        }
        public List<User> GetUserUsingWorkingType(int workingId, int companyId)
        {
            try
            {
                var users = _unitOfWork.AppDbContext.User.Where(m => m.CompanyId == companyId && !m.IsDeleted && m.WorkingTypeId == workingId).ToList();
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserUsingWorkingType");
                return new List<User>();
            }
        }

        public void Delete(WorkingType workingType)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        _unitOfWork.WorkingRepository.Delete(workingType);
                        _unitOfWork.Save();


                        // save to system log 
                        var content = AttendanceResource.msgDeleteWorkingType;
                        List<string> details = new List<string>
                        {
                            $"{AttendanceResource.headerName} : {workingType.Name}"
                        };
                        var contentsDetails = string.Join("\n", details);
                        _unitOfWork.SystemLogRepository.Add(workingType.Id, SystemLogType.Attendance, ActionLogType.Delete, content, contentsDetails, null, _httpContext.User.GetCompanyId());

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        public void AssignMultipleUserToWorkingTime(int workingTypeId, string listUserId)
        {
            try
            {
                String[] strListId = listUserId.Split(",");
                foreach (var userId in strListId)
                {
                    var user = _unitOfWork.UserRepository.GetById(Int32.Parse(userId));
                    user.WorkingTypeId = workingTypeId;
                    _unitOfWork.UserRepository.Update(user);

                    // Update workingTime in Attendance data (in database)
                    UpdateAttendanceWorkingTime(workingTypeId, Int32.Parse(userId), _httpContext.User.GetCompanyId());
                }
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AssignMultipleUserToWorkingTime");
            }
        }


        public void UpdateAttendanceWorkingTime(int workingTypeId, int userId, int companyId)
        {
            try
            {
                var day = DateTime.Today.AddDays(1);

                var attendance = _unitOfWork.AttendanceRepository.GetAttendanceAlreadyCreated(userId, companyId, day);

                while (attendance != null)
                {
                    var workingTypes = _unitOfWork.WorkingRepository.GetById(workingTypeId);
                    var listWorkings = JsonConvert.DeserializeObject<List<WorkingTime>>(workingTypes.WorkingDay);

                    foreach (var timeWork in listWorkings)
                    {
                        if (timeWork.Name == day.DayOfWeek.ToString())
                        {
                            attendance.WorkingTime = JsonConvert.SerializeObject(timeWork);

                            _unitOfWork.AttendanceRepository.Update(attendance);

                            break;
                        }
                    }

                    day = day.AddDays(1);
                    attendance = _unitOfWork.AttendanceRepository.GetAttendanceAlreadyCreated(userId, companyId, day);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateAttendanceWorkingTime");
            }
        }
    }
}