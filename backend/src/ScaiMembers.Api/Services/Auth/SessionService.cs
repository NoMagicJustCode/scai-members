using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ScaiMembers.Api.Configuration;
using ScaiMembers.Api.Models;

namespace ScaiMembers.Api.Services.Auth;

/// <summary>
/// Sessions are a signed JWT in an httpOnly cookie — never readable by page
/// scripts. Admin rights are re-checked against the database on every admin
/// request (AdminRequirement), so revoking them takes effect immediately.
/// </summary>
public class SessionService(IOptions<JwtSettings> jwtOptions, IHostEnvironment env)
{
    public const string CookieName = "scai_session";

    /// <summary>
    /// Mutating requests made with the session cookie must carry this header.
    /// Browsers only send custom headers cross-origin after a CORS preflight,
    /// which our CORS policy refuses for foreign origins — this blocks CSRF even
    /// from same-site pages such as uploaded HTML files (Phase 5).
    /// </summary>
    public const string CsrfHeader = "X-SCAI-Request";

    private readonly JwtSettings _jwt = jwtOptions.Value;

    public void SignIn(HttpResponse response, Member member)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwt.ExpirationMinutes);
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims:
            [
                new Claim(JwtRegisteredClaimNames.Sub, member.Id),
                new Claim("name", member.Name),
                new Claim("is_admin", member.IsAdmin ? "true" : "false"),
                new Claim("member_status", member.Status.ToString())
            ],
            expires: expires,
            signingCredentials: new SigningCredentials(SigningKey(_jwt), SecurityAlgorithms.HmacSha256));

        response.Cookies.Append(CookieName, new JwtSecurityTokenHandler().WriteToken(token), CookieOptions(expires));
    }

    public void SignOut(HttpResponse response) =>
        response.Cookies.Delete(CookieName, CookieOptions(null));

    private CookieOptions CookieOptions(DateTime? expires) => new()
    {
        HttpOnly = true,
        Secure = !env.IsDevelopment(),
        SameSite = SameSiteMode.Lax,
        Path = "/api",
        Expires = expires
    };

    public static SymmetricSecurityKey SigningKey(JwtSettings jwt) =>
        new(Encoding.UTF8.GetBytes(jwt.Secret));
}
