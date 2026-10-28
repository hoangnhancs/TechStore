using MassTransit;
using Microsoft.EntityFrameworkCore;
using VoucherService.Consumers;
using VoucherService.Data;
using VoucherService.Persistence;
using VoucherService.RequestHelpers;
using VoucherService.Services.Grpc;
using Shared.Web.Extensions;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSharedControllers();

builder.Services.AddDbContext<VoucherSvcDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddJwtFromCookieAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<VoucherSvcDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromHours(24);
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.AddConsumer<OrderConfirmedConsumer>();
    x.AddConsumer<OrderCancelledConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("voucher", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:ConnectionString"]);
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddGrpc();
builder.Services.AddScoped<IVoucherUnitOfWork, VoucherUnitOfWork>();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSharedMiddleware();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GrpcVoucherService>();

try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<VoucherSvcDbContext>();
    await context.Database.MigrateAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}

app.Run();
