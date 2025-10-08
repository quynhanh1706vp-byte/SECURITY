using System;
using System.Threading.Tasks;
using DeMasterProCloud.DataAccess.Models;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// Unit of work interface
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        ISystemInfoRepository SystemInfoRepository { get; }
        IUserRepository UserRepository { get; }
        IAccountRepository AccountRepository { get; }
        IDepartmentRepository DepartmentRepository { get; }
        IIcuDeviceRepository IcuDeviceRepository { get; }
        IAccessTimeRepository AccessTimeRepository { get; }
        IHolidayRepository HolidayRepository { get; }
        ICompanyRepository CompanyRepository { get; }
        ISystemLogRepository SystemLogRepository { get; }
        IEventRepository EventRepository { get; }
        IEventLogRepository EventLogRepository { get; }
        ISettingRepository SettingRepository { get; }
        IAccessGroupRepository AccessGroupRepository { get; }
        IAccessGroupDeviceRepository AccessGroupDeviceRepository { get; }
        IBuildingRepository BuildingRepository { get; }
        IUnregistedDeviceRepository UnregistedDevicesRepository { get; }
        IDeviceReaderRepository DeviceReaderRepository { get; }
        

        IVisitRepository VisitRepository { get; }
        ICardRepository CardRepository { get; }
        
        IWorkingRepository WorkingRepository { get; }
        
        IAttendanceRepository AttendanceRepository { get; }
        IPlugInRepository PlugInRepository { get; }

        IRoleRepository RoleRepository { get; }
        
        INotificationRepository NotificationRepository { get; }
        ICompanyAccountRepository CompanyAccountRepository { get; }
        IAttendanceLeaveRepository AttendanceLeaveRepository { get; }
        ICameraRepository CameraRepository { get; }
        IVehicleRepository VehicleRepository { get; }
        IShortenLinkRepository ShortenLinkRepository { get; }


        IHeaderRepository HeaderRepository { get; }
        IFirmwareVersionRepository FirmwareVersionRepository { get; }

        IDataListSettingRepository DataListSettingRepository { get; }
        IDepartmentDeviceRepository DepartmentDeviceRepository { get; }

        IAccessScheduleRepository AccessScheduleRepository { get; }
        IWorkShiftRepository WorkShiftRepository { get; }
        IUserAccessScheduleRepository UserAccessScheduleRepository { get; }
        IAccessWorkShiftRepository AccessWorkShiftRepository { get; }
        

        AppDbContext AppDbContext { get; }
        void Save();

        void Clear();
        Task SaveAsync();
    }
}