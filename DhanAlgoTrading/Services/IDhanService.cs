using DhanAlgoTrading.Models.DhanApi;

namespace DhanAlgoTrading.Services
{
    public interface IDhanService
    {
        Task<DhanUserProfileDto?> GetUserProfileAsync();
        Task<IEnumerable<string>> GetExpiryDatesAsync(ExpiryListRequestDto expiryRequest); // From Part 2
        Task<IEnumerable<OptionInstrument>> GetOptionChainAsync(OptionChainRequestDto optionChainRequest); // From Part 2
        Task<OrderResponseDto?> PlaceOrderAsync(OrderRequestDto orderRequest); // Method for Part 3

        // New for Part 4
        Task<OrderDataDto?> GetOrderStatusAsync(string orderId);
        Task<IEnumerable<OrderDataDto>> GetOrderBookAsync();
        Task<PositionBookDto?> GetPositionsAsync(); // Changed to PositionBookDto for clarity
        Task<IEnumerable<TradeDataDto>> GetTradeBookAsync(string? orderId = null);
        Task<OrderResponseDto?> SquareOffPositionAsync(SquareOffRequestDto squareOffRequest);
        Task<FundLimitResponseDto?> GetFundLimitsAsync();
        Task<MarginCalculatorResponseDto?> GetOrderMarginsAsync(MarginCalculatorRequestDto marginRequest);
        Task<SuperOrderResponseDto?> PlaceSuperOrderAsync(SuperOrderRequestDto superOrderRequest);
        Task<IEnumerable<SuperOrderDataDto>> GetSuperOrdersAsync();
        Task<SuperOrderResponseDto?> ModifySuperOrderAsync(string orderId, ModifySuperOrderRequestDto modifyRequest);
        Task<SuperOrderResponseDto?> CancelSuperOrderLegAsync(string orderId, string orderLeg);
        Task<MarketQuoteLtpResponseDto?> GetLtpDataAsync(MarketDataRequestDto request);
        Task<MarketQuoteOhlcResponseDto?> GetOhlcDataAsync(MarketDataRequestDto request);
        Task<MarketFullQuoteResponseDto?> GetFullMarketQuoteAsync(MarketDataRequestDto request);
        Task<IEnumerable<DhanInstrumentDto>> GetInstrumentsBySegmentAsync(string exchangeSegment);
        Task<IEnumerable<OrderResponseDto>?> PlaceSliceOrderAsync(OrderRequestDto sliceOrderRequest);



    }
}
