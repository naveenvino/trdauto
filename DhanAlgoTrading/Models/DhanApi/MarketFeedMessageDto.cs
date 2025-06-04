using System.Text.Json;
using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class MarketFeedMessageDto
    {
        [JsonPropertyName("type")] public string? Type { get; set; }
        [JsonPropertyName("data")] public JsonElement? Data { get; set; }
    }
}
