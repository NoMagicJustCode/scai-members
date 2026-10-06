using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MongoDB.Driver;
using ScaiMembers.Api.Models;
using ScaiMembers.Api.Services.Email;

namespace ScaiMembers.Api.Services;

public enum DecisionOutcome { Done, NotFound, AlreadyDecided, MemberEmailExists }

/// <param name="EmailSent">False if the decision stands but the applicant could not be notified.</param>
public record DecisionResult(DecisionOutcome Outcome, bool EmailSent = false);

/// <summary>
/// Board decisions on applications (§7(2)): admission or refusal, no reason
/// required. Only applications with a confirmed email address reach the board.
/// </summary>
public class BoardService(
    MongoDbContext db,
    AuditService audit,
    IEmailSender email,
    ILogger<BoardService> logger)
{
    private const string Signature =
        "Second Circuit – Verein für digitale Gedankenfreiheit\nVienna · ZVR 1684464197";

    private static readonly TimeSpan RetainAfterDecision = TimeSpan.FromDays(30);

    public async Task<List<MembershipApplication>> ListApplicationsAsync(bool pending, CancellationToken ct)
    {
        var query = db.Applications.Find(a => a.EmailVerified
            && (pending ? a.State == ApplicationState.Pending : a.State != ApplicationState.Pending));
        return pending
            ? await query.SortBy(a => a.SubmittedAt).ToListAsync(ct)
            : await query.SortByDescending(a => a.DecidedAt).Limit(100).ToListAsync(ct);
    }

    public async Task<DecisionResult> ApproveAsync(string id, ClaimsPrincipal actor, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var application = await ClaimAsync(id, ApplicationState.Approved, actor, now, ct);
        if (application is null) return await NotDecidableAsync(id, ct);

        var member = new Member
        {
            MemberType = application.MemberType,
            Name = application.Name,
            Representative = application.Representative,
            DateOfBirth = application.DateOfBirth,
            Email = application.Email,
            EmailVerified = true,
            PostalAddress = application.PostalAddress,
            Country = application.Country,
            MembershipClass = application.RequestedClass,
            Status = MemberStatus.Active,
            JoinedAt = now,
            Consents = application.Consents,
            CreatedAt = now,
            UpdatedAt = now
        };

        try
        {
            await db.Members.InsertOneAsync(member, cancellationToken: ct);
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // A member record with this address exists (e.g. a former member). Undo the claim.
            await db.Applications.UpdateOneAsync(a => a.Id == id,
                Builders<MembershipApplication>.Update
                    .Set(a => a.State, ApplicationState.Pending)
                    .Unset(a => a.DecidedAt)
                    .Unset(a => a.DecidedByMemberId)
                    .Unset(a => a.PurgeAt),
                cancellationToken: CancellationToken.None);
            return new DecisionResult(DecisionOutcome.MemberEmailExists);
        }

        await db.Applications.UpdateOneAsync(a => a.Id == id,
            Builders<MembershipApplication>.Update.Set(a => a.MemberId, member.Id), cancellationToken: ct);
        await audit.LogAsync(actor, "application.approve", "application", id, $"member {member.Id}", ct);

        var sent = await TrySendAsync(new EmailMessage(member.Email, member.Name,
            "Welcome to Second Circuit",
            $"Hello {member.Name},\n\n" +
            $"the board has admitted you as {ClassName(member.MembershipClass)} member of Second Circuit – " +
            $"Verein für digitale Gedankenfreiheit, effective {now:d MMMM yyyy}.\n\n" +
            "Invitations to the General Assembly will be sent to this email address (§11(3) of the " +
            "statutes). Please keep it up to date. Information about membership fees and the " +
            "member area will follow.\n\n" +
            "Welcome aboard.\n\n" + Signature), ct);
        return new DecisionResult(DecisionOutcome.Done, sent);
    }

    public async Task<DecisionResult> RejectAsync(string id, ClaimsPrincipal actor, CancellationToken ct)
    {
        var application = await ClaimAsync(id, ApplicationState.Rejected, actor, DateTime.UtcNow, ct);
        if (application is null) return await NotDecidableAsync(id, ct);

        await audit.LogAsync(actor, "application.reject", "application", id, ct: ct);

        var sent = await TrySendAsync(new EmailMessage(application.Email, application.Name,
            "Your membership application — Second Circuit",
            $"Hello {application.Name},\n\n" +
            "thank you for your interest in Second Circuit. The board has decided not to admit you " +
            "as a member at this time. As provided in §7(2) of the statutes, no reasons are given.\n\n" +
            "The data from your application will be deleted within 30 days.\n\n" + Signature), ct);
        return new DecisionResult(DecisionOutcome.Done, sent);
    }

    /// <summary>Atomically moves a confirmed, pending application to its decided state.</summary>
    private async Task<MembershipApplication?> ClaimAsync(
        string id, ApplicationState decision, ClaimsPrincipal actor, DateTime now, CancellationToken ct) =>
        await db.Applications.FindOneAndUpdateAsync(
            Builders<MembershipApplication>.Filter.Where(
                a => a.Id == id && a.State == ApplicationState.Pending && a.EmailVerified),
            Builders<MembershipApplication>.Update
                .Set(a => a.State, decision)
                .Set(a => a.DecidedAt, now)
                .Set(a => a.DecidedByMemberId, actor.FindFirst(JwtRegisteredClaimNames.Sub)?.Value)
                .Set(a => a.PurgeAt, now + RetainAfterDecision),
            new FindOneAndUpdateOptions<MembershipApplication> { ReturnDocument = ReturnDocument.After },
            ct);

    private async Task<DecisionResult> NotDecidableAsync(string id, CancellationToken ct)
    {
        var exists = await db.Applications.Find(a => a.Id == id && a.EmailVerified).AnyAsync(ct);
        return new DecisionResult(exists ? DecisionOutcome.AlreadyDecided : DecisionOutcome.NotFound);
    }

    private async Task<bool> TrySendAsync(EmailMessage message, CancellationToken ct)
    {
        try
        {
            await email.SendAsync(message, ct);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Decision email could not be sent");
            return false;
        }
    }

    private static string ClassName(MembershipClass c) => c switch
    {
        MembershipClass.Supporting => "an extraordinary",
        MembershipClass.Honorary => "an honorary",
        _ => "an ordinary"
    };
}
