using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MongoDB.Driver;
using ScaiMembers.Api.Configuration;
using ScaiMembers.Api.Contracts;
using ScaiMembers.Api.Models;
using ScaiMembers.Api.Services;
using ScaiMembers.Api.Services.Auth;

namespace ScaiMembers.Api.Controllers;

/// <summary>
/// Phase 3: sign-in for board members (admins) only. Phase 4 opens it to all
/// active members and adds password reset.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(MongoDbContext db, SessionService sessions, ILogger<AuthController> logger)
    : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting(RateLimits.Login)]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var member = await db.Members.Find(m => m.Email == email).FirstOrDefaultAsync(ct);

        // Always run the hash, so timing does not reveal whether the address exists.
        var passwordOk = PasswordHasher.Verify(request.Password, member?.PasswordHash);
        if (member is null || !passwordOk || !member.IsAdmin || member.Status != MemberStatus.Active)
        {
            logger.LogWarning("Failed sign-in attempt");
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Email or password is incorrect.");
        }

        sessions.SignIn(Response, member);
        return Ok(ToResponse(member));
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        sessions.SignOut(Response);
        return NoContent();
    }

    /// <summary>The signed-in user, or 401. The frontend calls this on load.</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var id = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var member = id is null ? null : await db.Members.Find(m => m.Id == id).FirstOrDefaultAsync(ct);
        if (member is null || !member.IsAdmin || member.Status != MemberStatus.Active)
        {
            sessions.SignOut(Response);
            return Unauthorized();
        }
        return Ok(ToResponse(member));
    }

    private static SessionResponse ToResponse(Member m) => new(m.Id, m.Name, m.Email, m.IsAdmin);
}
