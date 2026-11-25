using System.Text.Json;
using RubinoDigital.Api.Dtos;

namespace RubinoDigital.Api.Services;

/// <summary>
/// Pricing engine — the server-side single source of truth. It mirrors
/// pricing.json exactly, and the Angular calculator mirrors this file.
/// Rule order (do not change without updating both + the tests):
///   subtotalBeforeDelivery = base + extraPages*£120 + (add-ons - bundle discount)
///   bundle discount = 10% of add-ons subtotal when 4+ add-ons are selected
///   total = subtotalBeforeDelivery * delivery multiplier (Standard 1.00, Fast 1.15, Rush 1.30)
///   deposit = 30% of total, balance = 70% of total
///   timelineWeeks = baseWeeks + ceil(extraPages/4) + ceil(addOns/4)
/// Care plans are monthly and are NOT included in the one-off total.
/// </summary>
public class PricingService
{
    public record ProjectTypeDef(string Id, string Name, decimal Price, int BaseWeeks, string Description);
    public record AddOnDef(string Id, string Name, decimal Price, string Description);
    public record CarePlanDef(string Id, string Name, decimal MonthlyPrice, string Description);

    public static readonly IReadOnlyList<ProjectTypeDef> ProjectTypes = new[]
    {
        new ProjectTypeDef("template", "Template", 349m, 1, "A ready-made design, customised with your content and branding."),
        new ProjectTypeDef("landing", "Landing Page", 499m, 1, "One focused page designed to turn visitors into enquiries or sales."),
        new ProjectTypeDef("business", "Business Website", 799m, 2, "A complete multi-page website for your business."),
        new ProjectTypeDef("ecommerce", "E-commerce", 1499m, 4, "An online shop with products, checkout and payments."),
        new ProjectTypeDef("webapp", "Web App", 2999m, 6, "A custom web application with accounts, dashboards and workflows."),
        new ProjectTypeDef("custom", "Custom Software", 4999m, 8, "Bespoke software built around how your business actually works."),
    };

    public static readonly IReadOnlyList<AddOnDef> AddOns = new[]
    {
        new AddOnDef("ecommerce-payments", "E-commerce & online payments", 400m, "Sell products and take payments online."),
        new AddOnDef("booking", "Booking / reservation system", 300m, "Let customers book a table, appointment or slot online."),
        new AddOnDef("cms-blog", "CMS & blog", 150m, "Edit your own pages and publish blog posts."),
        new AddOnDef("seo", "SEO setup", 200m, "Search-friendly pages, titles and sitemap so Google can find you."),
        new AddOnDef("branding", "Logo & branding", 250m, "Logo, colours and a simple brand kit."),
        new AddOnDef("animation-3d", "3D scroll & cinematic animation", 350m, "Scroll-driven 3D effects like the SMASH burger site."),
        new AddOnDef("accounts", "User accounts & login", 400m, "Customers or staff can sign up, log in and manage their details."),
        new AddOnDef("admin-dashboard", "Admin dashboard", 500m, "A private dashboard to manage orders, bookings or content."),
        new AddOnDef("stripe", "Stripe payments integration", 300m, "Secure card payments with Stripe (test mode until you go live)."),
        new AddOnDef("mobile-app", "Mobile app version", 800m, "Your site as an app-style mobile experience."),
        new AddOnDef("copywriting", "Copywriting", 180m, "Clear, professional wording for your pages."),
        new AddOnDef("analytics", "Analytics & tracking", 120m, "See how many visitors you get and what they do."),
        new AddOnDef("hosting", "Hosting & domain setup", 150m, "Domain, hosting and SSL set up for you."),
        new AddOnDef("multilingual", "Multilingual", 250m, "Your site in a second language."),
        new AddOnDef("live-chat", "Live chat & support", 180m, "Visitors can message you straight from the site."),
        new AddOnDef("integrations", "Third-party integrations", 350m, "Connect a CRM, email tool or other service you already use."),
        new AddOnDef("accessibility", "Accessibility & performance", 120m, "Fast loading and usable for everyone, including screen readers."),
    };

    public static readonly IReadOnlyList<CarePlanDef> CarePlans = new[]
    {
        new CarePlanDef("essential", "Essential Care", 49m, "Hosting checks, updates and small fixes."),
        new CarePlanDef("growth", "Growth Care", 99m, "Everything in Essential, plus monthly content updates and reports."),
        new CarePlanDef("premium", "Premium Care", 199m, "Everything in Growth, plus priority support and new features each month."),
    };

    public const decimal ExtraPagePrice = 120m;
    public const decimal BundleDiscountRate = 0.10m;
    public const int BundleDiscountMinAddOns = 4;
    public const decimal DepositRate = 0.30m;

