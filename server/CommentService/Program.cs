using CommentService.Data;
using CommentService.Persistence;
using CommentService.RequestHelpers;
using CommentService.Services;
using CommentService.SignalR;
using CommentService.Workers;
using IdentityService.Grpc;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSharedControllers();

builder.Services.AddDbContext<CommentSvcDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddJwtFromCookieAuthentication(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<CommentSvcDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("comment", false));

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
        ?? throw new InvalidOperationException("'GrpcIdentity' address is not configured.")))
    .ConfigureChannel(o => o.HttpHandler = new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
        EnableMultipleHttp2Connections = true
    });

builder.Services.AddScoped<GrpcIdentityClient>();
builder.Services.AddScoped<ICommentUnitOfWork, CommentUnitOfWork>();
// builder.Services.AddScoped<IUserSyncService, UserSyncService>();
builder.Services.AddHostedService<UserInforSyncWorker>();
builder.Services.AddSignalR();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSharedMiddleware();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<CommentHub>("/hubs/comment");

try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CommentSvcDbContext>();
    await context.Database.MigrateAsync();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating the database.");
}

app.Run();

