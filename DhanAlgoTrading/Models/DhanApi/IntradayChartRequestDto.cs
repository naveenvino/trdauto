using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class IntradayChartRequestDto
    {
        [JsonPropertyName("securityId")] public string? SecurityId { get; set; }
        [JsonPropertyName("exchangeSegment")] public string? ExchangeSegment { get; set; }
        [JsonPropertyName("fromDate")] public string? FromDate { get; set; }
        [JsonPropertyName("toDate")] public string? ToDate { get; set; }
        [JsonPropertyName("resolution")] public string? Resolution { get; set; }
    }
}
