using System.Text.Json;
using OcppEvCharging.Server.Data;
using OcppEvCharging.Server.Models.Ocpp16;

namespace OcppEvCharging.Server.Ocpp.Handlers;

public class HeartbeatHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HeartbeatHandler> _logger;

    public HeartbeatHandler(IServiceProvider serviceProvider, ILogger<HeartbeatHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<object> HandleAsync(string chargePointId, JsonElement payload)
    {
        _logger.LogDebug("Heartbeat from {ChargePointId}", chargePointId);

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OcppDbContext>();

        var chargePoint = await db.ChargePoints.FindAsync(chargePointId);
        if (chargePoint != null)
        {
            chargePoint.LastHeartbeat = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return new HeartbeatResponse
        {
            CurrentTime = DateTime.UtcNow.ToString("o")
        };
    }
}
