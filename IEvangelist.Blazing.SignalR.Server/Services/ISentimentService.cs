using IEvangelist.Blazing.SignalR.Server.Models;

namespace IEvangelist.Blazing.SignalR.Server.Services
{
    public interface ISentimentService
    {
        Prediction Predict(string text);
    }
}