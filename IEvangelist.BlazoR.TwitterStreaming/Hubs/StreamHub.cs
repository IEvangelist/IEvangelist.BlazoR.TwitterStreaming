using IEvangelist.BlazoR.Services;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IEvangelist.BlazoR.TwitterStreaming.Hubs
{
    public class StreamHub : Hub<ITwitterClient>
    {
        readonly ITwitterService<StreamHub> _twitterService;

        public StreamHub(ITwitterService<StreamHub> twitterService) => _twitterService = twitterService;

        public Task RemoveTrack(string track) => _twitterService.RemoveTrackAsync(track);

        public Task AddTracks(ISet<string> tracks) => _twitterService.AddTracksAsync(tracks);

        public Task Start() => _twitterService.StartTweetStreamAsync();

        public void Pause() => _twitterService.PauseTweetStream();

        public void Stop() => _twitterService.StopTweetStream();
    }
}