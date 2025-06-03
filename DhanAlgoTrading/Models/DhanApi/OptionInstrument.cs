using System.Text.Json.Serialization;

namespace DhanAlgoTrading.Models.DhanApi
{
    public class OptionInstrument
    {
        [JsonPropertyName("security_id")]
        public string SecurityId { get; set; }

        [JsonPropertyName("trading_symbol")]
        public string TradingSymbol { get; set; }

        [JsonPropertyName("lot_size")]
        public int LotSize { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("expiry_date")]
        public string ExpiryDate { get; set; }

        [JsonPropertyName("strike_price")]
        public decimal StrikePrice { get; set; }

        [JsonPropertyName("instrument_type")]
        public string InstrumentType { get; set; }

        [JsonPropertyName("option_type")]
        public string OptionType { get; set; }

        [JsonPropertyName("exchange_segment")]
        public string ExchangeSegment { get; set; }
    }
}
