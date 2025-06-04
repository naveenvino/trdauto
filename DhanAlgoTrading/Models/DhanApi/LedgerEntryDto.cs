using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class LedgerEntryDto
    {
        [JsonPropertyName("date")] public string? Date { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
        [JsonPropertyName("debit")] public decimal Debit { get; set; }
        [JsonPropertyName("credit")] public decimal Credit { get; set; }
        [JsonPropertyName("balance")] public decimal Balance { get; set; }
    }
}
