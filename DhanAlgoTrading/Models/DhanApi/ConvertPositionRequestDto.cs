using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class ConvertPositionRequestDto
    {
        [JsonPropertyName("dhanClientId")]
        public string? DhanClientId { get; set; }

        [JsonPropertyName("fromProductType")]
        public string? FromProductType { get; set; }

        [JsonPropertyName("exchangeSegment")]
        public string? ExchangeSegment { get; set; }

        [JsonPropertyName("positionType")]
        public string? PositionType { get; set; }

        [JsonPropertyName("securityId")]
        public string? SecurityId { get; set; }

        [JsonPropertyName("tradingSymbol")]
        public string? TradingSymbol { get; set; }

        [JsonPropertyName("convertQty")]
        public int ConvertQty { get; set; }

        [JsonPropertyName("toProductType")]
        public string? ToProductType { get; set; }
    }
}
