using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using ScaiMembers.Api.Models;

namespace ScaiMembers.Api.Services.Auth;

/// <summary>The caller is an active member who is currently flagged as admin in the database.</summary>
public class AdminRequirement : IAuthorizationRequirement;

public class AdminRequirementHandler(MongoDbContext db) : AuthorizationHandler<AdminRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
    {
        var id = context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (id is null) return;

        var isAdmin = await db.Members
            .Find(m => m.Id == id && m.IsAdmin && m.Status == MemberStatus.Active)
            .AnyAsync();
        if (isAdmin) context.Succeed(requirement);
    }
}
