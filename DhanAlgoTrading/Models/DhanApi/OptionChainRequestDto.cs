using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class OptionChainRequestDto
    {
        [JsonPropertyName("UnderlyingScrip")]
        public int UnderlyingScrip { get; set; }

        [JsonPropertyName("UnderlyingSeg")]
        public string UnderlyingSeg { get; set; }

        [JsonPropertyName("ExpiryDate")] // Or whatever the API expects for expiry
        public string ExpiryDate { get; set; } // Format "YYYY-MM-DD"
    }
}
