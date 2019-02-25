using System;
using IEvangelist.Blazing.SignalR.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Streaming;
using TweetResult = IEvangelist.Blazing.SignalR.Server.Models.TweetResult;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public class TwitterService : ITwitterService
    {
        bool _isInitialized;

        readonly ILogger<TwitterService> _logger;
        readonly IHubContext<StreamHub> _hubContext;
        readonly IFilteredStream _filteredStream;

        public TwitterService(
            ILogger<TwitterService> logger,
            IHubContext<StreamHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
            _filteredStream = Stream.CreateFilteredStream();
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

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _filteredStream.AddTrack("#developercommunity");
            _filteredStream.AddTrack("#ndcminnesota");
            _filteredStream.AddTrack("#signalr");
            _filteredStream.AddTrack("@davidpine7");

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

            _filteredStream.DisconnectMessageReceived += (sender, args) =>
            {
                _logger.LogWarning("Disconnected from twitter _filteredStream.", args);
            };

            _filteredStream.StreamStarted += (sender, args) =>
            {
                _logger.LogInformation("Twitter stream started.");
            };

            _filteredStream.StreamStopped += (sender, args) =>
            {
                _logger.LogInformation($"Twitter stream stopped, {args}.");
            };

            _filteredStream.StreamResumed += (sender, args) =>
            {
                _logger.LogInformation("Twitter stream resumed.");
            };

            _filteredStream.StreamPaused += (sender, args) =>
            {
                _logger.LogInformation("Twitter stream paused.");
            };

            _filteredStream.WarningFallingBehindDetected += (sender, args) =>
            {
                _logger.LogInformation($"Twitter stream falling behind, {args.WarningMessage}.");
            };

            _isInitialized = true;
        }
    }
}