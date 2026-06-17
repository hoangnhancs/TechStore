using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using ProductService.Grpc;
using RecommendationService.Data;
using RecommendationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add DbContext for storing embeddings
builder.Services.AddDbContext<RecommandationSvcDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddGrpcClient<GrpcProduct.GrpcProductClient>(o =>
    o.Address = new Uri(builder.Configuration["GrpcProduct"]
        ?? throw new InvalidOperationException("'GrpcProduct' address is not configured.")));

builder.Services.AddScoped<GrpcProductClient>();

builder.Services.AddHttpClient<ProductSvcHttpClient>().AddPolicyHandler(GetRetryPolicy());

// VectorService client for ML embeddings
builder.Services.AddHttpClient<VectorServiceClient>()
    .AddPolicyHandler(GetRetryPolicy());

var app = builder.Build();

// Seed database with product embeddings
await DbInitializer.SeedData(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));

