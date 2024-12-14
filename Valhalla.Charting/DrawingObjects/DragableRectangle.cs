using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using ScottPlot;
using System.Diagnostics;

namespace Valhalla.Charting.DrawingObjects
{
    public class DragableRectangle : Valhalla.TechnicalAnalysis.DrawingObjects.Rectangle
    {
        #region private fields
        private Rectangle _rect;
        private bool _inCreationMode = true;
        private bool _startedToDraw = false;
        private AvaPlot _plot;
        private DragableAnchor _anchorTopRight, _anchorTopLeft, _anchorBottomRight, _anchorBottomLeft;
        #endregion

        #region public fields
        public override DateTime X1
        {
            get
            {
                return NumericConversion.ToDateTime(this._rect.X1);
            }
            set
            {
                this._rect.X1= NumericConversion.ToNumber(value);
            }
        }
        public override DateTime X2
        {
            get
            {
                return NumericConversion.ToDateTime(this._rect.X2);
            }
            set
            {
                this._rect.X2 = NumericConversion.ToNumber(value);
            }
        }
        public override double Y1
        {
            get
            {
                return this._rect.Y1;
            }
            set
            {
                this._rect.Y1 = value;
            }
        }

        public override double Y2
        {
            get
            {
                return this._rect.Y2;
            }
            set
            {
                this._rect.Y2= value;
            }
        }

        public override string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool IsVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool IsDragable { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        #endregion
        public DragableRectangle(AvaPlot plot, double x1, double x2, double y1, double y2)
        {
            this._plot = plot;
            /*this.X1 = x1;
            this.X2 = x2;
            this.Y1 = y1;
            this.Y2 = y2;*/

            this._rect = this._plot.Plot.Add.Rectangle(x1, x2, y1, y2);
            this._plot.PointerMoved += this._plot_PointerMoved;

            this._anchorTopRight = new DragableAnchor(plot, x1, y1, this._rect.LineColor);
            this._anchorBottomRight = new DragableAnchor(plot, x1, y1, this._rect.LineColor);
            this._anchorTopLeft = new DragableAnchor(plot, x1, y1, this._rect.LineColor);
            this._anchorBottomLeft = new DragableAnchor(plot, x1, y1, this._rect.LineColor);

            this._anchorBottomRight.OnMoved += this._anchorBottomRight_OnMoved;
            this._anchorBottomLeft.OnMoved += this._anchorBottomLeft_OnMoved1;
            this._anchorTopRight.OnMoved += this._anchorTopRight_OnMoved;
            this._anchorTopLeft.OnMoved += this._anchorTopLeft_OnMoved;
            this._inCreationMode = true;
           
        }

        private void _anchorTopLeft_OnMoved(DragableAnchor sender, double X, double Y)
        {
            this._rect.X2 = X;
            this._rect.Y2 = Y;

            this._anchorTopRight.Y = Y;
            this._anchorBottomLeft.X = NumericConversion.ToDateTime(X);

            Debug.WriteLine(NumericConversion.ToDateTime(this._rect.X2));
        }

        private void _anchorTopRight_OnMoved(DragableAnchor sender, double X, double Y)
        {
            this._rect.X1 = X;
            this._rect.Y2 = Y;

            this._anchorTopLeft.Y = Y;
            this._anchorBottomRight.X = NumericConversion.ToDateTime(X);
        }

        private void _anchorBottomLeft_OnMoved1(DragableAnchor sender, double X, double Y)
        {
            this._rect.X2 = X;
            this._rect.Y1 = Y;

            this._anchorTopLeft.X = NumericConversion.ToDateTime(X);
            this._anchorBottomRight.Y = Y;

            this._plot.Refresh();
        }

        private void _anchorBottomRight_OnMoved(DragableAnchor sender, double X, double Y)
        {
            this._rect.X1 = X;
            this._rect.Y1 = Y;

            this._anchorTopRight.X = NumericConversion.ToDateTime(X);
            this._anchorBottomLeft.Y = Y;
            this._plot.Refresh();
        }

        private void _plot_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            this._inCreationMode = false;
            this._plot.PointerMoved -= this._plot_PointerMoved;

            this._anchorBottomRight.X = NumericConversion.ToDateTime(this._rect.X1);
            this._anchorBottomRight.Y = this._rect.Y1;

            this._anchorTopRight.X = NumericConversion.ToDateTime(this._rect.X1);
            this._anchorTopRight.Y = this._rect.Y2;

            this._anchorBottomLeft.X = NumericConversion.ToDateTime(this._rect.X2);
            this._anchorBottomLeft.Y = this._rect.Y1;

            this._plot.PointerReleased -= this._plot_PointerReleased;
        }

        private void _plot_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (!this._inCreationMode)
                return;

            var points = e.GetPosition(_plot);

            Pixel mousePixel = new(points.X, points.Y);
            Coordinates mouseLocation = this._plot.Plot.GetCoordinates(mousePixel); 

            this._rect.X1 = mouseLocation.X;
            this._rect.Y1 = mouseLocation.Y;


            this._plot.Refresh();

            if (!this._startedToDraw)
            {
                this._plot.PointerReleased += this._plot_PointerReleased;
                this._startedToDraw = true;
            }
        }

        public override void Refresh()
        {
            this._plot.Refresh();
        }
    }
}
