using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using PaymentService.Consumers;
using PaymentService.Data;
using PaymentService.Persistence;
using PaymentService.Repositories;
using PaymentService.Repositories.Interface;
using PaymentService.RequestHelpers;
using PaymentService.Services;
using PaymentService.Services.Interface;
using PaymentService.SignalR;
using SharedWeb.Middleware;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddTransient<ExceptionMiddleware>();

builder.Services.AddScoped<IPaymentUnitOfWork, PaymentUnitOfWork>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentServiceFactory, PaymentServiceFactory>();

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

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.


app.MapHub<PaymentHub>("/hubs/payment"); //map hub cho client kết nối

app.Run();

