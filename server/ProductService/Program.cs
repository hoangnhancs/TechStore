using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Persistence;
using ProductService.Repositories;
using ProductService.Repositories.Interface;
using ProductService.RequestHelpers;
using ProductService.Services;
using ProductService.Services.Product;
using SharedWeb.Middleware;
using PhotoService.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddControllers();
builder.Services.AddDbContext<ProductSvcDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add JWT Bearer authentication only
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key configuration is missing")))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Request.Cookies.TryGetValue("access_token", out var accessToken);
                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetProductListByCategoryQuery).Assembly);
});

builder.Services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<ProductSvcDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(10); // Thời gian polling lấy message từ Outbox table để gửi đi
        o.UsePostgres();
        o.UseBusOutbox(); // Dùng bus outbox => publish trong transaction của EF thay vì gửi ngay
    });

    // x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("product", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            h.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddGrpc();

builder.Services.AddTransient<ExceptionMiddleware>();

builder.Services.AddScoped<IProductUnitOfWork, ProductUnitOfWork>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IBannerImageRepository, BannerImageRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IFilterTagRepository, FilterTagRepository>();

builder.Services.AddPhotoServices(builder.Configuration);

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.

try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ProductSvcDbContext>();
    var logger = services.GetRequiredService<ILogger<DbInitializer>>();
    await context.Database.MigrateAsync();
    await DbInitializer.SeedData(context, logger);
}
catch (Exception ex)
{
    Console.WriteLine("An error occurred creating the DB: ", ex.Message);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<GrpcProductService>();


app.Run();
