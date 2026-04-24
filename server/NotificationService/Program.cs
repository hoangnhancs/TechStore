using EmailService.Extensions;
using MassTransit;
using Shared.Web.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using NotificationService.Consumers;
using NotificationService.Data;
using NotificationService.Persistence;
using NotificationService.RequestHelpers;
using SharedWeb.Middleware;
using IdentityService.Grpc;
using NotificationService.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSharedControllers();

builder.Services.AddDbContext<NotificationSvcDbContext>(options =>
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

builder.Services.AddScoped<ExceptionMiddleware>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

builder.Services.AddMassTransit(x =>
{
    // Add Entity Framework Outbox FIRST for reliable event delivery
    //x.AddEntityFrameworkOutbox<NotificationSvcDbContext>(o =>
    //{
    //    o.QueryDelay = TimeSpan.FromSeconds(10); // Polling interval for outbox messages
    //    o.UsePostgres();
    //    o.UseBusOutbox(); // Use outbox within EF transaction
    //});

    // Register consumers for Saga commands
    x.AddConsumer<OrderConfirmedConsumer>();

    // Set endpoint naming to match other services
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notification", false));

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
builder.Services.AddGrpcClient<GrpcIdentity.GrpcIdentityClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcIdentity"] ?? throw new InvalidOperationException("GrpcIdentity address is not configured"));
});

builder.Services.AddScoped<NotificationService.Services.GrpcIdentityClient>();
builder.Services.AddScoped<INotificationUnitOfWork, NotificationUnitOfWork>();


builder.Services.AddEmailServices(builder.Configuration);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSignalR();

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<NotificationSvcDbContext>();
    var logger = services.GetRequiredService<ILogger<DbInitializer>>();
    var grpcIdentityClient = services.GetRequiredService<NotificationService.Services.GrpcIdentityClient>();
    await context.Database.MigrateAsync();
    await DbInitializer.SeedData(context, logger, grpcIdentityClient);
}
catch (Exception ex)
{
    Console.WriteLine("An error occurred creating the DB: ", ex.Message);
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/hubs/notification"); //map hub cho client kết nối


app.MapControllers();


app.Run();

