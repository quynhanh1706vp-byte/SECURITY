using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Visit;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Repository
{
    public interface IVisitRepository : IGenericRepository<Visit>
    {

        IQueryable<Visit> GetByCompanyId(int companyId);
        IQueryable<Visit> GetVisitValidByAccessGroupIds(int companyId, List<int> accessGroupIds);
        IQueryable<Visit> GetByFirstApprovalId(int companyId, int approval1Id);
        Visit GetByVisitId(int companyId, int visitId);
        List<Visit> GetByVisitIds(int companyId, List<int> visitIds);
        List<Visit> GetByVisitIdsWithNoTracking(int companyId, List<int> visitIds);
        Visit GetByCardId(int? companyId, string cardId);
        Visit GetByPlateNumber(int? companyId, string plateNumber);
        Visit GetByCardIdExceptThis(int companyId, int id, string cardId);
        void CreateDefaultVisitSetting(int companyId);
        VisitSetting GetVisitSetting(int companyId);
        void UpdateVisitSetting(VisitSetting setting);
        IEnumerable<EventLog> GetHistoryVisitor(int id, int pageNumber, int pageSize, out int totalRecords, out int recordsFiltered);
        int GetByDayRegisteredVisitorsCount(int companyId, DateTime day);
        Visit GetVisitorByPhoneAndAccessTime(string phone, DateTime time, int companyId);
        Visit GetByNationIdNumber(string nationIdNumber, int companyId);
        List<Visit> GetByGroupId(string groupId);
        List<Visit> GetByGroupIdAndStatus(string groupId, short status);
        Visit GetByPhoneAndCompanyId(string phone, int companyId);
        void AddNationalIdCardForVisit(NationalIdCard nationalIdCard);
        Visit GetByAliasId(string aliasId, int companyId);
        IQueryable<Visit> GetVisitInvalidInCompanyId(int companyId);
    }

    public class VisitRepository : GenericRepository<Visit>, IVisitRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        public VisitRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext,
            contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<VisitRepository>();
        }

        public IQueryable<Visit> GetByCompanyId(int companyId)
        {
            return _dbContext.Visit
                .Where(m => m.CompanyId == companyId && !m.IsDeleted);
        }

        public IQueryable<Visit> GetByFirstApprovalId(int companyId, int approval1Id)
        {
            var visits = _dbContext.Visit.Where(m => m.CompanyId == companyId && m.ApproverId1 == approval1Id && m.Status == (short)VisitChangeStatusType.Waiting);

            return visits;
        }

        public Visit GetByVisitId(int companyId, int visitId)
        {
            return _dbContext.Visit
                .Include(v => v.AccessGroup)
                .Include(v => v.Card)
                .Include(v => v.NationalIdCard)
                .FirstOrDefault(m => m.Id == visitId && m.CompanyId == companyId && !m.IsDeleted);
        }

        public List<Visit> GetByVisitIds(int companyId, List<int> visitIds)
        {
            return _dbContext.Visit.Where(m =>
                visitIds.Contains(m.Id) && m.CompanyId == companyId && !m.IsDeleted).ToList();
        }

        public List<Visit> GetByVisitIdsWithNoTracking(int companyId, List<int> visitIds)
        {
            return _dbContext.Visit.AsNoTracking().Where(m =>
                visitIds.Contains(m.Id) && m.CompanyId == companyId && !m.IsDeleted).ToList();
        }

        public Visit GetByCardId(int? companyId, string cardId)
        {
            var card = _dbContext.Card.Where(m => m.CardId.Equals(cardId) && m.CompanyId == companyId && !m.IsDeleted).FirstOrDefault();

            if(card != null)
            {
                var visitId = card.VisitId;

                if (visitId != null)
                    return _dbContext.Visit.Include(m => m.Card).Where(m => m.Id == visitId && m.CompanyId == companyId && !m.IsDeleted && m.Status != (short)VisitChangeStatusType.CardReturned).FirstOrDefault();
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        public Visit GetByPlateNumber(int? companyId, string plateNumber)
        {
            plateNumber = plateNumber.ToLower()/*.RemoveAllEmptySpace()*/;
            var vehicle = _dbContext.Vehicle.Where(m => !m.IsDeleted && m.PlateNumber.ToLower()/*.RemoveAllEmptySpace()*/.Equals(plateNumber)).FirstOrDefault();

            if (vehicle != null)
            {
                var visitId = vehicle.VisitId;

                if (visitId != null)
                    return _dbContext.Visit.Where(m => m.Id == visitId && m.CompanyId == companyId && !m.IsDeleted && m.Status != (short)VisitChangeStatusType.CardReturned).FirstOrDefault();
                else
                    return null;
            }
            else
            {
                return null;
            }
        }

        public Visit GetByCardIdExceptThis(int companyId, int id, string cardId)
        {
            return _dbContext.Visit.FirstOrDefault(m => m.Id != id && m.CardId.Equals(cardId) && m.CompanyId == companyId
                                                        && !m.IsDeleted && m.Status != (short)VisitChangeStatusType.CardReturned);
        }

        public void CreateDefaultVisitSetting(int companyId)
        {
            var accessGroup = _dbContext.AccessGroup.FirstOrDefault(x => x.IsDefault && x.CompanyId == companyId && !x.IsDeleted);
            var setting = new VisitSetting
            {
                CompanyId = companyId,
                ApprovalStepNumber = (short)VisitSettingType.NoStep,
                VisibleFields = JsonConvert.SerializeObject(Helpers.GetSettingVisibleFields(null, typeof(VisitSettingModel))),
                ListFieldsEnable = JsonConvert.SerializeObject(Helpers.GetSettingVisibleFields(null, typeof(VisitModel), Constants.Settings.VisitListFieldsIgnored)),
                AccessGroupId = accessGroup?.Id ?? 0,
                FirstApproverAccounts = "[]",
                SecondsApproverAccounts = "[]",
                GroupDevices = "[]",
            };
            try
            {
                _dbContext.VisitSetting.Add(setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateDefaultVisitSetting");
            }
        }

        public VisitSetting GetVisitSetting(int companyId)
        {
            return _dbContext.VisitSetting.FirstOrDefault(m => m.CompanyId == companyId);
        }
        
        public IQueryable<Visit> GetVisitValidByAccessGroupIds(int companyId, List<int> accessGroupIds)
        {
            var data = _dbContext.Visit
                .Where(u => accessGroupIds.Any(a => a == u.AccessGroupId) && u.EndDate > DateTime.Now && !u.IsDeleted);

            if (companyId != 0)
            {
                data = data.Where(u => u.CompanyId == companyId);
            }

            return data;
        }

        public void UpdateVisitSetting(VisitSetting setting)
        {
            try
            {
                _dbContext.VisitSetting.Update(setting);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateVisitSetting");
            }
        }

        public IEnumerable<EventLog> GetHistoryVisitor(int id, int pageNumber, int pageSize, out int totalRecords, out int recordsFiltered)
        {
            var visitor = _dbContext.Visit.FirstOrDefault(c => c.Id == id);

            if (visitor != null)
            {
                List<int> visitIds = new List<int>();
                visitIds.Add(id);
                var visitIdOlds = _dbContext.Visit.Where(m => 
                        (!string.IsNullOrEmpty(visitor.Phone) && m.Phone == visitor.Phone) 
                        || (!string.IsNullOrEmpty(visitor.Email) && m.Email == visitor.Email)
                        || (!string.IsNullOrEmpty(visitor.NationalIdNumber) && m.NationalIdNumber == visitor.NationalIdNumber)
                ).Select(m => m.Id);
                visitIds.AddRange(visitIdOlds);
                
                var data = _dbContext.EventLog
                    // .Include(m => m.User)
                    // .Include(m => m.User.Department)
                    .Include(m => m.Icu)
                    .Include(m => m.Icu.Building)
                    .Include(m => m.Company)
                    .Include(m => m.Visit)
                    .Where(c => c.CompanyId == visitor.CompanyId && c.IsVisit == true 
                                     && visitIds.Contains(c.VisitId.Value));

                totalRecords = data.Count();
                data = data.OrderByDescending(m => m.EventTime);
                data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                recordsFiltered = data.Count();
                return data;
            }
            totalRecords = 0;
            recordsFiltered = 0;
            return new List<EventLog>();

        }

        public int GetByDayRegisteredVisitorsCount(int companyId, DateTime day)
        {
            return _dbContext.Visit.Count(m => m.CompanyId == companyId
                                               && !m.IsDeleted 
                                               && m.StartDate < day.AddDays(1)
                                               && m.EndDate >= day);
        }

        public Visit GetVisitorByPhoneAndAccessTime(string phone, DateTime time, int companyId)
        {
            try
            {
                return _dbContext.Visit.FirstOrDefault(m => !m.IsDeleted && m.CompanyId == companyId
                                                             && m.Phone == phone
                                                             && m.StartDate <= time && time <= m.EndDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetVisitorByPhoneAndAccessTime");
                return null;
            }
        }

        public Visit GetByNationIdNumber(string nationIdNumber, int companyId)
        {
            try
            {
                return _dbContext.Visit.Where(m =>
                        !m.IsDeleted && m.CompanyId == companyId && m.NationalIdNumber == nationIdNumber)
                    .OrderByDescending(m => m.Id).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByNationIdNumber");
                return null;
            }
        }

        public List<Visit> GetByGroupId(string groupId)
        {
            if (string.IsNullOrEmpty(groupId)) return null;

            try
            {
                var visits = _dbContext.Visit.Include(v => v.Card).Include(v => v.AccessGroup).ThenInclude(a => a.AccessGroupDevice).Where(v => v.GroupId.Equals(groupId));

                return visits.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByGroupId");
                return null;
            }
        }

        public List<Visit> GetByGroupIdAndStatus(string groupId, short status)
        {
            if (string.IsNullOrEmpty(groupId)) return null;

            try
            {
                var visits = _dbContext.Visit.Where(v => v.GroupId.Equals(groupId) && v.Status == status);

                return visits.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByGroupIdAndStatus");
                return null;
            }
        }

        public Visit GetByPhoneAndCompanyId(string phone, int companyId)
        {
            return _dbContext.Visit.FirstOrDefault(m => !m.IsDeleted && m.CompanyId == companyId && m.Phone == phone);
        }
        
        public void AddNationalIdCardForVisit(NationalIdCard nationalIdCard)
        {
            try
            {
                _dbContext.NationalIdCard.Add(nationalIdCard);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddNationalIdCardForVisit");
            }
        }

        public Visit GetByAliasId(string aliasId, int companyId)
        {
            return _dbContext.Visit.FirstOrDefault(m => !m.IsDeleted && m.AliasId == aliasId && companyId == m.CompanyId);
        }

        public IQueryable<Visit> GetVisitInvalidInCompanyId(int companyId)
        {
            return _dbContext.Visit.Where(m => !m.IsDeleted && m.EndDate >= DateTime.UtcNow && companyId == m.CompanyId);
        }
    }
}