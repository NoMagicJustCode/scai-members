using System.ComponentModel.DataAnnotations;
using ScaiMembers.Api.Models;

namespace ScaiMembers.Api.Contracts;

/// <summary>
/// Public application form (§7). Fields are exactly those allowed by §20(2);
/// cross-field rules (18+, organisation representative, consents) are checked
/// in ApplicationService.
/// </summary>
public class SubmitApplicationRequest
{
    public MemberType MemberType { get; set; } = MemberType.Person;

    [Required, MaxLength(200)]
    public string Name { get; set; } = null!;

    /// <summary>Required for organisations.</summary>
    [MaxLength(200)]
    public string? Representative { get; set; }

    /// <summary>Required for persons; must be 18+ (§7(1)).</summary>
    public DateOnly? DateOfBirth { get; set; }

    [Required, EmailAddress, MaxLength(254)]
    public string Email { get; set; } = null!;

    [MaxLength(500)]
    public string? PostalAddress { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    /// <summary>Ordinary or Supporting. Honorary membership is conferred, not applied for (§7(4)).</summary>
    public MembershipClass RequestedClass { get; set; } = MembershipClass.Ordinary;

    [MaxLength(1000)]
    public string? Motivation { get; set; }

    /// <summary>§7(1): "I share the values and goals of the association."</summary>
    public bool SharesValues { get; set; }

    public bool HasReadStatutes { get; set; }

    /// <summary>GDPR Art. 6(1)(b).</summary>
    public bool AcceptsPrivacyPolicy { get; set; }

    /// <summary>
    /// The document versions the applicant was shown. Must match the current
    /// config, so each consent record names the version actually agreed to.
    /// </summary>
    [Required, MaxLength(50)]
    public string StatutesVersion { get; set; } = null!;

    [Required, MaxLength(50)]
    public string PrivacyPolicyVersion { get; set; } = null!;
}

public class VerifyEmailRequest
{
    [Required, MaxLength(200)]
    public string Token { get; set; } = null!;
}

/// <summary>Fees and document versions for the landing page and the application form.</summary>
public record PublicConfigResponse(
    decimal JoiningFee,
    decimal AnnualFeeOrdinary,
    decimal AnnualFeeSupporting,
    string Currency,
    string StatutesVersion,
    string PrivacyPolicyVersion);
