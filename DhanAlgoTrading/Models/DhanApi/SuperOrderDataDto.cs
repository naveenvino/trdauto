using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class SuperOrderDataDto
    {
        [JsonPropertyName("dhanClientId")]
        public string? DhanClientId { get; set; }

        [JsonPropertyName("orderId")] // Main entry leg order ID
        public string? OrderId { get; set; }

        [JsonPropertyName("correlationId")]
        public string? CorrelationId { get; set; }

        [JsonPropertyName("orderStatus")] // Overall status of the Super Order (entry leg)
        public string? OrderStatus { get; set; } // e.g., "PENDING", "TRADING", "CLOSED", "REJECTED"

        [JsonPropertyName("transactionType")]
        public string? TransactionType { get; set; }

        [JsonPropertyName("exchangeSegment")]
        public string? ExchangeSegment { get; set; }

        [JsonPropertyName("productType")]
        public string? ProductType { get; set; }

        [JsonPropertyName("orderType")] // Entry leg order type
        public string? OrderType { get; set; }

        [JsonPropertyName("validity")]
        public string? Validity { get; set; }

        [JsonPropertyName("tradingSymbol")]
        public string? TradingSymbol { get; set; }

        [JsonPropertyName("securityId")]
        public string? SecurityId { get; set; }

        [JsonPropertyName("quantity")] // Entry leg quantity
        public int Quantity { get; set; }

        [JsonPropertyName("remainingQuantity")] // For the entry leg
        public int RemainingQuantity { get; set; }

        [JsonPropertyName("ltp")] // Last Traded Price of the instrument
        public decimal Ltp { get; set; }

        [JsonPropertyName("price")] // Entry leg price
        public decimal Price { get; set; }

        [JsonPropertyName("afterMarketOrder")]
        public bool AfterMarketOrder { get; set; }

        [JsonPropertyName("legName")] // Should be "ENTRY_LEG" for the main part of SuperOrder
        public string? LegName { get; set; }

        [JsonPropertyName("exchangeOrderId")]
        public string? ExchangeOrderId { get; set; }

        [JsonPropertyName("createTime")]
        public string? CreateTime { get; set; }

        [JsonPropertyName("updateTime")]
        public string? UpdateTime { get; set; }

        [JsonPropertyName("exchangeTime")]
        public string? ExchangeTime { get; set; }

        [JsonPropertyName("omsErrorDescription")]
        public string? OmsErrorDescription { get; set; }

        [JsonPropertyName("averageTradedPrice")]
        public decimal AverageTradedPrice { get; set; }

        [JsonPropertyName("filledQty")]
        public int FilledQty { get; set; }

        [JsonPropertyName("legDetails")]
        public List<SuperOrderLegDetailDto>? LegDetails { get; set; }
    }
}
