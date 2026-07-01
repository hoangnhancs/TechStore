using EmailService.Extensions;
using EmailService.Services.Interface;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddEmailServices(builder.Configuration);

builder.Services.AddCors(options =>
    options.AddPolicy("portfolio", policy =>
    {
        var allowedOrigin = builder.Configuration["AllowedOrigin"] ?? "https://ec.io.vn";
        policy
            .WithOrigins(allowedOrigin, "http://localhost:3000", "http://localhost:5500")
            .AllowAnyMethod()
            .AllowAnyHeader();
    }));

builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("contact", context =>
    {
        // Behind Cloudflare, real IP is in CF-Connecting-IP header
        var ip = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault()
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(ip,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromHours(1),
                QueueLimit = 0
            });
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (ctx, _) =>
    {
        ctx.HttpContext.Response.ContentType = "application/json";
        await ctx.HttpContext.Response.WriteAsync(
            """{"message":"Bạn đã gửi quá nhiều lần. Vui lòng thử lại sau 1 giờ."}""", _);
    };
});

var app = builder.Build();
app.UseCors("portfolio");
app.UseRateLimiter();

app.MapPost("/api/send-email", async (
    ContactRequest req,
    IEmailService emailService,
    IHttpClientFactory httpFactory,
    IConfiguration config) =>
{
    var turnstileSecret = config["Turnstile:SecretKey"]
        ?? throw new InvalidOperationException("Turnstile:SecretKey not configured.");

    using var http = httpFactory.CreateClient();
    var form = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string, string>("secret", turnstileSecret),
        new KeyValuePair<string, string>("response", req.TurnstileToken)
    });
    var verifyResp = await http.PostAsync("https://challenges.cloudflare.com/turnstile/v0/siteverify", form);
    var turnstile = await verifyResp.Content.ReadFromJsonAsync<TurnstileResponse>();
    if (turnstile?.Success != true)
        return Results.BadRequest(new { message = "Xác minh thất bại. Vui lòng thử lại." });

    var recipient = config["Contact:RecipientEmail"]
        ?? throw new InvalidOperationException("Contact:RecipientEmail not configured.");

    var subject = $"[Portfolio] {req.Subject}";
    var body = $"""
        <h2 style="color:#333">Tin nhắn mới từ portfolio</h2>
        <p><strong>Họ tên:</strong> {req.Name}</p>
        <p><strong>Email:</strong> <a href="mailto:{req.Email}">{req.Email}</a></p>
        <p><strong>Tiêu đề:</strong> {req.Subject}</p>
        <hr/>
        <p style="white-space:pre-line">{req.Message}</p>
        """;

    await emailService.SendEmailAsync(recipient, subject, body);
    return Results.Ok(new { message = "Gửi thành công!" });
}).RequireRateLimiting("contact");

app.Run();

record ContactRequest(
    string Name,
    string Email,
    string Subject,
    string Message,
    string TurnstileToken);

record TurnstileResponse(
    [property: JsonPropertyName("success")] bool Success);
