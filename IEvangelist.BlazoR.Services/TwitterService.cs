using IEvangelist.BlazoR.Services.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Streaming;

namespace IEvangelist.BlazoR.Services
{
    public class TwitterService<T> : BackgroundService, ITwitterService<T> where T : Hub
    {
        readonly ILogger<TwitterService<T>> _logger;
        readonly IHubContext<T> _hubContext;
        readonly ISentimentService _sentimentService;
        readonly IFilteredStream _filteredStream;

        static bool IsInitialized = false;
        static readonly object Locker = new object();

        public TwitterService(
            ILogger<TwitterService<T>> logger,
            IHubContext<T> hubContext,
            ISentimentService sentimentService,
            IFilteredStream filteredStream)
        {
            _logger = logger;
            _hubContext = hubContext;
            _sentimentService = sentimentService;
            _filteredStream = filteredStream;

            InitializeStream();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                stoppingToken.ThrowIfCancellationRequested();
                if (_filteredStream.TracksCount > 0)
                {
                    await StartTweetStreamAsync();

                    if (_filteredStream.StreamState != StreamState.Running)
                    {
                        await Task.Delay(7500);
                    }
                }
                else
                {
                    await Task.Delay(5000);
                }
            }
        }

        public void RemoveTrack(string track) =>
            HandleTracks(false, track);

        public void AddTracks(ISet<string> tracks) =>
            HandleTracks(true, tracks?.ToArray());

        void HandleTracks(bool add, params string[] tracks)
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
        }

        public async Task StartTweetStreamAsync()
        {
            if (_filteredStream.StreamState != StreamState.Running)
            {
                _logger.LogInformation("Starting tweet stream.");
                await _filteredStream.StartStreamMatchingAnyConditionAsync();
            }
        }

        public void PauseTweetStream()
        {
            if (_filteredStream.StreamState != StreamState.Pause)
            {
                _logger.LogInformation("Pausing tweet stream.");
                _filteredStream.PauseStream();
            }
        }

        public void StopTweetStream()
        {
            if (_filteredStream.StreamState != StreamState.Stop)
            {
                _logger.LogInformation("Stoping tweet stream.");
                _filteredStream.StopStream();
            }
        }

        void InitializeStream()
        {
            lock (Locker)
            {
                if (IsInitialized) return;

                _filteredStream.AddCustomQueryParameter("omit_script", "true");

                //_filteredStream.KeepAliveReceived += async (o, e) =>
                //    await SendStatusUpdateAsync("Keep alive recieved...");
                //_filteredStream.LimitReached += async (o, e) =>
                //    await SendStatusUpdateAsync($"Limit receached, missed {e.NumberOfTweetsNotReceived:#,#} tweets...");
                //_filteredStream.JsonObjectReceived += async (o, e) =>
                //    await SendStatusUpdateAsync($"JSON recieved {e.Json}...");
                //_filteredStream.UnmanagedEventReceived += async (o, e) =>
                //    await SendStatusUpdateAsync($"Unexpected JSON message recieved {e.JsonMessageReceived}...");

                _filteredStream.DisconnectMessageReceived += OnDisconnectedMessageReceived;
                _filteredStream.MatchingTweetReceived += OnMatchingTweetReceived;
                //_filteredStream.NonMatchingTweetReceived += OnNonMatchingTweetReceived;
                _filteredStream.StreamStarted += OnStreamStarted;
                _filteredStream.StreamStopped += OnStreamStopped;
                _filteredStream.StreamResumed += OnStreamResumed;
                _filteredStream.StreamPaused += OnStreamPaused;
                _filteredStream.WarningFallingBehindDetected += OnFallingBehindDetected;

                IsInitialized = true;
            }
        }

        async void OnNonMatchingTweetReceived(object sender, TweetEventArgs args) =>
            await BroadcastTweet(args?.Tweet, true);

        async void OnMatchingTweetReceived(object sender, MatchedTweetReceivedEventArgs args) =>
            await BroadcastTweet(args?.Tweet, false);

        async Task BroadcastTweet(ITweet iTweet, bool isOffTopic)
        {
            if (iTweet is null)
            {
                return;
            }

            // If twitter thinks this might be sensitive
            // Let's check out its sentiment with machine learning...
            if (iTweet.PossiblySensitive)
            {
                var prediction = _sentimentService.Predict(iTweet.Text);
                if (prediction?.Percentage < 50)
                {
                    return;
                }
            }

            var tweet = iTweet.GenerateOEmbedTweet();
            if (tweet is null)
            {
                return;
            }

            await _hubContext.Clients.All.SendAsync("TweetReceived",
                new TweetResult
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
            var latestException = ExceptionHandler.GetLastException();
            var status = $"Twitter stream disconnected, {args.DisconnectMessage}...{latestException?.TwitterDescription}";
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
            var status = $"Twitter stream stopped {args.DisconnectMessage}... {args.Exception.Message}";
            _logger.LogInformation(status);

            await SendStatusUpdateAsync(status);
        }

        async void OnStreamResumed(object sender, EventArgs args)
        {
            const string status = "Twitter stream resumed...";
            _logger.LogInformation(status);

            await SendStatusUpdateAsync(status);
        }

        async void OnStreamPaused(object sender, EventArgs args)
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

        async Task SendStatusUpdateAsync(string status) =>
            await _hubContext.Clients.All.SendAsync(
                "StatusUpdated",
                new Status
                {
                    IsStreaming = _filteredStream.StreamState == StreamState.Running,
                    Message = status
                });
    }
}