namespace DhanAlgoTrading.Models.Configuration
{
    public class DhanApiSettings
    {
        public string? BaseUrl { get; set; }
        public string? LiveOrderUpdateUrl { get; set; }
        public string? MarketFeedUrl { get; set; }
        public string? DepthFeedUrl { get; set; }
        public string? AccessToken { get; set; }
        public string? ClientId { get; set; }
        public string? TradingViewWebhookPassphrase { get; set; } // New for Webhook security

    }
}
