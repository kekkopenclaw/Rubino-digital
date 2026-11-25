using System.Net;

namespace RubinoDigital.Api.Middleware;

/// <summary>Global error handling — returns a safe JSON error, never a stack
/// trace or a secret. Logs the full exception server-side only.</summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error {Method} {Path}", context.Request.Method, context.Request.Path);
            if (context.Response.HasStarted) return;
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Something went wrong. Please try again." });
        }
    }
}
