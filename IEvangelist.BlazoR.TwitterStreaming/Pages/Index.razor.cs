using IEvangelist.BlazoR.Services.Models;
using IEvangelist.BlazoR.TwitterStreaming.Options;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IEvangelist.BlazoR.TwitterStreaming.Pages
{
    public class IndexComponent : ComponentBase
    {
        protected string Track { get; set; }

        protected readonly List<TweetResult> Tweets = new List<TweetResult>();
        protected readonly ISet<string> Tracks = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "#SignalR",
            "#DeveloperCommunity",
            "#Blazor",
            "@devreach",
            "@davidpine7"
        };

        [Inject]
        protected IJSRuntime JSRuntime { get; set; }
        [Inject]
        protected ILogger<IndexComponent> Logger { get; set; }
        [Inject]
        protected IOptions<TweeteROptions> Options { get; set; }

        protected override async Task OnInitializedAsync() => 
            await JSRuntime.InvokeVoidAsync(
                "start", $"{Options.Value.BaseAddress}/streamHub", Tracks);

        protected async Task AddTracks()
        {
            Tracks.Add(Track);
            Track = string.Empty;

            StateHasChanged();

            await JSRuntime.InvokeVoidAsync(nameof(AddTracks), Tracks);
        }

        protected async Task RemoveTrack(string track)
        {
            Tracks.Remove(track);
            StateHasChanged();

            await JSRuntime.InvokeVoidAsync(nameof(RemoveTrack), track);
        }

        protected async Task Start() =>
            await JSRuntime.InvokeVoidAsync("startStream");

        protected async Task Stop() =>
            await JSRuntime.InvokeVoidAsync("stopStream");

        protected async Task Pause() =>
            await JSRuntime.InvokeVoidAsync("pauseStream");
    }
}