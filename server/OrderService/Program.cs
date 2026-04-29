using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using Shared.Web.Extensions;
using OrderService.Persistence;
using OrderService.Repositories;
using OrderService.Repositories.Interface;
using OrderService.RequestHelpers;
using OrderService.Saga;
using OrderService.Services;
using OrderService.Services.Order;
using OrderService.SignalR;



// using ProductService;
using SharedWeb.Middleware;
using ProductService.Grpc;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSharedControllers();
builder.Services.AddDbContext<OrderSvcDbContext>(options =>
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

builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetListOrdersInRangeDateQuery).Assembly);
});

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

// Configure MassTransit with RabbitMQ
builder.Services.AddMassTransit(x =>
{
    // Add Entity Framework Outbox FIRST for reliable event delivery
    x.AddEntityFrameworkOutbox<OrderSvcDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10); // Polling interval for outbox messages
        o.UsePostgres();
        o.UseBusOutbox(); // Use outbox within EF transaction
    });

    // Register Saga State Machine
    x.AddSagaStateMachine<OrderSagaStateMachine, OrderSagaState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<OrderSvcDbContext>();
            r.UsePostgres();
        });

    // Register consumers for Saga commands
    x.AddConsumer<OrderService.Consumers.ConfirmOrderConsumer>();
    x.AddConsumer<OrderService.Consumers.CancelOrderConsumer>();
    x.AddConsumer<OrderService.Consumers.SetOrderWaitingForPaymentConsumer>();
    x.AddConsumer<OrderService.Consumers.ConfirmCodOrderConsumer>();
    x.AddConsumer<OrderService.Consumers.RetryPaymentConsumer>();
    
    // Set endpoint naming to match other services
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("order", false));
    
    x.AddQuartzConsumers(); // Add Quartz consumers for scheduled tasks

    x.UsingRabbitMq((context, cfg) =>
    {
        // cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        // {
        //     h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
        //     h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        // });

        cfg.UseDelayedMessageScheduler();
        cfg.Host(builder.Configuration["RabbitMq:ConnectionString"]);
        // ConfigureEndpoints handles both saga and consumers with consistent naming
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddQuartz(q =>
{
    q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 10);
    q.UsePersistentStore(s =>
    {
        s.UseProperties = true;
        s.UsePostgres(builder.Configuration["ConnectionStrings:DefaultConnection"]!);
    });
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

builder.Services.AddGrpcClient<GrpcProduct.GrpcProductClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcProduct"] ?? throw new InvalidOperationException("GrpcProduct address is not configured"));
});

builder.Services.AddScoped<ExceptionMiddleware>();
builder.Services.AddScoped<GrpcProductClient>();
builder.Services.AddScoped<IOrderUnitOfWork, OrderUnitOfWork>(); //chỉ đăng ký UnitOfWork, Repository sẽ được khởi tạo trong UnitOfWork
// builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<OrderHub>("/hubs/order"); //map hub cho client kết nối

app.Run();

