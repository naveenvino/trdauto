namespace DhanAlgoTrading.Models.DhanApi
{
    public class MarketDataRequestDto : Dictionary<string, List<string>>
    {
        // Example usage:
        // var request = new MarketDataRequestDto
        // {
        //     { "NSE_EQ", new List<string> { "1333", "11536" } },
        //     { "NSE_FNO", new List<string> { "YOUR_OPTION_SECURITY_ID_1", "YOUR_OPTION_SECURITY_ID_2" } }
        // };
    }
}
