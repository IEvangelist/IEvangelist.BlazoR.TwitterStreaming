using Blazor.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IEvangelist.Blazing.SignalR.Shared;

namespace IEvangelist.Blazing.SignalR.Client.Services
{
    public class TwitterStreamService : ITwitterStreamService
    {
        readonly HubConnection _connection;
        readonly Task _startTask;

        public TwitterStreamService()
        {
            _connection =
                new HubConnectionBuilder()
                   .WithUrl("/streamHub")
                   .AddMessagePackProtocol()
                   .Build();

            _connection.OnClose(async ex => await _connection.StartAsync());
            _startTask = _connection.StartAsync();
        }

        public void HandleTweets(Func<TweetResult, Task> handler) 
            => _connection.On("TweetReceived", handler);

        public void HandleStatusUpdates(Func<string, Task> handler)
            => _connection.On("StatusUpdated", handler);

        public async Task AddTracksAsync(List<string> tracks)
        {
            await _startTask;
            await _connection.InvokeAsync("AddTracks", tracks);
        }

        public async Task RemoveTrackAsync(string track)
        {
            await _startTask;
            await _connection.InvokeAsync("RemoveTracks", track);
        }

        public async Task StartAsync()
        {
            await _startTask;
            await _connection.InvokeAsync("StartTweetStream");
        }

        public async Task PauseAsync()
        {
            await _startTask;
            await _connection.InvokeAsync("PauseTweetStream");
        }

        public async Task StopAsync()
        {
            await _startTask;
            await _connection.InvokeAsync("StopTweetStream");
        }
    }
}