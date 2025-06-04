using Microsoft.Extensions.Options; // For IOptions
using System.Net.Http.Headers; // For MediaTypeWithQualityHeaderValue
using System.Text.Json; // For JsonSerializer, JsonSerializerOptions
using System.Text.Json.Serialization; // For JsonIgnoreCondition
using System.Net.Http.Json;
using DhanAlgoTrading.Models.Configuration;
using DhanAlgoTrading.Models.DhanApi;
using System.Web;
using System.Net;

namespace DhanAlgoTrading.Api.Services
{
    public partial class DhanService : DhanAlgoTrading.Services.IDhanService
    {
        private readonly HttpClient _httpClient;
        private readonly DhanApiSettings _apiSettings;
        private readonly ILogger<DhanService> _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public DhanService(
            HttpClient httpClient, // Injected by AddHttpClient in Program.cs
            IOptions<DhanApiSettings> apiSettingsOptions, // Injected by DI
            ILogger<DhanService> logger) // Injected by DI
        {
            _httpClient = httpClient;
            _apiSettings = apiSettingsOptions.Value; // Get the configured settings
            _logger = logger;

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // Configure HttpClient instance (BaseAddress, Default Headers)
            if (string.IsNullOrWhiteSpace(_apiSettings.BaseUrl))
            {
                _logger.LogError("Dhan API BaseUrl is not configured in appsettings.");
                // Throw an exception to prevent the service from being used in an invalid state
                throw new InvalidOperationException("Dhan API BaseUrl must be configured.");
            }
            _httpClient.BaseAddress = new Uri(_apiSettings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrWhiteSpace(_apiSettings.AccessToken) && _apiSettings.AccessToken != "YOUR_DHAN_ACCESS_TOKEN_HERE")
            {
                // TryAddWithoutValidation is used because 'access-token' might not be a standard header format
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("access-token", _apiSettings.AccessToken);
            }
            else
            {
                _logger.LogWarning("Dhan API AccessToken is not configured or is using a placeholder value. API calls requiring authentication will likely fail.");
            }
        }

        // Retrieves the logged in user's profile information
        public async Task<DhanUserProfileDto?> GetUserProfileAsync()
        {
            _logger.LogInformation("GetUserProfileAsync called.");

            // Endpoint for retrieving the logged in user's profile details.
            // According to DhanHQ API documentation the profile information is
            // available via GET /profile. The BaseAddress is configured from
            // settings so we only provide the relative path here.
            var requestUri = "/profile";

            try
            {
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    // Set necessary headers, common in DhanHQ APIs [cite: 56, 60, 148, 150, 166]
                    if (!string.IsNullOrWhiteSpace(_apiSettings.AccessToken))
                    {
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiSettings.AccessToken);
                        requestMessage.Headers.TryAddWithoutValidation("access-token", _apiSettings.AccessToken);
                    }
                    if (!string.IsNullOrWhiteSpace(_apiSettings.ClientId))
                    {
                        requestMessage.Headers.TryAddWithoutValidation("client-id", _apiSettings.ClientId);
                    }

                    _logger.LogInformation("Sending request to {RequestUri}", requestUri);
                    HttpResponseMessage response = await _httpClient.SendAsync(requestMessage);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        _logger.LogInformation("Successfully received user profile status: {ResponseContent}", responseContent);

                        // Deserialize to our DTO
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        DhanUserProfileDto? profileStatus = JsonSerializer.Deserialize<DhanUserProfileDto>(responseContent, options);

                        if (profileStatus != null)
                        {
                            // Format the DTO into a string to match your method's return signature.
                            // You could also return the JSON string directly: return responseContent;
                            // Or, ideally, change the method signature to return Task<DhanUserProfileDto?>
                            // and handle the object in the calling code.
                            return profileStatus;
                        }
                        else
                        {
                            _logger.LogWarning("Failed to deserialize user profile status from response: {ResponseContent}", responseContent);
                            return null;
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogError("Failed to get user profile status. Status Code: {StatusCode}, Reason: {ReasonPhrase}, Content: {ErrorContent}",
                            response.StatusCode, response.ReasonPhrase, errorContent);
                        // Refer to API error codes in the PDF [cite: 47, 49] if available for this endpoint.
                        return null;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while getting user profile status");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization failed while processing user profile status");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while getting user profile status");
                return null;
            }
        }

        public async Task<IEnumerable<string>> GetExpiryDatesAsync(ExpiryListRequestDto expiryRequest)
        {
            if (expiryRequest == null)
            {
                _logger.LogWarning("ExpiryListRequestDto cannot be null for GetExpiryDatesAsync.");
                return Enumerable.Empty<string>();
            }

            var requestUri = "/v2/optionchain/expirylist";
            _logger.LogInformation("Fetching expiry dates via POST to URI: {RequestUri} with Request: {@ExpiryRequest}", requestUri, expiryRequest);

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = JsonContent.Create(expiryRequest, options: _jsonSerializerOptions)
                };

                if (!string.IsNullOrWhiteSpace(_apiSettings.ClientId) && _apiSettings.ClientId != "YOUR_DHAN_CLIENT_ID_HERE")
                {
                    requestMessage.Headers.TryAddWithoutValidation("client-id", _apiSettings.ClientId);
                }
                else
                {
                    _logger.LogWarning("ClientId header not added for GetExpiryDatesAsync as it's not configured or is a placeholder.");
                }

                var httpResponse = await _httpClient.SendAsync(requestMessage);
                var responseContentString = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var expiryResponse = JsonSerializer.Deserialize<DhanExpiryDatesResponseDto>(responseContentString, _jsonSerializerOptions);
                    if (expiryResponse?.Data != null)
                    {
                        _logger.LogInformation("Received {Count} expiry dates.", expiryResponse.Data.Count);
                        return expiryResponse.Data;
                    }

