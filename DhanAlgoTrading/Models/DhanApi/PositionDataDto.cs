using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class PositionDataDto
    {
        [JsonPropertyName("dhanClientId")]
        public string? DhanClientId { get; set; }

        [JsonPropertyName("tradingSymbol")]
        public string? TradingSymbol { get; set; }

        [JsonPropertyName("securityId")]
        public string? SecurityId { get; set; }

        [JsonPropertyName("positionType")] // e.g., "LONG", "SHORT" (Dhan might use different terms like "NET")
        public string? PositionType { get; set; }

        [JsonPropertyName("exchangeSegment")]
        public string? ExchangeSegment { get; set; }

        [JsonPropertyName("productType")]
        public string? ProductType { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } // Net quantity

        [JsonPropertyName("buyAvgPrice")]
        public decimal BuyAvgPrice { get; set; }

        [JsonPropertyName("sellAvgPrice")]
        public decimal SellAvgPrice { get; set; }

        [JsonPropertyName("netAvgPrice")] // Or realized P/L avg price
        public decimal NetAvgPrice { get; set; }

        [JsonPropertyName("ltp")] // Last Traded Price
        public decimal LastTradedPrice { get; set; }

        [JsonPropertyName("unrealizedProfit")]
        public decimal UnrealizedProfit { get; set; }

        [JsonPropertyName("realizedProfit")]
        public decimal RealizedProfit { get; set; }

        [JsonPropertyName("buyValue")]
        public decimal BuyValue { get; set; }

        [JsonPropertyName("sellValue")]
        public decimal SellValue { get; set; }

        [JsonPropertyName("multiplier")] // Usually 1 for equities/futures, lot_size for options if qty is in lots
        public int Multiplier { get; set; }
    }
}
