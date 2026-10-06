namespace ScaiMembers.Api.Services.Email;

/// <summary>
/// Used when no SMTP host is configured. In Development the full message is
/// logged so links can be followed locally. Outside Development nothing is sent
/// and the call throws, so a missing SMTP setup never looks like a delivered email.
/// </summary>
public class LogEmailSender(ILogger<LogEmailSender> logger, IHostEnvironment env) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        if (!env.IsDevelopment())
            throw new InvalidOperationException(
                "SMTP is not configured (Smtp__Host is empty); email cannot be sent.");

        logger.LogInformation(
            "DEV EMAIL (not sent)\nTo: {Name} <{Address}>\nSubject: {Subject}\n\n{Body}",
            message.ToName, message.ToAddress, message.Subject, message.Body);
        return Task.CompletedTask;
    }
}
