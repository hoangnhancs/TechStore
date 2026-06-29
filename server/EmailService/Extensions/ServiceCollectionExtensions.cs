using EmailService.Builder;
using EmailService.Configs;
using EmailService.Interfaces;
using EmailService.Services;
using EmailService.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace EmailService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings")
                ?? throw new InvalidOperationException("EmailSettings section is missing in configuration"));

            var provider = configuration["EmailSettings:Provider"];
            switch (provider)
            {
                case "Mailpit":
                    services.AddScoped<IEmailService, MailpitEmailService>();
                    break;
                case "Resend":
                    var apiKey = configuration["EmailSettings:Resend:ApiKey"]
                        ?? throw new InvalidOperationException("'EmailSettings:Resend:ApiKey' is not configured.");
                    services.AddOptions();
                    services.AddHttpClient<ResendClient>();
                    services.Configure<ResendClientOptions>(o => o.ApiToken = apiKey);
                    services.AddTransient<IResend>(sp => sp.GetRequiredService<ResendClient>());
                    services.AddScoped<IEmailService, ResendEmailService>();
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported email provider: {provider}");
            }

            services.AddScoped<IEmailTemplateBuilder, EmailTemplateBuilder>();
            return services;
        }
    }
}