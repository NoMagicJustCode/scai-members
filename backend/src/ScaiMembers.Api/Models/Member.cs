using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ScaiMembers.Api.Models;

/// <summary>
/// A member of the association, per the statutes of
/// "Second Circuit – Verein für digitale Gedankenfreiheit" (ZVR 1684464197).
///
/// Data minimisation (§20(2) of the statutes): name, date of birth, contact
/// data and fee-administration data only. Do not add fields "just in case".
/// </summary>
public class Member
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    /// <summary>§7(1): natural persons (18+) and legal persons may join.</summary>
    [BsonElement("memberType")]
    [BsonRepresentation(BsonType.String)]
    public MemberType MemberType { get; set; } = MemberType.Person;

    /// <summary>Full name of the person, or registered name of the organisation.</summary>
    [BsonElement("name")]
    public string Name { get; set; } = null!;

    /// <summary>For organisations: the natural person representing them.</summary>
    [BsonElement("representative")]
    public string? Representative { get; set; }

    /// <summary>§20(2). Persons only; used to validate the 18+ requirement (§7(1)).</summary>
    [BsonElement("dateOfBirth")]
    public DateOnly? DateOfBirth { get; set; }

    /// <summary>
    /// Legally significant (§11(3)): invitations to the General Assembly are sent
    /// to the address the member has provided. Unique; member-editable in /me.
    /// </summary>
    [BsonElement("email")]
    public string Email { get; set; } = null!;

    [BsonElement("emailVerified")]
    public bool EmailVerified { get; set; }

    /// <summary>§20(2): contact data.</summary>
    [BsonElement("postalAddress")]
    public string? PostalAddress { get; set; }

    [BsonElement("country")]
    public string? Country { get; set; }

    /// <summary>§6: ordinary / supporting ("außerordentlich") / honorary.</summary>
    [BsonElement("membershipClass")]
    [BsonRepresentation(BsonType.String)]
    public MembershipClass MembershipClass { get; set; } = MembershipClass.Ordinary;

    /// <summary>§7–§8 lifecycle.</summary>
    [BsonElement("status")]
    [BsonRepresentation(BsonType.String)]
    public MemberStatus Status { get; set; } = MemberStatus.Applied;

    [BsonElement("joinedAt")]
    public DateTime? JoinedAt { get; set; }

    /// <summary>Set on resignation (§8(2)), exclusion (§8(3)–(4)) or other end.</summary>
    [BsonElement("endedAt")]
    public DateTime? EndedAt { get; set; }

    [BsonElement("endReason")]
    [BsonRepresentation(BsonType.String)]
    public MemberEndReason? EndReason { get; set; }

    /// <summary>Argon2id hash. Null until the member sets a password (Phase 4).</summary>
    [BsonElement("passwordHash")]
    public string? PasswordHash { get; set; }

    [BsonElement("isAdmin")]
    public bool IsAdmin { get; set; }

    /// <summary>GDPR: versioned, timestamped consent records.</summary>
    [BsonElement("consents")]
    public List<ConsentRecord> Consents { get; set; } = [];

    /// <summary>§8(3): fee administration. Entries added manually by the board.</summary>
    [BsonElement("payments")]
    public List<PaymentRecord> Payments { get; set; } = [];

    /// <summary>
    /// §8(3): exclusion for unpaid fees requires two written reminders.
    /// This list is the evidence trail.
    /// </summary>
    [BsonElement("reminders")]
    public List<ReminderRecord> Reminders { get; set; } = [];

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum MemberType { Person, Organisation }

public enum MembershipClass { Ordinary, Supporting, Honorary }

public enum MemberStatus { Applied, Active, Resigned, Excluded, Ended }

public enum MemberEndReason { Resignation, ExclusionUnpaidFees, ExclusionConduct, Death, LossOfLegalPersonality }

public class ConsentRecord
{
    [BsonElement("type")]
    public string Type { get; set; } = null!; // e.g. "values", "statutes-read", "privacy-policy"

    [BsonElement("documentVersion")]
    public string DocumentVersion { get; set; } = null!;

    [BsonElement("givenAt")]
    public DateTime GivenAt { get; set; } = DateTime.UtcNow;
}

public class PaymentRecord
{
    [BsonElement("year")]
    public int Year { get; set; }

    [BsonElement("amount")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Amount { get; set; }

    [BsonElement("currency")]
    public string Currency { get; set; } = "EUR";

    [BsonElement("receivedAt")]
    public DateTime ReceivedAt { get; set; }

    [BsonElement("method")]
    public string Method { get; set; } = "bank-transfer";

    [BsonElement("reference")]
    public string? Reference { get; set; }
}

public class ReminderRecord
{
    [BsonElement("sentAt")]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [BsonElement("channel")]
    public string Channel { get; set; } = "email";

    [BsonElement("regardingYear")]
    public int RegardingYear { get; set; }
}
