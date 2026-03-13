using System.Net.WebSockets;
using Moq;
using OcppEvCharging.Server.Ocpp;

namespace OcppEvCharging.Tests;

public class ConnectionManagerTests
{
    [Fact]
    public void Add_NewConnection_CanBeRetrieved()
    {
        var manager = new ChargePointConnectionManager();
        var ws = new Mock<WebSocket>().Object;
        var connection = new ChargePointConnection("CP001", ws);

        manager.Add(connection);

        var retrieved = manager.Get("CP001");
        Assert.NotNull(retrieved);
        Assert.Equal("CP001", retrieved.ChargePointId);
    }

    [Fact]
    public void Get_NonExistent_ReturnsNull()
    {
        var manager = new ChargePointConnectionManager();
        Assert.Null(manager.Get("CP999"));
    }

    [Fact]
    public void Remove_ExistingConnection_IsRemoved()
    {
        var manager = new ChargePointConnectionManager();
        var ws = new Mock<WebSocket>().Object;
        manager.Add(new ChargePointConnection("CP001", ws));

        manager.Remove("CP001");

        Assert.Null(manager.Get("CP001"));
    }

    [Fact]
    public void GetAll_MultipleConnections_ReturnsAll()
    {
        var manager = new ChargePointConnectionManager();
        manager.Add(new ChargePointConnection("CP001", new Mock<WebSocket>().Object));
        manager.Add(new ChargePointConnection("CP002", new Mock<WebSocket>().Object));
        manager.Add(new ChargePointConnection("CP003", new Mock<WebSocket>().Object));

        var all = manager.GetAll();

        Assert.Equal(3, all.Count);
    }

    [Fact]
    public void Add_SameId_OverwritesPreviousConnection()
    {
        var manager = new ChargePointConnectionManager();
        var ws1 = new Mock<WebSocket>().Object;
        var ws2 = new Mock<WebSocket>().Object;
        manager.Add(new ChargePointConnection("CP001", ws1));
        manager.Add(new ChargePointConnection("CP001", ws2));

        var retrieved = manager.Get("CP001");
        Assert.NotNull(retrieved);
        Assert.Same(ws2, retrieved.WebSocket);
    }

    [Fact]
    public void ChargePointConnection_SetsTimestamps()
    {
        var before = DateTime.UtcNow;
        var conn = new ChargePointConnection("CP001", new Mock<WebSocket>().Object);
        var after = DateTime.UtcNow;

        Assert.InRange(conn.ConnectedAt, before, after);
        Assert.InRange(conn.LastMessageAt, before, after);
    }
}
