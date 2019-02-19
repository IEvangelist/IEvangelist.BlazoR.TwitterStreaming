using IEvangelist.Blazing.SignalR.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IEvangelist.Blazing.SignalR.Client.Services
{
    public class TwitterStreamService : ITwitterStreamService
    {
        public async Task SubscribeAsync(Action<TweetResult> callback, CancellationToken token)
        {
            var connection =
                new HubConnectionBuilder()
                    .WithUrl("/streamHub")
                    .Build();

            connection.Closed += async (error) => await connection.StartAsync();

            var channel = 
                await connection.StreamAsChannelAsync<TweetResult>("StartStreaming", token);

            while (await channel.WaitToReadAsync())
            {
                while (channel.TryRead(out var tweet))
                {
                    callback(tweet);
                }
            }
        }
    }
}