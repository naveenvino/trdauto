using DhanAlgoTrading.Models.Configuration;
using DhanAlgoTrading.Models.DhanApi;
using DhanAlgoTrading.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DhanAlgoTrading.Tests
{
    public class LiveOrderUpdateServiceTests
    {
        private static LiveOrderUpdateService CreateService()
        {
            var options = Options.Create(new DhanApiSettings
            {
                LiveOrderUpdateUrl = "wss://example",
                AccessToken = "token",
                ClientId = "cid"
            });

            return new LiveOrderUpdateService(NullLogger<LiveOrderUpdateService>.Instance, options);
        }

        [Fact]
        public void ProcessMessage_ValidOrderAlert_RaisesEvent()
        {
            var service = CreateService();
            OrderUpdateDataDto? received = null;
            service.OrderUpdateReceived += (_, d) => received = d;

            const string json = "{\"Data\":{\"OrderNo\":\"ORD123\",\"Status\":\"COMPLETE\",\"Symbol\":\"INFY\",\"TxnType\":\"BUY\",\"Quantity\":10,\"Price\":100.5,\"TradedQty\":10,\"AvgTradedPrice\":100.5},\"Type\":\"order_alert\"}";

            service.ProcessMessage(json);

            Assert.NotNull(received);
            Assert.Equal("ORD123", received?.OrderNo);
        }

        [Fact]
        public void ProcessMessage_InvalidType_DoesNotRaiseEvent()
        {
            var service = CreateService();
            bool fired = false;
            service.OrderUpdateReceived += (_, _) => fired = true;

            const string json = "{\"Data\":{\"OrderNo\":\"O1\"},\"Type\":\"other\"}";

            service.ProcessMessage(json);

            Assert.False(fired);
        }
    }
}
