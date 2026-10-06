using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ScaiMembers.Api.Models;

/// <summary>
/// File-sharing module (Phase 5): uploaded HTML/PDF documents served
/// under slug links (e.g. scai.world/f/{slug}), public, password-protected,
/// or members-only.
/// </summary>
public class SharedFile
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    /// <summary>URL slug, unique. Short random string or admin-chosen.</summary>
    [BsonElement("slug")]
    public string Slug { get; set; } = null!;

    [BsonElement("originalName")]
    public string OriginalName { get; set; } = null!;

    /// <summary>Display title shown on the access page.</summary>
    [BsonElement("title")]
    public string? Title { get; set; }

    [BsonElement("contentType")]
    public string ContentType { get; set; } = null!; // application/pdf | text/html

    [BsonElement("sizeBytes")]
    public long SizeBytes { get; set; }

    /// <summary>Path on the uploads volume, relative to Upload:Path.</summary>
    [BsonElement("storagePath")]
    public string StoragePath { get; set; } = null!;

    [BsonElement("visibility")]
    [BsonRepresentation(BsonType.String)]
    public FileVisibility Visibility { get; set; } = FileVisibility.Public;

    /// <summary>Argon2id hash; only when Visibility == Password.</summary>
    [BsonElement("passwordHash")]
    public string? PasswordHash { get; set; }

    [BsonElement("uploadedByMemberId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UploadedByMemberId { get; set; } = null!;

    [BsonElement("uploadedAt")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("downloadCount")]
    public long DownloadCount { get; set; }
}

public enum FileVisibility { Public, Password, Members }
