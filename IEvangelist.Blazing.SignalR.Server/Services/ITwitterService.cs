using System.Threading;
using System.Threading.Channels;
using Tweetinvi.Models;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public interface ITwitterService
    {
        ChannelReader<IOEmbedTweet> StartStreaming(CancellationToken token);
    }
}