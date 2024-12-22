using DynamicData;
using ReactiveUI;
using ScottPlot;
using SkiaSharp;
using Valhalla.TechnicalAnalysis.Models;

namespace Valhalla.Charting.CustomSeries
{
    public enum VolumetricType:int
    {
        ValueArea=0,
        BidAsk=1
    }

    public enum StatisticsBarEdge
    {
        Top,
        Bottom,
        None
    }
    public class AdvancedStockPlottable : ReactiveObject, IPlottable
    {
        private string _risingHex = "#e85c58", _failingHex = "#e85c58";
        private bool _useVolumetrics = false, _displayTradesBar = true, _displayVolumesBar = true;
        private StatisticsBarEdge _statisticsBarEdge = StatisticsBarEdge.None;
        private VolumetricType _volumetricType = VolumetricType.ValueArea;
        private int _barStatisticHeight = 20;

        public List<StatisticsBarEdge> StatisticsBarEdgeList { get; set; } = Enum.GetValues(typeof(StatisticsBarEdge)).Cast<StatisticsBarEdge>().ToList();
        public List<VolumetricType> VolumetricsTypeList { get; set; } = Enum.GetValues(typeof(VolumetricType)).Cast<VolumetricType>().ToList();
        public bool IsVisible { get; set; } = true;

        public IAxes Axes { get; set; } = new Axes();

        public bool UseVolumetric
        {
            get { return this._useVolumetrics; }
            set
            {
                this.RaiseAndSetIfChanged(ref this._useVolumetrics, value);
            } 
        }

        public bool DisplayTradesBar
        {
            get { return this._displayTradesBar; }
            set
            {
                this.RaiseAndSetIfChanged(ref this._displayTradesBar, value);
            }
        }

        public bool DisplayVolumesBar
        {
            get { return this._displayVolumesBar; }
            set
            {
                this.RaiseAndSetIfChanged(ref this._displayVolumesBar, value);
            }
        }

        public StatisticsBarEdge SelectedStatisticsBarEdge
        {
            get { return this._statisticsBarEdge; }
            set
            {
                this.RaiseAndSetIfChanged(ref this._statisticsBarEdge, value);
            }
        }
        public VolumetricType SelectedVolumetricType
        {
            get { return this._volumetricType; }
            set
            {
                this._volumetricType = value;
                this.RaiseAndSetIfChanged(ref this._volumetricType, value);
            }
        }

        public List<OHLCDatas> Datas { get; set; } = new List<OHLCDatas>();

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
        public IEnumerable<LegendItem> LegendItems => Enumerable.Empty<LegendItem>();

        public AxisLimits GetAxisLimits()
        { 
            if (this.Datas.Count == 0)
                return new(0,0,0,0);

            double left = this.Datas.First().Time.ToOADate() - this.Datas.First().TimeSpan.TotalDays / 2;
            double right = this.Datas.Last().Time.ToOADate() + this.Datas.Last().TimeSpan.TotalDays / 2;
            double bottom = this.Datas.Min(x => x.Low);
            double top = this.Datas.Max(x => x.High);

            return new(left, right, bottom, top);
        }

        public double PixelsBetweenCandles()
        { 
            if (this.Datas.Count == 0 || this.Datas.Count < 2) return 0;

            var ohlc1 = this.Datas[0];
            var ohlc2 = this.Datas[1];

            double centerNumber = NumericConversion.ToNumber(ohlc1.Time);
            var center = Axes.GetPixelX(centerNumber);


            double centerNumber2 = NumericConversion.ToNumber(ohlc2.Time);
            var center2 = Axes.GetPixelX(centerNumber2);

            return Math.Abs(center - center2);
        }

