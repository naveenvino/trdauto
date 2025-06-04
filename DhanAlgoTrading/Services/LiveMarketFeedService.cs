using DhanAlgoTrading.Models.Configuration;
using DhanAlgoTrading.Models.DhanApi;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DhanAlgoTrading.Services
{
    public class LiveMarketFeedService : BackgroundService
    {
        private readonly ILogger<LiveMarketFeedService> _logger;
        private readonly DhanApiSettings _apiSettings;
        private ClientWebSocket? _clientWebSocket;
        private readonly JsonSerializerOptions _jsonOptions;

        public event EventHandler<MarketFeedMessageDto>? MarketFeedReceived;

        private void Publish(MarketFeedMessageDto msg)
        {
            MarketFeedReceived?.Invoke(this, msg);
        }

        public LiveMarketFeedService(ILogger<LiveMarketFeedService> logger, IOptions<DhanApiSettings> options)
        {
            _logger = logger;
            _apiSettings = options.Value;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(_apiSettings.MarketFeedUrl) ||
                string.IsNullOrWhiteSpace(_apiSettings.AccessToken) ||
                string.IsNullOrWhiteSpace(_apiSettings.ClientId))
            {
                _logger.LogError("LiveMarketFeedService cannot start due to missing configuration.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _clientWebSocket = new ClientWebSocket();
                    await _clientWebSocket.ConnectAsync(new Uri(_apiSettings.MarketFeedUrl), stoppingToken);
                    await SendAuthorizationAsync(stoppingToken);
                    await ReceiveMessagesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "LiveMarketFeedService connection error. Will retry.");
                }
                finally
                {
                    if (_clientWebSocket != null)
                    {
                        try
                        {
                            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        }
                        catch { }
                        _clientWebSocket.Dispose();
                        _clientWebSocket = null;
                    }
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }

        private async Task SendAuthorizationAsync(CancellationToken token)
        {
            if (_clientWebSocket == null) return;
            var auth = new WebSocketAuthRequestDto
            {
                LoginReq = new LoginRequestPayload
                {
                    ClientId = _apiSettings.ClientId,
                    Token = _apiSettings.AccessToken,
                    MsgCode = 42
                },
                UserType = "SELF"
            };
            var json = JsonSerializer.Serialize(auth, _jsonOptions);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, token);
        }

        private async Task ReceiveMessagesAsync(CancellationToken token)
        {
            if (_clientWebSocket == null) return;
            var buffer = new byte[8192];
            while (_clientWebSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
            {
                var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                try
                {
                    var msg = JsonSerializer.Deserialize<MarketFeedMessageDto>(json, _jsonOptions);
                    if (msg != null) Publish(msg);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to deserialize market feed message: {Json}", json);
                }
            }
        }

        public async Task SendAsync(string message, CancellationToken token = default)
        {
            if (_clientWebSocket?.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, token);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            if (_clientWebSocket?.State == WebSocketState.Open)
            {
                await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Service stopping", stoppingToken);
            }
            _clientWebSocket?.Dispose();
            await base.StopAsync(stoppingToken);
        }
    }
}
