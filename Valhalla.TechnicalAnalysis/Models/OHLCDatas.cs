namespace Valhalla.TechnicalAnalysis.Models
{
    public class OHLCDatas
    {
        public double Open { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Close { get; set; }

        public DateTime Time { get; set; }

        public TimeSpan TimeSpan { get; set; }

        public List<TickDatas> Ticks { get; set; } = new List<TickDatas> ();

        public OHLCDatas(double open, double high, double low, double close, DateTime time, TimeSpan timeSpan)
        {
            this.Open = open;
            this.High = high;
            this.Low = low;
            this.Close = close;
            this.Time = time;
            this.TimeSpan = timeSpan;
        }
    }
}
