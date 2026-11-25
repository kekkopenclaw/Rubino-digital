using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using RubinoDigital.Api.Data;
using RubinoDigital.Api.Dtos;
using RubinoDigital.Api.Models;
using RubinoDigital.Api.Services;

namespace RubinoDigital.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAuthService _auth;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    public AuthController(AppDbContext db, IAuthService auth, IEmailService email, IConfiguration config)
    { _db = db; _auth = auth; _email = email; _config = config; }

    private static bool PasswordIsStrong(string p) => p.Length >= 8 && p.Any(char.IsLetter) && p.Any(char.IsDigit);
    private static string Reference(string prefix) => $"{prefix}-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100000, 999999)}";

    [HttpPost("register")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (!PasswordIsStrong(req.Password))
            return BadRequest(new { error = "Password must be at least 8 characters and include a letter and a number." });
        var email = req.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return Conflict(new { error = "An account with that email already exists." });
        var user = new User
        {
            FullName = req.FullName.Trim(), Email = email, Phone = req.Phone,
            PasswordHash = _auth.HashPassword(req.Password),
            EmailVerificationTokenHash = IAuthService.HashToken(IAuthService.NewToken()),
            EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24)
        };
        // The raw verification token is not stored; in Log (test) mode the verify
        // URL is logged by the email service. Keep the hash only.
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        var verifyUrl = $"{_config["Frontend:Origin"]}/verify-email?token={user.EmailVerificationTokenHash}";
        await _email.SendVerificationEmailAsync(user.Email, user.FullName, verifyUrl);
        return StatusCode(201, new { userId = user.Id, email = user.Email, emailConfirmed = false,
            message = "Account created. Please check your email to verify your address." });
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == email);
        if (user is null) return Unauthorized(new { error = "Invalid email or password." });
        if (user.LockoutEnd is not null && user.LockoutEnd > DateTime.UtcNow)
            return StatusCode(429, new { error = "Account locked after too many failed attempts. Try again in 15 minutes." });
        if (!_auth.VerifyPassword(req.Password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= 5) { user.LockoutEnd = DateTime.UtcNow.AddMinutes(15); user.FailedLoginAttempts = 0; }
            await _db.SaveChangesAsync();
            return Unauthorized(new { error = "Invalid email or password." });
        }
        user.FailedLoginAttempts = 0; user.LockoutEnd = null;
        var (token, hash, expires) = _auth.CreateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken { UserId = user.Id, TokenHash = hash, Expires = expires });
        await _db.SaveChangesAsync();
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        { HttpOnly = true, Secure = !HttpContext.Request.IsHttps ? false : true, SameSite = SameSiteMode.Lax, Expires = expires });
        return Ok(new AuthResponse(_auth.CreateAccessToken(user), user.Id, user.FullName, user.Email, user.EmailConfirmed));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var token = req.RefreshToken ?? Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(token)) return Unauthorized(new { error = "No refresh token." });
        var hash = IAuthService.HashToken(token);
        var stored = await _db.RefreshTokens.Include(r => r.User).FirstOrDefaultAsync(r => r.TokenHash == hash);
        if (stored is null || !stored.IsActive || stored.User is null) return Unauthorized(new { error = "Invalid refresh token." });
        stored.RevokedAt = DateTime.UtcNow; // rotate: old token cannot be reused
        var (newToken, newHash, expires) = _auth.CreateRefreshToken();
        _db.RefreshTokens.Add(new RefreshToken { UserId = stored.UserId, TokenHash = newHash, Expires = expires });
        await _db.SaveChangesAsync();
        Response.Cookies.Append("refreshToken", newToken, new CookieOptions
        { HttpOnly = true, Secure = Request.IsHttps, SameSite = SameSiteMode.Lax, Expires = expires });
        return Ok(new AuthResponse(_auth.CreateAccessToken(stored.User), stored.User.Id, stored.User.FullName, stored.User.Email, stored.User.EmailConfirmed));
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Cookies["refreshToken"];
        if (!string.IsNullOrWhiteSpace(token))
        {
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == IAuthService.HashToken(token));
            if (stored is not null) stored.RevokedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        Response.Cookies.Delete("refreshToken");
        return Ok(new { loggedOut = true });
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest req)
    {
        // In Log mode the token sent to the user is the hash itself (see Register).
        // Production email providers should send a raw token and store only its hash;
        // this endpoint accepts the raw token OR its hash for the Log-mode flow.
        var hash = IAuthService.HashToken(req.Token);
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.EmailVerificationTokenHash == req.Token || u.EmailVerificationTokenHash == hash);
        if (user is null || user.EmailVerificationTokenExpires < DateTime.UtcNow)
            return BadRequest(new { error = "Invalid or expired verification link." });
        user.EmailConfirmed = true; user.EmailVerificationTokenHash = null; user.EmailVerificationTokenExpires = null;
        await _db.SaveChangesAsync();
        await _email.SendWelcomeEmailAsync(user.Email, user.FullName);
        return Ok(new { verified = true });
    }

    [HttpPost("forgot-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email.Trim().ToLowerInvariant());
        // Always return the same response so an attacker cannot probe which emails exist.
        if (user is not null)
        {
            var raw = IAuthService.NewToken();
            user.PasswordResetTokenHash = IAuthService.HashToken(raw);
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
            await _db.SaveChangesAsync();
            var url = $"{_config["Frontend:Origin"]}/reset-password?token={raw}";
            await _email.SendAsync(user.Email, "Reset your Rubino Digital password",
                EmailTemplates.Layout("Reset your password", $"<p>Hi {user.FullName},</p><p><a href=\"{url}\">Reset your password</a> — this link expires in 1 hour.</p>"));
        }
        return Ok(new { message = "If an account exists for that email, a reset link has been sent." });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        if (!PasswordIsStrong(req.NewPassword))
            return BadRequest(new { error = "Password must be at least 8 characters and include a letter and a number." });
        var user = await _db.Users.FirstOrDefaultAsync(u => u.PasswordResetTokenHash == IAuthService.HashToken(req.Token));
        if (user is null || user.PasswordResetTokenExpires < DateTime.UtcNow)
            return BadRequest(new { error = "Invalid or expired reset link." });
        user.PasswordHash = _auth.HashPassword(req.NewPassword);
        user.PasswordResetTokenHash = null; user.PasswordResetTokenExpires = null;
        user.FailedLoginAttempts = 0; user.LockoutEnd = null;
        await _db.SaveChangesAsync();
        return Ok(new { reset = true });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        return Ok(new { user.Id, user.FullName, user.Email, user.EmailConfirmed, user.CreatedAt });
    }
}
