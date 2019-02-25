using Blazor.Extensions.Logging;
using IEvangelist.Blazing.SignalR.Client.Services;
using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace IEvangelist.Blazing.SignalR.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder => builder.AddBrowserConsole());
            services.AddSingleton<ITwitterStreamService, TwitterStreamService>();
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}