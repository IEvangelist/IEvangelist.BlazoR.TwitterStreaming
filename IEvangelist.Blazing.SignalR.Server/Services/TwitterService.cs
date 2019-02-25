using IEvangelist.Blazing.SignalR.Server.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Threading;
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
        readonly IFilteredStream _filteredStream; 

        public TwitterService(
            ILogger<TwitterService> logger,
            IHubContext<StreamHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
            _filteredStream = Stream.CreateFilteredStream();
        }

        public async Task StartStreamingAsync(CancellationToken token)
        {
            if (_filteredStream.StreamState == StreamState.Running)
            {
                return;
            }

            _filteredStream.AddTrack("#developercommunity");
            _filteredStream.AddTrack("#ndcminnesota");
            _filteredStream.AddTrack("#signalr");
            _filteredStream.AddTrack("@davidpine7");

            _filteredStream.MatchingTweetReceived += async (sender, args) =>
            {
                if (token.IsCancellationRequested)
                {
                    _filteredStream.StopStream();
                    token.ThrowIfCancellationRequested();
                }

                var tweet = Tweet.GetOEmbedTweet(args.Tweet);
                await _hubContext.Clients.All.SendAsync("TweetReceived", new TweetResult
                {
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
                }, token);
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

            await _filteredStream.StartStreamMatchingAnyConditionAsync();
        }
    }
}