    private static readonly Dictionary<string, decimal> DeliveryMultipliers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["standard"] = 1.00m, ["fast"] = 1.15m, ["rush"] = 1.30m
    };

    public QuoteResult Calculate(CalculateQuoteRequest req)
    {
        var type = ProjectTypes.FirstOrDefault(t => t.Id.Equals(req.ProjectType, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"Unknown project type: {req.ProjectType}", nameof(req));
        var deliveryKey = string.IsNullOrWhiteSpace(req.Delivery) ? "standard" : req.Delivery.Trim().ToLowerInvariant();
        if (!DeliveryMultipliers.TryGetValue(deliveryKey, out var multiplier))
            throw new ArgumentException($"Unknown delivery option: {req.Delivery}", nameof(req));
        var extraPages = Math.Max(0, Math.Min(50, req.ExtraPages));
        var addOnIds = (req.AddOnIds ?? new List<string>()).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var addOns = addOnIds.Select(id => AddOns.FirstOrDefault(a => a.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException($"Unknown add-on: {id}", nameof(req)))
            .Select(a => new QuoteLine(a.Id, a.Name, a.Price)).ToList();

        var extraPagesCost = extraPages * ExtraPagePrice;
        var addOnsSubtotal = addOns.Sum(a => a.Price);
        var bundleDiscount = addOns.Count >= BundleDiscountMinAddOns
            ? Math.Round(addOnsSubtotal * BundleDiscountRate, 2) : 0m;
        var subtotalBeforeDelivery = type.Price + extraPagesCost + (addOnsSubtotal - bundleDiscount);
        var deliverySurcharge = Math.Round(subtotalBeforeDelivery * (multiplier - 1m), 2);
        var total = Math.Round(subtotalBeforeDelivery * multiplier, 2);
        var deposit = Math.Round(total * DepositRate, 2);
        var balance = Math.Round(total - deposit, 2);
        var timelineWeeks = type.BaseWeeks
            + (int)Math.Ceiling(extraPages / 4.0)
            + (int)Math.Ceiling(addOns.Count / 4.0);

        CarePlanDef? care = null;
        if (!string.IsNullOrWhiteSpace(req.CarePlan) && !req.CarePlan.Equals("none", StringComparison.OrdinalIgnoreCase))
        {
            care = CarePlans.FirstOrDefault(c => c.Id.Equals(req.CarePlan, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException($"Unknown care plan: {req.CarePlan}", nameof(req));
        }

        return new QuoteResult(
            type.Id, type.Name, type.Price,
            extraPages, extraPagesCost,
            addOns, addOnsSubtotal, bundleDiscount,
            deliveryKey, multiplier, deliverySurcharge,
            subtotalBeforeDelivery, total, deposit, balance,
            care?.Id, care?.MonthlyPrice ?? 0m, timelineWeeks, "GBP");
    }

    /// <summary>pricing.json as served by GET /api/pricing — same numbers as above.</summary>
    public object PricingDocument() => new
    {
        currency = "GBP", vatIncluded = true, extraPagePrice = ExtraPagePrice,
        bundleDiscountRate = BundleDiscountRate, bundleDiscountMinAddOns = BundleDiscountMinAddOns,
        depositRate = DepositRate,
        projectTypes = ProjectTypes.Select(t => new { id = t.Id, name = t.Name, price = t.Price, priceLabel = $"£{t.Price:N0}", baseWeeks = t.BaseWeeks, description = t.Description }),
        addOns = AddOns.Select(a => new { id = a.Id, name = a.Name, price = a.Price, priceLabel = $"£{a.Price:N0}", description = a.Description }),
        delivery = new[] {
            new { id = "standard", name = "Standard", multiplier = 1.00m, label = "Standard — no extra charge" },
            new { id = "fast", name = "Fast", multiplier = 1.15m, label = "Fast — +15%" },
            new { id = "rush", name = "Rush", multiplier = 1.30m, label = "Rush — +30%" } },
        carePlans = CarePlans.Select(c => new { id = c.Id, name = c.Name, monthlyPrice = c.MonthlyPrice, priceLabel = $"£{c.MonthlyPrice:N0}/month", description = c.Description })
    };

    // pricing.json at the repo root ships with identical numbers. This check
    // lets the tests prove the JSON file and the in-code table agree.
    public static bool PricingJsonMatchesCode(string jsonPath)
    {
        if (!File.Exists(jsonPath)) return false;
        using var doc = JsonDocument.Parse(File.ReadAllText(jsonPath));
        var root = doc.RootElement;
        return root.GetProperty("extraPagePrice").GetDecimal() == ExtraPagePrice
            && root.GetProperty("projectTypes").GetArrayLength() == ProjectTypes.Count
            && root.GetProperty("addOns").GetArrayLength() == AddOns.Count;
    }
}
