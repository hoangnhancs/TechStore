using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailService.Interfaces;
using HandlebarsDotNet;

namespace EmailService.Builder
{
    public class EmailTemplateBuilder : IEmailTemplateBuilder
    {
        private readonly string _templateDir;
        public EmailTemplateBuilder()
        {
            _templateDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
        }
        public async Task<string> BuildAsync(string templateName, object parameters)
        {
            var path = Path.Combine(_templateDir, $"{templateName}.html");

            if (!File.Exists(path))
                throw new FileNotFoundException($"Template '{templateName}' không tồn tại");

            var source = await File.ReadAllTextAsync(path);

            var template = Handlebars.Compile(source);

            return template(parameters);
        }
    }
}