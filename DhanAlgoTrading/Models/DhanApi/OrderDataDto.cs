using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class OrderDataDto
    {
        [JsonPropertyName("dhanClientId")]
        public string? DhanClientId { get; set; }

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("exchangeOrderId")]
        public string? ExchangeOrderId { get; set; }

        [JsonPropertyName("correlationId")]
        public string? CorrelationId { get; set; }

        [JsonPropertyName("orderStatus")] // e.g., "TRADING", "REJECTED", "CANCELLED", "FILLED", "PARTIALLY_FILLED", "PENDING"
        public string? OrderStatus { get; set; }

        [JsonPropertyName("transactionType")] // "BUY" or "SELL"
        public string? TransactionType { get; set; }

        [JsonPropertyName("exchangeSegment")]
        public string? ExchangeSegment { get; set; }

        [JsonPropertyName("productType")]
        public string? ProductType { get; set; }

        [JsonPropertyName("orderType")]
        public string? OrderType { get; set; }

        [JsonPropertyName("securityId")]
        public string? SecurityId { get; set; }

        [JsonPropertyName("tradingSymbol")] // Usually provided in order details
        public string? TradingSymbol { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("filledQuantity")]
        public int FilledQuantity { get; set; }

        [JsonPropertyName("pendingQuantity")]
        public int PendingQuantity { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; } // Order price

        [JsonPropertyName("averagePrice")]
        public decimal AveragePrice { get; set; } // Average fill price

        [JsonPropertyName("triggerPrice")]
        public decimal TriggerPrice { get; set; }

        [JsonPropertyName("validity")]
        public string? Validity { get; set; }

        [JsonPropertyName("message")] // Rejection message, etc.
        public string? Message { get; set; }

        [JsonPropertyName("exchangeTime")] // Timestamp from exchange
        public string? ExchangeTime { get; set; } // Or DateTime if format is consistent

        [JsonPropertyName("drvExpiryDate")]
        public string? DrvExpiryDate { get; set; }

        [JsonPropertyName("drvOptionType")]
        public string? DrvOptionType { get; set; }

        [JsonPropertyName("drvStrikePrice")]
        public decimal? DrvStrikePrice { get; set; }
    }
}
