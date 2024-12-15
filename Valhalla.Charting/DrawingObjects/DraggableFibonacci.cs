using ScottPlot.Avalonia;
using ScottPlot.Plottables;
using ScottPlot;
using Valhalla.Charting.Interfaces;

namespace Valhalla.Charting.DrawingObjects
{
    public class DraggableFibonacci : Valhalla.TechnicalAnalysis.DrawingObjects.Fibonacci, IPlottableContainer
    {
        #region private fields
        private LinePlot _mainDraggableLine, _topLine, _middleLine, _bottomLine, _line76, _line61, _line38, _line23;
        private Text _topText, _middleText, _bottomText, _text76, _text61, _text38, _text23;
        private bool _inCreationMode = false;
        private bool _startedToDraw = false;
        private AvaPlot _plot;
        private DraggableAnchor _anchorLeft, _anchorRight;
        #endregion

        #region public fields
        public override DateTime X1
        {
            get
            {
                return NumericConversion.ToDateTime(this._mainDraggableLine.Start.X);
            }
            set
            {
                this._mainDraggableLine.Start = new(NumericConversion.ToNumber(value), this.Y1);
                this.Refresh();
            }
        }
        public override DateTime X2
        {
            get
            {
                return NumericConversion.ToDateTime(this._mainDraggableLine.End.X);
            }
            set
            {
                this._mainDraggableLine.End = new(NumericConversion.ToNumber(value), this.Y2);
                this.Refresh();
            }
        }
        public override double Y1
        {
            get
            {
                return this._mainDraggableLine.Start.Y;
            }
            set
            {
                this._mainDraggableLine.Start = new(NumericConversion.ToNumber(this.X1), value);
                this.Refresh();
            }
        }

        public override double Y2
        {
            get
            {
                return this._mainDraggableLine.End.Y;
            }
            set
            {
                this._mainDraggableLine.End = new(NumericConversion.ToNumber(this.X2), value);
                this.Refresh();
            }
        }

