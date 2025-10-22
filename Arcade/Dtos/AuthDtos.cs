using System.ComponentModel.DataAnnotations;

namespace Arcade.Dtos;

public record RegisterDto
{
    [Required]
    [StringLength(40, MinimumLength = 3)]
    [RegularExpression("^[a-zA-Z0-9_.-]+$")]
    public required string UserName { get; init; }

    [Required]
    [MinLength(6)]
    public required string Password { get; init; }

    [Required]
    [Compare(nameof(Password))]
    public required string ConfirmPassword { get; init; }
}

public record LoginDto
{
    [Required]
    public required string UserName { get; init; }

    [Required]
    public required string Password { get; init; }
}
