using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class SuperOrderLegDetailDto
    {
        [JsonPropertyName("orderId")] // This seems to be the main orderId repeated, or specific leg orderId if different
        public string? OrderId { get; set; }

        [JsonPropertyName("legName")] // e.g., "STOP_LOSS_LEG", "TARGET_LEG"
        public string? LegName { get; set; }

        [JsonPropertyName("transactionType")] // "BUY" or "SELL"
        public string? TransactionType { get; set; }

        // PDF shows "totalQuatity" - assuming typo, using "totalQuantity"
        [JsonPropertyName("totalQuantity")] // Or "totalQuatity" if API uses that typo
        public int TotalQuantity { get; set; } // Total quantity for this leg

        [JsonPropertyName("remainingQuantity")]
        public int RemainingQuantity { get; set; }

        [JsonPropertyName("triggeredQuantity")]
        public int TriggeredQuantity { get; set; }

        [JsonPropertyName("price")] // Price for this leg (e.g., stop loss price, target price)
        public decimal Price { get; set; }

        [JsonPropertyName("orderStatus")] // Status of this specific leg (e.g., "PENDING")
        public string? OrderStatus { get; set; }

        [JsonPropertyName("trailingJump")]
        public decimal? TrailingJump { get; set; } // Nullable as it might not always be present
    }
}
