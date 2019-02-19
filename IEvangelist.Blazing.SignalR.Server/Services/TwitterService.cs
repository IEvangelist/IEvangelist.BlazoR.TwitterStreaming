using IEvangelist.Blazing.SignalR.Shared;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Tweetinvi;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public class TwitterService : ITwitterService
    {
        readonly ILogger<TwitterService> _logger;

        public TwitterService(ILogger<TwitterService> logger) => _logger = logger;

        public ChannelReader<TweetResult> StartStreaming(CancellationToken token)
        {
            var channel = Channel.CreateUnbounded<TweetResult>();

            _ = WriteTweetsFromStream(channel.Writer, token);

            return channel.Reader;
        }

        async Task WriteTweetsFromStream(ChannelWriter<TweetResult> writer, CancellationToken token)
        {
            var stream = Stream.CreateFilteredStream();

            stream.AddTrack("#developercommunity");
            stream.AddTrack("#ndcminnesota");
            stream.AddTrack("#signalr");
            stream.AddTrack("@davidpine7");

            stream.MatchingTweetReceived += async (sender, args) =>
            {
                if (token.IsCancellationRequested)
                {
                    stream.StopStream();
                    token.ThrowIfCancellationRequested();
                }

                var tweet = Tweet.GetOEmbedTweet(args.Tweet);
                await writer.WriteAsync(new TweetResult
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
            stream.DisconnectMessageReceived += (sender, args) =>
            {
                _logger.LogWarning("Disconnected from twitter stream.", args);
            };
            stream.StreamStarted += (sender, args) =>
            {
                _logger.LogInformation("Twitter stream started.");
            };
            stream.StreamStopped += (sender, args) =>
            {
                _logger.LogInformation($"Twitter stream stopped, {args}.");
            };
            stream.StreamResumed += (sender, args) =>
            {
                _logger.LogInformation("Twitter stream resumed.");
            };
            stream.StreamPaused += (sender, args) =>
            {
                _logger.LogInformation("Twitter stream paused.");
            };
            stream.WarningFallingBehindDetected += (sender, args) =>
            {
                _logger.LogInformation($"Twitter stream falling behind, {args.WarningMessage}.");
            };

            await stream.StartStreamMatchingAnyConditionAsync();

            // I'm not certain this is correct... It feels very wrong.
            // I need to investigate this more to see the correct way to have an event-driven
            // streaming mechenism, that sits behind a channel reader instead of blocking.
            token.WaitHandle.WaitOne();
            
            writer.TryComplete();
        }
    }
}