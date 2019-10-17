using IEvangelist.BlazoR.Services.Models;
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
        protected ILogger<IndexComponent> Logger { get; set; }
        [Inject]
        protected NavigationManager UriHelper { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var streamHub = $"{UriHelper.BaseUri}streamHub";

            Logger.LogInformation("On initialized called.");

            _connection =
                new HubConnectionBuilder()
                   .WithUrl(streamHub)
                   //.AddMessagePackProtocol()
                   .WithAutomaticReconnect()
                   .Build();

            // This should fire from the stream hub, when a status is pumped through.

            _connection.On<Status>("StatusUpdatedAsync", OnStatusUpdated);
            _connection.On<TweetResult>("TweetReceivedAsync", OnTweetReceived);

            await _connection.StartAsync();

            // I do know that the "AddTracks" is getting called, verified that with a break point
            // Then it changes the status and that's where things break
            await _connection.InvokeAsync("AddTracks", Tracks);
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

            await _connection.InvokeAsync("AddTracks", Tracks);
        }

        protected async Task RemoveTrack(string track)
        {
            Tracks.Remove(track);
            StateHasChanged();

            await _connection.InvokeAsync("AddTracks", track);
        }

        protected async Task StartAsync() => 
            await _connection.InvokeAsync("Start");

        protected async Task StopAsync() =>
            await _connection.InvokeAsync("Stop");

        protected async Task PauseAsync() =>
            await _connection.InvokeAsync("Pause");
    }
}