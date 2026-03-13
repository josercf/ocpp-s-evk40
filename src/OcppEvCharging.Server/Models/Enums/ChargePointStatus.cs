using System.Text.Json.Serialization;

namespace OcppEvCharging.Server.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ChargePointStatus
{
    Available,
    Preparing,
    Charging,
    SuspendedEVSE,
    SuspendedEV,
    Finishing,
    Reserved,
    Unavailable,
    Faulted
}
