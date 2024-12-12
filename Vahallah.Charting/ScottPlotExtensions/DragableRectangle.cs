using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using ScottPlot;

namespace Valhalla.Charting.ScottPlotExtensions
{
    public class DragableRectangle
    {
        private Rectangle _rect;
        private bool _inCreationMode = true;
        private bool _startedToDraw = false;
        private AvaPlot _plot;
        private DragableAnchor _anchorTopRight, _anchorTopLeft, _anchorBottomRight, _anchorBottomLeft;
        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }

        public DragableRectangle(AvaPlot plot, double x1, double x2, double y1, double y2)
        {
            this._plot = plot;
            this.X1 = x1;
            this.X2 = x2;
            this.Y1 = y1;
            this.Y2 = y2;

            _rect = _plot.Plot.Add.Rectangle(x1, x2, y1, y2);
            _plot.PointerMoved += this._plot_PointerMoved;

            _anchorTopRight = new DragableAnchor(plot, x1, y1, _rect.LineColor);
            _anchorBottomRight = new DragableAnchor(plot, x1, y1, _rect.LineColor);
            _anchorTopLeft = new DragableAnchor(plot, x1, y1, _rect.LineColor);
            _anchorBottomLeft = new DragableAnchor(plot, x1, y1, _rect.LineColor);

            _anchorBottomRight.OnMoved += this._anchorBottomRight_OnMoved;
            _anchorBottomLeft.OnMoved += this._anchorBottomLeft_OnMoved1;
            _anchorTopRight.OnMoved += this._anchorTopRight_OnMoved;
            _anchorTopLeft.OnMoved += this._anchorTopLeft_OnMoved;
            _inCreationMode = true;
        }

        private void _anchorTopLeft_OnMoved(DragableAnchor sender, double X, double Y)
        {
            _rect.X2 = X;
            _rect.Y2 = Y;

            _anchorTopRight.Y = Y;
            _anchorBottomLeft.X = X;
        }

        private void _anchorTopRight_OnMoved(DragableAnchor sender, double X, double Y)
        {
            _rect.X1 = X;
            _rect.Y2 = Y;

            _anchorTopLeft.Y = Y;
            _anchorBottomRight.X = X;
        }

        private void _anchorBottomLeft_OnMoved1(DragableAnchor sender, double X, double Y)
        {
            _rect.X2 = X;
            _rect.Y1 = Y;

            _anchorTopLeft.X = X;
            _anchorBottomRight.Y = Y;

            _plot.Refresh();
        }

        private void _anchorBottomRight_OnMoved(DragableAnchor sender, double X, double Y)
        {
            _rect.X1 = X;
            _rect.Y1 = Y;

            _anchorTopRight.X = X;
            _anchorBottomLeft.Y = Y;
            _plot.Refresh();
        }

        private void _plot_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            _inCreationMode = false;
            _plot.PointerMoved -= this._plot_PointerMoved;

            _anchorBottomRight.X = _rect.X1;
            _anchorBottomRight.Y = _rect.Y1;

            _anchorTopRight.X = _rect.X1;
            _anchorTopRight.Y = _rect.Y2;

            _anchorBottomLeft.X = _rect.X2;
            _anchorBottomLeft.Y = _rect.Y1;

            _plot.PointerReleased -= this._plot_PointerReleased;
        }

        private void _plot_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (!_inCreationMode)
                return;

            var points = e.GetPosition(_plot);

            Pixel mousePixel = new(points.X, points.Y);
            Coordinates mouseLocation = _plot.Plot.GetCoordinates(mousePixel);
            //   _plot.Cursor = nearest.IsReal ? Cursors.Hand : Cursors.Arrow;

            _rect.X1 = mouseLocation.X;
            _rect.Y1 = mouseLocation.Y;


            _plot.Refresh();

            if (!_startedToDraw)
            {
                _plot.PointerReleased += this._plot_PointerReleased;
                _startedToDraw = true;
            }
        }
    }
}
