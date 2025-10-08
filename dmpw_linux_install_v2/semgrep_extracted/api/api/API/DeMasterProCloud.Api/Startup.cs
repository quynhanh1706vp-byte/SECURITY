using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using AspNetCoreRateLimit;
using DeMasterProCloud.Api.Infrastructure.Filters;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Common.Resources;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.DataModel.Api;
using DeMasterProCloud.DataModel.Setting;
using DeMasterProCloud.DataModel.PlugIn;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service;
using DinkToPdf;
using DinkToPdf.Contracts;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RequestLocalizationMiddleware = DeMasterProCloud.Api.Infrastructure.Middlewares.RequestLocalizationMiddleware;
using StatusCodes = Microsoft.AspNetCore.Http.StatusCodes;
using User = DeMasterProCloud.DataAccess.Models.User;
using Prometheus;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using NSwag;
using NSwag.Generation.Processors.Security;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http.Features;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using FluentScheduler;
using DeMasterProCloud.Service.Infrastructure;

namespace DeMasterProCloud.Api
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, IOptions<RequestLocalizationOptions> locOptions)
        {
            Console.WriteLine(@"Environment: " + env.EnvironmentName);
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            LocOptions = locOptions;
            FileHelpers.CleanFileAppSetting(env.EnvironmentName);
            AppLog.Init();
        }

        public IOptions<RequestLocalizationOptions> LocOptions;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            #region Bind Strongly Type Config

            var jwtSection = Configuration.GetSection(Constants.Settings.JwtSection);
            var jwtOptions = new JwtOptionsModel();
            jwtSection.Bind(jwtOptions);
            services.Configure<JwtOptionsModel>(jwtSection);

            // SECURITY: Determine if we're in production for security settings
            // Consider Development, Docker, localhost, and staging as non-production for HTTP compatibility
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var isLocalDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase) ||
                                    environment.Equals("Docker", StringComparison.OrdinalIgnoreCase) ||
                                    environment.Equals("Local", StringComparison.OrdinalIgnoreCase) ||
                                    environment.Equals("Staging", StringComparison.OrdinalIgnoreCase);

            // Also check if running on localhost/loopback addresses
            var isLocalhost = Environment.GetEnvironmentVariable("DOCKER_LOCALHOST") == "true" ||
                            Environment.GetEnvironmentVariable("LOCAL_DEVELOPMENT") == "true";

            var cultureSection = Configuration.GetSection(Constants.Settings.Cultures);
            var cultureOptions = new List<string>();
            cultureSection.Bind(cultureOptions);
            services.Configure<List<string>>(cultureSection);

            var supportedCultures = new List<CultureInfo>();
            foreach (var culture in cultureOptions)
            {
                supportedCultures.Add(new CultureInfo(culture));
            }

            #endregion

            #region framework services

            // configration for localiztion
            services.AddLocalization(options => options.ResourcesPath = Constants.Settings.ResourcesDir);
            services.Configure<RequestLocalizationOptions>(
                opts =>
                {
                    opts.DefaultRequestCulture = new RequestCulture(cultureOptions[0]);
                    // Formatting numbers, dates, etc.
                    opts.SupportedCultures = supportedCultures;
                    // UI strings that we have localized.
                    opts.SupportedUICultures = supportedCultures;
                    opts.RequestCultureProviders = new List<IRequestCultureProvider>
                    {
                        new QueryStringRequestCultureProvider(),
                        new CookieRequestCultureProvider()
                    };
                });

            // Service for init mapping
            services.AddAutoMapper(typeof(Startup).Assembly);

            // Config authentication
            services.AddAuthentication(options =>
                {
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(option =>
                {
                    option.ExpireTimeSpan = TimeSpan.FromMinutes(int.Parse(
                        Configuration[Constants.Settings.LoginExpiredTime] ?? Constants.Settings.DefaultExpiredTime));
                    option.AccessDeniedPath = Constants.Route.AccessDeniedPage;
                    // SECURITY: Enhanced cookie security settings (environment-aware)
                    option.Cookie.HttpOnly = true;
                    option.Cookie.SecurePolicy = (isLocalDevelopment || isLocalhost) ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
                    option.Cookie.SameSite = SameSiteMode.Strict;
                });

            // Config Authorize
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.Policy.SystemAdmin, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.SystemAdmin).ToString()));

                options.AddPolicy(Constants.Policy.SuperAdmin, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.SuperAdmin).ToString()));

                options.AddPolicy(Constants.Policy.PrimaryAdmin, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.PrimaryManager).ToString()));

                options.AddPolicy(Constants.Policy.SecondaryAdmin, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.SecondaryManager).ToString()));

                options.AddPolicy(Constants.Policy.SystemAndSuperAdmin, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.SystemAdmin).ToString(),
                        ((short) AccountType.SuperAdmin).ToString()));

                options.AddPolicy(Constants.Policy.SystemAndSuperAndPrimaryAdmin, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.SystemAdmin).ToString(),
                        ((short) AccountType.SuperAdmin).ToString(),
                        ((short) AccountType.PrimaryManager).ToString(),
                        ((short) AccountType.DynamicRole).ToString()));

                options.AddPolicy(Constants.Policy.SuperAndPrimaryAdmin, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.SuperAdmin).ToString(),
                        ((short) AccountType.PrimaryManager).ToString(),
                        ((short) AccountType.DynamicRole).ToString()));

                options.AddPolicy(Constants.Policy.PrimaryAndSecondaryAdmin, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.PrimaryManager).ToString(),
                        ((short) AccountType.SecondaryManager).ToString(),
                        ((short) AccountType.DynamicRole).ToString()));

                options.AddPolicy(Constants.Policy.SuperAndPrimaryAndSecondaryAdmin, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.SuperAdmin).ToString(),
                        ((short) AccountType.PrimaryManager).ToString(),
                        ((short) AccountType.SecondaryManager).ToString(),
                        ((short) AccountType.DynamicRole).ToString()));

                options.AddPolicy(Constants.Policy.SuperAndPrimaryAndSecondaryAdminAndEmployee, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.SuperAdmin).ToString(),
                        ((short) AccountType.PrimaryManager).ToString(),
                        ((short) AccountType.SecondaryManager).ToString(),
                        ((short) AccountType.Employee).ToString(),
                        ((short) AccountType.DynamicRole).ToString()));

                options.AddPolicy(Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdmin, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.SystemAdmin).ToString(),
                        ((short) AccountType.SuperAdmin).ToString(),
                        ((short) AccountType.PrimaryManager).ToString(),
                        ((short) AccountType.SecondaryManager).ToString(),
                        ((short) AccountType.DynamicRole).ToString()));

                options.AddPolicy(Constants.Policy.Employee, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.Employee).ToString()));

                options.AddPolicy(Constants.Policy.PrimaryAndSecondaryAdminAndEmployee, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.PrimaryManager).ToString(),
                        ((short) AccountType.SecondaryManager).ToString(),
                        ((short) AccountType.Employee).ToString(),
                        ((short) AccountType.DynamicRole).ToString()));

                options.AddPolicy(Constants.Policy.SystemAndSuperAndPrimaryAndSecondaryAdminAndEmployee, policy =>
                    policy.RequireClaim(Constants.ClaimName.AccountType,
                        ((short) AccountType.SystemAdmin).ToString(),
                        ((short) AccountType.PrimaryManager).ToString(),
                        ((short) AccountType.SecondaryManager).ToString(),
                        ((short) AccountType.Employee).ToString(),
                        ((short) AccountType.DynamicRole).ToString()));
                //options.AddPolicy("ValidAccount", policy =>
                //            policy.Requirements.Add(new ValidAccountRequirement(new[] { Status.Valid})));
            });

            // SECURITY: Enhanced HSTS configuration
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365); // Industry standard: 1 year
            });

            // Add bearer authentication
            services.AddAuthentication()
                .AddJwtBearer(cfg =>
                {
                    //cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Issuer,
                        // Validate the token expiry  
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // Add firebase authentication
            if (FirebaseApp.DefaultInstance == null)
            {
                try
                {
                    string firebaseConfig = Configuration.GetSection("Firebase:Credential").Value;
                    var firebaseApp = FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromJson(firebaseConfig),
                    });
                }
                catch (Exception ex)
                {
                    _ = ex.StackTrace;
                    //logger.LogWarning("Can not create Firebase client!");
                }
            }

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //    .AddJwtBearer(options =>
            //    {
            //        var firebaseProjectName = "DeMasterPro";
            //        options.Authority = "https://securetoken.google.com/" + firebaseProjectName;
            //        options.TokenValidationParameters = new TokenValidationParameters
            //        {
            //            ValidateIssuer = true,
            //            ValidIssuer = "https://securetoken.google.com/" + firebaseProjectName,
            //            ValidateAudience = true,
            //            ValidAudience = firebaseProjectName,
            //            ValidateLifetime = true
            //        };
            //    });

            // Config the db connection string
            bool enableLogSql = Configuration.GetSection("EnableLogSql").Get<bool>();
            services.AddEntityFrameworkNpgsql().AddDbContext<AppDbContext>(options =>
            {
                var connection = Environment.GetEnvironmentVariable(Constants.Settings.DefaultEnvironmentConnection);
                if (string.IsNullOrEmpty(connection))
                {
                    connection = Configuration.GetConnectionString(Constants.Settings.DefaultConnection);
                }

                options.UseNpgsql(connection,
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(Constants.Settings.DeMasterProCloudDataAccess);
                        // Configuring Connection Resiliency: 
                        // https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
                        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), new List<string>());
                        sqlOptions.CommandTimeout(3600);
                    });
                // Changing default behavior when client evaluation occurs to throw. 
                // Default in EF Core would be to log a warning when client evaluation is performed.
                // options.ConfigureWarnings(
                //     warnings => warnings.Ignore(RelationalEventId.QueryClientEvaluationWarning));
                // Check Client vs. Server evaluation: 
                // https://docs.microsoft.com/en-us/ef/core/querying/client-eval
                if (!enableLogSql)
                {
                    options.UseLoggerFactory(new LoggerFactory()).EnableSensitiveDataLogging();
                }
            });

            // NSwag Tools
            services.AddOpenApiDocument(document =>
            {
                document.AddSecurity("JWT", Enumerable.Empty<string>(), new NSwag.OpenApiSecurityScheme
                {
                    Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
                    Name = "Authorization",
                    In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                    Description = CommonResource.lblSwaggerDescription,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });
                document.Title = CommonResource.lblSwaggerTitle;
                document.Version = Constants.Swagger.V1;
                document.Description = CommonResource.lblSwaggerDescription;
                document.OperationProcessors.Add(
                    new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });
            
            // Cors Config
            services.AddCors(options =>
            {
                options.AddPolicy(Constants.Swagger.CorsPolicy,
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
            services.AddLogging(builder => builder.AddConsole(options => options.TimestampFormat = "yyyy:MM:dd hh:mm:ss "));

            // Adds a default in-memory implementation of IDistributedCache
            services.AddDistributedMemoryCache();
            // services.AddMemoryCache();
            services.AddSession(options =>
            {
                options.Cookie.Name = Constants.AppSession;
                options.IdleTimeout = TimeSpan.FromMinutes(Convert.ToDouble(
                    Configuration[Constants.Settings.ExpiredSessionTime] ?? Constants.Settings.DefaultExpiredTime));
                // SECURITY: Enhanced session cookie security (environment-aware)
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = (isLocalDevelopment || isLocalhost) ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.IsEssential = true;
            });
            // Custom service
            services.AddRouting(options => options.LowercaseUrls = true);
            
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            #endregion

            #region Application services

            // Common services
            services.AddScoped<IJwtHandler, JwtHandler>();
            services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ValidateModelFilter>();
            services.AddSingleton<ITempDataProvider, CookieTempDataProvider>();

            // Services business
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IAccessTimeService, AccessTimeService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<IHolidayService, HolidayService>();
            services.AddScoped<ISystemLogService, SystemLogService>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IAccessGroupService, AccessGroupService>();
            services.AddScoped<IAccessGroupDeviceService, AccessGroupDeviceService>();
            services.AddScoped<IEventLogService, EventLogService>();
            services.AddScoped<IUnregistedDeviceService, UnregistedDeviceService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IBuildingService, BuildingService>();
            services.AddScoped<IVisitService, VisitService>();
            services.AddScoped<IWorkingService, WorkingService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IPluginService, PlugInService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ICameraService, CameraService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<ISystemInfoService, SystemInfoService>();
            services.AddScoped<IShortenLinkService, ShortenLinkService>();
            services.AddScoped<IFirmwareVersionService, FirmwareVersionService>();
            services.AddScoped<IDeviceSDKService, DeviceSDKService>();
            services.AddScoped<IDeviceReaderService, DeviceReaderService>();
           
            services.AddScoped<IAccessScheduleService, AccessScheduleService>();
            services.AddScoped<IWorkShiftService, WorkShiftService>();
            services.AddScoped<IWebSocketService, WebSocketService>();

            services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = Constants.MaximumRequestBodySize;
            });
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = Constants.MaximumRequestBodySize;
            });

            services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
            });

            services.AddMvc(options =>
                {
                    options.EnableEndpointRouting = false;
                    options.Filters.Add<TrimStringPropertiesActionFilter>();
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                .AddControllersAsServices()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddSessionStateTempDataProvider()
                .AddFluentValidation(fvc =>
                    fvc.RegisterValidatorsFromAssemblyContaining<Startup>()).AddViewLocalization(
                    LanguageViewLocationExpanderFormat.Suffix,
                    opts => { opts.ResourcesPath = Constants.Settings.ResourcesDir; })
                .AddDataAnnotationsLocalization();
            services.AddControllers();
            //var context = new CustomAssemblyLoadContext();
            //if (Environment.OSVersion.Platform == PlatformID.WinCE)
            //{
            //    context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
            //}
            //else if (Environment.OSVersion.Platform == PlatformID.Unix)
            //{
            //    context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.so"));
            //}

            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

            #endregion

            #region Rate Limit

            // needed to load configuration from appsettings.json
            services.AddOptions();

            //load general configuration from appsettings.json
            //load ip rules from appsettings.json
            // IP Rate Limit
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            // services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));
            // services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));
            // services.Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRatePolicies"));

            // inject counter and rules stores
            services.AddSingleton<IIpPolicyStore, DistributedCacheIpPolicyStore>();
            // services.AddSingleton<IClientPolicyStore, DistributedCacheClientPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();

            // https://github.com/aspnet/Hosting/issues/793
            // the IHttpContextAccessor service is not registered by default.
            // the clientId/clientIp resolvers use it.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configuration (resolvers, counter key builders)
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            
            // Register the IProcessingStrategy
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

            #endregion
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            IConfiguration configuration)
        {
            ApplicationVariables.Env = env;
            ApplicationVariables.Configuration = configuration;
            // ApplicationVariables.LicenseVerified = false;
            // Rate Limit
            app.UseIpRateLimiting();
            // app.UseClientRateLimiting();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseOpenApi(a => a.PostProcess = (document, _) =>
                {
                    document.Schemes.Clear();
                    document.Schemes = new[] {OpenApiSchema.Http};
                });
            }
            else
            {
                app.UseExceptionHandler(Constants.Route.ErrorPage);
                app.UseHsts();
                app.UseOpenApi(a => a.PostProcess = (document, _) =>
                {
                    document.Schemes.Clear();
                    document.Schemes = new[] {OpenApiSchema.Https, OpenApiSchema.Http};
                });
            }

            // SECURITY: Add comprehensive security headers middleware (environment-aware)
            app.Use(async (context, next) =>
            {
                // Determine if this is a local/development environment
                var currentEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
                var isLocalEnv = currentEnvironment.Equals("Development", StringComparison.OrdinalIgnoreCase) ||
                               currentEnvironment.Equals("Docker", StringComparison.OrdinalIgnoreCase) ||
                               currentEnvironment.Equals("Local", StringComparison.OrdinalIgnoreCase) ||
                               currentEnvironment.Equals("Staging", StringComparison.OrdinalIgnoreCase) ||
                               Environment.GetEnvironmentVariable("DOCKER_LOCALHOST") == "true" ||
                               Environment.GetEnvironmentVariable("LOCAL_DEVELOPMENT") == "true";

                // Only add HSTS in production environments
                if (!isLocalEnv && !context.Response.Headers.ContainsKey("Strict-Transport-Security"))
                {
                    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains; preload";
                }

                // Content Security Policy (relaxed for local development and Docker)
                var cspPolicy = isLocalEnv
                    ? "default-src 'self' 'unsafe-inline' 'unsafe-eval'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: http: https:; font-src 'self' data:; connect-src 'self' ws: wss: http: https:; frame-ancestors 'none';"
                    : "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; font-src 'self' data:; connect-src 'self'; frame-ancestors 'none';";

                context.Response.Headers["Content-Security-Policy"] = cspPolicy;

                // X-Content-Type-Options
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";

                // X-Frame-Options
                context.Response.Headers["X-Frame-Options"] = "DENY";

                // X-XSS-Protection
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

                // Referrer Policy
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

                // Permissions Policy
                context.Response.Headers["Permissions-Policy"] =
                    "geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=(), gyroscope=(), speaker=()";

                await next();
            });
            
            app.UseSwaggerUi();
            app.UseHttpsRedirection();
            app.UseSession();
            app.UseStaticFiles();
            // SECURITY: Configure secure cookie policy (environment-aware)
            var currentEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            var isLocalEnvironment = currentEnv.Equals("Development", StringComparison.OrdinalIgnoreCase) ||
                                    currentEnv.Equals("Docker", StringComparison.OrdinalIgnoreCase) ||
                                    currentEnv.Equals("Local", StringComparison.OrdinalIgnoreCase) ||
                                    currentEnv.Equals("Staging", StringComparison.OrdinalIgnoreCase) ||
                                    Environment.GetEnvironmentVariable("DOCKER_LOCALHOST") == "true" ||
                                    Environment.GetEnvironmentVariable("LOCAL_DEVELOPMENT") == "true";

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = isLocalEnvironment ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.Strict
            });
            app.UseAuthentication();
            app.UseMetricServer();
            app.UseHttpMetrics();

            // run cronjob 
            JobManager.Initialize(new MyRegistry(Configuration));

            app.Use(async (context, next) =>
            {
                await next();
                // Custom 404 error
                if (context.Response.StatusCode == StatusCodes.Status404NotFound
                    && !context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/json";
                    var response = new ApiErrorResult(context.Response.StatusCode);
                    //var payload = JsonConvert.SerializeObject(response);
                    var payload = Helpers.JsonConvertCamelCase(response);
                    await context.Response.WriteAsync(payload);
                }
            });
            var logDir = Path.GetDirectoryName(Configuration[Constants.Logger.LogFile]);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            // create secret key fo company
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                using (var db = services.GetRequiredService<AppDbContext>())
                {
                    try
                    {
                        // update secretCode to system info
                        string secretCode = "";
                        var companyTemplate = db.Company.FirstOrDefault(m => !m.IsDeleted && !string.IsNullOrEmpty(m.SecretCode));
                        secretCode = companyTemplate != null ? companyTemplate.SecretCode : Helpers.GenerateCompanyKey();
                        var systemInfo = db.SystemInfo.OrderByDescending(m => m.Id).FirstOrDefault();
                        if (systemInfo == null)
                        {
                            db.SystemInfo.Add(new SystemInfo()
                            {
                                SecretCode = secretCode,
                                UpdatedOn = DateTime.UtcNow,
                            });
                            db.SaveChanges();
                        }
                        else if(systemInfo != null && string.IsNullOrEmpty(systemInfo.SecretCode))
                        {
                            systemInfo.SecretCode = secretCode;
                            db.SystemInfo.Update(systemInfo);
                            db.SaveChanges();
                        }
                        else
                        {
                            secretCode = systemInfo.SecretCode;
                        }

                        // update secretCode to company (if null)
                        var companies = db.Company.Where(m => !m.IsDeleted && string.IsNullOrEmpty(m.SecretCode));
                        foreach (var company in companies)
                        {
                            company.SecretCode = secretCode;
                            db.Company.Update(company);
                        }

                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating secret code: {ex.Message}");
                    }
                }
            }

            // Check company have already have setting default if not create new
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                using (var db = services.GetRequiredService<AppDbContext>())
                {
                    try
                    {
                        var settings = Configuration.GetSection(Constants.Settings.FileSettings)?.Get<List<FileSetting>>();
                        var listCompanyId = db.Company.Where(a => !a.IsDeleted).Select(
                            c => c.Id).ToList();
                        if (settings != null && settings.Any())
                        {
                            if (listCompanyId.Count != 0)
                            {
                                foreach (var companyId in listCompanyId)
                                {
                                    foreach (var i in settings)
                                    {
                                        bool settingCompany = db.Setting.Any(
                                            c => c.CompanyId == companyId && c.Key == i.Key);
                                        if (settingCompany != true)
                                        {
                                            var newSetting = new Setting
                                            {
                                                Key = i.Key,
                                                Value = JsonConvert.SerializeObject(i.Values),
                                                CompanyId = companyId
                                            };
                                            db.Setting.Add(newSetting);
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating default settings: {ex.Message}");
                    }
                }
            }

            // Check timezone on building add default timezone if null
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                using (var db = services.GetRequiredService<AppDbContext>())
                {
                    try
                    {
                        var buildingList = db.Building.Where(b => !b.IsDeleted).Select(
                            c => c.Id).ToList();
                        foreach (var buildingId in buildingList)
                        {
                            Building building = db.Building.Where(c => c.Id == buildingId).Single();
                            if (building.TimeZone == null)
                            {
                                building.TimeZone = Helpers.GetLocalTimeZone();
                                db.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating building timezone: {ex.Message}");
                    }
                }
            }
            
            // Update default language, timezone profile account
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                using (var db = services.GetRequiredService<AppDbContext>())
                {
                    try
                    {
                        var companies = db.Company.Where(c => !c.IsDeleted);
                        foreach (var company in companies)
                        {
                            var accounts = db.Account.Where(a => !a.IsDeleted && a.CompanyId == company.Id);
                            var language = JsonConvert.DeserializeObject<List<string>>(db.Setting.FirstOrDefault(s => s.Key == "language" && s.CompanyId == company.Id).Value);
                            var building = db.Building.FirstOrDefault(b => !b.IsDeleted && b.CompanyId == company.Id);
                            foreach (var account in accounts)
                            {
                                try
                                {
                                    bool isUpdate = false;
                                    if (string.IsNullOrEmpty(account.Language))
                                    {
                                        account.Language = language[0];
                                        isUpdate = true;
                                    }

                                    if (string.IsNullOrEmpty(account.TimeZone))
                                    {
                                        account.TimeZone = building?.TimeZone;
                                        isUpdate = true;
                                    }

                                    if (isUpdate)
                                    {
                                        db.Account.Update(account);
                                        db.SaveChanges();
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                    Console.WriteLine(e);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating account language/timezone: {ex.Message}");
                    }
                }
            }

            // Check working type of company is default and create if is null
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                using (var db = services.GetRequiredService<AppDbContext>())
                {
                    try
                    {
                        var companies = db.Company.Where(m => !m.IsDeleted).Select(
                            c => c.Id).ToList();
                        foreach (var companyId in companies)
                        {
                            WorkingType workingType = db.WorkingType.FirstOrDefault(c => c.CompanyId == companyId && c.IsDefault);
                            if (workingType == null)
                            {
                                var workingTime = new WorkingType
                                {
                                    Name = Constants.Attendance.DefaultName,
                                    IsDefault = true,
                                    CompanyId = companyId,
                                    WorkingDay = Constants.Attendance.DefaultWorkingTime
                                };
                                db.WorkingType.Add(workingTime);
                                db.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking working type of company: {ex.Message}");
                    }
                }
            }

            // Check plugin of company is default and create if is null
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                using (var db = services.GetRequiredService<AppDbContext>())
                {
                    try
                    {
                        var companies = db.Company.Where(m => !m.IsDeleted).Select(
                            c => c.Id).ToList();
                        foreach (var companyId in companies)
                        {
                            PlugIn solution = db.PlugIn.Where(c => c.CompanyId == companyId).FirstOrDefault();
                            if (solution == null)
                            {
                                var plugIns = new PlugIns
                                {
                                    AccessControl = true,
                                    TimeAttendance = true,
                                    VisitManagement = true,
                                    Common = true,
                                    QrCode = true,
                                    PassCode = true,
                                    ScreenMessage = true,
                                };

                                var plugInsDescription = new PlugInsDescription()
                                {
                                    AccessControl = Constants.PlugInValue.AccessControlDescription,
                                    TimeAttendance = Constants.PlugInValue.TimeAttendanceDescription,
                                    VisitManagement = Constants.PlugInValue.VisitManagementDescription,
                                    CanteenManagement = Constants.PlugInValue.CanteenManagementDescription,
                                    CardIssuing = Constants.PlugInValue.CardIssuingDescription,
                                    Common = Constants.PlugInValue.CommonDescription,
                                    QrCode = Constants.PlugInValue.QrCodeDescription,
                                    PassCode = Constants.PlugInValue.PassCodeDescription,
                                    ScreenMessage = Constants.PlugInValue.ScreenMessageDescription,
                                    CameraPlugIn = Constants.PlugInValue.CameraPlugInDescription,

                                    ArmyManagement = Constants.PlugInValue.ArmyManagementDescription,
                                };
                                var json = JsonConvert.SerializeObject(plugIns);
                                var jsonDescription = JsonConvert.SerializeObject(plugInsDescription);

                                var newSolution = new PlugIn
                                {
                                    CompanyId = companyId,
                                    PlugIns = json,
                                    PlugInsDescription = jsonDescription
                                };
                                db.PlugIn.Add(newSolution);
                                db.SaveChanges();
                            }
                            else
                            {
                                // update plugin
                                var isUpdate = false;
                                var pluginSettings = configuration.GetSection("DefaultPlugin")?.Get<List<PlugInSettingModel>>();
                                var plugins = JsonConvert.DeserializeObject<Dictionary<string, bool>>(solution.PlugIns);
                                var pluginDescriptions = JsonConvert.DeserializeObject<Dictionary<string, string>>(solution.PlugInsDescription);

                                foreach (var pluginSetting in pluginSettings)
                                {
                                    if (!plugins.ContainsKey(pluginSetting.Name))
                                    {
                                        plugins.Add(pluginSetting.Name, pluginSetting.IsEnable);
                                        isUpdate = true;
                                    }

                                    if (!pluginDescriptions.ContainsKey(pluginSetting.Name))
                                    {
                                        pluginDescriptions.Add(pluginSetting.Name, pluginSetting.Description);
                                        isUpdate = true;
                                    }
                                }

                                if (isUpdate)
                                {
                                    solution.PlugIns = JsonConvert.SerializeObject(plugins, Formatting.Indented);
                                    solution.PlugInsDescription = JsonConvert.SerializeObject(pluginDescriptions, Formatting.Indented);

                                    db.PlugIn.Update(solution);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error checking plugin of company: {ex.Message}");
                    }
                }
            }

            // Run update default role permissions
            try
            {
                using (var serviceScope = app.ApplicationServices.CreateScope())
                {
                    var services = serviceScope.ServiceProvider;

                    var roleService = services.GetService<IRoleService>();
                    roleService.UpdateDefaultPermission();
                }
            }
            catch (Exception e)
            {
            }
            
            // create new leave request setting for company
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                using (var db = services.GetRequiredService<AppDbContext>())
                {
                    try
                    {
                        var companies = db.Company.Where(c => !c.IsDeleted);
                        foreach (var company in companies)
                        {
                            var setting = db.LeaveRequestSetting.FirstOrDefault(m => m.CompanyId == company.Id);
                            if (setting == null)
                            {
                                db.LeaveRequestSetting.Add(new LeaveRequestSetting()
                                {
                                    NumberDayOffYear = 12,
                                    NumberDayOffPreviousYear = 90,
                                    CompanyId = company.Id,
                                });
                            }
                        }

                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error creating leave request setting for company: {ex.Message}");
                    }
                }
            }

            // migration change link image companies setting from base64 to url
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                using (var db = services.GetRequiredService<AppDbContext>())
                {
                    try
                    {
                        // logo
                        var settingsLogo = db.Setting.Where(m => m.Key == Constants.Settings.Logo);
                        foreach (var setting in settingsLogo)
                        {
                            if (!string.IsNullOrEmpty(setting.Value))
                            {
                                try
                                {
                                    JArray jsonArray = JsonConvert.DeserializeObject<JArray>(setting.Value);

                                    var settingValue = jsonArray[0].ToString();

                                    var company = db.Company.FirstOrDefault(m => m.Id == setting.CompanyId);
                                    if (settingValue.IsTextBase64())
                                    {
                                        // save image logo
                                        string hostApi = configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                                        string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/setting";
                                        string fileName = $"{Constants.Settings.Logo}.jpg";
                                        bool isSaveImage = FileHelpers.SaveFileImageSecure(settingValue, basePath, fileName);
                                        string path = Path.Combine(basePath, fileName);
                                        jsonArray[0] = $"{hostApi}/static/{path}";
                                        setting.Value = JsonConvert.SerializeObject(jsonArray);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                            }

                        }

                        // qr_logo
                        var settingsQrLogo = db.Setting.Where(m => m.Key == Constants.Settings.QRLogo);
                        foreach (var setting in settingsQrLogo)
                        {
                            if (!string.IsNullOrEmpty(setting.Value))
                            {
                                try
                                {
                                    JArray jsonArray = JsonConvert.DeserializeObject<JArray>(setting.Value);

                                    var settingValue = jsonArray[0].ToString();

                                    var company = db.Company.FirstOrDefault(m => m.Id == setting.CompanyId);
                                    if (settingValue.IsTextBase64())
                                    {
                                        // save image logo
                                        string hostApi = configuration.GetSection(Constants.Settings.DefineConnectionApi).Get<string>();
                                        string basePath = $"{Constants.Settings.DefineFolderImages}/{company.Code}/setting";
                                        string fileName = $"{Constants.Settings.QRLogo}.jpg";
                                        bool isSaveImage = FileHelpers.SaveFileImageSecure(settingValue, basePath, fileName);
                                        string path = Path.Combine(basePath, fileName);
                                        jsonArray[0] = $"{hostApi}/static/{path}";
                                        setting.Value = JsonConvert.SerializeObject(jsonArray);
                                    }

                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                            }

                        }
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error migrating image companies setting from base64 to url: {ex.Message}");
                    }
                }
            }
            
            // [TEMP] set enable auto sync in camera service
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                using (var db = services.GetRequiredService<AppDbContext>())
                {
                    try
                    {
                        var settings = db.Setting.Where(m => m.Key == Constants.Settings.CameraSetting);
                        foreach (var setting in settings)
                        {
                            if (!string.IsNullOrEmpty(setting.Value) && !setting.Value.Contains("isEnableAutoSync"))
                            {
                                try
                                {
                                    var newValue = JsonConvert.DeserializeObject<CameraSetting>(setting.Value) ?? new CameraSetting();
                                    newValue.IsEnableAutoSync = true;
                                    setting.Value = Helpers.JsonConvertCamelCase(newValue);
                                    db.Setting.Update(setting);
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error setting enable auto sync in camera service: {ex.Message}");
                    }
                }
            }
            
            // company setting language default
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var services = serviceScope.ServiceProvider;
                using (var db = services.GetRequiredService<AppDbContext>())
                {
                    try
                    {
                        var companyList = db.Company;
                        foreach (var company in companyList)
                        {
                            var languagesSettingDefault = db.Setting.FirstOrDefault(m => m.Key == Constants.Settings.KeyListLanguageOfCompany && m.CompanyId == company.Id);
                            if (languagesSettingDefault == null)
                            {
                                db.Setting.Add(new Setting()
                                {
                                    CompanyId = company.Id,
                                    Key = Constants.Settings.KeyListLanguageOfCompany,
                                    Value = JsonConvert.SerializeObject(Constants.Settings.ListLanguageDefault)
                                });
                            }
                        }
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error setting company language default: {ex.Message}");
                    }
                }
            }
            
            //call ConfigureLogger in a centralized place in the code
            loggerFactory.AddFile(Configuration[Constants.Logger.LogFile], LogLevel.Error);
            loggerFactory.AddFile(Configuration[Constants.Logger.LogFile], LogLevel.Warning);
            var options = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            ValidatorOptions.Global.LanguageManager = new LanguageManager(options);
            app.UseRequestLocalization(options.Value);
            app.UseMiddleware<RequestLocalizationMiddleware>();
            app.UseCors(Constants.Swagger.CorsPolicy);
            app.UseMvcWithDefaultRoute();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseCookiePolicy();
            
            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromSeconds(120),
            };
            app.UseWebSockets(webSocketOptions);
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var token = context.Request.Query["token"];
                        if (!ValidTokenWebSocket(token, out int companyId))
                        {
                            context.Response.StatusCode = 401;
                            await context.Response.WriteAsync("Unauthorized");
                            return;
                        }

                        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        string id = Guid.NewGuid().ToString();
                        try
                        {
                            ApplicationVariables.ClientSockets.Add(id, new WebSocketItem()
                            {
                                Client = webSocket,
                                CompanyId = companyId,
                            });
                            await Echo(context, webSocket, id);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                            if (!context.Response.HasStarted)
                            {
                                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                                await context.Response.WriteAsync("System Error");
                            }
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        Console.WriteLine("WebSocket connection request is not valid.");
                    }
                }
                else
                {
                    await next();
                }
            });

        }
        
        public static async Task Echo(HttpContext context, WebSocket webSocket, string id)
        {
            var buffer = new byte[1024 * 4];
            try
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    // Process received message if needed
                    // For now, just receive the next message
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                {
                    await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                }
            }
            catch (WebSocketException)
            {
                // WebSocket connection was closed abruptly by client - this is normal
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
            try
            {
                ApplicationVariables.ClientSockets.Remove(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static bool ValidTokenWebSocket(string token, out int companyId)
        {
            companyId = 0;
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                return jwtToken.ValidFrom <= DateTime.UtcNow && DateTime.UtcNow <= jwtToken.ValidTo 
                    && int.TryParse(jwtToken.Claims.FirstOrDefault(c => c.Type == Constants.ClaimName.CompanyId)?.Value, out companyId);;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}