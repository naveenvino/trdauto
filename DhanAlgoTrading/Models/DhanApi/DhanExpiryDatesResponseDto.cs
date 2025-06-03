using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class DhanExpiryDatesResponseDto
    {
        [JsonPropertyName("data")]
        public List<string> Data { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}
