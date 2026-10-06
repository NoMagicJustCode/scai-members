using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ScaiMembers.Api.Models;

namespace ScaiMembers.Api.Services;

/// <summary>
/// Append-only record of board actions. Details hold ids, never personal data,
/// so entries can outlive the records they refer to.
/// </summary>
public class AuditService(MongoDbContext db)
{
    public Task LogAsync(ClaimsPrincipal actor, string action, string targetType, string targetId,
        string? details = null, CancellationToken ct = default) =>
        db.AuditLog.InsertOneAsync(new AuditLogEntry
        {
            ActorMemberId = actor.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "unknown",
            ActorName = actor.FindFirst("name")?.Value ?? "unknown",
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            Details = details,
            Timestamp = DateTime.UtcNow
        }, cancellationToken: ct);
}
