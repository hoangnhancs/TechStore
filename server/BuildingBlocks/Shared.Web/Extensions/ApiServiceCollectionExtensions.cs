using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SharedWeb.Middleware;

namespace Shared.Web.Extensions
{
    public static class ApiServiceCollectionExtensions
    {
        /// <summary>
        /// Registers controllers with shared JSON settings and the global exception middleware.
        /// </summary>
        public static IServiceCollection AddSharedControllers(this IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(x =>
                    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            // Register ExceptionMiddleware so UseSharedMiddleware() can activate it via DI.
            services.AddScoped<ExceptionMiddleware>();

            return services;
        }

        /// <summary>
        /// Configures JWT Bearer authentication that reads the token from the "access_token" cookie.
        /// </summary>
        public static IServiceCollection AddJwtFromCookieAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtKey = configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("'Jwt:Key' configuration is missing.");

            services.AddAuthentication(options =>
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
                    ValidIssuer              = configuration["Jwt:Issuer"],
                    ValidAudience            = configuration["Jwt:Audience"],
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

            return services;
        }

        /// <summary>
        /// Activates the global <see cref="ExceptionMiddleware"/> in the request pipeline.
        /// Must be called before UseAuthentication / UseAuthorization.
        /// </summary>
        public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionMiddleware>();
        }
    }
}