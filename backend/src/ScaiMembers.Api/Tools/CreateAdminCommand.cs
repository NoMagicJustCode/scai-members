using System.Net.Mail;
using System.Text;
using MongoDB.Driver;
using ScaiMembers.Api.Models;
using ScaiMembers.Api.Services;
using ScaiMembers.Api.Services.Auth;

namespace ScaiMembers.Api.Tools;

/// <summary>
/// Bootstraps a board account from the server console, since there is no
/// admin yet to admit one through the UI:
///   dotnet run --project src/ScaiMembers.Api -- create-admin
///   docker compose exec -it backend dotnet ScaiMembers.Api.dll create-admin
/// For an existing member, it grants admin rights and sets a new password.
/// </summary>
public static class CreateAdminCommand
{
    public const string Name = "create-admin";

    public static async Task<int> RunAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<MongoDbContext>();

        Console.WriteLine("Create or promote a board account for members.scai.world\n");

        var email = Prompt("Email: ").Trim().ToLowerInvariant();
        if (!MailAddress.TryCreate(email, out _))
        {
            Console.Error.WriteLine("Not a valid email address.");
            return 1;
        }

        var existing = await db.Members.Find(m => m.Email == email).FirstOrDefaultAsync();
        if (existing is not null)
            Console.WriteLine($"Found member \"{existing.Name}\" ({existing.Status}). They will be made admin.");

        var name = existing?.Name ?? Prompt("Full name: ").Trim();
        if (name.Length == 0)
        {
            Console.Error.WriteLine("Name is required.");
            return 1;
        }

        var password = PromptHidden($"Password (min. {PasswordHasher.MinLength} characters): ");
        if (password.Length < PasswordHasher.MinLength)
        {
            Console.Error.WriteLine("Password is too short.");
            return 1;
        }
        if (PromptHidden("Repeat password: ") != password)
        {
            Console.Error.WriteLine("Passwords do not match.");
            return 1;
        }

        var hash = PasswordHasher.Hash(password);
        var now = DateTime.UtcNow;

        if (existing is not null)
        {
            await db.Members.UpdateOneAsync(m => m.Id == existing.Id,
                Builders<Member>.Update
                    .Set(m => m.IsAdmin, true)
                    .Set(m => m.PasswordHash, hash)
                    .Set(m => m.UpdatedAt, now));
            Console.WriteLine($"\n{existing.Name} is now an admin.");
            if (existing.Status != MemberStatus.Active)
                Console.WriteLine("Note: sign-in requires an active membership; this member is not active.");
        }
        else
        {
            // Board members are members of the association; founders join at the founding assembly.
            var member = new Member
            {
                Name = name,
                Email = email,
                EmailVerified = true,
                MembershipClass = MembershipClass.Ordinary,
                Status = MemberStatus.Active,
                JoinedAt = now,
                IsAdmin = true,
                PasswordHash = hash,
                CreatedAt = now,
                UpdatedAt = now
            };
            await db.Members.InsertOneAsync(member);
            Console.WriteLine($"\nCreated admin {name} <{email}>.");
            Console.WriteLine("Date of birth and address can be completed later (§20(2)).");
        }
        return 0;
    }

    // Piped input on Windows can start with a byte-order mark; it must not end up in an email address.
    private static string ReadLine() => (Console.ReadLine() ?? "").TrimStart('﻿');

    private static string Prompt(string label)
    {
        Console.Write(label);
        return ReadLine();
    }

    private static string PromptHidden(string label)
    {
        Console.Write(label);
        if (Console.IsInputRedirected)
            return ReadLine();

        var sb = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) break;
            if (key.Key == ConsoleKey.Backspace)
            {
                if (sb.Length > 0) sb.Length--;
                continue;
            }
            if (!char.IsControl(key.KeyChar)) sb.Append(key.KeyChar);
        }
        Console.WriteLine();
        return sb.ToString();
    }
}
