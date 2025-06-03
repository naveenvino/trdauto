using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
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
    public class DhanServiceTests
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

        private class CapturingHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _response;
            public HttpRequestMessage? CapturedRequest { get; private set; }

            public CapturingHandler(HttpResponseMessage response)
            {
                _response = response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                CapturedRequest = request;
                return Task.FromResult(_response);
            }
        }

        [Fact]
        public async Task GetExpiryDatesAsync_ReturnsList()
        {
            var json = "{\"data\":[\"2024-06-01\",\"2024-06-08\"]}";
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            };

            var httpClient = new HttpClient(new FakeHandler(response))
            {
                BaseAddress = new Uri("https://api.test/")
            };
            var settings = Options.Create(new DhanApiSettings { BaseUrl = "https://api.test/", ClientId = "cid", AccessToken = "token" });
            var service = new DhanService(httpClient, settings, NullLogger<DhanService>.Instance);

            var result = await service.GetExpiryDatesAsync(new ExpiryListRequestDto());

            Assert.Contains("2024-06-01", result);
        }

        [Fact]
        public async Task GetUserProfileAsync_UsesBaseAddress()
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}")
            };

            var handler = new CapturingHandler(response);
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.test/")
            };
            var settings = Options.Create(new DhanApiSettings { BaseUrl = "https://api.test/", ClientId = "cid", AccessToken = "token" });
            var service = new DhanService(httpClient, settings, NullLogger<DhanService>.Instance);

            await service.GetUserProfileAsync();

            Assert.Equal("https://api.test/profile", handler.CapturedRequest?.RequestUri?.ToString());
        }
    }
}
