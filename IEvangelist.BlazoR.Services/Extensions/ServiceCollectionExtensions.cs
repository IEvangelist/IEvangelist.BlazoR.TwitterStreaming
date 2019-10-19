using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Tweetinvi;
using Tweetinvi.Streaming;

namespace IEvangelist.BlazoR.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazoRTwitterServices<THub>(
            this IServiceCollection services,
            IConfiguration configuration)
            where THub : Hub
        {
            services.AddSignalR(options => options.KeepAliveInterval = TimeSpan.FromSeconds(5))
                    .AddAzureSignalR();

            Auth.SetUserCredentials(
                configuration["Authentication:Twitter:ConsumerKey"],
                configuration["Authentication:Twitter:ConsumerSecret"],
                configuration["Authentication:Twitter:AccessToken"],
                configuration["Authentication:Twitter:AccessTokenSecret"]);

            return services.AddSingleton<ISentimentService, SentimentService>()
                           .AddHostedService<TwitterService<THub>>()
                           .AddSingleton<IFilteredStream>(_ => Stream.CreateFilteredStream());
        }
    }
}