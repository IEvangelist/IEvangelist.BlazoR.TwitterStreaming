using IEvangelist.BlazoR.Services.Models;
using System.Threading.Tasks;

namespace IEvangelist.BlazoR.Services
{
    public interface ITwitterClient
    {
        Task TweetReceivedAsync(TweetResult tweet);

        Task StatusUpdatedAsync(Status status);
    }
}