using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class MarginCalculatorResponseDto
    {
        [JsonPropertyName("totalMargin")]
        public decimal TotalMargin { get; set; }

        [JsonPropertyName("spanMargin")]
        public decimal SpanMargin { get; set; }

        [JsonPropertyName("exposureMargin")]
        public decimal ExposureMargin { get; set; }

        [JsonPropertyName("availableBalance")]
        public decimal AvailableBalance { get; set; }

        [JsonPropertyName("variableMargin")]
        public decimal VariableMargin { get; set; }

        [JsonPropertyName("insufficientBalance")]
        public decimal InsufficientBalance { get; set; }

        [JsonPropertyName("brokerage")]
        public decimal Brokerage { get; set; }

        [JsonPropertyName("leverage")]
        public string? Leverage { get; set; } // Represented as string like "4.00" in PDF

        // Custom field for internal error reporting from service
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ErrorMessage { get; set; }
    }
}
