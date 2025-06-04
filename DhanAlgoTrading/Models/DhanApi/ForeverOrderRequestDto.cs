using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class ForeverOrderRequestDto
    {
        [JsonPropertyName("transactionType")] public string? TransactionType { get; set; }
        [JsonPropertyName("exchangeSegment")] public string? ExchangeSegment { get; set; }
        [JsonPropertyName("orderType")] public string? OrderType { get; set; }
        [JsonPropertyName("securityId")] public string? SecurityId { get; set; }
        [JsonPropertyName("quantity")] public int Quantity { get; set; }
        [JsonPropertyName("price")] public decimal? Price { get; set; }
        [JsonPropertyName("triggerPrice")] public decimal? TriggerPrice { get; set; }
        [JsonPropertyName("validity")] public string? Validity { get; set; }
    }
}
