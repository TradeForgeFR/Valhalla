using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using ScottPlot;
using System.Diagnostics;

namespace Valhalla.Charting.DrawingObjects
{
    public class DraggableTrendLine : Valhalla.TechnicalAnalysis.DrawingObjects.Rectangle
    {
        #region private fields
        private LinePlot _line;
        private bool _inCreationMode = true;
        private bool _startedToDraw = false;
        private AvaPlot _plot;
        private DraggableAnchor _anchorLeft, _anchorRight;
        #endregion

        #region public fields
        public override DateTime X1
        {
            get
            {
                return NumericConversion.ToDateTime(this._line.Start.X);
            }
            set
            {
                this._line.Start = new(NumericConversion.ToNumber(value), this.Y1);
                this.Refresh();
            }
        }
        public override DateTime X2
        {
            get
            {
                return NumericConversion.ToDateTime(this._line.End.X);
            }
            set
            {
                this._line.End = new(NumericConversion.ToNumber(value), this.Y2);
                this.Refresh();
            }
        }
        public override double Y1
        {
            get
            {
                return this._line.Start.Y;
            }
            set 
            {
                this._line.Start = new(NumericConversion.ToNumber(this.X1), value);
                this.Refresh();
            }
        }

        public override double Y2
        {
            get
            {
                return this._line.End.Y;
            }
            set
            {
                this._line.End = new(NumericConversion.ToNumber(this.X2), value);
                this.Refresh();
            }
        }

        public override string Name { get; set; }
        public override bool IsVisible
        {
            get
            {
                return this._line.IsVisible;
            }
            set
            {
                this._line.IsVisible = value;
                if (value)
                {
                    this._anchorRight.IsVisible = this.IsDraggable;
                    this._anchorLeft.IsVisible = this.IsDraggable;
                }
                else
                {
                    this._anchorRight.IsVisible = false;
                    this._anchorLeft.IsVisible = false;
                }
                this.Refresh();
            }
        }
        public override bool IsDraggable
        {
            get
            {
                return this._anchorRight.IsVisible;
            }
            set
            {
                this._anchorRight.IsVisible = value;
                this._anchorLeft.IsVisible = value; 
                this._anchorRight.IsDraggable = value;
                this._anchorLeft.IsDraggable = value; 
                this.Refresh();
            }
        }
        #endregion
        public DraggableTrendLine(AvaPlot plot, double x1, double x2, double y1, double y2)
        {
            this._plot = plot;

            this._line = this._plot.Plot.Add.Line(x1, y1, x2, y2);
            this._plot.PointerMoved += this._plot_PointerMoved;

            this._anchorLeft = new DraggableAnchor(plot, x1, y1, this._line.LineColor); 
            this._anchorRight = new DraggableAnchor(plot, x2, y2, this._line.LineColor); 

            this._anchorLeft.OnMoved += this._anchorRight_OnMoved;
            this._anchorRight.OnMoved += this._anchorLeft_OnMoved;
            this._inCreationMode = true;

        }

        private void _anchorLeft_OnMoved(DraggableAnchor sender, double X, double Y)
        {
            this._line.Start = new(NumericConversion.ToNumber(this._anchorRight.X), this._anchorRight.Y);
            this._line.End = new(NumericConversion.ToNumber(this._anchorLeft.X), this._anchorLeft.Y);
            this.Refresh();
        }

        private void _anchorRight_OnMoved(DraggableAnchor sender, double X, double Y)
        {
            this._line.Start = new(NumericConversion.ToNumber(this._anchorRight.X), this._anchorRight.Y);
            this._line.End = new(NumericConversion.ToNumber(this._anchorLeft.X), this._anchorLeft.Y);
            this.Refresh();
        }

        private void _plot_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            this._inCreationMode = false;
            this._plot.PointerMoved -= this._plot_PointerMoved;
            this._plot.PointerReleased -= this._plot_PointerReleased;

            this._anchorRight.X = this.X2;
            this._anchorRight.Y = this.Y2;
        }

        private void _plot_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (!this._inCreationMode)
                return;

            var points = e.GetPosition(_plot);

            Pixel mousePixel = new(points.X, points.Y);
            Coordinates mouseLocation = this._plot.Plot.GetCoordinates(mousePixel);

            this._line.End = new(mouseLocation.X, mouseLocation.Y);

            this.Refresh();

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
