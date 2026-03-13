using System.Text.Json.Serialization;

namespace OcppEvCharging.Server.Models.Ocpp16;

public class StartTransactionResponse
{
    [JsonPropertyName("transactionId")]
    public int TransactionId { get; set; }

    [JsonPropertyName("idTagInfo")]
    public IdTagInfo IdTagInfo { get; set; } = new();
}