                    _logger.LogWarning("API call returned success but response body was null or invalid. Body: {ResponseBody}", responseContentString);
                    return Enumerable.Empty<string>();
                }
                else
                {
                    _logger.LogError("HTTP error {StatusCode} while fetching expiry dates. Response: {ErrorContent}", httpResponse.StatusCode, responseContentString);
                    return Enumerable.Empty<string>();
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request exception while fetching expiry dates.");
                return Enumerable.Empty<string>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while fetching expiry dates.");
                return Enumerable.Empty<string>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching expiry dates.");
                return Enumerable.Empty<string>();
            }
        }

        public async Task<IEnumerable<OptionInstrument>> GetOptionChainAsync(OptionChainRequestDto optionChainRequest)
        {
            if (optionChainRequest == null)
            {
                _logger.LogWarning("OptionChainRequestDto cannot be null for GetOptionChainAsync.");
                return Enumerable.Empty<OptionInstrument>();
            }

            var requestUri = "/v2/optionchain";
            _logger.LogInformation("Fetching option chain via POST to URI: {RequestUri} with Request: {@OptionChainRequest}", requestUri, optionChainRequest);

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri)
                {
                    Content = JsonContent.Create(optionChainRequest, options: _jsonSerializerOptions)
                };

                if (!string.IsNullOrWhiteSpace(_apiSettings.ClientId) && _apiSettings.ClientId != "YOUR_DHAN_CLIENT_ID_HERE")
                {
                    requestMessage.Headers.TryAddWithoutValidation("client-id", _apiSettings.ClientId);
                }
                else
                {
                    _logger.LogWarning("ClientId header not added for GetOptionChainAsync as it's not configured or is a placeholder.");
                }

                var httpResponse = await _httpClient.SendAsync(requestMessage);
                var responseContentString = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var instruments = JsonSerializer.Deserialize<List<OptionInstrument>>(responseContentString, _jsonSerializerOptions);
                    if (instruments != null)
                    {
                        _logger.LogInformation("Received {Count} option instruments.", instruments.Count);
                        return instruments;
                    }

                    _logger.LogWarning("Option chain API call returned success but response body was null or invalid. Body: {ResponseBody}", responseContentString);
                    return Enumerable.Empty<OptionInstrument>();
                }
                else
                {
                    _logger.LogError("HTTP error {StatusCode} while fetching option chain. Response: {ErrorContent}", httpResponse.StatusCode, responseContentString);
                    return Enumerable.Empty<OptionInstrument>();
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request exception while fetching option chain.");
                return Enumerable.Empty<OptionInstrument>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while fetching option chain.");
                return Enumerable.Empty<OptionInstrument>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching option chain.");
                return Enumerable.Empty<OptionInstrument>();
            }
        }

        public async Task<OrderResponseDto?> PlaceOrderAsync(OrderRequestDto orderRequest)
        {
            if (orderRequest == null)
            {
                _logger.LogWarning("OrderRequestDto cannot be null for PlaceOrderAsync.");
                return new OrderResponseDto { CustomStatus = "ValidationFailed", CustomMessage = "Order request cannot be null." };
            }

            // Ensure dhanClientId is set from settings, as it's part of the request body for POST /v2/orders
            if (string.IsNullOrWhiteSpace(_apiSettings.ClientId) || _apiSettings.ClientId == "YOUR_DHAN_CLIENT_ID_HERE")
            {
                _logger.LogError("Dhan ClientId is not configured. Cannot place order.");
                return new OrderResponseDto { CustomStatus = "ConfigurationError", CustomMessage = "Dhan ClientId not configured." };
            }
            orderRequest.DhanClientId = _apiSettings.ClientId;


            var requestUri = "/v2/orders";
            _logger.LogInformation("Placing order via POST to URI: {RequestUri} with Order: {@OrderRequest}", requestUri, orderRequest);

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
                requestMessage.Content = JsonContent.Create(orderRequest, options: _jsonSerializerOptions);

                // The /v2/orders endpoint might not need a separate 'client-id' header
                // if 'dhanClientId' is in the body and 'access-token' is in the header.
                // This was needed for /v2/optionchain/expirylist, but verify for /v2/orders.
                // For now, let's assume it's NOT needed in the header if it's in the body for /v2/orders.
                // If testing shows it's required, uncomment the lines below and ensure _apiSettings.ClientId is available.
                /*
                if (!string.IsNullOrWhiteSpace(_apiSettings.ClientId) && _apiSettings.ClientId != "YOUR_DHAN_CLIENT_ID_HERE")
                {
                    requestMessage.Headers.TryAddWithoutValidation("client-id", _apiSettings.ClientId);
                }
                else
                {
                     _logger.LogWarning("ClientId header not added for PlaceOrderAsync as it's not configured or is a placeholder.");
                }
                */

                var httpResponse = await _httpClient.SendAsync(requestMessage);
                var responseContentString = await httpResponse.Content.ReadAsStringAsync(); // Read content for logging/parsing

                if (httpResponse.IsSuccessStatusCode)
                {
                    var orderResponse = JsonSerializer.Deserialize<OrderResponseDto>(responseContentString, _jsonSerializerOptions);
                    if (orderResponse != null)
                    {
                        _logger.LogInformation("Order placement API call successful or processed. Order ID: {OrderId}, API Status: {ApiOrderStatus}, Dhan Message: {DhanMessage}",
                                               orderResponse.OrderId, orderResponse.ApiOrderStatus, orderResponse.DhanMessage);
                        orderResponse.CustomStatus = "ApiSuccess";
                        return orderResponse;
                    }
                    _logger.LogWarning("Order placement API call returned success status code, but response body was null or invalid. Body: {ResponseBody}", responseContentString);
                    return new OrderResponseDto { CustomStatus = "ApiSuccessWithInvalidResponse", CustomMessage = "API success, but no valid response body." };
                }
                else
                {
                    _logger.LogError("HTTP error {StatusCode} while placing order. Response: {ErrorContent}", httpResponse.StatusCode, responseContentString);
                    OrderResponseDto? errorResponse = null;
                    try
                    {
                        errorResponse = JsonSerializer.Deserialize<OrderResponseDto>(responseContentString, _jsonSerializerOptions);
                        _logger.LogInformation("Parsed error response from Dhan: OrderId={OrderId}, ApiOrderStatus={ApiOrderStatus}, DhanMessage={DhanMessage}, ErrorCode={ErrorCode}",
                                               errorResponse?.OrderId, errorResponse?.ApiOrderStatus, errorResponse?.DhanMessage, errorResponse?.ErrorCode);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Could not parse error response from Dhan as OrderResponseDto. Raw error: {ErrorContent}", responseContentString);
                    }

                    return new OrderResponseDto
                    {
                        CustomStatus = "ApiError",
                        CustomMessage = $"HTTP Error: {httpResponse.StatusCode}. Details: {responseContentString}",
                        OrderId = errorResponse?.OrderId, // Propagate if available
                        ApiOrderStatus = errorResponse?.ApiOrderStatus ?? httpResponse.StatusCode.ToString(),
                        DhanMessage = errorResponse?.DhanMessage,
                        ErrorCode = errorResponse?.ErrorCode
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request exception while placing order.");
                return new OrderResponseDto { CustomStatus = "HttpRequestFailed", CustomMessage = httpEx.Message };
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON exception while placing order or processing response.");
                return new OrderResponseDto { CustomStatus = "JsonProcessingFailed", CustomMessage = jsonEx.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while placing order.");
                return new OrderResponseDto { CustomStatus = "UnexpectedError", CustomMessage = ex.Message };
            }
        }

        // --- Methods for Part 4: Fetching Order Status & Positions ---
        public async Task<OrderDataDto?> GetOrderStatusAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                _logger.LogWarning("OrderId cannot be empty for GetOrderStatusAsync.");
                return null;
            }

            var requestUri = $"/v2/orders/{orderId}";
            _logger.LogInformation("Fetching order status from URI: {RequestUri}", requestUri);

            try
            {
                // This endpoint likely does not require the client-id header if access-token is already set globally
                var response = await _httpClient.GetAsync(requestUri); // GET request

                if (response.IsSuccessStatusCode)
                {
                    var orderData = await response.Content.ReadFromJsonAsync<OrderDataDto>(_jsonSerializerOptions);
                    _logger.LogInformation("Successfully fetched status for Order ID: {OrderId}", orderId);
                    return orderData;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("HTTP error {StatusCode} while fetching status for Order ID: {OrderId}. Response: {ErrorContent}",
                                     response.StatusCode, orderId, errorContent);
                    return null;
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request exception while fetching status for Order ID: {OrderId}.", orderId);
                return null;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while fetching status for Order ID: {OrderId}.", orderId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching status for Order ID: {OrderId}.", orderId);
                return null;
            }
        }

        public async Task<IEnumerable<OrderDataDto>> GetOrderBookAsync()
        {
            var requestUri = "/v2/orders"; // GET request for all orders (or paginated)
            _logger.LogInformation("Fetching order book from URI: {RequestUri}", requestUri);

            try
            {
                var orderBook = await _httpClient.GetFromJsonAsync<List<OrderDataDto>>(requestUri, _jsonSerializerOptions);
                if (orderBook != null)
                {
                    _logger.LogInformation("Successfully fetched {Count} orders for the order book.", orderBook.Count);
                    return orderBook;
                }
                _logger.LogWarning("Received null or empty list for order book from {RequestUri}", requestUri);
                return Enumerable.Empty<OrderDataDto>();
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error while fetching order book from {RequestUri}.", requestUri);
                return Enumerable.Empty<OrderDataDto>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while fetching order book from {RequestUri}.", requestUri);
                return Enumerable.Empty<OrderDataDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching order book from {RequestUri}.", requestUri);
                return Enumerable.Empty<OrderDataDto>();
            }
        }

        public async Task<PositionBookDto?> GetPositionsAsync()
        {
            var requestUri = "/v2/positions"; // GET request
            _logger.LogInformation("Fetching positions from URI: {RequestUri}", requestUri);

            try
            {
                // The /v2/positions endpoint often returns a wrapper object that contains a list of positions
                // and potentially summary data. Adjust DTO 'PositionBookDto' accordingly.
                var positionBook = await _httpClient.GetFromJsonAsync<PositionBookDto>(requestUri, _jsonSerializerOptions);
                _logger.LogInformation("Successfully fetched positions.");
                return positionBook;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error while fetching positions from {RequestUri}.", requestUri);
                return null;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while fetching positions from {RequestUri}.", requestUri);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching positions from {RequestUri}.", requestUri);
                return null;
            }
        }
        // --- Method for Part 5A: Fetching Trade Book ---
        public async Task<IEnumerable<TradeDataDto>> GetTradeBookAsync(string? orderId = null)
        {
            var requestUri = "/v2/trades";
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                // If Dhan API supports filtering trades by order_id via query parameter.
                // Example: /v2/trades?order_id={orderId}
                // VERIFY THIS QUERY PARAMETER NAME FROM DHAN DOCS.
                var query = HttpUtility.ParseQueryString(string.Empty);
                query["order_id"] = orderId; // Assuming 'order_id' is the query param name
                requestUri = $"/v2/trades?{query.ToString()}";
                _logger.LogInformation("Fetching trade book for Order ID {OrderId} from URI: {RequestUri}", orderId, requestUri);
            }
            else
            {
                _logger.LogInformation("Fetching all trades for the day from URI: {RequestUri}", requestUri);
            }

            try
            {
                // This endpoint might not need the client-id header if access-token is sufficient. Verify.
                var trades = await _httpClient.GetFromJsonAsync<List<TradeDataDto>>(requestUri, _jsonSerializerOptions);
                if (trades != null)
                {
                    _logger.LogInformation("Successfully fetched {Count} trades.", trades.Count);
                    return trades;
                }
                _logger.LogWarning("Received null or empty list for trade book from {RequestUri}", requestUri);
                return Enumerable.Empty<TradeDataDto>();
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error while fetching trade book from {RequestUri}.", requestUri);
                return Enumerable.Empty<TradeDataDto>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while fetching trade book from {RequestUri}.", requestUri);
                return Enumerable.Empty<TradeDataDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching trade book from {RequestUri}.", requestUri);
                return Enumerable.Empty<TradeDataDto>();
            }
        }
        // --- Method for Part 5B: Squaring Off Positions ---
        public async Task<OrderResponseDto?> SquareOffPositionAsync(SquareOffRequestDto squareOffRequest)
        {
            if (squareOffRequest == null)
            {
                _logger.LogWarning("SquareOffRequestDto cannot be null.");
                return new OrderResponseDto { CustomStatus = "ValidationFailed", CustomMessage = "SquareOff request cannot be null." };
            }

            // Query current positions to find the one to square off
            // This is a simplified approach. A more robust solution might directly query the specific position
            // or take more details if a user can have multiple products for the same securityId (e.g. different expiries)
            // For now, we assume securityId + productType is unique enough, or the GetPositionsAsync returns identifiable positions.

            var positionsResponse = await GetPositionsAsync();
            PositionDataDto? positionToSquare = null;

            if (positionsResponse?.Positions != null)
            {
                // Attempt to find the position based on securityId.
                // You might need more criteria if a securityId can have positions in different product types (e.g. INTRADAY and NORMAL)
                // or different segments, though squareOffRequest should specify these.
                positionToSquare = positionsResponse.Positions.FirstOrDefault(p =>
                    p.SecurityId == squareOffRequest.SecurityId &&
                    p.ProductType == squareOffRequest.ProductType && // Ensure squaring off correct product type
                    p.ExchangeSegment == squareOffRequest.ExchangeSegment // Ensure squaring off correct segment
                                                                          // Potentially add more filters like trading symbol if needed for absolute certainty
                );
            }

            if (positionToSquare == null || positionToSquare.Quantity == 0)
            {
                _logger.LogWarning("No open position found for Security ID: {SecurityId}, ProductType: {ProductType}, ExchangeSegment: {ExchangeSegment} to square off, or quantity is zero.",
                                   squareOffRequest.SecurityId, squareOffRequest.ProductType, squareOffRequest.ExchangeSegment);
                return new OrderResponseDto { CustomStatus = "NoPositionFound", CustomMessage = "No open position found to square off or quantity is zero." };
            }

            // Determine transaction type for square-off
            // If net quantity is positive (long), we sell. If negative (short), we buy.
            string transactionType = positionToSquare.Quantity > 0 ? "SELL" : "BUY";
            int quantityToSquareOff = Math.Abs(positionToSquare.Quantity);

            _logger.LogInformation("Attempting to square off position for Security ID: {SecurityId}. Current Qty: {CurrentQty}. Action: {TransactionType} {QuantityToSquareOff}",
                positionToSquare.SecurityId, positionToSquare.Quantity, transactionType, quantityToSquareOff);

            var orderRequest = new OrderRequestDto
            {
                // DhanClientId will be set by PlaceOrderAsync
                TransactionType = transactionType,
                ExchangeSegment = positionToSquare.ExchangeSegment, // Use segment from position
                ProductType = positionToSquare.ProductType,       // Use product type from position
                OrderType = squareOffRequest.OrderType ?? "MARKET", // Default to MARKET if not specified
                SecurityId = positionToSquare.SecurityId,
                Quantity = quantityToSquareOff,
                Price = "MARKET".Equals(squareOffRequest.OrderType, StringComparison.OrdinalIgnoreCase) ? 0 : squareOffRequest.Price, // Price only for LIMIT
                TriggerPrice = squareOffRequest.TriggerPrice, // For SL orders
                Validity = squareOffRequest.Validity ?? "DAY", // Default to DAY
                CorrelationId = squareOffRequest.CorrelationId ?? $"SQOFF_{positionToSquare.SecurityId}_{DateTime.UtcNow.Ticks}"
                // Other fields like AfterMarketOrder can be added if needed for square-off
            };

            return await PlaceOrderAsync(orderRequest);
        }

        // --- Methods for Part 6: Fund and Margin Checks ---
        public async Task<FundLimitResponseDto?> GetFundLimitsAsync()
        {
            var requestUri = "/v2/fundlimit"; // As per PDF page 19 [cite: 60] (GET request)
            _logger.LogInformation("Fetching fund limits from URI: {RequestUri}", requestUri);

            try
            {
                // This GET request likely only needs the access-token (already in default headers)
                var fundLimits = await _httpClient.GetFromJsonAsync<FundLimitResponseDto>(requestUri, _jsonSerializerOptions);
                if (fundLimits != null)
                {
                    _logger.LogInformation("Successfully fetched fund limits for Client ID: {DhanClientId}", fundLimits.DhanClientId);
                    return fundLimits;
                }
                _logger.LogWarning("Received null response when fetching fund limits from {RequestUri}", requestUri);
                return null;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error while fetching fund limits from {RequestUri}.", requestUri);
                return null;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while fetching fund limits from {RequestUri}.", requestUri);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching fund limits from {RequestUri}.", requestUri);
                return null;
            }
        }

        public async Task<MarginCalculatorResponseDto?> GetOrderMarginsAsync(MarginCalculatorRequestDto marginRequest)
        {
            if (marginRequest == null)
            {
                _logger.LogWarning("MarginCalculatorRequestDto cannot be null.");
                return null;
            }

            // Ensure dhanClientId is set from settings for the request body
            if (string.IsNullOrWhiteSpace(_apiSettings.ClientId) || _apiSettings.ClientId == "YOUR_DHAN_CLIENT_ID_HERE")
            {
                _logger.LogError("Dhan ClientId is not configured. Cannot calculate margin.");
                return new MarginCalculatorResponseDto { ErrorMessage = "Dhan ClientId not configured." };
            }
            marginRequest.DhanClientId = _apiSettings.ClientId;


            var requestUri = "/v2/margincalculator"; // As per PDF page 19 [cite: 56] (POST request)
            _logger.LogInformation("Calculating order margins via POST to URI: {RequestUri} with Request: {@MarginRequest}",
                                   requestUri, marginRequest);

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
                requestMessage.Content = JsonContent.Create(marginRequest, options: _jsonSerializerOptions);

                // The sample cURL for margincalculator on PDF page 19 includes 'client-id' header.
                // However, the request body also includes 'dhanClientId'.
                // Let's add 'client-id' header for consistency with other POSTs that needed it,
                // but this should be verified if it's strictly necessary when dhanClientId is in the body.
                if (!string.IsNullOrWhiteSpace(_apiSettings.ClientId) && _apiSettings.ClientId != "YOUR_DHAN_CLIENT_ID_HERE")
                {
                    requestMessage.Headers.TryAddWithoutValidation("client-id", _apiSettings.ClientId);
                }
                else
                {
                    _logger.LogWarning("ClientId header not added for GetOrderMarginsAsync as it's not configured or is a placeholder.");
                }

                var httpResponse = await _httpClient.SendAsync(requestMessage);
                var responseContentString = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var marginResponse = JsonSerializer.Deserialize<MarginCalculatorResponseDto>(responseContentString, _jsonSerializerOptions);
                    if (marginResponse != null)
                    {
                        _logger.LogInformation("Successfully calculated margins. Total Margin: {TotalMargin}", marginResponse.TotalMargin);
                        return marginResponse;
                    }
                    _logger.LogWarning("Margin calculation API call returned success status, but response body was null or invalid. Body: {ResponseBody}", responseContentString);
                    return new MarginCalculatorResponseDto { ErrorMessage = "API success, but no valid response body." };
                }
                else
                {
                    _logger.LogError("HTTP error {StatusCode} while calculating margins. Response: {ErrorContent}", httpResponse.StatusCode, responseContentString);
                    // Try to parse a generic error if possible, or just return the message
                    return new MarginCalculatorResponseDto { ErrorMessage = $"HTTP Error: {httpResponse.StatusCode}. Details: {responseContentString}" };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request exception while calculating margins.");
                return new MarginCalculatorResponseDto { ErrorMessage = $"HTTP Request Exception: {httpEx.Message}" };
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON exception while calculating margins or processing response.");
                return new MarginCalculatorResponseDto { ErrorMessage = $"JSON Processing Exception: {jsonEx.Message}" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while calculating margins.");
                return new MarginCalculatorResponseDto { ErrorMessage = $"Unexpected Error: {ex.Message}" };
            }
        }

        public async Task<SuperOrderResponseDto?> PlaceSuperOrderAsync(SuperOrderRequestDto superOrderRequest)
        {
            if (superOrderRequest == null)
            {
                _logger.LogWarning("SuperOrderRequestDto cannot be null for PlaceSuperOrderAsync.");
                return new SuperOrderResponseDto { CustomStatus = "ValidationFailed", CustomMessage = "Super order request cannot be null." };
            }

            // Ensure dhanClientId is set from settings for the request body
            if (string.IsNullOrWhiteSpace(_apiSettings.ClientId) || _apiSettings.ClientId == "YOUR_DHAN_CLIENT_ID_HERE")
            {
                _logger.LogError("Dhan ClientId is not configured. Cannot place super order.");
                return new SuperOrderResponseDto { CustomStatus = "ConfigurationError", CustomMessage = "Dhan ClientId not configured." };
            }
            superOrderRequest.DhanClientId = _apiSettings.ClientId;

            var requestUri = "/v2/super/orders"; // Endpoint from PDF page 101
            _logger.LogInformation("Placing Super Order via POST to URI: {RequestUri} with Request: {@SuperOrderRequest}",
                                   requestUri, superOrderRequest);

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
                requestMessage.Content = JsonContent.Create(superOrderRequest, options: _jsonSerializerOptions);

                var httpResponse = await _httpClient.SendAsync(requestMessage);
                var responseContentString = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var superOrderResponse = JsonSerializer.Deserialize<SuperOrderResponseDto>(responseContentString, _jsonSerializerOptions);
                    if (superOrderResponse != null)
                    {
                        _logger.LogInformation("Super Order placement API call processed. Order ID: {OrderId}, API Order Status: {ApiOrderStatus}",
                                               superOrderResponse.OrderId, superOrderResponse.ApiOrderStatus);
                        superOrderResponse.CustomStatus = "ApiSuccess";
                        return superOrderResponse;
                    }
                    _logger.LogWarning("Super Order placement API call returned success status, but response body was null or invalid. Body: {ResponseBody}", responseContentString);
                    return new SuperOrderResponseDto { CustomStatus = "ApiSuccessWithInvalidResponse", CustomMessage = "API success, but no valid response body." };
                }
                else
                {
                    _logger.LogError("HTTP error {StatusCode} while placing super order. Response: {ErrorContent}", httpResponse.StatusCode, responseContentString);
                    SuperOrderResponseDto? errorResponse = null;
                    try
                    {
                        // Attempt to deserialize the error content into the SuperOrderResponseDto
                        // This assumes Dhan might return a structured error with fields like errorCode, errorMessage
                        // that match properties in SuperOrderResponseDto (after adding them).
                        errorResponse = JsonSerializer.Deserialize<SuperOrderResponseDto>(responseContentString, _jsonSerializerOptions);
                        _logger.LogInformation("Parsed error response from Dhan for Super Order: OrderId={OrderId}, ApiOrderStatus={ApiOrderStatus}, DhanErrorCode={DhanErrorCode}, DhanErrorMessage={DhanErrorMessage}",
                                               errorResponse?.OrderId, errorResponse?.ApiOrderStatus, errorResponse?.DhanErrorCode, errorResponse?.DhanErrorMessage);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Could not parse error response from Dhan as SuperOrderResponseDto. Raw error: {ErrorContent}", responseContentString);
                    }

                    return new SuperOrderResponseDto
                    {
                        CustomStatus = "ApiError",
                        CustomMessage = $"HTTP Error: {httpResponse.StatusCode}. Details: {responseContentString}",
                        ApiOrderStatus = errorResponse?.ApiOrderStatus ?? httpResponse.StatusCode.ToString(),
                        DhanErrorCode = errorResponse?.DhanErrorCode, // Populate from parsed error if available
                        DhanErrorMessage = errorResponse?.DhanErrorMessage // Populate from parsed error if available
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request exception while placing super order.");
                return new SuperOrderResponseDto { CustomStatus = "HttpRequestFailed", CustomMessage = httpEx.Message };
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON exception while placing super order or processing response.");
                return new SuperOrderResponseDto { CustomStatus = "JsonProcessingFailed", CustomMessage = jsonEx.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while placing super order.");
                return new SuperOrderResponseDto { CustomStatus = "UnexpectedError", CustomMessage = ex.Message };
            }
        }
        public async Task<IEnumerable<SuperOrderDataDto>> GetSuperOrdersAsync()
        {
            var requestUri = "/v2/super/orders";
            _logger.LogInformation("Fetching Super Orders from URI: {RequestUri}", requestUri);

            try
            {
                // The sample cURL for this GET request on PDF page 104 shows 'access-token' (default)
                // and 'Content-Type: application/json' (usually not strictly needed for GET with no body,
                // but HttpClient usually adds an Accept header for application/json anyway).
                // No separate 'client-id' header is shown in the sample for this GET request.
                var superOrders = await _httpClient.GetFromJsonAsync<List<SuperOrderDataDto>>(requestUri, _jsonSerializerOptions);

                if (superOrders != null)
                {
                    _logger.LogInformation("Successfully fetched {Count} super orders.", superOrders.Count);
                    return superOrders;
                }

                _logger.LogWarning("Received null or empty list for super orders from {RequestUri}", requestUri);
                return Enumerable.Empty<SuperOrderDataDto>();
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error (Status: {StatusCode}) while fetching super orders from {RequestUri}.", httpEx.StatusCode, requestUri);
                return Enumerable.Empty<SuperOrderDataDto>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while fetching super orders from {RequestUri}.", requestUri);
                return Enumerable.Empty<SuperOrderDataDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching super orders from {RequestUri}.", requestUri);
                return Enumerable.Empty<SuperOrderDataDto>();
            }
        }

        public async Task<SuperOrderResponseDto?> ModifySuperOrderAsync(string orderId, ModifySuperOrderRequestDto modifyRequest)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                _logger.LogWarning("OrderId cannot be empty for ModifySuperOrderAsync.");
                return new SuperOrderResponseDto { CustomStatus = "ValidationFailed", CustomMessage = "Order ID cannot be empty." };
            }
            if (modifyRequest == null)
            {
                _logger.LogWarning("ModifySuperOrderRequestDto cannot be null.");
                return new SuperOrderResponseDto { CustomStatus = "ValidationFailed", CustomMessage = "Modify super order request cannot be null." };
            }

            // Ensure dhanClientId is set from settings, as it's part of the request body
            if (string.IsNullOrWhiteSpace(_apiSettings.ClientId) || _apiSettings.ClientId == "YOUR_DHAN_CLIENT_ID_HERE")
            {
                _logger.LogError("Dhan ClientId is not configured. Cannot modify super order.");
                return new SuperOrderResponseDto { CustomStatus = "ConfigurationError", CustomMessage = "Dhan ClientId not configured." };
            }
            modifyRequest.DhanClientId = _apiSettings.ClientId;
            modifyRequest.OrderId = orderId; // Ensure OrderId from path is in the body as per Dhan docs.

            var requestUri = $"/v2/super/orders/{orderId}";
            _logger.LogInformation("Modifying Super Order via PUT to URI: {RequestUri} with Request: {@ModifyRequest}",
                                   requestUri, modifyRequest);

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Put, requestUri);
                requestMessage.Content = JsonContent.Create(modifyRequest, options: _jsonSerializerOptions);

                // Sample cURL for PUT /super/orders/{order-id} (page 102) shows access-token and Content-Type,
                // but not a separate client-id header, as dhanClientId is in the body.

                var httpResponse = await _httpClient.SendAsync(requestMessage);
                var responseContentString = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var superOrderResponse = JsonSerializer.Deserialize<SuperOrderResponseDto>(responseContentString, _jsonSerializerOptions);
                    if (superOrderResponse != null)
                    {
                        _logger.LogInformation("Super Order modification API call processed. Order ID: {OrderId}, API Order Status: {ApiOrderStatus}",
                                               superOrderResponse.OrderId, superOrderResponse.ApiOrderStatus);
                        superOrderResponse.CustomStatus = "ApiSuccess";
                        return superOrderResponse;
                    }
                    _logger.LogWarning("Super Order modification API call returned success status, but response body was null or invalid. Body: {ResponseBody}", responseContentString);
                    return new SuperOrderResponseDto { CustomStatus = "ApiSuccessWithInvalidResponse", CustomMessage = "API success, but no valid response body." };
                }
                else
                {
                    _logger.LogError("HTTP error {StatusCode} while modifying super order. Response: {ErrorContent}", httpResponse.StatusCode, responseContentString);
                    SuperOrderResponseDto? errorResponse = null;
                    try
                    {
                        errorResponse = JsonSerializer.Deserialize<SuperOrderResponseDto>(responseContentString, _jsonSerializerOptions);
                        _logger.LogInformation("Parsed error response from Dhan for Super Order modification: OrderId={OrderId}, ApiOrderStatus={ApiOrderStatus}, DhanErrorCode={DhanErrorCode}, DhanErrorMessage={DhanErrorMessage}",
                                               errorResponse?.OrderId, errorResponse?.ApiOrderStatus, errorResponse?.DhanErrorCode, errorResponse?.DhanErrorMessage);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Could not parse error response from Dhan as SuperOrderResponseDto during modification. Raw error: {ErrorContent}", responseContentString);
                    }

                    return new SuperOrderResponseDto
                    {
                        CustomStatus = "ApiError",
                        CustomMessage = $"HTTP Error: {httpResponse.StatusCode}. Details: {responseContentString}",
                        OrderId = orderId, // Return the original orderId for context
                        ApiOrderStatus = errorResponse?.ApiOrderStatus ?? httpResponse.StatusCode.ToString(),
                        DhanErrorCode = errorResponse?.DhanErrorCode,
                        DhanErrorMessage = errorResponse?.DhanErrorMessage
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request exception while modifying super order {OrderId}.", orderId);
                return new SuperOrderResponseDto { OrderId = orderId, CustomStatus = "HttpRequestFailed", CustomMessage = httpEx.Message };
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON exception while modifying super order {OrderId} or processing response.", orderId);
                return new SuperOrderResponseDto { OrderId = orderId, CustomStatus = "JsonProcessingFailed", CustomMessage = jsonEx.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while modifying super order {OrderId}.", orderId);
                return new SuperOrderResponseDto { OrderId = orderId, CustomStatus = "UnexpectedError", CustomMessage = ex.Message };
            }
        }

        public async Task<SuperOrderResponseDto?> CancelSuperOrderLegAsync(string orderId, string orderLeg)
        {
            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(orderLeg))
            {
                _logger.LogWarning("OrderId and OrderLeg cannot be empty for CancelSuperOrderLegAsync.");
                return new SuperOrderResponseDto { CustomStatus = "ValidationFailed", CustomMessage = "Order ID and Order Leg must be provided." };
            }

            // Validate orderLeg against known values if necessary, e.g., "ENTRY_LEG", "TARGET_LEG", "STOP_LOSS_LEG"
            // For simplicity, we assume the caller provides a valid leg name.

            var requestUri = $"/v2/super/orders/{orderId}/{orderLeg.ToUpper()}"; // Ensure orderLeg is uppercase if API expects it
            _logger.LogInformation("Canceling Super Order Leg via DELETE to URI: {RequestUri}", requestUri);

            try
            {
                // Sample cURL for DELETE /super/orders/{order-id}/{order-leg} (page 104) shows access-token and Content-Type.
                // No separate client-id header is shown. No request body for DELETE.

                var httpResponse = await _httpClient.DeleteAsync(requestUri);
                var responseContentString = await httpResponse.Content.ReadAsStringAsync();

                // Dhan docs say "On successful completion of request ‘202 Accepted’ response status code will appear."
                // And the response structure shown is {"orderId": "...", "orderStatus": "CANCELLED"}
                if (httpResponse.IsSuccessStatusCode || httpResponse.StatusCode == HttpStatusCode.Accepted)
                {
                    SuperOrderResponseDto? cancelResponse = null;
                    if (!string.IsNullOrWhiteSpace(responseContentString))
                    {
                        try
                        {
                            cancelResponse = JsonSerializer.Deserialize<SuperOrderResponseDto>(responseContentString, _jsonSerializerOptions);
                        }
                        catch (JsonException jsonEx)
                        {
                            _logger.LogWarning(jsonEx, "Could not parse successful cancellation response as SuperOrderResponseDto. Raw: {ResponseContent}", responseContentString);
                        }
                    }

                    if (cancelResponse == null) // If body was empty or not parsable but status was success/accepted
                    {
                        cancelResponse = new SuperOrderResponseDto { OrderId = orderId, ApiOrderStatus = "CANCELLED" }; // Assume success
                    }

                    _logger.LogInformation("Super Order Leg cancellation API call processed for Order ID: {OrderId}, Leg: {OrderLeg}. API Status: {ApiOrderStatus}",
                                           cancelResponse.OrderId ?? orderId, orderLeg, cancelResponse.ApiOrderStatus);
                    cancelResponse.CustomStatus = "ApiSuccess";
                    return cancelResponse;
                }
                else
                {
                    _logger.LogError("HTTP error {StatusCode} while canceling super order leg. Order ID: {OrderId}, Leg: {OrderLeg}. Response: {ErrorContent}",
                                     httpResponse.StatusCode, orderId, orderLeg, responseContentString);
                    SuperOrderResponseDto? errorResponse = null;
                    try
                    {
                        errorResponse = JsonSerializer.Deserialize<SuperOrderResponseDto>(responseContentString, _jsonSerializerOptions);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Could not parse error response from Dhan as SuperOrderResponseDto during cancellation. Raw error: {ErrorContent}", responseContentString);
                    }

                    return new SuperOrderResponseDto
                    {
                        CustomStatus = "ApiError",
                        CustomMessage = $"HTTP Error: {httpResponse.StatusCode}. Details: {responseContentString}",
                        OrderId = orderId,
                        ApiOrderStatus = errorResponse?.ApiOrderStatus ?? httpResponse.StatusCode.ToString(),
                        DhanErrorCode = errorResponse?.DhanErrorCode,
                        DhanErrorMessage = errorResponse?.DhanErrorMessage
                    };
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request exception while canceling super order leg {OrderId}/{OrderLeg}.", orderId, orderLeg);
                return new SuperOrderResponseDto { OrderId = orderId, CustomStatus = "HttpRequestFailed", CustomMessage = httpEx.Message };
            }
            catch (JsonException jsonEx) // This might catch errors from ReadFromJsonAsync if used, or parsing error response
            {
                _logger.LogError(jsonEx, "JSON exception while canceling super order leg {OrderId}/{OrderLeg} or processing response.", orderId, orderLeg);
                return new SuperOrderResponseDto { OrderId = orderId, CustomStatus = "JsonProcessingFailed", CustomMessage = jsonEx.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while canceling super order leg {OrderId}/{OrderLeg}.", orderId, orderLeg);
                return new SuperOrderResponseDto { OrderId = orderId, CustomStatus = "UnexpectedError", CustomMessage = ex.Message };
            }
        }

        private async Task<TResponse?> PostMarketDataRequestAsync<TRequest, TResponse>(string endpoint, TRequest requestDto) where TResponse : class
        {
            _logger.LogInformation("Sending POST request to {Endpoint} with Body: {@RequestDto}", endpoint, requestDto);
            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
                requestMessage.Content = JsonContent.Create(requestDto, options: _jsonSerializerOptions);

                // Market Feed APIs (ltp, ohlc, quote) require client-id header as per PDF (e.g., page 48)
                if (!string.IsNullOrWhiteSpace(_apiSettings.ClientId) && _apiSettings.ClientId != "YOUR_DHAN_CLIENT_ID_HERE")
                {
                    requestMessage.Headers.TryAddWithoutValidation("client-id", _apiSettings.ClientId);
                }
                else
                {
                    _logger.LogWarning("ClientId header not added for {Endpoint} as it's not configured or is a placeholder. This might be required.", endpoint);
                }

                var httpResponse = await _httpClient.SendAsync(requestMessage);
                var responseContentString = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseData = JsonSerializer.Deserialize<TResponse>(responseContentString, _jsonSerializerOptions);
                    if (responseData != null)
                    {
                        _logger.LogInformation("Successfully received data from {Endpoint}.", endpoint);
                        return responseData;
                    }
                    _logger.LogWarning("API call to {Endpoint} returned success status, but response body was null or invalid. Body: {ResponseBody}", endpoint, responseContentString);
                    return null; // Or a TResponse indicating error
                }
                else
                {
                    _logger.LogError("HTTP error {StatusCode} from {Endpoint}. Response: {ErrorContent}", httpResponse.StatusCode, endpoint, responseContentString);
                    return null; // Or a TResponse indicating error, possibly parsing errorContent
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request exception for {Endpoint}.", endpoint);
                return null;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON exception for {Endpoint}.", endpoint);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred for {Endpoint}.", endpoint);
                return null;
            }
        }

        // POST /v2/marketfeed/ltp (dmerged.pdf page 48)
        public async Task<MarketQuoteLtpResponseDto?> GetLtpDataAsync(MarketDataRequestDto request)
        {
            return await PostMarketDataRequestAsync<MarketDataRequestDto, MarketQuoteLtpResponseDto>("/v2/marketfeed/ltp", request);
        }

        // POST /v2/marketfeed/ohlc (dmerged.pdf page 49)
        public async Task<MarketQuoteOhlcResponseDto?> GetOhlcDataAsync(MarketDataRequestDto request)
        {
            return await PostMarketDataRequestAsync<MarketDataRequestDto, MarketQuoteOhlcResponseDto>("/v2/marketfeed/ohlc", request);
        }

        // POST /v2/marketfeed/quote (dmerged.pdf page 50-51)
        public async Task<MarketFullQuoteResponseDto?> GetFullMarketQuoteAsync(MarketDataRequestDto request)
        {
            // The response structure for "quote" is very nested.
            // The DTO MarketFullQuoteResponseDto needs to match it carefully.
            return await PostMarketDataRequestAsync<MarketDataRequestDto, MarketFullQuoteResponseDto>("/v2/marketfeed/quote", request);
        }

        public async Task<IEnumerable<DhanInstrumentDto>> GetInstrumentsBySegmentAsync(string exchangeSegment)
        {
            if (string.IsNullOrWhiteSpace(exchangeSegment))
            {
                _logger.LogWarning("ExchangeSegment cannot be empty for GetInstrumentsBySegmentAsync.");
                return Enumerable.Empty<DhanInstrumentDto>();
            }

            // URL Encode the segment in case it contains special characters, though less likely for segment codes.
            string encodedSegment = WebUtility.UrlEncode(exchangeSegment.ToUpper());
            var requestUri = $"/instruments/v2/{encodedSegment}";
            // Alternative path from PDF page 26: var requestUri = $"/v2/instrument/{encodedSegment}";
            // The /instruments/v2/ path seems more consistent with /instruments/v2/expiries and /instruments/v2/options.
            // This needs to be tested.

            _logger.LogInformation("Fetching instruments for segment {ExchangeSegment} from URI: {RequestUri}", exchangeSegment, requestUri);

            try
            {
                // This GET request likely only needs the access-token (already in default headers).
                // The response is expected to be a list of instrument objects.
                var instruments = await _httpClient.GetFromJsonAsync<List<DhanInstrumentDto>>(requestUri, _jsonSerializerOptions);

                if (instruments != null)
                {
                    _logger.LogInformation("Successfully fetched {Count} instruments for segment {ExchangeSegment}.", instruments.Count, exchangeSegment);
                    return instruments;
                }

                _logger.LogWarning("Received null or empty list for instruments in segment {ExchangeSegment} from {RequestUri}", exchangeSegment, requestUri);
                return Enumerable.Empty<DhanInstrumentDto>();
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request error (Status: {StatusCode}) while fetching instruments for segment {ExchangeSegment} from {RequestUri}.",
                                 httpEx.StatusCode, exchangeSegment, requestUri);
                return Enumerable.Empty<DhanInstrumentDto>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization error while fetching instruments for segment {ExchangeSegment} from {RequestUri}.",
                                 exchangeSegment, requestUri);
                return Enumerable.Empty<DhanInstrumentDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching instruments for segment {ExchangeSegment} from {RequestUri}.",
                                 exchangeSegment, requestUri);
                return Enumerable.Empty<DhanInstrumentDto>();
            }
        }

        public async Task<IEnumerable<OrderResponseDto>?> PlaceSliceOrderAsync(OrderRequestDto sliceOrderRequest)
        {
            if (sliceOrderRequest == null)
            {
                _logger.LogWarning("SliceOrderRequest (OrderRequestDto) cannot be null for PlaceSliceOrderAsync.");
                // Consider returning a specific error DTO or throwing an argument exception
                return null;
            }

            // Ensure dhanClientId is set from settings for the request body
            if (string.IsNullOrWhiteSpace(_apiSettings.ClientId) || _apiSettings.ClientId == "YOUR_DHAN_CLIENT_ID_HERE")
            {
                _logger.LogError("Dhan ClientId is not configured. Cannot place slice order.");
                // Return a list containing an error response DTO
                return new List<OrderResponseDto> { new OrderResponseDto { CustomStatus = "ConfigurationError", CustomMessage = "Dhan ClientId not configured." } };
            }
            sliceOrderRequest.DhanClientId = _apiSettings.ClientId;

            var requestUri = "/v2/orders/slicing"; // Endpoint from PDF page 68
            _logger.LogInformation("Placing Slice Order via POST to URI: {RequestUri} with Request: {@SliceOrderRequest}",
                                   requestUri, sliceOrderRequest);

            try
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, requestUri);
                requestMessage.Content = JsonContent.Create(sliceOrderRequest, options: _jsonSerializerOptions);

                // The sample cURL for POST /orders/slicing (page 68) shows 'access-token' (default)
                // and 'Content-Type' (default). It does NOT show a separate 'client-id' header,
                // as 'dhanClientId' is part of the request body.

                var httpResponse = await _httpClient.SendAsync(requestMessage);
                var responseContentString = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    // The PDF (page 69) shows the response as an array of objects, each with "orderId" and "orderStatus".
                    // We can deserialize this into a List<OrderResponseDto>.
                    var sliceOrderResponses = JsonSerializer.Deserialize<List<OrderResponseDto>>(responseContentString, _jsonSerializerOptions);

                    if (sliceOrderResponses != null)
                    {
                        _logger.LogInformation("Slice Order placement API call processed. Received {Count} responses.", sliceOrderResponses.Count);
                        foreach (var resp in sliceOrderResponses) { resp.CustomStatus = "ApiSuccess"; }
                        return sliceOrderResponses;
                    }
                    _logger.LogWarning("Slice Order placement API call returned success status, but response body was null or invalid. Body: {ResponseBody}", responseContentString);
                    return new List<OrderResponseDto> { new OrderResponseDto { CustomStatus = "ApiSuccessWithInvalidResponse", CustomMessage = "API success, but no valid response body." } };
                }
                else
                {
                    _logger.LogError("HTTP error {StatusCode} while placing slice order. Response: {ErrorContent}", httpResponse.StatusCode, responseContentString);
                    // Attempt to parse a generic error structure if possible, or just return the raw message in a single error DTO
                    OrderResponseDto? errorDetails = null;
                    try { errorDetails = JsonSerializer.Deserialize<OrderResponseDto>(responseContentString, _jsonSerializerOptions); } catch { }

                    return new List<OrderResponseDto> { new OrderResponseDto {
                        CustomStatus = "ApiError",
                        CustomMessage = $"HTTP Error: {httpResponse.StatusCode}. Details: {responseContentString}",
                        ApiOrderStatus = errorDetails?.ApiOrderStatus ?? httpResponse.StatusCode.ToString(),
                        DhanErrorCode = errorDetails?.ErrorCode,
                        DhanMessage = errorDetails?.DhanMessage
                    }};
                }
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP request exception while placing slice order.");
                return new List<OrderResponseDto> { new OrderResponseDto { CustomStatus = "HttpRequestFailed", CustomMessage = httpEx.Message } };
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON exception while placing slice order or processing response.");
                return new List<OrderResponseDto> { new OrderResponseDto { CustomStatus = "JsonProcessingFailed", CustomMessage = jsonEx.Message } };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while placing slice order.");
                return new List<OrderResponseDto> { new OrderResponseDto { CustomStatus = "UnexpectedError", CustomMessage = ex.Message } };
            }
        }

        // ------- Additional API methods -------

        public async Task<IEnumerable<DhanHoldingDto>> GetHoldingsAsync()
        {
            var requestUri = "/v2/holdings";
            _logger.LogInformation("Fetching holdings from URI: {RequestUri}", requestUri);
            try
            {
                var holdings = await _httpClient.GetFromJsonAsync<List<DhanHoldingDto>>(requestUri, _jsonSerializerOptions);
                return holdings ?? Enumerable.Empty<DhanHoldingDto>();
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                _logger.LogError(ex, "Error fetching holdings from {RequestUri}", requestUri);
                return Enumerable.Empty<DhanHoldingDto>();
            }
        }

        public async Task<bool> CancelOrderAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                _logger.LogWarning("CancelOrderAsync called with empty orderId");
                return false;
            }

            var requestUri = $"/v2/orders/{orderId}";
            _logger.LogInformation("Cancelling order via DELETE to URI: {RequestUri}", requestUri);
            try
            {
                var response = await _httpClient.DeleteAsync(requestUri);
                return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Accepted;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while cancelling order {OrderId}", orderId);
                return false;
            }
        }

        public async Task<OrderResponseDto?> ModifyOrderAsync(string orderId, ModifyOrderRequestDto modifyRequest)
        {
            if (string.IsNullOrWhiteSpace(orderId) || modifyRequest == null)
            {
                _logger.LogWarning("ModifyOrderAsync invalid parameters");
                return null;
            }

            if (string.IsNullOrWhiteSpace(_apiSettings.ClientId) || _apiSettings.ClientId == "YOUR_DHAN_CLIENT_ID_HERE")
            {
                _logger.LogError("Dhan ClientId not configured. Cannot modify order");
                return null;
            }

            modifyRequest.DhanClientId = _apiSettings.ClientId;
            modifyRequest.OrderId = orderId;

            var requestUri = $"/v2/orders/{orderId}";
            _logger.LogInformation("Modifying order via PUT to URI: {RequestUri}", requestUri);
            try
            {
                var httpResponse = await _httpClient.PutAsJsonAsync(requestUri, modifyRequest, _jsonSerializerOptions);
                var content = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.IsSuccessStatusCode)
                {
                    var resp = JsonSerializer.Deserialize<OrderResponseDto>(content, _jsonSerializerOptions);
                    if (resp != null) resp.CustomStatus = "ApiSuccess";
                    return resp;
                }

                _logger.LogError("HTTP error {StatusCode} while modifying order. Response: {Content}", httpResponse.StatusCode, content);
                return null;
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                _logger.LogError(ex, "Error modifying order {OrderId}", orderId);
                return null;
            }
        }

        public async Task<OrderDataDto?> GetOrderByCorrelationIdAsync(string correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                _logger.LogWarning("GetOrderByCorrelationIdAsync called with empty id");
                return null;
            }

            var requestUri = $"/v2/orders/external/{WebUtility.UrlEncode(correlationId)}";
            _logger.LogInformation("Fetching order by correlation id from URI: {RequestUri}", requestUri);
            try
            {
                return await _httpClient.GetFromJsonAsync<OrderDataDto>(requestUri, _jsonSerializerOptions);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                _logger.LogError(ex, "Error fetching order by correlation id {CorrelationId}", correlationId);
                return null;
            }
        }

        public async Task<bool> ConvertPositionAsync(ConvertPositionRequestDto convertRequest)
        {
            if (convertRequest == null)
            {
                _logger.LogWarning("ConvertPositionAsync called with null request");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_apiSettings.ClientId) || _apiSettings.ClientId == "YOUR_DHAN_CLIENT_ID_HERE")
            {
                _logger.LogError("Dhan ClientId not configured. Cannot convert position");
                return false;
            }
            convertRequest.DhanClientId = _apiSettings.ClientId;

            var requestUri = "/v2/positions/convert";
            _logger.LogInformation("Converting position via POST to URI: {RequestUri}", requestUri);
            try
            {
                var response = await _httpClient.PostAsJsonAsync(requestUri, convertRequest, _jsonSerializerOptions);
                return response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Accepted;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error during ConvertPositionAsync");
                return false;
            }
        }

        // ------------------- Newly Added APIs -------------------
        public async Task<IEnumerable<ForeverOrderDto>> GetForeverOrdersAsync()
        {
            var requestUri = "/v2/forever/all";
            try
            {
                var orders = await _httpClient.GetFromJsonAsync<List<ForeverOrderDto>>(requestUri, _jsonSerializerOptions);
                return orders ?? new List<ForeverOrderDto>();
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                _logger.LogError(ex, "Error fetching forever orders");
                return Enumerable.Empty<ForeverOrderDto>();
            }
        }

        public async Task<OrderResponseDto?> PlaceForeverOrderAsync(ForeverOrderRequestDto request)
        {
            if (request == null)
            {
                _logger.LogWarning("PlaceForeverOrderAsync called with null request");
                return null;
            }
            var requestUri = "/v2/forever";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(requestUri, request, _jsonSerializerOptions);
                return await response.Content.ReadFromJsonAsync<OrderResponseDto>(_jsonSerializerOptions);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                _logger.LogError(ex, "Error placing forever order");
                return null;
            }
        }

        public async Task<bool> CancelForeverOrderAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId)) return false;
            var requestUri = $"/v2/forever/{WebUtility.UrlEncode(orderId)}";
            try
            {
                var resp = await _httpClient.DeleteAsync(requestUri);
                return resp.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error cancelling forever order {OrderId}", orderId);
                return false;
            }
        }

        public async Task<EdisTpinResponseDto?> GenerateEdisTpinAsync()
        {
            var requestUri = "/v2/edis/tpin";
            try
            {
                return await _httpClient.GetFromJsonAsync<EdisTpinResponseDto>(requestUri, _jsonSerializerOptions);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                _logger.LogError(ex, "Error generating EDIS TPIN");
                return null;
            }
        }

        public async Task<bool> SubmitEdisFormAsync(EdisFormRequestDto form)
        {
            if (form == null) return false;
            var requestUri = "/v2/edis/form";
            try
            {
                var resp = await _httpClient.PostAsJsonAsync(requestUri, form, _jsonSerializerOptions);
                return resp.IsSuccessStatusCode || resp.StatusCode == HttpStatusCode.Accepted;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error submitting EDIS form");
                return false;
            }
        }

        public async Task<EdisInquireResponseDto?> InquireEdisAsync(string isin)
        {
            if (string.IsNullOrWhiteSpace(isin)) return null;
            var requestUri = $"/v2/edis/inquire/{WebUtility.UrlEncode(isin)}";
            try
            {
                return await _httpClient.GetFromJsonAsync<EdisInquireResponseDto>(requestUri, _jsonSerializerOptions);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                _logger.LogError(ex, "Error inquiring EDIS for {Isin}", isin);
                return null;
            }
        }

        public async Task<KillSwitchResponseDto?> SetKillSwitchAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return null;
            var requestUri = $"/v2/killswitch?killSwitchStatus={WebUtility.UrlEncode(status)}";
            try
            {
                var resp = await _httpClient.PostAsync(requestUri, null);
                return await resp.Content.ReadFromJsonAsync<KillSwitchResponseDto>(_jsonSerializerOptions);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                _logger.LogError(ex, "Error setting kill switch status {Status}", status);
                return null;
            }
        }

        public async Task<IEnumerable<LedgerEntryDto>> GetLedgerAsync()
        {
            var requestUri = "/v2/ledger";
            try
            {
                var ledger = await _httpClient.GetFromJsonAsync<List<LedgerEntryDto>>(requestUri, _jsonSerializerOptions);
                return ledger ?? new List<LedgerEntryDto>();
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                _logger.LogError(ex, "Error fetching ledger");
                return Enumerable.Empty<LedgerEntryDto>();
            }
        }

        public async Task<IEnumerable<HistoricalTradeDto>> GetHistoricalTradesAsync(string fromDate, string toDate, int page)
        {
            var requestUri = $"/v2/trades/{fromDate}/{toDate}/{page}";
            try
            {
                var trades = await _httpClient.GetFromJsonAsync<List<HistoricalTradeDto>>(requestUri, _jsonSerializerOptions);
                return trades ?? new List<HistoricalTradeDto>();
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                _logger.LogError(ex, "Error fetching historical trades");
                return Enumerable.Empty<HistoricalTradeDto>();
            }
        }

        public async Task<HistoricalChartResponseDto?> GetHistoricalChartAsync(HistoricalChartRequestDto request)
        {
            if (request == null) return null;
            var requestUri = "/v2/charts/historical";
            try
            {
                var resp = await _httpClient.PostAsJsonAsync(requestUri, request, _jsonSerializerOptions);
                return await resp.Content.ReadFromJsonAsync<HistoricalChartResponseDto>(_jsonSerializerOptions);
            }
            catch (Exception ex) when (ex is HttpRequestException || ex is JsonException)
            {
                _logger.LogError(ex, "Error fetching historical chart");
                return null;
            }
        }

    }

}
