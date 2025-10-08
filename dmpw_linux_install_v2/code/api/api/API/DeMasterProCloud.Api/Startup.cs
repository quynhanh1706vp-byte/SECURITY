using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DeMasterProCloud.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            // TODO: Cấu hình đúng scheme bạn đang dùng (JWT/Cookie/…)
            // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //         .AddJwtBearer(options => { /* ... */ });

            // Global Authorize = deny-by-default
            services.AddMvc(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddAuthorization();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();   // bắt buộc trước MVC
            app.UseAuthorization();

            // .NET Core 2.x: UseMvc
            app.UseMvc();

            // Với .NET Core 3.x (endpoint routing) thì thay bằng:
            // app.UseRouting();
            // app.UseAuthentication();
            // app.UseAuthorization();
            // app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
