using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class SuperOrderResponseDto
    {
        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; } // The entry leg order ID

        [JsonPropertyName("orderStatus")] // e.g., "PENDING", "TRADING", "REJECTED"
        public string? ApiOrderStatus { get; set; }

        // Fields to capture structured error details from Dhan, like the "Market is Closed" error
        [JsonPropertyName("errorType")] // e.g., "Order_Error"
        public string? DhanErrorType { get; set; }

        [JsonPropertyName("errorCode")] // e.g., "DH-906"
        public string? DhanErrorCode { get; set; }

        [JsonPropertyName("errorMessage")] // e.g., "Market is Closed! Want to place an offline order?"
        public string? DhanErrorMessage { get; set; }

        // Custom fields for your application's internal use
        public string? CustomStatus { get; set; }
        public string? CustomMessage { get; set; }
    }
}
