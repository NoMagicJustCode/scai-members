namespace ScaiMembers.Api.Services.Email;

/// <summary>Plain-text transactional email. No tracking pixels, no HTML templates.</summary>
public record EmailMessage(string ToAddress, string ToName, string Subject, string Body);

/// <summary>
/// Mail to a member's address is legally significant (§11(3) — GV invitations).
/// Every sender implementation must either deliver or throw; never fail silently.
/// </summary>
public class EmailDeliveryException(Exception inner)
    : Exception("Email could not be delivered.", inner);

public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
}