        public override string Name { get; set; }
        public override bool IsVisible
        {
            get
            {
                return this._mainDraggableLine.IsVisible;
            }
            set
            {
                this._mainDraggableLine.IsVisible = value;
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
        public DraggableFibonacci(AvaPlot plot, double x1, double x2, double y1, double y2, bool isManualDrawingMode = true)
        {
            this._plot = plot;

            this.CreatePottables(plot, x1, x2, y1, y2);

            if (isManualDrawingMode)
            {
                this._inCreationMode = isManualDrawingMode;
                this._plot.PointerMoved += this._plot_PointerMoved;
            }
        }

        private void CreatePottables(AvaPlot plot, double x1, double x2, double y1, double y2)
        {
            this._mainDraggableLine = this._plot.Plot.Add.Line(x1, y1, x2, y2);
            this._mainDraggableLine.LineWidth = 1;
            this._bottomLine = this._plot.Plot.Add.Line(x1, y1, x2, y2);
            this._bottomLine.LineWidth = 2;
            this._topLine = this._plot.Plot.Add.Line(x1, y1, x2, y2);
            this._topLine.LineWidth = 2;
            this._middleLine = this._plot.Plot.Add.Line(x1, y1, x2, y2);
            this._middleLine.LineWidth = 2;
            this._line76 = this._plot.Plot.Add.Line(x1, y1, x2, y2);
            this._line76.LineWidth = 2;
            this._line61 = this._plot.Plot.Add.Line(x1, y1, x2, y2);
            this._line61.LineWidth = 2;
            this._line38 = this._plot.Plot.Add.Line(x1, y1, x2, y2);
            this._line38.LineWidth = 2;
            this._line23 = this._plot.Plot.Add.Line(x1, y1, x2, y2);
            this._line23.LineWidth = 2;

            this._topText = this._plot.Plot.Add.Text("TopText", new(x1, y1));
            this._topText.LabelFontColor = this._topLine.Color;
            this._topText.LabelFontSize = 12;
            this._topText.LabelOffsetY = 3;
            this._topText.LabelBold = true;

            this._middleText = this._plot.Plot.Add.Text("MiddleText", new(x1, y1));
            this._middleText.LabelFontColor = this._middleLine.Color;
            this._middleText.LabelFontSize = 12;
            this._middleText.LabelOffsetY = 3;
            this._middleText.LabelBold = true;

            this._bottomText = this._plot.Plot.Add.Text("BottomText", new(x1, y1));
            this._bottomText.LabelFontColor = this._bottomLine.Color;
            this._bottomText.LabelFontSize = 12;
            this._bottomText.LabelOffsetY = 3;
            this._bottomText.LabelBold = true;

            this._text76 = this._plot.Plot.Add.Text("Text76", new(x1, y1));
            this._text76.LabelFontColor = this._line76.Color;
            this._text76.LabelFontSize = 12;
            this._text76.LabelOffsetY = 3;
            this._text76.LabelBold = true;

            this._text61 = this._plot.Plot.Add.Text("Text61", new(x1, y1));
            this._text61.LabelFontColor = this._line61.Color;
            this._text61.LabelFontSize = 12;
            this._text61.LabelOffsetY = 3;
            this._text61.LabelBold = true;

            this._text38 = this._plot.Plot.Add.Text("Text38", new(x1, y1));
            this._text38.LabelFontColor = this._line38.Color;
            this._text38.LabelFontSize = 12;
            this._text38.LabelOffsetY = 3;
            this._text38.LabelBold = true;

            this._text23 = this._plot.Plot.Add.Text("Text23", new(x1, y1));
            this._text23.LabelFontColor = this._line23.Color;
            this._text23.LabelFontSize = 12;
            this._text23.LabelOffsetY = 3;
            this._text23.LabelBold = true;

            this._anchorLeft = new DraggableAnchor(plot, x1, y1, this._mainDraggableLine.LineColor);
            this._anchorRight = new DraggableAnchor(plot, x2, y2, this._mainDraggableLine.LineColor);

            this._anchorLeft.OnMoved += this._anchorRight_OnMoved;
            this._anchorRight.OnMoved += this._anchorLeft_OnMoved;
        }
        private void _anchorLeft_OnMoved(DraggableAnchor sender, double X, double Y)
        {
            this._mainDraggableLine.Start = new(NumericConversion.ToNumber(this._anchorRight.X), this._anchorRight.Y);
            this._mainDraggableLine.End = new(NumericConversion.ToNumber(this._anchorLeft.X), this._anchorLeft.Y);
            this.Refresh();
        }

        private void _anchorRight_OnMoved(DraggableAnchor sender, double X, double Y)
        {
            this._mainDraggableLine.Start = new(NumericConversion.ToNumber(this._anchorRight.X), this._anchorRight.Y);
            this._mainDraggableLine.End = new(NumericConversion.ToNumber(this._anchorLeft.X), this._anchorLeft.Y);
            this.Refresh();
        }

        private void _plot_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            this._inCreationMode = false;
            this._plot.PointerMoved -= this._plot_PointerMoved;
            this._plot.PointerReleased -= this._plot_PointerReleased;

            this._anchorRight.X = this.X2;
            this._anchorRight.Y = this.Y2;

            this._plot.UserInputProcessor.Enable();

            this.Refresh();
        }

        private void _plot_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (!this._inCreationMode)
                return;

            var points = e.GetPosition(_plot);

            Pixel mousePixel = new(points.X, points.Y);
            Coordinates mouseLocation = this._plot.Plot.GetCoordinates(mousePixel);

            this._mainDraggableLine.End = new(mouseLocation.X, mouseLocation.Y);

            this.Refresh();

            if (!this._startedToDraw)
            {
                this._plot.PointerReleased += this._plot_PointerReleased;
                this._startedToDraw = true;
            }
        }

