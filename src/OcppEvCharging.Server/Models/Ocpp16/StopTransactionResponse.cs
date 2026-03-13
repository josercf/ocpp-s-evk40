using System.Text.Json.Serialization;

namespace OcppEvCharging.Server.Models.Ocpp16;

public class StopTransactionResponse
{
    [JsonPropertyName("idTagInfo")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IdTagInfo? IdTagInfo { get; set; }
}
