using IEvangelist.BlazoR.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IEvangelist.BlazoR.TwitterStreaming.Services
{
    public interface IStreamService
    {
        Task InitializeAsync();

        Task Start();

        Task Stop();

        Task Pause();

        Task AddTracks(ISet<string> tracks);

        Task RemoveTrack(string track);

        IDisposable RegisterStatusUpdatedHandler(Func<Status, Task> handler);

        IDisposable RegisterTweetReceivedHandler(Func<TweetResult, Task> handler);
    }
}