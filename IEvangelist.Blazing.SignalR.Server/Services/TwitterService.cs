using System.Threading.Tasks;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public class TwitterService : ITwitterService
    {
        public Task RemoveTracksAsync(params string[] tracks)
            => HandleTracksAsync(false, tracks);

        public Task AddTracksAsync(params string[] tracks)
            => HandleTracksAsync(true, tracks);

        Task HandleTracksAsync(bool add, params string[] tracks) => Task.CompletedTask;

        public Task StartTweetStreamAsync() => Task.CompletedTask;

        public void PauseTweetStream()
        {
        }

        public void StopTweetStream()
        {
        }
    }
}