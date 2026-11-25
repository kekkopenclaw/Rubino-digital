using System.Security.Claims;
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
[Route("api/enquiries")]
public class EnquiriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    public EnquiriesController(AppDbContext db, IEmailService email, IConfiguration config)
    { _db = db; _email = email; _config = config; }

    [HttpPost]
    [EnableRateLimiting("enquiry")]
    public async Task<IActionResult> Create([FromBody] CreateEnquiryRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : (Guid?)null;
        var enquiry = new Enquiry
        {
            Reference = $"ENQ-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100000, 999999)}",
            UserId = userId, QuoteReference = req.QuoteReference,
            Name = req.Name.Trim(), Email = req.Email.Trim().ToLowerInvariant(),
            Phone = req.Phone, Message = req.Message.Trim(), QuoteSummary = req.QuoteSummary
        };
        _db.Enquiries.Add(enquiry);
        await _db.SaveChangesAsync();
        // Confirmation to the visitor (logged, not sent, in Log/test mode).
        await _email.SendEnquiryReceivedAsync(enquiry.Email, enquiry.Name, enquiry.Reference);
        // Notification to the agency — only if a recipient is configured.
        // No business email is invented: leave Agency__Email blank and this is skipped.
        var agencyEmail = _config["Agency:Email"];
        if (!string.IsNullOrWhiteSpace(agencyEmail))
            await _email.SendEnquiryNotificationAsync(agencyEmail, enquiry.Name, enquiry.Email, enquiry.Reference, enquiry.Message);
        return StatusCode(201, new EnquiryResponse(enquiry.Reference, enquiry.Status, enquiry.CreatedAt));
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> Mine()
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var list = await _db.Enquiries.Where(e => e.UserId == id).OrderByDescending(e => e.CreatedAt).ToListAsync();
        return Ok(list.Select(e => new { e.Reference, e.QuoteReference, e.Message, e.Status, e.CreatedAt }));
    }
}
