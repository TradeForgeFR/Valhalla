using Avalonia.Controls;
using ReactiveUI;
using ScottPlot;
using ScottPlot.Avalonia;
using ScottPlot.DataSources;
using Valhalla.Charting.CustomSeries;
using Valhalla.Charting.Managers;

namespace Valhalla.Charting
{
    public class StockChart : ReactiveObject
    {
        #region private
        private AvaPlot _avaplot;
        private PriceSerie _priceSerie;
        private Grid _avaPlotHost; 
        #endregion

        public StockChart()
        {
            this._avaplot = new AvaPlot();
            this._avaPlotHost = new Grid();
            this._avaPlotHost.Children.Add(this._avaplot);
            this.DrawingObjectsManager.AddPlot(this._avaplot);
        }

        #region Public Fields
        public AvaPlot AvaPlot { get { return this._avaplot; } }

        public Grid AvaPlotHost { get {  return this._avaPlotHost; } } 

        public DrawingObjectsManager DrawingObjectsManager { get; } = new DrawingObjectsManager();

        public PriceSerie PriceSerie { get {  return this._priceSerie; } }

        public bool? UseVolumetric
        {
            get => this._priceSerie?.UseVolumetric;
            set
            {
                this._priceSerie!.UseVolumetric = value!.Value;
                this.AvaPlot.Refresh();
                this.RaisePropertyChanged(nameof(UseVolumetric));
            }
        }
          
        public List<VolumetricType> VolumetricsType { get; set; } = Enum.GetValues(typeof(VolumetricType)).Cast<VolumetricType>().ToList();

        public VolumetricType? SelectedVolumetricType
        {
            get => this._priceSerie?.SelectedVolumetricType;
            set
            {
                if (!value.HasValue)
                    return;
                this._priceSerie!.SelectedVolumetricType = value!.Value;
                this.AvaPlot.Refresh();
                this.RaisePropertyChanged(nameof(UseVolumetric));
            }
        }
        #endregion

        public void FillPrice(OHLC[] bars)
        {
            var ticks = new List<List<TickAnalysis>>();

            foreach(OHLC bar in bars)
            {
                var listOfTrade = bar.Generate(5, 100);
                ticks.Add(listOfTrade);
            }

            OHLCSourceList dataSource = new(bars.ToList());
            this._priceSerie = new PriceSerie(dataSource,ticks);

            this.AvaPlot.Plot.Add.Plottable(this._priceSerie);
            this.AvaPlot.Plot.Axes.DateTimeTicksBottom();


            this.AvaPlot.Plot.PlotControl!.Refresh();
        }
    }


    public static class DataGenerator
    {
        private static Random random = new Random();
         
        public static List<TickAnalysis> Generate(this OHLC candle, double tickSize, int maxTicksInCandle)
        {
            var result = new List<TickAnalysis>();
            var range = (candle.High - candle.Low) / tickSize;
            range = range < 1 ? 1 : range;

            for (int j = 0; j < (int)range; j++)
            {
              //  random = new Random();
                double price = Math.Round(random.NextDouble() * (candle.High - candle.Low) + candle.Low, 2); // Round to 2 decimal places
                long tickVolume = (long)(random.NextDouble() * maxTicksInCandle);
                long delta = (long)(random.NextDouble() * 2 * tickVolume) - tickVolume;
                long buyVolume = random.Next(10, 500 + 1);
                long sellVolume = random.Next(10, 500 + 1);
                long trades = (long)(random.NextDouble() * maxTicksInCandle);
                long buyTrades = (long)(random.NextDouble() * trades);
                long sellTrades = trades - buyTrades;

                TickAnalysis tick = new TickAnalysis
                {
                    Price = price,
                    Volume = buyVolume+sellVolume,
                    Delta = delta,
                    BuyVolume = buyVolume,
                    SellVolume = sellVolume,
                    Trades = trades,
                    BuyTrades = buyTrades,
                    SellTrades = sellTrades,
                    //Time = tickTime
                };

                result.Add(tick);
            }

            return result;
        }
    }


    public class TickAnalysis
    {
        public double Price { get; set; }
        public double Volume { get; set; }
        public long Delta { get; set; }
        public double BuyVolume { get; set; }
        public double SellVolume { get; set; }
        public long Trades { get; set; }
        public long BuyTrades { get; set; }
        public long SellTrades { get; set; }
        public DateTime Time { get; set; }
    }
}
