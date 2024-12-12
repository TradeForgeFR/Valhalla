using Avalonia.Threading;
using ScottPlot;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Vahallah.Charting;
using Binance.Net.Clients;
using System.Linq;
using System;

namespace Vahallah.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region private fields
        private StockChart _stockChart = new StockChart();
        private BinanceSocketClient _binanceClient = new BinanceSocketClient();        
        #endregion

        #region public fields
        public StockChart StockChart { get {  return _stockChart; } }       
        #endregion

        public async Task FillTheChart()
        {
            var request = await _binanceClient.SpotApi.ExchangeData.GetUIKlinesAsync("BTCUSDT", Binance.Net.Enums.KlineInterval.OneHour, limit: 2000);

            if (request.Success)
            {
                var bars = request.Data.Result.Select(x => new OHLC((double)x.OpenPrice, (double)x.HighPrice, (double)x.LowPrice, (double)x.ClosePrice, x.OpenTime, TimeSpan.FromMinutes(60))).ToArray();

                List<OHLC> prices = new();
                var socket = new BinanceSocketClient();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StockChart.AvaPlot.Plot.Add.Candlestick(bars);
                    StockChart.AvaPlot.Plot.Axes.DateTimeTicksBottom();
                    StockChart.AvaPlot.Plot.PlotControl!.Refresh();
                }, DispatcherPriority.Background);
            }
            else
            {
                Debug.WriteLine("error");
            }
        } 
       
    }
}
