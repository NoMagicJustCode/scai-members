using System.ComponentModel.DataAnnotations;

namespace ScaiMembers.Api.Contracts;

public class LoginRequest
{
    [Required, MaxLength(254)]
    public string Email { get; set; } = null!;

    [Required, MaxLength(1024)]
    public string Password { get; set; } = null!;
}

public record SessionResponse(string Id, string Name, string Email, bool IsAdmin);
