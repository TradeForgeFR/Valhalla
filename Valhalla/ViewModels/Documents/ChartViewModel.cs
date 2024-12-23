using Dock.Model.Mvvm.Controls;
using System;
using System.Collections.Generic;
using Avalonia.Threading;
using Binance.Net.Clients;
using System.Linq;
using ScottPlot;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using ScottPlot.Plottables;
using Valhalla.Charting;

namespace Valhalla.ViewModels.Documents;

public class ChartViewModel : Document
{
    public ChartViewModel()
    {
        OHLCs = [];
        this.StockChart.AvaPlot.Plot.Axes.DateTimeTicksBottom();
        this.StockChart.AvaPlot.Plot.Add.Candlestick(OHLCs);
        Thread updateChart = new Thread(UpdateChart);
        updateChart.IsBackground = true;
        updateChart.Start();
    }
    
    #region private fields
    private readonly List<OHLC> OHLCs;
    private readonly StockChart _stockChart = new StockChart();
    private BinanceSocketClient _binanceClient = new BinanceSocketClient();        
    #endregion

    #region public fields
    public StockChart StockChart { get {  return _stockChart; } }       
    #endregion

    private void UpdateChart()
    {
        using (var subSocket = new SubscriberSocket())
        {
            subSocket.Options.ReceiveHighWatermark = 1000;
            subSocket.Connect("tcp://127.0.0.1:1234");
            subSocket.Subscribe("RithmicTick");
            Console.WriteLine("Subscriber socket connecting...");

            double openPrice = 0;
            double highPrice = 0;
            double lowPrice = 0;
            var openTime = new DateTime(0);
            var timeSpan = TimeSpan.FromSeconds(15);
            
            while (true)
            {
                var messageTopicReceived = subSocket.ReceiveFrameString();
                var messageReceived = subSocket.ReceiveFrameString();
                // Console.WriteLine(messageReceived);
                var now = DateTime.Now;
                
                var closePrice = double.Parse(messageReceived);
                if (now.Ticks / timeSpan.Ticks > openTime.Ticks / timeSpan.Ticks)
                {
                    openTime = new DateTime(now.Ticks / timeSpan.Ticks * timeSpan.Ticks);
                    openPrice = highPrice = lowPrice = closePrice;
                    
                    // Console.WriteLine("DT: " + openTime + " Open Price: " + openPrice + " High Price: " + highPrice + " Low Price: " + lowPrice + " Close Price: " + closePrice);
                    
                    OHLCs.Add(new OHLC(openPrice, highPrice, lowPrice, closePrice, openTime, timeSpan));
                }
                else
                {
                    highPrice = closePrice > highPrice ? closePrice : highPrice;
                    lowPrice = closePrice < lowPrice ? closePrice : lowPrice;

                    // Console.WriteLine("DT: " + openTime + " Open Price: " + openPrice + " High Price: " + highPrice + " Low Price: " + lowPrice + " Close Price: " + closePrice);
                    
                    int lastOhlcIndex = OHLCs.Count - 1;
                    OHLC updatedOhlc = OHLCs[lastOhlcIndex];
                    updatedOhlc.High = highPrice;
                    updatedOhlc.Low = lowPrice;
                    updatedOhlc.Close = closePrice;
                    OHLCs[lastOhlcIndex] = updatedOhlc;
                }                
                
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    // this.StockChart.AvaPlot.Plot.Axes.AutoScaleExpand();
                    this.StockChart.AvaPlot.Plot.PlotControl!.Refresh();
                }, DispatcherPriority.Background);
            }
        }
    } 
}

