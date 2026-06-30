using EmailService.Extensions;
using IdentityService.Data;
using IdentityService.Entities;
using IdentityService.Persistence;
using IdentityService.RequestHelpers;
using IdentityService.Services;
using IdentityService.Services.Interfaces;
using IdentityService.Services.OtherServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Web.Extensions;
using Shared.Web.Helper;
using Shared.Web.Helper.Interface;
using SharedWeb.Middleware;
using static IdentityService.Services.Address.CreateVirtualAddresses;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSharedControllers();

builder.Services.AddDbContext<IdentitySvcDbContext>(opt =>
{
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    opt.UseNpgsql(connStr);
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(CreateVirtualAddressesCommand).Assembly);
});

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddGrpc();

builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    opt.SignIn.RequireConfirmedEmail = true;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<IdentitySvcDbContext>()
    .AddDefaultTokenProviders();

// Override Identity's default cookie auth with JWT from cookie
builder.Services.AddJwtFromCookieAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IHttpContextAccessorHelper, HttpContextAccessorHelper>();
builder.Services.AddScoped<IUserAccessor, UserAccessor>();
builder.Services.AddScoped<ITokenServices, TokenServices>();
builder.Services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
builder.Services.AddEmailServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseSharedMiddleware();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GrpcIdentityService>();

try
{
    using var scope = app.Services.CreateScope();
    var context     = scope.ServiceProvider.GetRequiredService<IdentitySvcDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await context.Database.MigrateAsync();
    await DbInitializer.SeedData(userManager, roleManager);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating or seeding the database.");
}

app.Run();

