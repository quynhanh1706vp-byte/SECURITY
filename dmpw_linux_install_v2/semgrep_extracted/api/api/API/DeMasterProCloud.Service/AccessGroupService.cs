using AutoMapper;
using Bogus.Extensions;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.AccessGroup;
using DeMasterProCloud.DataModel.AccessGroupDevice;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service.Infrastructure;
using DeMasterProCloud.Service.Protocol;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.Service.RabbitMqQueue;
using Node = DeMasterProCloud.DataModel.Device.Node;
using DeMasterProCloud.DataModel.Header;
using System.Threading.Tasks;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.DataModel.DeviceSDK;

namespace DeMasterProCloud.Service
{
    public interface IAccessGroupService
    {
        List<AccessGroupListModel> GetPaginated(string filter, List<int> doorIds, List<int> userIds, int pageNumber,
            int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        List<AccessGroupDeviceDoor> GetPaginatedForDoors(int accessGroupId, string filter, List<int> operationType,
            int pageNumber,
            int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        List<AccessGroupDeviceUnAssignDoor> GetPaginatedForUnAssignDoors(int accessGroupId, string filter,
            List<int> operationType, List<int> buildingIds,
            int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        List<AccessGroupDeviceAssignDoor> GetPaginatedForAssignDoors(int accessGroupId, string filter,
            List<int> operationType,
            int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        List<Node> GetPaginatedAllDoorsForVisit(int accessGroupId, string filter,
            int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        List<UserForAccessGroup> GetPaginatedForUsers(int accessGroupId, string filter,
            int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered, out List<HeaderData> userHeader, string pageName,
            bool isAssigned = true);

        AccessGroup GetById(int accessGroupId);

        List<AccessGroup> GetByIds(List<int> accessGroupIds);

        List<AccessGroup> GetListAccessGroups();
        List<AccessGroup> GetListAccessGroupsExceptForVisitor();

        void Add(AccessGroupModel model);
        AccessGroup AddForVisitor(AccessGroupModel model);

        void Update(AccessGroupModel model);

        void Delete(AccessGroup accessGroup);

        void DeleteRange(List<AccessGroup> accessLevels);

        string AssignUsers(int accessGroupId, List<int> userIds);

        string AssignDoors(int accessGroupId, AccessGroupAssignDoor model, bool isPublish = true);

        void UnAssignUsers(int accessGroupId, List<int> userIds);

        void UnAssignDoors(int accessGroupId, List<int> doorIds, bool isDeleteAgd = true);

        void UnAssignAllDoors(int companyId, int accessGroupId);

        bool HasExistName(int accessGroupId, string name);

        string GetAGNameCanNotDelete(List<int> accessGroupId);
        void SendIdentificationToDevice(AccessGroupDevice agDevice, User user, Card card, bool isAdd);
        void SendIdentificationToDevice(List<AccessGroupDevice> agDevices, User user, Card card, bool isAdd);
        void SendIdentificationToDeviceVisitor(AccessGroupDevice agDevice, Visit visitor, Card card, bool isAdd);

        List<CardModel> GetCardListByUserId(int userId);

        void SendAddOrDeleteUser(string deviceAddress , IEnumerable<List<UserLog>> userLogs, bool isAddUser = true);

        void SendVisitor(AccessGroupDevice accessGroupDevice, bool isAddUser = true, Visit visit = null);

        List<UserLog> AddUserLog(int icuId, int tzId, List<User> users, ActionType actionType, string processId = null,
            decimal progressStart = 0, decimal progressRange = 0, string textFloorIds = null);

        IEnumerable<List<UserLog>> MakeUserLogData(IcuDevice device, List<int> userIds = null,
            List<int> visitIds = null, bool includeInvalid = false);

        IEnumerable<List<UserLog>> MakeUserLogData(IcuDevice device, User user, bool includeInvalid = false);
        void RemoveDeviceFromAllAccessGroup(List<int> doorIds);
        void AddAccessGroupWithAccessTime(List<string> accessTimeNames);

        bool CheckUserInDepartment(int companyId, int accountId, List<int> users);
        bool IsFullAccessGroup(int companyId, int accessGroupId);
    }

    public class AccessGroupService : IAccessGroupService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly HttpContext _httpContext;
        private readonly IDeviceSDKService _deviceSdkService;
        private readonly ISettingService _settingService;
        private readonly IMapper _mapper;

        public AccessGroupService(IUnitOfWork unitOfWork,
            IDeviceSDKService deviceSdkService, IConfiguration configuration,
            ILogger<AccessGroupService> logger, IHttpContextAccessor contextAccessor, ISettingService settingService)
        {
            _unitOfWork = unitOfWork;
            _deviceSdkService = deviceSdkService;
            _configuration = configuration;
            _logger = logger;
            if (contextAccessor != null)
                _httpContext = contextAccessor.HttpContext;
            _settingService = settingService;
            _mapper = MapperInstance.Mapper;
        }

        /// <summary>
        /// Get data with pagination
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public List<AccessGroupListModel> GetPaginated(string filter, List<int> doorIds, List<int> userIds,
            int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();

            // Case account type is dynamic role department level
            var accessGroupId = new List<int>();
            if (_httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
                accessGroupId =
                    _unitOfWork.AccessGroupRepository.GetAccessGroupIdByAccountDepartmentRole(companyId, accountId);

            var data = _unitOfWork.AppDbContext.AccessGroup.Where(m =>
                m.IsDeleted == false && m.CompanyId == companyId && m.Type != (short)AccessGroupType.VisitAccess &&
                m.Type != (short)AccessGroupType.PersonalAccess
                && (!accessGroupId.Any() || accessGroupId.Contains(m.Id)));

            // doorIds
            if (doorIds != null && doorIds.Count > 0)
            {
                data = data.Where(x => x.AccessGroupDevice.Any(m => doorIds.Contains(m.IcuId)));
            }

            // userIds
            if (userIds != null && userIds.Count > 0)
            {
                data = data.Where(x => x.User.Any(m => userIds.Contains(m.Id)));
            }


            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                var normalizedFilter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x => x.Name.RemoveDiacritics().ToLower().Contains(normalizedFilter)).AsQueryable();
            }

            recordsFiltered = data.Count();

            var resultList = data
                .Select(m => new AccessGroupListModel()
                {
                    Id = m.Id,
                    Name = m.Name,
                    IsDefault = m.IsDefault,
                    Type = m.Type,
                });

            resultList = Helpers.SortData<AccessGroupListModel>(resultList.AsEnumerable<AccessGroupListModel>(),
                sortDirection, sortColumn);

            // Get Default + Basic Access Group
            var basicAGs = resultList.Where(m =>
                    m.IsDefault || m.Type == (short)AccessGroupType.FullAccess ||
                    m.Type == (short)AccessGroupType.NoAccess)
                .ToList();
            // Get only normal access groups
            var normalAGs = resultList.Where(m => !m.IsDefault && m.Type == (short)AccessGroupType.NormalAccess);
            // Pagination
            var resultData = normalAGs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            if (basicAGs != null && basicAGs.Any())
            {
                pageSize -= basicAGs.Count;
                resultData = normalAGs.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                // Reverse original ordered.
                resultData = Enumerable.Reverse(resultData).ToList();
                // Add default AG to last of the reversed list.
                resultData.AddRange(basicAGs);
                // Reverse again
                resultData = Enumerable.Reverse(resultData).ToList();
                pageSize += basicAGs.Count;
            }

            // total users and total doors of access group
            var statusUsers = new List<int>() { (int)Status.Valid };
            foreach (var item in resultData)
            {
                var accessGroupIds = _unitOfWork.AccessGroupRepository
                    .Gets(m => m.Id == item.Id || m.ParentId == item.Id)
                    .Select(m => m.Id).ToList();
                item.TotalUsers = _unitOfWork.UserRepository.Count(m =>
                    !m.IsDeleted && accessGroupIds.Contains(m.AccessGroupId) && statusUsers.Contains(m.Status));
                item.TotalDoors = _unitOfWork.AccessGroupRepository.GetDevicesByAccessGroupId(item.Id).Count;
            }

            return resultData;
        }

        /// <summary>
        /// Get data with pagination
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public List<AccessGroupDeviceDoor> GetPaginatedForDoors(int accessGroupId, string filter,
            List<int> operationType,
            int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();

            var accessGroup = _unitOfWork.AccessGroupRepository.GetByIdAndCompanyId(companyId, accessGroupId);
            if (accessGroup == null)
            {
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<AccessGroupDeviceDoor>();
            }

            if (accessGroup.ParentId != null)
            {
                accessGroup =
                    _unitOfWork.AccessGroupRepository.GetByIdAndCompanyId(companyId, accessGroup.ParentId.Value);
            }

            if (operationType == null || operationType.Count <= 0)
            {
                operationType = new List<int>()
                {
                    (int)OperationType.Entrance,
                    (int)OperationType.FireDetector
                };
            }

            var data = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, accessGroupId)
                .Where(agd => operationType.Contains(agd.Icu.OperationType))
                .AsEnumerable<AccessGroupDevice>().Select(_mapper.Map<AccessGroupDeviceDoor>).AsQueryable();

            var accountTypeTemp = _httpContext.User.GetAccountType();
            var accountIdTemp = _httpContext.User.GetAccountId();
            var companyIdTemp = _httpContext.User.GetCompanyId();

            // check plugin
            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyIdTemp);
            if (plugin != null)
            {
                PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
                if (plugIns.DepartmentAccessLevel && accountTypeTemp == (short)AccountType.DynamicRole)
                {
                    List<int> doorIdList =
                        _unitOfWork.DepartmentDeviceRepository.GetDoorIdsByAccountDepartmentRole(companyIdTemp,
                            accountIdTemp);
                    data = data.Where(x => doorIdList.Contains(x.Id));
                }
            }


            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                var normalizedFilter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x => (x.DeviceAddress?.RemoveDiacritics()?.ToLower()?.Contains(normalizedFilter) == true) ||
                                       (x.DoorName?.RemoveDiacritics()?.ToLower()?.Contains(normalizedFilter) == true) ||
                                       (x.Timezone?.RemoveDiacritics()?.ToLower()?.Contains(normalizedFilter) == true) ||
                                       (x.Building?.RemoveDiacritics()?.ToLower()?.Contains(normalizedFilter) == true)).AsQueryable();
            }

            recordsFiltered = data.Count();

            // Default sort ( asc - DoorName )
            data = data.OrderBy(c => c.DoorName);

            data = data.OrderBy($"{sortColumn} {sortDirection}");
            if (pageSize >= 0)
            {
                data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }

            var accessGroupList = data.ToList();

            var agDevices = accessGroup.AccessGroupDevice.Select(m => m.Icu.DeviceAddress).ToList();
            foreach (var eachData in accessGroupList)
            {
                if (agDevices.Contains(eachData.DeviceAddress))
                {
                    eachData.IsParent = true;
                }
                else
                {
                    eachData.IsParent = false;
                }
            }

            return accessGroupList;
        }

        /// <summary>
        /// Get data with pagination
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="filter"></param>
        /// <param name="operationType"></param>
        /// <param name="buildingIds"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public List<AccessGroupDeviceUnAssignDoor> GetPaginatedForUnAssignDoors(int accessGroupId, string filter,
            List<int> operationType, List<int> buildingIds,
            int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();
            var dataList = new List<AccessGroupDeviceUnAssignDoor>();

            if (operationType == null || operationType.Count <= 0)
            {
                operationType = new List<int>()
                {
                    (int)OperationType.Entrance,
                    (int)OperationType.FireDetector
                };
            }

