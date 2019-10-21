using IEvangelist.BlazoR.Services.Extensions;
using IEvangelist.BlazoR.TweeteR.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IEvangelist.BlazoR.TweeteR
{
    public class Startup
    {
        readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services) =>
            services.AddBlazoRTwitterServices<StreamHub>(_configuration);

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors("OpenAllPolicy");
            app.UseEndpoints(endpoints => endpoints.MapHub<StreamHub>("/streamHub"));
        }
    }
}