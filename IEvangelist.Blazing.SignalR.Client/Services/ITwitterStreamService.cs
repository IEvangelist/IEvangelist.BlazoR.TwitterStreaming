using System;
using System.Threading;
using System.Threading.Tasks;
using IEvangelist.Blazing.SignalR.Client.Models;

namespace IEvangelist.Blazing.SignalR.Client.Services
{
    public interface ITwitterStreamService
    {
        Task SubscribeAsync(Func<TweetResult, Task> handler);
    }
}