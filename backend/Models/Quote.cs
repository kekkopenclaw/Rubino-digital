namespace RubinoDigital.Api.Models;

public class Quote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Reference { get; set; } = string.Empty; // e.g. RD-2026-000123
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    public string ProjectType { get; set; } = string.Empty;
    public int ExtraPages { get; set; }
    public string AddOnIdsJson { get; set; } = "[]"; // JSON array of add-on ids
    public string Delivery { get; set; } = "standard";
    public string? CarePlan { get; set; }
    public decimal Total { get; set; }
    public decimal Deposit { get; set; }
    public decimal Balance { get; set; }
    public int TimelineWeeks { get; set; }
    public string Status { get; set; } = "Draft"; // Draft | Enquired | Accepted
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
