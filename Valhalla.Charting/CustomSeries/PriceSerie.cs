using ScottPlot;
using SkiaSharp;

namespace Valhalla.Charting.CustomSeries
{
    public class PriceSerie(IOHLCSource data, List<List<TickAnalysis>> ticks) : IPlottable
    {
        public List<List<TickAnalysis>> Ticks { get; } = ticks;
        public bool IsVisible { get; set; } = true;

        public IAxes Axes { get; set; } = new Axes();

        public IOHLCSource Data { get; } = data;


        /// <summary>
        /// Fractional width of the candle symbol relative to its time span
        /// </summary>
        public double SymbolWidth = 0.1;

        public LineStyle RisingLineStyle { get; } = new()
        {
            Color = Colors.Black,//Color.FromHex("#089981"),
            Width = 1,
        };

        public LineStyle FallingLineStyle { get; } = new()
        {
            Color = Colors.Black,//Color.FromHex("#f23645"),
            Width = 1,
        };

        public FillStyle RisingFillStyle { get; } = new()
        {
            Color = Color.FromHex("#5fa8b7"),
        };

        public FillStyle FallingFillStyle { get; } = new()
        {
            Color = Color.FromHex("#e85c58"),
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
            
            var result = Data.GetPriceRange(minIndexInView, maxIndexInView); 
           
            return result;
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
           // this.DrawCandles(rp);

            this.DrawCandlesWithArea(rp);
        }

        private void DrawCandles(RenderPack rp)
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

                double centerNumber = NumericConversion.ToNumber(ohlc.DateTime);
                center = Axes.GetPixelX(centerNumber);
                double halfWidthNumber = ohlc.TimeSpan.TotalDays / 2 * .8;
                xPxLeft = Axes.GetPixelX(centerNumber - halfWidthNumber);
                xPxRight = Axes.GetPixelX(centerNumber + halfWidthNumber);

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

        private void DrawCandlesWithArea(RenderPack rp)
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

                double centerNumber = NumericConversion.ToNumber(ohlc.DateTime);
                center = Axes.GetPixelX(centerNumber);
                double halfWidthNumber = ohlc.TimeSpan.TotalDays / 2 * SymbolWidth;
                xPxLeft = Axes.GetPixelX(centerNumber - halfWidthNumber);
                xPxRight = Axes.GetPixelX(centerNumber + halfWidthNumber);

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
                        // fill the body
                        fillStyle.Render(rp.Canvas, rect, paint);

                        verticalLine = new PixelLine(xPxLeft, Axes.GetPixelY(ohlc.Open), xPxLeft, Axes.GetPixelY(ohlc.Close));
                        Drawing.DrawLine(rp.Canvas, paint, verticalLine, lineStyle);
                        verticalLine = new PixelLine(xPxRight, Axes.GetPixelY(ohlc.Open), xPxRight, Axes.GetPixelY(ohlc.Close));
                        Drawing.DrawLine(rp.Canvas, paint, verticalLine, lineStyle);
                        verticalLine = new PixelLine(xPxRight, Axes.GetPixelY(ohlc.Open), xPxLeft, Axes.GetPixelY(ohlc.Open));
                        Drawing.DrawLine(rp.Canvas, paint, verticalLine, lineStyle);
                        verticalLine = new PixelLine(xPxRight, Axes.GetPixelY(ohlc.Close), xPxLeft, Axes.GetPixelY(ohlc.Close));
                        Drawing.DrawLine(rp.Canvas, paint, verticalLine, lineStyle);

                        // start drawing the right value area
                        var right = ohlc.TimeSpan.TotalDays * .91;
                        var left = xPxRight + (float)2;
                        var maxRight = Axes.GetPixelX(centerNumber + right);
                        
                        FillStyle rangeStyle = new FillStyle()
                        {
                            Color = Colors.Black
                        };
                         
                        
                        var tradeList = this.Ticks[i];
                        int tickCount = tradeList.Count-1;
                        var tickRange = (top - bottom) / tickCount;
                        var bigestVolume = tradeList.Max(x => x.Volume);
                        var startPrice = top+(tickRange/2);
                        for (int x =0; x<=tickCount; x++)
                        {
                            if (tradeList[x].Volume== bigestVolume)
                                rangeStyle.Color = Colors.Orange;
                            else
                                rangeStyle.Color = Colors.Gray.WithOpacity(0.2);

                            var ratio = Math.Abs(.91 * (tradeList[x].Volume / bigestVolume));
                            right = ohlc.TimeSpan.TotalDays * ratio;
                            maxRight = Axes.GetPixelX(centerNumber + right);

                            yPxRange = new(startPrice, startPrice-tickRange);
                            xPxRange = new(left, maxRight < left ? left + (left - maxRight) : maxRight);
                            rect = new(xPxRange, yPxRange);                          
                            rangeStyle.Render(rp.Canvas, rect, paint);
                            startPrice -= tickRange;
                        }
                       
                    }
                    else
                    {
                        lineStyle.Render(rp.Canvas, rect.BottomLine, paint);
                    }
                }
            }
        }
    }
}
