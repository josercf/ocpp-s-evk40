using System.Text.Json.Serialization;

namespace OcppEvCharging.Server.Models.Ocpp16;

public class StopTransactionRequest
{
    [JsonPropertyName("transactionId")]
    public int TransactionId { get; set; }

    [JsonPropertyName("idTag")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IdTag { get; set; }

    [JsonPropertyName("meterStop")]
    public int MeterStop { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Reason { get; set; }

    [JsonPropertyName("transactionData")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<MeterValue>? TransactionData { get; set; }
}
