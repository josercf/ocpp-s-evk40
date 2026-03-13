using System.Globalization;
using System.Text.Json;
using OcppEvCharging.Server.Data;
using OcppEvCharging.Server.Models.Ocpp16;

namespace OcppEvCharging.Server.Ocpp.Handlers;

public class MeterValuesHandler
{
    private readonly TimeSeriesRepository _timeSeriesRepo;
    private readonly ILogger<MeterValuesHandler> _logger;

    public MeterValuesHandler(TimeSeriesRepository timeSeriesRepo, ILogger<MeterValuesHandler> logger)
    {
        _timeSeriesRepo = timeSeriesRepo;
        _logger = logger;
    }

    public async Task<object> HandleAsync(string chargePointId, JsonElement payload)
    {
        var request = payload.Deserialize<MeterValuesRequest>();
        if (request == null)
            throw new OcppProtocolException("Invalid MeterValues payload");

        var records = new List<MeterValueRecord>();

        foreach (var meterValue in request.MeterValue)
        {
            if (!DateTime.TryParse(meterValue.Timestamp, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var timestamp))
            {
                timestamp = DateTime.UtcNow;
            }

            foreach (var sample in meterValue.SampledValue)
            {
                if (!double.TryParse(sample.Value, CultureInfo.InvariantCulture, out var value))
                {
                    _logger.LogWarning("Could not parse meter value: {Value}", sample.Value);
                    continue;
                }

                var measurand = sample.Measurand ?? "Energy.Active.Import.Register";

                records.Add(new MeterValueRecord(
                    Timestamp: timestamp,
                    ChargePointId: chargePointId,
                    ConnectorId: request.ConnectorId,
                    TransactionId: request.TransactionId,
                    Measurand: measurand,
                    Value: value,
                    Unit: sample.Unit,
                    Phase: sample.Phase));

                _logger.LogDebug(
                    "MeterValue from {ChargePointId}: {Measurand}={Value}{Unit} (Connector={ConnectorId}, Tx={TransactionId})",
                    chargePointId, measurand, value, sample.Unit,
                    request.ConnectorId, request.TransactionId);
            }
        }

        if (records.Count > 0)
        {
            await _timeSeriesRepo.InsertMeterValuesBatchAsync(records);
        }

        _logger.LogInformation("Processed {Count} meter values from {ChargePointId}",
            records.Count, chargePointId);

        return new MeterValuesResponse();
    }
}
