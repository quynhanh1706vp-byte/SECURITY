using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.Repository;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DeMasterProCloud.DataModel.Building;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.Service.RabbitMqQueue;
using System.Globalization;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.Service.Infrastructure;
using Bogus.Extensions;

namespace DeMasterProCloud.Service
{
    /// <summary>
    /// Building service interface
    /// </summary>
    public interface IBuildingService : IPaginationService<BuildingListModel>
    {
        List<Building> GetByCompanyId(int companyId);
        int Add(BuildingModel model);
        void AddDoors(Building building, List<int> doorIds);
        IQueryable<BuildingDoorModel> GetPaginatedDoors(int id, string filter, short operationType, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);
        List<BuildingDoorModel> GetPaginatedDoors(List<int> id, string filter, short operationType, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);
        IQueryable<BuildingDoorModel> GetPaginatedAccessibleDoors(int id, string filter, short operationType, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);
        IQueryable<BuildingUnAssignDoorModel> GetPaginatedUnAssignDoors(int id, string filter, short operationType, int pageNumber,
            int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        void Delete(int id);
        void DeleteRange(List<Building> buildings);
        void UnAssignDoors(int buildingId, List<int> doorIds);
        void Update(int id, BuildingModel model);
        bool IsExistedBuildingName(int id, string name);
        Building GetById(int id);
        List<Building> GetByIds(List<int> ids);

        List<BuildingListItemModel> GetListBuildingTree(string search, short operationType, out int recordsTotal, out int recordsFiltered, int pageNumber, int pageSize, string sortColumn, string sortDirection);
        List<BuildingListModel> GetListBuildingWithLevel(int level, string search, out int recordsTotal, out int recordsFiltered, int pageNumber, int pageSize, string sortColumn, string sortDirection);
        List<List<string>> GenerateAllBuildingPath(int companyId);

        List<Building> GetChildBuildingByParentIds(List<int> ids);

        BuildingDataModel InitData(BuildingModel buildingData);
        bool IsDefaultBuilding(int buildingId);

        void WriteSystemLog(List<int> logObjIds, ActionLogType actionType, string msg, string detailMsg = "");
    }

    public class BuildingService : IBuildingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly ILogger _logger;
        private readonly IDeviceService _deviceService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;


        public BuildingService(IUnitOfWork unitOfWork, IConfiguration configuration, 
            IHttpContextAccessor httpContextAccessor, ILogger<BuildingService> logger, IDeviceService deviceService)
        {
            _unitOfWork = unitOfWork;
            _httpContext = httpContextAccessor.HttpContext;
            _logger = logger;
            _deviceService = deviceService;
            _configuration = configuration;
            _mapper = MapperInstance.Mapper;
        }

        public List<Building> GetByCompanyId(int companyId)
        {
            try
            {
                var buildings = _unitOfWork.BuildingRepository.GetByCompanyId(companyId).ToList();

                return buildings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCompanyId");
                return new List<Building>();
            }
        }

        public int Add(BuildingModel model)
        {
            int buildingId = 0;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var building = _mapper.Map<Building>(model);
                        building.CompanyId = _httpContext.User.GetCompanyId();
                        if (String.IsNullOrEmpty(model.TimeZone))
                        {
                            var currentAccount = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId());
                            building.TimeZone = currentAccount?.TimeZone ?? Constants.DefaultTimeZone;
                        }
                        _unitOfWork.BuildingRepository.Add(building);
                        _unitOfWork.Save();

                        //Save system log
                        var content = BuildingResource.msgAdd;
                        List<string> details = new List<string>
                        {
                            $"{BuildingResource.lblBuildingName} : {building.Name}",
                            $"{BuildingResource.lblAddress} : {building.Address}",
                            $"{BuildingResource.lblCity} : {building.City}",
                            $"{BuildingResource.lblCountry} : {building.Country}",
                            $"{BuildingResource.lblPostalCode} : {building.PostalCode}",
                            $"{BuildingResource.lblTimezone} : {building.TimeZone}"
                        };
                        var contentsDetails = string.Join("<br />", details);

                        _unitOfWork.SystemLogRepository.Add(building.Id, SystemLogType.Building, ActionLogType.Add,
                            content, contentsDetails, null, _httpContext.User.GetCompanyId());

