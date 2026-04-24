using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PaymentService.Consumers;
using Shared.Web.Extensions;
using PaymentService.Data;
using PaymentService.Persistence;
using PaymentService.Repositories;
using PaymentService.Repositories.Interface;
using PaymentService.RequestHelpers;
using PaymentService.Services;
using PaymentService.Services.Interface;
using PaymentService.Services.Payment;
using PaymentService.SignalR;
using SharedWeb.Middleware;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Load local secrets if exists (ignored by Git)
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddDbContext<PaymentSvcDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key configuration is missing")))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Request.Cookies.TryGetValue("access_token", out var accessToken);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddScoped<ExceptionMiddleware>();

builder.Services.AddScoped<IPaymentUnitOfWork, PaymentUnitOfWork>(); // Chỉ đăng ký UnitOfWork, Repository sẽ được khởi tạo trong UnitOfWork
// builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentServiceFactory, PaymentServiceFactory>();

// Register Payment Service Implementations
builder.Services.AddScoped<StripePaymentService>();
builder.Services.AddScoped<MomoPaymentService>();
builder.Services.AddScoped<VNPayPaymentService>();
builder.Services.AddScoped<BankTransferPaymentService>();
builder.Services.AddScoped<CODPaymentService>();

builder.Services.AddSharedControllers();

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

builder.Services.AddMassTransit(x =>
{
    // Add Entity Framework Outbox FIRST for reliable event delivery
    x.AddEntityFrameworkOutbox<PaymentSvcDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10); // Polling interval for outbox messages
        o.UsePostgres();
        o.UseBusOutbox(); // Use outbox within EF transaction
    });

    // Register consumers for Saga commands
    x.AddConsumer<CreatePaymentConsumer>();
    
    // Set endpoint naming to match other services
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("payment", false));
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        // ConfigureEndpoints handles both saga and consumers with consistent naming
        cfg.ConfigureEndpoints(context);
    });
});

//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("SignalR", policy =>
//    {
//        policy.WithOrigins("http://localhost:3000") // origin của FE
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials(); // ← BẮT BUỘC cho SignalR
//    });
//});

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseCors("SignalR");           // 1. CORS trước tiên

//app.UseWebSockets();              // 2. WebSocket support

app.UseAuthentication();          // 3. Auth
app.UseAuthorization();           // 4. Authorization

app.MapControllers();
app.MapHub<PaymentHub>("/hubs/payment");

app.Run();

