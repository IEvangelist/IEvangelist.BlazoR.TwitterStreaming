using IEvangelist.Blazing.SignalR.Server.Hubs;
using IEvangelist.Blazing.SignalR.Server.Services;
using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Net.Mime;
using Tweetinvi;
using Tweetinvi.Streaming;

namespace IEvangelist.Blazing.SignalR.Server
{
    public class Startup
    {
        const string CorsPolicy = nameof(CorsPolicy);

        readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) => _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConnections();
            services.AddSignalR(options => options.KeepAliveInterval = TimeSpan.FromSeconds(5))
                    .AddMessagePackProtocol();

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    MediaTypeNames.Application.Octet,
                    WasmMediaTypeNames.Application.Wasm,
                });
            });

            Auth.SetUserCredentials(
                _configuration["Authentication:Twitter:ConsumerKey"],
                _configuration["Authentication:Twitter:ConsumerSecret"],
                _configuration["Authentication:Twitter:AccessToken"],
                _configuration["Authentication:Twitter:AccessTokenSecret"]);

            services.AddSingleton<ITwitterService, TwitterService>();
            services.AddSingleton<IFilteredStream>(_ => Stream.CreateFilteredStream());

            services.AddMvc()
                    .AddJsonOptions(
                         options =>
                             options.SerializerSettings
                                    .ContractResolver = new DefaultContractResolver());

            services.AddCors(
                options =>
                options.AddPolicy(CorsPolicy,
                    builder =>
                    builder.AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowAnyOrigin()
                           .AllowCredentials()));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCookiePolicy();
            app.UseCors(CorsPolicy);

            app.UseSignalR(routes => routes.MapHub<StreamHub>("/streamHub"));
            app.UseMvc(routes => routes.MapRoute("default", "{controller}/{action}/{id?}"));
            app.UseBlazor<Client.Startup>();
        }
    }
}