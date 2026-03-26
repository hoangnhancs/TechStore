using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Configs
{
    public class MailpitSettings
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1025;
    }
}