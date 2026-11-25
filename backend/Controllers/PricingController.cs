using Microsoft.AspNetCore.Mvc;
using RubinoDigital.Api.Services;

namespace RubinoDigital.Api.Controllers;

[ApiController]
[Route("api/pricing")]
public class PricingController : ControllerBase
{
    private readonly PricingService _pricing;
    public PricingController(PricingService pricing) => _pricing = pricing;

    [HttpGet]
    public IActionResult Get() => Ok(_pricing.PricingDocument());
}

[ApiController]
[Route("api/portfolio")]
public class PortfolioController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new[]
    {
        new { id = "filemyreturn", name = "FileMyReturn", type = "Web App / Custom Software",
            description = "Online Self Assessment tax platform — customers create an account, complete their return in the software, pay by card, and the return is checked and filed to HMRC.",
            technologies = new[] { ".NET 10", "Angular 18", "Stripe", "HMRC sandbox" }, priceFrom = 2999m },
        new { id = "smash", name = "SMASH — Cinematic Burger Site", type = "Business Website",
            description = "Dark, gold-accented 3D-scroll restaurant site with a burger builder, 90-second sear timer and table reservations.",
            technologies = new[] { "Angular 18", "CSS 3D", "Scroll animation" }, priceFrom = 799m }
    });
}

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new[]
    {
        new { id = "smash-template", name = "SMASH Restaurant Template", price = 349m, projectType = "template",
            addOnIds = new[] { "booking", "animation-3d" },
            description = "The SMASH burger site as a ready-made template — your menu, photos and branding, live in days." },
        new { id = "filemyreturn-platform", name = "FileMyReturn Platform", price = 2999m, projectType = "webapp",
            addOnIds = new[] { "accounts", "admin-dashboard", "stripe" },
            description = "The FileMyReturn tax platform pattern — accounts, forms, payments and an admin dashboard — rebuilt for your service." },
        new { id = "template-pack", name = "Starter Template Pack", price = 499m, projectType = "landing",
            addOnIds = new[] { "seo", "analytics" },
            description = "A landing page plus a business template, with SEO and analytics set up, ready to launch." }
    });
}
