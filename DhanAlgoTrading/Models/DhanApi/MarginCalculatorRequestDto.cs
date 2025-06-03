using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class MarginCalculatorRequestDto
    {
        [JsonPropertyName("dhanClientId")]
        public string? DhanClientId { get; set; } // Will be set by DhanService

        [JsonPropertyName("exchangeSegment")] // e.g., "NSE_EQ", "NSE_FNO"
        public string? ExchangeSegment { get; set; }

        [JsonPropertyName("transactionType")] // "BUY" or "SELL"
        public string? TransactionType { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("productType")] // e.g., "CNC", "INTRADAY", "MARGIN"
        public string? ProductType { get; set; }

        [JsonPropertyName("securityId")]
        public string? SecurityId { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; } // Price at which order is placed (even for margin calc)

        [JsonPropertyName("triggerPrice")] // Conditionally required for SL-M & SL-L
        public decimal? TriggerPrice { get; set; }
    }
}
