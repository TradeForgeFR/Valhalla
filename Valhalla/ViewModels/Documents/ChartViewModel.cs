using Dock.Model.Mvvm.Controls;
using System;
using Avalonia.Threading;
using Binance.Net.Clients;
using System.Linq;
using ScottPlot;
using System.Diagnostics;
using System.Threading.Tasks;
using Valhalla.Charting;

namespace Valhalla.ViewModels.Documents;

public class ChartViewModel : Document
{
    public ChartViewModel()
    {
        this.FillTheChart();
    }
    
    #region private fields
    private StockChart _stockChart = new StockChart();
    private BinanceSocketClient _binanceClient = new BinanceSocketClient();        
    #endregion

    #region public fields
    public StockChart StockChart { get {  return _stockChart; } }       
    #endregion

    public async Task FillTheChart()
    {
        var request = await this._binanceClient.SpotApi.ExchangeData.GetUIKlinesAsync("BTCUSDT", Binance.Net.Enums.KlineInterval.OneHour, limit: 2000);

        if (request.Success)
        {
            var bars = request.Data.Result.Select(x => new OHLC((double)x.OpenPrice, (double)x.HighPrice, (double)x.LowPrice, (double)x.ClosePrice, x.OpenTime, TimeSpan.FromMinutes(60))).ToArray();
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.StockChart.AvaPlot.Plot.Add.Candlestick(bars);
                this.StockChart.AvaPlot.Plot.Axes.DateTimeTicksBottom();
                this.StockChart.AvaPlot.Plot.PlotControl!.Refresh();
            }, DispatcherPriority.Background);
        }
        else
        {
            Debug.WriteLine("error");
        }
    } 
}

