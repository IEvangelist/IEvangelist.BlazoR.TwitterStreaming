using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Tweetinvi;
using Tweetinvi.Streaming;

namespace IEvangelist.BlazoR.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlazoRTwitterServices<THub>(this IServiceCollection services)
            where THub : Hub<ITwitterClient> =>
            services.AddSingleton<ISentimentService, SentimentService>()
                    .AddSingleton<ITwitterService<THub>, TwitterService<THub>>()
                    .AddSingleton<IFilteredStream>(_ => Stream.CreateFilteredStream());
    }
}