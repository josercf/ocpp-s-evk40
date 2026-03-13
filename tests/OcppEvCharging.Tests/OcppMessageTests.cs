using System.Text.Json;
using OcppEvCharging.Server.Ocpp;

namespace OcppEvCharging.Tests;

public class OcppMessageTests
{
    [Fact]
    public void Parse_BootNotificationCall_ParsesCorrectly()
    {
        var raw = """[2,"19223201","BootNotification",{"chargePointVendor":"SKEV","chargePointModel":"SKEV40"}]""";

        var msg = OcppMessage.Parse(raw);

        Assert.Equal(OcppConstants.MessageTypeCall, msg.MessageTypeId);
        Assert.Equal("19223201", msg.UniqueId);
        Assert.Equal("BootNotification", msg.Action);
        Assert.Equal("SKEV", msg.Payload.GetProperty("chargePointVendor").GetString());
        Assert.Equal("SKEV40", msg.Payload.GetProperty("chargePointModel").GetString());
    }

    [Fact]
    public void Parse_HeartbeatCall_ParsesCorrectly()
    {
        var raw = """[2,"abc123","Heartbeat",{}]""";

        var msg = OcppMessage.Parse(raw);

        Assert.Equal(OcppConstants.MessageTypeCall, msg.MessageTypeId);
        Assert.Equal("abc123", msg.UniqueId);
        Assert.Equal("Heartbeat", msg.Action);
    }

    [Fact]
    public void Parse_CallResult_ParsesCorrectly()
    {
        var raw = """[3,"19223201",{"status":"Accepted","currentTime":"2024-01-01T00:00:00Z","interval":60}]""";

        var msg = OcppMessage.Parse(raw);

        Assert.Equal(OcppConstants.MessageTypeCallResult, msg.MessageTypeId);
        Assert.Equal("19223201", msg.UniqueId);
        Assert.Null(msg.Action);
        Assert.Equal("Accepted", msg.Payload.GetProperty("status").GetString());
    }

    [Fact]
    public void Parse_CallError_ParsesCorrectly()
    {
        var raw = """[4,"19223201","NotImplemented","Unknown action",{}]""";

        var msg = OcppMessage.Parse(raw);

        Assert.Equal(OcppConstants.MessageTypeCallError, msg.MessageTypeId);
        Assert.Equal("19223201", msg.UniqueId);
        Assert.Equal("NotImplemented", msg.ErrorCode);
        Assert.Equal("Unknown action", msg.ErrorDescription);
    }

    [Fact]
    public void Parse_InvalidJson_ThrowsException()
    {
        Assert.Throws<JsonException>(() => OcppMessage.Parse("not json"));
    }

    [Fact]
    public void Parse_NotArray_ThrowsOcppProtocolException()
    {
        Assert.Throws<OcppProtocolException>(() => OcppMessage.Parse("""{"key":"value"}"""));
    }

    [Fact]
    public void Parse_UnknownMessageType_ThrowsOcppProtocolException()
    {
        Assert.Throws<OcppProtocolException>(() => OcppMessage.Parse("""[9,"id","Action",{}]"""));
    }

    [Fact]
    public void CreateCallResult_FormatsCorrectly()
    {
        var result = OcppMessage.CreateCallResult("uid-123", new { status = "Accepted", interval = 60 });
        var parsed = JsonSerializer.Deserialize<JsonElement>(result);

        Assert.Equal(JsonValueKind.Array, parsed.ValueKind);
        Assert.Equal(3, parsed[0].GetInt32()); // CALLRESULT
        Assert.Equal("uid-123", parsed[1].GetString());
        Assert.Equal("Accepted", parsed[2].GetProperty("status").GetString());
        Assert.Equal(60, parsed[2].GetProperty("interval").GetInt32());
    }

    [Fact]
    public void CreateCallError_FormatsCorrectly()
    {
        var result = OcppMessage.CreateCallError("uid-456", "InternalError", "Something went wrong");
        var parsed = JsonSerializer.Deserialize<JsonElement>(result);

        Assert.Equal(JsonValueKind.Array, parsed.ValueKind);
        Assert.Equal(4, parsed[0].GetInt32()); // CALLERROR
        Assert.Equal("uid-456", parsed[1].GetString());
        Assert.Equal("InternalError", parsed[2].GetString());
        Assert.Equal("Something went wrong", parsed[3].GetString());
    }

    [Fact]
    public void Parse_MeterValuesCall_ParsesNestedPayload()
    {
        var raw = """
        [2,"mv-001","MeterValues",{
            "connectorId":1,
            "transactionId":42,
            "meterValue":[{
                "timestamp":"2024-06-01T12:00:00Z",
                "sampledValue":[
                    {"value":"230.5","measurand":"Voltage","unit":"V","phase":"L1"},
                    {"value":"16.2","measurand":"Current.Import","unit":"A","phase":"L1"},
                    {"value":"3700","measurand":"Power.Active.Import","unit":"W"}
                ]
            }]
        }]
        """;

        var msg = OcppMessage.Parse(raw);

        Assert.Equal("MeterValues", msg.Action);
        Assert.Equal(1, msg.Payload.GetProperty("connectorId").GetInt32());
        Assert.Equal(42, msg.Payload.GetProperty("transactionId").GetInt32());

        var meterValues = msg.Payload.GetProperty("meterValue");
        Assert.Equal(1, meterValues.GetArrayLength());

        var sampledValues = meterValues[0].GetProperty("sampledValue");
        Assert.Equal(3, sampledValues.GetArrayLength());
        Assert.Equal("230.5", sampledValues[0].GetProperty("value").GetString());
    }

    [Fact]
    public void Parse_StartTransactionCall_ParsesCorrectly()
    {
        var raw = """[2,"st-001","StartTransaction",{"connectorId":1,"idTag":"RFID001","meterStart":12345,"timestamp":"2024-06-01T10:00:00Z"}]""";

        var msg = OcppMessage.Parse(raw);

        Assert.Equal("StartTransaction", msg.Action);
        Assert.Equal(1, msg.Payload.GetProperty("connectorId").GetInt32());
        Assert.Equal("RFID001", msg.Payload.GetProperty("idTag").GetString());
        Assert.Equal(12345, msg.Payload.GetProperty("meterStart").GetInt32());
    }

    [Fact]
    public void Parse_StatusNotificationCall_ParsesCorrectly()
    {
        var raw = """[2,"sn-001","StatusNotification",{"connectorId":1,"errorCode":"NoError","status":"Available"}]""";

        var msg = OcppMessage.Parse(raw);

        Assert.Equal("StatusNotification", msg.Action);
        Assert.Equal("Available", msg.Payload.GetProperty("status").GetString());
        Assert.Equal("NoError", msg.Payload.GetProperty("errorCode").GetString());
    }
}
