using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class HistoricalCandle
    {
        [JsonPropertyName("time")] public string? Time { get; set; }
        [JsonPropertyName("open")] public decimal Open { get; set; }
        [JsonPropertyName("high")] public decimal High { get; set; }
        [JsonPropertyName("low")] public decimal Low { get; set; }
        [JsonPropertyName("close")] public decimal Close { get; set; }
        [JsonPropertyName("volume")] public int Volume { get; set; }
    }

    public class HistoricalChartResponseDto
    {
        [JsonPropertyName("candles")] public List<HistoricalCandle>? Candles { get; set; }
    }
}
