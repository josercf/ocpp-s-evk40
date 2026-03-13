using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace OcppEvCharging.Server.Ocpp;

public class ChargePointConnection
{
    public string ChargePointId { get; }
    public WebSocket WebSocket { get; }
    public DateTime ConnectedAt { get; } = DateTime.UtcNow;
    public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

    public ChargePointConnection(string chargePointId, WebSocket webSocket)
    {
        ChargePointId = chargePointId;
        WebSocket = webSocket;
    }
}

public class ChargePointConnectionManager
{
    private readonly ConcurrentDictionary<string, ChargePointConnection> _connections = new();

    public void Add(ChargePointConnection connection)
    {
        _connections[connection.ChargePointId] = connection;
    }

    public void Remove(string chargePointId)
    {
        _connections.TryRemove(chargePointId, out _);
    }

    public ChargePointConnection? Get(string chargePointId)
    {
        _connections.TryGetValue(chargePointId, out var connection);
        return connection;
    }

    public IReadOnlyCollection<ChargePointConnection> GetAll()
    {
        return _connections.Values.ToList().AsReadOnly();
    }
}
