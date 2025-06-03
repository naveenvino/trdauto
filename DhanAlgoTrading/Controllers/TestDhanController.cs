using DhanAlgoTrading.Api.Services;
using Microsoft.AspNetCore.Mvc;
using DhanAlgoTrading.Models.DhanApi;
using DhanAlgoTrading.Services;

namespace DhanAlgoTrading.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestDhanController : ControllerBase
    {
        private readonly IDhanService _dhanService;
        private readonly ILogger<TestDhanController> _logger;

        public TestDhanController(IDhanService dhanService, ILogger<TestDhanController> logger)
        {
            // ... (Constructor and other methods from previous parts - unchanged) ...
            _dhanService = dhanService;
            _logger = logger;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetUserProfileAsync()
        {
            _logger.LogInformation("TestDhanController: GetUserProfile endpoint called.");
            var profileData = await _dhanService.GetUserProfileAsync();

            if (profileData == null)
            {
                return StatusCode(500, "Failed to retrieve profile data.");
            }

            return Ok(profileData);
        }

        [HttpPost("expiries")]
        public async Task<ActionResult<IEnumerable<string>>> GetExpiryDates([FromBody] ExpiryListRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }
            _logger.LogInformation("TestDhanController: GetExpiryDates POST called for Scrip: {Scrip}, Segment: {Segment}",
                                   request.UnderlyingScrip, request.UnderlyingSeg);
            var expiryDates = await _dhanService.GetExpiryDatesAsync(request);
            if (expiryDates == null || !expiryDates.Any())
            {
                return NotFound($"No expiry dates found for Scrip: {request.UnderlyingScrip}, Segment: {request.UnderlyingSeg}. Check subscription and parameters.");
            }
            return Ok(expiryDates);
        }

        [HttpPost("optionchain")]
        public async Task<ActionResult<IEnumerable<OptionInstrument>>> GetOptionChain([FromBody] OptionChainRequestDto request)
        {
            if (request == null)
            {
                return BadRequest("Request body cannot be null.");
            }
            _logger.LogInformation("TestDhanController: GetOptionChain POST called for Scrip: {Scrip}, Segment: {Segment}, Expiry: {Expiry}",
                                   request.UnderlyingScrip, request.UnderlyingSeg, request.ExpiryDate);
            var optionChain = await _dhanService.GetOptionChainAsync(request);

            if (optionChain == null || !optionChain.Any())
            {
                return NotFound($"Option chain not found for Scrip: {request.UnderlyingScrip}, Segment: {request.UnderlyingSeg} with expiry {request.ExpiryDate}.");
            }
            return Ok(optionChain);
        }

        // --- Endpoint for Part 3: Placing Orders ---
        [HttpPost("placeorder")]
        public async Task<ActionResult<OrderResponseDto>> PlaceOrder([FromBody] OrderRequestDto orderRequest)
        {
            if (!ModelState.IsValid) // Basic model validation
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("TestDhanController: PlaceOrder POST called with Order: {@OrderRequest}", orderRequest);

            var orderResponse = await _dhanService.PlaceOrderAsync(orderRequest);

            if (orderResponse == null)
            {
                _logger.LogError("Order placement returned a null response from service.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to place order; null response from service.");
            }

            // Check the custom status set by the service, or rely on OrderId/ApiOrderStatus
            if (orderResponse.CustomStatus == "ApiSuccess" || (!string.IsNullOrEmpty(orderResponse.OrderId) && orderResponse.ApiOrderStatus?.ToUpper() != "REJECTED"))
            {
                _logger.LogInformation("Order placement processed by API. Order ID: {OrderId}, API Status: {ApiOrderStatus}", orderResponse.OrderId, orderResponse.ApiOrderStatus);
                return Ok(orderResponse);
            }
            else
            {
                _logger.LogError("Order placement failed or was rejected by API. CustomStatus: {CustomStatus}, CustomMessage: {CustomMessage}, OrderId: {OrderId}, ApiOrderStatus: {ApiOrderStatus}, DhanMessage: {DhanMessage}, ErrorCode: {ErrorCode}",
                                 orderResponse.CustomStatus, orderResponse.CustomMessage, orderResponse.OrderId, orderResponse.ApiOrderStatus, orderResponse.DhanMessage, orderResponse.ErrorCode);

                // Determine appropriate HTTP status code based on the nature of the failure
                if (orderResponse.CustomStatus == "ValidationFailed" || orderResponse.ApiOrderStatus == "400") return BadRequest(orderResponse);
                if (orderResponse.ApiOrderStatus == "401" || orderResponse.ApiOrderStatus == "403") return Unauthorized(orderResponse); // More specific if possible
                return StatusCode(StatusCodes.Status500InternalServerError, orderResponse);
            }
        }
        [HttpGet("orders/{orderId}")]
        public async Task<ActionResult<OrderDataDto>> GetOrderStatus(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return BadRequest("Order ID cannot be empty.");
            }
            _logger.LogInformation("TestDhanController: GetOrderStatus called for Order ID: {OrderId}", orderId);
            var orderStatus = await _dhanService.GetOrderStatusAsync(orderId);
            if (orderStatus == null)
            {
                return NotFound($"Order status not found for Order ID: {orderId}. It might be an invalid ID or an error occurred.");
            }
            return Ok(orderStatus);
        }

        [HttpGet("orderbook")]
        public async Task<ActionResult<IEnumerable<OrderDataDto>>> GetOrderBook()
        {
            _logger.LogInformation("TestDhanController: GetOrderBook called.");
            var orderBook = await _dhanService.GetOrderBookAsync();
            if (orderBook == null) // Service returns empty list on error, but good to check for explicit null too
            {
                return Ok(Enumerable.Empty<OrderDataDto>()); // Return empty list if service indicates error by returning null
            }
            return Ok(orderBook);
        }

        [HttpGet("positions")]
        public async Task<ActionResult<PositionBookDto>> GetPositions()
        {
            _logger.LogInformation("TestDhanController: GetPositions called.");
            var positions = await _dhanService.GetPositionsAsync();
            if (positions == null)
            {
                // This could mean an error, or simply no positions / an empty position book structure was returned
                _logger.LogWarning("GetPositions returned null from service. This might indicate an API error or an empty dataset.");
                return NotFound("Positions not found or an error occurred while fetching.");
            }
            return Ok(positions);
        }
        // --- Endpoint for Part 5A: Fetching Trade Book ---
        [HttpGet("tradebook")]
        public async Task<ActionResult<IEnumerable<TradeDataDto>>> GetTradeBook([FromQuery] string? orderId = null)
        {
            _logger.LogInformation("TestDhanController: GetTradeBook called. OrderId (optional): {OrderId}", orderId);
            var tradeBook = await _dhanService.GetTradeBookAsync(orderId);

            if (tradeBook == null) // Service might return null on significant error, though it aims for empty list
            {
                _logger.LogWarning("GetTradeBook returned null from service for OrderId: {OrderId}", orderId);
                return Ok(Enumerable.Empty<TradeDataDto>()); // Or NotFound based on expected behavior
            }
            // No need to check for !tradeBook.Any() to return NotFound, an empty list is a valid response.
            return Ok(tradeBook);
        }
        [HttpPost("squareoff")]
        public async Task<ActionResult<OrderResponseDto>> SquareOffPosition([FromBody] SquareOffRequestDto squareOffRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("TestDhanController: SquareOffPosition POST called with Request: {@SquareOffRequest}", squareOffRequest);

            var orderResponse = await _dhanService.SquareOffPositionAsync(squareOffRequest);

            if (orderResponse == null)
            {
                _logger.LogError("SquareOffPosition returned a null response from service.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to square off position; null response from service.");
            }

            if (orderResponse.CustomStatus == "ApiSuccess" || (!string.IsNullOrEmpty(orderResponse.OrderId) && orderResponse.ApiOrderStatus?.ToUpper() != "REJECTED"))
            {
                _logger.LogInformation("SquareOff order placement processed by API. Order ID: {OrderId}, API Status: {ApiOrderStatus}", orderResponse.OrderId, orderResponse.ApiOrderStatus);
                return Ok(orderResponse);
            }
            else
            {
                _logger.LogError("SquareOff order placement failed or was rejected by API. CustomStatus: {CustomStatus}, CustomMessage: {CustomMessage}, OrderId: {OrderId}, ApiOrderStatus: {ApiOrderStatus}",
                                 orderResponse.CustomStatus, orderResponse.CustomMessage, orderResponse.OrderId, orderResponse.ApiOrderStatus);
                // You might want to return different HTTP status codes based on the reason for failure
                if (orderResponse.CustomStatus == "NoPositionFound") return NotFound(orderResponse);
                return BadRequest(orderResponse); // General failure for other cases like API errors
            }
        }

        [HttpGet("fundlimit")]
        public async Task<ActionResult<FundLimitResponseDto>> GetFundLimits()
        {
            _logger.LogInformation("TestDhanController: GetFundLimits called.");
            var fundLimits = await _dhanService.GetFundLimitsAsync();
            if (fundLimits == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve fund limits or an error occurred.");
            }
            return Ok(fundLimits);
        }

        [HttpPost("margincalculator")]
        public async Task<ActionResult<MarginCalculatorResponseDto>> GetOrderMargins([FromBody] MarginCalculatorRequestDto marginRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("TestDhanController: GetOrderMargins POST called with Request: {@MarginRequest}", marginRequest);
            var marginResponse = await _dhanService.GetOrderMarginsAsync(marginRequest);

            if (marginResponse == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to calculate margins or an error occurred.");
            }
            if (!string.IsNullOrWhiteSpace(marginResponse.ErrorMessage)) // Check if service layer indicated an error
            {
                return BadRequest(marginResponse); // Or another appropriate status code
            }
            return Ok(marginResponse);
        }

        [HttpPost("placesuperorder")]
        public async Task<ActionResult<SuperOrderResponseDto>> PlaceSuperOrder([FromBody] SuperOrderRequestDto superOrderRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("TestDhanController: PlaceSuperOrder POST called with Request: {@SuperOrderRequest}", superOrderRequest);

            var superOrderResponse = await _dhanService.PlaceSuperOrderAsync(superOrderRequest);

            if (superOrderResponse == null)
            {
                _logger.LogError("Super Order placement returned a null response from service.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to place super order; null response from service.");
            }

            if (superOrderResponse.CustomStatus == "ApiSuccess" || !string.IsNullOrEmpty(superOrderResponse.OrderId))
            {
                _logger.LogInformation("Super Order placement processed by API. Order ID: {OrderId}, API Status: {ApiOrderStatus}",
                                       superOrderResponse.OrderId, superOrderResponse.ApiOrderStatus);
                return Ok(superOrderResponse);
            }
            else
            {
                _logger.LogError("Super Order placement failed. CustomStatus: {CustomStatus}, CustomMessage: {CustomMessage}, OrderId: {OrderId}, ApiOrderStatus: {ApiOrderStatus}",
                                 superOrderResponse.CustomStatus, superOrderResponse.CustomMessage, superOrderResponse.OrderId, superOrderResponse.ApiOrderStatus);
                // Determine appropriate HTTP status code
                if (superOrderResponse.CustomStatus == "ValidationFailed") return BadRequest(superOrderResponse);
                return StatusCode(StatusCodes.Status500InternalServerError, superOrderResponse);
            }
        }
        // --- Endpoint for Part 8B: Retrieving Super Orders ---
        [HttpGet("superorders")]
        public async Task<ActionResult<IEnumerable<SuperOrderDataDto>>> GetSuperOrders()
        {
            _logger.LogInformation("TestDhanController: GetSuperOrders GET called.");
            var superOrders = await _dhanService.GetSuperOrdersAsync();

            if (superOrders == null) // Service might return null on significant error, though it aims for empty list
            {
                _logger.LogWarning("GetSuperOrders returned null from service.");
                return Ok(Enumerable.Empty<SuperOrderDataDto>()); // Or NotFound based on expected behavior
            }
            // An empty list is a valid response if there are no super orders.
            return Ok(superOrders);
        }
        [HttpPut("superorders/{orderId}")]
        public async Task<ActionResult<SuperOrderResponseDto>> ModifySuperOrder(string orderId, [FromBody] ModifySuperOrderRequestDto modifyRequest)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return BadRequest("Order ID cannot be empty.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("TestDhanController: ModifySuperOrder PUT called for Order ID: {OrderId} with Request: {@ModifyRequest}",
                                   orderId, modifyRequest);

            var superOrderResponse = await _dhanService.ModifySuperOrderAsync(orderId, modifyRequest);

            if (superOrderResponse == null)
            {
                _logger.LogError("Super Order modification returned a null response from service for Order ID: {OrderId}.", orderId);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to modify super order {orderId}; null response from service.");
            }

            if (superOrderResponse.CustomStatus == "ApiSuccess" ||
                (superOrderResponse.ApiOrderStatus != null &&
                 !superOrderResponse.ApiOrderStatus.ToUpper().Contains("REJECTED") &&
                 !superOrderResponse.ApiOrderStatus.ToUpper().Contains("CANCELLED"))) // Successful modification might return TRANSIT or PENDING
            {
                _logger.LogInformation("Super Order modification processed by API for Order ID: {OrderId}. API Status: {ApiOrderStatus}",
                                       superOrderResponse.OrderId ?? orderId, superOrderResponse.ApiOrderStatus);
                return Ok(superOrderResponse);
            }
            else
            {
                _logger.LogError("Super Order modification failed for Order ID: {OrderId}. CustomStatus: {CustomStatus}, CustomMessage: {CustomMessage}, ApiOrderStatus: {ApiOrderStatus}, DhanErrorCode: {DhanErrorCode}, DhanErrorMessage: {DhanErrorMessage}",
                                 orderId, superOrderResponse.CustomStatus, superOrderResponse.CustomMessage, superOrderResponse.ApiOrderStatus, superOrderResponse.DhanErrorCode, superOrderResponse.DhanErrorMessage);

                if (superOrderResponse.CustomStatus == "ValidationFailed") return BadRequest(superOrderResponse);
                if (superOrderResponse.ApiOrderStatus?.StartsWith("4") == true || !string.IsNullOrEmpty(superOrderResponse.DhanErrorCode))
                {
                    return BadRequest(superOrderResponse);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, superOrderResponse);
            }
        }

        [HttpDelete("superorders/{orderId}/legs/{orderLeg}")]
        public async Task<ActionResult<SuperOrderResponseDto>> CancelSuperOrderLeg(string orderId, string orderLeg)
        {
            if (string.IsNullOrWhiteSpace(orderId) || string.IsNullOrWhiteSpace(orderLeg))
            {
                return BadRequest("Order ID and Order Leg must be provided.");
            }
            _logger.LogInformation("TestDhanController: CancelSuperOrderLeg DELETE called for Order ID: {OrderId}, Leg: {OrderLeg}",
                                   orderId, orderLeg);

            var cancelResponse = await _dhanService.CancelSuperOrderLegAsync(orderId, orderLeg);

            if (cancelResponse == null)
            {
                _logger.LogError("Super Order Leg cancellation returned a null response from service for Order ID: {OrderId}, Leg: {OrderLeg}.", orderId, orderLeg);
                return StatusCode(StatusCodes.Status500InternalServerError, $"Failed to cancel super order leg {orderLeg} for order {orderId}; null response from service.");
            }

            // Dhan docs suggest 202 Accepted for successful cancellation, and response body might have orderId and status "CANCELLED"
            if (cancelResponse.CustomStatus == "ApiSuccess" ||
                "CANCELLED".Equals(cancelResponse.ApiOrderStatus, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Super Order Leg cancellation processed by API for Order ID: {OrderId}, Leg: {OrderLeg}. API Status: {ApiOrderStatus}",
                                       cancelResponse.OrderId ?? orderId, orderLeg, cancelResponse.ApiOrderStatus);
                return Ok(cancelResponse); // Or return NoContent() if API returns 202 with no meaningful body for success.
            }
            else
            {
                _logger.LogError("Super Order Leg cancellation failed for Order ID: {OrderId}, Leg: {OrderLeg}. CustomStatus: {CustomStatus}, CustomMessage: {CustomMessage}, ApiOrderStatus: {ApiOrderStatus}, DhanErrorCode: {DhanErrorCode}, DhanErrorMessage: {DhanErrorMessage}",
                                 orderId, orderLeg, cancelResponse.CustomStatus, cancelResponse.CustomMessage, cancelResponse.ApiOrderStatus, cancelResponse.DhanErrorCode, cancelResponse.DhanErrorMessage);

                if (cancelResponse.CustomStatus == "ValidationFailed") return BadRequest(cancelResponse);
                if (cancelResponse.ApiOrderStatus?.StartsWith("4") == true || !string.IsNullOrEmpty(cancelResponse.DhanErrorCode))
                {
                    return BadRequest(cancelResponse);
                }
                return StatusCode(StatusCodes.Status500InternalServerError, cancelResponse);
            }
        }

        [HttpPost("market/ltp")]
        public async Task<ActionResult<MarketQuoteLtpResponseDto>> GetLtpData([FromBody] MarketDataRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("TestDhanController: GetLtpData POST called with Request: {@Request}", request);
            var response = await _dhanService.GetLtpDataAsync(request);
            if (response == null) return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching LTP data.");
            if (!"success".Equals(response.Status, StringComparison.OrdinalIgnoreCase)) return BadRequest(response); // Or Ok(response) if you want to show Dhan's error structure
            return Ok(response);
        }

        [HttpPost("market/ohlc")]
        public async Task<ActionResult<MarketQuoteOhlcResponseDto>> GetOhlcData([FromBody] MarketDataRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("TestDhanController: GetOhlcData POST called with Request: {@Request}", request);
            var response = await _dhanService.GetOhlcDataAsync(request);
            if (response == null) return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching OHLC data.");
            if (!"success".Equals(response.Status, StringComparison.OrdinalIgnoreCase)) return BadRequest(response);
            return Ok(response);
        }

        [HttpPost("market/quote")]
        public async Task<ActionResult<MarketFullQuoteResponseDto>> GetFullMarketQuote([FromBody] MarketDataRequestDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _logger.LogInformation("TestDhanController: GetFullMarketQuote POST called with Request: {@Request}", request);
            var response = await _dhanService.GetFullMarketQuoteAsync(request);
            if (response == null) return StatusCode(StatusCodes.Status500InternalServerError, "Error fetching full market quote.");
            if (!"success".Equals(response.Status, StringComparison.OrdinalIgnoreCase)) return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("instruments/{exchangeSegment}")]
        public async Task<ActionResult<IEnumerable<DhanInstrumentDto>>> GetInstrumentsBySegment(string exchangeSegment)
        {
            if (string.IsNullOrWhiteSpace(exchangeSegment))
            {
                return BadRequest("Exchange segment cannot be empty.");
            }
            _logger.LogInformation("TestDhanController: GetInstrumentsBySegment GET called for Segment: {ExchangeSegment}", exchangeSegment);

            var instruments = await _dhanService.GetInstrumentsBySegmentAsync(exchangeSegment);

            if (instruments == null) // Service might return null on significant error
            {
                _logger.LogWarning("GetInstrumentsBySegment returned null from service for Segment: {ExchangeSegment}", exchangeSegment);
                return Ok(Enumerable.Empty<DhanInstrumentDto>()); // Or NotFound/500 based on expected behavior
            }
            // An empty list is a valid response if the segment has no instruments or is invalid.
            return Ok(instruments);
        }

        [HttpPost("orders/slicing")]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> PlaceSliceOrder([FromBody] OrderRequestDto sliceOrderRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _logger.LogInformation("TestDhanController: PlaceSliceOrder POST called with Request: {@SliceOrderRequest}", sliceOrderRequest);

            var sliceOrderResponses = await _dhanService.PlaceSliceOrderAsync(sliceOrderRequest);

            if (sliceOrderResponses == null || !sliceOrderResponses.Any())
            {
                _logger.LogError("Slice Order placement returned a null or empty response from service.");
                // This could be a valid scenario if no slices were needed, or an error.
                // The service method should ideally return a list with an error DTO in case of failure.
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to place slice order or no slices generated; null/empty response from service.");
            }

            // Check if the first response indicates an error (assuming error is returned as a single item in the list by the service)
            var firstResponse = sliceOrderResponses.First();
            if (firstResponse.CustomStatus != "ApiSuccess" && firstResponse.CustomStatus != "ApiSuccessWithInvalidResponse") // Check for our custom error statuses
            {
                _logger.LogError("Slice Order placement failed. First response indicates error: {@FirstResponse}", firstResponse);
                return BadRequest(sliceOrderResponses); // Return the list which might contain error details
            }

            _logger.LogInformation("Slice Order placement processed by API. Received {Count} responses.", sliceOrderResponses.Count());
            return Ok(sliceOrderResponses);
        }
    }
}
