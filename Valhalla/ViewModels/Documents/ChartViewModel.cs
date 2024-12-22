using Dock.Model.Mvvm.Controls;
using System;
using Avalonia.Threading;
using Binance.Net.Clients;
using System.Linq;
using ScottPlot;
using System.Diagnostics;
using System.Threading.Tasks;
using Valhalla.Charting;
using Valhalla.TechnicalAnalysis.Models;

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
        var request = await this._binanceClient.SpotApi.ExchangeData.GetUIKlinesAsync("BTCUSDT", Binance.Net.Enums.KlineInterval.OneMinute, limit: 1000);

        if (request.Success)
        {
            var bars = request.Data.Result.Select(x => new OHLCDatas((double)x.OpenPrice, (double)x.HighPrice, (double)x.LowPrice, (double)x.ClosePrice, x.OpenTime, TimeSpan.FromMinutes(1))).ToArray();
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                this.StockChart.FillPrice(bars.ToList());
            }, DispatcherPriority.Background);
        }
        else
        {
            Debug.WriteLine("error");
        }
    } 
}

