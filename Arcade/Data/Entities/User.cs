using System.ComponentModel.DataAnnotations;

namespace Arcade.Data.Entities

{
    public class User
    {

        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Email { get; set; }

        [Required, MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
