namespace Valhalla.Core.MarketData;

/// <summary>
/// 
/// </summary>
public class ConnectionsMngr
{
    private Connection[] _connections = [];

    public Connection[] Connections => _connections;
    
    /// <summary>
    /// Add a connection
    /// </summary>
    /// <param name="connection"></param>
    public void AddConnection(Connection connection)
    {
        _connections.Append(connection);
    }
}