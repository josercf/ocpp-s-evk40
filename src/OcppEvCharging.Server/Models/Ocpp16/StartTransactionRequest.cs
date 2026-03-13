using System.Text.Json.Serialization;

namespace OcppEvCharging.Server.Models.Ocpp16;

public class StartTransactionRequest
{
    [JsonPropertyName("connectorId")]
    public int ConnectorId { get; set; }

    [JsonPropertyName("idTag")]
    public string IdTag { get; set; } = string.Empty;

    [JsonPropertyName("meterStart")]
    public int MeterStart { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("reservationId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ReservationId { get; set; }
}
