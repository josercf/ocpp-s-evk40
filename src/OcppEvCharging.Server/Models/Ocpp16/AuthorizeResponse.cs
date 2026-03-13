using System.Text.Json.Serialization;

namespace OcppEvCharging.Server.Models.Ocpp16;

public class AuthorizeResponse
{
    [JsonPropertyName("idTagInfo")]
    public IdTagInfo IdTagInfo { get; set; } = new();
}
