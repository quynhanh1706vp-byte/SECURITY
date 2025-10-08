using System;
using System.Collections.Generic;
using System.IO;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using DeMasterProCloud.Repository;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.Service.RabbitMqQueue;
using Constants = DeMasterProCloud.Common.Infrastructure.Constants;

namespace DeMasterProCloud.Service.Infrastructure
{
    public class MyRegistry : Registry
    {
        public MyRegistry(IConfiguration configuration)
        {
            // schedule every day
            // 00h00m
            Schedule(() => new UpdateUptimePerADay(configuration)).ToRunEvery(1).Days().At(0, 0);
            // 22h00m
            Schedule(() => new AutoDeleteNotification(configuration)).ToRunEvery(1).Days().At(22, 0);

            // 23h30m
            Schedule(() => new CheckLimitStoredFileMedia(configuration)).ToRunEvery(1).Days().At(23, 30);

            // schedule every time
            // 60 seconds/time
            bool disableRecheckAttendanceCheckin = configuration.GetSection("Cronjob:DisableRecheckAttendanceCheckin").Get<bool>();
            if (!disableRecheckAttendanceCheckin)
            {
                Schedule(() => new RecheckAttendanceCheckin(configuration)).ToRunNow().AndEvery(60).Seconds();
            }

            // 120 seconds/time
            bool disableSendDeviceCommonInstruction = configuration.GetSection("Cronjob:DisableSendDeviceCommonInstruction").Get<bool>();
            if (!disableSendDeviceCommonInstruction)
            {
                Schedule(() => new SendDeviceCommonInstruction(configuration)).ToRunNow().AndEvery(120).Seconds();
            }
            // 2 minutes/time
            bool disableCheckAccessScheduleSendToDevice = configuration.GetSection("Cronjob:DisableCheckAccessScheduleSendToDevice").Get<bool>();
            if (!disableCheckAccessScheduleSendToDevice)
            {
                Schedule(() => new CheckAccessScheduleSendToDevice(configuration)).ToRunNow().AndEvery(Constants.Settings.DefaultCheckAccessScheduleCronjob).Minutes();
            }
            
            // 1 minute/time
            Schedule(() => new ResetLoginFailCount(configuration)).ToRunNow().AndEvery(60).Seconds();
        }

        public class RecheckAttendanceCheckin : IJob
        {
            private IUnitOfWork _unitOfWork;
            private readonly IConfiguration _configuration;
            private readonly ILogger<RecheckAttendanceCheckin> _logger;

            public RecheckAttendanceCheckin(IConfiguration configuration)
            {
                _configuration = configuration;
                _logger = ApplicationVariables.LoggerFactory.CreateLogger<RecheckAttendanceCheckin>();
            }

            public void Execute()
            {
                _unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                DateTime start = DateTime.UtcNow;
                var companies = _unitOfWork.CompanyRepository.GetCompaniesByPlugin(Constants.PlugIn.TimeAttendance);
                IAttendanceService attendanceService = new AttendanceService(new HttpContextAccessor(), _configuration);
                foreach (var company in companies)
                {
                    try
                    {
                        if (company.TimeRecheckAttendance != 0)
                        {
                            if ((start.Hour * 60 + start.Minute) % company.TimeRecheckAttendance == 0)
                            {
                                var startString = start.Date.ToString("yyyy-MM-dd");
                                var lastString = start.Date.AddDays(1).ToString("yyyy-MM-dd");
                                attendanceService.Recheck(Constants.Attendance.RangTimeToDay, company.Id, startString, lastString);
                            }
                        }
                        else
                        {
                            // from 20h00 to 20h02: recheck attendance for all companies not checkin real time
                            if (start.Hour == 20 && 0 <= start.Minute && start.Minute <= 2)
                            {
                                var startString = start.Date.ToString("yyyy-MM-dd");
                                var lastString = start.Date.AddDays(1).ToString("yyyy-MM-dd");
                                attendanceService.Recheck(Constants.Attendance.RangTimeToDay, company.Id, startString, lastString);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message + ex.StackTrace);
                        _logger.LogError($"Company: {company.Id} - {company.Name}");
                    }
                }
                
                _unitOfWork.Dispose();
            }
        }

