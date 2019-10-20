using IEvangelist.BlazoR.Services;
using IEvangelist.BlazoR.Services.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IEvangelist.BlazoR.TweeteR.Hubs
{
    public class StreamHub : Hub<ITwitterClient>
    {
        readonly ITwitterService<StreamHub> _twitterService;

        public StreamHub(ITwitterService<StreamHub> twitterService) =>
            _twitterService = twitterService;

        public void RemoveTrack(string track) =>
            _twitterService.RemoveTrack(track);

        public void AddTracks(ISet<string> tracks) =>
            _twitterService.AddTracks(tracks);

        public Task Start() =>
            _twitterService.StartTweetStreamAsync();

        public void Pause() =>
            _twitterService.PauseTweetStream();

        public void Stop() =>
            _twitterService.StopTweetStream();
    }
}