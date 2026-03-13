using Microsoft.EntityFrameworkCore;
using OcppEvCharging.Server.Data.Entities;

namespace OcppEvCharging.Server.Data;

public class OcppDbContext : DbContext
{
    public OcppDbContext(DbContextOptions<OcppDbContext> options) : base(options) { }

    public DbSet<ChargePoint> ChargePoints => Set<ChargePoint>();
    public DbSet<ChargingSession> ChargingSessions => Set<ChargingSession>();
    public DbSet<ConnectorStatus> ConnectorStatuses => Set<ConnectorStatus>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConnectorStatus>()
            .HasKey(cs => new { cs.ChargePointId, cs.ConnectorId });

        modelBuilder.Entity<ChargingSession>()
            .Property(s => s.TransactionId)
            .UseIdentityAlwaysColumn();
    }
}
