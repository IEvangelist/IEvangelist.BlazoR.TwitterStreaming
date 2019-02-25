using IEvangelist.Blazing.SignalR.Server.Services;
using Microsoft.AspNetCore.SignalR;
using System.Threading;
using System.Threading.Tasks;

namespace IEvangelist.Blazing.SignalR.Server.Hubs
{
    public class StreamHub : Hub
    {
        readonly ITwitterService _twitterService;
        readonly CancellationToken _token = new CancellationToken();

        public StreamHub(ITwitterService twitterService) => _twitterService = twitterService;

        public Task StartStreaming() 
            => _twitterService.StartStreamingAsync(_token);
    }
}