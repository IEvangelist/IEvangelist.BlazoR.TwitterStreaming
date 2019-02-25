using Blazor.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using IEvangelist.Blazing.SignalR.Client.Models;

namespace IEvangelist.Blazing.SignalR.Client.Services
{
    public class TwitterStreamService : ITwitterStreamService
    {
        public async Task SubscribeAsync(Func<TweetResult, Task> handler)
        {
            var connection =
                new HubConnectionBuilder()
                    .WithUrl("/streamHub")
                    .AddMessagePackProtocol()
                    .Build();

            connection.OnClose(async ex => await connection.StartAsync());
            connection.On<TweetResult>("TweetReceived", handler);

            await connection.StartAsync();
            await connection.InvokeAsync("StartStreaming");
        }
    }
}