using System.ComponentModel.DataAnnotations;
using RubinoDigital.Api.Services;

namespace RubinoDigital.Api.Dtos;

// --- Auth ---
public record RegisterRequest(
    [Required, StringLength(100)] string FullName,
    [Required, EmailAddress, StringLength(200)] string Email,
    [Required, StringLength(100, MinimumLength = 8)] string Password,
    string? Phone);
public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public record RefreshRequest(string? RefreshToken);
public record AuthResponse(string Token, Guid UserId, string FullName, string Email, bool EmailConfirmed);
public record VerifyEmailRequest([Required] string Token);
public record ForgotPasswordRequest([Required, EmailAddress] string Email);
public record ResetPasswordRequest([Required] string Token, [Required, StringLength(100, MinimumLength = 8)] string NewPassword);

// --- Quotes ---
public record CalculateQuoteRequest(
    [Required] string ProjectType,
    [Range(0, 50)] int ExtraPages,
    List<string>? AddOnIds,
    string Delivery = "standard",
    string? CarePlan = null);
public record QuoteLine(string Id, string Name, decimal Price);
public record QuoteResult(
    string ProjectType, string ProjectTypeName, decimal BasePrice,
    int ExtraPages, decimal ExtraPagesCost,
    List<QuoteLine> AddOns, decimal AddOnsSubtotal, decimal BundleDiscount,
    string Delivery, decimal DeliveryMultiplier, decimal DeliverySurcharge,
    decimal Subtotal, decimal Total, decimal Deposit, decimal Balance,
    string? CarePlan, decimal CarePlanMonthly, int TimelineWeeks, string Currency);
public record CreateQuoteRequest(CalculateQuoteRequest Quote);
public record QuoteResponse(string Reference, QuoteResult Calculation, string Status, DateTime CreatedAt);

// --- Enquiries ---
public record CreateEnquiryRequest(
    [Required, StringLength(100)] string Name,
    [Required, EmailAddress, StringLength(200)] string Email,
    string? Phone,
    [Required, StringLength(4000, MinimumLength = 10)] string Message,
    string? QuoteReference,
    string? QuoteSummary);
public record EnquiryResponse(string Reference, string Status, DateTime CreatedAt);

// --- Payments (test-mode stub) ---
public record CheckoutRequest([Required] string QuoteReference);
public record CheckoutResponse(string Mode, string QuoteReference, decimal Amount, string Currency,
    string? CheckoutUrl, string TestCard, string Message);
