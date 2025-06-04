using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class EdisFormRequestDto
    {
        [JsonPropertyName("isin")] public string? Isin { get; set; }
        [JsonPropertyName("qty")] public int Quantity { get; set; }
        [JsonPropertyName("tpin")] public string? Tpin { get; set; }
    }
}
