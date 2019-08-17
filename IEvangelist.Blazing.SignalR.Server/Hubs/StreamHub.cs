using IEvangelist.Blazing.SignalR.Server.Services;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IEvangelist.Blazing.SignalR.Server.Hubs
{
    public class StreamHub : Hub
    {
        readonly ITwitterService _twitterService;

        public StreamHub(ITwitterService twitterService) => _twitterService = twitterService;

        public Task RemoveTrack(string track) => _twitterService.RemoveTrackAsync(track);

        public Task AddTracks(ISet<string> tracks) => _twitterService.AddTracksAsync(tracks);

        public Task StartTweetStream() => _twitterService.StartTweetStreamAsync();

        public void PauseTweetStream() => _twitterService.PauseTweetStream();

        public void StopTweetStream() => _twitterService.StopTweetStream();
    }
}