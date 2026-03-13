using System.Globalization;
using System.Text.Json;
using OcppEvCharging.Server.Data;
using OcppEvCharging.Server.Data.Entities;
using OcppEvCharging.Server.Models.Enums;
using OcppEvCharging.Server.Models.Ocpp16;

namespace OcppEvCharging.Server.Ocpp.Handlers;

public class StartTransactionHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StartTransactionHandler> _logger;

    public StartTransactionHandler(IServiceProvider serviceProvider, ILogger<StartTransactionHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<object> HandleAsync(string chargePointId, JsonElement payload)
    {
        var request = payload.Deserialize<StartTransactionRequest>();
        if (request == null)
            throw new OcppProtocolException("Invalid StartTransaction payload");

        if (!DateTime.TryParse(request.Timestamp, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var startTime))
        {
            startTime = DateTime.UtcNow;
        }

        _logger.LogInformation(
            "StartTransaction from {ChargePointId}: Connector={ConnectorId}, IdTag={IdTag}, MeterStart={MeterStart}",
            chargePointId, request.ConnectorId, request.IdTag, request.MeterStart);

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OcppDbContext>();

        var session = new ChargingSession
        {
            ChargePointId = chargePointId,
            ConnectorId = request.ConnectorId,
            IdTag = request.IdTag,
            StartTime = startTime,
            MeterStart = request.MeterStart
        };

        db.ChargingSessions.Add(session);
        await db.SaveChangesAsync();

        _logger.LogInformation("Created transaction {TransactionId} for {ChargePointId}",
            session.TransactionId, chargePointId);

        return new StartTransactionResponse
        {
            TransactionId = session.TransactionId,
            IdTagInfo = new IdTagInfo
            {
                Status = AuthorizationStatus.Accepted
            }
        };
    }
}
