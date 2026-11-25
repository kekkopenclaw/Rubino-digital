using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using RubinoDigital.Api.Models;

namespace RubinoDigital.Api.Services;

// Passwords are BCrypt hashes only. Refresh / verification / reset tokens are
// stored as SHA-256 hashes only — the raw token is never written to the database.
public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    string CreateAccessToken(User user);
    (string Token, string TokenHash, DateTime Expires) CreateRefreshToken();
    static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    static string NewToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
}

public class AuthService : IAuthService
{
    private readonly IConfiguration _config;
    public AuthService(IConfiguration config) => _config = config;

    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

    public string CreateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName)
        };
        var minutes = int.Parse(_config["Jwt:AccessExpiryMinutes"] ?? "15");
        var jwt = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"], audience: _config["Jwt:Issuer"], claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    public (string, string, DateTime) CreateRefreshToken()
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var days = int.Parse(_config["Jwt:RefreshExpiryDays"] ?? "7");
        return (token, IAuthService.HashToken(token), DateTime.UtcNow.AddDays(days));
    }
}
