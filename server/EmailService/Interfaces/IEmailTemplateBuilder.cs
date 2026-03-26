using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmailService.Interfaces
{
    public interface IEmailTemplateBuilder
    {
        Task<string> BuildAsync(string templateName, Dictionary<string, string> parameters);
    }
}