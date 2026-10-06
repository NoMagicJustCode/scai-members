using Microsoft.AspNetCore.Mvc;
using ScaiMembers.Api.Contracts;
using ScaiMembers.Api.Services;

namespace ScaiMembers.Api.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController(ConfigService config) : ControllerBase
{
    /// <summary>Fees (§12(f)) and current document versions, for the public pages.</summary>
    [HttpGet("public")]
    public async Task<PublicConfigResponse> GetPublic(CancellationToken ct)
    {
        var cfg = await config.GetAsync(ct);
        return new PublicConfigResponse(
            cfg.JoiningFee,
            cfg.AnnualFeeOrdinary,
            cfg.AnnualFeeSupporting,
            cfg.Currency,
            cfg.StatutesVersion,
            cfg.PrivacyPolicyVersion);
    }
}
