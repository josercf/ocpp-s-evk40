using System.Text.Json;

namespace OcppEvCharging.Server.Ocpp;

public class OcppMessage
{
    public int MessageTypeId { get; set; }
    public string UniqueId { get; set; } = string.Empty;
    public string? Action { get; set; }
    public JsonElement Payload { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorDescription { get; set; }

    public static OcppMessage Parse(string rawMessage)
    {
        var array = JsonSerializer.Deserialize<JsonElement>(rawMessage);

        if (array.ValueKind != JsonValueKind.Array)
            throw new OcppProtocolException("Message must be a JSON array");

        var messageTypeId = array[0].GetInt32();

        return messageTypeId switch
        {
            OcppConstants.MessageTypeCall => new OcppMessage
            {
                MessageTypeId = messageTypeId,
                UniqueId = array[1].GetString()!,
                Action = array[2].GetString(),
                Payload = array[3]
            },
            OcppConstants.MessageTypeCallResult => new OcppMessage
            {
                MessageTypeId = messageTypeId,
                UniqueId = array[1].GetString()!,
                Payload = array[2]
            },
            OcppConstants.MessageTypeCallError => new OcppMessage
            {
                MessageTypeId = messageTypeId,
                UniqueId = array[1].GetString()!,
                ErrorCode = array[2].GetString(),
                ErrorDescription = array[3].GetString(),
                Payload = array.GetArrayLength() > 4 ? array[4] : default
            },
            _ => throw new OcppProtocolException($"Unknown message type: {messageTypeId}")
        };
    }

    public static string CreateCallResult(string uniqueId, object payload)
    {
        var json = JsonSerializer.Serialize(new object[]
        {
            OcppConstants.MessageTypeCallResult,
            uniqueId,
            payload
        });
        return json;
    }

    public static string CreateCallError(string uniqueId, string errorCode, string errorDescription)
    {
        var json = JsonSerializer.Serialize(new object[]
        {
            OcppConstants.MessageTypeCallError,
            uniqueId,
            errorCode,
            errorDescription,
            new { }
        });
        return json;
    }
}

public class OcppProtocolException : Exception
{
    public OcppProtocolException(string message) : base(message) { }
}
