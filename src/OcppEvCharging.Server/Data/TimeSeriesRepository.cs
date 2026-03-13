using Npgsql;

namespace OcppEvCharging.Server.Data;

public class TimeSeriesRepository
{
    private readonly string _connectionString;
    private readonly ILogger<TimeSeriesRepository> _logger;

    public TimeSeriesRepository(IConfiguration configuration, ILogger<TimeSeriesRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("TimescaleDb")
            ?? throw new InvalidOperationException("TimescaleDb connection string is required");
        _logger = logger;
    }

    public async Task InsertMeterValueAsync(
        DateTime timestamp,
        string chargePointId,
        int connectorId,
        int? transactionId,
        string measurand,
        double value,
        string? unit,
        string? phase)
    {
        const string sql = @"
            INSERT INTO meter_values (time, charge_point_id, connector_id, transaction_id, measurand, value, unit, phase)
            VALUES (@time, @chargePointId, @connectorId, @transactionId, @measurand, @value, @unit, @phase)";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("time", timestamp);
        cmd.Parameters.AddWithValue("chargePointId", chargePointId);
        cmd.Parameters.AddWithValue("connectorId", connectorId);
        cmd.Parameters.AddWithValue("transactionId", (object?)transactionId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("measurand", measurand);
        cmd.Parameters.AddWithValue("value", value);
        cmd.Parameters.AddWithValue("unit", (object?)unit ?? DBNull.Value);
        cmd.Parameters.AddWithValue("phase", (object?)phase ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task InsertMeterValuesBatchAsync(List<MeterValueRecord> records)
    {
        if (records.Count == 0) return;

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var writer = await conn.BeginBinaryImportAsync(
            "COPY meter_values (time, charge_point_id, connector_id, transaction_id, measurand, value, unit, phase) FROM STDIN (FORMAT BINARY)");

        foreach (var record in records)
        {
            await writer.StartRowAsync();
            await writer.WriteAsync(record.Timestamp, NpgsqlTypes.NpgsqlDbType.TimestampTz);
            await writer.WriteAsync(record.ChargePointId, NpgsqlTypes.NpgsqlDbType.Varchar);
            await writer.WriteAsync(record.ConnectorId, NpgsqlTypes.NpgsqlDbType.Integer);

            if (record.TransactionId.HasValue)
                await writer.WriteAsync(record.TransactionId.Value, NpgsqlTypes.NpgsqlDbType.Integer);
            else
                await writer.WriteNullAsync();

            await writer.WriteAsync(record.Measurand, NpgsqlTypes.NpgsqlDbType.Varchar);
            await writer.WriteAsync(record.Value, NpgsqlTypes.NpgsqlDbType.Double);

            if (record.Unit != null)
                await writer.WriteAsync(record.Unit, NpgsqlTypes.NpgsqlDbType.Varchar);
            else
                await writer.WriteNullAsync();

            if (record.Phase != null)
                await writer.WriteAsync(record.Phase, NpgsqlTypes.NpgsqlDbType.Varchar);
            else
                await writer.WriteNullAsync();
        }

        await writer.CompleteAsync();

        _logger.LogDebug("Inserted {Count} meter values via binary copy", records.Count);
    }
}

public record MeterValueRecord(
    DateTime Timestamp,
    string ChargePointId,
    int ConnectorId,
    int? TransactionId,
    string Measurand,
    double Value,
    string? Unit,
    string? Phase);
