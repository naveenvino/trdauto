using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class HistoricalTradeDto
    {
        [JsonPropertyName("tradeDate")] public string? TradeDate { get; set; }
        [JsonPropertyName("exchangeSegment")] public string? ExchangeSegment { get; set; }
        [JsonPropertyName("securityId")] public string? SecurityId { get; set; }
        [JsonPropertyName("transactionType")] public string? TransactionType { get; set; }
        [JsonPropertyName("price")] public decimal Price { get; set; }
        [JsonPropertyName("quantity")] public int Quantity { get; set; }
    }
}
