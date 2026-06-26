using MassTransit;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using ProductService.Grpc;
using RecommendationService.Consumers;
using RecommendationService.Data;
using RecommendationService.Persistence;
using RecommendationService.Services;
using Shared.Web.Extensions;
using SharedWeb.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSharedControllers();
builder.Services.AddGrpc();

builder.Services.AddJwtFromCookieAuthentication(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
// Add DbContext for storing embeddings
builder.Services.AddDbContext<RecommandationSvcDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
        o => o.UseVector()));

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<RecommandationSvcDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10);
        o.UsePostgres();
        o.UseBusOutbox();
    }); // Configure outbox to use the existing DbContext and Postgres

    x.AddConsumer<OrderCreatedConsumer>();
    x.AddConsumer<ProductCreatedConsumer>();
    x.AddConsumer<ProductUpdatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("recommendation", false)); // Use kebab-case for endpoint names, with "order" as the prefix

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

builder.Services.AddGrpcClient<GrpcProduct.GrpcProductClient>(o =>
    o.Address = new Uri(builder.Configuration["GrpcProduct"]
        ?? throw new InvalidOperationException("'GrpcProduct' address is not configured.")));

builder.Services.AddScoped<GrpcProductClient>();
builder.Services.AddScoped<IRecommendationUnitOfWork, RecommendationUnitOfWork>();

builder.Services.AddHttpClient<ProductSvcHttpClient>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(5);
}).AddPolicyHandler(GetRetryPolicy());

// VectorService client for ML embeddings
builder.Services.AddHttpClient<VectorServiceClient>()
    .AddPolicyHandler(GetRetryPolicy());

var app = builder.Build();

// Seed database with product embeddings

app.UseSharedMiddleware();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<GrpcRecommendationService>();
app.MapControllers();

await DbInitializer.SeedData(app);

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));

