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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSharedControllers();

builder.Services.AddHttpClient<ProductSvcHttpClient>().AddPolicyHandler(GetRetryPolicy());
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddJwtFromCookieAuthentication(builder.Configuration);

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

builder.Services.AddGrpcClient<GrpcProduct.GrpcProductClient>(o =>
    o.Address = new Uri(builder.Configuration["GrpcProduct"]
        ?? throw new InvalidOperationException("'GrpcProduct' address is not configured.")));

builder.Services.AddGrpcClient<GrpcRecommendation.GrpcRecommendationClient>(o =>
    o.Address = new Uri(builder.Configuration["GrpcRecommendation"]
        ?? throw new InvalidOperationException("'GrpcRecommendation' address is not configured.")));

builder.Services.AddScoped<GrpcProductClient>();
builder.Services.AddScoped<GrpcRecommendationClient>();

var app = builder.Build();

// Initialize MongoDB BEFORE any other operations
//await DB.InitAsync("SearchDb", MongoClientSettings
//    .FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

//app.Lifetime.ApplicationStarted.Register(async () =>
//{
//    try
//    {
//        await DbInitializer.SeedData(app);
//    }
//    catch (Exception ex)
//    {
//        var logger = app.Services.GetRequiredService<ILogger<Program>>();
//        logger.LogError(ex, "An error occurred while seeding the search database.");
//    }
//});

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

