using Avalonia.Controls;
using ReactiveUI;
using ScottPlot.Avalonia;
using Valhalla.Charting.Managers;

namespace Valhalla.Charting
{
    public class StockChart : ReactiveObject
    {
        #region private
        private AvaPlot _avaplot;
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
        #endregion
    }
}
