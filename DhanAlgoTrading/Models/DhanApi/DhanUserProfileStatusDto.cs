namespace DhanAlgoTrading.Models.DhanApi
{
    public class DhanUserProfileStatusDto
    {
        public bool IsTokenValid { get; set; }
        public List<string>? ActiveSegments { get; set; } // e.g., ["NSE_EQ", "NSE_FNO"]
        public bool IsDataApiSubscribed { get; set; }
        public DateTime? DataApiSubscriptionExpiry { get; set; } // Assuming it might have an expiry
        public string? DdpiStatus { get; set; } // e.g., "ACTIVE", "INACTIVE"
        public bool IsMtfEnabled { get; set; }
    }
}
