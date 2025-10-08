using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Bogus.Extensions;
using AutoMapper;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.EventLog;
using DeMasterProCloud.DataModel.AccessSchedule;
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
using DeMasterProCloud.DataModel.DeviceSDK;

namespace DeMasterProCloud.Service
{
    public interface IAccessScheduleService
    {
        Dictionary<string, object> GetInit();
       
        List<AccessScheduleListModel> GetPaginated(string content, List<int> userIds, int pageNumber, int pageSize,
            string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered);
        List<UserForAccessSchedule> GetPaginatedForUsers(int accessScheduleId, List<int> idsIgnore, string filter,
            int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered, bool isAssigned = true);
        
        AccessSchedule GetById(int id);
        List<AccessSchedule> GetByIds(List<int> ids);
        AccessScheduleModel GetByAccessScheduleId(int id);
        AccessScheduleDetailModel GetDetailByAccessScheduleId(int id);
        List<AccessScheduleUserInfo> GetUsersByAccessSchedule(AccessScheduleModel model);
        bool CheckAccessScheduleOverlap(AccessScheduleModel model);
        List<string> GetOverlappingUserNames(AccessScheduleModel model);
        (bool hasOverlap, List<string> userNames) CheckAccessScheduleOverlapWithUsers(AccessScheduleModel model);
        int Add(AccessScheduleModel model);
        bool Edit(AccessSchedule accessSchedule, AccessScheduleModel model);
        bool Delete(AccessSchedule accessSchedule);
        bool DeleteRange(List<AccessSchedule> accessSchedules);
       
       
        string AssignUsers(List<User> usersToAssign, int deviceId, int companyId, out List<string> messageIds);
        void UnAssignUsers(List<int> userIds, int deviceId, AccessSchedule accessSchedule);
        string AssignUsersToAccessSchedule(int accessScheduleId, List<int> userIds);
     
        string UnAssignUsersToAccessSchedule(int accessScheduleId, List<int> userIds);
       
    
    }
    
