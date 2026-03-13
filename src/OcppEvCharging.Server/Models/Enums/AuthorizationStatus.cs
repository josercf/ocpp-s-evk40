using System.Text.Json.Serialization;

namespace OcppEvCharging.Server.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AuthorizationStatus
{
    Accepted,
    Blocked,
    Expired,
    Invalid,
    ConcurrentTx
}
