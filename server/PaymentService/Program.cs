using MassTransit;
using Microsoft.EntityFrameworkCore;
using PaymentService.Consumers;
using PaymentService.Data;
using PaymentService.Jobs;
using PaymentService.Persistence;
using PaymentService.RequestHelpers;
using PaymentService.Services;
using PaymentService.Services.Interface;
using PaymentService.SignalR;
using Shared.Web.Extensions;
using SharedWeb.Middleware;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

// builder.Services.AddOpenApi();
builder.Services.AddSharedControllers();

builder.Services.AddDbContext<PaymentSvcDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddJwtFromCookieAuthentication(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<PaymentSvcDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.AddConsumer<CreatePaymentConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("payment", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        // cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        // {
        //     h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
        //     h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        // });
        cfg.Host(builder.Configuration["RabbitMq:ConnectionString"]);
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddScoped<IPaymentUnitOfWork, PaymentUnitOfWork>();
builder.Services.AddScoped<IPaymentServiceFactory, PaymentServiceFactory>();

builder.Services.AddScoped<StripePaymentService>();
builder.Services.AddScoped<MomoPaymentService>();
builder.Services.AddScoped<VNPayPaymentService>();
builder.Services.AddScoped<BankTransferPaymentService>();
builder.Services.AddScoped<CODPaymentService>();

builder.Services.AddHostedService<PaymentReconciliationJob>();

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// if (app.Environment.IsDevelopment())
//     app.MapOpenApi();

app.UseSharedMiddleware();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PaymentHub>("/hubs/payment");

try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<PaymentSvcDbContext>();
    await context.Database.MigrateAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}

app.Run();


