using System.Threading.Tasks;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public interface ITwitterService
    {
        Task AddTracksAsync(params string[] tracks);

        Task StartTweetStreamAsync();

        void PauseTweetStream();

        void StopTweetStream();
    }
}