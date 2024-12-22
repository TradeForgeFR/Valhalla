using NetMQ.Sockets;

namespace Valhalla.Core.PluginBase;

public interface IProvider
{
    string Name { get; }
    string Description { get; }

    int Execute(string publisherConnectionString, string subscriberConnectionString);
}