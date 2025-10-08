using System.Linq;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Department;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace DeMasterProCloud.Repository
{
    /// <summary>
    /// Department Repository
    /// </summary>
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        void AddDefault(Company company);
        void InitDepartment(DepartmentModel model);
        bool IsDepartmentNameExist(DepartmentModel model);
        bool IsDepartmentNumberExist(DepartmentModel model);
        Department GetByIdAndCompanyId(int? id, int companyId);
        List<Department> GetByIdsAndCompanyId(List<int> ids, int companyId);
        List<Department> GetByNamesAndCompanyId(List<string> names, int companyId);
        Department GetDefautDepartmentByCompanyId(int companyId);
        new void DeleteRange(IEnumerable<Department> departments);
        IEnumerable<Node> GetDepartmentHierarchy(int? id = null);
        IQueryable<Department> GetChildDepartment(int id);
        List<Department> GetByCompanyId(int companyId);
        IQueryable<Department> GetIQueryableByCompanyId(int companyId);
        Department GetByNameAndCompany(string departmentName, int companyId);
        Department GetByDepartmentCode(string departCode);
        Department GetByNumberAndCompany(string departmentNumber, int companyId);
        bool IsBelongToCompany(int departmentId, int companyId);
        List<int> GetDepartmentIdsByAccountDepartmentRole(int companyId, int accountId);
        List<Department> GetDepartmentsByAccountDepartmentRole(int companyId, int accountId);
        int GetCountByCompanyId(int companyId);
    }

    public class DepartmentRepository : GenericRepository<Department>, IDepartmentRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly HttpContext _httpContext;
        public DepartmentRepository(AppDbContext dbContext, IHttpContextAccessor contextAccessor) : base(dbContext, contextAccessor)
        {
            _dbContext = dbContext;
            _httpContext = contextAccessor?.HttpContext;
        }

        /// <summary>
        /// Add a default department by company
        /// </summary>
        /// <param name="company"></param>
        public void AddDefault(Company company)
        {
            var deparment = GetDefault(company.Id);
            if (deparment == null)
            {
                deparment = new Department
                {
                    CompanyId = company.Id,
                    DepartName = company.Name,
                    DepartNo = "1"
                };
                Add(deparment);
            }
        }

        /// <summary>
        /// Get default department
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Department GetDefault(int companyId)
        {
            return Get(c => c.CompanyId == companyId && c.DepartNo == "1");
        }

        /// <summary>
        /// Get department hieararchy
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<Node> GetDepartmentHierarchy(int? id = null)
        {
            int companyId = _httpContext.User.GetCompanyId();
            var departments = _dbContext.Department
                .Where(m => m.CompanyId == companyId && !m.IsDeleted).OrderBy(m => m.ParentId).ToList();

            if (id != null)
            {
                Remove(departments, id.Value);
            }

            var nodeItems = departments.Select(
                m => new Node
                {
                    Id = m.Id,
                    Text = m.DepartName,
                    ParentId = m.ParentId
                });
            var nodes = nodeItems.BuildTree();
            return nodes;
        }

        /// <summary>
        /// Get child department
        /// </summary>
        /// <param name="id">parent department identifier </param>
        /// <returns></returns>
        public IQueryable<Department> GetChildDepartment(int id)
        {
            var departments = _dbContext.Department
                .Where(m => m.ParentId == id && !m.IsDeleted).AsNoTracking();

            return departments;
        }

        /// <summary>
        /// Init department model
        /// </summary>
        /// <param name="model"></param>
        public void InitDepartment(DepartmentModel model)
        {
            var companyId = _httpContext.User.GetCompanyId();
            var departments = _dbContext.Department
                .Where(m => m.CompanyId == companyId && !m.IsDeleted).OrderBy(m => m.ParentId).ToList();
            Remove(departments, model.Id);
            //model.ParentDepartments = departments.Select(m => new SelectListItem
            //{
            //    Text = m.DepartName,
            //    Value = m.Id.ToString()
            //});
        }

        /// <summary>
        /// Check if department name is exists
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool IsDepartmentNameExist(DepartmentModel model)
        {
            var companyId = _httpContext.User.GetCompanyId();
            return _dbContext.Department.Any(m =>
                m.DepartName == model.Name && m.CompanyId == companyId &&
                m.Id != model.Id && !m.IsDeleted);
        }

        /// <summary>
        /// Check if department number is exists
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool IsDepartmentNumberExist(DepartmentModel model)
        {
            var companyId = _httpContext.User.GetCompanyId();
            return _dbContext.Department.Any(m => m.DepartNo == model.Number && m.CompanyId == companyId &&
                m.Id != model.Id && !m.IsDeleted);
        }

        /// <summary>
        /// Get by id and company
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Department GetByIdAndCompanyId(int? id, int companyId)
        {
            return _dbContext.Department.FirstOrDefault(m => m.Id == id && m.CompanyId == companyId && !m.IsDeleted);
        }

        /// <summary>
        /// Get by ids and company
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<Department> GetByIdsAndCompanyId(List<int> ids, int companyId)
        {
            return GetMany(m => ids.Contains(m.Id) && m.CompanyId == companyId && !m.IsDeleted).ToList();
        }

        /// <summary>   Gets by names and company identifier. </summary>
        /// <remarks>   Edward, 2020-03-03. </remarks>
        /// <param name="names">        The names. </param>
        /// <param name="companyId">    . </param>
        /// <returns>   The by names and company identifier. </returns>
        public List<Department> GetByNamesAndCompanyId(List<string> names, int companyId)
        {
            return GetMany(m => names.Contains(m.DepartName) && m.CompanyId == companyId && !m.IsDeleted).ToList();
        }

        /// <summary>
        /// Get default department by company
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Department GetDefautDepartmentByCompanyId(int companyId)
        {
            return GetByCompanyId(companyId).OrderBy(c => c.Id).FirstOrDefault();

            //return Get(m =>
            //    m.CompanyId == companyId && m.ParentId == null && !m.IsDeleted && m.Id == Constants.DefaultDepartmentId);
        }

        /// <summary>
        /// Get by companyId
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<Department> GetByCompanyId(int companyId)
        {
            return GetMany(m => m.CompanyId == companyId && !m.IsDeleted).ToList();
        }

        /// <summary>
        /// Get by department code
        /// </summary>
        /// <param name="departCode"></param>
        /// <returns></returns>
        public Department GetByDepartmentCode(string departCode)
        {
            return Get(x => x.DepartNo == departCode && x.CompanyId == _httpContext.User.GetCompanyId() && !x.IsDeleted);
        }

        /// <summary>
        /// Return list ids of departments for account type enable department dynamic role
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public List<int> GetDepartmentIdsByAccountDepartmentRole(int companyId, int accountId)
        {
            // get department ids enable level department permission
            var departmentIds = from c in _dbContext.CompanyAccount
                join r in _dbContext.DynamicRole on c.DynamicRoleId equals r.Id
                join u in _dbContext.User on c.AccountId equals u.AccountId
                join d in _dbContext.Department on u.DepartmentId equals d.Id
                where c.CompanyId == companyId & c.AccountId == accountId
                    & !r.IsDeleted & r.EnableDepartmentLevel & r.CompanyId == companyId
                    & !u.IsDeleted & u.CompanyId == companyId
                    & !d.IsDeleted & d.CompanyId == companyId
                select d.Id;
            var ids = departmentIds.ToList();
            
            // get department children
            var parentIds = ids;
            do
            {
                if(!parentIds.Any())
                    break;

                var ids1 = parentIds;
                var childrenIds = _dbContext.Department.Where(d => d.ParentId.HasValue && ids1.Contains(d.ParentId.Value) && !d.IsDeleted).Select(d => d.Id);
                parentIds = childrenIds.ToList();
                ids.AddRange(parentIds);
            } while (parentIds.Any());
            
            return ids.ToList();
        }

        /// <summary>
        /// Return list of departments for account type enable department dynamic role
        /// </summary>
        /// <param name="companyId"></param>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public List<Department> GetDepartmentsByAccountDepartmentRole(int companyId, int accountId)
        {
            // get department enable level department permission
            var departmentIds = from c in _dbContext.CompanyAccount
                join r in _dbContext.DynamicRole on c.DynamicRoleId equals r.Id
                join u in _dbContext.User on c.AccountId equals u.AccountId
                join d in _dbContext.Department on u.DepartmentId equals d.Id
                where c.CompanyId == companyId & c.AccountId == accountId
                    & !r.IsDeleted & r.EnableDepartmentLevel & r.CompanyId == companyId
                    & !u.IsDeleted & u.CompanyId == companyId
                    & !d.IsDeleted & d.CompanyId == companyId
                select d;
            var departments = departmentIds.ToList();
            
            // get department children
            var parentIds = departments.Select(d => d.Id).ToList();
            do
            {
                if(!parentIds.Any())
                    break;

                var ids = parentIds;
                var childrenIds = _dbContext.Department.Where(d => ids.Contains(d.ParentId.Value));
                parentIds = childrenIds.Select(x => x.Id).ToList();
                departments.AddRange(childrenIds);
            } while (parentIds.Any());
            
            return departments.ToList();
        }

        public int GetCountByCompanyId(int companyId)
        {
            return _dbContext.Department.Count(m => m.CompanyId == companyId && !m.IsDeleted);
        }
        
        #region Helpers
        /// <summary>
        /// Remove a list of department
        /// </summary>
        /// <param name="items"></param>
        /// <param name="id"></param>
        private void Remove(List<Department> items, int id)
        {
            var department = items.FirstOrDefault(m => m.Id == id);
            items.Remove(department);
            var childItems = items.Where(m => m.ParentId == id).ToList();
            if (childItems.Any())
            {
                foreach (var childItem in childItems)
                {
                    items.Remove(childItem);
                    Remove(items, childItem.Id);
                }
            }
        }

        /// <summary>
        /// Get department by companyId
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns> IQueryable<Department> </Department></returns>
        public IQueryable<Department> GetIQueryableByCompanyId(int companyId)
        {
            return _dbContext.Department.Where(m => m.CompanyId == companyId && !m.IsDeleted);
        }

        /// <summary>
        /// Get department by DepartName, companyId
        /// </summary>
        /// <param name="departmentName"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Department GetByNameAndCompany(string departmentName, int companyId)
        {
            return Get(m =>
                m.CompanyId == companyId 
                && m.DepartName == departmentName 
                && !m.IsDeleted);
        }

        /// <summary>
        /// Get department by DepartNo, CompanyId
        /// </summary>
        /// <param name="departmentNumber"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Department GetByNumberAndCompany(string departmentNumber, int companyId)
        {
            return Get(m =>
                m.CompanyId == companyId
                && m.DepartNo.ToLower().Equals(departmentNumber.ToLower())
                && !m.IsDeleted);
        }

        public bool IsBelongToCompany(int departmentId, int companyId)
        {
            var department = Get(m =>
                m.CompanyId == companyId
                && m.Id == departmentId
                && !m.IsDeleted);
            if (department != null){
                return true;
            } else
            {
                return false;
            }
        }

        #endregion
    }
}
