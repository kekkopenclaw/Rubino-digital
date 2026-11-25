using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RubinoDigital.Api.Data;
using RubinoDigital.Api.Middleware;
using RubinoDigital.Api.Services;

// Rubino Digital API — .NET 10, single app serving the Angular 18 build.
// Free-only stack: SQLite, JWT, BCrypt, no paid service is required to run it.
// Every secret comes from environment variables. Nothing secret is in this repo.
var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
    builder.Logging.AddJsonConsole();

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<PricingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

var emailProvider = builder.Configuration["Email:Provider"] ?? "Log";
if (emailProvider.Equals("Resend", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddHttpClient<IEmailService, ResendEmailService>();
else if (emailProvider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddHttpClient<IEmailService, SendGridEmailService>();
else
    builder.Services.AddScoped<IEmailService, LogEmailService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? string.Empty;
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RubinoDigital.Api";
if (builder.Environment.IsProduction() && jwtKey.Length < 32)
    throw new InvalidOperationException("Jwt:Key must be a long random secret (32+ characters) in production.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidateAudience = true, ValidateLifetime = true,
            ValidateIssuerSigningKey = true, ValidIssuer = jwtIssuer, ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.FromMinutes(2)
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("auth", l => { l.PermitLimit = 5; l.Window = TimeSpan.FromMinutes(1); l.QueueLimit = 0; });
    o.AddFixedWindowLimiter("enquiry", l => { l.PermitLimit = 10; l.Window = TimeSpan.FromMinutes(1); l.QueueLimit = 0; });
    o.RejectionStatusCode = 429;
});

// CORS: only the configured frontend origin. Never AllowAnyOrigin in production.
var frontendOrigin = builder.Configuration["Frontend:Origin"] ?? "http://localhost:4200";
builder.Services.AddCors(o =>
    o.AddPolicy("Frontend", p => p.WithOrigins(frontendOrigin).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.EnsureSeeded(db, scope.ServiceProvider.GetRequiredService<IAuthService>());
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("Frontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok", service = "RubinoDigital.Api", framework = ".NET 10",
    paymentsMode = "test", stripeConfigured = !string.IsNullOrWhiteSpace(builder.Configuration["Stripe:SecretKey"]),
    utc = DateTime.UtcNow
}));
app.MapFallbackToFile("index.html");
app.Run();
