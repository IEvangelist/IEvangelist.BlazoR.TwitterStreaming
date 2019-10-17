using IEvangelist.BlazoR.Services.Extensions;
using IEvangelist.BlazoR.TwitterStreaming.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net.Mime;
using Tweetinvi;

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

            services.AddSignalR(options => options.KeepAliveInterval = TimeSpan.FromSeconds(5))
                    .AddAzureSignalR();
                    //.AddMessagePackProtocol();

            Auth.SetUserCredentials(
                _configuration["Authentication:Twitter:ConsumerKey"],
                _configuration["Authentication:Twitter:ConsumerSecret"],
                _configuration["Authentication:Twitter:AccessToken"],
                _configuration["Authentication:Twitter:AccessTokenSecret"]);

            services.AddResponseCompression(options => options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] 
            {
                MediaTypeNames.Application.Octet,
                "application/wasm"
            }));

            services.AddBlazoRTwitterServices<StreamHub>();
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapHub<StreamHub>("/streamHub");
            });
        }
    }
}
