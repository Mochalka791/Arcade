using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using Arcade.Data;
using Arcade.Data.Dtos;
using Arcade.Data.Entities; 
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Arcade.Data.Security;

public static class AuthEndpointExtensions
{
    private static readonly TimeSpan SignInDuration = TimeSpan.FromDays(7);

    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", RegisterAsync);
        group.MapPost("/login", LoginAsync);
        group.MapPost("/logout", LogoutAsync);
    }

    private static async Task<IResult> RegisterAsync(
        RegisterDto dto,
        ArcadeDbContext dbContext,
        PasswordHasher hasher,
        CancellationToken cancellationToken)
    {
        if (!MiniValidator.TryValidate(dto, out var errors))
            return Results.ValidationProblem(errors);

        var normalizedName = dto.Username.Trim();
        if (normalizedName.Length is < 3 or > 40)
            return Results.BadRequest(new { message = "Ungültiger Benutzername." });

        var lookupName = normalizedName.ToLowerInvariant();
        var exists = await dbContext.Users
            .AnyAsync(u => u.Username.ToLower() == lookupName, cancellationToken);

        if (exists)
            return Results.Conflict(new { message = "Benutzername bereits vergeben." });

        var hash = hasher.Hash(dto.Password);

        var user = new User
        {
            Username = normalizedName,
            Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
            PasswordHash = hash,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Results.Ok();
    }

    private static async Task<IResult> LoginAsync(
     LoginDto dto,
     HttpContext httpContext,
     ArcadeDbContext dbContext,
     PasswordHasher hasher,
     CancellationToken cancellationToken)
    {
        if (!MiniValidator.TryValidate(dto, out var errors))
            return Results.ValidationProblem(errors);

        var lookupName = dto.Username.Trim().ToLowerInvariant();
        var user = await dbContext.Users
            .SingleOrDefaultAsync(u => u.Username.ToLower() == lookupName, cancellationToken);

        if (user is null || !hasher.Verify(dto.Password, user.PasswordHash))
        {
            await Task.Delay(Random.Shared.Next(25, 75), cancellationToken);
            return Results.BadRequest(new { message = "Ungültige Zugangsdaten." });
        }

        var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.Username),
        new(ClaimTypes.Role, "spieler")
    };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

        var props = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(SignInDuration)
        };
        return Results.SignIn(principal, props, CookieAuthenticationDefaults.AuthenticationScheme);
    }


    private static async Task<IResult> LogoutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Ok();
    }
}

file static class MiniValidator
{
    public static bool TryValidate<T>(T instance, out Dictionary<string, string[]> errors)
    {
        var validationContext = new ValidationContext(instance!);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(instance!, validationContext, validationResults, true);

        if (isValid)
        {
            errors = new Dictionary<string, string[]>();
            return true;
        }

        errors = validationResults
            .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
            .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? string.Empty).ToArray());

        return false;
    }
}
