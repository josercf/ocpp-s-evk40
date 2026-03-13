using System.Text.Json;
using OcppEvCharging.Server.Ocpp;

namespace OcppEvCharging.Tests;

/// <summary>
/// Tests that simulate a full OCPP message round-trip:
/// charger sends CALL -> server parses -> creates CALLRESULT -> verify format
/// </summary>
public class ProtocolRoundTripTests
{
    [Fact]
    public void BootNotification_RoundTrip()
    {
        // Charger sends CALL
        var call = """[2,"boot-001","BootNotification",{"chargePointVendor":"SKEV","chargePointModel":"SKEV40","firmwareVersion":"1.0.0","chargePointSerialNumber":"SN12345"}]""";
        var msg = OcppMessage.Parse(call);

        Assert.Equal(OcppConstants.MessageTypeCall, msg.MessageTypeId);
        Assert.Equal("BootNotification", msg.Action);

        // Server creates response
        var response = OcppMessage.CreateCallResult(msg.UniqueId, new
        {
            status = "Accepted",
            currentTime = "2024-06-01T12:00:00Z",
            interval = 60
        });

        // Verify response is valid OCPP CALLRESULT
        var respMsg = OcppMessage.Parse(response);
        Assert.Equal(OcppConstants.MessageTypeCallResult, respMsg.MessageTypeId);
        Assert.Equal("boot-001", respMsg.UniqueId);
        Assert.Equal("Accepted", respMsg.Payload.GetProperty("status").GetString());
        Assert.Equal(60, respMsg.Payload.GetProperty("interval").GetInt32());
    }

    [Fact]
    public void Heartbeat_RoundTrip()
    {
        var call = """[2,"hb-001","Heartbeat",{}]""";
        var msg = OcppMessage.Parse(call);

        var response = OcppMessage.CreateCallResult(msg.UniqueId, new
        {
            currentTime = "2024-06-01T12:00:00Z"
        });

        var respMsg = OcppMessage.Parse(response);
        Assert.Equal(OcppConstants.MessageTypeCallResult, respMsg.MessageTypeId);
        Assert.Equal("hb-001", respMsg.UniqueId);
        Assert.NotNull(respMsg.Payload.GetProperty("currentTime").GetString());
    }

    [Fact]
    public void MeterValues_RoundTrip()
    {
        var call = """
        [2,"mv-001","MeterValues",{
            "connectorId":1,
            "transactionId":42,
            "meterValue":[{
                "timestamp":"2024-06-01T12:00:00Z",
                "sampledValue":[
                    {"value":"230.5","measurand":"Voltage","unit":"V","phase":"L1"},
                    {"value":"16.2","measurand":"Current.Import","unit":"A","phase":"L1"},
                    {"value":"3700","measurand":"Power.Active.Import","unit":"W"},
                    {"value":"15234","measurand":"Energy.Active.Import.Register","unit":"Wh"},
                    {"value":"35.5","measurand":"Temperature","unit":"Celsius"}
                ]
            }]
        }]
        """;

        var msg = OcppMessage.Parse(call);
        Assert.Equal("MeterValues", msg.Action);

        // Verify payload structure matches SKEV40 expected format
        Assert.Equal(1, msg.Payload.GetProperty("connectorId").GetInt32());
        Assert.Equal(42, msg.Payload.GetProperty("transactionId").GetInt32());

        var meterValues = msg.Payload.GetProperty("meterValue");
        var samples = meterValues[0].GetProperty("sampledValue");
        Assert.Equal(5, samples.GetArrayLength());

        // Server responds with empty object (per OCPP 1.6 spec)
        var response = OcppMessage.CreateCallResult(msg.UniqueId, new { });
        var respMsg = OcppMessage.Parse(response);
        Assert.Equal("mv-001", respMsg.UniqueId);
    }

    [Fact]
    public void StartTransaction_RoundTrip()
    {
        var call = """[2,"st-001","StartTransaction",{"connectorId":1,"idTag":"RFID001","meterStart":12345,"timestamp":"2024-06-01T10:00:00Z"}]""";
        var msg = OcppMessage.Parse(call);

        var response = OcppMessage.CreateCallResult(msg.UniqueId, new
        {
            transactionId = 1,
            idTagInfo = new { status = "Accepted" }
        });

        var respMsg = OcppMessage.Parse(response);
        Assert.Equal(1, respMsg.Payload.GetProperty("transactionId").GetInt32());
        Assert.Equal("Accepted", respMsg.Payload.GetProperty("idTagInfo").GetProperty("status").GetString());
    }

    [Fact]
    public void StopTransaction_RoundTrip()
    {
        var call = """[2,"sp-001","StopTransaction",{"transactionId":1,"meterStop":22345,"timestamp":"2024-06-01T14:00:00Z","reason":"Local"}]""";
        var msg = OcppMessage.Parse(call);

        Assert.Equal("StopTransaction", msg.Action);
        Assert.Equal(1, msg.Payload.GetProperty("transactionId").GetInt32());
        Assert.Equal(22345, msg.Payload.GetProperty("meterStop").GetInt32());

        var response = OcppMessage.CreateCallResult(msg.UniqueId, new
        {
            idTagInfo = new { status = "Accepted" }
        });

        var respMsg = OcppMessage.Parse(response);
        Assert.Equal("Accepted", respMsg.Payload.GetProperty("idTagInfo").GetProperty("status").GetString());
    }

    [Fact]
    public void ErrorResponse_RoundTrip()
    {
        var call = """[2,"unk-001","UnknownAction",{}]""";
        var msg = OcppMessage.Parse(call);

        var error = OcppMessage.CreateCallError(msg.UniqueId, "NotImplemented", "Unknown action: UnknownAction");
        var errMsg = OcppMessage.Parse(error);

        Assert.Equal(OcppConstants.MessageTypeCallError, errMsg.MessageTypeId);
        Assert.Equal("unk-001", errMsg.UniqueId);
        Assert.Equal("NotImplemented", errMsg.ErrorCode);
        Assert.Equal("Unknown action: UnknownAction", errMsg.ErrorDescription);
    }
}
