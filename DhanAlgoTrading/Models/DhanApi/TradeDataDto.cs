using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class TradeDataDto
    {
        [JsonPropertyName("dhanClientId")]
        public string? DhanClientId { get; set; }

        [JsonPropertyName("orderId")] // The original order ID this trade belongs to
        public string? OrderId { get; set; }

        [JsonPropertyName("exchangeOrderId")]
        public string? ExchangeOrderId { get; set; }

        [JsonPropertyName("tradeId")] // Unique ID for this specific trade/fill
        public string? TradeId { get; set; }

        [JsonPropertyName("exchangeSegment")]
        public string? ExchangeSegment { get; set; }

        [JsonPropertyName("productType")]
        public string? ProductType { get; set; }

        [JsonPropertyName("transactionType")] // "BUY" or "SELL"
        public string? TransactionType { get; set; }

        [JsonPropertyName("securityId")]
        public string? SecurityId { get; set; }

        [JsonPropertyName("tradingSymbol")]
        public string? TradingSymbol { get; set; }

        [JsonPropertyName("orderType")] // The type of the original order
        public string? OrderType { get; set; }

        [JsonPropertyName("quantity")] // Quantity filled in this trade
        public int Quantity { get; set; }

        [JsonPropertyName("price")] // Price at which this trade was executed
        public decimal Price { get; set; }

        [JsonPropertyName("tradeTime")] // Timestamp of the trade execution
        public string? TradeTime { get; set; } // Or DateTime if format is consistent and parsable
    }
}
