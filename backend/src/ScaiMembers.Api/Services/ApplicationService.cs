using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ScaiMembers.Api.Configuration;
using ScaiMembers.Api.Contracts;
using ScaiMembers.Api.Models;
using ScaiMembers.Api.Services.Email;

namespace ScaiMembers.Api.Services;

public enum SubmitOutcome { Accepted, Invalid, StaleDocuments }

public record SubmitResult(SubmitOutcome Outcome, Dictionary<string, string[]>? Errors = null)
{
    public static SubmitResult Accepted { get; } = new(SubmitOutcome.Accepted);
}

/// <summary>
/// Membership applications (Â§7) up to the point of board review: submission,
/// double opt-in, and the duplicate handling around it. Board decisions are Phase 3.
/// </summary>
public class ApplicationService(
    MongoDbContext db,
    ConfigService config,
    IEmailSender email,
    IOptions<AppSettings> appOptions,
    ILogger<ApplicationService> logger)
{
    private const string Signature =
        "Second Circuit â€“ Verein fÃ¼r digitale Gedankenfreiheit\nVienna Â· ZVR 1684464197";

    private readonly AppSettings _app = appOptions.Value;

    /// <summary>
    /// The response is identical whether or not the address is already known,
    /// so the endpoint cannot be used to probe who is a member. The owner of the
    /// address learns the real situation by email instead.
    /// </summary>
    public async Task<SubmitResult> SubmitAsync(SubmitApplicationRequest req, CancellationToken ct)
    {
        var errors = Validate(req);
        if (errors.Count > 0)
            return new SubmitResult(SubmitOutcome.Invalid, errors);

        var cfg = await config.GetAsync(ct);
        if (req.StatutesVersion != cfg.StatutesVersion || req.PrivacyPolicyVersion != cfg.PrivacyPolicyVersion)
            return new SubmitResult(SubmitOutcome.StaleDocuments);

        var address = NormalizeEmail(req.Email);
        var name = req.Name.Trim();

        var existingMember = await db.Members
            .Find(m => m.Email == address
                && (m.Status == MemberStatus.Active || m.Status == MemberStatus.Applied))
            .AnyAsync(ct);
        if (existingMember)
        {
            await SendAsync(new EmailMessage(address, name,
                "Your membership application â€” Second Circuit",
                $"Hello {name},\n\n" +
                "we received a membership application for this email address, but it already " +
                "belongs to a member of the association, so no new application was created.\n\n" +
                "If you did not submit this, you can ignore this email.\n\n" + Signature), ct);
            return SubmitResult.Accepted;
        }

        var pending = await db.Applications
            .Find(a => a.Email == address && a.State == ApplicationState.Pending)
            .FirstOrDefaultAsync(ct);
        if (pending is { EmailVerified: true })
        {
            await SendAsync(new EmailMessage(address, name,
                "Your membership application â€” Second Circuit",
                $"Hello {name},\n\n" +
                "we received another membership application for this email address. An earlier " +
                "application is already with the board, so the new one was not recorded. " +
                "You will hear from us once the board has decided.\n\n" + Signature), ct);
            return SubmitResult.Accepted;
        }
        if (pending is not null)
            // Unconfirmed earlier attempt: the newest submission replaces it.
            await db.Applications.DeleteOneAsync(a => a.Id == pending.Id, ct);

        var token = GenerateToken();
        var now = DateTime.UtcNow;
        var application = new MembershipApplication
        {
            MemberType = req.MemberType,
            Name = name,
            Representative = req.MemberType == MemberType.Organisation ? req.Representative?.Trim() : null,
            DateOfBirth = req.MemberType == MemberType.Person ? req.DateOfBirth : null,
            Email = address,
            EmailVerificationTokenHash = HashToken(token),
            EmailVerificationExpiresAt = now.AddHours(_app.EmailVerificationHours),
            PostalAddress = NullIfBlank(req.PostalAddress),
            Country = NullIfBlank(req.Country),
            RequestedClass = req.RequestedClass,
            Motivation = NullIfBlank(req.Motivation),
            Consents =
            [
                new ConsentRecord { Type = "values", DocumentVersion = cfg.StatutesVersion, GivenAt = now },
                new ConsentRecord { Type = "statutes-read", DocumentVersion = cfg.StatutesVersion, GivenAt = now },
                new ConsentRecord { Type = "privacy-policy", DocumentVersion = cfg.PrivacyPolicyVersion, GivenAt = now }
            ],
            SubmittedAt = now
        };
        await db.Applications.InsertOneAsync(application, cancellationToken: ct);

        var link = $"{_app.PublicBaseUrl.TrimEnd('/')}/apply/confirm?token={Uri.EscapeDataString(token)}";
        try
        {
            await SendAsync(new EmailMessage(address, name,
                "Confirm your membership application â€” Second Circuit",
                $"Hello {name},\n\n" +
                "thank you for applying for membership of Second Circuit â€“ Verein fÃ¼r digitale " +
                "Gedankenfreiheit.\n\n" +
                $"Please confirm your email address by opening this link within {_app.EmailVerificationHours} hours:\n" +
                $"{link}\n\n" +
                "After confirmation, the board will review your application (Â§7 of the statutes).\n\n" +
                "If you did not apply, ignore this email. Unconfirmed applications are deleted automatically.\n\n" +
                Signature), ct);
        }
        catch
        {
            // Without the email the applicant cannot confirm; don't keep data they can't act on.
            await db.Applications.DeleteOneAsync(a => a.Id == application.Id, CancellationToken.None);
            throw;
        }

        logger.LogInformation("Application {Id} submitted, awaiting email confirmation", application.Id);
        return SubmitResult.Accepted;
    }

    /// <summary>Double opt-in. Returns false for unknown, used or expired tokens.</summary>
    public async Task<bool> VerifyEmailAsync(string token, CancellationToken ct)
    {
        var hash = HashToken(token);
        var result = await db.Applications.UpdateOneAsync(
            a => a.EmailVerificationTokenHash == hash
                && a.State == ApplicationState.Pending
                && a.EmailVerificationExpiresAt > DateTime.UtcNow,
            Builders<MembershipApplication>.Update
                .Set(a => a.EmailVerified, true)
                .Unset(a => a.EmailVerificationTokenHash)
                .Unset(a => a.EmailVerificationExpiresAt),
            cancellationToken: ct);
        return result.ModifiedCount == 1;
    }

    private async Task SendAsync(EmailMessage message, CancellationToken ct)
    {
        try
        {
            await email.SendAsync(message, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new EmailDeliveryException(ex);
        }
    }

    private static Dictionary<string, string[]> Validate(SubmitApplicationRequest req)
    {
        var errors = new Dictionary<string, string[]>();
        void Add(string field, string message) => errors[field] = [message];

        if (string.IsNullOrWhiteSpace(req.Name))
            Add(nameof(req.Name), "Name is required.");

        if (req.MemberType == MemberType.Person)
        {
            if (req.DateOfBirth is not { } dob)
                Add(nameof(req.DateOfBirth), "Date of birth is required.");
            else if (AgeOn(dob, DateOnly.FromDateTime(DateTime.UtcNow)) < 18)
                Add(nameof(req.DateOfBirth), "Members must be at least 18 years old (Â§7(1) of the statutes).");
        }
        else if (string.IsNullOrWhiteSpace(req.Representative))
        {
            Add(nameof(req.Representative), "Organisations must name a representative.");
        }

        if (req.RequestedClass == MembershipClass.Honorary)
            Add(nameof(req.RequestedClass), "Honorary membership is conferred by the General Assembly and cannot be applied for.");

        if (!req.SharesValues)
            Add(nameof(req.SharesValues), "Required (Â§7(1) of the statutes).");
        if (!req.HasReadStatutes)
            Add(nameof(req.HasReadStatutes), "Required.");
        if (!req.AcceptsPrivacyPolicy)
            Add(nameof(req.AcceptsPrivacyPolicy), "Required.");

        return errors;
    }

    private static int AgeOn(DateOnly dob, DateOnly today)
    {
        var age = today.Year - dob.Year;
        if (dob > today.AddYears(-age)) age--;
        return age;
    }

    private static string NormalizeEmail(string address) => address.Trim().ToLowerInvariant();

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GenerateToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string HashToken(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
