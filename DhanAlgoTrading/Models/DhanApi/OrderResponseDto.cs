using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class OrderResponseDto
    {
        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("clientOrderId")]
        public string? ClientOrderId { get; set; } // Your correlationId might be echoed here

        [JsonPropertyName("orderStatus")] // Dhan's status for the order itself
        public string? ApiOrderStatus { get; set; } // Renamed to avoid conflict

        [JsonPropertyName("exchangeOrderId")]
        public string? ExchangeOrderId { get; set; }

        [JsonPropertyName("message")] // Dhan's message field
        public string? DhanMessage { get; set; }

        [JsonPropertyName("errorCode")] // Dhan's error code field
        public string? ErrorCode { get; set; }

        // Custom fields for your application's internal use
        public string? CustomStatus { get; set; } // e.g., "ApiSuccess", "ApiError", "ValidationFailed"
        public string? CustomMessage { get; set; } // For your app's error messages
        public string? DhanErrorCode { get; internal set; }
    }
}
