using System.Collections.Generic;
using System.Threading.Tasks;
using IEvangelist.Blazing.SignalR.Client.Services;
using IEvangelist.Blazing.SignalR.Shared;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace IEvangelist.Blazing.SignalR.Client.Pages
{
    public class TwitterStreamComponent : BlazorComponent
    {
        protected bool IsStreaming;
        protected string Status = "Waiting for tweets...";

        protected readonly List<TweetResult> OffTopicTweets = new List<TweetResult>();
        protected readonly List<TweetResult> Tweets = new List<TweetResult>();
        protected readonly List<string> Tracks = new List<string>
        {
            "#DeveloperCommunity",
            "#NDCMinnesota",
            "#SignalR",
            "@DavidPine7"
        };

        [Inject]
        protected ITwitterStreamService StreamService { get; set; }
        [Inject]
        protected ILogger<TwitterStreamComponent> Logger { get; set; }

        protected override Task OnInitAsync()
        {
            StreamService.HandleTweets(OnTweetReceived);
            StreamService.HandleStatusUpdates(OnStatusUpdated);

            return StreamService.AddTracksAsync(Tracks);
        }

        async Task OnStatusUpdated(Status status)
        {
            Logger.LogInformation($"Status: IsStreaming {status.IsStreaming}, Message {status.Message}.");

            Status = status.Message;
            IsStreaming = status.IsStreaming;

            StateHasChanged();

            await Task.CompletedTask;
        }

        async Task OnTweetReceived(TweetResult tweet)
        {
            if (tweet.IsOffTopic)
            {
                OffTopicTweets.Add(tweet);
            }
            else
            {
                Tweets.Add(tweet);
            }

            StateHasChanged();

            await JSRuntime.Current.InvokeAsync<bool>("twttr.widgets.load");
        }

        protected async Task AddTrack()
        {
            var track = await JSRuntime.Current.InvokeAsync<string>("getAndClearTrack");

            Tracks.Add(track);
            StateHasChanged();

            await StreamService.AddTracksAsync(Tracks);
        }

        protected async Task RemoveTrack(string track)
        {
            Tracks.Remove(track);
            StateHasChanged();

            await StreamService.RemoveTrackAsync(track);
        }
    }
}