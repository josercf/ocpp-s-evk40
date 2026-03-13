using System.Text.Json.Serialization;
using OcppEvCharging.Server.Models.Enums;

namespace OcppEvCharging.Server.Models.Ocpp16;

public class IdTagInfo
{
    [JsonPropertyName("status")]
    public AuthorizationStatus Status { get; set; } = AuthorizationStatus.Accepted;

    [JsonPropertyName("expiryDate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ExpiryDate { get; set; }
}
