using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Extensions.Http;
using ProductService.Grpc;
using RecommendationService.Data;
using RecommendationService.Persistence;
using RecommendationService.Services;
using Shared.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddGrpc();

//builder.Services.AddJwtFromCookieAuthentication(builder.Configuration);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
// Add DbContext for storing embeddings
builder.Services.AddDbContext<RecommandationSvcDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddGrpcClient<GrpcProduct.GrpcProductClient>(o =>
    o.Address = new Uri(builder.Configuration["GrpcProduct"]
        ?? throw new InvalidOperationException("'GrpcProduct' address is not configured.")));

builder.Services.AddScoped<GrpcProductClient>();
builder.Services.AddScoped<IRecommandationUnitOfWork, RecommandationUnitOfWork>();

builder.Services.AddHttpClient<ProductSvcHttpClient>().AddPolicyHandler(GetRetryPolicy());

// VectorService client for ML embeddings
builder.Services.AddHttpClient<VectorServiceClient>()
    .AddPolicyHandler(GetRetryPolicy());

var app = builder.Build();

// Seed database with product embeddings

app.MapGrpcService<GrpcRecommendationService>();

await DbInitializer.SeedData(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));

