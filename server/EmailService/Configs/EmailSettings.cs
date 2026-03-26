using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Configs
{
    public class EmailSettings
    {
        public string Provider { get; set; } = string.Empty;
        public MailpitSettings Mailpit { get; set; } = new();
        public ResendSettings Resend { get; set; } = new();
    }
}