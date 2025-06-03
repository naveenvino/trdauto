using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class DhanInstrumentDto
    {
        [JsonPropertyName("EXCH_ID")] // Example, verify actual JSON key from API response
        public string? ExchangeId { get; set; }

        [JsonPropertyName("SEGMENT")]
        public string? Segment { get; set; }

        [JsonPropertyName("SECURITY_ID")] // This is Dhan's unique ID for the instrument
        public string? SecurityId { get; set; }

        [JsonPropertyName("ISIN")]
        public string? Isin { get; set; }

        [JsonPropertyName("INSTRUMENT")] // e.g., "OPTIDX", "FUTSTK", "EQUITY"
        public string? InstrumentName { get; set; } // Renamed from INSTRUMENT to avoid C# keyword conflict

        [JsonPropertyName("UNDERLYING_SECURITY_ID")]
        public string? UnderlyingSecurityId { get; set; }

        [JsonPropertyName("UNDERLYING_SYMBOL")]
        public string? UnderlyingSymbol { get; set; }

        [JsonPropertyName("SYMBOL_NAME")] // Scrip name like "NIFTY", "RELIANCE"
        public string? SymbolName { get; set; }

        [JsonPropertyName("DISPLAY_NAME")] // Custom Dhan display symbol
        public string? DisplayName { get; set; }

        [JsonPropertyName("INSTRUMENT_TYPE")] // More detailed type from exchange
        public string? InstrumentType { get; set; }

        [JsonPropertyName("SERIES")] // e.g., "EQ", "F&O" series
        public string? Series { get; set; }

        [JsonPropertyName("LOT_SIZE")]
        public int? LotSize { get; set; }

        [JsonPropertyName("SM_EXPIRY_DATE")] // Property name from "Detailed tag" in PDF
        public string? ExpiryDate { get; set; } // Format "YYYY-MM-DD" or as returned by API

        [JsonPropertyName("STRIKE_PRICE")]
        public decimal? StrikePrice { get; set; }

        [JsonPropertyName("OPTION_TYPE")] // "CE", "PE", or null/empty for non-options
        public string? OptionType { get; set; }

        [JsonPropertyName("TICK_SIZE")]
        public decimal? TickSize { get; set; }

        [JsonPropertyName("EXPIRY_FLAG")] // M - Monthly, W - Weekly
        public string? ExpiryFlag { get; set; }
    }
}
