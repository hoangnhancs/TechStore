using EmailService.Extensions;
using IdentityService.Grpc;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Consumers;
using NotificationService.Data;
using NotificationService.Persistence;
using NotificationService.RequestHelpers;
using NotificationService.Services;
using NotificationService.SignalR;
using NotificationService.Workers;
using Shared.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSharedControllers();

builder.Services.AddDbContext<NotificationSvcDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddJwtFromCookieAuthentication(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<OrderConfirmedConsumer>();
    x.AddConsumer<OrderCancelledConsumer>();
    x.AddConsumer<CommentCreatedConsumer>();
    x.AddConsumer<ReviewCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notification", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        // cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        // {
        //     h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
        //     h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        // });
        cfg.Host(new Uri(builder.Configuration["RabbitMq:ConnectionString"] ?? throw new InvalidOperationException("'RabbitMq:ConnectionString' is not configured.")));
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddGrpcClient<GrpcIdentity.GrpcIdentityClient>(o =>
    o.Address = new Uri(builder.Configuration["GrpcIdentity"]
        ?? throw new InvalidOperationException("'GrpcIdentity' address is not configured.")));

builder.Services.AddScoped<GrpcIdentityClient>();
builder.Services.AddScoped<INotificationUnitOfWork, NotificationUnitOfWork>();
builder.Services.AddScoped<NotificationService.Services.Sender.INotificationServiceSender, NotificationService.Services.Sender.NotificationServiceSender>();
builder.Services.AddHostedService<UserInforSyncWorker>();

builder.Services.AddEmailServices(builder.Configuration);
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSharedMiddleware();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");

try
{
    using var scope = app.Services.CreateScope();
    var context           = scope.ServiceProvider.GetRequiredService<NotificationSvcDbContext>();
    var logger            = scope.ServiceProvider.GetRequiredService<ILogger<DbInitializer>>();
    var grpcIdentityClient = scope.ServiceProvider.GetRequiredService<GrpcIdentityClient>();
    await context.Database.MigrateAsync();
    await DbInitializer.SeedData(context, logger, grpcIdentityClient);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
}

app.Run();

