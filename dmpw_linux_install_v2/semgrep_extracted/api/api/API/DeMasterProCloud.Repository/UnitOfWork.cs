using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using DeMasterProCloud.DataAccess.Models;
using Microsoft.Extensions.Options;
using DeMasterProCloud.DataModel.Api;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DeMasterProCloud.Common.Infrastructure;

namespace DeMasterProCloud.Repository
{
    /// <inheritdoc />
    /// <summary>
    /// Unit of work class
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        #region inject field variables
        private readonly AppDbContext _appDbContext;
        private readonly ILogger _logger;
        #endregion

        #region data members
        private readonly IHttpContextAccessor _httpContext;
        private ISystemInfoRepository _systemInfoRepository;
        private IUserRepository _userRepository;
        private IAccountRepository _accountRepository;
        private IDepartmentRepository _departmentRepository;
        private IIcuDeviceRepository _icuDeviceRepository;
        private IAccessTimeRepository _accessTimeRepository;
        private IHolidayRepository _holidayRepository;
        private ICompanyRepository _companyRepository;
        private ISystemLogRepository _systemLogRepository;
        private IEventRepository _eventRepository;
        private IEventLogRepository _eventLogRepository;
        private ISettingRepository _settingRepository;
        private IAccessGroupRepository _accessGroupRepository;
        private IAccessGroupDeviceRepository _accessGroupDeviceRepository;
        private IUnregistedDeviceRepository _unregistedDevicesRepository;
        private IBuildingRepository _buildingRepository;
        private IVisitRepository _visitRepository;
        private ICardRepository _cardRepository;
        private IWorkingRepository _workingRepository;
        private IAttendanceRepository _attendanceRepository;
        private IPlugInRepository _plugInRepository;
        private IRoleRepository _roleRepository;

        private INotificationRepository _notificationRepository;
        private ICompanyAccountRepository _companyAccountRepository;
        private IAttendanceLeaveRepository _attendanceLeaveRepository;
        private ICameraRepository _cameraRepository;
        private IVehicleRepository _vehicleRepository;
        private IShortenLinkRepository _shortenLinkRepository;
        private IHeaderRepository _headerRepository;
        private IFirmwareVersionRepository _firmwareVersionRepository;
        private IDataListSettingRepository _dataListSettingRepository;
        private IDepartmentDeviceRepository _departmentDeviceRepository;
        private IDeviceReaderRepository _deviceReaderRepository;
        
        private IAccessScheduleRepository _accessScheduleRepository;
        private IWorkShiftRepository _workShiftRepository;
        private IUserAccessScheduleRepository _userAccessScheduleRepository;
        private IAccessWorkShiftRepository _accessWorkShiftRepository;
        #endregion

        /// <summary>
        /// Unit of work constructor
        /// </summary>
        /// <param name="appDbContext"></param>
        /// <param name="contextAccessor"></param>
        public UnitOfWork(AppDbContext appDbContext, IHttpContextAccessor contextAccessor)
        {
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<UnitOfWork>();
            _appDbContext = appDbContext;
            _httpContext = contextAccessor;
        }

        public UnitOfWork(AppDbContext appDbContext)
        {
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<UnitOfWork>();
            _appDbContext = appDbContext;
        }

        #region Properties
        /// <summary>
        /// Get AppDbContext
        /// </summary>
        public AppDbContext AppDbContext => _appDbContext;

        #endregion
        

        #region Methods

        /// <summary>
        /// Get SystemInfoRepository
        /// </summary>
        public ISystemInfoRepository SystemInfoRepository
        {
            get
            {
                return _systemInfoRepository = _systemInfoRepository ?? new SystemInfoRepository(_appDbContext, _httpContext);
            }
        }
        
        /// <summary>
        /// Get UserRepository
        /// </summary>
        public IUserRepository UserRepository
        {
            get
            {
                return _userRepository =
                    _userRepository ?? new UserRepository(_appDbContext, _httpContext);
            }
        }

        /// <summary>
        /// Get UserLoginRepository
        /// </summary>
        public IAccountRepository AccountRepository
        {
            get
            {
                return _accountRepository = _accountRepository ??
                                              new AccountRepository(
                                                  _appDbContext, _httpContext);
            }
        }

