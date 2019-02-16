using IEvangelist.Blazing.SignalR.Server.Services;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Channels;
using Tweetinvi.Models;

namespace IEvangelist.Blazing.SignalR.Server.Hubs
{
    public class StreamHub : Hub
    {
        readonly ITwitterService _twitterService;

        public StreamHub(ITwitterService twitterService) => _twitterService = twitterService;

        public ChannelReader<IOEmbedTweet> StartStreaming()
        {
            // Is this right?
            var cts = new CancellationTokenSource();
            return _twitterService.StartStreaming(cts.Token);
        }
    }
}