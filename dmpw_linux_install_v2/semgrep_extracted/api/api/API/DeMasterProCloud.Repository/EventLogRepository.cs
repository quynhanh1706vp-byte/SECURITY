using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Linq.Dynamic.Core;
using DeMasterProCloud.Common.Infrastructure;
using Microsoft.AspNetCore.Http;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.EventLog;
using System.Threading;
using DeMasterProCloud.DataModel.Meeting;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.DataModel.WorkingModel;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// Interface for EventLog repository
    /// </summary>
    public interface IEventLogRepository : IGenericRepository<EventLog>
    {
        long GetMaxIndexByIcu(int icuId);
        bool IsExisted(int icuId, DateTime? eventTime);
        bool IsDuplicated(EventLog eventLog);
        List<EventLog> GetAllEventLogNormalAccess(int companyId);

        List<EventLog> GetEventLogs(int companyId, List<int> eventTypes, DateTime start, DateTime end);
        int GetEventLogsCount(int companyId, List<int> eventTypes, DateTime start, DateTime end);
        int GetEventLogsPersonCount(int companyId, List<int> eventTypes, DateTime start, DateTime end, bool isUser, bool isIn);
        int GetEventLogsCount(DateTime start, DateTime end);
        int GetEventByTypeAccess(List<int> eventTypes, DateTime start, DateTime end);
        
        void AddEventLog(EventLog model);
        List<EventLog> GetFirstInOutLogNormalAccess(int companyId, DateTime firstTime, DateTime lastTime, TimeSpan offset);
        List<EventLog> GetFirstInOutNormalAccessToday(int companyId, TimeSpan opffSet);

        IEnumerable<EventLogListModel> GetPaginatedEventLog(List<EventLog> data, string culture);
        IEnumerable<EventLogReportListModel> GetPaginatedEventLogReport(List<EventLog> data, string culture, TimeSpan offSet);
        List<EventLog> GetFirstLastNormalAccessEventByUser(int id, DateTime startTime);
        List<EventLog> GetAllNormalAccessEventByUser(int id, DateTime startTime);
        List<EventLog> GetAllEventLogNormalAccessByTime(int companyId, int deviceId, DateTime start, DateTime end, string inOut);
        RecordMeetingReport GetRecordMeetingUserByTimeAndDevice(int companyId, int userId, int visitId, List<int> deviceIds, DateTime start, DateTime end);
        IQueryable<EventLogHistory> GetEventLogNormalAccessUserByTimeAndDevice(int companyId, int userId, List<int> deviceIds, DateTime start, DateTime end);
        int GetAllEventLogNormalAccessByTimeCount(int companyId, int deviceId, DateTime start, DateTime end, string inOut, bool isVisit=false);
        int GetUniqueUserCount(int companyId, DateTime start, DateTime end);
        IQueryable<EventLog> GetUniqueAccessUserIds(int companyId, DateTime startTime);
        EventLog GetLastEvent(int companyId, int userId, DateTime startTime, DateTime endTime, EventType normalAccess, string inOut);
        EventLog GetLastEventOfVisitor(int companyId, int visitId, DateTime startTime, DateTime endTime, EventType normalAccess, string inOut);
        List<EventLog> GetAllEventLogNormalAccessByTime(int companyId, DateTime start, DateTime end);
        int GetTotalVisitorAccessByTime(int companyId, DateTime start, DateTime end);
        double GetBodyTemperatureOnDayByUser(int userId, DateTime time);
        IQueryable<EventLog> GetAccessNormalByUserAndDateRange(int userId, DateTime start, DateTime end);
        IQueryable<EventLog> GetEventLogByTypesAndDeviceIds(List<int> types, List<int> devicesId, DateTime start, DateTime end);
        EventLog GetDetailById(int id);
        EventLog GetUniqueEventLog(int companyId, int deviceId, DateTime eventTime, string cardId);
    }

    /// <summary>
    /// EventLog repository
    /// </summary>
    public class EventLogRepository : GenericRepository<EventLog>, IEventLogRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        public EventLogRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<EventLogRepository>();
        }

        /// <summary>
        /// Get max event log index by icu
        /// </summary>
        /// <param name="icuId"></param>
        /// <returns></returns>
        public long GetMaxIndexByIcu(int icuId)
        {
            if (!_dbContext.EventLog.Any(c => c.IcuId == icuId))
            {
                return 0;
            }
            return _dbContext.EventLog.Where(c => c.IcuId == icuId).Max(c => c.Index);
        }

        /// <summary>
        /// Check if there is a event log is existed.
        /// </summary>
        /// <param name="icuId"></param>
        /// <param name="eventTime"></param>
        /// <returns></returns>
        public bool IsExisted(int icuId, DateTime? eventTime)
        {
            return _dbContext.EventLog.Any(c => c.IcuId == icuId && c.EventTime == eventTime);
        }

        public bool IsDuplicated(EventLog eventLog)
        {
            var minEventTime = new DateTime(eventLog.EventTime.Year, eventLog.EventTime.Month, eventLog.EventTime.Day,
                eventLog.EventTime.Hour, eventLog.EventTime.Minute, eventLog.EventTime.Second);
            var maxEventTime = minEventTime.AddSeconds(1);
            
            return _dbContext.EventLog.Any(e => e.IcuId == eventLog.IcuId
                                        // && e.EventTime.ToSettingDateTimeString().Equals(eventLog.EventTime.ToSettingDateTimeString())
                                        && minEventTime <= e.EventTime && e.EventTime < maxEventTime
                                        && e.EventType == eventLog.EventType
                                        && e.CardId == eventLog.CardId
                                        && e.Antipass == eventLog.Antipass);
        }

        public List<EventLog> GetAllEventLogNormalAccess(int companyId)
        {
            return _dbContext.EventLog.Where(e => e.UserId != null
                                                  // && e.CardId != null
                                                  && e.EventType == (short)EventType.NormalAccess
                                                  && e.Icu != null
                                                  && e.CompanyId == companyId).ToList();
        }
        
        

        public List<EventLog> GetEventLogs(int companyId, List<int> eventTypes, DateTime start, DateTime end)
        {
            return _dbContext.EventLog.Where(e => eventTypes.Contains(e.EventType)
                                                  && e.Icu != null
                                                  && e.CompanyId == companyId
                                                  && e.EventTime > start
                                                  && e.EventTime <= end).ToList();
        }

        public int GetEventLogsCount(int companyId, List<int> eventTypes, DateTime start, DateTime end)
        {
            return _dbContext.EventLog.Where(e => eventTypes.Contains(e.EventType)
                                                  && e.Icu != null
                                                  && e.CompanyId == companyId
                                                  && e.EventTime > start
                                                  && e.EventTime <= end).Count();
        }

        public int GetEventLogsPersonCount(int companyId, List<int> eventTypes, DateTime start, DateTime end, bool isUser, bool isIn)
        {
            var data = _dbContext.EventLog.Where(e =>
                eventTypes.Contains(e.EventType) &&
                e.Icu != null &&
                e.CompanyId == companyId &&
                e.EventTime > start &&
                e.EventTime <= end &&
                (isUser ? e.UserId != null : e.VisitId != null) &&
                (isIn ? e.Antipass.ToLower() == "in" : e.Antipass.ToLower() == "out")
            );

            return isUser
                ? data.Where(m => m.UserId.HasValue).Select(m => m.UserId).Distinct().Count()
                : data.Where(m => m.VisitId.HasValue).Select(m => m.VisitId).Distinct().Count();
        }

        public int GetEventLogsCount(DateTime start, DateTime end)
        {
            return _dbContext.EventLog.Where(e => e.EventTime >= start && e.EventTime <= end).Count();
        }

        public int GetEventByTypeAccess(List<int> eventTypes, DateTime start, DateTime end)
        {
            return _dbContext.EventLog.Where(e => e.EventTime > start
                                                  && e.EventTime <= end
                                                  && eventTypes.Contains(e.EventType)).Count();
        }
        
        public List<EventLog> GetAllEventLogNormalAccessByTime(int companyId, DateTime start, DateTime end)
        {
            return _dbContext.EventLog.Where(e => e.UserId != null
                                                  // && e.CardId != null
                                                  && e.EventType == (short)EventType.NormalAccess
                                                  && e.Icu != null
                                                  && e.CompanyId == companyId
                                                  && e.EventTime > start
                                                  && e.EventTime <=end).ToList();
        }

        public int GetTotalVisitorAccessByTime(int companyId, DateTime start, DateTime end)
        {
            return _dbContext.EventLog.Where(e => e.IsVisit && e.VisitId != null
                                                  && e.EventType == (short) EventType.NormalAccess
                                                  && e.Icu != null
                                                  && e.CompanyId == companyId
                                                  && e.EventTime > start
                                                  && e.EventTime <= end).Select(u => u.VisitId).Distinct().Count();
        }

        public int GetUniqueUserCount(int companyId, DateTime start, DateTime end)
        {
            int result = _dbContext.EventLog.Where(e => e.UserId != null
                                                  // && e.CardId != null
                                                  && e.EventType == (int)EventType.NormalAccess
                                                  && e.CompanyId == companyId
                                                  && e.EventTime > start
                                                  && e.EventTime <= end).Select(u => u.UserId).Distinct().Count();
            return result;
        }

        public List<EventLog> GetAllEventLogNormalAccessByTime(int companyId, int deviceId, DateTime start, DateTime end, string inOut)
        {
            List<int> eventTypes = new List<int>(new int[] { (int)EventType.NormalAccess, (int)EventType.PressedButton });
            return _dbContext.EventLog.Where(e => e.UserId != null
                                                  // && e.CardId != null
                                                  && eventTypes.Contains(e.EventType)
                                                  && e.IcuId == deviceId
                                                  && e.CompanyId == companyId
                                                  && e.Antipass == inOut
                                                  && e.EventTime >start
                                                  && e.EventTime <=end).ToList();
        }

        public RecordMeetingReport GetRecordMeetingUserByTimeAndDevice(int companyId, int userId, int visitId, List<int> deviceIds, DateTime start, DateTime end)
        {
            if(userId != 0)
            {

                var data = _dbContext.EventLog.Where(c =>
                        c.CompanyId == companyId
                        && deviceIds.Contains(c.IcuId)
                        && c.UserId.HasValue && c.UserId.Value == userId
                        && c.EventTime >= start && c.EventTime < end
                        && c.EventType == (short)EventType.NormalAccess)
                    .Include(m => m.User)
                    .Include(m => m.User.Department)
                    .Include(m => m.Icu)
                    .Include(m => m.Icu.Building)
                    .Include(m => m.Company)
                    .Include(m => m.Visit)
                    .OrderBy(m => m.EventTime);

                EventLog firstData = null, lastData = null;
                if (data.Count() == 1)
                {
                    firstData = data.First();
                }
                else if(data.Count() > 1)
                {
                    firstData = data.First();
                    lastData = data.Last();
                }

                return new RecordMeetingReport()
                {
                    FirstTimeAccess = firstData?.EventTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault),
                 
                    IsAttendance = firstData != null
                };
            } else
            {
                var data = _dbContext.EventLog.Where(c =>
                        c.CompanyId == companyId
                        && deviceIds.Contains(c.IcuId)
                        && c.VisitId.HasValue && c.VisitId.Value == visitId
                        && c.EventTime >= start && c.EventTime < end
                        && c.EventType == (short)EventType.NormalAccess)
                    .Include(m => m.Icu)
                    .Include(m => m.Icu.Building)
                    .Include(m => m.Company)
                    .Include(m => m.Visit)
                    .OrderBy(m => m.EventTime);

                EventLog firstData = null, lastData = null;
                if (data.Count() == 1)
                {
                    firstData = data.First();
                }
                else if(data.Count() > 1)
                {
                    firstData = data.First();
                    lastData = data.Last();
                }

                return new RecordMeetingReport()
                {
                    FirstTimeAccess = firstData?.EventTime.ConvertDefaultDateTimeToString(Constants.Settings.DateTimeFormatDefault),
                   
                    IsAttendance = firstData != null
                };
            }
        }
        
        public IQueryable<EventLogHistory> GetEventLogNormalAccessUserByTimeAndDevice(int companyId, int userId, List<int> deviceIds, DateTime start, DateTime end)
        {
            // var deviceDefault = _dbContext.IcuDevice.Include(m => m.Building)
            //     .FirstOrDefault(m => deviceIds.Contains(m.Id) && !m.IsDeleted);
            // if (deviceDefault == null)
            // {
            //     return new EnumerableQuery<EventLogHistory>(new List<EventLogHistory>());
            // }
            //
            // TimeZoneInfo zone = deviceDefault.Building.TimeZone.ToTimeZoneInfo();
            // var offset = zone.BaseUtcOffset;
            // var endDate = end.Date.Subtract(offset);
            // var startDate = start.Date.Subtract(offset);

            var data = _dbContext.EventLog.Where(c =>
                    c.CompanyId == companyId
                    && deviceIds.Contains(c.IcuId)
                    && c.UserId.HasValue && c.UserId.Value == userId
                    && c.EventTime >= start && c.EventTime < end
                    && c.EventType == (short)EventType.NormalAccess)
                .Include(m => m.User)
                .Include(m => m.User.Department)
                .Include(m => m.Icu)
                .Include(m => m.Icu.Building)
                .Include(m => m.Company)
                .Include(m => m.Visit)
                .OrderBy(m => m.EventTime)
                .Select(x => new EventLogHistory
                {
                    InOut = x.Antipass,
                    AccessTime = DateTimeHelper.ConvertDateTimeToIso(x.EventTime),
                    CardId = x.CardId,
                    EventDetail = x.EventType,
                    DoorName = x.DoorName,
                    UnixTime = x.EventTime.ToSettingDateTimeUnique(),
                    Avatar = x.User.Avatar,
                    ImageCamera = x.ImageCamera,
                    OtherCardId = x.OtherCardId,
                    ResultCheckIn = x.ResultCheckIn
                });

            return data;
        }

        public int GetAllEventLogNormalAccessByTimeCount(int companyId, int deviceId, DateTime start, DateTime end, string inOut,bool isVisit=false)
        {
            List<int> eventTypes = new List<int>(new int[] { (int)EventType.NormalAccess, (int)EventType.PressedButton });
            return _dbContext.EventLog.Where(e => e.UserId != null
                                                  // && e.CardId != null
                                                  && eventTypes.Contains(e.EventType)
                                                  && e.IcuId == deviceId
                                                  && e.CompanyId == companyId
                                                  && e.Antipass == inOut
                                                  && e.IsVisit == isVisit
                                                  && e.EventTime > start
                                                  && e.EventTime <= end).Count();
        }



        public List<EventLog> GetFirstInOutLogNormalAccess(int companyId, DateTime firstTime, DateTime lastTime, TimeSpan offsetTime)
        {
            var zero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            List<DateTime> allDates = new List<DateTime>();
            for (DateTime i = firstTime; i <= lastTime; i = i.AddDays(1))
            {
                allDates.Add(i);
            }
            List<EventLog> lstEventLog = new List<EventLog>();

            var users = _dbContext.User.Where(u => u.CompanyId == companyId && u.WorkingTypeId != null && !u.IsDeleted);
            foreach (var user in users)
            {
                WorkingType workingType = _dbContext.WorkingType.Find(user.WorkingTypeId);
                var listWorking =
                    JsonConvert.DeserializeObject<List<WorkingTime>>(workingType.WorkingDay);
                for (int i = 0; i < allDates.Count(); i++)
                {
                    CheckHolidayAttendance(firstTime.Date.AddDays(i), user.Id, companyId, listWorking);

                    var lst = GetFirstInOutLogNormalAccessByUser(companyId, user.Id,
                        firstTime.Date.AddDays(i).Subtract(offsetTime),
                        firstTime.Date.AddDays(i+1).Subtract(offsetTime));
                    var attendance = _dbContext.Attendance.Where(a => a.UserId == user.Id
                                                                      && a.CompanyId == companyId
                                                                      && a.Date == firstTime.Date).FirstOrDefault();
                    if (attendance != null)
                    {
                        attendance.ClockInD = zero;
                        attendance.ClockOutD = zero;
                        try
                        {
                            _dbContext.Attendance.Update(attendance);
                            _dbContext.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error in GetFirstInOutLogNormalAccess");
                        }
                    }
                    lstEventLog.Concat(lst);
                }
            }
            return lstEventLog.ToList();
        }


        /// <summary>
        /// Get all events of of 1 user in give date
        /// </summary>
        /// <param name="userId">Id of user </param>
        /// <param name="firstTime">Starting time of the day</param>
        /// <returns></returns>

        public List<EventLog> GetAllNormalAccessEventByUser(int userId, DateTime firstTime)
        {
            List<EventLog> result = _dbContext.EventLog.Where(e => e.UserId == userId &&
                                                      /*e.CardId != null &&*/ e.Icu != null &&
                                                      e.EventType == (short)EventType.NormalAccess &&
                                                      e.EventTime >= firstTime &&
                                                      e.EventTime < firstTime.AddDays(1)).OrderBy(e => e.EventTime).ToList();
            return result;
        }



        /// <summary>
        /// Get 1st and last event of of 1 user in give date
        /// </summary>
        /// <param name="userId">Id of user </param>
        /// <param name="firstTime">Starting time of the day</param>
        /// <returns></returns>

        public List<EventLog> GetFirstLastNormalAccessEventByUser(int userId, DateTime firstTime)
        {
            List<EventLog> lstEventLog = new List<EventLog>();
            var inFirst = _dbContext.EventLog.Where(e => e.UserId == userId &&
                                                         /*e.CardId != null &&*/ e.Icu != null &&
                                                         e.EventType == (short)EventType.NormalAccess &&
                                                         e.Antipass.ToLower() == "in" &&
                                                         e.EventTime >= firstTime &&
                                                         e.EventTime < firstTime.AddDays(1)
            ).OrderBy(e => e.EventTime).FirstOrDefault();

            var outFirst = _dbContext.EventLog.Where(e => e.UserId == userId &&
                                                          /*e.CardId != null &&*/ e.Icu != null &&
                                                          e.EventType == (short)EventType.NormalAccess &&
                                                          e.Antipass.ToLower() == "out" &&
                                                          e.EventTime >= firstTime &&
                                                          e.EventTime < firstTime.AddDays(1)
            ).OrderBy(e => e.EventTime).FirstOrDefault();

            var last = _dbContext.EventLog.Where(e => e.UserId == userId &&
                                                      /*e.CardId != null &&*/ e.Icu != null &&
                                                      e.EventType == (short)EventType.NormalAccess &&
                                                      e.EventTime >= firstTime &&
                                                      e.EventTime < firstTime.AddDays(1))
                .OrderBy(e => e.EventTime).LastOrDefault();

            if (inFirst != null)
            {
                if (outFirst !=null && outFirst.EventTime < inFirst.EventTime)
                {
                    lstEventLog.Add(outFirst);
                    lstEventLog.Add(inFirst);

                    if (last != null)
                        if (inFirst.EventTime != last.EventTime)
                        {
                            lstEventLog.Add(last);
                        }
                }
                else if (outFirst != null)
                {
                    lstEventLog.Add(inFirst);
                    if (last != null)
                        if (inFirst.EventTime != last.EventTime)
                        {
                            lstEventLog.Add(last);
                        }
                }
                else
                {
                    lstEventLog.Add(inFirst);
                }
            }

            return lstEventLog;

        }

        private List<EventLog> GetFirstInOutLogNormalAccessByUser(int companyId, int userId, DateTime firstTime, DateTime lastTime)
        {
            List<EventLog> lstEventLog = new List<EventLog>();
            var inFirst = _dbContext.EventLog.Where(e => e.UserId == userId &&
                                                         /*e.CardId != null &&*/ e.Icu != null && 
                                                         e.CompanyId == companyId &&
                                                         e.EventType == (short)EventType.NormalAccess &&
                                                         e.Antipass.ToLower() == "in" && 
                                                         e.EventTime >= firstTime &&
                                                         e.EventTime < lastTime
            ).OrderBy(e => e.EventTime).FirstOrDefault();
            var last = _dbContext.EventLog.Where(e => e.UserId == userId &&
                                                      /*e.CardId != null &&*/ e.Icu != null && 
                                                      e.CompanyId == companyId &&
                                                      e.EventType == (short)EventType.NormalAccess &&
                                                      e.EventTime >= firstTime &&
                                                      e.EventTime < lastTime)
                .OrderBy(e => e.EventTime).LastOrDefault();
            if (inFirst != null)
            {
                lstEventLog.Add(inFirst);
                if (last != null)
                    if (inFirst.EventTime != last.EventTime)
                    {
                        lstEventLog.Add(last);
                    }
            }

            return lstEventLog;

        }
        public List<EventLog> GetFirstInOutNormalAccessToday(int companyId, TimeSpan offsetTime)
        {

            List<EventLog> lstEventLog = new List<EventLog>();

            var users = _dbContext.User.Where(u => u.CompanyId == companyId && u.WorkingTypeId != null && !u.IsDeleted);
            foreach (var user in users)
            {
                var lst = GetFirstInOutLogNormalAccessByUser(companyId, user.Id, 
                    DateTime.Now.Date.Subtract(offsetTime),
                    DateTime.Now.Date.AddDays(1).Subtract(offsetTime));
                lstEventLog.Concat(lst);
            }
            return lstEventLog.ToList();
        }

        private void CheckHolidayAttendance(DateTime date, int userId, int companyId, List<WorkingTime> listWorking)
        {
            var zero = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            foreach (var timeWork in listWorking)
            {
                var type = Enum.GetName(typeof(AttendanceType), (short) AttendanceType.Holiday);
                if (timeWork.Name == date.DayOfWeek.ToString() &&
                    timeWork.Type == type)
                {
                    var attendance = _dbContext.Attendance.Where(a => a.UserId == userId
                                                                      && a.CompanyId == companyId
                                                                      && a.Date == date).FirstOrDefault();
                    if (attendance != null)
                    {
                        attendance.Type = (short) AttendanceType.Holiday;
                        attendance.StartD = zero;
                        attendance.ClockInD = zero;
                        attendance.ClockOutD = zero;
                        attendance.EndD = zero;
                        try
                        {
                            _dbContext.Attendance.Update(attendance);
                            _dbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error in CheckHolidayAttendance");
                        }
                    }
                    else
                    {
                        var newAttendance = new Attendance
                        {
                            Date = date.Date,
                            Type = (short) AttendanceType.Holiday,
                            UserId = userId,
                            CompanyId = companyId,
                            ClockInD = zero,
                            ClockOutD = zero,
                            StartD = zero,
                            EndD = zero
                        };
                        try
                        {
                            _dbContext.Attendance.Add(newAttendance);
                            _dbContext.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error in CheckHolidayAttendance");
                        }
                    }
                }
            }
        }

        public void AddEventLog(EventLog model)
        {
            try
            {
                _dbContext.EventLog.Add(model);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddEventLog");
            }
        }


        public IEnumerable<EventLogListModel> GetPaginatedEventLog(List<EventLog> data, string culture)
        {
            var listLanguageCurrent = JsonConvert.DeserializeObject<List<LanguageModel>>(JsonConvert.SerializeObject(Constants.Settings.ListLanguageCurrent));
            int cultureCode = listLanguageCurrent.FirstOrDefault(m => m.Tag == culture)?.Id ?? 0;

            var result = (from a in data
                          join b in _dbContext.Event on a.EventType equals b.EventNumber
                          where b.Culture == cultureCode
                          select new EventLogListModel
                          {
                              EventLogId = a.Id,
                              AccessTime = a.EventTime.ConvertDefaultDateTimeToString(),
                              EventTime = a.EventTime.ConvertDefaultDateTimeToString(),
                              UnixTime = a.EventTime.ToSettingDateTimeUnique(),
                              Building = a.Icu != null && a.Icu.Building != null ? a.Icu.Building.Name : "",
                              CardId = a.CardType == (short)CardType.PassCode ? "******" : a.CardId,
                              CardStatus = ((CardStatus)a.CardStatus).GetDescription(),
                              CardType = ((CardType)a.CardType).GetDescription(),
                              CardTypeId = a.CardType,
                              Department = a.User != null && a.User.Department != null ? a.User.Department.DepartName : "",
                              DepartmentName = a.User != null && a.User.Department != null ? a.User.Department.DepartName : "",
                              Device = a.Icu != null ? a.Icu.DeviceAddress : "",
                              DeviceAddress = a.Icu != null ? a.Icu.DeviceAddress : "",
                              DoorName = string.IsNullOrEmpty(a.DoorName) ? (a.Icu != null ? a.Icu.Name : "") : a.DoorName,
                              EventDetail = b.EventName,
                              EventType = (short) a.EventType,
                              ExpireDate = a.IsVisit
                              ? a.Visit.EndDate.ToSettingDateString()
                              : a.User?.ExpiredDate.ToSettingDateString(),
                              IcuId = a.IcuId,
                              InOut = Constants.AntiPass.ToList().FindIndex(x => x.Equals(a.Antipass, StringComparison.OrdinalIgnoreCase)) != -1
                              ? ((Antipass)Enum.Parse(typeof(Antipass), a.Antipass, true)).GetDescription()
                              : "",
                              IssueCount = a.IssueCount,
                              UserId = a.UserId,
                              UserName = a.UserName,
                              WorkType = a.IsVisit
                              ? 0
                              : a.User?.WorkType,
                              WorkTypeName = a.IsVisit
                              ? (a.Visit != null && !string.IsNullOrEmpty(a.Visit.VisitType) ? ((VisitType)int.Parse(a.Visit.VisitType)).GetDescription() : "")
                              : a.User != null && a.User.WorkType.HasValue ? ((WorkType)a.User.WorkType).GetDescription() : "",
                              UserType = a.VisitId != null ? (short)UserType.Visit : a.UserId != null ? (short)UserType.Normal : a.UserId,
                              Type = a.User != null ? a.User.WorkType ?? -1 : a.Visit != null ? (short) Int32.Parse(a.Visit.VisitType) : (short) -1,
                              VerifyMode = a.Icu != null ? ((VerifyMode)a.Icu.VerifyMode).GetDescription() : string.Empty,
                              VisitId = a.VisitId,
                              ImageCamera = a.ImageCamera,
                              OtherCardId = a.OtherCardId,
                              ResultCheckIn = a.ResultCheckIn,
                              BodyTemperature = a.BodyTemperature,
                              Videos = a.Videos,
                              AllowedBelonging = a.Visit?.AllowedBelonging,
                              Avatar = a.IsVisit 
                                  ? a.Visit != null ? a.Visit.Avatar : ""
                                  : a.User != null ? a.User.Avatar : "",
                              DeviceManagerIds = string.IsNullOrEmpty(a.Icu?.DeviceManagerIds) ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(a.Icu?.DeviceManagerIds),
                              PersonTypeArmy = a.IsVisit 
                                  ? (a.Visit != null && !string.IsNullOrEmpty(a.Visit.VisitType) ? ((VisitArmyType)int.Parse(a.Visit.VisitType)).GetDescription() : "")
                                  : a.User?.WorkType != null ? ((Army_WorkType)a.User.WorkType.Value).GetDescription() : ""
                          });

            return result;
        }

        public IEnumerable<EventLogReportListModel> GetPaginatedEventLogReport(List<EventLog> data, string culture, TimeSpan offSet)
        {
            var listLanguageCurrent = JsonConvert.DeserializeObject<List<LanguageModel>>(JsonConvert.SerializeObject(Constants.Settings.ListLanguageCurrent));
            int cultureCode = listLanguageCurrent.FirstOrDefault(m => m.Tag == culture)?.Id ?? 0;

            var result = (from a in data
                          join b in _dbContext.Event on a.EventType equals b.EventNumber
                          where b.Culture == cultureCode
                          select new EventLogReportListModel
                          {
                              AccessTime = a.EventTime.Add(offSet).ToString(ApplicationVariables.Configuration[Constants.DateTimeServerFormat + ":" + Thread.CurrentThread.CurrentCulture.Name]),
                              EventTime = a.EventTime.Add(offSet).ToString(ApplicationVariables.Configuration[Constants.DateTimeServerFormat + ":" + Thread.CurrentThread.CurrentCulture.Name]),
                              Action = a.Antipass,
                              BirthDay = a.User != null && a.User.BirthDay != null ? a.User.BirthDay.ToSettingDateString() : "",
                              Building = a.Icu != null && a.Icu.Building != null ? a.Icu.Building.Name : "",
                              CardId = a.CardType == (short)CardType.PassCode ? "******" : a.CardId,
                              CardStatus = ((CardStatus)a.CardStatus).GetDescription(),
                              CardType = ((CardType)a.CardType).GetDescription(),
                              Department = a.User != null && a.User.Department != null ? a.User.Department.DepartName : "",
                              DepartmentName = a.User != null && a.User.Department != null ? a.User.Department.DepartName : "",
                              DeviceAddress = a.Icu != null ? a.Icu.DeviceAddress : "",
                              DoorName = a.Icu != null ? a.Icu.Name : "",
                              EmployeeNumber = a.User != null ? a.User.EmpNumber : "",
                              EventLogId = a.Id,
                              EventDetail = b.EventName,
                              IcuId = a.IcuId,
                              InOut = Constants.AntiPass.ToList().FindIndex(x => x.Equals(a.Antipass, StringComparison.OrdinalIgnoreCase)) != -1
                              ? ((Antipass)Enum.Parse(typeof(Antipass), a.Antipass, true)).GetDescription()
                              : "",
                              IssueCount = a.IssueCount,
                              UserCode = a.User != null ? a.User.UserCode : "",
                              UserId = a.UserId,
                              UserName = a.UserName,
                              WorkType = a.IsVisit
                              ? 0
                              : a.User?.WorkType,
                              WorkTypeName = a.IsVisit
                              ? (a.Visit != null && !string.IsNullOrEmpty(a.Visit.VisitType) ? ((VisitType)int.Parse(a.Visit.VisitType)).GetDescription() : "")
                              : a.User != null && a.User.WorkType.HasValue ? ((WorkType)a.User.WorkType).GetDescription() : "",
                              VisitId = a.VisitId,
                              ImageCamera = (a.CardType != (short)CardType.VehicleId && a.CardType != (short)CardType.VehicleMotoBikeId) ? a.ImageCamera : null,
                              OtherCardId = a.OtherCardId,
                              VehicleImage = (a.CardType == (short)CardType.VehicleId || a.CardType != (short)CardType.VehicleMotoBikeId) ? a.ImageCamera : null,
                              Videos = (a.CardType != (short)CardType.VehicleId && a.CardType != (short)CardType.VehicleMotoBikeId) ? a.Videos : null,
                              VideosVehicle = (a.CardType == (short)CardType.VehicleId || a.CardType != (short)CardType.VehicleMotoBikeId) ? a.Videos : null,
                              ResultCheckIn = a.ResultCheckIn,
                              BodyTemperature = a.BodyTemperature,
                              DelayOpenDoorByCamera = a.DelayOpenDoorByCamera,
                              Distance = a.Distance,
                              SearchScore = a.SearchScore,
                              LivenessScore = a.LivenessScore,
                              Avatar = a.User != null ? a.User.Avatar : a.Visit?.Avatar,
                          });

            return result;
        }

        public IQueryable<EventLog> GetUniqueAccessUserIds(int companyId, DateTime startTime)
        {
            var result = _dbContext.EventLog.Where(e => e.UserId != null
                                                        // && e.CardId != null
                                                        && e.EventType == (int)EventType.NormalAccess
                                                        && e.CompanyId == companyId
                                                        && e.EventTime > startTime);
                                                  // && e.EventTime <= endTime);
                                                    // .Select(e=>e.UserId).Distinct().ToList();
            return result;
        }

        public EventLog GetLastEvent(int companyId, int userId, DateTime startTime, DateTime endTime, EventType normalAccess, string inOut)
        {
            return _dbContext.EventLog.Where(e => e.UserId == userId
                                                  // && e.CardId != null
                                                  && e.EventType == (int)normalAccess
                                                  && e.Antipass.ToLower() == inOut.ToLower()
                                                  && e.CompanyId == companyId
                                                  && e.EventTime > startTime
                                                  && e.EventTime <= endTime).OrderBy(e => e.EventTime).LastOrDefault();
        }
        
        public EventLog GetLastEventOfVisitor(int companyId, int visitId, DateTime startTime, DateTime endTime, EventType normalAccess, string inOut)
        {
            return _dbContext.EventLog.Where(e => e.VisitId == visitId
                                                  // && e.CardId != null
                                                  && e.EventType == (int)normalAccess
                                                  && e.Antipass.ToLower() == inOut.ToLower()
                                                  && e.CompanyId == companyId
                                                  && e.EventTime > startTime
                                                  && e.EventTime <= endTime).OrderBy(e => e.EventTime).LastOrDefault();
        }
        
        public double GetBodyTemperatureOnDayByUser(int userId, DateTime time)
        {
            DateTime startDay = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            DateTime endDay = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59);
            var eventLogs = _dbContext.EventLog.Where(m =>
                    m.UserId == userId
                    && startDay <= m.EventTime && m.EventTime <= endDay)
                .OrderByDescending(m => m.BodyTemperature).ToList();
            if (eventLogs.Any())
            {
                return eventLogs[0].BodyTemperature;
            }
            else
            {
                return 0;
            }
        }

        public IQueryable<EventLog> GetAccessNormalByUserAndDateRange(int userId, DateTime start, DateTime end)
        {
            return _dbContext.EventLog.Where(m => m.UserId == userId
                                             && m.EventType == (short)EventType.NormalAccess
                                             && m.EventTime >= start
                                             && m.EventTime < end);
        }

        public IQueryable<EventLog> GetEventLogByTypesAndDeviceIds(List<int> types, List<int> devicesId, DateTime start, DateTime end)
        {
            return _dbContext.EventLog.Where(m => devicesId.Contains(m.IcuId)
                                                  && (types == null || !types.Any() || types.Contains(m.EventType))
                                                  && m.EventTime >= start
                                                  && m.EventTime <= end);
        }

        public EventLog GetDetailById(int id)
        {
            return _dbContext.EventLog
                .Include(m => m.Icu).ThenInclude(m => m.Building)
                .Include(m => m.User).ThenInclude(m => m.Department)
                .Include(m => m.Visit)
                .FirstOrDefault(m => m.Id == id);
        }

        public EventLog GetUniqueEventLog(int companyId, int deviceId, DateTime eventTime, string cardId)
        {
            return _dbContext.EventLog
                .Include(m => m.User)
                .Include(m => m.User.Department)
                .Include(m => m.Visit)
                .Include(m => m.Icu).ThenInclude(m => m.Building)
                .FirstOrDefault(m => m.CompanyId == companyId && m.IcuId == deviceId && m.EventTime == eventTime && m.CardId == cardId);
        }
    }
}