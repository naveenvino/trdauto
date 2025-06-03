using System.Collections.Generic;
using System.Threading.Tasks;
using DhanAlgoTrading.Controllers;
using DhanAlgoTrading.Models.Configuration;
using DhanAlgoTrading.Models.DhanApi;
using DhanAlgoTrading.Models.TradingView;
using DhanAlgoTrading.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DhanAlgoTrading.Tests
{
    public class TradingViewWebhookControllerTests
    {
        private class StubDhanService : IDhanService
        {
            public Task<string?> GetUserProfileAsync() => Task.FromResult<string?>(null);
            public Task<IEnumerable<string>> GetExpiryDatesAsync(ExpiryListRequestDto expiryRequest) => Task.FromResult<IEnumerable<string>>(new List<string>());
            public Task<IEnumerable<OptionInstrument>> GetOptionChainAsync(OptionChainRequestDto optionChainRequest) => Task.FromResult<IEnumerable<OptionInstrument>>(new List<OptionInstrument>());
            public Task<OrderResponseDto?> PlaceOrderAsync(OrderRequestDto orderRequest) => Task.FromResult<OrderResponseDto?>(null);
            public Task<OrderDataDto?> GetOrderStatusAsync(string orderId) => Task.FromResult<OrderDataDto?>(null);
            public Task<IEnumerable<OrderDataDto>> GetOrderBookAsync() => Task.FromResult<IEnumerable<OrderDataDto>>(new List<OrderDataDto>());
            public Task<PositionBookDto?> GetPositionsAsync() => Task.FromResult<PositionBookDto?>(null);
            public Task<IEnumerable<TradeDataDto>> GetTradeBookAsync(string? orderId = null) => Task.FromResult<IEnumerable<TradeDataDto>>(new List<TradeDataDto>());
            public Task<OrderResponseDto?> SquareOffPositionAsync(SquareOffRequestDto squareOffRequest) => Task.FromResult<OrderResponseDto?>(null);
            public Task<FundLimitResponseDto?> GetFundLimitsAsync() => Task.FromResult<FundLimitResponseDto?>(null);
            public Task<MarginCalculatorResponseDto?> GetOrderMarginsAsync(MarginCalculatorRequestDto marginRequest) => Task.FromResult<MarginCalculatorResponseDto?>(null);
            public Task<SuperOrderResponseDto?> PlaceSuperOrderAsync(SuperOrderRequestDto superOrderRequest) => Task.FromResult<SuperOrderResponseDto?>(null);
            public Task<IEnumerable<SuperOrderDataDto>> GetSuperOrdersAsync() => Task.FromResult<IEnumerable<SuperOrderDataDto>>(new List<SuperOrderDataDto>());
            public Task<SuperOrderResponseDto?> ModifySuperOrderAsync(string orderId, ModifySuperOrderRequestDto modifyRequest) => Task.FromResult<SuperOrderResponseDto?>(null);
            public Task<SuperOrderResponseDto?> CancelSuperOrderLegAsync(string orderId, string orderLeg) => Task.FromResult<SuperOrderResponseDto?>(null);
            public Task<MarketQuoteLtpResponseDto?> GetLtpDataAsync(MarketDataRequestDto request) => Task.FromResult<MarketQuoteLtpResponseDto?>(null);
            public Task<MarketQuoteOhlcResponseDto?> GetOhlcDataAsync(MarketDataRequestDto request) => Task.FromResult<MarketQuoteOhlcResponseDto?>(null);
            public Task<MarketFullQuoteResponseDto?> GetFullMarketQuoteAsync(MarketDataRequestDto request) => Task.FromResult<MarketFullQuoteResponseDto?>(null);
            public Task<IEnumerable<DhanInstrumentDto>> GetInstrumentsBySegmentAsync(string exchangeSegment) => Task.FromResult<IEnumerable<DhanInstrumentDto>>(new List<DhanInstrumentDto>());
            public Task<IEnumerable<OrderResponseDto>?> PlaceSliceOrderAsync(OrderRequestDto sliceOrderRequest) => Task.FromResult<IEnumerable<OrderResponseDto>?>(null);
        }

        [Fact]
        public async Task ReceiveTradingViewAlert_InvalidPassphrase_ReturnsUnauthorized()
        {
            var options = Options.Create(new DhanApiSettings { TradingViewWebhookPassphrase = "secret", BaseUrl = "https://api", ClientId = "cid" });
            var controller = new TradingViewWebhookController(NullLogger<TradingViewWebhookController>.Instance, new StubDhanService(), options);

            var result = await controller.ReceiveTradingViewAlert(new TradingViewAlertDto
            {
                Passphrase = "wrong",
                Action = "TEST",
                UnderlyingScrip = 1,
                ExchangeSegment = "NSE_FNO"
            });

            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
