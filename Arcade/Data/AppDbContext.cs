using Arcade.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Arcade.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var user = modelBuilder.Entity<User>();
        user.ToTable("Users");
        user.HasKey(u => u.Id);
        user.Property(u => u.Id).ValueGeneratedOnAdd();
        user.Property(u => u.UserName)
            .IsRequired()
            .HasMaxLength(40);
        user.HasIndex(u => u.UserName)
            .IsUnique();
        user.Property(u => u.PasswordHash)
            .IsRequired();
        user.Property(u => u.PasswordSalt)
            .IsRequired();
        user.Property(u => u.CreatedUtc)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();
    }
}
