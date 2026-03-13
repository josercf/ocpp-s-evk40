using System.Text.Json.Serialization;

namespace OcppEvCharging.Server.Models.Ocpp16;

public class MeterValuesRequest
{
    [JsonPropertyName("connectorId")]
    public int ConnectorId { get; set; }

    [JsonPropertyName("transactionId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TransactionId { get; set; }

    [JsonPropertyName("meterValue")]
    public List<MeterValue> MeterValue { get; set; } = new();
}

public class MeterValue
{
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("sampledValue")]
    public List<SampledValue> SampledValue { get; set; } = new();
}

public class SampledValue
{
    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;

    [JsonPropertyName("context")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Context { get; set; }

    [JsonPropertyName("format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Format { get; set; }

    [JsonPropertyName("measurand")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Measurand { get; set; }

    [JsonPropertyName("phase")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Phase { get; set; }

    [JsonPropertyName("location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Location { get; set; }

    [JsonPropertyName("unit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Unit { get; set; }
}
