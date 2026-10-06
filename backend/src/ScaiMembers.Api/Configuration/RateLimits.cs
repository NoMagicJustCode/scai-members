using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace ScaiMembers.Api.Configuration;

/// <summary>Per-IP limits on the public, unauthenticated endpoints (ARCHITECTURE.md §5).</summary>
public static class RateLimits
{
    public const string Apply = "apply";
    public const string Verify = "verify";
    public const string Login = "login";

    public static void Configure(RateLimiterOptions options)
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        AddPerIp(options, Apply, permits: 5, window: TimeSpan.FromMinutes(15));
        AddPerIp(options, Verify, permits: 20, window: TimeSpan.FromMinutes(15));
        AddPerIp(options, Login, permits: 10, window: TimeSpan.FromMinutes(15));
    }

    private static void AddPerIp(RateLimiterOptions options, string name, int permits, TimeSpan window) =>
        options.AddPolicy(name, http => RateLimitPartition.GetFixedWindowLimiter(
            http.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions { PermitLimit = permits, Window = window }));
}
