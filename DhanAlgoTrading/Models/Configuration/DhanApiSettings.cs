namespace DhanAlgoTrading.Models.Configuration
{
    public class DhanApiSettings
    {
        public string? BaseUrl { get; set; }
        public string? LiveOrderUpdateUrl { get; set; } // <<<< THIS LINE WAS ADDED IN PART 7
        public string? AccessToken { get; set; }
        public string? ClientId { get; set; }
        public string? TradingViewWebhookPassphrase { get; set; } // New for Webhook security

    }
}
