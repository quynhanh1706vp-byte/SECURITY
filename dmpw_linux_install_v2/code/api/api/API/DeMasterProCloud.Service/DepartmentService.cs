using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Infrastructure.Exceptions;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Department;
using DeMasterProCloud.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.DataModel.DepartmentDevice;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Node = DeMasterProCloud.DataModel.Department.Node;
using DeMasterProCloud.DataModel.PlugIn;
using Bogus;
using Bogus.Extensions;

namespace DeMasterProCloud.Service
{
    /// <summary>
    /// Department service interface
    /// </summary>
    public interface IDepartmentService
    {
        void InitDepartment(DepartmentDataModel depModel);

        bool IsDepartmentNumberExist(DepartmentModel depModel);

        int Add(DepartmentModel depModel);

        void Update(DepartmentModel model);

        void Delete(Department model);

        void DeleteRange(List<Department> departments);

        Department GetByIdAndCompany(int? id, int companyId);

        List<Department> GetByIdsAndCompany(List<int> ids, int companyId);

        IEnumerable<Node> GetDeparmentHierarchy(int? id = null);
        List<Department> GetListParentDepartment(int companyId);

        bool IsDepartmentNameExist(DepartmentModel model);

        bool IsUserExist(int departmentId);

        List<Department> GetDescendantsOrself(int id, int companyId);

        Department GetDefaultDepartment(int companyId);

        List<Department> GetByCompanyId(int companyId);

        bool ImportFile(string type, IFormFile file, out int total, out int fail);

        byte[] Export(string type, string filter, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered);

        List<DepartmentListModel> FilterDataWithOrder(string filter, string sortColumn, string sortDirection,
            out int totalRecords,
            out int recordsFiltered);

        Account GetAccount(int accountId);
        CompanyAccount GetCompanyAccount(int accountId, int companyId);

        int GetUserCount(int companyId, int departmentId);
        List<DepartmentListModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);
        Department GetById(int id);
        List<DepartmentListItemModel> ListDepartmentParent(string filter, string sortColumn, string sortDirection,
         out int totalRecords, out int recordsFiltered);
        List<DepartmentListItemModel> GetPaginatedListMenu(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);
        List<DepartmentListItemModel> GetListDepartment(DepartmentFilterModel filter, out int recordsTotal, out int recordsFiltered);
        bool CheckEditDepartment(DepartmentModel model);
        // List<int> GetDepartmentIdsByAccountDepartmentRole();

        Department GetByNumberAndCompany(string number, int companyId);

        string AssignUsers(int departmentId, List<int> userIds);

        string UnAssignUsers(List<int> userIds);
        int GetNewAutoDepartmentNumber(int companyId);
        
        // Department Level
        List<DepartmentDeviceModel> GetDepartmentDevices(int departmentId, string search,
            int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);
        List<DepartmentDeviceModel> GetDepartmentDevicesUnassign(int departmentId, string search,
            int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered);

