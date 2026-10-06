namespace ScaiMembers.Api.Configuration;

public class AppSettings
{
    public const string SectionName = "App";

    /// <summary>Public frontend URL; used to build links in emails.</summary>
    public string PublicBaseUrl { get; set; } = "http://localhost:5173";

    /// <summary>How long a double-opt-in link stays valid before the application is discarded.</summary>
    public int EmailVerificationHours { get; set; } = 48;
}

public class MongoDbSettings
{
    public const string SectionName = "MongoDB";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "scai_members";
}

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "ScaiMembers";
    public string Audience { get; set; } = "ScaiMembers";

    /// <summary>
    /// Session length (one working session). Admin rights are re-checked in the
    /// database on every admin request, so revocation does not wait for expiry.
    /// </summary>
    public int ExpirationMinutes { get; set; } = 480;
}

public class CorsSettings
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = ["http://localhost:5173"];
}

/// <summary>File-sharing module settings (Phase 5 — Chris's feature).</summary>
public class UploadSettings
{
    public const string SectionName = "Upload";

    public string Path { get; set; } = "./uploads";
    public long MaxFileSizeBytes { get; set; } = 25 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = [".pdf", ".html"];
}

/// <summary>
/// Transactional email (verification, application results, GV invitations).
/// With no Host set, mail is written to the log instead of sent (local development).
/// </summary>
public class SmtpSettings
{
    public const string SectionName = "Smtp";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host);

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "members@scai.world";
    public string FromName { get; set; } = "Second Circuit";
}
