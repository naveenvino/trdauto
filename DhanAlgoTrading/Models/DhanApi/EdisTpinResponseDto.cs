using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class EdisTpinResponseDto
    {
        [JsonPropertyName("tpin")] public string? Tpin { get; set; }
        [JsonPropertyName("validTill")] public string? ValidTill { get; set; }
    }
}
