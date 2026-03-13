using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using OcppEvCharging.Server.Models.Enums;
using OcppEvCharging.Server.Ocpp.Handlers;

namespace OcppEvCharging.Tests;

public class AuthorizeHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidIdTag_ReturnsAccepted()
    {
        var handler = new AuthorizeHandler(NullLogger<AuthorizeHandler>.Instance);
        var payload = JsonSerializer.Deserialize<JsonElement>("""{"idTag":"RFID001"}""");

        var result = await handler.HandleAsync("CP001", payload);

        var json = JsonSerializer.Serialize(result);
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.Equal("Accepted", parsed.GetProperty("idTagInfo").GetProperty("status").GetString());
    }

    [Fact]
    public async Task HandleAsync_EmptyPayload_ReturnsAccepted()
    {
        var handler = new AuthorizeHandler(NullLogger<AuthorizeHandler>.Instance);
        var payload = JsonSerializer.Deserialize<JsonElement>("""{}""");

        var result = await handler.HandleAsync("CP001", payload);

        var json = JsonSerializer.Serialize(result);
        var parsed = JsonSerializer.Deserialize<JsonElement>(json);
        Assert.Equal("Accepted", parsed.GetProperty("idTagInfo").GetProperty("status").GetString());
    }
}
