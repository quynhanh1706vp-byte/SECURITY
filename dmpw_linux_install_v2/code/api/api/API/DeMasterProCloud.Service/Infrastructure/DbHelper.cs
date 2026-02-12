using System;
using DeMasterProCloud.Common.Infrastructure;
using DeMasterProCloud.DataAccess.Models;
using DeMasterProCloud.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace DeMasterProCloud.Service.Infrastructure
{
    public class DbHelper
    {
        /// <summary>
        ///  Create and return an UnitOfWork instance
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static IUnitOfWork CreateUnitOfWork(IConfiguration configuration, IHttpContextAccessor contextAccessor = null)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .EnableSensitiveDataLogging(true);

            options.UseNpgsql(configuration.GetConnectionString(Constants.Settings.DefaultConnection),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(Constants.Settings.DeMasterProCloudDataAccess);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                    sqlOptions.CommandTimeout(3600);
                });
            // options.ConfigureWarnings(
            //     warnings => warnings.Ignore(RelationalEventId.QueryClientEvaluationWarning));

            var context = new AppDbContext(options.Options);
            return new UnitOfWork(context, contextAccessor);
        }
    }
}