        public virtual void Render(RenderPack rp)
        {
            using SKPaint paint = new();
             

            // ensure there is at list 60 pixels between the candles 
            var spaceBetweenCandles = this.PixelsBetweenCandles();
            var isSpaceEnough = spaceBetweenCandles >= 60;

            for (int i = 0; i < this.Datas.Count; i++)
            {
                OHLCDatas ohlc = this.Datas[i];
                bool isRising = ohlc.Close >= ohlc.Open;
                LineStyle lineStyle = isRising ? RisingLineStyle : FallingLineStyle;
                FillStyle fillStyle = isRising ? RisingFillStyle : FallingFillStyle;

                float top = Axes.GetPixelY(ohlc.High);
                float bottom = Axes.GetPixelY(ohlc.Low);

                float center, xPxLeft, xPxRight;

                double centerNumber = NumericConversion.ToNumber(ohlc.Time);
                center = Axes.GetPixelX(centerNumber);
                double halfWidthNumber = (this.UseVolumetric && isSpaceEnough) ? ohlc.TimeSpan.TotalDays / 2 * .1 : ohlc.TimeSpan.TotalDays / 2 * .8;
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

                        if (this.UseVolumetric && isSpaceEnough)
                        {
                            switch (this.SelectedVolumetricType)
                            {
                                case VolumetricType.ValueArea:
                                    this.RenderValueArea(ohlc, centerNumber, xPxRight, rp);
                                    break;
                                case VolumetricType.BidAsk:
                                    this.RenderFootPrint(ohlc, centerNumber, xPxRight, rp);
                                    break;
                            }

                            this.RenderBottomPanel(ohlc, centerNumber - halfWidthNumber,  rp);
                        } 
                    }
                    else
                    {
                        lineStyle.Render(rp.Canvas, rect.BottomLine, paint);
                    }
                }
            }
        }

        private void RenderValueArea(OHLCDatas ohlc, double centerNumber, float xPxRight, RenderPack rp)
        {
            if (ohlc.Ticks.Count == 0)
                return;

            using SKPaint paint = new();

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

            var tradeList = ohlc.Ticks;
            int tickCount = tradeList.Count - 1;
            var tickRange = (top - bottom) / tickCount;
            var bigestVolume = tradeList.Max(x => x.Volume);
            var startPrice = top + (tickRange / 2);
            float smallesFontSize = 12;

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

                var boxHeight = Math.Abs(yPxRange.Top - yPxRange.Bottom);

                var text = new LabelStyle()
                {
                    ForeColor = tradeList[x].Volume == bigestVolume ? Colors.White : Colors.Black,
                    FontSize = Math.Min(.5f * boxHeight, smallesFontSize),
                    Text = tradeList[x].Volume.ToString(),
                    Bold = tradeList[x].Volume == bigestVolume
                };

                // get the textSize in pixels
                var textSize = text.Measure();

                // draw bids text
                var verticalMiddle = (yPxRange.Top + yPxRange.Bottom) / 2;
                Pixel pixel = new(left + 4, verticalMiddle - (textSize.Height / 4));
                text.Render(rp.Canvas, pixel, paint);

                startPrice -= tickRange;
            }
        }

        private void RenderFootPrint(OHLCDatas ohlc, double centerNumber, float xPxRight, RenderPack rp)
        {
            if (ohlc.Ticks.Count == 0)
                return;

            using SKPaint paint = new();

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

            var tradeList = ohlc.Ticks;
            int tickCount = tradeList.Count - 1;
            var tickRange = (top - bottom) / tickCount;
            var bigestVolume = tradeList.Max(x => x.Volume);
            var startPrice = top + (tickRange / 2);
            var maxBuyVolume = tradeList.Max(x => x.BuyVolume);
            var maxSellVolume = tradeList.Max(x => x.SellVolume);

            var line = new PixelLine(left, startPrice, maxRight, startPrice);
            Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);
            float smallesFontSize = 12;

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

                var boxHeight = Math.Abs(yPxRange.Top - yPxRange.Bottom);
                var text = new LabelStyle()
                {
                    ForeColor = tradeList[x].Volume == bigestVolume ? Colors.White : Colors.Black,
                    FontSize = Math.Min(.5f * boxHeight, smallesFontSize),
                    Text = tradeList[x].SellVolume.ToString(),
                    Bold = tradeList[x].Volume == bigestVolume
                };

                // get the textSize in pixels
                var textSize = text.Measure();


                // draw bids text
                var verticalMiddle = (yPxRange.Top + yPxRange.Bottom) / 2;
                var horizontalMiddle = (xPxRange.Right - xPxRange.Left) / 4;
                Pixel pixel = new(((left + horizontalMiddle) - (textSize.Width / 4)) - 2, verticalMiddle - (textSize.Height / 4));
                text.Render(rp.Canvas, pixel, paint);

                // draw asks text
                text.Text = tradeList[x].BuyVolume.ToString();
                textSize = text.Measure();
                pixel = new(((left + (3 * horizontalMiddle)) - (textSize.Width / 4)) - 2, verticalMiddle - (textSize.Height / 4));
                text.Render(rp.Canvas, pixel, paint);

                startPrice -= tickRange;
            }

            line = new PixelLine(left, bottom - (tickRange / 2), maxRight, bottom - (tickRange / 2));
            Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);

            var center = (left + maxRight) / 2;
            line = new PixelLine(center, top + (tickRange / 2), center, bottom - (tickRange / 2));
            Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);

            line = new PixelLine(left, top + (tickRange / 2), left, bottom - (tickRange / 2));
            Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);

            line = new PixelLine(maxRight, top + (tickRange / 2), maxRight, bottom - (tickRange / 2));
            Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);
        }

        private void RenderBottomPanel(OHLCDatas ohlc, double leftValue, RenderPack rp)
        {
            if (this.SelectedStatisticsBarEdge == StatisticsBarEdge.None)
                return;

            // make basic calculations
            var volumes = ohlc.Ticks.Sum(x => x.Volume);
            var barTotalBuys = ohlc.Ticks.Sum(x => x.BuyVolume);
            var barTotalSells = ohlc.Ticks.Sum(x => x.SellVolume);
            var delta = barTotalBuys - barTotalSells;
            var isDivergent = ohlc.Open > ohlc.Close && delta > 0;
            var maxVolume = this.Datas.Max(x=> x.Ticks.Sum(t  => t.Volume));  
            using SKPaint paint = new();

            // Step 1: Initialize the drawing elements
            var (rowHeight, top, bottom, xPxRange, left, right, lineStyle) = InitializeDrawingElements(ohlc, leftValue);

            // Step 2: Render a white background
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.White, Colors.White, string.Empty, this.DisplayTradesBar, false);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.White, Colors.White, string.Empty, this.DisplayVolumesBar, false);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.White, Colors.White, string.Empty, true, false);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.White, Colors.White, string.Empty, true, false);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.White, Colors.White, string.Empty, true, false);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.White, Colors.White, string.Empty, false, false);

            // Step 3: Re Initialize the drawing elements
            (rowHeight, top, bottom, xPxRange, left, right, lineStyle) = InitializeDrawingElements(ohlc, leftValue); 

            // Step 4: Render rows with datas
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.Beige, Colors.Black, ohlc.Ticks.Sum(x => x.Trades).ToString(), this.DisplayTradesBar, false);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.Orange.WithOpacity(maxVolume/ volumes), Colors.Black, ohlc.Ticks.Sum(x=> x.Volume).ToString(), this.DisplayVolumesBar, false);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, this.RisingFillStyle.Color.WithOpacity(volumes/ barTotalBuys), Colors.Black, ohlc.Ticks.Sum(x => x.BuyVolume).ToString(), true, false);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, this.FallingFillStyle.Color.WithOpacity(volumes/ barTotalSells), Colors.Black, ohlc.Ticks.Sum(x => x.SellVolume).ToString(), true, false);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, delta>0? Colors.Green : Colors.IndianRed, Colors.Black, delta.ToString(), true, false);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.Bisque, Colors.Black, string.Empty, false, false);

            // Step 5 
            (rowHeight, top, bottom, xPxRange, left, right, lineStyle) = InitializeDrawingElements(ohlc, leftValue, true);

            // Step 2: Render titles
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.Black, Colors.White, "Trades", this.DisplayTradesBar, true);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.Black, Colors.White, "Volumes" , this.DisplayVolumesBar, true);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.Black, Colors.White, "Buy volumes", true, true);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.Black, Colors.White, "Sell volumes", true, true);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.Black, Colors.White, "Delta", true, true);
            RenderRow(ohlc, rp, paint, rowHeight, ref top, ref bottom, xPxRange, Colors.Black, Colors.White, string.Empty, false, true);


            // Step 3: Draw the bar
            // DrawStatisticsBar(left, right, top, bottom, rp, paint, lineStyle, ohlc);
        }

        private (int rowHeight, float top, float bottom, PixelRangeX xPxRange, float left, float right, LineStyle lineStyle) InitializeDrawingElements(OHLCDatas ohlc, double leftValue, bool isForTitle = false)
        {
            var rowHeight = this.SelectedStatisticsBarEdge == StatisticsBarEdge.Bottom ? _barStatisticHeight : -_barStatisticHeight;
            float top = this.SelectedStatisticsBarEdge == StatisticsBarEdge.Bottom ? Axes.GetPixelY(this.Axes.YAxis.Min) - rowHeight : Axes.GetPixelY(this.Axes.YAxis.Max) - rowHeight;
            float bottom = this.SelectedStatisticsBarEdge == StatisticsBarEdge.Bottom ? Axes.GetPixelY(this.Axes.YAxis.Min) : Axes.GetPixelY(this.Axes.YAxis.Max);

            var days = ohlc.TimeSpan.TotalDays;
            var left = Axes.GetPixelX(leftValue) - 1;
            var right = Axes.GetPixelX(leftValue + days) + 1;

            if (isForTitle)
            {
                left = Axes.GetPixelX(this.Axes.XAxis.Max) - 80;
                right = Axes.GetPixelX(this.Axes.XAxis.Max);
            }
            PixelRangeX xPxRange = new(left, right);

            LineStyle lineStyle = new LineStyle()
            {
                Color = Colors.Black,
                Width = 0.5f
            };

            return (rowHeight, top, bottom, xPxRange, left, right, lineStyle);
        }
         
        private void RenderRow(OHLCDatas ohlc, RenderPack rp, SKPaint paint, int rowHeight, ref float top, ref float bottom, PixelRangeX xPxRange, ScottPlot.Color backColor, Color textColor, string text, bool shouldRender, bool isTitle)
        {
            if (!shouldRender)
                return;
            
            FillStyle rangeStyle = new FillStyle()
            {
                Color = backColor
            };

            PixelRangeY yPxRange = new(bottom, top);
            PixelRect rect = new(xPxRange, yPxRange);
            rangeStyle.Render(rp.Canvas, rect, paint);

            PixelLine line = new PixelLine(xPxRange.Left, top, xPxRange.Right, top);
            Drawing.DrawLine(rp.Canvas, paint, line, new LineStyle { Color = Colors.Black, Width = 0.5f });

            if (!string.IsNullOrEmpty(text))
            {
                // Define the LabelStyle
                var label = new LabelStyle()
                {
                    ForeColor = textColor,
                    FontSize = 10,
                    Text = text,
                    Bold = isTitle
                };

                // Measure the text size in pixels
                var textSize = label.Measure();

                // Calculate the proper middle positions
                var verticalMiddle = (yPxRange.Top + yPxRange.Bottom) / 2;
                var horizontalMiddle = (xPxRange.Right + xPxRange.Left) / 2;

                // Adjust the pixel position to center the text
                Pixel pixel = new(horizontalMiddle - (textSize.Width / 2), verticalMiddle - (textSize.Height / 2));

                // Render the text
                label.Render(rp.Canvas, pixel, paint);
            }

            top -= rowHeight;
            bottom -= rowHeight;
        }

        private void DrawStatisticsBar(float left, float right, float top, float bottom, RenderPack rp, SKPaint paint, LineStyle lineStyle, OHLCDatas ohlc)
        {
            var y = this.SelectedStatisticsBarEdge == StatisticsBarEdge.Bottom ? Axes.GetPixelY(this.Axes.YAxis.Min) : Axes.GetPixelY(this.Axes.YAxis.Max);
            PixelLine line = new PixelLine(left, top, left, y);
            Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);

            var barsCount = this.Datas.Count - 1;
            var index = this.Datas.IndexOf(ohlc);
            if (barsCount == index)
            {
                line = new PixelLine(right, top, right, y);
                Drawing.DrawLine(rp.Canvas, paint, line, lineStyle);
            }
        }
    }
}