        public IDepartmentRepository DepartmentRepository
        {
            get
            {
                return _departmentRepository =
                    _departmentRepository ?? new DepartmentRepository(_appDbContext, _httpContext);
            }
        }

        public IIcuDeviceRepository IcuDeviceRepository
        {
            get
            {
                return _icuDeviceRepository =
                    _icuDeviceRepository ?? new IcuDeviceRepository(_appDbContext, _httpContext);
            }
        }

        public IUnregistedDeviceRepository UnregistedDevicesRepository
        {
            get
            {
                return _unregistedDevicesRepository =
                    _unregistedDevicesRepository ?? new UnregistedDeviceRepository(_appDbContext, _httpContext);
            }
        }

        public IAccessTimeRepository AccessTimeRepository
        {
            get
            {
                return _accessTimeRepository =
                    _accessTimeRepository ?? new AccessTimeRepository(_appDbContext, _httpContext);
            }
        }

        public IHolidayRepository HolidayRepository
        {
            get
            {
                return _holidayRepository =
                    _holidayRepository ?? new HolidayRepository(_appDbContext, _httpContext);
            }
        }

        public ICompanyRepository CompanyRepository
        {
            get
            {
                return _companyRepository =
                    _companyRepository ?? new CompanyRepository(_appDbContext, _httpContext);
            }
        }

        public ISystemLogRepository SystemLogRepository
        {
            get
            {
                return _systemLogRepository =
                    _systemLogRepository ?? new SystemLogRepository(_appDbContext, _httpContext);
            }
        }

        public IEventRepository EventRepository
        {
            get
            {
                return _eventRepository =
                    _eventRepository ?? new EventRepository(_appDbContext, _httpContext);
            }
        }


        public INotificationRepository NotificationRepository
        {
            get
            {
                return _notificationRepository =
                    _notificationRepository ?? new NotificationRepository(_appDbContext, _httpContext);
            }
        }

        public IEventLogRepository EventLogRepository
        {
            get
            {
                return _eventLogRepository =
                    _eventLogRepository ?? new EventLogRepository(_appDbContext, _httpContext);
            }
        }

        public ISettingRepository SettingRepository
        {
            get
            {
                return _settingRepository =
                    _settingRepository ?? new SettingRepository(_appDbContext, _httpContext);
            }
        }

        public IAccessGroupRepository AccessGroupRepository
        {
            get
            {
                return _accessGroupRepository =
                    _accessGroupRepository ?? new AccessGroupRepository(_appDbContext, _httpContext);
            }
        }

        public IAccessGroupDeviceRepository AccessGroupDeviceRepository
        {
            get
            {
                return _accessGroupDeviceRepository =
                    _accessGroupDeviceRepository ?? new AccessGroupDeviceRepository(_appDbContext, _httpContext);
            }
        }


        public IBuildingRepository BuildingRepository
        {
            get
            {
                return _buildingRepository =
                    _buildingRepository ?? new BuildingRepository(_appDbContext, _httpContext);
            }
        }

        public IVisitRepository VisitRepository
        {
            get
            {
                return _visitRepository =
                    _visitRepository ?? new VisitRepository(_appDbContext, _httpContext);
            }
        }

        public ICardRepository CardRepository
        {
            get
            {
                return _cardRepository =
                    _cardRepository ?? new CardRepository(_appDbContext, _httpContext);
            }
        }

        public IWorkingRepository WorkingRepository
        {
            get
            {
                return _workingRepository =
                    _workingRepository ?? new WorkingRepository(_appDbContext, _httpContext);
            }
        }

        public IAttendanceRepository AttendanceRepository
        {
            get
            {
                return _attendanceRepository = _attendanceRepository ?? new AttendanceRepository(_appDbContext, _httpContext);
            }
        }

        public IPlugInRepository PlugInRepository
        {
            get
            {
                return _plugInRepository = _plugInRepository ?? new PlugInRepository(_appDbContext, _httpContext);
            }
        }

