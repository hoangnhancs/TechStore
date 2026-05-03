using IdentityService.Grpc;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ReviewService.Data;
using ReviewService.Persistence;
using ReviewService.Repositories;
using ReviewService.Repositories.Interface;
using ReviewService.RequestHelpers;
using ReviewService.Services;
using ReviewService.SignalR;
using Shared.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSharedControllers();

builder.Services.AddDbContext<ReviewSvcDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddJwtFromCookieAuthentication(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<ReviewSvcDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("review", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:ConnectionString"]);

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddGrpcClient<GrpcIdentity.GrpcIdentityClient>(o =>
    o.Address = new Uri(builder.Configuration["GrpcIdentity"]
        ?? throw new InvalidOperationException("'GrpcIdentity' address is not configured.")));

builder.Services.AddScoped<IReviewUnitOfWork, ReviewUnitOfWork>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<GrpcIdentityClient>();
builder.Services.AddSignalR();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSharedMiddleware();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ReviewHub>("/hubs/review");

try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ReviewSvcDbContext>();
    await context.Database.MigrateAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}

app.Run();
