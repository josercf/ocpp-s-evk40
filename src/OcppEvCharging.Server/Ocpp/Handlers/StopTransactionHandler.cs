using System.Globalization;
using System.Text.Json;
using OcppEvCharging.Server.Data;
using OcppEvCharging.Server.Data.Entities;
using OcppEvCharging.Server.Models.Enums;
using OcppEvCharging.Server.Models.Ocpp16;

namespace OcppEvCharging.Server.Ocpp.Handlers;

public class StopTransactionHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSeriesRepository _timeSeriesRepo;
    private readonly ILogger<StopTransactionHandler> _logger;

    public StopTransactionHandler(
        IServiceProvider serviceProvider,
        TimeSeriesRepository timeSeriesRepo,
        ILogger<StopTransactionHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _timeSeriesRepo = timeSeriesRepo;
        _logger = logger;
    }

    public async Task<object> HandleAsync(string chargePointId, JsonElement payload)
    {
        var request = payload.Deserialize<StopTransactionRequest>();
        if (request == null)
            throw new OcppProtocolException("Invalid StopTransaction payload");

        if (!DateTime.TryParse(request.Timestamp, CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var stopTime))
        {
            stopTime = DateTime.UtcNow;
        }

        _logger.LogInformation(
            "StopTransaction from {ChargePointId}: TransactionId={TransactionId}, MeterStop={MeterStop}, Reason={Reason}",
            chargePointId, request.TransactionId, request.MeterStop, request.Reason);

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OcppDbContext>();

        var session = await db.ChargingSessions.FindAsync(request.TransactionId);
        if (session != null)
        {
            session.StopTime = stopTime;
            session.MeterStop = request.MeterStop;
            session.StopReason = request.Reason;
            session.EnergyKwh = (request.MeterStop - session.MeterStart) / 1000.0;

            await db.SaveChangesAsync();

            _logger.LogInformation(
                "Transaction {TransactionId} completed: {EnergyKwh:F2} kWh, Duration: {Duration}",
                session.TransactionId, session.EnergyKwh,
                session.StopTime - session.StartTime);
        }
        else
        {
            _logger.LogWarning("Transaction {TransactionId} not found", request.TransactionId);
        }

        // Process any transaction data (final meter values)
        if (request.TransactionData != null)
        {
            var records = new List<MeterValueRecord>();
            foreach (var meterValue in request.TransactionData)
            {
                if (!DateTime.TryParse(meterValue.Timestamp, CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var timestamp))
                {
                    timestamp = DateTime.UtcNow;
                }

                foreach (var sample in meterValue.SampledValue)
                {
                    if (!double.TryParse(sample.Value, CultureInfo.InvariantCulture, out var value))
                        continue;

                    records.Add(new MeterValueRecord(
                        Timestamp: timestamp,
                        ChargePointId: chargePointId,
                        ConnectorId: session?.ConnectorId ?? 1,
                        TransactionId: request.TransactionId,
                        Measurand: sample.Measurand ?? "Energy.Active.Import.Register",
                        Value: value,
                        Unit: sample.Unit,
                        Phase: sample.Phase));
                }
            }

            if (records.Count > 0)
            {
                await _timeSeriesRepo.InsertMeterValuesBatchAsync(records);
            }
        }

        return new StopTransactionResponse
        {
            IdTagInfo = new IdTagInfo
            {
                Status = AuthorizationStatus.Accepted
            }
        };
    }
}
