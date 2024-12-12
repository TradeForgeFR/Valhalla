using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ScottPlot;
using ScottPlot.Avalonia;
using Vahallah.Charting.ScottPlotExtensions;

namespace Vahallah.Charting
{
    public class StockChart : ReactiveObject
    {
        #region private
        private AvaPlot _avaplot;
        private Grid _avaPlotHost;
        private bool _drawingRectangleMode = false;
        #endregion

        public StockChart()
        {
            this._avaplot = new AvaPlot();
            this._avaPlotHost = new Grid();
            this._avaPlotHost.Children.Add(this._avaplot);

            this.AvaPlot.PointerPressed += Plot_PointerPressed;

            StartDrawingRectCommand = new RelayCommand(() =>
            {
                _drawingRectangleMode = true;
            });
        }

        #region Public Fields
        public AvaPlot AvaPlot { get { return this._avaplot; } }

        public Grid AvaPlotHost { get {  return this._avaPlotHost; } }

        public RelayCommand? StartDrawingRectCommand { get; set; }
        #endregion

        private void Plot_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var plot = sender as AvaPlot;
            var points = e.GetPosition(plot);

            Pixel mousePixel = new(points.X, points.Y);
            Coordinates mouseLocation = plot!.Plot.GetCoordinates(mousePixel);

            if (_drawingRectangleMode)
            {
                plot!.StartDrawingDragableRectangle(mouseLocation.X, mouseLocation.Y);

                _drawingRectangleMode = false;
            }
        }


    }
}
