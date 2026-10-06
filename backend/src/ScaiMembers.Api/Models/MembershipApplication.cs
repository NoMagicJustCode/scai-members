using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ScaiMembers.Api.Models;

/// <summary>
/// A membership application (§7 of the statutes).
/// The board decides on admission and may refuse without stating grounds (§7(2)).
/// </summary>
public class MembershipApplication
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    /// <summary>Snapshot of the applicant's data; copied into a Member on approval.</summary>
    [BsonElement("memberType")]
    [BsonRepresentation(BsonType.String)]
    public MemberType MemberType { get; set; } = MemberType.Person;

    [BsonElement("name")]
    public string Name { get; set; } = null!;

    [BsonElement("representative")]
    public string? Representative { get; set; }

    [BsonElement("dateOfBirth")]
    public DateOnly? DateOfBirth { get; set; }

    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("emailVerified")]
    public bool EmailVerified { get; set; }

    /// <summary>SHA-256 of the double-opt-in token. The plain token exists only in the email.</summary>
    [BsonElement("emailVerificationTokenHash")]
    public string? EmailVerificationTokenHash { get; set; }

    /// <summary>
    /// TTL-indexed: an unverified application is deleted automatically once this
    /// passes (data minimisation). Cleared on verification, which exempts it.
    /// </summary>
    [BsonElement("emailVerificationExpiresAt")]
    public DateTime? EmailVerificationExpiresAt { get; set; }

    [BsonElement("postalAddress")]
    public string? PostalAddress { get; set; }

    [BsonElement("country")]
    public string? Country { get; set; }

    /// <summary>Requested class: ordinary or supporting (§6). Honorary is not applied for (§7(4)).</summary>
    [BsonElement("requestedClass")]
    [BsonRepresentation(BsonType.String)]
    public MembershipClass RequestedClass { get; set; } = MembershipClass.Ordinary;

    /// <summary>Optional, short. Not required by the statutes.</summary>
    [BsonElement("motivation")]
    public string? Motivation { get; set; }

    /// <summary>Required checkboxes, versioned and timestamped (values/statutes/privacy).</summary>
    [BsonElement("consents")]
    public List<ConsentRecord> Consents { get; set; } = [];

    [BsonElement("state")]
    [BsonRepresentation(BsonType.String)]
    public ApplicationState State { get; set; } = ApplicationState.Pending;

    [BsonElement("submittedAt")]
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Board decision trail: who decided and when. No reason required (§7(2)).</summary>
    [BsonElement("decidedAt")]
    public DateTime? DecidedAt { get; set; }

    [BsonElement("decidedByMemberId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? DecidedByMemberId { get; set; }

    /// <summary>Set when an approved application becomes a Member.</summary>
    [BsonElement("memberId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? MemberId { get; set; }

    /// <summary>
    /// TTL-indexed: set to decision + 30 days (privacy policy v1.0). A declined
    /// applicant's data then disappears; an admitted applicant's data lives on
    /// in the Member record. Who decided stays in the audit log.
    /// </summary>
    [BsonElement("purgeAt")]
    public DateTime? PurgeAt { get; set; }
}

public enum ApplicationState { Pending, Approved, Rejected, Withdrawn }
