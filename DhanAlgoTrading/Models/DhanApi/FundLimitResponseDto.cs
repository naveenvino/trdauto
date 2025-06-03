using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class FundLimitResponseDto
    {
        [JsonPropertyName("dhanClientId")]
        public string? DhanClientId { get; set; }

        [JsonPropertyName("availabelBalance")] // Note: "availabelBalance" as per PDF [cite: 60]
        public decimal AvailabelBalance { get; set; } // Corrected spelling to AvailableBalance for consistency if preferred internally

        [JsonPropertyName("sodLimit")]
        public decimal SodLimit { get; set; }

        [JsonPropertyName("collateralAmount")]
        public decimal CollateralAmount { get; set; }

        [JsonPropertyName("receiveableAmount")]
        public decimal ReceiveableAmount { get; set; }

        [JsonPropertyName("utilizedAmount")]
        public decimal UtilizedAmount { get; set; }

        [JsonPropertyName("blockedPayoutAmount")]
        public decimal BlockedPayoutAmount { get; set; }

        [JsonPropertyName("withdrawableBalance")]
        public decimal WithdrawableBalance { get; set; }

    }
}
