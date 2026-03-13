using System.Text.Json.Serialization;

namespace OcppEvCharging.Server.Models.Ocpp16;

public class StatusNotificationRequest
{
    [JsonPropertyName("connectorId")]
    public int ConnectorId { get; set; }

    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; set; } = "NoError";

    [JsonPropertyName("status")]
    public string Status { get; set; } = "Available";

    [JsonPropertyName("timestamp")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Timestamp { get; set; }

    [JsonPropertyName("info")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Info { get; set; }

    [JsonPropertyName("vendorId")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? VendorId { get; set; }

    [JsonPropertyName("vendorErrorCode")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? VendorErrorCode { get; set; }
}
