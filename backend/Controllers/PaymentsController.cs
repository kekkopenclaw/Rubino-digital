using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RubinoDigital.Api.Data;
using RubinoDigital.Api.Dtos;
using RubinoDigital.Api.Services;

namespace RubinoDigital.Api.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPaymentService _payments;
    public PaymentsController(AppDbContext db, IPaymentService payments) { _db = db; _payments = payments; }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest req)
    {
        var quote = await _db.Quotes.FirstOrDefaultAsync(q => q.Reference == req.QuoteReference);
        if (quote is null) return NotFound(new { error = "Quote not found." });
        // Deposit only at checkout — the balance is invoiced on delivery.
        return Ok(_payments.CreateTestCheckout(quote.Reference, quote.Deposit));
    }
}
