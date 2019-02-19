using IEvangelist.Blazing.SignalR.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IEvangelist.Blazing.SignalR.Client.Services
{
    public interface ITwitterStreamService
    {
        Task SubscribeAsync(Action<TweetResult> callback, CancellationToken token);
    }
}