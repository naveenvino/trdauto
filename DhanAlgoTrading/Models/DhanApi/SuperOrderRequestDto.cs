using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class SuperOrderRequestDto
    {
        [JsonPropertyName("dhanClientId")]
        public string? DhanClientId { get; set; } // Will be set by DhanService

        [JsonPropertyName("correlationId")]
        public string? CorrelationId { get; set; }

        [JsonPropertyName("transactionType")] // "BUY" or "SELL"
        public string? TransactionType { get; set; }

        [JsonPropertyName("exchangeSegment")] // e.g., "NSE_EQ", "NSE_FNO"
        public string? ExchangeSegment { get; set; }

        [JsonPropertyName("productType")] // e.g., "CNC", "INTRADAY", "MARGIN", "MTF" (Super Orders might have specific allowed types)
        public string? ProductType { get; set; }

        [JsonPropertyName("orderType")] // For entry leg: "LIMIT", "MARKET"
        public string? OrderType { get; set; }

        [JsonPropertyName("securityId")]
        public string? SecurityId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")] // Entry price if orderType is "LIMIT"
        public decimal Price { get; set; } // Note: PDF shows this as required, even for MARKET in sample JSON, might be 0 for MARKET.

        [JsonPropertyName("targetPrice")]
        public decimal TargetPrice { get; set; }

        [JsonPropertyName("stopLossPrice")]
        public decimal StopLossPrice { get; set; }

        [JsonPropertyName("trailingJump")] // Price jump by which Stop Loss should be trailed (optional, can be 0 if no trailing)
        public decimal? TrailingJump { get; set; }
    }
}
