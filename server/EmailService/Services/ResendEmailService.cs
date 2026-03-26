using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailService.Services.Interface;
using Microsoft.Extensions.Configuration;
using Resend;

namespace EmailService.Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly IConfiguration _configuration;
        public ResendEmailService(IResend resend, IConfiguration configuration)
        {
            _resend = resend;
            _configuration = configuration;
        }
        public Task SendOrderConfirmationEmailAsync(string to, string subject, string body)
        {
            throw new NotImplementedException();
        }

        public Task SendOrderStatusUpdateEmailAsync(string to, string subject, string body)
        {
            throw new NotImplementedException();
        }
        public async Task SendEmailAsync(string email, string subject, string body)
        {
            var message = new EmailMessage
            {
                From = _configuration["EmailSettings:Resend:From"] ?? "whatever@resend.dev",
                Subject = subject,
                HtmlBody = body,
            };
            message.To.Add(email);
            await _resend.EmailSendAsync(message);
        }
    }
}