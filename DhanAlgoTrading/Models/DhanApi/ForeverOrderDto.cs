using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class ForeverOrderDto
    {
        [JsonPropertyName("orderId")] public string? OrderId { get; set; }
        [JsonPropertyName("status")] public string? Status { get; set; }
        [JsonPropertyName("securityId")] public string? SecurityId { get; set; }
        [JsonPropertyName("quantity")] public int Quantity { get; set; }
        [JsonPropertyName("price")] public decimal? Price { get; set; }
        [JsonPropertyName("triggerPrice")] public decimal? TriggerPrice { get; set; }
    }
}
