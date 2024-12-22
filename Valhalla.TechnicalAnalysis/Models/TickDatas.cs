namespace Valhalla.TechnicalAnalysis.Models
{
    public class TickDatas
    {
        public double Price { get; set; }
        public double Volume
        {
            get
            { 
                return BuyVolume + SellVolume; 
            }
        }
        public long Delta { get; set; }
        public double BuyVolume { get; set; }
        public double SellVolume { get; set; }
        public long Trades
        {
            get
            {
                return BuyTrades + SellTrades;
            }
        }
        public long BuyTrades { get; set; }
        public long SellTrades { get; set; }
        public DateTime Time { get; set; }
    }

}
