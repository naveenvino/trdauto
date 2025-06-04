using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DhanAlgoTrading.Api.Services;
using DhanAlgoTrading.Models.Configuration;
using DhanAlgoTrading.Models.DhanApi;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace DhanAlgoTrading.Tests
{
    public class DhanServiceAdditionalTests
    {
        private class FakeHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;
            public FakeHandler(HttpResponseMessage response)
            {
                _response = response;
            }
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_response);
            }
        }

        [Fact]
        public async Task PlaceOrderAsync_ReturnsSuccess()
        {
            var json = "{\"orderId\":\"101\",\"orderStatus\":\"SUCCESS\",\"message\":\"ok\"}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var httpClient = new HttpClient(new FakeHandler(response))
            {
                BaseAddress = new Uri("https://api.test/")
            };
            var settings = Options.Create(new DhanApiSettings { BaseUrl = "https://api.test/", ClientId = "cid", AccessToken = "token" });
            var service = new DhanService(httpClient, settings, NullLogger<DhanService>.Instance);

            var request = new OrderRequestDto
            {
                TransactionType = "BUY",
                ExchangeSegment = "NSE_FNO",
                ProductType = "INTRADAY",
                OrderType = "MARKET",
                SecurityId = "1",
                Quantity = 1
            };

            var result = await service.PlaceOrderAsync(request);

            Assert.Equal("101", result?.OrderId);
            Assert.Equal("ApiSuccess", result?.CustomStatus);
        }

        [Fact]
        public async Task PlaceOrderAsync_ReturnsValidationFailedWhenNull()
        {
            var httpClient = new HttpClient(new FakeHandler(new HttpResponseMessage(HttpStatusCode.OK)))
            {
                BaseAddress = new Uri("https://api.test/")
            };
            var settings = Options.Create(new DhanApiSettings { BaseUrl = "https://api.test/", ClientId = "cid", AccessToken = "token" });
            var service = new DhanService(httpClient, settings, NullLogger<DhanService>.Instance);

            var result = await service.PlaceOrderAsync(null!);

            Assert.Equal("ValidationFailed", result?.CustomStatus);
        }

        [Fact]
        public async Task PlaceOrderAsync_ReturnsApiErrorOnHttpError()
        {
            var json = "{\"orderId\":\"55\",\"orderStatus\":\"REJECTED\",\"message\":\"bad\",\"errorCode\":\"E\"}";
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var httpClient = new HttpClient(new FakeHandler(response))
            {
                BaseAddress = new Uri("https://api.test/")
            };
            var settings = Options.Create(new DhanApiSettings { BaseUrl = "https://api.test/", ClientId = "cid", AccessToken = "token" });
            var service = new DhanService(httpClient, settings, NullLogger<DhanService>.Instance);

            var req = new OrderRequestDto { TransactionType = "BUY", SecurityId = "1", Quantity = 1, OrderType = "MARKET" };
            var result = await service.PlaceOrderAsync(req);

            Assert.Equal("ApiError", result?.CustomStatus);
            Assert.Equal("55", result?.OrderId);
            Assert.Equal("E", result?.ErrorCode);
        }

        [Fact]
        public async Task GetOrderBookAsync_ReturnsOrders()
        {
            var json = "[{\"orderId\":\"1\"},{\"orderId\":\"2\"}]";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var httpClient = new HttpClient(new FakeHandler(response))
            {
                BaseAddress = new Uri("https://api.test/")
            };
            var settings = Options.Create(new DhanApiSettings { BaseUrl = "https://api.test/", ClientId = "cid", AccessToken = "token" });
            var service = new DhanService(httpClient, settings, NullLogger<DhanService>.Instance);

            var result = await service.GetOrderBookAsync();

            Assert.Equal(2, new List<OrderDataDto>(result).Count);
        }

        [Fact]
        public async Task GetOrderBookAsync_ReturnsEmptyOnHttpError()
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var httpClient = new HttpClient(new FakeHandler(response))
            {
                BaseAddress = new Uri("https://api.test/")
            };
            var settings = Options.Create(new DhanApiSettings { BaseUrl = "https://api.test/", ClientId = "cid", AccessToken = "token" });
            var service = new DhanService(httpClient, settings, NullLogger<DhanService>.Instance);

            var result = await service.GetOrderBookAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPositionsAsync_ReturnsPositions()
        {
            var json = "{\"positions\":[{\"securityId\":\"1\",\"quantity\":1}],\"totalNetPnl\":10}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var httpClient = new HttpClient(new FakeHandler(response))
            {
                BaseAddress = new Uri("https://api.test/")
            };
            var settings = Options.Create(new DhanApiSettings { BaseUrl = "https://api.test/", ClientId = "cid", AccessToken = "token" });
            var service = new DhanService(httpClient, settings, NullLogger<DhanService>.Instance);

            var result = await service.GetPositionsAsync();

            Assert.NotNull(result);
            Assert.Single(result?.Positions ?? new List<PositionDataDto>());
        }

        [Fact]
        public async Task GetPositionsAsync_ReturnsNullOnHttpError()
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var httpClient = new HttpClient(new FakeHandler(response))
            {
                BaseAddress = new Uri("https://api.test/")
            };
            var settings = Options.Create(new DhanApiSettings { BaseUrl = "https://api.test/", ClientId = "cid", AccessToken = "token" });
            var service = new DhanService(httpClient, settings, NullLogger<DhanService>.Instance);

            var result = await service.GetPositionsAsync();

            Assert.Null(result);
        }

        [Fact]
        public async Task GetHoldingsAsync_ReturnsList()
        {
            var json = "[{}]";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            var httpClient = new HttpClient(new FakeHandler(response)) { BaseAddress = new Uri("https://api.test/") };
            var settings = Options.Create(new DhanApiSettings { BaseUrl = "https://api.test/", ClientId = "cid", AccessToken = "token" });
            var service = new DhanService(httpClient, settings, NullLogger<DhanService>.Instance);

            var result = await service.GetHoldingsAsync();

            Assert.Single(result);
        }

        [Fact]
        public async Task ConvertPositionAsync_ReturnsTrueOnSuccess()
        {
            var response = new HttpResponseMessage(HttpStatusCode.Accepted);
            var httpClient = new HttpClient(new FakeHandler(response)) { BaseAddress = new Uri("https://api.test/") };
            var settings = Options.Create(new DhanApiSettings { BaseUrl = "https://api.test/", ClientId = "cid", AccessToken = "token" });
            var service = new DhanService(httpClient, settings, NullLogger<DhanService>.Instance);

            var req = new ConvertPositionRequestDto { FromProductType = "INTRADAY", ToProductType = "CNC" };
            var result = await service.ConvertPositionAsync(req);

            Assert.True(result);
        }
    }
}
