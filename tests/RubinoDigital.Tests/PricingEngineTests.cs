using RubinoDigital.Api.Dtos;
using RubinoDigital.Api.Services;

namespace RubinoDigital.Tests;

/// <summary>Pricing engine tests — the numbers in pricing.json and the
/// Angular calculator must match these. Run: dotnet test</summary>
public class PricingEngineTests
{
    private readonly PricingService _pricing = new();

    [Fact]
    public void BusinessWebsite_PlusTwoExtraPages_Is1039()
    {
        // The headline verification: £799 base + 2 x £120 = £1,039, standard delivery.
        var result = _pricing.Calculate(new CalculateQuoteRequest("business", 2, new List<string>(), "standard", null));
        Assert.Equal(799m, result.BasePrice);
        Assert.Equal(240m, result.ExtraPagesCost);
        Assert.Equal(1039m, result.Total);
        Assert.Equal(1039m, result.Subtotal);
        Assert.Equal(311.70m, result.Deposit);   // 30% of £1,039
        Assert.Equal(727.30m, result.Balance);   // 70% of £1,039
        Assert.Equal(3, result.TimelineWeeks);   // 2 base weeks + ceil(2/4) = 3
    }

    [Fact]
    public void FourOrMoreAddOns_GetTenPercentBundleDiscount()
    {
        // CMS £150 + SEO £200 + Analytics £120 + Copywriting £180 = £650; 10% = £65 off.
        var result = _pricing.Calculate(new CalculateQuoteRequest("business", 0,
            new List<string> { "cms-blog", "seo", "analytics", "copywriting" }, "standard", null));
        Assert.Equal(650m, result.AddOnsSubtotal);
        Assert.Equal(65m, result.BundleDiscount);
        Assert.Equal(799m + 650m - 65m, result.Total);
    }

    [Fact]
    public void ThreeAddOns_GetNoBundleDiscount()
    {
        var result = _pricing.Calculate(new CalculateQuoteRequest("business", 0,
            new List<string> { "cms-blog", "seo", "analytics" }, "standard", null));
        Assert.Equal(0m, result.BundleDiscount);
        Assert.Equal(799m + 150m + 200m + 120m, result.Total);
    }

    [Fact]
    public void FastDelivery_AddsFifteenPercent()
    {
        var result = _pricing.Calculate(new CalculateQuoteRequest("business", 2, new List<string>(), "fast", null));
        Assert.Equal(1.15m, result.DeliveryMultiplier);
        Assert.Equal(1039m * 1.15m, result.Total);       // £1,194.85
        Assert.Equal(155.85m, result.DeliverySurcharge); // 15% of £1,039
        Assert.Equal(Math.Round(1194.85m * 0.30m, 2), result.Deposit);
    }

    [Fact]
    public void RushDelivery_AddsThirtyPercent_AndDepositIsThirtyPercent()
    {
        var result = _pricing.Calculate(new CalculateQuoteRequest("landing", 0, new List<string>(), "rush", null));
        Assert.Equal(499m * 1.30m, result.Total); // £648.70
        Assert.Equal(Math.Round(648.70m * 0.30m, 2), result.Deposit);
        Assert.Equal(result.Total - result.Deposit, result.Balance);
    }

    [Fact]
    public void CarePlan_IsMonthly_AndNotInOneOffTotal()
    {
        var result = _pricing.Calculate(new CalculateQuoteRequest("business", 0, new List<string>(), "standard", "growth"));
        Assert.Equal(99m, result.CarePlanMonthly);
        Assert.Equal(799m, result.Total);
    }

    [Fact]
    public void UnknownProjectType_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            _pricing.Calculate(new CalculateQuoteRequest("nope", 0, new List<string>(), "standard", null)));
    }

    [Fact]
    public void PricingJson_MatchesCodeTable()
    {
        // pricing.json lives at the repo root; the test runs from tests/bin, so walk up.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "pricing.json")))
            dir = dir.Parent;
        Assert.NotNull(dir);
        Assert.True(PricingService.PricingJsonMatchesCode(Path.Combine(dir!.FullName, "pricing.json")));
        Assert.Equal(6, PricingService.ProjectTypes.Count);
        Assert.Equal(17, PricingService.AddOns.Count);
        Assert.Equal(3, PricingService.CarePlans.Count);
    }
}
