using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OcppEvCharging.Server.Data.Entities;

[Table("connector_status")]
public class ConnectorStatus
{
    [Column("charge_point_id")]
    [MaxLength(64)]
    public string ChargePointId { get; set; } = string.Empty;

    [Column("connector_id")]
    public int ConnectorId { get; set; }

    [Column("status")]
    [MaxLength(32)]
    public string Status { get; set; } = "Available";

    [Column("error_code")]
    [MaxLength(32)]
    public string? ErrorCode { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
