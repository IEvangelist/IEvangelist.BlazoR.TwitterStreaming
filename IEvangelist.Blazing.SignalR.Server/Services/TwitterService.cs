using IEvangelist.Blazing.SignalR.Server.Hubs;
using IEvangelist.Blazing.SignalR.Shared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Streaming;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public class TwitterService : ITwitterService
    {
        readonly ILogger<TwitterService> _logger;
        readonly IHubContext<StreamHub> _hubContext;
        readonly IFilteredStream _filteredStream;

        public TwitterService(
            ILogger<TwitterService> logger,
            IHubContext<StreamHub> hubContext,
            IFilteredStream filteredStream)
        {
            _logger = logger;
            _hubContext = hubContext;
            _filteredStream = filteredStream;
            
            InitializeStream();
        }

        public Task RemoveTracksAsync(params string[] tracks)
            => HandleTracksAsync(false, tracks);

        public Task AddTracksAsync(params string[] tracks)
            => HandleTracksAsync(true, tracks);

        async Task HandleTracksAsync(bool add, params string[] tracks)
        {
            StopTweetStream();
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

        void InitializeStream()
        {
            _filteredStream.AddCustomQueryParameter("omit_script", "true");
            _filteredStream.NonMatchingTweetReceived += OnNonMatchingTweetReceived;
            _filteredStream.MatchingTweetReceived += OnMatchingTweetReceived;
            _filteredStream.DisconnectMessageReceived += OnDisconnectedMessageReceived;
            _filteredStream.StreamStarted += OnStreamStarted;
            _filteredStream.StreamStopped += OnStreamStopped;
            _filteredStream.StreamResumed += OnStreamResumed;
            _filteredStream.StreamPaused += OnStreamPaused;
            _filteredStream.WarningFallingBehindDetected += OnFallingBehindDetected;
        }

        async void OnNonMatchingTweetReceived(object sender, TweetEventArgs args) 
            => await BroadcastTweet(args?.Tweet, true);

        async void OnMatchingTweetReceived(object sender, MatchedTweetReceivedEventArgs args) 
            => await BroadcastTweet(args?.Tweet, false);

        async Task BroadcastTweet(ITweet iTweet, bool isOffTopic)
        {
            if (iTweet is null)
            {
                return;
            }

            var tweet = Tweet.GetOEmbedTweet(iTweet);
            if (tweet is null)
            {
                return;
            }

            await _hubContext.Clients.All.SendAsync("TweetReceived", new TweetResult
            {
                IsOffTopic = isOffTopic,
                AuthorName = tweet.AuthorName,
                AuthorURL = tweet.AuthorURL,
                CacheAge = tweet.CacheAge,
                Height = tweet.Height,
                HTML = tweet.HTML,
                ProviderURL = tweet.ProviderURL,
                Type = tweet.Type,
                URL = tweet.URL,
                Version = tweet.Version,
                Width = tweet.Width
            });
        }

        async void OnDisconnectedMessageReceived(object sender, DisconnectedEventArgs args)
        {
            const string status = "Twitter stream disconnected...";
            _logger.LogWarning(status, args);

            await SendStatusUpdateAsync(status);
        }

        async void OnStreamStarted(object sender, EventArgs args)
        {
            const string status = "Twitter stream started...";
            _logger.LogInformation(status);

            await SendStatusUpdateAsync(status);
        }

        async void OnStreamStopped(object sender, StreamExceptionEventArgs args)
        {
            var status = $"Twitter stream stopped, {args.DisconnectMessage}...";
            _logger.LogInformation(status);

            await SendStatusUpdateAsync(status);
        }

        async void OnStreamResumed(object sender, EventArgs e)
        {
            const string status = "Twitter stream resumed...";
            _logger.LogInformation(status);

            await SendStatusUpdateAsync(status);
        }

        async void OnStreamPaused(object sender, EventArgs e)
        {
            const string status = "Twitter stream paused...";
            _logger.LogInformation(status);

            await SendStatusUpdateAsync(status);
        }

        async void OnFallingBehindDetected(object sender, WarningFallingBehindEventArgs args)
        {
            var status = $"Twitter stream falling behind, {args.WarningMessage}...";
            _logger.LogInformation(status);

            await SendStatusUpdateAsync(status);
        }

        async Task SendStatusUpdateAsync(string status)
            => await _hubContext.Clients.All.SendAsync(
                "StatusUpdated",
                new Status
                {
                    IsStreaming = _filteredStream.StreamState == StreamState.Running,
                    Message = status
                });
    }
}