using IEvangelist.Blazing.SignalR.Shared;
using System.Threading;
using System.Threading.Channels;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public interface ITwitterService
    {
        ChannelReader<TweetResult> StartStreaming(CancellationToken token);
    }
}