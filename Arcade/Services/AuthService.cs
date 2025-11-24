using Arcade.Data;
using Arcade.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Arcade.Services;

public class AuthService
{
    private readonly ArcadeDbContext _db;

    public AuthService(ArcadeDbContext db)
    {
        _db = db;
    }

    public async Task<(bool ok, string? error)> RegisterAsync(string username, string password, string? email)
    {
        username = username.Trim();

        if (username.Length < 3) return (false, "Username zu kurz.");
        if (password.Length < 6) return (false, "Passwort zu kurz.");

        var exists = await _db.Users.AnyAsync(u => u.Username == username);
        if (exists) return (false, "Username ist schon vergeben.");

        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new User
        {
            Username = username,
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            PasswordHash = hash
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return (true, null);
    }

    public async Task<(bool ok, User? user, string? error)> LoginAsync(string username, string password)
    {
        username = username.Trim();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null) return (false, null, "User nicht gefunden.");

        var ok = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!ok) return (false, null, "Falsches Passwort.");

        return (true, user, null);
    }
}
