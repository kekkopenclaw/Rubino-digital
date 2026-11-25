using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubinoDigital.Api.Data;
using RubinoDigital.Api.Dtos;
using RubinoDigital.Api.Models;
using RubinoDigital.Api.Services;

namespace RubinoDigital.Api.Controllers;

[ApiController]
[Route("api/quotes")]
public class QuotesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PricingService _pricing;
    public QuotesController(AppDbContext db, PricingService pricing) { _db = db; _pricing = pricing; }

    private Guid? CurrentUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(v, out var id) ? id : null;
    }

    [HttpPost("calculate")]
    public IActionResult Calculate([FromBody] CalculateQuoteRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try { return Ok(_pricing.Calculate(req)); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQuoteRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        QuoteResult calc;
        try { calc = _pricing.Calculate(req.Quote); }
        catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
        var quote = new Quote
        {
            Reference = $"RD-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100000, 999999)}",
            UserId = CurrentUserId(),
            ProjectType = calc.ProjectType, ExtraPages = calc.ExtraPages,
            AddOnIdsJson = JsonSerializer.Serialize(req.Quote.AddOnIds ?? new List<string>()),
            Delivery = calc.Delivery, CarePlan = calc.CarePlan,
            Total = calc.Total, Deposit = calc.Deposit, Balance = calc.Balance,
            TimelineWeeks = calc.TimelineWeeks, Status = "Draft"
        };
        _db.Quotes.Add(quote);
        await _db.SaveChangesAsync();
        return StatusCode(201, new QuoteResponse(quote.Reference, calc, quote.Status, quote.CreatedAt));
    }

    [HttpGet("{reference}")]
    public async Task<IActionResult> GetByReference(string reference)
    {
        var quote = await _db.Quotes.FirstOrDefaultAsync(q => q.Reference == reference);
        if (quote is null) return NotFound(new { error = "Quote not found." });
        var calc = _pricing.Calculate(new CalculateQuoteRequest(quote.ProjectType, quote.ExtraPages,
            JsonSerializer.Deserialize<List<string>>(quote.AddOnIdsJson) ?? new List<string>(),
            quote.Delivery, quote.CarePlan));
        return Ok(new QuoteResponse(quote.Reference, calc, quote.Status, quote.CreatedAt));
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> Mine()
    {
        var id = CurrentUserId()!.Value;
        var quotes = await _db.Quotes.Where(q => q.UserId == id).OrderByDescending(q => q.CreatedAt).ToListAsync();
        return Ok(quotes.Select(q => new { q.Reference, q.ProjectType, q.Total, q.Deposit, q.Balance, q.TimelineWeeks, q.Status, q.CreatedAt }));
    }
}
