using System.Text;
using System.Text.Json;

namespace RubinoDigital.Api.Services;

// Email is Log-only by default: nothing is sent, the message is written to the
// application log. To send real mail, set Email__Provider to Resend or
// SendGrid — both have a free tier (100 emails/day) — and set Email__ApiKey
// from your own free account. No paid email service is needed.
public interface IEmailService
{
    Task SendAsync(string to, string subject, string htmlBody);
    Task SendVerificationEmailAsync(string to, string name, string verifyUrl);
    Task SendWelcomeEmailAsync(string to, string name);
    Task SendEnquiryReceivedAsync(string to, string name, string reference);
    Task SendEnquiryNotificationAsync(string agencyEmail, string name, string email, string reference, string message);
}

public static class EmailTemplates
{
    public static string Layout(string title, string body) =>
        "<html><body style=\"font-family:system-ui,sans-serif;max-width:600px;margin:0 auto;padding:24px\">" +
        $"<h1>{System.Net.WebUtility.HtmlEncode(title)}</h1>{body}" +
        "<p style=\"color:#666;font-size:13px\">Rubino Digital — Websites, web apps and custom software.</p></body></html>";
    public static string Verify(string name, string url) => Layout("Verify your email",
        $"<p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p><p>Please confirm your email address:</p>" +
        $"<p><a href=\"{System.Net.WebUtility.HtmlEncode(url)}\">Verify my email</a></p><p>This link expires in 24 hours.</p>");
    public static string Welcome(string name) => Layout("Welcome to Rubino Digital",
        $"<p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p><p>Your account is ready. Build a quote in the quote builder, or reply to talk about your project.</p>");
    public static string EnquiryReceived(string name, string reference) => Layout("We got your enquiry",
        $"<p>Hi {System.Net.WebUtility.HtmlEncode(name)},</p><p>Thanks for getting in touch. Your reference is <strong>{System.Net.WebUtility.HtmlEncode(reference)}</strong>. We will reply as soon as we can.</p>");
}

public class LogEmailService : IEmailService
{
    private readonly ILogger<LogEmailService> _logger;
    public LogEmailService(ILogger<LogEmailService> logger) => _logger = logger;
    public Task SendAsync(string to, string subject, string htmlBody)
    { _logger.LogInformation("EMAIL (Log provider — not sent) to {To}: {Subject}", to, subject); return Task.CompletedTask; }
    public Task SendVerificationEmailAsync(string to, string name, string url) => SendAsync(to, "Verify your Rubino Digital email", EmailTemplates.Verify(name, url));
    public Task SendWelcomeEmailAsync(string to, string name) => SendAsync(to, "Welcome to Rubino Digital", EmailTemplates.Welcome(name));
    public Task SendEnquiryReceivedAsync(string to, string name, string reference) => SendAsync(to, "We got your enquiry", EmailTemplates.EnquiryReceived(name, reference));
    public Task SendEnquiryNotificationAsync(string agencyEmail, string name, string email, string reference, string message)
        => SendAsync(agencyEmail, $"New enquiry {reference} from {name}",
            EmailTemplates.Layout("New enquiry", $"<p>From: {System.Net.WebUtility.HtmlEncode(name)} ({System.Net.WebUtility.HtmlEncode(email)})</p><p>Reference: {System.Net.WebUtility.HtmlEncode(reference)}</p><p>{System.Net.WebUtility.HtmlEncode(message)}</p>"));
}

public class ResendEmailService : IEmailService
{
    private readonly HttpClient _http; private readonly IConfiguration _config;
    public ResendEmailService(HttpClient http, IConfiguration config) { _http = http; _config = config; }
    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var from = _config["Email:From"] ?? "hello@rubinodigital.example";
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config["Email:ApiKey"]);
        req.Content = new StringContent(JsonSerializer.Serialize(new { from, to, subject, html = htmlBody }), Encoding.UTF8, "application/json");
        var res = await _http.SendAsync(req); res.EnsureSuccessStatusCode();
    }
    public Task SendVerificationEmailAsync(string to, string name, string url) => SendAsync(to, "Verify your Rubino Digital email", EmailTemplates.Verify(name, url));
    public Task SendWelcomeEmailAsync(string to, string name) => SendAsync(to, "Welcome to Rubino Digital", EmailTemplates.Welcome(name));
    public Task SendEnquiryReceivedAsync(string to, string name, string reference) => SendAsync(to, "We got your enquiry", EmailTemplates.EnquiryReceived(name, reference));
    public Task SendEnquiryNotificationAsync(string agencyEmail, string name, string email, string reference, string message)
        => SendAsync(agencyEmail, $"New enquiry {reference} from {name}", EmailTemplates.Layout("New enquiry", $"<p>From: {name} ({email})</p><p>Reference: {reference}</p>"));
}

public class SendGridEmailService : IEmailService
{
    private readonly HttpClient _http; private readonly IConfiguration _config;
    public SendGridEmailService(HttpClient http, IConfiguration config) { _http = http; _config = config; }
    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var from = _config["Email:From"] ?? "hello@rubinodigital.example";
        var payload = new
        {
            personalizations = new[] { new { to = new[] { new { email = to } } } },
            from = new { email = from }, subject,
            content = new[] { new { type = "text/html", value = htmlBody } }
        };
        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send");
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config["Email:ApiKey"]);
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var res = await _http.SendAsync(req); res.EnsureSuccessStatusCode();
    }
    public Task SendVerificationEmailAsync(string to, string name, string url) => SendAsync(to, "Verify your Rubino Digital email", EmailTemplates.Verify(name, url));
    public Task SendWelcomeEmailAsync(string to, string name) => SendAsync(to, "Welcome to Rubino Digital", EmailTemplates.Welcome(name));
    public Task SendEnquiryReceivedAsync(string to, string name, string reference) => SendAsync(to, "We got your enquiry", EmailTemplates.EnquiryReceived(name, reference));
    public Task SendEnquiryNotificationAsync(string agencyEmail, string name, string email, string reference, string message)
        => SendAsync(agencyEmail, $"New enquiry {reference} from {name}", EmailTemplates.Layout("New enquiry", $"<p>From: {name} ({email})</p><p>Reference: {reference}</p>"));
}
