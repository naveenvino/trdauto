using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.TradingView
{
    public class TradingViewAlertDto
    {
        [JsonPropertyName("passphrase")]
        public string? Passphrase { get; set; }

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; } // Can be parsed to DateTime if needed

        [JsonPropertyName("strategy_name")]
        public string? StrategyName { get; set; }

        [JsonPropertyName("action")] // e.g., "SELL_CALL_ATM", "SELL_PUT_OTM_2", "BUY_CALL", "SQUARE_OFF_ALL_NIFTY_OPTIONS"
        public string? Action { get; set; }

        [JsonPropertyName("underlying_scrip")] // Scrip code for underlying (e.g., NIFTY's int code)
        public int UnderlyingScrip { get; set; }

        [JsonPropertyName("underlying_segment")] // Segment of the underlying (e.g., "IDX_I")
        public string? UnderlyingSegment { get; set; }

        [JsonPropertyName("exchange_segment")] // Segment for placing the F&O order (e.g., "NSE_FNO")
        public string? ExchangeSegment { get; set; }

        [JsonPropertyName("product_type")] // "INTRADAY", "NORMAL", etc.
        public string? ProductType { get; set; }

        [JsonPropertyName("expiry_type")] // "WEEKLY", "MONTHLY"
        public string? ExpiryType { get; set; }

        [JsonPropertyName("expiry_offset_weeks")] // 0 for current, 1 for next, etc.
        public int ExpiryOffsetWeeks { get; set; }

        [JsonPropertyName("expiry_offset_months")] // 0 for current, 1 for next, etc.
        public int ExpiryOffsetMonths { get; set; }

        [JsonPropertyName("strike_otm_levels")] // For OTM options, e.g., 0 for ATM, 1 for 1st OTM, 2 for 2nd OTM
        public int StrikeOtmLevels { get; set; }

        [JsonPropertyName("quantity_lots")]
        public int QuantityLots { get; set; }

        [JsonPropertyName("order_type")] // "LIMIT", "MARKET"
        public string? OrderType { get; set; }

        // For LIMIT orders, you might send an absolute price or a buffer
        [JsonPropertyName("limit_price_absolute")]
        public decimal? LimitPriceAbsolute { get; set; }

        [JsonPropertyName("limit_price_buffer_percent")] // e.g., 0.5 for 0.5%. Positive to buy above LTP, negative to sell below LTP.
        public decimal? LimitPriceBufferPercent { get; set; }

        // For Super Orders (optional)
        [JsonPropertyName("is_super_order")]
        public bool IsSuperOrder { get; set; } = false;

        [JsonPropertyName("target_percent")] // Target as a percentage of entry price
        public decimal? TargetPercent { get; set; }

        [JsonPropertyName("stoploss_percent")] // Stop-loss as a percentage of entry price
        public decimal? StoplossPercent { get; set; }

        [JsonPropertyName("trailing_jump_percent")] // Trailing stop-loss jump as a percentage
        public decimal? TrailingJumpPercent { get; set; }

        // You can add more custom fields as needed by your strategies
        [JsonPropertyName("custom_params")]
        public Dictionary<string, string>? CustomParams { get; set; }
    }
}
