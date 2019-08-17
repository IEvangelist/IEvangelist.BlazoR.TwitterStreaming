using System.Collections.Generic;
using System.Threading.Tasks;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public interface ITwitterService
    {
        Task RemoveTrackAsync(string track);

        Task AddTracksAsync(ISet<string> tracks);

        Task StartTweetStreamAsync();

        void PauseTweetStream();

        void StopTweetStream();
    }
}