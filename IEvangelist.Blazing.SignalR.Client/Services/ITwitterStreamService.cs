using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IEvangelist.Blazing.SignalR.Shared;

namespace IEvangelist.Blazing.SignalR.Client.Services
{
    public interface ITwitterStreamService
    {
        void HandleStatusUpdates(Func<Status, Task> handler);

        void HandleTweets(Func<TweetResult, Task> handler);

        Task AddTracksAsync(ISet<string> tracks);

        Task RemoveTrackAsync(string track);

        Task StartAsync();

        Task PauseAsync();

        Task StopAsync();
    }
}