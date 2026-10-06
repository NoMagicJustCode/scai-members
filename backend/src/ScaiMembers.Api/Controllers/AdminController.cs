using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ScaiMembers.Api.Contracts;
using ScaiMembers.Api.Models;
using ScaiMembers.Api.Services;

namespace ScaiMembers.Api.Controllers;

/// <summary>Board area: application review (§7), member list, fee settings (§12(f)).</summary>
[ApiController]
[Route("api/admin")]
[Authorize(Policy = "RequireAdmin")]
public class AdminController(
    MongoDbContext db,
    BoardService board,
    ConfigService config,
    AuditService audit) : ControllerBase
{
    [HttpGet("applications")]
    public async Task<List<AdminApplicationDto>> Applications([FromQuery] bool pending = true, CancellationToken ct = default)
    {
        var apps = await board.ListApplicationsAsync(pending, ct);
        var deciderIds = apps.Select(a => a.DecidedByMemberId).OfType<string>().Distinct().ToList();
        var names = await db.Members.Find(m => deciderIds.Contains(m.Id))
            .Project(m => new { m.Id, m.Name })
            .ToListAsync(ct);
        var nameOf = names.ToDictionary(n => n.Id, n => n.Name);

        return apps.Select(a => new AdminApplicationDto(
            a.Id, a.MemberType, a.Name, a.Representative, a.DateOfBirth, a.Email,
            a.PostalAddress, a.Country, a.RequestedClass, a.Motivation, a.SubmittedAt,
            a.State, a.DecidedAt,
            a.DecidedByMemberId is { } d ? nameOf.GetValueOrDefault(d) : null)).ToList();
    }

    [HttpPost("applications/{id}/approve")]
    public Task<IActionResult> Approve(string id, CancellationToken ct) =>
        Decide(id, () => board.ApproveAsync(id, User, ct), "Admitted.");

    /// <summary>§7(2): the board may refuse without stating grounds — no reason field by design.</summary>
    [HttpPost("applications/{id}/reject")]
    public Task<IActionResult> Reject(string id, CancellationToken ct) =>
        Decide(id, () => board.RejectAsync(id, User, ct), "Declined.");

    private async Task<IActionResult> Decide(string id, Func<Task<DecisionResult>> decide, string doneMessage)
    {
        if (!ObjectId.TryParse(id, out _)) return NotFound();

        var result = await decide();
        return result.Outcome switch
        {
            DecisionOutcome.NotFound => NotFound(),
            DecisionOutcome.AlreadyDecided => Problem(statusCode: StatusCodes.Status409Conflict,
                title: "This application has already been decided."),
            DecisionOutcome.MemberEmailExists => Problem(statusCode: StatusCodes.Status409Conflict,
                title: "A member record with this email address already exists."),
            _ => Ok(new DecisionResponse(
                result.EmailSent ? doneMessage : $"{doneMessage} The notification email could not be sent — please inform the applicant directly.",
                result.EmailSent))
        };
    }

    [HttpGet("members")]
    public async Task<AdminMemberListResponse> Members(CancellationToken ct)
    {
        var members = await db.Members.Find(FilterDefinition<Member>.Empty)
            .SortBy(m => m.Name)
            .ToListAsync(ct);
        var active = members.Count(m => m.Status == MemberStatus.Active);

        return new AdminMemberListResponse(
            members.Select(m => new AdminMemberDto(
                m.Id, m.MemberType, m.Name, m.Representative, m.Email,
                m.MembershipClass, m.Status, m.JoinedAt, m.IsAdmin)).ToList(),
            active,
            (active + 9) / 10);
    }

    [HttpGet("settings")]
    public async Task<PlatformSettingsDto> GetSettings(CancellationToken ct)
    {
        var c = await config.GetAsync(ct);
        return new PlatformSettingsDto
        {
            JoiningFee = c.JoiningFee,
            AnnualFeeOrdinary = c.AnnualFeeOrdinary,
            AnnualFeeSupporting = c.AnnualFeeSupporting,
            Currency = c.Currency,
            StatutesVersion = c.StatutesVersion,
            PrivacyPolicyVersion = c.PrivacyPolicyVersion,
            UpdatedAt = c.UpdatedAt
        };
    }

    [HttpPut("settings")]
    public async Task<PlatformSettingsDto> UpdateSettings(PlatformSettingsDto dto, CancellationToken ct)
    {
        dto.StatutesVersion = dto.StatutesVersion.Trim();
        dto.PrivacyPolicyVersion = dto.PrivacyPolicyVersion.Trim();
        dto.UpdatedAt = DateTime.UtcNow;

        await config.UpdateAsync(dto, ct);
        await audit.LogAsync(User, "settings.update", "config", "config",
            JsonSerializer.Serialize(new
            {
                dto.JoiningFee, dto.AnnualFeeOrdinary, dto.AnnualFeeSupporting, dto.Currency,
                dto.StatutesVersion, dto.PrivacyPolicyVersion
            }), ct);
        return dto;
    }
}
