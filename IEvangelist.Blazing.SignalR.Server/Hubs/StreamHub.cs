using IEvangelist.Blazing.SignalR.Server.Services;
using IEvangelist.Blazing.SignalR.Shared;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IEvangelist.Blazing.SignalR.Server.Hubs
{
    public class StreamHub : Hub
    {
        readonly ITwitterService _twitterService;

        public StreamHub(ITwitterService twitterService) => _twitterService = twitterService;

        public ChannelReader<TweetResult> StartStreaming(CancellationToken token) 
            => _twitterService.StartStreaming(token);

        public Task SendMessage(string message)
            => Clients.All.SendAsync("SendMessage", message);
    }
}