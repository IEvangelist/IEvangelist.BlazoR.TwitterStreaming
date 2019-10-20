using IEvangelist.BlazoR.TwitterStreaming.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Net.Mime;

namespace IEvangelist.BlazoR.TwitterStreaming
{
    public class Startup
    {
        readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddTelerikBlazor();

            services.Configure<TweeteROptions>(
                _configuration.GetSection(nameof(TweeteROptions)));

            //services.AddCors(options =>
            //    options.AddPolicy(
            //        "OpenAllPolicy",
            //        policy =>
            //            policy.AllowAnyOrigin()
            //                  .AllowAnyMethod()
            //                  .AllowAnyHeader()
            //                  .AllowCredentials()));

            services.AddResponseCompression(
                options =>
                    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] 
                    {
                        MediaTypeNames.Application.Octet,
                        "application/wasm"
                    }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");                
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            //app.UseCors("OpenAllPolicy");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}