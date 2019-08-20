namespace IEvangelist.BlazoR.Services
{
    public interface ISentimentService
    {
        Prediction Predict(string text);
    }
}