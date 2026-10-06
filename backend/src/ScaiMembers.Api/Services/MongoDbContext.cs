using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ScaiMembers.Api.Configuration;
using ScaiMembers.Api.Models;

namespace ScaiMembers.Api.Services;

public class MongoDbContext
{
    public IMongoDatabase Database { get; }

    public IMongoCollection<Member> Members { get; }
    public IMongoCollection<MembershipApplication> Applications { get; }
    public IMongoCollection<SharedFile> SharedFiles { get; }
    public IMongoCollection<AuditLogEntry> AuditLog { get; }
    public IMongoCollection<PlatformConfig> Config { get; }

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        Database = client.GetDatabase(settings.Value.DatabaseName);

        Members = Database.GetCollection<Member>("members");
        Applications = Database.GetCollection<MembershipApplication>("applications");
        SharedFiles = Database.GetCollection<SharedFile>("shared_files");
        AuditLog = Database.GetCollection<AuditLogEntry>("audit_log");
        Config = Database.GetCollection<PlatformConfig>("config");

        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        Members.Indexes.CreateMany([
            new CreateIndexModel<Member>(
                Builders<Member>.IndexKeys.Ascending(m => m.Email),
                new CreateIndexOptions { Unique = true }),
            new CreateIndexModel<Member>(
                Builders<Member>.IndexKeys.Ascending(m => m.Status))
        ]);

        Applications.Indexes.CreateMany([
            new CreateIndexModel<MembershipApplication>(
                Builders<MembershipApplication>.IndexKeys.Ascending(a => a.State)),
            new CreateIndexModel<MembershipApplication>(
                Builders<MembershipApplication>.IndexKeys.Ascending(a => a.Email)),
            new CreateIndexModel<MembershipApplication>(
                Builders<MembershipApplication>.IndexKeys.Descending(a => a.SubmittedAt)),
            new CreateIndexModel<MembershipApplication>(
                Builders<MembershipApplication>.IndexKeys.Ascending(a => a.EmailVerificationTokenHash),
                new CreateIndexOptions { Sparse = true }),
            // Unverified applications expire (GDPR minimisation); verified ones have no expiry date.
            new CreateIndexModel<MembershipApplication>(
                Builders<MembershipApplication>.IndexKeys.Ascending(a => a.EmailVerificationExpiresAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }),
            // Decided applications are purged 30 days after the decision (privacy policy).
            new CreateIndexModel<MembershipApplication>(
                Builders<MembershipApplication>.IndexKeys.Ascending(a => a.PurgeAt),
                new CreateIndexOptions { ExpireAfter = TimeSpan.Zero })
        ]);

        SharedFiles.Indexes.CreateOne(new CreateIndexModel<SharedFile>(
            Builders<SharedFile>.IndexKeys.Ascending(f => f.Slug),
            new CreateIndexOptions { Unique = true }));

        AuditLog.Indexes.CreateOne(new CreateIndexModel<AuditLogEntry>(
            Builders<AuditLogEntry>.IndexKeys.Descending(a => a.Timestamp)));
    }
}
