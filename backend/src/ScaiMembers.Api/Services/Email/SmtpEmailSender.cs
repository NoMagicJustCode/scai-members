using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ScaiMembers.Api.Configuration;

namespace ScaiMembers.Api.Services.Email;

public class SmtpEmailSender(IOptions<SmtpSettings> options) : IEmailSender
{
    private readonly SmtpSettings _smtp = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_smtp.FromName, _smtp.FromAddress));
        mime.To.Add(new MailboxAddress(message.ToName, message.ToAddress));
        mime.Subject = message.Subject;
        mime.Body = new TextPart("plain") { Text = message.Body };

        using var client = new SmtpClient();
        // Auto: STARTTLS on 587, implicit TLS on 465.
        await client.ConnectAsync(_smtp.Host, _smtp.Port, SecureSocketOptions.Auto, ct);
        if (!string.IsNullOrEmpty(_smtp.Username))
            await client.AuthenticateAsync(_smtp.Username, _smtp.Password, ct);
        await client.SendAsync(mime, ct);
        await client.DisconnectAsync(true, ct);
    }
}
