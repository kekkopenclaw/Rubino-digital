namespace RubinoDigital.Api.Models;

public class Enquiry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Reference { get; set; } = string.Empty; // e.g. ENQ-2026-000123
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public string? QuoteReference { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? QuoteSummary { get; set; }
    public string Status { get; set; } = "New"; // New | Replied | Closed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
