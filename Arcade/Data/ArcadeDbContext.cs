using Arcade.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arcade.Data;

public class ArcadeDbContext : DbContext
{
    public ArcadeDbContext(DbContextOptions<ArcadeDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Username).HasMaxLength(50).IsRequired();
            e.Property(x => x.Email).HasMaxLength(100);
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
        });
    }
}