        bool AssignDepartmentDevice(int id, List<int> doorIds);
        bool UnAssignDepartmentDevice(int id, List<int> doorIds);
        bool CheckDoorInDepartment(int companyId, int accountId, List<int> doors);
        bool CheckPermissionDepartmentLevel(int parentId);
        void GenerateTestData(int numberOfDept);
    }

    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpContext _httpContext;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        /// <summary>
        /// String array display in header sheet when export file
        /// </summary>
        private readonly string[] _header =
        {
            DepartmentResource.lblDepartmentNumber,
            DepartmentResource.lblDepartmentName,
            DepartmentResource.lblDepartmentUserCount,
            DepartmentResource.lblDepartmentManager,
            DepartmentResource.lblMaxPercentCheckout,
            DepartmentResource.lblMaxNumberCheckout
        };

        public DepartmentService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILogger<DepartmentService> logger)
        {
            _unitOfWork = unitOfWork;
            _httpContext = httpContextAccessor.HttpContext;
            _logger = logger;
            _mapper = MapperInstance.Mapper;
        }

        /// <summary>
        /// Initital data
        /// </summary>
        /// <param name="depModel"></param>
        public void InitDepartment(DepartmentDataModel depModel)
        {
            try
            {
                // What is the purpose of the source code right below here.
                _unitOfWork.DepartmentRepository.InitDepartment(depModel);

                if(depModel.DepartmentManagerId != null)
                {
                    // get account by Id.
                    var managerUser = _unitOfWork.UserRepository.GetByAccountId(depModel.DepartmentManagerId ?? 0, _httpContext.User.GetCompanyId());

                    depModel.DepartmentManagerId = managerUser?.Id;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in InitDepartment");
            }
        }

        /// <summary>
        /// Add department
        /// </summary>
        /// <param name="depModel"></param>
        public int Add(DepartmentModel depModel)
        {
            var departmentId = 0;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var department = _mapper.Map<Department>(depModel);
                        department.CompanyId = _httpContext.User.GetCompanyId();
                        department.ParentId = (department.ParentId == null || department.ParentId == 0) ? null : department.ParentId;
                        _unitOfWork.DepartmentRepository.Add(department);
                        _unitOfWork.Save();

                        //Save system log
                        var content = $"{DepartmentResource.lblAddNew}";
                        var contentsDetails = $"{DepartmentResource.lblDepartmentNumber} : {department.DepartNo}<br />" +
                                            $"\n{DepartmentResource.lblDepartmentName} : {department.DepartName}";

                        if(depModel.DepartmentManagerId != null)
                        {
                            var manager = _unitOfWork.AccountRepository.GetById( depModel.DepartmentManagerId.Value);
                            if(manager != null)
                            {
                                contentsDetails += $"\n{DepartmentResource.lblDepartmentManager} : {manager.User.FirstOrDefault()?.FirstName}";
                            }
                        }

                        if (depModel.ParentId != null)
                        {
                            var parentDept = _unitOfWork.DepartmentRepository.GetById(depModel.ParentId.Value);
                            if (parentDept != null)
                            {
                                contentsDetails += $"\n{DepartmentResource.lblParentDepartment} : {parentDept.DepartName}";
                            }
                        }

                        if (depModel.AccessGroupId != null)
                        {
                            var defaultAG = _unitOfWork.AccessGroupRepository.GetById(depModel.AccessGroupId.Value);
                            if (defaultAG != null)
                            {
                                contentsDetails += $"\n{AccessGroupResource.lblDefaultAccessGroup} : {defaultAG.Name}";
                            }
                        }

                        _unitOfWork.SystemLogRepository.Add(department.Id, SystemLogType.Department, ActionLogType.Add,
                            content, contentsDetails, null, department.CompanyId);

                        _unitOfWork.Save();
                        transaction.Commit();
                        departmentId = department.Id;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
            return departmentId;
        }

        /// <summary>
        /// Update department
        /// </summary>
        /// <param name="model"></param>
        public void Update(DepartmentModel model)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        //var department = _unitOfWork.DepartmentRepository.GetByIdAndCompanyId(model.Id, _httpContext.User.GetCompanyId());
                        //var department = _unitOfWork.DepartmentRepository.GetById(model.Id);
                        var department = _unitOfWork.AppDbContext.Department.Include(d => d.Parent).FirstOrDefault(d => d.Id == model.Id);
                        var currentDepartmentName = department.DepartName;
                        var currentDepartmentManagerId = department.DepartmentManagerId;
                        var currentParentDepartmentId = department.ParentId;
                        var currentParentDepartmentName = department.Parent?.DepartName;

                        List<string> changes = new List<string>();

                        _mapper.Map(model, department);

                        if (currentDepartmentName != model.Name)
                        {
                            changes.Add(Helpers.CreateChangedValueContents(DepartmentResource.lblDepartmentName, currentDepartmentName, model.Name));
                        }

                        if(currentParentDepartmentId != model.ParentId)
                        {
                            string newParentName = "";
                            if(model.ParentId != null)
                                newParentName = _unitOfWork.DepartmentRepository.GetById(model.ParentId.Value)?.DepartName;

                            changes.Add(Helpers.CreateChangedValueContents(DepartmentResource.lblParentDepartment, currentParentDepartmentName, newParentName));
                        }

                        _unitOfWork.DepartmentRepository.Update(department);

                        if (currentDepartmentManagerId != model.DepartmentManagerId)
                        {
                            var oldManagerName = "";
                            var newManagerName = "";

                            if (currentDepartmentManagerId != null)
                            {
                                var oldManager = _unitOfWork.AccountRepository.GetById(currentDepartmentManagerId.Value);
                                var oldManaterUser = _unitOfWork.UserRepository.GetUserByAccountId(oldManager.Id, oldManager.CompanyId.Value);

                                oldManagerName = $"{oldManager.Username}";

                                if (oldManaterUser != null)
                                {
                                    oldManagerName = $"{oldManaterUser.FirstName}({oldManager.Username})";
                                }
                            }

                            if (model.DepartmentManagerId != null)
                            {
                                var newManager = _unitOfWork.AccountRepository.GetById(model.DepartmentManagerId.Value);
                                var newManaterUser = _unitOfWork.UserRepository.GetUserByAccountId(newManager.Id, newManager.CompanyId.Value);

                                newManagerName = $"{newManager.Username}";

                                if (newManaterUser != null)
                                {
                                    newManagerName = $"{newManaterUser?.FirstName}({newManager.Username})";
                                }
                            }

                            var visitSetting = _unitOfWork.VisitRepository.GetVisitSetting(_httpContext.User.GetCompanyId());

                            if (visitSetting != null && visitSetting.ApprovalStepNumber > 0)
                            {
                                var firstApprovalAccounts = JsonConvert.DeserializeObject<List<int>>(visitSetting.FirstApproverAccounts);

                                if (currentDepartmentManagerId != null)
                                {
                                    //// Remove old department manager.
                                    //if (firstApprovalAccounts.Contains(currentDepartmentManagerId.Value))
                                    //{
                                    //    firstApprovalAccounts.Remove(currentDepartmentManagerId.Value);
                                    //}
                                }

                                if (model.DepartmentManagerId != null)
                                {
                                    // Add new department manager.
                                    if (!firstApprovalAccounts.Contains(model.DepartmentManagerId.Value))
                                    {
                                        firstApprovalAccounts.Add(model.DepartmentManagerId.Value);
                                    }
                                }

                                visitSetting.FirstApproverAccounts = JsonConvert.SerializeObject(firstApprovalAccounts);
                                // Update visit setting.
                                _unitOfWork.VisitRepository.UpdateVisitSetting(visitSetting);
                            }

                            changes.Add(Helpers.CreateChangedValueContents(DepartmentResource.lblDepartmentManager, oldManagerName, newManagerName));
                        }

                        if (changes.Any())
                        {
                            var content = $"{DepartmentResource.lblUpdate} ({currentDepartmentName})";
                            var contentsDetails = string.Join("\n", changes);

                            _unitOfWork.SystemLogRepository.Add(department.Id, SystemLogType.Department, ActionLogType.Update,
                                content, contentsDetails, null, _httpContext.User.GetCompanyId());
                        }

                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        ex.ToString();
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Delete a department
        /// </summary>
        /// <param name="department"></param>
        public void Delete(Department department)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // Check if there are child departments of this department.
                        var childDepts = _unitOfWork.DepartmentRepository.GetChildDepartment(department.Id);
                        // If there are child department, update their parentId. (Step 1 up)
                        if (childDepts.Any())
                        {
                            foreach (var childDept in childDepts)
                            {
                                childDept.ParentId = department.ParentId;
                                _unitOfWork.DepartmentRepository.Update(childDept);
                            }
                        }

                        // Delete department data from system
                        _unitOfWork.DepartmentRepository.DeleteFromSystem(department);

                        //Save system log
                        var content =
                            $"{ActionLogTypeResource.Delete} : {department.DepartName} ({DepartmentResource.lblDepartmentName})";

                        _unitOfWork.SystemLogRepository.Add(department.Id, SystemLogType.Department,
                            ActionLogType.Delete, content, null, null, department.CompanyId);

                        //Save to database
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
        }

        /// <summary>
        /// Delete a list of department
        /// </summary>
        /// <param name="departments"></param>
        public void DeleteRange(List<Department> departments)
        {
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        // Delete departments data from system
                        _unitOfWork.DepartmentRepository.DeleteRangeFromSystem(departments);
                        _unitOfWork.Save();

                        // Update child departments.
                        foreach (var department in departments)
                        {
                            // Check if there are child departments of this department.
                            var childDepts = _unitOfWork.DepartmentRepository.GetChildDepartment(department.Id);
                            // If there are child department, update their parentId. (Step 1 up)
                            if (childDepts.Any())
                            {
                                if (department.ParentId == null)
                                {
                                    // There is no parent department of this deleted department.
                                    foreach (var childDept in childDepts)
                                    {
                                        childDept.ParentId = department.ParentId;
                                        _unitOfWork.DepartmentRepository.Update(childDept);
                                    }
                                }
                                else
                                {
                                    // Get deleted department's parent
                                    var parentDept = _unitOfWork.AppDbContext.Department.FirstOrDefault(m => m.Id == department.ParentId);
                                    while (parentDept.ParentId != null && parentDept.IsDeleted)
                                    {
                                        parentDept = _unitOfWork.AppDbContext.Department.FirstOrDefault(m => m.Id == parentDept.ParentId);
                                    }

                                    foreach (var childDept in childDepts)
                                    {
                                        childDept.ParentId = parentDept.Id;
                                        _unitOfWork.DepartmentRepository.Update(childDept);
                                    }
                                }
                            }
                        }

                        //Save system log
                        if (departments.Count == 1)
                        {
                            var department = departments.First();
                            var content =
                                $"{ActionLogTypeResource.Delete}: {department.DepartName} ({DepartmentResource.lblDepartmentName})";

                            _unitOfWork.SystemLogRepository.Add(department.Id, SystemLogType.Department, ActionLogType.Delete,
                                content, null, null, department.CompanyId);
                        }
                        else
                        {
                            var department = departments.First();
                            var content = DepartmentResource.lblDelete;
                            var departmentIds = departments.Select(c => c.Id).ToList();
                            var departmentNames = departments.Select(c => c.DepartName).ToList();
                            var contentDetails = $"{DepartmentResource.lblDepartmentName} : {string.Join(", ", departmentNames)}";

                            _unitOfWork.SystemLogRepository.Add(department.Id, SystemLogType.Department, ActionLogType.DeleteMultiple,
                                content, contentDetails, departmentIds, department.CompanyId);
                        }
                        _unitOfWork.Save();
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            });
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
        public List<DepartmentListModel> GetPaginated(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var data = FilterDataWithOrder(filter, sortColumn, sortDirection, out totalRecords, out recordsFiltered);


                var result = pageNumber <= 0 ? data : data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginated");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<DepartmentListModel>();
            }
        }

        public List<DepartmentListItemModel> GetPaginatedListMenu(string filter, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var data = ListDepartmentParent(filter, sortColumn, sortDirection, out totalRecords, out recordsFiltered);
                if (data == null)
                {
                    totalRecords = 0;
                    recordsFiltered = 0;
                    return new List<DepartmentListItemModel>();
                }

                var result = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPaginatedListMenu");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<DepartmentListItemModel>();
            }
        }

        /// <summary>
        /// Check if department number is exist
        /// </summary>
        /// <param name="depModel"></param>
        /// <returns></returns>
        public bool IsDepartmentNumberExist(DepartmentModel depModel)
        {
            try
            {
                return _unitOfWork.DepartmentRepository.IsDepartmentNumberExist(depModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsDepartmentNumberExist");
                return false;
            }
        }

        /// <summary>
        /// Get department by id and company
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Department GetByIdAndCompany(int? id, int companyId)
        {
            try
            {
                return _unitOfWork.DepartmentRepository.GetByIdAndCompanyId(id, companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdAndCompany");
                return null;
            }
        }

        /// <summary>
        /// Get department by number and company
        /// </summary>
        /// <param name="number"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public Department GetByNumberAndCompany(string number, int companyId)
        {
            try
            {
                return _unitOfWork.DepartmentRepository.GetByNumberAndCompany(number, companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByNumberAndCompany");
                return null;
            }
        }

        /// <summary>
        /// Get a list department by list id and company
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<Department> GetByIdsAndCompany(List<int> ids, int companyId)
        {
            try
            {
                return _unitOfWork.DepartmentRepository.GetByIdsAndCompanyId(ids, companyId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByIdsAndCompany");
                return new List<Department>();
            }
        }

        /// <summary>
        /// Get list descendant or self of deparment
        /// </summary>
        /// <param name="id"></param>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public List<Department> GetDescendantsOrself(int id, int companyId)
        {
            try
            {
                var departments = _unitOfWork.DepartmentRepository.GetMany(m =>
                    !m.IsDeleted && m.CompanyId == companyId).ToList();
                if (departments.Any())
                {
                    return GetSelfOrChild(id, departments).ToList();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDescendantsOrself");
                return null;
            }
        }

        /// <summary>
        /// Get default department for user
        /// </summary>
        /// <returns></returns>
        public Department GetDefaultDepartment(int companyId)
        {
            try
            {
                var department = _unitOfWork.DepartmentRepository.GetByCompanyId(companyId).OrderBy(c => c.Id).FirstOrDefault();

                return department;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDefaultDepartment");
                return null;
            }
        }

        /// <summary>
        /// Get default department for user
        /// </summary>
        /// <returns></returns>
        public List<Department> GetByCompanyId(int companyId)
        {
            try
            {
                var departments = _unitOfWork.DepartmentRepository.GetMany(m =>
                    !m.IsDeleted && m.CompanyId == companyId).ToList();
                return departments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByCompanyId");
                return new List<Department>();
            }
        }

        /// <summary>
        /// Check user presence
        /// </summary>
        /// <returns></returns>
        public bool IsUserExist(int departmentId)
        {
            try
            {
                var existUsers = _unitOfWork.UserRepository.GetByDepartmentId(departmentId);

                if (existUsers.Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsUserExist");
                return false;
            }
        }

        public List<Department> GetListParentDepartment(int companyId)
        {
            try
            {
                var departments = _unitOfWork.DepartmentRepository.GetByCompanyId(companyId);
                departments = departments.Where(x => x.ParentId == null).ToList();
                return departments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetListParentDepartment");
                return new List<Department>();
            }
        }

        /// <summary>
        /// Check if department name is exist
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool IsDepartmentNameExist(DepartmentModel model)
        {
            try
            {
                return _unitOfWork.DepartmentRepository.IsDepartmentNameExist(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IsDepartmentNameExist");
                return false;
            }
        }

        /// <summary>
        /// Get hierachy departments
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<Node> GetDeparmentHierarchy(int? id = null)
        {
            try
            {
                return _unitOfWork.DepartmentRepository.GetDepartmentHierarchy(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDeparmentHierarchy");
                return Enumerable.Empty<Node>();
            }
        }

        #region Helpers

        /// <summary>
        /// Get self or child of department
        /// </summary>public const string ApiDepartmentsImport = "/departments/import";
        /// <param name="id"></param>
        /// <param name="departments"></param>
        /// <returns></returns>
        private IEnumerable<Department> GetSelfOrChild(int id, List<Department> departments)
        {
            yield return departments.FirstOrDefault(m => m.Id == id);
            foreach (var department in departments.Where(m => m.ParentId == id))
            {
                foreach (var child in GetSelfOrChild(department.Id, departments))
                {
                    yield return child;
                }
            }
        }

        public void Import()
        {
        }

        /// <summary>
        /// Export department data to excel or txt file
        /// </summary>
        /// <param name="type"></param>
        /// <param name="filter"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns> byte[] fileByte </returns>
        public byte[] Export(string type, string filter, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered)
        {
            try
            {
                var fileByte = type == Constants.Excel
                    ? ExportExcel(filter, sortColumn, sortDirection, out totalRecords,
                        out recordsFiltered)
                    : ExportTxt(filter, sortColumn, sortDirection, out totalRecords,
                        out recordsFiltered);

                //Save system log
                var companyId = _httpContext.User.GetCompanyId();

                var content = DepartmentResource.msgExportDepartmentList;
                var contentsDetails = $"{AccountResource.lblUsername} : {_httpContext.User.GetUsername()}";

                _unitOfWork.SystemLogRepository.Add(1, SystemLogType.Department, ActionLogType.Export, content, contentsDetails, null, _httpContext.User.GetCompanyId());
                _unitOfWork.Save();

                return fileByte;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Export");
                totalRecords = 0;
                recordsFiltered = 0;
                return new byte[0];
            }
        }

        /// <summary>
        /// Export department data to excel file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public byte[] ExportExcel(string filter, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered)
        {
            try
            {
                byte[] result;
                using (var package = new ExcelPackage())
                {
                    // add a new worksheet to the empty workbook
                    var worksheet =
                        package.Workbook.Worksheets.Add(DepartmentResource.lblDepartment); //Worksheet name

                    var departments = FilterDataWithOrder(filter, sortColumn, sortDirection, out totalRecords,
                            out recordsFiltered)
                        .ToList();

                //First add the headers for user sheet
                for (var i = 0; i < _header.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = _header[i];
                }
                var recordIndex = 2;

                foreach (var department in departments)
                {
                    //For the Department sheet
                    var colIndex = 1;
                    worksheet.Cells[recordIndex, colIndex++].Value = department.DepartmentNumber;
                    worksheet.Cells[recordIndex, colIndex++].Value = department.DepartmentName;
                    worksheet.Cells[recordIndex, colIndex++].Value = department.NumberUser;
                    worksheet.Cells[recordIndex, colIndex].Value = department.DepartmentManager;
                    worksheet.Cells[recordIndex, colIndex++].Value = department.MaxPercentCheckout;
                    worksheet.Cells[recordIndex, colIndex].Value = department.MaxNumberCheckout;
                    recordIndex++;
                }

                    result = package.GetAsByteArray();
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExportExcel");
                totalRecords = 0;
                recordsFiltered = 0;
                return new byte[0];
            }
        }

        /// <summary>
        /// Export department data to txt file
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public byte[] ExportTxt(string filter, string sortColumn, string sortDirection, out int totalRecords,
            out int recordsFiltered)
        {
            try
            {
                var departments = FilterDataWithOrder(filter, sortColumn, sortDirection, out totalRecords, out recordsFiltered)
                    .Select(x => new object[]
                    {
                        x.DepartmentNumber,
                        x.DepartmentName,
                        x.NumberUser,
                        x.DepartmentManager,
                        x.MaxPercentCheckout,
                        x.MaxNumberCheckout
                    }).ToList();

                // Build the file content
                var departmentTxt = new StringBuilder();
                departments.ForEach(line =>
                {
                    departmentTxt.AppendLine(string.Join(",", line));
                });

                byte[] buffer = Encoding.UTF8.GetBytes($"{string.Join(",", _header)}\r\n{departmentTxt}");
                return buffer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExportTxt");
                totalRecords = 0;
                recordsFiltered = 0;
                return new byte[0];
            }
        }

        /// <summary>
        /// Filter and order data
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <param name="totalRecords"></param>
        /// <param name="recordsFiltered"></param>
        /// <returns></returns>
        public List<DepartmentListModel> FilterDataWithOrder(string filter, string sortColumn, string sortDirection,
            out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();

                // check permission account type dynamic role department level
                var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(_httpContext.User.GetCompanyId());
                PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
                List<int> departmentIds = new List<int>();
                if (_httpContext.User.GetAccountType() == (short)AccountType.DynamicRole && plugIns.DepartmentAccessLevel)
                {
                    departmentIds = _unitOfWork.DepartmentRepository
                        .GetDepartmentIdsByAccountDepartmentRole(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId());
                }

                var data = _unitOfWork.AppDbContext.Department.Include(m => m.Parent).Include(m => m.DepartmentManager)
                    .Where(m => !m.IsDeleted && m.CompanyId == companyId && (!departmentIds.Any() || departmentIds.Contains(m.Id)));

                totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.Where(x =>
                    x.DepartNo.RemoveDiacritics().ToLower().Contains(filter) ||
                    x.DepartName.RemoveDiacritics().ToLower().Contains(filter) ||
                    (x.Parent != null && x.Parent.DepartName.RemoveDiacritics().ToLower().Contains(filter)));
            }

            recordsFiltered = data.Count();

            var result = data.AsEnumerable()
                .Select(m =>
                {
                    DepartmentListModel listModel = new DepartmentListModel
                    {
                        Id = m.Id,
                        DepartmentNumber = m.DepartNo,
                        DepartmentName = m.DepartName,
                        NumberUser = GetUserCount(companyId, m.Id),
                        DepartmentManagerId = m.DepartmentManagerId?.ToString(),
                        DepartmentManager = GetUserNameByEmail(companyId, m.DepartmentManager?.Username),
                        MaxNumberCheckout = m.MaxNumberCheckout,
                        MaxPercentCheckout = m.MaxPercentCheckout
                    };
                    return listModel;
                });

            var departmentListModels = result as DepartmentListModel[] ?? result.ToArray();

            if (sortDirection.Equals("desc"))
            {
                result = departmentListModels.OrderByDescending(c => sortColumn);
            }
            else if (sortDirection.Equals("asc"))
            {
                result = departmentListModels.OrderBy(c => sortColumn);
            }

                return departmentListModels.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in FilterDataWithOrder");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<DepartmentListModel>();
            }
        }



        public List<DepartmentListItemModel> ListDepartmentParent(string filter, string sortColumn, string sortDirection,
         out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();

            var data = _unitOfWork.AppDbContext.Department
                .Include(m => m.Parent)
                .Include(m => m.DepartmentManager)
                .Where(m => !m.IsDeleted && m.CompanyId == companyId);
            if (!data.Any())
            {
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<DepartmentListItemModel>();
            }

            totalRecords = data.Count();

            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.Trim().RemoveDiacritics().ToLower();
                data = data.Where(x =>
                    x.DepartNo.RemoveDiacritics().ToLower().Contains(filter) ||
                    x.DepartName.RemoveDiacritics().ToLower().Contains(filter) ||
                    (x.Parent != null && x.Parent.DepartName.RemoveDiacritics().ToLower().Contains(filter)));
            }

            recordsFiltered = data.Count();

            var result = data.AsEnumerable()
                .Select(m =>
                {
                    DepartmentListItemModel listModel = new DepartmentListItemModel
                    //(m.Id, m.DepartNo, m.DepartName, false, null, null, m.DepartmentManagerId?.ToString()
                    //    , GetUserNameByEmail(companyId, m.DepartmentManager?.Username), GetUserCount(companyId, m.Id), m.ParentId)
                    {
                        Id = m.Id,
                        DepartmentNumber = m.DepartNo?.ToString(),
                        IsRoot = false,
                        EditUrl = null,
                        DeleteUrl = null,
                        DepartmentManagerId = _unitOfWork.UserRepository.GetUserByAccountId(m.DepartmentManagerId ?? 0, companyId)?.Id.ToString() ?? m.DepartmentManagerId?.ToString(),
                        DepartmentManager = GetUserNameByEmail(companyId, m.DepartmentManager?.Username)?.ToString(),
                        DepartmentName = m.DepartName?.ToString(),
                        NumberUser = GetUserCount(companyId, m.Id),
                        ParentId = Convert.ToInt32(m.ParentId),
                        ParentDepartment = m.Parent == null ? "string" : m.Parent.DepartName?.ToString(),
                        MaxNumberCheckout = m.MaxNumberCheckout,
                        MaxPercentCheckout = m.MaxPercentCheckout
                    };
                    return listModel;
                });

            if (sortDirection.Equals("desc"))
            {
                switch (sortColumn)
                {
                    case "DepartmentName":
                        result = result.OrderByDescending(c => c.DepartmentName);
                        break;
                    case "DepartmentNumber":
                        result = result.OrderByDescending(c => c.DepartmentNumber);
                        break;
                    case "DepartmentManager":
                        result = result.OrderByDescending(c => c.DepartmentManager);
                        break;
                    case "NumberUser":
                        result = result.OrderByDescending(c => c.NumberUser);
                        break;
                    default:
                        result = result.OrderByDescending(c => c.DepartmentName);
                        break;
                }
            }
            else if (sortDirection.Equals("asc"))
            {
                switch (sortColumn)
                {
                    case "DepartmentName":
                        result = result.OrderBy(c => c.DepartmentName);
                        break;
                    case "DepartmentNumber":
                        result = result.OrderBy(c => c.DepartmentNumber);
                        break;
                    case "DepartmentManager":
                        result = result.OrderBy(c => c.DepartmentManager);
                        break;
                    case "NumberUser":
                        result = result.OrderBy(c => c.NumberUser);
                        break;
                    default:
                        result = result.OrderBy(c => c.DepartmentName);
                        break;
                }
            }

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ListDepartmentParent");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<DepartmentListItemModel>();
            }
        }

        public static List<DepartmentListItemModel> GenerateTree(List<DepartmentListItemModel> collection, DepartmentListItemModel rootItem)
        {
            List<DepartmentListItemModel> lst = new List<DepartmentListItemModel>();
            var listCollection = collection.Where(c => c.ParentId == rootItem.Id).ToList();
            foreach (DepartmentListItemModel c in listCollection)
            {
                lst.Add(new DepartmentListItemModel
                {
                    Id = c.Id,
                    ParentId = rootItem.Id,
                    Children = GenerateTree(collection, c),
                    ParentDepartment = rootItem.DepartmentName?.ToString(),
                    DepartmentManagerId = c.DepartmentManagerId?.ToString(),
                    DepartmentNumber = c.DepartmentNumber?.ToString(),
                    DepartmentName = c.DepartmentName?.ToString(),
                    NumberUser = c.NumberUser,
                    IsRoot = c.IsRoot,
                    EditUrl = c.EditUrl,
                    DeleteUrl = c.DeleteUrl,
                    DepartmentManager = c.DepartmentManager?.ToString(),
                    AccessGroupId = c.AccessGroupId,
                    AccessGroupName = c.AccessGroupName,
                    MaxNumberCheckout = c.MaxNumberCheckout,
                    MaxPercentCheckout = c.MaxPercentCheckout
                });
            }
            return lst.OrderBy(m => m.DepartmentName).ToList();
        }

        public bool CheckEditDepartment(DepartmentModel model)
        {
            if (model.ParentId == null)
            {
                model.ParentId = 0;
            }
            if (model.Id == model.ParentId)
                return false;
            var collection = _unitOfWork.AppDbContext.Department
                .Where(x => x.CompanyId == _httpContext.User.GetCompanyId())
                .Select(x => new DepartmentListItemModel
            {
                Id = x.Id,
                ParentId = Convert.ToInt32(x.ParentId)
            }).ToList();
            
            DepartmentListItemModel dep = new DepartmentListItemModel();
            dep.Id = model.Id;
            dep.DepartmentName = model.Name;
            dep.ParentId = Convert.ToInt32(model.ParentId);
            dep.DepartmentManagerId = model.DepartmentManagerId.ToString();
            
            var listData = GenerateDepartment(collection, dep);
            
            
            foreach (var item in listData)
            {
                if (model.Id == item.ParentId)
                    return false;
            }
            return true;
        }

        private List<DepartmentListItemModel> GenerateDepartment(List<DepartmentListItemModel> collection, DepartmentListItemModel rootItem)
        {

            var listCollection = collection.FirstOrDefault(c => c.Id == rootItem.ParentId);


            var listParent = ListParentId(rootItem.ParentId);

            if (listCollection == null)
                return listParent;

            listParent.AddRange(GenerateDepartment(collection, listCollection));


            return listParent;
        }

        private static List<DepartmentListItemModel> ListParentId(int parentId)
        {

            DepartmentListItemModel result = new DepartmentListItemModel();
            result.ParentId = parentId;
            return new List<DepartmentListItemModel>(){result};
        }


        public List<DepartmentListItemModel> GetListDepartment(DepartmentFilterModel filter, out int recordsTotal, out int recordsFiltered)
        {
            List<DepartmentListItemModel> departments = new List<DepartmentListItemModel>();

            var companyId = _httpContext.User.GetCompanyId();
            var accountId = _httpContext.User.GetAccountId();
            var accountType = _httpContext.User.GetAccountType();
            
            var dbDepartments = _unitOfWork.AppDbContext.Department
                .Include(m => m.Parent)
                .Include(m => m.DepartmentManager)
                .Where(d => !d.IsDeleted && d.CompanyId == companyId)
                .OrderBy($"{filter.SortColumn} {filter.SortDirection}").AsQueryable();
            
            recordsTotal = dbDepartments.Count();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                dbDepartments = dbDepartments.Where(m => m.DepartNo.RemoveDiacritics().ToLower().Contains(filter.Search.RemoveDiacritics().ToLower()) 
                                                         || m.DepartName.RemoveDiacritics().ToLower().Contains(filter.Search.RemoveDiacritics().ToLower()));
            }
            departments = dbDepartments.AsEnumerable().Select(m =>
            {
                DepartmentListItemModel listModel = new DepartmentListItemModel
                {
                    Id = m.Id,
                    DepartmentNumber = m.DepartNo?.ToString(),
                    IsRoot = false,
                    EditUrl = null,
                    DeleteUrl = null,
                    DepartmentManagerId = $"{m.DepartmentManagerId ?? 0}",
                    DepartmentManager = GetUserNameByEmail(companyId, m.DepartmentManager?.Username)?.ToString(),
                    DepartmentName = m.DepartName?.ToString(),
                    NumberUser = GetUserCount(companyId, m.Id),
                    ParentId = m.ParentId != null ? Convert.ToInt32(m.ParentId) : 0,
                    ParentDepartment = m.Parent == null ? "-" : m.Parent.DepartName?.ToString(),
                    MaxNumberCheckout = m.MaxNumberCheckout,
                    MaxPercentCheckout = m.MaxPercentCheckout
                };
                return listModel;
            }).ToList();
            
            // check account type dynamic role enable department role
            int departmentId = 0; // departmentId of user have dynamic role enable department level
            if (accountType == (short)AccountType.DynamicRole)
            {
                var departmentIds = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
                if (departmentIds.Any())
                {
                    departmentId = _unitOfWork.UserRepository.GetUserByAccountId(accountId, companyId).DepartmentId;
                    departments = departments.Where(x => departmentIds.Contains(x.Id)).ToList();
                }
            }

            // Convert DepartmanagerId (accountId -> UserId)
            foreach(var eachDept in departments)
            {
                // case department permission enable, get department tree with departmentId
                if (departmentId != 0 && eachDept.Id != departmentId) continue;

                // How to display the department manager name in department information page. (edit button)
                if (!string.IsNullOrEmpty(eachDept.DepartmentManagerId))
                {
                    var user = _unitOfWork.UserRepository.GetUserByAccountId(Int32.Parse(eachDept.DepartmentManagerId), companyId);

                    if (user != null)
                    {
                        eachDept.DepartmentManagerId = user.Id.ToString();
                    }
                }
            }

            List<DepartmentListItemModel> lst = new List<DepartmentListItemModel>();
            foreach (var c in departments)
            {
                // case department permission enable, get department tree with departmentId
                if (departmentId != 0 && c.Id != departmentId) break;

                //// How to display the department manager name in department information page. (edit button)
                //if (!string.IsNullOrEmpty(c.DepartmentManagerId))
                //{
                //    var user = _unitOfWork.UserRepository.GetUserByAccountId(Int32.Parse(c.DepartmentManagerId), companyId);

                //    if (user != null)
                //    {
                //        c.DepartmentManagerId = user.Id.ToString();
                //    }
                //}

                var lstDepartment = GenerateTree(departments, c);

                lst.Add(new DepartmentListItemModel
                {
                    Id = c.Id,
                    ParentId = c.ParentId,
                    Children = lstDepartment,
                    DepartmentManagerId = c.DepartmentManagerId,
                    DepartmentNumber = c.DepartmentNumber,
                    DepartmentName = c.DepartmentName,
                    NumberUser = c.NumberUser,
                    IsRoot = c.IsRoot,
                    EditUrl = c.EditUrl,
                    DeleteUrl = c.DeleteUrl,
                    DepartmentManager = c.DepartmentManager,
                    AccessGroupId = c.AccessGroupId,
                    AccessGroupName = c.AccessGroupName,
                    MaxNumberCheckout = c.MaxNumberCheckout,
                    MaxPercentCheckout = c.MaxPercentCheckout
                });
            }

            lst = lst.Where(x => departmentId != 0 || x.ParentId == 0).ToList();
            
            recordsFiltered = recordsTotal;

            if (filter.PageSize > 0)
            {
                lst = lst.Skip((filter.PageNumber - 1) * filter.PageSize).Take(filter.PageSize).ToList();
            }

            return lst;
        }


        private void ExportStringHandler(string field, ExcelWorksheet worksheet,
            int recordIndex, int colIndex, string loginName)
        {
            worksheet.Cells[recordIndex, colIndex].Value = field.ToString();

            worksheet.Cells[recordIndex, colIndex].Style.Fill.PatternType =
                ExcelFillStyle.Solid;
            worksheet.Cells[recordIndex, colIndex].Style.Fill.BackgroundColor
                .SetColor(Color.Yellow);
            worksheet.Cells[recordIndex, colIndex].AddComment("Incorrect Data", loginName);
        }

        /// <summary>
        /// Export invalid data to file
        /// </summary>
        /// <param name="departments"></param>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public byte[] ExportErrorData(List<DepartmentModel> departments, string loginName)
        {
            byte[] result;
            using (var package = new ExcelPackage())
            {
                // add a new worksheet to the empty workbook
                var worksheet =
                    package.Workbook.Worksheets.Add(Constants.WorkSheetName); //Worksheet name

                //First add the headers
                for (var i = 0; i < _header.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = _header[i];
                }

                //Add values
                var recordIndex = 2;
                departments = DistinctByExtension.DistinctBy(departments, c => c.Name).ToList();
                foreach (var department in departments)
                {
                    var colIndex = 1;
                    ExportStringHandler(department.Number, worksheet, recordIndex, colIndex++, loginName);
                    ExportStringHandler(department.Name, worksheet, recordIndex, colIndex++, loginName);
                    ExportStringHandler(department.ParentId + "", worksheet, recordIndex, colIndex, loginName);
                    ExportStringHandler(department.MaxPercentCheckout + "", worksheet, recordIndex, colIndex++, loginName);
                    ExportStringHandler(department.MaxNumberCheckout + "", worksheet, recordIndex, colIndex, loginName);
                    recordIndex++;
                }

                result = package.GetAsByteArray();
            }

            return result;
        }

        /// <summary>
        /// import department file
        /// </summary>
        /// <param name="type"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool ImportFile(string type, IFormFile file, out int total, out int fail)
        {
            var data = new List<DepartmentModel>();
            var totalList = new List<DepartmentModel>();
            var failList = new List<DepartmentModel>();

            var isSuccess = type == Constants.Excel ?
                LoadDepartmentsFromExcelFile(file, data, out total, ref failList)
                : LoadDepartmentsFromTextFile(file, data, out total, ref failList);

            fail = failList.Count();
            //if (isSuccess)
            //{
            //    var companyId = _httpContext.User.GetCompanyId();

            //    //Save system log
            //    var content = $"{DepartmentResource.msgImportDepartmentList}";
            //    var contentsDetails = $"{AccountResource.lblUsername} : {_httpContext.User.GetUsername()}" +
            //                          $"\n{CommonResource.lblFileName} : {file.FileName}";

            //    _unitOfWork.SystemLogRepository.Add(1, SystemLogType.Department, ActionLogType.Import,
            //        content, contentsDetails, null, companyId);

            //    _unitOfWork.Save();
            //}

            //isSuccess = true;
            return isSuccess;
        }

        /// <summary>
        /// Import file to DB
        /// </summary>
        /// <param name="listImportDepartments"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private bool Import(string fileName, List<DepartmentModel> listImportDepartments, List<DepartmentModel> failedToImport)
        {
            bool result = false;
            var companyId = _httpContext.User.GetCompanyId();

            var invalidDepartments = failedToImport;
            //var validDepartments = new List<DepartmentModel>();
            var validDepartments = listImportDepartments;
            //var updatingDepartments = new List<DepartmentModel>();
            var nameList = new List<string>();

            // [Edward] 2020.03.03
            // Delete logic about updating department through imported file.
            //foreach (var department in listImportDepartments)
            //{
            //    if (IsDepartmentNameExist(department))
            //    {
            //        invalidDepartments.Add((department));
            //    }
            //    else if (IsDepartmentNumberExist(department))
            //    {
            //        var dept = _unitOfWork.DepartmentRepository.GetByDepartmentCode(department.Number);
            //        department.Id = dept.Id;
            //        department.ParentId = dept.ParentId;
            //        updatingDepartments.Add((department));
            //    }
            //    else
            //    {
            //        validDepartments.Add(department);
            //    }
            //}

            validDepartments = validDepartments.AsQueryable().OrderBy(m => m.ParentId).ToList();
            if (validDepartments.Any())
            {
                foreach (var validDepartment in validDepartments)
                {
                    nameList.Add(validDepartment.Name);
                    Add(validDepartment);
                }
                result = true;
            }
            else
            {
                result = false;
            }

            // [Edward] 2020.03.03
            // Delete logic about updating department through imported file.
            //if (updatingDepartments.Any())
            //{
            //    foreach (var updatingDepartment in updatingDepartments)
            //    {
            //        Update(updatingDepartment);
            //    }
            //}

            //Save system log
            var content = $"{DepartmentResource.msgImportDepartmentList}";
            var contentsDetails = $"{AccountResource.lblUsername} : {_httpContext.User.GetUsername()}" +
                                  $"\n{CommonResource.lblFileName} : {fileName}";

            if (invalidDepartments.Any())
            {
                result = false;

                contentsDetails += "\n" + string.Format(DepartmentResource.msgFailedToImport, invalidDepartments.Count());

                foreach (var fail in invalidDepartments)
                {
                    contentsDetails += $"\n{DepartmentResource.lblDepartmentNumber} : {fail.Number}" +
                        $" | {DepartmentResource.lblDepartmentName} : {fail.Name}";
                }
            }

            var departments = _unitOfWork.DepartmentRepository.GetByNamesAndCompanyId(nameList, companyId);

            var departmentIds = departments.Select(c => c.Id).ToList();
            var departmentId = departmentIds.FirstOrDefault();

            _unitOfWork.SystemLogRepository.Add(departmentId, SystemLogType.Department, ActionLogType.Import,
                content, contentsDetails, departmentIds, companyId);

            _unitOfWork.Save();

            return result;
        }

        ///// <summary>
        ///// Load user from text file
        ///// </summary>
        ///// <param name="filePath"></param>
        ///// <param name="data"></param>
        private bool LoadDepartmentsFromTextFile(IFormFile file, List<DepartmentModel> data, out int total, ref List<DepartmentModel> failList)
        {
            try
            {
                var csvfilerecord = FileHelpers.ConvertToStringArray(file);

                total = 0;

                foreach (var row in csvfilerecord.Skip(1))
                {
                    if (string.IsNullOrEmpty(row) || row.Equals("\r")) continue;
                    var item = ReadLineFromCsv(row);

                    total++;

                    if (IsOkToImport(item, data))
                        data.Add(item);
                    else
                        failList.Add(item);
                }
                return Import(file.FileName, data, failList);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>   Loads departments from excel file. </summary>
        /// <remarks>   Edward, 2020-03-03. </remarks>
        /// <exception cref="InvalidFormatException">   Thrown when an Invalid Format error condition
        ///                                             occurs. </exception>
        /// <param name="file"> file to import. </param>
        /// <param name="data"> The data. </param>
        /// <returns>   True if it succeeds, false if it fails. </returns>
        private bool LoadDepartmentsFromExcelFile(IFormFile file, List<DepartmentModel> data, out int total, ref List<DepartmentModel> failList)
        {
            try
            {
                using (var package = new ExcelPackage(FileHelpers.ConvertToStream(file)))
                {
                    ExcelWorksheet worksheet;
                    int columnCount;
                    try
                    {
                        worksheet = package.Workbook.Worksheets[1];
                        columnCount = worksheet.Dimension.End.Column;
                    }
                    catch (Exception)
                    {
                        throw new InvalidFormatException();
                    }

                    if (columnCount != _header.Length)
                    {
                        throw new InvalidFormatException();
                    }

                    total = 0;

                    for (int i = worksheet.Dimension.Start.Row + 1;
                        i <= worksheet.Dimension.End.Row;
                        i++)
                    {
                        var item = ReadLineFromExcel(worksheet, i);

                        total++;

                        if (IsOkToImport(item, data))
                            data.Add(item);
                        else
                            failList.Add(item);
                    }
                }

                var fileName = file.FileName;
                return Import(file.FileName, data, failList);
            }
            catch (Exception e)
            {
                _logger.LogError($"{e.Message}:{Environment.NewLine} {e.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Check the DepartmentModel to see if it can be imported into the system.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private Boolean IsOkToImport(DepartmentModel item, List<DepartmentModel> data)
        {
            if (string.IsNullOrEmpty(item.Number) || string.IsNullOrEmpty(item.Name))
            {
                return false;
            }

            item.Number = item.Number.TrimStart().TrimEnd();
            item.Name = item.Name.TrimStart().TrimEnd();

            var companyId = _httpContext.User.GetCompanyId();

            var department = _unitOfWork.DepartmentRepository.GetByNameAndCompany(item.Name, companyId);
            if (department != null)
            {
                return false;
            }

            foreach (var eachData in data)
            {
                if (eachData.Name.Equals(item.Name))
                    return false;
            }

            department = _unitOfWork.DepartmentRepository.GetByNumberAndCompany(item.Number, companyId);
            if (department != null)
            {
                return false;
            }

            foreach (var eachData in data)
            {
                if (eachData.Number.Equals(item.Number))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Read txt file
        /// </summary>
        /// <param name="csvLine"></param>
        /// <returns></returns>
        private DepartmentModel ReadLineFromCsv(string csvLine)
        {
            string[] values = csvLine.Split(',');
            var columnCount = values.Length;

            if (columnCount != _header.Length)
            {
                throw new InvalidFormatException();
            }

            var colIndex = 0;

            var model = new DepartmentModel
            {
                Number = Convert.ToString(values[colIndex++]),
                Name = Convert.ToString(values[colIndex]),
                MaxPercentCheckout = Convert.ToInt32(values[colIndex++]),
                MaxNumberCheckout = Convert.ToInt32(values[colIndex]),
                //ParentId = Constants.DefaultDepartmentId
            };
            return model;
        }

        /// <summary>
        /// Read Line from Excel
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private DepartmentModel ReadLineFromExcel(ExcelWorksheet worksheet, int row)
        {
            var colIndex = 1;
            var cells = worksheet.Cells;
            var model = new DepartmentModel
            {
                Number = (Convert.ToString(cells[row, colIndex++].Value)),
                Name = (Convert.ToString(cells[row, colIndex++].Value)),
                MaxPercentCheckout = Convert.ToInt32(cells[row, colIndex++].Value),
                MaxNumberCheckout = Convert.ToInt32(cells[row, colIndex].Value),
            };
            var parentDepartmentName = (Convert.ToString(cells[row, colIndex].Value));
            var parentDepartment =
                _unitOfWork.DepartmentRepository.GetByNameAndCompany(parentDepartmentName, _httpContext.User.GetCompanyId());
            model.ParentId = parentDepartment?.Id ?? 0;

            return model;
        }

        #endregion Helpers

        public Account GetAccount(int accountId)
        {
            try
            {
                return _unitOfWork.AccountRepository.GetById(accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAccount");
                return null;
            }
        }

        public CompanyAccount GetCompanyAccount(int accountId, int companyId)
        {
            try
            {
                return _unitOfWork.CompanyAccountRepository.GetCompanyAccountByCompanyAndAccount(companyId, accountId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCompanyAccount");
                return null;
            }
        }

        /// <summary>   Gets user count. </summary>
        /// <remarks>   Edward, 2020-03-17. </remarks>
        /// <param name="companyId">    Identifier for the company. </param>
        /// <param name="departmentId"> Identifier for the department. </param>
        /// <returns>   The user count. </returns>
        public int GetUserCount(int companyId, int departmentId)
        {
            try
            {
                return _unitOfWork.UserRepository.GetCountByDepartmentId(companyId, departmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserCount");
                return 0;
            }
        }


        public string GetUserNameByEmail(int companyId, string emailAddress)
        {
            try
            {
                if (emailAddress == null)
                {
                    return null;
                }

                var user = _unitOfWork.UserRepository.GetUserByEmail(companyId, emailAddress);

                if (user != null)
                {
                    return $"{user.FirstName} {user.LastName}({emailAddress})";
                }
                else
                {
                    return emailAddress;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUserNameByEmail");
                return null;
            }
        }

        public Department GetById(int id)
        {
            try
            {
                return _unitOfWork.DepartmentRepository.GetById(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return null;
            }
        }

        public string AssignUsers(int departmentId, List<int> userIds)
        {
            var returnValue = string.Empty;
            var companyId = _httpContext.User.GetCompanyId();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var usersToAssign = _unitOfWork.UserRepository.GetByIds(companyId, userIds);

                        var newDepartment = _unitOfWork.DepartmentRepository.GetById(departmentId);

                        //Update users to new access group
                        foreach (var user in usersToAssign)
                        {
                            //var content = $"{ActionLogTypeResource.Update} : {user.FirstName + " " + user.LastName} ({user.UserCode})";
                            var content = $"{UserResource.msgChangeDepartment} : {user.FirstName + " " + user.LastName} ({user.UserCode})";
                            //var contentDetails = string.Format(MessageResource.msgChangeInfo, UserResource.lblDepartment, user.Department.DepartName, newDepartment.DepartName);
                            var contentDetails = Helpers.CreateChangedValueContents(UserResource.lblDepartment, user.Department.DepartName, newDepartment.DepartName);

                            user.DepartmentId = departmentId;

                            //if (plugin.DeptAcGrpConn)
                            //{
                            //    var departmentAccessGroup = _unitOfWork.AppDbContext.DepartmentAccessGroup.Include(da => da.AccessGroup).FirstOrDefault(da => da.DepartmentId == departmentId);
                            //    if (departmentAccessGroup != null)
                            //    {
                            //        var oldAgName = user.AccessGroup.Name;
                            //        if (user.AccessGroup.Type == (short)AccessGroupType.PersonalAccess)
                            //            oldAgName = $"{user.AccessGroup.Parent.Name}*";

                            //        contentDetails += $"\n{string.Format(MessageResource.msgChangeInfo, UserResource.lblAccessGroup, oldAgName, departmentAccessGroup.AccessGroup.Name)}";

                            //        user.AccessGroupId = departmentAccessGroup.AccessGroupId;
                            //    }
                            //}

                            _unitOfWork.SystemLogRepository.Add(user.Id, SystemLogType.User, ActionLogType.Update,
                                content, contentDetails, null, user.CompanyId);

                            _unitOfWork.UserRepository.Update(user);
                        }

                        _unitOfWork.Save();
                        transaction.Commit();

                        returnValue = "Ok";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        returnValue = ex.Message;
                        throw;
                    }
                }
            });

            return returnValue;
        }

        public string UnAssignUsers(List<int> userIds)
        {
            var returnValue = string.Empty;
            var companyId = _httpContext.User.GetCompanyId();

            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        var usersToUnAssign = _unitOfWork.UserRepository.GetByIds(companyId, userIds);

                        // remove department id
                        foreach (var user in usersToUnAssign)
                        {
                            user.DepartmentId = -1;
                            _unitOfWork.UserRepository.Update(user);
                        }

                        _unitOfWork.Save();

                        returnValue = "Ok";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        returnValue = ex.Message;
                        throw;
                    }
                }
            });

            return returnValue;
        }
        
        public int GetNewAutoDepartmentNumber(int companyId)
        {
            try
            {
                var count = _unitOfWork.DepartmentRepository.GetCountByCompanyId(companyId);
                var department = _unitOfWork.DepartmentRepository.GetByNumberAndCompany(count.ToString(), companyId);
                while (department != null)
                {
                    count += 1;
                    department = _unitOfWork.DepartmentRepository.GetByNumberAndCompany(count.ToString(), companyId);
                }

                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetNewAutoDepartmentNumber");
                return 0;
            }
        }

        public List<DepartmentDeviceModel> GetDepartmentDevices(int departmentId, string search,
            int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                totalRecords = 0;
                recordsFiltered = 0;
                var department = _unitOfWork.AppDbContext.Department.Include(x => x.DepartmentDevice)
                    .FirstOrDefault(x => !x.IsDeleted && x.CompanyId == companyId && x.Id == departmentId);
                if (department != null)
                {
                    var data = _unitOfWork.DepartmentDeviceRepository.GetByDepartmentId(department.Id)
                        .Select(_mapper.Map<DepartmentDeviceModel>).AsQueryable();

                totalRecords = data.Count();

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim().RemoveDiacritics().ToLower();
                    data = data.Where(x => x.DeviceAddress.RemoveDiacritics().ToLower().Contains(search) ||
                                           x.DoorName.RemoveDiacritics().ToLower().Contains(search));
                }

                recordsFiltered = data.Count();

                // Default sort ( asc - DoorName )
                data = data.OrderBy(c => c.DoorName);

                data = data.OrderBy($"{sortColumn} {sortDirection}");
                if (pageSize >= 0)
                {
                    data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                }
                return data.ToList();
            }

                return new List<DepartmentDeviceModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDepartmentDevices");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<DepartmentDeviceModel>();
            }
        }

        public List<DepartmentDeviceModel> GetDepartmentDevicesUnassign(int departmentId, string search, int pageNumber, int pageSize, string sortColumn,
            string sortDirection, out int totalRecords, out int recordsFiltered)
        {
            try
            {
                var companyId = _httpContext.User.GetCompanyId();
                totalRecords = 0;
                recordsFiltered = 0;
                var department = _unitOfWork.AppDbContext.Department.Include(x => x.DepartmentDevice)
                    .FirstOrDefault(x => !x.IsDeleted && x.CompanyId == companyId && x.Id == departmentId);
                if (department != null)
                {
                    var data = _unitOfWork.AppDbContext.IcuDevice
                            .Include(c => c.ActiveTz)
                            .Where(c =>
                            c.CompanyId == companyId && c.Status == (short)Status.Valid &&
                            c.DepartmentDevice.All(x => x.DepartmentId != department.Id) &&
                            !c.Company.IsDeleted && !c.IsDeleted).Select(_mapper.Map<DepartmentDeviceModel>).AsQueryable();

                totalRecords = data.Count();

                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim().RemoveDiacritics().ToLower();
                    data = data.Where(x => x.DeviceAddress.RemoveDiacritics().ToLower().Contains(search) ||
                                           x.DoorName.RemoveDiacritics().ToLower().Contains(search));
                }

                recordsFiltered = data.Count();

                // Default sort ( asc - DoorName )
                data = data.OrderBy(c => c.DoorName);

                data = data.OrderBy($"{sortColumn} {sortDirection}");
                if (pageSize >= 0)
                {
                    data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                }
                return data.ToList();
            }

                return new List<DepartmentDeviceModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetDepartmentDevicesUnassign");
                totalRecords = 0;
                recordsFiltered = 0;
                return new List<DepartmentDeviceModel>();
            }
        }

        public bool AssignDepartmentDevice(int id, List<int> doorIds)
        {
            bool result = false;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        int companyId = _httpContext.User.GetCompanyId();
                        var department = _unitOfWork.DepartmentRepository.GetByIdAndCompanyId(id, companyId);

                        if (department != null)
                        {
                            foreach (var door in doorIds)
                            {
                                var departmentDevice = new DepartmentDevice()
                                {
                                    DepartmentId = department.Id,
                                    IcuId = door
                                };
                                _unitOfWork.DepartmentDeviceRepository.Add(departmentDevice);
                                _unitOfWork.Save();
                            }
                        }

                        transaction.Commit();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                        result = false;
                    }
                }
            });
            return result;
        }

        public bool UnAssignDepartmentDevice(int id, List<int> doorIds)
        {
            bool result = false;
            _unitOfWork.AppDbContext.Database.CreateExecutionStrategy().Execute(() =>
            {
                using (var transaction = _unitOfWork.AppDbContext.Database.BeginTransaction())
                {
                    try
                    {
                        int companyId = _httpContext.User.GetCompanyId();
                        var department = _unitOfWork.DepartmentRepository.GetByIdAndCompanyId(id, companyId);

                        if (department != null)
                        {
                            var departmentDevices =
                                _unitOfWork.DepartmentDeviceRepository.GetByDepartmentIdAndDoorIds(department.Id,
                                    doorIds);

                            _unitOfWork.DepartmentDeviceRepository.DeleteRange(departmentDevices);
                            _unitOfWork.Save();
                        }

                        transaction.Commit();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        _logger.LogError($"{ex.Message}:{Environment.NewLine} {ex.StackTrace}");
                        result = false;
                    }
                }
            });
            return result;
        }

        public bool CheckDoorInDepartment(int companyId, int accountId, List<int> doors)
        {
            if (companyId == 0 && _httpContext.User.GetAccountType() == (short)AccountType.SystemAdmin)
            {
                return true;
            }
            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
            PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
            if (plugIns.DepartmentAccessLevel && _httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
            {
                var listDepartments = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, accountId);
                var listIcuOfDepartment = _unitOfWork.DepartmentDeviceRepository.GetByDepartmentIds(listDepartments)
                    .Select(x => x.IcuId).Distinct().ToList();
                foreach (int number in doors)
                {
                    if (!listIcuOfDepartment.Contains(number))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool CheckPermissionDepartmentLevel(int parentId)
        {
            var accountType = _httpContext.User.GetAccountType();
            var companyId = _httpContext.User.GetCompanyId();
            var plugin = _unitOfWork.PlugInRepository.GetPlugInByCompany(companyId);
            PlugIns plugIns = JsonConvert.DeserializeObject<PlugIns>(plugin.PlugIns);
            if (plugIns.DepartmentAccessLevel && accountType == (short)AccountType.DynamicRole)
            {
                if (parentId != 0)
                {  
                   var listDepartments = _unitOfWork.DepartmentRepository.GetDepartmentIdsByAccountDepartmentRole(companyId, _httpContext.User.GetAccountId());
                   return listDepartments.Contains(parentId);
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Generate test data
        /// </summary>
        /// <param name="numberOfDept"></param>
        public void GenerateTestData(int numberOfDept)
        {
            var companyId = _httpContext.User.GetCompanyId();

            var userCode = _unitOfWork.AppDbContext.Department.Count(d => d.CompanyId == companyId);

            for (var i = 0; i < numberOfDept; i++)
            {
                var fakeDepartment = new Faker<Department>()
                    .RuleFor(u => u.CompanyId, f => companyId)
                    .RuleFor(u => u.DepartName, (f, u) => f.Company.CompanyName())
                    .RuleFor(u => u.DepartNo, f => $"FAKE-{userCode++}");

                var department = fakeDepartment.Generate();
                _unitOfWork.DepartmentRepository.Add(department);

                _unitOfWork.Save();
            }

            _unitOfWork.Save();
        }
    }
}