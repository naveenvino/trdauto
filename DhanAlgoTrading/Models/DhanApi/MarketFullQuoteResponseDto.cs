using static DhanAlgoTrading.Models.DhanApi.MarketQuoteOhlcResponseDto;
using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
   
        public class MarketDepthEntry
        {
            [JsonPropertyName("quantity")]
            public int Quantity { get; set; }
            [JsonPropertyName("orders")]
            public int Orders { get; set; }
            [JsonPropertyName("price")]
            public decimal Price { get; set; }
        }

        public class MarketDepth
        {
            [JsonPropertyName("buy")]
            public List<MarketDepthEntry>? Buy { get; set; }
            [JsonPropertyName("sell")]
            public List<MarketDepthEntry>? Sell { get; set; }
        }

        public class FullQuoteInfo
        {
            [JsonPropertyName("average_price")]
            public decimal AveragePrice { get; set; }
            [JsonPropertyName("buy_quantity")]
            public int BuyQuantity { get; set; }
            [JsonPropertyName("depth")]
            public MarketDepth? Depth { get; set; }
            [JsonPropertyName("last_price")]
            public decimal LastPrice { get; set; }
            [JsonPropertyName("last_quantity")]
            public int LastQuantity { get; set; }
            [JsonPropertyName("last_trade_time")] // e.g., "01/01/1980 00:00:00" - consider parsing to DateTime
            public string? LastTradeTime { get; set; }
            [JsonPropertyName("lower_circuit_limit")]
            public decimal LowerCircuitLimit { get; set; }
            [JsonPropertyName("net_change")]
            public decimal NetChange { get; set; }
            [JsonPropertyName("ohlc")]
            public OhlcData? Ohlc { get; set; } // Reusing OhlcData from above
            [JsonPropertyName("oi")]
            public int OpenInterest { get; set; }
            [JsonPropertyName("oi_day_high")]
            public int OiDayHigh { get; set; }
            [JsonPropertyName("oi_day_low")] // PDF shows "oi day low" - using JsonPropertyName for mapping
            
            public int OiDayLow { get; set; }
            [JsonPropertyName("sell_quantity")]
            public int SellQuantity { get; set; }
            [JsonPropertyName("upper_circuit_limit")]
            public decimal UpperCircuitLimit { get; set; }
            [JsonPropertyName("volume")]
            public int Volume { get; set; }
        }

        // Response: {"data": {"NSE_FNO": {"49081": { ...full quote info... }}, ...}, "status": "success"}
        public class MarketFullQuoteResponseDto
        {
            [JsonPropertyName("status")]
            public string? Status { get; set; }

            [JsonPropertyName("data")]
            public Dictionary<string, Dictionary<string, FullQuoteInfo>>? Data { get; set; }

            [JsonPropertyName("internalErrorCode")]
            public string? InternalErrorCode { get; set; }
            [JsonPropertyName("internalErrorMessage")]
            public string? InternalErrorMessage { get; set; }
        }
    }
