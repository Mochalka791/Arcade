using Arcade.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arcade.Data;

public class ArcadeDbContext : DbContext
{
    public ArcadeDbContext(DbContextOptions<ArcadeDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<SnakeStats> SnakeStats => Set<SnakeStats>();
    public DbSet<TetrisStats> TetrisStats => Set<TetrisStats>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Username).IsUnique();

            e.Property(x => x.Username)
                .HasMaxLength(50)
                .IsRequired();

            e.Property(x => x.Email)
                .HasMaxLength(100);

            e.Property(x => x.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();
        });

        b.Entity<SnakeStats>(e =>
        {
            e.ToTable("SnakeStats");

            e.HasKey(x => x.Id);

            e.Property(x => x.HighScore)
                .IsRequired()
                .HasDefaultValue(0);

            e.Property(x => x.GamesPlayed)
                .IsRequired()
                .HasDefaultValue(0);

            e.Property(x => x.AverageScore)
                .IsRequired()
                .HasDefaultValue(0m);

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<TetrisStats>(e =>
        {
            e.ToTable("TetrisStats");

            e.HasKey(x => x.Id);

            e.Property(x => x.HighScore)
                .IsRequired()
                .HasDefaultValue(0);

            e.Property(x => x.GamesPlayed)
                .IsRequired()
                .HasDefaultValue(0);

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
