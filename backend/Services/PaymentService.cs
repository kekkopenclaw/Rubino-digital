using RubinoDigital.Api.Dtos;

namespace RubinoDigital.Api.Services;

// Payments — TEST-MODE STUB ONLY. No money moves, no payment library is
// referenced, and no live key exists anywhere in this repo.
// To take real payments later, create a FREE Stripe account (no monthly fee),
// stay in test mode with sk_test_ keys, and implement checkout with Stripe's
// free Checkout API. Live payments need your own Stripe account + bank details.
// Until then this returns test-mode information and the quote stays unpaid.
public interface IPaymentService
{
    CheckoutResponse CreateTestCheckout(string quoteReference, decimal amount);
}

public class PaymentService : IPaymentService
{
    private readonly IConfiguration _config;
    public PaymentService(IConfiguration config) => _config = config;

    public CheckoutResponse CreateTestCheckout(string quoteReference, decimal amount)
    {
        var configured = !string.IsNullOrWhiteSpace(_config["Stripe:SecretKey"]);
        return new CheckoutResponse(
            Mode: "test",
            QuoteReference: quoteReference,
            Amount: amount,
            Currency: "GBP",
            CheckoutUrl: null,
            TestCard: "4242 4242 4242 4242",
            Message: configured
                ? "Test mode: a Stripe test key is set, but checkout is a stub in this build. Implement Stripe Checkout (free test mode) to take test payments."
                : "Test mode: no Stripe key is set. Add Stripe__SecretKey (sk_test_…) from your own free Stripe test account to take test payments. No payment was taken.");
    }
}
