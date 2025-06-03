using System;
using System.Globalization;
using DhanAlgoTrading.Models.Configuration;
using DhanAlgoTrading.Models.DhanApi;
using DhanAlgoTrading.Models.TradingView;
using DhanAlgoTrading.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DhanAlgoTrading.Controllers
{
    [ApiController]
    [Route("api/webhook")] // Define a base route for webhooks
    public class TradingViewWebhookController : ControllerBase
    {
        private readonly ILogger<TradingViewWebhookController> _logger;
        private readonly IDhanService _dhanService;
        private readonly DhanApiSettings _apiSettings; // For webhook passphrase

        public TradingViewWebhookController(
            ILogger<TradingViewWebhookController> logger,
            IDhanService dhanService,
            IOptions<DhanApiSettings> apiSettingsOptions)
        {
            _logger = logger;
            _dhanService = dhanService;
            _apiSettings = apiSettingsOptions.Value;
        }

        [HttpPost("tradingview")] // Full path: /api/webhook/tradingview
        public async Task<IActionResult> ReceiveTradingViewAlert([FromBody] TradingViewAlertDto alert)
        {
            _logger.LogInformation("Received TradingView Alert: {@Alert}", alert);

            // 1. Security Check: Validate Passphrase
            if (string.IsNullOrWhiteSpace(_apiSettings.TradingViewWebhookPassphrase) ||
                alert.Passphrase != _apiSettings.TradingViewWebhookPassphrase)
            {
                _logger.LogWarning("Invalid or missing passphrase in TradingView alert. Passphrase received: {ReceivedPassphrase}", alert.Passphrase);
                return Unauthorized("Invalid passphrase.");
            }

            // 2. Basic Alert Validation
            if (alert == null || string.IsNullOrWhiteSpace(alert.Action) || alert.UnderlyingScrip <= 0 || string.IsNullOrWhiteSpace(alert.ExchangeSegment))
            {
                _logger.LogWarning("Invalid alert payload: Missing critical fields.");
                return BadRequest("Invalid alert payload: Missing action, underlying_scrip, or exchange_segment.");
            }

            try
            {
                // --- Example: Basic processing for "SELL_CALL_ATM" ---
                if ("SELL_CALL_ATM".Equals(alert.Action, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Processing SELL_CALL_ATM action for strategy: {StrategyName}", alert.StrategyName);

                    // a. Get Expiry Dates
                    var expiryRequest = new ExpiryListRequestDto { UnderlyingScrip = alert.UnderlyingScrip, UnderlyingSeg = alert.UnderlyingSegment };
                    var expiryDates = (await _dhanService.GetExpiryDatesAsync(expiryRequest))?.ToList();

                    if (expiryDates == null || !expiryDates.Any())
                    {
                        _logger.LogError("No expiry dates found for UnderlyingScrip: {Scrip}, Segment: {Segment}", alert.UnderlyingScrip, alert.UnderlyingSegment);
                        return BadRequest($"No expiry dates found for underlying {alert.UnderlyingScrip}.");
                    }

                    // b. Select target expiry based on ExpiryType and offsets
                    string targetExpiry;
                    try
                    {
                        var orderedDates = expiryDates
                            .Select(d => DateTime.Parse(d, CultureInfo.InvariantCulture))
                            .OrderBy(d => d)
                            .ToList();

                        if (string.Equals(alert.ExpiryType, "WEEKLY", StringComparison.OrdinalIgnoreCase))
                        {
                            var idx = Math.Clamp(alert.ExpiryOffsetWeeks, 0, orderedDates.Count - 1);
                            targetExpiry = orderedDates[idx].ToString("yyyy-MM-dd");
                        }
                        else if (string.Equals(alert.ExpiryType, "MONTHLY", StringComparison.OrdinalIgnoreCase))
                        {
                            var monthlyGroups = orderedDates
                                .GroupBy(d => new { d.Year, d.Month })
                                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                                .ToList();
                            var idx = Math.Clamp(alert.ExpiryOffsetMonths, 0, monthlyGroups.Count - 1);
                            targetExpiry = monthlyGroups[idx].First().ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            targetExpiry = orderedDates.First().ToString("yyyy-MM-dd");
                        }
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogError(ex, "Failed to parse expiry dates returned from API: {Dates}", string.Join(',', expiryDates));
                        return StatusCode(StatusCodes.Status500InternalServerError, "Invalid expiry date format from API.");
                    }
                    _logger.LogInformation("Selected target expiry: {TargetExpiry}", targetExpiry);


                    // c. Get Option Chain
                    var optionChainRequest = new OptionChainRequestDto
                    {
                        UnderlyingScrip = alert.UnderlyingScrip,
                        UnderlyingSeg = alert.UnderlyingSegment,
                        ExpiryDate = targetExpiry
                    };
                    var optionChain = (await _dhanService.GetOptionChainAsync(optionChainRequest))?.ToList();

                    if (optionChain == null || !optionChain.Any())
                    {
                        _logger.LogError("Option chain not found for UnderlyingScrip: {Scrip}, Expiry: {Expiry}", alert.UnderlyingScrip, targetExpiry);
                        return BadRequest($"Option chain not found for {alert.UnderlyingScrip} on {targetExpiry}.");
                    }

                    // d. Select ATM Call Option (Simplified: needs underlying LTP)
                    var ltpRequest = new MarketDataRequestDto { { alert.UnderlyingSegment ?? "IDX_I", new List<string> { alert.UnderlyingScrip.ToString() } } };
                    var ltpResponse = await _dhanService.GetLtpDataAsync(ltpRequest);
                    decimal underlyingLtp = 0;

                    if (ltpResponse?.Data != null &&
                        ltpResponse.Data.TryGetValue(alert.UnderlyingSegment ?? "IDX_I", out var segmentData) &&
                        segmentData.TryGetValue(alert.UnderlyingScrip.ToString(), out var ltpInfo))
                    {
                        underlyingLtp = ltpInfo.LastPrice;
                    }
                    else
                    {
                        _logger.LogWarning("Could not fetch LTP for underlying {UnderlyingScrip}. Cannot determine ATM accurately. Proceeding with closest available.", alert.UnderlyingScrip);
                    }
                    _logger.LogInformation("Underlying LTP for {Scrip} is {LTP}", alert.UnderlyingScrip, underlyingLtp);


                    OptionInstrument? atmCall = null;
                    if (underlyingLtp > 0)
                    {
                        atmCall = optionChain
                           .Where(o => "CALL".Equals(o.OptionType, StringComparison.OrdinalIgnoreCase))
                           .OrderBy(o => Math.Abs(o.StrikePrice - underlyingLtp))
                           .ThenBy(o => o.StrikePrice)
                           .FirstOrDefault();
                    }
                    else
                    {
                        atmCall = optionChain.FirstOrDefault(o => "CALL".Equals(o.OptionType, StringComparison.OrdinalIgnoreCase));
                    }


                    if (atmCall == null || atmCall.SecurityId == null || atmCall.LotSize == 0)
                    {
                        _logger.LogError("Could not find a suitable ATM Call option or missing SecurityId/LotSize. UnderlyingLTP: {LTP}", underlyingLtp);
                        return BadRequest("Could not determine ATM Call option.");
                    }
                    _logger.LogInformation("Selected ATM Call: {TradingSymbol}, SecurityId: {SecurityId}, Strike: {Strike}, LotSize: {LotSize}",
                                          atmCall.TradingSymbol, atmCall.SecurityId, atmCall.StrikePrice, atmCall.LotSize);

                    // e. Prepare OrderRequestDto
                    var orderRequest = new OrderRequestDto
                    {
                        TransactionType = "SELL",
                        ExchangeSegment = alert.ExchangeSegment,
                        ProductType = alert.ProductType,
                        OrderType = alert.OrderType,
                        SecurityId = atmCall.SecurityId,
                        Quantity = alert.QuantityLots * atmCall.LotSize,
                        Validity = "DAY",
                        CorrelationId = $"TV_{alert.StrategyName}_{Guid.NewGuid().ToString().Substring(0, 8)}"
                    };

                    if ("LIMIT".Equals(alert.OrderType, StringComparison.OrdinalIgnoreCase))
                    {
                        if (alert.LimitPriceAbsolute.HasValue)
                        {
                            orderRequest.Price = alert.LimitPriceAbsolute.Value;
                        }
                        // Corrected logic: Fetch option LTP and then use it.
                        else if (alert.LimitPriceBufferPercent.HasValue)
                        {
                            // Fetch LTP for the selected option contract
                            var optionLtpRequest = new MarketDataRequestDto { { atmCall.ExchangeSegment ?? "NSE_FNO", new List<string> { atmCall.SecurityId } } };
                            var optionLtpResponse = await _dhanService.GetLtpDataAsync(optionLtpRequest);
                            decimal optionLtp = 0;

                            if (optionLtpResponse?.Data != null &&
                                optionLtpResponse.Data.TryGetValue(atmCall.ExchangeSegment ?? "NSE_FNO", out var optSegmentData) &&
                                optSegmentData.TryGetValue(atmCall.SecurityId, out var optLtpInfo))
                            {
                                optionLtp = optLtpInfo.LastPrice;
                            }

                            if (optionLtp > 0)
                            { // Check if we successfully fetched option LTP
                                orderRequest.Price = Math.Round(optionLtp * (1 - (alert.LimitPriceBufferPercent.Value / 100)), 2); // Buffer for selling
                                _logger.LogInformation("Calculated LIMIT price for selling: {LimitPrice} based on Option LTP: {OptionLTP} and Buffer: {Buffer}%",
                                                       orderRequest.Price, optionLtp, alert.LimitPriceBufferPercent.Value);
                            }
                            else
                            {
                                _logger.LogError("Cannot calculate LIMIT price for option {SecurityId}: LTP not available for the option itself.", atmCall.SecurityId);
                                return BadRequest("Cannot determine LIMIT price due to missing option LTP.");
                            }
                        }
                        else
                        {
                            _logger.LogError("LIMIT order specified but no absolute price or buffer percent provided for calculation.");
                            return BadRequest("LIMIT order requires price details (absolute or buffer).");
                        }
                    }
                    else if ("MARKET".Equals(alert.OrderType, StringComparison.OrdinalIgnoreCase))
                    {
                        orderRequest.Price = 0;
                    }


                    // f. Pre-trade Fund/Margin Check
                    var fundLimits = await _dhanService.GetFundLimitsAsync();
                    if (fundLimits == null)
                    {
                        _logger.LogWarning("Unable to fetch fund limits for margin check.");
                    }

                    var marginRequestDto = new MarginCalculatorRequestDto
                    {
                        ExchangeSegment = orderRequest.ExchangeSegment,
                        TransactionType = orderRequest.TransactionType,
                        Quantity = orderRequest.Quantity,
                        ProductType = orderRequest.ProductType,
                        SecurityId = orderRequest.SecurityId,
                        Price = orderRequest.Price,
                        TriggerPrice = orderRequest.TriggerPrice
                    };

                    var marginResponse = await _dhanService.GetOrderMarginsAsync(marginRequestDto);
                    if (marginResponse != null && fundLimits != null && marginResponse.TotalMargin > fundLimits.AvailabelBalance)
                    {
                        _logger.LogWarning("Insufficient margin. Required: {Required}, Available: {Available}", marginResponse.TotalMargin, fundLimits.AvailabelBalance);
                        return BadRequest("Insufficient margin for order.");
                    }

                    // g. Place Order
                    _logger.LogInformation("Placing order: {@OrderRequest}", orderRequest);
                    var orderResponse = await _dhanService.PlaceOrderAsync(orderRequest);

                    if (orderResponse != null && !string.IsNullOrEmpty(orderResponse.OrderId) && orderResponse.ApiOrderStatus?.ToUpper() != "REJECTED")
                    {
                        _logger.LogInformation("Order placed successfully/pending via webhook. Order ID: {OrderId}, Status: {Status}",
                                               orderResponse.OrderId, orderResponse.ApiOrderStatus);
                        return Ok(new { message = "Alert processed, order placed/pending.", orderDetails = orderResponse });
                    }
                    else
                    {
                        _logger.LogError("Failed to place order via webhook. Response: {@OrderResponse}", orderResponse);
                        return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Failed to place order.", errorDetails = orderResponse });
                    }
                }
                else
                {
                    _logger.LogInformation("Action '{Action}' not yet implemented in webhook.", alert.Action);
                    return Ok(new { message = $"Alert received for action: {alert.Action}. Processing not yet implemented." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing TradingView alert.");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the alert.");
            }
        }
    }
}
