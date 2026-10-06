namespace ScaiMembers.Api.Configuration;

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

    /// <summary>Access token lifetime. Short by design; refresh comes in Phase 4.</summary>
    public int ExpirationMinutes { get; set; } = 60;
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

/// <summary>Transactional email (Phase 1 — verification, application results, GV invitations).</summary>
public class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "members@scai.world";
    public string FromName { get; set; } = "Second Circuit";
}
