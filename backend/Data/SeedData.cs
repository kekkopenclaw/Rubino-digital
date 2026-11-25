using RubinoDigital.Api.Models;
using RubinoDigital.Api.Services;

namespace RubinoDigital.Api.Data;

/// <summary>Demo account only. Email is a placeholder domain; change or remove before real use.</summary>
public static class SeedData
{
    public static void EnsureSeeded(AppDbContext db, IAuthService auth)
    {
        if (db.Users.Any(u => u.Email == "demo@rubinodigital.example")) return;
        db.Users.Add(new User
        {
            FullName = "Demo Client",
            Email = "demo@rubinodigital.example",
            PasswordHash = auth.HashPassword("Demo123!"),
            EmailConfirmed = true
        });
        db.SaveChanges();
    }
}
