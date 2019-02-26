using System.Threading.Tasks;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public interface ITwitterService
    {
        Task RemoveTracksAsync(params string[] tracks);

        Task AddTracksAsync(params string[] tracks);

        Task StartTweetStreamAsync();

        void PauseTweetStream();

        void StopTweetStream();
    }
}