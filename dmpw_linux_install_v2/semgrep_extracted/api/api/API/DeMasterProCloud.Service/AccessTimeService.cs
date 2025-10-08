using Bogus.Extensions;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Timezone;
using DeMasterProCloud.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using DeMasterProCloud.DataModel.RabbitMq;
using DeMasterProCloud.Service.RabbitMqQueue;
using Microsoft.Extensions.Configuration;
using Constants = DeMasterProCloud.Common.Infrastructure.Constants;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.Service.Infrastructure;

namespace DeMasterProCloud.Service
{
    public interface IAccessTimeService// : IPaginationService<AccessTimeListModel>
    {
        List<AccessTimeListModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);
        int Add(AccessTimeModel model);
        void Update(AccessTimeModel model, AccessTime timezone);
        bool AssignAccessTimeForUser(AssignAccessTime model);
        bool UnAssignAccessTimeFromUser(AssignAccessTime model);
        AccessTimeModel InitData(AccessTime timezone);
        AccessTime GetByIdAndCompany(int timezoneId, int companyId);
        void Delete(AccessTime timezone);
        void DeleteRange(List<AccessTime> timezones);
        List<AccessTime> GetByIdsAndCompany(List<int> ids, int companyId);
        int GetTimezoneCount(int companyId);
        bool IsExistedName(int timezoneId, string name);
    }

    public class AccessTimeService : IAccessTimeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;


        public AccessTimeService(IUnitOfWork unitOfWork, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<AccessTimeService> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;

            _httpContext = httpContextAccessor.HttpContext;
            _logger = logger;
        }

        public AccessTimeModel InitData(AccessTime timezone)
        {
            if (timezone == null)
            {
                return null;
            }

            var model = new AccessTimeModel
            {
                Id = timezone.Id,
                Name = timezone.Name,
                Remarks = timezone.Remarks,
                CompanyId = timezone.CompanyId,
                Position = timezone.Position,
                Monday = ConvertStringToDayDetail(new[] { timezone.MonTime1, timezone.MonTime2, timezone.MonTime3, timezone.MonTime4 }),
                Tuesday = ConvertStringToDayDetail(new[] { timezone.TueTime1, timezone.TueTime2, timezone.TueTime3, timezone.TueTime4 }),
                Wednesday = ConvertStringToDayDetail(new[] { timezone.WedTime1, timezone.WedTime2, timezone.WedTime3, timezone.WedTime4 }),
                Thursday = ConvertStringToDayDetail(new[] { timezone.ThurTime1, timezone.ThurTime2, timezone.ThurTime3, timezone.ThurTime4 }),
                Friday = ConvertStringToDayDetail(new[] { timezone.FriTime1, timezone.FriTime2, timezone.FriTime3, timezone.FriTime4 }),
                Saturday = ConvertStringToDayDetail(new[] { timezone.SatTime1, timezone.SatTime2, timezone.SatTime3, timezone.SatTime4 }),
                Sunday = ConvertStringToDayDetail(new[] { timezone.SunTime1, timezone.SunTime2, timezone.SunTime3, timezone.SunTime4 }),
                HolidayType1 = ConvertStringToDayDetail(new[] { timezone.HolType1Time1, timezone.HolType1Time2, timezone.HolType1Time3, timezone.HolType1Time4 }),
                HolidayType2 = ConvertStringToDayDetail(new[] { timezone.HolType2Time1, timezone.HolType2Time2, timezone.HolType2Time3, timezone.HolType2Time4 }),
                HolidayType3 = ConvertStringToDayDetail(new[] { timezone.HolType3Time1, timezone.HolType3Time2, timezone.HolType3Time3, timezone.HolType3Time4 }),
                CreatedBy = timezone.CreatedBy,
                CreatedOn = timezone.CreatedOn,
                UpdatedBy = timezone.UpdatedBy,
                UpdatedOn = timezone.UpdatedOn
            };

            return model;
        }

        public int Add(AccessTimeModel model)
        {
            var result = 0;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var positions = GetPositionsByCompany(_httpContext.User.GetCompanyId());
                        var timezone = new AccessTime
                        {
                            CompanyId = _httpContext.User.GetCompanyId(),
                            Name = model.Name,
                            Remarks = model.Remarks
                        };
                        //Calculate position
                        var curPositions = positions.OrderByDescending(c => c)
                            .ToList();

                        var maxRange = curPositions.FirstOrDefault();

                        var rangePosition = Enumerable.Range(1, maxRange);
                        var availablePositions = rangePosition.Except(curPositions)
                            .OrderByDescending(c => c)
                            .ToList();
                        if (availablePositions.Any())
                        {
                            timezone.Position = availablePositions.FirstOrDefault();
                        }
                        else
                        {
                            timezone.Position = maxRange + 1;
                        }

                        MappingData(model, timezone);
                        _unitOfWork.AccessTimeRepository.Add(timezone);

                        //Save system log
                        var content = $"{ActionLogTypeResource.Add} {AccessTimeResource.lblTimezone} : {timezone.Name} ({AccessTimeResource.lblTimezoneName})";
                        _unitOfWork.SystemLogRepository.Add(timezone.Id, SystemLogType.AccessTime, ActionLogType.Add, content, null, null, _httpContext.User.GetCompanyId());
                        _unitOfWork.Save();

                        transaction.Commit();
                        result = timezone.Id;

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError(ex.Message + ex.StackTrace);
                    }
                }
            });

            UpdateTimezoneListByCompany(_httpContext.User.GetCompanyId());
            return result;
        }

        public void Update(AccessTimeModel model, AccessTime timezone)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        MappingData(model, timezone);
                        timezone.Name = model.Name;
                        timezone.Remarks = model.Remarks;
                        _unitOfWork.AccessTimeRepository.Update(timezone);

                        //Save system log
                        var content = $"{ActionLogTypeResource.Update} : {timezone.Name} ({AccessTimeResource.lblTimezoneName})";
                        _unitOfWork.SystemLogRepository.Add(timezone.Id, SystemLogType.AccessTime, ActionLogType.Update, content, null, null, _httpContext.User.GetCompanyId());

                        // Save all changes
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in Update");
                        transaction.Rollback();
                        throw;
                    }
                }
            });

            UpdateTimezoneListByCompany(_httpContext.User.GetCompanyId());
        }
        public void Delete(AccessTime timezone)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // Delete AccessTime from system.
                        _unitOfWork.AccessTimeRepository.DeleteFromSystem(timezone);
                        _unitOfWork.Save();

                        //Save system log
                        var content = $"{ActionLogTypeResource.Delete} : {timezone.Name} ({AccessTimeResource.lblTimezoneName})";
                        _unitOfWork.SystemLogRepository.Add(timezone.Id, SystemLogType.AccessTime, ActionLogType.Delete, content, null, null, _httpContext.User.GetCompanyId());

                        //Save all changes
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in Delete");
                        transaction.Rollback();
                        throw;
                    }
                }
            });

            UpdateTimezoneListByCompany(_httpContext.User.GetCompanyId());
        }

        public void DeleteRange(List<AccessTime> timezones)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var companyId = _httpContext.User.GetCompanyId();
                        _unitOfWork.AccessTimeRepository.DeleteRangeFromSystem(timezones);

                        //Save system log
                        var timezoneIds = timezones.Select(c => c.Id).ToList();
                        var timezoneNames = timezones.Select(c => c.Name).ToList();
                        var content = string.Format(ActionLogTypeResource.DeleteMultipleType, AccessTimeResource.lblTimezone);
                        var contentDetails = $"{AccessTimeResource.lblTimezoneName} : {string.Join(", ", timezoneNames)}";

                        _unitOfWork.SystemLogRepository.Add(timezoneIds.First(), SystemLogType.AccessTime, ActionLogType.DeleteMultiple, content, contentDetails, timezoneIds, companyId);

                        //Save all changes
                        _unitOfWork.Save();

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in DeleteRange");
                        transaction.Rollback();
                        throw;
                    }
                }
            });

            UpdateTimezoneListByCompany(_httpContext.User.GetCompanyId());
        }

        public int GetTimezoneCount(int companyId)
        {
            return _unitOfWork.AccessTimeRepository.GetTimezoneCount(companyId);
        }

        public List<AccessTimeListModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var accessTimeListModels = _unitOfWork.AppDbContext.AccessTime.Where(m => !m.IsDeleted && m.CompanyId == companyId).ToList();

            var data = accessTimeListModels.Select(m => new AccessTimeListModel
            {
                Id = m.Id,
                AccessTimeName = (m.Id == 1 || m.Id == 2) ? ((DefaultTimezoneType)m.Id).GetDescription() : m.Name,
                Remark = m.Remarks,
                Position = m.Position
            });

            totalRecords = data.Count();
            if (!string.IsNullOrEmpty(filter))
            {
                var normalizedFilter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.AsEnumerable().Where(x => (x.AccessTimeName?.RemoveDiacritics()?.ToLower()?.Contains(normalizedFilter) == true)
                                       || (!string.IsNullOrEmpty(x.Remark) && x.Remark.RemoveDiacritics().ToLower().Contains(normalizedFilter))).AsQueryable();
            }
            recordsFiltered = data.Count();

            // Default sort ( asc - AccessTimeName )
            data = data.OrderBy(c => c.AccessTimeName);

            if (int.TryParse(sortColumn, out var intSortColumn))
            {
                var nameColumn = typeof(AccessTimeListModel).GetProperty(ColumnDefines.AccessTime[intSortColumn]);
                if (nameColumn != null)
                    data = data.AsQueryable().OrderBy($"{nameColumn.Name} {sortDirection}");
            }
            else if (!string.IsNullOrEmpty(sortColumn))
            {
                sortColumn = char.ToUpper(sortColumn[0]) + sortColumn.Substring(1);
                var nameColumn = typeof(AccessTimeListModel).GetProperty(sortColumn);
                if (nameColumn != null)
                {
                    data = data.AsQueryable().OrderBy($"{sortColumn} {sortDirection}");
                }
            }

            data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            return data.ToList();
        }

        public AccessTime GetByIdAndCompany(int timezoneId, int companyId)
        {
            var account = _unitOfWork.AccountRepository.Get(m => m.Id == _httpContext.User.GetAccountId() && !m.IsDeleted);
            var companyAccount = _unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompanyAndAccount(_httpContext.User.GetCompanyId(), account.Id);

            var accessTime = _unitOfWork.AccessTimeRepository.GetByIdAndCompany(timezoneId, companyId);

            //accessTime.CreatedOn = Helpers.ConvertToUserTimeZoneReturnDate(accessTime.CreatedOn, companyAccount.TimeZone);
            //accessTime.UpdatedOn = Helpers.ConvertToUserTimeZoneReturnDate(accessTime.UpdatedOn, companyAccount.TimeZone);

            var offSet = account.TimeZone.ToTimeZoneInfo().BaseUtcOffset;

            //accessTime.CreatedOn = Helpers.ConvertToUserTime(accessTime.CreatedOn, account.TimeZone);
            //accessTime.UpdatedOn = Helpers.ConvertToUserTime(accessTime.UpdatedOn, account.TimeZone);
            accessTime.CreatedOn = Helpers.ConvertToUserTime(accessTime.CreatedOn, offSet);
            accessTime.UpdatedOn = Helpers.ConvertToUserTime(accessTime.UpdatedOn, offSet);

            return accessTime;
        }

        public List<AccessTime> GetByIdsAndCompany(List<int> ids, int companyId)
        {
            return _unitOfWork.AccessTimeRepository.GetByIdsAndCompany(ids, companyId);
        }

        public AccessTime GetTimezoneByNameAndCompany(int companyId, string name)
        {
            return _unitOfWork.AccessTimeRepository.GetTimezoneByNameAndCompany(companyId, name);
        }

        public bool IsExistedName(int timezoneId, string name)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var timezone = GetTimezoneByNameAndCompany(companyId, name);
            if (timezone != null && timezoneId != timezone.Id)
            {
                return true;
            }
            return false;
        }

        private List<string> GetListDeviceAddress(int timezoneId)
        {
            var listDevice1 = new List<string>();
            var listDevice2 = new List<string>();
            var devicesFromDoor =
                _unitOfWork.IcuDeviceRepository.GetByTimezoneId(_httpContext.User.GetCompanyId(), timezoneId).ToList();
            if (devicesFromDoor.Any())
            {
                listDevice1 = devicesFromDoor.Select(x => x.DeviceAddress).ToList();
            }
            var devicesFromAgDevice =
                _unitOfWork.AccessGroupDeviceRepository.GetByTimezoneId(_httpContext.User.GetCompanyId(), timezoneId)
                    .Select(x => x.Icu).ToList();
            if (devicesFromAgDevice.Any())
            {
                listDevice2 = devicesFromAgDevice.Select(x => x.DeviceAddress).ToList();
            }

            var result = listDevice1.Union(listDevice2);
            return result.ToList();
        }

        private void MappingData(AccessTimeModel model, AccessTime timezone)
        {
            if (model.Monday != null && model.Monday.Any())
            {
                var monday = ConvertDayDetailToString(model.Monday);
                timezone.MonTime1 = monday[0] ?? Constants.Settings.DefaultTimezoneNotUse;
                timezone.MonTime2 = monday[1];
                timezone.MonTime3 = monday[2];
                timezone.MonTime4 = monday[3];
            }

            if (model.Tuesday != null && model.Tuesday.Any())
            {
                var tuesday = ConvertDayDetailToString(model.Tuesday);
                timezone.TueTime1 = tuesday[0] ?? Constants.Settings.DefaultTimezoneNotUse;
                timezone.TueTime2 = tuesday[1];
                timezone.TueTime3 = tuesday[2];
                timezone.TueTime4 = tuesday[3];
            }

            if (model.Wednesday != null && model.Wednesday.Any())
            {
                var wednesday = ConvertDayDetailToString(model.Wednesday);
                timezone.WedTime1 = wednesday[0] ?? Constants.Settings.DefaultTimezoneNotUse;
                timezone.WedTime2 = wednesday[1];
                timezone.WedTime3 = wednesday[2];
                timezone.WedTime4 = wednesday[3];
            }

            if (model.Thursday != null && model.Thursday.Any())
            {
                var thursday = ConvertDayDetailToString(model.Thursday);
                timezone.ThurTime1 = thursday[0] ?? Constants.Settings.DefaultTimezoneNotUse;
                timezone.ThurTime2 = thursday[1];
                timezone.ThurTime3 = thursday[2];
                timezone.ThurTime4 = thursday[3];
            }

            if (model.Friday != null && model.Friday.Any())
            {
                var friday = ConvertDayDetailToString(model.Friday);
                timezone.FriTime1 = friday[0] ?? Constants.Settings.DefaultTimezoneNotUse;
                timezone.FriTime2 = friday[1];
                timezone.FriTime3 = friday[2];
                timezone.FriTime4 = friday[3];
            }

            if (model.Saturday != null && model.Saturday.Any())
            {
                var saturday = ConvertDayDetailToString(model.Saturday);

                timezone.SatTime1 = saturday[0] ?? Constants.Settings.DefaultTimezoneNotUse;
                timezone.SatTime2 = saturday[1];
                timezone.SatTime3 = saturday[2];
                timezone.SatTime4 = saturday[3];
            }

            if (model.Sunday != null && model.Sunday.Any())
            {
                var sunday = ConvertDayDetailToString(model.Sunday);
                timezone.SunTime1 = sunday[0] ?? Constants.Settings.DefaultTimezoneNotUse;
                timezone.SunTime2 = sunday[1];
                timezone.SunTime3 = sunday[2];
                timezone.SunTime4 = sunday[3];
            }

            if (model.HolidayType1 != null && model.HolidayType1.Any())
            {
                var holidayType1 = ConvertDayDetailToString(model.HolidayType1);
                timezone.HolType1Time1 = holidayType1[0] ?? Constants.Settings.DefaultTimezoneNotUse;
                timezone.HolType1Time2 = holidayType1[1];
                timezone.HolType1Time3 = holidayType1[2];
                timezone.HolType1Time4 = holidayType1[3];
            }

            if (model.HolidayType2 != null && model.HolidayType2.Any())
            {
                var holidayType2 = ConvertDayDetailToString(model.HolidayType2);

                timezone.HolType2Time1 = holidayType2[0] ?? Constants.Settings.DefaultTimezoneNotUse;
                timezone.HolType2Time2 = holidayType2[1];
                timezone.HolType2Time3 = holidayType2[2];
                timezone.HolType2Time4 = holidayType2[3];
            }

            if (model.HolidayType3 != null && model.HolidayType3.Any())
            {
                var holidayType3 = ConvertDayDetailToString(model.HolidayType3);

                timezone.HolType3Time1 = holidayType3[0] ?? Constants.Settings.DefaultTimezoneNotUse;
                timezone.HolType3Time2 = holidayType3[1];
                timezone.HolType3Time3 = holidayType3[2];
                timezone.HolType3Time4 = holidayType3[3];
            }
        }

        private string[] ConvertDayDetailToString(List<DayDetail> dayDetails)
        {
            var result = new string[Constants.Settings.NumberTimezoneOfDay];
            for (int i = 0; i < result.Length; i++)
            {
                if (dayDetails.ElementAtOrDefault(i) != null)
                {
                    result[i] = JsonConvert.SerializeObject(dayDetails[i]);
                }
            }
            return result;
        }

        private List<DayDetail> ConvertStringToDayDetail(string[] dayDetails)
        {
            return dayDetails.Where(x => !string.IsNullOrEmpty(x)).Select(JsonConvert.DeserializeObject<DayDetail>).ToList();
        }

        private List<int> GetPositionsByCompany(int companyId)
        {
            return _unitOfWork.AccessTimeRepository.GetPositionsByCompany(companyId);
        }

        private void UpdateTimezoneListByCompany(int companyId)
        {
            IWebSocketService webSocketService = new WebSocketService();
            var devices = _unitOfWork.IcuDeviceRepository.GetByCompany(companyId);
            var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, webSocketService);
            string sender = _httpContext?.User?.GetUsername() ?? Constants.RabbitMq.SenderDefault;
            foreach (var device in devices)
            {
                try
                {
                    deviceInstructionQueue.SendUpdateTimezone(new UpdateTimezoneQueueModel()
                    {
                        MsgId = Guid.NewGuid().ToString(),
                        MessageType = Constants.Protocol.UpdateTimezone,
                        DeviceId = device.Id,
                        DeviceAddress = device.DeviceAddress,
                        Sender = sender,
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in UpdateTimezoneListByCompany");
                }
            }
        }

        private AccessGroup AddChildAccessGroupToParentOfUser(List<DoorModel> doorList, Company company, int userId, int parentAccessGroupId)
        {
            var accessGroup = _unitOfWork.AccessGroupRepository.GetById(parentAccessGroupId);
            if (doorList != null && doorList.Any())
            {
                var agDevices = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(company.Id, parentAccessGroupId)
                    .Select(m => new DoorModel()
                    {
                        DoorId = m.IcuId,
                        AccessTimeId = m.TzId,
                    }).ToList();
                var additionalDoorList = doorList.Select(m => new DoorModel()
                {
                    DoorId = m.DoorId,
                    AccessTimeId = m.AccessTimeId,
                }).Except(agDevices, new DoorModelCompare()).ToList();

                // New Access Group
                AccessGroup newAccessGroup = null;
                if (accessGroup != null && accessGroup.Type == (short)AccessGroupType.PersonalAccess)
                {
                    newAccessGroup = accessGroup;

                    // remove old agd unassigned
                    var deletedAgd = agDevices.Except(doorList.Select(m => new DoorModel()
                    {
                        DoorId = m.DoorId,
                        AccessTimeId = m.AccessTimeId,
                    }), new DoorModelCompare()).ToList();
                    if (deletedAgd.Any())
                    {
                        foreach (var item in deletedAgd)
                        {
                            try
                            {
                                _unitOfWork.AccessGroupDeviceRepository.Delete(m =>
                                    m.IcuId == item.DoorId &&
                                    m.TzId == item.AccessTimeId &&
                                    m.AccessGroupId == accessGroup.Id);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error in AddChildAccessGroupToParentOfUser");
                            }
                        }
                        try
                        {
                            _unitOfWork.Save();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error in AddChildAccessGroupToParentOfUser");
                        }
                    }
                }
                else
                {
                    if (additionalDoorList.Any())
                    {
                        newAccessGroup = new AccessGroup
                        {
                            Name = Constants.Settings.NameAccessGroupPersonal + userId,
                            CompanyId = company.Id,
                            Type = (short)AccessGroupType.PersonalAccess,
                            ParentId = parentAccessGroupId
                        };
                        try
                        {
                            _unitOfWork.AccessGroupRepository.Add(newAccessGroup);
                            _unitOfWork.Save();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error in AddChildAccessGroupToParentOfUser");
                        }
                    }
                }

                if (additionalDoorList.Any() && newAccessGroup != null)
                {
                    foreach (var door in additionalDoorList)
                    {
                        var detailDoor = _unitOfWork.IcuDeviceRepository.GetById(door.DoorId);
                        var detailModel = new AccessGroupDevice
                        {
                            IcuId = detailDoor.Id,
                            TzId = doorList.First(m => m.DoorId == door.DoorId).AccessTimeId,
                            AccessGroupId = newAccessGroup.Id
                        };
                        try
                        {
                            _unitOfWork.AccessGroupDeviceRepository.Add(detailModel);
                            _unitOfWork.Save();
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error in AddChildAccessGroupToParentOfUser");
                        }
                    }

                    return newAccessGroup;
                }
            }

            return null;
        }
        public bool AssignAccessTimeForUser(AssignAccessTime model)
        {
            var result = false;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var accessTime = _unitOfWork.AccessTimeRepository.GetById(model.AccessTimeId);
                        var companyId = _httpContext.User.GetCompanyId();
                        var company = _unitOfWork.CompanyRepository.GetById(companyId);



                        var deviceList = model.DeviceList.Select(m => new DoorModel()
                        {
                            DoorId = m.Id,
                            AccessTimeId = accessTime.Id,
                        }).ToList();



                        foreach (var userCode in model.UserCodes)
                        {
                            var user = _unitOfWork.UserRepository.GetByUserCode(companyId, userCode);
                            if (user != null)
                            {

                                List<IcuDevice> deletedDevices = null; // old device (system will send message delete card)

                                // old device
                                user = _unitOfWork.UserRepository.GetById(user.Id);
                                var oldListAgd = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, user.AccessGroupId).ToList();
                                deletedDevices = oldListAgd.Select(m => m.Icu).ToList();
                                int oldApprovalStatus = user.ApprovalStatus;



                                // new thread send update user to all doors
                                List<IcuDevice> addedDevices = new List<IcuDevice>();
                                string sender = _httpContext.User.GetUsername();

                                IWebSocketService webSocketService = new WebSocketService();
                                var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, webSocketService);
                                try
                                {

                                    // delete user info
                                    deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                                    {
                                        MessageType = Constants.Protocol.DeleteUser,
                                        MsgId = Guid.NewGuid().ToString(),
                                        Sender = sender,
                                        UserIds = new List<int>() { user.Id },
                                        CompanyCode = company.Code,
                                        DeviceIds = deletedDevices.Select(m => m.Id).ToList(),
                                    });
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }


                                var accessGroupIdCurrent = user.AccessGroupId;
                                // create child access group
                                // create new person access group
                                var newAccessGroup = AddChildAccessGroupToParentOfUser(deviceList, company, user.Id, accessGroupIdCurrent);
                                if (newAccessGroup != null)
                                {

                                    user.AccessGroupId = newAccessGroup.Id;
                                }

                                _unitOfWork.UserRepository.Update(user);
                                _unitOfWork.Save();

                                var newListAgd = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, user.AccessGroupId).ToList();
                                addedDevices = newListAgd.Select(m => m.Icu).ToList();

                                try
                                {
                                    var listApprovedStatus = new List<int>()
                                    {
                                        (int)ApprovalStatus.NotUse,
                                        (int)ApprovalStatus.Approved,
                                    };
                                    if (listApprovedStatus.Contains(user.ApprovalStatus))
                                    {
                                        // add user info
                                        Console.WriteLine("ADD");
                                        deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel()
                                        {
                                            MessageType = Constants.Protocol.AddUser,
                                            MsgId = Guid.NewGuid().ToString(),
                                            Sender = sender,
                                            UserIds = new List<int>() { user.Id },
                                            CompanyCode = company.Code,
                                            DeviceIds = addedDevices.Select(m => m.Id).ToList(),
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                            }
                        }





                        // create access group device


                        result = true;
                        // Save all changes
                        _unitOfWork.Save();
                        transaction.Commit();



                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in AssignAccessTimeForUser");
                        result = false;
                        transaction.Rollback();
                        throw;
                    }
                }
            });




            return result;

        }
        public bool UnAssignAccessTimeFromUser(AssignAccessTime model)
        {
            var result = false;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var companyId = _httpContext.User.GetCompanyId();
                        var company = _unitOfWork.CompanyRepository.GetById(companyId);
                        var sender = _httpContext.User.GetUsername();

                        // Get queue services
                        IWebSocketService webSocketService = new WebSocketService();
                        var deviceInstructionQueue = new DeviceInstructionQueue(_unitOfWork, _configuration, webSocketService);
                        
                        var accessGroupDefault = _unitOfWork.AccessGroupRepository.Gets(m =>m.IsDefault && !m.IsDeleted && m.CompanyId == companyId &&
                           m.Type != (short)AccessGroupType.PersonalAccess &&
                           m.Type != (short)AccessGroupType.VisitAccess).FirstOrDefault();

                        foreach (var userCode in model.UserCodes)
                        {
                            var user = _unitOfWork.UserRepository.GetByUserCode(companyId, userCode);
                            if (user == null) continue;

                            // Get current user devices
                            user = _unitOfWork.UserRepository.GetById(user.Id);
                            var oldListAgd = _unitOfWork.AccessGroupDeviceRepository.GetByAccessGroupId(companyId, user.AccessGroupId).ToList();
                            var deletedDevices = oldListAgd.Select(m => m.Icu).ToList();

                            // Remove access time from devices
                            var deviceList = new List<DoorModel>();

                            try
                            {
                                // Delete user from old devices
                                deviceInstructionQueue.SendInstructionCommon(new InstructionCommonModel
                                {
                                    MessageType = Constants.Protocol.DeleteUser,
                                    MsgId = Guid.NewGuid().ToString(),
                                    Sender = sender,
                                    UserIds = new List<int> { user.Id },
                                    CompanyCode = company.Code,
                                    DeviceIds = deletedDevices.Select(m => m.Id).ToList()
                                });

                                // Update access group
                                var accessGroupIdCurrent = accessGroupDefault.Id;
                                var newAccessGroup = AddChildAccessGroupToParentOfUser(deviceList, company, user.Id, accessGroupIdCurrent);
                                if (newAccessGroup != null)
                                {
                                    user.AccessGroupId = newAccessGroup.Id;
                                    _unitOfWork.UserRepository.Update(user);
                                    _unitOfWork.Save();
                                }
                                else
                                {
                                    user.AccessGroupId = accessGroupIdCurrent;
                                    _unitOfWork.UserRepository.Update(user);
                                    _unitOfWork.Save();
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error unassigning access time for user {user.Id}: {ex.Message}");
                                continue;
                            }
                        }

                        result = true;
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in UnAssignAccessTimeFromUser");
                        result = false;
                        transaction.Rollback();
                        throw;
                    }
                }
            });

            return result;
        }

    }
}
