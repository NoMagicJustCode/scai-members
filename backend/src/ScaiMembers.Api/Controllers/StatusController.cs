using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using ScaiMembers.Api.Services;

namespace ScaiMembers.Api.Controllers;

[ApiController]
[Route("api/status")]
public class StatusController(MongoDbContext db) : ControllerBase
{
    /// <summary>Health check: proves the API is up and MongoDB is reachable.</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var mongoOk = false;
        try
        {
            var result = await db.Database.RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1));
            mongoOk = result.Contains("ok") && result["ok"].ToDouble() == 1.0;
        }
        catch
        {
            // mongoOk stays false — reported below, not thrown.
        }

        return Ok(new
        {
            service = "scai-members",
            organisation = "Second Circuit – Verein für digitale Gedankenfreiheit",
            zvr = "1684464197",
            phase = 3,
            mongo = mongoOk ? "ok" : "unreachable",
            timeUtc = DateTime.UtcNow
        });
    }
}
