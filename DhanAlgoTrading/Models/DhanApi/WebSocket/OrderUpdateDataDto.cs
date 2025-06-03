using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Api.Models.DhanApi.WebSocket
{
    public class OrderUpdateDataDto // Corresponds to the "Data" object in the WebSocket message
    {
        [JsonPropertyName("Exchange")]
        public string? Exchange { get; set; }

        [JsonPropertyName("Segment")]
        public string? Segment { get; set; }

        [JsonPropertyName("Source")]
        public string? Source { get; set; }

        [JsonPropertyName("SecurityId")]
        public string? SecurityId { get; set; }

        [JsonPropertyName("ClientId")]
        public string? ClientId { get; set; }

        [JsonPropertyName("ExchOrderNo")]
        public string? ExchOrderNo { get; set; }

        [JsonPropertyName("OrderNo")] // This is Dhan's Order ID
        public string? OrderNo { get; set; }

        [JsonPropertyName("Product")] // e.g., "C" for CNC, "I" for INTRADAY
        public string? Product { get; set; }

        [JsonPropertyName("TxnType")] // "B" for Buy, "S" for Sell
        public string? TxnType { get; set; }

        [JsonPropertyName("OrderType")] // "LMT", "MKT", "SL", "SLM"
        public string? OrderType { get; set; }

        [JsonPropertyName("Validity")] // "DAY", "IOC"
        public string? Validity { get; set; }

        [JsonPropertyName("DiscQuantity")]
        public int DiscQuantity { get; set; }

        [JsonPropertyName("DiscQtyRem")]
        public int DiscQtyRem { get; set; }

        [JsonPropertyName("RemainingQuantity")]
        public int RemainingQuantity { get; set; }

        [JsonPropertyName("Quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("TradedQty")]
        public int TradedQty { get; set; }

        [JsonPropertyName("Price")]
        public decimal Price { get; set; }

        [JsonPropertyName("TriggerPrice")]
        public decimal TriggerPrice { get; set; }

        [JsonPropertyName("TradedPrice")]
        public decimal TradedPrice { get; set; }

        [JsonPropertyName("AvgTradedPrice")]
        public decimal AvgTradedPrice { get; set; }

        [JsonPropertyName("OffMktFlag")] // "1" for AMO, "0" otherwise
        public string? OffMktFlag { get; set; }

        [JsonPropertyName("OrderDateTime")] // e.g., "2024-09-11 14:39:29"
        public string? OrderDateTime { get; set; }

        [JsonPropertyName("ExchOrderTime")]
        public string? ExchOrderTime { get; set; }

        [JsonPropertyName("LastUpdatedTime")]
        public string? LastUpdatedTime { get; set; }

        [JsonPropertyName("Remarks")]
        public string? Remarks { get; set; }

        [JsonPropertyName("MktType")] // "NL" for Normal Market
        public string? MktType { get; set; }

        [JsonPropertyName("ReasonDescription")] // e.g., "CONFIRMED", or rejection reason
        public string? ReasonDescription { get; set; }

        [JsonPropertyName("LegNo")]
        public int LegNo { get; set; }

        [JsonPropertyName("Instrument")] // "EQUITY", "OPTIDX", etc.
        public string? Instrument { get; set; }

        [JsonPropertyName("Symbol")]
        public string? Symbol { get; set; }

        [JsonPropertyName("ProductName")] // "CNC", "INTRADAY" (more descriptive than "Product")
        public string? ProductName { get; set; }

        [JsonPropertyName("Status")] // "TRANSIT", "PENDING", "REJECTED", "CANCELLED", "TRADED", "EXPIRED"
        public string? Status { get; set; }

        [JsonPropertyName("LotSize")]
        public int LotSize { get; set; }

        [JsonPropertyName("StrikePrice")]
        public decimal StrikePrice { get; set; }

        [JsonPropertyName("ExpiryDate")] // "0001-01-01 00:00:00" for non-derivatives
        public string? ExpiryDate { get; set; }

        [JsonPropertyName("OptType")] // "CE", "PE", "XX"
        public string? OptType { get; set; }

        [JsonPropertyName("DisplayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("Isin")]
        public string? Isin { get; set; }

        [JsonPropertyName("Series")]
        public string? Series { get; set; }

        [JsonPropertyName("CorrelationId")]
        public string? CorrelationId { get; set; }

        // Other fields like goodTillDaysDate, refLtp, tickSize, AlgoId, Multiplier
        // can be added if needed from PDF page 42-43.
    }

    public class WebSocketOrderUpdateMessageDto
    {
        [JsonPropertyName("Data")]
        public OrderUpdateDataDto? Data { get; set; }

        [JsonPropertyName("Type")] // e.g., "order_alert"
        public string? Type { get; set; }
    }
}