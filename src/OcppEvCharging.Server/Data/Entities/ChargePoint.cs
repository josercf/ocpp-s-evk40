using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OcppEvCharging.Server.Data.Entities;

[Table("charge_points")]
public class ChargePoint
{
    [Key]
    [Column("charge_point_id")]
    [MaxLength(64)]
    public string ChargePointId { get; set; } = string.Empty;

    [Column("vendor")]
    [MaxLength(64)]
    public string? Vendor { get; set; }

    [Column("model")]
    [MaxLength(64)]
    public string? Model { get; set; }

    [Column("serial_number")]
    [MaxLength(64)]
    public string? SerialNumber { get; set; }

    [Column("firmware_version")]
    [MaxLength(64)]
    public string? FirmwareVersion { get; set; }

    [Column("registered_at")]
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    [Column("last_heartbeat")]
    public DateTime? LastHeartbeat { get; set; }
}
