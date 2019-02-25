using Blazor.Extensions;
using System;
using System.Threading.Tasks;
using IEvangelist.Blazing.SignalR.Client.Models;

namespace IEvangelist.Blazing.SignalR.Client.Services
{
    public class TwitterStreamService : ITwitterStreamService
    {
        readonly HubConnection _connection;

        public TwitterStreamService()
        {
            _connection =
                new HubConnectionBuilder()
                   .WithUrl("/streamHub")
                   .AddMessagePackProtocol()
                   .Build();

            _connection.OnClose(async ex => await _connection.StartAsync());
        }

        public async Task SubscribeAsync(Func<TweetResult, Task> handler)
        {
            _connection.On<TweetResult>("TweetReceived", handler);

            await _connection.StartAsync();
            await _connection.InvokeAsync("StartTweetStream");
        }

        public async Task PauseAsync()
            => await _connection.InvokeAsync("PauseTweetStream");

        public async Task StopAsync()
            => await _connection.InvokeAsync("StopTweetStream");
    }
}