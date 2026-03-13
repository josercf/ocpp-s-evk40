using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using OcppEvCharging.Server.Ocpp.Handlers;

namespace OcppEvCharging.Server.Ocpp;

public class OcppMessageRouter
{
    private readonly ChargePointConnectionManager _connectionManager;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OcppMessageRouter> _logger;

    public OcppMessageRouter(
        ChargePointConnectionManager connectionManager,
        IServiceProvider serviceProvider,
        ILogger<OcppMessageRouter> logger)
    {
        _connectionManager = connectionManager;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task HandleWebSocketAsync(HttpContext context, string chargePointId)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = 400;
            return;
        }

        // Validate OCPP subprotocol
        var requestedProtocols = context.WebSockets.WebSocketRequestedProtocols;
        string? selectedProtocol = null;

        if (requestedProtocols.Contains(OcppConstants.ProtocolOcpp16))
        {
            selectedProtocol = OcppConstants.ProtocolOcpp16;
        }
        else if (requestedProtocols.Count == 0)
        {
            // Some chargers don't send subprotocol - accept anyway
            _logger.LogWarning("ChargePoint {ChargePointId} connected without OCPP subprotocol", chargePointId);
            selectedProtocol = OcppConstants.ProtocolOcpp16;
        }
        else
        {
            _logger.LogWarning("ChargePoint {ChargePointId} requested unsupported protocols: {Protocols}",
                chargePointId, string.Join(", ", requestedProtocols));
            context.Response.StatusCode = 400;
            return;
        }

        using var webSocket = await context.WebSockets.AcceptWebSocketAsync(selectedProtocol);
        var connection = new ChargePointConnection(chargePointId, webSocket);
        _connectionManager.Add(connection);

        _logger.LogInformation("ChargePoint {ChargePointId} connected", chargePointId);

        try
        {
            await ProcessMessagesAsync(connection);
        }
        catch (WebSocketException ex)
        {
            _logger.LogWarning(ex, "WebSocket error for {ChargePointId}", chargePointId);
        }
        finally
        {
            _connectionManager.Remove(chargePointId);
            _logger.LogInformation("ChargePoint {ChargePointId} disconnected", chargePointId);

            if (webSocket.State == WebSocketState.Open)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server closing", CancellationToken.None);
            }
        }
    }

    private async Task ProcessMessagesAsync(ChargePointConnection connection)
    {
        var buffer = new byte[8192];
        var messageBuffer = new MemoryStream();

        while (connection.WebSocket.State == WebSocketState.Open)
        {
            messageBuffer.SetLength(0);

            WebSocketReceiveResult result;
            do
            {
                result = await connection.WebSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                    return;

                messageBuffer.Write(buffer, 0, result.Count);
            }
            while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var rawMessage = Encoding.UTF8.GetString(messageBuffer.ToArray());
                connection.LastMessageAt = DateTime.UtcNow;

                _logger.LogDebug("Received from {ChargePointId}: {Message}",
                    connection.ChargePointId, rawMessage);

                await HandleMessageAsync(connection, rawMessage);
            }
        }
    }

    private async Task HandleMessageAsync(ChargePointConnection connection, string rawMessage)
    {
        OcppMessage message;
        try
        {
            message = OcppMessage.Parse(rawMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse OCPP message from {ChargePointId}: {Message}",
                connection.ChargePointId, rawMessage);
            return;
        }

        if (message.MessageTypeId != OcppConstants.MessageTypeCall)
        {
            _logger.LogDebug("Received non-CALL message (type {Type}) from {ChargePointId}",
                message.MessageTypeId, connection.ChargePointId);
            return;
        }

        string response;
        try
        {
            var result = await DispatchAsync(connection.ChargePointId, message.Action!, message.Payload);
            response = OcppMessage.CreateCallResult(message.UniqueId, result);
        }
        catch (OcppProtocolException ex)
        {
            _logger.LogError(ex, "Protocol error handling {Action} from {ChargePointId}",
                message.Action, connection.ChargePointId);
            response = OcppMessage.CreateCallError(
                message.UniqueId,
                OcppConstants.ErrorCodes.FormationViolation,
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling {Action} from {ChargePointId}",
                message.Action, connection.ChargePointId);
            response = OcppMessage.CreateCallError(
                message.UniqueId,
                OcppConstants.ErrorCodes.InternalError,
                "Internal server error");
        }

        _logger.LogDebug("Sending to {ChargePointId}: {Response}",
            connection.ChargePointId, response);

        var responseBytes = Encoding.UTF8.GetBytes(response);
        await connection.WebSocket.SendAsync(
            new ArraySegment<byte>(responseBytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    private async Task<object> DispatchAsync(string chargePointId, string action, JsonElement payload)
    {
        return action switch
        {
            OcppConstants.Actions.BootNotification =>
                await _serviceProvider.GetRequiredService<BootNotificationHandler>()
                    .HandleAsync(chargePointId, payload),

            OcppConstants.Actions.Heartbeat =>
                await _serviceProvider.GetRequiredService<HeartbeatHandler>()
                    .HandleAsync(chargePointId, payload),

            OcppConstants.Actions.Authorize =>
                await _serviceProvider.GetRequiredService<AuthorizeHandler>()
                    .HandleAsync(chargePointId, payload),

            OcppConstants.Actions.StatusNotification =>
                await _serviceProvider.GetRequiredService<StatusNotificationHandler>()
                    .HandleAsync(chargePointId, payload),

            OcppConstants.Actions.MeterValues =>
                await _serviceProvider.GetRequiredService<MeterValuesHandler>()
                    .HandleAsync(chargePointId, payload),

            OcppConstants.Actions.StartTransaction =>
                await _serviceProvider.GetRequiredService<StartTransactionHandler>()
                    .HandleAsync(chargePointId, payload),

            OcppConstants.Actions.StopTransaction =>
                await _serviceProvider.GetRequiredService<StopTransactionHandler>()
                    .HandleAsync(chargePointId, payload),

            // Accept but ignore these common messages
            OcppConstants.Actions.DataTransfer or
            OcppConstants.Actions.DiagnosticsStatusNotification or
            OcppConstants.Actions.FirmwareStatusNotification =>
                HandleNotImplemented(chargePointId, action),

            _ => throw new OcppProtocolException($"Unknown action: {action}")
        };
    }

    private object HandleNotImplemented(string chargePointId, string action)
    {
        _logger.LogDebug("Received unhandled action {Action} from {ChargePointId} - returning empty response",
            action, chargePointId);
        return new { };
    }
}
