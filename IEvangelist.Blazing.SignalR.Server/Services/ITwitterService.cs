using System.Threading;
using System.Threading.Tasks;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public interface ITwitterService
    {
        Task StartStreamingAsync(CancellationToken token);
    }
}