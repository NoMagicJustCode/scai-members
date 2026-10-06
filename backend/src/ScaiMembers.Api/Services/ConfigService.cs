using MongoDB.Driver;
using ScaiMembers.Api.Contracts;
using ScaiMembers.Api.Models;

namespace ScaiMembers.Api.Services;

/// <summary>
/// Reads the single PlatformConfig document. Fees are set by the General
/// Assembly (§12(f)) and edited by the board in /admin/settings (Phase 3);
/// until then the document is created with zero fees as a visible placeholder.
/// </summary>
public class ConfigService(MongoDbContext db)
{
    public async Task<PlatformConfig> GetAsync(CancellationToken ct = default)
    {
        var defaults = new PlatformConfig();
        return await db.Config.FindOneAndUpdateAsync(
            Builders<PlatformConfig>.Filter.Eq(c => c.Id, defaults.Id),
            Builders<PlatformConfig>.Update
                .SetOnInsert(c => c.JoiningFee, defaults.JoiningFee)
                .SetOnInsert(c => c.AnnualFeeOrdinary, defaults.AnnualFeeOrdinary)
                .SetOnInsert(c => c.AnnualFeeSupporting, defaults.AnnualFeeSupporting)
                .SetOnInsert(c => c.Currency, defaults.Currency)
                .SetOnInsert(c => c.StatutesVersion, defaults.StatutesVersion)
                .SetOnInsert(c => c.PrivacyPolicyVersion, defaults.PrivacyPolicyVersion)
                .SetOnInsert(c => c.UpdatedAt, defaults.UpdatedAt),
            new FindOneAndUpdateOptions<PlatformConfig>
            {
                IsUpsert = true,
                ReturnDocument = ReturnDocument.After
            },
            ct);
    }

    public async Task UpdateAsync(PlatformSettingsDto s, CancellationToken ct = default)
    {
        await GetAsync(ct); // ensure the document exists
        await db.Config.UpdateOneAsync(
            c => c.Id == "config",
            Builders<PlatformConfig>.Update
                .Set(c => c.JoiningFee, s.JoiningFee)
                .Set(c => c.AnnualFeeOrdinary, s.AnnualFeeOrdinary)
                .Set(c => c.AnnualFeeSupporting, s.AnnualFeeSupporting)
                .Set(c => c.Currency, s.Currency)
                .Set(c => c.StatutesVersion, s.StatutesVersion)
                .Set(c => c.PrivacyPolicyVersion, s.PrivacyPolicyVersion)
                .Set(c => c.UpdatedAt, s.UpdatedAt ?? DateTime.UtcNow),
            cancellationToken: ct);
    }
}
