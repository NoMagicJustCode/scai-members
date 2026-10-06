using System.ComponentModel.DataAnnotations;
using ScaiMembers.Api.Models;

namespace ScaiMembers.Api.Contracts;

public record AdminApplicationDto(
    string Id,
    MemberType MemberType,
    string Name,
    string? Representative,
    DateOnly? DateOfBirth,
    string Email,
    string? PostalAddress,
    string? Country,
    MembershipClass RequestedClass,
    string? Motivation,
    DateTime SubmittedAt,
    ApplicationState State,
    DateTime? DecidedAt,
    string? DecidedBy);

public record DecisionResponse(string Message, bool EmailSent);

public record AdminMemberDto(
    string Id,
    MemberType MemberType,
    string Name,
    string? Representative,
    string Email,
    MembershipClass MembershipClass,
    MemberStatus Status,
    DateTime? JoinedAt,
    bool IsAdmin);

/// <param name="ActiveCount">Basis for the one-tenth thresholds of §9(3) and §11(2).</param>
/// <param name="OneTenth">Members needed to reach one tenth (rounded up).</param>
public record AdminMemberListResponse(List<AdminMemberDto> Members, int ActiveCount, int OneTenth);

/// <summary>Fees are decided by the General Assembly (§12(f)); the board records them here.</summary>
public class PlatformSettingsDto
{
    [Range(0, 100000)]
    public decimal JoiningFee { get; set; }

    [Range(0, 100000)]
    public decimal AnnualFeeOrdinary { get; set; }

    [Range(0, 100000)]
    public decimal AnnualFeeSupporting { get; set; }

    [Required, RegularExpression("^[A-Z]{3}$")]
    public string Currency { get; set; } = "EUR";

    /// <summary>Changing a version makes applicants consent to the new document.</summary>
    [Required, MaxLength(50)]
    public string StatutesVersion { get; set; } = null!;

    [Required, MaxLength(50)]
    public string PrivacyPolicyVersion { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }
}
