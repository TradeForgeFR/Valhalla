using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using ScottPlot;
using Valhalla.TechnicalAnalysis.Interfaces;

namespace Valhalla.Charting.DrawingObjects
{
    public delegate void AnchorMovedHandler(DragableAnchor sender, double X, double Y);
    public class DragableAnchor : IDrawingObject, ISingleCoordinateDrawingObject
    {
        #region private fields
        private AvaPlot _plot;
        private int? _indexBeingDragged;
        private double[] _xs;
        private double[] _ys;
        #endregion

        #region public fields
        public Scatter? Scatter;
        public event AnchorMovedHandler? OnMoved;
        #endregion

        public DragableAnchor(AvaPlot plot, double x, double y, ScottPlot.Color color)
        {
            this._xs = [x];
            this._ys = [y];
            this.Scatter = plot.Plot.Add.Scatter(_xs, _ys);
            this.Scatter.LineWidth = 2;
            this.Scatter.MarkerSize = 5;
            this.Scatter.Smooth = true;
            this.Scatter.Color = color;

            this._plot = plot;
            this._plot.PointerPressed += _plot_PointerPressed;
            this._plot.PointerMoved += _plot_PointerMoved;
            this._plot.PointerReleased += Plot_PointerReleased;            
        }

        private void Plot_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            this._indexBeingDragged = null;
            this._plot.UserInputProcessor.Enable();
            this._plot.Refresh();
        }

        private void _plot_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            var points = e.GetPosition(_plot);

            Pixel mousePixel = new(points.X, points.Y);
            Coordinates mouseLocation = this._plot.Plot.GetCoordinates(mousePixel);
            DataPoint nearest = Scatter.Data.GetNearest(mouseLocation, this._plot.Plot.LastRender);

            if (_indexBeingDragged.HasValue)
            {
                this._xs[this._indexBeingDragged.Value] = mouseLocation.X;
                this._ys[this._indexBeingDragged.Value] = mouseLocation.Y;

                this.OnMoved?.Invoke(this, mouseLocation.X, mouseLocation.Y);
                this._plot.Refresh();
            }
        }
        private void _plot_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var points = e.GetPosition(this._plot);

            Pixel mousePixel = new(points.X, points.Y);
            Coordinates mouseLocation = this._plot.Plot.GetCoordinates(mousePixel);
            DataPoint nearest = Scatter.Data.GetNearest(mouseLocation, this._plot.Plot.LastRender);
            _indexBeingDragged = nearest.IsReal ? nearest.Index : null;

            if (this._indexBeingDragged.HasValue)
                this._plot.UserInputProcessor.Disable();
        }

        public void Refresh()
        {
            this._plot.Refresh();
        }

        public DateTime X
        {
            get
            {
                return NumericConversion.ToDateTime(this._xs[0]);
            }
            set
            {
                this._xs[0] = NumericConversion.ToNumber(value); 
                this._plot.Refresh();
            }
        }

        public double Y
        {
            get
            {
                return this._ys[0];
            }
            set
            {
                this._ys[0] = value;
                this._plot.Refresh();
            }
        }

        public string Name { get; set; }
        public bool IsVisible
        {
            get
            {
                return this.Scatter!.IsVisible;
            }
            set
            {
                this.Scatter!.IsVisible = value;
            }
        }
        public bool IsDragable { get; set; } = false;
    }
}
