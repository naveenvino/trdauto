using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class OrderRequestDto
    {
        [JsonPropertyName("dhanClientId")]
        public string? DhanClientId { get; set; } // Will be set by DhanService

        [JsonPropertyName("correlationId")]
        public string? CorrelationId { get; set; }

        [JsonPropertyName("transactionType")] // "BUY" or "SELL"
        public string? TransactionType { get; set; }

        [JsonPropertyName("exchangeSegment")] // e.g., "NSE_FNO", "NSE_EQ"
        public string? ExchangeSegment { get; set; }

        [JsonPropertyName("productType")] // e.g., "INTRADAY", "CNC", "MARGIN"
        public string? ProductType { get; set; }

        [JsonPropertyName("orderType")] // e.g., "LIMIT", "MARKET", "SL", "SLM"
        public string? OrderType { get; set; }

        [JsonPropertyName("securityId")] // Unique numeric ID of the instrument
        public string? SecurityId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; } // Required for LIMIT, SL-L. 0 for MARKET.

        [JsonPropertyName("triggerPrice")]
        public decimal? TriggerPrice { get; set; } // Required for SL, SL-M

        [JsonPropertyName("validity")] // "DAY", "IOC"
        public string? Validity { get; set; }

        [JsonPropertyName("disclosedQuantity")]
        public int? DisclosedQuantity { get; set; }

        [JsonPropertyName("afterMarketOrder")]
        public bool? AfterMarketOrder { get; set; } // Typically boolean: true or false

        [JsonPropertyName("amoTime")] // e.g., "09:00"
        public string? AmoTime { get; set; }

        [JsonPropertyName("boProfitValue")]
        public decimal? BoProfitValue { get; set; }

        [JsonPropertyName("boStopLossValue")]
        public decimal? BoStopLossValue { get; set; }

        [JsonPropertyName("drvExpiryDate")] // "YYYY-MM-DD"
        public string? DrvExpiryDate { get; set; }

        [JsonPropertyName("drvOptionType")] // "CALL" / "PUT"
        public string? DrvOptionType { get; set; }

        [JsonPropertyName("drvStrikePrice")]
        public decimal? DrvStrikePrice { get; set; }
    }
}
