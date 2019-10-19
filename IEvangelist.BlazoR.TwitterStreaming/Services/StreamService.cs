using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Blazor.Extensions;
using IEvangelist.BlazoR.Services.Models;
using Microsoft.Extensions.Logging;

namespace IEvangelist.BlazoR.TwitterStreaming.Services
{
    public class StreamService : IStreamService
    {
        readonly ILogger<StreamService> _logger;
        readonly HubConnection _connection;

        public StreamService(
            ILogger<StreamService> logger,
            HubConnectionBuilder builder) =>
            (_logger, _connection) =
                (logger, builder.WithUrl("http://localhost:61977/streamHub")
                                .Build());

        async Task IStreamService.InitializeAsync()
        {
            _connection.OnClose(async ex =>
            {
                await _connection.StartAsync();
                _logger.LogError(ex, ex.Message);
            });

            await _connection.StartAsync();
            _logger.LogInformation("Initialized.");
        }

        IDisposable IStreamService.RegisterStatusUpdatedHandler(
            Func<Status, Task> handler) =>
            _connection.On<Status>("StatusUpdated", handler);

        IDisposable IStreamService.RegisterTweetReceivedHandler(
            Func<TweetResult, Task> handler) =>
            _connection.On<TweetResult>("TweetReceived", handler);

        async Task IStreamService.AddTracks(ISet<string> tracks) =>
            await _connection.InvokeAsync(nameof(IStreamService.AddTracks), tracks);

        async Task IStreamService.RemoveTrack(string track) =>
             await _connection.InvokeAsync(nameof(IStreamService.RemoveTrack), track);

        async Task IStreamService.Start() =>
            await _connection.InvokeAsync(nameof(IStreamService.Start));

        async Task IStreamService.Stop() =>
            await _connection.InvokeAsync(nameof(IStreamService.Stop));

        async Task IStreamService.Pause() =>
            await _connection.InvokeAsync(nameof(IStreamService.Pause));
    }
}