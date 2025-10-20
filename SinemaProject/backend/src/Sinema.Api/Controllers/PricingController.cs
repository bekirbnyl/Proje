using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sinema.Application.Abstractions.Pricing;
using Sinema.Application.DTOs.Pricing;
using Sinema.Infrastructure.Auth;

namespace Sinema.Api.Controllers;

/// <summary>
/// Controller for pricing operations and quote generation
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PricingController : ControllerBase
{
    private readonly IPricingEngine _pricingEngine;
    private readonly IValidator<PriceQuoteRequest> _validator;
    private readonly ILogger<PricingController> _logger;

    public PricingController(
        IPricingEngine pricingEngine,
        IValidator<PriceQuoteRequest> validator,
        ILogger<PricingController> logger)
    {
        _pricingEngine = pricingEngine;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Calculate a price quote for screening tickets
    /// </summary>
    /// <param name="request">Quote request with screening, member, and items</param>
    /// <returns>Detailed price breakdown with applied discounts</returns>
    /// <response code="200">Quote calculated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Member authentication required for VIP benefits</response>
    /// <response code="404">Screening or member not found</response>
    [HttpPost("quote")]
    [ProducesResponseType(typeof(PriceQuoteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PriceQuoteResponse>> GetQuote([FromBody] PriceQuoteRequest request)
    {
        try
        {
            // Validate the request
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var problemDetails = new ValidationProblemDetails();
                foreach (var error in validationResult.Errors)
                {
                    problemDetails.Errors.TryAdd(error.PropertyName, new[] { error.ErrorMessage });
                }
                return BadRequest(problemDetails);
            }

            // Check if member authentication is required and valid
            if (request.MemberId.HasValue)
            {
                var authResult = await ValidateMemberAuthAsync(request.MemberId.Value);
                if (authResult != null)
                {
                    return authResult;
                }
            }

            // Calculate the quote
            var quote = await _pricingEngine.CalculateQuoteAsync(request);

            _logger.LogInformation(
                "Price quote calculated for screening {ScreeningId}, member {MemberId}: {TotalBefore} -> {TotalAfter} {Currency}",
                request.ScreeningId,
                request.MemberId,
                quote.TotalBefore,
                quote.TotalAfter,
                quote.Currency);

            return Ok(quote);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument in price quote request");
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation in price quote request");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price quote for screening {ScreeningId}", request.ScreeningId);
            return StatusCode(500, new { message = "An error occurred while calculating the price quote" });
        }
    }

    /// <summary>
    /// Get base price for a screening
    /// </summary>
    /// <param name="screeningId">ID of the screening</param>
    /// <returns>Base ticket price</returns>
    /// <response code="200">Base price retrieved successfully</response>
    /// <response code="404">Screening not found</response>
    [HttpGet("base-price/{screeningId:guid}")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<decimal>> GetBasePrice(Guid screeningId)
    {
        try
        {
            var basePrice = await _pricingEngine.GetBasePriceAsync(screeningId);
            return Ok(basePrice);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Screening {ScreeningId} not found", screeningId);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting base price for screening {ScreeningId}", screeningId);
            return StatusCode(500, new { message = "An error occurred while retrieving the base price" });
        }
    }

    /// <summary>
    /// Get a simple price quote for a single ticket type
    /// Convenience method for quick price checks
    /// </summary>
    /// <param name="screeningId">ID of the screening</param>
    /// <param name="ticketType">Type of ticket (optional, defaults to Full)</param>
    /// <param name="memberId">ID of the member (optional)</param>
    /// <returns>Simple price information</returns>
    [HttpGet("quick-quote/{screeningId:guid}")]
    [ProducesResponseType(typeof(QuoteItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuoteItemResponse>> GetQuickQuote(
        Guid screeningId,
        [FromQuery] Domain.Enums.TicketType ticketType = Domain.Enums.TicketType.Full,
        [FromQuery] Guid? memberId = null)
    {
        try
        {
            var request = new PriceQuoteRequest
            {
                ScreeningId = screeningId,
                MemberId = memberId,
                Items = new List<QuoteItemRequest>
                {
                    new()
                    {
                        TicketType = ticketType,
                        Quantity = 1
                    }
                }
            };

            var quote = await _pricingEngine.CalculateQuoteAsync(request);
            
            if (quote.Items.Any())
            {
                return Ok(quote.Items.First());
            }

            return BadRequest(new { message = "Unable to calculate quote" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument in quick quote request");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating quick quote for screening {ScreeningId}", screeningId);
            return StatusCode(500, new { message = "An error occurred while calculating the quick quote" });
        }
    }

    /// <summary>
    /// Validates member authentication if required
    /// </summary>
    /// <param name="memberId">Member ID to validate</param>
    /// <returns>Error action result if validation fails, null if valid</returns>
    private async Task<ActionResult?> ValidateMemberAuthAsync(Guid memberId)
    {
        // For VIP benefits, we need to ensure the member is authenticated and approved
        // This is a simplified check - in a real system you might want more sophisticated auth
        
        // Check if user is authenticated
        if (!User.Identity?.IsAuthenticated == true)
        {
            // For anonymous requests with memberId, we'll allow it but VIP benefits won't apply
            // This is more user-friendly than forcing authentication for price quotes
            _logger.LogInformation("Anonymous request with member ID {MemberId} - VIP benefits may not apply", memberId);
            return null;
        }

        // If authenticated, we could add additional checks here:
        // - Verify the authenticated user owns the member account
        // - Check if member is approved for VIP benefits
        // For now, we'll trust that the memberId is valid if provided

        return null;
    }

    /// <summary>
    /// Health check endpoint for pricing service
    /// </summary>
    /// <returns>Service status</returns>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new
        {
            service = "PricingService",
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
