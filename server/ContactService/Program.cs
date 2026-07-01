using EmailService.Extensions;
using EmailService.Services.Interface;
using System.Text.Json.Serialization;

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

var app = builder.Build();
app.UseCors("portfolio");

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
});

app.Run();

record ContactRequest(
    string Name,
    string Email,
    string Subject,
    string Message,
    string TurnstileToken);

record TurnstileResponse(
    [property: JsonPropertyName("success")] bool Success);
