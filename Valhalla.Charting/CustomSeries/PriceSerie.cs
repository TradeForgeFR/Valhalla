using ScottPlot;
using ScottPlot.AxisPanels;
using ScottPlot.TickGenerators;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Valhalla.Charting.CustomSeries
{
    public class PriceSerie(IOHLCSource data) : IPlottable
    {
        public bool IsVisible { get; set; } = true;

        public IAxes Axes { get; set; } = new Axes();

        public IOHLCSource Data { get; } = data;

        /// <summary>
        /// X position of candles is sourced from the OHLC's DateTime by default.
        /// If this option is enabled, X position will be an ascending integers starting at 0 with no gaps.
        /// </summary>
        public bool Sequential { get; set; } = false;

        /// <summary>
        /// Fractional width of the candle symbol relative to its time span
        /// </summary>
        public double SymbolWidth = .05;

        public LineStyle RisingLineStyle { get; } = new()
        {
            Color = Color.FromHex("#089981"),
            Width = 2,
        };

        public LineStyle FallingLineStyle { get; } = new()
        {
            Color = Color.FromHex("#f23645"),
            Width = 2,
        };

        public FillStyle RisingFillStyle { get; } = new()
        {
            Color = Color.FromHex("#089981"),
        };

        public FillStyle FallingFillStyle { get; } = new()
        {
            Color = Color.FromHex("#f23645"),
        };

        public Color RisingColor
        {
            set
            {
                RisingLineStyle.Color = value;
                RisingFillStyle.Color = value;
            }
        }

        public Color FallingColor
        {
            set
            {
                FallingLineStyle.Color = value;
                FallingFillStyle.Color = value;
            }
        }

        public IEnumerable<LegendItem> LegendItems => Enumerable.Empty<LegendItem>();

        public AxisLimits GetAxisLimits()
        {
            AxisLimits limits = Data.GetLimits(); // TODO: Data.GetSequentialLimits()

            if (Sequential)
            {
                return new AxisLimits(
                    left: -0.5, // extra to account for body size
                    right: Data.GetOHLCs().Count - 1 + 0.5, // extra to account for body size
                    bottom: limits.Bottom,
                    top: limits.Top);
            }

            var ohlcs = Data.GetOHLCs();
            if (ohlcs.Count == 0)
                return limits;

            double left = ohlcs.First().DateTime.ToOADate() - ohlcs.First().TimeSpan.TotalDays / 2;
            double right = ohlcs.Last().DateTime.ToOADate() + ohlcs.Last().TimeSpan.TotalDays / 2;

            return new(left, right, limits.Bottom, limits.Top);
        }

        public CoordinateRange GetPriceRangeInView()
        {
            var ohlcs = Data.GetOHLCs();
            if (ohlcs.Count == 0)
                return CoordinateRange.NoLimits;

            int minIndexInView = (int)NumericConversion.Clamp(Axes.XAxis.Min, 0, ohlcs.Count - 1);
            int maxIndexInView = (int)NumericConversion.Clamp(Axes.XAxis.Max, 0, ohlcs.Count - 1);
            return Data.GetPriceRange(minIndexInView, maxIndexInView);
        }

        public (int index, OHLC ohlc)? GetOhlcNearX(double x)
        {
            int ohlcIndex = (int)Math.Round(x);
            return (ohlcIndex >= 0 && ohlcIndex < Data.Count)
                ? (ohlcIndex, Data.GetOHLCs()[ohlcIndex])
                : null;
        }

        public virtual void Render(RenderPack rp)
        {
            using SKPaint paint = new();

            var ohlcs = Data.GetOHLCs();
            for (int i = 0; i < ohlcs.Count; i++)
            {
                OHLC ohlc = ohlcs[i];
                bool isRising = ohlc.Close >= ohlc.Open;
                LineStyle lineStyle = isRising ? RisingLineStyle : FallingLineStyle;
                FillStyle fillStyle = isRising ? RisingFillStyle : FallingFillStyle;

                float top = Axes.GetPixelY(ohlc.High);
                float bottom = Axes.GetPixelY(ohlc.Low);

                float center, xPxLeft, xPxRight;
                if (Sequential == false)
                {
                    double centerNumber = NumericConversion.ToNumber(ohlc.DateTime);
                    center = Axes.GetPixelX(centerNumber);
                    double halfWidthNumber = ohlc.TimeSpan.TotalDays / 2 * SymbolWidth;
                    xPxLeft = Axes.GetPixelX(centerNumber - halfWidthNumber);
                    xPxRight = Axes.GetPixelX(centerNumber + halfWidthNumber);
                }
                else
                {
                    center = Axes.GetPixelX(i);
                    xPxLeft = Axes.GetPixelX(i - (float)SymbolWidth / 2);
                    xPxRight = Axes.GetPixelX(i + (float)SymbolWidth / 2);
                }

                // do not render OHLCs off the screen
                if (xPxRight < rp.DataRect.Left || xPxLeft > rp.DataRect.Right)
                    continue;

                float yPxOpen = Axes.GetPixelY(ohlc.Open);
                float yPxClose = Axes.GetPixelY(ohlc.Close);

                // low/high line
                PixelLine verticalLine = new(center, top, center, bottom);
                Drawing.DrawLine(rp.Canvas, paint, verticalLine, lineStyle);

                // open/close body
                bool barIsAtLeastOnePixelWide = xPxRight - xPxLeft > 1;
                if (barIsAtLeastOnePixelWide)
                {
                    PixelRangeX xPxRange = new(xPxLeft, xPxRight);
                    PixelRangeY yPxRange = new(Math.Min(yPxOpen, yPxClose), Math.Max(yPxOpen, yPxClose));
                    PixelRect rect = new(xPxRange, yPxRange);
                    if (yPxOpen != yPxClose)
                    {
                        fillStyle.Render(rp.Canvas, rect, paint);
                    }
                    else
                    {
                        lineStyle.Render(rp.Canvas, rect.BottomLine, paint);
                    }
                }
            }
        }
    }

    public abstract class ValhallaAxisBase : LabelStyleProperties
    {
        public bool IsVisible { get; set; } = true;

        public abstract Edge Edge { get; }

        public virtual CoordinateRangeMutable Range { get; private set; } = CoordinateRangeMutable.NotSet;
        public float MinimumSize { get; set; } = 0;
        public float MaximumSize { get; set; } = float.MaxValue;
        public float SizeWhenNoData { get; set; } = 15;
        public PixelPadding EmptyLabelPadding { get; set; } = new(10, 5);
        public PixelPadding PaddingBetweenTickAndAxisLabels { get; set; } = new(5, 3);
        public PixelPadding PaddingOutsideAxisLabels { get; set; } = new(2, 2);

        /// <summary>
        /// Controls whether labels should be clipped to the boundaries of the data area
        /// </summary>
        public bool ClipLabel { get; set; } = false;

        public double Min
        {
            get => Range.Min;
            set => Range.Min = value;
        }

        public double Max
        {
            get => Range.Max;
            set => Range.Max = value;
        }

        public override string ToString()
        {
            return base.ToString() + " " + Range.ToString();
        }

        public virtual ITickGenerator TickGenerator { get; set; } = null!;

        [Obsolete("use LabelText, LabelFontColor, LabelFontSize, LabelFontName, etc. or properties of LabelStyle", false)]
        public LabelStyle Label => LabelStyle;

        public override LabelStyle LabelStyle { get; set; } = new()
        {
            Text = string.Empty,
            FontSize = 16,
            Bold = true,
            Rotation = -90,
        };

        public bool ShowDebugInformation { get; set; } = false;

        public LineStyle FrameLineStyle { get; } = new()
        {
            Width = 1,
            Color = Colors.Black,
            AntiAlias = false,
        };

        public TickMarkStyle MajorTickStyle { get; set; } = new()
        {
            Length = 4,
            Width = 1,
            Color = Colors.Black,
            AntiAlias = false,
        };

        public TickMarkStyle MinorTickStyle { get; set; } = new()
        {
            Length = 2,
            Width = 1,
            Color = Colors.Black,
            AntiAlias = false,
        };

        public LabelStyle TickLabelStyle { get; set; } = new()
        {
            Alignment = Alignment.MiddleCenter
        };

        /// <summary>
        /// Apply a single color to all axis components: label, tick labels, tick marks, and frame
        /// </summary>
        public void Color(Color color)
        {
            LabelStyle.ForeColor = color;
            TickLabelStyle.ForeColor = color;
            MajorTickStyle.Color = color;
            MinorTickStyle.Color = color;
            FrameLineStyle.Color = color;
        }

        /// <summary>
        /// Draw a line along the edge of an axis on the side of the data area
        /// </summary>
        public static void DrawFrame(RenderPack rp, PixelRect panelRect, Edge edge, LineStyle lineStyle)
        {
            PixelLine pxLine = edge switch
            {
                Edge.Left => new(panelRect.Right, panelRect.Bottom, panelRect.Right, panelRect.Top),
                Edge.Right => new(panelRect.Left, panelRect.Bottom, panelRect.Left, panelRect.Top),
                Edge.Bottom => new(panelRect.Left, panelRect.Top, panelRect.Right, panelRect.Top),
                Edge.Top => new(panelRect.Left, panelRect.Bottom, panelRect.Right, panelRect.Bottom),
                _ => throw new NotImplementedException(edge.ToString()),
            };

            if (edge == Edge.Top && !lineStyle.AntiAlias)
            {
                // move the top frame line slightly down so the vertical pixel snaps
                // to the same level as the top of the left and right frame lines
                // https://github.com/ScottPlot/ScottPlot/pull/3976
                pxLine = pxLine.WithDelta(0, .1f);
            }

            using SKPaint paint = new();
            Drawing.DrawLine(rp.Canvas, paint, pxLine, lineStyle);
        }

        private static void DrawTicksHorizontalAxis(RenderPack rp, LabelStyle label, PixelRect panelRect, IEnumerable<Tick> ticks, IAxis axis, TickMarkStyle majorStyle, TickMarkStyle minorStyle)
        {
            if (axis.Edge != Edge.Bottom && axis.Edge != Edge.Top)
            {
                throw new InvalidOperationException();
            }

            using SKPaint paint = new();

            foreach (Tick tick in ticks)
            {
                // draw tick
                paint.Color = tick.IsMajor ? majorStyle.Color.ToSKColor() : minorStyle.Color.ToSKColor();
                paint.StrokeWidth = tick.IsMajor ? majorStyle.Width : minorStyle.Width;
                paint.IsAntialias = tick.IsMajor ? majorStyle.AntiAlias : minorStyle.AntiAlias;
                float tickLength = tick.IsMajor ? majorStyle.Length : minorStyle.Length;
                float xPx = axis.GetPixel(tick.Position, panelRect);
                float y = axis.Edge == Edge.Bottom ? panelRect.Top : panelRect.Bottom;
                float yEdge = axis.Edge == Edge.Bottom ? y + tickLength : y - tickLength;
                PixelLine pxLine = new(xPx, y, xPx, yEdge);
                var lineStyle = tick.IsMajor ? majorStyle : minorStyle;
                lineStyle.Render(rp.Canvas, paint, pxLine);

                // draw label
                if (string.IsNullOrWhiteSpace(tick.Label) || !label.IsVisible)
                    continue;
                label.Text = tick.Label;
                float pxDistanceFromTick = 2;
                float pxDistanceFromEdge = tickLength + pxDistanceFromTick;
                float yPx = axis.Edge == Edge.Bottom ? y + pxDistanceFromEdge : y - pxDistanceFromEdge;
                Pixel labelPixel = new(xPx, yPx);
                if (label.Rotation == 0)
                    label.Alignment = axis.Edge == Edge.Bottom ? Alignment.UpperCenter : Alignment.LowerCenter;
                label.Render(rp.Canvas, labelPixel, paint);
            }
        }

        private static void DrawTicksVerticalAxis(RenderPack rp, LabelStyle label, PixelRect panelRect, IEnumerable<Tick> ticks, IAxis axis, TickMarkStyle majorStyle, TickMarkStyle minorStyle)
        {
            if (axis.Edge != Edge.Left && axis.Edge != Edge.Right)
            {
                throw new InvalidOperationException();
            }

            using SKPaint paint = new();

            foreach (Tick tick in ticks)
            {
                // draw tick
                paint.Color = tick.IsMajor ? majorStyle.Color.ToSKColor() : minorStyle.Color.ToSKColor();
                paint.StrokeWidth = tick.IsMajor ? majorStyle.Width : minorStyle.Width;
                paint.IsAntialias = tick.IsMajor ? majorStyle.AntiAlias : minorStyle.AntiAlias;
                float tickLength = tick.IsMajor ? majorStyle.Length : minorStyle.Length;
                float yPx = axis.GetPixel(tick.Position, panelRect);
                float x = axis.Edge == Edge.Left ? panelRect.Right : panelRect.Left;
                float xEdge = axis.Edge == Edge.Left ? x - tickLength : x + tickLength;
                PixelLine pxLine = new(x, yPx, xEdge, yPx);
                var lineStyle = tick.IsMajor ? majorStyle : minorStyle;
                lineStyle.Render(rp.Canvas, paint, pxLine);

                // draw label
                if (string.IsNullOrWhiteSpace(tick.Label) || !label.IsVisible)
                    continue;
                label.Text = tick.Label; float pxDistanceFromTick = 5;
                float pxDistanceFromEdge = tickLength + pxDistanceFromTick;
                float xPx = axis.Edge == Edge.Left ? x - pxDistanceFromEdge : x + pxDistanceFromEdge;
                Pixel px = new(xPx, yPx);
                if (label.Rotation == 0)
                    label.Alignment = axis.Edge == Edge.Left ? Alignment.MiddleRight : Alignment.MiddleLeft;
                label.Render(rp.Canvas, px, paint);
            }
        }

        public static void DrawTicks(RenderPack rp, LabelStyle label, PixelRect panelRect, IEnumerable<Tick> ticks, IAxis axis, TickMarkStyle majorStyle, TickMarkStyle minorStyle)
        {
            if (axis.Edge.IsVertical())
                DrawTicksVerticalAxis(rp, label, panelRect, ticks, axis, majorStyle, minorStyle);
            else
                DrawTicksHorizontalAxis(rp, label, panelRect, ticks, axis, majorStyle, minorStyle);
        }

        /// <summary>
        /// Replace the <see cref="TickGenerator"/> with a <see cref="NumericManual"/> pre-loaded with the given ticks.
        /// </summary>
        public void SetTicks(double[] xs, string[] labels)
        {
            if (xs.Length != labels.Length)
                throw new ArgumentException($"{nameof(xs)} and {nameof(labels)} must have equal length");

            NumericManual manualTickGen = new();

            for (int i = 0; i < xs.Length; i++)
            {
                manualTickGen.AddMajor(xs[i], labels[i]);
            }

            TickGenerator = manualTickGen;
        }
    }

    public abstract class ValhallaXAxisBase : ValhallaAxisBase, IXAxis
    {
        public double Width => Range.Span;

        public ValhallaXAxisBase()
        {
            LabelRotation = 0;
        }

        public virtual float Measure()
        {
            if (!IsVisible)
                return 0;

            if (!Range.HasBeenSet)
                return SizeWhenNoData;

            using SKPaint paint = new();

            float tickHeight = MajorTickStyle.Length;

            float maxTickLabelHeight = TickGenerator.Ticks.Length > 0
                ? TickGenerator.Ticks.Select(x => TickLabelStyle.Measure(x.Label, paint).Height).Max()
                : 0;

            float axisLabelHeight = string.IsNullOrEmpty(LabelStyle.Text) && LabelStyle.Image is null
                ? EmptyLabelPadding.Vertical
                : LabelStyle.Measure(LabelText, paint).Height
                    + PaddingBetweenTickAndAxisLabels.Vertical
                    + PaddingOutsideAxisLabels.Vertical;

            return tickHeight + maxTickLabelHeight + axisLabelHeight;
        }

        public float GetPixel(double position, PixelRect dataArea)
        {
            double pxPerUnit = dataArea.Width / Width;
            double unitsFromLeftEdge = position - Min;
            float pxFromEdge = (float)(unitsFromLeftEdge * pxPerUnit);
            return dataArea.Left + pxFromEdge;
        }

        public double GetCoordinate(float pixel, PixelRect dataArea)
        {
            double pxPerUnit = dataArea.Width / Width;
            float pxFromLeftEdge = pixel - dataArea.Left;
            double unitsFromEdge = pxFromLeftEdge / pxPerUnit;
            return Min + unitsFromEdge;
        }

        private PixelRect GetPanelRectangleBottom(PixelRect dataRect, float size, float offset)
        {
            return new PixelRect(
                left: dataRect.Left,
                right: dataRect.Right,
                bottom: dataRect.Bottom + offset + size,
                top: dataRect.Bottom + offset);
        }

        private PixelRect GetPanelRectangleTop(PixelRect dataRect, float size, float offset)
        {
            return new PixelRect(
                left: dataRect.Left,
                right: dataRect.Right,
                bottom: dataRect.Top - offset,
                top: dataRect.Top - offset - size);
        }

        public PixelRect GetPanelRect(PixelRect dataRect, float size, float offset)
        {
            return Edge == Edge.Bottom
                ? GetPanelRectangleBottom(dataRect, size, offset)
                : GetPanelRectangleTop(dataRect, size, offset);
        }

        public virtual void Render(RenderPack rp, float size, float offset)
        {
            if (!IsVisible)
                return;

            using SKPaint paint = new();

            PixelRect panelRect = GetPanelRect(rp.DataRect, size, offset);

            float y = Edge == Edge.Bottom
                ? panelRect.Bottom - PaddingOutsideAxisLabels.Vertical
                : panelRect.Top + PaddingOutsideAxisLabels.Vertical;

            Pixel labelPoint = new(panelRect.HorizontalCenter, y);

            if (ShowDebugInformation)
            {
                Drawing.DrawDebugRectangle(rp.Canvas, panelRect, labelPoint, LabelFontColor);
            }

            LabelAlignment = Alignment.LowerCenter;

            rp.CanvasState.Save();

            if (ClipLabel)
                rp.CanvasState.Clip(panelRect);

            LabelStyle.Render(rp.Canvas, labelPoint, paint);

            rp.CanvasState.Restore();

            DrawTicks(rp, TickLabelStyle, panelRect, TickGenerator.Ticks, this, MajorTickStyle, MinorTickStyle);
            DrawFrame(rp, panelRect, Edge, FrameLineStyle);
        }

        public double GetPixelDistance(double distance, PixelRect dataArea)
        {
            return distance * dataArea.Width / Width;
        }

        public double GetCoordinateDistance(float distance, PixelRect dataArea)
        {
            return distance / (dataArea.Width / Width);
        }

        public void RegenerateTicks(PixelLength size)
        {
            using SKPaint paint = new();
            TickLabelStyle.ApplyToPaint(paint);
            TickGenerator.Regenerate(Range.ToCoordinateRange, Edge, size, paint, TickLabelStyle);
        }
    }

    public class ValhallaDateTimeXAxis : ValhallaXAxisBase, IXAxis
    {
        public override Edge Edge { get; } = Edge.Bottom;

        private IDateTimeTickGenerator _tickGenerator = new DateTimeAutomatic();

        public override ITickGenerator TickGenerator
        {
            get => _tickGenerator;
            set
            {
                if (value is not IDateTimeTickGenerator)
                    throw new ArgumentException($"Date axis must have a {nameof(ITickGenerator)} generator");

                _tickGenerator = (IDateTimeTickGenerator)value;
            }
        }

        public IEnumerable<double> ConvertToCoordinateSpace(IEnumerable<DateTime> dates) =>
            TickGenerator is IDateTimeTickGenerator dateTickGenerator
                ? dateTickGenerator.ConvertToCoordinateSpace(dates)
                : throw new InvalidOperationException("Date axis configured with non-date tick generator");
    }
}
