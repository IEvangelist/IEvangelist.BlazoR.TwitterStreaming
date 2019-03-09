namespace IEvangelist.Blazing.SignalR.Server.Models
{
    public class Prediction
    {
        // 0 = bad, 1 = good
        public float Probability { get; set; }

        public float Percentage => Probability * 100;
    }
}