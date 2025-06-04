using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class EdisInquireResponseDto
    {
        [JsonPropertyName("isin")] public string? Isin { get; set; }
        [JsonPropertyName("status")] public string? Status { get; set; }
        [JsonPropertyName("quantity")] public int Quantity { get; set; }
    }
}
