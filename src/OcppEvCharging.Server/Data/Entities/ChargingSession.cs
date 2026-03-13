using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OcppEvCharging.Server.Data.Entities;

[Table("charging_sessions")]
public class ChargingSession
{
    [Key]
    [Column("transaction_id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TransactionId { get; set; }

    [Column("charge_point_id")]
    [MaxLength(64)]
    public string ChargePointId { get; set; } = string.Empty;

    [Column("connector_id")]
    public int ConnectorId { get; set; }

    [Column("id_tag")]
    [MaxLength(20)]
    public string? IdTag { get; set; }

    [Column("start_time")]
    public DateTime StartTime { get; set; }

    [Column("stop_time")]
    public DateTime? StopTime { get; set; }

    [Column("meter_start")]
    public int MeterStart { get; set; }

    [Column("meter_stop")]
    public int? MeterStop { get; set; }

    [Column("energy_kwh")]
    public double? EnergyKwh { get; set; }

    [Column("stop_reason")]
    [MaxLength(32)]
    public string? StopReason { get; set; }
}
