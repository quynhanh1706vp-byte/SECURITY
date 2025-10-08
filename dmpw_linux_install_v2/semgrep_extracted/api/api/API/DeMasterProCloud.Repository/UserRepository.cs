using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.User;
using Newtonsoft.Json;
using DeMasterProCloud.DataModel.PlugIn;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Repository
{
    public interface IUserRepository : IGenericRepository<User>
    {
        List<User> GetUsersByCardId(int? companyId, List<string> cardIds);
        IQueryable<User> GetAssignUsersByAccessGroupIds(int companyId, List<int> accessGroupIds);
        IQueryable<User> GetUnAssignUsersByAccessGroupIds(int companyId, List<int> accessGroupIds);
        IQueryable<User> GetUnAssignUsers(int companyId, int accessGroupId);
        IQueryable<User> GetUnAssignUsersForMasterCard(int companyId, int accessGroupId);
        User GetByCardId(int? companyId, string cardId);
        List<User> GetByIds(int companyId, List<int> userIds);
        List<User> GetByIdsWithNoTracking(int companyId, List<int> userIds);
        IQueryable<User> GetByIds(List<int> userIds);
        User GetByIdIncludeDepartment(int? companyId, int userId);
        List<User> GetByDepartmentId(int departmentId);
        List<User> GetByDepartmentIds(List<int> departmentIds);
        List<User> GetUsersByFirstName(string firstName);
        bool IsKeyPadPasswordExist(int companyId, int userId, string enctypedKeyPadPassword);
        User GetByCardIdIncludeDepartment(int? companyId, string cardId);
        User GetByPlateNumberIncludeDepartment(int? companyId, string plateNumber);
        IQueryable<User> GetByCompanyId(int companyId, List<int> isValid = null);
        IQueryable<User> GetUserByCompany(int companyId);
        User GetByUserId(int companyId, int userId);
        int GetNewUserCode(int companyId);
        User GetByUserCode(int companyId, string userCode);
        IQueryable<User> GetByUserCodes(int companyId, List<string> userCodes);
        IQueryable<User> GetByConditions(int companyId, List<string> userCodes, List<string> cardids);
        List<User> GetAllUserInSystemNoWorkingTime();
        int GetCountByDepartmentId(int companyId, int departmentId);
        User GetUserByEmail(int companyId, string emailAddress);
        User GetUserByAccountId(int accountId, int companyId);
        List<User> GetUsersByAccountId(int accountId);
        User GetUserIncludeCardsById(int id);
        string GetNameByUsernameAndCompanyId(string userName, int companyId);
        User? GetUserByUsernameAndCompanyId(string userName, int companyId);
        void CreateDefaultAccessSetting(int companyId);
        AccessSetting GetAccessSetting(int companyId);
        void UpdateAccessSetting(AccessSetting setting);
        User GetByAccountId(int accountId, int companyId);
        User GetByNationalIdNumber(string idNumber, int companyId);
        List<User> GetUsersByWorkType(int companyId, short workType);

        IQueryable<User> GetAssignUsersByDeviceId(int companyId, List<int> deviceIds);

        IQueryable<User> GetUnAssignUsersByDeviceId(int companyId, List<int> deviceIds);
        void AddNationalIdCardForUser(NationalIdCard nationalIdCard);
    }
    
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;
        public UserRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext,
            contextAccessor)
        {
            _dbContext = dbContext;
            _logger = ApplicationVariables.LoggerFactory.CreateLogger<UserRepository>();
        }
        
        public List<User> GetUsersByCardId(int? companyId, List<string> cardIds)
        {
            var userIds = _dbContext.Card.Where(c => cardIds.Contains(c.CardId) && c.CompanyId == companyId && !c.IsDeleted).Select(u => u.UserId)
                .ToList();
            return _dbContext.User.AsNoTracking().Where(c =>
                userIds.Contains(c.Id) && c.CompanyId == companyId && !c.IsDeleted).ToList();
        }
        public User GetByIdIncludeDepartment(int? companyId, int userId)
        {
            return _dbContext.User
                .Include(c => c.Department)
                .ThenInclude(c => c.Parent)
                .Include(m => m.Card)
                .Include(m => m.WorkingType).FirstOrDefault(c => c.Id == userId && c.CompanyId == companyId && !c.IsDeleted);
        }
        public List<User> GetByIds(int companyId, List<int> userIds)
        {
            return _dbContext.User.Where(m => userIds.Any(c => c == m.Id) && m.CompanyId == companyId && !m.IsDeleted)
                .Include(c => c.Department)
                .Include(c => c.Card).ThenInclude(m => m.FingerPrint)
                .Include(c => c.AccessGroup).ThenInclude(c => c.AccessGroupDevice).ThenInclude(c => c.Icu)
                .Include(c => c.AccessGroup).ThenInclude(c => c.Parent).ToList();
        }
        public List<User> GetByIdsWithNoTracking(int companyId, List<int> userIds)
        {
            return _dbContext.User.AsNoTracking().Where(m => userIds.Any(c => c == m.Id) && m.CompanyId == companyId && !m.IsDeleted)
                .Include(c => c.Department)
                .Include(c => c.Card).ThenInclude(m => m.FingerPrint)
                .Include(c => c.AccessGroup).ThenInclude(c => c.AccessGroupDevice).ThenInclude(c => c.Icu)
                .Include(c => c.AccessGroup).ThenInclude(c => c.Parent).ToList();
        }
        
        public IQueryable<User> GetByIds(List<int> userIds)
        {
            return _dbContext.User.Where(m => userIds.Any(c => c == m.Id) && !m.IsDeleted);
        }

        public User GetByCardId(int? companyId, string cardId)
        {
            var userId = _dbContext.Card.Where(c => cardId.Equals(c.CardId) && c.CompanyId == companyId && !c.IsDeleted)
                .Select(u => u.UserId).FirstOrDefault();


            return _dbContext.User.FirstOrDefault(c =>
                c.Id == userId && c.CompanyId == companyId && !c.IsDeleted);
        }
        
        public List<User> GetByDepartmentId(int departmentId)
        {
            return _dbContext.User.Where(c => c.DepartmentId == departmentId && !c.IsDeleted).ToList();
        }
        public List<User> GetByDepartmentIds(List<int> departmentIds)
        {
            return _dbContext.User.Where(c => departmentIds.Contains(c.DepartmentId) && !c.IsDeleted).ToList();
        }
        public List<User> GetUsersByWorkType(int companyId, short workType)
        {
            return _dbContext.User.Where(c => c.CompanyId == companyId && !c.IsDeleted && c.WorkType == workType).ToList();
        }
        
        public List<User> GetUsersByFirstName(string firstName)
        {
            return _dbContext.User.Where(u => u.FirstName.ToUpper().Contains(firstName.ToUpper())).ToList();
        }

        public User GetByCardIdIncludeDepartment(int? companyId, string cardId)
        {
            var userId = _dbContext.Card.Where(c => cardId.ToLower().Equals(c.CardId.ToLower()) && c.CompanyId == companyId && !c.IsDeleted)
                .Select(u => u.UserId).ToList().FirstOrDefault();

            if (userId != null)
                return _dbContext.User.Include(c => c.Department)
                    .ThenInclude(x => x.Parent)
                    .Include(m => m.Card)
                    .Include(m => m.WorkingType).FirstOrDefault(c => c.Id == userId && c.CompanyId == companyId && !c.IsDeleted);
            else
                return null;
        }

        public User GetByPlateNumberIncludeDepartment(int? companyId, string plateNumber)
        {
            var userId = _dbContext.Vehicle.Where(c => !c.IsDeleted && plateNumber.ToLower()/*.RemoveAllEmptySpace()*/.Equals(c.PlateNumber.ToLower()/*.RemoveAllEmptySpace()*/) && c.CompanyId == companyId)
                .Select(u => u.UserId).ToList().FirstOrDefault();

            if (userId != null)
                return _dbContext.User.Include(c => c.Department)
                    .ThenInclude(x => x.Parent)
                    .Include(m => m.WorkingType).FirstOrDefault(c => c.Id == userId && c.CompanyId == companyId && !c.IsDeleted);
            else
                return null;
        }
        
        public List<User> GetByKeyPadPws(int companyId, List<string> keyPadPws)
        {
            return _dbContext.User.AsNoTracking().Where(c =>
                    keyPadPws.Any(m => m == c.KeyPadPw) && c.CompanyId == companyId && !c.IsDeleted)
                .ToList();
        }
        
        public bool IsKeyPadPasswordExist(int companyId, int userId, string enctypedKeyPadPassword)
        {
            if (!string.IsNullOrEmpty(enctypedKeyPadPassword))
            {
                return Get(m =>
                           m.KeyPadPw == enctypedKeyPadPassword && m.CompanyId == companyId &&
                           m.Id != userId && !m.IsDeleted) != null;
            }

            return false;
        }
        
        public IQueryable<User> GetAssignUsersByAccessGroupIds(int companyId, List<int> accessGroupIds)
        {
            var data = _dbContext.User.Include(m => m.Department).Include(m => m.AccessGroup)
                .Where(u => accessGroupIds.Any(a => a == u.AccessGroupId) && !u.IsDeleted && u.Status == (short) Status.Valid);

            if (companyId != 0)
            {
                data = data.Where(u => u.CompanyId == companyId);
            }

            //return _dbContext.User.Include(m => m.Department)
            //    .Where(u => u.CompanyId == companyId && accessGroupIds.Any(a => a == u.AccessGroupId) && !u.IsDeleted);

            return data;
        }
        
        public IQueryable<User> GetUnAssignUsersByAccessGroupIds(int companyId, List<int> accessGroupIds)
        {
            var data = _dbContext.User.Include(m => m.Department).Include(m => m.AccessGroup)
                .Where(u => !accessGroupIds.Any(a => a == u.AccessGroupId) && !u.IsDeleted && u.Status == (short)Status.Valid);

            if (companyId != 0)
            {
                data = data.Where(u => u.CompanyId == companyId);
            }

            //return _dbContext.User.Include(m => m.Department)
            //    .Where(u => u.CompanyId == companyId && accessGroupIds.Any(a => a == u.AccessGroupId) && !u.IsDeleted);

            return data;
        }

        public IQueryable<User> GetUnAssignUsers(int companyId, int accessGroupId)
        {
            // IsMasterCard
            //return _dbContext.User.Include(c => c.Department).Include(c => c.AccessGroup).Where(m =>
            //    m.CompanyId == companyId && m.AccessGroupId != accessGroupId && !m.IsDeleted && !m.IsMasterCard);

            return _dbContext.User
                .Include(c => c.Department)
                .Include(c => c.AccessGroup).ThenInclude(e => e.Parent)
                .Where(m => m.CompanyId == companyId 
                        && m.AccessGroupId != accessGroupId 
                        && !m.IsDeleted);
        }

        public IQueryable<User> GetUnAssignUsersForMasterCard(int companyId, int accessGroupId)
        {
            return _dbContext.User
                .Include(c => c.Department)
                .Include(c => c.AccessGroup).ThenInclude(e => e.Parent)
                .Where(m => m.CompanyId == companyId 
                        && m.AccessGroupId != accessGroupId 
                        && !m.IsDeleted);
        }

        public IQueryable<User> GetByCompanyId(int companyId, List<int> isValid = null)
        {
            IQueryable<User> data = _dbContext.User
                .Include(m => m.Department)
                .Include(m => m.AccessGroup).ThenInclude(n => n.Parent)
                .Include(m => m.Card)
               .Where(m => !m.IsDeleted);

            if (isValid != null && isValid.Any())
            {
                data = data.Where(m => isValid.Contains(m.Status));
            }

            if (companyId != 0)
            {
                data = data.Where(m => m.CompanyId == companyId);
            }

            return data;
        }

        public IQueryable<User> GetUserByCompany(int companyId)
        {
            var data = _dbContext.User.Include(m => m.Department).Include(m => m.AccessGroup)
                .Where(m => !m.IsDeleted);

            if (companyId != 0)
            {
                data = data.Where(m => m.CompanyId == companyId);
            }

            //return _dbContext.User.Include(m => m.Department).Include(m => m.AccessGroup)
            //    .Where(m => m.CompanyId == companyId && !m.IsDeleted);

            return data;
        }

        public User GetByUserId(int companyId, int userId)
        {
            return _dbContext.User
                .Include(m => m.AccessGroup)
                .Include(m => m.Department)
                .Include(m => m.WorkingType)
                .Include(m => m.Card)
                .Include(m => m.Vehicle)
                .Include(m => m.Face)
                .Include(m => m.NationalIdCard)
                .FirstOrDefault(m => m.Id == userId && m.CompanyId == companyId && !m.IsDeleted);
        }

        public int GetNewUserCode(int companyId)
        {
            var userCodeMax = _dbContext.User.Where(m => m.CompanyId == companyId && !m.IsDeleted)
                .OrderByDescending(m => m.UserCode).FirstOrDefault();
            if (userCodeMax != null)
            {
                if (int.TryParse(userCodeMax.UserCode, out int userCode))
                {
                    return userCode + 1;
                }
                return 1;
            }
            else
            {
                return 1;
            }
        }
        
        public User GetByUserCode(int companyId, string userCode)
        {
            //var userId = _dbContext.Card.Where(c => cardId.Equals(c.CardId) && c.CompanyId == companyId && !c.IsDeleted)
            //    .Select(u => u.UserId).ToList().FirstOrDefault();

            return _dbContext.User
                .Include(m => m.AccessGroup)
                .Include(m => m.Department)
                .Include(m => m.WorkingType)
                .Include(m => m.Card)
                .Include(m => m.Vehicle)
                .Include(m => m.Face)
                .FirstOrDefault(c => c.UserCode.Equals(userCode) && c.CompanyId == companyId && !c.IsDeleted);
        }

        public IQueryable<User> GetByUserCodes(int companyId, List<string> userCodes)
        {
            if(userCodes == null)
                userCodes = new List<string>();

            IQueryable<User> result = _dbContext.User
                .Include(user => user.Card)
                .Where(user => userCodes.Contains(user.UserCode) && user.CompanyId == companyId && !user.IsDeleted);

            return result;
        }

        public IQueryable<User> GetByConditions(int companyId, List<string> userCodes, List<string> cardids)
        {
            IQueryable<User> result = _dbContext.User
                .Include(user => user.Card)
                .Where(user => user.CompanyId == companyId && !user.IsDeleted);

            if (userCodes != null && userCodes.Any())
            {
                result = result.Where(user => userCodes.Contains(user.UserCode));
            }

            if (cardids != null && cardids.Any())
            {
                result = result.Where(user => user.Card.Select(c => c.CardId).Any(cardid => cardids.Contains(cardid)));
            }

            return result;
        }
        
        public List<User> GetAllUserInSystemNoWorkingTime()
        {
            return _dbContext.User.Include(m => m.WorkingType)
                .Where(m => !m.IsDeleted && m.WorkingTypeId == null).ToList();
        }

        public int GetCountByDepartmentId(int companyId, int departmentId)
        {
            var count = _dbContext.User.Where(c => c.DepartmentId == departmentId && c.CompanyId == companyId && !c.IsDeleted).Count();

            return count;
        }

        public User GetUserByEmail(int companyId, string emailAddress)
        {
            var user = _dbContext.User
                .FirstOrDefault(c => c.CompanyId == companyId 
                                     && ( c.Email.Equals(emailAddress) ) 
                                     && !c.IsDeleted);

            return user;
        }

        public User GetUserByAccountId(int accountId, int companyId)
        {
            var user = _dbContext.User.Include(m => m.Department).Where(u => u.AccountId == accountId && !u.IsDeleted && u.CompanyId == companyId).FirstOrDefault();
            return user;
        }

        public List<User> GetUsersByAccountId(int accountId)
        {
            return _dbContext.User.Where(u => u.AccountId == accountId && !u.IsDeleted).ToList();
        }

        public User GetUserIncludeCardsById(int id)
        {
            return _dbContext.User.Include(m => m.Card).FirstOrDefault(m => m.Id == id);
        }

        public string GetNameByUsernameAndCompanyId(string userName, int companyId)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                var account = _dbContext.Account.FirstOrDefault(m => m.Username.ToLower() == userName.ToLower());
                if (account != null)
                {
                    var user = _dbContext.User.FirstOrDefault(m => m.AccountId == account.Id && m.CompanyId == companyId && !m.IsDeleted);
                    if (user != null)
                    {
                        return user.FirstName;
                    }
                }   
            }
            
            return userName;
        }
        public User? GetUserByUsernameAndCompanyId(string userName, int companyId)
        {
            if (!string.IsNullOrEmpty(userName))
            {
                var account = _dbContext.Account.FirstOrDefault(m => m.Username.ToLower() == userName.ToLower());
                if (account != null)
                {
                    var user = _dbContext.User.FirstOrDefault(m => m.AccountId == account.Id && m.CompanyId == companyId && !m.IsDeleted);
                    if (user != null)
                    {
                        return user;
                    }
                }   
            }
            
            return null;
        }
        
        public void CreateDefaultAccessSetting(int companyId)
        {
            var setting = new AccessSetting
            {
                CompanyId = companyId,
                ApprovalStepNumber = (short)VisitSettingType.NoStep,
                VisibleFields = JsonConvert.SerializeObject(Helpers.GetSettingVisibleFields(null, typeof(AccessSettingModel))),
                //ListFieldsEnable = JsonConvert.SerializeObject(Helpers.GetSettingVisibleFields(null, typeof(VisitModel), Constants.Settings.VisitListFieldsIgnored))
            };
            try
            {
                _dbContext.AccessSetting.Add(setting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateDefaultAccessSetting");
            }
        }

        public AccessSetting GetAccessSetting(int companyId)
        {
            return _dbContext.AccessSetting.FirstOrDefault(m => m.CompanyId == companyId);
        }

        public void UpdateAccessSetting(AccessSetting setting)
        {
            try
            {
                _dbContext.AccessSetting.Update(setting);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateAccessSetting");
            }
        }

        public User GetByAccountId(int accountId, int companyId)
        {
            return _dbContext.User.FirstOrDefault(m => !m.IsDeleted && m.AccountId == accountId && m.CompanyId == companyId);
        }

        public User GetByNationalIdNumber(string idNumber, int companyId)
        {
            return _dbContext.User.FirstOrDefault(m => m.NationalIdNumber == idNumber && m.CompanyId == companyId && !m.IsDeleted);
        }


        /// <summary>
        /// Get user by access groups
        /// </summary>
        /// <param name="companyId"> company identifier </param>
        /// <param name="deviceIds"> list of device </param>
        /// <returns></returns>
        public IQueryable<User> GetAssignUsersByDeviceId(int companyId, List<int> deviceIds)
        {
            var agdData = _dbContext.AccessGroupDevice
                .Include(x => x.AccessGroup)
                .Include(x => x.Icu)
                //.Include(x => x.Tz)
                .Where(x => deviceIds.Contains(x.IcuId)
                        && !x.AccessGroup.IsDeleted
                        && !x.Icu.IsDeleted);

            if (companyId != 0)
                agdData = agdData.Where(x => x.AccessGroup.CompanyId == companyId);

            var accessGroupIds = new HashSet<int>(agdData.Select(x => x.AccessGroupId).Distinct().AsEnumerable<int>());
            var childAccessGroupIds = new HashSet<int>(_dbContext.AccessGroup
                .Where(m => m.ParentId != null && accessGroupIds.Contains(m.ParentId.Value))
                .Select(m => m.Id).AsEnumerable<int>());

            if (childAccessGroupIds.Any())
                accessGroupIds.UnionWith(childAccessGroupIds);

            //var device = _dbContext.IcuDevice.FirstOrDefault(d => deviceIds.Contains(d.Id));
            var buildingIds = _dbContext.IcuDevice.Where(d => deviceIds.Contains(d.Id) && d.BuildingId != null).Select(d => d.BuildingId.Value).ToList();

            var data = _dbContext.User
                .Include(u => u.Department)
                .Include(u => u.AccessGroup)
                .Where(u => (accessGroupIds.Any(a => a == u.AccessGroupId) && !u.IsDeleted && u.Status == (short)Status.Valid));

            if (companyId != 0)
            {
                data = data.Where(u => u.CompanyId == companyId);
            }

            return data;
        }


        /// <summary>
        /// Get unassigned user by access groups
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="accessGroupIds"></param>
        /// <returns></returns>
        public IQueryable<User> GetUnAssignUsersByDeviceId(int companyId, List<int> deviceIds)
        {
            var agdData = _dbContext.AccessGroupDevice
                .Include(x => x.AccessGroup)
                .Include(x => x.Icu)
                .Include(x => x.Tz)
                .Where(x => deviceIds.Contains(x.IcuId) && !x.AccessGroup.IsDeleted && !x.Icu.IsDeleted);

            if (companyId != 0)
                agdData = agdData.Where(x => x.AccessGroup.CompanyId == companyId);

            var accessGroupIds = agdData.Select(x => x.AccessGroupId).ToList();
            var childAccessGroupIds = _dbContext.AccessGroup
                .Where(m => m.ParentId != null && accessGroupIds.Contains(m.ParentId.Value))
                .Select(m => m.Id).ToList();

            if (childAccessGroupIds.Any())
                accessGroupIds.AddRange(childAccessGroupIds);

            var device = _dbContext.IcuDevice.FirstOrDefault(d => deviceIds.Contains(d.Id));

            var data = _dbContext.User
                .Include(m => m.Department)
                .Include(m => m.AccessGroup)
                .Where(u => !accessGroupIds.Any(a => a == u.AccessGroupId) && !u.IsDeleted && u.Status == (short)Status.Valid);

            if (companyId != 0)
            {
                data = data.Where(u => u.CompanyId == companyId);
            }

            return data;
        }

        public void AddNationalIdCardForUser(NationalIdCard nationalIdCard)
        {
            try
            {
                _dbContext.NationalIdCard.Add(nationalIdCard);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddNationalIdCardForUser");
            }
        }
    }
}