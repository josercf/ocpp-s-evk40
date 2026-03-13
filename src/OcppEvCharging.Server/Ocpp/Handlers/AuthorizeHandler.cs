using System.Text.Json;
using OcppEvCharging.Server.Models.Enums;
using OcppEvCharging.Server.Models.Ocpp16;

namespace OcppEvCharging.Server.Ocpp.Handlers;

public class AuthorizeHandler
{
    private readonly ILogger<AuthorizeHandler> _logger;

    public AuthorizeHandler(ILogger<AuthorizeHandler> logger)
    {
        _logger = logger;
    }

    public Task<object> HandleAsync(string chargePointId, JsonElement payload)
    {
        var request = payload.Deserialize<AuthorizeRequest>();

        _logger.LogInformation("Authorize from {ChargePointId}: IdTag={IdTag}",
            chargePointId, request?.IdTag);

        // Accept all authorization requests
        object response = new AuthorizeResponse
        {
            IdTagInfo = new IdTagInfo
            {
                Status = AuthorizationStatus.Accepted
            }
        };

        return Task.FromResult(response);
    }
}
