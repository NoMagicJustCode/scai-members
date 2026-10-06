using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace ScaiMembers.Api.Services.Auth;

/// <summary>
/// Argon2id for every password (ARCHITECTURE.md §5). Hashes are stored in the
/// standard PHC string format, so parameters can be raised later without
/// invalidating existing hashes.
/// </summary>
public static class PasswordHasher
{
    public const int MinLength = 12;

    private const int MemoryKiB = 65536; // 64 MiB
    private const int Iterations = 3;
    private const int Parallelism = 1;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    // Verified against when the account does not exist, so response time does not reveal it.
    private static readonly Lazy<string> DummyHash = new(() => Hash(Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))));

    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Compute(password, salt, MemoryKiB, Iterations, Parallelism, HashBytes);
        return $"$argon2id$v=19$m={MemoryKiB},t={Iterations},p={Parallelism}${B64(salt)}${B64(hash)}";
    }

    public static bool Verify(string password, string? encoded)
    {
        if (encoded is null)
        {
            Verify(password, DummyHash.Value);
            return false;
        }

        var parts = encoded.Split('$');
        // ["", "argon2id", "v=19", "m=..,t=..,p=..", salt, hash]
        if (parts.Length != 6 || parts[1] != "argon2id" || parts[2] != "v=19")
            return false;

        try
        {
            var p = parts[3].Split(',').Select(kv => kv.Split('=')).ToDictionary(kv => kv[0], kv => int.Parse(kv[1]));
            var salt = FromB64(parts[4]);
            var expected = FromB64(parts[5]);
            var actual = Compute(password, salt, p["m"], p["t"], p["p"], expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (Exception ex) when (ex is FormatException or KeyNotFoundException or IndexOutOfRangeException)
        {
            return false;
        }
    }

    private static byte[] Compute(string password, byte[] salt, int memoryKiB, int iterations, int parallelism, int length)
    {
        using var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            MemorySize = memoryKiB,
            Iterations = iterations,
            DegreeOfParallelism = parallelism
        };
        return argon.GetBytes(length);
    }

    private static string B64(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=');

    private static byte[] FromB64(string s) => Convert.FromBase64String(s.PadRight(s.Length + (4 - s.Length % 4) % 4, '='));
}
