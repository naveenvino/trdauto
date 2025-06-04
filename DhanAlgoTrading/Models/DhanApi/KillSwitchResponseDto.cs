using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class KillSwitchResponseDto
    {
        [JsonPropertyName("status")] public string? Status { get; set; }
    }
}
