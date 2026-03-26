using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Services.Interface
{
    public interface IEmailService
    {
        Task SendOrderConfirmationEmailAsync(string to, string subject, string body);
        Task SendOrderStatusUpdateEmailAsync(string to, string subject, string body);
        Task SendEmailAsync(string email, string subject, string body);
    }
}