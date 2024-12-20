using DynamicData;
using ScottPlot;
using SkiaSharp;

namespace Valhalla.Charting.CustomSeries
{
    public enum VolumetricType:int
    {
        ValueArea=0,
        FootPrint=1
    }
    public class PriceSerie(IOHLCSource data, List<List<TickAnalysis>> ticks) : IPlottable
    { 
        private string _risingHex = "#e85c58", _failingHex = "#e85c58";
        public List<List<TickAnalysis>> Ticks { get; } = ticks;
        public bool IsVisible { get; set; } = true;

        public IAxes Axes { get; set; } = new Axes();

        public IOHLCSource Data { get; } = data;

        public bool UseVolumetric { get; set; } = true;

        public VolumetricType SelectedVolumetricType { get; set; } = VolumetricType.ValueArea;

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
            this.DrawCandles(rp);
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
                double halfWidthNumber = this.UseVolumetric ? ohlc.TimeSpan.TotalDays / 2 * SymbolWidth : ohlc.TimeSpan.TotalDays / 2 * .8;
                xPxLeft = Axes.GetPixelX(centerNumber - halfWidthNumber);
                xPxRight = Axes.GetPixelX(centerNumber + halfWidthNumber);

                // do not render OHLCs off the screen
                if (xPxRight < rp.DataRect.Left || xPxLeft > rp.DataRect.Right)
                    continue;

                float yPxOpen = Axes.GetPixelY(ohlc.Open);
                float yPxClose = Axes.GetPixelY(ohlc.Close);

                // low/high line
                PixelLine line = new(center, top, center, bottom);
                Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);

