using Arcade.Components;
using Arcade.Data;
using Arcade.Data.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Razor Components mit Server-Interaktivität
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

// DB + Services
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=app.db"));

builder.Services.AddScoped<PasswordHasher>();

// AuthN/AuthZ
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "arcade.auth";
        options.LoginPath = "/anmelden";
        options.LogoutPath = "/abmelden";
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();                // wichtig vor MapRazorComponents
app.MapStaticAssets();

app.MapRazorComponents<App>()        // genau EINMAL
   .AddInteractiveServerRenderMode();

app.MapAuthEndpoints();

app.Run();
