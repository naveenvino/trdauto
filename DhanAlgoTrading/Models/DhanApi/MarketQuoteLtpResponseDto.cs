using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{

    public class LtpInfo
    {
        [JsonPropertyName("last_price")]
        public decimal LastPrice { get; set; }
    }

    public class MarketQuoteLtpResponseDto
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("data")]
        public Dictionary<string, Dictionary<string, LtpInfo>>? Data { get; set; }

        // To capture potential error structure, e.g. {"status":"failed", "internalErrorCode":"806", "internalErrorMessage":"Data API not enabled"}
        [JsonPropertyName("internalErrorCode")]
        public string? InternalErrorCode { get; set; }
        [JsonPropertyName("internalErrorMessage")]
        public string? InternalErrorMessage { get; set; }
    }
}
