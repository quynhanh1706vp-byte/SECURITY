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
    /// <summary>Demaster-pro cloud API</summary>
    public class Program
    {
        /// <summary>This is main</summary>
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
                var unitOfWork   = services.GetRequiredService<IUnitOfWork>();
                DbInitializer.Initialize(unitOfWork, configuration);
            }

            host.Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseStartup<Startup>();
    }
}
