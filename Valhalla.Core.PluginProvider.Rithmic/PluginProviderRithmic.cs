using System.Net;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using ProtoBuf;
using Valhalla.Core.PluginBase;
using Rti;

namespace Valhalla.Core.PluginProvider.Rithmic;

public class PluginProviderRithmic : IProvider
{
    private readonly ILogger _logger;

    public PluginProviderRithmic()
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<PluginProviderRithmic>();
    }
    public string Name => "Rithmic";

    public string Description => "Uses satellite assembly to get Rithmic data.";

    public int Execute(string publisherConnectionString, string subscriberConnectionString, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Execute {Name} : {Description}");
        
        using (var publisherSocket = new PublisherSocket(publisherConnectionString))
        {
            var ws = new ClientWebSocket();
            if(!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("HTTPS_PROXY")))
                ws.Options.Proxy = new WebProxy(Environment.GetEnvironmentVariable("HTTPS_PROXY"));
            else if(!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("https_proxy")))
                ws.Options.Proxy = new WebProxy(Environment.GetEnvironmentVariable("https_proxy"));
            // ws.ConnectAsync(new Uri("wss://rituz00100.rithmic.com"), CancellationToken.None).Wait();
            ws.ConnectAsync(new Uri("wss://rprotocol-mobile.rithmic.com"), CancellationToken.None).Wait();

            var requestLogin = new RequestLogin()
            {
                TemplateId = 10,
                TemplateVersion = "5.27",
                UserMsgs = { "Hello", },
                User = "<login>",
                Password = "<password>",
                AppName = "Valhalla",
                AppVersion = "0-dev",
                SystemName = "Rithmic Paper Trading",
                InfraType = RequestLogin.SysInfraType.TickerPlant,
                AggregatedQuotes = false
            };

            var requestBuffer = new MemoryStream();
            Serializer.Serialize(requestBuffer, requestLogin);
            ws.SendAsync(requestBuffer.ToArray(), WebSocketMessageType.Binary, WebSocketMessageFlags.EndOfMessage, CancellationToken.None).AsTask().Wait();
            var responseBuffer = new byte[1024];
            var result = ws.ReceiveAsync(responseBuffer, CancellationToken.None).Result;
            var responseStream = new MemoryStream(responseBuffer, 0, result.Count);
            var responseLogin = Serializer.Deserialize<ResponseLogin>(responseStream);
            
            if (responseLogin.RpCodes[0] == "0") 
                _logger.LogInformation($"Execute {Name} : Login successful");
            else
                _logger.LogInformation($"Execute {Name} : Login failed");

            
            var requestMarketDataUpdate = new RequestMarketDataUpdate()
            {
                TemplateId = 100,
                UserMsgs = { "Hello", },
                Symbol = "NQH5",
                Exchange = "CME",
                request = RequestMarketDataUpdate.Request.Subscribe,
                update_bits = (uint)RequestMarketDataUpdate.UpdateBits.LastTrade
            };
            
            requestBuffer = new MemoryStream();
            Serializer.Serialize(requestBuffer, requestMarketDataUpdate);
            ws.SendAsync(requestBuffer.ToArray(), WebSocketMessageType.Binary, WebSocketMessageFlags.EndOfMessage, CancellationToken.None).AsTask().Wait();

            responseBuffer = new byte[1024];

            while (!cancellationToken.IsCancellationRequested)
            {
                result = ws.ReceiveAsync(responseBuffer, CancellationToken.None).Result;
                responseStream = new MemoryStream(responseBuffer, 0, result.Count);
                var responseBase = Serializer.Deserialize<Base>(responseStream);

                switch (responseBase.TemplateId)
                {
                    case 150:
                        responseStream = new MemoryStream(responseBuffer, 0, result.Count);
                        var responseLastTrade = Serializer.Deserialize<LastTrade>(responseStream);
                        if (responseLastTrade.TradePrice != 0)
                        {
                            _logger.LogInformation("Sending message : {0}", responseLastTrade.TradePrice.ToString());
                            publisherSocket.SendMoreFrame("RithmicTick").SendFrame(responseLastTrade.TradePrice.ToString());
                        }
                        break;
                }
            }
        }

        return 0;
    }
}