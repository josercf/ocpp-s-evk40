using System.Text.Json.Serialization;

namespace OcppEvCharging.Server.Models.Ocpp16;

public class AuthorizeRequest
{
    [JsonPropertyName("idTag")]
    public string IdTag { get; set; } = string.Empty;
}