                // open/close body
                bool barIsAtLeastOnePixelWide = xPxRight - xPxLeft > 1;
                if (barIsAtLeastOnePixelWide)
                {
                    PixelRangeX xPxRange = new(xPxLeft, xPxRight);
                    PixelRangeY yPxRange = new(Math.Min(yPxOpen, yPxClose), Math.Max(yPxOpen, yPxClose));
                    PixelRect rect = new(xPxRange, yPxRange);
                    if (yPxOpen != yPxClose)
                    {
                        if (this.UseVolumetric)
                        {
                            switch(this.SelectedVolumetricType)
                            {
                                case VolumetricType.ValueArea:
                                    this.DrawValueArea(ohlc, centerNumber, xPxRight, paint, rp);
                                    break;
                                case VolumetricType.FootPrint:
                                    this.DrawFootPrint(ohlc, centerNumber, xPxRight, paint, rp);
                                    break;
                            } 
                        }

                        // fill the body
                        fillStyle.Render(rp.Canvas, rect, paint);

                        // border the body
                        line = new PixelLine(xPxLeft, Axes.GetPixelY(ohlc.Open), xPxLeft, Axes.GetPixelY(ohlc.Close));
                        Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);
                        line = new PixelLine(xPxRight, Axes.GetPixelY(ohlc.Open), xPxRight, Axes.GetPixelY(ohlc.Close));
                        Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);
                        line = new PixelLine(xPxRight, Axes.GetPixelY(ohlc.Open), xPxLeft, Axes.GetPixelY(ohlc.Open));
                        Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);
                        line = new PixelLine(xPxRight, Axes.GetPixelY(ohlc.Close), xPxLeft, Axes.GetPixelY(ohlc.Close));
                        Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);
                    }
                    else
                    {
                        lineStyle.Render(rp.Canvas, rect.BottomLine, paint);
                    }
                }
            }
        }

        private void DrawValueArea(OHLC ohlc, double centerNumber, float xPxRight, SKPaint paint, RenderPack rp)
        {
            float top = Axes.GetPixelY(ohlc.High);
            float bottom = Axes.GetPixelY(ohlc.Low);

            // start drawing the right value area
            var right = ohlc.TimeSpan.TotalDays * .91;
            var left = xPxRight + (float)2;
            var maxRight = Axes.GetPixelX(centerNumber + right);

            FillStyle rangeStyle = new FillStyle()
            {
                Color = Colors.Black
            };

            var tradeList = this.Ticks[this.Data.GetOHLCs().IndexOf(ohlc)];
            int tickCount = tradeList.Count - 1;
            var tickRange = (top - bottom) / tickCount;
            var bigestVolume = tradeList.Max(x => x.Volume);
            var startPrice = top + (tickRange / 2);
            for (int x = 0; x <= tickCount; x++)
            {
                if (tradeList[x].Volume == bigestVolume)
                    rangeStyle.Color = Colors.Orange;
                else
                    rangeStyle.Color = Colors.Gray.WithOpacity(0.2);

                var ratio = Math.Abs(.91 * (tradeList[x].Volume / bigestVolume));
                right = ohlc.TimeSpan.TotalDays * ratio;
                maxRight = Axes.GetPixelX(centerNumber + right);

                PixelRangeY yPxRange = new(startPrice, startPrice - tickRange);
                PixelRangeX xPxRange = new(left, maxRight < left ? left + (left - maxRight) : maxRight);
                PixelRect rect = new(xPxRange, yPxRange);
                rangeStyle.Render(rp.Canvas, rect, paint);
                startPrice -= tickRange;
            }
        }

        private void DrawFootPrint(OHLC ohlc, double centerNumber, float xPxRight, SKPaint paint, RenderPack rp)
        {
            float top = Axes.GetPixelY(ohlc.High);
            float bottom = Axes.GetPixelY(ohlc.Low);

            // start drawing the right value area
            var right = ohlc.TimeSpan.TotalDays * .91;
            var left = xPxRight + (float)4;
            var maxRight = Axes.GetPixelX(centerNumber + right);

            FillStyle rangeStyle = new FillStyle()
            {
                Color = Colors.Black
            };
            LineStyle lineStyle = new LineStyle()
            {
                Color = Colors.Gray,
                Width = 0.5f
            };

            var tradeList = this.Ticks[this.Data.GetOHLCs().IndexOf(ohlc)];
            int tickCount = tradeList.Count - 1;
            var tickRange = (top - bottom) / tickCount;
            var bigestVolume = tradeList.Max(x => x.Volume);
            var startPrice = top + (tickRange / 2);
            var maxBuyVolume = tradeList.Max(x => x.BuyVolume);
            var maxSellVolume = tradeList.Max(x => x.SellVolume);

            var line = new PixelLine(left, startPrice, maxRight, startPrice);
            Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);

            for (int x = 0; x <= tickCount; x++)
            {
                if (tradeList[x].SellVolume > tradeList[x].BuyVolume)
                    rangeStyle.Color = Color.FromHex("#e85c58").WithOpacity(tradeList[x].SellVolume / bigestVolume);
                else
                    rangeStyle.Color = Color.FromHex("#5fa8b7").WithOpacity(tradeList[x].BuyVolume / bigestVolume);

                rangeStyle.Color = tradeList[x].Volume == bigestVolume ? Colors.Orange : rangeStyle.Color;

                PixelRangeY yPxRange = new(startPrice, startPrice - tickRange);
                PixelRangeX xPxRange = new(left, maxRight < left ? left + (left - maxRight) : maxRight);
                PixelRect rect = new(xPxRange, yPxRange);
                rangeStyle.Render(rp.Canvas, rect, paint);

                line = new PixelLine(left, startPrice - tickRange, maxRight, startPrice - tickRange);
                Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);

                var rangeInPixel = (double)(yPxRange.Bottom- yPxRange.Top);

                // ensure there is a minium of 10 pixels space
                if (rangeInPixel > 8)
                {
                    int fontSize = 8;
                    if (rangeInPixel > 25)
                        fontSize = 11;

                   
                    var text = new LabelStyle()
                    {
                        ForeColor = tradeList[x].Volume == bigestVolume? Colors.White: Colors.Black,
                        FontSize = fontSize,
                        Text = tradeList[x].SellVolume.ToString(),
                        Bold = true
                    };

                    // get the textSize in pixels
                    var textSize = text.Measure();

                    // draw bids text
                    var verticalMiddle = (yPxRange.Top + yPxRange.Bottom) / 2;
                    var horizontalMiddle = (xPxRange.Right - xPxRange.Left) / 4;
                    Pixel pixel = new(((left + horizontalMiddle) - (textSize.Width / 4))-2, verticalMiddle - (textSize.Height / 4));
                    text.Render(rp.Canvas, pixel, paint);

                    // draw asks text
                    text.Text = tradeList[x].BuyVolume.ToString();
                    textSize = text.Measure();
                    pixel = new(((left + (3 * horizontalMiddle)) - (textSize.Width / 4))-2, verticalMiddle - (textSize.Height / 4));
                    text.Render(rp.Canvas, pixel, paint);
                }              

                startPrice -= tickRange;
            }

            line = new PixelLine(left, bottom - (tickRange / 2), maxRight, bottom - (tickRange / 2));
            Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);

            var center = (left + maxRight) / 2; 
            line = new PixelLine(center, top + (tickRange / 2), center, bottom-(tickRange/2));
            Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);

            line = new PixelLine(left, top + (tickRange / 2), left, bottom - (tickRange / 2));
            Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);

            line = new PixelLine(maxRight, top + (tickRange / 2), maxRight, bottom - (tickRange / 2));
            Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);
        }
    }
}
