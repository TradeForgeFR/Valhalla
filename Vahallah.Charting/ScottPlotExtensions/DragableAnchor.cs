using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using ScottPlot;

namespace Vahallah.Charting.ScottPlotExtensions
{
    public delegate void AnchorMovedHandler(DragableAnchor sender, double X, double Y);
    public class DragableAnchor
    {
        private AvaPlot _plot;
        public Scatter Scatter;
        int? IndexBeingDragged = null;

        private double[] Xs;
        private double[] Ys;

        public event AnchorMovedHandler OnMoved;
        public DragableAnchor(AvaPlot plot, double x, double y, ScottPlot.Color color)
        {
            Xs = new[] { x };
            Ys = new[] { y };
            Scatter = plot.Plot.Add.Scatter(Xs, Ys);
            Scatter.LineWidth = 2;
            Scatter.MarkerSize = 5;
            Scatter.Smooth = true;
            Scatter.Color = color;

            plot.PointerPressed += _plot_PointerPressed;
            plot.PointerMoved += _plot_PointerMoved;
            plot.PointerReleased += Plot_PointerReleased;
            _plot = plot;
        }

        private void Plot_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            IndexBeingDragged = null;
            _plot.UserInputProcessor.Enable();
            _plot.Refresh();
        }

        private void _plot_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            var points = e.GetPosition(_plot);

            Pixel mousePixel = new(points.X, points.Y);
            Coordinates mouseLocation = _plot.Plot.GetCoordinates(mousePixel);
            DataPoint nearest = Scatter.Data.GetNearest(mouseLocation, _plot.Plot.LastRender);

            if (IndexBeingDragged.HasValue)
            {
                Xs[IndexBeingDragged.Value] = mouseLocation.X;
                Ys[IndexBeingDragged.Value] = mouseLocation.Y;

                OnMoved?.Invoke(this, mouseLocation.X, mouseLocation.Y);
                _plot.Refresh();
            }
        }
        private void _plot_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
        {
            var points = e.GetPosition(_plot);

            Pixel mousePixel = new(points.X, points.Y);
            Coordinates mouseLocation = _plot.Plot.GetCoordinates(mousePixel);
            DataPoint nearest = Scatter.Data.GetNearest(mouseLocation, _plot.Plot.LastRender);
            IndexBeingDragged = nearest.IsReal ? nearest.Index : null;

            if (IndexBeingDragged.HasValue)
                _plot.UserInputProcessor.Disable();
        }

        public double X
        {
            get
            {
                return Xs[0];
            }
            set
            {
                Xs[0] = value;
                _plot.Refresh();
            }
        }

        public double Y
        {
            get
            {
                return Ys[0];
            }
            set
            {
                Ys[0] = value;
                _plot.Refresh();
            }
        }
    }
}
