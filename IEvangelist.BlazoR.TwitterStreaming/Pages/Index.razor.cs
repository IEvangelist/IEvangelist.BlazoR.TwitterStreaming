using IEvangelist.BlazoR.Services;
using IEvangelist.BlazoR.Services.Models;
using IEvangelist.BlazoR.TwitterStreaming.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IEvangelist.BlazoR.TwitterStreaming.Pages
{
    public class IndexComponent : ComponentBase
    {
        HubConnection _connection;

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
        protected ITwitterService<StreamHub> StreamService { get; set; }
        [Inject]
        protected ILogger<IndexComponent> Logger { get; set; }
        [Inject]
        protected IUriHelper UriHelper { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var streamHub = $"{UriHelper.GetBaseUri()}streamHub";

            _connection =
                new HubConnectionBuilder()
                   .WithUrl(streamHub, HttpTransportType.WebSockets | HttpTransportType.LongPolling)
                   .AddMessagePackProtocol()
                   .WithAutomaticReconnect()
                   .Build();

            _connection.On<Status>("StatusUpdatedAsync", OnStatusUpdated);
            _connection.On<TweetResult>("TweetReceivedAsync", OnTweetReceived);

            await _connection.StartAsync();
            await StreamService.AddTracksAsync(Tracks);
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

            await JSRuntime.InvokeAsync<bool>("twttr.widgets.load");
        }

        protected async Task AddTrack()
        {
            Tracks.Add(Track);
            Track = string.Empty;

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