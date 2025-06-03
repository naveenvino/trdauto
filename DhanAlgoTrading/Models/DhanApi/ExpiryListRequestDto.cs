using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class ExpiryListRequestDto
    {
        [JsonPropertyName("UnderlyingScrip")]
        public int UnderlyingScrip { get; set; } // e.g., 13 or 26000 for NIFTY

        [JsonPropertyName("UnderlyingSeg")]
        public string UnderlyingSeg { get; set; } // e.g., "IDX_I"
    }
}
