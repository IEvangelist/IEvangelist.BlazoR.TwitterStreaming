using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IEvangelist.BlazoR.Services
{
    public interface ITwitterService<T> where T : Hub<ITwitterClient>
    {
        Task RemoveTrackAsync(string track);

        Task AddTracksAsync(ISet<string> tracks);

        Task StartTweetStreamAsync();

        void PauseTweetStream();

        void StopTweetStream();
    }
}