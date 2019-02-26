using IEvangelist.Blazing.SignalR.Server.Services;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace IEvangelist.Blazing.SignalR.Server.Hubs
{
    public class StreamHub : Hub
    {
        readonly ITwitterService _twitterService;

        public StreamHub(ITwitterService twitterService) => _twitterService = twitterService;

        public Task RemoveTracks(string[] tracks) => _twitterService.RemoveTracksAsync(tracks);

        public Task AddTracks(string[] tracks) => _twitterService.AddTracksAsync(tracks);

        public Task StartTweetStream() => _twitterService.StartTweetStreamAsync();

        public void PauseTweetStream() => _twitterService.PauseTweetStream();

        public void StopTweetStream() => _twitterService.StopTweetStream();
    }
}