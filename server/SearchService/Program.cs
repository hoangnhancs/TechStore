using MassTransit;
using Polly;
using Shared.Web.Extensions;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;
using SharedWeb.Middleware;
using ProductService.Grpc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSharedControllers();

builder.Services.AddHttpClient<ProductSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());//auto mapper 14.x.x and lower
builder.Services.AddMassTransit(x =>
{
   x.AddConsumer<ProductCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            h.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        cfg.ReceiveEndpoint("search-aution-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));
            e.ConfigureConsumer<ProductCreatedConsumer>(context);
        });
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddGrpcClient<GrpcProduct.GrpcProductClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcProduct"] ?? throw new InvalidOperationException("GrpcProduct address is not configured"));
});

builder.Services.AddScoped<GrpcProductClient>();
builder.Services.AddScoped<ExceptionMiddleware>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.SeedData(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
});

app.MapControllers();

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
        .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));
