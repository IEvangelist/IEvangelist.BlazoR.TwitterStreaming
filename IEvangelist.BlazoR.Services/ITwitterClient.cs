using IEvangelist.BlazoR.Services.Models;
using System.Threading.Tasks;

namespace IEvangelist.BlazoR.Services
{
    public interface ITwitterClient
    {
        Task TweetReceived(TweetResult tweet);

        Task StatusUpdated(Status status);
    }
}