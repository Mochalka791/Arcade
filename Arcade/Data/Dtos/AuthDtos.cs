using System.ComponentModel.DataAnnotations;

namespace Arcade.Data.Dtos;

public sealed class RegisterDto
{
    [Required, MinLength(3), MaxLength(40)]
    public string Username { get; set; } = string.Empty;

    [EmailAddress, MaxLength(100)]
    public string? Email { get; set; }

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
