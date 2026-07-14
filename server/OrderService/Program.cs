using IdentityService.Grpc;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OrderService.Consumers;
using OrderService.Data;
using OrderService.Persistence;
using OrderService.RequestHelpers;
using OrderService.Saga;
using OrderService.Services;
using OrderService.SettingConfig;
using OrderService.SignalR;
using OrderService.Workers;
using ProductService.Grpc;
using Quartz;
using Shared.Web.Extensions;


DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSharedControllers();

builder.Services.AddDbContext<OrderSvcDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddJwtFromCookieAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<OrderSvcDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromHours(24);
        o.UsePostgres();
        o.UseBusOutbox();
    }); // Configure outbox to use the existing DbContext and Postgres

    x.AddSagaStateMachine<OrderSagaStateMachine, OrderSagaState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<OrderSvcDbContext>();
            r.UsePostgres();
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
        });

    x.AddConsumer<ConfirmOrderConsumer>();
    x.AddConsumer<CancelOrderConsumer>();
    x.AddConsumer<SetOrderWaitingForConfirmationConsumer>();
    x.AddConsumer<ConfirmCodOrderConsumer>();
    x.AddConsumer<RetryPaymentConsumer>();
    x.AddConsumer<OrderNotificationConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("order", false)); // Use kebab-case for endpoint names, with "order" as the prefix

    x.AddQuartzConsumers(); // Add Quartz consumers for scheduled tasks

    x.UsingRabbitMq((context, cfg) =>
    {
        // cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        // {
        //     h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
        //     h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        // });
        cfg.Host(builder.Configuration["RabbitMq:ConnectionString"]);

        // Retry on PostgreSQL serialization failures (40001) caused by concurrent saga updates
        cfg.UseMessageRetry(r =>
        {
            r.Handle<PostgresException>(ex => ex.SqlState == "40001");
            r.Intervals(50, 150, 500, 1000, 3000);
        });

        // Option 1: RabbitMQ Delayed Plugin
        // Ưu: delay message trực tiếp trong RabbitMQ → survive app restart
        // Nhược: cần cài plugin, phụ thuộc cấu hình persistence của RabbitMQ
        // cfg.UseDelayedMessageScheduler();


        // Option 2: MassTransit transport-based scheduler
        // Ưu: đơn giản, không cần Quartz
        // Nhược: phụ thuộc transport (RabbitMQ không có delayed plugin → không delay đúng),
        // không persistent như Quartz
        // cfg.UsePublishMessageScheduler();


        // Option 3: Quartz + persistent store
        // Ưu: job lưu DB → survive cả app restart lẫn RabbitMQ restart
        // Nhược: setup phức tạp hơn, cần tạo bảng QRTZ_*
        cfg.UseMessageScheduler(new Uri("queue:quartz"));

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddQuartz(q =>
{
    // q.UseMicrosoftDependencyInjectionJobFactory();

    // Option A: In-memory (dev/test)
    // q.UseInMemoryStore();

    // Option B: Persistent store (production)
    q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 10);
    q.UsePersistentStore(s =>
    {
        s.UseProperties = true;
        s.UsePostgres(builder.Configuration["ConnectionStrings:DefaultConnection"] ?? throw new InvalidOperationException("'ConnectionStrings:DefaultConnection' is not configured."));
        s.UseNewtonsoftJsonSerializer(); // Removed: Not a Quartz method
    });
});
// Set Quartz serializer type to 'json' for persistent store
// builder.Services.Configure<QuartzOptions>(options =>
// {
//     options["quartz.serializer.type"] = "json";
// });

// Reduce how often Quartz polls Postgres for due triggers so Neon can auto-suspend
builder.Services.Configure<QuartzOptions>(options =>
{
    options["quartz.scheduler.idleWaitTime"] = TimeSpan.FromHours(24).TotalMilliseconds.ToString("F0");
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


builder.Services.AddGrpcClient<GrpcProduct.GrpcProductClient>(o =>
    o.Address = new Uri(builder.Configuration["GrpcProduct"]
        ?? throw new InvalidOperationException("'GrpcProduct' address is not configured.")))
    .ConfigureChannel(o => o.HttpHandler = new SocketsHttpHandler
    {
        PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
        KeepAlivePingDelay = TimeSpan.FromSeconds(60),
        KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
        EnableMultipleHttp2Connections = true
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

builder.Services.Configure<PaymentOptions>(
    builder.Configuration.GetSection("PaymentOptions"));

builder.Services.AddScoped<GrpcProductClient>();
builder.Services.AddScoped<GrpcIdentityClient>();
builder.Services.AddScoped<IOrderUnitOfWork, OrderUnitOfWork>();
// builder.Services.AddHostedService<UserInforSyncWorker>();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSharedMiddleware();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<OrderHub>("/hubs/order");

app.Run();

