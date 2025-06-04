using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class DhanUserProfileDto
    {
        [JsonPropertyName("dhanClientId")]
        public string? DhanClientId { get; set; }

        [JsonPropertyName("tokenValidity")]
        public string? TokenValidity { get; set; }

        [JsonPropertyName("activeSegment")]
        public string? ActiveSegment { get; set; }

        [JsonPropertyName("ddpi")]
        public string? Ddpi { get; set; }

        [JsonPropertyName("dataPlan")]
        public string? DataPlan { get; set; }
    }
}
