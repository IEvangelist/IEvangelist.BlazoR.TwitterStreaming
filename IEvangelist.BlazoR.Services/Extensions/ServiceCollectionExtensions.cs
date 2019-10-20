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

            services.AddCors(options =>
                options.AddPolicy("OpenAllPolicy",
                    policy =>
                        policy.WithOrigins(configuration["TwitterStreamingUI:BaseAddress"])
                              .AllowAnyHeader()
                              .WithMethods("GET", "POST")
                              .AllowCredentials()));

            Auth.SetUserCredentials(
                configuration["Authentication:Twitter:ConsumerKey"],
                configuration["Authentication:Twitter:ConsumerSecret"],
                configuration["Authentication:Twitter:AccessToken"],
                configuration["Authentication:Twitter:AccessTokenSecret"]);

            return services.AddSingleton<ISentimentService, SentimentService>()
                           .AddSingleton<ITwitterService<THub>, TwitterService<THub>>()
                           .AddHostedService<TwitterService<THub>>()
                           .AddSingleton<IFilteredStream>(_ => 
                           {
                               var stream = Stream.CreateFilteredStream();
                               stream.StallWarnings = true;

                               return stream;
                           });
        }
    }
}