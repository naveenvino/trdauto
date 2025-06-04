using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class ModifyOrderRequestDto
    {
        [JsonPropertyName("dhanClientId")]
        public string? DhanClientId { get; set; }

        [JsonPropertyName("orderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("orderType")]
        public string? OrderType { get; set; }

        [JsonPropertyName("legName")]
        public string? LegName { get; set; }

        [JsonPropertyName("quantity")]
        public int? Quantity { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }

        [JsonPropertyName("disclosedQuantity")]
        public int? DisclosedQuantity { get; set; }

        [JsonPropertyName("triggerPrice")]
        public decimal? TriggerPrice { get; set; }

        [JsonPropertyName("validity")]
        public string? Validity { get; set; }
    }
}
