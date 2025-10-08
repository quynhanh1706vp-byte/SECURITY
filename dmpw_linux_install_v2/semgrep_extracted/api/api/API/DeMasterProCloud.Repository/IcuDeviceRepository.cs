
using System;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Device;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Repository
{
    public interface IIcuDeviceRepository : IGenericRepository<IcuDevice>
    {
        List<IcuDevice> GetUndeletedDevice();
        IcuDevice GetDeviceByCompanyAndAddress(int companyId, string icuAddr);
        IcuDevice GetActiveDeviceByCompanyAndAddress(int companyId, string icuAddr);
        IcuDevice GetDeviceByAddress(string icuAddr, bool isCompany = true);
        IcuDevice GetDeviceByMacAddress(string macAddr);
        List<IcuDevice> GetByIdsAndCompany(List<int> icuIds, int companyId);
        IQueryable<IcuDevice> GetDevicesByCompany(int companyId);
        List<IcuDevice> GetByIdsAndBuildingAndCompany(List<int> idArr, int buildingId, int companyId);
        IcuDevice GetByIcuId(int icuId);
        List<IcuDevice> GetByIds(List<int> idArr);
        IQueryable<IcuDevice> GetActiveDevicesByCompany(int companyId);
        List<IcuDevice> GetNotDeletedDevices();
        IQueryable<IcuDevice> GetDevicesByAccessGroup(int companyId, int accessGroupId);
        IQueryable<IcuDevice> GetUnAssignDevicesByCompany(int companyId, int accessGroupId);
        IQueryable<IcuDevice> GetUnAssignDevicesByDepartment(int companyId, int accessGroupId, List<int> departmentIds);
        IQueryable<IcuDevice> GetAssignDevicesByCompany(int companyId, int accessGroupId);
        IQueryable<IcuDevice> GetAssignDevicesByDepartment(int companyId, int accessGroupId, List<int> departmentIds);
        IQueryable<IcuDevice> GetDoorsByBuildingId(int companyId, int buildingId, short operationType);
        IQueryable<IcuDevice> GetUnAssignDoorsByBuildingId(int companyId, int buildingId, short operationType);
        bool HasTimezone(int timezoneId, int companyId);
        List<IcuDevice> GetByTimezoneId(int companyId, int timezoneId);
        List<IcuDevice> GetValidDoorsByCompany(int companyId);
        IEnumerable<IcuDevice> GetDoors(int? companyId = null);
        IcuDevice GetByIdAndCompanyId(int companyId, int id);

        List<IcuDevice> GetByCompany(int companyId);
        IcuDevice GetByIdAndCompanyIdWithBuilding(int companyId, int id);

        IcuDevice GetDeviceByRid(int companyId, string rid);
        List<EventLog> GetEventLogData(int deviceId);
        List<EventLog> GetRecentEventLogData(int deviceId);
        void UpdateDevice(IcuDevice IcuDevice);
        void ReUpdateUpTimOnlineDevice();
        void ReUpdateUpTimOnlineDeviceById(int id);

        IEnumerable<Node> GetAGDeviceHierarchy(List<IcuDevice> devices, int accessGroupId);
        List<RecentlyDisconnectedDeviceModel> getRecentlyDisconnectedDevices();

        IQueryable<IcuDevice> GetDeviceAllInfoByCompany(int companyId);
        IEnumerable<IcuDevice> GetDeviceAllInfoByCompanyE(int companyId);
        IQueryable<IcuDevice> GetDeviceIncludeBuildingAndAGDForUser(int companyId, int userId);
        IQueryable<IcuDevice> GetDevicesByBuildingIds(List<int> buildingIds, int companyId);
        List<IcuDevice> GetDevicesByUserId(int userId);
    }
    public class IcuDeviceRepository : GenericRepository<IcuDevice>, IIcuDeviceRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        //private readonly HttpContext _httpContext;

        public IcuDeviceRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<IcuDeviceRepository>();
            //_httpContext = contextAccessor.HttpContext;
        }

        /// <summary>
        /// Get Undeleted Devices
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="icuAddr"></param>
        /// <returns></returns>
        public List<IcuDevice> GetUndeletedDevice()
        {
            return _dbContext.IcuDevice.Include(m => m.Company).Where(m => !m.IsDeleted).ToList();
        }


        /// <summary>
        /// Get device by company and device address
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="icuAddr"></param>
        /// <returns></returns>
        public IcuDevice GetDeviceByCompanyAndAddress(int companyId, string icuAddr)
        {
            return _dbContext.IcuDevice.AsNoTracking()
                .Include(c => c.Company).Include(c => c.ActiveTz).Include(c => c.PassageTz).FirstOrDefault(c => c.CompanyId == companyId && c.DeviceAddress == icuAddr
                && !c.IsDeleted
                && !c.Company.IsDeleted);
        }

        /// <summary>
        /// Get active devices by company and device address
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="icuAddr"></param>
        /// <returns></returns>
        public IcuDevice GetActiveDeviceByCompanyAndAddress(int companyId, string icuAddr)
        {
            return _dbContext.IcuDevice.Include(c => c.Company).FirstOrDefault(c =>
                c.CompanyId == companyId && c.DeviceAddress == icuAddr && c.Status == (short)Status.Valid &&
                !c.IsDeleted &&
                !c.Company.IsDeleted);
        }

        /// <summary>
        /// Get IcuDevice by device address
        /// </summary>
        /// <param name="icuAddr">device address</param>
        /// <param name="isCompany">is device assign to company?</param>
        /// <returns></returns>
        public IcuDevice GetDeviceByAddress(string icuAddr, bool isCompany = true)
        {
            if (isCompany)
            {
                return _dbContext.IcuDevice.Include(c => c.Company).FirstOrDefault(i => i.DeviceAddress == icuAddr && !i.IsDeleted && i.CompanyId != null);
            }
            else
            {
                return _dbContext.IcuDevice.Include(c => c.Company).FirstOrDefault(i => i.DeviceAddress == icuAddr && !i.IsDeleted);
            }
        }

        /// <summary>
        /// Get IcuDevice by mac address
        /// </summary>
        /// <param name="macAddr"></param>
        /// <returns></returns>
        public IcuDevice GetDeviceByMacAddress(string macAddr)
        {
            return _dbContext.IcuDevice.Include(c => c.Company).FirstOrDefault(i => i.MacAddress.ToLower().Equals(macAddr.ToLower()) && !i.IsDeleted);
        }

        /// <summary>
        /// Get devices by list of id and company
        /// </summary>
        /// <param name="idArr"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<IcuDevice> GetByIdsAndCompany(List<int> idArr, int companyId)
        {
            return _dbContext.IcuDevice.Include(c => c.Company)
                .Where(m => idArr.Contains(m.Id) && m.CompanyId == companyId &&
                            !m.IsDeleted && !m.Company.IsDeleted).ToList();
        }

        /// <summary>
        /// Get devices by list of id
        /// </summary>
        /// <param name="idArr"></param>
        /// <returns></returns>
        public List<IcuDevice> GetByIds(List<int> idArr)
        {
            return _dbContext.IcuDevice.Include(c => c.Company)
                .Where(m => idArr.Contains(m.Id) &&
                            !m.IsDeleted && !m.Company.IsDeleted).ToList();
        }

        /// <summary>
        /// Get devices by list of id and company
        /// </summary>
        /// <param name="idArr"></param>
        /// <param name="buildingId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<IcuDevice> GetByIdsAndBuildingAndCompany(List<int> idArr, int buildingId, int companyId)
        {
            return _dbContext.IcuDevice.Where(m =>
                    idArr.Contains(m.Id) && m.BuildingId == buildingId && m.CompanyId == companyId && !m.IsDeleted)
                .ToList();
        }

        /// <summary>
        /// Get by icu id
        /// </summary>
        /// <param name="icuId"></param>
        /// <returns></returns>
        public IcuDevice GetByIcuId(int icuId)
        {
            var device = _dbContext.IcuDevice.FirstOrDefault(c =>
               c.Id == icuId && !c.IsDeleted);

            if (device != null && device.Company == null && device.CompanyId != null)
            {
                device.Company = _dbContext.Company.FirstOrDefault(m => m.Id == device.CompanyId && !m.IsDeleted);
            }

            return device;

            //return _dbContext.IcuDevice.Include(c => c.Company).FirstOrDefault(c =>
            //    c.Id == icuId &&
            //    !c.Company.IsDeleted && !c.IsDeleted);
        }

        /// <summary>
        /// Get by icu id
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IcuDevice GetByIdAndCompanyId(int companyId, int id)
        {
            return _dbContext.IcuDevice.Include(c => c.Company).FirstOrDefault(c =>
                c.Id == id && c.CompanyId == companyId &&
                !c.Company.IsDeleted && !c.IsDeleted);
        }

        public IcuDevice GetByIdAndCompanyIdWithBuilding(int companyId, int id)
        {
            return _dbContext.IcuDevice
                .Include(c => c.Company)
                .Include(c => c.Building)
                .FirstOrDefault(c => c.Id == id && c.CompanyId == companyId && c.BuildingId != null && !c.Company.IsDeleted && !c.IsDeleted);
        }

        /// <summary>
        /// Get devices by company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IQueryable<IcuDevice> GetDevicesByCompany(int companyId)
        {
            if (companyId != 0)
            {
                var data = _dbContext.IcuDevice.Include(c => c.Company).Include(c => c.Building).Where(c => !c.Company.IsDeleted && !c.IsDeleted && c.CompanyId == companyId);
                return data;
            }
            else
            {
                var data = _dbContext.IcuDevice.Include(c => c.Building).Where(c => !c.IsDeleted);
                return data;
            }
        }


        /// <summary>
        /// Get device information that include all info for making user protocol data.
        /// </summary>
        /// <param name="companyId"> oompany Identifier </param>
        /// <returns></returns>
        public IQueryable<IcuDevice> GetDeviceAllInfoByCompany(int companyId)
        {
            var data = _dbContext.IcuDevice
                    .Include(m => m.AccessGroupDevice).ThenInclude(n => n.Tz)
                    .Include(m => m.AccessGroupDevice).ThenInclude(n => n.AccessGroup)
                    .Include(m => m.Building)
                    .Include(m => m.Company)
                    .Where(m => !m.IsDeleted && !m.Building.IsDeleted);

            if(companyId != 0)
            {
                data = data.Where(m => m.CompanyId == companyId);
            }

            return data;
        }

        /// <summary>
        /// Get device information that include all info for making user protocol data.
        /// </summary>
        /// <param name="companyId"> oompany Identifier </param>
        /// <returns></returns>
        public IEnumerable<IcuDevice> GetDeviceAllInfoByCompanyE(int companyId)
        {
            //var data = _dbContext.IcuDevice
            //        .Include(m => m.AccessGroupDevice).ThenInclude(n => n.Tz)
            //        .Include(m => m.AccessGroupDevice).ThenInclude(n => n.AccessGroup.User).ThenInclude(o => o.Card)
            //        .Include(m => m.AccessGroupDevice).ThenInclude(n => n.AccessGroup.User).ThenInclude(o => o.Department)
            //        .Include(m => m.AccessGroupDevice).ThenInclude(n => n.AccessGroup.Visit).ThenInclude(o => o.Card)
            //        .Where(m => !m.IsDeleted && !m.Building.IsDeleted).AsEnumerable();

            var data = _dbContext.IcuDevice
                    .Include(m => m.AccessGroupDevice).ThenInclude(n => n.Tz)
                    .Include(m => m.AccessGroupDevice).ThenInclude(n => n.AccessGroup.User).ThenInclude(o => o.Card)
                    .Include(m => m.AccessGroupDevice).ThenInclude(n => n.AccessGroup.User).ThenInclude(o => o.Department)
                    .Include(m => m.AccessGroupDevice).ThenInclude(n => n.AccessGroup.Visit).ThenInclude(o => o.Card)
                    .Where(m => !m.IsDeleted).AsEnumerable();

            if (companyId != 0)
            {
                data = data.Where(m => m.CompanyId == companyId).AsEnumerable();
            }

            return data;
        }

        public IQueryable<IcuDevice> GetDeviceIncludeBuildingAndAGDForUser(int companyId, int userId)
        {
            var data = _dbContext.IcuDevice
                .Include(m => m.AccessGroupDevice).ThenInclude(n => n.Tz)
                .Include(m => m.Company)
                .Where(m => !m.IsDeleted && !m.Building.IsDeleted)
                .Where(m => m.CompanyId.Value == companyId);

            return data;
        }

        public IQueryable<IcuDevice> GetUnAssignDevicesByDepartment(int companyId, int accessGroupId, List<int> departmentIds)
        {
            var listDeviceInepartment = _dbContext.DepartmentDevice.Where(x => departmentIds.Contains(x.DepartmentId))
                .Select(x => x.IcuId).ToList();
            return _dbContext.IcuDevice.Include(c => c.Building)
                .Include(c => c.ActiveTz)
                .Include(x => x.Company)
                .Where(c => c.CompanyId == companyId && c.Status == (short)Status.Valid &&
                c.AccessGroupDevice.All(x => x.AccessGroupId != accessGroupId) &&
                listDeviceInepartment.Contains(c.Id) && !c.Company.IsDeleted && !c.IsDeleted);
        }
        public IQueryable<IcuDevice> GetAssignDevicesByDepartment(int companyId, int accessGroupId, List<int> departmentIds)
        {
            var listDeviceInepartment = _dbContext.DepartmentDevice.Where(x => departmentIds.Contains(x.DepartmentId))
                .Select(x => x.IcuId).ToList();
            return _dbContext.IcuDevice.Include(c => c.Building)
                .Include(c => c.ActiveTz)
                .Include(x => x.Company)
                .Where(c => c.CompanyId == companyId && c.Status == (short)Status.Valid &&
                c.AccessGroupDevice.Any(x => x.AccessGroupId == accessGroupId) &&
                listDeviceInepartment.Contains(c.Id) && !c.Company.IsDeleted && !c.IsDeleted);
        }

        /// <summary>
        /// Get list assign doors
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        public IQueryable<IcuDevice> GetDoorsByBuildingId(int companyId, int buildingId, short operationType)
        {
            List<short> operationTypes = new List<short>();
            operationTypes.Add(operationType);

            if(operationType == (short)OperationType.Entrance)
            {
                operationTypes.Add((short)OperationType.FireDetector);
                operationTypes.Add((short)OperationType.Reception);
            }

            //var data = _dbContext.IcuDevice.Include(c => c.ActiveTz).Include(c => c.PassageTz).Where(c =>
            //    c.BuildingId == buildingId && !c.Company.IsDeleted && !c.IsDeleted && c.OperationType == operationType);

            var data = _dbContext.IcuDevice.Include(c => c.ActiveTz).Include(c => c.PassageTz).Where(c =>
                c.BuildingId == buildingId && !c.Company.IsDeleted && !c.IsDeleted && operationTypes.Contains(c.OperationType));

            if (companyId != 0)
            {
                data = data.Where(c => c.CompanyId == companyId);
            }

            return data;
        }

        /// <summary>
        /// Get list assign doors
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        public IQueryable<IcuDevice> GetUnAssignDoorsByBuildingId(int companyId, int buildingId, short operationType)
        {
            List<short> operationTypes = new List<short>();
            operationTypes.Add(operationType);

            if (operationType == (short)OperationType.Entrance)
            {
                operationTypes.Add((short)OperationType.FireDetector);
                operationTypes.Add((short)OperationType.Reception);
            }

            //return _dbContext.IcuDevice.Include(c => c.Building).Where(c =>
            //    c.CompanyId == companyId && c.BuildingId != buildingId && c.OperationType==operationType &&
            //    !c.Company.IsDeleted && !c.IsDeleted);

            return _dbContext.IcuDevice.Include(c => c.Building).Where(c =>
                c.CompanyId == companyId && c.BuildingId != buildingId && operationTypes.Contains(c.OperationType) &&
                !c.Company.IsDeleted && !c.IsDeleted);
        }

        /// <summary>
        /// Get active devices by company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IQueryable<IcuDevice> GetActiveDevicesByCompany(int companyId)
        {
            return _dbContext.IcuDevice.Include(c => c.Company).Include(c => c.AccessGroupDevice)
                .ThenInclude(c => c.Tz).Where(c =>
                    c.CompanyId == companyId && c.Status == (short)Status.Valid &&
                    !c.Company.IsDeleted && !c.IsDeleted);
        }
        
        public List<IcuDevice> GetNotDeletedDevices()
        {
            var devices = _dbContext.IcuDevice.Include(c => c.Company)
                .Include(c => c.Building)
                .Where(c => !c.IsDeleted && (c.DeviceType != (short)DeviceType.DesktopApp) && c.CompanyId != null);

            return devices.ToList();
        }

        /// <summary>
        /// Get un-assigned devices by company
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="accessGroupId"></param>
        /// <returns></returns>
        public IQueryable<IcuDevice> GetUnAssignDevicesByCompany(int companyId, int accessGroupId)
        {
            return _dbContext.IcuDevice.Include(c => c.Building).Include(c => c.ActiveTz).Where(c =>
                c.CompanyId == companyId && c.Status == (short)Status.Valid &&
                c.AccessGroupDevice.All(x => x.AccessGroupId != accessGroupId) &&
                !c.Company.IsDeleted && !c.IsDeleted);
        }
        /// <summary>
        /// Get assigned devices by company
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="accessGroupId"></param>
        /// <returns></returns>
        public IQueryable<IcuDevice> GetAssignDevicesByCompany(int companyId, int accessGroupId)
        {
            return _dbContext.IcuDevice.Include(c => c.Building).Include(c => c.ActiveTz).Where(c =>
                c.CompanyId == companyId && c.Status == (short)Status.Valid &&
                c.AccessGroupDevice.Any(x => x.AccessGroupId == accessGroupId) &&
                !c.Company.IsDeleted && !c.IsDeleted);
        }

        ///// <summary>
        ///// Get all devices, including flags to distinguish whether devices are assigned to AG
        ///// </summary>
        ///// <param name="companyId"></param>
        ///// <param name="accessGroupId"></param>
        ///// <returns></returns>
        //public IQueryable<IcuDevice> GetAllDevicesWithAssignFlag(int companyId, int accessGroupId)
        //{
        //    return _dbContext.IcuDevice.Include(c => c.Building).Include(c => c.ActiveTz).Where(c =>
        //        c.CompanyId == companyId && c.Status == (short)Status.Valid &&
        //        c.AccessGroupDevice.All(x => x.AccessGroupId != accessGroupId) &&
        //        !c.Company.IsDeleted && !c.IsDeleted);
        //}

        /// <summary>
        /// Get devices by access group
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="accessGroupId"></param>
        /// <returns></returns>
        public IQueryable<IcuDevice> GetDevicesByAccessGroup(int companyId, int accessGroupId)
        {
            return _dbContext.IcuDevice.Include(c => c.Company).Include(c => c.AccessGroupDevice)
                .ThenInclude(c => c.Tz).Include(c => c.PassageTz).Where(c =>
                c.CompanyId == companyId && c.AccessGroupDevice.Any(x => x.AccessGroupId == accessGroupId) &&
                c.Status == (short)Status.Valid && !c.Company.IsDeleted && !c.IsDeleted);
        }

        /// <summary>
        /// Check if door is assigned to a timezone
        /// </summary>
        /// <param name="timezoneId"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public bool HasTimezone(int timezoneId, int companyId)
        {
            return _dbContext.IcuDevice.Any(c =>
                c.CompanyId == companyId && (c.ActiveTzId == timezoneId || c.PassageTzId == timezoneId)
                                         && !c.IsDeleted);
        }

        /// <summary>
        /// Get all door by timezone id
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="timezoneId"></param>
        /// <returns></returns>
        public List<IcuDevice> GetByTimezoneId(int companyId, int timezoneId)
        {
            return _dbContext.IcuDevice.Where(x =>
                    x.ActiveTzId == timezoneId || x.PassageTzId == timezoneId && x.CompanyId == companyId && !x.IsDeleted)
                .ToList();
        }

        /// <summary>
        /// Get valid doors by company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<IcuDevice> GetValidDoorsByCompany(int companyId)
        {
            return _dbContext.IcuDevice.Where(c => c.CompanyId == companyId && c.Status == (short)Status.Valid && !c.IsDeleted).ToList();
        }

        /// <summary>
        /// Get list door by company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public IEnumerable<IcuDevice> GetDoors(int? companyId = null)
        {
            var doors = _dbContext.IcuDevice.Include(c => c.Company)
                .Where(c => !c.Company.IsDeleted && !c.IsDeleted).AsQueryable();
            if (companyId != null && companyId.Value != 0)
            {
                doors = doors.Where(c => c.CompanyId == companyId.Value);
            }

            return doors.AsEnumerable().DistinctBy(c => c.Id);
        }

        public List<IcuDevice> GetByCompany(int companyId)
        {
            var icuDevices = _dbContext.IcuDevice.Include(c => c.Company).Include(c => c.Building)
                .Where(m => m.CompanyId == companyId &&
                            !m.IsDeleted && !m.Company.IsDeleted).ToList();
            return icuDevices;
        }

        public IcuDevice GetDeviceByRid(int companyId, string rid)
        {
            var icuDevices = _dbContext.IcuDevice.Include(c => c.Company).Include(m => m.Building)
                .Single(m => m.CompanyId == companyId && m.DeviceAddress == rid && !m.IsDeleted && !m.Company.IsDeleted);
            return icuDevices;
        }

        public List<EventLog> GetRecentEventLogData(int deviceId)
        {
            var device = GetByIcuId(deviceId);
            List<EventLog> eventLogData = new List<EventLog>();
            eventLogData = (from c in _dbContext.EventLog
                            where c.EventTime <= DateTime.Now
                            && c.EventTime >= DateTime.Now.AddDays(-1)
                            && c.CompanyId == device.CompanyId
                            && c.IcuId == deviceId
                            && (c.EventType == (short)EventType.CommunicationSucceed
                            || c.EventType == (short)EventType.CommunicationFailed)
                            orderby c.EventTime
                            select new EventLog
                            {
                                EventTime = c.EventTime,
                                EventType = c.EventType
                            }).ToList();
            return eventLogData;
        }

        public List<EventLog> GetEventLogData(int deviceId)
        {
            var device = GetByIcuId(deviceId);

            var companyId = device.CompanyId;
            List<EventLog> eventLogData;
            if (string.IsNullOrEmpty(device.CreateTimeOnlineDevice))
            {

                eventLogData = (from c in _dbContext.EventLog
                                where c.EventTime <= DateTime.Now
                                && c.CompanyId == companyId
                                && c.IcuId == deviceId
                                && (c.EventType == (short)EventType.CommunicationSucceed
                                || c.EventType == (short)EventType.CommunicationFailed)
                                select new EventLog
                                {
                                    EventTime = c.EventTime,
                                    EventType = c.EventType
                                }).ToList();

            }
            else
            {
                var timeOnlineDevice = Convert.ToDateTime(device.CreateTimeOnlineDevice);
                eventLogData = (from c in _dbContext.EventLog
                                where c.EventTime > timeOnlineDevice
                                && c.EventTime <= DateTime.Now
                                && c.CompanyId == companyId
                                && c.IcuId == deviceId
                                && (c.EventType == (short)EventType.CommunicationSucceed
                                || c.EventType == (short)EventType.CommunicationFailed)
                                select new EventLog
                                {
                                    EventTime = c.EventTime,
                                    EventType = c.EventType
                                }).ToList();
            }
            return eventLogData;
        }
        public void UpdateDevice(IcuDevice IcuDevice)
        {
            try
            {
                _dbContext.IcuDevice.Update(IcuDevice);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateDevice");
            }
        }

        public void ReUpdateUpTimOnlineDevice()
        {
            var listIcuDevice = _dbContext.IcuDevice.Where(x => x.IsDeleted == false).ToList();
            foreach (var item in listIcuDevice)
            {
                item.UpTimeOnlineDevice = 0;
                item.CreateTimeOnlineDevice = "";
                try
                {
                    _dbContext.IcuDevice.Update(item);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ReUpdateUpTimOnlineDevice");
                }

            }
            try
            {
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReUpdateUpTimOnlineDevice");
            }
        }
        public void ReUpdateUpTimOnlineDeviceById(int id)
        {
            var icuDevice = _dbContext.IcuDevice.Where(x => x.IsDeleted == false && x.Id == id).FirstOrDefault();
            if (icuDevice != null)
            {
                icuDevice.UpTimeOnlineDevice = 0;
                try
                {
                    _dbContext.IcuDevice.Update(icuDevice);
                    _dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in ReUpdateUpTimOnlineDeviceById");
                }
            }

        }


        /// <summary>
        /// Get device list as tree structure.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Node> GetAGDeviceHierarchy(List<IcuDevice> devices, int accessGroupId)
        {
            var buildingIds = devices.Where(m => m.BuildingId != null).Select(m => m.BuildingId.Value).ToList();

            var buildings = _dbContext.Building.Where(m => buildingIds.Contains(m.Id)).ToList();

            var nodeItems = buildings.Select(
                m => new Node
                {
                    Id = m.Id,
                    BuildingName = m.Name,
                    Devices = devices.Where(c => c.BuildingId == m.Id)
                            .Select(c => new SimpleData() { Id = c.Id,
                                                            Name = c.Name,
                                                            DeviceAddress = c.DeviceAddress,
                                                            ActiveTZId = c.ActiveTzId.Value,
                                                            IsAssigned = c.AccessGroupDevice.Select(x => x.AccessGroupId).Contains(accessGroupId) ? true : false }).ToList(),
                });

            try
            {
                var nodes = nodeItems.BuildTree();
                return nodes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
                return null;
            }
        }

        public List<RecentlyDisconnectedDeviceModel> getRecentlyDisconnectedDevices()
        {

            List<RecentlyDisconnectedDeviceModel> devices = (from a in _dbContext.IcuDevice
                                              join b in _dbContext.EventLog on a.Id equals b.IcuId
                                              where  !b.CameraId.HasValue
                                                  && b.EventTime <= DateTime.UtcNow
                                                  && b.EventTime >= DateTime.UtcNow.AddDays(-1)
                                                  && b.EventType == (short)EventType.CommunicationFailed
                                                  && a.IsDeleted == false

                                              select new RecentlyDisconnectedDeviceModel
                                              {
                                                  Id = a.Id,
                                                  ConnectionStatus = a.ConnectionStatus,
                                                  DeviceAddress = a.DeviceAddress,
                                                  Company = a.Company.Name,
                                                  DoorName = a.Name,
                                              }
                                       ).Distinct().ToList();

            return devices;
        }

        public IQueryable<IcuDevice> GetDevicesByBuildingIds(List<int> buildingIds, int companyId)
        {
            return _dbContext.IcuDevice.Where(m => !m.IsDeleted && m.CompanyId == companyId && buildingIds.Contains(m.BuildingId.Value));
        }

        public List<IcuDevice> GetDevicesByUserId(int userId)
        {
            var user = _dbContext.User.FirstOrDefault(m => m.Id == userId);
            if (user != null && user.AccessGroupId != 0)
            {
                var accessGroupDeviceIds = _dbContext.AccessGroupDevice.Where(m => m.AccessGroupId == user.AccessGroupId)
                    .Select(m => m.IcuId);

                return _dbContext.IcuDevice.Where(m => accessGroupDeviceIds.Contains(m.Id) && !m.IsDeleted).ToList();
            }

            return new List<IcuDevice>();
        }
    }
}
