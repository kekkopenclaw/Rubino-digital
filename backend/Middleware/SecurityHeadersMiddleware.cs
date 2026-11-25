namespace RubinoDigital.Api.Middleware;

/// <summary>Security headers on every response. CSP allows Angular's inline
/// styles; tighten script-src if you add an external script.</summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var h = context.Response.Headers;
        h["X-Content-Type-Options"] = "nosniff";
        h["X-Frame-Options"] = "DENY";
        h["Referrer-Policy"] = "strict-origin-when-cross-origin";
        h["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        h["Content-Security-Policy"] =
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; font-src 'self' data:; connect-src 'self'; " +
            "frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
        await _next(context);
    }
}
