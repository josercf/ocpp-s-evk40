using System.Text.Json.Serialization;

namespace OcppEvCharging.Server.Models.Ocpp16;

public class HeartbeatResponse
{
    [JsonPropertyName("currentTime")]
    public string CurrentTime { get; set; } = DateTime.UtcNow.ToString("o");
}
