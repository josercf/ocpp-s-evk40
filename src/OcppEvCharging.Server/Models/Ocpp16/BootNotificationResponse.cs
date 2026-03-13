using System.Text.Json.Serialization;
using OcppEvCharging.Server.Models.Enums;

namespace OcppEvCharging.Server.Models.Ocpp16;

public class BootNotificationResponse
{
    [JsonPropertyName("currentTime")]
    public string CurrentTime { get; set; } = DateTime.UtcNow.ToString("o");

    [JsonPropertyName("interval")]
    public int Interval { get; set; } = 300;

    [JsonPropertyName("status")]
    public RegistrationStatus Status { get; set; }
}
