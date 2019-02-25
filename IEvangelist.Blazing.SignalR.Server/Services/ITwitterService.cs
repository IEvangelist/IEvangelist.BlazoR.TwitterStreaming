using System.Threading.Tasks;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public interface ITwitterService
    {
        void Initialize();

        Task StartTweetStreamAsync();

        void PauseTweetStream();

        void StopTweetStream();
    }
}