    public class AccessScheduleService : IAccessScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly HttpContext _httpContext;
        private readonly IAccessGroupService _accessGroupService;
        private readonly IDeviceSDKService _deviceSDKService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        
        public AccessScheduleService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, 
            IConfiguration configuration, IAccessGroupService accessGroupService, IDeviceSDKService deviceSDKService)
        {
            _accessGroupService = accessGroupService;
            _httpContext = httpContextAccessor.HttpContext;
            _unitOfWork = unitOfWork;
            _configuration = configuration;

            _deviceSDKService = deviceSDKService;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<AccessScheduleService>();
            _mapper = MapperInstance.Mapper;
        }

        public Dictionary<string , object> GetInit()
        {
            try
            {
                Dictionary<string, object> data = new Dictionary<string, object>();





                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetInit");
                return new Dictionary<string, object>();
            }
        }

       
        
        public List<AccessScheduleListModel> GetPaginated(string content, List<int> userIds, int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var accessSchedules = _unitOfWork.AppDbContext.AccessSchedule.Include(x => x.UserAccessSchedule).Where(x => !x.IsDeleted);
                totalRecords = accessSchedules.Count();
                if (totalRecords == 0)
                {
                    recordsFiltered = 0;
                    return new List<AccessScheduleListModel>();
                }

                var data = accessSchedules;
                if (!string.IsNullOrEmpty(content))
                {
                    data = data.Where(m => m.Content.ToLower().Contains(content.ToLower()));
                }
                // if (!string.IsNullOrEmpty(startDateFrom))
                // {
                //     var startTime = startDateFrom.ConvertDefaultStringToDateTime() ?? DateTime.UtcNow;
                //     data = data.Where(m => startTime <= m.StartTime);
                // }
                // if (!string.IsNullOrEmpty(endDateFrom))
                // {
                //     var endTime = endDateFrom.ConvertDefaultStringToDateTime() ?? DateTime.UtcNow;
                //     data = data.Where(m => m.StartTime <= endTime);
                // }
                if (userIds.Any())
                {
                    data = data.Where(m => m.UserAccessSchedule.Any(x => userIds.Contains(x.UserId)));
                }


                recordsFiltered = data.Count();
                // data = data.OrderBy($"{sortColumn} {sortDirection}");
                var result = _mapper.Map<List<AccessScheduleListModel>>(data);
                string columnName = sortColumn.ToLower();
                if (!string.IsNullOrEmpty(sortColumn) && totalRecords > 0)
                {
                    result = Helpers.SortData<AccessScheduleListModel>(result.AsEnumerable<AccessScheduleListModel>(), sortDirection, columnName).ToList();
                }
                result = result.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginated");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<AccessScheduleListModel>();
            }
        }

        public List<UserForAccessSchedule> GetPaginatedForUsers(int accessScheduleId, List<int> idsIgnore, string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered, bool isAssigned = true)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var accountId = _httpContext.User.GetAccountId();

                var plugIns = _unitOfWork.AppDbContext.PlugIn.Where(x => x.CompanyId == companyId).Select(x => x.PlugIns).FirstOrDefault();
                var value = JsonConvert.DeserializeObject<PlugIns>(plugIns);

                // var accessSchedules = _unitOfWork.AccessScheduleRepository.GetByAccessScheduleId(accessScheduleId);
                var userAccessSchedules = _unitOfWork.AppDbContext.UserAccessSchedule.Where(x => x.AccessScheduleId == accessScheduleId).Select(x => x.UserId).ToList();

                var data = _unitOfWork.UserRepository.GetByCompanyId(companyId, new List<int>() { (int)Status.Valid });

                data = isAssigned ? data.Where(m => userAccessSchedules.Contains(m.Id)) : data.Where(m => !userAccessSchedules.Contains(m.Id));

                // check account type dynamic role enable department role
                var accountType = _httpContext.User.GetAccountType();
                if (accountType == (short)AccountType.DynamicRole && value.DepartmentAccessLevel)
                {
                    var departmentIds = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
                    if (departmentIds.Any())
                    {
                        data = data.Where(x => departmentIds.Contains(x.DepartmentId));
                    }
                }
                totalRecords = data.Count();

                if (!string.IsNullOrEmpty(filter))
                {
                    filter = filter.Trim().RemoveDiacritics().ToLower();
                    data = data.AsEnumerable().Where(x => (x.FirstName + " " + (x.LastName ?? "")).Trim().RemoveDiacritics().ToLower().Contains(filter) ||
                        x.Department.DepartName.RemoveDiacritics().ToLower().Contains(filter)).AsQueryable();
                }

                if (idsIgnore.Any())
                {
                    data = data.Where(x => !idsIgnore.Contains(x.Id));
                }

                recordsFiltered = data.Count();



                var resultList = data
                    .Select(m => new UserForAccessSchedule()
                    {
                        Id = m.Id,
                        UserCode = m.UserCode,
                        FirstName = m.FirstName,
                        DepartmentName = m.Department.DepartName ?? "",
                        WorkTypeName = m.WorkType != null ? ((WorkType)m.WorkType).GetDescription() : "",
                    }).ToList();

                string columnName = sortColumn.ToLower();
                if (!string.IsNullOrEmpty(sortColumn) && totalRecords > 0)
                {
                    resultList = Helpers.SortData<UserForAccessSchedule>(resultList.AsEnumerable<UserForAccessSchedule>(), sortDirection, columnName).ToList();
                }

                var resultData = resultList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                return resultData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginatedForUsers");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<UserForAccessSchedule>();
            }
        }

        
        public AccessSchedule GetById(int id)
        {
            try
            {
                return _unitOfWork.AccessScheduleRepository.GetById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return null;
            }
        }

        public List<AccessSchedule> GetByIds(List<int> ids)
        {
            try
            {
                return _unitOfWork.AccessScheduleRepository.GetMany(m => ids.Contains(m.Id) && !m.IsDeleted).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIds");
                return new List<AccessSchedule>();
            }
        }

        public AccessScheduleModel GetByAccessScheduleId(int id)
        {
            try
            {
                return _unitOfWork.AccessScheduleRepository.GetByAccessScheduleId(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByAccessScheduleId");
                return null;
            }
        }

        public AccessScheduleDetailModel GetDetailByAccessScheduleId(int id)
        {
            try
            {
                var accessSchedule = _unitOfWork.AppDbContext.AccessSchedule.FirstOrDefault(m => !m.IsDeleted && m.Id == id);
                if (accessSchedule == null) return null;

                var result = _mapper.Map<AccessScheduleDetailModel>(accessSchedule);

                result.Users = _unitOfWork.AppDbContext.UserAccessSchedule.Include(x => x.User).ThenInclude(z => z.Department)
                    .Where(x => !x.User.IsDeleted && x.AccessScheduleId == accessSchedule.Id)
                    .Select(x => new UserAccessScheduleModel()
                    {
                        Id = x.User.Id,
                        Name = x.User.FirstName,
                        Avatar = x.User.Avatar,
                        DepartmentName = x.User.Department.DepartName ?? ""
                    }).ToList();

                result.WorkShifts = _unitOfWork.AppDbContext.AccessWorkShift.Include(x => x.WorkShift).Where(x => x.AccessScheduleId == accessSchedule.Id)
                    .Select(x => new WorkShiftAccessScheduleModel()
                    {
                        Id = x.WorkShift.Id,
                        Name = x.WorkShift.Name,
                        StartDate = x.WorkShift.StartTime,
                        EndDate = x.WorkShift.EndTime
                    }).ToList();


                var doorIdList = JsonConvert.DeserializeObject<List<int>>(accessSchedule.DoorIds);
                result.Doors = _unitOfWork.AppDbContext.IcuDevice.Where(x => doorIdList.Contains(x.Id))
                    .Select(x => new DoorModel()
                    {
                        Id = x.Id,
                        DoorName = x.Name
                    }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDetailByAccessScheduleId");
                return null;
            }
        }

        public List<AccessScheduleUserInfo> GetUsersByAccessSchedule(AccessScheduleModel model)
        {
            try
            {
                var users = _unitOfWork.UserRepository.GetByIds(_httpContext.User.GetCompanyId(), model.UserIds);
                return users.Select(m => new AccessScheduleUserInfo()
                {
                    Id = m.Id,
                    Name = m.FirstName,
                    Email = m.Email,
                    DepartmentName = m.Department?.DepartName,
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsersByAccessSchedule");
                return new List<AccessScheduleUserInfo>();
            }
        }

        public bool CheckAccessScheduleOverlap(AccessScheduleModel model)
        {
            try
            {
                return GetOverlappingUserNames(model).Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAccessScheduleOverlap");
                return false;
            }
        }

        public List<string> GetOverlappingUserNames(AccessScheduleModel model)
        {
            try
            {
                return CheckAccessScheduleOverlapWithUsers(model).userNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetOverlappingUserNames");
                return new List<string>();
            }
        }

        public (bool hasOverlap, List<string> userNames) CheckAccessScheduleOverlapWithUsers(AccessScheduleModel model)
        {
            try
            {
                var overlappingSchedules = _unitOfWork.AccessScheduleRepository.GetAccessSchedulesOverlap(model)
                    .Include(a => a.UserAccessSchedule)
                    .ThenInclude(ua => ua.User)
                    .ToList();

                var userNames = new List<string>();

                foreach (var schedule in overlappingSchedules)
                {
                    var users = schedule.UserAccessSchedule
                        .Where(ua => model.UserIds != null && model.UserIds.Contains(ua.UserId))
                        .Select(ua => ua.User.FirstName + " " + ua.User.LastName)
                        .ToList();

                    userNames.AddRange(users);
                }

                var distinctUserNames = userNames.Distinct().ToList();
                return (distinctUserNames.Any(), distinctUserNames);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAccessScheduleOverlapWithUsers");
                return (false, new List<string>());
            }
        }

        public int Add(AccessScheduleModel model)
        {
            var result = 0;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    int companyId = _httpContext.User.GetCompanyId();
                    var usersToAssign = _unitOfWork.UserRepository.GetByIdsWithNoTracking(companyId, model.UserIds);
                    var company = _unitOfWork.CompanyRepository.GetById(companyId);

                    // add accessSchedule
                    AccessSchedule accessSchedule = _mapper.Map<AccessSchedule>(model);
                    accessSchedule.CompanyId = companyId;
                    try
                    {
                        var doorIds = JsonConvert.DeserializeObject<List<int>>(accessSchedule.DoorIds);


                        _unitOfWork.AccessScheduleRepository.Add(accessSchedule);
                        _unitOfWork.Save();

                        // add user 
                        if (model.UserIds != null && model.UserIds.Any())
                        {
                            foreach (var user in usersToAssign)
                            {
                                var userAccessSchedule = new UserAccessSchedule()
                                {
                                    AccessScheduleId = accessSchedule.Id,
                                    UserId = user.Id
                                };
                                _unitOfWork.UserAccessScheduleRepository.Add(userAccessSchedule);
                            }
                            _unitOfWork.Save();
                        }
                        transaction.Commit();
                        // check accessSchedule running
                        var accessScheduleCheck = _unitOfWork.AccessScheduleRepository.GetAccessScheduleRunning();
                        if ((accessScheduleCheck == null || accessScheduleCheck.StartTime >= accessSchedule.EndTime) && doorIds?.Any() == true)
                        {
                            string timezone = GetTimezoneSendToDevice(doorIds[0]);
                            var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;
                            // init time user send to device
                            foreach (var user in usersToAssign)
                            {
                                user.ExpiredDate = model.EndTime.ConvertDefaultStringToDateTime();
                                user.EffectiveDate = model.StartTime.ConvertDefaultStringToDateTime();
                            }


                            if (usersToAssign.Count > 0)
                            {
                                foreach (var doorId in doorIds)
                                {
                                    string resultAssign = AssignUsers(usersToAssign, doorId, companyId, out var msgIds);
                                    if (!string.IsNullOrEmpty(resultAssign))
                                    {
                                        _logger.LogError(resultAssign);
                                        result = 0;
                                    }

                                }
                            }
                        }
                        // add work shift
                        if (model.WorkShiftIds != null && model.WorkShiftIds.Any())
                        {
                            foreach (var workShiftId in model.WorkShiftIds)
                            {
                                var workShift = _unitOfWork.WorkShiftRepository.GetById(workShiftId);
                                if (workShift == null) continue;
                                var workShiftAccessSchedule = new AccessWorkShift()
                                {
                                    AccessScheduleId = accessSchedule.Id,
                                    WorkShiftId = workShiftId
                                };
                                _unitOfWork.AccessWorkShiftRepository.Add(workShiftAccessSchedule);
                            }
                            _unitOfWork.Save();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        result = 0;
                        transaction.Rollback();
                    }
                    result = accessSchedule.Id;
                }
            });
            
            return result;
        }

        public bool Edit(AccessSchedule accessSchedule, AccessScheduleModel model)
        {
            bool result = false;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var userName = _httpContext.User.GetUsername();
                        var companyId = _httpContext.User.GetCompanyId();
                        var company = _unitOfWork.CompanyRepository.GetById(companyId);

                        var accessScheduleCheck = _unitOfWork.AccessScheduleRepository.GetAccessScheduleRunning();
                        bool accessScheduleCheckedRunning = accessScheduleCheck == null || accessScheduleCheck.StartTime >= accessSchedule.EndTime;
                        var doorIds = JsonConvert.DeserializeObject<List<int>>(accessSchedule.DoorIds);
                        
                        _unitOfWork.Save();

                        // edit accessSchedule
                        _mapper.Map(model, accessSchedule);
                        accessSchedule.CompanyId = companyId;

                        _unitOfWork.AccessScheduleRepository.Update(accessSchedule);
                        _unitOfWork.Save();
                        // assign new uses
                        if (accessScheduleCheckedRunning && doorIds?.Any() == true)
                        {
                            var usersIds = _unitOfWork.AppDbContext.UserAccessSchedule.Where(x => x.AccessScheduleId == accessSchedule.Id).Select(x => x.UserId).ToList();
                            var usersToAssign = _unitOfWork.UserRepository.GetByIdsWithNoTracking(companyId, usersIds).ToList();

                            string timezone = GetTimezoneSendToDevice(doorIds[0]);
                            var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;

                            // init time user send to device
                            foreach (var user in usersToAssign)
                            {
                                user.ExpiredDate = model.EndTime.ConvertDefaultStringToDateTime();
                                user.EffectiveDate = model.StartTime.ConvertDefaultStringToDateTime();
                            }

                            var doorIdsNew = JsonConvert.DeserializeObject<List<int>>(accessSchedule.DoorIds);
                            foreach (var doorId in doorIdsNew)
                            {
                                AssignUsers(usersToAssign, doorId, _httpContext.User.GetCompanyId(), out var msgIds);

                            }

                            
                            
                        }
                        // check update work shift
                        var workShifts = _unitOfWork.AppDbContext.AccessWorkShift.Where(x => x.AccessScheduleId == accessSchedule.Id).ToList();
                        foreach (var workShift in workShifts)
                        {
                            if (model.WorkShiftIds == null || !model.WorkShiftIds.Contains(workShift.WorkShiftId))
                            {
                                _unitOfWork.AccessWorkShiftRepository.Delete(workShift);
                                _unitOfWork.Save();
                            }
                        }
                        if (model.WorkShiftIds != null && model.WorkShiftIds.Any())
                        {
                            foreach (var workShiftId in model.WorkShiftIds)
                            {
                                var workShift = _unitOfWork.WorkShiftRepository.GetById(workShiftId);
                                if (workShift == null) continue;

                                // Check if the AccessWorkShift already exists
                                var existingWorkShiftAccess = _unitOfWork.AppDbContext.AccessWorkShift
                                    .FirstOrDefault(x => x.AccessScheduleId == accessSchedule.Id && x.WorkShiftId == workShiftId);

                                if (existingWorkShiftAccess == null)
                                {
                                    var workShiftAccessSchedule = new AccessWorkShift()
                                    {
                                        AccessScheduleId = accessSchedule.Id,
                                        WorkShiftId = workShiftId
                                    };
                                    _unitOfWork.AccessWorkShiftRepository.Add(workShiftAccessSchedule);
                                    _unitOfWork.Save();
                                }
                            }
                        }
                        // check update user access
                        var userAccess = _unitOfWork.AppDbContext.UserAccessSchedule.Where(x => x.AccessScheduleId == accessSchedule.Id).ToList();
                        foreach (var user in userAccess)
                        {
                            if (model.UserIds == null || !model.UserIds.Contains(user.UserId))
                            {
                                _unitOfWork.UserAccessScheduleRepository.Delete(user);
                                _unitOfWork.Save();
                            }
                        }
                        if (model.UserIds != null && model.UserIds.Any())
                        {
                            foreach (var userId in model.UserIds)
                            {
                                var user = _unitOfWork.UserRepository.GetById(userId);
                                if (user == null) continue;

                                // Check if the UserAccessSchedule already exists
                                var existingUserAccess = _unitOfWork.AppDbContext.UserAccessSchedule
                                    .FirstOrDefault(x => x.AccessScheduleId == accessSchedule.Id && x.UserId == userId);

                                if (existingUserAccess == null)
                                {
                                    var userAccessSchedule = new UserAccessSchedule()
                                    {
                                        AccessScheduleId = accessSchedule.Id,
                                        UserId = userId
                                    };
                                    _unitOfWork.UserAccessScheduleRepository.Add(userAccessSchedule);
                                    _unitOfWork.Save();
                                }
                            }
                        }
                        transaction.Commit();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        _logger.LogError(ex.Message + ex.StackTrace);
                        transaction.Rollback();
                    }
                }
            });
            return result;
        }

            public bool Delete(AccessSchedule accessSchedule)
        {
            bool result = false;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        int companyId = _httpContext.User.GetCompanyId();
                        var userName = _httpContext.User.GetUsername();
                        // un-assign users
                        var doorIds = JsonConvert.DeserializeObject<List<int>>(accessSchedule.DoorIds);
                        var accessScheduleCheck = _unitOfWork.AccessScheduleRepository.GetAccessScheduleRunning();
                        bool accessScheduleCheckedRunning = accessScheduleCheck == null || accessScheduleCheck.StartTime >= accessSchedule.EndTime;
                        var accessScheduleUsers = _unitOfWork.AppDbContext.UserAccessSchedule.Where(x => x.AccessScheduleId == accessSchedule.Id).ToList();

                        if (accessScheduleCheckedRunning && doorIds?.Any() == true && accessScheduleUsers.Any())
                        {
                            var userIds = accessScheduleUsers.Select(x => x.UserId).ToList();
                            foreach (var doorId in doorIds)
                            {
                                UnAssignUsers(userIds, doorId, accessSchedule);
                            }
                        }
                                    
                        // delete user accessSchedule, visit accessSchedule
                        _unitOfWork.UserAccessScheduleRepository.DeleteRange(accessScheduleUsers);
                        _unitOfWork.Save();

                        accessSchedule.IsDeleted = true;
                        _unitOfWork.AccessScheduleRepository.Update(accessSchedule);
                        _unitOfWork.Save();
                        transaction.Commit();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        result = false;
                        transaction.Rollback();
                    }
                }
            });

            return result;
        }

        public bool DeleteRange(List<AccessSchedule> accessSchedules)
        {
            bool result = false;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        int companyId = _httpContext.User.GetCompanyId();
                        var userName = _httpContext.User.GetUsername();
                        // un-assign users
                        foreach (var accessSchedule in accessSchedules)
                        {
                            var doorIds = JsonConvert.DeserializeObject<List<int>>(accessSchedule.DoorIds);
                            var accessScheduleCheck = _unitOfWork.AccessScheduleRepository.GetAccessScheduleRunning();
                            bool accessScheduleCheckedRunning = accessScheduleCheck == null || accessScheduleCheck.StartTime >= accessSchedule.EndTime;
                            var accessScheduleUsers = _unitOfWork.AppDbContext.UserAccessSchedule.Where(x => x.AccessScheduleId == accessSchedule.Id).ToList();

                            if (accessScheduleCheckedRunning && doorIds?.Any() == true && accessScheduleUsers.Any())
                            {
                                var userIds = accessScheduleUsers.Select(x => x.UserId).ToList();
                                foreach (var doorId in doorIds)
                                {
                                    UnAssignUsers(userIds, doorId, accessSchedule);
                                }
                            }
                            
                            // delete user accessSchedule, visit accessSchedule
                            _unitOfWork.UserAccessScheduleRepository.DeleteRange(accessScheduleUsers);
                            _unitOfWork.Save();

                            accessSchedule.IsDeleted = true;
                            _unitOfWork.AccessScheduleRepository.Update(accessSchedule);
                            _unitOfWork.Save();
                        }
                        transaction.Commit();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        transaction.Rollback();
                        result = false;
                    }
                }
            });
            return result;
        }

        

        

        public string AssignUsers(List<User> usersToAssign, int deviceId, int companyId, out List<string> messageIds)
        {
            // check status device
            messageIds = null;
            var company = _unitOfWork.CompanyRepository.GetById(companyId);
            var device = _unitOfWork.IcuDeviceRepository.GetById(deviceId);
            if(device == null || (company != null && !company.AutoSyncUserData && device.ConnectionStatus != (short)ConnectionStatus.Online))
            {
                return $"Device (deviceId{deviceId}) - {device?.Name} disconnected!";
            }

            var accessGroup = _unitOfWork.AccessGroupRepository.GetDefaultAccessGroup(companyId);
            var accessGroupDevice = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupIdAndDeviceId(companyId, accessGroup.Id, deviceId);
            
            string returnValue = string.Empty;
            var startTime = DateTime.Now;
            var addingTime = DateTime.Now;
            var savingTime = DateTime.Now;
            int addUserCount = 0;
            int removeUserCount = 0;
            var userLogsProtocol = new List<UserLogProtocolData>();
            try
            {
                //int maxIcuUser = Constants.Settings.DefaultMaxIcuUser;
                //if (!string.IsNullOrEmpty(_configuration[Constants.Settings.MaxIcuUser]))
                //{
                //    maxIcuUser = Convert.ToInt32(_configuration[Constants.Settings.MaxIcuUser]);
                //}

                int maxIcuUser = Helpers.GetMaximumUserCount(device.DeviceType);

                int userCount = _unitOfWork.AccessGroupDeviceRepository.GetUserCount(accessGroupDevice.IcuId);
                if (userCount + usersToAssign.Count > maxIcuUser)
                {
                    returnValue = string.Format(AccessGroupResource.msgUnableToAssignOverMaxUser,
                        maxIcuUser,
                        $"{accessGroupDevice.Icu.Name} ({accessGroupDevice.Icu.DeviceAddress})");

                    return returnValue;
                }

                //Add user log
                var userLogs = _accessGroupService.AddUserLog(accessGroupDevice.IcuId, accessGroupDevice.TzId, usersToAssign, ActionType.Add);

                //Add user log protocol data
                var userLogProtocol = new UserLogProtocolData
                {
                    IcuAddress = accessGroupDevice.Icu.DeviceAddress,
                    UserLogs = userLogs,
                    ProtocolType = Constants.Protocol.AddUser
                };
                userLogsProtocol.Add(userLogProtocol);
                addUserCount += userLogs.Count;

                addingTime = DateTime.Now;
                savingTime = DateTime.Now;

                // send card to sdk 
                var cards = userLogs.Select(x => new SDKCardModel()
                {
                    EmployeeNumber = x?.Id.ToString(),
                    UserName = x?.User?.FirstName,
                    CardId = x?.CardId,
                    IssueCount = x?.IssueCount ?? 0,
                    AdminFlag = 0,
                    EffectiveDate = usersToAssign?.FirstOrDefault()?.EffectiveDate?.ConvertDefaultDateTimeToString(Constants.DateTimeFormat.DdMMyyyyHHmm),
                    ExpireDate = usersToAssign?.FirstOrDefault()?.ExpiredDate?.ConvertDefaultDateTimeToString(Constants.DateTimeFormat.DdMMyyyyHHmm),
                    CardStatus = x?.CardStatus ?? 0,
                    UserId = x?.Id.ToString(),
                    IdType = x?.CardType ?? 0,
                    DepartmentName = x?.DepartmentName,
                    Timezone = x?.TzPosition ?? 0,
                    AntiPassBack = 0,
                    AccessGroupId = 0,
                    Position = x?.Position,
                    Avatar = x?.Avatar,
                    FaceData = new SDKFaceDataList(),
                    FloorIndex = new List<int>(),
                    WorkType = x?.WorkType?.ToString(),
                    Grade = x?.Grade?.ToString(),
                    FingerTemplates = x?.FingerTemplates
                }).ToList();

                _deviceSDKService.AddCard(device.DeviceAddress, cards);
            }
            catch (Exception ex)
            {
                returnValue = ex.Message;
            }
            if (!string.IsNullOrEmpty(returnValue))
            {
                return returnValue;
            }
            return returnValue;
        }

        public void UnAssignUsers(List<int> userIds, int deviceId, AccessSchedule accessSchedule)
        {
            var userLogsProtocol = new List<UserLogProtocolData>();
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var usersToUnAssign = _unitOfWork.UserRepository.GetByIdsWithNoTracking(companyId, userIds);
                string timezone = GetTimezoneSendToDevice(deviceId);
                var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;

                foreach (var user in usersToUnAssign)
                {
                    user.EffectiveDate = accessSchedule.StartTime.ConvertToUserTime(offSet);
                    user.ExpiredDate = accessSchedule.EndTime.ConvertToUserTime(offSet);
                }

                var accessGroup = _unitOfWork.AccessGroupRepository.GetDefaultAccessGroup(companyId);
                var accessGroupDevice = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupIdAndDeviceId(companyId, accessGroup.Id, deviceId);
                //Un-assign users from current access group
                userLogsProtocol.AddRange(UnAssignUsersFromAccessGroup(accessGroupDevice, usersToUnAssign));


                // send delete card to sdk 
                var userLogs = _accessGroupService.AddUserLog(accessGroupDevice.IcuId, accessGroupDevice.TzId, usersToUnAssign, ActionType.Delete);
                if (userLogs != null && userLogs.Any())
                {
                    var firstUser = usersToUnAssign.FirstOrDefault();
                    var cards = userLogs.Select(x => new SDKCardModel()
                    {
                        EmployeeNumber = x?.Id.ToString(),
                        UserName = x?.User?.FirstName,
                        CardId = x?.CardId,
                        IssueCount = x?.IssueCount ?? 0,
                        AdminFlag = 0,
                        EffectiveDate = firstUser?.EffectiveDate?.ConvertDefaultDateTimeToString(Constants.DateTimeFormat.DdMMyyyyHHmm),
                        ExpireDate = firstUser?.ExpiredDate?.ConvertDefaultDateTimeToString(Constants.DateTimeFormat.DdMMyyyyHHmm),
                        CardStatus = x?.CardStatus ?? 0,
                        UserId = x?.Id.ToString(),
                        IdType = x?.CardType ?? 0,
                        DepartmentName = x?.DepartmentName,
                        Timezone = x?.TzPosition ?? 0,
                        AntiPassBack = 0,
                        AccessGroupId = 0,
                        Position = x?.Position,
                        Avatar = x?.Avatar,
                        FaceData = new SDKFaceDataList(),
                        FloorIndex = new List<int>(),
                        WorkType = x?.WorkType?.ToString(),
                        Grade = x?.Grade?.ToString(),
                        FingerTemplates = x?.FingerTemplates
                    }).ToList();

                    if (accessGroupDevice?.Icu?.DeviceAddress != null)
                    {
                        _deviceSDKService.DeleteCard(accessGroupDevice.Icu.DeviceAddress, cards);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return;
            }

            

           
        }

        public string AssignUsersToAccessSchedule(int accessScheduleId, List<int> userIds)
        {
            string result = string.Empty;
            int companyId = _httpContext.User.GetCompanyId();
            var accessSchedule = _unitOfWork.AccessScheduleRepository.GetById(accessScheduleId);
            if (accessSchedule == null)
            {
                return MessageResource.RecordNotFound;
            }

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    var usersToAssign = _unitOfWork.UserRepository.GetByIdsWithNoTracking(companyId, userIds);
                    try
                    {
                        // add user metting
                        foreach (var user in usersToAssign)
                        {
                            var userAccessSchedule = new UserAccessSchedule()
                            {
                                AccessScheduleId = accessSchedule.Id,
                                UserId = user.Id
                            };
                            _unitOfWork.UserAccessScheduleRepository.Add(userAccessSchedule);
                        }

                        _unitOfWork.Save();
                        var doorIds = JsonConvert.DeserializeObject<List<int>>(accessSchedule.DoorIds);
                        if (userIds != null && userIds.Any())
                        {
                            // check accessSchedule running
                            var accessScheduleCheck = _unitOfWork.AccessScheduleRepository.GetAccessScheduleRunning();
                            if (accessScheduleCheck == null || accessScheduleCheck.StartTime >= accessSchedule.EndTime)
                            {
                                // init time user send to device
                                foreach (var user in usersToAssign)
                                {
                                    user.ExpiredDate = accessSchedule.EndTime;
                                    user.EffectiveDate = accessSchedule.StartTime;
                                }

                                
                                if (usersToAssign.Count > 0)
                                {
                                    foreach (var doorId in doorIds)
                                    {
                                        string resultAssign = AssignUsers(usersToAssign, doorId, companyId,
                                            out var msgIds);
                                        if (!string.IsNullOrEmpty(resultAssign))
                                        {
                                            _logger.LogError(resultAssign);
                                            result = resultAssign;
                                        }
                                    }
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        result = ex.Message;
                        transaction.Rollback();
                    }
                }
            });
            return result;
        }

        
        public string UnAssignUsersToAccessSchedule(int accessScheduleId, List<int> userIds)
        {
            string result = string.Empty;
            var accessSchedule = _unitOfWork.AccessScheduleRepository.GetById(accessScheduleId);
            if (accessSchedule == null)
            {
                return MessageResource.RecordNotFound;
            }

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    
                    try
                    {
                        int companyId = _httpContext.User.GetCompanyId();
                        // un-assign users
                        var doorIds = JsonConvert.DeserializeObject<List<int>>(accessSchedule.DoorIds);
                        var accessScheduleCheck = _unitOfWork.AccessScheduleRepository.GetAccessScheduleRunning();
                        bool accessScheduleCheckedRunning = accessScheduleCheck == null || accessScheduleCheck.StartTime >= accessSchedule.EndTime;
                        var accessScheduleUsers = _unitOfWork.AppDbContext.UserAccessSchedule.Where(x => x.AccessScheduleId == accessSchedule.Id).ToList();

                        if (accessScheduleCheckedRunning && doorIds?.Any() == true && accessScheduleUsers.Any())
                        {
                            var userIds = accessScheduleUsers.Select(x => x.UserId).ToList();
                            foreach (var doorId in doorIds)
                            {
                                UnAssignUsers(userIds, doorId, accessSchedule);
                            }
                        }
                        _unitOfWork.UserAccessScheduleRepository.DeleteRange(accessScheduleUsers);
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        result = ex.Message;
                        transaction.Rollback();
                    }
                }
            });
            return result;
        }


        private List<UserLogProtocolData> UnAssignUsersFromAccessGroup(AccessGroupDevice accessGroupDevice, List<User> users)
        {
            var userLogsProtocol = new List<UserLogProtocolData>();

            //Add user log data
            var userLogs = _accessGroupService.AddUserLog(accessGroupDevice.IcuId, accessGroupDevice.TzId, users, ActionType.Delete);

            //Add user log protocol data
            var userLogProtocol = new UserLogProtocolData
            {
                IcuAddress = accessGroupDevice.Icu.DeviceAddress,
                UserLogs = userLogs,
                ProtocolType = Constants.Protocol.DeleteUser
            };
            userLogsProtocol.Add(userLogProtocol);
            
            return userLogsProtocol;
        }

        private string GetTimezoneSendToDevice(int deviceId)
        {
            string timezone = Constants.DefaultTimeZone;
            try
            {
                var deviceDefault = _unitOfWork.IcuDeviceRepository.GetById(deviceId);
                timezone = _unitOfWork.BuildingRepository.GetById(deviceDefault.BuildingId.Value).TimeZone;
            }
            catch (Exception e)
            {
                _logger.LogError($"Can not get building by device - deviceId = {deviceId}");
            }

            return timezone;
        }
    }
}