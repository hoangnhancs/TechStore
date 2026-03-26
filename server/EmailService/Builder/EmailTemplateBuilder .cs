using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailService.Interfaces;

namespace EmailService.Builder
{
    public class EmailTemplateBuilder : IEmailTemplateBuilder
    {
        private readonly string _templateDir;
        public EmailTemplateBuilder()
        {
            _templateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        }
        public async Task<string> BuildAsync(string templateName, Dictionary<string, string> parameters)
        {
            var path = Path.Combine(_templateDir, $"{templateName}.html");
        
            if (!File.Exists(path))
                throw new FileNotFoundException($"Template '{templateName}' không tồn tại: {path}");

            var template = await File.ReadAllTextAsync(path);

            foreach (var (key, value) in parameters)
                template = template.Replace($"{{{{{key}}}}}", value);

            return template;
        }
    }
}