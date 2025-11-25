using Arcade.Components;
using Arcade.Data;
using Arcade.Data.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// DB-Verbindung
var cs = builder.Configuration.GetConnectionString("ArcadeDb")
         ?? "Server=localhost;Port=3306;Database=arcadetestdb;User=root;Password=;SslMode=None;";

var serverVersion = new MariaDbServerVersion(new Version(10, 4, 32));

builder.Services.AddDbContext<ArcadeDbContext>(options =>
    options.UseMySql(cs, serverVersion));

// PasswordHasher
builder.Services.AddScoped<PasswordHasher>();

builder.Services.AddHttpContextAccessor();

// Cookie Auth
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "arcade.auth";
        options.LoginPath = "/anmelden";
        options.LogoutPath = "/abmelden";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);

        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpClient();

var app = builder.Build();

// DB-Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ArcadeDbContext>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "DB Migration failed. App starts without DB.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapStaticAssets();

// Razor Components aktivieren
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();

// API Endpoints laden (Login/Registrierung etc.)
app.MapAuthEndpoints();

// LOGOUT-ROUTE
app.MapGet("/abmelden", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.Run();
