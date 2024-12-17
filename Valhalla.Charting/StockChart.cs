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
        #endregion

        public void FillPrice(OHLC[] bars)
        {
            OHLCSourceList dataSource = new(bars.ToList());
            this._priceSerie = new PriceSerie(dataSource);
            this.AvaPlot.Plot.Add.Plottable(this._priceSerie);
            this.AvaPlot.Plot.Axes.DateTimeTicksBottom();
            this.AvaPlot.Plot.PlotControl!.Refresh();
        }
    }
}
