using IEvangelist.BlazoR.Services.Models;
using Microsoft.ML;
using System.IO;

namespace IEvangelist.BlazoR.Services
{
    public class SentimentService : ISentimentService
    {
        readonly MLContext _context = new MLContext();
        readonly PredictionEngine<Sentiment, Prediction> _predictionEngine;

        public SentimentService()
        {
            var model = _context.Model.Load(File.Open(@".\SentimentModel.zip", FileMode.Open));
            _predictionEngine = model.CreatePredictionEngine<Sentiment, Prediction>(_context);
        }

        public Prediction Predict(string text)
            => _predictionEngine?.Predict(new Sentiment { SentimentText = text });
    }
}