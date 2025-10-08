using AutoMapper;
using Bogus.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Holiday;
using DeMasterProCloud.Repository;
using System.Linq.Dynamic.Core;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.Service.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DeMasterProCloud.Service.Protocol;
using DeMasterProCloud.Service.RabbitMqQueue;
using Microsoft.Extensions.Configuration;

namespace DeMasterProCloud.Service
{
    /// <summary>
    /// Holiday service interface
    /// </summary>
    public interface IHolidayService
    {
        IQueryable<Holiday> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);
        void InitData(HolidayModel model);
        bool Add(HolidayModel model);
        bool Add(List<HolidayModel> models);
        bool Update(HolidayModel model, Holiday holiday);
        Holiday GetHolidayByIdAndCompany(int companyId, int holidayId);
        bool Delete(Holiday holiday);
        List<Holiday> GetByIds(List<int> ids);
        bool DeleteRange(List<Holiday> holidays);
        int GetHolidayCount(int companyId);
        bool IsExistedName(int holidayId, string name);
        bool IsOverLapDurationTime(int holidayId, string startDate, string endDate);
    }

    public class HolidayService : IHolidayService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IWebSocketService _webSocketService;
        private readonly HttpContext _httpContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public HolidayService(IUnitOfWork unitOfWork, IWebSocketService webSocketService,
            IHttpContextAccessor httpContextAccessor, ILogger<HolidayService> logger, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _webSocketService = webSocketService;
            _httpContext = httpContextAccessor.HttpContext;
            _logger = logger;
            _mapper = MapperInstance.Mapper;
            _configuration = configuration;
        }

        /// <summary>
        /// Initial data
        /// </summary>
        /// <param name="model"></param>
        public void InitData(HolidayModel model)
        {
            model.HolidayTypeItems = EnumHelper.ToEnumList<HolidayType>();
            if (model.Type == 0)
            {
                model.Type = (int)HolidayType.HolidayType1;
            }
        }
        
        public bool Add(HolidayModel model)
        {
            var isSuccess = true;
            var companyId = _httpContext.User.GetCompanyId();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var holiday = _mapper.Map<Holiday>(model);
                        holiday.CompanyId = companyId;

                        // Datetime start 00:00:00, end 23:59:59
                        holiday.StartDate = new DateTime(holiday.StartDate.Year, holiday.StartDate.Month, holiday.StartDate.Day, 0, 0, 0);
                        holiday.EndDate = new DateTime(holiday.EndDate.Year, holiday.EndDate.Month, holiday.EndDate.Day, 23, 59, 59);

                        _unitOfWork.HolidayRepository.Add(holiday);
                        _unitOfWork.Save();

                        //Save system log
                        var content = $"{HolidayResource.lblAddNew}";
                        List<string> details = new List<string>()
                        {
                            $"{HolidayResource.lblHolidayName} : {holiday.Name}",
                            $"{HolidayResource.lblStartDate} : {holiday.StartDate}",
                            $"{HolidayResource.lblEndDate} : {holiday.EndDate}",
                            $"{HolidayResource.lblRecurring} : {(holiday.Recursive ? CommonResource.Use : CommonResource.NotUse)}",
                            $"{HolidayResource.lblHolidayType} : {holiday.Type}"
                        };
                        var contentsDetail = string.Join("<br />", details);
                        _unitOfWork.SystemLogRepository.Add(holiday.Id, SystemLogType.Holiday, ActionLogType.Add, content, contentsDetail, null, companyId);

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                        isSuccess = false;
                    }
                }
            });
            
            // Send to icu
            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
            string sender = _httpContext.User.GetUsername();
            var devices = _unitOfWork.IcuDeviceRepository.GetByCompany(companyId);
            foreach (var device in devices)
            {
                deviceInstructionQueue.SendHoliday(new HolidayQueueModel()
                {
                    DeviceId = device.Id,
                    DeviceAddress = device.DeviceAddress,
                    MsgId = Guid.NewGuid().ToString(),
                    MessageType = Constants.Protocol.UpdateHoliday,
                    Sender = sender,
                });
            }

            return isSuccess;
        }
        
        public bool Add(List<HolidayModel> models)
        {
            var isSuccess = true;
            var companyId = _httpContext.User.GetCompanyId();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var model in models)
                        {
                            var holiday = _mapper.Map<Holiday>(model);
                            holiday.CompanyId = companyId;

                            // Datetime start 00:00:00, end 23:59:59
                            holiday.StartDate = new DateTime(holiday.StartDate.Year, holiday.StartDate.Month, holiday.StartDate.Day, 0, 0, 0);
                            holiday.EndDate = new DateTime(holiday.EndDate.Year, holiday.EndDate.Month, holiday.EndDate.Day, 23, 59, 59);

                            _unitOfWork.HolidayRepository.Add(holiday);
                            _unitOfWork.Save();

                            //Save system log
                            var content = $"{HolidayResource.lblAddNew}";
                            List<string> details = new List<string>()
                            {
                                $"{HolidayResource.lblHolidayName} : {holiday.Name}",
                                $"{HolidayResource.lblStartDate} : {holiday.StartDate}",
                                $"{HolidayResource.lblEndDate} : {holiday.EndDate}",
                                $"{HolidayResource.lblRecurring} : {(holiday.Recursive ? CommonResource.Use : CommonResource.NotUse)}",
                                $"{HolidayResource.lblHolidayType} : {holiday.Type}"
                            };
                            var contentsDetail = string.Join("<br />", details);
                            _unitOfWork.SystemLogRepository.Add(holiday.Id, SystemLogType.Holiday, ActionLogType.Add, content, contentsDetail, null, companyId);
                        }

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                        isSuccess = false;
                    }
                }
            });

            //Send to icu
            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
            string sender = _httpContext.User.GetUsername();
            var devices = _unitOfWork.IcuDeviceRepository.GetByCompany(companyId);
            foreach (var device in devices)
            {
                deviceInstructionQueue.SendHoliday(new HolidayQueueModel()
                {
                    DeviceId = device.Id,
                    DeviceAddress = device.DeviceAddress,
                    MsgId = Guid.NewGuid().ToString(),
                    MessageType = Constants.Protocol.UpdateHoliday,
                    Sender = sender,
                });
            }

            return isSuccess;
        }

        public bool Update(HolidayModel model, Holiday holiday)
        {
            var isSuccess = true;
            var companyId = _httpContext.User.GetCompanyId();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        List<string> changes = new List<string>();
                        var existingName = holiday.Name;
                        var isChanged = CheckChange(holiday, model, ref changes);

                        //Update holiday
                        _mapper.Map(model, holiday);
                        // Datetime start 00:00:00, end 23:59:59
                        holiday.StartDate = new DateTime(holiday.StartDate.Year, holiday.StartDate.Month, holiday.StartDate.Day, 0, 0, 0);
                        holiday.EndDate = new DateTime(holiday.EndDate.Year, holiday.EndDate.Month, holiday.EndDate.Day, 23, 59, 59);
                        _unitOfWork.HolidayRepository.Update(holiday);

                        if (isChanged)
                        {
                            //Save system log
                            var content = string.Format(HolidayResource.lblUpdateHoliday, existingName);
                            var contentsDetails = string.Join("\n", changes);
                            _unitOfWork.SystemLogRepository.Add(holiday.Id, SystemLogType.Holiday, ActionLogType.Update,
                                content, contentsDetails, null, companyId);
                        }

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                        isSuccess = false;
                    }
                }
            });
            //Send to icu and save message log
            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
            string sender = _httpContext.User.GetUsername();
            var devices = _unitOfWork.IcuDeviceRepository.GetByCompany(companyId);
            foreach (var device in devices)
            {
                deviceInstructionQueue.SendHoliday(new HolidayQueueModel()
                {
                    DeviceId = device.Id,
                    DeviceAddress = device.DeviceAddress,
                    MsgId = Guid.NewGuid().ToString(),
                    MessageType = Constants.Protocol.UpdateHoliday,
                    Sender = sender,
                });
            }

            return isSuccess;
        }

        public bool Delete(Holiday holiday)
        {
            var isSuccess = true;
            var companyId = _httpContext.User.GetCompanyId();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        holiday.IsDeleted = true;
                        holiday.UpdatedOn = DateTime.UtcNow;
                        _unitOfWork.HolidayRepository.Update(holiday);

                        //Save system log
                        var content = $"{ActionLogTypeResource.Delete}: {holiday.Name} ({HolidayResource.lblHolidayName})";
                        _unitOfWork.SystemLogRepository.Add(holiday.Id, SystemLogType.Holiday, ActionLogType.Delete, content, null, null, companyId);

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                        isSuccess = false;
                    }
                }
            });

            //Send to icu and save message log
            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
            string sender = _httpContext.User.GetUsername();
            var devices = _unitOfWork.IcuDeviceRepository.GetByCompany(companyId);
            foreach (var device in devices)
            {
                deviceInstructionQueue.SendHoliday(new HolidayQueueModel()
                {
                    DeviceId = device.Id,
                    DeviceAddress = device.DeviceAddress,
                    MsgId = Guid.NewGuid().ToString(),
                    MessageType = Constants.Protocol.UpdateHoliday,
                    Sender = sender,
                });
            }

            return isSuccess;
        }

        public bool DeleteRange(List<Holiday> holidays)
        {
            var isSuccess = true;
            var companyId = _httpContext.User.GetCompanyId();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        foreach (var holiday in holidays)
                        {
                            holiday.IsDeleted = true;
                            holiday.UpdatedOn = DateTime.UtcNow;
                            _unitOfWork.HolidayRepository.Update(holiday);
                        }

                        //Save system log
                        var holidayIds = holidays.Select(c => c.Id).ToList();
                        var holidayNames = holidays.Select(c => c.Name).ToList();
                        var content = string.Format(ActionLogTypeResource.DeleteMultipleType, HolidayResource.lblHoliday);
                        var contentDetails = $"{HolidayResource.lblHolidayName}: {string.Join(", ", holidayNames)}";

                        _unitOfWork.SystemLogRepository.Add(holidayIds.First(), SystemLogType.Holiday, ActionLogType.DeleteMultiple, content, contentDetails, holidayIds, companyId);
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                        isSuccess = false;
                    }
                }
            });
            // Send to icu
            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, _webSocketService);
            string sender = _httpContext.User.GetUsername();
            var devices = _unitOfWork.IcuDeviceRepository.GetByCompany(companyId);
            foreach (var device in devices)
            {
                deviceInstructionQueue.SendHoliday(new HolidayQueueModel()
                {
                    DeviceId = device.Id,
                    DeviceAddress = device.DeviceAddress,
                    MsgId = Guid.NewGuid().ToString(),
                    MessageType = Constants.Protocol.UpdateHoliday,
                    Sender = sender,
                });
            }

            return isSuccess;
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
        public IQueryable<Holiday> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var data = _unitOfWork.HolidayRepository.GetHolidayByCompany(companyId).AsQueryable();

                totalRecords = data.Count();

                if (!string.IsNullOrEmpty(filter))
                {
                    filter = filter.Trim().RemoveDiacritics().ToLower();
                    data = data.Where(x => x.Name.ToLower().RemoveDiacritics().Contains(filter)
                                           || ((HolidayType)x.Type).GetDescription().RemoveDiacritics().ToLower().Contains(filter));
                }

                recordsFiltered = data.Count();

                // Default sort ( asc - HolidayName )
                data = data.OrderBy(c => c.Name);

                try
                {
                    int int_sortColumn = Int32.Parse(sortColumn);

                    int_sortColumn = int_sortColumn > ColumnDefines.Holiday.Length - 1 ? 0 : int_sortColumn;
                    data = data.OrderBy($"{ColumnDefines.Holiday[int_sortColumn]} {sortDirection}");
                }
                catch
                {
                    if (!string.IsNullOrEmpty(sortColumn))
                    {
                        sortColumn = sortColumn.ToPascalCase();

                        switch (sortColumn)
                        {
                            case var _ when sortColumn == ColumnDefines.Holiday[1]:
                                sortColumn = "Type";
                                break;
                            case var _ when sortColumn == ColumnDefines.Holiday[4]:
                                sortColumn = "Recursive";
                                break;
                        }
                        data = Helpers.SortData<Holiday>(data.AsEnumerable<Holiday>(), sortDirection, sortColumn);
                        if (sortColumn.ToLower() == ColumnDefines.Holiday[2].ToLower()
                            || sortColumn.ToLower() == ColumnDefines.Holiday[3].ToLower())
                        {
                            data = data.OrderBy($"{sortColumn} {sortDirection}");
                        }
                    }
                }

                data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginated");
                totalRecords = 0;
                recordsFiltered = 0;
                return Enumerable.Empty<Holiday>().AsQueryable();
            }
        }

        /// <summary>
        /// Check if there are some have changes
        /// </summary>
        /// <param name="holiday"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool HasChange(Holiday holiday, HolidayModel model)
        {
            return holiday.StartDate.Subtract(Convert.ToDateTime(model.StartDate)).Days != 0
                   || holiday.EndDate.Subtract(Convert.ToDateTime(model.EndDate)).Days != 0
                   || holiday.Type != model.Type || holiday.Recursive != model.Recursive;
        }

        /// <summary>
        /// Get list of holiday by ids
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<Holiday> GetByIds(List<int> ids)
        {
            try
            {
                return _unitOfWork.HolidayRepository.GetMany(m => ids.Contains(m.Id))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIds");
                return new List<Holiday>();
            }
        }

        /// <summary>
        /// Check two holiday is overlaped or not
        /// </summary>
        /// <param name="holidayId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public bool IsOverLapDurationTime(int holidayId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                var holidays = _unitOfWork.HolidayRepository.GetHolidayByCompany(companyId);
                foreach (var holiday in holidays)
                {
                    if (holidayId != holiday.Id && holiday.StartDate.Date.Subtract(endDate.Date).Days <= 0
                                                && startDate.Date.Subtract(holiday.EndDate.Date).Days <= 0)
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsOverLapDurationTime");
                return false;
            }
        }

        /// <summary>
        /// Get holiday by id and company
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="holidayId"></param>
        /// <returns></returns>
        public Holiday GetHolidayByIdAndCompany(int companyId, int holidayId)
        {
            try
            {
                var holiday = _unitOfWork.HolidayRepository.GetHolidayByIdAndCompany(companyId, holidayId);

                // Convert timezone
                //var accountTimezone = _unitOfWork.AccountRepository.Get(m => m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted).TimeZone;

                //holiday.StartDate = Helpers.ConvertToUserTime(Convert.ToDateTime(holiday.StartDate), accountTimezone);
                //holiday.EndDate = Helpers.ConvertToUserTime(Convert.ToDateTime(holiday.EndDate), accountTimezone);

                return holiday;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetHolidayByIdAndCompany");
                return null;
            }
        }

        /// <summary>
        /// Get holiday by name and company
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public Holiday GetHolidayByNameAndCompany(int companyId, string name)
        {
            return _unitOfWork.HolidayRepository.GetHolidayByNameAndCompany(companyId, name);
        }

        /// <summary>
        /// Get holiday by company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<Holiday> GetHolidayByCompany(int companyId)
        {
            return _unitOfWork.HolidayRepository.GetHolidayByCompany(companyId);
        }

        /// <summary>
        /// Get holiday count
        /// </summary>
        /// <returns></returns>
        public int GetHolidayCount(int companyId)
        {
            try
            {
                var countHoliday = 0;
                var holidays = _unitOfWork.HolidayRepository.GetHolidayByCompany(companyId);
                if (holidays.Any())
                {
                    countHoliday = GetAllCountHoliday(holidays);
                }
                return countHoliday;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetHolidayCount");
                return 0;
            }
        }

        /// <summary>
        /// Check if holiday name is exist
        /// </summary>
        /// <param name="holidayId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsExistedName(int holidayId, string name)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var holiday = GetHolidayByNameAndCompany(companyId, name);
            return holiday != null && holidayId != holiday.Id;
        }

        /// <summary>
        /// Check two holiday is overlaped or not
        /// </summary>
        /// <param name="holidayId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public bool IsOverLapDurationTime(int holidayId, string startDate, string endDate)
        {
            //            Need to validate datetime before doing the logic
            //Now i can input any value for the datetime and push to the server.
            //Incase datetime is invalid so it get 500 internal error.
            if (!Helpers.DateFormatCheck(startDate) || !Helpers.DateFormatCheck(endDate))
            {
                return false;
            }
            return IsOverLapDurationTime(holidayId, Convert.ToDateTime(startDate)
                , Convert.ToDateTime(endDate));
        }

        /// <summary>
        /// Make holiday protocol data
        /// </summary>
        /// <param name="holidays"></param>
        /// <param name="protocolType"></param>
        /// <returns></returns>
        public HolidayProtocolData MakeUpdateHolidayProtocolData(List<Holiday> holidays, string protocolType)
        {
            var holidayProtocolData = new HolidayProtocolData
            {
                MsgId = Guid.NewGuid().ToString(),
                Type = protocolType
            };

            var holidayProtocolHeaderData = new HolidayProtocolDataHeader
            {
                Total = GetAllCountHoliday(holidays),
                Holidays = _mapper.Map<List<HolidayProtocolDataDetail>>(holidays)
            };

            holidayProtocolData.Data = holidayProtocolHeaderData;
            return holidayProtocolData;
        }

        /// <summary>
        /// Get count holidays
        /// </summary>
        /// <param name="holidays"></param>
        /// <returns></returns>
        public int GetAllCountHoliday(List<Holiday> holidays)
        {
            var count = 0;
            foreach (var holiday in holidays)
            {
                var listDate = DateTimeHelper.GetListRangeDate(holiday.StartDate, holiday.EndDate);
                count += listDate.Count;
            }

            return count;
        }

        /// <summary>
        /// Checking if there are any changes.
        /// </summary>
        /// <param name="holiday">Holiday that contains existing information</param>
        /// <param name="model">Model that contains new information</param>
        /// <param name="changes">List of changes</param>
        /// <returns></returns>
        internal bool CheckChange(Holiday holiday, HolidayModel model, ref List<string> changes)
        {
            if (model.Id != 0)
            {
                if (holiday.Name != model.Name)
                {
                    changes.Add(Helpers.CreateChangedValueContents(HolidayResource.lblHolidayName, holiday.Name, model.Name));
                }

                if (holiday.Type != model.Type)
                {
                    changes.Add(Helpers.CreateChangedValueContents(HolidayResource.lblHolidayType, holiday.Type, model.Type));
                }

                if (holiday.StartDate.ToSettingDateString() != model.StartDate)
                {
                    changes.Add(Helpers.CreateChangedValueContents(HolidayResource.lblStartDate,
                        holiday.StartDate.ToSettingDateString(), model.StartDate));
                }

                if (holiday.EndDate.ToSettingDateString() != model.EndDate)
                {
                    changes.Add(Helpers.CreateChangedValueContents(HolidayResource.lblEndDate,
                        holiday.EndDate.ToSettingDateString(), model.EndDate));
                }

                if (holiday.Recursive != model.Recursive)
                {
                    changes.Add(Helpers.CreateChangedValueContents(HolidayResource.lblRecurring,
                        holiday.Recursive ? CommonResource.Use : CommonResource.NotUse,
                        model.Recursive ? CommonResource.Use : CommonResource.NotUse));
                }

            }

            return changes.Count() > 0;
        }

    }
}