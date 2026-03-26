using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace EmailService.Services.Interface
{
    public class MailpitEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public MailpitEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var smtp = new SmtpClient();
            var host = _configuration["EmailSettings:Mailpit:Host"] ?? "localhost";
            var port = int.Parse(_configuration["EmailSettings:Mailpit:Port"] ?? "1025");
            
            await smtp.ConnectAsync(host, port, SecureSocketOptions.None);

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse("noreply@localhost"));
            message.To.Add(MailboxAddress.Parse(to));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }

        public Task SendOrderConfirmationEmailAsync(string to, string subject, string body)
        {
            throw new NotImplementedException();
        }

        public Task SendOrderStatusUpdateEmailAsync(string to, string subject, string body)
        {
            throw new NotImplementedException();
        }
    }
}