        public class AutoDeleteNotification : IJob
        {
            private readonly IConfiguration _configuration;
            private IUnitOfWork _unitOfWork;
            private readonly ILogger<AutoDeleteNotification> _logger;

            public AutoDeleteNotification(IConfiguration configuration)
            {
                _configuration = configuration;
                _logger = ApplicationVariables.LoggerFactory.CreateLogger<AutoDeleteNotification>();
            }

            public void Execute()
            {
                _unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                
                try
                {
                    DateTime dayStored = DateTime.Today.AddDays(-Constants.Settings.LimitDayStoredNotification);

                    while (true)
                    {
                        var notifications = _unitOfWork.NotificationRepository.Gets(m => m.CreatedOn < dayStored && m.ReceiveId != 0)
                            .Skip(0).Take(Constants.DefaultPaggingQuery);
                        if (!notifications.Any())
                        {
                            break;
                        }

                        _unitOfWork.NotificationRepository.DeleteRange(notifications);
                        _unitOfWork.Save();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
                finally
                {
                    _unitOfWork.Dispose();
                }
            }
        }

        public class UpdateUptimePerADay : IJob
        {
            private readonly IConfiguration _configuration;
            private IUnitOfWork _unitOfWork;
            private readonly ILogger<UpdateUptimePerADay> _logger;

            public UpdateUptimePerADay(IConfiguration configuration)
            {
                _configuration = configuration;
                _logger = ApplicationVariables.LoggerFactory.CreateLogger<UpdateUptimePerADay>();
            }

            public void Execute()
            {
                _unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                try
                {
                    IDeviceService deviceService = new DeviceService(_unitOfWork);
                    int count = _unitOfWork.IcuDeviceRepository.Count(m => !m.IsDeleted && m.DeviceType != (short)DeviceType.DesktopApp && m.CompanyId.HasValue);
                    int pageSize = Constants.DefaultPaggingQuery;
                    int totalPage = count / pageSize;
                    totalPage = totalPage * pageSize < count ? totalPage + 1 : totalPage;
                    for (int pageNumber = 1; pageNumber <= totalPage; pageNumber++)
                    {
                        if (pageNumber == totalPage)
                        {
                            pageSize = count - (pageNumber - 1) * pageSize;
                        }
                        
                        List<IcuDevice> devices = _unitOfWork.IcuDeviceRepository
                            .Gets(m => !m.IsDeleted && m.DeviceType != (short)DeviceType.DesktopApp && m.CompanyId.HasValue)
                            .OrderBy(m => m.Id)
                            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                        
                        foreach (var device in devices)
                        {
                            deviceService.UpdateUpTimeToDevice(device.Id);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"[ERROR] Error in Update Uptime cronjob.");
                    Console.WriteLine($"[ERROR] Exception message : {e.Message}");
                    Console.WriteLine($"[ERROR] Inner exception message : {e.InnerException?.Message}");
                }
                finally
                {
                    _unitOfWork.Dispose();
                }
            }
        }
        
        public class CheckLimitStoredFileMedia : IJob
        {
            private readonly IConfiguration _configuration;
            private IUnitOfWork _unitOfWork;
            private readonly ILogger _logger;

            public CheckLimitStoredFileMedia(IConfiguration configuration)
            {
                _configuration = configuration;
                _logger = ApplicationVariables.LoggerFactory.CreateLogger<CheckLimitStoredFileMedia>();
            }

            public void Execute()
            {
                _unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                DateTime now = DateTime.UtcNow;
                var companies = _unitOfWork.CompanyRepository.GetCompanies();
                
                foreach (var company in companies)
                {
                    try
                    {
                        // image event-log
                        string folderImage = $"{Constants.Settings.DefineFolderImages}/{company.Code}";
                        if (Directory.Exists(folderImage))
                        {
                            var directories = Directory.GetDirectories(folderImage);
                            foreach (var directory in directories)
                            {
                                if (DateTime.TryParseExact(directory.Split('/').Last(), Constants.DateTimeFormat.DdMdYyyy, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime value))
                                {
                                    if ((now - value).Days > company.TimeLimitStoredImage)
                                    {
                                        Directory.Delete(directory, true);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Error check limit stored images");
                        _logger.LogError(e.Message + e.StackTrace);
                    }

                    try
                    {
                        // video event-log
                        string folderVideo = $"{Constants.Settings.DefineFolderVideos}/{company.Code}";
                        if (Directory.Exists(folderVideo))
                        {
                            var directories = Directory.GetDirectories(folderVideo);
                            foreach (var directory in directories)
                            {
                                if (DateTime.TryParseExact(directory.Split('/').Last(), Constants.DateTimeFormat.DdMdYyyy, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime value))
                                {
                                    if ((now - value).Days > company.TimeLimitStoredVideo)
                                    {
                                        Directory.Delete(directory, true);
                                    }
                                }
                            }
                        }
                        
                        // record video event-log
                        string folderRecordVideo = $"{Constants.Settings.DefineFolderRecordVideos}/{company.Code}";
                        if (Directory.Exists(folderRecordVideo))
                        {
                            var directories = Directory.GetDirectories(folderRecordVideo);
                            foreach (var directory in directories)
                            {
                                if (DateTime.TryParseExact(directory.Split('/').Last(), Constants.DateTimeFormat.DdMdYyyy, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime value))
                                {
                                    if ((now - value).Days > company.TimeLimitStoredVideo)
                                    {
                                        Directory.Delete(directory, true);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Error check limit stored videos");
                        _logger.LogError(e.Message + e.StackTrace);
                    }

                    try
                    {
                        // file log of device
                        int dayLimit = _configuration.GetSection(Constants.Settings.DefineLimitSaveLogOfDevice).Get<int>();
                        dayLimit = dayLimit > 0 ? dayLimit : 7;
                        DateTime timeLimit = DateTime.UtcNow.AddDays(-dayLimit);
                    
                        var devices = _unitOfWork.IcuDeviceRepository.GetByCompany(company.Id);
                        foreach (var device in devices)
                        {
                            try
                            {
                                string pathFolder = $"{Constants.Settings.DefineFolderDataLogs}/{company.Code}/{device.DeviceAddress}";

                                // Validate that the path doesn't contain path traversal attempts
                                if (pathFolder.Contains("..") || pathFolder.Contains("~"))
                                {
                                    _logger.LogWarning($"Security violation: Path traversal attempt blocked for device {device.DeviceAddress}");
                                    continue;
                                }

                                // Normalize path separators
                                string cleanPath = pathFolder.Replace('\\', '/');

                                // Ensure the path starts with the allowed base directory
                                string baseDirectory = Constants.Settings.DefineFolderDataLogs ?? "data_logs";
                                if (!cleanPath.StartsWith(baseDirectory + "/") && !cleanPath.Equals(baseDirectory))
                                {
                                    _logger.LogWarning($"Security violation: Access denied to path outside data_logs directory for device {device.DeviceAddress}");
                                    continue;
                                }

                                DirectoryInfo directoryInfo = new DirectoryInfo(cleanPath);
                                if (directoryInfo.Exists)
                                {
                                    var fileNeedToDelete = directoryInfo.GetFiles().Where(m => m.CreationTimeUtc < timeLimit);
                                    foreach (var item in fileNeedToDelete)
                                    {
                                        item.Delete();
                                    }
                                }
                            }
                            catch (Exception deviceException)
                            {
                                _logger.LogError($"Error processing device {device.DeviceAddress}: {deviceException.Message}");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Error check limit stored file logs of device");
                        _logger.LogError(e.Message + e.StackTrace);
                    }
                }
                
                _unitOfWork.Dispose();
            }
        }
        
        public class SendDeviceCommonInstruction : IJob
        {
            private readonly IConfiguration _configuration;
            private IUnitOfWork _unitOfWork;
            private readonly ILogger<SendDeviceCommonInstruction> _logger;

            public SendDeviceCommonInstruction(IConfiguration configuration)
            {
                _configuration = configuration;
                _logger = ApplicationVariables.LoggerFactory.CreateLogger<SendDeviceCommonInstruction>();
            }

            public void Execute()
            {
                DateTime start = DateTime.UtcNow;
                int count = 0;
                _unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                AccessGroupService accessGroupService = new AccessGroupService(_unitOfWork, null, _configuration, null, null, null);
                var deviceService = new DeviceService(_unitOfWork, accessGroupService);
                IWebSocketService webSocketService = new WebSocketService();
                var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, webSocketService);

                var companies = _unitOfWork.CompanyRepository.GetAll().Where(m => !m.IsDeleted).ToList();
                foreach (var company in companies)
                {
                    try
                    {
                        count = _unitOfWork.IcuDeviceRepository.Count(m => !m.IsDeleted && m.DeviceType != (short)DeviceType.DesktopApp && m.CompanyId == company.Id/* && m.ConnectionStatus == (short)ConnectionStatus.Online*/);
                        if (count == 0)
                        {
                            continue;
                        }
                        int pageSize = Constants.DefaultPaggingQuery;
                        int totalPage = count / pageSize;
                        totalPage = totalPage * pageSize < count ? totalPage + 1 : totalPage;

                        var devices = _unitOfWork.IcuDeviceRepository
                            .Gets(m => !m.IsDeleted && m.DeviceType != (short)DeviceType.DesktopApp && m.CompanyId == company.Id/* && m.ConnectionStatus == (short)ConnectionStatus.Online*/)
                            .Include(m => m.Company)
                            .Include(m => m.Building)
                            .OrderBy(m => m.Id).ToList();
                        
                        // send device info
                        deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                        {
                            DeviceIds = devices.Where(x => x.DeviceType != (short)DeviceType.EbknReader)
                                .Select(m => m.Id).ToList(),
                            MessageType = Constants.Protocol.LoadDeviceInfo,
                            MsgId = Guid.NewGuid().ToString(),
                            Sender = Constants.RabbitMq.SenderDefault,
                            CompanyCode = company?.Code,
                        });

                        for (int pageNumber = 1; pageNumber <= totalPage; pageNumber++)
                        {
                            if (pageNumber == totalPage)
                            {
                                pageSize = count - (pageNumber - 1) * pageSize;
                            }

                            List<IcuDevice> deviceList = devices.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                            // send set time
                            var groupDevices = deviceList.GroupBy(d => d.Building.TimeZone).ToList();
                            foreach (var deviceBuildingGroup in groupDevices)
                            {
                                deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                                {
                                    DeviceIds = deviceBuildingGroup.Select(m => m.Id).ToList(),
                                    MessageType = Constants.Protocol.SetTime,
                                    MsgId = Guid.NewGuid().ToString(),
                                    Sender = Constants.RabbitMq.SenderDefault,
                                    CompanyCode = company?.Code,
                                });
                            }
                            
                            // update device status disconnect if time update < limit last communication time (default 5 min)
                            var minutesLimit = _configuration.GetSection(Constants.Settings.DefineLimitLastCommunicationTime).Get<int>();
                            if (minutesLimit == 0)
                                minutesLimit = 5;
                            DateTime checkDisconnectTime = DateTime.UtcNow.AddSeconds(-(minutesLimit * 60));
                            var devicesWillSetDisconnect = deviceList.Where(m => m.LastCommunicationTime < checkDisconnectTime && m.ConnectionStatus != (short)ConnectionStatus.Offline).ToList();
                            if (devicesWillSetDisconnect.Any())
                            {
                                Console.WriteLine($"[Cronjob SendDeviceCommonInstruction]: current time: {DateTime.UtcNow.ToString(Constants.DateTimeFormatDefault)}");
                                foreach (var item in devicesWillSetDisconnect)
                                {
                                    Console.WriteLine($"[Cronjob SendDeviceCommonInstruction]: set device offline {item.DeviceAddress} - {item.LastCommunicationTime.ToString(Constants.DateTimeFormatDefault)}");
                                    deviceService.ChangedConnectionStatus(item, (short)ConnectionStatus.Offline);
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        _logger.LogError($"{company.Name} - {company.Code}");
                        _logger.LogError(ex.Message + ex.StackTrace);
                    }
                }

                _unitOfWork.Dispose();
                DateTime end = DateTime.UtcNow;
                Console.WriteLine($"[CRONJOB SEND DEVICE INSTRUCTION COMMON]: devices = {count}, times = {end.Subtract(start).TotalMilliseconds} (ms)");
            }
        }

        public class CheckAccessScheduleSendToDevice : IJob
        {
            private readonly IConfiguration _configuration;
            private IUnitOfWork _unitOfWork;
            private IAccessScheduleService _accessScheduleService;

            private readonly ILogger _logger;

            public CheckAccessScheduleSendToDevice(IConfiguration configuration)
            {
                _configuration = configuration;
                _logger = ApplicationVariables.LoggerFactory.CreateLogger<CheckAccessScheduleSendToDevice>();
            }

            public void Execute()
            {
                _unitOfWork = DbHelper.CreateUnitOfWork(_configuration);
                try
                {
                    IAccessGroupService accessGroupService = new AccessGroupService(_unitOfWork, null, _configuration,
                        ApplicationVariables.LoggerFactory.CreateLogger<AccessGroupService>(), null, null);

                    // device sdk service
                    var deviceSDKService = new DeviceSDKService(_configuration);

                    _accessScheduleService = new AccessScheduleService(_unitOfWork, new HttpContextAccessor(), _configuration, accessGroupService, deviceSDKService);

                    ApplicationVariables.Configuration = _configuration;

                    var companies = _unitOfWork.CompanyRepository.GetAll().Where(m => !m.IsDeleted);
                    foreach (var company in companies)
                    {
                        var accessSchedule = _unitOfWork.AccessScheduleRepository.GetAccessScheduleRunning();
                        if (accessSchedule == null) continue;
                        List<string> ignoredDeviceIds = new List<string>();


                        var dataDoor = JsonConvert.DeserializeObject<List<int>>(accessSchedule.DoorIds);

                        var doorIds = dataDoor;
                        var userIds = _unitOfWork.AccessScheduleRepository.GetUserAssign(accessSchedule.Id).Select(x => x.UserId).ToList();
                        if (doorIds?.Any() == true && userIds.Any())
                        {
                            doorIds.Sort();
                            string compareDoorIds = JsonConvert.SerializeObject(doorIds);

                            if (!ignoredDeviceIds.Contains(compareDoorIds))
                            {
                                // get users to assign
                                var usersToAssign = _unitOfWork.UserRepository.GetByIdsWithNoTracking(company.Id, userIds);

                                // assign users
                                ignoredDeviceIds.Add(compareDoorIds);
                                string timezone = GetTimezoneSendToDevice(doorIds[0]);
                                var offSet = timezone.ToTimeZoneInfo().BaseUtcOffset;
                                // init time user send to device
                                foreach (var user in usersToAssign)
                                {
                                    user.ExpiredDate = accessSchedule.EndTime.ConvertToUserTime(offSet);
                                    user.EffectiveDate = accessSchedule.StartTime.ConvertToUserTime(offSet);
                                }
                                AssignUsers(doorIds, usersToAssign, company.Id);
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Cronjob meeting: " + ex.Message);
                }
                finally
                {
                    _unitOfWork.Dispose();
                }
            }

            private string AssignUsers(List<int> doorIds, List<User> usersToAssign, int companyId)
            {

                foreach (var doorId in doorIds)
                {
                    string resultAssign = _accessScheduleService.AssignUsers(usersToAssign, doorId, companyId, out var msgIds);
                    if (!string.IsNullOrEmpty(resultAssign))
                    {
                        _logger.LogWarning(resultAssign);
                        return null;
                    }

                }

                return "";
            }

            private string GetTimezoneSendToDevice(int deviceId)
            {
                string timezone = "";
                try
                {
                    var deviceDefault = _unitOfWork.IcuDeviceRepository.GetById(deviceId);
                    timezone = _unitOfWork.BuildingRepository.GetById(deviceDefault.BuildingId.Value).TimeZone;
                }
                catch (Exception)
                {
                    _logger.LogError($"Can not get building by device - deviceId = {deviceId}");
                }

                return timezone;
            }
            

        }
        public class ResetLoginFailCount : IJob
        {
            private readonly IConfiguration _configuration;
            private readonly IUnitOfWork _unitOfWork;
            private readonly ILogger _logger;

            public ResetLoginFailCount(IConfiguration configuration)
            {
                _configuration = configuration;
                _unitOfWork = DbHelper.CreateUnitOfWork(configuration);
                _logger = ApplicationVariables.LoggerFactory.CreateLogger<ResetLoginFailCount>();
            }

            public void Execute()
            {
                try
                {
                    Console.WriteLine("==>START RESET LOGIN FAIL COUNT FOR UNLOCKED ACCOUNTS<==");

                    // Get all accounts that have failed login attempts
                    var accountsWithFailures = _unitOfWork.AppDbContext.Account
                        .Where(a => !a.IsDeleted && !string.IsNullOrEmpty(a.LoginConfig))
                        .ToList();

                    int resetCount = 0;
                    foreach (var account in accountsWithFailures)
                    {
                        try
                        {
                            var loginConfig = JsonConvert.DeserializeObject<DeMasterProCloud.DataModel.Account.LoginConfigModel>(account.LoginConfig);
                            if (loginConfig != null && loginConfig.LoginFailCount > 0)
                            {
                                // Get company's login settings to check lockout duration
                                var compAcc = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByAccount(account.Id);

                                if(compAcc.Count > 0)
                                {
                                    
                                    var companyId = compAcc.FirstOrDefault()?.CompanyId ?? 0;
                                    if (companyId > 0)
                                    {
                                        var loginSetting = _unitOfWork.SettingRepository.GetLoginSetting(companyId);
                                        
                                        // If lockout duration has passed, reset the fail count
                                        if (loginSetting != null && loginSetting.TimeoutWhenWrongPassword > 0 && loginConfig.LastTimeLoginFail != DateTime.MinValue)
                                        {
                                            var timeSinceLastFail = DateTime.UtcNow - loginConfig.LastTimeLoginFail;
                                            if (timeSinceLastFail.TotalMinutes >= loginSetting.TimeoutWhenWrongPassword)
                                            {
                                                loginConfig.LoginFailCount = 0;
                                                loginConfig.LastTimeLoginFail = DateTime.MinValue;
                                                account.LoginConfig = JsonConvert.SerializeObject(loginConfig);
                                                _unitOfWork.AccountRepository.Update(account);
                                                resetCount++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to process account {account.Id}: {ex.Message}");
                        }
                    }

                    if (resetCount > 0)
                    {
                        _unitOfWork.Save();
                        Console.WriteLine($"Reset login fail count for {resetCount} accounts");
                    }
                    else
                    {
                        Console.WriteLine("No accounts required login fail count reset");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in ResetLoginFailCount cronjob: {ex.Message}", ex);
                }
                finally
                {
                    _unitOfWork?.Dispose();
                }

                Console.WriteLine("==>END RESET LOGIN FAIL COUNT FOR UNLOCKED ACCOUNTS<==");
            }
        }

    }
}
