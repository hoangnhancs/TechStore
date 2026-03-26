using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailService.Builder;
using EmailService.Configs;
using EmailService.Interfaces;
using EmailService.Services;
using EmailService.Services.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmailService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailServices(
            this IServiceCollection services, // "this" = đây là extension method
            IConfiguration configuration
        )
        {
            // Đăng ký các dịch vụ liên quan đến email ở đây
            // Ví dụ: services.AddScoped<IEmailSender, EmailSender>();
            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings") ?? throw new InvalidOperationException("EmailSettings section is missing in configuration"));            
            var provider = configuration["EmailSettings:Provider"];
            switch (provider)
            {
                case "Mailpit":
                    services.AddScoped<IEmailService, MailpitEmailService>();
                    break;
                case "Resend":
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