                        _unitOfWork.Save();
                        transaction.Commit();
                        buildingId = building.Id;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        buildingId = 0;
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                    }
                }
            });
            return buildingId;
        }

        public void AddDoors(Building building, List<int> doorIds)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        List<int> deviceTypesToSend = new List<int>()
                        {
                            (int) DeviceType.Icu300N,
                            (int) DeviceType.Icu300NX,
                            (int) DeviceType.Icu400,
                            (int) DeviceType.ITouchPop,
                            (int) DeviceType.ITouchPopX,
                            (int) DeviceType.DQMiniPlus,
                            (int) DeviceType.IT100,
                            (int) DeviceType.PM85,
                        };

                        var companyId = _httpContext.User.GetCompanyId();
                        var icuDevices = _deviceService.GetByIdsAndCompany(doorIds, companyId, true).ToList();

                        string groupMsgId = "";
                        List<string> doorNames = new List<string>();

                        foreach (var icuDevice in icuDevices)
                        {
                            if (icuDevice.BuildingId == null) continue;

                            if (icuDevice.Building == null)
                            {
                                icuDevice.Building = GetById(icuDevice.BuildingId.Value);
                            }
                            
                            // Change building information
                            icuDevice.BuildingId = building.Id;
                            _unitOfWork.IcuDeviceRepository.Update(icuDevice);
                        }

                        // Divided this loop from above because of the UnitOfWork. (in consumer)
                        foreach (var icuDevice in icuDevices)
                        {
                            if (icuDevice.Building == null)
                            {
                                icuDevice.Building = GetById(icuDevice.BuildingId.Value);
                            }

                            // Send device instruction (set time)
                            IWebSocketService webSocketService = new WebSocketService();
                            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, webSocketService);
                            deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                            {
                                DeviceId = icuDevice.Id,
                                DeviceAddress = icuDevice.DeviceAddress,
                                MessageType = Constants.Protocol.DeviceInstruction,
                                Command = Constants.CommandType.SetTime,
                                MsgId = Guid.NewGuid().ToString(),
                            });
                            // Save door name for system log
                            doorNames.Add(icuDevice.Name);
                        }

                        //Save system log
                        var buildingName = building.Name;
                        var assignedDoorIds = icuDevices.Select(c => c.Id).ToList();
                        var content = string.Format(BuildingResource.msgAssignDoors, buildingName);
                        var contentDetails = $"{DeviceResource.lblDeviceCount} : {assignedDoorIds.Count}<br />" +
                                            $"{BuildingResource.lblAssignDoor}: {string.Join(", ", doorNames)}";

                        _unitOfWork.SystemLogRepository.Add(building.Id, SystemLogType.Building, ActionLogType.AssignDoor,
                            content, contentDetails, assignedDoorIds, companyId);

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
        /// Update building
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        public void Update(int id, BuildingModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var building = _unitOfWork.BuildingRepository.GetById(id);
                        var currentBuildingName = building.Name;

                        if (String.IsNullOrEmpty(model.TimeZone))
                        {
                            var currentAccount = _unitOfWork.AccountRepository.GetById(_httpContext.User.GetAccountId());
                            building.TimeZone = currentAccount?.TimeZone ?? Constants.DefaultTimeZone;
                        }
                        var current = building.TimeZone;

                        List<string> changes = new List<string>();

                        var isChange = CheckChange(building, model, ref changes);

                        _mapper.Map(model, building);
                        _unitOfWork.BuildingRepository.Update(building);

                        if (isChange)
                        {
                            // Save the system log type as update.
                            var content = $"{BuildingResource.msgUpdate}\n{BuildingResource.lblBuildingName} : {currentBuildingName}";
                            var contentsDetails = string.Join("\n", changes);
                            _unitOfWork.SystemLogRepository.Add(building.Id, SystemLogType.Building, ActionLogType.Update,
                                content, contentsDetails, null, _httpContext.User.GetCompanyId());
                        }

                        _unitOfWork.Save();

                        if (model.TimeZone != current)
                        {
                            var devices = _unitOfWork.AppDbContext.IcuDevice.Where(m => m.BuildingId == id && !m.IsDeleted).ToList();
                            if (devices.Any())
                            {
                                IWebSocketService webSocketService = new WebSocketService();
                                var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, webSocketService);
                                foreach (var device in devices)
                                {
                                    deviceInstructionQueue.SendDeviceInstruction(new InstructionQueueModel()
                                    {
                                        DeviceId = device.Id,
                                        DeviceAddress = device.DeviceAddress,
                                        MessageType = Constants.Protocol.DeviceInstruction,
                                        Command = Constants.CommandType.SetTime,
                                        MsgId = Guid.NewGuid().ToString(),
                                    });
                                }
                            }
                        }

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
        
        /// <summary>
        /// Delete building
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public void Delete(int id)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var companyId = _httpContext.User.GetCompanyId();
                        var buildingDefault = _unitOfWork.BuildingRepository.GetDefaultByCompanyIdIgnoreId(companyId, new List<int>(){id});
                        var doors = _unitOfWork.IcuDeviceRepository.GetDoorsByBuildingId(companyId, id, (short)OperationType.Entrance)
                            .ToList();
                        var canteenDevices = _unitOfWork.IcuDeviceRepository.GetDoorsByBuildingId(companyId, id, (short)OperationType.Restaurant)
                            .ToList();
                        foreach (var icuDevice in doors)
                        {
                            icuDevice.BuildingId = buildingDefault.Id;
                            _unitOfWork.IcuDeviceRepository.Update(icuDevice);
                        }
                        foreach (var icuDevice in canteenDevices)
                        {
                            icuDevice.BuildingId = buildingDefault.Id;
                            _unitOfWork.IcuDeviceRepository.Update(icuDevice);
                        }

                        // Delete building data from system
                        var building = _unitOfWork.BuildingRepository.GetById(id);
                        _unitOfWork.BuildingRepository.DeleteFromSystem(building);

                        //Save system log
                        var content = BuildingResource.msgDelete;
                        var contentsDetails = $"{BuildingResource.lblBuildingName} : {building.Name}";

                        _unitOfWork.SystemLogRepository.Add(building.Id, SystemLogType.Building, ActionLogType.Delete,
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

        /// <summary>
        /// Delete a list buildings
        /// </summary>
        /// <param name="buildings"></param>
        public void DeleteRange(List<Building> buildings)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var companyId = _httpContext.User.GetCompanyId();
                        var buildingDefault = _unitOfWork.BuildingRepository.GetDefaultByCompanyIdIgnoreId(companyId, buildings.Select(x => x.Id).ToList());
                        foreach (var building in buildings)
                        {
                            var doors = _unitOfWork.IcuDeviceRepository.GetDoorsByBuildingId(companyId, building.Id, (short)OperationType.Entrance)
                            .ToList();
                            var canteenDevices = _unitOfWork.IcuDeviceRepository.GetDoorsByBuildingId(companyId, building.Id, (short)OperationType.Restaurant)
                                .ToList();
                            foreach (var icuDevice in doors)
                            {
                                icuDevice.BuildingId = buildingDefault.Id;
                                _unitOfWork.IcuDeviceRepository.Update(icuDevice);
                            }
                            foreach (var icuDevice in canteenDevices)
                            {
                                icuDevice.BuildingId = buildingDefault.Id;
                                _unitOfWork.IcuDeviceRepository.Update(icuDevice);
                            }

                            // Delete building data from system
                            _unitOfWork.BuildingRepository.DeleteFromSystem(building);
                        }


                        if (buildings.Count == 1)
                        {
                            //Save system log
                            var building = buildings.First();
                            var content = BuildingResource.msgDelete;
                            var contentsDetails = $"{BuildingResource.lblBuildingName} : {building.Name}";

                            _unitOfWork.SystemLogRepository.Add(building.Id, SystemLogType.Building, ActionLogType.Delete,
                                content, contentsDetails, null, _httpContext.User.GetCompanyId());
                        }
                        else
                        {
                            //Save system log
                            var buildingIds = buildings.Select(c => c.Id).ToList();
                            var buildingNames = buildings.Select(c => c.Name).ToList();
                            var content = string.Format(ActionLogTypeResource.DeleteMultipleType, BuildingResource.lblBuilding);
                            var contentDetails = $"{BuildingResource.lblBuildingName}: {string.Join(", ", buildingNames)}";

                            _unitOfWork.SystemLogRepository.Add(buildingIds.First(), SystemLogType.Building, ActionLogType.DeleteMultiple,
                                content, contentDetails, buildingIds, _httpContext.User.GetCompanyId());
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
        }

        /// <summary>
        /// UnAssign door from building
        /// </summary>
        /// <param name="buildingId"></param>
        /// <param name="doorIds"></param>
        /// <returns></returns>
        public void UnAssignDoors(int buildingId, List<int> doorIds)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var companyId = _httpContext.User.GetCompanyId();
                        //GetAll doors by building
                        var icuDevices = _unitOfWork.IcuDeviceRepository.GetByIdsAndBuildingAndCompany(doorIds, buildingId, companyId);
                        List<string> doorNames = new List<string>();

                        foreach (var icuDevice in icuDevices)
                        {
                            icuDevice.BuildingId = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(companyId).Id;
                            doorNames.Add(icuDevice.Name);

                            _unitOfWork.IcuDeviceRepository.Update(icuDevice);
                            _unitOfWork.Save();
                        }

                        //Save system log
                        var buildingName = _unitOfWork.BuildingRepository.GetById(buildingId);
                        var unAssignedDoorIds = icuDevices.Select(c => c.Id).ToList();

                        var content = string.Format(BuildingResource.msgUnAssignDoors, buildingName);
                        var contentDetails = $"{DeviceResource.lblDeviceCount} : {unAssignedDoorIds.Count}\n" +
                                            $"{BuildingResource.lblUnAssignDoor}: {string.Join(", ", doorNames)}";

                        _unitOfWork.SystemLogRepository.Add(buildingId, SystemLogType.Building, ActionLogType.UnassignDoor,
                            content, contentDetails, unAssignedDoorIds, companyId);

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
        public IQueryable<BuildingListModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var data = _unitOfWork.AppDbContext.Building
                    .Where(m => !m.IsDeleted && m.CompanyId == companyId);
                totalRecords = data.Count();

                if (!string.IsNullOrEmpty(filter))
                {
                    var normalizedFilter = filter.Trim().RemoveDiacritics().ToLower();
                    data = data.AsEnumerable().Where(x => x.Name.RemoveDiacritics().ToLower().Contains(normalizedFilter)).AsQueryable();
                }

                recordsFiltered = data.Count();
                data = data.OrderBy($"{sortColumn} {sortDirection}");
                data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                return data.AsEnumerable<Building>().Select(_mapper.Map<BuildingListModel>).AsQueryable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginated");
                totalRecords = 0;
                recordsFiltered = 0;
                return Enumerable.Empty<BuildingListModel>().AsQueryable();
            }
        }


        /// <summary>
        /// Get data with pagination
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public IQueryable<BuildingDoorModel> GetPaginatedDoors(int id, string filter, short operationType, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var data = _unitOfWork.IcuDeviceRepository.GetDoorsByBuildingId(companyId, id, operationType).ToList();

                var accountTypeTemp = _httpContext.User.GetAccountType();
                var accountIdTemp = _httpContext.User.GetAccountId();
                var companyIdTemp = _httpContext.User.GetCompanyId();


                // check plugin
                var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyIdTemp);
                PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
                if (plugIns.DepartmentAccessLevel && accountTypeTemp == (short)AccountType.DynamicRole)
                {
                    List<int> doorIdList = _unitOfWork.DepartmentDeviceRepository.GetDoorIdsByAccountDepartmentRole(companyIdTemp, accountIdTemp);
                    data = data.Where(x => doorIdList.Contains(x.Id)).ToList();
                }

                totalRecords = data.Count();

                if (!string.IsNullOrEmpty(filter))
                {
                    filter = filter.Trim().RemoveDiacritics().ToLower();
                    data = data.Where(x =>
                        x.Name.RemoveDiacritics().ToLower().Contains(filter) ||
                        x.DeviceAddress.RemoveDiacritics().ToLower().Contains(filter) ||
                        //x.Building.Name.ToLower().Contains(filter.ToLower()) ||
                        ((DeviceType)x.DeviceType).GetDescription().RemoveDiacritics().ToLower().Contains(filter) ||
                        x.ActiveTz.Name.RemoveDiacritics().ToLower().Contains(filter) ||
                        x.PassageTz.Name.RemoveDiacritics().ToLower().Contains(filter)).ToList();
                }

                recordsFiltered = data.Count();

                var result = data.AsEnumerable<IcuDevice>().Select(_mapper.Map<BuildingDoorModel>).AsQueryable();

                // Default sort ( asc - DoorName )
                result = result.OrderBy(c => c.DoorName);

                try
                {
                    int intSortColumn = Int32.Parse(sortColumn);

                    intSortColumn = intSortColumn > ColumnDefines.BuildingDoorList.Length - 1 ? 0 : intSortColumn;
                    result = result.OrderBy($"{ColumnDefines.BuildingDoorList[intSortColumn]} {sortDirection}");
                }
                catch
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        sortColumn = Char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);

                        if (result.FirstOrDefault()?.GetType().GetProperty(sortColumn) != null)
                        {
                            if (sortDirection.Equals("desc"))
                            {
                                result = result.OrderByDescending(c => c.GetType().GetProperty(sortColumn).GetValue(c, null));
                            }
                            else if (sortDirection.Equals("asc"))
                            {
                                result = result.OrderBy(c => c.GetType().GetProperty(sortColumn).GetValue(c, null));
                            }
                        }
                    }
                }

                if (pageSize > 0)
                    result = result.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginatedDoors");
                totalRecords = 0;
                recordsFiltered = 0;
                return Enumerable.Empty<BuildingDoorModel>().AsQueryable();
            }
        }


        /// <summary>
        /// Get data with pagination
        /// </summary>
        /// <param name="ids"> list of building identifier </param>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public List<BuildingDoorModel> GetPaginatedDoors(List<int> ids, string filter, short operationType, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var data = _unitOfWork.IcuDeviceRepository.GetDevicesByBuildingIds(ids, companyId)
                    .Include(c => c.ActiveTz)
                    .Include(c => c.PassageTz)
                    .ToList();

            var accountTypeTemp = _httpContext.User.GetAccountType();
            var accountIdTemp = _httpContext.User.GetAccountId();
            var companyIdTemp = _httpContext.User.GetCompanyId();


            // check plugin
            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyIdTemp);
            PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
            if (plugIns.DepartmentAccessLevel && accountTypeTemp == (short)AccountType.DynamicRole)
            {
                List<int> doorIdList = _unitOfWork.DepartmentDeviceRepository.GetDoorIdsByAccountDepartmentRole(companyIdTemp, accountIdTemp);
                data = data.Where(x => doorIdList.Contains(x.Id)).ToList();
            }

            List<short> operationTypes = new List<short>();
            operationTypes.Add(operationType);

            if (operationType == (short)OperationType.Entrance)
            {
                operationTypes.Add((short)OperationType.FireDetector);
                operationTypes.Add((short)OperationType.Reception);
            }

            data = data.Where(m => operationTypes.Contains(m.OperationType)).ToList();

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.Where(x =>
                    x.Name.RemoveDiacritics().ToLower().Contains(filter) ||
                    x.DeviceAddress.RemoveDiacritics().ToLower().Contains(filter) ||
                    ((DeviceType)x.DeviceType).GetDescription().RemoveDiacritics().ToLower().Contains(filter) ||
                    (x.ActiveTz != null && x.ActiveTz.Name.RemoveDiacritics().ToLower().Contains(filter)) ||
                    (x.PassageTz != null && x.PassageTz.Name.RemoveDiacritics().ToLower().Contains(filter))).ToList();
            }

            recordsFiltered = data.Count();

            var result = data.AsEnumerable<IcuDevice>().Select(_mapper.Map<BuildingDoorModel>).AsQueryable();

            // Default sort ( asc - DoorName )
            result = result.OrderBy(c => c.DoorName);

            try
            {
                int intSortColumn = Int32.Parse(sortColumn);

                intSortColumn = intSortColumn > ColumnDefines.BuildingDoorList.Length - 1 ? 0 : intSortColumn;
                result = result.OrderBy($"{ColumnDefines.BuildingDoorList[intSortColumn]} {sortDirection}");
            }
            catch
            {
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    sortColumn = Char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);

                    if (result.FirstOrDefault()?.GetType().GetProperty(sortColumn) != null)
                    {
                        if (sortDirection.Equals("desc"))
                        {
                            result = result.OrderByDescending(c => c.GetType().GetProperty(sortColumn).GetValue(c, null));
                        }
                        else if (sortDirection.Equals("asc"))
                        {
                            result = result.OrderBy(c => c.GetType().GetProperty(sortColumn).GetValue(c, null));
                        }
                    }
                }
            }

            if(pageSize > 0 && pageNumber > 0)
                result = result.Skip((pageNumber - 1) * pageSize).Take(pageSize);


            var accountTimezone = _unitOfWork.AccountRepository.Get(m =>
                    m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted).TimeZone;

            TimeZoneInfo cstZone = accountTimezone.ToTimeZoneInfo();

            var dataResult = new List<BuildingDoorModel>();

                foreach (var device in result)
                {
                    device.LastCommunicationTime = DateTime.TryParseExact(device.LastCommunicationTime, Constants.Settings.DateTimeFormatDefault, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtValue)
                        ? dtValue.ConvertDefaultDateTimeToString() : device.LastCommunicationTime;

                    dataResult.Add(device);
                }

                return dataResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginatedDoors");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<BuildingDoorModel>();
            }
        }

        public IQueryable<BuildingDoorModel> GetPaginatedAccessibleDoors(int id, string filter, short operationType, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var accountId = _httpContext.User.GetAccountId();
                var user = _unitOfWork.UserRepository.GetUserByAccountId(accountId, companyId);

                var data = _unitOfWork.IcuDeviceRepository.GetDoorsByBuildingId(companyId, id, operationType)
                    .Include(m => m.AccessGroupDevice)
                    .Where(m => m.AccessGroupDevice.Any(n => n.AccessGroupId == user.AccessGroupId));
                totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x =>
                    x.Name.RemoveDiacritics().ToLower().Contains(filter) ||
                    x.DeviceAddress.RemoveDiacritics().ToLower().Contains(filter) ||
                    //x.Building.Name.ToLower().Contains(filter.ToLower()) ||
                    ((DeviceType)x.DeviceType).GetDescription().RemoveDiacritics().ToLower().Contains(filter) ||
                    x.ActiveTz.Name.RemoveDiacritics().ToLower().Contains(filter) ||
                    x.PassageTz.Name.RemoveDiacritics().ToLower().Contains(filter)).AsQueryable();
            }

            recordsFiltered = data.Count();

            var result = data.AsEnumerable<IcuDevice>().Select(_mapper.Map<BuildingDoorModel>).AsQueryable();

            // Default sort ( asc - DoorName )
            result = result.OrderBy(c => c.DoorName);

            try
            {
                int intSortColumn = Int32.Parse(sortColumn);

                intSortColumn = intSortColumn > ColumnDefines.BuildingDoorList.Length - 1 ? 0 : intSortColumn;
                result = result.OrderBy($"{ColumnDefines.BuildingDoorList[intSortColumn]} {sortDirection}");
            }
            catch
            {
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    sortColumn = Char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);

                    if (result.FirstOrDefault()?.GetType().GetProperty(sortColumn) != null)
                    {
                        if (sortDirection.Equals("desc"))
                        {
                            result = result.OrderByDescending(c => c.GetType().GetProperty(sortColumn));
                        }
                        else if (sortDirection.Equals("asc"))
                        {
                            result = result.OrderBy(c => c.GetType().GetProperty(sortColumn));
                        }
                    }
                }
            }

                result = result.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginatedAccessibleDoors");
                totalRecords = 0;
                recordsFiltered = 0;
                return Enumerable.Empty<BuildingDoorModel>().AsQueryable();
            }
        }

        /// <summary>
        /// Get data with pagination
        /// </summary>
        /// <param name="id"></param>
        /// <param name="filter"></param>
        /// <param name="pageNumber"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public IQueryable<BuildingUnAssignDoorModel> GetPaginatedUnAssignDoors(int id, string filter, short operationType, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var accountId = _httpContext.User.GetAccountId();
                var data = _unitOfWork.IcuDeviceRepository.GetUnAssignDoorsByBuildingId(companyId, id, operationType);
                totalRecords = data.Count();
            
            // check door list with department access level
            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
            PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
            if (plugIns.DepartmentAccessLevel && _httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
            {
                var listDepartments = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
                var listIcuOfDepartment = _unitOfWork.DepartmentDeviceRepository.GetByDepartmentIds(listDepartments)
                    .Select(x => x.IcuId).Distinct().ToList();
                data = data.Where(x => listIcuOfDepartment.Contains(x.Id));
            }
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x =>
                    x.Name.RemoveDiacritics().ToLower().Contains(filter) ||
                    x.DeviceAddress.RemoveDiacritics().ToLower().Contains(filter) ||
                    x.Building.Name.RemoveDiacritics().ToLower().Contains(filter)).AsQueryable();
            }

            recordsFiltered = data.Count();
            data = data.OrderBy(c => c.Name);
            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            var result = data.AsEnumerable<IcuDevice>().Select(_mapper.Map<BuildingUnAssignDoorModel>).AsQueryable();
            try
            {
                int intSortColumn = Int32.Parse(sortColumn);

                intSortColumn = intSortColumn > ColumnDefines.BuildingUnAssignDoorList.Length - 1 ? 0 : intSortColumn;
                result = result.OrderBy($"{ColumnDefines.BuildingUnAssignDoorList[intSortColumn]} {sortDirection}");
            }
            catch
            {
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    sortColumn = Char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);

                    if (result.FirstOrDefault()?.GetType().GetProperty(sortColumn) != null)
                    {
                        if (sortDirection.Equals("desc"))
                        {
                            result = result.OrderByDescending(c => c.GetType().GetProperty(sortColumn).GetValue(c, null));
                        }
                        else if (sortDirection.Equals("asc"))
                        {
                            result = result.OrderBy(c => c.GetType().GetProperty(sortColumn).GetValue(c, null));
                        }
                    }
                }
            }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginatedUnAssignDoors");
                totalRecords = 0;
                recordsFiltered = 0;
                return Enumerable.Empty<BuildingUnAssignDoorModel>().AsQueryable();
            }
        }

        /// <summary>
        /// Check exist building
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsExistedBuildingName(int id, string name)
        {
            try
            {
                return _unitOfWork.AppDbContext.Building.Any(c => !c.IsDeleted && c.Id != id && c.Name.ToLower() == name.ToLower() && c.CompanyId == _httpContext.User.GetCompanyId());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsExistedBuildingName");
                return false;
            }
        }

        /// <summary>
        /// Get building by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Building GetById(int id)
        {
            try
            {
                return _unitOfWork.BuildingRepository.GetByIdAndCompanyId(_httpContext.User.GetCompanyId(), id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return null;
            }
        }

        /// <summary>
        /// Get get building
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<Building> GetByIds(List<int> ids)
        {
            try
            {
                return _unitOfWork.BuildingRepository.GetByIdsAndCompanyId(_httpContext.User.GetCompanyId(), ids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIds");
                return new List<Building>();
            }
        }

        /// <summary>
        /// Checking if there are any changes.
        /// </summary>
        /// <param name="building">Building that contains existing information</param>
        /// <param name="model">Model that contains new information</param>
        /// <param name="changes">List of changes</param>
        /// <returns></returns>
        internal bool CheckChange(Building building, BuildingModel model, ref List<string> changes)
        {
            if (model.Id != 0)
            {
                if (building.Name != model.Name)
                {
                    changes.Add(Helpers.CreateChangedValueContents(BuildingResource.lblBuildingName, building.Name, model.Name));
                }

                if (building.Address != model.Address)
                {
                    changes.Add(Helpers.CreateChangedValueContents(BuildingResource.lblAddress, building.Address, model.Address));
                }

                if (building.City != model.City)
                {
                    changes.Add(Helpers.CreateChangedValueContents(BuildingResource.lblCity, building.City, model.City));

                }

                if (building.Country != model.Country)
                {
                    changes.Add(Helpers.CreateChangedValueContents(BuildingResource.lblCountry, building.Country, model.Country));

                }

                if (building.PostalCode != model.PostalCode)
                {
                    changes.Add(Helpers.CreateChangedValueContents(BuildingResource.lblPostalCode, building.PostalCode, model.PostalCode));

                }

                if (building.ParentId != model.ParentId)
                {
                    var oldBuildingName = _unitOfWork.BuildingRepository.GetById(building.ParentId ?? 0)?.Name;
                    var newBuildingName = _unitOfWork.BuildingRepository.GetById(model.ParentId ?? 0)?.Name;

                    changes.Add(Helpers.CreateChangedValueContents(BuildingResource.lblParentBuilding, oldBuildingName, newBuildingName));
                }
            }

            return changes.Any();
        }
        
        public List<BuildingListItemModel> GetListBuildingTree(string search, short operationType, out int recordsTotal, out int recordsFiltered, int pageNumber, int pageSize, string sortColumn, string sortDirection)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var accountId = _httpContext.User.GetAccountId();
                var accountType = _httpContext.User.GetAccountType();

                var data = _unitOfWork.AppDbContext.Building
                    .Include(m => m.Parent)
                    .Include(m => m.IcuDevice).ThenInclude(n => n.ActiveTz)
                    .Include(m => m.IcuDevice).ThenInclude(n => n.PassageTz)
                    .Where(m => !m.IsDeleted && m.CompanyId == companyId);
                recordsTotal = data.Count();
            
            var listDoorOfDepartment = new List<int>(); // departmentId of user have dynamic role enable department level
            if (CheckPluginDepartmentAccessLevel() && accountType == (short)AccountType.DynamicRole)
            {
                listDoorOfDepartment = _unitOfWork.DepartmentDeviceRepository.GetDoorIdsByAccountDepartmentRole(companyId, accountId);
            }

            List<short> operationTypes = new List<short>();
            operationTypes.Add(operationType);

            if (operationType == (short)OperationType.Entrance)
            {
                operationTypes.Add((short)OperationType.FireDetector);
                operationTypes.Add((short)OperationType.Reception);
            }


            var accountTypeTemp = _httpContext.User.GetAccountType();
            var accountIdTemp = _httpContext.User.GetAccountId();
            var companyIdTemp = _httpContext.User.GetCompanyId();


            // check plugin
            List<int> doorIdList = new List<int>();
            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyIdTemp);
            PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
            if (plugIns.DepartmentAccessLevel && accountTypeTemp == (short)AccountType.DynamicRole)
            {
                doorIdList = _unitOfWork.DepartmentDeviceRepository.GetDoorIdsByAccountDepartmentRole(companyIdTemp, accountIdTemp);
            }

            // building list ignore search filter
            var buildingsIgnoreSearch = data.AsEnumerable<Building>().Select(m => new BuildingListItemModel()
            {
                Id = m.Id,
                Address = m.Address,
                TimeZone = m.TimeZone,
                Name = m.Name,
                ParentId = m.ParentId ?? 0,
                ParentName = m.Parent?.Name,
                DoorList = (plugIns.DepartmentAccessLevel &&             accountTypeTemp == (short)AccountType.DynamicRole)
                     ? 
                     m.IcuDevice.ToList()
                    .Where(n => doorIdList.Contains(n.Id) && !n.IsDeleted && n.Status == (short)Status.Valid 
                        && operationTypes.Contains(n.OperationType) && (!listDoorOfDepartment.Any() || listDoorOfDepartment.Contains(n.Id)))
                    .Select(n => new BuildingDoorModel()
                    {
                        Id = n.Id,
                        ActiveTz = n.ActiveTz?.Name,
                        PassageTz = n.PassageTz?.Name,
                        DeviceAddress = n.DeviceAddress,
                        DeviceType = ((DeviceType)n.DeviceType).GetDescription(),
                        DoorName = n.Name,
                        DoorStatus = n.DoorStatus,
                        DoorStatusId = n.DoorStatusId,
                        ConnectionStatus = n.ConnectionStatus,
                        OperationTypeId = n.OperationType,
                        VerifyModeId = n.VerifyMode,
                        OperationType = ((OperationType)n.OperationType).GetDescription(),
                        VerifyMode = ((VerifyMode)n.VerifyMode).GetDescription(),
                    }).ToList()
                    :
                     m.IcuDevice.ToList()
                    .Where(n => !n.IsDeleted && n.Status == (short)Status.Valid 
                        && operationTypes.Contains(n.OperationType) && (!listDoorOfDepartment.Any() || listDoorOfDepartment.Contains(n.Id)))
                    .Select(n => new BuildingDoorModel()
                    {
                        Id = n.Id,
                        ActiveTz = n.ActiveTz?.Name,
                        PassageTz = n.PassageTz?.Name,
                        DeviceAddress = n.DeviceAddress,
                        DeviceType = ((DeviceType)n.DeviceType).GetDescription(),
                        DoorName = n.Name,
                        DoorStatus = n.DoorStatus,
                        DoorStatusId = n.DoorStatusId,
                        ConnectionStatus = n.ConnectionStatus,
                        OperationTypeId = n.OperationType,
                        VerifyModeId = n.VerifyMode,
                        OperationType = ((OperationType)n.OperationType).GetDescription(),
                        VerifyMode = ((VerifyMode)n.VerifyMode).GetDescription(),
                    }).ToList()
            }).ToList();

            recordsFiltered = data.Count();

            data = data.OrderBy($"{sortColumn} {sortDirection}");

            //if(pageSize > 0)
            //{
            //    data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
            //}

            var buildings = data.AsEnumerable<Building>().Select(m => new BuildingListItemModel()
            {
                Id = m.Id,
                Address = m.Address,
                TimeZone = m.TimeZone,
                Name = m.Name,
                ParentId = m.ParentId ?? 0,
                ParentName = m.Parent?.Name,
                DoorList = (plugIns.DepartmentAccessLevel &&          accountTypeTemp == (short)AccountType.DynamicRole)
                    ? 
                    m.IcuDevice.ToList()
                    .Where(n => doorIdList.Contains(n.Id) && !n.IsDeleted && n.Status == (short)Status.Valid 
                        && operationTypes.Contains(n.OperationType) && (!listDoorOfDepartment.Any() || listDoorOfDepartment.Contains(n.Id)))
                    .Select(n => new BuildingDoorModel()
                    {
                        Id = n.Id,
                        ActiveTz = n.ActiveTz?.Name,
                        PassageTz = n.PassageTz?.Name,
                        DeviceAddress = n.DeviceAddress,
                        DeviceType = ((DeviceType) n.DeviceType).GetDescription(),
                        DoorName = n.Name,
                        DoorStatus = n.DoorStatus,
                        DoorStatusId = n.DoorStatusId,
                        ConnectionStatus = n.ConnectionStatus,
                        OperationTypeId = n.OperationType,
                        VerifyModeId = n.VerifyMode,
                        OperationType = ((OperationType)n.OperationType).GetDescription(),
                        VerifyMode = ((VerifyMode)n.VerifyMode).GetDescription(),
                    }).ToList()
                    :
                    m.IcuDevice.ToList()
                    .Where(n => !n.IsDeleted && n.Status == (short)Status.Valid 
                        && operationTypes.Contains(n.OperationType) && (!listDoorOfDepartment.Any() || listDoorOfDepartment.Contains(n.Id)))
                    .Select(n => new BuildingDoorModel()
                    {
                        Id = n.Id,
                        ActiveTz = n.ActiveTz?.Name,
                        PassageTz = n.PassageTz?.Name,
                        DeviceAddress = n.DeviceAddress,
                        DeviceType = ((DeviceType) n.DeviceType).GetDescription(),
                        DoorName = n.Name,
                        DoorStatus = n.DoorStatus,
                        DoorStatusId = n.DoorStatusId,
                        ConnectionStatus = n.ConnectionStatus,
                        OperationTypeId = n.OperationType,
                        VerifyModeId = n.VerifyMode,
                        OperationType = ((OperationType)n.OperationType).GetDescription(),
                        VerifyMode = ((VerifyMode)n.VerifyMode).GetDescription(),
                    }).ToList()
            }).ToList();

            // Apply search filter to the in-memory list
            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = search.Trim().RemoveDiacritics().ToLower();
                buildings = buildings.Where(m => m.Name.RemoveDiacritics().ToLower().Contains(normalizedSearch)).ToList();
            }

            List<BuildingListItemModel> lst = new List<BuildingListItemModel>();
            List<BuildingListItemModel> listBuildingHaveParent = new List<BuildingListItemModel>();
            foreach (var item in buildings)
            {
                var lstBuilding = GenerateTree(buildingsIgnoreSearch, item);

                lst.Add(new BuildingListItemModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Address = item.Address,
                    TimeZone = item.TimeZone,
                    ParentId = item.ParentId,
                    ParentName = item.ParentName,
                    Children = lstBuilding,
                    DoorList = item.DoorList
                });
                if(item.ParentId != 0)
                {
                    listBuildingHaveParent.Add(new BuildingListItemModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Address = item.Address,
                        TimeZone = item.TimeZone,
                        ParentId = item.ParentId,
                        ParentName = item.ParentName,
                        Children = lstBuilding,
                        DoorList = item.DoorList
                    });
                }
            }

            List<BuildingListItemModel> lstTemp1 = new List<BuildingListItemModel>();
            List<BuildingListItemModel> lstTemp2 = new List<BuildingListItemModel>();
            foreach (var bdHaveParent in listBuildingHaveParent)
            {
                var check = lst.Where(m => m.Id == bdHaveParent.ParentId).FirstOrDefault();
                if(check != null)
                {
                    lstTemp1.Add(bdHaveParent);
                } else
                {
                    lstTemp2.Add(bdHaveParent);
                }
               
            }

            List<BuildingListItemModel> lstResult = new List<BuildingListItemModel>();
            foreach (var bd in lst)
            {
                var check = lstTemp1.Where(m=> m.Id == bd.Id).FirstOrDefault();
                if(check != null)
                {
                    
                } else
                {
                    lstResult.Add(bd);
                }
               
            }
            foreach (var bd in lstTemp2)
            {
                var check = lstResult.Where(m=> m.Id == bd.Id).FirstOrDefault();
                if(check != null)
                {
                    
                } else
                {
                    lstResult.Add(bd);
                }
               
            }

            // var resultList = lst.Where(x => x.ParentId == 0).ToList();
            var resultList = !string.IsNullOrEmpty(search) 
                            ? lstResult.ToList() 
                            : lst.Where(x => x.ParentId == 0).ToList();

            recordsFiltered = resultList.Count();

                if (pageSize > 0)
                {
                    resultList = resultList.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }

                return resultList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetListBuildingTree");
                recordsTotal = 0;
                recordsFiltered = 0;
                return new List<BuildingListItemModel>();
            }
        }

        private bool CheckPluginDepartmentAccessLevel()
        {
            try
            {
                var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(_httpContext.User.GetCompanyId());
                PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
                return plugIns.DepartmentAccessLevel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckPluginDepartmentAccessLevel");
                return false;
            }
        }
        
        private List<BuildingListItemModel> GenerateTree(List<BuildingListItemModel> collection, BuildingListItemModel rootItem)
        {
            List<BuildingListItemModel> lst = new List<BuildingListItemModel>();
            var listCollection = collection.Where(m => m.ParentId == rootItem.Id).ToList();
            foreach (BuildingListItemModel item in listCollection)
            {
                lst.Add(new BuildingListItemModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Address = item.Address,
                    TimeZone = item.TimeZone,
                    Children = GenerateTree(collection, item),
                    ParentId = rootItem.Id,
                    ParentName = rootItem.Name,
                    DoorList = item.DoorList
                });
            }
            return lst;
        }

        public List<BuildingListModel> GetListBuildingWithLevel(int level, string search, out int recordsTotal, out int recordsFiltered, int pageNumber, int pageSize, string sortColumn, string sortDirection)
        {
            try
            {
                var buildingsTree = GetListBuildingTree(search,0, out recordsTotal, out recordsFiltered, 1, 99999999, sortColumn, sortDirection);
                List<BuildingListModel> results = new List<BuildingListModel>();
            foreach (var building in buildingsTree)
            {
                List<BuildingListItemModel> lstTemp = new List<BuildingListItemModel>();
                lstTemp.Add(building);
                for (int i = 0; i <= level; i++)
                {
                    if (i == level)
                    {
                        if (lstTemp.Count > 0)
                        {
                            foreach (var temp in lstTemp)
                            {
                                results.Add(new BuildingListModel()
                                {
                                    Id = temp.Id,
                                    Name = temp.Name
                                });
                            }
                        }
                        break;
                    }
                    else
                    {
                        var changeLstTemp = new List<BuildingListItemModel>();
                        foreach (var item in lstTemp)
                        {
                            if(item.Children != null && item.Children.Any())
                                changeLstTemp.AddRange(item.Children);
                        }

                        lstTemp = changeLstTemp;
                        if(!lstTemp.Any()) break;
                    }
                }
            }

            recordsTotal = results.Count;
            if (recordsTotal == 0)
            {
                recordsFiltered = 0;
                return new List<BuildingListModel>();
            }
            if (!string.IsNullOrEmpty(search))
            {
                var normalizedSearch = search.Trim().RemoveDiacritics().ToLower();
                results = results.Where(m => m.Name.RemoveDiacritics().ToLower().Contains(normalizedSearch)).ToList();
            }

                recordsFiltered = results.Count;
                if (recordsFiltered != 0 && pageSize > 0)
                {
                    return results.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetListBuildingWithLevel");
                recordsTotal = 0;
                recordsFiltered = 0;
                return new List<BuildingListModel>();
            }
        }

        public List<List<string>> GenerateAllBuildingPath(int companyId)
        {
            try
            {
                var data = _unitOfWork.AppDbContext.Building.Include(m => m.Parent)
                    .Where(m => !m.IsDeleted && m.CompanyId == companyId);

                if (!data.Any()) return new List<List<string>>();
            
            var buildings = data.AsEnumerable<Building>().Select(m => new BuildingListItemModel()
            {
                Id = m.Id,
                Name = m.Name,
                ParentId = m.ParentId ?? 0,
                ParentName = m.Parent?.Name
            }).ToList();
            
            List<List<string>> result = new List<List<string>>();

            foreach (var item in buildings)
            {
                List<BuildingListItemModel> temp = new List<BuildingListItemModel>();
                var tempBuilding = item;
                List<string> buildingNames = new List<string>();

                while (tempBuilding != null)
                {
                    temp.Add(tempBuilding);
                    int parentId = tempBuilding.ParentId;
                    tempBuilding = buildings.FirstOrDefault(m => m.Id == parentId);
                }

                for (int i = temp.Count - 1; i >= 0; i--)
                {
                    buildingNames.Add(temp[i].Name);
                }
                
                result.Add(buildingNames);
            }
            
            
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateAllBuildingPath");
                return new List<List<string>>();
            }
        }

        public List<Building> GetChildBuildingByParentIds(List<int> ids)
        {
            try
            {
                var childBuildings = _unitOfWork.AppDbContext.Building.Where(m => m.ParentId != null && ids.Contains(m.ParentId.Value) && !m.IsDeleted);

                return childBuildings.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetChildBuildingByParentIds");
                return new List<Building>();
            }
        }


        public BuildingDataModel InitData(BuildingModel buildingData)
        {
            try
            {
                BuildingDataModel result = _mapper.Map<BuildingDataModel>(buildingData);
                var parentBuildings = GetListBuildingWithLevel(0, "", out var _, out var _, 0, 0, "name", "asc");
                if(parentBuildings != null && parentBuildings.Any())
                {
                    result.ParentBuildings = parentBuildings.Where(pb => pb.Id != buildingData.Id).ToList();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitData");
                return null;
            }
        }

        public bool IsDefaultBuilding(int buildingId)
        {
            try
            {
                var defaultBuilding = _unitOfWork.BuildingRepository.GetDefaultByCompanyId(_httpContext.User.GetCompanyId());
                if (defaultBuilding != null && defaultBuilding.Id == buildingId)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsDefaultBuilding");
                return false;
            }
        }
        
        public void WriteSystemLog(List<int> logObjIds, ActionLogType actionType, string msg, string detailMsg = "")
        {
            try
            {
                _unitOfWork.SystemLogRepository.Add(logObjIds.First(), SystemLogType.Building, actionType,
                            msg, detailMsg, logObjIds, _httpContext.User.GetCompanyId());

                _unitOfWork.Save();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}