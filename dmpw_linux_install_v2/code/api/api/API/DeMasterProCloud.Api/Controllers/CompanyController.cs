using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using DeMasterProCloud.DataModel.Company;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.PlugIn;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Company Controller
    /// </summary>
    //[ApiExplorerSettings(IgnoreApi = true)]
    [Produces("application/json")]
    [Authorize(Policy = Constants.Policy.SystemAndSuperAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CompanyController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IVisitService _visitService;
        private readonly IUnregistedDeviceService _unregistedDeviceService;
        private readonly IDeviceService _deviceService;
        private readonly HttpContext _httpContext;
        private readonly IMapper _mapper;

        /// <summary>
        /// Company controller
        /// </summary>
        /// <param name="companyService"></param>
        /// <param name="visitService"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="unregistedDeviceService"></param>
        /// <param name="deviceService"></param>
        /// <param name="mapper"></param>
        public CompanyController(ICompanyService companyService, IVisitService visitService,IHttpContextAccessor httpContextAccessor, 
            IUnregistedDeviceService unregistedDeviceService, IDeviceService deviceService, IMapper mapper)
        {
            _companyService = companyService;
            _visitService = visitService;
            _unregistedDeviceService = unregistedDeviceService;
            _deviceService = deviceService;
            _httpContext = httpContextAccessor.HttpContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Get the Company information
        /// </summary>
        /// <param name="id">Identified of Company</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Company Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCompaniesId)]
        public IActionResult Get(int id)
        {
            var company = _companyService.GetById(id);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }
            var model = _mapper.Map<CompanyModel>(company);
            model.LogoEnable = true;
            model.MiniLogoEnable = true;
            model.ListLanguage = _companyService.GetListLanguageForCompany(id);
            bool enableCustomizeLanguage = _companyService.CheckPluginByCompany(company.Id, Constants.PlugIn.CustomizeLanguage);
            model.AllLanguage = enableCustomizeLanguage ? _companyService.GetListLanguageForCompany(0) : null;
            return Ok(model);
        }

        /// <summary>
        /// Get list of Companies with pagination.
        /// </summary>
        /// <param name="search">Query string that filter Company by Name or Code.</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: 'desc' for descending , 'asc' for ascending </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Data filtered by string of search does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCompanies)]
        public IActionResult Gets(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id", string sortDirection = "desc")
        {
            sortColumn = Helpers.CheckPropertyInObject<CompanyListModel>(sortColumn, "Id", new []{"Code", "Name", "Createdon"});
            var companies = _companyService.GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal, out var recordsFiltered).ToList();

            var pagingData = new PagingData<CompanyListModel>
            {
                Data = companies,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get Company information
        /// </summary>
        /// <param name="id">Identified of Company</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Company Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiCompanyId)]
        public IActionResult GetInfomation(int id)
        {
            var companyInfo = _companyService.GetDefaultInfoById(id);
            if (companyInfo == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            return Ok(companyInfo);
        }

        /// <summary>
        /// Add a company
        /// </summary>
        /// <param name="model">JSON model for Company</param>
        /// <returns></returns>
        /// <response code="201">Success: Create new a company</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiCompanies)] 
        public IActionResult Add([FromBody]CompanyModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }
            if (!string.IsNullOrEmpty(model.Code) && _companyService.GetByCode(model.Code) != null)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.Exist, CompanyResource.lblCompanyCode));
            }
            var companyId= _companyService.Add(model);
            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, CompanyResource.lblCompany.ToLower()), companyId.ToString());
        }

        /// <summary>
        /// Assign device(s) to a Company
        /// </summary>
        /// <param name="deviceIds">selected device(s) Id(s)</param>
        /// <param name="id">companyId</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Company Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiAssignCompany)]
        //[Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult AssignDoor([FromBody]List<int> deviceIds, int id)
        {
            if (!deviceIds.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            _companyService.AssignToCompany(deviceIds, id);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageAssignSuccess, DeviceResource.lblDevice));
        }

        /// <summary>
        /// Assign unregistered ids to company
        /// </summary>
        /// <param name="unregisteredIds"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiCompaniesIdAssignUnregisteredDevice)]
        public IActionResult AssignUnregisteredDevice([FromBody]List<int> unregisteredIds, int id)
        {
            if (unregisteredIds == null || !unregisteredIds.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.BadRequest);
            }

            var company = _companyService.GetById(id);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }
            
            var missingDevices = _unregistedDeviceService.GetAll();
            if (missingDevices == null)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.msgNotFoundUnregistedDevice);
            }
            
            missingDevices = missingDevices.Where(m => unregisteredIds.Contains(m.Id)).ToList();
            int countAddNewDevice = missingDevices.Count(m => m.RegisterType == (short)Registertype.NewDevice);
            var checkLimit = _deviceService.CheckLimitDevicesAdded(countAddNewDevice);
            if (!checkLimit.IsAdded)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(DeviceResource.msgMaximumAddDevice, checkLimit.NumberOfMaximum));
            }

            var cameraUnregistered = new List<UnregistedDevice>();
            var otherDevices = new List<UnregistedDevice>();

            foreach (var device in missingDevices)
            {
                if (device.DeviceType.ToUpper().Contains("CAMERA"))
                    cameraUnregistered.Add(device);
                else
                    otherDevices.Add(device);
            }

            // assign camera(s) in company
            if (cameraUnregistered.Any())
            {
                var result = _companyService.AssignCameraToCompany(cameraUnregistered, id);
                if (!string.IsNullOrEmpty(result))
                {
                    return new ApiSuccessResult(StatusCodes.Status400BadRequest, result);
                }
            }

            // assign device(s) in company
            if (otherDevices.Any())
            {
                var deviceIds = _unregistedDeviceService.AddMissingDevice(otherDevices);
                if (!deviceIds.Any())
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.UnprocessableEntity);
                }

                _companyService.AssignToCompany(deviceIds, id);
            }

            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageAssignSuccess, DeviceResource.lblDevice));
        }

        /// <summary>
        /// Edit company information
        /// </summary>
        /// <param name="id">Identified of Company</param>
        /// <param name="model">JSON model for Company</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Company Id does not exist in DB</response>
        /// <response code="422">Validation Failed: Data in Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiCompaniesId)]
        public IActionResult Edit(int id, [FromBody]CompanyModel model)
        {
            model.Id = id;

            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }

            var company = _companyService.GetById(id);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }
            
            if (!string.IsNullOrEmpty(model.Code))
            {
                var companyByCode = _companyService.GetByCode(model.Code);
                if (companyByCode != null && companyByCode.Id != company.Id)
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.Exist, CompanyResource.lblCompanyCode));
                }
            }

            _companyService.Update(model);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, CompanyResource.lblCompany, model.Name));
        }

        /// <summary>
        /// Delete a company
        /// </summary>
        /// <param name="id">Identified of Company</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Company Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiCompaniesId)]
        public IActionResult Delete(int id)
        {
            var company = _companyService.GetById(id);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            _companyService.Delete(company);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, CompanyResource.lblCompany));
        }

        /// <summary>
        /// Delete multiple companies
        /// </summary>
        /// <param name="ids">List of company ids</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: List of Company Ids does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiCompanies)]
        public IActionResult DeleteMultiple(List<int> ids)
        {
            var companies = _companyService.GetByIds(ids);
            if (!companies.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            var rootCompanies = companies.Where(m => m.RootFlag).ToList();
            var delCompanies = companies.Except(rootCompanies).ToList();

            if (delCompanies.Any())
            {
                _companyService.DeleteRange(delCompanies);
            }
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, CompanyResource.lblCompany));
        }
        
        /// <summary>
        /// Reset key companies
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPatch]
        [Route(Constants.Route.ApiRegenerateCompaniesKey)]
        //[Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult RegenerateAesKey()
        {
            _companyService.ResetAesKeyCompanies();
            return Ok();
        }
        
        
        /// <summary>
        /// Reset key company
        /// </summary>
        /// <param name="id">Company Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response code="500">Internal Server Error: Company Id does not exist in DB</response>
        [HttpPatch]
        [Route(Constants.Route.ApiRegenerateCompanyKey)]
        //[Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult RegenerateAesKeyCompany(int id)
        {
            _companyService.ResetAesKeyCompany(id);
            return Ok();
        }

        ///// <summary>
        ///// Get Logo image from Company
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //[AllowAnonymous]
        //[Route(Constants.Route.ApiCompaniesLogo)]
        //public IActionResult Logo(int id = 0)
        //{
        //    var logoDefault = "avatar.png";
        //    var companyId = id;
        //    if (id == 0)
        //    {
        //        companyId = _httpContext.User.GetCompanyId();
        //        logoDefault = "logo.png";
        //    }
        //    var company = _companyService.GetById(companyId);
        //    if (company?.Logo != null)
        //    {
        //        return File(company.Logo, "image/png");
        //    }

        //    var webRootPath = _hostingEnvironment.WebRootPath;
        //    var file = System.IO.Path.Combine(webRootPath, "images", logoDefault);
        //    var byteData = System.IO.File.ReadAllBytes(file);
        //    return File(byteData, "image/png");
        //}

        ///// <summary>
        ///// Get mini logo image from Company
        ///// </summary>
        ///// <param name="id"></param>
        ///// <returns></returns>
        //[HttpGet]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //[AllowAnonymous]
        //[Route(Constants.Route.ApiCompaniesMiniLogo)]
        //public IActionResult MiniLogo(int id = 0)
        //{
        //    var miniLogoDefault = "avatar.png";
        //    var companyId = id;
        //    if (id == 0)
        //    {
        //        companyId = _httpContext.User.GetCompanyId();
        //        miniLogoDefault = "logo-xs.png";
        //    }
        //    var company = _companyService.GetById(companyId);
        //    if (company?.MiniLogo != null)
        //    {
        //        return File(company.MiniLogo, "image/png");
        //    }

        //    var webRootPath = _hostingEnvironment.WebRootPath;
        //    var file = System.IO.Path.Combine(webRootPath, "images", miniLogoDefault);
        //    var byteData = System.IO.File.ReadAllBytes(file);
        //    return File(byteData, "image/png");
        //}

        //[HttpGet]
        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //[Route(Constants.Route.ApiCompanies)]
        //public IActionResult Get(string search, int pageNumber = 1, int pageSize = 10, int sortColumn = 0,
        //    string sortDirection = "desc")
        //{
        //    var companies = _companyService
        //        .GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
        //            out var recordsFiltered).AsEnumerable().Select(m =>
        //        {
        //            m.ExpiredTo = m.ExpiredDate.ToSettingDateString();
        //            m.CreatedOn = m.CreatedDate.ToSettingDateString();
        //            m.Id = m.RootFlag ? 0 : m.Id;
        //            return m;
        //        }).ToList();

        //    var pagingData = new PagingData<CompanyListModel>
        //    {
        //        Data = companies,
        //        Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
        //    };
        //    return Ok(pagingData);
        //}
        
        /// <summary>
        /// Get plug-ins with company by id
        /// </summary>
        /// <param name="id">Company Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Company Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiGetPlugByCompany)]
        //[Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetPluginByCompany(int id)
        {
            var companyPlugIn = _companyService.GetPluginByCompany(id);
            if (companyPlugIn != null)
            {
                return Ok(companyPlugIn);
            }

            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
        }
        
        /// <summary>
        /// Update plug-ins to company by id
        /// </summary>
        /// <param name="id">Company Id</param>
        /// <param name="model">JSON model for Company</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Company Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPatch]
        [Route(Constants.Route.ApiGetPlugByCompany)]
        //[Authorize(Policy = Constants.Policy.SystemAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdatePluginByCompany(int id, [FromBody] PlugIns model)
        {
            var plugIn = _companyService.GetPluginByCompany(id);
            if (plugIn != null)
            {
                var plugInId = _companyService.UpdatePluginByCompany(id, model);

                return new ApiSuccessResult(StatusCodes.Status200OK,
                    string.Format(MessageResource.MessageEnabledSuccess, UserResource.lblPlugIn, plugInId));
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
        }
        
        /// <summary>
        /// Get fields visible of visit setting
        /// </summary>
        /// <param name="companyId">Company Id</param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiVisitsSettingVisible)]
        public IActionResult GetVisibleFieldSettingVisit(int companyId)
        {
            var company = _companyService.GetById(companyId);
            if (company == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);

            var visitSetting = _visitService.GetVisitSettingByCompanyId(companyId);
            if (visitSetting == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);

            return Ok(Helpers.GetSettingVisibleFields(visitSetting.VisibleFields, typeof(DeMasterProCloud.DataModel.Visit.VisitSettingModel)));
        }

        /// <summary>
        /// Update fields visible of visit setting
        /// </summary>
        /// <param name="companyId">Company Id</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPut]
        [Route(Constants.Route.ApiVisitsSettingVisible)]
        public IActionResult UpdateVisibleFieldSettingVisit(int companyId, [FromBody] Dictionary<string, bool> model)
        {
            var company = _companyService.GetById(companyId);
            if (company == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);

            var visitSetting = _visitService.GetVisitSettingByCompanyId(companyId);
            if (visitSetting == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);

            _visitService.UpdateSettingVisibleFieldsByCompanyId(companyId, model);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, "visit", "setting"));
        }

        /// <summary>
        /// Get detail of company by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiCompaniesIdDetail)]
        public IActionResult GetCompanyDetailById(int id)
        {
            var data = _companyService.GetCompanyViewDetailById(id);
            if (data == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);

            return Ok(data);
        }





        /// <summary>
        /// Get company setting about DB encryption.
        /// </summary>
        /// <param name="id">Identified of Company</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Company Id does not exist in DB</response>
        /// <response code="422">Validation Failed: Data in Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiEnableDBEncrption)]
        public IActionResult GetEncryptSetting(int id)
        {
            var company = _companyService.GetById(id);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            var result = _companyService.GetEncryptSetting(id);

            return Ok(result);
        }

        /// <summary>
        /// Update company setting about DB encryption.
        /// </summary>
        /// <param name="id">Identified of Company</param>
        /// <param name="model">JSON model for Company</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Company Id does not exist in DB</response>
        /// <response code="422">Validation Failed: Data in Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiEnableDBEncrption)]
        public IActionResult EditEncryptSetting(int id, [FromBody] EncryptSettingModel model)
        {
            model.Id = id;

            var company = _companyService.GetById(id);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            _companyService.UpdateEncryptSetting(model);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, "", ""));
        }



        /// <summary>
        /// Get company setting about DB encryption.
        /// </summary>
        /// <param name="id">Identified of Company</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Company Id does not exist in DB</response>
        /// <response code="422">Validation Failed: Data in Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiEnableExpiredPW)]
        public IActionResult GetExpiredPWSetting(int id)
        {
            var company = _companyService.GetById(id);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            var result = _companyService.GetExpiredPWSetting(id);

            return Ok(result);
        }

        /// <summary>
        /// Update company setting about DB encryption.
        /// </summary>
        /// <param name="id">Identified of Company</param>
        /// <param name="model">JSON model for Company</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Company Id does not exist in DB</response>
        /// <response code="422">Validation Failed: Data in Model JSON wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiEnableExpiredPW)]
        public IActionResult EditExpiredPWSetting(int id, [FromBody] ExpiredPWSettingModel model)
        {
            model.Id = id;

            var company = _companyService.GetById(id);
            if (company == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            }

            _companyService.UpdateExpiredPWSetting(model);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, "", ""));
        }


        /// <summary>
        /// TEST for encryption.
        /// </summary>
        /// <param name="plainData"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiEncryptTest)]
        public IActionResult TEST_ENC(string plainData)
        {
            if (string.IsNullOrWhiteSpace(plainData)) return Ok("Please enter the value you want to test for encryption.");

            try
            {
                var result = _companyService.TestENC(plainData);

                return Ok(result);
            }
            catch (System.Exception e)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, $"ERROR : {e.Message} | INNER-EXCEPTION : {e.InnerException?.Message}");
            }
        }


        /// <summary>
        /// TEST for decryption.
        /// </summary>
        /// <param name="encData"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiDecryptTest)]
        public IActionResult TEST_DEC(string encData)
        {
            if (string.IsNullOrWhiteSpace(encData)) return Ok("Please enter the value you want to test for decryption.");

            try
            {
                var result = _companyService.TestDEC(encData);

                return Ok(result);
            }
            catch (System.Exception e)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, $"ERROR : {e.Message} | INNER-EXCEPTION : {e.InnerException?.Message}");
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route(Constants.Route.ApiCompanyResetAllowIp)]
        public IActionResult ResetAllowIpSetting(int companyId)
        {
            try
            {
                _companyService.ResetAllowIpSetting(companyId);

                return Ok();
            }
            catch (System.Exception e)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity);
            }
        }
    }
}
