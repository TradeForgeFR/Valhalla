using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using Valhalla.Core.PluginBase;


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

    public int Execute(string publisherConnectionString, string subscriberConnectionString)
    {
        _logger.LogInformation($"Execute {Name} : {Description}");
        
        using (var publisherSocket = new PublisherSocket(publisherConnectionString))
        {
            var price = 22160.0;
            var rand = new Random();
            while (true)
            {
                price += rand.Next(-4, 4) * 0.25;
                var msg = price.ToString();
                _logger.LogInformation("Sending message : {0}", msg);
                publisherSocket.SendMoreFrame("RithmicTick").SendFrame(msg);
                Thread.Sleep(200 * rand.Next(1, 10));
            }
        }

        return 0;
    }
}