            var query = CheckPluginDepartmentAccessLevel() &&
                        _httpContext.User.GetAccountType() == (short)AccountType.DynamicRole
                ? _unitOfWork.IcuDeviceRepository
                    .GetUnAssignDevicesByDepartment(companyId, accessGroupId,
                        _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId))
                : _unitOfWork.IcuDeviceRepository.GetUnAssignDevicesByCompany(companyId, accessGroupId);

            query = query.Where(d => operationType.Contains(d.OperationType));
            totalRecords = query.Count();
            
            if (buildingIds != null && buildingIds.Any())
            {
                query = query.Where(d => buildingIds.Contains(d.BuildingId ?? 0));
            }

            dataList = query.AsEnumerable<IcuDevice>()
                .Select(_mapper.Map<AccessGroupDeviceUnAssignDoor>)
                .ToList();


            var data = dataList.AsQueryable();

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                var normalizedFilter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x => (x.DeviceAddress?.RemoveDiacritics()?.ToLower()?.Contains(normalizedFilter) == true) ||
                                       (x.DoorName?.RemoveDiacritics()?.ToLower()?.Contains(normalizedFilter) == true) ||
                                       (x.Building?.RemoveDiacritics()?.ToLower()?.Contains(normalizedFilter) == true)).AsQueryable();
            }

            recordsFiltered = data.Count();

            data = data.OrderBy($"{sortColumn} {sortDirection}");
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return data.ToList();
        }

        /// <summary>
        /// Get data with pagination
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public List<AccessGroupDeviceAssignDoor> GetPaginatedForAssignDoors(int accessGroupId, string filter,
            List<int> operationType,
            int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();
            var dataList = new List<AccessGroupDeviceAssignDoor>();

            if (operationType == null || operationType.Count <= 0)
            {
                operationType = new List<int>()
                {
                    (int)OperationType.Entrance,
                    (int)OperationType.FireDetector
                };
            }

            if (CheckPluginDepartmentAccessLevel() &&
                _httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
            {
                var departmentOfUserLoggin =
                    _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
                dataList = _unitOfWork.IcuDeviceRepository
                    .GetAssignDevicesByDepartment(companyId, accessGroupId, departmentOfUserLoggin)
                    .Where(d => operationType.Contains(d.OperationType))
                    .AsEnumerable<IcuDevice>().Select(_mapper.Map<AccessGroupDeviceAssignDoor>).ToList();
            }
            else
            {
                dataList = _unitOfWork.IcuDeviceRepository.GetAssignDevicesByCompany(companyId, accessGroupId)
                    .Where(d => operationType.Contains(d.OperationType))
                    .AsEnumerable<IcuDevice>().Select(_mapper.Map<AccessGroupDeviceAssignDoor>).ToList();
            }

            var data = dataList.AsQueryable();

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                var normalizedFilter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x => (x.DeviceAddress?.RemoveDiacritics()?.ToLower()?.Contains(normalizedFilter) == true) ||
                                       (x.DoorName?.RemoveDiacritics()?.ToLower()?.Contains(normalizedFilter) == true) ||
                                       (x.Building?.RemoveDiacritics()?.ToLower()?.Contains(normalizedFilter) == true)).AsQueryable();
            }

            recordsFiltered = data.Count();

            data = data.OrderBy($"{sortColumn} {sortDirection}");
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return data.ToList();
        }

        /// <summary>
        /// Get unassignDoor data with pagination for visit
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public List<Node> GetPaginatedAllDoorsForVisit(int accessGroupId, string filter,
            int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var data = _unitOfWork.IcuDeviceRepository.GetActiveDevicesByCompany(companyId);

            totalRecords = data.Count();

            var treeData = _unitOfWork.IcuDeviceRepository.GetAGDeviceHierarchy(data.ToList(), accessGroupId);

            if (!string.IsNullOrEmpty(filter))
            {
                var normalizedFilter = filter.Trim().RemoveDiacritics().ToLower();
                treeData = treeData.Where(x => x.BuildingName.RemoveDiacritics().ToLower().Contains(normalizedFilter));
            }

            if (treeData != null)
            {
                recordsFiltered = treeData.Count();
                treeData = treeData.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            }
            else
            {
                recordsFiltered = 0;
                treeData = new List<Node>();
            }

            return treeData.ToList();
        }

        /// <summary>
        /// Get data with pagination
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public List<UserForAccessGroup> GetPaginatedForUsers(int accessGroupId, string filter,
            int pageNumber, int pageSize, string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered, out List<HeaderData> userHeader, string pageName,
            bool isAssigned = true)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();

            var plugIns = _unitOfWork.AppDbContext.PlugIn.Where(x => x.CompanyId == companyId).Select(x => x.PlugIns)
                .FirstOrDefault();
            var value = JsonConvert.DeserializeObject<PlugIns>(plugIns);

            var accessGroups = _unitOfWork.AppDbContext.AccessGroup
                .Where(m => m.Id == accessGroupId || m.ParentId == accessGroupId)
                .Select(m => m.Id).ToList();

            var data = _unitOfWork.UserRepository.GetByCompanyId(companyId, new List<int>() { (int)Status.Valid });

            if (isAssigned)
                data = data.Where(m => accessGroups.Contains(m.AccessGroupId));
            else
            {
                var isFullAccessGroup =
                    _unitOfWork.AccessGroupRepository.GetFullAccessGroup(_httpContext.User.GetCompanyId()).Id ==
                    accessGroupId;

                data = isFullAccessGroup
                    ? _unitOfWork.UserRepository.GetUnAssignUsersForMasterCard(companyId, accessGroupId)
                    : _unitOfWork.UserRepository.GetUnAssignUsers(companyId, accessGroupId);

                data = data.Where(m => m.Status == (int)Status.Valid);
            }

            // check account type dynamic role enable department role
            var accountType = _httpContext.User.GetAccountType();
            if (accountType == (short)AccountType.DynamicRole)
            {
                var departmentIds =
                    _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
                if (departmentIds.Any())
                {
                    data = data.Where(x => departmentIds.Contains(x.DepartmentId));
                }
            }

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                var normalizedFilter = filter.Trim().RemoveDiacritics().ToLower();

                var userIdsFilteredByCardId = _unitOfWork.CardRepository.GetFilteredCards(companyId, filter)
                    .Select(c => c.UserId).ToList();

                data = data.AsEnumerable().Where(x => (x.FirstName + " " + (x.LastName ?? "")).Trim().RemoveDiacritics().ToLower().Contains(normalizedFilter)).AsQueryable();
            }

            recordsFiltered = data.Count();

            // Get header data
            userHeader = new List<HeaderData>();

            if (!string.IsNullOrEmpty(pageName))
            {
                string[] arrayHeader = ColumnDefines.UserHeader;
                if (!isAssigned)
                {
                    arrayHeader = ColumnDefines.UnAssignedUserHeader;
                }

                userHeader = _settingService.GetUserHeaderData(pageName, arrayHeader, companyId, accountId);
            }

            // Convert time according to accountTimezone
            var accountTimezone = _unitOfWork.AccountRepository.GetById(accountId).TimeZone;
            var offSet = accountTimezone.ToTimeZoneInfo().BaseUtcOffset;

            var resultList = data
                .Select(m => new UserForAccessGroup()
                {
                    Id = m.Id,
                    Avatar = m.Avatar,
                    UserCode = m.UserCode,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    DepartmentName = m.Department.DepartName ?? "",
                    Position = m.Position,
                    ExpiredDate = m.ExpiredDate.HasValue
                        ? m.ExpiredDate.Value.ConvertToUserTime(offSet)
                            .ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault)
                        : DateTime.UtcNow.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault),
                    WorkTypeName = m.WorkType != null ? ((WorkType)m.WorkType).GetDescription() : "",
                    AccessGroupName = (m.AccessGroup != null && m.AccessGroup.Parent == null)
                        ? m.AccessGroup.Name
                        : $"{m.AccessGroup.Parent.Name} *",
                    ApprovalStatus = ((ApprovalStatus)m.ApprovalStatus).GetDescription(),
                    EmployeeNo = m.EmpNumber
                });

            var sortHeader = userHeader.FirstOrDefault(m => m.HeaderVariable.Equals(sortColumn.ToPascalCase()));

            if (!string.IsNullOrEmpty(sortColumn) && totalRecords > 0)
            {
                if (sortHeader != null && !sortHeader.IsCategory)
                {
                    resultList = Helpers.SortData<UserForAccessGroup>(resultList.AsEnumerable<UserForAccessGroup>(),
                        sortDirection, sortColumn);
                }
                else if (sortHeader != null && sortHeader.IsCategory)
                {
                    if (Int32.TryParse(sortColumn, out int columnId))
                    {
                        var result = resultList.ToList();

                        if (sortDirection.Equals("asc"))
                            result = result
                                .OrderBy(
                                    m => m.CategoryOptions.FirstOrDefault(c => c.Category.Id == columnId)?.OptionName,
                                    new NullOrEmptyStringReducer()).ToList();
                        else
                            result = result.OrderByDescending(m =>
                                m.CategoryOptions.FirstOrDefault(c => c.Category.Id == columnId)?.OptionName).ToList();

                        resultList = result.AsQueryable();
                    }
                }
            }

            var resultData = resultList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // Set cardId information to each users
            foreach (var user in resultData)
            {
                var cardList = GetCardListByUserId(user.Id);
                user.CardList = cardList.Where(c => c.CardType != (int)CardType.VehicleId && c.CardType != (int)CardType.VehicleMotoBikeId).ToList();
                user.PlateNumberList = cardList.Where(c => c.CardType == (int)CardType.VehicleId || c.CardType == (int)CardType.VehicleMotoBikeId).ToList();
            }

            return resultData;
        }

        /// <summary>
        /// Get list access group by list id and company
        /// </summary>
        /// <param name="accessGroupIds"></param>
        /// <returns></returns>
        public List<AccessGroup> GetByIds(List<int> accessGroupIds)
        {
            try
            {
                return _unitOfWork.AccessGroupRepository.GetByIdsAndCompanyId(_httpContext.User.GetCompanyId(),
                    accessGroupIds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIds");
                return new List<AccessGroup>();
            }
        }

        /// <summary>
        /// Get access group by id and company
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <returns></returns>
        public AccessGroup GetById(int accessGroupId)
        {
            try
            {
                return _unitOfWork.AccessGroupRepository.GetByIdAndCompanyId(_httpContext.User.GetCompanyId(),
                    accessGroupId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return null;
            }
        }

        /// <summary>
        /// Get list access group is valid
        /// </summary>
        /// <returns></returns>
        public List<AccessGroup> GetListAccessGroups()
        {
            try
            {
                return _unitOfWork.AccessGroupRepository.GetListAccessGroups(_httpContext.User.GetCompanyId());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetListAccessGroups");
                return new List<AccessGroup>();
            }
        }

        public List<AccessGroup> GetListAccessGroupsExceptForVisitor()
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var accountId = _httpContext.User.GetAccountId();
                var accountType = _httpContext.User.GetAccountType();
                var accessGroups = _unitOfWork.AccessGroupRepository.GetListAccessGroups(_httpContext.User.GetCompanyId());

                // check permission account type dynamic role department level
                var accessGroupId = new List<int>();
                if (accountType == (short)AccountType.DynamicRole)
                {
                    accessGroupId =
                        _unitOfWork.AccessGroupRepository.GetAccessGroupIdByAccountDepartmentRole(companyId, accountId);
                }

                accessGroups = accessGroups.AsQueryable().Where(m =>
                    m.Type != (short)AccessGroupType.VisitAccess
                    && m.Type != (short)AccessGroupType.PersonalAccess
                    && (!accessGroupId.Any() || accessGroupId.Contains(m.Id))).ToList();

                return accessGroups;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetListAccessGroupsExceptForVisitor");
                return new List<AccessGroup>();
            }
        }

        /// <summary>
        /// Add a access group
        /// </summary>
        /// <param name="model"></param>
        public void Add(AccessGroupModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var accessGroup = _mapper.Map<AccessGroup>(model);
                        accessGroup.CompanyId = _httpContext.User.GetCompanyId();
                        accessGroup.Type = (short)AccessGroupType.NormalAccess;
                        _unitOfWork.AccessGroupRepository.Add(accessGroup);
                        _unitOfWork.Save();

                        var content = $"{AccessGroupResource.msgAddAccessGroup}";
                        List<string> details = new List<string>();
                        details.Add($"{AccessGroupResource.lblAccessGroupName} : {accessGroup.Name}");

                        if (accessGroup.IsDefault)
                        {
                            SetAllAccessGroupToNotDefault(accessGroup.CompanyId, accessGroup.Id);
                            details.Add(
                                string.Format(AccessGroupResource.msgChangeDefaultAccessGroup, accessGroup.Name));
                        }

                        var contentsDetail = string.Join("\n", details);

                        // save system log
                        _unitOfWork.SystemLogRepository.Add(accessGroup.Id, SystemLogType.AccessGroup,
                            ActionLogType.Add,
                            content, contentsDetail, null, _httpContext.User.GetCompanyId());

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


        /// <summary>
        /// Add a access group for visitor
        /// </summary>
        /// <param name="model"></param>
        public AccessGroup AddForVisitor(AccessGroupModel model)
        {
            AccessGroup accessGroup = null;

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        accessGroup = _mapper.Map<AccessGroup>(model);
                        var companyId = _httpContext.User.GetCompanyId();

                        accessGroup.CompanyId = companyId;
                        accessGroup.Type = (short)AccessGroupType.VisitAccess;
                        accessGroup.Name = Constants.Settings.NameAccessGroupVisitor + "_" + accessGroup.Name +
                                           DateTime.UtcNow;
                        accessGroup.IsDefault = false;

                        _unitOfWork.AccessGroupRepository.Add(accessGroup);
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

            return accessGroup;
        }


        /// <summary>
        /// update a access group
        /// </summary>
        /// <param name="model"></param>
        public void Update(AccessGroupModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var companyId = _httpContext.User.GetCompanyId();

                        var accessGroup = _unitOfWork.AccessGroupRepository.GetByIdAndCompanyId(companyId, model.Id);
                        var existAccessGroupName = accessGroup.Name;
                        var existDefaultStatus = accessGroup.IsDefault;

                        _mapper.Map(model, accessGroup);
                        _unitOfWork.AccessGroupRepository.Update(accessGroup);
                        _unitOfWork.Save();

                        // Save system log
                        // This list contains all the changes.
                        List<string> changes = new List<string>();
                        if (existAccessGroupName != accessGroup.Name)
                        {
                            // AccessGroup name is changed.
                            changes.Add(string.Format(AccessGroupResource.msgChangeAccessGroupName,
                                existAccessGroupName, accessGroup.Name));
                        }

                        if (existDefaultStatus == false && accessGroup.IsDefault == true)
                        {
                            // This AccessGroup has become Default Access Group.
                            SetAllAccessGroupToNotDefault(companyId, accessGroup.Id);
                            changes.Add(
                                string.Format(AccessGroupResource.msgChangeDefaultAccessGroup, accessGroup.Name));
                        }
                        else if (existDefaultStatus == true && accessGroup.IsDefault == false)
                        {
                            // This AccessGroup has bacome normal AccessGroup from Default AccessGroup.
                            // So, System should set the other AccessGroup as Default AccessGroup.
                            var fullAccessGroup = _unitOfWork.AccessGroupRepository.GetFullAccessGroup(companyId);
                            fullAccessGroup.IsDefault = true;

                            changes.Add(string.Format(AccessGroupResource.msgChangeDefaultAccessGroup,
                                fullAccessGroup.Name));

                            _unitOfWork.AccessGroupRepository.Update(fullAccessGroup);
                            _unitOfWork.Save();
                        }

                        var content = $"{AccessGroupResource.msgChangeAccessGroup}";
                        var contentsDetail = $"{string.Join("<br />", changes)}";
                        _unitOfWork.SystemLogRepository.Add(accessGroup.Id, SystemLogType.AccessGroup,
                            ActionLogType.Update,
                            content, contentsDetail, null, companyId);

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

        /// <summary>
        /// Delete a access group
        /// </summary>
        /// <param name="accessGroup"></param>
        public void Delete(AccessGroup accessGroup)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        _unitOfWork.AccessGroupRepository.DeleteFromSystem(accessGroup);

                        // Delete access group device of this ag
                        var agDevices =
                            _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(accessGroup.CompanyId,
                                accessGroup.Id, null, false);
                        _unitOfWork.AccessGroupDeviceRepository.DeleteRange(agDevices);

                        // Delete child PAG (Set IsDeleted = TRUE)
                        var childPAGs = _unitOfWork.AppDbContext.AccessGroup.Include(a => a.User).Where(a =>
                            a.ParentId == accessGroup.Id && !a.User.Any(user => !user.IsDeleted)).ToList();
                        if (childPAGs != null && childPAGs.Count != 0)
                        {
                            _unitOfWork.AccessGroupRepository.DeleteRangeFromSystem(childPAGs);
                            foreach (var childPAG in childPAGs)
                            {
                                // Delete access group device of this ag
                                var agDevicesChildPAG =
                                    _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(childPAG.CompanyId,
                                        childPAG.Id, null, false);
                                _unitOfWork.AccessGroupDeviceRepository.DeleteRange(agDevicesChildPAG);
                            }
                        }

                        //Save system log
                        var content = $"{AccessGroupResource.msgDeleteAccessGroup}";
                        var contentDetails = $"{AccessGroupResource.lblAccessGroupName} : {accessGroup.Name}";

                        _unitOfWork.SystemLogRepository.Add(accessGroup.Id, SystemLogType.AccessGroup,
                            ActionLogType.Delete,
                            content, contentDetails, null, _httpContext.User.GetCompanyId());
                        //Save to database
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

            // Do we need the below code?
            //UnAssignUserAndAddToDefaultAccessGroup(new List<int> { accessGroup.Id });
            //UnAssigrDoorAndAddToDefaultAccessGroup(new List<int> { accessGroup.Id });
        }

        /// <summary>
        /// Delete a list access group
        /// </summary>
        /// <param name="accessGroups"></param>
        public void DeleteRange(List<AccessGroup> accessGroups)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var accessGroup in accessGroups)
                        {
                            _unitOfWork.AccessGroupRepository.DeleteFromSystem(accessGroup);

                            // Delete access group device of this ag
                            var agDevices =
                                _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(accessGroup.CompanyId,
                                    accessGroup.Id, null, false);
                            _unitOfWork.AccessGroupDeviceRepository.DeleteRange(agDevices);

                            // Delete child PAG (Set IsDeleted = TRUE)
                            var childPAGs = _unitOfWork.AppDbContext.AccessGroup.Include(a => a.User).Where(a =>
                                a.ParentId == accessGroup.Id && !a.User.Any(user => !user.IsDeleted)).ToList();
                            if (childPAGs != null && childPAGs.Count != 0)
                            {
                                _unitOfWork.AccessGroupRepository.DeleteRangeFromSystem(childPAGs);
                                foreach (var childPAG in childPAGs)
                                {
                                    // Delete access group device of this ag
                                    var agDevicesChildPAG =
                                        _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(childPAG.CompanyId,
                                            childPAG.Id, null, false);
                                    _unitOfWork.AccessGroupDeviceRepository.DeleteRange(agDevicesChildPAG);
                                }
                            }
                        }

                        //Save system log
                        if (accessGroups.Count == 1)
                        {
                            var accessGroup = accessGroups.First();
                            var content = $"{AccessGroupResource.msgDeleteAccessGroup}";
                            var contentDetails = $"{AccessGroupResource.lblAccessGroupName} : {accessGroup.Name}";

                            _unitOfWork.SystemLogRepository.Add(accessGroup.Id, SystemLogType.AccessGroup,
                                ActionLogType.Delete,
                                content, contentDetails, null, _httpContext.User.GetCompanyId());
                        }
                        else
                        {
                            var content = $"{AccessGroupResource.msgDeleteAccessGroup}";

                            var accessGroupIds = accessGroups.Select(c => c.Id).ToList();
                            var accessGroupNames = accessGroups.Select(c => c.Name).ToList();

                            var contentDetails =
                                $"{AccessGroupResource.lblAccessGroupCount} : {accessGroups.Count}<br />" +
                                $"{AccessGroupResource.lblAccessGroupName} : {string.Join(", ", accessGroupNames)}";

                            _unitOfWork.SystemLogRepository.Add(accessGroupIds.First(), SystemLogType.AccessGroup,
                                ActionLogType.DeleteMultiple,
                                content, contentDetails, accessGroupIds, _httpContext.User.GetCompanyId());
                        }

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

            // Do we need the below code?
            //var agIds = accessGroups.Select(x => x.Id).ToList();
            //UnAssignUserAndAddToDefaultAccessGroup(agIds);
            //UnAssigrDoorAndAddToDefaultAccessGroup(agIds);
        }

        /// <summary>
        /// Assign list users
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="userIds"></param>
        public string AssignUsers(int accessGroupId, List<int> userIds)
        {
            var returnValue = string.Empty;
            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();
            var sender = _httpContext.User.GetUsername();
            var company = _unitOfWork.CompanyRepository.GetById(companyId);

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(async () =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        IWebSocketService webSocketService = new WebSocketService();
                        AccessControlQueue accessControlQueue = new AccessControlQueue(_unitOfWork, webSocketService);
                        var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, webSocketService);
                        var usersToAssign = _unitOfWork.UserRepository.GetByIds(companyId, userIds);
                        var assignedUserNames = usersToAssign.Select(m =>
                                $"{m.FirstName} ({(m.AccessGroup != null ? m.AccessGroup.Type != (short)AccessGroupType.PersonalAccess ? m.AccessGroup.Name : (m.AccessGroup.Parent?.Name + " *") : "")})")
                            .ToList();

                        // send delete user from old access group
                        var oldAccessGroupIds = usersToAssign.Select(m => m.AccessGroupId).Distinct().ToList();

                        int version = 3;
                        if (version == 1)
                        {
                            foreach (int oldAccessGroupId in oldAccessGroupIds)
                            {
                                var sendUserIds = usersToAssign.Where(m => m.AccessGroupId == oldAccessGroupId)
                                    .Select(m => m.Id).ToList();
                                var devices =
                                    _unitOfWork.AccessGroupRepository.GetDevicesByAccessGroupId(oldAccessGroupId);
                                deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                                {
                                    DeviceIds = devices.Select(m => m.Id).ToList(),
                                    MessageType = Constants.Protocol.DeleteUser,
                                    MsgId = Guid.NewGuid().ToString(),
                                    Sender = sender,
                                    UserIds = sendUserIds,
                                    CompanyCode = company?.Code,
                                });
                            }
                        }
                        else if (version == 2)
                        {
                            // Get list of devices that should receive DELETE message.
                            var devices = _unitOfWork.AppDbContext.AccessGroupDevice.Include(agd => agd.Icu)
                                .Where(agd => oldAccessGroupIds.Contains(agd.AccessGroupId)).Select(agd => agd.Icu)
                                .Distinct().ToList();
                            deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                            {
                                DeviceIds = devices.Select(m => m.Id).ToList(),
                                MessageType = Constants.Protocol.DeleteUser,
                                MsgId = Guid.NewGuid().ToString(),
                                Sender = sender,
                                UserIds = userIds,
                                CompanyCode = company?.Code,
                            });
                        }
                        else if (version == 3)
                        {
                            var threadResult = new ManualResetEvent(false);
                            new Thread(async () =>
                            {
                                try
                                {
                                    // Get list of devices that should receive DELETE message.
                                    var devices = _unitOfWork.AppDbContext.AccessGroupDevice.Include(agd => agd.Icu)
                                        .Where(agd => oldAccessGroupIds.Contains(agd.AccessGroupId))
                                        .Select(agd => agd.Icu).Distinct().ToList();
                                    foreach (var oldDevice in devices)
                                    {
                                        Console.WriteLine($"[OLD] Device address : {oldDevice.DeviceAddress}");
                                    }

                                    await SendCardToDevice(userIds, devices, sender, Constants.Protocol.DeleteUser);
                                }
                                catch (Exception ex)
                                {
                                }
                                finally
                                {
                                    // Báo hiệu rằng quá trình import đã hoàn thành
                                    threadResult.Set();
                                }
                            }).Start();
                            threadResult.WaitOne();
                        }

                        // change access group of all user
                        List<AccessGroup> toBeDeletedAG = new List<AccessGroup>();
                        foreach (var user in usersToAssign)
                        {
                            if (user.AccessGroup.Type == (short)AccessGroupType.PersonalAccess)
                                toBeDeletedAG.Add(user.AccessGroup);

                            user.AccessGroupId = accessGroupId;
                            _unitOfWork.UserRepository.Update(user);
                            //_unitOfWork.Save();
                        }

                        // delete PAG
                        if (toBeDeletedAG != null && toBeDeletedAG.Any())
                        {
                            // Delete if an old access group is PAG.
                            _unitOfWork.AccessGroupRepository.DeleteRange(toBeDeletedAG);
                        }

                        _unitOfWork.Save();

                        // save system log
                        var assignedUserIds = usersToAssign.Select(c => c.Id).ToList();
                        var newAccessGroup = _unitOfWork.AccessGroupRepository.GetById(accessGroupId);
                        var content = AccessGroupResource.msgAssignUsers;
                        var contentDetails =
                            $"{AccessGroupResource.lblAccessGroupName} : {newAccessGroup.Name}, <br />" +
                            $"{UserResource.lblUserCount} : {assignedUserIds.Count}, <br />" +
                            $"{UserResource.lblUser} ({AccessGroupResource.lblOldAccessGroupName}) :<br /> {string.Join("<br />", assignedUserNames)}";

                        _unitOfWork.SystemLogRepository.Add(accessGroupId, SystemLogType.AccessGroup,
                            ActionLogType.AssignUser, content, contentDetails, assignedUserIds, companyId);
                        _unitOfWork.Save();
                        transaction.Commit();

                        // send add user to new access group
                        var devicesInNewAccessGroup =
                            _unitOfWork.AccessGroupRepository.GetDevicesByAccessGroupId(accessGroupId);
                        foreach (var newDevice in devicesInNewAccessGroup)
                        {
                            Console.WriteLine($"[NEW] Device address : {newDevice.DeviceAddress}");
                        }

                        if (version == 1 || version == 2)
                        {
                            // device (new access group)
                            deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                            {
                                DeviceIds = devicesInNewAccessGroup.Select(m => m.Id).ToList(),
                                MessageType = Constants.Protocol.AddUser,
                                MsgId = Guid.NewGuid().ToString(),
                                Sender = sender,
                                UserIds = userIds,
                                CompanyCode = company?.Code,
                            });
                        }
                        else if (version == 3)
                        {
                            new Thread(async () =>
                            {
                                try
                                {
                                    SendCardToDevice(userIds, devicesInNewAccessGroup, sender,
                                        Constants.Protocol.AddUser);
                                }
                                catch (Exception ex)
                                {
                                }
                            }).Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex.Message + ex.StackTrace);
                        returnValue = ex.Message;
                    }
                }
            });

            return returnValue;
        }

        private async Task SendCardToDevice(List<int> userIds, List<IcuDevice> devices, string sender,
            string messageType = Constants.Protocol.AddUser)
        {
            try
            {
                IUnitOfWork unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                IWebSocketService webSocketService = new WebSocketService();
                var accessControlQueue = new AccessControlQueue(unitOfWork, webSocketService);
                var deviceInstructionQueue = new DeviceInstructionQueue(unitOfWork, _configuration, webSocketService);
                try
                {
                    var company = _unitOfWork.CompanyRepository.GetById(devices.First().CompanyId.Value);
                    // device
                    deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                    {
                        MessageType = messageType,
                        MsgId = Guid.NewGuid().ToString(),
                        Sender = sender,
                        UserIds = userIds,
                        CompanyCode = company.Code,
                        DeviceIds = devices.Select(m => m.Id).Distinct().ToList(),
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    unitOfWork.Dispose();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Assign list doors
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="model"></param>
        /// <param name="isPublish"></param>
        public string AssignDoors(int accessGroupId, AccessGroupAssignDoor model, bool isPublish = true)
        {
            var returnValue = string.Empty;
            var companyId = model.Doors.Select(c => c.CompanyId).FirstOrDefault();
            var doorIds = model.Doors.Select(c => c.DoorId).ToList();
            var icuDevicesToAssign = _unitOfWork.IcuDeviceRepository.GetByIds(doorIds);
            var company = _unitOfWork.CompanyRepository.GetById(companyId);

            var accessGroup = _unitOfWork.AppDbContext.AccessGroup
                .Include(x => x.User).ThenInclude(user => user.Department)
                .Include(x => x.User).ThenInclude(user => user.Card).Include(accessGroup1 => accessGroup1.Visit)
                .ThenInclude(visit => visit.Card)
                .FirstOrDefault(x => x.CompanyId == companyId && x.Id == accessGroupId && !x.IsDeleted);

            var childPersonalAccessGroups = _unitOfWork.AppDbContext.AccessGroup
                .Include(x => x.AccessGroupDevice)
                .Include(x => x.User).ThenInclude(user => user.Department)
                .Include(x => x.User).ThenInclude(user => user.Card)
                .Where(x => x.CompanyId == companyId && x.ParentId == accessGroupId && !x.IsDeleted).ToList();

            List<User> usersToAssign = new List<User>();
            List<Visit> visitorsToAssign = new List<Visit>();

            if (accessGroup?.User != null && accessGroup.User.Any())
            {
                usersToAssign = accessGroup.User.ToList();
            }

            if (accessGroup?.Visit != null && accessGroup.Visit.Any())
            {
                visitorsToAssign = accessGroup.Visit.ToList();
            }

            if (childPersonalAccessGroups.Any())
            {
                //List<User> users = new List<User>();
                foreach (var childPersonalAccessGroup in childPersonalAccessGroups)
                {
                    if (!childPersonalAccessGroup.AccessGroupDevice.Any(m => doorIds.Contains(m.IcuId)))
                    {
                        usersToAssign.AddRange(childPersonalAccessGroup.User.ToList());
                    }
                }
            }

            if (usersToAssign.Any())
            {
                usersToAssign = usersToAssign.Where(m => !m.IsDeleted && m.Status == (short)Status.Valid)
                    .Where(m => m.ApprovalStatus == (int)ApprovalStatus.Approved ||
                                m.ApprovalStatus == (int)ApprovalStatus.NotUse).ToList();
            }

            if (visitorsToAssign.Any())
            {
                visitorsToAssign = visitorsToAssign.Where(m => !m.IsDeleted
                                                               && (m.Status == (short)VisitChangeStatusType.Approved ||
                                                                   m.Status == (short)VisitChangeStatusType
                                                                       .AutoApproved)).ToList();
            }

            if (icuDevicesToAssign != null && icuDevicesToAssign.Any())
            {
                _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
                {
                    using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                    {
                        try
                        {
                            IWebSocketService webSocketService = new WebSocketService();
                            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, webSocketService);
                            var details = new List<string>();

                            List<int> deviceIdsForUser = new List<int>();
                            List<int> deviceIdsForVisitor = new List<int>();
                            foreach (var icuDevice in icuDevicesToAssign)
                            {
                                var tzId = model.Doors.First(c => c.DoorId == icuDevice.Id).TzId;

                                var timezone = _unitOfWork.AccessTimeRepository.GetById(tzId);
                                var tzName = timezone != null ? timezone.Name : string.Empty;
                                var detail = $"{icuDevice.Name} ( {tzName} )";
                                details.Add(detail);

                                //기존에 Access Group device에 있었나 확인해야함
                                //있었으면 지우고
                                //없으면 다음으로 넘어감
                                var existAccessGroupDevice =
                                    _unitOfWork.AccessGroupDeviceRepository.GetByIcuIdInOtherCompany(companyId,
                                        icuDevice.Id);

                                if (existAccessGroupDevice.Any())
                                {
                                    _unitOfWork.AccessGroupDeviceRepository.DeleteRange(existAccessGroupDevice);
                                }

                                var accessGroupDevice =
                                    _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupIdAndDeviceId(companyId,
                                        accessGroupId, icuDevice.Id);

                                if (accessGroupDevice == null)
                                {
                                    //Add access group device
                                    AddAccessGroupDevice(accessGroupId, icuDevice.Id, timezone?.Id ?? 0);
                                }

                                //if (accessGroup?.User != null && accessGroup.User.Any())
                                if (usersToAssign.Any())
                                {
                                    var maxDeviceUser = Helpers.GetMaximumUserCount(icuDevice.DeviceType);

                                    var cardCount = 0;

                                    usersToAssign.ForEach(m => cardCount += m.Card.Count(card => !card.IsDeleted));

                                    if (cardCount > maxDeviceUser)
                                    {
                                        returnValue = string.Format(AccessGroupResource.msgUnableToAssignOverMaxUser,
                                            maxDeviceUser,
                                            $"{icuDevice.Name} ({icuDevice.DeviceAddress})");

                                        _logger.LogWarning(returnValue);
                                        return;
                                    }

                                    if (isPublish)
                                    {
                                        deviceIdsForUser.Add(icuDevice.Id);
                                    }
                                }


                                if (visitorsToAssign.Any())
                                {
                                    var maxDeviceUser = Helpers.GetMaximumUserCount(icuDevice.DeviceType);

                                    var cardCount = 0;

                                    visitorsToAssign.ForEach(m => cardCount += m.Card.Count(card => !card.IsDeleted));

                                    if (cardCount > maxDeviceUser)
                                    {
                                        returnValue = string.Format(AccessGroupResource.msgUnableToAssignOverMaxUser,
                                            maxDeviceUser,
                                            $"{icuDevice.Name} ({icuDevice.DeviceAddress})");

                                        _logger.LogWarning(returnValue);
                                        return;
                                    }

                                    if (isPublish)
                                    {
                                        deviceIdsForVisitor.Add(icuDevice.Id);
                                    }
                                }
                            }

                            if (accessGroup != null && accessGroup.Type != (short)AccessGroupType.VisitAccess)
                            {
                                //Save system log
                                var assignedDoorIds = icuDevicesToAssign.Select(c => c.Id).ToList();
                                var content = AccessGroupResource.msgAssignDoors;
                                var contentDetails =
                                    $"{AccessGroupResource.lblAccessGroupName} : {accessGroup.Name}<br />" +
                                    $"{DeviceResource.lblDeviceCount} : {doorIds.Count}<br />" +
                                    $"{DeviceResource.lblDoorName}({AccessTimeResource.lblAccessTime}) :<br />{string.Join("<br />", details)}";

                                _unitOfWork.SystemLogRepository.Add(accessGroupId, SystemLogType.AccessGroup,
                                    ActionLogType.AssignDoor,
                                    content, contentDetails, assignedDoorIds, companyId);
                            }

                            _unitOfWork.Save();
                            transaction.Commit();

                            if (deviceIdsForUser.Any())
                            {
                                deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                                {
                                    DeviceIds = deviceIdsForUser.Distinct().ToList(),
                                    MessageType = Constants.Protocol.AddUser,
                                    MsgId = Guid.NewGuid().ToString(),
                                    Sender = Constants.RabbitMq.SenderDefault,
                                    UserIds = usersToAssign.Select(x => x.Id).ToList(),
                                    CardIds = usersToAssign
                                        .SelectMany(x => x.Card.Where(m => !m.IsDeleted).Select(y => y.Id)).ToList(),
                                    CompanyCode = company?.Code,
                                });
                            }

                            if (deviceIdsForVisitor.Any())
                            {
                                deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                                {
                                    DeviceIds = deviceIdsForVisitor.Distinct().ToList(),
                                    MessageType = Constants.Protocol.AddUser,
                                    MsgId = Guid.NewGuid().ToString(),
                                    Sender = Constants.RabbitMq.SenderDefault,
                                    VisitIds = visitorsToAssign.Select(x => x.Id).ToList(),
                                    CardIds = visitorsToAssign
                                        .SelectMany(x => x.Card.Where(m => !m.IsDeleted).Select(y => y.Id)).ToList(),
                                    CompanyCode = company?.Code,
                                });
                            }
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                        }
                    }
                });
                if (!string.IsNullOrEmpty(returnValue))
                {
                    return returnValue;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Add access group device
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="icuId"></param>
        /// <param name="tzId"></param>
        public void AddAccessGroupDevice(int accessGroupId, int icuId, int tzId)
        {
            try
            {
                var accessGroupDevice = new AccessGroupDevice
                {
                    AccessGroupId = accessGroupId,
                    IcuId = icuId,
                    TzId = tzId
                };
                _unitOfWork.AccessGroupDeviceRepository.Add(accessGroupDevice);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddAccessGroupDevice");
            }
        }

        /// <summary>
        /// Add user information into UserLog
        /// </summary>
        /// <param name="icuId"></param>
        /// <param name="tzId"></param>
        /// <param name="users"></param>
        /// <param name="actionType"></param>
        /// <param name="processId"></param>
        /// <param name="progressStart"></param>
        /// <param name="progressRange"></param>
        /// <param name="textFloorIds"></param>
        /// <returns></returns>
        public List<UserLog> AddUserLog(int icuId, int tzId, List<User> users, ActionType actionType,
            string processId = null, decimal progressStart = 0, decimal progressRange = 0, string textFloorIds = null)
        {
            try
            {
                var userLogs = new List<UserLog>();

                var device = _unitOfWork.IcuDeviceRepository.GetByIcuId(icuId);
                if (device == null)
                {
                    _logger.LogWarning($"Device not found: deviceId = {icuId}");
                    return null;
                }

            var companyId = device?.CompanyId;
            var deviceType = device?.DeviceType;
            var deviceTimeZone = device?.Building.TimeZone;
            var offSet = deviceTimeZone.ToTimeZoneInfo().BaseUtcOffset;

            var userIdList = _unitOfWork.CardRepository.GetUserIdHasCards(companyId ?? 0);
            var usersHasCard = users.Where(c => userIdList.Contains(c.Id)).ToList();

            var tz = _unitOfWork.AccessTimeRepository.GetByIdAndCompany(tzId, companyId ?? 0);
            var totalUserHasCard = usersHasCard.Count;

            //if (isBuildingMaster)
            //{
            //    // Get buildingMaster in this building that the device is assigned.
            //    var buildingMasters = _unitOfWork.AppDbContext.BuildingMaster.Where(m => m.BuildingId == device.BuildingId).Select(m => m.UserId);
            //    usersHasCard.ForEach(m => m.IsMasterCard = buildingMasters.Contains(m.Id));
            //}

            // For check card type by device type
            var validCardStatus = Helpers.GetCardStatusToSend();
            var cardTypes = Helpers.GetMatchedIdentificationType(deviceType.Value);

            foreach (var user in usersHasCard)
            {
                var cards = user.Card.Where(c =>
                    validCardStatus.Contains(c.CardStatus) && !c.IsDeleted && !string.IsNullOrEmpty(c.CardId)).ToList();
                var tzPosition = tz?.Position ?? 0;

                if (user.AccessGroup.Type == (int)AccessGroupType.PersonalAccess)
                {
                    var userPAGD = _unitOfWork.AccessGroupDeviceRepository
                        .GetByAccessGroupId(user.CompanyId, user.AccessGroupId).FirstOrDefault(m =>
                            m.AccessGroupId == user.AccessGroupId && m.IcuId == icuId);

                    //var userPAGD = user.AccessGroup.AccessGroupDevice.FirstOrDefault(m => m.IcuId == icuId);

                    if (userPAGD != null)
                    {
                        var pTz = _unitOfWork.AccessTimeRepository.GetByIdAndCompany(userPAGD.TzId, companyId ?? 0);
                        tzPosition = pTz.Position;
                    }
                }

                if (cardTypes.Any())
                {
                    cards = cards.Where(m => cardTypes.Contains(m.CardType)).ToList();
                }

                var department = user.Department;

                //var cards = _unitOfWork.CardRepository.GetByUserId(companyId ?? 0, user.Id);
                //var department = _unitOfWork.DepartmentRepository.GetByIdAndCompanyId(user.DepartmentId, companyId ?? 0);

                var departmentName = string.Empty;

                if (!string.IsNullOrEmpty(department.DepartName))
                {
                    departmentName = department.DepartName;
                }

                foreach (var card in cards)
                {
                    // Face Id 
                    if (deviceType != (short)DeviceType.IT100 && card.CardType == (int)CardType.FaceId)
                    {
                        continue;
                    }

                    //if (!card.IsDeleted && (actionType.Equals(ActionType.Delete) || (card.CardStatus == (int)CardStatus.Normal || card.CardStatus == (int)CardStatus.Transfer)))
                    if (!card.IsDeleted && (actionType.Equals(ActionType.Delete) ||
                                            (Helpers.GetCardStatusToSend().Contains(card.CardStatus))))
                    {
                        var issueCount = card != null ? card.IssueCount : 0;

                        var userLog = new UserLog
                        {
                            IcuId = icuId,
                            CardId = card.CardId,
                            CardType = card.CardType,
                            UserId = user.Id,
                            User = user,
                            EffectiveDate = user.EffectiveDate.Value.ConvertToUserTime(offSet),
                            ExpiredDate = user.ExpiredDate.Value.ConvertToUserTime(offSet),
                            //EffectiveDate = user.EffectiveDate.Value.ConvertToUserTime(deviceTimeZone),
                            //ExpiredDate = user.ExpiredDate.Value.ConvertToUserTime(deviceTimeZone),
                            KeyPadPw = user.KeyPadPw,
                            TzPosition = tzPosition,
                            Action = (short)actionType,
                            TransferStatus = (short)TransferType.SavedToServerSuccess,
                            DepartmentName = departmentName,
                            IssueCount = issueCount,
                            CardStatus = card.CardStatus,
                            Avatar = user.Avatar,
                            WorkType = user.WorkType.ToString(),
                            Grade = user.Grade,
                            //MilitaryNumber = user.UserArmy.First().MilitaryNumber,
                            AntiPassBack =
                                card.CardStatus // TODO This line shell be deleted after all devices in system are updated.
                        };
                        userLogs.Add(userLog);
                    }
                }
                // Update progress to frontend
                //var progress = (totalMsg - publishCount)*100 / (decimal)(totalMsg*progressRange) + (decimal)10 + progressRange*(remainCount);
                //if (processId != null)
                //{
                //    i++;
                //    var progress = progressStart + (i) * progressRange / (decimal)(totalUserHasCard);
                //    _queueService.Publish(Constants.RabbitMq.LongProcessProgressTopic, ProcessProgressProtocolData.MakeLongProcessProgressMessage(processId, progress, Constants.LongProgressName.Preparing));
                //}
            }

                return userLogs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddUserLog");
                return new List<UserLog>();
            }
        }

        /// <summary>
        /// Un-assign users from access group
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="userIds"></param>
        public void UnAssignUsers(int accessGroupId, List<int> userIds)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();
            var sender = _httpContext.User.GetUsername();
            var company = _unitOfWork.CompanyRepository.GetById(companyId);

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        IWebSocketService webSocketService = new WebSocketService();
                        AccessControlQueue accessControlQueue = new AccessControlQueue(_unitOfWork, webSocketService);
                        var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, webSocketService);
                        var usersToUnAssign = _unitOfWork.UserRepository.GetByIds(companyId, userIds);

                        // Un-assign users from current access group
                        var devicesInOldAccessGroup =
                            _unitOfWork.AccessGroupRepository.GetDevicesByAccessGroupId(accessGroupId);

                        // device (old access group)
                        deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                        {
                            DeviceIds = devicesInOldAccessGroup.Select(m => m.Id).ToList(),
                            MessageType = Constants.Protocol.DeleteUser,
                            MsgId = Guid.NewGuid().ToString(),
                            Sender = sender,
                            UserIds = userIds,
                            CompanyCode = company?.Code,
                        });

                        // change user to access group default
                        var defaultAccessGroup = _unitOfWork.AccessGroupRepository.GetDefaultAccessGroup(companyId);
                        foreach (var user in usersToUnAssign)
                        {
                            user.AccessGroupId = defaultAccessGroup.Id;
                            user.UpdatedOn = DateTime.UtcNow;
                            user.UpdatedBy = accountId;
                            _unitOfWork.UserRepository.Update(user);
                            _unitOfWork.Save();
                        }

                        // assign users to access group default
                        var devicesInNewAccessGroup =
                            _unitOfWork.AccessGroupRepository.GetDevicesByAccessGroupId(defaultAccessGroup.Id);

                        // device (new access group - default)
                        deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                        {
                            DeviceIds = devicesInNewAccessGroup.Select(m => m.Id).ToList(),
                            MessageType = Constants.Protocol.AddUser,
                            MsgId = Guid.NewGuid().ToString(),
                            Sender = sender,
                            UserIds = userIds,
                            CompanyCode = company?.Code,
                        });

                        //Save system log
                        var unAssignedUserIds = usersToUnAssign.Select(c => c.Id).ToList();
                        var unAssignedUserNames = usersToUnAssign.Select(c => c.FirstName).ToList();
                        var content = AccessGroupResource.msgUnAssignUsers;
                        var contentDetails = $"{UserResource.lblUserCount} : {unAssignedUserIds.Count}\n" +
                                             $"{AccessGroupResource.lblUnAssignUser}: {string.Join(", ", unAssignedUserNames)}";

                        _unitOfWork.SystemLogRepository.Add(accessGroupId, SystemLogType.AccessGroup,
                            ActionLogType.UnassignUser,
                            content, contentDetails, unAssignedUserIds, companyId);

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

        public void UnAssignAllDoors(int companyId, int accessGroupId)
        {
            var agds = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, accessGroupId).ToList();

            if (agds != null && agds.Any())
            {
                List<int> doorIds = agds.Select(m => m.IcuId).ToList();

                UnAssignDoors(accessGroupId, doorIds);
            }
        }

        public void UnAssignDoors(int accessGroupId, List<int> doorIds, bool isDeleteAgd = true)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        IWebSocketService webSocketService = new WebSocketService();
                        var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, webSocketService);
                        var companyId = _httpContext.User.GetCompanyId();
                        var company = _unitOfWork.CompanyRepository.GetById(companyId);
                        var users = _unitOfWork.AppDbContext.User.Include(u => u.Card).Include(u => u.Department)
                            .Where(u => u.CompanyId == _httpContext.User.GetCompanyId()
                                        && u.AccessGroupId == accessGroupId && !u.IsDeleted).ToList();
                        // === FOR PAG ===
                        var childPersonalAccessGroups = _unitOfWork.AppDbContext.AccessGroup
                            .Include(x => x.AccessGroupDevice)
                            .Include(x => x.User).ThenInclude(user => user.Department)
                            .Include(x => x.User).ThenInclude(user => user.Card)
                            .Where(x => x.CompanyId == companyId && x.ParentId == accessGroupId && !x.IsDeleted)
                            .ToList();

                        if (childPersonalAccessGroups.Any())
                        {
                            foreach (var childPersonalAccessGroup in childPersonalAccessGroups)
                            {
                                if (!childPersonalAccessGroup.AccessGroupDevice.Any(m => doorIds.Contains(m.IcuId)))
                                {
                                    users.AddRange(childPersonalAccessGroup.User.ToList());
                                }

                                users.AddRange(childPersonalAccessGroup.User.ToList());
                            }
                        }

                        if (users.Any())
                        {
                            users = users.Where(m => !m.IsDeleted && m.Status == (short)Status.Valid)
                                .Where(m => m.ApprovalStatus == (int)ApprovalStatus.Approved ||
                                            m.ApprovalStatus == (int)ApprovalStatus.NotUse).ToList();
                        }

                        var accessGroupDevices = _unitOfWork.AccessGroupDeviceRepository
                            .GetByAccessGroupId(companyId, accessGroupId).Where(m => doorIds.Contains(m.IcuId))
                            .ToList();
                        var accessGroup =
                            _unitOfWork.AppDbContext.AccessGroup.FirstOrDefault(a => a.Id == accessGroupId);

                        List<string> doorNames = new List<string>();
                        if (accessGroup != null)
                        {
                            List<int> deviceIdsForUser = new List<int>();
                            List<int> deviceIdsForVisitor = new List<int>();
                            Visit visit = null;
                            foreach (var accessGroupDevice in accessGroupDevices)
                            {
                                if (accessGroup.Name.Contains(Constants.Settings.NameAccessGroupVisitor))
                                {
                                    String[] str = accessGroup.Name.Split("-");
                                    var visitId = Int32.Parse(str[1]);
                                    visit = _unitOfWork.AppDbContext.Visit.Include(visit1 => visit1.Card)
                                        .Single(v => v.Id == visitId);
                                    if (visit != null)
                                    {
                                        visit.Card = _unitOfWork.CardRepository.GetByVisitId(companyId, visit.Id);
                                        deviceIdsForVisitor.Add(accessGroupDevice.IcuId);
                                    }
                                }
                                else
                                {
                                    //Add user log data
                                    if (users.Count != 0)
                                    {
                                        deviceIdsForUser.Add(accessGroupDevice.IcuId);
                                    }
                                }

                                doorNames.Add(_unitOfWork.IcuDeviceRepository.GetByIcuId(accessGroupDevice.IcuId).Name);

                                //Remove relationship from access group device
                                if (isDeleteAgd)
                                    _unitOfWork.AccessGroupDeviceRepository.Delete(accessGroupDevice);
                            }

                            if (deviceIdsForVisitor.Any() && visit != null)
                            {
                                deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                                {
                                    DeviceIds = deviceIdsForVisitor.Distinct().ToList(),
                                    MessageType = Constants.Protocol.DeleteUser,
                                    MsgId = Guid.NewGuid().ToString(),
                                    Sender = _httpContext != null
                                        ? _httpContext.User.GetUsername()
                                        : Constants.RabbitMq.SenderDefault,
                                    VisitIds = new List<int> { visit.Id },
                                    CardIds = visit.Card?.Where(m => !m.IsDeleted).Select(m => m.Id).ToList(),
                                    CompanyCode = company?.Code,
                                });
                            }

                            if (deviceIdsForUser.Any())
                            {
                                deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                                {
                                    DeviceIds = deviceIdsForUser.Distinct().ToList(),
                                    MessageType = Constants.Protocol.DeleteUser,
                                    MsgId = Guid.NewGuid().ToString(),
                                    Sender = Constants.RabbitMq.SenderDefault,
                                    UserIds = users.Select(x => x.Id).ToList(),
                                    CardIds = users.SelectMany(x => x.Card.Where(m => !m.IsDeleted).Select(y => y.Id))
                                        .ToList(),
                                    CompanyCode = company?.Code,
                                });
                            }

                            if (accessGroup.Type != (short)AccessGroupType.VisitAccess)
                            {
                                //Save system log
                                var content = AccessGroupResource.msgUnAssignDoors;
                                var contentDetails =
                                    $"{AccessGroupResource.lblAccessGroupName} : {accessGroup.Name}<br />" +
                                    $"{DeviceResource.lblDeviceCount} : {doorIds.Count}<br />" +
                                    $"{AccessGroupResource.lblUnAssignDoor}: {string.Join(", ", doorNames)}";

                                _unitOfWork.SystemLogRepository.Add(accessGroupId, SystemLogType.AccessGroup,
                                    ActionLogType.UnassignDoor,
                                    content, contentDetails, doorIds, companyId);
                            }
                        }

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                    }
                }
            });
        }

        /// <summary>
        /// Make user protocol data
        /// </summary>
        /// <param name="userLogs"> User data </param>
        /// <param name="protocolType"> protocol type </param>
        /// <param name="currentIndex"> current index </param>
        /// <returns></returns>
        public List<SDKCardModel> MakeUserProtocolDataWithIndex(List<UserLog> userLogs)
        {
            var users = _mapper.Map<List<SDKCardModel>>(userLogs);

            foreach (var detailData in users)
            {
                if (detailData.IdType == (int)CardType.FaceId)
                {
                    try
                    {
                        var face = _unitOfWork.AppDbContext.Face.FirstOrDefault(m =>
                            m.UserId == Int32.Parse(detailData.UserId));
                        detailData.FaceData = _mapper.Map<SDKFaceDataList>(face);
                    }
                    catch
                    {
                        _logger.LogWarning(
                            $"Failed to parse string data to integer. String value is {detailData.UserId}");
                    }
                }
            }

            return users;
        }


        /// <summary>
        /// Make user protocol data
        /// </summary>
        /// <param name="userLogs"> User data </param>
        /// <param name="protocolType"> protocol type </param>
        /// <param name="currentIndex"> current index </param>
        /// <returns></returns>
        public VehicleProtocolData MakeVehicleProtocolDataWithIndex(List<UserLog> userLogs, string protocolType,
            int totalIndex, int currentIndex)
        {
            var sender = Constants.RabbitMq.SenderDefault;
            try
            {
                if (_httpContext != null)
                    sender = _httpContext.User.GetUsername();
            }
            catch (Exception)
            {
            }

            var vehicleProtocolData = new VehicleProtocolData
            {
                MsgId = Guid.NewGuid().ToString(),
                Type = protocolType,
                Sender = sender
            };

            var updateFlag = currentIndex == (totalIndex - 1) ? (int)UpdateFlag.Stop : (int)UpdateFlag.Continue;

            var vehicleProtocolHeaderData = new VehicleProtocolHeaderData
            {
                Total = userLogs == null ? 0 : userLogs.Count,
                UpdateFlag = updateFlag,
                TotalIndex = totalIndex,
                FrameIndex = currentIndex
            };

            //var companyId = userLogs.First().User != null ? userLogs.First().User.CompanyId : (userLogs.First().Visit != null ? userLogs.First().Visit.CompanyId : 0);
            var companyId = userLogs.First().CompanyId;

            var cardIds = userLogs.Select(m => m.CardId.ToLower()).ToList();
            var vehicles = _unitOfWork.AppDbContext.Vehicle.Where(m =>
                !m.IsDeleted && m.CompanyId == companyId && cardIds.Contains(m.PlateNumber.ToLower())).ToList();

            //var test = _unitOfWork.VehicleRepository.GetByPlateNumber();

            List<VehicleProtocolDetailData> vehiclesData = new List<VehicleProtocolDetailData>();

            foreach (var vehicleLog in userLogs)
            {
                var vehicle = vehicles.FirstOrDefault(m => m.PlateNumber == vehicleLog.CardId);

                if (vehicle == null)
                {
                    //This case has problem with DB data.

                    continue;
                }

                VehicleProtocolDetailData data = new VehicleProtocolDetailData()
                {
                    PlateNumber = vehicle.PlateNumber,
                    UserId = vehicleLog.UserId,
                    EffectiveDate = vehicleLog.EffectiveDate == null
                        ? DateTime.UtcNow.ToString(Constants.DateTimeFormat.YyyyMMdd)
                        : vehicleLog.EffectiveDate.Value.ToString(Constants.DateTimeFormat.YyyyMMdd),
                    ExpiredDate = vehicleLog.ExpiredDate == null
                        ? "20991231"
                        : vehicleLog.ExpiredDate.Value.ToString(Constants.DateTimeFormat.YyyyMMdd),
                };

                vehiclesData.Add(data);
            }

            vehicleProtocolHeaderData.Users = vehiclesData;

            vehicleProtocolData.Data = vehicleProtocolHeaderData;

            return vehicleProtocolData;
        }


        /// <summary>
        /// If this access group(acceddGroupId) has become default AG, then other AG should be set not default AG.
        /// </summary>
        /// <param name="companyId">the id of the company to which this AG belongs</param>
        /// <param name="accessGroupId">access group id that become as default AG</param>
        internal void SetAllAccessGroupToNotDefault(int companyId, int accessGroupId)
        {
            var accessGroups = _unitOfWork.AccessGroupRepository.GetAccessGroupUnSetDefault(companyId, accessGroupId);
            foreach (var access in accessGroups)
            {
                access.IsDefault = false;
            }
        }

        /// <summary>
        /// Check name in company is already exist on db
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasExistName(int accessGroupId, string name)
        {
            try
            {
                int companyId = _httpContext.User.GetCompanyId();
                return _unitOfWork.AccessGroupRepository.HasExistName(accessGroupId, companyId, name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in HasExistName");
                return false;
            }
        }

        /// <summary>
        /// Check access group can be delete
        /// </summary>
        /// <param name="accessGroupId"></param>
        /// <returns></returns>
        public string GetAGNameCanNotDelete(List<int> accessGroupId)
        {
            try
            {
                var result = "";

                List<string> names = new List<string>();

            //var accessGroups = GetByIds(accessGroupId);
            var accessGroups = _unitOfWork.AppDbContext.AccessGroup.Include(m => m.User)
                .Where(m => accessGroupId.Contains(m.Id)).ToList();

            var agUsers = accessGroups.SelectMany(a => a.User).ToList();
            if (agUsers.Any(u => !u.IsDeleted && u.Status == (short)UserStatus.NotUse))
            {
                var entiredUsers = agUsers.Where(u => !u.IsDeleted && u.Status == (short)UserStatus.NotUse).ToList();
                // Move this users to NoAccessGroup.
                var noAG = _unitOfWork.AccessGroupRepository.GetNoAccessGroup(_httpContext.User.GetCompanyId());
                foreach (var eUser in entiredUsers)
                {
                    // System can assign these users to NoAccessGroup by using 'AssignUser' function.
                    // But I think these user's data had been already sent to device when these users was changed to 'Invalid' status.
                    // So, in this process, the system does not send these users data to device. Only change accessGroupId value.
                    eUser.AccessGroupId = noAG.Id;
                    _unitOfWork.UserRepository.Update(eUser);
                }

                _unitOfWork.Save();

                accessGroups = _unitOfWork.AppDbContext.AccessGroup.Include(m => m.User)
                    .Where(m => accessGroupId.Contains(m.Id)).ToList();
            }

            foreach (var group in accessGroups)
            {
                var user = group.User;
                if (group.IsDefault
                    || group.Type == (short)AccessGroupType.FullAccess
                    || group.Type == (short)AccessGroupType.NoAccess
                    || group.Type == (short)AccessGroupType.VisitAccess
                    || (group.Type == (short)AccessGroupType.PersonalAccess && user.Any(m => !m.IsDeleted))
                    || user.Any(m => !m.IsDeleted))
                {
                    names.Add(group.Name);
                }
            }

            var childAGs = _unitOfWork.AppDbContext.AccessGroup.Include(m => m.User)
                .Where(m => accessGroupId.Contains(m.ParentId.Value)).ToList();

            var cagUsers = childAGs.SelectMany(a => a.User).ToList();
            if (cagUsers.Any(u => !u.IsDeleted && u.Status == (short)UserStatus.NotUse))
            {
                var entiredUsers = cagUsers.Where(u => !u.IsDeleted && u.Status == (short)UserStatus.NotUse).ToList();
                // Move this users to NoAccessGroup.
                var noAG = _unitOfWork.AccessGroupRepository.GetNoAccessGroup(_httpContext.User.GetCompanyId());
                foreach (var eUser in entiredUsers)
                {
                    // System can assign these users to NoAccessGroup by using 'AssignUser' function.
                    // But I think these user's data had been already sent to device when these users was changed to 'Invalid' status.
                    // So, in this process, the system does not send these users data to device. Only change accessGroupId value.
                    eUser.AccessGroupId = noAG.Id;
                    _unitOfWork.UserRepository.Update(eUser);
                }

                _unitOfWork.Save();

                childAGs = _unitOfWork.AppDbContext.AccessGroup.Include(m => m.User)
                    .Where(m => accessGroupId.Contains(m.ParentId.Value)).ToList();
            }

            foreach (var child in childAGs)
            {
                var user = child.User;
                if (child.IsDefault
                    || child.Type == (short)AccessGroupType.FullAccess
                    || child.Type == (short)AccessGroupType.NoAccess
                    || child.Type == (short)AccessGroupType.VisitAccess
                    || (child.Type == (short)AccessGroupType.PersonalAccess && user.Any(m => !m.IsDeleted)))
                {
                    if (!names.Contains(child.Parent.Name))
                    {
                        names.Add(child.Parent.Name);
                    }
                }
            }

                if (names.Any()) result = string.Join(", ", names);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAGNameCanNotDelete");
                return string.Empty;
            }
        }

        /// <summary>
        /// Send a new card to device
        /// </summary>
        public void SendIdentificationToDevice(AccessGroupDevice agDevice, User user, Card card, bool isAdd)
        {
            try
            {
                if (string.IsNullOrEmpty(card.CardId)) return;
                if (isAdd && card.IsDeleted) return;
                //if (!(card.CardStatus == (short)CardStatus.Normal || card.CardStatus == (short)CardStatus.Transfer))
                if (!Helpers.GetCardStatusToSend().Contains(card.CardStatus))
                {
                    _logger.LogWarning(
                        $"Send Identification Warning: CardId = {card.CardId} -- Card Status = {card.CardStatus}");
                    return;
                }

            var deviceTimeZone = agDevice.Icu.Building.TimeZone;
            var offSet = deviceTimeZone.ToTimeZoneInfo().BaseUtcOffset;

            var userProtocolData = new List<SDKCardModel>
            {
                new()
                {
                    EmployeeNumber = user.EmpNumber,
                    UserName = user.FirstName,
                    DepartmentName = user.Department?.DepartName,
                    Position = user.Position,
                    CardId = card.CardId,
                    IssueCount = card.IssueCount,
                    AdminFlag = 0,
                    EffectiveDate = user.EffectiveDate.Value.ConvertToUserTime(offSet)
                        .ToString(Constants.DateTimeFormat.DdMMyyyyHHmm),
                    ExpireDate = user.ExpiredDate.Value.ConvertToUserTime(offSet)
                        .ToString(Constants.DateTimeFormat.DdMMyyyyHHmm),
                    AntiPassBack = card.CardStatus,
                    CardStatus = card.CardStatus,
                    Timezone = agDevice.Tz?.Position ?? 1,
                    Password = user.KeyPadPw,
                    UserId = user.Id.ToString(),
                    IdType = card.CardType,

                    // For Face ID
                    FaceData = card.CardType == (int)CardType.FaceId
                        ? _mapper.Map<SDKFaceDataList>(user.Face.FirstOrDefault())
                        : null,
                    Avatar = string.IsNullOrEmpty(user.Avatar) ? "" : user.Avatar
                }
            };
                if (isAdd)
                {
                    _deviceSdkService.AddCard(agDevice.Icu?.DeviceAddress ?? "", userProtocolData);
                }
                else
                {
                    _deviceSdkService.DeleteCard(agDevice.Icu?.DeviceAddress ?? "", userProtocolData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendIdentificationToDevice");
            }
        }

        public void SendIdentificationToDevice(List<AccessGroupDevice> agDevices, User user, Card card, bool isAdd)
        {
            try
            {
                if (string.IsNullOrEmpty(card.CardId)) return;
                if (isAdd && card.IsDeleted) return;

                if (!Helpers.GetCardStatusToSend().Contains(card.CardStatus))
                {
                    _logger.LogWarning(
                        $"Send Identification Warning: CardId = {card.CardId} -- Card Status = {card.CardStatus}");
                    return;
                }

            var protocolData = new UserProtocolData()
            {
                Type = isAdd ? Constants.Protocol.AddUser : Constants.Protocol.DeleteUser,
                Sender = _httpContext != null ? _httpContext.User.GetUsername() : Constants.RabbitMq.SenderDefault
            };

            var groupAGDs = agDevices.GroupBy(m => m.Icu.Building ?? new Building()).ToList();

            List<string> fingerTemplates = null;
            if (card.CardType == (short)CardType.FingerPrint || card.CardType == (short)CardType.AratekFingerPrint)
            {
                fingerTemplates = _unitOfWork.CardRepository.GetFingerPrintByCard(card.Id).Select(m => m.Templates)
                    .ToList();
            }

            var userData = new SDKCardModel()
            {
                EmployeeNumber = user.EmpNumber,
                UserName = user.FirstName,
                DepartmentName = user.Department?.DepartName,
                CardId = card.CardId,
                IssueCount = card.IssueCount,
                AntiPassBack = card.CardStatus,
                CardStatus = card.CardStatus,
                Password = user.KeyPadPw,
                UserId = user.Id.ToString(),
                IdType = card.CardType,
                AccessGroupId = user.AccessGroupId,
                FingerTemplates = fingerTemplates,
                // For Face ID
                //FaceData = card.CardType == (int)CardType.FaceId ? Mapper.Map<FaceDataList>(face) : null
                FaceData = card.CardType == (int)CardType.FaceId
                    ? _mapper.Map<SDKFaceDataList>(user.Face.FirstOrDefault())
                    : null,
                Avatar = user.Avatar
            };

            foreach (var groupAGD in groupAGDs)
            {
                var rimezone = groupAGD.Key.TimeZone;
                var offset = rimezone.ToTimeZoneInfo().BaseUtcOffset;

                userData.AdminFlag = 0;
                userData.EffectiveDate = user.EffectiveDate.Value.ConvertToUserTime(offset)
                    .ToString(Constants.DateTimeFormat.DdMMyyyyHHmm);
                userData.ExpireDate = user.ExpiredDate.Value.ConvertToUserTime(offset)
                    .ToString(Constants.DateTimeFormat.DdMMyyyyHHmm);

                foreach (var aGD in groupAGD)
                {
                    var cardTypes = Helpers.GetMatchedIdentificationType(aGD.Icu.DeviceType);
                    if (!cardTypes.Contains(card.CardType)) continue;

                    protocolData.MsgId = Guid.NewGuid().ToString();
                    userData.Timezone = aGD.Tz?.Position ?? 1;

                    var userProtocolData = new List<SDKCardModel>() { userData };

                    if (isAdd)
                    {
                        _deviceSdkService.AddCard(aGD.Icu?.DeviceAddress ?? "", userProtocolData);
                    }
                    else
                    {
                        _deviceSdkService.DeleteCard(aGD.Icu?.DeviceAddress ?? "", userProtocolData);
                    }
                }
            }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendIdentificationToDevice");
            }
        }

        public void SendAddOrDeleteUser(string deviceAddress, IEnumerable<List<UserLog>> userLogs, bool isAddUser = true)
        {
            try
            {
                foreach (var userLog in userLogs)
                {
                    var userProtocolData = MakeUserProtocolDataWithIndex(userLog);
                    if (isAddUser)
                    {
                        _deviceSdkService.AddCard(deviceAddress, userProtocolData);
                    }
                    else
                    {
                        _deviceSdkService.DeleteCard(deviceAddress, userProtocolData);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendAddOrDeleteUser");
            }
        }

        public void SendVisitor(AccessGroupDevice accessGroupDevice, bool isAddUser = true, Visit visit = null)
        {
            try
            {
                if (visit != null)
                {
                List<short> visitStatus = new List<short>()
                {
                    (short)VisitChangeStatusType.Approved,
                    (short)VisitChangeStatusType.AutoApproved,
                    (short)VisitChangeStatusType.CardIssued
                };

                if (!isAddUser)
                {
                    // SEND DELETE
                    visitStatus.Add((short)VisitChangeStatusType.CardReturned);
                    visitStatus.Add((short)VisitChangeStatusType.Finished);
                    visitStatus.Add((short)VisitChangeStatusType.FinishedWithoutReturnCard);
                }

                if (visitStatus.Contains(visit.Status))
                {
                    var cards = _unitOfWork.AppDbContext.Card.Where(c => !c.IsDeleted && c.VisitId == visit.Id)
                        .ToList();
                    List<SDKCardModel> userProtocolData = new List<SDKCardModel>();

                    var deviceTimezone = accessGroupDevice.Icu.Building.TimeZone;
                    var offSet = deviceTimezone.ToTimeZoneInfo().BaseUtcOffset;

                    foreach (var card in cards)
                    {
                        SDKCardModel userDetail = new SDKCardModel()
                        {
                            EmployeeNumber = visit.VisitorName,
                            UserName = visit.VisitorName,
                            DepartmentName = visit.VisitorDepartment,
                            CardId = card.CardId,
                            IssueCount = card.IssueCount,
                            AdminFlag = (short)(card.IsMasterCard ? 1 : 0),
                            EffectiveDate = visit.StartDate.ConvertToUserTime(offSet)
                                .ToString(Constants.DateTimeFormat.DdMMyyyyHHmm),
                            ExpireDate = visit.EndDate.ConvertToUserTime(offSet)
                                .ToString(Constants.DateTimeFormat.DdMMyyyyHHmm),
                            AntiPassBack = card.CardStatus,
                            UserId = visit.AliasId ?? visit.Id.ToString(),
                            Timezone = accessGroupDevice.Tz?.Position ?? 1,
                            Password = ""
                        };

                        userProtocolData.Add(userDetail);
                    }

                    if (isAddUser)
                    {
                        _deviceSdkService.AddCard(accessGroupDevice.Icu?.DeviceAddress ?? "", userProtocolData);
                    }
                    else
                    {
                        _deviceSdkService.DeleteCard(accessGroupDevice.Icu?.DeviceAddress ?? "", userProtocolData);
                    }
                }
            }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendVisitor");
            }
        }

        public List<CardModel> GetCardListByUserId(int userId)
        {
            try
            {
                var cards = _unitOfWork.CardRepository.GetByUserId(_httpContext.User.GetCompanyId(), userId)
                    .Where(m => m.CardStatus < (short)CardStatus.WaitingForPrinting);
                var cardModelList = new List<CardModel>();
                foreach (var card in cards)
                {
                    var cardModel = _mapper.Map<CardModel>(card);
                    if (cardModel.CardType == (short)CardType.PassCode)
                    {
                        cardModel.CardId = Constants.DynamicQr.PassCodeString;
                    }
                    else if (cardModel.CardType == (short)CardType.VehicleId || cardModel.CardType == (short)CardType.VehicleMotoBikeId)
                    {
                        continue;
                    }

                    cardModelList.Add(cardModel);
                }

                return cardModelList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCardListByUserId");
                return new List<CardModel>();
            }
        }

        public void SendIdentificationToDeviceVisitor(AccessGroupDevice agDevice, Visit visitor, Card card, bool isAdd)
        {
            try
            {
                var cardTypes = Helpers.GetMatchedIdentificationType(agDevice.Icu.DeviceType);
                if (!cardTypes.Contains(card.CardType)) return;

            List<short> visitStatus = new List<short>()
            {
                (short)VisitChangeStatusType.Approved,
                (short)VisitChangeStatusType.AutoApproved,
                (short)VisitChangeStatusType.CardIssued
            };

            if (!isAdd)
            {
                // SEND DELETE
                visitStatus.Add((short)VisitChangeStatusType.CardReturned);
                visitStatus.Add((short)VisitChangeStatusType.Finished);
                visitStatus.Add((short)VisitChangeStatusType.FinishedWithoutReturnCard);
            }

            if (!visitStatus.Contains(visitor.Status)) return;

            var deviceTimeZone = agDevice.Icu.Building.TimeZone;
            var offSet = deviceTimeZone.ToTimeZoneInfo().BaseUtcOffset;

            var userProtocolData = new List<SDKCardModel>
            {
                new SDKCardModel()
                {
                    EmployeeNumber = visitor.VisitorEmpNumber,
                    UserName = visitor.VisitorName,
                    DepartmentName = visitor.VisitorDepartment,
                    CardId = card.CardId,
                    IssueCount = card.IssueCount,
                    AdminFlag = (short)(card.IsMasterCard ? 1 : 0),
                    EffectiveDate = visitor.StartDate.ConvertToUserTime(offSet)
                        .ToString(Constants.DateTimeFormat.DdMMyyyyHHmm),
                    ExpireDate = visitor.EndDate.ConvertToUserTime(offSet)
                        .ToString(Constants.DateTimeFormat.DdMMyyyyHHmm),
                    CardStatus = card.CardStatus,
                    Timezone = agDevice.Tz?.Position ?? 1,
                    Password = "",
                    UserId = visitor.AliasId ?? visitor.Id.ToString(),
                }
            };

                if (isAdd)
                {
                    _deviceSdkService.AddCard(agDevice.Icu?.DeviceAddress ?? "", userProtocolData);
                }
                else
                {
                    _deviceSdkService.DeleteCard(agDevice.Icu?.DeviceAddress ?? "", userProtocolData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendIdentificationToDeviceVisitor");
            }
        }

        public IEnumerable<List<UserLog>> MakeUserLogData(IcuDevice device, List<int> userIds = null,
            List<int> visitIds = null, bool includeInvalid = false)
        {
            var maxSplit = Helpers.GetMaxSplit(device.DeviceType);

            if (device.Building == null)
            {
                device.Building = _unitOfWork.BuildingRepository.GetById(device.BuildingId.Value);
            }

            // Get access setting for checking approval step number.
            //var accessSetting = _unitOfWork.UserRepository.GetAccessSetting(device.CompanyId.Value);

            var deviceTimeZone = device?.Building.TimeZone;
            var offSet = deviceTimeZone.ToTimeZoneInfo().BaseUtcOffset;

            //List<int> faceDevices = new List<int>()
            //{
            //    (int)DeviceType.IT100
            //};

            // Check card type by device type.
            var cardTypes = Helpers.GetMatchedIdentificationType(device.DeviceType);

            //bool isFaceData = faceDevices.Contains(device.DeviceType);

            var agDevices2 = _unitOfWork.AccessGroupDeviceRepository.GetByIcuId(device.CompanyId.Value, device.Id);
            List<UserLog> userLogs = new List<UserLog>();


            foreach (var agDevice in agDevices2)
            {
                if (agDevice.Tz == null)
                {
                    var timezone = _unitOfWork.AccessTimeRepository.GetById(agDevice.TzId);
                    agDevice.Tz = timezone;
                }

                int tzPosition = agDevice.Tz.Position;

                // ========= Solution 1 =========
                //IEnumerable<User> users = agDevice.AccessGroup.User.Where(m => !m.IsDeleted && m.Status == (short)Status.Valid);

                //if (userIds != null)
                //    users = users.Where(m => userIds.Contains(m.Id));
                // ========= Solution 1 =========

                // ========= Solution 2 =========
                var accessGroup = _unitOfWork.AppDbContext.AccessGroup.Include(m => m.Child)
                    .FirstOrDefault(m => m.Id == agDevice.AccessGroupId);

                var childAG = accessGroup.Child.Select(m => m.Id);

                var userData = _unitOfWork.AppDbContext.User
                    .Include(m => m.Card).ThenInclude(m => m.FingerPrint)
                    .Include(m => m.Department)
                    .Where(m => !m.IsDeleted && m.CompanyId == device.CompanyId &&
                                (m.AccessGroupId == agDevice.AccessGroupId || childAG.Contains(m.AccessGroupId)));
                if (!includeInvalid)
                {
                    userData = userData.Where(m => m.Status == (short)Status.Valid);
                }

                if (userIds != null)
                    userData = userData.Where(m => userIds.Contains(m.Id));

                //// filtering Approved users.
                //if(accessSetting.ApprovalStepNumber > (int) VisitSettingType.NoStep)
                userData = userData.Where(m =>
                    m.ApprovalStatus == (int)ApprovalStatus.Approved || m.ApprovalStatus == (int)ApprovalStatus.NotUse);

                IEnumerable<User> users = userData.AsEnumerable<User>();
                // ========= Solution 2 =========


                var u = users.GetEnumerator();

                while (u.MoveNext())
                {
                    //var cards = isFaceData
                    //    ? u.Current.Card.Where(x => (x.CardStatus == (int)CardStatus.Normal || x.CardStatus == (int)CardStatus.Transfer) && !x.IsDeleted)
                    //    : u.Current.Card.Where(x => (x.CardStatus == (int)CardStatus.Normal || x.CardStatus == (int)CardStatus.Transfer) && !x.IsDeleted && x.CardType != (int)CardType.FaceId);

                    //var cards = u.Current.Card.Where(x => (x.CardStatus == (int)CardStatus.Normal || x.CardStatus == (int)CardStatus.Transfer) && !x.IsDeleted && cardTypes.Contains(x.CardType));
                    var cards = u.Current.Card.Where(x =>
                        Helpers.GetCardStatusToSend().Contains(x.CardStatus) && !x.IsDeleted &&
                        cardTypes.Contains(x.CardType) && !string.IsNullOrEmpty(x.CardId));

                    string militaryNumber = "";
                    string workTypeName = "";
                    string gradeName = "";

                    string departmentName = "";
                    if (u.Current.Department == null)
                    {
                        departmentName = _unitOfWork.DepartmentRepository.GetById(u.Current.DepartmentId).DepartName;
                    }
                    else
                    {
                        departmentName = u.Current.Department.DepartName;
                    }

                    //var log = cards.Select(x => Mapper.Map<UserLog>(x)).ToList();
                    var log = cards.Select(x => _mapper.Map<UserLog>(x)).ToList();
                    log.ForEach(m =>
                    {
                        m.TzPosition = tzPosition;
                        m.DepartmentName = departmentName;
                        m.Grade = gradeName;
                        m.MilitaryNumber = militaryNumber;
                        m.WorkType = workTypeName;
                    });

                    userLogs = userLogs.Concat(log).ToList();
                }

                var visits = _unitOfWork.AppDbContext.Visit.Include(m => m.Card).Where(m =>
                    !m.IsDeleted && m.EndDate > DateTime.Now && m.CompanyId == device.CompanyId &&
                    (m.AccessGroupId == agDevice.AccessGroupId || childAG.Contains(m.AccessGroupId)));
                if (visitIds != null)
                    visits = visits.Where(m => visitIds.Contains(m.Id));

                var v = visits.GetEnumerator();

                while (v.MoveNext())
                {
                    //var cards = isFaceData
                    //    ? v.Current.Card.Where(x => (x.CardStatus == (int)CardStatus.Normal || x.CardStatus == (int)CardStatus.Transfer) && !x.IsDeleted)
                    //    : v.Current.Card.Where(x => (x.CardStatus == (int)CardStatus.Normal || x.CardStatus == (int)CardStatus.Transfer) && !x.IsDeleted && x.CardType != (int)CardType.FaceId);

                    //var cards = v.Current.Card.Where(x => (x.CardStatus == (int)CardStatus.Normal || x.CardStatus == (int)CardStatus.Transfer) && !x.IsDeleted && cardTypes.Contains(x.CardType));
                    var cards = v.Current.Card.Where(x =>
                        Helpers.GetCardStatusToSend().Contains(x.CardStatus) && !x.IsDeleted &&
                        cardTypes.Contains(x.CardType) && !string.IsNullOrEmpty(x.CardId));
                    var cardUserLogs = cards.Select(x => _mapper.Map<UserLog>(x)).ToList();
                    cardUserLogs.ForEach(m => { m.TzPosition = tzPosition; });

                    userLogs = userLogs.Concat(cardUserLogs).ToList();
                }

                //userLogs.ForEach(m => m.TzPosition = agDevice.Tz.Position);
            }

            // Add unit vehicle for LPR
            if (userIds == null && device.DeviceType == (short)DeviceType.NexpaLPR)
            {
                var unitVehicle = _unitOfWork.AppDbContext.Vehicle
                    .Where(m => !m.IsDeleted && m.CompanyId == device.CompanyId.Value).ToList();

                if (unitVehicle.Any())
                {
                    var unitVehicleCard = _unitOfWork.AppDbContext.Card.Where(m =>
                        !m.IsDeleted && unitVehicle.Select(n => n.PlateNumber).Contains(m.CardId)).ToList();

                    if (unitVehicleCard.Any())
                    {
                        userLogs.AddRange(unitVehicleCard.Select(x => _mapper.Map<UserLog>(x)));
                    }
                }
            }

            if (device.OperationType != (short)OperationType.Restaurant)
                userLogs.ForEach(m => m.MasterFlag = false);

            // Check userLogs whether 
            if (userLogs.Any())
            {
                userLogs.ForEach(m =>
                {
                    if (m.EffectiveDate.HasValue)
                    {
                        m.EffectiveDate = m.EffectiveDate.Value.ConvertToUserTime(offSet);
                    }
                    else
                    {
                        _logger.LogWarning(
                            $"[MakeUserLogData - EffectiveDate] | UserId : {m.UserId} | This user doesn't have effective date.");
                        //_logger.LogWarning(JsonConvert.SerializeObject(m));
                    }
                });
                userLogs.ForEach(m =>
                {
                    if (m.ExpiredDate.HasValue)
                    {
                        m.ExpiredDate = m.ExpiredDate.Value.ConvertToUserTime(offSet);
                    }
                    else
                    {
                        _logger.LogWarning(
                            $"[MakeUserLogData - ExpiredDate] | UserId : {m.UserId} | This user doesn't have expired date.");
                        //_logger.LogWarning(JsonConvert.SerializeObject(m));
                    }
                });
            }

            // Remove duplicates card.
            userLogs = userLogs.AsQueryable().DistinctBy(m => m.CardId).ToList();

            // Console.WriteLine("=============================================================");
            // Console.WriteLine($"Device: {device.Name}, Total: {userLogs.Count}");
            // Console.WriteLine("==> Access Group Device <==");
            // foreach (var accessGroupDevice in agDevices2.Where(m => m.AccessGroup.Type != (short)AccessGroupType.VisitAccess))
            // {
            //     Console.WriteLine($"AccessGroupId: {accessGroupDevice.AccessGroupId}, IcuId: {accessGroupDevice.IcuId}, TzId: {accessGroupDevice.TzId}");
            // }
            // Console.WriteLine("===========================");
            // foreach (var item in userLogs)
            // {
            //     Console.WriteLine("{" + $"\"CardId\": '{item.CardId}', \"UserId\": '{item.UserId}', \"Username\": '{item.User?.FirstName}', \"CardType\": '{item.CardType}'" + "},");
            // }
            // Console.WriteLine("=============================================================");

            return userLogs.SplitList(maxSplit);
        }

        public IEnumerable<List<UserLog>> MakeUserLogData(IcuDevice device, User user, bool includeInvalid = false)
        {
            var maxSplit = Helpers.GetMaxSplit(device.DeviceType);

            if (device.Building == null)
            {
                device.Building = _unitOfWork.BuildingRepository.GetById(device.BuildingId.Value);
            }

            // Get access setting for checking approval step number.
            //var accessSetting = _unitOfWork.UserRepository.GetAccessSetting(device.CompanyId.Value);

            var deviceTimeZone = device?.Building.TimeZone;
            var offSet = deviceTimeZone.ToTimeZoneInfo().BaseUtcOffset;

            // Check card type by device type.
            var cardTypes = Helpers.GetMatchedIdentificationType(device.DeviceType);

            var agDevice = _unitOfWork.AccessGroupDeviceRepository
                .GetByAccessGroupId(user.CompanyId, user.AccessGroupId).FirstOrDefault(m => m.IcuId == device.Id);

            List<UserLog> userLogs = new List<UserLog>();

            if (agDevice == null)
            {
                return userLogs.SplitList(maxSplit);
            }
            //var agDevice = _unitOfWork.AccessGroupDeviceRepository.GetByIcuId(device.CompanyId.Value, device.Id).FirstOrDefault(m => m.AccessGroupId == user.AccessGroupId);

            string militaryNumber = "";
            string workTypeName = "";
            string gradeName = "";

            var userCount = 0;
            //var usersCount = 0;
            //var visitorCount = 0;

            if (agDevice.Tz == null)
            {
                var timezone = _unitOfWork.AccessTimeRepository.GetById(agDevice.TzId);
                agDevice.Tz = timezone;
            }

            int tzPosition = agDevice.Tz.Position;

            //var cards = user.Card.Where(x => (x.CardStatus == (int)CardStatus.Normal || x.CardStatus == (int)CardStatus.Transfer) && !x.IsDeleted && cardTypes.Contains(x.CardType));
            var cards = user.Card.Where(x =>
                Helpers.GetCardStatusToSend().Contains(x.CardStatus) && !x.IsDeleted &&
                cardTypes.Contains(x.CardType) && !string.IsNullOrEmpty(x.CardId));

            var department = _unitOfWork.DepartmentRepository.GetById(user.DepartmentId);

            var log = cards.Select(x => _mapper.Map<UserLog>(x)).ToList();
            log.ForEach(m =>
            {
                m.TzPosition = tzPosition;
                m.DepartmentName = department != null ? department.DepartName : "";
                m.MilitaryNumber = militaryNumber;
                m.Grade = gradeName;
                m.WorkType = workTypeName;
            });

            userLogs = userLogs.Concat(log).ToList();

            if (device.OperationType != (short)OperationType.Restaurant)
                userLogs.ForEach(m => m.MasterFlag = false);

            userLogs = userLogs.ToList();

            // Check userLogs whether 
            if (userLogs.Any())
            {
                userLogs.ForEach(m =>
                {
                    if (m.EffectiveDate.HasValue)
                    {
                        m.EffectiveDate = m.EffectiveDate.Value.ConvertToUserTime(offSet);
                    }
                    else
                    {
                        _logger.LogWarning("[MakeUserLogData - EffectiveDate]");
                        _logger.LogWarning(m.UserId.ToString());
                        _logger.LogWarning(JsonConvert.SerializeObject(m));
                    }
                });
                userLogs.ForEach(m =>
                {
                    if (m.ExpiredDate.HasValue)
                    {
                        m.ExpiredDate = m.ExpiredDate.Value.ConvertToUserTime(offSet);
                    }
                    else
                    {
                        _logger.LogWarning("[MakeUserLogData - ExpiredDate]");
                        _logger.LogWarning(m.UserId.ToString());
                        _logger.LogWarning(JsonConvert.SerializeObject(m));
                    }
                });
            }

            return userLogs.SplitList(maxSplit);
        }

        public void RemoveDeviceFromAllAccessGroup(List<int> doorIds)
        {
            try
            {
                var accessGroups = _unitOfWork.AccessGroupRepository.GetListAccessGroups(_httpContext.User.GetCompanyId());
                var accessGroupIds = accessGroups.Where(m => m.Type == (short)AccessGroupType.NormalAccess)
                    .Select(m => m.Id);
                foreach (var accessGroupId in accessGroupIds)
                {
                    UnAssignDoors(accessGroupId, doorIds);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RemoveDeviceFromAllAccessGroup");
            }
        }

        public void AddAccessGroupWithAccessTime(List<string> accessTimeNames)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var accessTimeListModels = _unitOfWork.AppDbContext.AccessTime
                    .Where(m => !m.IsDeleted && m.CompanyId == companyId).ToList();

                var doorList = _unitOfWork.IcuDeviceRepository.GetByCompany(companyId);
                foreach (var accessTimeName in accessTimeNames)
                {
                    var accessTime =
                        accessTimeListModels.FirstOrDefault(x => x.Name.ToLower().Equals(accessTimeName.ToLower()));
                    var ag = _unitOfWork.AccessGroupRepository.GetByNameAndCompanyId(companyId,
                        string.Concat(Constants.DefaultAccessGroupName, accessTimeName));
                    if (accessTime != null)
                    {
                        if (ag == null)
                        {
                            var accessGroup = new AccessGroup()
                            {
                                Name = string.Concat(Constants.DefaultAccessGroupName, accessTimeName),
                                IsDefault = false
                            };
                            accessGroup.CompanyId = companyId;
                            accessGroup.Type = (short)AccessGroupType.NormalAccess;
                            _unitOfWork.AccessGroupRepository.Add(accessGroup);
                            _unitOfWork.Save();

                            var content = $"{AccessGroupResource.msgAddAccessGroup}";
                            List<string> details = new List<string>();
                            details.Add($"{AccessGroupResource.lblAccessGroupName} : {accessGroup.Name}");
                            var contentsDetail = string.Join("\n", details);

                            // save system log
                            _unitOfWork.SystemLogRepository.Add(accessGroup.Id, SystemLogType.AccessGroup,
                                ActionLogType.Add,
                                content, contentsDetail, null, _httpContext.User.GetCompanyId());

                            _unitOfWork.Save();

                            var assignDoor = new AccessGroupAssignDoor();
                            foreach (var door in doorList)
                            {
                                assignDoor.Doors.Add(new AccessGroupAssignDoorDetail()
                                {
                                    CompanyId = companyId,
                                    DoorId = door.Id,
                                    TzId = accessTime.Id
                                });
                            }

                            AssignDoors(accessGroup.Id, assignDoor, false);
                        }
                        // else
                        // {
                        //     var assignDoor = new AccessGroupAssignDoor();
                        //     foreach (var door in doorList)
                        //     {
                        //         assignDoor.Doors.Add(new AccessGroupAssignDoorDetail()
                        //         {
                        //             CompanyId = companyId,
                        //             DoorId = door.Id,
                        //             TzId = accessTime.Id
                        //         });
                        //     }
                        //     AssignDoors(ag.Id, assignDoor, false);
                        // }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message + ex.StackTrace);
            }
        }

        public bool CheckUserInDepartment(int companyId, int accountId, List<int> users)
        {
            try
            {
                if (CheckPluginDepartmentAccessLevel() &&
                    _httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
                {
                    var listDepartments =
                        _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
                    var listUserOfDepartment = _unitOfWork.UserRepository.GetByDepartmentIds(listDepartments)
                        .Select(x => x.Id).Distinct().ToList();
                    foreach (int id in users)
                    {
                        if (!listUserOfDepartment.Contains(id))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckUserInDepartment");
                return false;
            }
        }

        public bool IsFullAccessGroup(int companyId, int accessGroupId)
        {
            try
            {
                var ag = _unitOfWork.AccessGroupRepository.GetFullAccessGroup(companyId);
                return ag.Id == accessGroupId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsFullAccessGroup");
                return false;
            }
        }

        /// <summary>
        /// Check enable plugin Department Access Level
        /// </summary>
        /// <returns></returns>
        private bool CheckPluginDepartmentAccessLevel()
        {
            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(_httpContext.User.GetCompanyId());
            PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
            return plugIns.DepartmentAccessLevel;
        }
    }
}