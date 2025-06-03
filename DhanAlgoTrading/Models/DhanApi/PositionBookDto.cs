using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class PositionBookDto
    {
        [JsonPropertyName("totalNetPnl")] // Example summary field
        public decimal? TotalNetPnl { get; set; }

        [JsonPropertyName("totalRealizedPnl")]
        public decimal? TotalRealizedPnl { get; set; }

        [JsonPropertyName("totalUnrealizedPnl")]
        public decimal? TotalUnrealizedPnl { get; set; }

        [JsonPropertyName("positions")] // Assuming positions are in a list called "positions"
        public List<PositionDataDto>? Positions { get; set; }
    }
}
