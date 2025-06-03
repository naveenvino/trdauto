using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class SquareOffRequestDto
    {
        [JsonPropertyName("securityId")]
        public string? SecurityId { get; set; } // ID of the instrument to square off

        [JsonPropertyName("productType")] // Product type of the position to square off
        public string? ProductType { get; set; }

        [JsonPropertyName("exchangeSegment")] // Exchange segment of the position
        public string? ExchangeSegment { get; set; }

        // Optional parameters for how to square off
        [JsonPropertyName("orderType")] // e.g., "MARKET", "LIMIT". Defaults to MARKET if not provided.
        public string? OrderType { get; set; }

        [JsonPropertyName("price")] // Required if orderType is "LIMIT"
        public decimal? Price { get; set; }

        [JsonPropertyName("triggerPrice")] // Required if orderType is "SL" or "SL-L"
        public decimal? TriggerPrice { get; set; }

        [JsonPropertyName("validity")] // e.g., "DAY", "IOC". Defaults to DAY if not provided.
        public string? Validity { get; set; }

        [JsonPropertyName("correlationId")] // Optional client-side identifier for the square-off order
        public string? CorrelationId { get; set; }
    }
}
