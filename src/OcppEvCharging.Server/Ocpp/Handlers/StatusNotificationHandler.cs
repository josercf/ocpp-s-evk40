using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OcppEvCharging.Server.Data;
using OcppEvCharging.Server.Data.Entities;
using OcppEvCharging.Server.Models.Ocpp16;

namespace OcppEvCharging.Server.Ocpp.Handlers;

public class StatusNotificationHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StatusNotificationHandler> _logger;

    public StatusNotificationHandler(IServiceProvider serviceProvider, ILogger<StatusNotificationHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<object> HandleAsync(string chargePointId, JsonElement payload)
    {
        var request = payload.Deserialize<StatusNotificationRequest>();
        if (request == null)
            throw new OcppProtocolException("Invalid StatusNotification payload");

        _logger.LogInformation(
            "StatusNotification from {ChargePointId}: Connector={ConnectorId}, Status={Status}, Error={ErrorCode}",
            chargePointId, request.ConnectorId, request.Status, request.ErrorCode);

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OcppDbContext>();

        var connectorStatus = await db.ConnectorStatuses
            .FirstOrDefaultAsync(cs => cs.ChargePointId == chargePointId && cs.ConnectorId == request.ConnectorId);

        if (connectorStatus == null)
        {
            connectorStatus = new ConnectorStatus
            {
                ChargePointId = chargePointId,
                ConnectorId = request.ConnectorId,
                Status = request.Status,
                ErrorCode = request.ErrorCode,
                UpdatedAt = DateTime.UtcNow
            };
            db.ConnectorStatuses.Add(connectorStatus);
        }
        else
        {
            connectorStatus.Status = request.Status;
            connectorStatus.ErrorCode = request.ErrorCode;
            connectorStatus.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        return new StatusNotificationResponse();
    }
}
