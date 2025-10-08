using AutoMapper;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.User;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeMasterProCloud.Api.Infrastructure.Mapper;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.AccessGroupDevice;
using DeMasterProCloud.DataModel.Device;
using DeMasterProCloud.DataModel.EventLog;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
using DeMasterProCloud.DataModel.Account;
using DeMasterProCloud.DataModel.PlugIn;
using Newtonsoft.Json;
using DeMasterProCloud.DataModel.Visit;
using DeMasterProCloud.DataModel.Header;
using DeMasterProCloud.Service.Infrastructure.Header;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// User controller
    /// </summary>
    [Produces("application/ms-excel", "application/json", "application/text")]
    public class UserController : Controller
    {
        private readonly IEventLogService _eventLogService;
        private readonly IUserService _userService;
        private readonly ICompanyService _companyService;
        private readonly HttpContext _httpContext;
        private readonly ICameraService _cameraService;
        private readonly IConfiguration _configuration;
        private readonly IAccessGroupService _accessGroupService;
        private readonly IAccountService _accountService;
        private readonly IBuildingService _buildingService;
        private readonly IPluginService _pluginService;
        private readonly IMapper _mapper;

        private readonly string[] _headers = {
            "Request",
            "Number of user",
            "Time receiving request (t1)",
            "Time finishing request (t2)",
            "Processing time in seconds (t2-t1)"
        };

        public UserController(IUserService userService, IHttpContextAccessor httpContextAccessor,
            ICameraService cameraService, IConfiguration configuration, IEventLogService eventLogService,
            ICompanyService companyService, IAccessGroupService accessGroupService, IAccountService accountService, 
            IBuildingService buildingService, IPluginService pluginService, IMapper mapper)
        {
            _userService = userService;
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration;
            _eventLogService = eventLogService;
            _cameraService = cameraService;
            _companyService = companyService;
            _accessGroupService = accessGroupService;
            _accountService = accountService;
            _buildingService = buildingService;
            _pluginService = pluginService;
            _mapper = mapper;
        }

        /// <summary>
        /// Get init user management page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersInit)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetInit()
        {
            int companyId = _httpContext.User.GetCompanyId();
            var data = _userService.GetInit(companyId);
            
            // all of building tree
            var buildings = _buildingService.GetListBuildingTree("",0, out _, out _, 1, 0, "Name", "asc");
            data.Add("buildings", buildings);
            
            return Ok(data);
        }

        /// <summary>
        /// Get user list with pagination
        /// </summary>
        /// <param name="search">Query string that filter by user name, email, name of department or expired date</param>
        /// <param name="searchAll">search all</param>
        /// <param name="position">Query string that filter by position</param>
        /// <param name="cardId">Query string that filter by cardId</param>
        /// <param name="empNumber">Query string that filter by empNumber</param>
        /// <param name="expiredDate">Query string that filter by expiredDate - format: dd.MM.yyyy HH:mm:ss</param>
        /// <param name="expiredDateEnd"> Query string tath filter by end of expiredDate - format : dd.MM.yyyy HH:mm:ss </param>
        /// <param name="departmentIds">Query string that filter by departmentIds</param>
        /// <param name="workTypeIds">Query string that filter by workTypeList</param>
        /// <param name="category"> Category option Id list </param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="isValid"> valid user or invalid user or all user </param>
        /// <param name="departmentId"> identifier of department </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult Gets(string searchAll, string search, string position, string cardId, string empNumber, string expiredDate, string expiredDateEnd, List<int> departmentIds, List<int> workTypeIds, List<int> category = null, int pageNumber = 1, int pageSize = 10, string sortColumn = null,
            string sortDirection = "desc", List<int> isValid = null, int departmentId = 0, UserFilterAddOns addOns = null)
        {
            if (isValid == null || !isValid.Any())
                isValid = new List<int>() { (int) UserStatus.Use };

            var filter = new UserFilterModel()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                FirstName = search,
                Search = searchAll,
                Position = position,
                CardId = cardId,
                Status = isValid,
                ExpiredDate = expiredDate.ConvertDefaultStringToDateTime(),
                ExpiredDateEnd = expiredDateEnd.ConvertDefaultStringToDateTime(),
                EmpNumber = empNumber,
                DepartmentIds = departmentIds,
                WorkTypeIds = workTypeIds,
                Category = category,
                AddOns = addOns
            };
            var users = _userService.GetPaginated(filter, out var recordsTotal, out var recordsFiltered, out List<HeaderData> userHeader, Page.User, departmentId).ToList();
            
            if (userHeader != null && userHeader.Any())
            {
                var pagingData = new PagingData<UserListModel, HeaderData>
                {
                    Data = users,
                    Header = userHeader,
                    Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
                };

                return Ok(pagingData);
            }
            else
            {
                var pagingData = new PagingData<UserListModel>
                {
                    Data = users,
                    Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
                };

                return Ok(pagingData);
            }
        }


        /// <summary>
        /// Get user list with pagination
        /// </summary>
        /// <param name="search">Query string that filter by user name, email, name of department or expired date</param>
        /// <param name="searchAll">search all</param>
        /// <param name="position">Query string that filter by position</param>
        /// <param name="cardId">Query string that filter by cardId</param>
        /// <param name="empNumber">Query string that filter by empNumber</param>
        /// <param name="expiredDate">Query string that filter by expiredDate - format: dd.MM.yyyy HH:mm:ss</param>
        /// <param name="expiredDateEnd"> Query string tath filter by end of expiredDate - format : dd.MM.yyyy HH:mm:ss </param>
        /// <param name="departmentIds">Query string that filter by departmentIds</param>
        /// <param name="workTypeIds">Query string that filter by workTypeList</param>
        /// <param name="category"> Category option Id list </param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="isValid"> valid user or invalid user or all user </param>
        /// <param name="departmentId"> identifier of department </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersReturnUserModel)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetsReturnUserModel(string searchAll, string search, string position, string cardId, string empNumber, string expiredDate, string expiredDateEnd, List<int> departmentIds, List<int> workTypeIds, List<int> category = null, int pageNumber = 1, int pageSize = 10, string sortColumn = null,
            string sortDirection = "desc", List<int> isValid = null, int departmentId = 0, UserFilterAddOns addOns = null)
        {
            if (isValid == null || !isValid.Any())
                isValid = new List<int>() { (int) UserStatus.Use };

            var filter = new UserFilterModel()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                FirstName = search,
                Search = searchAll,
                Position = position,
                CardId = cardId,
                Status = isValid,
                ExpiredDate = expiredDate.ConvertDefaultStringToDateTime(),
                ExpiredDateEnd = expiredDateEnd.ConvertDefaultStringToDateTime(),
                EmpNumber = empNumber,
                DepartmentIds = departmentIds,
                WorkTypeIds = workTypeIds,
                Category = category,
                AddOns = addOns
            };
            var users = _userService.GetPaginatedReturnUserModel(filter, out var recordsTotal, out var recordsFiltered);
            
          
            var pagingData = new PagingData<UserModel>
            {
                Data = users.ToList(),
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }

        /// <summary>
        /// Get assigned user list in department.
        /// </summary>
        /// <param name="search">Query string that filter by user name, email, name of department or expired date</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="isValid"> valid user or invalid user or all user </param>
        /// <param name="departmentId"> identifier of department </param>
        /// <param name="category"> filter option - category </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersDeptAssign)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetAssignUsers(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = null,
            string sortDirection = "desc", List<int> isValid = null, int departmentId = 0, List<int> category = null)
        {
            if(isValid == null || isValid.Count == 0)
            {
                // Except invaild user.
                isValid = [ (int)UserStatus.Use ];
            }

            var filter = new UserFilterModel()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                Search = search,
                Status = isValid,
                Category = category
            };
            var users = _userService.GetPaginated(filter, out var recordsTotal, out var recordsFiltered, out List<HeaderData> userHeader, Page.Department + Page.User, departmentId);

            var dataUsers = users.ToList();
            var pagingData = new PagingData<UserListModel, HeaderData>
            {
                Data = dataUsers,
                Header = userHeader,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }
        
        /// <summary>
        /// Get unassigned user list in department.
        /// </summary>
        /// <param name="search">Query string that filter by user name, email, name of department or expired date</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="isValid"> valid user or invalid user or all user </param>
        /// <param name="departmentId"> identifier of department </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersDeptUnassign2)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetUnassignUsers2(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = null,
            string sortDirection = "desc", List<int> isValid = null, int departmentId = 0)
        {
            sortColumn = SortColumnMapping.UserColumn(sortColumn);
            var filter = new UserFilterModel()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                FirstName = search,
                Status = isValid,
            };
            var users = _userService.GetPaginated(filter, out var recordsTotal, out var recordsFiltered, out List<HeaderData> userHeader, Page.Department + Page.UnAssignUser, departmentId, false);

            var dataUsers = users.ToList();
            var pagingData = new PagingData<UserListModel, HeaderData>
            {
                Data = dataUsers,
                Header = userHeader,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };

            return Ok(pagingData);
        }

        /// <summary>
        /// Get user by id. In the case id = 0 then just getting initial data.
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult Get(int id)
        {
            var model = new UserDataModel();
            if (id != 0)
            {
                var user = _userService.GetById(id);
                if (user == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
                }
                model = _mapper.Map<UserDataModel>(user);
            }
            _userService.InitData(model);
            return Ok(model);
        }


        /// <summary>
        /// Get Card Count by accessGroup ID
        /// </summary>
        /// <param name="id"> user identifier. </param>
        /// <returns> IActionResult about card count that specific user has </returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersCardCount)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetCardCount(int id)
        {
            int cardCount = _userService.GetCardCount(id);
            return Ok(cardCount);
        }
        
        /// <summary>
        /// Add new a user
        /// </summary>
        /// <param name="model">JSON model for User</param>
        /// <returns></returns>
        /// <response code="201">Create new a user</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Data of Model JSON wrong. Or card is existed in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.User)]
        public IActionResult Add([FromBody] UserModel model)
        {
            int companyId = _httpContext.User.GetCompanyId();

            var checkLimitModel = _companyService.CheckLimitCountOfUsers(companyId, 1);
            if (!checkLimitModel.IsAdded)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(UserResource.msgMaximumAddUser, checkLimitModel.NumberOfMaximum));
            }

            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            // check assign full access group when enable plugin Department Access Level
            var isFullAg = _accessGroupService.IsFullAccessGroup(companyId, model.AccessGroupId);
            var enablePlugin = _pluginService.CheckPluginCondition(Constants.PlugIn.DepartmentAccessLevel, companyId);
            if (isFullAg && enablePlugin && _httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgCannotAssignAgToUser);
            }

            // check access setting for approve.
            var accessSetting = _userService.GetAccessSettingByCompany();

            if (accessSetting.ApprovalStepNumber == (short)VisitSettingType.NoStep)
            {
                // model.ApproverId1 = _httpContext.User.GetAccountId();
                // model.ApproverId2 = model.ApproverId1;

                model.ApprovalStatus = (int)ApprovalStatus.NotUse;
            }
            else if (accessSetting.ApprovalStepNumber == (short)VisitSettingType.FirstStep)
            {
                var approvalAccounts = _accountService.GetAccessApprovalAccount();
                if (!approvalAccounts.Where(m => m.Id == model.ApproverId1).Any())
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgInvalidAccount);
                }

                model.ApprovalStatus = (int)ApprovalStatus.ApprovalWaiting1;
            }
            else if (accessSetting.ApprovalStepNumber == (short)VisitSettingType.SecondStep)
            {
                var approvalAccounts = _accountService.GetAccessApprovalAccount();
                if (!approvalAccounts.Where(m => m.Id == model.ApproverId1).Any()
                    || !approvalAccounts.Where(m => m.Id == model.ApproverId2).Any())
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgInvalidAccount);
                }

                model.ApprovalStatus = (int)ApprovalStatus.ApprovalWaiting1;
            }

            if (model.Avatar.Contains("data:application/octet-stream"))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.MessageNoExtension));
            }

            string avatarBase64 = "";
            string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
            var company = _companyService.GetById(companyId);
            if (model.Avatar.IsTextBase64())
            {
                Console.WriteLine("[UserController.Add] Starting avatar save process");
                avatarBase64 = model.Avatar;
                string userCode = !string.IsNullOrWhiteSpace(model.UserCode) ? model.UserCode : "user";
                Console.WriteLine($"[UserController.Add] UserCode: {userCode}");

                // Use secure file saving to prevent path traversal attacks
                string fileName = $"{userCode}.{Guid.NewGuid().ToString()}.jpg";
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/avatar";
                Console.WriteLine($"[UserController.Add] FileName: {fileName}");
                Console.WriteLine($"[UserController.Add] BasePath: {basePath}");
                Console.WriteLine($"[UserController.Add] CompanyCode: {company.Code}");
                Console.WriteLine($"[UserController.Add] Avatar length: {model.Avatar.Length}");

                bool success = FileHelpers.SaveFileImageSecure(model.Avatar, basePath, fileName, Constants.Image.MaximumImageStored);
                Console.WriteLine($"[UserController.Add] SaveFileImageSecure result: {success}");

                if (!success)
                {
                    Console.WriteLine("[UserController.Add] Avatar save failed!");
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, "Invalid user code or avatar save failed");
                }

                // Construct URL to match the static route: /static/{rootFolder}/{companyCode}/{category}/{fileName}
                model.Avatar = $"{connectionApi}/static/{Constants.Settings.DefineFolderImages}/{company.Code}/avatar/{fileName}";
                Console.WriteLine($"[UserController.Add] Final avatar URL: {model.Avatar}");
            }
            
            // stored avatar of National ID Card
            if (model.NationalIdCard != null && model.NationalIdCard.Avatar.IsTextBase64())
            {
                // Use secure file saving to prevent path traversal attacks
                string fileName = $"card_avatar.{Guid.NewGuid().ToString()}.jpg";
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/visitor";
                bool success = FileHelpers.SaveFileImageSecure(model.NationalIdCard.Avatar, basePath, fileName, Constants.Image.MaximumImageStored);

                if (success)
                {
                    // Construct URL to match the static route: /static/{rootFolder}/{companyCode}/{category}/{fileName}
                    model.NationalIdCard.Avatar = $"{connectionApi}/static/{Constants.Settings.DefineFolderImages}/{company.Code}/visitor/{fileName}";
                }
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                if (_userService.IsDuplicatedAccountCreated(0, model.Email))
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.Exist, UserResource.lblEmail));
                }
            }

            if (string.IsNullOrWhiteSpace(model.Username) && !string.IsNullOrWhiteSpace(model.Email))
            {
                model.Username = model.Email;
            }

            bool isSendCardToDevice = accessSetting.ApprovalStepNumber == (short)VisitSettingType.NoStep;
            var userId = _userService.Add(model, isSendCardToDevice);

            // Checking if card is existed in system.
            if (model.CardList != null && model.CardList.Any())
            {
                foreach (var card in model.CardList)
                {
                    var isCardIdExist = _userService.IsCardIdExist(card.CardId);

                    if (isCardIdExist)
                    {
                        return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.Exist, UserResource.lblCardId));
                    }
                    else
                    {
                        // This case is not used in DMPW. But this logic can be used other (3rd-party) project.
                        // This case means that the model data has cardModel list value.
                        // In this case, we just 'add' card. Not send to device.
                        var user = _userService.GetByIdAndCompany(userId, companyId);
                        foreach (var cardModel in model.CardList)
                        {
                            _userService.AddIdentification(user, cardModel, false);
                        }
                    }
                }
            }

            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, UserResource.lblUser.ToLower()), userId.ToString());
        }

        /// <summary>
        /// Edit user by id
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="model">JSON model for User</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id does not exist in DB</response>
        /// <response code="422">Data email of Model JSON wrong.</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiUsersId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.User)]
        public IActionResult Edit(int id, [FromBody] UserModel model)
        {
            model.Id = id;
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }

            var user = _userService.GetById(id);
            if (user == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
            }

            if (!_userService.CanEditData(user))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgCanNotUpdateWaiting, UserResource.lblUserInfomation));
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                if (_userService.IsDuplicatedAccountCreated(id, model.Email))
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.Exist, UserResource.lblEmail));
                }
            }

            if (string.IsNullOrWhiteSpace(model.Username) && !string.IsNullOrWhiteSpace(model.Email))
            {
                model.Username = model.Email;
            }

            if (model.Username != null)
            {
                var account = _userService.GetAccountByUserName(model.Username);
                if (account != null)
                {
                    var userLinkedAccount = _userService.GetUserByLinkedAccount(account.Id, user.CompanyId);
                    if (userLinkedAccount != null)
                    {
                        if (userLinkedAccount.Id != user.Id)
                        {
                            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.Exist, AccountResource.lblEmail));
                        }
                    }
                }
            }

            if (model.Avatar.Contains("data:application/octet-stream"))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.MessageNoExtension));
            }

            // check assign full access group when enable plugin Department Access Level
            var isFullAg = _accessGroupService.IsFullAccessGroup(user.CompanyId, model.AccessGroupId);
            var enablePlugin = _pluginService.CheckPluginCondition(Constants.PlugIn.DepartmentAccessLevel, user.CompanyId);
            if (isFullAg && enablePlugin && _httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.msgCannotAssignAgToUser);
            }
            if (model.Avatar.IsTextBase64())
            {
                string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                if (!string.IsNullOrWhiteSpace(user.Avatar))
                {
                    FileHelpers.DeleteFileFromLink(user.Avatar.Replace($"{connectionApi}/static/", ""));
                }

                var company = _companyService.GetById(_httpContext.User.GetCompanyId());

                // Use secure file saving to prevent path traversal attacks
                string fileName = $"{user.UserCode}.{Guid.NewGuid().ToString()}.jpg";
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/avatar";
                bool success = FileHelpers.SaveFileImageSecure(model.Avatar, basePath, fileName, Constants.Image.MaximumImageStored);

                if (success)
                {
                    // Construct URL to match the static route: /static/{rootFolder}/{companyCode}/{category}/{fileName}
                    model.Avatar = $"{connectionApi}/static/{Constants.Settings.DefineFolderImages}/{company.Code}/avatar/{fileName}";
                }
            }

            _userService.Update(model);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, UserResource.lblUser.ToLower(), ""));
        }

        /// <summary>
        /// add account to user
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="emailAccount">email account</param>
        [HttpPut]
        [Route(Constants.Route.ApiUsersAddAccount)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.User)]
        public IActionResult AddUserToAccount(int userId, string emailAccount)
        {

            var user = _userService.GetById(userId);
            var model = _mapper.Map<UserDataModel>(user);
            if (user == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
            }

            if (!_userService.CanEditData(user))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.msgCanNotUpdateWaiting, UserResource.lblUserInfomation));
            }

            

            if (string.IsNullOrWhiteSpace(model.Username) && !string.IsNullOrWhiteSpace(emailAccount))
            {
                model.Username = emailAccount;
                model.Email = emailAccount;
            }

            if (model.Username != null)
            {
                var account = _userService.GetAccountByUserName(model.Username);
                if (account != null)
                {
                    var userLinkedAccount = _userService.GetUserByLinkedAccount(account.Id, user.CompanyId);
                    if (userLinkedAccount != null)
                    {
                        if (userLinkedAccount.Id != user.Id)
                        {
                            return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.Exist, AccountResource.lblEmail));
                        }
                    }
                }
            }
            _userService.Update(model);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, UserResource.lblUser.ToLower(), ""));
        }



        /// <summary>
        /// Delete a user by Id
        /// </summary>
        /// <param name="id">Id of UserLogin to delete</param>
        /// <returns>Json result object</returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiUsersId)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.User)]
        public IActionResult Delete(int id)
        {
            var user = _userService.GetById(id);
            if (user == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
            }

            bool isDeleted = _userService.Delete(user, out string message);
            if(!isDeleted)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(UserResource.DeletedUserFailed, message));

            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, UserResource.lblUser.ToLower()));
        }

        /// <summary>
        /// Delete multiple user
        /// </summary>
        /// <param name="ids">List user id to delete</param>
        /// <returns>Json result object</returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: List of User Ids does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiUsers)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Delete + Page.User)]
        public IActionResult DeleteMultiple(List<int> ids)
        {
            var users = _userService.GetUsersByUserIds(ids);
            if (users == null || !users.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
            }

            bool isDeleted = _userService.DeleteRange(users, out string message);
            if(!isDeleted)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(UserResource.DeletedUserFailed, message));

            if (ids.Count > 1)
            {
                return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteMultipleSuccess, UserResource.lblUser.ToLower()));
            }
            else
            {
                return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, UserResource.lblUser.ToLower()));
            }
        }


        /// <summary>
        /// Get Door List for 1 user
        /// </summary>
        /// <param name="id">User id to Accessible Doors</param>
        /// <param name="search">Query string that filter by user name, address or type of device, mode of verify, passage or active</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="isValid">valid user or all user</param>
        /// <returns>Json result object</returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersAccessibleDoors)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessibleDoor)]
        public IActionResult AccessibleDoors(int id, string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "Id",
            string sortDirection = "desc", bool isValid = true)
        {
            sortColumn = SortColumnMapping.AccessibleDoorColumn(sortColumn);
            var user = _userService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
            if (user == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
            }

            var doors = _userService.GetPaginatedAccessibleDoors(user, search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                out var recordsFiltered);

            IPageHeader pageHeader = new PageHeader(_configuration, Page.AccessibleDoor + Page.Device, _httpContext.User.GetCompanyId());
            var header = pageHeader.GetHeaderList(_httpContext.User.GetCompanyId(), _httpContext.User.GetAccountId());

            var pagingData = new PagingData<AccessibleDoorModel, HeaderData>
            {
                Data = doors,
                Header = header,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get Door List for user current
        /// </summary>
        /// <param name="search">Query string that filter by user name, address or type of device, mode of verify, passage or active</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="isValid">valid user or all user</param>
        /// <returns>Json result object</returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersAccessibleDoorsWithUser)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.AccessibleDoor)]
        public IActionResult AccessibleDoorsWithUser(string search, int pageNumber = 1, int pageSize = 10,
            string sortColumn = "Id", string sortDirection = "desc", bool isValid = true)
        {
            var user = _userService.GetUserByAccountId(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId());
            if (user == null)
            {
                var pagingDataNull = new PagingData<AccessGroupDeviceDoor>
                {
                    Data = new List<AccessGroupDeviceDoor>()
                };
                return Ok(pagingDataNull);
            }
            sortColumn = Helpers.CheckPropertyInObject<AccessGroupDeviceDoor>(sortColumn, "DeviceAddress", ColumnDefines.AccessGroupForDoors);
            var devices = _accessGroupService.GetPaginatedForDoors(user.AccessGroupId, search, null, pageNumber, pageSize, sortColumn, sortDirection,
                out var recordsTotal, out var recordsFiltered);

            var pagingData = new PagingData<AccessGroupDeviceDoor>
            {
                Data = devices,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Export Door List for 1 user
        /// </summary>
        /// <param name="id">User id to Accessible Doors</param>
        /// <param name="search">Query string that filter by user name, address or type of device, mode of verify, passage or active</param>
        /// <param name="type">type of file export</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns>Json result object</returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersAccessibleDoorsExport)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Export + Page.AccessibleDoor)]
        public IActionResult ExportAccessibleDoors(int id, string search, string type = "excel", string sortColumn = "Id",
            string sortDirection = "desc")
        {
            sortColumn = SortColumnMapping.DeviceColumn(sortColumn);
            var user = _userService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
            if (user == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
            }

            var fileData = _userService.ExportAccessibleDoors(user, type, search, out var totalRecords, out var recordsFiltered, out string fullName, sortColumn, sortDirection);

            if (totalRecords == 0 || recordsFiltered == 0)
            {
                return new ApiSuccessResult(StatusCodes.Status200OK,
                    string.Format(MessageResource.MessageExportDataIsZero, UserResource.lblAccessibleDoors.ToLower()));
            }

            return File(fileData, type.Equals("excel") ? "application/ms-excel" : "text/csv", fullName);
        }



        /// <summary>
        /// Generate test data
        /// </summary>
        /// <param name="numberOfUser">Number of list users</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route("/users/create-test-data")]
        [Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CreateTestData(int numberOfUser)
        {
            if (numberOfUser == 0)
            {
                return Ok(new { message = "No user data is created!" });
            }

            var stopWatch = Stopwatch.StartNew();
            _userService.GenerateTestData(numberOfUser);
            stopWatch.Stop();
            Trace.WriteLine($"Elapsed time {stopWatch.ElapsedMilliseconds} ms");
            return Ok(new
            {
                message =
                    $"{numberOfUser} user(s) data were created successfully in {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds).TotalSeconds} seconds!"
            });
        }


        /// <summary>
        /// Generate test data
        /// </summary>
        /// <param name="numberOfUser">Number of list users</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route("/users/create-test-face-data")]
        [Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult CreateTestFaceData(int numberOfUser, bool createAccount)
        {
            if (numberOfUser == 0)
            {
                return Ok(new { message = "No user data is created!" });
            }

            var stopWatch = Stopwatch.StartNew();
            _userService.GenerateTestFaceData(numberOfUser, createAccount);
            stopWatch.Stop();
            Trace.WriteLine($"Elapsed time {stopWatch.ElapsedMilliseconds} ms");
            return Ok(new
            {
                message =
                    $"{numberOfUser} user(s) data were created successfully in {TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds).TotalSeconds} seconds!"
            });
        }


        /// <summary>
        /// Import users data
        /// </summary>
        /// <param name="file">file include list of users to import</param>
        /// <param name="type">type of file</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Data of file imported wrong. Or extension file wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiUsersImport)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.User)]
        public IActionResult ImportUser(IFormFile file, string type = "excel")
        {
            if (file.Length == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgFailLengthFile);
            }
            var fileType = FileHelpers.GetFileExtension(file);
            List<string> extensions = new List<string>();

            switch (type.ToLower())
            {
                case "excel":
                    extensions.Add(".xls");
                    extensions.Add(".xlsx");
                    break;
                //case "hancell":
                //extensions.Add(".cell");
                //break;
                default:
                    extensions.Add(".txt");
                    extensions.Add(".csv");
                    break;
            }

            if (!extensions.Contains(fileType.ToLower()))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    string.Format(MessageResource.msgErrorFileExtension, string.Join(" or ", extensions)));
            }

            try
            {
                // Validate file headers before processing
                var headerValidation = _userService.ValidateImportFileHeaders(file);
                if (!headerValidation.Result)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, headerValidation.Message);
                }

                var stream = FileHelpers.ConvertToStream(file);
                var companyId = _httpContext.User.GetCompanyId();
                var accountId = _httpContext.User.GetAccountId();
                var accountName = _httpContext.User.GetUsername();

                // Fire-and-forget
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var result = await _userService.ImportFile(type, stream, companyId, accountId, accountName);
                        if (!result.Result)
                        {
                            Console.WriteLine($"Import failed: {result.Message}");
                        }
                        else
                        {
                            Console.WriteLine("Import completed successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error importing user: {ex}");
                    }

                });
                return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.msgUserImportSuccess);
            }
            catch (Exception e)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, "ERROR to user import.");
            }
        }

        /// <summary>
        /// Export user data
        /// </summary>
        /// <param name="search">Query string that filter by user name, address or type of device, mode of verify, passage or active</param>
        /// <param name="cardId">Query string that filter by cardId</param>
        /// <param name="searchAll">search all</param>
        /// <param name="category"> category filter </param>
        /// <param name="type">type of file export</param>
        /// <param name="isValid"> only valid user or only invalid user or all user </param>
        /// <param name="empNumber">Query string that filter by empNumber</param>
        /// <param name="expiredDate">Query string that filter by expiredDate - format: dd.MM.yyyy HH:mm:ss</param>
        /// <param name="expiredDateEnd"> Query string tath filter by end of expiredDate - format : dd.MM.yyyy HH:mm:ss </param>
        /// <param name="departmentIds">Query string that filter by departmentIds</param>
        /// <param name="workTypeIds">Query string that filter by workTypeList</param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="position">Query string that filter by position</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        //[Produces("application/text", "application/ms-excel")]
        [HttpGet]
        [Route(Constants.Route.ApiUsersExportProjectD)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Export + Page.User)]
        public IActionResult ExportUser(string searchAll, List<int> category, string position = "", string cardId = "", string empNumber = "",string expiredDate = "", string expiredDateEnd = "", List<int> departmentIds = null, List<int> workTypeIds = null, string type = "excel", List<int> isValid = null, string search = "", string sortColumn = null, string sortDirection = "desc")
        {
            var filter = new UserFilterModel()
            {
                SortColumn = SortColumnMapping.UserColumn(sortColumn),
                SortDirection = sortDirection,
                FirstName = search,
                CardId = cardId,
                Position = position,
                Status = isValid,
                Search = searchAll,
                ExpiredDate = expiredDate.ConvertDefaultStringToDateTime(),
                ExpiredDateEnd = expiredDateEnd.ConvertDefaultStringToDateTime(),
                EmpNumber = empNumber,
                DepartmentIds = departmentIds,
                WorkTypeIds = workTypeIds,
                Category = category,
            };

            try
            {
                var fileData = _userService.Export(type, filter, out var totalRecords, out var recordsFiltered);
                var filename = string.Format(Constants.ExportFileFormat, UserResource.lblExport + "_User", DateTime.Now);

                string extension;
                string fileMIME;

                switch (type.ToLower())
                {
                    case "excel":
                        extension = "xlsx";
                        fileMIME = "application/ms-excel";
                        break;
                    case "hancell":
                        extension = "cell";
                        fileMIME = "application/octet-stream";
                        break;
                    default:
                        extension = "csv";
                        fileMIME = "text/csv";
                        break;
                }

                string fullName = $"{filename}.{extension}";

                if (totalRecords == 0 || recordsFiltered == 0)
                {
                    return new ApiSuccessResult(StatusCodes.Status200OK,
                        string.Format(MessageResource.MessageExportDataIsZero, UserResource.lblUser.ToLower()));
                }

                return File(fileData, fileMIME, fullName);
            }
            catch (Exception)
            {
                // There was not the catch for "Throw" in _userService.Export.
                return new ApiSuccessResult(StatusCodes.Status422UnprocessableEntity, $"{ActionLogTypeResource.Export} {ActionLogTypeResource.Fail}");
            }
        }


        /// <summary>
        /// Export assigned user list in department.
        /// </summary>
        /// <param name="search">Query string that filter by user name, email, name of department or expired date</param>
        /// <param name="type"> Export type. (excel, etc.. ) </param>
        /// <param name="sortColumn">Sort Column by string of the column.</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="isValid"> valid user or invalid user or all user </param>
        /// <param name="departmentId"> identifier of department </param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiExportUsersDeptAssign)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Export + Page.User)]
        public IActionResult ExportAssignDeptUsers(string search, string type = "excel", string sortColumn = null,
            string sortDirection = "desc", List<int> isValid = null, int departmentId = 0)
        {
            if(isValid == null)
            {
                // Except invaild user.
                isValid = [ (int)UserStatus.Use ];
            }

            var filter = new UserFilterModel()
            {
                PageNumber = 0,
                PageSize = 0,
                SortColumn = sortColumn,
                SortDirection = sortDirection,
                FirstName = search,
                Status = isValid,
                DepartmentIds = [ departmentId ]
            };

            try
            {
                var fileData = _userService.ExportUserData(type, filter, Page.Department + Page.User, out var totalRecords, out var recordsFiltered);
                var filename = string.Format(Constants.ExportFileFormat, UserResource.lblExport + "_" + DepartmentResource.lblDepartment + "_User", DateTime.Now);

                string extension;
                string fileMIME;

                switch (type.ToLower())
                {
                    case "excel":
                        extension = "xlsx";
                        fileMIME = "application/ms-excel";
                        break;
                    case "hancell":
                        extension = "cell";
                        fileMIME = "application/octet-stream";
                        break;
                    default:
                        extension = "csv";
                        fileMIME = "text/csv";
                        break;
                }

                string fullName = $"{filename}.{extension}";

                if (totalRecords == 0 || recordsFiltered == 0)
                {
                    return new ApiSuccessResult(StatusCodes.Status200OK,
                        string.Format(MessageResource.MessageExportDataIsZero, UserResource.lblUser));
                }

                return File(fileData, fileMIME, fullName);
            }
            catch (Exception)
            {
                // There was not the catch for "Throw" in _userService.Export.
                return new ApiSuccessResult(StatusCodes.Status422UnprocessableEntity, $"{ActionLogTypeResource.Export} {ActionLogTypeResource.Fail}");
            }
        }



        /// <summary>
        /// Get List of card types.
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiCardTypes)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetListCardType()
        {
            var type = typeof(CardType);
            var plugins = _companyService.GetPluginByCompany(_httpContext.User.GetCompanyId());
            List<int> excludeCardType = new List<int>
            {
                (short)CardType.VehicleId,
                (short)CardType.VehicleMotoBikeId
            };

            if (!string.IsNullOrWhiteSpace(plugins.PlugIns))
            {
                var plugin = JsonConvert.DeserializeObject<PlugIns>(plugins.PlugIns);
                if (plugin != null)
                {
                    if (!plugin.QrCode)
                    {
                        excludeCardType.Add((short)CardType.QrCode);
                        excludeCardType.Add((short)CardType.NFCPhone);
                    }

                    if (!plugin.PassCode)
                    {
                        excludeCardType.Add((short)CardType.PassCode);
                    }

                    if (!plugin.CameraPlugIn)
                    {
                        excludeCardType.Add((short)CardType.HFaceId);
                    }

                    if (!plugin.Vein)
                    {
                        excludeCardType.Add((short)CardType.Vein);
                    }
                }
            }
            var data = Enum
                .GetNames(type)
                .Select(name => new
                {
                    Id = (int)Enum.Parse(type, name),
                    Name = Enum.Parse(type, name).GetDescription()
                })
                .Where(m => !excludeCardType.Contains(m.Id))
                .ToArray();
            return Ok(data);
        }

        /// <summary>
        /// Get card by user
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiUserIdentification)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetCardsByUser(int id)
        {
            if (id != 0)
            {
                var user = _userService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
                if (user != null)
                {
                    var cards = _userService.GetCardListByUserId(user.Id)
                        .Where(m => m.CardType != (int)CardType.VehicleId && m.CardType != (int)CardType.VehicleMotoBikeId);
                    return Ok(cards);
                }
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
        }

        /// <summary>
        /// Add a new identification to exist user
        /// </summary>
        /// <param name="id">exist user Id</param>
        /// <param name="model">card model</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id does not exist in DB</response>
        /// <response code="422">Data of Model JSON wrong. Or cardId wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiUserIdentification)]
        [Route(Constants.Route.ApiArmyUserIdentification)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.User)]
        public IActionResult AddNewIdentification(int id, [FromBody] CardModel model)
        {
            if (model != null)
            {
                ModelState.Clear();
                // Checking validation is failed.
                if (!TryValidateModel(model))
                {
                    return new ValidationFailedResult(ModelState);
                }
                // Checking validation is succeed.
                else
                {
                    // Procedure 1. Verify that the card ID entered is already registered in the company.
                    var isCardExist = !string.IsNullOrWhiteSpace(model.CardId) && _userService.IsCardIdExist(model.CardId.RemoveAllEmptySpace());
                    if (isCardExist)
                    {
                        // If card ID is already in this company, return error No.422 with message that means "Exist already".
                        return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.Exist, UserResource.lblCardId));
                    }

                    // Procedure 2. If the company doesn't have the card ID value, check the 'id' parameter value.
                    if (id == 0)
                    {
                        // If the id value is 0, it returns 200 OK response.
                        // In this case, the card info is registered together with a new user data at once later.
                        // This means that the user data(id) is not yet in DB.
                        // FE uses this API to check only if the company has a card ID value.
                        return new ApiSuccessResult(StatusCodes.Status200OK);
                    }

                    // Procedure 3. Check the user data if the 'id' parameter value is not 0.
                    // Get User data from DB by using 'id' parameter.
                    var user = _userService.GetById(id);
                    if (user == null)
                    {
                        // If User data is null or deleted, return error No.404 with message that means "Not found user data".
                        return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
                    }

                    // Procedure 4. Check the type of card.
                    if (model.CardType == (short)CardType.Vein)
                    {
                        // If the card type is 7 (=Vein), return error No.422 with message that means "Can't add thie type of card".
                        // This is because the Vein type card is not the type that is registered by the DMP. This type is registered through the 3rd party program. (Not the DMP)
                        // The meaning of the vein type is to be used in eventLog.
                        return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.MessageCanNotAddCardType));
                    }
                    // The following types of cards are the type of cards that should have only 1 data per user.
                    List<short> onlyOneCardTypes =
                    [
                        (short)CardType.QrCode,
                        (short)CardType.PassCode,
                        (short)CardType.NFCPhone,
                        (short)CardType.FaceId,
                        (short)CardType.FingerPrint,
                        (short)CardType.AratekFingerPrint,
                        (short)CardType.VNID,
                    ];
                    // Verify that the card type it is currently adding is one of the above types.
                    if (onlyOneCardTypes.Contains((short)model.CardType))
                    {
                        // Check that the user already has card data of above types.
                        var isExist = _userService.IsCardExist((short)model.CardType, user.Id);
                        // If the user already have the type of this card, return error No.422 with message that means "Exist already".
                        if (isExist) return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.ExistCardData, ((CardType)model.CardType).GetDescription()));
                    }

                    // Procedure 5. If card is not in this company and the user has no reason for disqualification to have new data, update the exist user information
                    var card = _userService.AddIdentification(user, model);

                    // Procedure 6. Return result of fucntion.
                    // If the function of the service returns null, it is a failure.
                    if (card == null || string.IsNullOrWhiteSpace(card.CardId))
                    {
                        return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.MsgFail));
                    }
                    // If returned result is not null, it is a success. So API returns success.
                    else
                    {
                        return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, model.CardType == (short)CardType.QrCode ? UserResource.lblQR : UserResource.lblCard, UserResource.lblUser), card.CardId);
                    }

                }
            }
            else
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.InvalidInformation);
            }
        }

        /// <summary>
        /// Get card by user
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="cardId">Card Id</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiCardByUser)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetCardByUser(int id, int cardId)
        {
            if (id != 0)
            {
                var user = _userService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
                if (user != null)
                {
                    var card = _userService.GetCardByUser(id, cardId);
                    return Ok(card);
                }
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
        }

        /// <summary>
        /// Update card by user
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="cardId">Card Id</param>
        /// <param name="model">JSON model for Card</param>
        /// <returns></returns>
        /// <response code="400">Bad request: when card Id is changed </response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id or Card Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiCardByUser)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.User)]
        public IActionResult UpdateCardByUser(int id, int cardId, [FromBody] CardModel model)
        {
            if (id != 0)
            {
                // Checking validation is failed.
                if (!TryValidateModel(model))
                {
                    return new ValidationFailedResult(ModelState);
                }
                
                var user = _userService.GetByIdAndCompany(id, _httpContext.User.GetCompanyId());
                if (user != null)
                {
                    var isCardIdExist = _userService.IsExistGetCardByUser(id, cardId);

                    if (isCardIdExist)
                    {
                        try
                        {
                            _userService.UpdateCardByUser(id, cardId, model);

                            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, UserResource.lblCard.ToLower(), ""));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            return new ApiErrorResult(StatusCodes.Status400BadRequest, e.Message);
                        }
                    }
                    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundCard);
                }
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
        }

        /// <summary>
        /// Delete identification
        /// </summary>
        /// <param name="id">user index number</param>
        /// <param name="cardId">index number of card, not cardId</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id or Card Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiCardByUser)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.User)]
        public IActionResult DeleteCardByUser(int id, int cardId)
        {
            if (id != 0)
            {
                // check if the user is not null and not deleted
                var user = _userService.GetById(id);
                // if user is null or deleted, return error No.404
                if (user == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
                }
                else
                {
                    // Check for exist the card belong to user
                    var card = _userService.GetCardByUser(id, cardId);
                    // if card is not belong to user, return error No.404
                    if (card == null)
                    {
                        return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundCard);
                    }
                    else
                    {
                        // if card is belong to user, delete the card from user
                        _userService.DeleteCardByUser(user, cardId);

                        return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageDeleteSuccess, UserResource.lblCard.ToLower()));
                    }
                }
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
        }

        /// <summary>
        /// Get Dynamic QR by user
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiGetDynamicQrByUser)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckAddOnAttribute(Constants.PlugIn.QrCode)]
        public IActionResult GetDynamicQrByUser()
        {
            var user = _userService.GetUserByAccountId(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId());
            if (user != null)
            {
                var identification = _userService.GetQrByUserId(user.Id);
                if (identification != null)
                {
                    var dynamicQr = _userService.GetDynamicQrCode(user.Id, identification.CardId);
                    return Ok(dynamicQr);
                }
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
        }

        /// <summary>
        /// Get dynamic qr-code by email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiGetDynamicQrByEmail)]
        [Authorize(Policy = Constants.Policy.PrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckAddOnAttribute(Constants.PlugIn.QrCode)]
        public IActionResult GetDynamicQrByEmail(string email)
        {
            if (!_userService.IsEmailValid(email))
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.EmailInvalid);
            }

            var user = _userService.GetUserByEmail(email, _httpContext.User.GetCompanyId());
            if (user != null)
            {
                var identification = _userService.GetQrByUserId(user.Id);
                if (identification != null)
                {
                    var dynamicQr = _userService.GetDynamicQrCode(user.Id, identification.CardId);
                    return Ok(dynamicQr);
                }
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
        }

        /// <summary>
        /// Get NFC Phone Id by user
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiNFCPhoneIdByUser)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetNFCPhoneIdByUser()
        {
            var user = _userService.GetUserByAccountId(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId());
            if (user != null)
            {
                var identification = _userService.GetNFCPhoneIdByUserId(user.Id);
                if (identification != null)
                {
                    return Ok(new NFCPhoneID
                    {
                        NfcPhoneId = identification.CardId,
                    });
                }
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
        }

        /// <summary>
        /// Get card list by user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiCardListByUser)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetAllCardsByUser()
        {
            var user = _userService.GetUserByAccountId(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId());
            if (user != null)
            {
                var identifications = _userService.GetCardListByUserId(user.Id);
                return Ok(identifications);
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
        }

        /// <summary>
        /// Assign User to Default working time
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPatch]
        [Route(Constants.Route.AssignUserToDeFaultWorkingTime)]
        [Authorize(Policy = Constants.Policy.PrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckAddOnAttribute(Constants.PlugIn.TimeAttendance)]
        public IActionResult AssignUserToDefaultWorkingTime()
        {
            _userService.AssignUserToDefaultWorkingTime();
            return Ok();
        }

        /// <summary>
        /// View Access history Attendance by user
        /// </summary>
        /// <param name="id">User Id</param>
        /// <param name="start">String of date time start</param>
        /// <param name="end">String of date time end</param>
        /// <param name="eventType">event type</param>
        /// <param name="cardType">card type</param>
        /// <param name="inOut">String Antipass</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessHistoryUser)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.EventManagement)]
        public IActionResult AccessHistoryAttendance(int id, DateTime start, DateTime end, int eventType, int cardType, string inOut = "", 
            int pageNumber = 1, int pageSize = 10, string sortColumn = "AccessTime", string sortDirection = "desc")
        {
            var user = _userService.GetById(id);
            if (user != null)
            {
                var accessHistory = _eventLogService
                    .GetAccessHistoryAttendance(id, start, end, eventType, inOut, cardType,
                        pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                        out var recordsFiltered).ToList();

                var pagingData = new PagingData<EventLogHistory>
                {
                    Data = accessHistory,
                    Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
                };

                return Ok(pagingData);
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
        }

        /// <summary>
        /// View Access history Attendance Each user
        /// </summary>
        /// <param name="start">String of date time start</param>
        /// <param name="end">String of date time end</param>
        /// <param name="eventType">event type</param>
        /// <param name="cardType">card type</param>
        /// <param name="inOut">String Antipass</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string name of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: User Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccessHistoryEachUser)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        // [CheckAddOnAttribute(Constants.PlugIn.TimeAttendance)]
        public IActionResult AccessHistoryAttendanceEachUser(DateTime start, DateTime end, int eventType, int cardType, string inOut = "", 
            int pageNumber = 1, int pageSize = 10, string sortColumn = "AccessTime", string sortDirection = "desc")
        {
            var user = _userService.GetUserByAccountId(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId());
            if (user != null)
            {
                var accessHistory = _eventLogService
                    .GetAccessHistoryAttendance(user.Id, start, end, eventType, inOut, cardType, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                        out var recordsFiltered).AsEnumerable().ToList();

                var pagingData = new PagingData<EventLogHistory>
                {
                    Data = accessHistory,
                    Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
                };

                return Ok(pagingData);
            }
            return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
        }

        /// <summary>
        /// Validation Dynamic QR by user
        /// </summary>
        /// <param name="dynamicQr">String of dynamic Qr-code</param>
        /// <param name="rid">String of RFid</param>
        /// <param name="actionType">Type of Action</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: dynamicQr and rid not null</response>
        /// <response code="422">Data rid or dynamicQr wrong.</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiValidationDynamicQrByUser)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdminAndEmployee, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckAddOnAttribute(Constants.PlugIn.QrCode)]
        public IActionResult ValidationDynamicQrByUser(string dynamicQr, string rid, string actionType)
        {
            if (!string.IsNullOrWhiteSpace(dynamicQr) && !string.IsNullOrWhiteSpace(rid))
            {
                var user = _userService.GetUserByAccountId(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId());
                var validDevice = _userService.GetDeviceFromCompany(rid);
                if (!validDevice)
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.InvalidDevice);
                }

                if (dynamicQr.Substring(0, Constants.DynamicQr.NameProject.Length) != Constants.DynamicQr.NameProject)
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.InvalidQrCode);
                }

                if (dynamicQr.Length < Constants.DynamicQr.MaxLengthQr)
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.InvalidQrCode);
                }

                var validation = _userService.ValidationDynamicQr(dynamicQr.Substring(Constants.DynamicQr.NameProject.Length + 1));

                var validationQr = new ValidationQr
                {
                    Messages = validation
                };

                _eventLogService.CreateEventLogForDesktopApp(rid, user, validation, actionType);
                return Ok(validationQr);
            }
            return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.Required, "qr"));
        }

        /// <summary>
        /// Update avatar user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiUsersIdAvatar)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdminAndEmployee, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult AddAvatarForUser(int id, [FromBody] AccountAvatarModel model)
        {
            var user = _userService.GetUserByUserId(id);
            if (user == null) return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.RecordNotFound);
            if (string.IsNullOrWhiteSpace(model.Avatar))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgEmptyAvatar);
            }

            // check type string avatar
            string[] links = new[] { "http://", "https://" };
            if (links.Contains(model.Avatar.Substring(0, links[0].Length))
                || links.Contains(model.Avatar.Substring(0, links[1].Length)))
            {
                model.Avatar = Helpers.GetImageBase64FromUrl(model.Avatar);
                if (string.IsNullOrWhiteSpace(model.Avatar))
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgEmptyAvatar);
                }
            }

            // test data avatar
            if (!model.Avatar.IsTextBase64())
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgIsTextBase64Avatar);

            string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
            if (!string.IsNullOrWhiteSpace(user.Avatar))
            {
                FileHelpers.DeleteFileFromLink(user.Avatar.Replace($"{connectionApi}/static/", ""));
            }

            var company = _companyService.GetById(_httpContext.User.GetCompanyId());

            // Use secure file saving to prevent path traversal attacks
            string fileName = $"{user.UserCode}.{Guid.NewGuid().ToString()}.jpg";
            string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/avatar";
            bool success = FileHelpers.SaveFileImageSecure(model.Avatar, basePath, fileName, Constants.Image.MaximumImageStored);

            if (!success)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, "Invalid avatar or save failed");
            }

            // Construct URL to match the static route: /static/{rootFolder}/{companyCode}/{category}/{fileName}
            string avatarUrl = $"{connectionApi}/static/{Constants.Settings.DefineFolderImages}/{company.Code}/avatar/{fileName}";
            _userService.UpdateAvatar(user.Id, avatarUrl);

            string messageSuccess = string.Format(MessageResource.MessageUpdateSuccess, UserResource.lblUser.ToLower(), "").ReplaceSpacesString();
            return new ApiSuccessResult(StatusCodes.Status200OK, messageSuccess);
        }



        /// <summary>
        /// Send specific user data to all devices.
        /// </summary>
        /// <param name="ids"> list of users th send </param>
        /// <param name="isSend"> true -> ADD user / false -> DELETE user </param>
        /// <returns></returns>
        /// <response code="201">Create new a user</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Data of Model JSON wrong. Or card is existed in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiUsersSend)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.User)]
        public IActionResult SendUser([FromBody] List<int> ids, bool isSend = true)
        {
            if (!ids.Any())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.Required, UserResource.lblUser));
            }

            //// Delete all users.
            //_userService.SendUsersToAllDoors(ids, false);

            if (isSend)
            {
                // Delete before add
                _userService.SendDeleteUsersToAllDevice(ids, _httpContext.User.GetCompanyId());
            }

            // Add all users.
            _userService.SendUsersToAllDoors(ids, isSend);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, UserResource.lblUser.ToLower(), ""));
        }



        /// <summary>
        /// Add new multiple users
        /// </summary>
        /// <param name="models"> list of JSON model for User</param>
        /// <returns></returns>
        /// <response code="201">Create new a user</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Data of Model JSON wrong. Or card is existed in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiUsersMultiple)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Add + Page.User)]
        public IActionResult AddMultiple([FromBody] List<UserModel> models)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            var company = _companyService.GetById(_httpContext.User.GetCompanyId());
            foreach (var model in models)
            {
                if (model.Avatar.IsTextBase64())
                {
                    string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                    string userCode = !string.IsNullOrWhiteSpace(model.UserCode) ? model.UserCode : "user";

                    // Use secure file saving to prevent path traversal attacks
                    string fileName = $"{userCode}.{Guid.NewGuid().ToString()}.jpg";
                    string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/avatar";
                    bool success = FileHelpers.SaveFileImageSecure(model.Avatar, basePath, fileName);

                    if (success)
                    {
                        // Construct URL to match the static route: /static/{rootFolder}/{companyCode}/{category}/{fileName}
                        model.Avatar = $"{connectionApi}/static/{Constants.Settings.DefineFolderImages}/{company.Code}/avatar/{fileName}";
                    }
                }
            }

            var userIds = _userService.AddMultiple(models);

            return new ApiSuccessResult(StatusCodes.Status201Created,
                string.Format(MessageResource.MessageAddSuccess, UserResource.lblUser.ToLower()), string.Join(",", userIds));
        }
        
        /// <summary>
        /// Edit multiple users
        /// </summary>
        /// <param name="models"> list of JSON model for User</param>
        /// <returns></returns>
        /// <response code="201">Create new a user</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Data of Model JSON wrong. Or card is existed in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiUsersMultiple)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.User)]
        public IActionResult EditMultiple([FromBody] List<UserModel> models)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            var company = _companyService.GetById(_httpContext.User.GetCompanyId());

            foreach (var model in models)
            {
                if (model.Avatar.IsTextBase64())
                {
                    string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                    string userCode = !string.IsNullOrWhiteSpace(model.UserCode) ? model.UserCode : "user";

                    // Use secure file saving to prevent path traversal attacks
                    string fileName = $"{userCode}.{Guid.NewGuid().ToString()}.jpg";
                    string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/avatar";
                    bool success = FileHelpers.SaveFileImageSecure(model.Avatar, basePath, fileName);

                    if (success)
                    {
                        // Construct URL to match the static route: /static/{rootFolder}/{companyCode}/{category}/{fileName}
                        model.Avatar = $"{connectionApi}/static/{Constants.Settings.DefineFolderImages}/{company.Code}/avatar/{fileName}";
                    }
                }
            }

            var userIds = _userService.UpdateMultiple(models);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, UserResource.lblUser.ToLower(), ""), string.Join(",", userIds));
        }

        /// <summary>
        /// get list of types report problem
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiReportProblemTypes)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetReportProblemDeviceTypes()
        {
            var types = EnumHelper.ToEnumList<ReportProblemType>();
            return Ok(types);
        }
        
        /// <summary>
        /// Get Access setting
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiAccessSetting)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetAccessSetting()
        {
            var visitSetting = _userService.GetAccessSettingByCompany();

            return Ok(visitSetting);
        }

        /// <summary>
        /// Get account approval access setting
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiAccountApprovalAccessSetting)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Approve + Page.User)]
        public IActionResult GetApprovalAccessSetting()
        {
            return Ok(_accountService.GetApprovalAccessSetting(_httpContext.User.GetCompanyId()));
        }

        /// <summary>
        /// Get length of user's review
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiGetLengthUsersReview)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetLengthUsersReview()
        {
            return Ok(_userService.GetUsersReviewCount());
        }

        /// <summary>
        /// Update access setting
        /// </summary>
        /// <param name="model">JSON model for Setting visit</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Data of Approvals wrong</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="403">Forbidden: Data of Approvals wrong</response>
        /// <response code="404">Not Found: Access Time Id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiAccessSetting)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Edit + Page.AccessSetting)]
        public IActionResult EditAccessSetting([FromBody] AccessSettingModel model)
        {
            if (model.ApprovalStepNumber != 0)
            {
                if (model.FirstApproverAccounts == null)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, VisitResource.RequiredApprover);
                }
                // Check list account added is belong to company
                try
                {
                    var listAccountFirstApprove = JsonConvert.DeserializeObject<List<int>>(model.FirstApproverAccounts);
                    foreach (var id in listAccountFirstApprove)
                    {
                        var existed = _accountService.IsExistCompanyAccountbyAccountId(id, _httpContext.User.GetCompanyId());
                        if (!existed)
                        {
                            return new ApiErrorResult(StatusCodes.Status403Forbidden,
                                string.Format(MessageResource.AccountIsInValid, id));
                        }
                    }
                    if (model.ApprovalStepNumber != (short)VisitSettingType.SecondStep)
                    {
                        // update status of all user to Approval
                        _userService.UpdateAllUserCompanyToApproval(_httpContext.User.GetCompanyId(), model.ApprovalStepNumber);
                    }
                }
                catch (Exception e)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, e.Message);
                }
                // if (model.ApprovalStepNumber == (short)VisitSettingType.SecondsStep)
                // {
                //     var listAccountSecondsApprove = JsonConvert.DeserializeObject<List<int>>(model.SecondsApproverAccounts);
                //
                //     if (listAccountSecondsApprove == null || !listAccountSecondsApprove.Any())
                //     {
                //         return new ApiErrorResult(StatusCodes.Status400BadRequest, VisitResource.RequiredApprover);
                //     }
                //     // Check list account added is belong to company
                //     try
                //     {
                //         foreach (var id in listAccountSecondsApprove)
                //         {
                //             var existed = _accountService.IsExistCompanyAccountbyAccountId(id, _httpContext.User.GetCompanyId());
                //             if (!existed)
                //             {
                //                 return new ApiErrorResult(StatusCodes.Status403Forbidden,
                //                     string.Format(MessageResource.AccountIsInValid, id));
                //             }
                //         }
                //     }
                //     catch (Exception e)
                //     {
                //         return new ApiErrorResult(StatusCodes.Status400BadRequest, e.Message);
                //     }
                // }
            }

            var oldSetting = _userService.GetAccessSettingByCompany();

            if (oldSetting.ApprovalStepNumber != model.ApprovalStepNumber)
            {
                // Check whether there is any users that is not approved.
                if (_userService.ExistNotApprovedUser())
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.msgExistNotApproved);
                }
            }

            try
            {
                _userService.UpdateAccessSettingCompany(model);

                return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, UserResource.lblAccessSetting.ToLower(), ""));
            }
            catch (Exception)
            {
                return new ApiSuccessResult(StatusCodes.Status422UnprocessableEntity,
                string.Format(MessageResource.MessageUpdateFailed, UserResource.lblAccessSetting.ToLower()));
            }


        }



        /// <summary>
        /// Approve User access
        /// </summary>
        /// <param name="id"> User Id</param>
        /// <param name="model">JSON model include approved boolean</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Data of Approval wrong</response>
        /// <response code="401">Unauthorized: Lack of String Token Bearer</response>
        /// <response code="404">Not Found: Visit id does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        //[HttpPatch]
        [HttpPost]
        [Route(Constants.Route.ApiApproveUser)]
        //[Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.Approve + Page.User)]
        public IActionResult ApproveUser(int id, [FromBody] ApprovedModel model)
        {
            var accessSetting = _userService.GetAccessSettingByCompany();

            var user = _userService.GetById(id);
            int createdBy = user.CreatedBy;
            if (user.Id == 0)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundUser);
            }

            if (user.ApprovalStatus == (int)ApprovalStatus.Approved
                || user.ApprovalStatus == (int)ApprovalStatus.NotUse)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                    MessageResource.msgAlreadyApproved);
            }

            if (user.ApprovalStatus == (int)ApprovalStatus.ApprovalWaiting1)
            {
                if (_httpContext.User.GetAccountId() != user.ApproverId1)
                {
                    return new ApiErrorResult(StatusCodes.Status403Forbidden,
                        MessageResource.PermissionforApproved);
                }

                if (model.Approved)
                {
                    if (accessSetting.ApprovalStepNumber == (short)VisitSettingType.NoStep
                        || accessSetting.ApprovalStepNumber == (short)VisitSettingType.FirstStep)
                    {
                        _userService.UpdateApprovalUser(id, (short)ApprovalStatus.Approved);
                    }
                    else if (accessSetting.ApprovalStepNumber == (short)VisitSettingType.SecondStep)
                    {
                        _userService.UpdateApprovalUser(id, (short)ApprovalStatus.ApprovalWaiting2);
                    }
                }
                else
                {
                    _userService.UpdateApprovalUser(id, (short)ApprovalStatus.Rejected);
                }
            }
            else if (user.ApprovalStatus == (int)ApprovalStatus.ApprovalWaiting2)
            {
                if (_httpContext.User.GetAccountId() != user.ApproverId2)
                {
                    return new ApiErrorResult(StatusCodes.Status403Forbidden,
                        MessageResource.PermissionforApproved);
                }

                if (model.Approved)
                {
                    _userService.UpdateApprovalUser(id, (short)ApprovalStatus.Approved);
                }
                else
                {
                    _userService.UpdateApprovalUser(id, (short)ApprovalStatus.Rejected);
                }
            }
            else if (user.ApprovalStatus == (int)ApprovalStatus.UpdateWaiting1)
            {
                if (_httpContext.User.GetAccountId() != user.ApproverId1)
                {
                    return new ApiErrorResult(StatusCodes.Status403Forbidden,
                        MessageResource.PermissionforApproved);
                }

                if (model.Approved)
                {
                    if (accessSetting.ApprovalStepNumber == (short)VisitSettingType.NoStep
                        || accessSetting.ApprovalStepNumber == (short)VisitSettingType.FirstStep)
                    {
                        _userService.UpdateApprovalUser(id, (short)ApprovalStatus.Approved);
                    }
                    else if (accessSetting.ApprovalStepNumber == (short)VisitSettingType.SecondStep)
                    {
                        _userService.UpdateApprovalUser(id, (short)ApprovalStatus.UpdateWaiting2);
                    }
                }
                else
                {
                    _userService.UpdateApprovalUser(id, (short)ApprovalStatus.Rejected);
                }
            }
            else if (user.ApprovalStatus == (int)ApprovalStatus.UpdateWaiting2)
            {
                if (_httpContext.User.GetAccountId() != user.ApproverId2)
                {
                    return new ApiErrorResult(StatusCodes.Status403Forbidden,
                        MessageResource.PermissionforApproved);
                }

                if (model.Approved)
                {
                    _userService.UpdateApprovalUser(id, (short)ApprovalStatus.Approved);
                }
                else
                {
                    _userService.UpdateApprovalUser(id, (short)ApprovalStatus.Rejected);
                }
            }

            if (model.Approved)
            {
                return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.msgProcessed, UserResource.lblApproval.ToLower()));
            }
            else
            {
                return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.msgProcessed, UserResource.lblReject.ToLower()));
            }

        }


        /// <summary>
        /// Get user data by list of user code data
        /// </summary>
        /// <param name="usercodes"></param>
        /// <returns></returns>
        //[HttpGet]
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersByUserCode)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetsByUserCode([FromBody] List<string> usercodes = null)
        {
            var users = _userService.GetUsersByUserCodes(usercodes);
            var userData = users.ToList();

            var pagingData = new PagingData<UserListSimpleModel>
            {
                Data = userData,
                Meta = { RecordsTotal = userData.Count(), RecordsFiltered = userData.Count() }
            };

            return Ok(pagingData);
        }


        /// <summary>
        /// Get user data by several conditions
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersByCondition)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckPermission(ActionName.View + Page.User)]
        public IActionResult GetsUsersByCondition([FromBody] UserGetConditionModel conditions)
        {
            var users = _userService.GetUsersByConditions(conditions);
            var userData = users.ToList();

            var pagingData = new PagingData<UserListSimpleModel>
            {
                Data = userData,
                Meta = { RecordsTotal = userData.Count, RecordsFiltered = userData.Count }
            };

            return Ok(pagingData);
        }


        /// <summary>
        /// Remove QR data by user code
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersRemoveQR)]
        [Authorize(Policy = Constants.Policy.PrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult RemoveQR()
        {
            List<int> userIds = _userService.RemoveQR();

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, UserResource.lblQR, ""), string.Join(",", userIds));
        }


        /// <summary>
        /// Convert expired users to Invalid
        /// </summary>
        /// <returns></returns>
        //[HttpGet]
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUsersExpired)]
        [Authorize(Policy = Constants.Policy.PrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ConvertExpiredUser()
        {
            List<int> userIds = _userService.ConvertExpiredUserInvalid();

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, UserResource.lblUser.ToLower(), ""), string.Join(",", userIds));
        }
        /// <summary>
        /// update list avatar user
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiUpdateListAvatarUser)]
        [Authorize(Policy = Constants.Policy.PrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdateListAvatarUser(IFormFile file, string companyCode)
        {
            string extension = Path.GetExtension(file.FileName);
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

            if (imageExtensions.Contains(extension.ToLower()))
            {
                var company = _companyService.GetByCode(companyCode);
                var data = _userService.UpdateListAvatarUser(file, company);
                return Ok(data);
            }
            return new ApiErrorResult(StatusCodes.Status400BadRequest, "The file is not in the correct image format.");
        }

        /// <summary>
        /// Get init page register user
        /// </summary>
        /// <param name="companyCode"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route(Constants.Route.ApiRegisterUser)]
        public IActionResult RegisterUserInit(string companyCode)
        {
            var company = _companyService.GetByCode(companyCode);
            if (company == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundCompany);
            
            return Ok(_userService.GetRegisterUserInit(company));
        }
        
        /// <summary>
        /// Register user
        /// </summary>
        /// <param name="companyCode"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(Constants.Route.ApiRegisterUser)]
        public IActionResult RegisterUser(string companyCode, [FromBody] RegisterUserModel model)
        {
            var company = _companyService.GetByCode(companyCode);
            if (company == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.msgNotFoundCompany);
            
            if (!ModelState.IsValid)
                return new ValidationFailedResult(ModelState);

            if (!string.IsNullOrWhiteSpace(model.NationalIdNumber))
            {
                var userGetByIdNumber = _userService.GetByNationalIdNumber(model.NationalIdNumber, company.Id);
                if (userGetByIdNumber != null)
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.Exist, UserResource.lblId));
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var userGetByEmail = _userService.GetUserByEmail(model.Email, company.Id);
                if(userGetByEmail != null)
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.Exist, UserResource.lblEmail));
            }

            var userGetByPhone = _userService.GetUserByPhone(model.HomePhone, company.Id);
            if(userGetByPhone != null)
                return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.Exist, UserResource.lblHomePhone));
            
            if (!string.IsNullOrWhiteSpace(model.Avatar) && model.Avatar.IsTextBase64())
            {
                string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();

                // Use secure file saving to prevent path traversal attacks
                string fileName = $"user.{Guid.NewGuid().ToString()}.jpg";
                string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/avatar";
                bool success = FileHelpers.SaveFileImageSecure(model.Avatar, basePath, fileName, Constants.Image.MaximumImageStored);

                if (success)
                {
                    // Construct URL to match the static route: /static/{rootFolder}/{companyCode}/{category}/{fileName}
                    model.Avatar = $"{connectionApi}/static/{Constants.Settings.DefineFolderImages}/{company.Code}/avatar/{fileName}";
                }
            }
            else if (!string.IsNullOrWhiteSpace(model.Avatar))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.InvalidParameter + " " + UserResource.lblAvatar.ToLower());
            }

            var accountInfo = _userService.RegisterUser(model, company);
            if(accountInfo == null)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, string.Format(MessageResource.MessageAddNewFailed, UserResource.lblUser.ToLower()));
            
            return new ApiSuccessResult(StatusCodes.Status201Created, string.Format(MessageResource.MessageAddSuccess, UserResource.lblUser.ToLower()), Helpers.JsonConvertCamelCase(accountInfo));
        }

        /// <summary>
        /// get list user approver accounts
        /// </summary>
        /// <param name="search"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="sortColumn"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiUsersSettingFirstApprover)]
        [Route(Constants.Route.ApiUsersSettingSecondApprover)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUserApproverSetting(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "UserName", string sortDirection = "asc")
        {
            int companyId = _httpContext.User.GetCompanyId();
            // check access setting for approve.
            var accessSetting = _userService.GetAccessSettingByCompany();

            var apiPath = _httpContext.Request.Path.Value;
            string approverAccounts = string.Empty;
            switch (apiPath)
            {
                case Constants.Route.ApiUsersSettingSecondApprover:
                    approverAccounts = accessSetting.SecondApproverAccounts;
                    break;
                case Constants.Route.ApiUsersSettingFirstApprover:
                default:
                    approverAccounts = accessSetting.FirstApproverAccounts;
                    break;
            }

            List<int> accountIds = JsonConvert.DeserializeObject<List<int>>(approverAccounts);

            var accounts = _accountService.GetPaginatedAccountListByIds(search, accountIds, companyId, Page.AccessSetting + Page.User, pageNumber, pageSize, sortColumn, sortDirection,
                out var recordsFiltered, out var recordsTotal, out List<HeaderData> userHeader);

            var pagingData = new PagingDataWithHeader<AccountListModel>
            {
                Data = accounts,
                Header = userHeader,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }



        /// <summary>
        /// add user approver accounts
        /// </summary>
        /// <param name="ids">account ids</param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiUsersSettingFirstApprover)]
        [Route(Constants.Route.ApiUsersSettingSecondApprover)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult AddUserApproverSetting([FromBody] List<int> ids)
        {
            int companyId = _httpContext.User.GetCompanyId();

            // check access setting for approve.
            var accessSetting = _userService.GetAccessSettingByCompany();

            var apiPath = _httpContext.Request.Path.Value;
            string approverAccounts = string.Empty;
            switch (apiPath)
            {
                case Constants.Route.ApiUsersSettingSecondApprover:
                    approverAccounts = accessSetting.SecondApproverAccounts;
                    break;
                case Constants.Route.ApiUsersSettingFirstApprover:
                default:
                    approverAccounts = accessSetting.FirstApproverAccounts;
                    break;
            }

            List<int> accountIds = new List<int>();
            if (!string.IsNullOrEmpty(approverAccounts))
            {
                accountIds = JsonConvert.DeserializeObject<List<int>>(approverAccounts);
            }

            ids = ids.Where(m => !accountIds.Contains(m)).ToList();
            accountIds.AddRange(ids);
            approverAccounts = JsonConvert.SerializeObject(accountIds);

            switch (apiPath)
            {
                case Constants.Route.ApiUsersSettingSecondApprover:
                    accessSetting.SecondApproverAccounts = approverAccounts;
                    break;
                case Constants.Route.ApiUsersSettingFirstApprover:
                default:
                    accessSetting.FirstApproverAccounts = approverAccounts;
                    break;
            }

            _userService.UpdateAccessSettingCompany(accessSetting);
            return new ApiSuccessResult(StatusCodes.Status201Created, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }


        /// <summary>
        /// delete user approver accounts
        /// </summary>
        /// <param name="ids">account ids</param>
        /// <returns></returns>
        [HttpDelete]
        [Route(Constants.Route.ApiUsersSettingFirstApprover)]
        [Route(Constants.Route.ApiUsersSettingSecondApprover)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteUserApproverSetting(List<int> ids)
        {
            int companyId = _httpContext.User.GetCompanyId();

            // check access setting for approve.
            var accessSetting = _userService.GetAccessSettingByCompany();

            var apiPath = _httpContext.Request.Path.Value;
            string approverAccounts = string.Empty;
            switch (apiPath)
            {
                case Constants.Route.ApiUsersSettingSecondApprover:
                    approverAccounts = accessSetting.SecondApproverAccounts;
                    break;
                case Constants.Route.ApiUsersSettingFirstApprover:
                default:
                    approverAccounts = accessSetting.FirstApproverAccounts;
                    break;
            }

            List<int> accountIds = new List<int>();
            if (!string.IsNullOrEmpty(approverAccounts))
            {
                accountIds = JsonConvert.DeserializeObject<List<int>>(approverAccounts);
            }

            accountIds = accountIds.Where(m => !ids.Contains(m)).ToList();
            approverAccounts = JsonConvert.SerializeObject(accountIds);

            switch (apiPath)
            {
                case Constants.Route.ApiUsersSettingSecondApprover:
                    accessSetting.SecondApproverAccounts = approverAccounts;
                    break;
                case Constants.Route.ApiUsersSettingFirstApprover:
                default:
                    accessSetting.FirstApproverAccounts = approverAccounts;
                    break;
            }

            _userService.UpdateAccessSettingCompany(accessSetting);
            return new ApiSuccessResult(StatusCodes.Status200OK, string.Format(MessageResource.MessageUpdateSuccess, "", "").ReplaceSpacesString());
        }
    }
}