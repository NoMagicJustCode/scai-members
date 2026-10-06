using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ScaiMembers.Api.Models;

/// <summary>Append-only trail of board/admin actions (adapted from profiles-monorepo).</summary>
public class AuditLogEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonElement("actorMemberId")]
    public string ActorMemberId { get; set; } = null!;

    [BsonElement("actorName")]
    public string ActorName { get; set; } = null!;

    [BsonElement("action")]
    public string Action { get; set; } = null!; // e.g. "application.approve", "member.delete"

    [BsonElement("targetType")]
    public string TargetType { get; set; } = null!;

    [BsonElement("targetId")]
    public string TargetId { get; set; } = null!;

    [BsonElement("details")]
    public string? Details { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Admin-editable platform configuration. Fees are set by the General Assembly
/// (§12(f) of the statutes), therefore never hardcoded. Single document.
/// </summary>
public class PlatformConfig
{
    [BsonId]
    public string Id { get; set; } = "config";

    [BsonElement("joiningFee")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal JoiningFee { get; set; }

    [BsonElement("annualFeeOrdinary")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal AnnualFeeOrdinary { get; set; }

    [BsonElement("annualFeeSupporting")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal AnnualFeeSupporting { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "EUR";

    /// <summary>Version strings shown next to the consent checkboxes.</summary>
    [BsonElement("statutesVersion")]
    public string StatutesVersion { get; set; } = "V6";

    [BsonElement("privacyPolicyVersion")]
    public string PrivacyPolicyVersion { get; set; } = "1.0";

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
