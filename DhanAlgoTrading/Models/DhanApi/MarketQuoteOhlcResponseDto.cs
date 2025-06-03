using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
   
        public class OhlcData
        {
            [JsonPropertyName("open")]
            public decimal Open { get; set; }
            [JsonPropertyName("high")]
            public decimal High { get; set; }
            [JsonPropertyName("low")]
            public decimal Low { get; set; }
            [JsonPropertyName("close")]
            public decimal Close { get; set; }
        }

        public class OhlcInfo
        {
            [JsonPropertyName("last_price")]
            public decimal LastPrice { get; set; }

            [JsonPropertyName("ohlc")]
            public OhlcData? Ohlc { get; set; }
        }

        // Response: {"data": {"NSE_EQ": {"11536": {"last_price": 4525.55, "ohlc": {...}}}, ...}, "status": "success"}
        public class MarketQuoteOhlcResponseDto
        {
            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("data")]
            public Dictionary<string, Dictionary<string, OhlcInfo>>? Data { get; set; }

            [JsonPropertyName("internalErrorCode")]
            public string? InternalErrorCode { get; set; }
            [JsonPropertyName("internalErrorMessage")]
            public string? InternalErrorMessage { get; set; }
        }
    }
