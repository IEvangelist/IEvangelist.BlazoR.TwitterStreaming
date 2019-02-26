using IEvangelist.Blazing.SignalR.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Streaming;
using TweetResult = IEvangelist.Blazing.SignalR.Server.Models.TweetResult;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public class TwitterService : ITwitterService
    {
        readonly ILogger<TwitterService> _logger;
        readonly IHubContext<StreamHub> _hubContext;
        readonly IFilteredStream _filteredStream = Stream.CreateFilteredStream();

        public TwitterService(
            ILogger<TwitterService> logger,
            IHubContext<StreamHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
            
            WireEventListeners();
        }

        public Task RemoveTracksAsync(params string[] tracks)
            => HandleTracksAsync(false, tracks);

        public Task AddTracksAsync(params string[] tracks)
            => HandleTracksAsync(true, tracks);

        async Task HandleTracksAsync(bool add, params string[] tracks)
        {
            PauseTweetStream();
            foreach (var track in tracks)
            {
                if (add)
                {
                    _filteredStream.AddTrack(track);
                }
                else
                {
                    _filteredStream.RemoveTrack(track);
                }
            }

            await StartTweetStreamAsync();
        }

        public async Task StartTweetStreamAsync()
        {
            if (_filteredStream.StreamState != StreamState.Running)
            {
                await _filteredStream.StartStreamMatchingAnyConditionAsync();
            }
        }

        public void PauseTweetStream()
        {
            if (_filteredStream.StreamState != StreamState.Pause)
            {
                _filteredStream.PauseStream();
            }
        }

        public void StopTweetStream()
        {
            if (_filteredStream.StreamState != StreamState.Stop)
            {
                _filteredStream.StopStream();
            }
        }

        void WireEventListeners()
        {
            _filteredStream.MatchingTweetReceived += async (sender, args) =>
            {
                var tweet = Tweet.GetOEmbedTweet(args.Tweet);
                var index = tweet.HTML.IndexOf("<script", StringComparison.Ordinal);
                var html = tweet.HTML.Substring(0, index);
                await _hubContext.Clients.All.SendAsync("TweetReceived", new TweetResult
                {
                    AuthorName = tweet.AuthorName,
                    AuthorURL = tweet.AuthorURL,
                    CacheAge = tweet.CacheAge,
                    Height = tweet.Height,
                    HTML = html,
                    ProviderURL = tweet.ProviderURL,
                    Type = tweet.Type,
                    URL = tweet.URL,
                    Version = tweet.Version,
                    Width = tweet.Width
                });
            };

            _filteredStream.DisconnectMessageReceived += async (sender, args) =>
            {
                const string status = "Twitter stream disconnected";
                _logger.LogWarning(status, args);

                await SendStatusUpdateAsync(status);
            };

            _filteredStream.StreamStarted += async (sender, args) =>
            {
                const string status = "Twitter stream started";
                _logger.LogInformation(status);

                await SendStatusUpdateAsync(status);
            };

            _filteredStream.StreamStopped += async (sender, args) =>
            {
                var status = $"Twitter stream stopped, {args.DisconnectMessage}.";
                _logger.LogInformation(status);

                await SendStatusUpdateAsync(status);
            };

            _filteredStream.StreamResumed += async (sender, args) =>
            {
                const string status = "Twitter stream resumed";
                _logger.LogInformation(status);

                await SendStatusUpdateAsync(status);
            };

            _filteredStream.StreamPaused += async (sender, args) =>
            {
                const string status = "Twitter stream paused";
                _logger.LogInformation(status);

                await SendStatusUpdateAsync(status);
            };

            _filteredStream.WarningFallingBehindDetected += async (sender, args) =>
            {
                var status = $"Twitter stream falling behind, {args.WarningMessage}.";
                _logger.LogInformation(status);

                await SendStatusUpdateAsync(status);
            };
        }

        Task SendStatusUpdateAsync(string status)
            => _hubContext.Clients.All.SendAsync("StatusUpdated", status);
    }
}