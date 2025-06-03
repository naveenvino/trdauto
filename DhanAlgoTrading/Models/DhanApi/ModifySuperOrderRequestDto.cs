using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class ModifySuperOrderRequestDto
    {

        [JsonPropertyName("dhanClientId")]
        public string? DhanClientId { get; set; } // Will be set by DhanService

        [JsonIgnore] // OrderId will be taken from the URL path, but Dhan docs show it in body too.
                     // Set by service from path parameter.
        public string? OrderId { get; set; }

        [JsonPropertyName("orderType")] // For Entry Leg: "LIMIT", "MARKET". Required if modifying entry leg price/type.
        public string? OrderType { get; set; }

        [JsonPropertyName("legName")] // "ENTRY_LEG", "TARGET_LEG", "STOP_LOSS_LEG" - Required
        public string? LegName { get; set; }

        [JsonPropertyName("quantity")] // Only for ENTRY_LEG modification
        public int? Quantity { get; set; }

        [JsonPropertyName("price")] // For ENTRY_LEG if LIMIT order type
        public decimal? Price { get; set; }

        [JsonPropertyName("targetPrice")] // For ENTRY_LEG or TARGET_LEG modification
        public decimal? TargetPrice { get; set; }

        [JsonPropertyName("stopLossPrice")] // For ENTRY_LEG or STOP_LOSS_LEG modification
        public decimal? StopLossPrice { get; set; }

        [JsonPropertyName("trailingJump")] // For ENTRY_LEG or STOP_LOSS_LEG modification. Pass 0 to cancel trailing.
        public decimal? TrailingJump { get; set; }
    }
}
