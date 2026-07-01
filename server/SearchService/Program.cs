using MassTransit;
using MongoDB.Driver;
using MongoDB.Entities;
using Polly;
using Polly.Extensions.Http;
using ProductService.Grpc;
using Recommendations.Grpc;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;
using Shared.Web.Extensions;
using StackExchange.Redis;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSharedControllers();

builder.Services.AddHttpClient<ProductSvcHttpClient>().AddPolicyHandler(GetRetryPolicy());
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddJwtFromCookieAuthentication(builder.Configuration);

var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnection))
{
    try
    {
        var options = ParseRedisConnectionString(redisConnection);
        options.AbortOnConnectFail = false;
        var mux = ConnectionMultiplexer.Connect(options);
        builder.Services.AddSingleton<IConnectionMultiplexer>(mux);
        builder.Services.AddSingleton<ICacheService, RedisCacheService>();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Redis] Failed to connect, cache disabled: {ex.Message}");
        builder.Services.AddSingleton<ICacheService, NullCacheService>();
    }
}
else
{
    Console.WriteLine("[Redis] Connection string not configured, cache disabled.");
    builder.Services.AddSingleton<ICacheService, NullCacheService>();
}

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<ProductCreatedConsumer>();
    x.AddConsumer<ProductUpdatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:ConnectionString"]);
        cfg.ConfigureEndpoints(context);
    });
});

var grpcChannelOptions = new Action<Grpc.Net.ClientFactory.GrpcClientFactoryOptions>(_ => { });
var grpcKeepAlive = new SocketsHttpHandler
{
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
    KeepAlivePingDelay = TimeSpan.FromSeconds(60),
    KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
    EnableMultipleHttp2Connections = true
};

builder.Services.AddGrpcClient<GrpcProduct.GrpcProductClient>(o =>
    o.Address = new Uri(builder.Configuration["GrpcProduct"]
        ?? throw new InvalidOperationException("'GrpcProduct' address is not configured.")))
    .ConfigureChannel(o => o.HttpHandler = grpcKeepAlive);

builder.Services.AddGrpcClient<GrpcRecommendation.GrpcRecommendationClient>(o =>
    o.Address = new Uri(builder.Configuration["GrpcRecommendation"]
        ?? throw new InvalidOperationException("'GrpcRecommendation' address is not configured.")))
    .ConfigureChannel(o => o.HttpHandler = grpcKeepAlive);

builder.Services.AddScoped<GrpcProductClient>();
builder.Services.AddScoped<GrpcRecommendationClient>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedData(app);
}

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSharedMiddleware();

app.MapControllers();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));

// Supports both Railway's redis://user:pass@host:port URI and plain host:port format
static ConfigurationOptions ParseRedisConnectionString(string connectionString)
{
    if (!connectionString.StartsWith("redis://") && !connectionString.StartsWith("rediss://"))
        return ConfigurationOptions.Parse(connectionString);

    var uri = new Uri(connectionString);
    var options = new ConfigurationOptions
    {
        EndPoints = { { uri.Host, uri.Port } },
        AbortOnConnectFail = false,
        Ssl = uri.Scheme == "rediss",
    };
    if (!string.IsNullOrEmpty(uri.UserInfo))
    {
        var parts = uri.UserInfo.Split(':', 2);
        if (parts.Length == 2) options.Password = Uri.UnescapeDataString(parts[1]);
    }
    return options;
}
