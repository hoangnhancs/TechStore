using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("'Jwt:Key' configuration is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["Jwt:Issuer"],
        ValidAudience            = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("access_token", out var token)
                && !string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("customPolicy", policy =>
    {
        var clientApp = builder.Configuration["ClientApp"]
            ?? throw new InvalidOperationException("'ClientApp' configuration is missing.");

        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(origin =>
              {
                  var uri = new Uri(origin);
                  var host = uri.Host;

                  // Exact match và www variant
                  var clientUri = new Uri(clientApp);
                  var clientHost = clientUri.Host;
                  if (host.Equals(clientHost, StringComparison.OrdinalIgnoreCase))
                      return true;
                  if (host.Equals("www." + clientHost, StringComparison.OrdinalIgnoreCase))
                      return true;
                  if (clientHost.StartsWith("www.") && host.Equals(clientHost[4..], StringComparison.OrdinalIgnoreCase))
                      return true;

                  // Allow all Vercel preview deployments (*.vercel.app)
                  return host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase);
              });
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors("customPolicy");
app.UseWebSockets();
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();

