using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OcppEvCharging.Server.Data;
using OcppEvCharging.Server.Data.Entities;
using OcppEvCharging.Server.Models.Enums;
using OcppEvCharging.Server.Models.Ocpp16;

namespace OcppEvCharging.Server.Ocpp.Handlers;

public class BootNotificationHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BootNotificationHandler> _logger;

    public BootNotificationHandler(IServiceProvider serviceProvider, ILogger<BootNotificationHandler> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<object> HandleAsync(string chargePointId, JsonElement payload)
    {
        var request = payload.Deserialize<BootNotificationRequest>();
        if (request == null)
            throw new OcppProtocolException("Invalid BootNotification payload");

        _logger.LogInformation(
            "BootNotification from {ChargePointId}: {Vendor} {Model} (FW: {Firmware}, SN: {Serial})",
            chargePointId, request.ChargePointVendor, request.ChargePointModel,
            request.FirmwareVersion, request.ChargePointSerialNumber);

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OcppDbContext>();

        var chargePoint = await db.ChargePoints.FindAsync(chargePointId);
        if (chargePoint == null)
        {
            chargePoint = new ChargePoint
            {
                ChargePointId = chargePointId,
                Vendor = request.ChargePointVendor,
                Model = request.ChargePointModel,
                SerialNumber = request.ChargePointSerialNumber,
                FirmwareVersion = request.FirmwareVersion,
                RegisteredAt = DateTime.UtcNow
            };
            db.ChargePoints.Add(chargePoint);
        }
        else
        {
            chargePoint.Vendor = request.ChargePointVendor;
            chargePoint.Model = request.ChargePointModel;
            chargePoint.SerialNumber = request.ChargePointSerialNumber;
            chargePoint.FirmwareVersion = request.FirmwareVersion;
        }

        await db.SaveChangesAsync();

        return new BootNotificationResponse
        {
            CurrentTime = DateTime.UtcNow.ToString("o"),
            Interval = 60,
            Status = RegistrationStatus.Accepted
        };
    }
}
