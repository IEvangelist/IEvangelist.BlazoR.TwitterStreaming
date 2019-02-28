using System.Collections.Generic;
using System.Threading.Tasks;
using IEvangelist.Blazing.SignalR.Client.Services;
using IEvangelist.Blazing.SignalR.Shared;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.JSInterop;

namespace IEvangelist.Blazing.SignalR.Client.Pages
{
    public class TwitterStreamComponent : BlazorComponent
    {
        protected string Status = "Waiting for tweets...";

        protected readonly List<TweetResult> Tweets = new List<TweetResult>();
        protected readonly List<string> Tracks = new List<string>
        {
            "#developercommunity",
            "#ndcminnesota",
            "#signalr",
            "@davidpine7"
        };

        [Inject]
        protected ITwitterStreamService StreamService { get; set; }

        public string Track { get; set; } = string.Empty;

        protected override Task OnInitAsync()
        {
            StreamService.HandleTweets(OnTweetReceived);
            StreamService.HandleStatusUpdates(OnStatusUpdated);

            return StreamService.AddTracksAsync(Tracks);
        }

        protected Task OnStatusUpdated(string status)
        {
            Status = status;
            StateHasChanged();

            return Task.CompletedTask;
        }

        protected async Task OnTweetReceived(TweetResult tweet)
        {
            Tweets.Add(tweet);
            StateHasChanged();

            await JSRuntime.Current.InvokeAsync<bool>("twttr.widgets.load");
        }

        protected void OnTrackChanged(string track) => Track = track ?? string.Empty;

        protected Task AddTrack()
        {
            if (string.IsNullOrWhiteSpace(Track))
            {
                return Task.CompletedTask;
            }

            Tracks.Add(Track);
            Track = string.Empty;

            return StreamService.AddTracksAsync(Tracks);
        }

        protected Task RemoveTrack(string track)
        {
            Tracks.Remove(track);

            return StreamService.RemoveTrackAsync(track);
        }
    }
}