        public IRoleRepository RoleRepository
        {
            get
            {
                return _roleRepository = _roleRepository ?? new RoleRepository(_appDbContext, _httpContext);
            }
        }
        
        public IVehicleRepository VehicleRepository
        {
            get
            {
                return _vehicleRepository = _vehicleRepository ?? new VehicleRepository(_appDbContext, _httpContext);
            }
        }

        public IShortenLinkRepository ShortenLinkRepository
        {
            get
            {
                return _shortenLinkRepository = _shortenLinkRepository ?? new ShortenLinkRepository(_appDbContext, _httpContext);
            }
        }

        public IHeaderRepository HeaderRepository
        {
            get
            {
                return _headerRepository = _headerRepository ?? new HeaderRepository(_appDbContext, _httpContext);
            }
        }

        public ICompanyAccountRepository CompanyAccountRepository
        {
            get
            {
                return _companyAccountRepository = _companyAccountRepository ?? new CompanyAccountRepository(_appDbContext, _httpContext);
            }
        }
        
        public IAttendanceLeaveRepository AttendanceLeaveRepository
        {
            get
            {
                return _attendanceLeaveRepository = _attendanceLeaveRepository ?? new AttendanceLeaveRepository(_appDbContext, _httpContext);
            }
        }
        
        public ICameraRepository CameraRepository
        {
            get
            {
                return _cameraRepository = _cameraRepository ?? new CameraRepository(_appDbContext, _httpContext);
            }
        }

        public IFirmwareVersionRepository FirmwareVersionRepository
        {
            get
            {
                return _firmwareVersionRepository = _firmwareVersionRepository ?? new FirmwareVersionRepository(_appDbContext, _httpContext);
            }
        }

        public IDataListSettingRepository DataListSettingRepository
        {
            get
            {
                return _dataListSettingRepository = _dataListSettingRepository ?? new DataListSettingRepository(_appDbContext, _httpContext);
            }
        }

        public IDepartmentDeviceRepository DepartmentDeviceRepository
        {
            get
            {
                return _departmentDeviceRepository = _departmentDeviceRepository ?? new DepartmentDeviceRepository(_appDbContext, _httpContext);
            }
        }
        public IDeviceReaderRepository DeviceReaderRepository
        {
            get
            {
                return _deviceReaderRepository = _deviceReaderRepository ?? new DeviceReaderRepository(_appDbContext, _httpContext);
            }
        }

        public IAccessScheduleRepository AccessScheduleRepository
        {
            get
            {
                return _accessScheduleRepository = _accessScheduleRepository ?? new AccessScheduleRepository(_appDbContext, _httpContext);
            }
        }
        public IWorkShiftRepository WorkShiftRepository
        {
            get
            {
                return _workShiftRepository = _workShiftRepository ?? new WorkShiftRepository(_appDbContext, _httpContext);
            }
        }
        public IUserAccessScheduleRepository UserAccessScheduleRepository
        {
            get
            {
                return _userAccessScheduleRepository = _userAccessScheduleRepository ?? new UserAccessScheduleRepository(_appDbContext, _httpContext);
            }
        }
        public IAccessWorkShiftRepository AccessWorkShiftRepository
        {
            get
            {
                return _accessWorkShiftRepository = _accessWorkShiftRepository ?? new AccessWorkShiftRepository(_appDbContext, _httpContext);
            }
        }

        /// <summary>
        /// Save
        /// </summary>
        public void Save()
        {
            try
            {
                _appDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Save");
            }
        }
        /// <summary>
        /// Save Async
        /// </summary>
        public async Task SaveAsync()
        {
            try
            {
                await _appDbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SaveAsync");
            }
        }

        /// <summary>
        /// Clear unitOfWork changes.
        /// </summary>
        public void Clear()
        {
            var changedEntries = _appDbContext.ChangeTracker.Entries()
                    .Where(e => e.State == EntityState.Added ||
                                e.State == EntityState.Modified ||
                                e.State == EntityState.Deleted).ToList();

            foreach (var entry in changedEntries)
                entry.State = EntityState.Detached;
        }
        #endregion

        #region dispose
        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _appDbContext.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}