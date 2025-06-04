using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class DhanHoldingDto
    {
        [JsonPropertyName("exchange")] public string? Exchange { get; set; }
        [JsonPropertyName("tradingSymbol")] public string? TradingSymbol { get; set; }
        [JsonPropertyName("securityId")] public string? SecurityId { get; set; }
        [JsonPropertyName("isin")] public string? Isin { get; set; }
        [JsonPropertyName("totalQty")] public int TotalQty { get; set; }
        [JsonPropertyName("dpQty")] public int DpQty { get; set; }
        [JsonPropertyName("t1Qty")] public int T1Qty { get; set; }
        [JsonPropertyName("availableQty")] public int AvailableQty { get; set; }
        [JsonPropertyName("collateralQty")] public int CollateralQty { get; set; }
        [JsonPropertyName("avgCostPrice")] public decimal AvgCostPrice { get; set; }
    }
}
