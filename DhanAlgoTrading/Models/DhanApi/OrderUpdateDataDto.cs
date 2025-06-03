using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class OrderUpdateDataDto
    {
        [JsonPropertyName("Exchange")] public string? Exchange { get; set; }
        [JsonPropertyName("Segment")] public string? Segment { get; set; }
        [JsonPropertyName("Source")] public string? Source { get; set; }
        [JsonPropertyName("SecurityId")] public string? SecurityId { get; set; }
        [JsonPropertyName("ClientId")] public string? ClientId { get; set; }
        [JsonPropertyName("ExchOrderNo")] public string? ExchOrderNo { get; set; }
        [JsonPropertyName("OrderNo")] public string? OrderNo { get; set; }
        [JsonPropertyName("Product")] public string? Product { get; set; }
        [JsonPropertyName("TxnType")] public string? TxnType { get; set; }
        [JsonPropertyName("OrderType")] public string? OrderType { get; set; }
        [JsonPropertyName("Validity")] public string? Validity { get; set; }
        [JsonPropertyName("DiscQuantity")] public int DiscQuantity { get; set; }
        [JsonPropertyName("DiscQtyRem")] public int DiscQtyRem { get; set; }
        [JsonPropertyName("RemainingQuantity")] public int RemainingQuantity { get; set; }
        [JsonPropertyName("Quantity")] public int Quantity { get; set; }
        [JsonPropertyName("TradedQty")] public int TradedQty { get; set; }
        [JsonPropertyName("Price")] public decimal Price { get; set; }
        [JsonPropertyName("TriggerPrice")] public decimal TriggerPrice { get; set; }
        [JsonPropertyName("TradedPrice")] public decimal TradedPrice { get; set; }
        [JsonPropertyName("AvgTradedPrice")] public decimal AvgTradedPrice { get; set; }
        [JsonPropertyName("OffMktFlag")] public string? OffMktFlag { get; set; }
        [JsonPropertyName("OrderDateTime")] public string? OrderDateTime { get; set; }
        [JsonPropertyName("ExchOrderTime")] public string? ExchOrderTime { get; set; }
        [JsonPropertyName("LastUpdatedTime")] public string? LastUpdatedTime { get; set; }
        [JsonPropertyName("Remarks")] public string? Remarks { get; set; }
        [JsonPropertyName("MktType")] public string? MktType { get; set; }
        [JsonPropertyName("ReasonDescription")] public string? ReasonDescription { get; set; }
        [JsonPropertyName("LegNo")] public int LegNo { get; set; }
        [JsonPropertyName("Instrument")] public string? Instrument { get; set; }
        [JsonPropertyName("Symbol")] public string? Symbol { get; set; }
        [JsonPropertyName("ProductName")] public string? ProductName { get; set; }
        [JsonPropertyName("Status")] public string? Status { get; set; }
        [JsonPropertyName("LotSize")] public int LotSize { get; set; }
        [JsonPropertyName("StrikePrice")] public decimal StrikePrice { get; set; }
        [JsonPropertyName("ExpiryDate")] public string? ExpiryDate { get; set; }
        [JsonPropertyName("OptType")] public string? OptType { get; set; }
        [JsonPropertyName("DisplayName")] public string? DisplayName { get; set; }
        [JsonPropertyName("Isin")] public string? Isin { get; set; }
        [JsonPropertyName("Series")] public string? Series { get; set; }
        [JsonPropertyName("CorrelationId")] public string? CorrelationId { get; set; }
    }

    public class WebSocketOrderUpdateMessageDto
    {
        [JsonPropertyName("Data")]
        public OrderUpdateDataDto? Data { get; set; }

        [JsonPropertyName("Type")]
        public string? Type { get; set; }
    }
}
