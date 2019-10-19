using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IEvangelist.BlazoR.Services.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace IEvangelist.BlazoR.TwitterStreaming.Services
{
    public class StreamService : IStreamService
    {
        bool _isInitialized = false;

        static readonly SemaphoreSlim AsyncLock = new SemaphoreSlim(1, 1);

        readonly ILogger<StreamService> _logger;
        readonly HubConnection _connection =
            new HubConnectionBuilder()
                .WithUrl("http://localhost:61977/streamHub")
                .WithAutomaticReconnect()
                .Build();

        public StreamService(
            ILogger<StreamService> logger) => _logger = logger;

        async Task IStreamService.InitializeAsync()
        {
            await AsyncLock.WaitAsync();
            try
            {
                if (_isInitialized)
                {
                    _logger.LogInformation($"SignalR = {_connection.State}, Id = {_connection.ConnectionId}");
                    return;
                }

                await _connection.StartAsync();
                _logger.LogInformation($"SignalR = {_connection.State}, Id = {_connection.ConnectionId}");
            }
            finally
            {
                _isInitialized = true;
                AsyncLock.Release();
            }
        }

        IDisposable IStreamService.RegisterStatusUpdatedHandler(
            Func<Status, Task> handler) =>
            _connection.On<Status>("StatusUpdated", handler);

        IDisposable IStreamService.RegisterTweetReceivedHandler(
            Func<TweetResult, Task> handler) =>
            _connection.On<TweetResult>("TweetReceived", handler);

        Task IStreamService.AddTracks(ISet<string> tracks) =>
            _connection.InvokeAsync(nameof(IStreamService.AddTracks), tracks);

        Task IStreamService.RemoveTrack(string track) =>
             _connection.InvokeAsync(nameof(IStreamService.RemoveTrack), track);

        async Task IStreamService.Start()
        {
            if (_connection.State != HubConnectionState.Connected)
            {
                await _connection.StartAsync();
            }

            await _connection.InvokeAsync(nameof(IStreamService.Start));
        }

        Task IStreamService.Stop() =>
            _connection.InvokeAsync(nameof(IStreamService.Stop));

        Task IStreamService.Pause() =>
            _connection.InvokeAsync(nameof(IStreamService.Pause));
    }
}