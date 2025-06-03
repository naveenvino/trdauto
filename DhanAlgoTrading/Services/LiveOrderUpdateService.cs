using DhanAlgoTrading.Models.Configuration;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text;
using DhanAlgoTrading.Models.DhanApi;

namespace DhanAlgoTrading.Services
{
    public class LiveOrderUpdateService : BackgroundService
    {
        private readonly ILogger<LiveOrderUpdateService> _logger;
        private readonly DhanApiSettings _apiSettings;
        private ClientWebSocket? _clientWebSocket;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Event raised whenever an order update is received from the WebSocket.
        /// </summary>
        public event EventHandler<OrderUpdateDataDto>? OrderUpdateReceived;

        private void PublishOrderUpdate(OrderUpdateDataDto data)
        {
            OrderUpdateReceived?.Invoke(this, data);
        }

        internal void ProcessMessage(string receivedJson)
        {
            var orderUpdateMsg = JsonSerializer.Deserialize<WebSocketOrderUpdateMessageDto>(receivedJson, _jsonSerializerOptions);
            if (orderUpdateMsg?.Data != null && "order_alert".Equals(orderUpdateMsg.Type, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "LiveOrderUpdateService: Parsed Order Update for OrderNo: {OrderNo}, DhanStatus: {DhanStatus}, Symbol: {Symbol}, Txn: {TxnType}, Qty: {Qty}, Price: {Price}, TradedQty: {TradedQty}, AvgPrice: {AvgPrice}",
                    orderUpdateMsg.Data.OrderNo,
                    orderUpdateMsg.Data.Status,
                    orderUpdateMsg.Data.Symbol,
                    orderUpdateMsg.Data.TxnType,
                    orderUpdateMsg.Data.Quantity,
                    orderUpdateMsg.Data.Price,
                    orderUpdateMsg.Data.TradedQty,
                    orderUpdateMsg.Data.AvgTradedPrice
                );

                PublishOrderUpdate(orderUpdateMsg.Data);
            }
            else
            {
                _logger.LogWarning("LiveOrderUpdateService: Received WebSocket message is not a recognized order_alert or data is null. Type: {Type}", orderUpdateMsg?.Type);
            }
        }

