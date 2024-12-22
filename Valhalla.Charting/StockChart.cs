using Avalonia.Controls;
using ReactiveUI;
using ScottPlot.Avalonia;
using Valhalla.Charting.CustomSeries;
using Valhalla.Charting.Managers;
using Valhalla.TechnicalAnalysis.Models;

namespace Valhalla.Charting
{
    public class StockChart : ReactiveObject
    {
        #region private
        private AvaPlot _avaplot;
        private AdvancedStockPlottable _priceSerie = new AdvancedStockPlottable();
        private Grid _avaPlotHost; 
        #endregion

        public StockChart()
        {
            this._avaplot = new AvaPlot();
            this._avaPlotHost = new Grid();
            this._avaPlotHost.Children.Add(this._avaplot);
            this.DrawingObjectsManager.AddPlot(this._avaplot);
            this.AvaPlot.Plot.Add.Plottable(this._priceSerie);
            this.AvaPlot.Plot.Axes.DateTimeTicksBottom();
        }

        #region Public Fields
        public AvaPlot AvaPlot { get { return this._avaplot; } }

        public Grid AvaPlotHost { get {  return this._avaPlotHost; } } 

        public DrawingObjectsManager DrawingObjectsManager { get; } = new DrawingObjectsManager();

        public AdvancedStockPlottable PricesPlot { get {  return this._priceSerie; } }

   
        #endregion

        public void FillPrice(List<OHLCDatas> bars)
        {
            foreach(var bar in bars)
            {
                bar.Ticks = bar.Generate(5, 100);
            }

            this._priceSerie.Datas = bars;
           
            this.AvaPlot.Plot.Axes.AutoScale();
        }
    }


    public static class DataGenerator
    {
        private static Random random = new Random();
         
        public static List<TickDatas> Generate(this OHLCDatas candle, double tickSize, int maxTicksInCandle)
        {
            var result = new List<TickDatas>();
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

                TickDatas tick = new TickDatas
                {
                    Price = price, 
                    Delta = delta,
                    BuyVolume = buyVolume,
                    SellVolume = sellVolume, 
                    BuyTrades = buyTrades,
                    SellTrades = sellTrades,
                    //Time = tickTime
                };

                result.Add(tick);
            }

            return result;
        }
    }
}
