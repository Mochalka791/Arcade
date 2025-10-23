using System.ComponentModel.DataAnnotations;

namespace Arcade.Data.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(40)]
    public required string UserName { get; set; }

    [Required]
    public required byte[] PasswordHash { get; set; }

    [Required]
    public required byte[] PasswordSalt { get; set; }

    public DateTime CreatedUtc { get; set; }
}
