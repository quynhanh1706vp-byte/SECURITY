using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.Repository;
using DeMasterProCloud.Service;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DeMasterProCloud.Api
{
    /// <summary>
    /// Demaster-pro cloud API
    /// </summary>
    public class Program
    {
   
        /// <summary>
        /// This is main
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var host = CreateWebHostBuilder(args).Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                if (ApplicationVariables.LoggerFactory == null)
                {
                    ApplicationVariables.LoggerFactory = services.GetRequiredService<ILoggerFactory>();
                }
                
                var configuration = services.GetRequiredService<IConfiguration>();
                var unitOfWork = services.GetRequiredService<IUnitOfWork>();
                DbInitializer.Initialize(unitOfWork, configuration);
                
                IDeviceSDKService deviceSDKService = services.GetRequiredService<IDeviceSDKService>();
                deviceSDKService.Login();
                deviceSDKService.SubscribeWebhook();
            }
            host.Run();
        }
        
        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureKestrel((context, options) =>
                {
                    // Set properties and call methods on options
                })
                .UseUrls("http://*:5000")
                .UseIISIntegration()
                .UseSentry()
                .UseStartup<Startup>();
    }
}