        public override void Refresh()
        {
            var min = Math.Min(this._anchorRight.Y, this._anchorLeft.Y);
            var max = Math.Max(this._anchorRight.Y, this._anchorLeft.Y);
            var middle = (min+max) / 2;
            var level76 = min + ((max - min) / 100) * 76.4;
            var level61 = min + ((max - min) / 100) * 61.8;
            var level38 = min + ((max - min) / 100) * 38.2;
            var level23 = min + ((max - min) / 100) * 23.6;

            this._bottomLine.Start = new(NumericConversion.ToNumber(this._anchorLeft.X), min);
            this._bottomLine.End = new(NumericConversion.ToNumber(this._anchorRight.X), min);

            this._topLine.Start = new(NumericConversion.ToNumber(this._anchorLeft.X), max);
            this._topLine.End = new(NumericConversion.ToNumber(this._anchorRight.X), max);

            this._middleLine.Start = new(NumericConversion.ToNumber(this._anchorLeft.X), middle);
            this._middleLine.End = new(NumericConversion.ToNumber(this._anchorRight.X), middle);

            this._line76.Start = new(NumericConversion.ToNumber(this._anchorLeft.X), level76);
            this._line76.End = new(NumericConversion.ToNumber(this._anchorRight.X), level76);

            this._line61.Start = new(NumericConversion.ToNumber(this._anchorLeft.X), level61);
            this._line61.End = new(NumericConversion.ToNumber(this._anchorRight.X), level61);

            this._line38.Start = new(NumericConversion.ToNumber(this._anchorLeft.X), level38);
            this._line38.End = new(NumericConversion.ToNumber(this._anchorRight.X), level38);

            this._line23.Start = new(NumericConversion.ToNumber(this._anchorLeft.X), level23);
            this._line23.End = new(NumericConversion.ToNumber(this._anchorRight.X), level23);

            this._topText.Location = new(NumericConversion.ToNumber(this._anchorLeft.X), max);
            this._topText.LabelText = $"100,00 % ({Math.Round(max,3)})";

            this._middleText.Location = new(NumericConversion.ToNumber(this._anchorLeft.X), middle);
            this._middleText.LabelText = $"50,00 % ({Math.Round(middle, 3)})";

            this._bottomText.Location = new(NumericConversion.ToNumber(this._anchorLeft.X), min);
            this._bottomText.LabelText = $"0,00 % ({Math.Round(min, 3)})";

            this._text76.Location = new(NumericConversion.ToNumber(this._anchorLeft.X), level76);
            this._text76.LabelText = $"76,40 % ({Math.Round(level76, 3)})";

            this._text61.Location = new(NumericConversion.ToNumber(this._anchorLeft.X), level61);
            this._text61.LabelText = $"61,80 % ({Math.Round(level61, 3)})";

            this._text38.Location = new(NumericConversion.ToNumber(this._anchorLeft.X), level38);
            this._text38.LabelText = $"38,20 % ({Math.Round(level38, 3)})";

            this._text23.Location = new(NumericConversion.ToNumber(this._anchorLeft.X), level23);
            this._text23.LabelText = $"23,60 % ({Math.Round(level23, 3)})";

            this._plot.Refresh();
        }

        public void RemovePlottables()
        {
            this._plot.Plot.Remove(this._mainDraggableLine);
            this._plot.Plot.Remove(this._anchorLeft.Scatter!);
            this._plot.Plot.Remove(this._anchorRight.Scatter!);
            this._plot.Plot.Remove(this._bottomLine);
            this._plot.Plot.Remove(this._middleLine);
            this._plot.Plot.Remove(this._topLine);
            this._plot.Plot.Remove(this._topText);
            this._plot.Plot.Remove(this._middleText);
            this._plot.Plot.Remove(this._bottomText);
            this._plot.Plot.Remove(this._text76);
            this._plot.Plot.Remove(this._line76);
            this._plot.Plot.Remove(this._text61);
            this._plot.Plot.Remove(this._line61);
            this._plot.Plot.Remove(this._text38);
            this._plot.Plot.Remove(this._line38);
            this._plot.Plot.Remove(this._line23);
            this._plot.Plot.Remove(this._text23);

            this.Refresh();
        }
    }
}
