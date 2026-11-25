namespace RubinoDigital.Api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty; // BCrypt, never plain text
    public string? Phone { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? EmailVerificationTokenHash { get; set; }
    public DateTime? EmailVerificationTokenExpires { get; set; }
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetTokenExpires { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Quote> Quotes { get; set; } = new();
    public List<Enquiry> Enquiries { get; set; } = new();
    public List<RefreshToken> RefreshTokens { get; set; } = new();
}
