using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Web;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataModel;
using DeMasterProCloud.DataModel.Account;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Login;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ForgotPasswordModel = DeMasterProCloud.DataModel.Login.ForgotPasswordModel;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
using DeMasterProCloud.DataAccess.Models;
using FirebaseAdmin;
using System.Threading.Tasks;
using DeMasterProCloud.DataModel.Device;
using Microsoft.Extensions.Configuration;

namespace DeMasterProCloud.Api.Controllers
{
    /// <summary>
    /// Define a login route for api app
    /// </summary>
    [Produces("application/json")]
    public class AccountController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IAccountService _accountService;
        private readonly IRoleService _roleService;
        private readonly IUserService _userService;
        private readonly ICameraService _cameraService;
        private readonly IJwtHandler _jwtHandler;
        private readonly HttpContext _httpContext;
        private readonly IConfiguration _configuration;
        private readonly ISettingService _settingService;
        private readonly ISystemInfoService _systemInfoService;
        private readonly IMapper _mapper;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="companyService"></param>
        /// <param name="accountService"></param>
        /// <param name="roleService"></param>
        /// <param name="cameraService"></param>
        /// <param name="settingService"></param>
        /// <param name="jwtHandler"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="userService"></param>
        /// <param name="systemInfoService"></param>
        /// <param name="configuration"></param>
        /// <param name="mapper"></param>
        public AccountController(ICompanyService companyService, IAccountService accountService,
            IRoleService roleService, ICameraService cameraService, ISettingService settingService,
            IJwtHandler jwtHandler, IHttpContextAccessor httpContextAccessor, IUserService userService,
            ISystemInfoService systemInfoService, IConfiguration configuration, IMapper mapper)
        {
            _companyService = companyService;
            _accountService = accountService;
            _roleService = roleService;
            _jwtHandler = jwtHandler;
            _userService = userService;
            _cameraService = cameraService;
            _settingService = settingService;
            _httpContext = httpContextAccessor.HttpContext;
            _configuration = configuration;
            _systemInfoService = systemInfoService;
            _mapper = mapper;
        }

        /// <summary>
        /// Login by account and password
        /// </summary>
        /// <param name="model">JSON model for login(username, password)</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: username or password wrong</response>
        /// <response code="422">Unprocessable Entity: Model Body required username and password</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [AllowAnonymous]
        [Route(Constants.Route.ApiLogin)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Login([FromBody] LoginModel model)
        {
            bool isLicenseVerified = _systemInfoService.CheckLicenseVerified();
            if (!isLicenseVerified)
            {
                return Ok(new TemporaryTokenModel { LicenseVerified = false });
            }

            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            //Check user is no one, then make default account
            if (!_accountService.HasDefaultAccount())
            {
                return new ApiUnauthorizedResult((int)LoginUnauthorized.AccountNonExist, AccountResource.msgNonAccount);
            }


            // check lock account 
            var acc = _accountService.GetAccountByEmail(model.Username);
            if (acc != null)
            {
                var loginConfig = _accountService.ParseLoginConfig(acc.LoginConfig);
                List<Company> compAcc = _accountService.GetCompanyList(acc);
                if (compAcc.Count > 0)
                {
                    
                    var loginSetting = _settingService.GetLoginSetting(compAcc[0].Id);


                    // check password expired 
                    var passwordStatus = _accountService.GetPasswordStatus(acc, compAcc[0].Id);
                    if (passwordStatus.IsExpired)
                    {
                        return new ApiUnauthorizedResult((int)LoginUnauthorized.PasswordExpired,
                            passwordStatus.Message);
                    }

                    // Check if a is locked due to too many failed attempts
                    if (_accountService.IsAccountLocked(loginConfig, loginSetting))
                    {
                        int lockoutMinutes = 0;
                        if (loginSetting != null && loginSetting.TimeoutWhenWrongPassword != null)
                        {
                            lockoutMinutes = (int)loginSetting.TimeoutWhenWrongPassword;
                        }
                        string lockedMessage = lockoutMinutes > 0
                            ? string.Format(AccountResource.msgAccountLockedWithDuration, lockoutMinutes)
                            : AccountResource.msgAccountLocked;
                        return new ApiUnauthorizedResult((int)LoginUnauthorized.AccountLocked, lockedMessage);
                    }
                }

            }

            //Get the account in the system
            var account = _accountService.GetAuthenticatedAccount(model);
            if (account == null)
            {
                return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidCredentials,
                    MessageResource.InvalidCredentials);
            }

            //Check where if the account is valid status
            if (account.IsDeleted)
            {
                return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidCredentials,
                    MessageResource.InvalidCredentials);
            }

            //Check ip, time active with account
            if (!_accountService.CheckSessionInvalidLogin(account) && !model.EnableRemoveOldSession)
            {
                return new ApiUnauthorizedResult((int)LoginUnauthorized.AccountUseAnotherDevice,
                    AccountResource.msgAccountUseOther);
            }

            // Get list of company that user belong to
            List<Company> companies = _accountService.GetCompanyList(account);

            TokenModel authToken = null;
            if (companies.Count == 1)
            {
                // Validate if company & account is existed
                Company company = companies[0];

                if (company == null)
                {
                    return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidCredentials,
                        MessageResource.InvalidCredentials);
                }

