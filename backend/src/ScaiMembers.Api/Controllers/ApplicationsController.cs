using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ScaiMembers.Api.Contracts;
using ScaiMembers.Api.Configuration;
using ScaiMembers.Api.Services;
using ScaiMembers.Api.Services.Email;

namespace ScaiMembers.Api.Controllers;

[ApiController]
[Route("api/applications")]
public class ApplicationsController(ApplicationService applications, ILogger<ApplicationsController> logger)
    : ControllerBase
{
    /// <summary>
    /// Submit a membership application (§7). Always answers 202 for a valid
    /// form, whether or not the address is already known; see ApplicationService.
    /// </summary>
    [HttpPost]
    [EnableRateLimiting(RateLimits.Apply)]
    public async Task<IActionResult> Submit(SubmitApplicationRequest request, CancellationToken ct)
    {
        SubmitResult result;
        try
        {
            result = await applications.SubmitAsync(request, ct);
        }
        catch (EmailDeliveryException ex)
        {
            logger.LogError(ex.InnerException, "Application email could not be sent");
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "We could not send the confirmation email. Please try again later.");
        }

        return result.Outcome switch
        {
            SubmitOutcome.Invalid => ValidationProblem(new ValidationProblemDetails(result.Errors!)),
            SubmitOutcome.StaleDocuments => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "The statutes or privacy policy were updated. Please reload the form and review them."),
            _ => Accepted(new
            {
                message = "Thank you. Please check your inbox and confirm your email address."
            })
        };
    }

    /// <summary>Double opt-in: confirm the email address from the link in the email.</summary>
    [HttpPost("verify")]
    [EnableRateLimiting(RateLimits.Verify)]
    public async Task<IActionResult> Verify(VerifyEmailRequest request, CancellationToken ct)
    {
        if (await applications.VerifyEmailAsync(request.Token, ct))
            return Ok(new { message = "Your email address is confirmed. The board will review your application." });

        return Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "This confirmation link is invalid, already used, or expired.");
    }
}