        public LiveOrderUpdateService(
            ILogger<LiveOrderUpdateService> logger,
            IOptions<DhanApiSettings> apiSettingsOptions)
        {
            _logger = logger;
            _apiSettings = apiSettingsOptions.Value;
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrWhiteSpace(_apiSettings.LiveOrderUpdateUrl) ||
                string.IsNullOrWhiteSpace(_apiSettings.AccessToken) || _apiSettings.AccessToken == "YOUR_DHAN_ACCESS_TOKEN_HERE" ||
                string.IsNullOrWhiteSpace(_apiSettings.ClientId) || _apiSettings.ClientId == "YOUR_DHAN_CLIENT_ID_HERE")
            {
                _logger.LogError("LiveOrderUpdateService cannot start: Key API settings (WebSocket URL, AccessToken, or ClientId) are missing or placeholders.");
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _clientWebSocket = new ClientWebSocket();
                    Uri serverUri = new Uri(_apiSettings.LiveOrderUpdateUrl);
                    _logger.LogInformation("LiveOrderUpdateService: Connecting to {WebSocketUrl}...", serverUri);

                    var connectCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                    connectCts.CancelAfter(TimeSpan.FromSeconds(30));

                    await _clientWebSocket.ConnectAsync(serverUri, connectCts.Token);
                    _logger.LogInformation("LiveOrderUpdateService: Successfully connected to WebSocket.");

                    await SendAuthorizationAsync(stoppingToken);
                    await ReceiveMessagesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("LiveOrderUpdateService: Operation cancelled via stoppingToken.");
                    break;
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogWarning(ex, "LiveOrderUpdateService: Connection attempt timed out.");
                }
                catch (WebSocketException wsEx)
                {
                    _logger.LogError(wsEx, "LiveOrderUpdateService: WebSocketException occurred. State: {State}. Will attempt to reconnect.", _clientWebSocket?.State);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "LiveOrderUpdateService: An unexpected error occurred. Will attempt to reconnect.");
                }
                finally
                {
                    if (_clientWebSocket?.State == WebSocketState.Open || _clientWebSocket?.State == WebSocketState.Aborted || _clientWebSocket?.State == WebSocketState.CloseReceived)
                    {
                        try
                        {
                            var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing connection", closeCts.Token);
                            _logger.LogInformation("LiveOrderUpdateService: WebSocket connection closed gracefully.");
                        }
                        catch (Exception closeEx)
                        {
                            _logger.LogError(closeEx, "LiveOrderUpdateService: Exception during WebSocket CloseAsync.");
                        }
                    }
                    _clientWebSocket?.Dispose();
                    _clientWebSocket = null;
                }

                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("LiveOrderUpdateService: Attempting to reconnect in 30 seconds...");
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("LiveOrderUpdateService: Reconnect delay cancelled.");
                        break;
                    }
                }
            }
            _logger.LogInformation("LiveOrderUpdateService: Stopped.");
        }

        private async Task SendAuthorizationAsync(CancellationToken stoppingToken)
        {
            if (_clientWebSocket?.State != WebSocketState.Open)
            {
                _logger.LogWarning("LiveOrderUpdateService: Cannot send authorization, WebSocket is not open.");
                return;
            }

            var authRequest = new WebSocketAuthRequestDto
            {
                LoginReq = new LoginRequestPayload
                {
                    ClientId = _apiSettings.ClientId,
                    Token = _apiSettings.AccessToken,
                    MsgCode = 42
                },
                UserType = "SELF"
            };

            string authJson = JsonSerializer.Serialize(authRequest, _jsonSerializerOptions);
            byte[] authBytes = Encoding.UTF8.GetBytes(authJson);
            var segment = new ArraySegment<byte>(authBytes);

            _logger.LogInformation("LiveOrderUpdateService: Sending authorization message: {AuthJson}", authJson);
            await _clientWebSocket.SendAsync(segment, WebSocketMessageType.Text, true, stoppingToken);
            _logger.LogInformation("LiveOrderUpdateService: Authorization message sent.");
        }

        private async Task ReceiveMessagesAsync(CancellationToken stoppingToken)
        {
            var buffer = new ArraySegment<byte>(new byte[1024 * 8]);

            while (_clientWebSocket?.State == WebSocketState.Open && !stoppingToken.IsCancellationRequested)
            {
                WebSocketReceiveResult result = null;
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    do
                    {
                        if (stoppingToken.IsCancellationRequested) break;

                        if (buffer.Array == null)
                        {
                            _logger.LogError("LiveOrderUpdateService: Buffer array is null before ReceiveAsync. This should not happen.");
                            throw new InvalidOperationException("WebSocket buffer array is null.");
                        }
                        result = await _clientWebSocket.ReceiveAsync(buffer, stoppingToken);

                        _logger.LogDebug("LiveOrderUpdateService: ReceiveAsync completed. MessageType: {MessageType}, Count: {Count}, EndOfMessage: {EndOfMessage}, CloseStatus: {CloseStatus}",
                                         result.MessageType, result.Count, result.EndOfMessage, result.CloseStatus);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            _logger.LogInformation("LiveOrderUpdateService: WebSocket close message received from server. Status: {CloseStatus}, Description: {Description}",
                                result.CloseStatus, result.CloseStatusDescription);
                            if (_clientWebSocket.State == WebSocketState.CloseReceived && result.CloseStatus.HasValue)
                            {
                                _logger.LogInformation("LiveOrderUpdateService: Server initiated close. Responding with CloseOutputAsync.");
                                var closeCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                                try
                                {
                                    await _clientWebSocket.CloseOutputAsync(result.CloseStatus.Value, result.CloseStatusDescription, closeCts.Token);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Exception during CloseOutputAsync after receiving close message.");
                                }
                            }
                            return;
                        }
                        memoryStream.Write(buffer.Array!, buffer.Offset, result.Count);
                    } while (!result.EndOfMessage);

                    if (stoppingToken.IsCancellationRequested) break;

                    memoryStream.Seek(0, System.IO.SeekOrigin.Begin);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedJson = Encoding.UTF8.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length);
                        _logger.LogDebug("LiveOrderUpdateService: Raw message received: {Json}", receivedJson);

                        try
                        {
                            ProcessMessage(receivedJson);
                        }
                        catch (JsonException jsonEx)
                        {
                            _logger.LogError(jsonEx, "LiveOrderUpdateService: Error deserializing WebSocket message: {Json}", receivedJson);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("LiveOrderUpdateService: Received non-text message type: {MessageType}", result.MessageType);
                    }
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("LiveOrderUpdateService: StopAsync called.");
            if (_clientWebSocket != null && _clientWebSocket.State == WebSocketState.Open)
            {
                var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                try
                {
                    await _clientWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Service stopping", cts.Token);
                    _logger.LogInformation("LiveOrderUpdateService: WebSocket CloseOutputAsync completed.");
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("LiveOrderUpdateService: WebSocket CloseOutputAsync timed out during StopAsync.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "LiveOrderUpdateService: Exception during WebSocket CloseOutputAsync in StopAsync.");
                }
            }
            _clientWebSocket?.Dispose();
            await base.StopAsync(stoppingToken);
            _logger.LogInformation("LiveOrderUpdateService: Service fully stopped.");
        }
    }

}