                // check activated account with company
                if (!_accountService.CheckAccountActivated(account, company.Id))
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                        MessageResource.msgAccountNotActivated);
                }

                // if (model.Password == Helpers.GeneratePasswordDefaultWithCompany(company.Name))
                // {
                //     return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AccountResource.msgChangePasswordDefault);
                // }


                var loginConfig = _accountService.ParseLoginConfig(account.LoginConfig);
                var loginSetting = _settingService.GetLoginSetting(company.Id);

                // Check if account is locked due to too many failed attempts
                if (_accountService.IsAccountLocked(loginConfig, loginSetting))
                {
                    int lockoutMinutes = 0;
                    if (loginSetting != null && loginSetting.TimeoutWhenWrongPassword != null)
                    {
                        lockoutMinutes = (int)loginSetting.TimeoutWhenWrongPassword;
                    }
                    string lockedMessage = lockoutMinutes > 0
                        ? string.Format(AccountResource.msgAccountLockedWithDuration, lockoutMinutes)
                        : AccountResource.msgAccountLocked;
                    return new ApiUnauthorizedResult((int)LoginUnauthorized.AccountLocked, lockedMessage);
                }


                // check password expired 
                var passwordStatus = _accountService.GetPasswordStatus(account, company.Id);
                if (passwordStatus.IsExpired)
                {
                    return new ApiUnauthorizedResult((int)LoginUnauthorized.PasswordExpired,
                        passwordStatus.Message);
                }
                

                // Check Valid password period (legacy support)
                if (company.UseExpiredPW)
                {
                    if (account.UpdatePasswordOn == new DateTime())
                    {
                        // nothing to do now. But something can be done.
                    }

                    if (!account.RootFlag)
                    {
                        if (DateTime.UtcNow.Subtract(account.UpdatePasswordOn).Days >= company.PwValidPeriod)
                        {
                            return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidCredentials,
                                string.Format(AccountResource.msgPasswordExpired, company.PwValidPeriod));
                        }
                    }
                }

                authToken = _accountService.CreateAuthToken(account, company, updateRefreshToken: false);

                // hash qrcode offline
                string hashQrCode = _accountService.HashJsonToQrCode(account, company.Id);
                if (authToken != null)
                {
                    authToken.HashQrCode = hashQrCode;
                }

                bool allowThisIp = _companyService.CheckIpAllowInCompany(_httpContext.GetIpAddressRequest(), company.Id);
                if (!allowThisIp)
                {
                    return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidCredentials, MessageResource.msgNotAllowIpAddres);
                }

                _accountService.SaveCurrentLogin(account.Id);
                return Ok(authToken);
            }
            else if (companies.Count > 1)
            {
                var claims = new[]
                {
                    new Claim(Constants.ClaimName.Username, account.Username),
                    new Claim(Constants.ClaimName.AccountId, account.Id.ToString()),
                    new Claim(Constants.ClaimName.AccountType, account.Type.ToString())
                };
                var token = _jwtHandler.BuilToken(claims);

                List<CompanyApiDetailModel> companyApiModels = new List<CompanyApiDetailModel>();
                foreach (Company company in companies)
                {
                    CompanyApiDetailModel newCompany = new CompanyApiDetailModel
                    {
                        Id = company.Id,
                        Code = company.Code,
                        Name = company.Name,
                        Logo = _settingService.GetCurrentQRLogo(company.Id)?.Logo,
                    };
                    companyApiModels.Add(newCompany);
                }

                return Ok(new TemporaryTokenModel
                {
                    Status = 0,
                    TemporaryToken = token,
                    AccountId = account.Id,
                    LicenseVerified = true,
                    Companies = companyApiModels.OrderBy(x => x.Name).ToList()
                });
            }
            else
            {
                if (account.Type == (short)AccountType.SystemAdmin)
                    authToken = _accountService.CreateAuthTokenAdmin(account, updateRefreshToken: false);
                return Ok(authToken);
            }
        }

        /// <summary>
        /// Login by company and token temporary
        /// </summary>
        /// <param name="model">JSON model for login(companyId and temporary token)</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: companyId or account in temporaryToken does not exist</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        /// <response cdoe="500">Internal Server Error: temporaryToken not valid</response>
        [HttpPost]
        [AllowAnonymous]
        [Route(Constants.Route.ApiLoginWithCompany)]
        //[Authorize(Policy = Constants.Policy.SuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult SecondLogin([FromBody] LoginModelWithCompany model)
        {
            bool isLicenseVerified = _systemInfoService.CheckLicenseVerified();
            if (!isLicenseVerified)
            {
                return Ok(new TemporaryTokenModel { LicenseVerified = false });
            }

            string temporaryToken = model.TemporaryToken;
            int companyId = model.CompanyId;
            ClaimsPrincipal temporaryPrincipal = _jwtHandler.GetPrincipalFromExpiredToken(temporaryToken);
            string username = temporaryPrincipal.GetUsername();
            // Validate if the token is valid
            if (_jwtHandler.IsTokenExpired(temporaryToken))
            {
                return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidCredentials,
                    MessageResource.InvalidCredentials);
            }

            TokenModel authToken = null;
            Account account = _accountService.GetAccountByEmail(username);
            if (companyId == 0)
            {
                if (account.Type == (short)AccountType.SystemAdmin)
                    authToken = _accountService.CreateAuthTokenAdmin(account, updateRefreshToken: false);
            }
            else
            {
                // Validate if company & account is existed
                Company company = _companyService.GetById(companyId);
                if (account == null || company == null)
                {
                    return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidCredentials,
                        MessageResource.InvalidCredentials);
                }

                // check activated account with company
                if (!_accountService.CheckAccountActivated(account, company.Id))
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                        MessageResource.msgAccountNotActivated);
                }

                var loginConfig = _accountService.ParseLoginConfig(account.LoginConfig);
                var loginSetting = _settingService.GetLoginSetting(company.Id);

                // Check if account is locked due to too many failed attempts
                if (_accountService.IsAccountLocked(loginConfig, loginSetting))
                {
                   int lockoutMinutes = 0;
                   if (loginSetting != null && loginSetting.TimeoutWhenWrongPassword != null)
                   {
                       lockoutMinutes = (int)loginSetting.TimeoutWhenWrongPassword;
                   }
                   string lockedMessage = lockoutMinutes > 0
                       ? string.Format(AccountResource.msgAccountLockedWithDuration, lockoutMinutes)
                       : AccountResource.msgAccountLocked;
                   return new ApiUnauthorizedResult((int)LoginUnauthorized.AccountLocked, lockedMessage);
                }

                // check password expired 
                var passwordStatus = _accountService.GetPasswordStatus(account, company.Id);
                if (passwordStatus.IsExpired)
                {
                    return new ApiUnauthorizedResult((int)LoginUnauthorized.PasswordExpired,
                        passwordStatus.Message);
                }

                // Check Valid password period
                if (company.UseExpiredPW)
                {
                    if (account.UpdatePasswordOn == new DateTime())
                    {

                    }

                    if (!account.RootFlag)
                    {
                        if (DateTime.UtcNow.Subtract(account.UpdatePasswordOn).Days >= company.PwValidPeriod)
                        {
                            return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidCredentials,
                                string.Format(AccountResource.msgPasswordExpired, company.PwValidPeriod));
                        }
                    }
                }

                authToken = _accountService.CreateAuthToken(account, company, updateRefreshToken: false);
                
                // hash qrcode offline
                string hashQrCode = _accountService.HashJsonToQrCode(account, companyId);
                if (authToken != null)
                {
                    authToken.HashQrCode = hashQrCode;
                }
            }
            
            bool allowThisIp = _companyService.CheckIpAllowInCompany(_httpContext.GetIpAddressRequest(), model.CompanyId);
            if (!allowThisIp)
            {
                return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidCredentials, MessageResource.msgNotAllowIpAddres);
            }
            
            _accountService.SaveCurrentLogin(account.Id);
            return Ok(authToken);
        }

        /// <summary>
        /// Refresh token
        /// </summary>
        /// <param name="refreshToken">String of token refreshed</param>
        /// <param name="expiredToken">String timezone by expired of token</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: refreshToken or expiredToken not valid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiRefreshToken)]
        public IActionResult RefreshToken(string refreshToken, string expiredToken)
        {
            var responseStatus = new ResponseStatus();
            // If refreshToken or expiredToken is null return 
            if (refreshToken == null || expiredToken == null)
            {
                return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidToken, MessageResource.InvalidToken);
            }

            // Check if refresh token is valid
            if (_jwtHandler.IsTokenExpired(refreshToken))
            {
                return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidToken, MessageResource.ExpiredToken);
            }
            // Check if refresh token and token is a valid pair

            ClaimsPrincipal refreshTokenPrincipal = _jwtHandler.GetPrincipalFromExpiredToken(refreshToken);
            ClaimsPrincipal expiredTokenPrincipal = _jwtHandler.GetPrincipalFromExpiredToken(expiredToken);
            if (refreshTokenPrincipal == null || expiredTokenPrincipal == null)
            {
                return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidToken, MessageResource.InvalidToken);
            }

            int expiredAccountId = expiredTokenPrincipal.GetAccountId();
            int expiredCompanyId = expiredTokenPrincipal.GetCompanyId();
            int accountId = refreshTokenPrincipal.GetAccountId();
            int companyId = refreshTokenPrincipal.GetCompanyId();

            if (expiredAccountId != accountId && expiredCompanyId != companyId)
            {
                // The expired token and the refresh token is not same pair
                return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidToken,
                    MessageResource.ExpiredTokenAndRefreshTokenMismatched);
            }

            Account account = _accountService.GetById(accountId);
            if (account == null)
            {
                // The refresh token is wrong
                return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidToken, MessageResource.InvalidToken);
            }

            if (companyId != 0)
            {
                Company company = _companyService.GetById(companyId);
                TokenModel authToken = _accountService.CreateAuthToken(account, company, updateRefreshToken: false);
                responseStatus.message = "Refresh Token Success";
                responseStatus.statusCode = true;
                responseStatus.data = authToken;
            }
            else
            {
                TokenModel authToken = _accountService.CreateAuthTokenAdmin(account, updateRefreshToken: false);
                responseStatus.message = "Refresh Token Success";
                responseStatus.statusCode = true;
                responseStatus.data = authToken;
            }

            bool allowThisIp = _companyService.CheckIpAllowInCompany(_httpContext.GetIpAddressRequest(), companyId);
            if (!allowThisIp)
            {
                return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidCredentials, MessageResource.msgNotAllowIpAddres);
            }
            
            _accountService.SaveCurrentLogin(account.Id);
            
            return Ok(responseStatus);
        }

        /// <summary>
        /// Switch company
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: String Bearer Token requirement</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiSwitchCompany)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult SwitchCompany(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = "name", string sortDirection = "asc")
        {
            Account account = _accountService.GetById(_httpContext.User.GetAccountId());
            // Get list of company that user belong to
            List<Company> companies = _accountService.GetPaginatedCompanyList(account, search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
                out var recordsFiltered);
            var claims = new[]
            {
                new Claim(Constants.ClaimName.Username, account.Username),
                new Claim(Constants.ClaimName.AccountId, account.Id.ToString()),
                new Claim(Constants.ClaimName.AccountType, account.Type.ToString())
            };
            var token = _jwtHandler.BuilToken(claims);

            List<CompanyApiDetailModel> companyApiModels = new List<CompanyApiDetailModel>();
            foreach (Company company in companies)
            {
                CompanyApiDetailModel newCompany = new CompanyApiDetailModel
                {
                    Id = company.Id,
                    Code = company  .Code,
                    Name = company.Name,
                    Logo = _settingService.GetCurrentQRLogo(company.Id)?.Logo,
                };
                companyApiModels.Add(newCompany);
            }

            return Ok(new TemporaryTokenModel
            {
                Status = 0,
                TemporaryToken = token,
                AccountId = account.Id,
                Companies = companyApiModels,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            });
        }


        /// <summary>
        /// Get list of Accounts with pagination
        /// </summary>
        /// <param name="search">Query string that filter by username or email</param>
        /// <param name="pageNumber">Page number start from 1</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by string of the column</param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <param name="companyIds">List of company ids</param>
        /// <param name="ignoreApprovalVisit">Ignore list account approval of visit setting</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccounts)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Gets(string search, int pageNumber = 1, int pageSize = 10, string sortColumn = null,
            string sortDirection = "desc", List<int> companyIds = null, bool ignoreApprovalVisit = default)
        {
            //if (_httpContext.User.GetAccountType() == (short)AccountType.SystemAdmin)
            //{
            //var accounts = _accountService
            //.GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection, out var recordsTotal,
            //    out var recordsFiltered, companyIds).AsEnumerable().Select(Mapper.Map<AccountListModel>).ToList();

            var accounts = _accountService.GetPaginated(search, pageNumber, pageSize, sortColumn, sortDirection,
                out var recordsTotal,
                out var recordsFiltered, companyIds, ignoreApprovalVisit).ToList();

            var companyId = _httpContext.User.GetCompanyId();

            foreach (var accountModel in accounts)
            {
                if (accountModel != null)
                {
                    var companyAccount = _accountService.GetCompanyAccountById(accountModel.Id);
                    var dynamicRoleId = companyAccount.DynamicRoleId;
                    //accountModel.UserName = companyAccount.Account.FirstName;

                    try
                    {
                        if (_httpContext.User.GetAccountType() == (short)AccountType.SystemAdmin)
                        {
                            if (companyAccount.CompanyId != null)
                            {
                                //accountModel.Role = ((AccountType)account.Type).GetDescription();
                                accountModel.Role = ((AccountType)companyAccount.DynamicRole.TypeId).GetDescription();
                            }
                            else
                            {
                                accountModel.Role = AccountType.SystemAdmin.GetDescription();
                            }

                        }
                        else
                        {
                            if (dynamicRoleId == null)
                                //accountModel.Role = ((AccountType)account.Type).GetDescription();
                                accountModel.Role = ((AccountType)companyAccount.DynamicRole.TypeId).GetDescription();
                            else
                                accountModel.Role = _roleService.GetByIdAndCompanyId(dynamicRoleId.Value, companyId)
                                    .Name;
                        }
                    }
                    catch (Exception)
                    {

                    }

                    //// Get Company list name
                    //List<Company> companies = _accountService.GetCompanyList(account);
                    //accountModel.CompanyNames = new List<string>();
                    //foreach (Company company in companies) accountModel.CompanyNames.Add(company.Name);
                }
            }

            var pagingData = new PagingData<AccountListModel>
            {
                Data = accounts,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }


        /// <summary>
        /// Get Account (or company account) information
        /// </summary>
        /// <param name="id"> an identifier of company account </param>
        /// <returns></returns>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Id not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccountsId)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Get(int id)
        {
            var model = new AccountDataModel();
            if (id != 0)
            {
                var companyAccount = _accountService.GetCompanyAccountById(id);

                if (companyAccount == null)
                    return new ApiErrorResult(StatusCodes.Status404NotFound);

                if (_httpContext.User.GetAccountType() == (short)AccountType.SystemAdmin)
                {
                    model = _mapper.Map<AccountDataModel>(companyAccount);

                    try
                    {
                        if (companyAccount.DynamicRoleId != null && companyAccount.DynamicRole != null)
                        {
                            // Show the 'type' value when current account is system admin.
                            model.Role = companyAccount.DynamicRole.TypeId;
                        }
                    }
                    catch
                    {

                    }
                }
                else
                {
                    if (companyAccount == null || companyAccount.CompanyId != _httpContext.User.GetCompanyId())
                        return new ApiErrorResult(StatusCodes.Status404NotFound);

                    model = _mapper.Map<AccountDataModel>(companyAccount);

                    if (companyAccount.DynamicRoleId == null)
                    {
                        try
                        {
                            if (companyAccount.DynamicRole.TypeId == (int)AccountType.DynamicRole)
                            {
                                var dynamicRole = _roleService.GetByTypeAndCompanyId((int)AccountType.Employee,
                                    _httpContext.User.GetCompanyId()).First();
                                model.Role = dynamicRole.Id;
                            }
                            else
                            {
                                var dynamicRole = _roleService.GetByTypeAndCompanyId(companyAccount.DynamicRole.TypeId,
                                    _httpContext.User.GetCompanyId()).First();
                                model.Role = dynamicRole.Id;
                            }
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        model.Role = companyAccount.DynamicRoleId.Value;
                    }
                }

                if (companyAccount.Id == _httpContext.User.GetAccountId())
                    model.IsCurrentAccount = true;
            }

            _accountService.InitData(model);

            return Ok(model);
        }

        /// <summary>
        /// Get Account profile information
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route(Constants.Route.ApiAccountProfile)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetProfile()
        {
            var model = new AccountModel();
            int id = _httpContext.User.GetAccountId();

            if (id != 0)
            {
                var account = _accountService.GetById(id);
                var companyAccount =
                    _accountService.GetCompanyAccountByIdAndCompany(account.Id, _httpContext.User.GetCompanyId());

                if (account == null || (companyAccount == null && account.DynamicRoleId != null))
                    return new ApiErrorResult(StatusCodes.Status404NotFound);

                model = _mapper.Map<AccountModel>(account);
                model.PreferredSystem = companyAccount?.PreferredSystem ?? 0;
                model.Id = id;
                if (companyAccount is { CompanyId: not null })
                {
                    bool enableCustomizeLanguage = _companyService.CheckPluginByCompany(companyAccount.CompanyId.Value, Constants.PlugIn.CustomizeLanguage);
                    model.AllLanguage = enableCustomizeLanguage ? _companyService.GetListLanguageForCompany(companyAccount.CompanyId.Value) : _companyService.GetListLanguageForCompany(-1);
                }
                else
                {
                    model.AllLanguage = _companyService.GetListLanguageForCompany(-1);
                }
            }

            return Ok(model);
        }

        /// <summary>
        /// Add a account.
        /// </summary>
        /// <param name="model">JSON model for Account</param>
        /// <returns></returns>
        /// <response code="201">Create new item success</response>
        /// <response code="400">Bad Request: company does not exist</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [Route(Constants.Route.ApiAccounts)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Add([FromBody] AccountModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            short roleId = 0;

            var currentAccountType = _httpContext.User.GetAccountType();
            if (currentAccountType == (short)AccountType.SystemAdmin)
            {
                // system UI

                if (model.Role != 0)
                {
                    // new account ( not system admin )
                    var company = _companyService.GetById(model.CompanyId);
                    if (company == null)
                    {
                        return new ApiErrorResult(StatusCodes.Status400BadRequest,
                            string.Format(MessageResource.CompanyInformationMissing));
                    }
                    else
                    {
                        // When an account is created by system admin,
                        // If Role and company are not null, API should change the role Id.
                        // For example, secondary manager's type id is 3, but secondary manager's dynamic role Id(ex.146) is different by company.
                        // So, API changes roleId 3 -> 146(example).

                        // In this case, 'model.Role' has a account type.
                        var dynamicRoleId = _roleService.GetByTypeAndCompanyId(model.Role, company.Id).FirstOrDefault()
                            .Id;
                        roleId = (short)dynamicRoleId;
                    }
                }
                else
                {
                    // new account (systen admin)
                    roleId = (short)AccountType.SystemAdmin;
                }
            }
            else
            {


                // company manager
                var currentCompanyId = _httpContext.User.GetCompanyId();
                var company = _companyService.GetById(currentCompanyId);
                if (company == null)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest,
                        string.Format(MessageResource.AnUnexpectedErrorOccurred));
                }

                // Validate new password complexity
                var passwordValidation = _accountService.ValidatePasswordComplexity(model.Password);
                if (!passwordValidation.IsValid)
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, passwordValidation.GetCombinedMessage());



                if (model.Role == 0)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgInvalidAccountType);
                }
                else
                {
                    // In this case, 'model.Role' has a dynamicRoleId
                    roleId = model.Role;
                }
            }

            // Check if there is an account with same user name(email)
            if (_accountService.IsExist(model.Username))
            {
                var account = _accountService.GetAccountByEmail(model.Username);
                var companyAccount = _accountService.GetCompanyAccountByEmail(model.Username, model.CompanyId.Value);

                // If it is system admin then return Error
                if (account.CompanyId == null)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest,
                        string.Format(MessageResource.msgExistSystemAccount));
                }
                // If same company then return Error
                else if (companyAccount != null)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest,
                        string.Format(MessageResource.ExistAccount));
                }
                // If different company we add to new companyAccount
                else
                {
                    _accountService.AddAccountToCompany(account.Id, (int)model.CompanyId, roleId);

                    return new ApiSuccessResult(StatusCodes.Status201Created,
                        string.Format(MessageResource.MessageAddSuccess, AccountResource.lblAccount),
                        account.Id.ToString());
                }
            }
            else
            {
                try
                {
                    var accountId = _accountService.Add(model);

                    return new ApiSuccessResult(StatusCodes.Status201Created,
                        string.Format(MessageResource.MessageAddSuccess, AccountResource.lblAccount),
                        accountId.ToString());
                }
                catch (Exception e)
                {
                    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, e.Message);
                }
            }

        }


        /// <summary>
        /// Edit the account information.
        /// </summary>
        /// <param name="id"> identifier of companyAccount </param>
        /// <param name="model">JSON model for company account</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="404">Not Found: Id not exist in DB</response>
        /// <response code="422">unprocessable Entity: Email exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiAccountsId)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Edit(int id, [FromBody] AccountModel model)
        {
            model.Id = id;
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }

            //var account = _accountService.GetById(id);
            var companyAccount = _accountService.GetCompanyAccountById(id);

            if (_httpContext.User.GetAccountType() == (short)AccountType.SystemAdmin)
            {
                if (companyAccount == null)
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, AccountResource.msgInvalidAccount);
                }
                else
                {
                    if (model.Role == 0)
                    {
                        return new ApiErrorResult(StatusCodes.Status404NotFound, AccountResource.msgCanNotChangeRolePrimaryToSystem);
                    }
                    if (model.Role != 0 && model.CompanyId == 0)
                    {
                        return new ApiErrorResult(StatusCodes.Status404NotFound, AccountResource.msgCompanyNonExist);
                    }
                    var roleData = _roleService.GetByTypeAndCompanyId(model.Role, model.CompanyId.Value)
                        .FirstOrDefault();
                    if (roleData != null)
                    {
                        model.Role = (short)roleData.Id;
                    }
                    //model.Role = (short)_roleService.GetByTypeAndCompanyId(model.Role, model.CompanyId.Value).FirstOrDefault().Id;
                }
            }
            else
            {
                if (companyAccount == null || companyAccount.CompanyId != _httpContext.User.GetCompanyId())
                {
                    return new ApiErrorResult(StatusCodes.Status404NotFound, AccountResource.msgInvalidAccount);
                }
            }

            if (model.Username.ToLower() != companyAccount.Account.Username.ToLower())
            {
                var valid = _accountService.GetAccountByEmail(model.Username);
                if (valid != null)
                {
                    if (valid.Id != companyAccount.AccountId)
                    {
                        return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity,
                            string.Format(MessageResource.Exist, AccountResource.lblEmail));
                    }
                }
            }

            _accountService.Update(model);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, AccountResource.lblAccount, "")
                    .ReplaceSpacesString());
        }

        /// <summary>
        /// Update image for user
        /// </summary>
        /// <param name="model">Object model json include avatar</param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="400">Bad Request: Avatar not string.Empty</response>
        /// <response code="404">Not Found: Id not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiAccountsAvatar)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UploadAvatar([FromBody] AccountAvatarModel model)
        {
            var account = _accountService.GetById(_httpContext.User.GetAccountId());
            if (account == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, AccountResource.msgInvalidAccount);
            }

            var user = _userService.GetUserByAccountId(_httpContext.User.GetAccountId(),
                _httpContext.User.GetCompanyId());
            if (user == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, AccountResource.msgAccountNotLinkUser);
            }

            if (string.IsNullOrWhiteSpace(model.Avatar))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgEmptyAvatar);
            }

            // check permission allow user edit avatar themself
            if ((_httpContext.User.GetAccountType() == (short)AccountType.Employee ||
                 _httpContext.User.GetAccountType() == (short)AccountType.DynamicRole)
                && !_userService.CheckPermissionEditAvatar(_httpContext.User.GetCompanyId()))
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.Forbidden);
            }

            // test data avatar
            if (!model.Avatar.IsTextBase64())
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgIsTextBase64Avatar);
            }

            string connectionApi = _configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
            if (!string.IsNullOrWhiteSpace(user.Avatar))
            {
                FileHelpers.DeleteFileFromLink(user.Avatar.Replace($"{connectionApi}/static/", ""));
            }

            var company = _companyService.GetById(_httpContext.User.GetCompanyId());

            // Use secure file saving to prevent path traversal attacks
            string fileName = $"{user.UserCode}.{Guid.NewGuid().ToString()}.jpg";
            string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/avatar";
            bool saveImage = FileHelpers.SaveFileImageSecure(model.Avatar, basePath, fileName, Constants.Image.MaximumImageStored);
            if (!saveImage)
            {
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, MessageResource.MsgFail);
            }

            string path = Path.Combine(basePath, fileName);
            _userService.UpdateAvatar(user.Id, $"{connectionApi}/static/{path}");

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, AccountResource.lblAccount, ""));
        }

        /// <summary>
        /// Get avatar for user
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiAccountsAvatar)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetAvatar()
        {
            var user = _userService.GetUserByAccountId(_httpContext.User.GetAccountId(),
                _httpContext.User.GetCompanyId());
            if (user == null) return new ApiErrorResult(StatusCodes.Status404NotFound, AccountResource.msgInvalidUser);
            var model = new AccountAvatarModel();
            model.Avatar = user.Avatar;

            return Ok(model);
        }

        /// <summary>
        /// Edit the account profile information.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="422">Unprocessable Entity: Id or data model wrong</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiAccountProfile)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult EditProfile([FromBody] AccountModel model)
        {
            model.Id = _httpContext.User.GetAccountId();
            var account = _accountService.GetById(model.Id);
            if (!string.IsNullOrWhiteSpace(model.Username) && model.Username != account.Username)
            {
                return new ApiErrorResult(StatusCodes.Status403Forbidden, MessageResource.Forbidden);
            }
            
            if (!TryValidateModel(model))
            {
                return new ValidationFailedResult(ModelState);
            }

            _accountService.UpdateProfile(model);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, AccountResource.lblAccount, ""));
        }


        /// <summary>
        /// Delete a account.
        /// </summary>
        /// <param name="id">Account identifier</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Account is not allow delete</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Id not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiAccountsId)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Delete(int id)
        {
            //var account = _accountService.GetAccountByCurrentCompany(id);
            var companyAccount = _accountService.GetCompanyAccountById(id);
            var account = _accountService.GetAccountByCurrentCompany(companyAccount.AccountId.Value);

            if (companyAccount == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, AccountResource.msgInvalidAccount);
            }

            var accountLogin = _accountService.GetAccountLogin(User);

            if (!_accountService.IsAllowDelete(account, accountLogin))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgNotDelete);
            }

            _accountService.Delete(companyAccount);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, AccountResource.lblAccount));
        }


        /// <summary>
        /// Delete multiple accounts.
        /// </summary>
        /// <param name="ids">List of account ids</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Account not allow delete</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not found: List of Ids do not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpDelete]
        [Route(Constants.Route.ApiAccounts)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult DeleteMultiple(List<int> ids)
        {
            var companyId = _httpContext.User.GetCompanyId();

            var companyAccounts = _accountService.GetCompanyAccountsByIds(ids, companyId);
            if (!companyAccounts.Any())
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, AccountResource.msgInvalidAccount);
            }

            var accountLogin = _accountService.GetAccountLogin(User);

            if (companyAccounts.Any(m => !_accountService.IsAllowDelete(m.Account, accountLogin)))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgNotDelete);
            }

            if (companyId != 0)
            {
                // check approval accounts.
                var approvalSetting = _accountService.GetApprovalAccessSetting(companyId);

                if (approvalSetting.Any())
                {
                    foreach (var setting in approvalSetting)
                    {
                        var accounts = setting.Value.Select(m => m.Id).Intersect(ids).ToList();

                        if (accounts.Any())
                        {
                            // Don't expose sensitive account usernames in error messages
                            var accountCount = accounts.Count;
                            var accountType = setting.Key; // e.g., "FirstApprover", "SecondApprover"

                            return new ApiErrorResult(StatusCodes.Status400BadRequest,
                                $"{AccountResource.msgDeleteApprovalAccount} ({accountCount} {accountType} account(s))");
                        }
                    }
                }
            }
            else
            {
                foreach (var companyAccount in companyAccounts)
                {
                    if (companyAccount.CompanyId == null)
                        continue;

                    // check approval accounts.
                    var approvalSetting = _accountService.GetApprovalAccessSetting(companyAccount.CompanyId.Value);

                    if (approvalSetting.Any())
                    {
                        foreach (var setting in approvalSetting)
                        {
                            var accounts = setting.Value.Select(m => m.Id).Intersect(ids).ToList();
                            if (accounts.Any())
                            {
                                // Don't expose sensitive account usernames in error messages
                                var accountCount = accounts.Count;
                                var accountType = setting.Key; // e.g., "FirstApprover", "SecondApprover"

                                return new ApiErrorResult(StatusCodes.Status400BadRequest,
                                    $"{AccountResource.msgDeleteApprovalAccount} ({accountCount} {accountType} account(s))");
                            }
                        }
                    }
                }
            }

            _accountService.DeleteRange(companyAccounts);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageDeleteSuccess, AccountResource.lblAccount));
        }

        /// <summary>
        /// When forgot password
        /// </summary>
        /// <param name="model">JSON model for string of email login</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Email does not exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [AllowAnonymous]
        [Route(Constants.Route.ApiForgotPassword)]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            var account = _accountService.GetAccountByEmail(model.Email);
            if (account == null)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.MessageEmailNotRegister);
            }

            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSendEmailSuccess);
        }

        /// <summary>
        /// Reset password
        /// </summary>
        /// <param name="model">JSON model for object(new password, confirm new password and token string)</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Email does not exist or wrong token</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPost]
        [AllowAnonymous]
        [Route(Constants.Route.ApiResetPassword)]
        public IActionResult ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return new ValidationFailedResult(ModelState);
            }

            var clAcc = _jwtHandler.GetPrincipalFromExpiredToken(model.Token);
            if (clAcc == null)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.InvalidInformation);
            }

            var username = clAcc.GetUsername();
            var account = _accountService.GetAccountByEmail(username);
            if (account == null)
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, MessageResource.EmailDoestNotExist);
            }

            // Set user context from token for password validation
            HttpContext.User = clAcc;
            
            // Validate new password complexity with company settings from token
            var passwordValidation = _accountService.ValidatePasswordComplexity(model.NewPassword);
            if (!passwordValidation.IsValid)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, passwordValidation.GetCombinedMessage());

            account.Password = SecurePasswordHasher.GetSHA256Hash(model.NewPassword);
            account.UpdatePasswordOn = DateTime.UtcNow;

            // Update login configuration
            _accountService.UpdatePasswordChangedConfig(account);

            _accountService.ChangePassword(account);

            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageResetPassSuccess);
        }

        /// <summary>
        /// Get time zone
        /// </summary>
        /// <returns></returns>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [AllowAnonymous]
        [Route(Constants.Route.ApiAccountsListTimeZone)]
        public IActionResult GetListTimeZone()
        {
            var dictionary = _accountService.GetTimeZone();
            return Ok(dictionary);
        }

        /// <summary>
        /// Get preferred system options
        /// </summary>
        /// <returns></returns>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [AllowAnonymous]
        [Route(Constants.Route.ApiAccountsListSystem)]
        public IActionResult GetPreferredSystem()
        {
            List<EnumModel> codelist = _accountService.GetPreferredSystem();
            return Ok(codelist);
        }

        /// <summary>
        /// Updated Time Zone for User preferences page
        /// </summary>
        /// <param name="model">JSON model for timezone string</param>
        /// <returns></returns>
        /// <response code="400">Bad Request: Timezone empty</response>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="404">Not Found: Wrong token</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPatch]
        [Route(Constants.Route.ApiUpdateTimeZoneByAccounts)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdminAndEmployee,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdateTimeZone([FromBody] AccountTimeZoneModel model)
        {
            model.Id = _httpContext.User.GetAccountId();
            var account = _accountService.GetById(_httpContext.User.GetAccountId());
            if (account == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, AccountResource.msgInvalidAccount);
            }

            if (String.IsNullOrWhiteSpace(model.TimeZone))
            {
                return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgInvalidTimezon);
            }
            else
            {
                var timezone = Helpers.IsValidTimeZone(model.TimeZone);
                if (!timezone)
                {
                    return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgInvalidTimezon);
                }
            }

            _accountService.UpdateTimeZone(model, model.TimeZone);
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, AccountResource.lblAccount, ""));
        }

        /// <summary>
        /// Get version of system
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Authorize(Policy = Constants.Policy.SystemAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route(Constants.Route.ApiInformationVersionSystem)]
        public IActionResult GetVersionSystem()
        {
            var dictionary = _accountService.GetInfomationSystem();
            return Ok(dictionary);
        }


        /// <summary>
        ///      gets primary accounts.
        /// </summary>
        /// <param name="search">Query string that filter Account by Username(email).</param>
        /// <param name="pageNumber">Page number start from 1.</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="sortColumn">Sort Column by index of the column. Example </param>
        /// <param name="sortDirection">Sort direction: ‘desc’ for descending , ‘asc’ for ascending</param>
        /// <returns>   The primary accounts. </returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route(Constants.Route.ApiGetPrimaryAccounts)]
        public IActionResult GetPrimaryAccounts(string search, int pageNumber = 1, int pageSize = 10,
            int sortColumn = 0,
            string sortDirection = "desc")
        {
            var accounts = _accountService
                .GetPaginatedPrimaryAccount(search, pageNumber, pageSize, sortColumn, sortDirection,
                    out var recordsTotal,
                    out var recordsFiltered).AsEnumerable().Select(m => new AccountListModel()
                {
                    Id = m.AccountId.Value,
                    Email = m.Account.Username,
                    CompanyName = m.Company.Name,
                }).ToList();

            foreach (var account in accounts)
            {
                account.Role = HttpUtility.HtmlDecode(account.Role);
                account.AccountId = account.Id;

                var user = _userService.GetUserByAccountId(account.Id, _httpContext.User.GetCompanyId());

                if (user != null)
                {
                    account.FirstName = user.FirstName;
                    account.Position = user.Position;
                }
            }

            var pagingData = new PagingData<AccountListModel>
            {
                Data = accounts,
                Meta = { RecordsTotal = recordsTotal, RecordsFiltered = recordsFiltered }
            };
            return Ok(pagingData);
        }


        /// <summary>
        /// Get account type
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiGetAccountsType)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetAccountType()
        {
            var accountType = _accountService.GetAccountType();
            return Ok(accountType);
        }


        /// <summary>
        /// Get approval accounts
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiGetApprovalAccounts)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetApprovalAccounts()
        {
            var accounts = _accountService.GetApprovalAccount();

            return Ok(accounts);
        }

        /// <summary>
        /// Get access approval accounts
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiGetAccessApprovalAccounts)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetAccessApprovalAccounts()
        {
            var accounts = _accountService.GetAccessApprovalAccount();

            return Ok(accounts);
        }


        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiChangePassword)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ChangePasswordLogin([FromBody] ChangePasswordLoginModel model)
        {
            int accountId = _httpContext.User.GetAccountId();
            Account account = _accountService.GetById(accountId);
            
            if (!ModelState.IsValid)
                return new ValidationFailedResult(ModelState);

            if (account == null)
                return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);

            
            // Validate new password complexity with company settings
            var passwordValidation = _accountService.ValidatePasswordComplexity(model.NewPassword);
            if (!passwordValidation.IsValid)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, passwordValidation.GetCombinedMessage());

            account.Password = SecurePasswordHasher.GetSHA256Hash(model.NewPassword);
            account.UpdatePasswordOn = DateTime.UtcNow;

            // Update login configuration
            _accountService.UpdatePasswordChangedConfig(account);

            _accountService.ChangePassword(account);

            return new ApiSuccessResult(StatusCodes.Status200OK, AccountResource.msgChangePassSuccess);
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiChangePasswordNoLogin)]
        public IActionResult ChangePasswordWithoutLogin([FromBody] ChangePasswordModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError("username",
                    string.Format(MessageResource.Required, AccountResource.lblUsername));
            }
            else if (_accountService.GetAuthenticatedAccount(new LoginModel()
                     {
                         Username = model.Username,
                         Password = model.Password
                     }) == null)
            {
                ModelState.AddModelError("password", AccountResource.msgWrongPassword);
            }

            if (!ModelState.IsValid)
                return new ValidationFailedResult(ModelState);

            //if (string.IsNullOrEmpty(model.Username))
            //{
            //    ModelState.AddModelError("username", string.Format(MessageResource.Required, AccountResource.lblUsername));
            //    return new ValidationFailedResult(ModelState);
            //    //return new ApiErrorResult(StatusCodes.Status400BadRequest, string.Format(MessageResource.Required, AccountResource.lblUsername));
            //}

            //Account account = _accountService.GetAccountByEmail(model.Username);
            //if (account == null)
            //    return new ApiErrorResult(StatusCodes.Status404NotFound, MessageResource.ResourceNotFound);

            //if (!SecurePasswordHasher.Verify(model.Password, account.Password))
            //    return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AccountResource.msgWrongPassword);

            LoginModel loginModel = new LoginModel()
            {
                Username = model.Username,
                Password = model.Password
            };

            var account = _accountService.GetAuthenticatedAccount(loginModel);
            if (account == null)
            {
                //return new ApiUnauthorizedResult((int)LoginUnauthorized.InvalidCredentials, MessageResource.InvalidCredentials);
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, AccountResource.msgWrongPassword);
            }

            //if (model.Password.ToLower().Equals(model.NewPassword.ToLower()))
            //{
            //    return new ApiErrorResult(StatusCodes.Status400BadRequest, AccountResource.msgSamePasswordBefore);
            //}

            // Validate new password complexity with company settings  
            var passwordValidation = _accountService.ValidatePasswordComplexity(model.NewPassword);
            if (!passwordValidation.IsValid)
                return new ApiErrorResult(StatusCodes.Status422UnprocessableEntity, passwordValidation.GetCombinedMessage());

            account.Password = SecurePasswordHasher.GetSHA256Hash(model.NewPassword);
            account.UpdatePasswordOn = DateTime.UtcNow;

            // Update login configuration
            _accountService.UpdatePasswordChangedConfig(account);

            _accountService.ChangePassword(account);

            return new ApiSuccessResult(StatusCodes.Status200OK, AccountResource.msgChangePassSuccess);
        }

        /// <summary>
        /// Reset default time zone for user company
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiResetDefaultTimezone)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult ResetDefaultTimezone()
        {
            _accountService.ResetDefaultTimezoneByCompanyId(_httpContext.User.GetCompanyId());
            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, "timezone", "all accounts"));
        }

        /// <summary>
        /// Get last login of account
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiLastLogin)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetLastLogin()
        {
            return Ok(_accountService.GetLastLogin(_httpContext.User.GetAccountId(), _httpContext.User.GetCompanyId()));
        }

        /// <summary>
        /// Get token with username (military number)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiGetTokenWithUser)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAdmin,
            AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetTokenWithAccount([FromBody] AccountGetTokenModel model)
        {
            int companyId = _httpContext.User.GetCompanyId();
            return Ok(_accountService.GetTokenWithAccountAndHashKey(model, companyId));
        }

        /// <summary>
        /// get count visit, user waiting for review
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(Constants.Route.ApiAccountCountReview)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetCountReviewWithAccount()
        {
            return Ok(_accountService.GetAccountCountReview(_httpContext.User.GetAccountId(),
                _httpContext.User.GetCompanyId()));
        }

        /// <summary>
        /// Logout (clear current login info)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(Constants.Route.ApiLogout)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Logout()
        {
            _accountService.Logout(_httpContext.User.GetAccountId());
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSuccess);
        }

        /// <summary>
        /// Set the account as root account.
        /// </summary>
        /// <param name="username"> account's name </param>
        /// <param name="isRoot"></param>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="404">Not Found: Id not exist in DB</response>
        /// <response code="422">unprocessable Entity: Email exist in DB</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpPut]
        [Route(Constants.Route.ApiRootAccount)]
        [Authorize(Policy = Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult SetRootAccount(string username, bool isRoot)
        {
            var account = _accountService.GetAccountByEmail(username);

            if (account == null)
            {
                return new ApiErrorResult(StatusCodes.Status404NotFound, AccountResource.msgInvalidAccount);
            }

            _accountService.SetRootAccount(account, isRoot);

            return new ApiSuccessResult(StatusCodes.Status200OK,
                string.Format(MessageResource.MessageUpdateSuccess, AccountResource.lblAccount, "")
                    .ReplaceSpacesString());
        }

        /// <summary>
        /// Get list of accounts that have permission to approve visitor.
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiGetVisitApprovalAccounts)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetVisitApprovalAccounts(string search, int type = 0, string sortColumn = "Email", string sortDirection = "asc", int pageNumber = 1, int pageSize = 0)
        {
            var accounts = _accountService.GetApprovalAccount(search, type, true, sortColumn, sortDirection, pageNumber, pageSize, out int filteredRecord, out int totalRecord);

            var pagingData = new PagingData<AccountListModel>
            {
                Data = accounts,
                Meta = { RecordsTotal = totalRecord, RecordsFiltered = filteredRecord }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Get list of accounts that have permission to approve user.
        /// </summary>
        /// <returns></returns>
        /// <response code="401">Unauthorized: Token not invalid</response>
        /// <response code="403">Forbidden: Login with other account with role higher</response>
        /// <response code="429">Too Many Requests: Quota exceeded. Maximum allowed: 10 per 1s, 500 per 1m, 2000 per 1d.</response>
        [HttpGet]
        [Route(Constants.Route.ApiGetUserApprovalAccounts)]
        [Authorize(Policy = Constants.Policy.PrimaryAndSecondaryAdmin, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult GetUserApprovalAccounts(string search, int type = 0, string sortColumn = "Email", string sortDirection = "asc", int pageNumber = 1, int pageSize = 0)
        {
            var accounts = _accountService.GetApprovalAccount(search, type, false, sortColumn, sortDirection, pageNumber, pageSize, out int filteredRecord, out int totalRecord);

            var pagingData = new PagingData<AccountListModel>
            {
                Data = accounts,
                Meta = { RecordsTotal = totalRecord, RecordsFiltered = filteredRecord }
            };
            return Ok(pagingData);
        }

        /// <summary>
        /// Update device token (firebase)
        /// </summary>
        /// <param name="deviceToken"></param>
        /// <returns></returns>
        [HttpPut]
        [Route(Constants.Route.ApiAccountTokenInfo)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult UpdateDeviceToken(string deviceToken)
        {
            _accountService.UpdateDeviceTokenForAccount(_httpContext.User.GetAccountId(), deviceToken);
            return new ApiSuccessResult(StatusCodes.Status200OK, MessageResource.MessageSuccess);
        }
    }
}
