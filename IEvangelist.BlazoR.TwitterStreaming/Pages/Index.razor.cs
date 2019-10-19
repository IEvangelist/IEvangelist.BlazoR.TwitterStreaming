using IEvangelist.BlazoR.Services.Models;
using IEvangelist.BlazoR.TwitterStreaming.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IEvangelist.BlazoR.TwitterStreaming.Pages
{
    public class IndexComponent : ComponentBase
    {
        protected bool IsStreaming;
        protected string Status = "Waiting for tweets...";

        protected string Track { get; set; }

        protected readonly List<TweetResult> OffTopicTweets = new List<TweetResult>();
        protected readonly List<TweetResult> Tweets = new List<TweetResult>();
        protected readonly ISet<string> Tracks = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "#SignalR",
            "#Blazor",
            "@telerik",
            "@aspnet",
            "@devreach",
            "@davidpine7"
        };

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }
        [Inject]
        protected ILogger<IndexComponent> Logger { get; set; }
        [Inject]
        protected IStreamService StreamService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            StreamService.RegisterStatusUpdatedHandler(StatusUpdated);
            StreamService.RegisterTweetReceivedHandler(TweetReceived);

            await StreamService.InitializeAsync();

            Logger.LogInformation("Initialized component");

            await StreamService.AddTracks(Tracks);
        }

        async Task StatusUpdated(Status status)
        {
            Logger.LogInformation($"Status: IsStreaming {status.IsStreaming}, Message {status.Message}.");

            Status = status.Message;
            IsStreaming = status.IsStreaming;

            StateHasChanged();

            await Task.CompletedTask;
        }

        async Task TweetReceived(TweetResult tweet)
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

            try
            {
                await JSRuntime.InvokeVoidAsync("twttr.widgets.load");
            }
            catch (Exception ex)
            {
            }
        }

        protected async Task AddTracks()
        {
            Tracks.Add(Track);
            Track = string.Empty;

            StateHasChanged();

            await StreamService.AddTracks(Tracks);
        }

        protected async Task RemoveTrack(string track)
        {
            Tracks.Remove(track);
            StateHasChanged();

            await StreamService.RemoveTrack(track);
        }

        protected async Task Start() =>
            await StreamService.Start();

        protected async Task Stop() =>
            await StreamService.Stop();

        protected async Task Pause() =>
            await StreamService.Pause();
    }
}