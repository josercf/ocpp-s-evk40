using Microsoft.EntityFrameworkCore;
using OcppEvCharging.Server.Data;
using OcppEvCharging.Server.Ocpp;
using OcppEvCharging.Server.Ocpp.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to listen on port 6580
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(6580);
});

// Database - EF Core with PostgreSQL (same TimescaleDB instance)
builder.Services.AddDbContext<OcppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TimescaleDb")));

// Time series repository (direct Npgsql for hypertable writes)
builder.Services.AddSingleton<TimeSeriesRepository>();

// OCPP connection manager (singleton - tracks all connected charge points)
builder.Services.AddSingleton<ChargePointConnectionManager>();

// OCPP message router
builder.Services.AddSingleton<OcppMessageRouter>();

// OCPP message handlers
builder.Services.AddSingleton<BootNotificationHandler>();
builder.Services.AddSingleton<HeartbeatHandler>();
builder.Services.AddSingleton<AuthorizeHandler>();
builder.Services.AddSingleton<StatusNotificationHandler>();
builder.Services.AddSingleton<MeterValuesHandler>();
builder.Services.AddSingleton<StartTransactionHandler>();
builder.Services.AddSingleton<StopTransactionHandler>();

var app = builder.Build();

// Enable WebSockets
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

// OCPP WebSocket endpoint: /ocpp/{chargePointId}
app.Map("/ocpp/{chargePointId}", async (HttpContext context, string chargePointId) =>
{
    var router = context.RequestServices.GetRequiredService<OcppMessageRouter>();
    await router.HandleWebSocketAsync(context, chargePointId);
});

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Status endpoint - list connected charge points
app.MapGet("/status", (ChargePointConnectionManager connectionManager) =>
{
    var connections = connectionManager.GetAll().Select(c => new
    {
        c.ChargePointId,
        c.ConnectedAt,
        c.LastMessageAt
    });
    return Results.Ok(connections);
});

// Apply EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OcppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Could not apply migrations (database may not be ready yet). Tables should be created by init.sql");
    }
}

app.Logger.LogInformation("OCPP CSMS Server starting on port 6580");
app.Logger.LogInformation("WebSocket endpoint: ws://0.0.0.0:6580/ocpp/{{chargePointId}}");

app.Run();
